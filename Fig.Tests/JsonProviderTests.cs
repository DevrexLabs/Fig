﻿using System.IO;
using NUnit.Framework;

namespace Fig.Test
{
    [TestFixture]
    public class JsonProviderTests
    {
        private SettingsDictionary _settingsDictionary;

        [SetUp]
        public void Setup()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var source = new AppSettingsJsonSource(path);
            _settingsDictionary = source
                .ToSettingsDictionary();
        }

        [Test]
        public void SimpleKey()
        {
            Assert.AreEqual("42", _settingsDictionary["timeout"]);
        }

        [Test]
        public void CanReadNestedPrimitive()
        {
            var actual = _settingsDictionary["ConnectionStrings.DefaultConnection"];
            Assert.AreEqual("DataSource=app.db", actual);
        }

        [Test]
        public void CanReadArray()
        {
            Assert.AreEqual("10.0.0.1", _settingsDictionary["Servers.0"]);
            Assert.AreEqual("10.0.0.2", _settingsDictionary["Servers.1"]);
        }

        [Test]
        public void CanReadArrayOfObjects()
        {
            Assert.AreEqual("Bart", _settingsDictionary["Simpsons.0.Name"]);
            Assert.AreEqual("Homer", _settingsDictionary["Simpsons.1.Name"]);
            Assert.AreEqual("12", _settingsDictionary["Simpsons.0.age"]);
            Assert.AreEqual("35", _settingsDictionary["Simpsons.1.age"]);
        }

        [Test]
        public void CanGetArrayFromJsonArrayProperty()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build();

            var appSettings = settings.Bind<AppSettings>(path: "");
            Assert.IsTrue(appSettings.Servers is { Length: 2 });
            
            var servers = settings.Get<string[]>("Servers");
            Assert.AreEqual(2, servers.Length);
            Assert.AreEqual(new[] { "10.0.0.1", "10.0.0.2" }, servers);

            var serversCommaSeparated = settings.Get<string>("Servers");
            Assert.AreEqual("10.0.0.1,10.0.0.2", serversCommaSeparated);
        }

        private class AppSettings
        {
            public string[] Servers
            {
                get;
                // ReSharper disable once UnusedAutoPropertyAccessor.Local
                set;
            }
        }
    }
}