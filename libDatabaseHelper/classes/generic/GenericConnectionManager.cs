using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows.Forms;

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
        [TableColumn(true, true)]
        public int DatabaseType;

        [TableColumn(true, true, 100)]
        public string TypeName;

        [TableColumn(true, true, 200)]
        public string TypeAssembly;

        [TableColumn(false, true)]
        public string ConnectionString;

        public Type GetReferencedType()
        {
            return null;
        }
    }

    class NullType {}

    public class GenericConnectionManager
    {
        private static string _localDataFolder;
        private Dictionary<Type, string>                                    _connectionStrings;
        private Dictionary<Type, List<DbConnection>>                        _databaseConnections;
        private static Dictionary<DatabaseType, GenericConnectionManager>   _registeredConnectionManagers = new Dictionary<DatabaseType, GenericConnectionManager>();
        private DatabaseType _databaseType;

        public GenericConnectionManager(DatabaseType supportedDatabase)
        {
            _databaseType = supportedDatabase;
            _connectionStrings = new Dictionary<Type, string>();
            _databaseConnections = new Dictionary<Type, List<DbConnection>>();
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
                    MessageBox.Show("The application was not configured with a connection string. Can you please enter the connection string in the next dialog.", "Connection String Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return (frmConnectionStringSetter.ShowWindow(t, _databaseType) ? GetConnection() : null);
                }
                else
                {
                    return null;
                }
            }

            if (_databaseConnections == null)
            {
                _databaseConnections = new Dictionary<Type, List<DbConnection>>();
            }
            else
            {
                if (_databaseConnections.ContainsKey(t))
                {
                    var connections = _databaseConnections[t];
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
                    _databaseConnections[t] = new List<DbConnection>();
                }
            }

            var connectionCreated = CreateConnection(t, connectionString);
            try
            {
                connectionCreated.Open();
            }
            catch (System.Exception) {}

            if (connectionCreated.State == ConnectionState.Open && _databaseConnections[t].Contains(connectionCreated) == false)
            {
                _databaseConnections[t].Add(connectionCreated);
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

        public string GetConnectionString(Type type)
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
                    File.WriteAllBytes(dbFilePath, Properties.Resources.dblocaldata);

                    if (CheckConnectionString(connectionString) == false)
                    {
                        return;
                    }
                }

                try
                {
                    _connectionStrings[type] = connectionString;
                    var connectionData = new GenericConnectionDetails() { 
                        DatabaseType = (int)_databaseType,
                        TypeName = type.Name,
                        TypeAssembly = type.Assembly.FullName,
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
                if (connectionManager.GetSupportedDatabaseType() == DatabaseType.SqlCE)
                {
                    var dbFilePath = (_localDataFolder ?? "") + "dblocaldata.sdf";
                    var password = "KsvKgHk%9=ANb2g@w7Bu6m?txU$h3V";
                    var connectionString = "Data Source='" + dbFilePath + "';Encrypt Database=True;Password='" + password + "';File Mode=shared read;Persist Security Info=False;";

                    connectionManager.SetConnectionString<GenericConnectionDetails>(connectionString);
                }
                _registeredConnectionManagers[connectionManager.GetSupportedDatabaseType()] = connectionManager;
            }
        }
        #endregion

        public static GenericConnectionManager GetConnectionManager(DatabaseType databaseType)
        {
            if (_registeredConnectionManagers.ContainsKey(databaseType) == false)
            {
                if (databaseType == DatabaseType.SqlCE)
                {
                    RegisterConnectionManager<SqlCeConnectionManager>();
                    return GetConnectionManager(DatabaseType.SqlCE);
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
            }
        }
    }
}
