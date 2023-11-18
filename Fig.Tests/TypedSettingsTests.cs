using System.Collections;
using NUnit.Framework;

namespace Fig.Test
{
    public class TypedSettingsTests
    {
        private Settings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname",
                    [$"{nameof(TestClass)}.{nameof(TestClass.Age)}"] = "40",
                    [$"{nameof(TestClass)}.{nameof(TestClass.Pi)}"] = "3.14"
                })
                .Build();
        }

        [Test]
        public void WhenBindReturnsValue()
        {
            var testClass = _settings.Bind<TestClass>();

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(40, testClass.Age);
            Assert.AreEqual(3.14, testClass.Pi);
        }

        [Test]
        public void WhenBindAndMissingSettingsPropertyThrowsException()
        {
            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname"
                })
                .Build();

            Assert.Throws<ConfigurationException>(() => _settings.Bind<TestClass>(requireAll:true));
        }

        [Test]
        public void CanBindWhenPropertiesMissingAndNotRequireAll()
        {
            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname"
                })
                .Build();

            var testClass = _settings.Bind<TestClass>(requireAll: false);

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(0, testClass.Age);
            Assert.AreEqual(0, testClass.Pi);
        }
        
        
        [Test]
        public void WhenBindAbstractPropertyTypeAreIgnored()
        {
            _settings = new SettingsBuilder().Build();
            Assert.DoesNotThrow(() =>_settings.Bind<ClassWithAbstractPropertyTypes>());
        }

        private abstract class AbstractClass {}

        private class ClassWithAbstractPropertyTypes
        {
            public AbstractClass AbstractProperty { get; set; }
            public IList List { get; set; }
        }

        private class TestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public decimal Pi { get; set; }
            public int Zero { get; } = 0;
        }
        
    }
}