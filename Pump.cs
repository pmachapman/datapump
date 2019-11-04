// -----------------------------------------------------------------------
// <copyright file="Pump.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.Data;
    using System.IO;
    using System.Security;
    using System.Threading.Tasks;
    using FirebirdSql.Data.FirebirdClient;

    /// <summary>
    /// The main data pump.
    /// </summary>
    public static class Pump
    {
        /// <summary>
        /// Executes the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The asynchronous task.</returns>
        /// <exception cref="ArgumentException">Thrown when the configration is invalid.</exception>
        public static async Task ExecuteAsync(PumpConfiguration configuration)
        {
            if (configuration != default && configuration.IsValid())
            {
                try
                {
                    // Execute the Firebird query
                    var data = await ExecuteFirebirdQueryAsync(configuration.ConnectionString, File.ReadAllText(configuration.SqlFile)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException
                        || ex is ArgumentNullException
                        || ex is DirectoryNotFoundException
                        || ex is FbException
                        || ex is FileNotFoundException
                        || ex is IOException
                        || ex is NotSupportedException
                        || ex is PathTooLongException
                        || ex is SecurityException
                        || ex is UnauthorizedAccessException)
                    {
                        throw new ArgumentException(Properties.Resources.PumpFailure, ex);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                throw new ArgumentException(Properties.Resources.InvalidConfiguration);
            }
        }

        /// <summary>
        /// Executes the firebird query asynchronous.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sql">The SQL.</param>
        /// <returns>The data table.</returns>
        private static async Task<DataTable> ExecuteFirebirdQueryAsync(string connectionString, string sql)
        {
            if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(sql))
            {
                using FbConnection connection = new FbConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                using FbDataAdapter adapter = new FbDataAdapter(sql, connection);
                DataSet data = new DataSet();
                adapter.Fill(data);
                await connection.CloseAsync().ConfigureAwait(false);
                return data.Tables[0];
            }
            else
            {
                throw new ArgumentException(Properties.Resources.InvalidConfiguration);
            }
        }
    }
}
