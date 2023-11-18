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
                ["ServerIp"] = "127.0.0.1",
                ["ServerEndPoint"] = "127.0.0.1:80",

                ["env"] = "staging",
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