using System.Collections.Generic;

namespace Fig
{
    /// <summary>
    /// Base class for providers. Could probably be an interface?
    /// </summary>
    public abstract class Provider
    {
        public abstract bool TryGetValue(string key, out string value);

        /// <summary>
        /// Return all the keys in the 
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> AllKeys(string prefix = "");

        /// <summary>
        /// Retrieve the value associated with a given key
        /// </summary>
        /// <param name="key">Case insensitive key</param>
        /// <returns>the associated value or throw a KeyNotFoundException</returns>
        public abstract string Get(string key);
    }
}
