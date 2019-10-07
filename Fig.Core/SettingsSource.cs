using System;
using System.Collections;
using System.Collections.Generic;

namespace Fig
{
    public abstract class SettingsSource : ISettingsSource
    {
        /// <summary>
        /// Regular expression to validate keys including path separator and environment selector
        /// One or more non whitespace characters
        /// </summary>
        public const string KeyMatcher = @"[a-z0-9.]+(:[a-z]+)?";

        /// <summary>
        /// Only retrieve settings where the key starts with the prefix. Case-insensitive
        /// </summary>
        public string Prefix { get; set; }

        public IEnumerator<(string, string)> GetEnumerator()
        {
            foreach (var (key,val) in GetSettings())
            {
                if (key.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    var strippedKey = key.Remove(0, Prefix.Length);
                    yield return (strippedKey, val);
                }
            }
        }

        protected abstract IEnumerable<(string, string)> GetSettings();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SettingsDictionary ToSettingsDictionary()
        {
            var result = new SettingsDictionary();
            foreach (var (key, val) in GetSettings()) result[key] = val;
            return result;
        }
    }
}