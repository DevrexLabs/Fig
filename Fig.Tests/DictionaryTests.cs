using System;
using NUnit.Framework;

namespace Fig.Test
{
    public class DictionaryTests
    {
        [Test]
        public void KeyInLowerLayerHidesOtherKeys()
        {
            var cd = new LayeredSettingsDictionary();
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

            bool found = cd.TryGetValue("a", out var result);
            Assert.IsTrue(found);
            Assert.AreEqual("b", result);
        }
        
        [Test]
        public void CanExpandVariables()
        {
            var cd = new LayeredSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "a",
                ["profile"] = "test",
                ["a:prod"] = "b"
            };
            cd.Add(first);

            var actual = cd.ExpandVariables("appSettings.${Profile}.json");
            Assert.AreEqual("appSettings.test.json", actual);

        }
        
        [Test]
        public void AsString()
        {
            var cd = new LayeredSettingsDictionary();
            var first = new SettingsDictionary()
            {
                ["a"] = "b",
            };
            var second = new SettingsDictionary()
            {
                ["c"] = "d"
            };
            cd.Add(first);
            cd.Add(second);
            
            var settings = new Settings(cd);
            var toString = settings.ToString();
            
            Console.WriteLine(toString);

            var expected =   "-------------------- Layer 0 ----------------------" + Environment.NewLine
                           + "| c                      | d                      |" + Environment.NewLine
                           + "-------------------- Layer 1 ----------------------" + Environment.NewLine
                           + "| a                      | b                      |" + Environment.NewLine
                           + "---------------------------------------------------" + Environment.NewLine;

            Assert.AreEqual(expected, toString);
        }

        [Test]
        public void EmptyCompositeDictionaryAsStringReturnsCorrectResult()
        {
            var settingsString = new LayeredSettingsDictionary()
                .AsString();
            Assert.AreEqual(string.Empty, settingsString);
        }
    }
}