using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fig
{
    [Serializable]
    public class ConfigurationException : Exception
    {
        public List<string> Errors { get; private set; }

        public ConfigurationException(List<string> errors)
        {
            Errors = new List<string>(errors);
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}