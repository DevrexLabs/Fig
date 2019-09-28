using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Core
{
    public class EnvironmentVarsSettingsSource : SettingsSource
    {
        private string _prefix;
        private char _separator;
        
        public EnvironmentVarsSettingsSource(string prefix = "FIG_", char separator = '_')
        {
            _prefix = prefix;
            _separator = separator;
        }
        
        /// <summary>
        /// return all env vars that begin with the prefix (case-insensitive)
        /// Replace separator with a dot to follow conventions
        /// </summary>
        protected override IEnumerable<(string, string)> GetSettings()
        {
            //TODO: implement!
            return Enumerable.Empty<(string, string)>();
        }
    }
}