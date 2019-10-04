using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Fig.Test
{
    public class TypedSettingsTests
    {
        private Settings _settings;

        [SetUp]
        public void Setup()
        {
            this._settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary() {
                    [nameof(TestClass.Name)] = "Fullname",
                    [nameof(TestClass.Age)] = "40",
                    [nameof(TestClass.Pi)] = "3.14"
                })
                .Build();
        }

        [Test]
        public void WhenBindReturnsValue()
        {
            var testClass = this._settings.Bind<TestClass>();

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(40, testClass.Age);
            Assert.AreEqual(3.14, testClass.Pi);
        }

        [Test]
        public void WhenBindAndMissingSettingsPropertyThrowsException()
        {
            this._settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary() {
                    [nameof(TestClass.Name)] = "Fullname"
                })
                .Build();

            TestClass testClass;

            Assert.Throws<KeyNotFoundException>(() => testClass = this._settings.Bind<TestClass>());
        }

        [Test]
        public void WhenBindWithAllPropertiesNotRequiredAndMissingSettingsPropertyThrowsException()
        {
            this._settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary() {
                    [nameof(TestClass.Name)] = "Fullname"
                })
                .Build();

            var testClass = this._settings.Bind<TestClass>(false);

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(0, testClass.Age);
            Assert.AreEqual(0, testClass.Pi);
        }

        private class TestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public decimal Pi { get; set; }
        }
    }
}