using System;
using NUnit.Framework;

namespace Fig.Test
{
    public class BuilderTests
    {
        [Test]
        public void CanLoadRequiredFile()
        {
            var settings = new SettingsBuilder()
                .UseAppSettingsJson("appsettings.json", required: true)
                .Build();
            Assert.DoesNotThrow(() => settings.Get("AllowedHosts"));
        }

        [Test]
        public void CanLoadRequiredFileWithVariableExpansion()
        {
            Environment.SetEnvironmentVariable("PROFILE", "TEST");
            var settings = new SettingsBuilder()
                .UseEnvironmentVariables()
                .UseAppSettingsJson("appSettings.${PROFILE}.json", required: true)
                .Build();
            Assert.DoesNotThrow(() => settings.Get("AllowedHosts"));
        }

        [Test]
        public void CanReferenceOptionalNonExistentFile()
        {
            Assert.DoesNotThrow(() =>
            {
                var settings = new SettingsBuilder()
                    .UseAppSettingsJson("nonexistent.json", required: false)
                    .Build();            
            });
        }

        [Test]
        public void CanReferenceOptionalNonExistentFileWithVariableExpansion()
        {
            Assert.DoesNotThrow(() =>
            {
                var settings = new SettingsBuilder()
                    .UseAppSettingsJson("nonexistent.${ENV}.json", required: false)
                    .Build();
            });
        }
    }

    public class BindingPathTests
    {
        [Test]
        public void BindingPathIsRespected()
        {
            var expected = "expected";

            var settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    ["MyBindingPath.Property"] = expected
                }).Build();
            
            var typedSettings = settings.Bind<TypeWithExplicitBindingPath>(path: "MyBindingPath");
            
            Assert.AreEqual(expected, typedSettings.Property);

        }

        private class TypeWithExplicitBindingPath
        {
            public string Property { get; set; }
        }
    }
}