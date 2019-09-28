using System;

namespace Fig
{
    /// <summary>
    /// Converts from strings to other types
    /// </summary>
    public interface IStringConverter
    {
        T Convert<T>(string value);
        
        object Convert(string value, Type targetType);
    }
}