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

            var convertedValue = typeConverter.ConvertFromInvariantString(value);

            // If target type is an enum, check that the parsed string value is a valid value
            if (targetType.IsEnum && !Enum.IsDefined(targetType, convertedValue))
            {
                throw new ArgumentException($"The argument provided is not a valid Enum value.");
            }

            return convertedValue;
        }
    }
}