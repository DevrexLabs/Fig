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

        [Test]
        public void ConvertInteger()
        {
            Assert.AreEqual(42, _converter.Convert<int>("42"));
            Assert.AreEqual(-42, _converter.Convert<int>("-42"));
            Assert.That(() => _converter.Convert<int>("42.100"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertUnsignedInteger()
        {
            Assert.AreEqual(42, _converter.Convert<uint>("42"));
            Assert.That(() => _converter.Convert<uint>("-42"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertFloat()
        {
            Assert.AreEqual(42.01f, _converter.Convert<float>("42.01"));
            Assert.AreEqual(-42.01f, _converter.Convert<float>("-42.01"));

            Assert.That(() => _converter.Convert<float>("-4.402823E+38"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertShort()
        {
            Assert.AreEqual(42, _converter.Convert<short>("42"));
            Assert.AreEqual(-42, _converter.Convert<short>("-42"));
            Assert.That(() => _converter.Convert<short>("32768"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertUnsignedShort()
        {
            Assert.AreEqual(42, _converter.Convert<ushort>("42"));
            Assert.That(() => _converter.Convert<ushort>("65536"), Throws.ArgumentException);
            Assert.That(() => _converter.Convert<ushort>("-42"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertLong()
        {
            Assert.AreEqual(42, _converter.Convert<long>("42"));
            Assert.AreEqual(-42, _converter.Convert<long>("-42"));
            Assert.That(() => _converter.Convert<long>("9223372036854775808"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertUnsignedLong()
        {
            Assert.AreEqual(42, _converter.Convert<ulong>("42"));
            Assert.That(() => _converter.Convert<ulong>("-42"), Throws.ArgumentException);
            Assert.That(() => _converter.Convert<ulong>("18446744073709551616"), Throws.ArgumentException);
        }
    }
}