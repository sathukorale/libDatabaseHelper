using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using libDatabaseHelper.classes.generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using libDatabaseHelper.forms;
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
                var builder = new SqlConnectionStringBuilder(connectionString);

                if (File.Exists(builder.DataSource) == false)
                {
                    var engine = new SqlCeEngine(connectionString);
                    engine.CreateDatabase();
                }
            }
            catch {}

            try
            {
                var connection = new SqlCeConnection(connectionString);
                connection.Open();
                connection.Close();
            }
            catch (System.Data.SqlServerCe.SqlCeInvalidDatabaseFormatException)
            {
                try
                {
                    var engine = new SqlCeEngine(connectionString);
                    engine.Upgrade();
                }
                catch (System.Exception ex) 
                { 
                    Console.WriteLine("Attempt on Upgrading SQL CE Database Failed (Reason = \"" + ex.Message + "\")");
                    return false;
                }

                try
                {
                    var connection = new SqlCeConnection(connectionString);
                    connection.Open();
                    connection.Close();
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected override DbConnection CreateConnection(Type t, string connectionString)
        {
            try
            {
                var connectionCreated = new SqlCeConnection(connectionString);
                connectionCreated.Open();
                return connectionCreated;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to connect with conenction string = \"" + connectionString + "\" due to : " + ex.Message);
                return null;/*(frmConnectionStringSetter.ShowWindow(t, GetSupportedDatabaseType()) ? GetConnection() : null)*/ ;
            }
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
            var localConnectionString = "Data Source='" + dbFilePath + "';Encrypt Database=True;Password='" + password + "';File Mode=shared read;Persist Security Info=False;";

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
