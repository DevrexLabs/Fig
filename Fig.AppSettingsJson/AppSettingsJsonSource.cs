using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;

namespace Fig
{
    public class AppSettingsJsonSource : SettingsSource
    {
        /// <summary>
        /// Path separator used in the output
        /// </summary>
        private readonly string _separator = ".";

        private readonly string _path;

        private Dictionary<string, string> _data;

        /// <summary>
        /// Create an instance of AppSettingsJsonSource
        /// </summary>
        /// <param name="path">Path to the json file to load</param>
        public AppSettingsJsonSource(string path)
        {
            _path = path;
        }
        
        private void Load(JsonValue node, Stack<string> path = null)
        {
            path = path ?? new Stack<string>();
            if (node is JsonObject jo)
            {
                foreach (KeyValuePair<string, JsonValue> kvp in jo)
                {
                    path.Push(kvp.Key);
                    Load(kvp.Value, path);
                    path.Pop();
                }
            }
            if (node is JsonArray ja)
            {
                for(int i = 0; i < ja.Count; i++)
                {
                    path.Push(i.ToString());
                    Load(ja[i], path);
                    path.Pop();
                }
            }
            if (node is JsonPrimitive jp)
            {
                var value = jp.ToString();
                if (jp.JsonType == JsonType.String)
                {
                    //Remove the leading and trailing quotes
                    value = value.Substring(1, value.Length - 2);
                }
                var key = string.Join(_separator, path.Reverse());
                _data[key] = value;
            }
        }

        protected override IEnumerable<(string, string)> GetSettings()
        {
            var text = File.ReadAllText(_path);
            var root = (JsonObject)JsonValue.Parse(text);
            _data = new Dictionary<string,string>();
            Load(root);
            foreach (var kvp in _data) 
                yield return (kvp.Key, kvp.Value);
        }
    }
}
