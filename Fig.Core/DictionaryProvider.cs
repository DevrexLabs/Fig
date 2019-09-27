using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig
{

    /// <summary>
    /// Provider that wraps a plain old Dictionary.
    /// Useful for testing
    /// </summary>
    public class DictionaryProvider : Provider
    {
        Dictionary<string, string> _data
            = new Dictionary<string, string>(
                StringComparer.InvariantCultureIgnoreCase);

        public IEnumerable<string> AllKeys(string prefix = "")
            => _data.Keys.Where(key => key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));

        public bool TryGetValue(string key, out string value)
            => _data.TryGetValue(key, out value);

        public string Get(string key)
        {
            if (TryGetValue(key, out var result)) return result;
            throw new KeyNotFoundException(key);
        }
    }
}
