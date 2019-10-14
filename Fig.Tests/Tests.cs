using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fig.AppSettingsXml;
using NUnit.Framework;

namespace Fig.Test
{
    public class Tests
    {
        private ExampleSettings _settings;
        private SettingsDictionary _settingsDictionary;

        [SetUp]
        public void Setup()
        {
            _settings = new ExampleSettings();

            //Normally the builder does all this stuff
            //but we want to have a direct reference to the dictionary
            _settingsDictionary = new SettingsDictionary()
            {
                ["ExampleSettings.MyIntProperty"] = "400",
                ["ExampleSettings.RequiredInt"] = "200",
                ["ExampleSettings.MyReadonlyIntProperty"] = "600",
                ["ExampleSettings.MyTimeSpan"] = "00:20:00",

                ["MyTimeSpan"] = "00:42:00",

                ["Key"] = "Key",
                ["Key:PROD"] = "PROD",
                ["Key:TEST"] = "TEST",
                ["Key:PROD2"] = "PROD",

                ["env"] = "staging",
                ["ExampleSettings.MyTimeSpan:staging"] = "00:15:00",

                ["ExampleSettings.MyTimeSpan:FAIL"] = "not a timespan"
            };

            _settings.SettingsDictionary = new CompositeSettingsDictionary();
            _settings.SettingsDictionary.Add(_settingsDictionary);

            _settings.PreLoad();
        }

        [Test]
        public void PropertyChangedFiresWhenPropertyChanges()
        {
            string propertyChangedName = null;

            //Arrange
            _settings.PropertyChanged += (s, ea) => propertyChangedName = ea.PropertyName;

            //Act
            _settings.MyIntProperty = 500;

            //Assert
            Assert.NotNull(propertyChangedName, "The event was not received");
            Assert.AreEqual(nameof(_settings.MyIntProperty), propertyChangedName);
        }

        [Test]
        public void CanReadDefault()
        {
            Assert.AreEqual(1234, _settings.HasDefault);
        }

        [Test]
        public void CanUpdateRuntimeValue()
        {
            _settings.MyIntProperty = 100;
            Assert.AreEqual(100, _settings.MyIntProperty);
        }

        [Test]
        public void MissingPropertyWithoutDefaultFailsValidation()
        {
            var builder = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary);

            Assert.Throws<ConfigurationException>(() =>
            {
                builder.Build<UnresolvedPropertySettings>();
            });
        }

        [Test]
        public void BuilderBonanza()
        {
            var settings = new SettingsBuilder()
                .UseCommandLine(new[] { "--fig:MySettings.DefaultBeverage=coffee" })
                .UseEnvironmentVariables(prefix: "FIG_")
                .BasePath(Directory.GetCurrentDirectory())
                .UseAppSettingsJson(fileNameTemplate: "appSettings.${CONFIG}.json", required: false)
                .UseAppSettingsJson(fileNameTemplate: "appSettings.json", required: false)
                .UseAppSettingsXml(prefix: "fig:", includeConnectionStrings: false)
                .UseIniFile("appSettings.${CONFIG}.ini", required: false)
                .UseIniFile("appSettings.ini", required: false)
                .Build();
        }

        [Test]
        public void BindingPath()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build<MySettings>(prefix:"");

            Assert.AreEqual(TimeSpan.FromMinutes(42), settings.MyTimeSpan);
        }

        [Test]
        public void EnvironmentIsRespected()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build();

            //No Configuration, should return unqualified setting
            Assert.AreEqual("Key", settings.Get<string>("Key"));

            settings.SetEnvironment("prod");
            Assert.AreEqual("PROD", settings.Get<string>("Key"));

            settings.SetEnvironment("test");
            Assert.AreEqual("TEST", settings.Get<string>("Key"));
        }

        [Test]
        public void InitialEnvironment()
        {
            var dictionary = new SettingsDictionary()
            {
                ["env"] = "staging",
                ["ExampleSettings.MyTimeSpan:staging"] = "00:15:00",
                ["ExampleSettings.MyTimeSpan"] = "00:10:00",
                ["ExampleSettings.RequiredInt"] = "200",
                ["ExampleSettings.MyReadonlyIntProperty"] = "200"
            };

            var settings = new SettingsBuilder()
                .UseSettingsDictionary(dictionary)
                .SetEnvironment("${ENV}")
                .Build<ExampleSettings>();

            Assert.AreEqual(TimeSpan.FromMinutes(15), settings.MyTimeSpan);
            Assert.AreEqual("staging", settings.Environment);
        }

        [Test]
        public void EnvironmentSwitchRollsback()
        {
            int propertiesChanged = 0;
            _settings.PropertyChanged += (sender, args) => propertiesChanged++;

            Assert.Throws<ConfigurationException>(
                () => _settings.SetEnvironment("fail")
            );

            //Nothing should have changed
            Assert.AreEqual("", _settings.Environment);
            Assert.AreEqual(0, propertiesChanged);
        }

        [Test]
        public void EnvironmentChangeFiresPropertyChangeEvents()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build<MySettings>(prefix:"");

            //Set up callback to record all property change notifications
            var propertyChangeNotifications = new List<string>();
            settings.PropertyChanged += (sender, args) => propertyChangeNotifications.Add(args.PropertyName);

            //Initial value with no Environment set
            Assert.AreEqual("Key", settings.Key);

            //switching environment changes a
            settings.SetEnvironment("prod");
            Assert.AreEqual("PROD", settings.Key);
            Assert.AreEqual("Key", propertyChangeNotifications.Single());

            //changing env but no properties will change
            settings.SetEnvironment("prod2");
            Assert.AreEqual("PROD", settings.Key);
            //no changes so we shouldn't have received additional events
            Assert.AreEqual("Key", propertyChangeNotifications.Single());
        }

    }

    class UnresolvedPropertySettings : Settings
    {
        public int Foo => Get<int>();
    }

    class MySettings : Settings
    {
        public MySettings()
            : base(bindingPath: "")
        { }

        public TimeSpan MyTimeSpan => Get<TimeSpan>();

        public string Key => Get<string>();
    }
}