using System.Text.RegularExpressions;

namespace Fig
{
    
    /// <summary>
    /// Move environment qualifiers contained anywhere in the key to the end of the key
    /// </summary>
    internal class KeyTransformer
    {
        const string Pattern = "^(?<pre>.*)(?<env>:[a-z]+)(?<post>\\..+)$";
        private readonly Regex _regex;

        public KeyTransformer()
        {
            _regex = new Regex(Pattern, RegexOptions.IgnoreCase);
        }

        public string TransformKey(string key) 
            => _regex.Replace(key, HandleMatch);

        private static string HandleMatch(Match match)
        {
            var pre = match.Groups["pre"].Value;
            var env = match.Groups["env"].Value;
            var post = match.Groups["post"].Value;

            if (pre.Length == 0) post = post.Substring(1);
            return pre + post + env;
        }
    }
}