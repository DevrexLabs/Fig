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
            Environment.SetEnvironmentVariable("ENV", "TEST");
            var settings = new SettingsBuilder()
                .UseEnvironmentVariables()
                .UseAppSettingsJson("appsettings.${ENV}.json", required: true)
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

            var typedSettings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    ["MyBindingPath.Property"] = expected
                }).Build<TypeWithExplicitBindingPath>();
            
            Assert.AreEqual(expected, typedSettings.MyProperty);

        }

        private class TypeWithExplicitBindingPath : Settings
        {
            public TypeWithExplicitBindingPath() : base("MyBindingPath") { }

            public string MyProperty { get; set; }
        }
    }
}