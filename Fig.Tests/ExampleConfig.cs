using System;

namespace Fig.Test
{
    public class ExampleConfig : Config
    {
        [Config(Default = 42)]
        public int MyIntProperty
        {
            get => Get<int>();
            set => Set<int>(value);
        }

        [Config(Default = 43)]
        public int MyReadonlyIntProperty
        {
            get => Get<int>();
        }

        
        public int MissingDefault
        {
            get => Get<int>();
            set => Set<int>(value);
        }

        
        public TimeSpan MyTimeSpan
        {
            get => Get<TimeSpan>();
        }

    }
}
