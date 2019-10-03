namespace Fig
{
    public static class Extensions
    {
        public static SettingsBuilder UseAppSettingsJson(this SettingsBuilder builder, string fileNameTemplate, bool required)
        {
            builder.AddFileBasedSource(file => new AppSettingsJsonSource(file), fileNameTemplate, required);
            return builder;
        }
    }
}