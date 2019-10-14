using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Fig.Core;

namespace Fig
{
    public class SettingsBuilder
    {
        private CompositeSettingsDictionary _compositeDictionary
            = new CompositeSettingsDictionary();

        private string _environment = "";

        /// <summary>
        /// Where to look for files
        /// </summary>
        private string _basePath;

        public Settings Build()
        {
            return new Settings(_compositeDictionary) { Environment = _environment };
        }

        public T Build<T>(string prefix = null) where T : Settings, new()
        {
            var result = Activator.CreateInstance<T>();
            result.SettingsDictionary = _compositeDictionary;
            result.Environment = _environment;
            result.SetBindingPath(prefix);
            result.PreLoad();
            return result;
        }

        /// <summary>
        /// Set the environment using variable expansion
        /// </summary>
        public SettingsBuilder SetEnvironment(string environmentTemplate)
        {
            _environment = _compositeDictionary.ExpandVariables(environmentTemplate);
            return this;
        }

        public SettingsBuilder UseCommandLine(string[] args, string prefix = "--fig:", char delimiter = '=')
        {
            Add(new StringArraySource(args, prefix, delimiter).ToSettingsDictionary());
            return this;
        }

        public SettingsBuilder UseEnvironmentVariables(string prefix)
        {
            Add(new EnvironmentVarsSettingsSource(prefix).ToSettingsDictionary());
            return this;
        }

        private void Add(SettingsDictionary settingsDictionary)
        {
            _compositeDictionary.Add(settingsDictionary.WithNormalizedEnvironmentQualifiers());
        }

        protected internal void AddFileBasedSource(Func<string, SettingsSource> sourceFactory, string fileNameTemplate, bool required)
        {
            var fileName = _compositeDictionary.ExpandVariables(fileNameTemplate);
            var fullPath = Path.Combine(_basePath, fileName);
            if (File.Exists(fullPath))
            {
                var source = sourceFactory.Invoke(fullPath);
                Add(source.ToSettingsDictionary());
            }
            else if (required) throw new FileNotFoundException("No such file", fullPath);

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