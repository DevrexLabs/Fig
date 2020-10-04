using Fig.SqlSettings;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Fig.Test
{
    public class SqlProviderTests
    {
        private readonly Dictionary<string, string> _defaultSettingValues = new Dictionary<string, string>
        {
            {"DataDrive", "D:/" },
            {"CompanyName", "DevrexLabs" }
        };

        private SettingsDictionary _settings;

        [SetUp]
        public void SetUp()
        {
            // The using will dispose the connection, and all data from the memory database.
            using (var con = GetPrefilledMemoryDatabase())
            {
                _settings = new SqlSettingsSource(con, new SQLiteCommand("SELECT Key, Value FROM Settings", con))
                    .ToSettingsDictionary();
            }  
        }

        [Test]
        public void SqlSettingsSourceGetSettingsCorrect()
        {
            foreach (var kvp in _defaultSettingValues)
            {
                Assert.AreEqual(kvp.Value, _settings[kvp.Key]);
            }
        }

        private SQLiteConnection GetPrefilledMemoryDatabase()
        {
            var con = new SQLiteConnection("Data Source=:memory:");
            con.Open();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "CREATE TABLE Settings(Key VARCHAR(255) NOT NULL UNIQUE, Value VARCHAR(255) NOT NULL)";
                cmd.ExecuteNonQuery();

                foreach (var defaultSetting in _defaultSettingValues)
                {
                    cmd.CommandText = $"INSERT INTO Settings(Key, Value) VALUES('{defaultSetting.Key}','{defaultSetting.Value}')";
                    cmd.ExecuteNonQuery();
                }
            }
            // con must remain open, because 'in memory' db ceased to exist as soon as it's closed
            return con;
        }
    }
}
