using System.Collections.Generic;

namespace Fig
{
    /// <summary>
    /// A configuration source provides all the key value pairs
    /// from the underlying source
    /// </summary>
    public interface ISettingsSource : IEnumerable<(string,string)>
    {
    }
}
