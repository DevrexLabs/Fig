    namespace Fig.AppSettingsXml
{
    public static class SettingsBuilderExtensions
    {
        public static SettingsBuilder UseAppSettingsXml(this SettingsBuilder settingsBuilder, string prefix = null, bool includeConnectionStrings = true)
        {
            var dictionary = new AppSettingsXmlSource().ToSettingsDictionary();
            settingsBuilder.UseSettingsDictionary(dictionary);
            return settingsBuilder;
        }
    }
}