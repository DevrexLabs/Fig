using System;

namespace Fig.Test
{
    public class ExampleSettings : Settings
    {
        public int MyIntProperty
        {
            get => Get(() => 42);
            set => Set(value);
        }

        public int MyReadonlyIntProperty 
            => Get<int>();

        /// <summary>
        /// No default, thus required
        /// custom key, instead of property name
        /// </summary>
        public int RequiredInt
        {
            get => Get<int>("customKey");
            set => Set<int>(value);
        }
        
        public TimeSpan MyTimeSpan 
            => Get<TimeSpan>();
    }
}