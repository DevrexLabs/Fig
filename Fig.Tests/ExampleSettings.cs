using System;

namespace Fig.Test
{
    public class ExampleSettings
    {
        public int MyIntProperty
        {
            get;
            set;
        } = 42;

        public int MyReadonlyIntProperty
        {
            get;
            private set;
        }

        /// <summary>
        /// No default, thus required
        /// custom key, instead of property name
        /// </summary>
        public int RequiredInt
        {
            get;
            set;
        }

        public TimeSpan MyTimeSpan
        {
            get;
            set;
        }

        public int HasDefault => 42;
    }
}