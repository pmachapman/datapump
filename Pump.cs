﻿// -----------------------------------------------------------------------
// <copyright file="Pump.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using System.Security;
    using System.Threading.Tasks;
    using FirebirdSql.Data.FirebirdClient;
    using MySql.Data.MySqlClient;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;

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
                    if (configuration.FileType == FileType.CSV)
                    {
                        // Execute the query
                        using var writer = File.CreateText(configuration.OutputFile);
                        await foreach (var values in ExecuteQueryAsync(configuration.Database, configuration.ConnectionString, File.ReadAllText(configuration.SqlFile)).ConfigureAwait(false))
                        {
                            foreach (var value in values)
                            {
                                string? text = value?.ToString();
                                if (text != default)
                                {
                                    await writer.WriteAsync(text.EncodeCsvField()).ConfigureAwait(false);
                                    await writer.WriteAsync(',').ConfigureAwait(false);
                                }
                            }

                            await writer.WriteLineAsync().ConfigureAwait(false);
                            await writer.FlushAsync().ConfigureAwait(false);
                        }

                        writer.Close();
                    }
                    else if (configuration.FileType == FileType.XLS)
                    {
                        await WriteSpreadsheet(configuration, new HSSFWorkbook()).ConfigureAwait(false);
                    }
                    else if (configuration.FileType == FileType.XLSX)
                    {
                        await WriteSpreadsheet(configuration, new XSSFWorkbook()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException
                        || ex is ArgumentNullException
                        || ex is DirectoryNotFoundException
                        || ex is FbException
                        || ex is FileNotFoundException
                        || ex is IOException
                        || ex is MySqlException
                        || ex is NotSupportedException
                        || ex is PathTooLongException
                        || ex is SecurityException
                        || ex is SqlException
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
        /// Executes the firebird query asynchronously.
        /// </summary>
        /// <param name="database">The database type.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sql">The SQL.</param>
        /// <returns>
        /// Each row, starting with the column names.
        /// </returns>
        /// <exception cref="ArgumentException">There was was invalid data pump configuration.</exception>
        private static async IAsyncEnumerable<object[]> ExecuteQueryAsync(Database database, string connectionString, string sql)
        {
            if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(sql))
            {
                if (database == Database.Firebird)
                {
                    // Open the connection and run the query
                    using var connection = new FbConnection(connectionString);
                    await connection.OpenAsync().ConfigureAwait(false);
                    using var command = new FbCommand(sql, connection);
                    using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                    // Execute the reader
                    await foreach (var values in ExecuteReaderAsync(reader))
                    {
                        yield return values;
                    }

                    // Close the query and connection
                    await reader.CloseAsync().ConfigureAwait(false);
                    await command.DisposeAsync().ConfigureAwait(false);
                    await connection.CloseAsync().ConfigureAwait(false);
                }
                else if (database == Database.MSSQL)
                {
                    // Open the connection and run the query
                    using var connection = new SqlConnection(connectionString);
                    await connection.OpenAsync().ConfigureAwait(false);
                    using var command = new SqlCommand(sql, connection);
                    using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                    // Execute the reader
                    await foreach (var values in ExecuteReaderAsync(reader))
                    {
                        yield return values;
                    }

                    // Close the query and connection
                    await reader.CloseAsync().ConfigureAwait(false);
                    await command.DisposeAsync().ConfigureAwait(false);
                    await connection.CloseAsync().ConfigureAwait(false);
                }
                else if (database == Database.MySQL)
                {
                    // Open the connection and run the query
                    using var connection = new MySqlConnection(connectionString);
                    await connection.OpenAsync().ConfigureAwait(false);
                    using var command = new MySqlCommand(sql, connection);
                    using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                    // Execute the reader
                    await foreach (var values in ExecuteReaderAsync(reader))
                    {
                        yield return values;
                    }

                    // Close the query and connection
                    await reader.CloseAsync().ConfigureAwait(false);
                    await command.DisposeAsync().ConfigureAwait(false);
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
            else
            {
                throw new ArgumentException(Properties.Resources.InvalidConfiguration);
            }
        }

        /// <summary>
        /// Executes the reader asynchronously.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>
        /// Each row from the reader, including the field names as the first row.
        /// </returns>
        private static async IAsyncEnumerable<object[]> ExecuteReaderAsync(DbDataReader reader)
        {
            // Get the field names
            string[] fieldNames = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                fieldNames[i] = reader.GetName(i);
            }

            // Return the field names
            yield return fieldNames;

            // Return the values
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                yield return values;
            }
        }

        /// <summary>
        /// Writes the spreadsheet.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="workbook">The workbook.</param>
        /// <returns>The asynchronous task.</returns>
        private static async Task WriteSpreadsheet(PumpConfiguration configuration, IWorkbook workbook)
        {
            using var fs = new FileStream(configuration.OutputFile, FileMode.Create, FileAccess.Write);
            var sheet = workbook.CreateSheet("Output");
            int i = 0;
            await foreach (var values in ExecuteQueryAsync(configuration.Database, configuration.ConnectionString, File.ReadAllText(configuration.SqlFile)).ConfigureAwait(false))
            {
                var row = sheet.CreateRow(i++);
                for (int j = 0; j < values.Length; j++)
                {
                    row.CreateCell(j).SetCellValue(values[j]?.ToString());
                }
            }

            workbook.Write(fs);
        }
    }
}
