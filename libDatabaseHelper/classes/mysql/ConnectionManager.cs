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
using MySql.Data.MySqlClient;

namespace libDatabaseHelper.classes.mysql
{
    public class ConnectionManager : GenericConnectionManager
    {
        public ConnectionManager() : base(DatabaseType.MySQL)
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
                var connection = new MySqlConnection(connectionString);
                connection.Open();
                connection.Close();

                return true;
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 0)
                {
                    var builder = new MySqlConnectionStringBuilder(connectionString);

                    var madeConnectionString = connectionString.ToLower();
                    var index = madeConnectionString.IndexOf("database");
                    if (index >= 0)
                    {
                        madeConnectionString = connectionString.Substring(0, index) + connectionString.Substring(connectionString.IndexOf(";", index) + 1);
                        try
                        {
                            var connection = new MySqlConnection(madeConnectionString);
                            connection.Open();
                            var command = connection.CreateCommand();

                            command.CommandText = "CREATE DATABASE IF NOT EXISTS " + builder.Database;
                            command.ExecuteNonQuery();

                            connection.Close();

                            connection = new MySqlConnection(connectionString);
                            connection.Open();
                            connection.Close();

                            return true;
                        }
                        catch (MySqlException)
                        {
                            throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoDatabaseFound);
                        }
                    }
                    else
                    {
                        throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoDatabaseSpecified);
                    }
                }
                throw new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.InvalidConnectionString);
            }
            catch (Exception ex) { Console.WriteLine("Unable to create SQL CE database automatically. The database should be created manually. Error Details : " + ex.Message); }
            return false;
        }

        protected override DbConnection CreateConnection(Type t, string connectionString)
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
            }
            catch (Exception ex) { Console.WriteLine("Unexpected Error Occurred ! Error Details : " + ex.Message); }
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
    }
}
