﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Data.SqlServerCe;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.forms;
using libDatabaseHelper.classes.sqlce.entities;

namespace libDatabaseHelper.classes.sqlce
{
    public class DatabaseManager : GenericDatabaseManager
    {
        public DatabaseManager() : base(DatabaseType.SqlCE)
        {
            CreateTable<GenericConnectionDetails>();
            CreateTable<AuditEntry>();
        }

        public override bool TableExist(Type type)
        {
            var entity = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (entity == null)
                return false;

            var fields = entity.GetColumns(true).GetOtherColumns().ToList();

            var connection = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type);
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + (entity.GetType().Name) + "'";
            var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                return false;
            }

            var listToRemove = new List<string>();
            do
            {
                var columnName = reader.GetString(0).ToLower();
                {
                    List<FieldInfo> found;
                    var existing = (found = fields.Where(i => i.Name.ToLower() == columnName).ToList()).Any();

                    if (existing)
                    {
                        fields.Remove(found[0]);
                    }
                    else
                        listToRemove.Add(columnName);
                }
            }
            while (reader.Read());

            foreach (var fieldInfo in entity.GetColumns(true).GetOtherColumns().ToList())
            {
                var columnAttributes = (TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0];

                if (columnAttributes.ReferencedField != null && columnAttributes.ReferencedClass != null)
                {
                    Relationship.Add(columnAttributes.ReferencedClass, type);
                }
            }

            reader.Close();

            if (listToRemove.Any())
            {
                foreach (var column in listToRemove)
                {
                    var commandToRemove = "ALTER TABLE " + type.Name + " DROP COLUMN " + column;

                    command.CommandText = commandToRemove;
                    command.ExecuteNonQuery();
                }
            }

            if (fields.Any())
            {
                var columnToAdd = "";
                foreach (var column in fields)
                {
                    var columnAttributes = (TableColumn)column.GetCustomAttributes(typeof(TableColumn), true)[0];
                    columnToAdd += (columnToAdd == "" ? " " : ", ") + column.Name + " " +
                                    FieldTools.GetDbTypeString(column.FieldType,
                                        columnAttributes.IsAutogenerated || columnAttributes.IsUnique ||
                                        columnAttributes.IsPrimaryKey,
                                        columnAttributes.Length);
                    if (GenericFieldTools.IsTypeNumber(column.FieldType) && columnAttributes.IsAutogenerated &&
                        columnAttributes.AutogenrationMethod == null)
                    {
                        columnToAdd += " IDENTITY ";
                    }
                    else if (columnAttributes.IsUnique)
                    {
                        columnToAdd += " UNIQUE ";
                    }

                    command.CommandText = "ALTER TABLE " + type.Name + " ADD COLUMN " + columnToAdd;
                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE TABLE " + type.Name + " SET " + columnToAdd + "=" + (GenericFieldTools.IsTypeNumber(column.FieldType) ? "-1" : ("''"));
                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        public override bool CreateTable(Type type)
        {
            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (obj == null)
            {
                return false;
            }

            if (GetRegisteredDatabaseEntities().Contains(type) == false)
            {
                GetRegisteredDatabaseEntities().Add(type);
            }
            else
            {
                return false;
            }

            if (TableExist(type))
            {
                return false;
            }

            var result = obj.GetColumns(true);
            if (result == null || (result.GetOtherColumns().Count() == 0 && result.GetPrimaryKeys().Count() == 0))
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoColumnsFound);
            }
            else if (result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type);
            var command = connection.CreateCommand();

            var variabeDeclarations = "";
            foreach (var column in result.GetOtherColumns())
            {
                var columnAttributes = (TableColumn)column.GetCustomAttributes(typeof(TableColumn), true)[0];
                variabeDeclarations += (variabeDeclarations == "" ? " " : ", ") + column.Name + " " +
                                       FieldTools.GetDbTypeString(column.FieldType,
                                           columnAttributes.IsAutogenerated || columnAttributes.IsUnique ||
                                           columnAttributes.IsPrimaryKey,
                                           columnAttributes.Length);
                if (GenericFieldTools.IsTypeNumber(column.FieldType) && columnAttributes.IsAutogenerated &&
                    columnAttributes.AutogenrationMethod == null)
                {
                    variabeDeclarations += " IDENTITY ";
                }
                else if (columnAttributes.IsUnique)
                {
                    variabeDeclarations += " UNIQUE ";
                }
            }
            var primaryVariabeDeclarations = result.GetPrimaryKeys().Aggregate("", (current, column) => current + ((current == "" ? "" : ", ") + column.Name));
            if (primaryVariabeDeclarations != "")
            {
                primaryVariabeDeclarations = ", PRIMARY KEY (" + primaryVariabeDeclarations + ")";
            }

            var createStatement = "CREATE TABLE " + obj.GetType().Name + "(" + variabeDeclarations + primaryVariabeDeclarations + ")";

            command.CommandText = createStatement;
            return command.ExecuteNonQuery() >= 0;
        }

