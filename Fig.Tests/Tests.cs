using System;
using System.IO;
using System.Net;
using Fig.AppSettingsXml;
using NUnit.Framework;

namespace Fig.Test
{
    public class Tests
    {
        private Settings _settings;
        private MySettings _mySettings;
        private SettingsDictionary _settingsDictionary;

        [SetUp]
        public void Setup()
        {
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
                ["ServerIp"] = "127.0.0.1",
                ["ServerEndPoint"] = "127.0.0.1:80",

                ["env"] = "staging",
                ["ExampleSettings.MyTimeSpan:staging"] = "00:15:00",

                ["ExampleSettings.MyTimeSpan:FAIL"] = "not a timespan"
            };

            var dictionary = new LayeredSettingsDictionary();
            dictionary.Add(_settingsDictionary);
            _settings = new Settings(dictionary);
            _mySettings = _settings.Bind<MySettings>(path: "");
        }

        [Test]
        public void CanReadDefault()
        {
            Assert.AreEqual(1234, _mySettings.IntWithDefault);
        }
        
        [Test]
        public void BuilderBonanza()
        {
            new SettingsBuilder()
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
            Assert.AreEqual(TimeSpan.FromMinutes(42), _mySettings.MyTimeSpan);
        }

        [Test]
        public void EnvironmentIsRespected()
        {
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(_settingsDictionary)
                .Build();

            //No Configuration, should return unqualified setting
            Assert.AreEqual("Key", settings.Get<string>("Key"));

            settings.SetProfile("prod");
            Assert.AreEqual("PROD", settings.Get<string>("Key"));

            settings.SetProfile("test");
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
                .Build();
            
            var mySettings = settings.Bind<ExampleSettings>(requireAll: false);

            Assert.AreEqual(TimeSpan.FromMinutes(15), mySettings.MyTimeSpan);
            Assert.AreEqual("staging", settings.Profile);
        }
    }

    class MySettings
    {
        public int IntWithDefault => 1234;
        public TimeSpan MyTimeSpan { get; set; } = TimeSpan.FromMinutes(2);
        public IPAddress ServerIp { get; set; }
        public IPEndPoint ServerEndPoint { get; set; }

        public string Key { get; }
    }
}