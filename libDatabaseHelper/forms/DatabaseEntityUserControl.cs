using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.Properties;

namespace libDatabaseHelper.forms
{
    public class DatabaseEntityUserControl : UserControl
    {
        protected bool ReloadNeeded;
        protected bool Inited = false;
        protected GenericDatabaseEntity LoadedEntity;

        private Type _type;
        private string[] _dragDropSupportingExtensions;

        public delegate bool OnOkClickedEvent();
        public event OnOkClickedEvent OnOkClicked;

        public struct ReturnStatus
        {
            public GenericDatabaseEntity EntityUpdated;
            public bool UpdateState;

            public ReturnStatus(GenericDatabaseEntity updatedEntity, bool updateState)
            {
                EntityUpdated = updatedEntity;
                UpdateState = updateState;
            }
        }

        protected void SetEntityType(Type type)
        {
            _type = type;
        }

        public void ClearForm()
        {
            ReloadNeeded = false;
            FormUtils.ClearForm(this);
        }

        protected virtual bool ValidateCreatedEntity(ref GenericDatabaseEntity entity)
        {
            return true;
        }

        protected virtual void OnEntityUpdated(GenericDatabaseEntity entity)
        {
        }

        protected void MakeWindowDragDroppable(string[] supportedExtensions)
        {
            _dragDropSupportingExtensions = supportedExtensions;
            MakeWindowDragDroppable(this);
        }

        private void MakeWindowDragDroppable(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                MakeWindowDragDroppable(childControl);
            }

            AllowDrop = true;

            control.DragEnter += (sender, e) =>
            {
                bool ret = false;
                if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    var data = e.Data.GetData("FileName") as Array;
                    if (data != null)
                    {
                        if ((data.Length == 1) && (data.GetValue(0) is String))
                        {
                            string filename = ((string[])data)[0];
                            string extension = Path.GetExtension(filename).ToLower();
                            if (_dragDropSupportingExtensions.Any(ext => ext == extension))
                            {
                                ret = true;
                            }
                        }
                    }
                }
                e.Effect = ret ? DragDropEffects.Copy : DragDropEffects.None;
            };

