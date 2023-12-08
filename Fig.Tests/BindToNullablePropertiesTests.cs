using System;
using NUnit.Framework;

namespace Fig.Test;

public class BindToNullablePropertiesTests
{
   [Test]
    public void CanBindToNullableProperties()
    {
        var expectedDuration = TimeSpan.FromMinutes(30);
        var settings = new SettingsBuilder()
            .UseSettingsDictionary(new SettingsDictionary()
            {
                [$"{nameof(TestClass)}.{nameof(TestClass.Name)}"] = "Fullname",
                [$"{nameof(TestClass)}.{nameof(TestClass.Age)}"] = "40",
                [$"{nameof(TestClass)}.{nameof(TestClass.Duration)}"] = expectedDuration.ToString()
            })
            .Build();

        var testClass = settings.Bind<TestClass>();

        Assert.AreEqual("Fullname", testClass.Name);
        Assert.AreEqual(40, testClass.Age);
        Assert.AreEqual(expectedDuration, testClass.Duration);
    }

    private class TestClass
    {
        public string Name { get; set; }
        public int? Age { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}