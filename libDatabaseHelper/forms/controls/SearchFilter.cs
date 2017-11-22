using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;
using libDatabaseHelper.classes.sqlce;

namespace libDatabaseHelper.forms.controls
{
    public partial class SearchFilter : UserControl
    {
        public enum SearchMode
        {
            Simple,
            Advanced  
        }

        private Type ClassType;
        private SearchFilterSettings FilterSettings;
        private SearchMode Mode;

        private int HeightOnSimpleMode;
        private int HeightOnAdvancedMode;

        public delegate void SearchEventTriggeredEventHandler(SearchFilter sender, Type classType, Selector[] selectors);
        public event SearchEventTriggeredEventHandler OnSearchEventTriggered;

        [Category("Data")]
        [Description("The DataGridView which is to show the results.")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public DataGridView ResultsView { get; set; }

        public SearchFilter()
        {
            InitializeComponent();

            Mode = SearchMode.Simple;

            HeightOnAdvancedMode = txtMainSearchFilter.Height + lnkAdvancedSettings.Height + flpAdvancedSettings.Height + (5 * 4);
            HeightOnSimpleMode = txtMainSearchFilter.Height + lnkAdvancedSettings.Height + (5 * 2);
        }

        public void SetClassType(Type classType)
        {
            var classInstance = GenericDatabaseEntity.GetNonDisposableRefenceObject(classType);
            if (classInstance == null)
            {
                throw new Exception("The class type '" + classType.FullName + "' should be derived from 'GenericDatabaseEntity' for it to be used with 'SearchFilterControl'");
            }

            var columns = classInstance.GetColumns();
            var allColumns = columns.GetPrimaryKeys().ToList();
            allColumns.AddRange(columns.GetOtherColumns());

            var filteredColumnsSimpleSearch = allColumns.Select(i => GetSearchableField(i)).Where(i => i != null).ToArray();
            var filteredColumnsAdvancedSearch = allColumns.Select(i => GetSearchableField(i, true)).Where(i => i != null).ToArray();

            cntxSearchFilters.Items.AddRange(filteredColumnsSimpleSearch);
            cntxAdvancedSearchFilters.Items.AddRange(filteredColumnsAdvancedSearch);

            var filterSettings = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SearchFilterSettings>(new Selector[] { new Selector("ClassName", classType.FullName) });
            if (filterSettings.Any())
            {
                FilterSettings = (SearchFilterSettings) filterSettings.First();
            }
            else
            {
                if (!filteredColumnsSimpleSearch.Any()) return;

                var defaultSearchFieldName = cntxSearchFilters.Items[0].Tag as string;
                FilterSettings = new SearchFilterSettings() { ClassName = classType.FullName, DefaultSearchFieldName = defaultSearchFieldName };
                FilterSettings.Add();
            }

            ApplyFilterSettings(FilterSettings);

            ClassType = classType;
        }

        private ToolStripItem GetSearchableField(FieldInfo fieldInfo, bool isAdvancedSearch = false)
        {
            if (isAdvancedSearch == false && 
                GenericFieldTools.IsTypeNumber(fieldInfo.FieldType) == false && 
                GenericFieldTools.IsTypeString(fieldInfo.FieldType) == false) return null;

            var columnInfo = fieldInfo.GetCustomAttributes(typeof(TableColumn), true).Select(ii => ii as TableColumn).FirstOrDefault();
            if (columnInfo == null || columnInfo.IsASearchFilter == false || string.IsNullOrWhiteSpace(columnInfo.GridDisplayName)) return null;

            return new ToolStripMenuItem(columnInfo.GridDisplayName) { CheckState = CheckState.Unchecked, Tag = fieldInfo.Name };
        }

        private void ApplyFilterSettings(SearchFilterSettings filterSettings)
        {
            foreach (ToolStripMenuItem item in cntxSearchFilters.Items)
            {
                var fieldName = item.Tag as string;
                if (fieldName != filterSettings.DefaultSearchFieldName)
                {
                    item.CheckState = CheckState.Unchecked;
                }
                else
                {
                    item.CheckState = CheckState.Checked;
                    FormUtils.AddPlaceHolder(txtMainSearchFilter, (item.Text ?? "") + "...");
                    txtMainSearchFilter.Tag = item.Text;
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Selector[] selectors = null;
            if (Mode == SearchMode.Simple)
            {
                selectors = new[] { new Selector(txtMainSearchFilter.Tag as string, string.IsNullOrWhiteSpace(txtMainSearchFilter.Text) ? "*" : txtMainSearchFilter.Text) };
            }
            else
            {
                selectors = flpAdvancedSettings.Controls.OfType<SearchFilterControl>().Select(i => i.GetSearchFilter()).ToArray();
            }

            if (OnSearchEventTriggered != null) OnSearchEventTriggered.Invoke(this, ClassType, selectors);

            var instance = GenericDatabaseEntity.GetNonDisposableRefenceObject(ClassType);
            GenericDatabaseManager.GetDatabaseManager(instance.GetSupportedDatabaseType()).FillDataGridView(ClassType, ResultsView);
        }

        private void btnChangeDefaultFilter_Click(object sender, EventArgs e)
        {
            var btnSender = (Button)sender;
            var point = new Point(0, btnSender.Height);

            cntxSearchFilters.Show(btnSender.PointToScreen(point));
        }

        private void btnAddField_Click(object sender, EventArgs e)
        {
            var btnSender = (Button)sender;
            var point = new Point(0, btnSender.Height);

            foreach (ToolStripMenuItem item in cntxAdvancedSearchFilters.Items)
            {
                item.CheckState = CheckState.Unchecked;
            }

            var fieldNames = flpAdvancedSettings.Controls.OfType<SearchFilterControl>().Select(i => i.FieldName).ToArray();
            foreach (var fieldName in fieldNames)
            {
                var toolStripItem = cntxAdvancedSearchFilters.Items.OfType<ToolStripMenuItem>().FirstOrDefault(i => (i.Tag as string) == fieldName);
                if (toolStripItem != null)
                {
                    toolStripItem.CheckState = CheckState.Checked;
                }
            }

            cntxAdvancedSearchFilters.Show(btnSender.PointToScreen(point));
        }

        private void cntxSearchFilters_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                FilterSettings.DefaultSearchFieldName = e.ClickedItem.Tag as string;
                FilterSettings.Update();

                ApplyFilterSettings(FilterSettings);
            }
            catch { /* IGNORED */ }
        }

        private void cntxAdvancedSearchFilters_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var fieldName = e.ClickedItem.Tag as string;
            if (flpAdvancedSettings.Controls.OfType<SearchFilterControl>().Any(i => i.FieldName == fieldName)) return;

            var filterItem = GetSearchFilterControl(ClassType, e.ClickedItem.Tag as string);
            filterItem.OnRemoveControl += FilterItem_OnRemoveControl;

            flpAdvancedSettings.Controls.Add(filterItem);
            flpAdvancedSettings.Controls.SetChildIndex(btnAddField, flpAdvancedSettings.Controls.Count - 1);
            flpAdvancedSettings.Invalidate();
        }

