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
                ["MyTimeSpan"]  = "00:42:00",
                ["Key"] = "Key",
                ["Key:PROD"] = "PROD",
                ["Key:TEST"] = "TEST",
                ["Key:PROD2"] = "PROD",
                ["Config"] = "PROD"
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
            Assert.NotNull(propertyChangedName,"The event was not received");
            Assert.AreEqual(nameof(_settings.MyIntProperty), propertyChangedName);
        }

        [Test]
        public void CanReadDefault()
        {
            _settingsDictionary.Remove("ExampleSettings.MyIntProperty");
            _settings.PreLoad();
			Assert.AreEqual(42, _settings.MyIntProperty);
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
            bool removed = _settingsDictionary.Remove("ExampleSettings.RequiredInt");
            Assert.IsTrue(removed);
            Assert.Throws<ConfigurationException>(() => { 
                _settings.PreLoad();
            });
        }

        [Test]
        public void BuilderBonanza()
        {
            var settings = new SettingsBuilder()
                .UseCommandLine(new []{"--fig:MySettings.DefaultBeverage=coffee"})
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
                .Build<MySettings>();
            
            Assert.AreEqual(TimeSpan.FromMinutes(42), settings.MyTimeSpan);
        }
        
        [Test]
        public void ConfigurationIsRespected()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build();
            
            //No Configuration, should return unqualified setting
            Assert.AreEqual("Key", settings.Get<string>("Key"));
            
            settings.SetConfiguration("prod");
            Assert.AreEqual("PROD", settings.Get<string>("Key"));

            settings.SetConfiguration("test");
            Assert.AreEqual("TEST", settings.Get<string>("Key"));
        }
        
        public void ConfigurationChangeFiresPropertyChangeEvents()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build<MySettings>();

            var propertyChangeNotifications = new List<string>();
            settings.PropertyChanged += (sender, args) => propertyChangeNotifications.Add(args.PropertyName);
           
            Assert.AreEqual("Key", settings.Key);

            settings.SetConfiguration("prod");
            
            Assert.AreEqual("Key", propertyChangeNotifications.Single());
            
            //changing config but no properties will change
            settings.SetConfiguration("prod2");
            
            //no changes so we shouldn't have received additional events
            Assert.AreEqual("Key", propertyChangeNotifications.Single());
            
        }

    }

    class MySettings : Settings
    {
        public MySettings()
            :base(bindingPath: "") 
        {}
        
        public TimeSpan MyTimeSpan => Get<TimeSpan>();

        public string Key => Get<string>();
    }
}