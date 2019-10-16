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
        /// Lines of the read file
        /// </summary>
        private readonly string[] _lines;

        public IniFileSettingsSource(string path) : this(File.ReadAllLines(path)) { }

        internal IniFileSettingsSource(string[] lines)
        {
            _lines = lines;
        }

        internal static IEnumerable<(string, string)> Parse(IEnumerable<string> lines)
        {
            var sectionMatcher = new Regex(@"^\[(.+)\]$", RegexOptions.IgnoreCase);
            var keyValueMatcher = new Regex(@"^\s*(    .+?)\s*=\s*(.*)$", RegexOptions.IgnoreCase);

            //Last section matched
            var currentSection = "";
            foreach (var line in lines.Select(s => s.Trim()))
            {
                //Skip comments and empty lines
                if (string.IsNullOrEmpty(line) || line[0] == '#') continue;

                if (sectionMatcher.TryMatch(line, out var sectionMatch))
                {
                    currentSection = sectionMatch.Groups[1].Value?.Trim();
                }
                else if (keyValueMatcher.TryMatch(line, out var kvpMatch))
                {
                    var localKey = kvpMatch.Groups[1].Value?.Trim();
                    var key = PrependSection(currentSection, localKey);
                    var value = kvpMatch.Groups[2].Value?.Trim();
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
                key = section + "." + key;
            }
            return key;
        }



        protected override IEnumerable<(string, string)> GetSettings()
        {
            foreach (var (key, value) in Parse(_lines))
                yield return (key, value);

        }
    }
}