using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.classes.sqlce;
using libDatabaseHelper.Properties;
using libDatabaseHelper.classes.sqlce.entities;

namespace libDatabaseHelper.forms
{
    public class DatabaseEntityForm : Form
    {
        protected bool ReloadNeeded;
        protected bool Inited = false;
        protected DatabaseEntity LoadedEntity;

        public Type _type;
        private string[] _drag_drop_supporting_extensions;

        private static Dictionary<Type, DatabaseEntityForm> _listOfForms;

        public delegate bool OnOkClickedEvent();
        public event OnOkClickedEvent OnOkClicked;

        public struct ReturnStatus
        {
            public DatabaseEntity EntityUpdated;
            public bool UpdateState;

            public ReturnStatus(DatabaseEntity updatedEntity, bool updateState)
            {
                EntityUpdated = updatedEntity;
                UpdateState = updateState;
            }
        }

        public void ClearForm()
        {
            ReloadNeeded = false;
            FormUtils.ClearForm(this);
        }

        protected virtual bool ValidateCreatedEntity(ref DatabaseEntity entity)
        {
            return true;
        }

        protected void MakeWindowDragDroppable(string[] supported_extensions)
        {
            _drag_drop_supporting_extensions = supported_extensions;
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
                            if (_drag_drop_supporting_extensions.Any(ext => ext == extension))
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
                            foreach (var ext in _drag_drop_supporting_extensions)
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
                e.Effect = ret ? DragDropEffects.Copy : DragDropEffects.None;
            };
        }

        protected virtual void OnFileDragDropped(string file_name)
        {

        }

        public DatabaseEntityForm GetFormInstance<T>()
        {
            if (_listOfForms == null)
                _listOfForms = new Dictionary<Type, DatabaseEntityForm>();
            var presentedForm = _listOfForms[typeof(T)];
            if (presentedForm == null || presentedForm.IsDisposed)
            {
                presentedForm = Activator.CreateInstance<T>() as DatabaseEntityForm;
                _listOfForms.Add(typeof(T), presentedForm);
            }
            return presentedForm;
        }

        private void LoadToForm(DatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            if (loadedEntity != null)
            {
                loadedEntity.LoadToForm(this);
                if (! loadValuesOnly)
                {
                    LoadedEntity = loadedEntity;
                }
            }
        }

        public void InitWindow()
        {
            ReloadNeeded = false;
            LoadedEntity = null;
            ClearForm();
            PerformOneTimeInits();
        }

        private void PerformOneTimeInits()
        {
            if (! Inited)
            {
                FormUtils.MakeControlAndSubControlsSensitiveToKey(this, new[] { Keys.Escape }, (key) => { Close(); return 0; });
                Inited = true;
            }
        }

        public bool GetReloadState()
        {
            return ReloadNeeded;
        }

        public DatabaseEntity GetLoadedEntity()
        {
            return LoadedEntity;
        }

        public static DatabaseEntityForm ShowWindow<T>()
        {
            return ShowWindow<T>(null);
        }

        public static ReturnStatus ShowModalWindow<T>()
        {
            return ShowModalWindow<T>(null);
        }

        public static ReturnStatus ShowModalWindow(Type type)
        {
            return ShowModalWindow(type, null);
        }

        public static DatabaseEntityForm ShowWindow<T>(DatabaseEntity loadedEntity)
        {
            return ShowWindow(typeof (T), loadedEntity, false);
        }

        public static DatabaseEntityForm ShowWindow<T>(DatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            return ShowWindow(typeof(T), loadedEntity, loadValuesOnly);
        }

        public static DatabaseEntityForm ShowWindow(Type type, DatabaseEntity loadedEntity)
        {
            return ShowWindow(type, loadedEntity, false);
        }

        public static DatabaseEntityForm ShowWindow(Type type, DatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            if (_listOfForms == null)
                _listOfForms = new Dictionary<Type, DatabaseEntityForm>();
            var presentedForm = _listOfForms.Any(i => i.Key == type) ? _listOfForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
            {
                presentedForm = Activator.CreateInstance(type) as DatabaseEntityForm;
                _listOfForms.Add(type, presentedForm);
            }
            if (presentedForm == null)
                throw new Exception("Unable to create window [" + type.Name + "]");

            presentedForm.InitWindow();
            presentedForm.LoadToForm(loadedEntity, loadValuesOnly);
            presentedForm.PerformPreWindowShowActions();

            presentedForm.Show();
            return presentedForm;
        }

        public static ReturnStatus ShowModalWindow<T>(DatabaseEntity loadedEntity)
        {
            return ShowModalWindow<T>(loadedEntity, false);
        }

        public static ReturnStatus ShowModalWindow<T>(DatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            return ShowModalWindow(typeof(T), loadedEntity, loadValuesOnly);
        }

        public static ReturnStatus ShowModalWindow(Type type, DatabaseEntity loadedEntity)
        {
            return ShowModalWindow(type, loadedEntity, false);
        }

        public static ReturnStatus ShowModalWindow(Type type, DatabaseEntity loadedEntity, bool loadValuesOnly)
        {
            if (_listOfForms == null)
                _listOfForms = new Dictionary<Type, DatabaseEntityForm>();
            var presentedForm = _listOfForms.ContainsKey(type) ? _listOfForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
            {
                presentedForm = Activator.CreateInstance(type) as DatabaseEntityForm;
                _listOfForms.Remove(type);
                _listOfForms.Add(type, presentedForm);
            }
            if (presentedForm == null)
                throw new Exception("Unable to create window [" + type.Name + "]");

            presentedForm.InitWindow();
            presentedForm.LoadToForm(loadedEntity, loadValuesOnly);
            presentedForm.PerformPreWindowShowActions();

            presentedForm.ShowDialog();
            return new ReturnStatus(presentedForm.GetLoadedEntity(), presentedForm.GetReloadState());
        }

        protected void PerformPreWindowShowActions()
        { 
        
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
            Hide();
        }

        protected bool OnUpdateButtonClicked()
        {
            OnUpdateButtonClicked(null, EventArgs.Empty);
            return false;
        }

        public DatabaseEntity GetUpdatedEntity()
        {
            return GetUpdatedEntity(false);
        }

        public DatabaseEntity GetUpdatedEntity(bool create_new)
        {
            var entity = (create_new ? null : LoadedEntity) ?? Activator.CreateInstance(_type) as DatabaseEntity;
            entity.ValidateForm(this);
            return entity;
        }

        protected void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            var updating = LoadedEntity != null;
            try
            {
                var entity = GetUpdatedEntity();
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
                        else if (! updating &&
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
                            AuditEntry.AddAuditEntry(new ApplicationError
                            {
                                Entity = entity,
                                ExceptionString = ex.Message
                            }, "ApplicationError");
                        }
                    }
                    catch (Exception ex)
                    {
                        AuditEntry.AddAuditEntry(new ApplicationError
                        {
                            Entity = entity,
                            ExceptionString = ex.Message
                        }, "ApplicationError");
                    }
                    LoadedEntity = entity;
                    if (!status) return;
                    ClearForm();
                    Hide();
                    ReloadNeeded = true;
                    if (OnOkClicked != null)
                        OnOkClicked();
                }
                else
                {
                    MessageBox.Show(this, "Please fill all the required fields !", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(this, ex.GetErrorMessage(), "Error", MessageBoxButtons.OK,
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
