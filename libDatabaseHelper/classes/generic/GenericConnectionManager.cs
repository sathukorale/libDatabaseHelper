using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows.Forms;
using System.Linq;

using libDatabaseHelper.forms;
using libDatabaseHelper.Properties;
using libDatabaseHelper.classes.sqlce;
using libDatabaseHelper.classes.sqlce.entities;
using System.Data.SqlClient;
using System.Data.Common;

namespace libDatabaseHelper.classes.generic
{
    [TableProperties(Registration=libDatabaseHelper.classes.generic.TableProperties.RegistrationType.RegisterOnDatabaseManager)]
    public class GenericConnectionDetails : DatabaseEntity
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
            catch (DatabaseConnectionException)
            {
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    return null;
                    //MessageBox.Show("The application was not configured with a connection string. Can you please enter the connection string in the next dialog.", "Connection String Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    /*return (frmConnectionStringSetter.ShowWindow(t, _databaseType) ? GetConnection() : null);*/ 
                }
                else
                {
                    return null;
                }
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

            var connectionCreated = CreateConnection(t, connectionString);
            try
            {
                if (connectionCreated.State == ConnectionState.Closed)
                {
                    connectionCreated.Open();
                }
            }
            catch (System.Exception ex) { Console.WriteLine("Unable to Open Database Connection. Error Details : " + ex.Message); }

            if (connectionCreated.State == ConnectionState.Open && _databaseConnections[connectionString].Contains(connectionCreated) == false)
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
                    throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoConnectionStringFound);
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
                catch (System.Exception) { }
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
            throw new NotImplementedException("");
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
                            connectionMade.Close();
                        }
                        catch (System.Exception) { }
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
                    RegisterConnectionManager<ConnectionManager>();
                    return GetConnectionManager(databaseType);
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
