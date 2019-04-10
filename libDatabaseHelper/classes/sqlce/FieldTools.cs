using System;
using System.Linq;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.classes.sqlce
{
    public class FieldTools
    {
        public static string GetDbTypeString(Type type)
        {
            return GetDbTypeString(type, 0);
        }

        public static string GetDbTypeString(Type type, int length)
        {
            if (type == GenericFieldTools.TypeChar || type == GenericFieldTools.TypeByte)
                return "TINYINT";
            if (type == GenericFieldTools.TypeShort || type == GenericFieldTools.TypeUint16)
                return "SMALLINT";
            if (type == GenericFieldTools.TypeInt || type == GenericFieldTools.TypeUint32)
                return "INTEGER";
            if (type == GenericFieldTools.TypeLong || type == GenericFieldTools.TypeUint32)
                return "BIGINT";

            if (type == GenericFieldTools.TypeFloat)
                return "FLOAT";
            if (type == GenericFieldTools.TypeDouble || type == GenericFieldTools.TypeSDouble)
                return "REAL";

            if (type == GenericFieldTools.TypeString || type == GenericFieldTools.TypeSString)
                return "NVARCHAR(4000)";

            if (type == typeof(Date))
                return "DATETIME";

            if (type == GenericFieldTools.TypeBool || type == GenericFieldTools.TypeBoolean)
                return "BIT";

            if (type.GetInterfaces().Contains(typeof(ICustomType)))
            {
                var obj = Activator.CreateInstance(type) as ICustomType;
                if (obj != null) return obj.GetDbType();
            }

            return type == GenericFieldTools.TypeDateTime ? "DATETIME" : "IMAGE";
        }
    }
}
