using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fig.Core
{
    public class IniFileSettingsSource : SettingsSource
    {

        /// <summary>
        /// Path to the file to read
        /// </summary>
        private string _path;

        public IniFileSettingsSource(string path)
        {
            _path = path;
        }
        
        internal static IEnumerable<(string, string)> Parse(IEnumerable<string> lines)
        {
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
                    yield return (key, value);
                }
                else
                {
                    throw new Exception("Invalid ini file line: " + line);
                }
            }
        }

        private static string PrependSection(string section, string key)
        {
            if (!string.IsNullOrEmpty(section))
            {
                key = section + ":" + key;
            }
            return key;
        }



        protected override IEnumerable<(string, string)> GetSettings()
        {
            var lines = File.ReadAllLines(_path);
            foreach (var (key, value) in Parse(lines))
                yield return (key, value);

        }
    }
}