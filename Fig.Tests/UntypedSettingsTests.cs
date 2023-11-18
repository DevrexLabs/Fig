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
                    ["a.b"] = "a.b",
                })
                .Build();
        }

        [TestCase("${A}", ExpectedResult = "A", Description = "Single key")]
        [TestCase("${A}${A}", ExpectedResult = "AA", Description = "Duplicate keys")]
        [TestCase("${A}${D}", ExpectedResult = "A", Description = "Mixed present and missing")]
        [TestCase("${A}${B}", ExpectedResult = "AB", Description = "Multiple keys")]
        [TestCase("${a.b}", ExpectedResult = "a.b", Description = "Composite keys")]

        public string CanExpandVariables(string template)
        {
            return _settings.ExpandVariables(template);
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