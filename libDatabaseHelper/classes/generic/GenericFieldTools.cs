using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace libDatabaseHelper.classes.generic
{
    public class GenericFieldTools
    {
        public static readonly Type TypeChar = typeof(char);
        public static readonly Type TypeShort = typeof(short);
        public static readonly Type TypeInt = typeof(int);
        public static readonly Type TypeLong = typeof(long);
        public static readonly Type TypeByte = typeof(byte);
        public static readonly Type TypeUint16 = typeof(UInt16);
        public static readonly Type TypeUint32 = typeof(UInt32);
        public static readonly Type TypeUInt64 = typeof(UInt64);
        public static readonly Type TypeFloat = typeof(float);
        public static readonly Type TypeDouble = typeof(double);
        public static readonly Type TypeSDouble = typeof(Double);
        public static readonly Type TypeString = typeof(string);
        public static readonly Type TypeSString = typeof(String);
        public static readonly Type TypeDateTime = typeof(DateTime);
        public static readonly Type TypeBool = typeof(bool);
        public static readonly Type TypeBoolean = typeof(bool);
        public static readonly Type TypeObject = typeof(byte[]);
        public static readonly Type TypeSObject = typeof(Byte[]);

        public static DbType GetType(Type type)
        {
            if (type == TypeChar)
                return DbType.SByte;
            if (type == TypeShort)
                return DbType.Int16;
            if (type == TypeInt)
                return DbType.Int32;
            if (type == TypeLong)
                return DbType.Int64;

            if (type == TypeByte)
                return DbType.Byte;
            if (type == TypeUint16)
                return DbType.UInt16;
            if (type == TypeUint32)
                return DbType.UInt32;
            if (type == TypeUInt64)
                return DbType.UInt64;

            if (type == TypeFloat)
                return DbType.Decimal;
            if (type == TypeDouble || type == TypeSDouble)
                return DbType.Double;

            if (type == TypeString || type == TypeSString)
                return DbType.String;

            if (type == TypeBool || type == TypeBoolean)
                return DbType.Byte;

            if (type == typeof(Date))
                return DbType.Date;

            if (type == TypeObject || type == TypeSObject)
                return DbType.Object;

            return type == TypeDateTime ? DbType.DateTime : DbType.Binary;
        }

        public static bool IsTypeNumber(Type type)
        {
            return type == TypeChar || type == TypeByte || type == TypeShort || type == TypeUint16 ||
                   type == TypeInt || type == TypeUint32 || type == TypeLong || type == TypeUInt64;
        }

        public static bool IsTypeFloatingPoint(Type type)
        {
            return (type == TypeFloat || type == TypeDouble || type == TypeSDouble);
        }

        public static bool IsDateType(Type type)
        {
            return (type == TypeDateTime || type == typeof(Date));
        }

        public static bool IsTypeString(Type type)
        {
            return (type == TypeString || type == TypeSString);
        }

        public static int Compare(object obj1, object obj2)
        {
            if (IsDateType(obj1.GetType()))
            {
                var datetime1 = (DateTime)obj1;
                var datetime2 = (DateTime)obj2;

                return (datetime1 == datetime2 ? 0 : (datetime1 < datetime2 ? -1 : 1));
            }
            else if (IsTypeNumber(obj1.GetType()))
            {
                var number1 = (long)obj1;
                var number2 = (long)obj1;
                return number1 == number2 ? 0 : ((number1 < number2 ? -1 : 1));
            }
            else if (IsTypeFloatingPoint(obj1.GetType()))
            {
                var number1 = (double)obj1;
                var number2 = (double)obj1;
                var diff = number1 - number2;
                return Math.Abs(diff) < 0.00001 ? 0 : (number1 < number2 ? -1 : 1);
            }
            return Int32.MinValue;
        }

        public static object ConvertToType(Type type, object value)
        {
            if (value is DBNull)
            {
                if (IsTypeString(type))
                    return null;
                return 0;
            }

            if (type == TypeChar)
                return Convert.ToChar(value);
            if (type == TypeShort)
                return Convert.ToInt16(value);
            if (type == TypeInt)
                return Convert.ToInt32(value);
            if (type == TypeLong)
                return Convert.ToInt64(value);

            if (type == TypeByte)
                return Convert.ToByte(value);
            if (type == TypeUint16)
                return Convert.ToUInt16(value);
            if (type == TypeUint32)
                return Convert.ToUInt32(value);
            if (type == TypeUInt64)
                return Convert.ToUInt64(value);

            if (type == TypeFloat)
                return Convert.ToDecimal(value);
            if (type == TypeDouble || type == TypeSDouble)
                return Convert.ToDouble(value);

            if (type == TypeString || type == TypeSString)
                return Convert.ToString(value);

            if (type == TypeBool || type == TypeBoolean)
                return Convert.ToBoolean(value);

            if (type == TypeDateTime)
                return Convert.ToDateTime(value);

            if (type == typeof(Date))
                return new Date(Convert.ToDateTime(value));

            if (type == TypeObject || type == TypeSObject)
            {
                return value as byte[];
            }

            return "";
        }
    }
}
