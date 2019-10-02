using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Fig.Test
{
    public class UntypedSettingsTests
    {
        [Test]
        public void UntypedAccessDemo()
        {
            //todo: refactor, break out into setup and 3 different tests for each assert
            var settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary()
                {
                    ["CoffeeRefillInterval"] = "00:05:00"
                })
                .Build();
    
            var key = "CoffeeRefillInterval";
            
            //Notice how the return type is derived from the default callback
            var refillInterval = settings.Get(key, () => TimeSpan.FromMinutes(20));
            
            //Expect he value from the dictionary, not the default
            Assert.AreEqual(TimeSpan.FromMinutes(5), refillInterval);

            //no key, no default. BOOM!
            Assert.Throws<KeyNotFoundException>(
                () => refillInterval = settings.Get<TimeSpan>(key + "ouch"));
            
            //key missing, default kicks in
            var defaultTimespan = TimeSpan.FromSeconds(42);
            refillInterval = settings.Get<TimeSpan>(key + "ouch", () => defaultTimespan);
            Assert.AreEqual(defaultTimespan, refillInterval);

        }
    }
}