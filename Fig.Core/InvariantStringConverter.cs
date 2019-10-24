using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;

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
            if (value == null) return null;

            var typeConverter = TypeDescriptor.GetConverter(targetType);

            // Converts objects that the default TypeConverter for this type cannot convert.
            if (ConvertSpecialObjects(typeConverter, value, targetType, out object convertedValue))
            {
                return convertedValue;
            }

            convertedValue = typeConverter.ConvertFromInvariantString(value);

            // If target type is an enum, check that the parsed string value is a valid value
            if (targetType.IsEnum && !Enum.IsDefined(targetType, convertedValue))
            {
                throw new ArgumentException($"The argument provided is not a valid Enum value.");
            }

            return convertedValue;
        }

        private static bool ConvertSpecialObjects(TypeConverter converter, string value, Type targetType, out object convertedValue)
        {
            convertedValue = null;
            if (converter is ArrayConverter)
            {
                convertedValue = ConvertToArray(value, targetType);
                return true;
            }

            if (targetType == typeof(IPEndPoint))
            {
                convertedValue = ConvertIPEndPoint(value);
                return true;
            }

            if (targetType == typeof(IPAddress))
            {
                convertedValue = ConvertIPAddress(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Custom conversion method for arrays.
        /// For now only supports InvariantCulture when comma separating for decimal separators.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns>Array of targetType object</returns>
        private static object ConvertToArray(string value, Type targetType)
        {
            if (targetType == typeof(string[]))
            {
                return value.Split(',');
            }

            var elementType = targetType.GetElementType();
            if (elementType == null) return null;

            object[] values;
            if (elementType.IsEnum)
            {
                values = value.Split(',').Select(a => Enum.Parse(elementType, a)).ToArray();
            }
            else
            {
                // Some types we can convert from invariant string
                var converter = TypeDescriptor.GetConverter(elementType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    values = value.Split(',').Select(a => converter.ConvertFromInvariantString(a)).ToArray();
                }
                else
                {
                    var valuesToConvert = value.Split(',');
                    values = new object[valuesToConvert.Length];
                    for (int i=0; i < valuesToConvert.Length; i++)
                    {
                        // Attempt to convert to a special object incase the element type is one of them
                        if (ConvertSpecialObjects(converter, valuesToConvert[i], elementType, out object convertedValue))
                        {
                            values[i] = convertedValue;
                        }
                        else
                        {
                            values[i] = System.Convert.ChangeType(valuesToConvert[i], elementType);
                        }
                    }
                }
            }

            var outputArray = Array.CreateInstance(elementType, values.Length);
            Array.Copy(values, outputArray, values.Length);
            return outputArray;
        }

        /// <summary>
        /// Basic IPAddress parser
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private static IPAddress ConvertIPAddress(string ipAddress)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(ipAddress, out ip))
            {
                throw new FormatException("Invalid ip-adress: " + ipAddress);
            }
            return ip;
        }

        /// <summary>
        /// Handles both IPv4 and IPv6 notation.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        private static IPEndPoint ConvertIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress: " + string.Join(":", ep, 0, ep.Length - 1));
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
    }
}