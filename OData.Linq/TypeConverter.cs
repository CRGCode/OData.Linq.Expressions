using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OData.Linq
{
    /// <copydoc cref="ITypeConverter" />
    public class TypeConverter : ITypeConverter
    {
        private readonly Dictionary<Type, Func<IDictionary<string, object>, object>> _dictionaryConverters;
        private readonly Dictionary<Type, Func<object, object>> _objectConverters;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeConverter"/> class.
        /// </summary>
        public TypeConverter()
        {
            _dictionaryConverters = new Dictionary<Type, Func<IDictionary<string, object>, object>>();
            _objectConverters = new Dictionary<Type, Func<object, object>>();
        }

        /// <copydoc cref="ITypeConverter.RegisterTypeConverter{T}(Func{IDictionary{string, object}, object})" />
        public void RegisterTypeConverter<T>(Func<IDictionary<string, object>, object> converter)
        {
            RegisterTypeConverter(typeof(T), converter);
        }

        /// <copydoc cref="ITypeConverter.RegisterTypeConverter{T}(Func{object, object})" />
        public void RegisterTypeConverter<T>(Func<object, object> converter)
        {
            RegisterTypeConverter(typeof(T), converter);
        }

        /// <copydoc cref="ITypeConverter.RegisterTypeConverter(Type, Func{IDictionary{string, object}, object})" />
        public void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
        {
            lock (_dictionaryConverters)
            {
                if (_dictionaryConverters.ContainsKey(type))
                {
                    _dictionaryConverters.Remove(type);
                }
                _dictionaryConverters.Add(type, converter);
            }
        }

        /// <copydoc cref="ITypeConverter.RegisterTypeConverter(Type, Func{object, object})" />
        public void RegisterTypeConverter(Type type, Func<object, object> converter)
        {
            lock (_objectConverters)
            {
                if (_objectConverters.ContainsKey(type))
                {
                    _objectConverters.Remove(type);
                }
                _objectConverters.Add(type, converter);
            }
        }

        /// <copydoc cref="ITypeConverter.HasDictionaryConverter{T}" />
        public bool HasDictionaryConverter<T>()
        {
            return HasDictionaryConverter(typeof (T));
        }

        /// <copydoc cref="ITypeConverter.HasDictionaryConverter(Type)" />
        public bool HasDictionaryConverter(Type type)
        {
            return _dictionaryConverters.ContainsKey(type);
        }

        /// <copydoc cref="ITypeConverter.HasObjectConverter{T}" />
        public bool HasObjectConverter<T>()
        {
            return HasObjectConverter(typeof(T));
        }

        /// <copydoc cref="ITypeConverter.HasObjectConverter(Type)" />
        public bool HasObjectConverter(Type type)
        {
            return _objectConverters.ContainsKey(type);
        }

        /// <copydoc cref="ITypeConverter.Convert{T}(IDictionary{string, object})" />
        public T Convert<T>(IDictionary<string, object> value)
        {
            return (T)Convert(value, typeof(T));
        }

        /// <copydoc cref="ITypeConverter.Convert{T}(object)" />
        public T Convert<T>(object value)
        {
            return (T)Convert(value, typeof(T));
        }

        /// <copydoc cref="ITypeConverter.Convert(IDictionary{string, object}, Type)" />
        public object Convert(IDictionary<string, object> value, Type type)
        {
            if (_dictionaryConverters.TryGetValue(type, out var converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException($"No custom converter found for type {type}");
        }

        /// <copydoc cref="ITypeConverter.Convert(IDictionary{string, object}, Type)" />
        public object Convert(object value, Type type)
        {
            if (_objectConverters.TryGetValue(type, out var converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException($"No custom converter found for type {type}");
        }
    }

    public static class CustomConverters
    {
        private static ConcurrentDictionary<string, ITypeConverter> _converters;

        static CustomConverters()
        {
            _converters = new ConcurrentDictionary<string, ITypeConverter>();
        }

        public static ITypeConverter Converter(string uri)
        {
            // TODO: Have a settings switch whether we use global dictionary or not?
            return _converters.GetOrAdd(uri, new TypeConverter());
        }

        public static ITypeConverter Global => Converter("global");

        [Obsolete("Use ODataClientSettings.TypeCache.RegisterTypeConverter")]
        public static void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
        {
            Global.RegisterTypeConverter(type, converter);

            // Side-effect if we call the global is to register in all other converters
            foreach (var kvp in _converters)
            {
                if (kvp.Key != "global")
                {
                    kvp.Value.RegisterTypeConverter(type, converter);
                }
            }
        }

        [Obsolete("Use ODataClientSettings.TypeCache.RegisterTypeConverter")]
        public static void RegisterTypeConverter(Type type, Func<object, object> converter)
        {
            Global.RegisterTypeConverter(type, converter);

            // Side-effect if we call the global is to register in all other converters
            foreach (var kvp in _converters)
            {
                if (kvp.Key != "global")
                {
                    kvp.Value.RegisterTypeConverter(type, converter);
                }
            }
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static bool HasDictionaryConverter(Type type)
        {
            return Global.HasDictionaryConverter(type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static bool HasObjectConverter(Type type)
        {
            return Global.HasObjectConverter(type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static T Convert<T>(IDictionary<string, object> value)
        {
            return Global.Convert<T>(value);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static T Convert<T>(object value)
        {
            return Global.Convert<T>(value);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static object Convert(IDictionary<string, object> value, Type type)
        {
            return Global.Convert(value, type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static object Convert(object value, Type type)
        {
            return Global.Convert(value, type);
        }
    }

}