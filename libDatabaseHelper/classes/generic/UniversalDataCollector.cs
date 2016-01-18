using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using libDatabaseHelper.classes.sqlce;

namespace libDatabaseHelper.classes.generic
{
    public class UniversalDataCollector
    {
        private static bool _is_initialized = false;
        private static Dictionary<Type, List<DatabaseEntity>> _entitiesPerType = new Dictionary<Type, List<DatabaseEntity>>();

        private static void Init()
        {
            if (_is_initialized)
            {
                return;
            }
            else
            {
                _is_initialized = true;
            }

            DatabaseEntity.OnDatabaseEntityUpdated += new DatabaseEntity.UpdateEvent(DatabaseEntity_OnDatabaseEntityUpdated);
            DatabaseManager.OnBulkDelete += new DatabaseManager.BulkDelete(DatabaseManager_OnBulkDelete);
        }

        #region "Selectors"
        public static List<DatabaseEntity> Select<T>()
        {
            return Select(typeof(T), null);
        }

        public static List<DatabaseEntity> Select<T>(Selector[] selectors)
        {
            return Select(typeof(T), selectors);
        }

        public static List<DatabaseEntity> Select(Type type)
        {
            return Select(type, null);
        }

        public static List<DatabaseEntity> Select(Type type, Selector[] selectors)
        {
            if (selectors == null)
            {
                return _entitiesPerType[type];
            }
            return FindMatchingEntities(type, selectors);
        }
        #endregion

        #region "Event Handlers"
        static void DatabaseManager_OnBulkDelete(Type type, Selector[] selectors)
        {
            if (!_entitiesPerType.ContainsKey(type))
                return;

            if (selectors == null || selectors.Length <= 0)
            {
                _entitiesPerType[type].Clear();
            }
            else
            {
                var list = _entitiesPerType[type];
                var entities_to_remove = FindMatchingEntities(type, selectors);
                
                foreach (var entity in entities_to_remove)
                {
                    list.Remove(entity);
                }
            }
        }

        static void DatabaseEntity_OnDatabaseEntityUpdated(DatabaseEntity updatedEntity, Type type, DatabaseEntity.UpdateEventType event_type)
        {
            if (!_entitiesPerType.ContainsKey(type))
                return;

            if (event_type == DatabaseEntity.UpdateEventType.Add)
            {
                _entitiesPerType[type].Add(updatedEntity);
            }
            else if (event_type == DatabaseEntity.UpdateEventType.Remove)
            {
                _entitiesPerType[type].Remove(updatedEntity);
            }
            else
            {
                var list = _entitiesPerType[type];
                for (int i = 0 ; i < list.Count ; i++)
                {
                    var entity = list[i];
                    if (entity.Equals(updatedEntity))
                    {
                        list[i] = updatedEntity;
                        break;
                    }
                }
            }
        }
        #endregion

        #region "FindMatchingEntities"
        private static List<DatabaseEntity> FindMatchingEntities<T>(Selector[] selector)
        {
            return FindMatchingEntities(typeof(T), selector);
        }

        private static List<DatabaseEntity> FindMatchingEntities(Type type, Selector[] selectors)
        {
            var list = _entitiesPerType[type];
            var collected = new List<DatabaseEntity>();
            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];

                bool is_entity_valid = true;
                foreach (var selector in selectors)
                {
                    if (selector.OpeartorType == Selector.Operator.Equal)
                    {
                        if (!entity.GetFieldValue(selector.Field).Equals(selector.FieldValue1))
                        {
                            is_entity_valid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.LessThan)
                    {
                        var present_val = entity.GetFieldValue(selector.Field);
                        var sent_val = selector.FieldValue1;
                        if (FieldTools.Compare(present_val, sent_val) != -1)
                        {
                            is_entity_valid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.MoreThan)
                    {
                        var present_val = entity.GetFieldValue(selector.Field);
                        var sent_val = selector.FieldValue1;
                        if (FieldTools.Compare(present_val, sent_val) != 1)
                        {
                            is_entity_valid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.Between)
                    {
                        var present_val = entity.GetFieldValue(selector.Field);
                        if (FieldTools.Compare(present_val, selector.FieldValue1) != 1 && FieldTools.Compare(present_val, selector.FieldValue2) != -1)
                        {
                            is_entity_valid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.Like)
                    {
                        var str_field_value = entity.GetFieldValue(selector.Field).ToString();
                        var str_sent_value = selector.FieldValue1.ToString();
                        if (str_sent_value.StartsWith("%"))
                        {
                            str_sent_value = str_sent_value.Substring(1);
                        }
                        if (str_sent_value.EndsWith("%"))
                        {
                            str_sent_value = str_sent_value.Substring(0, str_sent_value.Length - 1);
                        }
                        var segments = str_sent_value.Split('%');
                        int prev_index = -1;
                        bool should_break = false;
                        foreach (var segment in segments)
                        {
                            if (str_field_value.IndexOf(segment, prev_index == -1 ? 0 : prev_index) <= prev_index)
                            {
                                should_break = true;
                                break;
                            }
                        }

                        if (should_break)
                        {
                            is_entity_valid = false;
                        }
                    }
                }

                if (is_entity_valid)
                {
                    collected.Add(entity);
                }
            }
            return collected;
        }
        #endregion

        #region "Registration"
        public static void Register<T>()
        {
            if (!_is_initialized)
            {
                Init();
            }

            if (! _entitiesPerType.ContainsKey(typeof(T)))
            {
                var allCurrentEntities = new List<DatabaseEntity>();
                allCurrentEntities.AddRange(DatabaseManager.Select<T>(null));

                _entitiesPerType.Add(typeof(T), allCurrentEntities);
            }
        }

        public static void RegisterAsync<T>()
        {
            if (!_is_initialized)
            {
                Init();
            }
            
            if (_entitiesPerType.ContainsKey(typeof(T)))
            {
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                var type = (Type)e.Argument;
                if (! _entitiesPerType.ContainsKey(type))
                {
                    var allCurrentEntities = new List<DatabaseEntity>();
                    allCurrentEntities.AddRange(DatabaseManager.Select(type));
                    e.Result = new object[2] { type, allCurrentEntities };
                }
            };
            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Result != null && e.Result is object[])
                {
                    var result = e.Result as object[];
                    var type = (Type)result[0];
                    if (! _entitiesPerType.ContainsKey(type))
                    {
                        _entitiesPerType.Add(type, (List<DatabaseEntity>)result[1]);
                    }
                }
            };
            worker.RunWorkerAsync(typeof(T));
        }
        #endregion
    }
}
