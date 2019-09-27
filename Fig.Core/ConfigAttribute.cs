using System;

namespace Fig
{
    public class ConfigAttribute : Attribute
    {
        /// <summary>
        /// Case insensitive name of the property in the underlying provider, if omitted uses the property name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The default value, if omitted the parameter MUST be present in the config
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Read from provider once and keep in memory.
        /// Default is true. Set to false to retrieve each time.
        /// </summary>
        public bool Cache { get; set; } = true;

        /// <summary>
        /// Override the setting at the class level
        /// </summary>
        public bool Encrypted { get; set; }

        internal string PropertyName { get; set; }
    }
}
