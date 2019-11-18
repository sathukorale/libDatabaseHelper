using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;
using libDatabaseHelper.classes.sqlce;

namespace libDatabaseHelper.forms
{
    public struct Result
    {
        public bool IsUpdated;
        public DatabaseEntity[] ViewedItems;
    };

    public partial class DatabaseEntityViewer : Form
    {
        private static Dictionary<Type, DatabaseEntityViewer> _presentedForms = new Dictionary<Type, DatabaseEntityViewer>();
        private static Dictionary<Type, Type> _registeredEditorTypes = new Dictionary<Type, Type>();

        private bool _isUpdated;
        private Selector[] _selectors;
        private Type _currentType = null;
        private DatabaseEntity _selectedItem;

        private DatabaseEntityViewer()
        {
            InitializeComponent();
            FormUtils.MakeControlAndSubControlsSensitiveToKey(this, new[] { Keys.Escape }, o => { Close(); return true; });
            GenericDatabaseEntity.OnDatabaseEntityUpdated += DatabaseEntity_OnDatabaseEntityUpdated;
        }

        private void DatabaseEntity_OnDatabaseEntityUpdated(GenericDatabaseEntity updatedEntity, Type type, GenericDatabaseEntity.UpdateEventType event_type)
        {
            if (type == _currentType && event_type == GenericDatabaseEntity.UpdateEventType.Remove)
            {
                try
                {
                    ViewItems(type, _selectors);
                }
                catch { }
            }
        }

        public Result GetItems()
        {
            return new Result
            {
                IsUpdated = _isUpdated,
                ViewedItems = dgvDatabaseEntities.Rows.OfType<DataGridViewRow>().Where(i => i.Tag is DatabaseEntity).Select(i => i.Tag as DatabaseEntity).ToArray()
            };
        }

        public DatabaseEntity GetSelectedItem()
        {
            return _selectedItem;
        }

        public void ViewItems<T>(Selector[] selectors, IWin32Window owner = null)
        {
            ViewItems(typeof(T), selectors);
        }

        public static void ShowWindow<T>(IWin32Window owner = null)
        {
            ShowWindow(typeof(T), null, owner);
        }

        public static void ShowWindow<T>(Selector[] selectors, IWin32Window owner = null)
        {
            ShowWindow(typeof(T), selectors, owner);
        }

        public static DatabaseEntityViewer ShowNonModalWindow<T>(Selector[] selectors, IWin32Window owner = null)
        {
            return ShowNonModalWindow(typeof(T), selectors, owner);
        }

        public static Result ShowAndGetItems<T>(Selector[] selectors, IWin32Window owner = null)
        {
            return ShowAndGetItems(typeof (T), selectors, owner);
        }

        public static DatabaseEntity ShowAndGetSelectedItem<T>(Selector[] selectors, IWin32Window owner = null)
        {
            return ShowAndGetSelectedItem(typeof(T), selectors, owner);
        }

        public static DatabaseEntity ShowAndGetSelectedItem(Type type, Selector[] selectors, IWin32Window owner = null)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new DatabaseEntityViewer();
            _presentedForms[type] = presentedForm;

            presentedForm.InitAsSelectorDialog();
            presentedForm.ViewItems(type, selectors);
            presentedForm.ShowDialog();

            return presentedForm.GetSelectedItem();
        }

        public void ViewItems(Type type, Selector[] selectors)
        {
            if (selectors == null)
                selectors = new Selector[0];

            _isUpdated = false;
            _currentType = type;
            _selectors = selectors;
            _selectedItem = null;
            var typeName = _currentType.Name;
            try
            {
                var attributes = System.Attribute.GetCustomAttributes(type).OfType<TableProperties>().FirstOrDefault();
                if (attributes != null)
                {
                    typeName = attributes.DisplayName;
                }
            }
            catch { }
            Text = typeName + " Viewer";

            FormUtils.AddToolTip(btnAddDatabaseEntity, "Add New " + typeName);
            FormUtils.AddToolTip(btnRemoveDatabaseEntity, "Remove Selected " + typeName + "(s)");
            FormUtils.AddToolTip(btnCopyEntity, "Copy Selected " + typeName);

            btnAddDatabaseEntity.Enabled = _registeredEditorTypes.ContainsKey(type);
            btnCopyEntity.Enabled = _registeredEditorTypes.ContainsKey(type);

            var instance = GenericDatabaseEntity.GetNonDisposableReferenceObject(type);
            if (instance != null)
            {
                GenericDatabaseManager.GetDatabaseManager(instance.GetSupportedDatabaseType()).FillDataGridViewAsItems(_currentType, ref dgvDatabaseEntities, selectors);
            }
        }

        private void InitAsSelectorDialog()
        {
            btnClose.Text = @"Select";
        }

        private void InitAsRegularDialog()
        {
            btnClose.Text = @"Close";
        }

        public static void ShowWindow(Type type, Selector[] selectors, IWin32Window owner = null)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new DatabaseEntityViewer();
            _presentedForms[type] = presentedForm;
            presentedForm.ShowInTaskbar = owner == null;

