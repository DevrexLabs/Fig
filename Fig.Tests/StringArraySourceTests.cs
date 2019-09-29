using NUnit.Framework;

namespace Fig.Test
{
    public class StringArraySourceTests
    {
        private SettingsDictionary _dictionary;

        [SetUp]
        public void Setup()
        {
            var args = new[]
            {
                "--fig:Key1=Value1",
                "--fig:Key2=Value2",
                "--FIG:key3=value3"
            };
            _dictionary = new StringArraySource(args, "--fig:", '=')
                .ToSettingsDictionary();
        }
        
        [Test]
        public void CanGetKey()
        {
            var found = _dictionary.TryGetValue("key1", out var val);
            Assert.IsTrue(found);
            Assert.AreEqual("Value1", val);
        }
        
        [Test]
        public void PrefixIsCaseInsensitive()
        {
            var found = _dictionary.TryGetValue("key3", out var val);
            Assert.IsTrue(found);
            Assert.AreEqual("value3", val);
        }

    }
}