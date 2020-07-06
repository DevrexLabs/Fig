using System.Collections.Generic;
using Fig.Core;
using NUnit.Framework;

namespace Fig.Test
{
    public class EnvironmentVarsSourceTests
    {
        [Test]
        public void WithDroppedPrefix()
        {
            var dictionary = new Dictionary<string, string>()
            {
                ["FIG_A"] = "fig.a",

            };
            var source = new EnvironmentVarsSettingsSource(dictionary, "FIG_", dropPrefix: true);
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
            var source = new EnvironmentVarsSettingsSource(dictionary, "FIG_", dropPrefix: false);
            var settings = source.ToSettingsDictionary();
            Assert.True(settings.ContainsKey("fig.a"));
        }
    }
}