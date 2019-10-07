using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Fig.Test")]
namespace Fig.AppSettingsXml
{
    /// <summary>
    /// Provider for NET Framework configuration files
    /// </summary>
    public class AppSettingsXmlSource : SettingsSource
    {

        private readonly NameValueCollection _nameValueCollection;

        private readonly ConnectionStringSettingsCollection _connectionStrings;
        
        public bool IncludeConnectionStrings { get; set; } = true;
        
        /// <summary>
        /// Prepend this prefix to connection string names
        /// </summary>
        public string ConnectionStringPrefix { get; set; } = "ConnectionStrings";
        
        
        public AppSettingsXmlSource()
            :this(ConfigurationManager.AppSettings, ConfigurationManager.ConnectionStrings)
        {
            
        }

        /// <summary>
        /// Create an instance of the provider based
        /// on an alternative NameValueCollection.
        /// Useful for testing.
        /// </summary>
        internal AppSettingsXmlSource(NameValueCollection nameValueCollection, 
            ConnectionStringSettingsCollection connectionStrings = null)
        {
            _nameValueCollection = nameValueCollection;
            _connectionStrings = connectionStrings ?? new ConnectionStringSettingsCollection();
        }
        

        public IEnumerable<string> AllKeys(string prefix = "")
        {
            var comparison = StringComparison.InvariantCultureIgnoreCase;
            return _nameValueCollection
                .AllKeys
                .Where(key => key.StartsWith(prefix, comparison));
        }
        
        protected override IEnumerable<(string, string)> GetSettings()
        {
            foreach (var key in _nameValueCollection.AllKeys)
            {
                yield return (key, _nameValueCollection.Get(key));
            }

            if (IncludeConnectionStrings)
            {
                var cs = _connectionStrings;
                var prefix = ConnectionStringPrefix + ".";
                for (int i = 0; i < cs.Count; i++)
                {
                   yield return (prefix + cs[i].Name, cs[i].ConnectionString);
                }
            }
    
        }
    }
}
