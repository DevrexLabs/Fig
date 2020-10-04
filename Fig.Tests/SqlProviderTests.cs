using Fig.SqlSettings;
using NUnit.Framework;
using System.Collections.Generic;
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

        private SqlSettingsSource _settings;

        [SetUp]
        public void SetUp()
        {
            using (var con = GetPrefilledMemoryDatabase())
            {
                _settings = new SqlSettingsSource("", "", "SELECT Key, Value FROM Settings");
            }  
        }

        [Test]
        public void SqlSettingsSourceGetSettingsCorrect()
        {
            var settings = _settings
                .ToSettingsDictionary();

            foreach (var kvp in _defaultSettingValues)
            {
                Assert.AreEqual(kvp.Value, settings[kvp.Key]);
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
            return con;
        }
    }
}
