using System;
using NUnit.Framework;

namespace Fig.Test
{
    public class DictionaryTests
    {
        [Test]
        public void KeyInLowerLayerHidesOtherKeys()
        {
            var cd = new CompositeSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "a"
            };
            var second = new SettingsDictionary()
            {
                ["a"] = "b"
            };
            cd.Add(first);
            cd.Add(second);

            bool found = cd.TryGetValue("a", null, out var result);
            Assert.IsTrue(found);
            Assert.AreEqual("a", result);
        }
        
        [Test]
        public void UnqualifiedKeyInLowerLayerHidesQualifiedKeysInLayersAbove()
        {
            var cd = new CompositeSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "a"
            };
            var second = new SettingsDictionary()
            {
                ["a:prod"] = "b"
            };
            cd.Add(first);
            cd.Add(second);

            bool found = cd.TryGetValue("a", "prod", out var result);
            Assert.IsTrue(found);
            Assert.AreEqual("a", result);
        }

        [Test]
        public void CanExpandVariables()
        {
            var cd = new CompositeSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "a",
                ["env"] = "test",
                ["a:prod"] = "b"
            };
            cd.Add(first);

            var actual = cd.ExpandVariables("appSettings.${ENV}.json");
            Assert.AreEqual("appSettings.test.json", actual);

        }

        [Test]
        public void QualifiedKeyInSameLayerHasPrecedence()
        {
            var cd = new CompositeSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "a",
                ["a:prod"] = "b"
            };
            cd.Add(first);

            bool found = cd.TryGetValue("a", "prod", out var result);
            Assert.IsTrue(found);
            Assert.AreEqual("b", result);
        }

        [Test]
        public void AsString()
        {
            var cd = new CompositeSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "b",
            };
            var second = new SettingsDictionary()
            {
                ["c:prod"] = "d"
            };
            cd.Add(first);
            cd.Add(second);
            
            var settings = new Settings(cd);
            var toString = settings.ToString();
            
            Console.WriteLine(toString);

            var expected =   "-------------------- Layer 0 ----------------------" + Environment.NewLine
                           + "| a                      | b                      |" + Environment.NewLine
                           + "-------------------- Layer 1 ----------------------" + Environment.NewLine
                           + "| c:prod                 | d                      |" + Environment.NewLine
                           + "---------------------------------------------------" + Environment.NewLine;

            Assert.AreEqual(expected, toString);
        }

        [Test]
        public void EmptyCompositeDictionaryAsStringReturnsCorrectResult()
        {
            var settingsString = new CompositeSettingsDictionary()
                .AsString();
            Assert.AreEqual(string.Empty, settingsString);
        }

        [Test]
        public void KeyTransformation()
        {
            var dict = new SettingsDictionary()
            {
                [":prod.a"] = "a:prod",
                ["a:prod.b"] = "a.b:prod",
                ["a.b:prod.c"] = "a.b.c:prod",
                ["a.b.c:prod"] = "a.b.c:prod",
                ["a.b"] = "a.b",
                ["a"] = "a"
            };

            var transformed = dict.WithNormalizedEnvironmentQualifiers();
            foreach (var key in transformed.Keys)
            {
                Assert.AreEqual(key, transformed[key]);
            }
        }
    }
}