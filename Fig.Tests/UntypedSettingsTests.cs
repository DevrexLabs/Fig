using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Fig.Test
{
    public class UntypedSettingsTests
    {
        private Settings _settings;
        private string _key;

        [SetUp]
        public void Setup()
        {
            this._key = "CoffeeRefillInterval";
            this._settings = new SettingsBuilder()
                .UseSettingsDictionary(new SettingsDictionary() {
                    [_key] = "00:05:00"
                })
                .Build();
        }

        [Test]
        public void UntypedSettingsWhenKeyFoundReturnsValue()
        {
            //Notice how the return type is derived from the default callback
            var refillInterval = this._settings.Get(this._key, () => TimeSpan.FromMinutes(20));

            //Expect he value from the dictionary, not the default
            Assert.AreEqual(TimeSpan.FromMinutes(5), refillInterval);
        }

        [Test]
        public void UntypedSettingsWhenKeyNotFoundAndNoDefaultThrowsException()
        {
            TimeSpan refillInterval;

            //no key, no default. BOOM!
            Assert.Throws<KeyNotFoundException>(() => refillInterval = this._settings.Get<TimeSpan>(this._key + "ouch"));
        }

        [Test]
        public void UntypedSettingsWhenKeyNotFoundAndDefaultReturnsValue()
        {
            TimeSpan refillInterval;

            //key missing, default kicks in
            var defaultTimespan = TimeSpan.FromSeconds(42);
            refillInterval = this._settings.Get<TimeSpan>(this._key + "ouch", () => defaultTimespan);

            Assert.AreEqual(defaultTimespan, refillInterval);
        }
    }
}