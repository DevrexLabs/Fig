using System.IO;
using Fig;
using NUnit.Framework;

namespace Tests
{
    public class JsonProviderTests
    {
        AppSettingsJsonProvider _jsonProvider;

        [SetUp]
        public void Setup()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            _jsonProvider = new AppSettingsJsonProvider(path);
        }

        [Test]
        public void CanReadPrimitive()
        {
            var actual = _jsonProvider.Get("Timeout");
            Assert.AreEqual("42", actual);
        }


        [Test]
        public void CanReadNestedPrimitive()
        {
            var actual = _jsonProvider.Get("ConnectionStrings.DefaultConnection");
            Assert.AreEqual("DataSource=app.db", actual);
        }

        [Test]
        public void CanReadArray()
        {
            var actual = _jsonProvider.Get("Servers.0");
            Assert.AreEqual("10.0.0.1", actual);
            Assert.AreEqual("10.0.0.2", _jsonProvider.Get("Servers.1"));
        }

        [Test]
        public void CanReadArrayOfObjects()
        {
            var actual = _jsonProvider.Get("Simpsons.0.Name");
            Assert.AreEqual("Bart", actual);
            Assert.AreEqual("Homer", _jsonProvider.Get("simpsons.1.name"));
        }

    }
}