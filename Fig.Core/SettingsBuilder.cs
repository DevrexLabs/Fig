using System;
using System.IO;
using Fig.Core;

namespace Fig
{
    public class SettingsBuilder
    {
        private readonly LayeredSettingsDictionary _layeredDictionary
            = new LayeredSettingsDictionary();

        private string _environment = "";

        /// <summary>
        /// Where to look for files, default is current working directory
        /// </summary>
        private string _basePath = "";

        public Settings Build()
        {
            return new Settings(_layeredDictionary) { Profile = _environment };
        }

        /// <summary>
        /// Set the environment using variable expansion
        /// </summary>
        public SettingsBuilder SetEnvironment(string environmentTemplate)
        {
            _environment = _layeredDictionary.ExpandVariables(environmentTemplate);
            return this;
        }

        public SettingsBuilder UseCommandLine(string[] args, string prefix = "--fig:", char delimiter = '=')
        {
            Add(new StringArraySource(args, prefix, delimiter).ToSettingsDictionary());
            return this;
        }

        /// <summary>
        /// Add all the current environment variables that start with a given prefix
        /// <remarks>The path separator (default is _) will be translated to a dot</remarks>
        /// </summary>
        /// <param name="prefix">For example FIG_, default is no prefix. </param>
        /// <param name="dropPrefix">When true, the prefix will not be included in the key</param>
        /// <returns></returns>
        public SettingsBuilder UseEnvironmentVariables(string prefix = "", bool dropPrefix = true)
        {
            Add(new EnvironmentVarsSettingsSource(prefix, dropPrefix).ToSettingsDictionary());
            return this;
        }

        private void Add(SettingsDictionary settingsDictionary)
        {
            _layeredDictionary.Add(settingsDictionary.WithNormalizedEnvironmentQualifiers());
        }

        protected internal void AddFileBasedSource(Func<string, SettingsSource> sourceFactory, string fileNameTemplate, bool required)
        {
            var fileName = _layeredDictionary.ExpandVariables(fileNameTemplate);
            var fullPath = Path.Combine(_basePath, fileName);
            if (File.Exists(fullPath))
            {
                var source = sourceFactory.Invoke(fullPath);
                Add(source.ToSettingsDictionary());
            }
            else if (required) throw new FileNotFoundException("No such file: " + fullPath, fullPath);

        }

        public SettingsBuilder UseIniFile(string fileNameTemplate, bool required = true)
        {
            AddFileBasedSource(f => new IniFileSettingsSource(f), fileNameTemplate, required);
            return this;
        }

        /// <summary>
        /// Set the base path for all file-based providers following this method call.
        /// All file names will be relative to this path.
        /// </summary>
        /// <param name="basePath"></param>
        public SettingsBuilder BasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }

        public SettingsBuilder UseSettingsDictionary(SettingsDictionary settingsDictionary)
        {
            Add(settingsDictionary);
            return this;
        }
    }
}