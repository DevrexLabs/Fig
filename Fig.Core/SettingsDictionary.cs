using System;
using System.Collections.Generic;

namespace Fig
{
    /// <summary>
    /// A case insensitive string dictionary
    /// </summary>
    public class SettingsDictionary : Dictionary<string,string>
    {
        public SettingsDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Return a new SettingsDictionary with environment qualifiers 
        /// </summary>
        /// <returns></returns>
        internal SettingsDictionary WithNormalizedEnvironmentQualifiers()
        {
            var result = new SettingsDictionary();
            var transformer = new KeyTransformer();
            foreach (var key in Keys)
            {
                var newKey = transformer.TransformKey(key);
                result[newKey] = this[key];
            }

            return result;
        }
    }
}