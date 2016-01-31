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
        private static Dictionary<Type, List<GenericDatabaseEntity>> _entitiesPerType = new Dictionary<Type, List<GenericDatabaseEntity>>();

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

            DatabaseEntity.OnDatabaseEntityUpdated += new GenericDatabaseEntity.UpdateEvent(DatabaseEntity_OnDatabaseEntityUpdated);
            DatabaseManager.OnBulkDelete += new GenericDatabaseManager.BulkDelete(DatabaseManager_OnBulkDelete);
        }

        public static void CleanUp()
        {
            foreach (var entityListRecord in _entitiesPerType)
            {
                entityListRecord.Value.Clear();
            }
        }

        #region "Selectors"
        public static List<GenericDatabaseEntity> Select<T>()
        {
            return Select(typeof(T), null);
        }

        public static List<GenericDatabaseEntity> Select<T>(Selector[] selectors)
        {
            return Select(typeof(T), selectors);
        }

        public static List<GenericDatabaseEntity> Select(Type type)
        {
            return Select(type, null);
        }

        public static List<GenericDatabaseEntity> Select(Type type, Selector[] selectors)
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

        static void DatabaseEntity_OnDatabaseEntityUpdated(GenericDatabaseEntity updatedEntity, Type type, GenericDatabaseEntity.UpdateEventType event_type)
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
        private static List<GenericDatabaseEntity> FindMatchingEntities<T>(Selector[] selector)
        {
            return FindMatchingEntities(typeof(T), selector);
        }

        private static List<GenericDatabaseEntity> FindMatchingEntities(Type type, Selector[] selectors)
        {
            var list = _entitiesPerType[type];
            var collected = new List<GenericDatabaseEntity>();
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
                        if (GenericFieldTools.Compare(present_val, sent_val) != -1)
                        {
                            is_entity_valid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.MoreThan)
                    {
                        var present_val = entity.GetFieldValue(selector.Field);
                        var sent_val = selector.FieldValue1;
                        if (GenericFieldTools.Compare(present_val, sent_val) != 1)
                        {
                            is_entity_valid = false;
                        }
                    }
                    else if (selector.OpeartorType == Selector.Operator.Between)
                    {
                        var present_val = entity.GetFieldValue(selector.Field);
                        if (GenericFieldTools.Compare(present_val, selector.FieldValue1) != 1 && GenericFieldTools.Compare(present_val, selector.FieldValue2) != -1)
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
            Register(typeof(T));
        }

        public static void Register(Type t)
        {
            if (!_is_initialized)
            {
                Init();
            }

            if (!_entitiesPerType.ContainsKey(t))
            {
                var allCurrentEntities = new List<GenericDatabaseEntity>();
                var referenceEntity = DatabaseEntity.GetNonDisposableRefenceObject(t);
                allCurrentEntities.AddRange(GenericDatabaseManager.GetDatabaseManager(referenceEntity.GetSupportedDatabaseType()).Select(t, null));

                _entitiesPerType.Add(t, allCurrentEntities);
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
                var tmpInstance = GenericDatabaseEntity.GetNonDisposableRefenceObject(typeof(T));
                if (! _entitiesPerType.ContainsKey(type))
                {
                    var allCurrentEntities = new List<GenericDatabaseEntity>();
                    allCurrentEntities.AddRange(GenericDatabaseManager.GetDatabaseManager(tmpInstance.GetSupportedDatabaseType()).Select(type));
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
                        _entitiesPerType.Add(type, (List<GenericDatabaseEntity>)result[1]);
                    }
                }
            };
            worker.RunWorkerAsync(typeof(T));
        }
        #endregion
    }
}
