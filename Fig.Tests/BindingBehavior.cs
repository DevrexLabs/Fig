using NUnit.Framework;

namespace Fig.Test;

public class BindingBehavior
{
    [Test]
    public void EmptyPathBindsToRoot()
    {
        var settings = new SettingsBuilder()
            .UseSettingsDictionary(new SettingsDictionary()
            {
                {"Hello", "Hi"},
                {"Number", "16"},
                {"OtherNumber", "16"}
            }).Build();

        var settingsWithDefaults = settings.Bind<SettingsWithDefaults>(path:"");
        Assert.AreEqual(settingsWithDefaults.Hello, "Hi");
        Assert.AreEqual(settingsWithDefaults.Number, 16);
        Assert.AreEqual(settingsWithDefaults.OtherNumber, 16);
        
    }

    [Test]
    public void CanBindToNullableProperties()
    {
        //empty dictionary
        var settings = new SettingsBuilder().Build();
        var settingsWithDefaults = settings.Bind<SettingsWithDefaults>();
        
        Assert.AreEqual(settingsWithDefaults.Hello, "Hello");
        Assert.AreEqual(settingsWithDefaults.Number, 42);
        Assert.AreEqual(settingsWithDefaults.OtherNumber, 42);
    }

    private class SettingsWithDefaults
    {
        public int Number { get; set; } = 42;
        public string Hello { get; set; } = nameof(Hello);

        public int? OtherNumber { get; set; } = 42;
    }
}