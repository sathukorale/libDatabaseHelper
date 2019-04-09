using System;
using System.Collections.Generic;

namespace libDatabaseHelper.classes.generic
{
    public class TranslatorRegistry
    {
        private static TranslatorRegistry _instance = null;
        public static TranslatorRegistry Instance => _instance ?? (_instance = new TranslatorRegistry());
        
        private readonly Dictionary<Type, ITranslator> _translatorRegistry = new Dictionary<Type, ITranslator>();

        public ITranslator Get<T>() where T: ITranslator
        {
            return Get(typeof(T));
        }

        public ITranslator Get(Type type)
        {
            if (type == null) return null;

            ITranslator translator = null;
            if (_translatorRegistry.TryGetValue(type, out translator))
            {
                return translator;
            }

            translator = Activator.CreateInstance(type) as ITranslator;
            _translatorRegistry.Add(type, translator);

            return translator;
        }
    }

    public interface ITranslator
    {
        object ToTranslated(object value);
        object FromTranslated(object value);
    }

    public class BooleanToYesNoTranslator : ITranslator
    {
        public object ToTranslated(object value)
        {
            var state = (bool) value;
            return state ? "Yes" : "No";
        }

        public object FromTranslated(object value)
        {
            var strValue = (value as string);
            if (string.IsNullOrWhiteSpace(strValue)) return false;

            return strValue.Equals("Yes", StringComparison.OrdinalIgnoreCase) ? true : false;
        }
    }

    public class BooleanToTrueFalseTranslator : ITranslator
    {
        public object ToTranslated(object value)
        {
            var state = (bool)value;
            return state ? "True" : "False";
        }

        public object FromTranslated(object value)
        {
            var strValue = (value as string);
            if (string.IsNullOrWhiteSpace(strValue)) return false;

            return strValue.Equals("True", StringComparison.OrdinalIgnoreCase) ? true : false;
        }
    }
}
