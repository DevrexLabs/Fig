using System;
using System.Collections.Specialized;
using Fig.AppSettingsXml;
using NUnit.Framework;

namespace Tests
{
    public class XmlProviderTests
    {
        AppSettingsXml _xmlProvider;

        [SetUp]
        public void Setup()
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var nvc = new NameValueCollection(comparer);
            nvc.Add("Timeout", "42");
            nvc.Add("ConnectionStrings.DefaultConnection",
                "DataSource=app.db");
            nvc.Add("Servers.0", "10.0.0.1");
            nvc.Add("Servers.1", "10.0.0.2");
            _xmlProvider = new AppSettingsXml(nvc);
        }

        [Test]
        public void CanReadPrimitive()
        {
            var actual = _xmlProvider.Get("Timeout");
            Assert.AreEqual("42", actual);
        }


        [Test]
        public void CanReadNestedPrimitive()
        {
            var actual = _xmlProvider.Get("ConnectionStrings.DefaultConnection");
            Assert.AreEqual("DataSource=app.db", actual);
        }

        [Test]
        public void CanReadArray()
        {
            var actual = _xmlProvider.Get("Servers.0");
            Assert.AreEqual("10.0.0.1", actual);
            Assert.AreEqual("10.0.0.2", _xmlProvider.Get("Servers.1"));
        }
    }
}