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
            if (!String.IsNullOrEmpty(env) && !key.Contains(":")) suffix = ":" + env;

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
        /// <param name="configuration">A specific configuration to use if key is unqualified</param>
        /// <returns>the resulting string or an exception if key is missing</returns>
        public string ExpandVariables(string template, string configuration = null)
        {
            var pattern = "\\$\\{\\s*(?<key>[a-z0-9.]+)(:(?<env>[a-z]+))?\\s*\\}";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return regex.Replace(template, m => GetCurrentEnvironment(m, configuration));
        }
        private string GetCurrentEnvironment(Match m, string configuration)
        {
            var key = m.Groups["key"]. Value;
            var env = m.Groups["env"].Success ? m.Groups["env"].Value : configuration;
            TryGetValue(key, env, out var result);
            return result;
        }

    }
}