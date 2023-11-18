using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fig
{
    /// <summary>
    /// A sequence of dictionaries where keys from layers with lower index
    /// hide keys in the layers with higher indexes.
    /// </summary>
    internal class LayeredSettingsDictionary
    {
        private readonly List<SettingsDictionary> _dictionaries 
            = new List<SettingsDictionary>();

        public void Add(SettingsDictionary settingsDictionary) 
            => _dictionaries.Insert(0, settingsDictionary);

        public bool TryGetValue(string key, string profile, out string value)
        {
            value = null;
            var suffix = "";
            if (!String.IsNullOrEmpty(profile) && !key.Contains(":")) suffix = ":" + profile;

            foreach (var sd in _dictionaries)
            {
                if (suffix != "" && sd.TryGetValue(key + suffix, out value)) break;
                if (sd.TryGetValue(key, out value)) break;
                if (ConcatIndices(sd, key, out value)) break;
            }

            return value != null;
        }

        /// <summary>
        /// Look for ${key} and replace with values from the dictionary 
        /// </summary>
        /// <param name="template">The string to expand</param>
        /// <param name="explicitProfile">A specific configuration to use if key is unqualified</param>
        /// <returns>the resulting string or an exception if key is missing</returns>
        internal string ExpandVariables(string template, string explicitProfile = null)
        {
            var pattern = "\\$\\{\\s*(?<key>[a-z0-9.]+)(:(?<profile>[a-z]+))?\\s*\\}";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return regex.Replace(template, m => GetCurrentProfile(m, explicitProfile));
        }

        public string AsString()
        {
            var maxWidths = new int[2];
            maxWidths[0] = _dictionaries.SelectMany(d => d.Keys).MaxOrDefault(k => k.Length) + 1;
            maxWidths[1] = _dictionaries.SelectMany(d => d.Values).MaxOrDefault(k => k.Length);
            var minWidth = 22;
            var colWidths = new []
            {
                Math.Max(minWidth, maxWidths[0]),
                Math.Max(minWidth, maxWidths[1])
            };
            
            var sb = new StringBuilder();
            for (var layer = 0; layer < _dictionaries.Count; layer++)
            {
                var header = (" Layer " + layer + " ")
                    .PadLeft(colWidths[0] + 7, '-')
                    .PadRight(colWidths[0] + 7 + colWidths[1], '-');

                sb.AppendLine(header);
                foreach (var pair in _dictionaries[layer])
                {
                    sb.Append("| ");
                    var keyWithPadding = pair.Key.PadRight(colWidths[0]);
                    sb.Append(keyWithPadding);
                    sb.Append(" | ");
                    var valWithPadding = pair.Value.PadRight(colWidths[1]);
                    sb.Append(valWithPadding);
                    sb.AppendLine(" |");
                }
            }

            // Only append a line, if there was data in the dictionary.
            if (_dictionaries.Count > 0)
            {
                sb.AppendLine("-".PadRight(colWidths.Sum(w => w) + 7, '-'));
            }

            return sb.ToString();
        }

        private string GetCurrentProfile(Match m, string configuration)
        {
            var key = m.Groups["key"]. Value;
            var profile = m.Groups["profile"].Success ? m.Groups["profile"].Value : configuration;
            TryGetValue(key, profile, out var result);
            return result;
        }

        private bool ConcatIndices(SettingsDictionary sd, string key, out string value)
        {
            value = null;

            var indices = new List<string>();
            while (sd.TryGetValue(key + "." + indices.Count, out value))
            {
                indices.Add(value);
            }

            if (indices.Count > 0)
            {
                value = string.Join(",", indices);
                return true;
            }
            return false;
        }
    }
}