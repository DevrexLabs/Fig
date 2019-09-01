using System;
using Fig;
using NUnit.Framework;

namespace Tests
{
    public class ConvertFromStringTests
    {
        Config _config = new Config();

        [Test]
        public void ConvertTimeSpan()
        {
            var timeSpan = TimeSpan.FromMinutes(42);
            var asString = timeSpan.ToString();
            Console.WriteLine(asString);
            var parsed = _config.Convert<TimeSpan>(asString);
            Assert.AreEqual(timeSpan, parsed);
        }
    }
}