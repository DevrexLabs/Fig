using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fig
{
    /// <summary>
    /// Base class for custom strongly typed configuration classes
    /// or use it as is if
    /// </summary>
    public class Config 
    {
        /// <summary>
        /// Prefix for all the keys in
        /// </summary>
        private string _prefix = "";

        /// <summary>
        /// Meta data about each configuration property
        /// such as the default value, key name if it differs from the property
        /// name, etc
        /// </summary>
        Dictionary<string, ConfigAttribute> _propertyConfigurations;

        /// <summary>
        /// Current in-memory values
        /// </summary>
        Dictionary<string, CacheEntry> _cache;

        Dictionary<string, List<Action<PropertyChanged>>> _listeners;

        /// <summary>
        /// Provider of the actual configuration data
        /// </summary>
        IProvider _provider;

        private class CacheEntry
        {
            public CacheEntry(object value)
            {
                OriginalValue = value;
                CurrentValue = value;
            }

            public readonly object OriginalValue;
            public object CurrentValue;
        }

        public Config(IProvider provider = null, string prefix = "")
        {
            _provider = provider ?? new DictionaryProvider();
            _prefix = prefix;
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            _cache = new Dictionary<string, CacheEntry>(comparer);
            _propertyConfigurations = new Dictionary<string, ConfigAttribute>(comparer);
            _listeners = new Dictionary<string, List<Action<PropertyChanged>>>(comparer);
        }

        /// <summary>
        /// Useful for testing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Convert<T>(string value)
        {
            return (T)Convert(value, typeof(T));
        }

        public object Convert(string value, Type targetType)
        {
            var typeConverter = TypeDescriptor.GetConverter(targetType);
            return typeConverter.ConvertFromString(value);

        }

        /// <summary>
        /// Retrieve a configuration value either
        /// Call directly with a string key
        /// or call from a strongly typed getter
        /// in a derived class
        /// </summary>
        /// <typeparam name="T">The type to convert the string parameter to</typeparam>
        /// <param name="propertyName">The case insensitive name of the property</param>
        /// <param name="reload">Force a reload from the underlying store</param>
        /// <returns>The retrieved and converted or cached value or an exception</returns>
        public T Get<T>([CallerMemberName] string propertyName = null, bool reload = false)
        {
            return (T) Get(typeof(T), propertyName, reload);
        }

        private object Get(Type propertyType, string propertyName, bool reload)
        {
            lock (this)
            {
                var propConfig = GetPropertyConfig(propertyName);
                var key = _prefix + propConfig.Name;
                if (!reload && propConfig.Cache && _cache.ContainsKey(key))
                {
                    return _cache[key].CurrentValue;
                }

                if (!_provider.TryGetValue(key, out string value))
                {
                    //Not in config and no default
                    if (propConfig.Default == null) throw new KeyNotFoundException(key);
                    return propConfig.Default;
                }

                ///This could throw, deal with it
                var result = Convert(value, propertyType);

                if (propConfig.Cache) _cache[key] = new CacheEntry(result);

                return result;
            }
        }

        /// <summary>
        /// Loads all your configuration properties and
        /// throw an exception if any required parameters are
        /// missing or any configuration value can't be interpreted
        /// </summary>
        public void Validate()
        {
            var errors = new List<string>();

            foreach(var property in GetType().GetProperties())
            {
                try
                {
                    var val = Get(property.PropertyType, property.Name, reload:false);
                }
                catch (KeyNotFoundException knf)
                {
                    errors.Add("Missing: " + knf.Message);
                }
                catch(NotSupportedException)
                {
                    errors.Add("Can't parse: " + property.Name);
                }
            }
            if (errors.Any()) throw new ConfigurationException(errors);

            //todo: Take the provider and iterate all the keys,
            //do we have keys that are not mapped to any property?
        }

        /// <summary>
        /// Update the current value. The value is not written to the underlying storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected void Set<T>(T value, [CallerMemberName] string propertyName = null)
        {
            var propConfig = GetPropertyConfig(propertyName);
            if (!propConfig.Cache)
                throw new InvalidOperationException("Can't write when cache is disabled");

            var key = _prefix + propConfig.Name;
            if (!_cache.ContainsKey(key))
            {
                _cache[key] = new CacheEntry(null); //there was never an original value!
            }
            var entry = _cache[key];

            if (entry.CurrentValue != null && entry.CurrentValue.Equals(value)) return;

            var before = entry.CurrentValue;
            entry.CurrentValue = value;

            Notify(propertyName, entry, before);
        }

        private void Notify(string key, CacheEntry entry, object before)
        {
            if (_listeners.TryGetValue(key, out var listeners))
            {
                var propertyChanged = new PropertyChanged(
                    key,
                    entry.CurrentValue,
                    before,
                    entry.OriginalValue);
                foreach(var listener in listeners)
                {
                    try { listener.Invoke(propertyChanged); }
                    catch { }
                }
            }
        }

        public void Subscribe(string propertyName, Action<PropertyChanged> callback)
        {
            if (!_listeners.ContainsKey(propertyName))
            {
                _listeners[propertyName] = new List<Action<PropertyChanged>>();
            }
            _listeners[propertyName].Add(callback);
        }

        private ConfigAttribute GetPropertyConfig(string propertyName)
        {
            try
            {
                var attr = (ConfigAttribute) GetType()
                .GetProperty(propertyName)
                .GetCustomAttributes(typeof(ConfigAttribute), inherit: false)
                .First();

                if (attr.Name == null) attr.Name = propertyName;

                return  attr;
            }
            catch (Exception)
            {
                return _defaultPropertyConfig;
            }
        }

        private static readonly ConfigAttribute _defaultPropertyConfig
            = new ConfigAttribute();
    }
}
