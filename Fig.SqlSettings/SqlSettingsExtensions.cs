using System;
using System.Data;

namespace Fig.SqlSettings
{
    public static class SqlSettingsExtensions
    {
        /// <summary>
        /// Provides a way to retrieve settings through an sql connection.
        /// </summary>
        /// <param name="settingsBuilder"></param>
        /// <param name="dbConnection"></param>
        /// <param name="dbCommand"></param>
        /// <param name="connectionStringKey"></param>
        /// <returns></returns>
        public static SettingsBuilder UseSql(this SettingsBuilder settingsBuilder, IDbConnection dbConnection, 
            string query = null, string connectionStringKey = null, bool disposeConnection = true)
        {
            if (connectionStringKey != null)
            {
                var settings = settingsBuilder.Build();
                var connectionString = settings.Get(connectionStringKey);
                dbConnection.ConnectionString = connectionString ?? 
                    throw new Exception($"ConnectionString with key '{connectionString}' was not found in the settings.");
            }

            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query ?? "SELECT key,value FROM FigSettings";
            var settingsDictionary = new SqlSettingsSource(dbConnection, dbCommand, disposeConnection).ToSettingsDictionary();           
            return settingsBuilder.UseSettingsDictionary(settingsDictionary);
        }

        /// <summary>
        /// Provides a way to retrieve settings through an sql connection.
        /// The dbConnection is disposed automatically.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingsBuilder"></param>
        /// <param name="query"></param>
        /// <param name="connectionStringKey"></param>
        /// <returns></returns>
        public static SettingsBuilder UseSql<T>(this SettingsBuilder settingsBuilder,
    string query = null, string connectionStringKey = null) where T : IDbConnection, new()
        {
            return settingsBuilder.UseSql(Activator.CreateInstance<T>(), query, connectionStringKey, true);
        }
    }
}
