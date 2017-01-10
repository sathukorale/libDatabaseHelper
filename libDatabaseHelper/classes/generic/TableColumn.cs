﻿using System;

namespace libDatabaseHelper.classes.generic
{
    public class TableColumn : Attribute
    {
        public delegate object GenerateAutogen(string field, string table);

        public TableColumn()
        {
            IsPrimaryKey = false;
            ShouldIncludeInTable = true;
            IsAutogenerated = false;
            AutogenrationMethod = null;
            Length = 0;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public TableColumn(bool primaryKey, bool isInTable)
        {
            IsPrimaryKey = primaryKey;
            ShouldIncludeInTable = isInTable;
            IsAutogenerated = false;
            AutogenrationMethod = null;
            Length = 0;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public TableColumn(bool isInTable)
        {
            IsPrimaryKey = false;
            ShouldIncludeInTable = isInTable;
            IsAutogenerated = false;
            AutogenrationMethod = null;
            Length = 0;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public TableColumn(bool primaryKey, bool isInTable, GenerateAutogen autogenMethod)
        {
            IsPrimaryKey = primaryKey;
            ShouldIncludeInTable = isInTable;
            AutogenrationMethod = autogenMethod;
            IsAutogenerated = false;
            Length = 0;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public TableColumn(bool isInTable, GenerateAutogen autogenMethod)
        {
            IsPrimaryKey = false;
            ShouldIncludeInTable = isInTable;
            AutogenrationMethod = autogenMethod;
            IsAutogenerated = false;
            Length = 0;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public TableColumn(bool primaryKey, bool isInTable, int autogenLength)
        {
            IsPrimaryKey = primaryKey;
            ShouldIncludeInTable = isInTable;
            Length = autogenLength;
            IsAutogenerated = false;
            AutogenrationMethod = null;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public TableColumn(bool isInTable, int autogenLength)
        {
            IsPrimaryKey = false;
            ShouldIncludeInTable = isInTable;
            Length = autogenLength;
            IsAutogenerated = false;
            AutogenrationMethod = null;
            IsAuditVisible = true;
            IsRetrievableFromDatabase = true;
            IsGridViewable = true;
            IsEditableOnFormWhenLoaded = true;
            IsUnique = false;
            Translators = null;
        }

        public bool IsPrimaryKey { get; set; }
        public bool ShouldIncludeInTable { get; set; }
        public bool IsAutogenerated { get; set; }
        public bool IsUnique { get; set; }
        public int Length { get; set; }
        public string GridDisplayName { get; set; }
        public bool IsGridViewable { get; set; }
        public GenerateAutogen AutogenrationMethod { get; set; }

        public Type ReferencedClass { get; set; }
        public string ReferencedField { get; set; }
        public bool IsAuditVisible { get; set; }
        public bool IsRetrievableFromDatabase { get; set; }
        public bool IsEditableOnFormWhenLoaded { get; set; }
        public string[] Translators { get; set; }

        public object DefaultValue { get; set; }
    }
}
