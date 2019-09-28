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
    }
}