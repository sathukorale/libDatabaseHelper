using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Data.SqlServerCe;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.forms;

namespace libDatabaseHelper.classes.sqlce
{
    public class DatabaseManager
    {
        private static readonly List<Type> _registeredDBEntities = new List<Type>();

        public delegate void BulkDelete(Type type, Selector[] selectors);
        public static event BulkDelete OnBulkDelete;

        public static List<Type> GetRegisteredTypes()
        {
            return _registeredDBEntities;
        }

        #region "DatabaseManager::TableExist()"
        public static bool TableExist<T>()
        {
            return TableExist(typeof(T));
        }

        public static bool TableExist(Type type)
        {
            var entity = Activator.CreateInstance(type) as DatabaseEntity;
            if (entity == null)
                return false;

            var fields = entity.GetColumns(true).GetOtherColumns().ToList();

            var connection = ConnectionManager.GetConnection();
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
                                        columnAttributes.IsAutogen || columnAttributes.IsUnique ||
                                        columnAttributes.PrimaryKey,
                                        columnAttributes.Length);
                    if (FieldTools.IsTypeNumber(column.FieldType) && columnAttributes.IsAutogen &&
                        columnAttributes.AutogenMethod == null)
                    {
                        columnToAdd += " IDENTITY ";
                    }
                    else if (columnAttributes.IsUnique)
                    {
                        columnToAdd += " UNIQUE ";
                    }

