using System;
using System.IO;
using System.Linq;

namespace Fig
{
    public static class DotEnv
    {
        /// <summary>
        /// Load key value pairs from a file into environment variables.
        /// </summary>
        /// <param name="fileName">The filename with key value pairs, defaults to ".env"</param>
        /// <param name="directory">Directory to look for the file in, defaults to 3 directories above, normally the project root</param>
        /// <returns>True if a file was found</returns>
        /// <exception>Throws an exception if there is an error processing the file</exception>
        public static bool Load(string fileName = ".env", string directory = "../../..")
        {
            var fullPath = Path.Combine(directory, fileName);
            
            if (!File.Exists(fullPath)) return false;
            LoadFile(fullPath);
            return true;
        }
        
        private static void LoadFile(string fullPath)
        {
            var keyValues = File.ReadAllLines(fullPath)
                .Select(line => line.Trim())
                .Where(line => !String.IsNullOrEmpty(line))
                .Where(line => !line.StartsWith("#"))
                .Select(line => line.Split(new[] { '=' }, 2));

            foreach (var kvp in keyValues)
            {
                Environment.SetEnvironmentVariable(kvp[0].Trim(), kvp[1].Trim());
            }
        }
    }
}