using System;

namespace Fig
{
    /// <summary>
    /// Used to describe the binding between a property and
    /// it's key in the dictionary
    /// </summary>
    internal class BindingInfo
    {
        /// <summary>
        /// The key of the setting in the settings dictionary
        /// </summary>
        internal string Key { get; set; }


        /// <summary>
        /// The name of the property on the strongly typed config class
        /// </summary>
        internal string PropertyName { get; set; }
    }
}
