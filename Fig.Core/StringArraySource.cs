using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Fig
{
    public class StringArraySource : SettingsSource
    {
        private readonly Regex _parser;
        private readonly string[] _args;
        
        public StringArraySource(string[] args, string prefix, char delimiter)
        {
            _args = args;
            var pattern = 
                "^" + 
                Regex.Escape(prefix) + 
                "(?<key>[a-z]+[-a-z0-9_.]*)" + 
                delimiter + 
                @"(?<val>\w+)$";
            _parser = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        protected override IEnumerable<(string, string)> GetSettings()
        {
            foreach (var arg in _args)
            {
                var match = _parser.Match(arg);
                if (match.Success)
                {
                    var key = match.Groups["key"].Value;
                    var val = match.Groups["val"].Value;
                    yield return (key, val);
                }
            }
        }
    }
}