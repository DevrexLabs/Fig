using System;
using System.Collections.Generic;

namespace Fig
{
    /// <summary>
    /// A case insensitive string dictionary
    /// </summary>
    public class SettingsDictionary : Dictionary<string,string>
    {
        public SettingsDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}