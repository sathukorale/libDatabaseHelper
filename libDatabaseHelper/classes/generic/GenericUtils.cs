using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Data.Common;
using System.Data;

namespace libDatabaseHelper.classes.generic
{
    public class GenericUtils
    {
        public static byte[] ObjectToByteArray(object value)
        {
            if (value == null)
                return null;

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, value);
                return ms.ToArray();
            }
        }

        public static object ByteArrayToObject(byte[] array)
        {
            if (array == null || array.Length <= 0)
                return null;

            using (var ms = new MemoryStream(array))
            {
                BinaryFormatter bf = new BinaryFormatter();
                ms.Write(array, 0, array.Length);
                ms.Seek(0, SeekOrigin.Begin);

                object obj = bf.Deserialize(ms);
                return obj;
            }
        }

        public static string CreateFolderStructure(string file)
        {
            var segments = file.Split('\\').Where(i => i.Trim() != "");
            string current_folder = "";
            foreach (var segment in segments)
            {
                current_folder += segment + "\\";
                if (!Directory.Exists(current_folder))
                {
                    Directory.CreateDirectory(current_folder);
                }
            }
            return current_folder;
        }

        public static string GetDataSourceFromConnnectionString(string connectionString)
        {
            var loweredText = connectionString.ToLower();
            var startIndex = loweredText.IndexOf("data source");
            startIndex = loweredText.IndexOf("=", startIndex) + 1;
            var endIndex = loweredText.IndexOf(";", startIndex);
            if (endIndex < 0)
            {
                endIndex = loweredText.Length - 1;
            }

            var dataSourceLocation = connectionString.Substring(startIndex, endIndex - startIndex).Trim().Replace("\"", "").Replace("'", "");
            return dataSourceLocation;
        }

        public static void AddWithValue(ref DbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        public static void AddWithValue(ref DbCommand command, string parameterName, object parameterValue, DbType dbType)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = dbType;
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            command.Parameters.Add(parameter);
        }

        public static void CleanupEverything()
        {
            UniversalDataModel.CleanUp();
            GenericConnectionManager.CloseAllConnections();
            GenericDatabaseEntity.CleanupNonDisposableReferenceObjects();
            GenericDatabaseManager.UnregisterAllDatabaseManagers();
        }
    }
}
