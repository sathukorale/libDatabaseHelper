using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
    }
}
