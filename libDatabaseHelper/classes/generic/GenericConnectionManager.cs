using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Data.Common;

namespace libDatabaseHelper.classes.generic
{
    [TableProperties(Registration=libDatabaseHelper.classes.generic.TableProperties.RegistrationType.RegisterOnDatabaseManager)]
    public class GenericConnectionDetails : libDatabaseHelper.classes.sqlce.DatabaseEntity
    {
        [TableColumn(true, true, 100)]
        public string TypeName;

        [TableColumn(false, true)]
        public int DatabaseType;

        [TableColumn(false, true)]
        public string ConnectionString;
    }

    class NullType {}

    public class GenericConnectionManager
    {
        private DatabaseType _databaseType;
        protected Dictionary<Type, string> _connectionStrings;
        protected Dictionary<string, List<DbConnection>> _databaseConnections;
        
        protected static string _localDataFolder;
        private static Dictionary<DatabaseType, GenericConnectionManager> _registeredConnectionManagers = new Dictionary<DatabaseType, GenericConnectionManager>();
        protected Dictionary<string, GenericConnectionDetails> _loadedConnectionDetails = new Dictionary<string,GenericConnectionDetails>();

        public GenericConnectionManager(DatabaseType supportedDatabase)
        {
            _databaseType = supportedDatabase;
            _connectionStrings = new Dictionary<Type, string>();
            _databaseConnections = new Dictionary<string, List<DbConnection>>();
        }

        #region "GetConnection"
        public DbConnection GetConnection()
        {
            return GetConnection(typeof(NullType));
        }

        public DbConnection GetConnection<T>()
        {
            return GetConnection(typeof(T));
        }

        public DbConnection GetConnection(Type t)
        {
            string connectionString = null;
            try
            {
                connectionString = GetConnectionString(t);
            }
            catch (DatabaseConnectionException ex)
            {
                throw ex;
                /*if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    return null;
                    MessageBox.Show("The application was not configured with a connection string. Can you please enter the connection string in the next dialog.", "Connection String Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return (frmConnectionStringSetter.ShowWindow(t, _databaseType) ? GetConnection() : null);
                }
                else
                {
                    return null;
                }*/
            }

            if (_databaseConnections == null)
            {
                _databaseConnections = new Dictionary<string, List<DbConnection>>();
            }
            else
            {
                if (_databaseConnections.ContainsKey(connectionString))
                {
                    var connections = _databaseConnections[connectionString];
                    foreach (var record in connections)
                    {
                        var connection = record;
                        if (connection != null && connection.State == ConnectionState.Open)
                        {
                            return connection;
                        }
                    }
                }
                else
                {
                    _databaseConnections[connectionString] = new List<DbConnection>();
                }
            }

            DbConnection connectionCreated = null;
            try
            {
                connectionCreated = CreateConnection(t, connectionString);
                if (connectionCreated.State == ConnectionState.Closed)
                {
                    connectionCreated.Open();
                }
            }
            catch (System.Exception ex) { Console.WriteLine("Unable to Open Database Connection. Error Details : " + ex.Message); }

            if (connectionCreated != null && connectionCreated.State == ConnectionState.Open && _databaseConnections[connectionString].Contains(connectionCreated) == false)
            {
                _databaseConnections[connectionString].Add(connectionCreated);
            }

            return connectionCreated;
        }
        #endregion

        #region "GetConnectionString"
        public string GetConnectionString()
        {
            return GetConnectionString(typeof(NullType));
        }

        public string GetConnectionString<T>()
        {
            return GetConnectionString(typeof(T));
        }

