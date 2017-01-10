using System;
using System.Linq;

using libDatabaseHelper.classes.generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using libDatabaseHelper.classes.sqlce.entities;

namespace libDatabaseHelper.classes.sqlce
{
    public class ConnectionManager : GenericConnectionManager
    {
        public ConnectionManager() : base(DatabaseType.SqlCE)
        {
        }

        public override string GetConnectionString(Type type)
        {
            if (_connectionStrings.ContainsKey(type) == false)
            {
                var entityType = type.Namespace + "." + type.Name;
                if (_loadedConnectionDetails.ContainsKey(entityType))
                {
                    _connectionStrings[type] = _loadedConnectionDetails[entityType].ConnectionString;
                }
                else
                {
                    if (_connectionStrings.ContainsKey(typeof(NullType)) == false)
                    {
                        var nullType = typeof(NullType);
                        entityType = nullType.Namespace + "." + nullType.Name;
                        if (_loadedConnectionDetails.ContainsKey(entityType))
                        {
                            _connectionStrings[nullType] = _loadedConnectionDetails[entityType].ConnectionString;
                        }
                        else
                        {
                            throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoConnectionStringFound);
                        }
                    }
                    return _connectionStrings[typeof(NullType)];
                }
            }
            return _connectionStrings[type];
        }

        public override bool CheckConnectionString(string connectionString)
        {
            try
            {
                var index = 0;
                var dataSource = connectionString.ToLower();
                dataSource = connectionString.Substring(dataSource.IndexOf("=", dataSource.IndexOf("data source")) + 1);
                dataSource = dataSource.Substring(0, ((index = dataSource.IndexOf(";")) != -1) ? index : dataSource.Length).Trim();

                if (dataSource.StartsWith("'") || dataSource.StartsWith("\""))
                {
                    dataSource = dataSource.Substring(1).Trim();
                }

                if (dataSource.EndsWith("'") || dataSource.EndsWith("\""))
                {
                    dataSource = dataSource.Substring(0, dataSource.Length - 1).Trim();
                }

                if (dataSource.EndsWith(".sdf") && File.Exists(dataSource) == false)
                {
                    var containingFolder = dataSource.Substring(0, dataSource.LastIndexOf("\\"));
                    if (Directory.Exists(containingFolder) == false)
                    {
                        GenericUtils.CreateFolderStructure(containingFolder);
                    }

                    var engine = new SqlCeEngine(connectionString);
                    engine.CreateDatabase();
                }
            }
            catch (Exception ex) { Console.WriteLine("Unable to create SQL CE database automatically. The database should be created manually. Error Details : " + ex.Message);  }

            var connectionCreated = CreateConnection(null, connectionString);
            if (connectionCreated != null && connectionCreated.State == System.Data.ConnectionState.Open)
            {
                connectionCreated.Close();
                return true;
            }
            return false;
        }

        protected override DbConnection CreateConnection(Type t, string connectionString)
        {
            SqlCeConnection connection = null;

            try
            {
                var index = 0;
                var dataSource = connectionString.ToLower();
                dataSource = connectionString.Substring(dataSource.IndexOf("=", dataSource.IndexOf("data source")) + 1);
                dataSource = dataSource.Substring(0, ((index = dataSource.IndexOf(";")) != -1) ? index : dataSource.Length).Trim();

                if (dataSource.StartsWith("'") || dataSource.StartsWith("\""))
                {
                    dataSource = dataSource.Substring(1).Trim();
                }

                if (dataSource.EndsWith("'") || dataSource.EndsWith("\""))
                {
                    dataSource = dataSource.Substring(0, dataSource.Length - 1).Trim();
                }

                if (dataSource.EndsWith(".sdf") &&  File.Exists(dataSource) == false)
                {
                    var containingFolder = dataSource.Substring(0, dataSource.LastIndexOf("\\"));
                    if (Directory.Exists(containingFolder) == false)
                    {
                        GenericUtils.CreateFolderStructure(containingFolder);
                    }

                    var engine = new SqlCeEngine(connectionString);
                    engine.CreateDatabase();
                }
            }
            catch (Exception ex) { Console.WriteLine("Exception caught while attempting to detect and create database. (Reason = \"" + ex.Message + "\")"); }

            try
            {
                connection = new SqlCeConnection(connectionString);
                connection.Open();
            }
            catch (System.Data.SqlServerCe.SqlCeInvalidDatabaseFormatException ex1)
            {
                Console.WriteLine(ex1.Message);
                try
                {
                    var engine = new SqlCeEngine(connectionString);
                    engine.Upgrade();

                    try
                    {
                        connection = new SqlCeConnection(connectionString);
                        connection.Open();
                    }
                    catch (System.Exception){}
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Attempt on Upgrading SQL CE Database Failed (Reason = \"" + ex.Message + "\")");
                }
            }
            catch (Exception ex) { Console.WriteLine("Unexpected Error Occurred ! Error Details (" + ex.GetType() + ") : " + ex.Message); }
            return connection;
        }

        public void LoadConnectionData()
        {
            var databaseManager = GenericDatabaseManager.GetDatabaseManager(GetSupportedDatabaseType());
            if (databaseManager != null)
            {
                try
                {
                    var connectionDetails = databaseManager.Select<GenericConnectionDetails>().Select(i => i as GenericConnectionDetails).ToList<GenericConnectionDetails>();
                    if (connectionDetails != null)
                    {
                        foreach (var connectionDetail in connectionDetails)
                        {
                            try
                            {
                                _loadedConnectionDetails[connectionDetail.TypeName] = connectionDetail;
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }

        protected override void InstallDefaultClasses()
        {
            var dbFilePath = (_localDataFolder ?? "") + "dblocaldata.sdf";
            var password = "KsvKgHk%9=ANb2g@w7Bu6m?txU$h3V";
            var localConnectionString = "Data Source='" + dbFilePath + "';Encrypt Database=True;Password='" + password + "';File Mode=Read Write;Persist Security Info=False;";

            if (File.Exists(dbFilePath) == false)
            {
                File.WriteAllBytes(dbFilePath, Properties.Resources.dblocaldata);
            }

            _connectionStrings[typeof(GenericConnectionDetails)] = localConnectionString;
            _connectionStrings[typeof(AuditEntry)] = localConnectionString;

            LoadConnectionData();
        }
    }
}
