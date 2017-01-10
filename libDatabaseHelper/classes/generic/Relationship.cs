using System;
using System.Collections.Generic;
using System.Linq;

namespace libDatabaseHelper.classes.generic
{
    public struct Relationship
    {
        private readonly List<Type> _referencingClasses;
        private readonly Type _referencedClass;

        private static Dictionary<Type, Relationship> _relationships = new Dictionary<Type, Relationship>();

        private Relationship(Type referencedClass)
        {
            _referencedClass = referencedClass;
            _referencingClasses = new List<Type>();
        }

        private void AddReference(Type referencingClass)
        {
            if (referencingClass != null & !_referencingClasses.Contains(referencingClass))
            {
                _referencingClasses.Add(referencingClass);
            }
        }

        public static void Add(Type referencedType, Type referencingType)
        {
            if (referencedType == null || referencingType == null)
                return;

            if (!_relationships.ContainsKey(referencedType))
            {
                var relationship = new Relationship(referencedType);
                relationship.AddReference(referencingType);

                _relationships.Add(referencedType, relationship);
            }
            else
            {
                _relationships[referencedType].AddReference(referencingType);
            }
        }

        public static bool CheckReferences(GenericDatabaseEntity entity)
        {
            if (entity == null) return false;

            var referencedType = entity.GetType();
            if (!_relationships.ContainsKey(referencedType)) return false;

            var references = _relationships[referencedType]._referencingClasses;

            foreach (var reference in references.Where(reference => reference != null))
            {
                var fieldReferencedBy = reference.GetFields().Where(i => i.GetCustomAttributes(typeof(TableColumn), true).Any()).ToList();
                var filters = new List<Selector>();

                foreach (var fieldInfo in fieldReferencedBy)
                {
                    var attr = (TableColumn)fieldInfo.GetCustomAttributes(typeof(TableColumn), true)[0];
                    if (attr != null && attr.ReferencedClass == referencedType)
                    {
                        filters.Add(new Selector(fieldInfo.Name, referencedType.GetField(attr.ReferencedField).GetValue(entity)));
                    }
                }

                if (filters.Count <= 0)
                    continue;

                var command = GenericConnectionManager.GetConnectionManager(entity.GetSupportedDatabaseType()).GetConnection(entity.GetType()).CreateCommand();
                var selectQuery = "SELECT COUNT(*) FROM " + reference.Name + " WHERE " + filters.Aggregate("", (currentStr, item) => currentStr + (currentStr == "" ? "" : " OR ") + item.SetToCommand(ref command));
                command.CommandText = selectQuery;

                int result;
                if (Int32.TryParse(command.ExecuteScalar().ToString(), out result) && result > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