        public virtual string GetConnectionString(Type type)
        {
            if (_connectionStrings.ContainsKey(type) == false)
            {
                if (_connectionStrings.ContainsKey(typeof(NullType)) == false)
                {
                    if (_databaseType == DatabaseType.SqlCE)
                    {
                        throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoConnectionStringFound);
                    }

                    try
                    {
                        string connectionString = GetConnectionManager(DatabaseType.SqlCE).GetConnectionString(type);
                        _connectionStrings[type] = connectionString;
                        return connectionString;
                    }
                    catch (DatabaseConnectionException)
                    {
                        throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoConnectionStringFound);
                    }
                }
                return _connectionStrings[typeof(NullType)];
            }
            return _connectionStrings[type];
        }
        #endregion

        #region "SetConnectionString"
        public void SetConnectionString(string connectionString)
        {
            SetConnectionString(typeof(NullType), connectionString);
        }

        public void SetConnectionString<T>(string connectionString)
        {
            SetConnectionString(typeof(T), connectionString);
        }

        public void SetConnectionString(Type type, string connectionString)
        {
            var connectionSuccessful = CheckConnectionString(connectionString);
            if (connectionSuccessful || (type == typeof(GenericConnectionDetails)))
            {
                if (connectionSuccessful == false)
                {
                    var dbFilePath = GenericUtils.GetDataSourceFromConnnectionString(connectionString);
                    try
                    {
                        File.WriteAllBytes(dbFilePath, Properties.Resources.dblocaldata);

                        if (CheckConnectionString(connectionString) == false)
                        {
                            return;
                        }
                    }
                    catch { Console.WriteLine(""); }
                }

                try
                {
                    _connectionStrings[type] = connectionString;
                    var connectionData = new GenericConnectionDetails() { 
                        DatabaseType = (int)_databaseType,
                        TypeName = type.Namespace + "." + type.Name,
                        ConnectionString = connectionString
                    };
                    if (connectionData.Exist() == GenericDatabaseEntity.ExistCondition.None)
                    {
                        connectionData.Add();
                    }
                    else
                    {
                        connectionData.Update();
                    }
                }
                catch (System.Exception ex) { Console.WriteLine(ex.Message); }
            }
        }
        #endregion

        #region "Virtual Implementations"
        protected virtual DbConnection CreateConnection(Type t, string connectionString)
        {
            throw new NotImplementedException("");
        }

        public virtual bool CheckConnectionString(string connectionString)
        {
            throw new NotImplementedException("");
        }

        protected virtual void InstallDefaultClasses()
        {
        }
        #endregion

        public void CloseConnections()
        {
            if (_databaseConnections != null)
            {
                foreach (var connectionsPerType in _databaseConnections)
                {
                    foreach (var connectionMade in connectionsPerType.Value)
                    {
                        try
                        {
                            if (connectionMade.State != ConnectionState.Closed)
                            {
                                connectionMade.Close();
                            }
                        }
                        catch (Exception) { }
                    }
                }
                _databaseConnections.Clear();
            }
        }

        public DatabaseType GetSupportedDatabaseType()
        {
            return _databaseType;
        }

        #region "RegisterConnectionManager"
        public static void RegisterConnectionManager<T>()
        {
            RegisterConnectionManager(typeof(T));
        }

        public static void RegisterConnectionManager(Type t)
        {
            var connectionManager = Activator.CreateInstance(t) as GenericConnectionManager;
            if (_registeredConnectionManagers.ContainsKey(DatabaseType.SqlCE) == false)
            {
                var sqlCeConnectionManager = Activator.CreateInstance<sqlce.ConnectionManager>();

                _registeredConnectionManagers[DatabaseType.SqlCE] = sqlCeConnectionManager;
                sqlCeConnectionManager.InstallDefaultClasses();

                GenericDatabaseManager.RegisterDatabaseManager<sqlce.DatabaseManager>();

                if (connectionManager.GetSupportedDatabaseType() == DatabaseType.SqlCE)
                {
                    return;
                }
            }

            if (connectionManager != null)
            {
                if (_registeredConnectionManagers.ContainsKey(connectionManager.GetSupportedDatabaseType()))
                {
                    try
                    {
                        _registeredConnectionManagers[connectionManager.GetSupportedDatabaseType()].CloseConnections();
                    }
                    catch { }
                }

                _registeredConnectionManagers[connectionManager.GetSupportedDatabaseType()] = connectionManager;
                connectionManager.InstallDefaultClasses();
            }
        }
        #endregion

        public static GenericConnectionManager GetConnectionManager(DatabaseType databaseType)
        {
            if (_registeredConnectionManagers.ContainsKey(databaseType) == false)
            {
                if (databaseType == DatabaseType.SqlCE)
                {
                    GenericConnectionManager.RegisterConnectionManager<libDatabaseHelper.classes.sqlce.ConnectionManager>();
                    GenericDatabaseManager.RegisterDatabaseManager<libDatabaseHelper.classes.sqlce.DatabaseManager>(true);

                    return _registeredConnectionManagers[databaseType];
                }
                throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoConnectionManagerFound);
            }
            return _registeredConnectionManagers[databaseType];
        }

        public static void SetLocalDataDirectory(string location)
        { 
            if (Directory.Exists(location) == false)
            {
                throw new DirectoryNotFoundException("The directory \"" + location + "\" specified as the local data directory does not exist.");
            }

            if (location.EndsWith("\\") == false)
                location += "\\";
            _localDataFolder = location;
        }

        public static void CloseAllConnections()
        {
            if (_registeredConnectionManagers != null)
            {
                foreach (var connectionManagerRecord in _registeredConnectionManagers)
                {
                    try
                    {
                        connectionManagerRecord.Value.CloseConnections();
                    }
                    catch (System.Exception) { }
                }
                _registeredConnectionManagers.Clear();
            }
        }
    }
}
