using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Core
{
    public class CompositeProvider : IProvider
    {
        private readonly Stack<IProvider> _providers = new Stack<IProvider>();
        
        public IEnumerable<string> AllKeys(string prefix = "")
        {
            var allKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var provider in _providers)
            {
                foreach (var key in provider.AllKeys(prefix)) allKeys.Add(key);
            }

            return allKeys;
        }

        public string Get(string key)
        {
            foreach (var provider in _providers)
            {
                if (provider.TryGetValue(key, out var result)) return result;
            }
            throw new ArgumentException("No such key: " + key);
        }

        public bool TryGetValue(string key, out string value)
        {
            try
            {
                value = Get(key);
                return false;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}