                    command.CommandText = "ALTER TABLE " + type.Name + " ADD COLUMN " + columnToAdd;
                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE TABLE " + type.Name + " SET " + columnToAdd + "=" + (FieldTools.IsTypeNumber(column.FieldType) ? "-1" : ("''"));
                    command.ExecuteNonQuery();
                }
            }

            return true;
        }
        #endregion

        #region "DatabaseManager::CreateTable()"
        public static bool CreateTable<T>()
        {
            return CreateTable(typeof(T));
        }

        public static bool CreateTable(Type type)
        {
            var obj = Activator.CreateInstance(type) as DatabaseEntity;
            if (obj == null)
            {
                return false;
            }

            if (!_registeredDBEntities.Contains(type))
            {
                _registeredDBEntities.Add(type);
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
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();

            var variabeDeclarations = "";
            foreach (var column in result.GetOtherColumns())
            {
                var columnAttributes = (TableColumn)column.GetCustomAttributes(typeof(TableColumn), true)[0];
                variabeDeclarations += (variabeDeclarations == "" ? " " : ", ") + column.Name + " " +
                                       FieldTools.GetDbTypeString(column.FieldType,
                                           columnAttributes.IsAutogen || columnAttributes.IsUnique ||
                                           columnAttributes.PrimaryKey,
                                           columnAttributes.Length);
                if (FieldTools.IsTypeNumber(column.FieldType) && columnAttributes.IsAutogen &&
                    columnAttributes.AutogenMethod == null)
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
        #endregion

        #region "DatabaseManager::DropTable()"
        public static bool DropTable<T>()
        {
            return DropTable(typeof(T));
        }

        public static bool DropTable(Type type)
        {
            var obj = Activator.CreateInstance(type) as DatabaseEntity;
            if (obj == null)
            {
                return false;
            }

            if (_registeredDBEntities.Contains(type))
            {
                _registeredDBEntities.Remove(type);
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

            var connection = ConnectionManager.GetConnection();
            var command = connection.CreateCommand();

            var createStatement = "DROP TABLE " + type.Name;

            command.CommandText = createStatement;
            return command.ExecuteNonQuery() >= 0;
        }
        #endregion

        #region "DatabaseManager::Select()"
        public static DatabaseEntity[] Select(Type type)
        {
            return Select(type, null);
        }

        public static DatabaseEntity[] Select<T>()
        {
            return Select(typeof(T), null);
        }

        public static DatabaseEntity[] Select<T>(Selector[] selectors)
        {
            return Select(typeof(T), selectors);
        }

        public static DatabaseEntity[] Select(Type type, Selector[] selectors)
        {
            var obj = Activator.CreateInstance(type) as DatabaseEntity;
            if (obj == null) return new DatabaseEntity[0];
            var result = obj.GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var selectFields =
                result.GetOtherColumns()
                    .Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsRetrievable)
                    .Aggregate("", (current, selector) => current + ((current == "" ? "" : ", ") + selector.Name));

            var connection = ConnectionManager.GetConnection();
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
        #endregion

        #region "DatabaseManager::DeleteMatching() & DatabaseManager::DeleteAll()"
        public static bool DeleteAll(Type type)
        {
            return DeleteMatching(type, null);
        }

        public static bool DeleteAll<T>()
        {
            return DeleteMatching(typeof(T), null);
        }

        public static bool DeleteMatching<T>(Selector[] selectors)
        {
            return DeleteMatching(typeof(T), selectors);
        }

        public static bool DeleteMatching(Type type, Selector[] selectors)
        {
            var obj = Activator.CreateInstance(type) as DatabaseEntity;
            if (obj == null) return false;
            var result = obj.GetColumns(true);
            if (result == null || result.GetPrimaryKeys() == null || !result.GetPrimaryKeys().Any())
            {
                throw new DatabaseException(DatabaseException.ErrorType.NoPrimaryKeyColumnsFound);
            }

            var connection = ConnectionManager.GetConnection();
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
                if (OnBulkDelete != null)
                    OnBulkDelete(type, selectors);
                return true;
            }
            return false;
        }
        #endregion

        #region "DatabaseManager::FillDataTable()"
        public static void FillDataTable(Type type, ref DataTable table)
        {
            FillDataTable(type, ref table, null, 0);
        }

        public static void FillDataTable(Type type, ref DataTable table, int limit)
        {
            FillDataTable(type, ref table, null, limit);
        }

        public static void FillDataTable(Type type, ref DataTable table, Selector[] selectors)
        {
            FillDataTable(type, ref table, selectors, 0);
        }

        public static void FillDataTable<T>(ref DataTable table)
        {
            FillDataTable(typeof(T), ref table, null, 0);
        }

        public static void FillDataTable<T>(ref DataTable table, int limit)
        {
            FillDataTable(typeof(T), ref table, null, limit);
        }

        public static void FillDataTable<T>(ref DataTable table, Selector[] selectors)
        {
            FillDataTable(typeof(T), ref table, selectors, 0);
        }

        public static void FillDataTable<T>(ref DataTable table, Selector[] selectors, int limit)
        {
            FillDataTable(typeof(T), ref table, selectors, limit);
        }

        public static void FillDataTable(Type type, ref DataTable table, Selector[] selectors, int limit)
        {
            var obj = Activator.CreateInstance(type) as DatabaseEntity;
            if (obj == null)
                return;

            var commandString = DatabaseEntity.GetSelectCommandString(type);
            var whereQuery = "";
            var command = ConnectionManager.GetConnection().CreateCommand();

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

            var fieldInfos = obj.GetColumns(true).GetOtherColumns().Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsRetrievable).ToArray();

            table.Rows.Clear();
            table.Columns.Clear();

            var translators = new List<string[]>();

            foreach (var fieldInfo in fieldInfos)
            {
                var cInfo = ((TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0]);
                if (fieldInfo.FieldType == typeof(bool))
                    table.Columns.Add(cInfo.DisplayName ?? fieldInfo.Name, typeof(bool));
                else
                    table.Columns.Add(cInfo.DisplayName ?? fieldInfo.Name);

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
        #endregion

        #region "DatabaseManager::FillDataGridView()"
        public static void FillDataGridView<T>(DataGridView grid)
        {
            FillDataGridView(typeof(T), grid, null, 0);
        }

        public static void FillDataGridView<T>(DataGridView grid, int limit)
        {
            FillDataGridView(typeof(T), grid, null, limit);
        }

        public static void FillDataGridView<T>(DataGridView grid, Selector[] selectors)
        {
            FillDataGridView(typeof(T), grid, selectors, 0);
        }

        public static void FillDataGridView<T>(DataGridView grid, Selector[] selectors, int limit)
        {
            FillDataGridView(typeof(T), grid, selectors, limit);
        }

        public static void FillDataGridView(Type type, DataGridView grid)
        {
            FillDataGridView(type, grid, null, 0);
        }

        public static void FillDataGridView(Type type, DataGridView grid, int limit)
        {
            FillDataGridView(type, grid, null, limit);
        }

        public static void FillDataGridView(Type type, DataGridView grid, Selector[] selectors)
        {
            FillDataGridView(type, grid, selectors, 0);
        }

        public static void FillDataGridView(Type type, DataGridView grid, Selector[] selectors, int limit)
        {
            grid.Hide();

            var table = (DataTable)grid.DataSource ?? new DataTable();
            FillDataTable(type, ref table, selectors, limit);
            grid.DataSource = table;

            var databaseEntity = Activator.CreateInstance(type) as DatabaseEntity;
            if (databaseEntity != null)
            {
                var fields = databaseEntity.GetColumns(true).GetOtherColumns().Where(i => false);
                foreach (var fieldInfo in fields)
                {
                    var dataGridViewColumn = grid.Columns[fieldInfo.Name];
                    if (dataGridViewColumn != null) dataGridViewColumn.ValueType = typeof(bool);
                }
            }

            grid.Show();
        }
        #endregion

        #region "DatabaseManager::FillDataGridViewAsItems()"
        public static void FillDataGridViewAsItems<T>(ref DataGridView grid)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, null, 0);
        }

        public static void FillDataGridViewAsItems<T>(ref DataGridView grid, int limit)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, null, limit);
        }

        public static void FillDataGridViewAsItems<T>(ref DataGridView grid, Selector[] selectors)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, selectors, 0);
        }

        public static void FillDataGridViewAsItems<T>(ref DataGridView grid, Selector[] selectors, int limit)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, selectors, limit);
        }

        public static void FillDataGridViewAsItems(Type type, ref DataGridView grid)
        {
            FillDataGridViewAsItems(type, ref grid, null, 0);
        }

        public static void FillDataGridViewAsItems(Type type, ref DataGridView grid, int limit)
        {
            FillDataGridViewAsItems(type, ref grid, null, limit);
        }

        public static void FillDataGridViewAsItems(Type type, ref DataGridView grid, Selector[] selectors)
        {
            FillDataGridViewAsItems(type, ref grid, selectors, 0);
        }

        public static void FillDataGridViewAsItems(Type type, ref DataGridView grid, Selector[] selectors, int limit)
        {
            grid.Rows.Clear();
            var results = Select(type, selectors);

            if (results != null && results.Length > 0)
            {
                int no_of_items_added = 0;
                foreach (var entity in results)
                {
                    if (limit <= 0 || no_of_items_added < limit)
                    {
                        entity.AddToDataGridViewRow(grid);
                        no_of_items_added++;
                    }
                }
            }
        }
        #endregion

        #region DatabaseManager::ParseDataReader()
        public static DatabaseEntity[] ParseDataReader(Type type, SqlCeDataReader reader)
        {
            var list = new List<DatabaseEntity>();
            try
            {
                while (reader.Read())
                {
                    try
                    {
                        var entity = Activator.CreateInstance(type) as DatabaseEntity;
                        if (entity == null) continue;
                        entity.Parse(reader);
                        list.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch
            {
            }
            reader.Close();
            return list.ToArray();
        }

        public static DatabaseEntity[] ParseDataReader<T>(SqlCeDataReader reader)
        {
            return ParseDataReader(typeof(T), reader);
        }
        #endregion
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

                var command = ConnectionManager.GetConnection().CreateCommand();
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
