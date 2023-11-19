using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fig
{
    public sealed class Settings
    {

        /// <summary>
        /// The actual data as multiple layers of key value pairs
        /// </summary>
        internal readonly LayeredSettingsDictionary SettingsDictionary;

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

        public override String ToString()
        {
            if (SettingsDictionary is null) return base.ToString();
            return SettingsDictionary.AsString();
        }
        
        private Settings(string bindingPath = null, IStringConverter converter = null)
        {
            _converter = converter ?? new InvariantStringConverter();
            _bindingPath = bindingPath ?? "";
        }
        

        internal Settings(LayeredSettingsDictionary settingsDictionary,
            string bindingPath = null, IStringConverter converter = null)
            : this(bindingPath, converter)
        {
            SettingsDictionary = settingsDictionary;
        }
        
        /// <summary>
        /// Replace all occurrences of the pattern ${key} within the provided template
        /// if KEY has a configuration selector, ie ${key:prod} it will take precedence over the current Environment
        /// </summary>
        internal string ExpandVariables(string template) => SettingsDictionary.ExpandVariables(template);

        /// <summary>
        /// Create an instance of T and populate all it's public properties,
        /// </summary>
        /// <param name="validate">If any property on the bound object is null, throw an exception. Default is true</param>
        /// <param name="path">Defaults to typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public T Bind<T>(bool validate = true, string path = null) where T : new()
        {
            var target = new T();
            Bind(target, validate, path);
            return target;
        }

        /// <summary>
        /// Populate the properties on a provided Type that match the keys in the SettingsDictionary.
        /// </summary>
        /// <param name="target">The object to set properties on</param>
        /// <param name="validate">If any property on the bound object is null, throw an exception. Default is true</param>
        /// <param name="path">Defaults to typeof(T).Name</param>
        /// <typeparam name="T"></typeparam>
        public void Bind<T>(T target, bool validate = true, string path = null)
        {
            path = path ?? typeof(T).Name;
            BindProperties(target, validate, path);
        }

        private void BindProperties(object target, bool validate, string bindingPath = null)
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
                    var defaultValue = property.GetValue(target);
                    var required = validate && defaultValue is null; 
                    var result = GetPropertyValue(property, name, required);
                    property.SetValue(target, result);
                }
                catch (TargetInvocationException ex)
                {
                    throw new ConfigurationException($"Failed to bind {property.Name}: " + ex.InnerException?.Message);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException($"Failed to bind {property.Name}: " + ex.GetType().Name);
                }
            }
        }
        
        private object GetPropertyValue(PropertyInfo property, string key, bool required)
        {
            if (SettingsDictionary.TryGetValue(key, out _))
            {
                return Get(property.PropertyType, key);
            }

            if (required) throw new ConfigurationException("Missing key " + key);
            return null;
        }
        
        /// <summary>
        /// Retrieve a value as string without any conversion.
        /// If the key is not found, return the default provided by the callback
        /// </summary>
        public string Get(string key, Func<string> @default = null)
            => Get<string>(key, @default);

        /// <summary>
        /// Retrieve a value as string without any conversion.
        /// If the key is not found, return the default
        /// </summary>
        public string Get(string key, string @default) => Get(key, () => @default);

        /// <summary>
        /// Retrieve the the value for the given key and convert to type T.
        /// If the key is missing return the provided default
        /// </summary>
        public T Get<T>(string key, T @default) => Get<T>(key, () => @default);
        
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
            var found = SettingsDictionary.TryGetValue(key, out var result);
            if (found) return _converter.Convert<T>(result);
            if (@default != null) return @default();
            throw new KeyNotFoundException();
        }

        public bool TryGet<T>(string key, out T result)
        {
            result = default(T);
            var found = SettingsDictionary.TryGetValue(key, out var value);
            if (found) result = _converter.Convert<T>(value);
            return found;
        }


        /// <summary>
        /// This is where the actual retrieval and type conversion happens
        /// </summary>
        private object Get(Type propertyType, string propertyName)
        {
            var key = propertyName;
            if (_bindingPath.Length > 0) key = _bindingPath +  "." + key;

            if (!SettingsDictionary.TryGetValue(key, out string value))
            {
                throw new KeyNotFoundException(key);
            }

            var result = _converter.Convert(value, propertyType);
            return result;
        }
    }
}