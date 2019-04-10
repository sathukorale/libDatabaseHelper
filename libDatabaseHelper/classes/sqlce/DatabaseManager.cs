using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.forms;
using libDatabaseHelper.classes.sqlce.entities;
using System.Data.SqlServerCe;
using System.IO;
using libDatabaseHelper.forms.controls;

namespace libDatabaseHelper.classes.sqlce
{
    public class DatabaseManager : GenericDatabaseManager
    {
        public DatabaseManager() : base(DatabaseType.SqlCE)
        {
            CreateTable<GenericConnectionDetails>();
            CreateTable<AuditEntry>();
            CreateTable<SearchFilterSettings>();
        }

        private delegate void FillArguments(ref DbCommand command);

        private bool ExecuteNonQuery(DbConnection connection, string commandString, FillArguments argumentFiller = null)
        {
            var command = connection.CreateCommand();
            var status = false;
            var transaction = (null as SqlCeTransaction);

            try
            {
                transaction = (SqlCeTransaction) connection.BeginTransaction(IsolationLevel.ReadCommitted);

                command.Transaction = transaction;
                command.CommandText = commandString;

                if (argumentFiller != null) argumentFiller.Invoke(ref command);

                command.ExecuteNonQuery();
                transaction.Commit(CommitMode.Immediate);

                status = true;
            }
            catch (System.Exception)
            {
                if (transaction != null) transaction.Rollback();
            }

            command.Dispose();
            return status;
        }

        public override bool TableExist(Type type)
        {
            var entity = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (entity == null)
                return false;

            var columns = entity.GetColumns(true);
            var fields = columns.GetOtherColumns().ToList();

            var manager = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase());
            var connection = manager.GetConnection(type);
            if (connection == null || connection.State != ConnectionState.Open)
            {
                throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.UnableToConnectToTheDatabase);
            }

            var command = connection.CreateCommand();
            var availableColumns = GetTableFields(type);
            var listToRemove = new List<string>();

            if (availableColumns == null || availableColumns.Any() == false) return false;

            foreach (var columnName in availableColumns)
            {
                List<FieldInfo> found;
                var existing = (found = fields.Where(i => i.Name.ToLower() == columnName).ToList()).Any();

                if (existing)
                {
                    fields.Remove(found[0]);
                }
                else
                {
                    listToRemove.Add(columnName);
                }
            }

            foreach (var fieldInfo in columns.GetOtherColumns().ToList())
            {
                var columnAttributes = (TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0];

                if (columnAttributes.ReferencedField != null && columnAttributes.ReferencedClass != null)
                {
                    Relationship.Add(columnAttributes.ReferencedClass, type);
                }
            }

            var currentPrimaryKeyDetails = GetPrimaryKeyDetails(type);
            var hasThePrimaryKeyChanged = (currentPrimaryKeyDetails != null) && 
                                          (columns.GetPrimaryKeys().Any(i => currentPrimaryKeyDetails.PrimaryKeyFields.Contains(i.Name.ToLower()) == false) ||
                                           currentPrimaryKeyDetails.PrimaryKeyFields.Any(i => columns.GetPrimaryKeys().Any(ii => ii.Name.ToLower() == i) == false));

            if (hasThePrimaryKeyChanged)
            {
                DeleteAll(type);

                var commandToDropConstraint = string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", type.Name, currentPrimaryKeyDetails.ConstraintName);
                if (ExecuteNonQuery(connection, commandToDropConstraint) == false)
                {
                    throw new Exception(string.Format("Failed to drop the primary key constraint, '{0}', which is a required step to imprint the current table structure.", currentPrimaryKeyDetails.ConstraintName));
                }
            }

            if (listToRemove.Any())
            {
                foreach (var column in listToRemove)
                {
                    var commandToRemove = string.Format("ALTER TABLE {0} DROP COLUMN {1}", type.Name, column);
                    ExecuteNonQuery(connection, commandToRemove);
                }
            }

            if (fields.Any())
            {
                foreach (var column in fields)
                {
                    var columnAttributes = (TableColumn)column.GetCustomAttributes(typeof(TableColumn), true)[0];
                    var columnToAdd =   column.Name + " " + FieldTools.GetDbTypeString(column.FieldType, columnAttributes.Length);
                    if (GenericFieldTools.IsTypeNumber(column.FieldType) && columnAttributes.IsAutogenerated &&
                        columnAttributes.AutogenrationMethod == null)
                    {
                        columnToAdd += " IDENTITY ";
                    }
                    else if (columnAttributes.IsUnique)
                    {
                        columnToAdd += " UNIQUE ";
                    }

                    if (ExecuteNonQuery(connection, string.Format("ALTER TABLE {0} ADD COLUMN {1}", type.Name, columnToAdd)))
                    {
                        var columnLocal = column;
                        ExecuteNonQuery(connection, string.Format("UPDATE {0} SET {1}=@{1}", type.Name, columnLocal.Name), (ref DbCommand dbCommand) => { GenericUtils.AddWithValue(ref dbCommand, "@" + columnLocal.Name, GenericFieldTools.GetDefaultValue(columnLocal.FieldType, columnAttributes.DefaultValue)); });
                    }
                }
            }

