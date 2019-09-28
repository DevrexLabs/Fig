using System;
using System.ComponentModel;

namespace Fig
{
    internal class InvariantStringConverter : IStringConverter
    {
        public T Convert<T>(string value)
        {
            return (T) Convert(value, typeof(T));
        }

        public object Convert(string value, Type targetType)
        {
            var typeConverter = TypeDescriptor.GetConverter(targetType);
            return typeConverter.ConvertFromInvariantString(value);
        }
    }
}