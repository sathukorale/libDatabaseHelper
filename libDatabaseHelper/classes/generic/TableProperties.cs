using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libDatabaseHelper.classes.generic
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false)]
    public class TableProperties : System.Attribute
    {
        public enum RegistrationType
        { 
            ManualRegistration,
            RegisterOnDatabaseManager,
            RegisterOnUniversalDataCollector
        };

        public RegistrationType Registration { get; set; }
    }
}
