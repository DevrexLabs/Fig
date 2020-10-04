using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Fig.SqlSettings
{
    public class SqlSettingsSource : SettingsSource
    {
        private readonly string _connectionString, _providerName, _query;

        public SqlSettingsSource(string connectionString, string providerName, string query)
        {
            _connectionString = connectionString;
            _providerName = providerName;
            _query = query;
        }

        protected override IEnumerable<(string, string)> GetSettings()
        {
            var dbConnection = CreateDbConnection();
            var settings = ExecuteDbCommand(dbConnection);

            foreach (var setting in settings)
                yield return (setting.Key, setting.Value);
        }

        /// <summary>
        /// Returns a DbConnection on success; null on failure.
        /// </summary>
        /// <returns></returns>
        private DbConnection CreateDbConnection()
        {
            DbConnection connection = null;

            // Create the DbProviderFactory and DbConnection.
            if (_connectionString != null)
            {
                try
                {
                    DbProviderFactory factory = DbProviderFactories.GetFactory(_providerName);

                    connection = factory.CreateConnection();
                    connection.ConnectionString = _connectionString;
                }
                catch
                {
                    // Set the connection to null if it was created.
                    if (connection != null)
                    {
                        connection = null;
                    }
                }
            }
            return connection;
        }

        /// <summary>
        /// Executes a 2 column query on the DbConnection.
        /// Returns a dictionary based on first column as key and second as value.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private Dictionary<string, string> ExecuteDbCommand(DbConnection connection)
        {
            // Check for valid DbConnection object.
            if (connection != null)
            {
                using (connection)
                {
                    // Open the connection.
                    connection.Open();

                    // Create and execute the DbCommand.
                    DbCommand command = connection.CreateCommand();
                    command.CommandText = _query;

                    using (var reader = command.ExecuteReader())
                    {
                        var dictionary = new Dictionary<string, string>();
                        while (reader.Read())
                        {
                            if (reader.FieldCount != 2 || (reader[0] != null && reader[1] != null))
                            {
                                dictionary.Add(reader[0].ToString(), reader[1].ToString());
                            }
                        }
                        return dictionary;
                    }
                }
            }
            else
            {
                throw new Exception($"Failed: DbConnection is null for [{_providerName}] [{_connectionString}].");
            }
        }
    }
}
