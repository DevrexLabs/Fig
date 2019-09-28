using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Core
{
    public class EnvironmentVarsSettingsSource : SettingsSource
    {
        private string _prefix;
        private char _separator;
        
        private IDictionary _vars;
        
        
        public EnvironmentVarsSettingsSource(string prefix = "", char separator = '_')
            :this(Environment.GetEnvironmentVariables(), prefix, separator)
        {
        }

        internal EnvironmentVarsSettingsSource(IDictionary vars, string prefix = "", char separator = '_')
        {
            _vars = vars;
            _prefix = prefix;
            _separator = separator;
        }
        
        /// <summary>
        /// return all env vars that begin with the prefix (case-insensitive)
        /// Replace separator with a dot to follow conventions
        /// </summary>
        protected override IEnumerable<(string, string)> GetSettings()
        {
            var filteredKeys = _vars
                .Keys
                .Cast<string>()
                .Where(k => k.StartsWith(_prefix, StringComparison.InvariantCultureIgnoreCase));
            
            foreach (var key in filteredKeys)
            {
                var transformedKey = key
                    .Remove(0, _prefix.Length)
                    .Replace('_', '.');

                yield return (transformedKey, (string) _vars[key]);
            }
        }
    }
}