            presentedForm.InitAsRegularDialog();
            presentedForm.ViewItems(type, selectors);
            presentedForm.ShowDialog(owner);
        }

        public static DatabaseEntityViewer ShowNonModalWindow(Type type, Selector[] selectors, IWin32Window owner = null)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new DatabaseEntityViewer();
            _presentedForms[type] = presentedForm;

            presentedForm.ShowInTaskbar = owner == null;
            presentedForm.InitAsRegularDialog();
            presentedForm.ViewItems(type, selectors);
            presentedForm.Show(owner);
            return presentedForm;
        }

        public static Result ShowAndGetItems(Type type, Selector[] selectors, IWin32Window owner = null)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new DatabaseEntityViewer();
            _presentedForms[type] = presentedForm;

            presentedForm.ShowInTaskbar = owner == null;
            presentedForm.InitAsRegularDialog();
            presentedForm.ViewItems(type, selectors);
            presentedForm.ShowDialog(owner);
            return presentedForm.GetItems();
        }

        public static void RegisterEditor<T>()
        {
            var instance = Activator.CreateInstance<T>() as DatabaseEntityForm;
            if (instance == null)
            {
                throw new InvalidCastException("Unable to Register Editor Window, Since the Registered Window is not of the Type DatabaseEntityForm.");
            }

            if (!_registeredEditorTypes.ContainsKey(instance.GetExtendedType()))
            {
                _registeredEditorTypes.Add(instance.GetExtendedType(), typeof(T));
            }
        }

        public static void RegisterEditor<T>(Type frmType)
        {
            if (! _registeredEditorTypes.ContainsKey(typeof (T)))
            {
                _registeredEditorTypes.Add(typeof(T), frmType);
            }
        }

        private void btnAddDatabaseEntity_Click(object sender, EventArgs e)
        {
            if (_registeredEditorTypes.ContainsKey(_currentType) == false) return;

            var form = DatabaseEntityForm.GetFormInstance(_registeredEditorTypes[_currentType]);
            if (form == null) return;

            form.Owner = this;
            form.ShowInTaskbar = false;

            var status = DatabaseEntityForm.ShowModalWindow(_registeredEditorTypes[_currentType]);

            Select();
            Focus();

            if (status.UpdateState)
            {
                ViewItems(_currentType, _selectors);
                _isUpdated = true;
            }
        }

        private void btnRemoveDatabaseEntity_Click(object sender, EventArgs e)
        {
            if (dgvDatabaseEntities.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Are you that you want to remove the selected record(s)?", "Remove Record(s) ?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                for ( int i = 0 ; i < dgvDatabaseEntities.SelectedRows.Count ; i++ )
                {
                    try
                    {
                        var row = dgvDatabaseEntities.SelectedRows[i];
                        var tag = row.Tag as DatabaseEntity;
                        if (tag != null)
                        {
                            _isUpdated = true;
                            tag.Remove();
                        }
                        dgvDatabaseEntities.Rows.Remove(row);
                    }
                    catch { }
                }
            }
            else
            {
                MessageBox.Show("Please select at least one record from the record list !", "No Record(s) Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void dgvDatabaseEntities_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvDatabaseEntities.RowCount)
            {
                var row = dgvDatabaseEntities.Rows[e.RowIndex];
                var obj = row.Tag as DatabaseEntity;

                if (obj == null) return;

                Type registeredEditorType = null;
                if (_registeredEditorTypes.TryGetValue(_currentType, out registeredEditorType) == false) return;

                var status = DatabaseEntityForm.ShowModalWindow(registeredEditorType, obj, false, this);

                Select();
                Focus();

                if (status.UpdateState)
                {
                    ViewItems(_currentType, _selectors);
                    _isUpdated = true;
                }
            }
        }

        private void btnCopyEntity_Click(object sender, EventArgs e)
        {
            if (dgvDatabaseEntities.Rows.Count <= 0)
            {
                MessageBox.Show("No records are available to perform this operation !", "No Records Available",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgvDatabaseEntities.SelectedRows.Count == 1)
            {
                var tag = dgvDatabaseEntities.SelectedRows[0].Tag as DatabaseEntity;
                if (_registeredEditorTypes.ContainsKey(_currentType))
                {
                    var status = DatabaseEntityForm.ShowModalWindow(_registeredEditorTypes[_currentType], tag, true);

                    Select();
                    Focus();

                    if (status.UpdateState)
                    {
                        ViewItems(_currentType, _selectors);
                        _isUpdated = true;
                    }
                }
            }
            else if (dgvDatabaseEntities.SelectedRows.Count > 1)
            {
                MessageBox.Show("Please select only one record from the record list !", "Multiple Records Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                MessageBox.Show("Please select at least one record from the record list !", "No Record(s) Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _selectedItem = dgvDatabaseEntities.SelectedRows[0].Tag as DatabaseEntity;
            Close();
        }

        private void frmDatabaseEntityViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*foreach (DataGridViewRow row in dgvDatabaseEntities.Rows)
            {
                row.Selected = false;
            }*/
        }
    }
}
