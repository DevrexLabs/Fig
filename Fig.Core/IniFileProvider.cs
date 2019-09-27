using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fig.Core
{
    public class IniFileProvider : IProvider
    {
        string[] _lines;
        Dictionary<string, string> _data;

        public IniFileProvider(string file)
        {
            _lines = File.ReadAllLines(file);
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            _data = new Dictionary<string, string>(comparer);
            foreach (var (key,value) in Parse(_lines)) _data[key] = value;
        }
        
        internal static List<(string, string)> Parse(IEnumerable<string> lines)
        {
            //list instead of dictionary because
            //we need them in order in case of duplicates
            var result = new List<(string, string)>();

            var sectionMatcher = new Regex(@"^\[\s*(?<section>[A-Z0-9:]+)\s*\]$", RegexOptions.IgnoreCase);
            var keyValueMatcher = new Regex(@"^(?<key>[A-Z0-9:]+)\s*=\s*(?<value>.*)$", RegexOptions.IgnoreCase);

            //Last section matched
            var currentSection = "";
            foreach (var line in lines.Select(s => s.Trim()))
            {
                //Skip comments and empty lines
                if (string.IsNullOrEmpty(line) || line[0] == '#') continue;

                if (sectionMatcher.TryMatch(line, out var sectionMatch))
                {
                    currentSection = sectionMatch.Groups["section"].Value;
                }
                else if (keyValueMatcher.TryMatch(line, out var kvpMatch))
                {
                    var localKey = kvpMatch.Groups["key"].Value;
                    var key = PrependSection(currentSection, localKey);
                    var value = kvpMatch.Groups["value"].Value;
                    result.Add((key, value));
                }
                else
                {
                    throw new Exception("Invalid ini file line: " + line);
                }
            }
            return result;
        }

        private static string PrependSection(string section, string key)
        {
            if (!string.IsNullOrEmpty(section))
            {
                key = section + ":" + key;
            }
            return key;
        }

        public bool TryGetValue(string key, out string value)
        {
            return _data.TryGetValue(key, out value);
        }

        public IEnumerable<string> AllKeys(string prefix = "")
        {
            return _data.Keys;
        }

        public string Get(string key)
        {
            return _data[key];
        }
    }
}