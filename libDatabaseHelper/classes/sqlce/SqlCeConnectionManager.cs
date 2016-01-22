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

namespace libDatabaseHelper.classes.sqlce
{
    public class SqlCeConnectionManager : GenericConnectionManager
    {
        public SqlCeConnectionManager() : base(DatabaseType.SqlCE)
        { 
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
                var builder = new SqlConnectionStringBuilder(connectionString);

                if (File.Exists(builder.DataSource) == false)
                {
                    var engine = new SqlCeEngine(connectionString);
                    engine.CreateDatabase();
                }
            }
            catch { }

            try
            {
                var connectionCreated = new SqlCeConnection(connectionString);
                connectionCreated.Open();
                return connectionCreated;
            }
            catch
            {
                return (frmConnectionStringSetter.ShowWindow(t, GetSupportedDatabaseType()) ? GetConnection() : null);
            }
        }
    }
}
