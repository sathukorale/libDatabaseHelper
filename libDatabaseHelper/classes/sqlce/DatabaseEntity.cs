﻿using System;
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
using libDatabaseHelper.classes.sqlce.entities;
using System.Data.Common;

namespace libDatabaseHelper.classes.sqlce
{
    public class DatabaseEntity : GenericDatabaseEntity
    {
        private bool _fillFromUniques = true;

        public DatabaseEntity()
        {
        }

        protected override DatabaseType FetchDatbaseType()
        {
            return DatabaseType.SqlCE;
        }

        protected override bool OnAdd(DbCommand command)
        {
            var paramString = "";
            var valueString = "";
            FieldInfo autogenField = null;
            var result = GetColumns(true);

            foreach (var field in result.GetOtherColumns().Where(i => ! ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsAutogenerated))
            {
                valueString += (valueString == "" ? "" : ", ") + field.Name;
                paramString += (paramString == "" ? "@" : ", @") + field.Name;
                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;
                GenericUtils.AddWithValue(ref command, "@" + field.Name, val);
            }

            foreach (var field in from field in result.GetOtherColumns().Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsAutogenerated) let col = ((TableColumn)field.GetCustomAttributes(typeof(TableColumn), true)[0]) where col != null && col.IsAutogenerated && GenericFieldTools.IsTypeNumber(field.FieldType) select field)
            {
                autogenField = field;
            }

            var commandString = "INSERT INTO " + GetType().Name + "( " + valueString + " ) VALUES ( " + paramString + " )";
            command.CommandText = commandString;
            if (command.ExecuteNonQuery() <= 0)
                return false;

            command.CommandText = "SELECT @@IDENTITY";
            var valReturned = command.ExecuteScalar();
            command.Dispose();

            if (autogenField != null)
                autogenField.SetValue(this, GenericFieldTools.ConvertToType(autogenField.FieldType, valReturned));

            return true;
        }

        protected override bool OnUpdate(DbCommand command)
        {
            var valueParamString = "";
            var primaryValueParamString = "";
            var result = GetColumns(true);

            foreach (var field in result.GetOtherColumns())
            {
                if (result.GetPrimaryKeys().Contains(field))
                    continue;

                valueParamString += (valueParamString == "" ? "" : ", ") + (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                GenericUtils.AddWithValue(ref command, "@" + field.Name, val);
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
                    GenericUtils.AddWithValue(ref command, "@" + field.Name, val);
            }

            var commandString = "UPDATE " + GetType().Name + " SET " + valueParamString + " WHERE " +
                                   primaryValueParamString;
            command.CommandText = commandString;
            var executionResult = command.ExecuteNonQuery();
            command.Dispose();

            return (executionResult > 0);
        }

        protected override bool OnRemove(DbCommand command)
        {
            var primaryValueParamString = "";
            var result = GetColumns();

            foreach (var field in result.GetPrimaryKeys())
            {
                primaryValueParamString += (primaryValueParamString == "" ? "" : " AND ") +
                                           (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                GenericUtils.AddWithValue(ref command, "@" + field.Name, val);
            }

            var commandString = "DELETE FROM " + GetType().Name + " WHERE " + primaryValueParamString;
            command.CommandText = commandString;
            var executionResult = command.ExecuteNonQuery();
            command.Dispose();

            return (executionResult > 0);
        }

        public override GenericDatabaseEntity.ExistCondition Exist(bool onlyCheck)
        {
            var result = GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = GenericConnectionManager.GetConnectionManager(_supportedDatabase).GetConnection(_entityType);
            var command = connection.CreateCommand();
            var primaryValueParamString = "";
            var inversePrimaryValueString = "";
            var filtered = (onlyCheck ? result.GetPrimaryKeys() : result.GetPrimaryKeys().Where(i => !((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsAutogenerated)).ToArray();

            foreach (var field in filtered)
            {
                primaryValueParamString += (primaryValueParamString == "" ? "" : " AND ") +
                                           (field.Name + " = @" + field.Name);
                inversePrimaryValueString += (inversePrimaryValueString == "" ? "" : " AND ") +
                                             (field.Name + " != @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                GenericUtils.AddWithValue(ref command, "@" + field.Name, val);
            }

            var commandString = "SELECT * FROM " + GetType().Name + " WHERE " + primaryValueParamString;
            command.CommandText = commandString;

            var cell = filtered.Any() ? command.ExecuteScalar() : null;

            var uniqueValueParamString = "";

            foreach (var field in result.GetOtherColumns().Where(i => ((TableColumn) i.GetCustomAttributes(typeof (TableColumn), true)[0]).IsUnique))
            {
                uniqueValueParamString += (uniqueValueParamString == "" ? "" : " OR ") + (field.Name + " = @" + field.Name);

                var val = field.GetValue(this);
                val = val is ICustomType ? (val as ICustomType).GetValue() : val;

                GenericUtils.AddWithValue(ref command, "@" + field.Name, val);
            }

            if (uniqueValueParamString == "")
            {
                command.Dispose();
                return cell == null ? ExistCondition.None : ExistCondition.RecordExists;
            }

            commandString = "SELECT * FROM " + GetType().Name + " WHERE " + (cell != null ? ("( " + inversePrimaryValueString + ") AND (") : "(") + uniqueValueParamString + ")";
            command.CommandText = commandString;
            var reader = command.ExecuteReader();
            var hadData = false;
            if (reader != null)
            {
                if (_fillFromUniques && reader.Read())
                {
                    foreach (var field in result.GetPrimaryKeys())
                    {
                        field.SetValue(this, GenericFieldTools.ConvertToType(field.FieldType, reader[field.Name]));
                    }
                    hadData = true;
                }
                reader.Close();
            }

            command.Dispose();

            if (cell == null && !hadData)
                return ExistCondition.None;

            return (cell != null ? ExistCondition.RecordExists : 0) | (hadData ? ExistCondition.UniqueKeyExists : 0);
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

            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);

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
    }
}