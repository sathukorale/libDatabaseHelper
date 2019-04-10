using System;
using System.Linq;

using libDatabaseHelper.classes.generic;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace libDatabaseHelper.classes.mysql
{
    public class ConnectionManager : GenericConnectionManager
    {
        public ConnectionManager() : base(DatabaseType.MySQL)
        {
        }

        public override bool CheckConnectionString(string connectionString)
        {
            MySqlConnection connection = null;
            Exception exceptionThrown = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
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
                            connection = new MySqlConnection(madeConnectionString);
                            connection.Open();

                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "CREATE DATABASE IF NOT EXISTS " + builder.Database;
                                command.ExecuteNonQuery();
                            }

                            connection.Close();

                            connection = new MySqlConnection(connectionString);
                            connection.Open();
                        }
                        catch (MySqlException)
                        {
                            exceptionThrown = new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoDatabaseFound);
                        }
                        catch (Exception exInner)
                        {
                            exceptionThrown = exInner;
                        }
                    }
                    else
                    {
                        exceptionThrown = new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.NoDatabaseSpecified);
                    }
                }
                else
                {
                    exceptionThrown = new DatabaseConnectionException(DatabaseConnectionException.ConnectionErrorType.InvalidConnectionString);
                }
            }
            catch (Exception exOuter) 
            {
                Console.WriteLine(@"Unable to create MySQL database automatically. The database should be created manually. Error Details : " + exOuter.Message);
                exceptionThrown = exOuter;
            }

            if (connection != null && connection.State != System.Data.ConnectionState.Closed)
            {
                try
                {
                    connection.Close();
                }
                catch (Exception) { /* IGNORED */ }
            }

            return exceptionThrown == null;
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
