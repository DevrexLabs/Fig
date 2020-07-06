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
        private readonly IStringConverter _converter;

        /// <summary>
        /// Path to the node in the tree to bind to for example:
        /// Given keys A.B.C and A.B.D, a binding path of A.B
        /// will bind the values of A.B.C and A.B.D to properties C and D of this type
        /// </summary>
        private string _bindingPath;

        private readonly HashSet<Type> _nonNestedPropertyTypes = new HashSet<Type> {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        public override String ToString()
        {
            if (SettingsDictionary is null) return base.ToString();
            return SettingsDictionary.AsString();
        }
 
        private class CacheEntry
        {
            /// <summary>
            /// The value as read from the underlying source.
            /// Not used
            /// </summary>
            private readonly object OriginalValue;

            public object CurrentValue;

            public CacheEntry(object value)
            {
                OriginalValue = value;
                CurrentValue = value;
            }

            public bool Update(object val)
            {
                if (CurrentValue == val) return false;

                CurrentValue = val;
                return true;
            }
        }

        private interface IBindResult
        {
            IEnumerable<string> Errors { get; }
            object Result { get; }
        }

        private class BindResultBase : IBindResult
        {
            public BindResultBase(IEnumerable<string> errors, object result)
            {
                Errors = errors;
                Result = result;
            }
            public IEnumerable<string> Errors { get; }
            public object Result { get; }

            object IBindResult.Result => this.Result;
        }

        private class BindResult<T> : BindResultBase
        {
            public BindResult(List<string> errors, T result) : base(errors, result)
            {
                Result = result;
            }

            public new T Result { get; }
        }

        protected Settings(string bindingPath = null, IStringConverter converter = null)
        {
            _converter = converter ?? new InvariantStringConverter();
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            _cache = new Dictionary<string, CacheEntry>(comparer);
            SetBindingPath(bindingPath);
            Environment = "";
        }

        private string GetBindingPath()
        {
            //Untyped Settings class should bind to the root of the tree
            if (GetType() == typeof(Settings)) return "";

            //All else default to the name of the class excluding namespace
            else return GetType().Name;
        }

        internal void SetBindingPath(string bindingPath)
        {
            _bindingPath = bindingPath ?? GetBindingPath();
        }

        internal Settings(CompositeSettingsDictionary settingsDictionary,
            string bindingPath = null, IStringConverter converter = null)
            : this(bindingPath, converter)
        {
            SettingsDictionary = settingsDictionary;
        }

        /// <summary>
        /// The current environment, used as a suffix when looking up keys, will take precedence over
        /// key with same name without suffix. The suffix does not include the : separator.
        /// </summary>
        public string Environment { get; internal set; }

        /// <summary>
        /// Change the current environment forcing a reload and possible property change notifications
        /// </summary>
        public void SetEnvironment(string environmentName)
        {
            var oldEnvironment = Environment;
            var oldCache = _cache;
            try
            {
                if (environmentName is null) throw new ArgumentNullException();
                if (environmentName == Environment) return;


                List<string> changedProperties = null;

                lock (this)
                {
                    Environment = environmentName;
                    PreLoad();
                    changedProperties = oldCache.Keys
                        .Where(key => !oldCache[key].CurrentValue.Equals(_cache[key].CurrentValue))
                        .ToList();
                }
                // Do this outside the lock to avoid deadlocks
                changedProperties.ForEach(NotifyPropertyChanged);
            }
            catch (Exception)
            {
                Environment = oldEnvironment;
                _cache = oldCache;
                throw;
            }
        }

        /// <summary>
        /// Replace all occurrences of the pattern ${key} within the provided template
        /// if KEY has a configuration selector, ie ${key:prod} it will take precedence over the current Environment
        /// </summary>
        public string ExpandVariables(string template) => SettingsDictionary.ExpandVariables(template, Environment);

        /// <summary>
        /// Create an instance of T and populate all it's public properties,
        /// </summary>
        /// <param name="requireAll">All the properties on the target must be bound, otherwise an exception is thrown</param>
        /// <param name="prefix">Defaults typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public T Bind<T>(bool requireAll = true, string prefix = null) where T : new()
        {
            prefix = prefix ?? typeof(T).Name;

            var result = GetBindResult<T>(requireAll, prefix);

            if (result.Errors.Any()) throw new ConfigurationException(result.Errors.ToList());

            return result.Result;
        }

        /// <summary>
        /// Populate the properties on a provided Type that match the keys in the SettingsDictionary.
        /// </summary>
        /// <param name="target">The object to set properties on</param>
        /// <param name="requireAll">All the properties on the target must be bound, otherwise an exception is thrown</param>
        /// <param name="prefix">Defaults typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public void Bind<T>(T target, bool requireAll = true, string prefix = null)
        {
            prefix = prefix ?? typeof(T).Name;

            var errors = new List<string>();

            var result = BindProperties(target, requireAll, prefix, false, errors);

            if (result.Errors.Any()) throw new ConfigurationException(errors);
        }

        private BindResult<T> GetBindResult<T>(bool requireAll = true, string prefix = null, bool preload = false, List<string> errors = null) where T : new()
        {
            var t = new T();
            return BindProperties(t, requireAll, prefix, preload, errors);
        }

        private BindResult<T> BindProperties<T>(T target, bool requireAll = true, string prefix = null, bool preload = false, List<string> errors = null)
        {
            errors = errors ?? new List<string>();

            prefix = prefix ?? _bindingPath;

            foreach (var prop in typeof(T).GetProperties())
            {
                try
                {
                    if (preload && prop.Name == nameof(Environment)) continue;

                    var readonlyProp = prop.GetSetMethod() is null;
                    var name = String.IsNullOrEmpty(prefix?.Trim()) ? prop.Name : $"{prefix}.{prop.Name}";

                    // No need to continue if the property is readonly and not part of a preload operation
                    if (readonlyProp && !preload) continue;

                    if (prop.PropertyType.IsAbstract || prop.PropertyType.IsInterface) continue;

                    var result = GetPropertyValue(prop, name, preload, requireAll);

                    if (!(result?.Result is null))
                    {
                        // When there is a preload and the value is not in the cache yet, do that here
                        if (preload && !_cache.ContainsKey(name))
                        {
                            _cache[name] = new CacheEntry(result.Result);
                        }
                        // Otherwise set the value of the property if it is not readonly
                        else if (!readonlyProp)
                        {
                            prop.SetValue(
                                target,
                                result.Result
                            );
                        }
                    }

                    if (result?.Errors?.Any() == true)
                    {
                        errors.AddRange(result.Errors);
                    }
                }
                catch (TargetInvocationException tie)
                {
                    switch (tie.InnerException)
                    {
                        case KeyNotFoundException _:
                            errors.Add(prop.Name + " not found");
                            break;
                        case FormatException fe:
                            errors.Add("Can't parse " + prop.Name);
                            break;
                        default:
                            errors.Add(prop.Name + " failed," + tie.InnerException.Message);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(prop.Name + " failed, " + ex.Message);
                }
            }

            return new BindResult<T>(errors, target);
        }

        /// <summary>
        /// Method to retrieve value either from the settings dictionary or from cache. Uses recursion to get nested class values
        /// </summary>
        /// <param name="prop">Property of class that a value is to be returned</param>
        /// <param name="name">Name of settings dictionary key that is linked to property</param>
        /// <param name="preload">Determines if the call is from a preload event or not</param>
        /// <param name="requireAll">Determines if all properties of the binding class should have a matching settings dictionary key and value</param>
        /// <returns></returns>
        private IBindResult GetPropertyValue(PropertyInfo prop, string name, bool preload, bool requireAll)
        {
            object value = null;
            var errors = new List<string>();

            if (IsNestedProperty(prop.PropertyType))
            {
                // Get nested property value via recursion
                var valObjectResult = GetType()
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == nameof(Settings.GetBindResult) && x.GetParameters()?.Length == 4)?
                    .MakeGenericMethod(prop.PropertyType)
                    .Invoke(this, new object[] { requireAll, name, false, errors });

                if (valObjectResult is IBindResult typedResult)
                {
                    if (typedResult.Errors?.Any() ?? false)
                    {
                        errors.AddRange(typedResult.Errors);
                    }

                    value = typedResult.Result;
                }
            }
            // When the call is not for a preload event and it is not nested, attempt to get a value
            else if (preload)
            {
                var getter = prop.GetGetMethod();
                if (getter == null) return null;

                value = getter.Invoke(this, Array.Empty<object>());
            }

            // When the value is still null, last chance call to check if there is a value in the settings dictionary
            if (value is null && SettingsDictionary.TryGetValue(name, Environment, out var result) || requireAll)
            {
                value = Get(prop.PropertyType, name);
            }

            return new BindResultBase(errors, value);
        }

        /// <summary>
        /// Determines whether the property type provided is a nested class or not
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True or False</returns>
        private bool IsNestedProperty(Type type) =>
            !type.IsPrimitive
            && !type.IsEnum
            && !this._nonNestedPropertyTypes.Contains(type);

        /// <summary>
        /// Get as string without any conversion
        /// </summary>
        public string Get(string key, Func<string> @default = null)
            => Get<string>(key, @default);

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
                .TryGetValue(key, Environment, out var result);
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
            if (@default is null) return (T)Get(typeof(T), propertyName);
            else return (T)Get(typeof(T), propertyName, key, () => @default());
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
            return (T)Get(typeof(T), propertyName, key, () => @default());
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
                key = GetKey(key, propertyName);
                if (_cache.ContainsKey(key))
                {
                    return _cache[key].CurrentValue;
                }

                if (!SettingsDictionary.TryGetValue(key, Environment, out string value))
                {
                    if (@default == null) throw new KeyNotFoundException(key);
                    return @default.Invoke();
                }

                //TODO: This could throw, deal with it
                var result = _converter.Convert(value, propertyType);
                return result;
            }
        }

        /// <summary>
        /// Preload the cache by invoking the getter of each public property
        /// </summary>
        /// <exception cref="ConfigurationException"></exception>
        internal void PreLoad()
        {
            _cache = new Dictionary<string, CacheEntry>(StringComparer.InvariantCultureIgnoreCase);

            var errors = new List<string>();

            var settingsType = typeof(Settings);
            var currentType = this.GetType();

            if (settingsType != currentType)
            {
                settingsType
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == nameof(Settings.BindProperties) && x.GetParameters()?.Length == 5)?
                    .MakeGenericMethod(currentType)
                    .Invoke(this, new object[] { this, false, null, true, errors });
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
            var key = propertyName ?? throw new ArgumentException(nameof(propertyName));

            bool shouldNotify = false;

            lock (this)
            {
                var entry = _cache[GetKey(null, key)];

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