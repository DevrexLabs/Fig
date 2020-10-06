using Fig.SqlSettings;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Fig.AppSettingsXml;
using System.Linq;

namespace Fig.Test
{
    public class SqlProviderTests
    {
        private readonly Dictionary<string, string> _defaultSettingValues = new Dictionary<string, string>
        {
            {"DataDrive", "D:/" },
            {"CompanyName", "DevrexLabs" }
        };

        [SetUp]
        public void SetUp()
        {
            // Check SqlProviderTests.db if all key and values match the _defaultSettingValues
            var defaultSettings = new Dictionary<string, string>();
            using (var con = new SQLiteConnection("DataSource=SqlProviderTests.db"))
            {
                var command = new SQLiteCommand("SELECT Key,Value FROM Settings", con);
                con.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.FieldCount != 2 || reader[0] == null || reader[1] == null) continue;
                        defaultSettings.Add(reader[0].ToString(), reader[1].ToString());
                    }
                }

                if (!defaultSettings.SequenceEqual(_defaultSettingValues))
                {
                    // Re-build the table data
                    command.CommandText = "DELETE FROM Settings";
                    command.ExecuteNonQuery();

                    foreach (var defaultSetting in _defaultSettingValues)
                    {
                        command.CommandText = $"INSERT INTO Settings(Key, Value) VALUES('{defaultSetting.Key}','{defaultSetting.Value}')";
                        command.ExecuteNonQuery();
                    }
                }
                con.Close();
            }
        }

        [Test]
        public void SqlSettingsSourceGetSettingsCorrect()
        {
            using (var con = GetPrefilledMemoryDatabase())
            {
                var settings = new SqlSettingsSource(con, new SQLiteCommand("SELECT Key, Value FROM Settings", con))
                    .ToSettingsDictionary();

                foreach (var kvp in _defaultSettingValues)
                {
                    Assert.AreEqual(kvp.Value, settings[kvp.Key]);
                }
            }
        }

        [Test]
        public void SettingsBuilderUseSqlOpenConnectionProvidedWithJsonCorrect()
        {
            using (var con = GetPrefilledMemoryDatabase())
            {
                var settings = new SettingsBuilder()
                    .UseAppSettingsJson(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), true)
                    .UseSql(con, "SELECT Key, Value FROM Settings")
                    .Build();

                foreach (var kvp in _defaultSettingValues)
                {
                    Assert.AreEqual(kvp.Value, settings.Get<string>(kvp.Key));
                }
            }
        }

        [Test]
        public void SettingsBuilderUseSqlOpenConnectionProvidedWithXmlCorrect()
        {
            using (var con = GetPrefilledMemoryDatabase())
            {
                var settings = new SettingsBuilder()
                    .UseAppSettingsXml()
                    .UseSql(con, "SELECT Key, Value FROM Settings")
                    .Build();

                foreach (var kvp in _defaultSettingValues)
                {
                    Assert.AreEqual(kvp.Value, settings.Get<string>(kvp.Key));
                }
            }
        }

        [Test]
        public void SettingsBuilderUseSqlNoOpenConnectionProvidedCorrect()
        {
            var settings = new SettingsBuilder()
                .UseAppSettingsJson(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), true)
                .UseSql<SQLiteConnection>("SELECT Key, Value FROM Settings", "ConnectionStrings.SQLiteConnection")
                .Build();

            foreach (var kvp in _defaultSettingValues)
            {
                Assert.AreEqual(kvp.Value, settings.Get<string>(kvp.Key));
            }
        }

        private SQLiteConnection GetPrefilledMemoryDatabase()
        {
            var con = new SQLiteConnection("DataSource=:memory:");
            con.Open();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "CREATE TABLE Settings(Key TEXT NOT NULL UNIQUE, Value TEXT NOT NULL)";
                cmd.ExecuteNonQuery();

                foreach (var defaultSetting in _defaultSettingValues)
                {
                    cmd.CommandText = $"INSERT INTO Settings(Key, Value) VALUES('{defaultSetting.Key}','{defaultSetting.Value}')";
                    cmd.ExecuteNonQuery();
                }
            }
            // con must remain open, because 'in memory' db ceases to exist as soon as it's closed
            return con;
        }
    }
}
