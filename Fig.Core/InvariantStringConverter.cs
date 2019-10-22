using System;
using System.ComponentModel;
using System.Linq;

namespace Fig
{
    internal class InvariantStringConverter : IStringConverter
    {
        public T Convert<T>(string value)
        {
            return (T)Convert(value, typeof(T));
        }

        public object Convert(string value, Type targetType)
        {
            var typeConverter = TypeDescriptor.GetConverter(targetType);

            if (typeConverter is ArrayConverter && !typeConverter.CanConvertFrom(typeof(string)))
            {
                return ConvertToArray(value, targetType);
            }

            var convertedValue = typeConverter.ConvertFromInvariantString(value);

            // If target type is an enum, check that the parsed string value is a valid value
            if (targetType.IsEnum && !Enum.IsDefined(targetType, convertedValue))
            {
                throw new ArgumentException($"The argument provided is not a valid Enum value.");
            }

            return convertedValue;
        }

        private object ConvertToArray(string value, Type targetType)
        {
            if (value == null) return null;
            if (targetType == typeof(string[]))
            {
                return value.Split(',');
            }
            else
            {
                var elementType = targetType.GetElementType();

                object[] values = null;
                if (elementType.IsEnum)
                {
                    values = value.Split(',').Select(a => Enum.Parse(elementType, a)).ToArray();
                }
                else
                {
                    // Some types are convertable
                    var converter = TypeDescriptor.GetConverter(elementType);
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        values = value.Split(',').Select(a => converter.ConvertFromInvariantString(a)).ToArray();
                    }
                    else
                    {
                        values = value.Split(',').Select(a => System.Convert.ChangeType(a, elementType)).ToArray();
                    }
                }

                var outputArray = Array.CreateInstance(elementType, values.Length);
                Array.Copy(values, outputArray, values.Length);
                return outputArray;
            }
        }
    }
}