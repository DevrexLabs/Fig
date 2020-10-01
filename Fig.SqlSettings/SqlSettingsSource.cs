using System.Collections.Generic;
using System.Data;

namespace Fig.SqlSettings
{
    public class SqlSettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string> _sqlSettingsDictionary;

        public SqlSettingsSource(IDbConnection connection, IDbCommand command)
        {
            _sqlSettingsDictionary = new Dictionary<string, string>();

            var stateWasClosed = connection.State == ConnectionState.Closed;
            if (stateWasClosed)
            {
                connection.Open();
            }

            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount != 2 || reader[0] == null || reader[1] == null) continue;
                    _sqlSettingsDictionary.Add(reader[0].ToString(), reader[1].ToString());
                }
            }

            if (stateWasClosed)
            {
                connection.Close();
            }
        }

        protected override IEnumerable<(string, string)> GetSettings()
        {
            foreach (var setting in _sqlSettingsDictionary)
                yield return (setting.Key, setting.Value);
        }
    }
}
