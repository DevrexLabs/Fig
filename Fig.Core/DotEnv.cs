using System;
using System.IO;
using System.Linq;

namespace Fig.PosifonCare.Utilities
{
    public static class DotEnv
    {
        /// <summary>
        /// Load key value pairs from a file into environment variables.
        /// Start in the current directory and climbs up the tree until the file is found
        /// or a stop condition is met
        /// </summary>
        /// <param name="fileName">The filename with key value pairs, defaults to ".env"</param>
        /// <param name="maxTraversals">Maximum number of parent directories to scan</param>
        /// <param name="stopDirectory">Stop looking if the current directory has a child directory with this name</param>
        /// <param name="stopFile">Stop scanning if the current directory has a file with this name</param>
        /// <returns>True if a file was found</returns>
        /// <exception>Throws an exception if there is an error processing the file</exception>
        public static bool Load(string fileName = ".env", int maxTraversals = 5, string stopDirectory = ".git", string stopFile = null)
        {
            var currentDirectory = Environment.CurrentDirectory;
            while (maxTraversals-- > 0)
            {
                var fullPath = Path.Combine(currentDirectory, fileName);
                if (File.Exists(fullPath))
                {
                    Parse(fullPath);
                    return true;
                }
                if (FinalDirectory(currentDirectory, stopDirectory, stopFile)) return false;
                var parentDirectory = Path.Combine(currentDirectory, "..");
                
                //avoid infinite loop when we are at the root of the directory
                if (parentDirectory == currentDirectory) return false;
                currentDirectory = parentDirectory;
            }
            return false;
        }

        private static bool FinalDirectory(string directory, string stopDirectory, string stopFile)
        {
            return Directory.Exists(Path.Combine(directory, stopDirectory))
                   || (stopFile != null && File.Exists(Path.Combine(directory, stopFile)));
        }

        private static void Parse(string fullPath)
        {
            var keyValuePairs = File.ReadAllLines(fullPath)
                .Where(line => !String.IsNullOrEmpty(line))
                .Select(line => line.Trim().Split(new[] { '=' }, 2));

            foreach (var kvp in keyValuePairs)
            {
                Environment.SetEnvironmentVariable(kvp[0].Trim(), kvp[1].Trim());
            }
        }
    }
}