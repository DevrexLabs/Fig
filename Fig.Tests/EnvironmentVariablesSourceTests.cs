using System.Collections.Generic;
using NUnit.Framework;

namespace Fig.Test
{
    public class EnvironmentVariablesSourceTests
    {
        [Test]
        public void WithDroppedPrefix()
        {
            var dictionary = new Dictionary<string, string>()
            {
                ["FIG_A"] = "fig.a",

            };
            var source = new EnvironmentVariablesSource(dictionary, "FIG_", dropPrefix: true);
            var settings = source.ToSettingsDictionary();
            Assert.True(settings.ContainsKey("a"));
        }
        
        [Test]
        public void WithRetainedPrefix()
        {
            var dictionary = new Dictionary<string, string>()
            {
                ["FIG_A"] = "fig.a",

            };
            var source = new EnvironmentVariablesSource(dictionary, "FIG_", dropPrefix: false);
            var settings = source.ToSettingsDictionary();
            Assert.True(settings.ContainsKey("fig.a"));
        }
    }
}