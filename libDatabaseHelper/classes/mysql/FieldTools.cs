using System;
using System.Linq;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.classes.mysql
{
    public class FieldTools
    {
        public static string GetDbTypeString(Type type)
        {
            return GetDbTypeString(type, false, 0);
        }

        public static string GetDbTypeString(Type type, bool uniqueOrPrimary, int length)
        {
            if (type == GenericFieldTools.TypeChar)
                return "TINYINT";
            if (type == GenericFieldTools.TypeByte)
                return "TINYINT UNSIGNED";
            if (type == GenericFieldTools.TypeShort)
                return "SMALLINT";
            if (type == GenericFieldTools.TypeUint16)
                return "SMALLINT UNSIGNED";
            if (type == GenericFieldTools.TypeInt)
                return "INT";
            if (type == GenericFieldTools.TypeUint32)
                return "INT UNSIGNED";
            if (type == GenericFieldTools.TypeLong)
                return "BIGINT";
            if (type == GenericFieldTools.TypeUint32)
                return "BIGINT UNSIGNED";

            if (type == GenericFieldTools.TypeFloat)
                return "FLOAT";
            if (type == GenericFieldTools.TypeDouble || type == GenericFieldTools.TypeSDouble)
                return "DOUBLE";

            if (type == GenericFieldTools.TypeString || type == GenericFieldTools.TypeSString)
                return uniqueOrPrimary ? ("VARCHAR (" + length + ")") : "TEXT";

            if (type == typeof(Date))
                return "DATE";

            if (type == GenericFieldTools.TypeBool || type == GenericFieldTools.TypeBoolean)
                return "BOOL";

            if (type.GetInterfaces().Contains(typeof(ICustomType)))
            {
                var obj = Activator.CreateInstance(type) as ICustomType;
                if (obj != null) return obj.GetDbType();
            }

            return type == GenericFieldTools.TypeDateTime ? "DATETIME" : "BLOB";
        }
    }
}
