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

        public override IEnumerable<string> AllKeys(string prefix = "")
            => _data.Keys.Where(key => key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));

        public override bool TryGetValue(string key, out string value)
            => _data.TryGetValue(key, out value);

        public override string Get(string key)
        {
            if (TryGetValue(key, out var result)) return result;
            throw new KeyNotFoundException(key);
        }

    }
}