            if (hasThePrimaryKeyChanged)
            {
                foreach (var primaryKey in columns.GetPrimaryKeys())
                {
                    var columnAttributes = (TableColumn) primaryKey.GetCustomAttributes(typeof(TableColumn), true)[0];
                    var dataType = FieldTools.GetDbTypeString(primaryKey.FieldType, columnAttributes.Length);

                    ExecuteNonQuery(connection, string.Format("ALTER TABLE {0} ALTER COLUMN {1} {2} NOT NULL", type.Name, primaryKey.Name, dataType));
                }

                var primaryKeyFieldString = columns.GetPrimaryKeys().Select(i => i.Name).Aggregate((a, b) => a + ", " + b);
                ExecuteNonQuery(connection, string.Format("ALTER TABLE {0} ADD CONSTRAINT pk_{1} PRIMARY KEY({2})", type.Name, DateTime.Now.Ticks, primaryKeyFieldString));
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
                                       FieldTools.GetDbTypeString(column.FieldType, columnAttributes.Length);
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
            var transaction = (SqlCeTransaction) connection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                command.Transaction = transaction;
                command.CommandText = createStatement;
                var executionResult = command.ExecuteNonQuery();

                transaction.Commit();
                command.Dispose();

                return executionResult >= 0;
            }
            catch (System.Exception)
            {
                transaction.Rollback();
            }
            return false;
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
            var transaction = (SqlCeTransaction) connection.BeginTransaction(IsolationLevel.ReadCommitted);
            var success = false;

            try
            {
                command.Transaction = transaction;
                command.CommandText = createStatement;
                var executionResult = command.ExecuteNonQuery();

                transaction.Commit(CommitMode.Immediate);
                success = (executionResult >= 0);
            }
            catch (System.Exception)
            {
                transaction.Rollback();
            }

            command.Dispose();
            return success;
        }

        public override GenericDatabaseEntity[] Select(Type type, Selector[] selectors)
        {
            var obj = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
            if (obj == null) return new GenericDatabaseEntity[0];

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

            var selectStatement = "DELETE FROM " + obj.GetType().Name + (selectors != null && selectors.Any() ? (string.Format(" WHERE {0}", whereStatement)) : "");
            var success = false;
            var transaction = null as DbTransaction;

            try
            {
                transaction = (SqlCeTransaction)connection.BeginTransaction(IsolationLevel.ReadCommitted);

                command.Transaction = transaction;
                command.CommandText = selectStatement;

                var executionResult = command.ExecuteNonQuery();
                if (executionResult >= 0)
                {
                    transaction.Commit();
                    success = true;

                    _OnBulkDelete(type, selectors);
                }
            }
            catch
            {
                if (transaction != null) transaction.Rollback();
            }

            command.Dispose();
            return success;
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

            var translators = new List<ITranslator>();

            foreach (var fieldInfo in fieldInfos)
            {
                try
                {
                    var cInfo = ((TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0]);

                    if (fieldInfo.FieldType == typeof(bool))
                        table.Columns.Add(cInfo.GridDisplayName ?? fieldInfo.Name, typeof(bool));
                    else
                        table.Columns.Add(cInfo.GridDisplayName ?? fieldInfo.Name);

                    var translator = cInfo.TranslatorType == null ? null : TranslatorRegistry.Instance.Get(cInfo.TranslatorType);
                    translators.Add(translator);
                }
                catch { /* IGNORED */ }
            }

            if (!reader.Read())
            {
                try { reader.Close(); } catch { /* IGNORED */ }
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
                            var value = reader[i];
                            row[i] = (translators.Count > i && i >= 0) ? translators[i].ToTranslated(value) : value.ToString();
                        }
                        else
                            row[i] = reader[i];
                    }
                    catch { /* IGNORED */ }
                }
            }
            while (reader.Read());

            try { reader.Close(); } catch { /* IGNORED */ }
            frmLoadingDialog.HideWindow();
            command.Dispose();
        }

        public override PrimaryKeyConstraintDetails GetPrimaryKeyDetails(Type type)
        {
            var command = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type).CreateCommand();
            var commandString = string.Format("SELECT INDEX_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.INDEXES WHERE PRIMARY_KEY = 1 AND TABLE_NAME = '{0}'", type.Name);

            command.CommandText = commandString;

            var reader = command.ExecuteReader();
            var primaryKeysPerConstraint = new Dictionary<string, List<string>>();

            try
            {
                while (reader.Read())
                {
                    try
                    {
                        var constraintName = reader.GetString(0);
                        var fieldName = reader.GetString(1).ToLower();

                        if (primaryKeysPerConstraint.ContainsKey(constraintName) == false) primaryKeysPerConstraint.Add(constraintName, new List<string>());

                        primaryKeysPerConstraint[constraintName].Add(fieldName);
                    }
                    catch { /* IGNORED */ }
                }
            }
            catch { /* IGNORED */ }

            try { reader.Close(); } catch { /* IGNORED */ }
            command.Dispose();

            if (primaryKeysPerConstraint.Any() == false) return null;
            if (primaryKeysPerConstraint.Count > 1) throw new InvalidDataException(string.Format("Due to some reason the table '{0}' has more than 1 primary key.", type.Name));

            var firstEntry = primaryKeysPerConstraint.First();
            return new PrimaryKeyConstraintDetails(firstEntry.Key, firstEntry.Value.ToArray());
        }

        public override string[] GetTableFields(Type type)
        {
            var connection = GenericConnectionManager.GetConnectionManager(GetSupportedDatabase()).GetConnection(type);
            var command = connection.CreateCommand();

            command.CommandText = string.Format("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{0}'", type.Name);

            var reader = command.ExecuteReader();
            var columns = new List<string>();

            while (reader.Read())
            {
                try
                {
                    var columnName = reader.GetString(0).ToLower();
                    columns.Add(columnName);
                }
                catch { /* IGNORED */ }
            }

            try { reader.Close(); } catch { /* IGNORED */ }
            command.Dispose();

            return columns.ToArray();
        }
    }
}
