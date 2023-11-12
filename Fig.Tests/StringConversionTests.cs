using System;
using System.Linq;
using System.Net;
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

        [Test]
        public void ConvertByte()
        {
            Assert.AreEqual(1, _converter.Convert<byte>("1"));
            Assert.AreEqual(12, _converter.Convert<byte>("12"));
            Assert.AreEqual(255, _converter.Convert<byte>("255"));
            Assert.That(() => _converter.Convert<byte>("256"), Throws.ArgumentException);
        }

        [Test]
        public void ConvertEnum()
        {
            Assert.AreEqual(TestEnum.Option1, _converter.Convert<TestEnum>("1"));
            Assert.AreEqual(TestEnum.Option2, _converter.Convert<TestEnum>("2"));
            Assert.AreEqual(TestEnum.Option1, _converter.Convert<TestEnum>("Option1"));
            Assert.AreEqual(TestEnum.Option2, _converter.Convert<TestEnum>("Option2"));
            Assert.That(() => _converter.Convert<TestEnum>("3"), Throws.ArgumentException);
            Assert.That(() => _converter.Convert<TestEnum>("Option3"), Throws.Exception);
        }

        [Test]
        public void ConvertStringArray()
        {
            Assert.AreEqual(new string[] { "1", "2" }, _converter.Convert<string[]>("1,2"));
            Assert.AreEqual(new string[] { "1" }, _converter.Convert<string[]>("1"));
            Assert.AreEqual(null, _converter.Convert<string[]>(null));
        }

        [Test]
        public void ConvertIntegerArray()
        {
            Assert.AreEqual(new int[] { 1, 2 }, _converter.Convert<int[]>("1,2"));
            Assert.AreEqual(new int[] { 1 }, _converter.Convert<int[]>("1"));
            Assert.AreEqual(null, _converter.Convert<int[]>(null));
            Assert.That(() => _converter.Convert<int[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertDateTimeArray()
        {
            var now = DateTime.Now;
            Assert.AreEqual(new DateTime[] { now, now.AddDays(1) }, _converter.Convert<DateTime[]>(now.ToString("O") + "," + now.AddDays(1).ToString("O")));
            Assert.AreEqual(new DateTime[] { now }, _converter.Convert<DateTime[]>(now.ToString("O")));
            Assert.AreEqual(null, _converter.Convert<DateTime[]>(null));
            Assert.That(() => _converter.Convert<DateTime[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertDateTimeOffsetArray()
        {
            var now = DateTimeOffset.Now;
            Assert.AreEqual(new DateTimeOffset[] { now, now.AddDays(1) }, _converter.Convert<DateTimeOffset[]>(now.ToString("O") + "," + now.AddDays(1).ToString("O")));
            Assert.AreEqual(new DateTimeOffset[] { now }, _converter.Convert<DateTimeOffset[]>(now.ToString("O")));
            Assert.AreEqual(null, _converter.Convert<DateTimeOffset[]>(null));
            Assert.That(() => _converter.Convert<DateTimeOffset[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertTimeSpanArray()
        {
            Assert.AreEqual(new TimeSpan[] { TimeSpan.FromMinutes(42), TimeSpan.FromMinutes(40) }, _converter.Convert<TimeSpan[]>(TimeSpan.FromMinutes(42).ToString() + "," + TimeSpan.FromMinutes(40).ToString()));
            Assert.AreEqual(new TimeSpan[] { TimeSpan.FromMinutes(42) }, _converter.Convert<TimeSpan[]>(TimeSpan.FromMinutes(42).ToString()));
            Assert.AreEqual(null, _converter.Convert<TimeSpan[]>(null));
            Assert.That(() => _converter.Convert<TimeSpan[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertUnsignedIntegerArray()
        {
            Assert.AreEqual(new uint[] { 1, 2 }, _converter.Convert<uint[]>("1,2"));
            Assert.AreEqual(new uint[] { 1 }, _converter.Convert<uint[]>("1"));
            Assert.AreEqual(null, _converter.Convert<uint[]>(null));
            Assert.That(() => _converter.Convert<uint[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertByteArray()
        {
            Assert.AreEqual(new byte[] { 1, 2 }, _converter.Convert<byte[]>("1,2"));
            Assert.AreEqual(new byte[] { 1 }, _converter.Convert<byte[]>("1"));
            Assert.AreEqual(null, _converter.Convert<byte[]>(null));
            Assert.That(() => _converter.Convert<byte[]>("256"), Throws.Exception);
        }

        [Test]
        public void ConvertFloatArray()
        {
            Assert.AreEqual(new float[] { 1, 2 }, _converter.Convert<float[]>("1,2"));
            Assert.AreEqual(new float[] { 1 }, _converter.Convert<float[]>("1"));
            Assert.AreEqual(null, _converter.Convert<float[]>(null));
            Assert.That(() => _converter.Convert<float[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertDoubleArray()
        {
            Assert.AreEqual(new double[] { 1, 2 }, _converter.Convert<double[]>("1,2"));
            Assert.AreEqual(new double[] { 1 }, _converter.Convert<double[]>("1"));
            Assert.AreEqual(null, _converter.Convert<double[]>(null));
            Assert.That(() => _converter.Convert<double[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertLongArray()
        {
            Assert.AreEqual(new long[] { 1, 2 }, _converter.Convert<long[]>("1,2"));
            Assert.AreEqual(new long[] { 1 }, _converter.Convert<long[]>("1"));
            Assert.AreEqual(null, _converter.Convert<long[]>(null));
            Assert.That(() => _converter.Convert<long[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertUnsignedLongArray()
        {
            Assert.AreEqual(new ulong[] { 1, 2 }, _converter.Convert<ulong[]>("1,2"));
            Assert.AreEqual(new ulong[] { 1 }, _converter.Convert<ulong[]>("1"));
            Assert.AreEqual(null, _converter.Convert<ulong[]>(null));
            Assert.That(() => _converter.Convert<ulong[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertUnsignedShortArray()
        {
            Assert.AreEqual(new ushort[] { 1, 2 }, _converter.Convert<ushort[]>("1,2"));
            Assert.AreEqual(new ushort[] { 1 }, _converter.Convert<ushort[]>("1"));
            Assert.AreEqual(null, _converter.Convert<ushort[]>(null));
            Assert.That(() => _converter.Convert<ushort[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertShortArray()
        {
            Assert.AreEqual(new short[] { 1, 2 }, _converter.Convert<short[]>("1,2"));
            Assert.AreEqual(new short[] { 1 }, _converter.Convert<short[]>("1"));
            Assert.AreEqual(null, _converter.Convert<short[]>(null));
            Assert.That(() => _converter.Convert<short[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertEnumArray()
        {
            Assert.AreEqual(new TestEnum[] { TestEnum.Option1, TestEnum.Option2 }, _converter.Convert<TestEnum[]>("Option1,Option2"));
            Assert.AreEqual(new TestEnum[] { TestEnum.Option1 }, _converter.Convert<TestEnum[]>("1"));
            Assert.AreEqual(null, _converter.Convert<TestEnum[]>(null));
            Assert.That(() => _converter.Convert<TestEnum[]>("1a"), Throws.Exception);
        }

        [Test]
        public void ConvertToIPAddress()
        {
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), _converter.Convert<IPAddress>("127.0.0.1"));
            Assert.AreEqual(null, _converter.Convert<IPAddress>(null));
        }

        [Test]
        public void ConvertToIPAddressArray()
        {
            Assert.AreEqual(new[] { "127.0.0.1", "18.84.21.3" }.Select(IPAddress.Parse).ToArray(), _converter.Convert<IPAddress[]>("127.0.0.1,18.84.21.3"));
            Assert.AreEqual(new[] { IPAddress.Parse("127.0.0.1") }, _converter.Convert<IPAddress[]>("127.0.0.1"));
            Assert.AreEqual(null, _converter.Convert<IPAddress[]>(null));
            Assert.That(() => _converter.Convert<IPAddress[]>("localhost"), Throws.Exception);
        }

        [Test]
        public void ConvertToIPEndPoint()
        {
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80), _converter.Convert<IPEndPoint>("127.0.0.1:80"));
            Assert.AreEqual(null, _converter.Convert<IPEndPoint>(null));
        }

        [Test]
        public void ConvertToIPEndPointArray()
        {
            Assert.AreEqual(new[] { "127.0.0.1", "18.84.21.3" }.Select(IPAddress.Parse).Select(a => new IPEndPoint(a, 80)).ToArray(), _converter.Convert<IPEndPoint[]>("127.0.0.1:80,18.84.21.3:80"));
            Assert.AreEqual(new[] { new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80) }, _converter.Convert<IPEndPoint[]>("127.0.0.1:80"));
            Assert.AreEqual(null, _converter.Convert<IPEndPoint[]>(null));
            Assert.That(() => _converter.Convert<IPEndPoint[]>("localhost:a8"), Throws.Exception);
        }

        private enum TestEnum
        {
            Option1 = 1,
            Option2 = 2
        }
    }
}