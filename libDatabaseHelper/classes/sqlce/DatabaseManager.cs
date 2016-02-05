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

            var manager = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase());
            var connection = manager.GetConnection(type);
            if (connection == null || connection.State != ConnectionState.Open)
            {
                throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.UnableToConnectToTheDatabase);
            }

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='" + (entity.GetType().Name) + "'";
            var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                command.Dispose();

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

                    command.CommandText = "UPDATE TABLE " + type.Name + " SET " + column.Name + "=" + (GenericFieldTools.IsTypeNumber(column.FieldType) ? "-1" : ("''"));
                    command.ExecuteNonQuery();
                }
            }

            command.Dispose();

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
            if (connection == null || connection.State != ConnectionState.Open)
            {
                throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.UnableToConnectToTheDatabase);
            }

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
            var executionResult = command.ExecuteNonQuery();
            command.Dispose();

            return executionResult >= 0;
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
            var executionResult = command.ExecuteNonQuery();
            command.Dispose();

            return executionResult >= 0;
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
            var results = ParseDataReader(type, reader);
            command.Dispose();
            return results;
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
            var executionResult = command.ExecuteNonQuery();
            command.Dispose();

            if (executionResult >= 0)
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
                command.Dispose();
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
            command.Dispose();
        }
    }
}
