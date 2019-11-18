using System;
using System.Collections.Generic;
using System.ComponentModel;

using libDatabaseHelper.classes.sqlce;
using System.Text.RegularExpressions;

namespace libDatabaseHelper.classes.generic
{
    public class UniversalDataModel
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

            GenericDatabaseEntity.OnDatabaseEntityUpdated += new GenericDatabaseEntity.UpdateEvent(DatabaseEntity_OnDatabaseEntityUpdated);
            GenericDatabaseManager.OnBulkDelete += new GenericDatabaseManager.BulkDelete(DatabaseManager_OnBulkDelete);
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

            if (event_type == GenericDatabaseEntity.UpdateEventType.Add)
            {
                _entitiesPerType[type].Add(updatedEntity);
            }
            else if (event_type == GenericDatabaseEntity.UpdateEventType.Remove)
            {
                _entitiesPerType[type].RemoveAll(i => i.Equals(updatedEntity));
            }
            else
            {
                var list = _entitiesPerType[type];
                var wasFound = false;
                for (var i = 0 ; i < list.Count ; i++)
                {
                    var entity = list[i];
                    if (!entity.Equals(updatedEntity)) continue;

                    list[i] = updatedEntity;
                    wasFound = true;
                    break;
                }

                if (wasFound == false)
                    list.Add(updatedEntity);
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
            return GenericDatabaseManager.FindMatchingEntities(list, selectors);
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

            if (_entitiesPerType.ContainsKey(t)) return;

            var allCurrentEntities = new List<GenericDatabaseEntity>();
            var referenceEntity = GenericDatabaseEntity.GetNonDisposableReferenceObject(t);

            if (referenceEntity == null) return;

            allCurrentEntities.AddRange(GenericDatabaseManager.GetDatabaseManager(referenceEntity.GetSupportedDatabaseType()).Select(t, null));
            _entitiesPerType.Add(t, allCurrentEntities);
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
                var tmpInstance = GenericDatabaseEntity.GetNonDisposableReferenceObject(typeof(T));

                if (_entitiesPerType.ContainsKey(type)) return;
                if (tmpInstance == null) return;

                var allCurrentEntities = new List<GenericDatabaseEntity>();
                allCurrentEntities.AddRange(GenericDatabaseManager.GetDatabaseManager(tmpInstance.GetSupportedDatabaseType()).Select(type));
                e.Result = new object[2] { type, allCurrentEntities };
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
