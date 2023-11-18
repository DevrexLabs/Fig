using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fig
{
    internal class EnvironmentVariablesSource : SettingsSource
    {
        private readonly string _prefix;

        private const char Separator = '_';
        
        private readonly bool _dropPrefix;
        private readonly IDictionary _vars;
        
        
        public EnvironmentVariablesSource(string prefix, bool dropPrefix)
            :this(Environment.GetEnvironmentVariables(), prefix, dropPrefix)
        {
        }

        internal EnvironmentVariablesSource(IDictionary vars, string prefix, bool dropPrefix)
        {
            _vars = vars;
            _prefix = prefix;
            _dropPrefix = dropPrefix;
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
                var transformedKey = key;
                if (_dropPrefix) transformedKey = key.Remove(0, _prefix.Length);
                transformedKey = transformedKey.Replace(Separator, '.');

                yield return (transformedKey, (string) _vars[key]);
            }
        }
    }
}