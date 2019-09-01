using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace Fig.AppSettingsXml
{
    /// <summary>
    /// Provider for NET Framework configuration files
    /// </summary>
    public class AppSettingsXml : Provider
    {
        // TODO: Not sure we need this, the appSettings keys
        // are likely case insensitive already.
        Dictionary<string, string> _caseInsensitiveMap;

        /// <summary>
        /// The appSettings section is a readonly NameValueCollection
        /// We use that here so that we can easily write automated tests
        /// </summary>
        NameValueCollection _settings;

        /// <summary>
        /// Create an instance of the provider based on the
        /// current application/web configuration
        /// </summary>
        public AppSettingsXml()
            :this(ConfigurationManager.AppSettings)
        {
        }

        /// <summary>
        /// Create an instance of the provider based
        /// on an alternative NameValueCollection.
        /// Useful for testing.
        /// </summary>
        public AppSettingsXml(NameValueCollection settings)
        {
            _settings = settings;
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            _caseInsensitiveMap = new Dictionary<string, string>(comparer);
            PopulateMap();
        }

        private void PopulateMap()
        {
            foreach (var key in AllKeys())
                _caseInsensitiveMap[key] = key;
        }

        public override IEnumerable<string> AllKeys(string prefix = "")
        {
            var comparison = StringComparison.InvariantCultureIgnoreCase;
            return _settings
                .AllKeys
                .Where(key => key.StartsWith(prefix, comparison));
        }

        public override string Get(string key)
        {
            var casedKey = _caseInsensitiveMap[key];
            var result = _settings[casedKey];
            if (result == null) throw new KeyNotFoundException(key);
            return result;
        }

        public override bool TryGetValue(string key, out string value)
        {
            try
            {
                value = Get(key);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }
    }
}
