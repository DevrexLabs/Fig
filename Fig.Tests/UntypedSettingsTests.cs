using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Fig.Test
{
    public class UntypedSettingsTests
    {
        private Settings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    ["CoffeeRefillInterval"] = "00:05:00",
                    ["A"] = "A",
                    ["B"] = "B",
                    ["b:prod"] = "b:prod",
                    ["a.b"] = "a.b",
                    ["a.b:test"] = "a.b:test",
                })
                .Build();
        }

        [TestCase("${A}", ExpectedResult = "A", Description = "Single key")]
        [TestCase("${A}${A}", ExpectedResult = "AA", Description = "Duplicate keys")]
        [TestCase("${A}${D}", ExpectedResult = "A", Description = "Mixed present and missing")]
        [TestCase("${A}${B}", ExpectedResult = "AB", Description = "Multiple keys")]
        [TestCase("${B:prod}", ExpectedResult = "b:prod", Description = "explicit configuration")]
        [TestCase("bumble ${B:prod}", ExpectedResult = "bumble b:prod", Description = "Mixed content")]
        [TestCase("${a.b}", ExpectedResult = "a.b", Description = "Composite keys")]
        [TestCase("${a.b:test}", ExpectedResult = "a.b:test", Description = "Composite key with configuration selector")]
        public string CanExpandVariables(string template)
        {
            return _settings.ExpandVariables(template);
        }

        [TestCase]
        public void KeyWithExplicitConfigurationTakesPrecedenceOverCurrentConfiguration()
        {
            //set current configuration to "fish"
            _settings.SetEnvironment("fish");

            //but explicitly ask for "test"
            Assert.AreEqual("a.b:test", _settings.ExpandVariables("${a.b:test}"), "_settings.ExpandVariables(key)");

            Assert.AreEqual("a.b:test", _settings.Get("a.b:test"), "_settings.Get(key)");
        }

        [Test]
        public void MissingKeyAndNoDefaultThrowsException()
        {
            Assert.Throws<KeyNotFoundException>(
                () => _settings.Get("ouch"));
        }

        [Test]
        public void SuppliedSettingTakesPrecedenceOverDefault()
        {
            var key = "CoffeeRefillInterval";

            //Notice how the return type is derived from the default callback
            var refillInterval = _settings.Get(key, () => TimeSpan.FromMinutes(20));

            //Expect the value from the dictionary, not the default
            Assert.AreEqual(TimeSpan.FromMinutes(5), refillInterval);
        }

        [Test]
        public void DefaultKicksInWhenSettingsIsMissing()
        {
            var expected = "foo";
            var actual = _settings.Get("missing", () => expected);
            Assert.AreEqual(actual, expected);
        }
    }
}