        private SearchFilterControl GetSearchFilterControl(Type classType, string fieldName)
        {
            var classInstance = GenericDatabaseEntity.GetNonDisposableRefenceObject(classType);
            if (classInstance == null)
            {
                throw new Exception("The class type '" + classType.FullName + "' should be derived from 'GenericDatabaseEntity' for it to be used with 'SearchFilterControl'");
            }

            var fieldInfo = classInstance.GetFieldInfo(fieldName);
            if (fieldInfo == null)
            {
                throw new Exception("The field '" + fieldName + "', could not be found within the class of type '" + classType.FullName + "'");
            }

            if (GenericFieldTools.IsTypeNumber(fieldInfo.FieldType) || GenericFieldTools.IsTypeString(fieldInfo.FieldType) || GenericFieldTools.IsTypeFloatingPoint(fieldInfo.FieldType))
                return new NamedTextBox(classType, fieldName);

            if (GenericFieldTools.IsTypeDate(fieldInfo.FieldType))
                return new NamedDateTimePicker(classType, fieldName);

            if (GenericFieldTools.IsTypeBool(fieldInfo.FieldType))
                return new NamedComboBox(classType, fieldName);

            return new NamedTextBox(classType, fieldName);
        }

        private void FilterItem_OnRemoveControl(SearchFilterControl sender)
        {
            flpAdvancedSettings.Controls.Remove(sender);
        }

        private void lnkAdvancedSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ToggleSearchMode();
        }

        private void ToggleSearchMode()
        {
            if (Mode == SearchMode.Simple)
            {
                SwitchToAdvancedFilterMode();
            }
            else
            {
                SwitchToSimpleFilterMode();
            }
        }

        private void SwitchToSimpleFilterMode()
        {
            Mode = SearchMode.Simple;
            txtMainSearchFilter.Enabled = true;
            btnChangeDefaultFilter.Enabled = true;
            Height = HeightOnSimpleMode;
            lnkAdvancedSettings.Text = "Show More Options";

            Invalidate();
        }

        private void SwitchToAdvancedFilterMode()
        {
            Mode = SearchMode.Advanced;
            txtMainSearchFilter.Enabled = false;
            btnChangeDefaultFilter.Enabled = false;
            Height = HeightOnAdvancedMode;
            lnkAdvancedSettings.Text = "Show Less Options";

            Invalidate();
        }

        private void SearchFilter_Load(object sender, EventArgs e)
        {
            SwitchToSimpleFilterMode();
        }
    }

    public class SearchFilterSettings : DatabaseEntity
    {
        [TableColumn(true, true)]
        public string ClassName;

        [TableColumn(false, true)]
        public string DefaultSearchFieldName;
    }
}
