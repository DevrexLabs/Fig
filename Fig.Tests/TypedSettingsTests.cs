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

            Assert.Throws<ConfigurationException>(() => _settings.Bind<TestClass>());
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
        public void WhenBindWithNestedClassPropertyReturnsValue()
        {
            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    [$"{nameof(TestClassWithNestedClass)}.{nameof(TestClassWithNestedClass.Name)}"] = "Fullname",
                    [$"{nameof(TestClassWithNestedClass)}.{nameof(TestClassWithNestedClass.Flags)}.{nameof(TestClassWithNestedClass.Flags.FlagOne)}"]
                        = "false",
                    [$"{nameof(TestClassWithNestedClass)}.{nameof(TestClassWithNestedClass.Flags)}.{nameof(TestClassWithNestedClass.Flags.TestEnum)}"]
                        = "Two"
                })
                .Build();

            var testClass = _settings.Bind<TestClassWithNestedClass>(false);

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(false, testClass.Flags.FlagOne);
            Assert.AreEqual(false, testClass.Flags.FlagTwo);
            Assert.AreEqual(NestedEnum.Two, testClass.Flags.TestEnum);
        }

        [Test]
        public void WhenEnvironmentSetAndBindReturnsValue()
        {
            var environment = "TEST";
            var propertyName = $"{nameof(TestClass)}.{nameof(TestClass.Name)}";
            var expectedValue = "FullnameTest";


            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    [$"{propertyName}"] = "Fullname",
                    [$"{propertyName}:{environment}"] = expectedValue
                })
                .Build();

            _settings.SetEnvironment(environment);

            var testClass = _settings.Bind<TestClass>(false);

            Assert.AreEqual(expectedValue, testClass.Name);
        }

        [Test]
        public void WhenEnvironmentNotSetAndBindReturnsValue()
        {
            var environment = "TEST";
            var propertyName = $"{nameof(TestClass)}.{nameof(TestClass.Name)}";
            var expectedValue = "Fullname";


            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    [$"{propertyName}"] = expectedValue,
                    [$"{propertyName}:{environment}"] = "FullnameTest"
                })
                .Build();

            var testClass = _settings.Bind<TestClass>(false);

            Assert.AreEqual(expectedValue, testClass.Name);
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

        private class TestClassWithNestedClass
        {
            public string Name { get; set; }
            public NestedClass Flags { get; set; }
        }

        private class NestedClass
        {
            public bool FlagOne { get; set; } = true;
            public bool FlagTwo { get; set; } = false;
            public NestedEnum TestEnum { get; set; } = NestedEnum.One;
        }

        private enum NestedEnum
        {
            One = 1,
            Two = 2
        }
    }
}