using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using libDatabaseHelper.classes.sqlce;
using DatabaseEntityFormGenerator.Properties;
using libDatabaseHelper.classes.generic;
using libDatabaseHelper.forms;

namespace DatabaseEntityFormCreator
{
    class Creator
    {
        public enum EntityControlType
        {
            DatabaseEntityForm,
            DatabaseEntityUserControl
        }

        public static bool CreateForm(string filepath, EntityControlType entityControlType)
        {
            if (!File.Exists(filepath))
            {
                MessageBox.Show("Unable to find the file specified !", 
                    "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var parentType = entityControlType == EntityControlType.DatabaseEntityForm ? "DatabaseEntityForm" : "DatabaseEntityUserControl";
            var classPrefix = entityControlType == EntityControlType.DatabaseEntityForm ? "frm" : "ctrl";
            var assembly = Assembly.LoadFile(filepath);
            var types = assembly.GetTypes();
            var matchingTypes = types.Where(i => i.BaseType != null && i.BaseType.FullName == typeof(DatabaseEntity).FullName).ToArray();

            matchingTypes = frmMatchingTypes.ShowWindow(matchingTypes);

            if (!matchingTypes.Any())
            {
                MessageBox.Show("Unable to find any classes that extends \"Database Entity\" !",
                    "No Supported Classes Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using (var fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() != DialogResult.OK) 
                    return true;

                foreach (var type in matchingTypes)
                {
                    var instance = GenericDatabaseEntity.GetNonDisposableReferenceObject(type);

                    if (instance == null) continue;

                    var className = classPrefix + type.Name;

                    var userCodeString = Resources.DBEntityForm_UserCode.Replace("NAMESPACE", type.Namespace).Replace("CLASS_NAME", className).Replace("ENTITY_CONTROL_TYPE", parentType);

                    var declarationCode = "";
                    var initCode = "";
                    var settingsCode = "";
                    var addingCode = "";
                    var tagCode = "";

                    var loadingCodeCalls = "";
                    var loadingCode = "";
                    var row = 0;
                    var tabIndex = 0;

                    var loadingCodeEntityUpdateHandlers = "";

                    loadingCodeCalls += Environment.NewLine + "\t\t\tSetEntityType(typeof(" + type.Name + "));";

                    foreach (var column in instance.GetColumns(true).GetOtherColumns())
                    {
                        var attr = column.GetCustomAttributes(typeof(TableColumn), true)[0] as TableColumn;
                        if (attr == null)
                            continue;

                        if (attr.ReferencedClass != null && attr.ReferencedField != null)
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.ComboBox cmb{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine);
                            initCode += String.Format("\t\tthis.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("\t\tthis.cmb{0} = new System.Windows.Forms.ComboBox();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 0, 0);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point(0, 0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size(110, 25);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);" + Environment.NewLine, column.Name);

                            settingsCode += String.Format("\t\tthis.cmb{0}.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 130, 90);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 160, 20);
                            settingsCode += String.Format("\t\tthis.cmb{0}.TabIndex = {1};" + Environment.NewLine, column.Name, tabIndex);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;", column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;", column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(lbl{0}, 0, {1});", column.Name, row);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(cmb{0}, 2, {1});", column.Name, row);

                            loadingCodeCalls += Environment.NewLine + "\t\t\tLoad" + column.Name + "s();";

                            loadingCode += Environment.NewLine + Environment.NewLine + "\t\t" + String.Format(Resources.ComboLoadingCode, column.Name, attr.ReferencedClass.Name, instance.GetSupportedDatabaseType() == DatabaseType.SqlCE ? "DatabaseType.SqlCE" : "DatabaseType.MySQL").Replace(Environment.NewLine, Environment.NewLine + "\t\t");

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\tcmb{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            loadingCodeEntityUpdateHandlers += "\t" + (loadingCodeEntityUpdateHandlers == "" ? "if" : "else if") + " (type == typeof(" + attr.ReferencedClass.Name + "))" + Environment.NewLine;
                            loadingCodeEntityUpdateHandlers += "\t{" + Environment.NewLine;
                            loadingCodeEntityUpdateHandlers += "\t\tLoad" + column.Name + "s();" + Environment.NewLine;
                            loadingCodeEntityUpdateHandlers += "\t}" + Environment.NewLine;

                            row += 2;
                            tabIndex++;
                        }
                        else if (GenericFieldTools.IsTypeString(column.FieldType) || GenericFieldTools.IsTypeString(column.FieldType) || GenericFieldTools.IsTypeFloatingPoint(column.FieldType))
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.TextBox txt{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine);
                            initCode += String.Format("\t\tthis.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("\t\tthis.txt{0} = new System.Windows.Forms.TextBox();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 0, 0);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point(0, 0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size(110, 25);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);" + Environment.NewLine, column.Name);

                            settingsCode += String.Format("\t\tthis.txt{0}.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.txt{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.txt{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 130, 90);
                            settingsCode += String.Format("\t\tthis.txt{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 160, 20);
                            settingsCode += String.Format("\t\tthis.txt{0}.TabIndex = {1};" + Environment.NewLine, column.Name, tabIndex);
                            settingsCode += String.Format("\t\tthis.txt{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.txt{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(lbl{0}, 0, {1});", column.Name, row);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(txt{0}, 2, {1});", column.Name, row);

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\ttxt{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            row += 2;
                            tabIndex++;
                        }
                        else if (column.FieldType == typeof(bool) || column.FieldType == typeof(Boolean))
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.CheckBox chk{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine);
                            initCode += String.Format("\t\tthis.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("\t\tthis.chk{0} = new System.Windows.Forms.CheckBox();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 0, 0);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point(0, 0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size(110, 25);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);" + Environment.NewLine, column.Name);

                            settingsCode += String.Format("\t\tthis.chk{0}.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.chk{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.chk{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 130, 90);
                            settingsCode += String.Format("\t\tthis.chk{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 124, 20);
                            settingsCode += String.Format("\t\tthis.chk{0}.TabIndex = {1};" + Environment.NewLine, column.Name, tabIndex);
                            settingsCode += String.Format("\t\tthis.chk{0}.RightToLeft = {1};" + Environment.NewLine, column.Name, "System.Windows.Forms.RightToLeft.No");
                            settingsCode += String.Format("\t\tthis.chk{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.chk{0}.Text = \"\";", column.Name);
                            settingsCode += String.Format("\t\tthis.chk{0}.TextAlign = System.Drawing.ContentAlignment.MiddleRight;", column.Name);
                            settingsCode += String.Format("\t\tthis.chk{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(lbl{0}, 0, {1});", column.Name, row);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(chk{0}, 2, {1});", column.Name, row);

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\tchk{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            row += 2;
                            tabIndex++;
                        }
                        else if (column.FieldType == typeof(DateTime) || column.FieldType == typeof(Date))
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.DateTimePicker dtp{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine);
                            initCode += String.Format("\t\tthis.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("\t\tthis.dtp{0} = new System.Windows.Forms.DateTimePicker();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 0, 0);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point(0, 0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size(110, 25);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);" + Environment.NewLine, column.Name);

                            settingsCode += String.Format("\t\tthis.dtp{0}.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));", column.Name);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 130, 90);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 160, 20);
                            settingsCode += String.Format("\t\tthis.dtp{0}.TabIndex = {1};" + Environment.NewLine, column.Name, tabIndex);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine, column.Name);
                            settingsCode += String.Format("\t\t\tthis.dtp{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(lbl{0}, 0, {1});", column.Name, row);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.tblFormContainer.Controls.Add(dtp{0}, 2, {1});", column.Name, row);

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\tdtp{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            row += 2;
                            tabIndex++;
                        }
                    }

                    declarationCode += (declarationCode == "" ? "" : Environment.NewLine);
                    declarationCode += "\tprivate System.Windows.Forms.TableLayoutPanel tblContainer;" + Environment.NewLine;
                    declarationCode += "\tprivate System.Windows.Forms.TableLayoutPanel tblFormContainer;" + Environment.NewLine;
                    declarationCode += "\tprivate System.Windows.Forms.TableLayoutPanel tblButtonContainer;" + Environment.NewLine;

                    initCode += (initCode == "" ? "" : Environment.NewLine);
                    initCode += "\t\tthis.tblContainer = new System.Windows.Forms.TableLayoutPanel();" + Environment.NewLine;
                    initCode += "\t\tthis.tblFormContainer = new System.Windows.Forms.TableLayoutPanel();" + Environment.NewLine;
                    initCode += "\t\tthis.tblButtonContainer = new System.Windows.Forms.TableLayoutPanel();" + Environment.NewLine;

                    settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                    settingsCode += "\t\tthis.tblFormContainer.AutoSize = true;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.ColumnCount = 3;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.Font = new System.Drawing.Font(\"Segoe UI\", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.Location = new System.Drawing.Point(5, 5);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.RowCount = " + ((tabIndex * 2) + 1) + ";" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblFormContainer.Size = new System.Drawing.Size(797, 537);" + Environment.NewLine;

                    for (var i = 0; i < tabIndex; i++)
                    {
                        settingsCode += "\t\tthis.tblFormContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));" + Environment.NewLine;
                        settingsCode += "\t\tthis.tblFormContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));" + Environment.NewLine;
                    }

                    settingsCode += "\t\tthis.tblFormContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));" + Environment.NewLine;

                    settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                    settingsCode += "\t\tthis.tblButtonContainer.ColumnCount = 5;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.Controls.Add(this.btnOK, 1, 0);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.Controls.Add(this.btnCancel, 3, 0);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.Location = new System.Drawing.Point(130, 507);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.RowCount = 1;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblButtonContainer.Size = new System.Drawing.Size(667, 30);" + Environment.NewLine;

                    settingsCode += (settingsCode == "" ? "" : Environment.NewLine);
                    settingsCode += "\t\tthis.tblContainer.ColumnCount = 1;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.Dock = System.Windows.Forms.DockStyle.Fill;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.Location = new System.Drawing.Point(130, 507);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.Margin = new System.Windows.Forms.Padding(0);" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.RowCount = 2;" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));" + Environment.NewLine;
                    settingsCode += "\t\tthis.tblContainer.Size = new System.Drawing.Size(667, 30);" + Environment.NewLine;

                    addingCode += (addingCode == "" ? "" : Environment.NewLine);
                    addingCode += String.Format("\t\tthis.tblButtonContainer.Controls.Add(btnReset, 0, 0);" + Environment.NewLine);
                    addingCode += String.Format("\t\tthis.tblButtonContainer.Controls.Add(btnOK, 2, 0);" + Environment.NewLine);
                    addingCode += String.Format("\t\tthis.tblButtonContainer.Controls.Add(btnCancel, 4, 0);" + Environment.NewLine);
                    addingCode += String.Format("\t\tthis.tblContainer.Controls.Add(this.tblFormContainer, 0, 0);" + Environment.NewLine);
                    addingCode += String.Format("\t\tthis.tblContainer.Controls.Add(this.tblButtonContainer, 0, 1);" + Environment.NewLine);
                    addingCode += String.Format("\t\tthis.Controls.Add(tblContainer);" + Environment.NewLine);

                    loadingCodeCalls += (loadingCodeCalls == "" ? "" : Environment.NewLine) + Environment.NewLine;
                    loadingCodeCalls += "\t\t\tGenericDatabaseEntity.OnDatabaseEntityUpdated += DatabaseEntity_OnDatabaseEntityUpdated;" + Environment.NewLine;
                    loadingCodeCalls += "\t\t\tGenericDatabaseManager.OnBulkDelete += GenericDatabaseManager_OnBulkDelete;";

                    loadingCode += Environment.NewLine + Environment.NewLine;
                    loadingCode += "\t\t" + Resources.BulkDeleteHandler.Replace("[HANDLING_CODE]", loadingCodeEntityUpdateHandlers).Replace(Environment.NewLine, Environment.NewLine + "\t\t");
                    loadingCode += Environment.NewLine + Environment.NewLine;
                    loadingCode += "\t\t" + Resources.EntityUpdateHandler.Replace("[HANDLING_CODE]", loadingCodeEntityUpdateHandlers).Replace(Environment.NewLine, Environment.NewLine + "\t\t");

                    var autogenCodeString = Resources.DBEntityForm_AutogenCode.Replace("NAMESPACE", type.Namespace).Replace("CLASS_NAME", className).Replace("[CONTROL_CONTAINER_SIZE]", 400 + "," + row).Replace("[WND_SIZE]", 400 + "," + (((tabIndex + 1) * 30) + 30));
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_DEC]", declarationCode);
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_INIT]", initCode);
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_SETTINGS]", settingsCode);
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_ADD]", addingCode);
                    autogenCodeString = autogenCodeString.Replace("[BTN_LOC_Y]", (row + 10).ToString(CultureInfo.InvariantCulture));
                    autogenCodeString = autogenCodeString.Replace("[BTN_1_LOC_X]", (10).ToString(CultureInfo.InvariantCulture));
                    autogenCodeString = autogenCodeString.Replace("[BTN_2_LOC_X]", (98).ToString(CultureInfo.InvariantCulture));
                    autogenCodeString = autogenCodeString.Replace("[BTN_3_LOC_X]", (199).ToString(CultureInfo.InvariantCulture));

                    if (File.Exists(fbd.SelectedPath + "\\" + className + ".cs"))
                        File.Delete(fbd.SelectedPath + "\\" + className + ".cs");

                    if (File.Exists(fbd.SelectedPath + "\\" + className + ".Designer.cs"))
                        File.Delete(fbd.SelectedPath + "\\" + className + ".Designer.cs");

                    if (File.Exists(fbd.SelectedPath + "\\" + className + ".resx"))
                        File.Delete(fbd.SelectedPath + "\\" + className + ".resx");

                    File.WriteAllText(fbd.SelectedPath + "\\" + className + ".cs", userCodeString.Replace("[LoadingCodeCalls]", tagCode + Environment.NewLine + loadingCodeCalls).Replace("[LoadingCalls]", loadingCode));
                    File.WriteAllText(fbd.SelectedPath + "\\" + className + ".Designer.cs", autogenCodeString);
                    File.WriteAllText(fbd.SelectedPath + "\\" + className + ".resx", Resources.RESXContent);
                }
            }
            MessageBox.Show("Form Creation Successful !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
    }
}
