using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace Fig.Test
{
    /// <summary>
    /// Tests that prove that we can convert from string to
    /// different types
    /// </summary>
    public class StringConversionTests
    {
        private IStringConverter _converter = new InvariantStringConverter();

        [Test]
        public void ConvertTimeSpan()
        {
            var timeSpan = TimeSpan.FromMinutes(42);
            var asString = timeSpan.ToString();
            var parsed = _converter.Convert<TimeSpan>(asString);
            Assert.AreEqual(timeSpan, parsed);
        }
        
        [Test]
        public void ConvertDateTime()
        {
            var dateTime = DateTime.Now;
            var asString = dateTime.ToString("O");
            Console.WriteLine(asString);
            var parsed = _converter.Convert<DateTime>(asString);
            Assert.AreEqual(dateTime, parsed);
        }

        [Test]
        public void ConvertDateTimeOffset()
        {
            var dateTime = DateTimeOffset.Now;
            var asString = dateTime.ToString("O");
            Console.WriteLine(asString);
            var parsed = _converter.Convert<DateTimeOffset>(asString);
            Assert.AreEqual(dateTime, parsed);
        }

        [Test]
        public void ConvertDouble()
        {
            var parsed = _converter.Convert<double>("3.141592");
            Assert.AreEqual(3.141592, parsed, 0.0000001);
        }
    }
}