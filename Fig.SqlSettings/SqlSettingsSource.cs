using System.Collections.Generic;
using System.Data;

namespace Fig.SqlSettings
{
    public class SqlSettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string> _sqlSettingsDictionary;
        private readonly IDbConnection _connection;
        private readonly IDbCommand _command;
        private readonly bool _disposeConnection;

        public SqlSettingsSource(IDbConnection connection, IDbCommand command, bool disposeConnection = false)
        {
            _sqlSettingsDictionary = new Dictionary<string, string>();
            _connection = connection;
            _command = command;
            _disposeConnection = disposeConnection;
        }

        /// <summary>
        /// Opens a database connection to retrieve the settings.
        /// Connection will only be closed if the initial state of the connection was closed.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<(string, string)> GetSettings()
        {
            var wasClosed = _connection.State == ConnectionState.Closed;
            if (wasClosed)
            {
                _connection.Open();
            }

            using (IDataReader reader = _command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.FieldCount != 2 || reader[0] == null || reader[1] == null) continue;
                    _sqlSettingsDictionary.Add(reader[0].ToString(), reader[1].ToString());
                }
            }

            if (wasClosed)
            {
                _connection.Close();
            }

            if (_disposeConnection)
            {
                _connection.Dispose();
            }

            foreach (var setting in _sqlSettingsDictionary)
                yield return (setting.Key, setting.Value);
        }
    }
}
