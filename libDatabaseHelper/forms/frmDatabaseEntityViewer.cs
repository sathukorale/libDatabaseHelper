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

    public partial class frmDatabaseEntityViewer : Form
    {
        private static Dictionary<Type, frmDatabaseEntityViewer> _presentedForms = new Dictionary<Type, frmDatabaseEntityViewer>();
        private static Dictionary<Type, Type> _registeredEditorTypes = new Dictionary<Type, Type>();

        private bool _isUpdated;
        private Selector[] _selectors;
        private Type _currentType;

        private frmDatabaseEntityViewer()
        {
            InitializeComponent();
            FormUtils.MakeControlAndSubControlsSensitiveToKey(this, new[] { Keys.Escape }, o => { Close(); return 0; });
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
            if (dgvDatabaseEntities.SelectedRows.Count <= 0)
                return null;

            return dgvDatabaseEntities.SelectedRows[0].Tag as DatabaseEntity;
        }

        public void ViewItems<T>(Selector[] selectors)
        {
            ViewItems(typeof(T), selectors);
        }

        public static void ShowWindow<T>(Selector[] selectors)
        {
            ShowWindow(typeof(T), selectors);
        }

        public static frmDatabaseEntityViewer ShowNonModalWindow<T>(Selector[] selectors)
        {
            return ShowNonModalWindow(typeof(T), selectors);
        }

        public static Result ShowAndGetItems<T>(Selector[] selectors)
        {
            return ShowAndGetItems(typeof (T), selectors);
        }

        public static DatabaseEntity ShowAndGetSelectedItem<T>(Selector[] selectors)
        {
            return ShowAndGetSelectedItem(typeof(T), selectors);
        }

        public static DatabaseEntity ShowAndGetSelectedItem(Type type, Selector[] selectors)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new frmDatabaseEntityViewer();
            _presentedForms[type] = presentedForm;
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

            btnAddDatabaseEntity.Enabled = _registeredEditorTypes.ContainsKey(type);


            var instance = Activator.CreateInstance(type) as GenericDatabaseEntity;
            GenericDatabaseManager.GetDatabaseManager(instance.GetSupportedDatabaseType()).FillDataGridViewAsItems(_currentType, ref dgvDatabaseEntities, selectors);
        }

        public static void ShowWindow(Type type, Selector[] selectors)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new frmDatabaseEntityViewer();
            _presentedForms[type] = presentedForm;
            presentedForm.ViewItems(type, selectors);
            presentedForm.ShowDialog();
        }

        public static frmDatabaseEntityViewer ShowNonModalWindow(Type type, Selector[] selectors)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new frmDatabaseEntityViewer();
            _presentedForms[type] = presentedForm;
            presentedForm.ViewItems(type, selectors);
            presentedForm.Show();
            return presentedForm;
        }

        public static Result ShowAndGetItems(Type type, Selector[] selectors)
        {
            var presentedForm = _presentedForms.ContainsKey(type) ? _presentedForms[type] : null;
            if (presentedForm == null || presentedForm.IsDisposed)
                presentedForm = new frmDatabaseEntityViewer();
            _presentedForms[type] = presentedForm;
            presentedForm.ViewItems(type, selectors);
            presentedForm.ShowDialog();
            return presentedForm.GetItems();
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
            if (_registeredEditorTypes.ContainsKey(_currentType))
            {
                var status = DatabaseEntityForm.ShowModalWindow(_registeredEditorTypes[_currentType]);

                Select();
                Focus();

                if (status.UpdateState)
                {
                    ViewItems(_currentType, _selectors);
                    _isUpdated = true;
                }
            }
        }

        private void btnRemoveDatabaseEntity_Click(object sender, EventArgs e)
        {
            if (dgvDatabaseEntities.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Are you that you want to remove the selected record(s)?", "Remove Record(s) ?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                foreach (DataGridViewRow row in dgvDatabaseEntities.SelectedRows)
                {
                    var tag = row.Tag as DatabaseEntity;
                    if (tag != null)
                    {
                        _isUpdated = true;
                        tag.Remove();
                    }
                    dgvDatabaseEntities.Rows.Remove(row);
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
            var row = dgvDatabaseEntities.Rows[e.RowIndex];
            if (row != null && row.Tag != null)
            {
                var obj = row.Tag as DatabaseEntity;
                if (obj != null)
                {
                    var status = DatabaseEntityForm.ShowModalWindow(_registeredEditorTypes[_currentType], obj);

                    Select();
                    Focus();

                    if (status.UpdateState)
                    {
                        ViewItems(_currentType, _selectors);
                        _isUpdated = true;
                    }
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
