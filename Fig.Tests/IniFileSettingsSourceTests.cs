using NUnit.Framework;

namespace Fig.Test
{
    /// <summary>
    /// Tests to prove that INI Files are correctly parsed
    /// </summary>
    public class IniFileSettingsSourceTests
    {
        private IniFileSettingsSource _iniFileSettingsSource;

        [SetUp]
        public void Setup()
        {
            _iniFileSettingsSource = new IniFileSettingsSource(new [] {
                "key1=This is a string with spaces",
                "[Section.A]",
                "ScanInterval=00:25:00",
                "#comment",
                "[OtherSection]",
                "key3=300",
                "", //empty line
                "[:PROD]",
                "root=root:PROD",
                "[SectionWithEnv:PROD]",
                "a=SectionWithEnv.a:PROD",
                "b=SectionWithEnv.b:PROD",
                "c.d=SectionWithEnv.c.d:PROD"
            });
        }

        [Test]
        public void IgnoreComments()
        {
            var settingsDictionary = _iniFileSettingsSource.ToSettingsDictionary();
            var getResult = settingsDictionary.ContainsKey("#comment");

            Assert.IsFalse(getResult);
        }

        [Test]
        public void KeyUnderNoSectionReturnsValue()
        {
            var expectedValue = "This is a string with spaces";
            var settingsDictionary = _iniFileSettingsSource.ToSettingsDictionary();
            var getResult = settingsDictionary.TryGetValue("key1", out var actualValue);

            Assert.IsTrue(getResult);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void SectionAndKeyUnderSectionReturnsValue()
        {
            var expectedValue = "300";
            var settingsDictionary = _iniFileSettingsSource.ToSettingsDictionary();
            var getResult = settingsDictionary.TryGetValue("OtherSection.key3", out var actualValue);

            Assert.IsTrue(getResult);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void SectionWithDotAndKeyUnderSectionReturnsValue()
        {
            var expectedValue = "00:25:00";
            var settingsDictionary = _iniFileSettingsSource.ToSettingsDictionary();
            var getResult = settingsDictionary.TryGetValue("Section.A.ScanInterval", out var actualValue);

            Assert.IsTrue(getResult);
            Assert.AreEqual(expectedValue, actualValue);
        }
        
        [Test]
        public void SectionWithEnvironmentQualifier()
        {
            var dict = _iniFileSettingsSource
                .ToSettingsDictionary()
                .WithNormalizedProfileQualifiers();
            
            Assert.AreEqual("root:PROD", dict["root:PROD"]);

            var keys = new[]
            {
                "SectionWithEnv.a:PROD",
                "SectionWithEnv.b:PROD",
                "SectionWithEnv.c.d:PROD",
                "root:PROD"
            };

            foreach (var key in keys)
            {
                Assert.AreEqual(key, dict[key]);
            }
        }
    }
}