using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fig
{
    /// <summary>
    /// Base class for your typed settings classes or use as is to read settings using string keys
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        
        /// <summary>
        /// Current in-memory property values
        /// </summary>
        private Dictionary<string, CacheEntry> _cache;

        /// <summary>
        /// The actual data as multiple layers of key value pairs
        /// </summary>
        internal CompositeSettingsDictionary SettingsDictionary;

        /// <summary>
        /// Component that can convert strings to various types, pluggable
        /// </summary>
        private IStringConverter _converter;

        /// <summary>
        /// Path to the node in the tree to bind to for example:
        /// Given keys A.B.C and A.B.D, a binding path of A.B
        /// will bind the values of A.B.C and A.B.D to properties C and D of this type 
        /// </summary>
        private string _bindingPath;

        private class CacheEntry
        {
            /// <summary>
            /// The value as read from the underlying source.
            /// Not used
            /// </summary>
            public readonly object OriginalValue;
            
            public object CurrentValue;

            public CacheEntry(object value)
            {
                OriginalValue = value;
                CurrentValue = value;
            }
        }

        public Settings(string bindingPath = null, IStringConverter converter = null)
        {
            _converter = converter ?? new InvariantStringConverter();
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            _cache = new Dictionary<string, CacheEntry>(comparer);
            _bindingPath = bindingPath ?? GetBindingPath();
        }

        private string GetBindingPath()
        {
            //Untyped Settings class should bind to the root of the tree
            if (GetType() == typeof(Settings)) return "";
            
            //All else default to the name of the class excluding namespace
            else return GetType().Name;
        }

       
        internal Settings(CompositeSettingsDictionary settingsDictionary, 
            string bindingPath = null, IStringConverter converter = null)
            :this(bindingPath, converter)
        {
            SettingsDictionary = settingsDictionary;
        }

        /// <summary>
        /// The current environment, used as a suffix when looking up keys, will take precedence over
        /// key with same name without suffix. The suffix does not include the : separator.
        /// </summary>
        public string Configuration { get; private set; }

        /// <summary>
        /// Change the configuration forcing a reload and possible property change notifications
        /// </summary>
        public void SetConfiguration(string configurationName)
        {
            if (configurationName is null) throw new ArgumentNullException();
            if (configurationName == Configuration) return;
            
            var  changedProperties = new List<string>();
            lock (this)
            {
                Configuration = configurationName;
                var oldCache = _cache;
                
                PreLoad(); //will create a new cache with new values based on the new configuration
                
                changedProperties.AddRange( oldCache
                    .Where(oc => _cache[oc.Key].OriginalValue != oc.Value.CurrentValue)
                    .Select(oc => oc.Key));
            }
            
            // Do this outside the lock to avoid deadlocks
            changedProperties.ForEach(NotifyPropertyChanged);
        }

        /// <summary>
        /// Replace all occurrences of the pattern ${KEY} within the provided template
        /// if KEY has a configuration selector, ie ${key:prod} it will take precedence over the current Configuration
        /// </summary>
        public string ExpandVariables(string template) => SettingsDictionary.ExpandVariables(template, Configuration);
        
        /// <summary>
        /// Create an instance of T and populate all it's public properties,
        /// </summary>
        public T Bind<T>(bool requireAll = true, string prefix = null) where T : new()
        {
            var t = new T();
            Bind(t, requireAll, prefix);
            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">The object to set properties on</param>
        /// <param name="requireAll">All the properties on the target must be bound, otherwise an exception is thrown</param>
        /// <param name="prefix">Defaults typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public void Bind<T>(T target, bool requireAll = true, string prefix = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get as string without any conversion
        /// </summary>
        public string Get(string key, Func<string> @default = null) => Get<string>(key, @default);

        /// <summary>
        /// Get the string for a given key and convert to the desired type
        /// </summary>
        /// <param name="key">case insensitive key to fetch</param>
        /// <param name="default">An optional function to provide the default if the key is missing</param>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <returns>The converted value</returns>
        /// <exception cref="KeyNotFoundException">Key is not present and no default was provided</exception>
        public T Get<T>(string key, Func<T> @default = null)
        {
            var found = SettingsDictionary
                .TryGetValue(key, Configuration, out var result);
            if (found) return _converter.Convert<T>(result);
            else if (@default != null) return @default();
            else throw new KeyNotFoundException();
        }

        /// <summary>
        /// Call this method from your strongly typed properties get-method
        /// </summary>
        /// <param name="key">an alternative key to use, defaults to the name of the calling property</param>
        /// <param name="propertyName">Ignore this property, the runtime will assign it automatically</param>
        /// <param name="default">Provide an optional default value</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Get<T>(string key = null, [CallerMemberName] string propertyName = null, Func<T> @default = null)
        {
            if (@default is null) return (T) Get(typeof(T), propertyName);
            else return (T) Get(typeof(T), propertyName, key,() => @default());
        }

        /// <summary>
        /// Convenience overload taking a non-optional default as first argument
        /// </summary>
        /// <param name="default"></param>
        /// <param name="key"></param>
        /// <param name="propertyName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Get<T>(Func<T> @default, string key = null, [CallerMemberName] string propertyName = null)
        {
            return (T) Get(typeof(T), propertyName, key, () => @default());
        }

        private string GetKey(string key, string propertyName)
        {
            key = key ?? propertyName;
            if (_bindingPath.Length > 0) key = _bindingPath + "." + key;
            return key;
        }
        
        /// <summary>
        /// This is where the actual retrieval, conversion and caching happens
        /// </summary>
        private object Get(Type propertyType, string propertyName, string key = null, Func<object> @default = null)
        {
            lock (this)
            {
                if (_cache.ContainsKey(propertyName))
                {
                    return _cache[propertyName].CurrentValue;
                }

                key = GetKey(key, propertyName);
                if (!SettingsDictionary.TryGetValue(key, Configuration, out string value))
                {
                    if (@default == null) throw new KeyNotFoundException(key);
                    return @default.Invoke();
                }

                //TODO: This could throw, deal with it
                var result = _converter.Convert(value, propertyType);

                 _cache[propertyName] = new CacheEntry(result);

                return result;
            }
        }

        /// <summary>
        /// Preload the cache by invoking the getter of each public property
        /// </summary>
        /// <exception cref="ConfigurationException"></exception>
        internal void PreLoad()
        {
            _cache = new Dictionary<string, CacheEntry>();
            var errors = new List<string>();

            foreach(var propertyInfo in GetType().GetProperties())
            {
                try
                {
                    var getter = propertyInfo.GetGetMethod();
                    if (getter == null) continue;
                    
                    var val = getter.Invoke(this, Array.Empty<object>());
                    _cache[propertyInfo.Name] = new CacheEntry(val);
                }
                catch (TargetInvocationException tie)
                {
                    if (tie.InnerException is KeyNotFoundException knf)
                        errors.Add("Missing: " + knf.Message);
                    else throw new Exception("Unexpected inner exception", tie.InnerException);
                }
                catch(NotSupportedException)
                {
                    errors.Add("Can't parse: " + propertyInfo.Name);
                }
            }
            if (errors.Any()) throw new ConfigurationException(errors);
        }

        /// <summary>
        /// Update the current value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected void Set<T>(T value, [CallerMemberName] string propertyName = null)
        {
            var key =  propertyName ?? throw new ArgumentException(nameof(propertyName));
            
            bool shouldNotify = false;

            lock (this)
            {
                var entry = _cache[key];

                if (entry.CurrentValue != null && !entry.CurrentValue.Equals(value))
                {
                    entry.CurrentValue = value;
                    shouldNotify = true;
                }
            }
            if (shouldNotify) NotifyPropertyChanged(key);
        }

        /// <summary>
        /// Sole member of the INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper method to fire the PropertyChanged event
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(string propertyName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception)
            {
                // ignored
                // Can't let an event handler crash our call chain
            }
        }
    }

}
