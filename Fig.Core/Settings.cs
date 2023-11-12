using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Fig
{
    /// <summary>
    /// Base class for your typed settings classes or use as is to read settings using string keys
    /// </summary>
    public sealed class Settings
    {

        /// <summary>
        /// The actual data as multiple layers of key value pairs
        /// </summary>
        internal readonly CompositeSettingsDictionary SettingsDictionary;

        /// <summary>
        /// Component that can convert strings to various types, pluggable
        /// </summary>
        private readonly IStringConverter _converter;

        /// <summary>
        /// Path to the node in the tree to bind to for example:
        /// Given keys A.B.C and A.B.D, a binding path of A.B
        /// will bind the values of A.B.C and A.B.D to properties C and D of this type
        /// </summary>
        private readonly string _bindingPath;

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
        
        private Settings(string bindingPath = null, IStringConverter converter = null)
        {
            _converter = converter ?? new InvariantStringConverter();
            _bindingPath = bindingPath ?? "";
            Profile = "";
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
        public string Profile { get; internal set; }

        /// <summary>
        /// Change the current environment forcing a reload and possible property change notifications
        /// </summary>
        public void SetProfile(string environmentName)
        {
            Profile = environmentName ?? throw new ArgumentNullException(nameof(environmentName));
        }

        /// <summary>
        /// Replace all occurrences of the pattern ${key} within the provided template
        /// if KEY has a configuration selector, ie ${key:prod} it will take precedence over the current Environment
        /// </summary>
        public string ExpandVariables(string template) => SettingsDictionary.ExpandVariables(template, Profile);

        /// <summary>
        /// Create an instance of T and populate all it's public properties,
        /// </summary>
        /// <param name="requireAll">All the properties on the target must be bound, otherwise an exception is thrown</param>
        /// <param name="path">Defaults typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public T Bind<T>(bool requireAll = true, string path = null) where T : new()
        {
            var target = new T();
            Bind(target, requireAll, path);
            return target;
        }

        /// <summary>
        /// Populate the properties on a provided Type that match the keys in the SettingsDictionary.
        /// </summary>
        /// <param name="target">The object to set properties on</param>
        /// <param name="requireAll">All the properties on the target must be bound, otherwise an exception is thrown</param>
        /// <param name="path">Defaults typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public void Bind<T>(T target, bool requireAll = true, string path = null)
        {
            path = path ?? typeof(T).Name;
            BindProperties(target, requireAll, path);
        }

        private void BindProperties(object target, bool requireAll = true, string bindingPath = null)
        { 
            bindingPath = bindingPath ?? _bindingPath;

            foreach (var property in target.GetType().GetProperties())
            {
                try
                {
                    var propertyIsReadonly = property.GetSetMethod() is null;

                    if (propertyIsReadonly 
                        || property.PropertyType.IsAbstract 
                        || property.PropertyType.IsInterface) continue;
                    
                    var name = String.IsNullOrEmpty(bindingPath?.Trim()) ? property.Name : $"{bindingPath}.{property.Name}";
                    var result = GetPropertyValue(property, name, requireAll);
                    property.SetValue(target, result);
                }
                catch (TargetInvocationException ex)
                {
                    throw;
                    throw new ConfigurationException($"Failed to bind {property.Name}: " + ex.InnerException.Message);
                }
                catch (Exception ex)
                {
                    throw;
                    throw new ConfigurationException($"Failed to bind {property.Name}: " + ex.GetType().Name);
                }
            }
        }
        
        private object GetPropertyValue(PropertyInfo property, string key, bool requireAll)
        {
            if (SettingsDictionary.TryGetValue(key, Profile, out _))
            {
                return Get(property.PropertyType, key);
            }

            if (requireAll) throw new ConfigurationException("Missing key " + key);
            return null;
        }
        
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
                .TryGetValue(key, Profile, out var result);
            if (found) return _converter.Convert<T>(result);
            else if (@default != null) return @default();
            else throw new KeyNotFoundException();
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

                if (!SettingsDictionary.TryGetValue(key, Profile, out string value))
                {
                    if (@default == null) throw new KeyNotFoundException(key);
                    return @default.Invoke();
                }

                //TODO: This could throw, deal with it
                var result = _converter.Convert(value, propertyType);
                return result;
            }
        }
    }
}