using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fig
{
    /// <summary>
    /// A sequence of dictionaries where keys from layers with lower index
    /// hide keys in the layers with higher indexes.
    /// </summary>
    internal class CompositeSettingsDictionary
    {
        private readonly List<SettingsDictionary> _dictionaries;

        public CompositeSettingsDictionary()
        {
            _dictionaries = new List<SettingsDictionary>();
        }

        public void Add(SettingsDictionary settingsDictionary) 
            => _dictionaries.Add(settingsDictionary);

        public bool TryGetValue(string key, string env, out string value)
        {
            value = null;
            
            var suffix = "";
            if (!String.IsNullOrEmpty(env)) suffix = ":" + env;

            foreach (var sd in _dictionaries)
            {
                if (suffix != "" && sd.TryGetValue(key + suffix, out value)) break;
                if (sd.TryGetValue(key, out value)) break;
            }

            return value != null;
        }

        /// <summary>
        /// Look for ${key} and replace with values from the dictionary 
        /// </summary>
        /// <param name="template">The string to expand</param>
        /// <returns>the resulting string or an exception if key is missing</returns>
        public string ExpandVariables(string template)
        {
            return Regex.Replace(template, @"\${(?<var>\w+)}", GetCurrentEnvironment);
        }
        private string GetCurrentEnvironment(Match m)
        {
            var key = m.Groups["var"]. Value;
            TryGetValue(key, null, out var result);
            return result;
        }

    }
}