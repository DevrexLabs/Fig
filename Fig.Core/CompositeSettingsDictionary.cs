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
    internal class CompositeSettingsDictionary
    {
        private readonly List<SettingsDictionary> _dictionaries;

        public CompositeSettingsDictionary()
        {
            _dictionaries = new List<SettingsDictionary>();
        }

        public void Add(SettingsDictionary settingsDictionary) 
            => _dictionaries.Add(settingsDictionary);

        public bool TryGetValue(string key, string env, out string value)
        {
            value = null;
            var suffix = "";
            if (!String.IsNullOrEmpty(env) && !key.Contains(":")) suffix = ":" + env;

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
        /// <param name="configuration">A specific configuration to use if key is unqualified</param>
        /// <returns>the resulting string or an exception if key is missing</returns>
        public string ExpandVariables(string template, string configuration = null)
        {
            var pattern = "\\$\\{\\s*(?<key>[a-z0-9.]+)(:(?<env>[a-z]+))?\\s*\\}";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return regex.Replace(template, m => GetCurrentEnvironment(m, configuration));
        }

        public string AsString()
        {
            var maxWidths = new int[2];
            maxWidths[0] = _dictionaries.SelectMany(d => d.Keys).MaxOrDefault(k => k.Length) + 1;
            maxWidths[1] = _dictionaries.SelectMany(d => d.Values).MaxOrDefault(k => k.Length);
            var minWidth = 22;
            var colWidths = new int[2]
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

        private string GetCurrentEnvironment(Match m, string configuration)
        {
            var key = m.Groups["key"]. Value;
            var env = m.Groups["env"].Success ? m.Groups["env"].Value : configuration;
            TryGetValue(key, env, out var result);
            return result;
        }

        private bool ConcatIndices(SettingsDictionary sd, string key, out string value)
        {
            var indices = new List<string>();
            while (sd.TryGetValue(key + "." + indices.Count, out value))
            {
                indices.Add(value);
            }
            value = indices.Count == 0 ? null : string.Join(",", indices);
            return value != null;
        }
    }
}