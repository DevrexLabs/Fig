using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;

namespace Fig
{
    public class AppSettingsJsonProvider : Provider
    {
        JsonObject _root;
        Dictionary<string, string> _data;

        /// <summary>
        /// Path separator for nested properties and array indicies
        /// </summary>
        private string _separator = ".";

        public AppSettingsJsonProvider(string path)
        {
            var text = File.ReadAllText(path);
            _root = (JsonObject)JsonValue.Parse(text);
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            _data = new Dictionary<string, string>(comparer);
            Load(_root);
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
                _data[key] = value;//.Substring(1, value.);
            }
        }

        public override IEnumerable<string> AllKeys(string prefix = "")
        {
            return _data.Keys;
        }

        public override string Get(string key)
        {
            return _data[key];
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _data.TryGetValue(key, out value);
        }
    }
}
