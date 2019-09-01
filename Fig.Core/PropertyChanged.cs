namespace Fig
{
    public class PropertyChanged
    {
        public string Name { get; private set; }
        public object PreviousValue { get; private set; }
        public object CurrentValue { get; private set; }
        public object OriginalValue { get; private set; }

        public PropertyChanged(string propertyName, object value, object previous, object original)
        {
            Name = propertyName;
            PreviousValue = previous;
            CurrentValue = value;
            OriginalValue = original;
        }
    }
}
