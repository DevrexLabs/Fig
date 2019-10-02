using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fig
{
    public class Settings : INotifyPropertyChanged
    {
        
        /// <summary>
        /// Current in-memory valuesn
        /// </summary>
        private Dictionary<string, CacheEntry> _cache;

        /// <summary>
        /// The actual configuration data as key value pairs
        /// </summary>
        internal CompositeSettingsDictionary SettingsDictionary;

        private IStringConverter _converter;

        /// <summary>
        /// When binding, look properties on this node in the tree
        /// default is to use the name of the class
        /// </summary>
        private string _bindingPath;

        private class CacheEntry
        {
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
            if (GetType() == typeof(Settings)) return "";
            else return GetType().Name;
        }

       
        internal Settings(CompositeSettingsDictionary settingsDictionary, string bindingPath = null, IStringConverter converter = null)
            :this(bindingPath, converter)
        {
            SettingsDictionary = settingsDictionary;
        }

        /// <summary>
        /// The current environment, used as a suffix when looking up keys, will take precedence over
        /// key with same name without suffix. Suffix does not include separator.
        /// </summary>
        public string Configuration { get; private set; }

        public void SwitchEnvironment(string configurationName)
        {
            Configuration = configurationName;
            //todo, reload
        }
        
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

        public T Get<T>(string key, Func<T> @default = null)
        {
            var found = SettingsDictionary
                .TryGetValue(key, Configuration, out var result);
            if (found) return _converter.Convert<T>(result);
            else if (@default != null) return @default();
            else throw new KeyNotFoundException();
        }

        protected T Get<T>(string key = null, [CallerMemberName] string propertyName = null, Func<T> @default = null)
        {
            if (@default is null) return (T) Get(typeof(T), propertyName);
            else return (T) Get(typeof(T), propertyName, key,() => @default());
        }

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
            var entry = _cache[key];

            if (entry.CurrentValue != null && !entry.CurrentValue.Equals(value))
            {
                entry.CurrentValue = value; 
                NotifyPropertyChanged(key);
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
