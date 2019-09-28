using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fig.Core;

namespace Fig
{
    public class SettingsBuilder
    {
        private Stack<Func<ISettingsSource>> _providers 
            = new Stack<Func<ISettingsSource>>();

        private SettingsDictionary _dictionary;

        /// <summary>
        /// Where to look for files
        /// </summary>
        private string _basePath;
        
        public T Build<T>() where T : Settings
        {
            var compositeDictionary = new CompositeSettingsDictionary();
            
            while (_providers.Any())
            {
                _dictionary = new SettingsDictionary();
                var provider = _providers.Pop()();
                foreach (var (key,val) in provider)
                {
                    _dictionary[key] = val;
                }
                compositeDictionary.Add(_dictionary);
            }

            var result = (T) Activator.CreateInstance(typeof(T),nonPublic:true);
            result.SettingsDictionary = compositeDictionary;
            return result;
        }

        public SettingsBuilder UseCommandLine(string prefix, string[] args)
        {
            //todo
            //_providers.Push(new CommandLineProvider(prefix, args));
            return this;
        }

        public SettingsBuilder UseEnvironmentVariables(string prefix)
        {
            _providers.Push(() => new EnvironmentVarsSettingsSource(prefix));
            return this;
        }

        public SettingsBuilder UseAppSettingsJson(string fileNameTemplate, bool required)
        {
            //_providers.Push(() => FileBasedProvider(fileName => new AppSettingsJsonConfigurationSource(fileName), fileNameTemplate, required);
            return this;
        }

        private IEnumerable<(string,string)> FileBasedProvider(
            Func<string, ISettingsSource> providerFactory,
            string fileNameTemplate, 
            bool required)
        {
            var fileName = Eval(fileNameTemplate);
            if (File.Exists(fileName))
            {
                var provider = providerFactory.Invoke(fileName);
                foreach(var (key,val) in provider) yield return (key, val);
            }
            else if (required) throw new FileNotFoundException("Configuration file not found", fileName);
        }
        
        private string Eval(string fileNameTemplate)
        {
            return Regex.Replace(fileNameTemplate, @"\${(?<env>\w+)}", LookupKey);
        }

        private string LookupKey(Match m)
        {
            var key = m.Groups["env"].Value;
            _dictionary.TryGetValue(key, out var result);
            return result;
        }
        
        public SettingsBuilder UseAppSettingsXml(string prefix, bool includeConnectionStrings)
        {
            return this;
        }

        public SettingsBuilder UseIniFile(string fileName, bool required)
        {
            return this;
        }

        public SettingsBuilder BasePath(string basePath)
        {
            _basePath = basePath;
            return this;
        }
    }
}