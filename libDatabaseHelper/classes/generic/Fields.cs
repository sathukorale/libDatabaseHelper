using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.Reflection;

using libDatabaseHelper.classes.sqlce;
using System.Data.Common;

namespace libDatabaseHelper.classes.generic
{
    public class TableColumnField
    {
        public TableColumnField(bool required, Type type, string fieldName)
        {
            Required = required;
            FieldName = fieldName;
            ObjType = type;
            DefaultValue = null;
        }
        public TableColumnField(bool required, Type type, string fieldName, object defaultValue)
        {
            Required = required;
            FieldName = fieldName;
            ObjType = type;
            DefaultValue = defaultValue;
        }

        public bool Required { get; set; }
        public string FieldName { get; set; }
        public Type ObjType { get; set; }
        public object DefaultValue { get; set; }
    }

    public interface ICustomType
    {
        string GetDbType();
        object GetValue();
        ICustomType SetValue(object value);
    }

    public class Date : ICustomType
    {
        private DateTime _value;

        public Date()
        {
            _value = DateTime.Now;
        }

        public Date(DateTime value)
        {
            _value = value;
        }

        public string GetDbType()
        {
            return "DATE";
        }

        public object GetValue()
        {
            return _value;
        }

        public ICustomType SetValue(object value)
        {
            _value = (DateTime)value;
            return this;
        }

        public static implicit operator Date(DateTime value)
        {
            return new Date(value);
        }

        public override string ToString()
        {
            return _value.ToShortDateString();
        }
    }

    public class RadioButtonField : TableColumnField
    {
        public RadioButtonField(bool required, string fieldName, Type type, string group, object value)
            : base(required, type, fieldName)
        {
            Group = group;
            Value = value;
        }

        public string Group { get; set; }
        public object Value { get; set; }
    }

    public interface IComboBoxItem
    {
        object GetID();
        string GetSelectQueryItems();
    }

    public class Selector
    {
        public enum Operator
        {
            Like,
            Between,
            Equal,
            LessThan,
            MoreThan
        }

        public string Field;
        public object FieldValue1;
        public object FieldValue2;
        public Operator OpeartorType;

        public Selector(string field, object fieldValue)
        {
            Field = field;
            FieldValue1 = fieldValue;
            FieldValue2 = null;
            OpeartorType = Operator.Equal;
        }

        public Selector(string field, object fieldValue1, Operator operatorType)
        {
            Field = field;
            FieldValue1 = fieldValue1;
            FieldValue2 = null;
            OpeartorType = operatorType;
        }

        public Selector(string field, object fieldValue1, object fieldValue2, Operator operatorType)
        {
            Field = field;
            FieldValue1 = fieldValue1;
            FieldValue2 = fieldValue2;
            OpeartorType = operatorType;
        }

        public string SetToCommand(ref DbCommand command)
        {
            var selectorString = "";
            if (OpeartorType == Operator.Between)
            {
                selectorString = String.Format("{0} > @{0}1 AND {0} < @{0}2", Field);

                if (!command.Parameters.Contains("@" + Field + "1"))
                {
                    var value = FieldValue1 is ICustomType ? (FieldValue1 as ICustomType).GetValue() : FieldValue1;
                    GenericUtils.AddWithValue(ref command, "@" + Field + "1", value, GenericFieldTools.GetType(FieldValue1.GetType()));
                }
                if (!command.Parameters.Contains("@" + Field + "2"))
                {
                    var value = FieldValue2 is ICustomType ? (FieldValue2 as ICustomType).GetValue() : FieldValue2;
                    GenericUtils.AddWithValue(ref command, "@" + Field + "2", value, GenericFieldTools.GetType(FieldValue2.GetType()));
                }
            }
            else
            {
                selectorString = String.Format("{0} {1} @{0}", Field,
                    (OpeartorType == Operator.Equal
                        ? "="
                        : (OpeartorType == Operator.LessThan ? "<" : (OpeartorType == Operator.MoreThan ? ">" : "LIKE"))));

                if (!command.Parameters.Contains("@" + Field))
                {
                    var value = FieldValue1 is ICustomType ? (FieldValue1 as ICustomType).GetValue() : FieldValue1;
                    GenericUtils.AddWithValue(ref command, "@" + Field, value, GenericFieldTools.GetType(FieldValue1.GetType()));
                }
            }
            return selectorString;
        }
    }

    public class Reference
    {
        public readonly Type ReferencedByClass;
        public readonly FieldInfo ReferencedByField;
        public readonly FieldInfo ReferredField;

        public Reference(Type referencedByClass, FieldInfo referencedByField, FieldInfo fieldReferred)
        {
            ReferencedByClass = referencedByClass;
            ReferencedByField = referencedByField;
            ReferredField = fieldReferred;
        }
    }

    public class ColumnResults
    {
        public List<FieldInfo> Ids;
        public List<FieldInfo> AllFieldInfos;
        public List<FieldInfo> FieldInfos;

        public ColumnResults(List<FieldInfo> ids, List<FieldInfo> allFields, List<FieldInfo> filteredFields)
        {
            Ids = ids;
            AllFieldInfos = allFields;
            FieldInfos = filteredFields;
        }
    }
}
