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
            var poco = _builder.Build().Bind<MyPoco>(prefix: "");
            Assert.AreEqual(true, poco.SipEnabled);
        }

        [Test]
        public void CanBindToSettingsSubTypeWithEmptyPrefix()
        {
            var settings = _builder.Build<MySettings>(prefix: "");
            Assert.AreEqual(true, settings.SipEnabled);
        }

        [Test]
        public void CanBindToPocoWithAlternatePrefix()
        {
            var poco = _builder.Build().Bind<MyPoco>(prefix: "MyPrefix");
            Assert.AreEqual(false, poco.SipEnabled);
        }

        [Test]
        public void CanBindToSettingsSubTypeWithAlternatePrefix()
        {
            var settings = _builder.Build<MySettings>(prefix: "MyPrefix");
            Assert.AreEqual(false, settings.SipEnabled);
        }

        private class MySettings : Settings
        {
            public bool SipEnabled => Get<bool>();
        }

        private class MyPoco
        {
            public bool SipEnabled { get; set; }
        }
    }
}