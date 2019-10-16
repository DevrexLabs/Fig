using NUnit.Framework;

namespace Fig.Test
{
    public class KeyTransformerTests
    {
        private KeyTransformer _transformer;

        [SetUp]
        public void Init()
        {
            _transformer = new KeyTransformer();
        }
        
        [Test]
        public void NoEnvironmentQuailifer()
        {
            string key = "A.B.C.D.E";
            Assert.AreEqual(key, _transformer.TransformKey(key));
        }

        [Test]
        public void ContainsEnvironmentQualifier()
        {
            string key = "A.B:PROD.C.D.E";
            Assert.AreEqual("A.B.C.D.E:PROD", _transformer.TransformKey(key));
        }
        
        [Test]
        public void BeginsWithEnvironmentQualifier()
        {
            string key = ":PROD.A.B.C.D.E";
            Assert.AreEqual("A.B.C.D.E:PROD", _transformer.TransformKey(key));
        }

        [Test]
        public void EndsWithEnvironmentQualifier()
        {
            string key = "A.B.C.D.E:PROD";
            Assert.AreEqual(key, _transformer.TransformKey(key));
        }

    }
}