using System;
using System.IO;
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
            _settings.PreLoad();
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
                .UseCommandLine(new []{"--fig:MySettings.=coffee", "--fig:"})
                .UseEnvironmentVariables(prefix: "FIG_")
                .BasePath(Directory.GetCurrentDirectory())
                .UseAppSettingsJson(fileNameTemplate: "appSettings.${CONFIG}.json", required: false)
                .UseAppSettingsJson(fileNameTemplate: "appSettings.json", required: false)
                .UseAppSettingsXml(prefix: "fig:", includeConnectionStrings: false)
                .UseIniFile("appSettings.${CONFIG}.ini", required: false)
                .UseIniFile("appSettings.ini", required: false)
                .Build();
        }
    }
}