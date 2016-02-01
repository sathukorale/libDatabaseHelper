using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.Common;

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

            var databaseEntity = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
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
        public static GenericDatabaseEntity[] ParseDataReader(Type type, DbDataReader reader)
        {
            var list = new List<GenericDatabaseEntity>();
            try
            {
                while (reader.Read())
                {
                    try
                    {
                        var entity = GenericDatabaseEntity.GetNonDisposableRefenceObject(type);
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

        public static GenericDatabaseEntity[] ParseDataReader<T>(DbDataReader reader)
        {
            return ParseDataReader(typeof(T), reader);
        }
        #endregion

        protected void _OnBulkDelete(Type type, Selector[] selectors)
        { 
            if (OnBulkDelete != null)
            {
                OnBulkDelete(type, selectors);
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
                    (GenericConnectionManager.GetConnectionManager(DatabaseType.SqlCE) as libDatabaseHelper.classes.sqlce.ConnectionManager).LoadConnectionData();
                }
                catch {}
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
    }
}
