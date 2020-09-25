using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig
{
    public static class EnumerableExtensions
    {
        public static int MaxOrDefault<T>(this IEnumerable<T> enumeration, Func<T, int> selector)
        {
            return enumeration.Any() ? enumeration.Max(selector) : default(int);
        }
    }
}