        public override bool DropTable(Type type)
        {
            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (obj == null)
            {
                return false;
            }

            if (GetRegisteredDatabaseEntities().Contains(type))
            {
                GetRegisteredDatabaseEntities().Remove(type);
            }

            if (! TableExist(type))
            {
                return false;
            }

            var result = obj.GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type);
            var command = connection.CreateCommand();

            var createStatement = "DROP TABLE " + type.Name;

            command.CommandText = createStatement;
            return command.ExecuteNonQuery() >= 0;
        }

        public override GenericDatabaseEntity[] Select(Type type, Selector[] selectors)
        {
            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (obj == null) return new DatabaseEntity[0];
            var result = obj.GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var selectFields =
                result.GetOtherColumns()
                    .Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsRetrievableFromDatabase)
                    .Aggregate("", (current, selector) => current + ((current == "" ? "" : ", ") + selector.Name));

            var connection = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type);
            var command = connection.CreateCommand();

            var whereStatement = "";
            if (selectors != null)
            {
                whereStatement = selectors.Aggregate("",
                    (current, selector) =>
                        current + ((current == "" ? "" : " AND ") + selector.SetToCommand(ref command)));
            }

            var selectStatement = "SELECT " + selectFields + " FROM " + obj.GetType().Name +
                                     (selectors != null && selectors.Any() ? (" WHERE " + whereStatement) : "");

            command.CommandText = selectStatement;

            var reader = command.ExecuteReader();

            return ParseDataReader(type, reader);
        }

        public override bool DeleteMatching(Type type, Selector[] selectors)
        {
            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (obj == null) return false;
            var result = obj.GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type);
            var command = connection.CreateCommand();

            var whereStatement = "";
            if (selectors != null)
                whereStatement = selectors.Aggregate("",
                    (current, selector) =>
                        current + ((current == "" ? "" : " AND ") + selector.SetToCommand(ref command)));

            var selectStatement = "DELETE FROM " + obj.GetType().Name +
                                     (selectors != null && selectors.Any() ? (" WHERE " + whereStatement) : "");
            command.CommandText = selectStatement;

            if (command.ExecuteNonQuery() >= 0)
            {
                _OnBulkDelete(type, selectors);
                return true;
            }
            return false;
        }

        public override void FillDataTable(Type type, ref DataTable table, Selector[] selectors, int limit)
        {
            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (obj == null)
                return;

            var commandString = DatabaseEntity.GetSelectCommandString(type);
            var whereQuery = "";
            var command = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type).CreateCommand();

            if (selectors != null && selectors.Any())
            {
                whereQuery = selectors.Aggregate(whereQuery, (current, selector) => current + ((current == "" ? "" : " AND ") + selector.SetToCommand(ref command)));
            }

            if (limit > 0)
            {
                commandString = commandString.Replace("SELECT ", "SELECT TOP (" + limit + ")");
            }

            command.CommandText = commandString + (whereQuery != "" ? (" WHERE " + whereQuery) : "");

            var reader = command.ExecuteReader();

            var fieldInfos = obj.GetColumns(true).GetOtherColumns().Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsRetrievableFromDatabase).ToArray();

            table.Rows.Clear();
            table.Columns.Clear();

            var translators = new List<string[]>();

            foreach (var fieldInfo in fieldInfos)
            {
                var cInfo = ((TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0]);
                if (fieldInfo.FieldType == typeof(bool))
                    table.Columns.Add(cInfo.GridDisplayName ?? fieldInfo.Name, typeof(bool));
                else
                    table.Columns.Add(cInfo.GridDisplayName ?? fieldInfo.Name);

                translators.Add(cInfo.Translators);
            }

            if (!reader.Read())
            {
                reader.Close();
                return;
            }

            frmLoadingDialog.ShowWindow();
            do
            {
                if (frmLoadingDialog.GetStatus())
                    break;
                var row = table.Rows.Add();
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    try
                    {
                        if (translators[i] != null)
                        {
                            var index = (int)reader[i];
                            row[i] = (translators.Count > i && i >= 0) ? translators[i][index] : index.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                            row[i] = reader[i];
                    }
                    catch { }
                }
            }
            while (reader.Read());
            reader.Close();
            frmLoadingDialog.HideWindow();
        }
    }

    public struct Relationship
    {
        private readonly List<Type> _referencingClasses;
        private readonly Type _referencedClass;

        private static Dictionary<Type, Relationship> _relationships = new Dictionary<Type, Relationship>();

        private Relationship(Type referencedClass)
        {
            _referencedClass = referencedClass;
            _referencingClasses = new List<Type>();
        }

        private void AddReference(Type referencingClass)
        {
            if (referencingClass != null & !_referencingClasses.Contains(referencingClass))
            {
                _referencingClasses.Add(referencingClass);
            }
        }

        public static void Add(Type referencedType, Type referencingType)
        {
            if (referencedType == null || referencingType == null)
                return;

            if (!_relationships.ContainsKey(referencedType))
            {
                var relationship = new Relationship(referencedType);
                relationship.AddReference(referencingType);

                _relationships.Add(referencedType, relationship);
            }
            else
            {
                _relationships[referencedType].AddReference(referencingType);
            }
        }

        public static bool CheckReferences(DatabaseEntity entity)
        {
            if (entity == null) return false;

            var referencedType = entity.GetType();
            if (!_relationships.ContainsKey(referencedType)) return false;

            var references = _relationships[referencedType]._referencingClasses;

            foreach (var reference in references.Where(reference => reference != null))
            {
                var fieldReferencedBy = reference.GetFields().Where(i => i.GetCustomAttributes(typeof(TableColumn), true).Any()).ToList();
                var filters = new List<Selector>();

                foreach (var fieldInfo in fieldReferencedBy)
                {
                    var attr = (TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0];
                    if (attr != null && attr.ReferencedClass == referencedType)
                    {
                        filters.Add(new Selector(fieldInfo.Name, referencedType.GetField(attr.ReferencedField).GetValue(entity)));
                    }
                }

                if (filters.Count <= 0)
                    continue;

                var command = GenericConnectionManager.GetConnectionManager(entity.GetSupportedDatabaseType()).GetConnection(entity.GetType()).CreateCommand();
                var selectQuery = "SELECT COUNT(*) FROM " + reference.Name + " WHERE " + filters.Aggregate("", (currentStr, item) => currentStr + (currentStr == "" ? "" : " OR ") + item.SetToCommand(ref command));
                command.CommandText = selectQuery;

                int result;
                if (Int32.TryParse(command.ExecuteScalar().ToString(), out result) && result > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
