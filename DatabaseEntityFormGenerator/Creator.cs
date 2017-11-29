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
                    var instance = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);

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
                    var y = 15;

                    loadingCodeCalls += Environment.NewLine + "\t\t\tSetEntityType(typeof(" + type.Name + "));";

                    foreach (var column in instance.GetColumns(true).GetOtherColumns())
                    {
                        var attr = column.GetCustomAttributes(typeof(TableColumn), true)[0] as TableColumn;
                        if (attr == null)
                            continue;

                        if (attr.ReferencedClass != null && attr.ReferencedField != null)
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine) + String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.ComboBox cmb{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine) + String.Format("this.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("this.cmb{0} = new System.Windows.Forms.ComboBox();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("this.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 10, y);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);

                            settingsCode += String.Format("\t\tthis.cmb{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 120, y);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 160, 20);
                            settingsCode += String.Format("\t\tthis.cmb{0}.TabIndex = {1};" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left);" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\tthis.cmb{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;", column.Name);
                            settingsCode += String.Format("\t\tthis.cmb{0}.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;", column.Name);
                            
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(lbl{0});", column.Name);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(cmb{0});", column.Name);

                            loadingCodeCalls += Environment.NewLine + "\t\t\tLoad" + column.Name + "s();";

                            loadingCode += Environment.NewLine + Environment.NewLine + "\t\t" + String.Format(Resources.ComboLoadingCode, column.Name, attr.ReferencedClass.Name, instance.GetSupportedDatabaseType() == DatabaseType.SqlCE ? "DatabaseType.SqlCE" : "DatabaseType.MySQL");

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\tcmb{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            y += 30;
                        }
                        else if (GenericFieldTools.IsTypeString(column.FieldType) || GenericFieldTools.IsTypeString(column.FieldType) || GenericFieldTools.IsTypeFloatingPoint(column.FieldType))
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine) + String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.TextBox txt{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine) + String.Format("this.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("this.txt{0} = new System.Windows.Forms.TextBox();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("this.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 10, y);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);

                            settingsCode += String.Format("\t\tthis.txt{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 120, y);
                            settingsCode += String.Format("\t\tthis.txt{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 160, 20);
                            settingsCode += String.Format("\t\tthis.txt{0}.TabIndex = {1};" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\tthis.txt{0}.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left);" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\t\tthis.txt{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(lbl{0});", column.Name);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(txt{0});", column.Name);

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\ttxt{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            y += 30;
                        }
                        else if (column.FieldType == typeof(bool) || column.FieldType == typeof(Boolean))
                        {
                            declarationCode += String.Format("\tprivate System.Windows.Forms.CheckBox chk{0};", column.Name);
                            initCode += String.Format("this.chk{0} = new System.Windows.Forms.CheckBox();", column.Name);

                            settingsCode += String.Format("\t\tthis.chk{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 10, y);
                            settingsCode += String.Format("\t\tthis.chk{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 124, 20);
                            settingsCode += String.Format("\t\tthis.chk{0}.TabIndex = {1};" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\tthis.chk{0}.RightToLeft = {1};" + Environment.NewLine, column.Name, "System.Windows.Forms.RightToLeft.Yes");
                            settingsCode += String.Format("\t\tthis.chk{0}.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\t\tthis.chk{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);
                            settingsCode += String.Format("\t\t\tthis.chk{0}.Text = \"{1}\";", column.Name, attr.GridDisplayName);
                            settingsCode += String.Format("\t\t\tthis.chk{0}.TextAlign = System.Drawing.ContentAlignment.MiddleRight;", column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(chk{0});", column.Name);

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\tchk{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            y += 30;
                        }
                        else if (column.FieldType == typeof(DateTime) || column.FieldType == typeof(Date))
                        {
                            declarationCode += (declarationCode == "" ? "" : Environment.NewLine) + String.Format("\tprivate System.Windows.Forms.Label lbl{0};" + Environment.NewLine, column.Name);
                            declarationCode += String.Format("\tprivate System.Windows.Forms.DateTimePicker dtp{0};", column.Name);

                            initCode += (initCode == "" ? "" : Environment.NewLine) + String.Format("this.lbl{0} = new System.Windows.Forms.Label();" + Environment.NewLine, column.Name);
                            initCode += String.Format("this.dtp{0} = new System.Windows.Forms.DateTimePicker();", column.Name);

                            settingsCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("this.lbl{0}.Text = \"{1}\";" + Environment.NewLine, column.Name, attr.GridDisplayName ?? column.Name);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 10, y);
                            settingsCode += String.Format("\t\tthis.lbl{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 100, 20);

                            settingsCode += String.Format("\t\tthis.dtp{0}.Location = new System.Drawing.Point({1}, {2});" + Environment.NewLine, column.Name, 120, y);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Size = new System.Drawing.Size({1}, {2});" + Environment.NewLine, column.Name, 160, 20);
                            settingsCode += String.Format("\t\tthis.dtp{0}.TabIndex = {1};" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\tthis.dtp{0}.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left);" + Environment.NewLine, column.Name, y / 30);
                            settingsCode += String.Format("\t\t\tthis.dtp{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(lbl{0});", column.Name);
                            addingCode += (addingCode == "" ? "" : Environment.NewLine) + String.Format("\t\tthis.pnlControlContainer.Controls.Add(dtp{0});", column.Name);

                            tagCode += (settingsCode == "" ? "" : Environment.NewLine) + String.Format("\t\t\tdtp{0}.Tag = new TableColumnField({1}, typeof({2}), \"{3}\");", column.Name, "true", type.Name, column.Name);

                            y += 30;
                        }
                    }

                    var autogenCodeString = Resources.DBEntityForm_AutogenCode.Replace("NAMESPACE", type.Namespace).Replace("CLASS_NAME", className).Replace("[CONTROL_CONTAINER_SIZE]", 300 + "," + y).Replace("[WND_SIZE]", 300 + "," + (y + 47));
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_DEC]", declarationCode);
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_INIT]", initCode);
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_SETTINGS]", settingsCode);
                    autogenCodeString = autogenCodeString.Replace("[OTHER_CONTROL_ADD]", addingCode);
                    autogenCodeString = autogenCodeString.Replace("[BTN_LOC_Y]", (y + 10).ToString(CultureInfo.InvariantCulture));
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