            control.DragDrop += (sender, e) =>
            {
                if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    var data = e.Data.GetData("FileName") as Array;
                    if (data != null)
                    {
                        if ((data.Length == 1) && (data.GetValue(0) is String))
                        {
                            string filename = ((string[])data)[0];
                            string extension = Path.GetExtension(filename).ToLower();
                            foreach (var ext in _dragDropSupportingExtensions)
                            {
                                if (ext == extension && File.Exists(filename))
                                {
                                    OnFileDragDropped(filename);
                                    break;
                                }
                            }
                        }
                    }
                }
            };
        }

        protected void AddResettableField(Button resettingButton, Control controlToReset)
        {
            var tableColumnData = controlToReset.Tag as TableColumnField;
            if (tableColumnData == null)
            {
                throw new InvalidOperationException("Unable to make field not marked 'TableColumnField', resettable.");
            }

            resettingButton.Tag = new ResettableControlDetail(controlToReset, tableColumnData.DefaultValue);
            resettingButton.Click+= (sender, e) => 
            {
                var resettableControlDetails = (sender as Control).Tag as ResettableControlDetail;
                if (resettableControlDetails != null)
                {
                    var controlToBeReset = resettableControlDetails.ControlToBeReset;
                    if (controlToBeReset is TextBox)
                    {
                        var txtToReset = controlToBeReset as TextBox;
                        txtToReset.Text = resettableControlDetails.DefaultValue as String;
                    }
                    else
                    {
                        throw new InvalidOperationException("Controls other than TextBoxes are not supported as of now.");
                    }
                }
            };
        }

        protected virtual void OnFileDragDropped(string fileName)
        {

        }

        public Type GetExtendedType()
        {
            return _type;
        }

        private void LoadToControl(GenericDatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            if (loadedEntity != null)
            {
                loadedEntity.LoadToForm(this, loadValuesOnly);
                if (! loadValuesOnly)
                {
                    LoadedEntity = loadedEntity;
                }
            }
        }

        public void InitControl()
        {
            ReloadNeeded = false;
            LoadedEntity = null;

            ClearForm();
        }

        public bool GetReloadState()
        {
            return ReloadNeeded;
        }

        public GenericDatabaseEntity GetLoadedEntity()
        {
            return LoadedEntity;
        }

        public void LoadEntity<T>(GenericDatabaseEntity loadedEntity, bool loadValuesOnly = false)
        {
            LoadEntity(typeof(T), loadedEntity, false);
        }

        public void LoadEntity(Type type, GenericDatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            InitControl();
            LoadToControl(loadedEntity, loadValuesOnly);
        }

        protected void OnResetButtonClicked(object sender, EventArgs e)
        {
            ReloadNeeded = false;
            ClearForm();
        }

        protected void OnCancelButtonClicked()
        {
            OnCancelButtonClicked(null, EventArgs.Empty);
        }

        protected void OnCancelButtonClicked(object sender, EventArgs e)
        {
            ReloadNeeded = false;
        }

        protected bool OnUpdateButtonClicked()
        {
            OnUpdateButtonClicked(null, EventArgs.Empty);
            return false;
        }

        public GenericDatabaseEntity GetUpdatedEntity()
        {
            return GetUpdatedEntity(false);
        }

        public GenericDatabaseEntity GetUpdatedEntity(bool createNew)
        {
            GenericDatabaseEntity entity = null;
            if (createNew || LoadedEntity == null)
            {
                entity = Activator.CreateInstance(_type) as GenericDatabaseEntity;
                if (LoadedEntity != null)
                {
                    entity.Set(LoadedEntity, true, false);
                }
            }
            else
            {
                entity = LoadedEntity;
            }

            entity.ValidateForm(this);
            return entity;
        }

        protected void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            var updating = LoadedEntity != null;
            try
            {
                var entity = GetUpdatedEntity(true);
                if (entity != null && ValidateCreatedEntity(ref entity))
                {
                    var status = false;
                    try
                    {
                        if (updating)
                        {
                            status = entity.Update();
                            MessageBox.Show(this,
                                status ? Resources.RecordUpdate_SUCCESS : Resources.RecordUpdate_UNSUCCESSFUL,
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            status = entity.Add();
                            MessageBox.Show(this,
                                status ? Resources.RecordInsertion_SUCCESS : Resources.RecordInsertion_UNSUCCESSFUL,
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (DatabaseException ex)
                    {
                        if (updating && (ex.GetErrorType() & DatabaseException.ErrorType.NonExistingRecord) == DatabaseException.ErrorType.NonExistingRecord)
                        {
                            status = entity.Add();
                            MessageBox.Show(this,
                                status ? Resources.RecordInsertion_SUCCESS : Resources.RecordInsertion_UNSUCCESSFUL,
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ReloadNeeded = status;
                        }
                        else if (!updating &&
                                 ((ex.GetErrorType() & DatabaseException.ErrorType.RecordAlreadyExists) ==
                                  DatabaseException.ErrorType.RecordAlreadyExists ||
                                  (ex.GetErrorType() & DatabaseException.ErrorType.AlreadyExistingUnqiueField) ==
                                  DatabaseException.ErrorType.AlreadyExistingUnqiueField))
                        {
                            if (MessageBox.Show(Resources.RecordExists, "Existing Record Found", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                status = entity.Update();
                                MessageBox.Show(this, status
                                    ? Resources.RecordUpdate_SUCCESS
                                    : Resources.RecordUpdate_UNSUCCESSFUL,
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ReloadNeeded = status;
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                "Unexpected scenario detected. This will be logged with the action details. The application was not able to process the action you requested !",
                                "Program Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }

                    if (status == false)
                    {
                        return;
                    }

                    if (LoadedEntity == null)
                    {
                        LoadedEntity = entity;
                    }
                    else
                    {
                        LoadedEntity.Set(entity);
                    }

                    OnEntityUpdated(LoadedEntity);
                    ClearForm();

                    ReloadNeeded = true;

                    if (OnOkClicked != null)
                    {
                        OnOkClicked();
                    }
                }
                else
                {
                    MessageBox.Show(this, "Please fill all the required fields !", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(this, ex.GetErrorMessage(), "Invalid Details", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void SetOkButton(Button button)
        {
            button.Click += OnUpdateButtonClicked;
        }

        public void SetResetButton(Button button)
        {
            button.Click += OnResetButtonClicked;
        }

        public void SetCancelButton(Button button)
        {
            button.Click += OnCancelButtonClicked;
        }
    }
}
