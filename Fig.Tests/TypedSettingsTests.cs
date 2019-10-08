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
                    [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname",
                    [$"{nameof(TestClass)}.{nameof(TestClass.Age)}"] = "40",
                    [$"{nameof(TestClass)}.{nameof(TestClass.Pi)}"] = "3.14"
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
                    [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname"
                })
                .Build();

            Assert.Throws<KeyNotFoundException>(() => this._settings.Bind<TestClass>());
        }

        [Test]
        public void WhenBindWithAllPropertiesNotRequiredAndMissingSettingsPropertyThrowsException()
        {
            this._settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary() {
                    [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname"
                })
                .Build();

            var testClass = this._settings.Bind<TestClass>(false);

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(0, testClass.Age);
            Assert.AreEqual(0, testClass.Pi);
        }

        [Test]
        public void WhenBindWithNestedClassPropertyReturnsValue()
        {
            this._settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary() {
                    [$"{nameof(TestClassWithNestedClass)}.{nameof(TestClassWithNestedClass.Name)}"] = "Fullname",
                    [$"{nameof(TestClassWithNestedClass)}.{nameof(TestClassWithNestedClass.Flags)}.{nameof(TestClassWithNestedClass.Flags.FlagOne)}"] = "false",
                    [$"{nameof(TestClassWithNestedClass)}.{nameof(TestClassWithNestedClass.Flags)}.{nameof(TestClassWithNestedClass.Flags.TestEnum)}"] = "Two"
                })
                .Build();

            var testClass = this._settings.Bind<TestClassWithNestedClass>(false);

            Assert.AreEqual("Fullname", testClass.Name);
            Assert.AreEqual(false, testClass.Flags.FlagOne);
            Assert.AreEqual(false, testClass.Flags.FlagTwo);
            Assert.AreEqual(NestedEnum.Two, testClass.Flags.TestEnum);
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

        private enum NestedEnum {
            One = 1,
            Two = 2
        }
    }
}