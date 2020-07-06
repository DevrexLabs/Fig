using NUnit.Framework;

namespace Fig.Test
{
    public class CommandLineArgsTests
    {
        [Test]
        public void WithPrefix()
        {
            var args = new[] {"--fig:Color=red", "--fig:Size=4", "other=20"};
            var settings = new SettingsBuilder().UseCommandLine(args).Build();
            var colorExists = settings.SettingsDictionary.TryGetValue("Color", "", out _);
            var sizeExists = settings.SettingsDictionary.TryGetValue("Size", "", out _);
            var prefixlessNotPresent = settings.SettingsDictionary.TryGetValue("Other", "", out _);
            
            Assert.True(colorExists);
            Assert.True(sizeExists);
            Assert.False(prefixlessNotPresent);
        }

        [Test]
        public void WithoutPrefix()
        {
            var args = new[] {"myApp.Color=red", "Size=4", "--other=20"};
            var settings = new SettingsBuilder().UseCommandLine(args, prefix:"").Build();
            var colorExists = settings.SettingsDictionary.TryGetValue("myApp.Color", "", out _);
            var sizeExists = settings.SettingsDictionary.TryGetValue("Size", "", out _);
            var nonMatchingKey = settings.SettingsDictionary.TryGetValue("--Other", "", out _);
            
            Assert.True(colorExists);
            Assert.True(sizeExists);
            Assert.False(nonMatchingKey);
        }
    }
}