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

namespace libDatabaseHelper.classes.generic
{
    public class ConnectionManager
    {
        private static string _connectionString;
        private static string _localDataFolder;
        private static List<SqlCeConnection> _listOfConnections;
        private static List<SqlCeConnection> _listOfLocalConnections;

        public static SqlCeConnection GetConnection()
        {
            if (_connectionString == null)
            {
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    MessageBox.Show("The application was not configured with a connection string. Can you please enter the connection string in the next dialog.", "Connection String Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return (frmConnectionStringSetter.ShowWindow() ? GetConnection() : null);
                }
                else
                {
                    return null;
                }
            }

            if (_listOfConnections == null)
            {
                _listOfConnections = new List<SqlCeConnection>();
            }
            else
            {
                foreach (var connection in _listOfConnections)
                {
                    try
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            return connection;
                        }
                    }
                    catch { }
                }
            }

            var engine = new SqlCeEngine(_connectionString);
            if (engine.Verify() == false)
            {
                engine.CreateDatabase();
            }

            var connectionCreated = new SqlCeConnection(_connectionString);
            try
            {
                connectionCreated.Open();
                _listOfConnections.Add(connectionCreated);
            }
            catch
            {
                return (frmConnectionStringSetter.ShowWindow() ? GetConnection() : null);
            }
            return connectionCreated;
        }

        public static void SetLocalDataFolder(string localDataFolder)
        {
            _localDataFolder = localDataFolder;
            if (Directory.Exists(_localDataFolder) == false)
            {
                GenericUtils.CreateFolderStructure(_localDataFolder);
            }
        }

        public static bool GetLocalSettings()
        {
            return GetRemoteConnectionString() != null;
        }

        public static string GetRemoteConnectionString()
        {
            using (var connection = GetLocalConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT ConnectionString FROM ConnectionStrings";

                _connectionString = (string) command.ExecuteScalar();

                var remoteConnection = GetConnection();
                if (remoteConnection != null)
                {
                    connection.Close();
                }
            }
            SetConnectionString(_connectionString);
            return _connectionString;
        }

        public static SqlCeConnection GetLocalConnection()
        {
            string dbFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (Directory.Exists(dbFilePath))
            {
                dbFilePath += (dbFilePath.EndsWith("\\") ? "" : "\\") + "VSTExtendData";
                if (! Directory.Exists(dbFilePath))
                    Directory.CreateDirectory(dbFilePath);
                dbFilePath += "\\dblocaldata.sdf";
            }
            else
            {
                dbFilePath = "dblocaldata.sdf";
            }

            const string password = "KsvKgHk%9=ANb2g@w7Bu6m?txU$h3V";
            string connectionString = "Data Source=\"" + dbFilePath + "\";Encrypt Database=True;Password=\"" + password + "\";File Mode=shared read;Persist Security Info=False;";
            if (_listOfLocalConnections == null)
            {
                _listOfLocalConnections = new List<SqlCeConnection>();
            }
            else
            {
                foreach (var madeConnection in _listOfLocalConnections)
                {
                    try
                    {
                        if (madeConnection.State == ConnectionState.Open)
                        {
                            return madeConnection;
                        }
                    }
                    catch { }
                }
            }

            if (File.Exists(dbFilePath) == false)
            {
                SqlCeEngine engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
            }

            var connection = new SqlCeConnection(connectionString);
            try
            {
                connection.Open();
                _listOfLocalConnections.Add(connection);
            }
            catch
            {
                return null;
            }
            return connection;
        }

        public static bool CheckConnectionString(string connectionString)
        {
            try
            {
                var connection = new SqlCeConnection(connectionString);
                connection.Open();
                connection.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            try
            {
                try
                {
                    GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<User>();
                    GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).CreateTable<AuditEntry>();
                }
                catch {}

                using (var connection = GetLocalConnection())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM ConnectionStrings";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO ConnectionStrings (ConnectionString) VALUES (@ConnectionString)";
                    command.Parameters.AddWithValue("@ConnectionString", connectionString);
                    command.ExecuteNonQuery();
                }
            }
            catch {}
        }

        public static void CloseAllConnections()
        {
            foreach (var connection in _listOfConnections)
            {
                try
                {
                    connection.Close();
                }
                catch {}
            }

            foreach (var connection in _listOfLocalConnections)
            {
                try
                {
                    connection.Close();
                }
                catch { }
            }
        }
    }
}
