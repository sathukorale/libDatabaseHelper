using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.classes.sqlce;
using libDatabaseHelper.classes.generic.entities;

namespace libDatabaseHelper.classes.sqlce
{
    public class DatabaseEntity
    {
        public delegate void UpdateEvent(DatabaseEntity updatedEntity, Type type, UpdateEventType event_type);
        public static event UpdateEvent OnDatabaseEntityUpdated;

        private static Dictionary<Type, List<Reference>> referenceMap = new Dictionary<Type, List<Reference>>();
        private static Dictionary<Type, List<FieldInfo>> fieldInfos = new Dictionary<Type, List<FieldInfo>>();
        private static Dictionary<Type, string> selectCommandStrings = new Dictionary<Type, string>();

        [Flags]
        public enum ExistCondition
        {
            RecordExists = 4,
            UniqueKeyExists = 2,
            None = 1
        }

        public enum UpdateEventType
        {
            Add,
            Update,
            Remove
        }

        private bool _fillFromUniques = true;

        public DatabaseEntity()
        {
            var columns_result = GetColumns(true);
            if (columns_result == null)
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoColumnsFound);
            }

            var columns = columns_result.GetOtherColumns();
            foreach (var column in columns)
            {
                try
                {
                    if (FieldTools.IsTypeNumber(column.FieldType))
                    {
                        column.SetValue(this, -1);
                    }
                    else if (FieldTools.IsTypeFloatingPoint(column.FieldType))
                    {
                        column.SetValue(this, -1);
                    }
                    else if (FieldTools.IsDateType(column.FieldType))
                    {
                        column.SetValue(this, DateTime.Now);
                    }
                    else
                    {
                        column.SetValue(this, null);
                    }
                }
                catch 
                {
                }
            }
        }

        public static string GetSelectCommandString<T>()
        {
            return GetSelectCommandString(typeof(T));
        }

        public static string GetSelectCommandString(Type type)
        {
            if (selectCommandStrings.ContainsKey(type))
                return selectCommandStrings[type];

            var cInstanceName = type.Name + "_1";

            var obj = Activator.CreateInstance(type) as DatabaseEntity;

            if (obj == null) return "";

            var fields = obj.GetColumns(true).GetOtherColumns().Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsRetrievableFromDatabase);

            var selectList = "";
            var dict = new Dictionary<Type, int>();

            foreach (var fieldInfo in fields)
            {
                var attr = (TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0];
                selectList += (selectList == "" ? "" : ", ");
                if (attr.ReferencedField != null && attr.ReferencedClass != null)
                {
                    var i = 0;
                    if (!dict.ContainsKey(attr.ReferencedClass))
                    {
                        dict.Add(attr.ReferencedClass, ++i);
                    }
                    else
                    {
                        i = dict[attr.ReferencedClass];
                        dict[attr.ReferencedClass] = ++i;
                    }

                    var cobj = (Activator.CreateInstance(attr.ReferencedClass) as IComboBoxItem);
                    if (cobj == null)
                    {
                        throw new DatabaseEntityException("Invalid referee class specified. The type specified : " + attr.ReferencedClass.Name + ", should be of type IComboBoxItem");
                    }

                    selectList += "(CASE WHEN " + cInstanceName + "." + fieldInfo.Name + " IN ( SELECT " + attr.ReferencedField + " FROM " +
                                  attr.ReferencedClass.Name + " ) THEN (SELECT " +
                                  cobj.GetSelectQueryItems().Replace("[OBJ]", attr.ReferencedClass.Name) + " FROM " +
                                  attr.ReferencedClass.Name + " WHERE " + cInstanceName + "." + fieldInfo.Name + " = " + attr.ReferencedClass.Name + "." +
                                  attr.ReferencedField + " LIMIT 1) ELSE \"\" END) as [" + attr.GridDisplayName + "]";
                }
                else
                {
                    selectList += cInstanceName + "." + fieldInfo.Name + " as [" + ((attr.GridDisplayName == null || attr.GridDisplayName.Trim() == "") ? fieldInfo.Name : attr.GridDisplayName) + "]";
                }
            }

            var selectCommand = "SELECT " + selectList + " FROM " + type.Name + " " + cInstanceName /*+ innerJoinQuery*/;

            selectCommandStrings.Add(type, selectCommand);

            return selectCommand;
        }

        public bool Add()
        {
            ExistCondition condition;
            if (((condition = Exist(false)) & ExistCondition.None) != ExistCondition.None)
            {
                throw new DatabaseException((DatabaseException.ErrorType) condition, condition);
            }
            var result = GetColumns(true);
            if (result == null || !result.GetOtherColumns().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoColumnsFound);
            }

            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();
            var paramString = "";
            var valueString = "";
            FieldInfo autogenField = null;
            foreach (var field in result.GetOtherColumns().Where(i => ! ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsAutogenerated))
            {
                valueString += (valueString == "" ? "" : ", ") + field.Name;
                paramString += (paramString == "" ? "@" : ", @") + field.Name;
                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;
                command.Parameters.AddWithValue("@" + field.Name, val);
            }

            foreach (var field in from field in result.GetOtherColumns().Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsAutogenerated) let col = ((TableColumn)field.GetCustomAttributes(typeof(TableColumn), true)[0]) where col != null && col.IsAutogenerated && FieldTools.IsTypeNumber(field.FieldType) select field)
            {
                autogenField = field;
            }

            var commandString = "INSERT INTO " + GetType().Name + "( " + valueString + " ) VALUES ( " + paramString + " )";
            command.CommandText = commandString;
            if (command.ExecuteNonQuery() <= 0)
                return false;

            command.CommandText = "SELECT @@IDENTITY";
            var valReturned = command.ExecuteScalar();

            if (autogenField != null)
                autogenField.SetValue(this, FieldTools.ConvertToType(autogenField.FieldType, valReturned));

            if (this is AuditEntry) 
                return true;

            AuditEntry.AddAuditEntry(this, "Added ");

            if (OnDatabaseEntityUpdated != null)
                OnDatabaseEntityUpdated(this, GetType(), UpdateEventType.Add);

            return true;
        }

        public bool Update()
        {
            ExistCondition condition;
            if ((condition = Exist()) != ExistCondition.RecordExists)
            {
                throw new DatabaseException((DatabaseException.ErrorType) condition, condition);
            }
            var result = GetColumns(true);
            if (result == null || !result.GetOtherColumns().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoColumnsFound);
            }
            if (!result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();
            var valueParamString = "";
            var primaryValueParamString = "";
            foreach (var field in result.GetOtherColumns())
            {
                if (result.GetPrimaryKeys().Contains(field))
                    continue;

                valueParamString += (valueParamString == "" ? "" : ", ") + (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                command.Parameters.AddWithValue("@" + field.Name, val);
            }

            foreach (var field in result.GetPrimaryKeys())
            {
                primaryValueParamString += (primaryValueParamString == "" ? "" : " AND ") +
                                           (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                if (command.Parameters.Contains("@" + field.Name))
                    command.Parameters["@" + field.Name].Value = val;
                else
                    command.Parameters.AddWithValue("@" + field.Name, val);
            }

            var commandString = "UPDATE " + GetType().Name + " SET " + valueParamString + " WHERE " +
                                   primaryValueParamString;
            command.CommandText = commandString;
            if (command.ExecuteNonQuery() <= 0)
                return false;
            if (this is AuditEntry) 
                return true;

            AuditEntry.AddAuditEntry(this, "Updated ");
            if (OnDatabaseEntityUpdated != null)
                OnDatabaseEntityUpdated(this, GetType(), UpdateEventType.Update);

            return true;
        }

        public bool Remove()
        {
            ExistCondition condition;
            if (((condition = Exist()) & ExistCondition.RecordExists) == 0)
            {
                throw new DatabaseException(DatabaseException.ErrorType.NonExistingRecord, condition);
            }
            if (Relationship.CheckReferences(this))
            {
                throw new DatabaseException(DatabaseException.ErrorType.ReferenceKeyViolation, condition);
            }
            var result = GetColumns();
            if (result == null || !result.GetOtherColumns().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoColumnsFound);
            }
            if (!result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            if (IsReferenced())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();
            var primaryValueParamString = "";

            foreach (var field in result.GetPrimaryKeys())
            {
                primaryValueParamString += (primaryValueParamString == "" ? "" : " AND ") +
                                           (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                command.Parameters.AddWithValue("@" + field.Name, val);
            }

            var commandString = "DELETE FROM " + GetType().Name + " WHERE " + primaryValueParamString;
            command.CommandText = commandString;
            if (command.ExecuteNonQuery() <= 0)
                return false;

            if (this is AuditEntry) 
                return true;

            AuditEntry.AddAuditEntry(this, "Removed ");
            if (OnDatabaseEntityUpdated != null)
                OnDatabaseEntityUpdated(this, GetType(), UpdateEventType.Remove);

            return true;
        }

        public bool IsReferenced()
        {
            if (! referenceMap.ContainsKey(GetType()))
            {
                var referenceList = new List<Reference>();
                foreach (var type in DatabaseManager.GetRegisteredTypes().Where(i => i != GetType()))
                {
                    referenceList.AddRange(from field in type.GetFields() let attrs = field.GetCustomAttributes(typeof (TableColumn), true).OfType<TableColumn>().Where(i => i.ReferencedClass == GetType()).ToArray() where attrs.Any() let attr = attrs.First() let referencedField = GetFieldInfo(attr.ReferencedField) where referencedField != null select new Reference(type, field, referencedField));
                }
                referenceMap.Add(GetType(), referenceList);
            }

            return referenceMap[GetType()].Where(referenceDetails => referenceDetails != null).Any(referenceDetails => DatabaseManager.Select(referenceDetails.ReferencedByClass, new[] {new Selector(referenceDetails.ReferencedByField.Name, referenceDetails.ReferredField.GetValue(this))}).Any());
        }

        public ExistCondition Exist()
        {
            return Exist(true);
        }

        public ExistCondition Exist(bool justCheck)
        {
            var result = GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();
            var primaryValueParamString = "";
            var inversePrimaryValueString = "";
            var filtered = (justCheck ? result.GetPrimaryKeys() : result.GetPrimaryKeys().Where(i => ! ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsAutogenerated)).ToArray();

            foreach (var field in filtered)
            {
                primaryValueParamString += (primaryValueParamString == "" ? "" : " AND ") +
                                           (field.Name + " = @" + field.Name);
                inversePrimaryValueString += (inversePrimaryValueString == "" ? "" : " AND ") +
                                             (field.Name + " != @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                command.Parameters.AddWithValue("@" + field.Name, val);
            }

            var commandString = "SELECT * FROM " + GetType().Name + " WHERE " + primaryValueParamString;
            command.CommandText = commandString;

            var cell = filtered.Any() ? command.ExecuteScalar() : null;

            var uniqueValueParamString = "";

            foreach (
                var field in
                    result.GetOtherColumns()
                        .Where(i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsUnique))
            {
                uniqueValueParamString += (uniqueValueParamString == "" ? "" : " OR ") +
                                          (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                command.Parameters.AddWithValue("@" + field.Name, val);
            }

            if (uniqueValueParamString == "")
                return cell == null ? ExistCondition.None : ExistCondition.RecordExists;
            commandString = "SELECT * FROM " + GetType().Name + " WHERE " +
                            (cell != null ? ("( " + inversePrimaryValueString + ") AND (") : "(") + uniqueValueParamString + ")";
            command.CommandText = commandString;
            var reader = command.ExecuteReader();
            var hadData = false;
            if (reader != null)
            {
                if (_fillFromUniques && reader.Read())
                {
                    foreach (var field in result.GetPrimaryKeys())
                    {
                        field.SetValue(this, FieldTools.ConvertToType(field.FieldType, reader[field.Name]));
                    }
                    hadData = true;
                }
                reader.Close();
            }

            if (cell == null && !hadData)
                return ExistCondition.None;

            return (cell != null ? ExistCondition.RecordExists : 0) |
                   (hadData ? ExistCondition.UniqueKeyExists : 0);
        }

        public DatabaseEntity Parse(SqlCeDataReader reader)
        {
            var columnsResult = GetColumns(true);
            foreach (
                var column in
                    columnsResult.GetOtherColumns()
                        .Where(i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsRetrievableFromDatabase))
            {
                var val = reader[column.Name];
                column.SetValue(this, FieldTools.ConvertToType(column.FieldType, val));
            }
            return this;
        }

        public ColumnResult GetColumns()
        {
            return GetColumns(false);
        }

        public ColumnResult GetColumns(bool includeAllColumns)
        {
            var contains = fieldInfos.ContainsKey(GetType());
            var includedFields = contains ? fieldInfos[GetType()] : GetType().GetFields()
                    .Where(i => (i.GetCustomAttributes(typeof (TableColumn), true).Count() != 0)).ToList();

            if (! contains)
            {
                var list = new List<FieldInfo>();
                list.AddRange(includedFields);
                fieldInfos.Add(GetType(), includedFields);
            }

            if (!includedFields.Any())
            {
                return null;
            }

            includedFields =
                includedFields.Where(i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).ShouldIncludeInTable).ToList();
            if (!includedFields.Any())
            {
                return null;
            }

            var ids =
                includedFields.Where(
                    i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsPrimaryKey).ToList();

            if (!includeAllColumns)
                includedFields =
                    includedFields.Where(
                        i =>
                            ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).ShouldIncludeInTable &&
                            !((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsPrimaryKey).ToList();

            if (!includeAllColumns)
                includedFields =
                    includedFields.Where(
                        i => ! ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsAutogenerated).ToList();

            return new ColumnResult(ids, includedFields);
        }

        public static object Autogen(string table, string field)
        {
            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();

            var commandText = "SELECT " + field + " FROM " + table + " ORDER BY " + field + " DESC LIMIT 1";
            command.CommandText = commandText;
            return command.ExecuteScalar();
        }

        public FieldInfo GetFieldInfo(string fieldName)
        {
            var columns = GetColumns(true);
            return columns.GetOtherColumns().FirstOrDefault(column => column.Name == fieldName);
        }

        public TableColumn GetFieldAttributes(string fieldName)
        {
            var columns = GetColumns(true);
            var firstOrDefault = columns.GetOtherColumns().FirstOrDefault(column => column.Name == fieldName);
            return firstOrDefault != null
                ? firstOrDefault.GetCustomAttributes(typeof (TableColumn), true).Select(i => i as TableColumn).First()
                : null;
        }

        public object GetFieldValue(string fieldName)
        {
            var columns = GetColumns(true);
            foreach (var column in columns.GetOtherColumns().Where(column => column.Name == fieldName))
            {
                return column.GetValue(this);
            }
            return "";
        }

        public void SetFieldValue(string fieldName, object value)
        {
            var columns = GetColumns(true);
            foreach (var column in columns.GetOtherColumns().Where(column => column.Name == fieldName))
            {
                column.SetValue(this, FieldTools.ConvertToType(column.FieldType, value));
            }
        }

        public void LoadToForm(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (control.HasChildren)
                {
                    LoadToForm(control);
                    continue;
                }

                var tag = control.Tag as TableColumnField;
                if (tag == null || tag.FieldName == null || tag.ObjType != GetType()) continue;

                var info = GetFieldAttributes(tag.FieldName);

                if (info != null)
                {
                    control.Enabled = info.IsEditableOnFormWhenLoaded;
                }

                if (control is TextBox)
                {
                    try
                    {
                        var value = GetFieldValue(tag.FieldName);
                        value = value is ICustomType ? (value as ICustomType).GetValue() : value;

                        if (value == null || value is DBNull) value = "";

                        control.Text = Convert.ToString(value);
                    }
                    catch
                    {
                        control.Text = "";
                    }
                }
                else if (control is ComboBox)
                {
                    var cmb = control as ComboBox;
                    var value = GetFieldValue(tag.FieldName);
                    if (value == null || value is DBNull) continue;
                    value = value is ICustomType ? (value as ICustomType).GetValue() : value;
                    var attr = GetFieldAttributes(tag.FieldName);
                    if (attr != null && attr.ReferencedClass != null)
                    {
                        cmb.SelectedIndex = -1;
                        cmb.ResetText();
                        cmb.SelectedItem = null;
                        foreach (
                            var item in
                                cmb.Items.OfType<IComboBoxItem>().Where(item => item.GetID().Equals(value)))
                        {
                            cmb.SelectedItem = item;
                            break;
                        }
                    }
                    else if (attr.ReferencedClass == null && FieldTools.IsTypeNumber(value.GetType()))
                    {
                        (control as ComboBox).SelectedIndex = (int) value;
                    }
                }
                else if (control is DateTimePicker)
                {
                    try
                    {
                        var value = GetFieldValue(tag.FieldName);
                        value = value is ICustomType ? (value as ICustomType).GetValue() : value;
                        if (value != null)
                            (control as DateTimePicker).Value = (DateTime) value;
                    }
                    catch
                    {
                    }
                }
                else if (control is CheckBox)
                {
                    var fieldInfo = GetFieldInfo(tag.FieldName);
                    if (fieldInfo.FieldType == typeof (bool) || fieldInfo.FieldType == typeof (Boolean))
                    {
                        var value = fieldInfo.GetValue(this);
                        if (value == null || value is DBNull) continue;
                        value = value is ICustomType ? (value as ICustomType).GetValue() : value;
                        if (value != null)
                            (control as CheckBox).Checked = (bool) value;
                    }
                }
                else if (control is RadioButton)
                {
                    var rbtnTag = tag as RadioButtonField;
                    if (rbtnTag == null)
                        continue;

                    var value = GetFieldValue(rbtnTag.FieldName);
                    if (value == null || value is DBNull || (!(value is bool))) continue;
                    value = value is ICustomType ? (value as ICustomType).GetValue() : value;

                    if (value != null)
                        (control as RadioButton).Checked = (rbtnTag.Value == value);
                }
            }
        }

        public bool ValidateForm(Control parentControl)
        {
            var listOfRbtnGroupsChecked = new List<string>();
            var controls = parentControl.Controls.OfType<Control>().OrderBy(i => i.TabIndex).ToArray();
            foreach (var control in controls)
            {
                if (control.Tag is TableColumnField)
                {
                    var tag = control.Tag as TableColumnField;
                    if (tag.ObjType != GetType()) continue;

                    var info = GetFieldInfo(tag.FieldName);
                    if (control is TextBox)
                    {
                        #region "When the control is a TextBox"

                        if (info != null)
                        {
                            bool is_txt_empty = (control.Text == null || control.Text.Trim()== "");
                            if (is_txt_empty && tag.DefaultValue != null)
                            {
                                control.Text = tag.DefaultValue.ToString();
                            }

                            if (is_txt_empty && tag.DefaultValue == null)
                            {
                                if (tag.Required)
                                {
                                    throw new ValidationException(control, "Some of the required fields are missing !");
                                }
                            }
                            else if ((info.FieldType == typeof (string) || info.FieldType == typeof (String)))
                            {
                                var attr = info.GetCustomAttributes(typeof (TableColumn), true)[0] as TableColumn;
                                if (attr != null)
                                {
                                    if (attr.IsUnique || attr.IsPrimaryKey || attr.IsAutogenerated)
                                    {
                                        if (control.Text.Length > attr.Length)
                                        {
                                            throw new ValidationException(control,
                                                "The length of the input you entered for the selected textbox should be less than " +
                                                attr.Length + " !");
                                        }
                                    }
                                    info.SetValue(this, control.Text);
                                }
                            }
                            else if (info.FieldType == typeof (char) || info.FieldType == typeof (Char))
                            {
                                try
                                {
                                    info.SetValue(this, Char.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be just one character, That is between the values of 0 and 128 of ASCII characters !");
                                }
                            }
                            else if (info.FieldType == typeof (byte) || info.FieldType == typeof (Byte))
                            {
                                try
                                {
                                    info.SetValue(this, Byte.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from 0 and expands up to 255 ! ");
                                }
                            }
                            else if (info.FieldType == typeof (short))
                            {
                                try
                                {
                                    info.SetValue(this, short.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (short.MinValue) +
                                        " and expands up to " + short.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (UInt16))
                            {
                                try
                                {
                                    info.SetValue(this, UInt16.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (UInt16.MinValue) +
                                        " and expands up to " + UInt16.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (Int32) || info.FieldType == typeof (int))
                            {
                                try
                                {
                                    info.SetValue(this, Int32.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (Int32.MinValue) +
                                        " and expands up to " + Int32.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (UInt32) || info.FieldType == typeof (uint))
                            {
                                try
                                {
                                    info.SetValue(this, UInt32.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (UInt32.MinValue) +
                                        " and expands up to " + UInt32.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (Int64) || info.FieldType == typeof (long))
                            {
                                try
                                {
                                    info.SetValue(this, Int64.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (Int64.MinValue) +
                                        " and expands up to " + Int64.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (UInt64))
                            {
                                try
                                {
                                    info.SetValue(this, UInt64.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (UInt64.MinValue) +
                                        " and expands up to " + UInt64.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (float))
                            {
                                try
                                {
                                    info.SetValue(this, float.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (float.MinValue) +
                                        " and expands up to " + float.MaxValue + " ! ");
                                }
                            }
                            else if (info.FieldType == typeof (double) || info.FieldType == typeof (Double))
                            {
                                try
                                {
                                    info.SetValue(this, Double.Parse(control.Text));
                                }
                                catch
                                {
                                    throw new ValidationException(control,
                                        "The value should be an integer value starting from " + (Double.MinValue) +
                                        " and expands up to " + Double.MaxValue + " ! ");
                                }
                            }
                        }
                        else return false;

                        #endregion
                    }
                    else if (control is ComboBox)
                    {
                        var combobox = control as ComboBox;
                        if (combobox.Items.Count > 0)
                        {
                            if (combobox.SelectedItem == null)
                            {
                                if (! tag.Required)
                                    continue;
                                throw new ValidationException(control,
                                    "Please select an item from the combobox selected !");
                            }

                            if (!combobox.Items.Contains(combobox.SelectedItem))
                                throw new ValidationException(control,
                                    "Please select an item from the combobox selected !");

                            var selectedItem = (control as ComboBox).SelectedItem as IComboBoxItem;
                            var attr = info.GetCustomAttributes(typeof (TableColumn), true).First() as TableColumn;
                            if (selectedItem != null)
                                info.SetValue(this, (selectedItem.GetID()));
                            else if (combobox.SelectedIndex != -1 && (control as ComboBox).SelectedItem != null &&
                                     !(attr is IComboBoxItem))
                            {
                                if (attr == null || !FieldTools.IsTypeNumber(info.FieldType)) 
                                    continue;

                                if (attr.ReferencedClass != null)
                                    info.SetValue(this, combobox.SelectedIndex);
                            }
                        }
                        else if (tag.Required)
                            throw new ValidationException(control,
                                "Please add items to the selected combo box using the add button next to it !");
                    }
                    else if (control is CheckBox)
                    {
                        if (info.FieldType != typeof (bool) && info.FieldType != typeof (Boolean))
                            throw new Exception("Invalid control type is used for this field !");
                        info.SetValue(this, (control as CheckBox).Checked);
                    }
                    else if (control is RadioButton)
                    {
                        var rbtnTag = tag as RadioButtonField;
                        if (rbtnTag == null || listOfRbtnGroupsChecked.Contains(rbtnTag.Group))
                            continue;

                        var radioButtons =
                            parentControl.Controls.OfType<RadioButton>()
                                .Where(
                                    i => i.Tag is RadioButtonField && (i.Tag as RadioButtonField).Group == rbtnTag.Group)
                                .Where(i => i.Checked);

                        if (! radioButtons.Any())
                        {
                            if (! tag.Required)
                                continue;
                            throw new ValidationException(control,
                                "Please select an option from the group highlighted !");
                        }
                        info.SetValue(this, rbtnTag.Value);

                        listOfRbtnGroupsChecked.Add(rbtnTag.Group);
                    }
                    else if (control is DateTimePicker)
                    {
                        info.SetValue(this,
                            info.FieldType.GetInterfaces().Contains(typeof (ICustomType))
                                ? (Activator.CreateInstance(info.FieldType) as ICustomType).SetValue(
                                    (control as DateTimePicker).Value)
                                : (object) ((control as DateTimePicker).Value));
                    }
                }
                else if (control.HasChildren)
                {
                    if (! ValidateForm(control))
                        return false;
                }
            }
            return true;
        }

        public string ToContentString()
        {
            var columns = GetColumns(true);
            var message = GetType().Name + " [ ";
            var dataString = "";

            foreach (var column in columns.GetOtherColumns())
            {
                var attr = (TableColumn) column.GetCustomAttributes(typeof (TableColumn), true)[0];
                if (! attr.IsAuditVisible) continue;

                if (attr.ReferencedClass == null || attr.ReferencedField == null)
                {
                    dataString += (dataString == "" ? "" : ", ") +
                                  (column.Name + " = \"" + Convert.ToString(column.GetValue(this)) + "\"");
                }
                else
                {
                    var entities = DatabaseManager.Select(attr.ReferencedClass,
                        new[] {new Selector(attr.ReferencedField, column.GetValue(this))});
                    if (entities.Any())
                    {
                        dataString += (dataString == "" ? "" : ", ") +
                                      (column.Name + " = ( " + entities[0].ToContentString() + " )");
                    }
                    else
                        dataString += (dataString == "" ? "" : ", ") +
                                      (column.Name + " = \"" + Convert.ToString(column.GetValue(this)) + "\"");
                }
            }

            message += dataString + " ]";
            return message;
        }

        private void CreateDataGridViewHeader(DataGridView grid)
        {
            var allFields = GetColumns(true);
            if (allFields == null) return;

            grid.Columns.Clear();
            var fields =
                allFields.GetOtherColumns()
                    .Where(i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsRetrievableFromDatabase);
            foreach (var fieldInfo in fields)
            {
                var cInfo = ((TableColumn) fieldInfo.GetCustomAttributes(typeof (TableColumn), true)[0]);
                var cindex = grid.Columns.Add(fieldInfo.Name, cInfo.GridDisplayName ?? fieldInfo.Name);
                grid.Columns[cindex].Visible = cInfo.IsGridViewable;
                grid.Columns[cindex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            Application.DoEvents();
        }

        public void AddToDataGridViewRow(DataGridView grid)
        {
            var allFields = GetColumns(true);
            if (allFields == null) return;

            if (grid.ColumnCount <= 0)
                CreateDataGridViewHeader(grid);
            var fields =
                allFields.GetOtherColumns()
                    .Where(i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsRetrievableFromDatabase);
            var index = grid.Rows.Add();
            foreach (var fieldInfo in fields)
            {
                try
                {
                    var tableColumn = (TableColumn) fieldInfo.GetCustomAttributes(typeof (TableColumn), true)[0];
                    if (tableColumn.ReferencedClass != null && tableColumn.ReferencedField != null)
                    {
                        var referenced = DatabaseManager.Select(tableColumn.ReferencedClass,
                            new[] { new Selector(tableColumn.ReferencedField, fieldInfo.GetValue(this)) });
                        if (referenced != null && referenced.Any())
                        {
                            grid.Rows[index].Cells[fieldInfo.Name].Value = referenced[0].ToString();
                        }
                    }
                    else if (tableColumn.Translators != null && FieldTools.IsTypeNumber(fieldInfo.FieldType))
                    {
                        var val = Convert.ToInt32(fieldInfo.GetValue(this) ?? "0");
                        grid.Rows[index].Cells[fieldInfo.Name].Value = tableColumn.Translators[val];
                    }
                    else
                        grid.Rows[index].Cells[fieldInfo.Name].Value = Convert.ToString(fieldInfo.GetValue(this) ?? "");
                }
                catch
                {
                    grid.Rows.RemoveAt(index);
                    index = -1;
                    break;
                }
            }

            if (index >= 0)
            {
                grid.Rows[index].Tag = this;
            }
        }

        public bool Equals(DatabaseEntity obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            var columns1 = GetColumns().GetPrimaryKeys().ToArray();
            var columns2 = obj.GetColumns().GetPrimaryKeys().ToArray();

            if (columns1.Count() != columns2.Count())
                return false;

            for (var i = 0; i < columns1.Count(); i++)
            {
                var value1 = columns1[i].GetValue(this);
                var value2 = columns2[i].GetValue(obj);

                if (value1 == null || value2 == null)
                    return false;

                if (! value1.Equals(value2))
                    return false;
            }
            return true;
        }

        public class ColumnResult
        {
            private readonly List<FieldInfo> _otherColumns;
            private readonly List<FieldInfo> _primaryKeys;

            public ColumnResult(List<FieldInfo> primaryKeys, List<FieldInfo> otherColumns)
            {
                _primaryKeys = primaryKeys;
                _otherColumns = otherColumns;
            }

            public IEnumerable<FieldInfo> GetPrimaryKeys()
            {
                return _primaryKeys;
            }

            public IEnumerable<FieldInfo> GetOtherColumns()
            {
                return _otherColumns;
            }
        }
    }
}