using NUnit.Framework;

namespace Fig.Test
{
    public class PrefixTests
    {
        private SettingsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new SettingsBuilder().UseSettingsDictionary(
                new SettingsDictionary()
                {
                    ["SipEnabled"] = "true",
                    ["MyPrefix.SipEnabled"] = "false"
                });
        }

        [Test]
        public void CanBindToPocoWithEmptyPrefix()
        {
            var poco = _builder.Build().Bind<MyPoco>(path: "");
            Assert.AreEqual(true, poco.SipEnabled);
        }

        [Test]
        public void CanBindToPocoWithAlternatePrefix()
        {
            var poco = _builder.Build().Bind<MyPoco>(path: "MyPrefix");
            Assert.AreEqual(false, poco.SipEnabled);
        }

        private class MyPoco
        {
            public bool SipEnabled { get; set; }
        }
    }
}