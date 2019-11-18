using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace libDatabaseHelper.classes.generic
{
    public enum DatabaseType
    {
        SqlCE = 1,
        MySQL = 2,
        MSSQL = 3,
        Oracle = 4,
        Generic = 0
    }

    public class DatabaseOperationFailedEntity
    { 
        private GenericDatabaseEntity   _entity;
        private Exception               _exception;

        public DatabaseOperationFailedEntity(GenericDatabaseEntity entity, Exception exception)
        {
            _entity = entity;
            _exception = exception;
        }

        public GenericDatabaseEntity GetEntity() { return _entity; }

        public Exception GetException() { return _exception; }
    }

    public class GenericDatabaseManager
    {
        private static Dictionary<DatabaseType, GenericDatabaseManager> _registeredDatabaseManagers = new Dictionary<DatabaseType, GenericDatabaseManager>();

        public delegate void BulkDelete(Type type, Selector[] selectors); 
        public static event BulkDelete OnBulkDelete;

        private DatabaseType _supportedDatabase;
        private readonly List<Type> _registeredDatabaseEntities = new List<Type>();

        public GenericDatabaseManager(DatabaseType dbType)
        {
            if (dbType == DatabaseType.Generic)
            {
                throw new ArgumentException("A database cannot be of type generic !");
            }
            _supportedDatabase = dbType;
        }

        public DatabaseType GetSupportedDatabase()
        {
            return _supportedDatabase;
        }

        public List<Type> GetRegisteredDatabaseEntities()
        {
            return _registeredDatabaseEntities;
        }

        #region "DatabaseManager::TableExist()"
        public bool TableExist<T>()
        {
            return TableExist(typeof(T));
        }

        public virtual bool TableExist(Type type)
        {
            throw new NotImplementedException("The class of type GenericDatabaseManager is not intended for direct use and should be extended by a fellow DatabaseManager");
        }
        #endregion

        #region "DatabaseManager::CreateTable()"
        public bool CreateTable<T>()
        {
            return CreateTable(typeof(T));
        }

        public virtual bool CreateTable(Type type)
        {
            throw new NotImplementedException("The class of type GenericDatabaseManager is not intended for direct use and should be extended by a fellow DatabaseManager");
        }
        #endregion

        #region "DatabaseManager::DropTable()"
        public bool DropTable<T>()
        {
            return DropTable(typeof(T));
        }

        public virtual bool DropTable(Type type)
        {
            throw new NotImplementedException("The class of type GenericDatabaseManager is not intended for direct use and should be extended by a fellow DatabaseManager");
        }
        #endregion

        #region "DatabaseManager::Select()"
        public GenericDatabaseEntity[] Select(Type type)
        {
            return Select(type, null);
        }

        public GenericDatabaseEntity[] Select<T>()
        {
            return Select(typeof(T), null);
        }

        public GenericDatabaseEntity[] Select<T>(Selector[] selectors)
        {
            return Select(typeof(T), selectors);
        }

        public virtual GenericDatabaseEntity[] Select(Type type, Selector[] selectors)
        {
            throw new NotImplementedException("The class of type GenericDatabaseManager is not intended for direct use and should be extended by a fellow DatabaseManager");
        }
        #endregion

        #region "DatabaseManager::DeleteMatching() & DatabaseManager::DeleteAll()"
        public bool DeleteAll(Type type)
        {
            return DeleteMatching(type, null);
        }

        public bool DeleteAll<T>()
        {
            return DeleteMatching(typeof(T), null);
        }

        public bool DeleteMatching<T>(Selector[] selectors)
        {
            return DeleteMatching(typeof(T), selectors);
        }

        public virtual bool DeleteMatching(Type type, Selector[] selectors)
        {
            throw new NotImplementedException("The class of type GenericDatabaseManager is not intended for direct use and should be extended by a fellow DatabaseManager");
        }
        #endregion

        #region "DatabaseManager::FillDataTable()"
        public void FillDataTable(Type type, ref DataTable table)
        {
            FillDataTable(type, ref table, null, 0);
        }

        public void FillDataTable(Type type, ref DataTable table, int limit)
        {
            FillDataTable(type, ref table, null, limit);
        }

        public void FillDataTable(Type type, ref DataTable table, Selector[] selectors)
        {
            FillDataTable(type, ref table, selectors, 0);
        }

        public void FillDataTable<T>(ref DataTable table)
        {
            FillDataTable(typeof(T), ref table, null, 0);
        }

        public void FillDataTable<T>(ref DataTable table, int limit)
        {
            FillDataTable(typeof(T), ref table, null, limit);
        }

        public void FillDataTable<T>(ref DataTable table, Selector[] selectors)
        {
            FillDataTable(typeof(T), ref table, selectors, 0);
        }

        public void FillDataTable<T>(ref DataTable table, Selector[] selectors, int limit)
        {
            FillDataTable(typeof(T), ref table, selectors, limit);
        }

        public virtual void FillDataTable(Type type, ref DataTable table, Selector[] selectors, int limit)
        {
            throw new NotImplementedException("The class of type GenericDatabaseManager is not intended for direct use and should be extended by a fellow DatabaseManager");
        }

        public void FillDataTable<T>(ICollection<T> collection, ref DataTable table)where T : GenericDatabaseEntity
        {
            var obj = GenericDatabaseEntity.GetNonDisposableReferenceObject(typeof(T));
            if (obj == null)
                return;

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

            foreach (var item in collection)
            {
                var row = table.Rows.Add();
                var fieldIndex = 0;
                foreach (var fieldInfo in fieldInfos)
                {
                    try
                    {
                        var value = fieldInfo.GetValue(item);

                        if (translators[fieldIndex] != null)
                        {
                            row[fieldIndex] = (translators.Count > fieldIndex && fieldIndex >= 0) ? translators[fieldIndex].ToTranslated(value) : value.ToString();
                        }
                        else
                        {
                            row[fieldIndex] = value;
                        }
                    }
                    catch { /* IGNORED */ }

                    fieldIndex++;
                }
            }
        }

        public void UpdateDataTable(ICollection<GenericDatabaseEntity> collection, ref DataTable table)
        {
            if (collection.Count == 0) return;

            var obj = collection.First();
            var fieldInfos = obj.GetColumns(true).GetOtherColumns().Where(i => ((TableColumn)i.GetCustomAttributes(typeof(TableColumn), true)[0]).IsRetrievableFromDatabase).ToArray();
            
            table.BeginLoadData();

            var translators = new List<ITranslator>();
            var shouldAddColumns = table.Columns.Count == 0;

            foreach (var fieldInfo in fieldInfos)
            {
                try
                {
                    var cInfo = ((TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0]);
                    var translator = cInfo.TranslatorType == null ? null : TranslatorRegistry.Instance.Get(cInfo.TranslatorType);
                    translators.Add(translator);

                    if (shouldAddColumns)
                    {
                        var columnName = cInfo.GridDisplayName ?? fieldInfo.Name;

                        if (fieldInfo.FieldType == typeof(bool))
                            table.Columns.Add(columnName, typeof(bool));
                        else
                            table.Columns.Add(columnName);
                    }
                }
                catch { /* IGNORED */ }
            }

            const string hdcn = "libDatabaseHelper-entity-obj"; // Hidden Data Column Name

            if (shouldAddColumns)
            {
                table.Columns.Add(hdcn, typeof(GenericDatabaseEntity));
                table.Columns[hdcn].ColumnMapping = MappingType.Hidden;
            }

            var entries = table.Rows.OfType<DataRow>().Select((row) => new KeyValuePair<GenericDatabaseEntity, DataRow>(row[hdcn] as GenericDatabaseEntity, row)).ToDictionary(i => i.Key, i => i.Value);
            var entriesToRemove = entries.Where(i => collection.Contains(i.Key) == false).Select(i => i.Value);

            foreach (var item in collection)
            {
                var row = null as DataRow;
                if (entries.TryGetValue(item, out row) == false)
                    row = table.Rows.Add();

                var fieldIndex = 0;
                foreach (var fieldInfo in fieldInfos)
                {
                    try
                    {
                        var value = fieldInfo.GetValue(item);

                        if (translators[fieldIndex] != null)
                        {
                            row[fieldIndex] = (translators.Count > fieldIndex && fieldIndex >= 0) ? translators[fieldIndex].ToTranslated(value) : value.ToString();
                        }
                        else
                        {
                            row[fieldIndex] = value;
                        }
                    }
                    catch { /* IGNORED */ }

                    fieldIndex++;
                }

                row[hdcn] = item;
            }

            foreach (var row in entriesToRemove) table.Rows.Remove(row);

            table.EndLoadData();
        }
        #endregion

        #region "DatabaseManager::FillDataGridView()"
        public void FillDataGridView<T>(DataGridView grid)
        {
            FillDataGridView(typeof(T), grid, null, 0);
        }

        public void FillDataGridView<T>(DataGridView grid, int limit)
        {
            FillDataGridView(typeof(T), grid, null, limit);
        }

        public void FillDataGridView<T>(DataGridView grid, Selector[] selectors)
        {
            FillDataGridView(typeof(T), grid, selectors, 0);
        }

        public void FillDataGridView<T>(DataGridView grid, Selector[] selectors, int limit)
        {
            FillDataGridView(typeof(T), grid, selectors, limit);
        }

        public void FillDataGridView(Type type, DataGridView grid)
        {
            FillDataGridView(type, grid, null, 0);
        }

        public void FillDataGridView(Type type, DataGridView grid, int limit)
        {
            FillDataGridView(type, grid, null, limit);
        }

        public void FillDataGridView(Type type, DataGridView grid, Selector[] selectors)
        {
            FillDataGridView(type, grid, selectors, 0);
        }

        public void FillDataGridView(Type type, DataGridView grid, Selector[] selectors, int limit)
        {
            grid.Hide();

            var table = (DataTable)grid.DataSource ?? new DataTable();
            FillDataTable(type, ref table, selectors, limit);
            grid.DataSource = table;

            var databaseEntity = GenericDatabaseEntity.GetNonDisposableReferenceObject(type);
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
        public void FillDataGridViewAsItems<T>(ref DataGridView grid)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, null, 0);
        }

        public void FillDataGridViewAsItems<T>(ref DataGridView grid, int limit)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, null, limit);
        }

        public void FillDataGridViewAsItems<T>(ref DataGridView grid, Selector[] selectors)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, selectors, 0);
        }

        public void FillDataGridViewAsItems<T>(ref DataGridView grid, Selector[] selectors, int limit)
        {
            FillDataGridViewAsItems(typeof(T), ref grid, selectors, limit);
        }

        public void FillDataGridViewAsItems(Type type, ref DataGridView grid)
        {
            FillDataGridViewAsItems(type, ref grid, null, 0);
        }

        public void FillDataGridViewAsItems(Type type, ref DataGridView grid, int limit)
        {
            FillDataGridViewAsItems(type, ref grid, null, limit);
        }

        public void FillDataGridViewAsItems(Type type, ref DataGridView grid, Selector[] selectors)
        {
            FillDataGridViewAsItems(type, ref grid, selectors, 0);
        }

        public void FillDataGridViewAsItems(Type type, ref DataGridView grid, Selector[] selectors, int limit)
        {
            grid.Rows.Clear();
            UpdateDataGridViewAsItems(type, ref grid, selectors, limit);
        }
        #endregion

        #region "DatabaseManager::UpdateDataGridViewAsItems()"
        public void UpdateDataGridViewAsItems<T>(ref DataGridView grid)
        {
            UpdateDataGridViewAsItems(typeof(T), ref grid, null, 0);
        }

        public void UpdateDataGridViewAsItems<T>(ref DataGridView grid, int limit)
        {
            UpdateDataGridViewAsItems(typeof(T), ref grid, null, limit);
        }

        public void UpdateDataGridViewAsItems<T>(ref DataGridView grid, Selector[] selectors)
        {
            UpdateDataGridViewAsItems(typeof(T), ref grid, selectors, 0);
        }

        public void UpdateDataGridViewAsItems<T>(ref DataGridView grid, Selector[] selectors, int limit)
        {
            UpdateDataGridViewAsItems(typeof(T), ref grid, selectors, limit);
        }

        public void UpdateDataGridViewAsItems(Type type, ref DataGridView grid)
        {
            UpdateDataGridViewAsItems(type, ref grid, null, 0);
        }

        public void UpdateDataGridViewAsItems(Type type, ref DataGridView grid, int limit)
        {
            UpdateDataGridViewAsItems(type, ref grid, null, limit);
        }

        public void UpdateDataGridViewAsItems(Type type, ref DataGridView grid, Selector[] selectors)
        {
            UpdateDataGridViewAsItems(type, ref grid, selectors, 0);
        }

        public void UpdateDataGridViewAsItems(Type type, ref DataGridView grid, Selector[] selectors, int limit)
        {
            var entries = grid.Rows.OfType<DataGridViewRow>().Select(i => new KeyValuePair<GenericDatabaseEntity, int>()).ToDictionary(i => i.Key, i => i.Value);
            var results = Select(type, selectors);

            if (results == null || results.Length <= 0) return;

            var gridView = grid;
            var removedRows = entries.Where(i => results.Contains(i.Key) == false).Select(i => gridView.Rows[i.Value]);

            var noOfItemsAdded = 0;
            foreach (var entity in results)
            {
                if (limit > 0 && noOfItemsAdded >= limit) continue;
                if (grid.ColumnCount <= 0) entity.CreateDataGridViewHeader(grid);

                var index = 0;
                if (entries.TryGetValue(entity, out index) == false)
                    index = grid.Rows.Add();

                entity.AddToDataGridViewRow(grid, index);
                noOfItemsAdded++;
            }
            
            foreach (var row in removedRows) grid.Rows.Remove(row);
        }

        public void UpdateDataGridViewAsItems<T>(ref DataGridView grid, ICollection<T> collection, Selector[] selectors) where T : GenericDatabaseEntity
        {
            var list = FindMatchingEntities(collection.OfType<GenericDatabaseEntity>().ToList(), selectors);
            UpdateDataGridViewAsItems(ref grid, list);
        }

        public void UpdateDataGridViewAsItems(ref DataGridView grid, ICollection<GenericDatabaseEntity> collection, Selector[] selectors)
        {
            var list = FindMatchingEntities(collection.ToList(), selectors);
            UpdateDataGridViewAsItems(ref grid, list);
        }

        public void UpdateDataGridViewAsItems(ref DataGridView grid, ICollection<GenericDatabaseEntity> collection)
        {
            var results = collection.ToArray();
            var table = grid.DataSource as DataTable ?? new DataTable();

            UpdateDataTable(results, ref table);

            grid.DataSource = table;

            // TODO : For the moment sorting will be disabled as enabling this option will cause the
            // data grid view to crash when its updated (only happens when a column is sorted)
            foreach (DataGridViewColumn col in grid.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        #endregion

        #region DatabaseManager::ParseDataReader()
        public static GenericDatabaseEntity[] ParseDataReader(Type type, DbDataReader reader)
        {
            var list = new List<GenericDatabaseEntity>();

            try
            {
                while (reader.Read())
                {
                    try
                    {
                        var entity = Activator.CreateInstance(type) as GenericDatabaseEntity;
                        if (entity == null) continue;
                        entity.Parse(reader);
                        list.Add(entity);
                    }
                    catch { /* IGNORED */ }
                }
            }
            catch { /* IGNORED */ }

            try
            {
                reader.Close();
            }
            catch { /* IGNORED */ }

            return list.ToArray();
        }

        public static GenericDatabaseEntity[] ParseDataReader<T>(DbDataReader reader)
        {
            return ParseDataReader(typeof(T), reader);
        }
        #endregion

        #region "DatabaseManager::Add/Update-Entities()"
        public List<DatabaseOperationFailedEntity> AddEntities(IEnumerable<GenericDatabaseEntity> entitiesToAdd)
        {
            var failedEntities = new List<DatabaseOperationFailedEntity>();
            var entitiesToUpdatePerType = entitiesToAdd.ToArray().GroupBy(i => i.GetType());

            foreach (var entitiesPerType in entitiesToUpdatePerType)
            {
                var connection = GenericConnectionManager.GetConnectionManager(_supportedDatabase).GetConnection(entitiesPerType.Key);
                var transaction = TransactionObject.CreateTransactionObject(_supportedDatabase, connection);
                transaction.EnableRegularCommit(false);

                if (entitiesPerType.First().GetSupportedDatabaseType() != GetSupportedDatabase())
                {
                    throw new InvalidOperationException("Unable to insert an entity which is managed by a different database type.");
                }

                foreach (var entityUpdate in entitiesPerType)
                {
                    try
                    {
                        if (entityUpdate.Add(transaction) == false)
                        {
                            failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, new Exception("Failed to add entity.")));
                        }
                    }
                    catch (DatabaseEntityException ex)
                    {
                        failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, ex));
                    }
                    catch (DatabaseException ex)
                    {
                        failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, ex));
                    }
                    catch (System.Exception ex)
                    {
                        failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, ex));
                    }
                }

                transaction.Commit(true);
            }

            return failedEntities;
        }

        public List<DatabaseOperationFailedEntity> UpdateEntities(IEnumerable<GenericDatabaseEntity> entitiesToUpdate)
        {
            var failedEntities = new List<DatabaseOperationFailedEntity>();
            var entitiesToUpdatePerType = entitiesToUpdate.ToArray().GroupBy(i => i.GetType());

            foreach (var entitiesPerType in entitiesToUpdatePerType)
            {
                var connection = GenericConnectionManager.GetConnectionManager(_supportedDatabase).GetConnection(entitiesPerType.Key);
                var transaction = TransactionObject.CreateTransactionObject(_supportedDatabase, connection);
                transaction.EnableRegularCommit(false);

                if (entitiesPerType.First().GetSupportedDatabaseType() != GetSupportedDatabase())
                {
                    throw new InvalidOperationException("Unable to update an entity which is managed by a different database type.");
                }

                foreach (var entityUpdate in entitiesPerType)
                {
                    try
                    {
                        if (entityUpdate.Update(transaction) == false)
                        {
                            failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, new Exception("Failed to update entity.")));
                        }
                    }
                    catch (DatabaseEntityException ex)
                    {
                        failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, ex));
                    }
                    catch (DatabaseException ex)
                    {
                        failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, ex));
                    }
                    catch (System.Exception ex)
                    {
                        failedEntities.Add(new DatabaseOperationFailedEntity(entityUpdate, ex));
                    }
                }

                transaction.Commit(true);
            }

            return failedEntities;
        }
        #endregion

        internal static List<GenericDatabaseEntity> FindMatchingEntities(List<GenericDatabaseEntity> list, Selector[] selectors)
        {
            Regex regexFilter = null;
            var collected = new List<GenericDatabaseEntity>();
            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];

                var isEntityValid = true;
                foreach (var selector in selectors)
                {
                    if (selector.OpeartorType == Selector.Operator.Equal)
                    {
                        if (!entity.GetFieldValue(selector.Field).Equals(selector.FieldValue1))
                        {
                            isEntityValid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.LessThan)
                    {
                        var presentVal = entity.GetFieldValue(selector.Field);
                        var sentVal = selector.FieldValue1;
                        if (GenericFieldTools.Compare(presentVal, sentVal) != -1)
                        {
                            isEntityValid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.MoreThan)
                    {
                        var presentVal = entity.GetFieldValue(selector.Field);
                        var sentVal = selector.FieldValue1;
                        if (GenericFieldTools.Compare(presentVal, sentVal) != 1)
                        {
                            isEntityValid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.Between)
                    {
                        var presentVal = entity.GetFieldValue(selector.Field);
                        if (GenericFieldTools.Compare(presentVal, selector.FieldValue1) != 1 && GenericFieldTools.Compare(presentVal, selector.FieldValue2) != -1)
                        {
                            isEntityValid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.Like)
                    {
                        var strFieldValue = entity.GetFieldValue(selector.Field).ToString();

                        if (regexFilter == null)
                        {
                            var strSentFilter = selector.FieldValue1.ToString();
                            regexFilter = new Regex(Regex.Escape(strSentFilter).Replace("%", "(.*)"));
                        }

                        isEntityValid = regexFilter.IsMatch(strFieldValue);
                    }
                }

                if (isEntityValid)
                {
                    collected.Add(entity);
                }
            }
            return collected;
        }

        protected void _OnBulkDelete(Type type, Selector[] selectors)
        { 
            if (OnBulkDelete != null)
            {
                try
                {
                    OnBulkDelete(type, selectors);
                }
                catch { }
            }
        }

        public static void RegisterDatabaseManager(GenericDatabaseManager databaseManager)
        {
            RegisterDatabaseManager(databaseManager, false);
        }

        public static void RegisterDatabaseManager<T>()
        {
            RegisterDatabaseManager(typeof(T));
        }

        public static void RegisterDatabaseManager(Type type)
        {
            var instance = Activator.CreateInstance(type) as GenericDatabaseManager;
            if (instance != null)
            {
                RegisterDatabaseManager(instance);
            }
        }

        public static void RegisterDatabaseManager<T>(bool forceRegistration)
        {
            RegisterDatabaseManager(typeof(T), forceRegistration);
        }

        public static void RegisterDatabaseManager(Type type, bool forceRegistration)
        {
            var instance = Activator.CreateInstance(type) as GenericDatabaseManager;
            if (instance != null)
            {
                RegisterDatabaseManager(instance, forceRegistration);
            }
        }

        public static void RegisterDatabaseManager(GenericDatabaseManager databaseManager, bool forceRegistration)
        {
            if (forceRegistration)
            {
                _registeredDatabaseManagers[databaseManager.GetSupportedDatabase()] = databaseManager;
            }
            else if (_registeredDatabaseManagers.ContainsKey(databaseManager.GetSupportedDatabase()) == false)
            {
                _registeredDatabaseManagers[databaseManager.GetSupportedDatabase()] = databaseManager;
            }

            if (databaseManager.GetSupportedDatabase() == DatabaseType.SqlCE)
            {
                try
                {
                    var connectionManager = (GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE) as sqlce.ConnectionManager);
                    if (connectionManager != null) connectionManager.LoadConnectionData();
                }
                catch { /* IGNORED */ }
            }
        }

        public static GenericDatabaseManager GetDatabaseManager(DatabaseType dbType)
        { 
            if (_registeredDatabaseManagers.ContainsKey(dbType) == false)
            {
                return null;
            }
            return _registeredDatabaseManagers[dbType];
        }

        public static void UnregisterAllDatabaseManagers()
        {
            _registeredDatabaseManagers.Clear();
        }

        public PrimaryKeyConstraintDetails GetPrimaryKeyDetails<T>()
        {
            return GetPrimaryKeyDetails(typeof(T));
        }

        public string[] GetTableFields<T>()
        {
            return GetTableFields(typeof(T));
        }

        public virtual PrimaryKeyConstraintDetails GetPrimaryKeyDetails(Type t)
        {
            throw new NotImplementedException("The 'GetPrimaryKeyDetails' should be implemented by the corresponding per database type 'DatabaseManager'.");
        }

        public virtual string[] GetTableFields(Type type)
        {
            throw new NotImplementedException("The 'GetTableFields' method should be implemented on the corresponding per-database type 'DatabaseManager'.s");
        }

        public class PrimaryKeyConstraintDetails
        {
            public readonly string ConstraintName;
            public readonly string[] PrimaryKeyFields;

            public PrimaryKeyConstraintDetails(string constraintName, string[] primaryKeyFields)
            {
                ConstraintName = constraintName;
                PrimaryKeyFields = primaryKeyFields;
            }
        }
    }
}
