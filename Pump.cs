// -----------------------------------------------------------------------
// <copyright file="Pump.cs" company="Conglomo">
// Copyright 2019-2020 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Threading.Tasks;
    using FirebirdSql.Data.FirebirdClient;
    using Microsoft.Data.SqlClient;
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
                switch (configuration.FileType)
                {
                    case FileType.CSV:
                        {
                            // Execute the query
                            using var writer = File.CreateText(configuration.OutputFile);
                            bool firstRow = true;
                            await foreach (var values in ExecuteQueryAsync(configuration.Database, configuration.ConnectionString, File.ReadAllText(configuration.SqlFile)).ConfigureAwait(false))
                            {
                                bool firstColumn = true;
                                for (int i = 0; i < values.Length; i++)
                                {
                                    string? text = values[i]?.ToString();
                                    if (text != default)
                                    {
                                        // Check for SYLK workaround
                                        if (firstRow && firstColumn && text == "ID")
                                        {
                                            await writer.WriteAsync("\"ID\"").ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await writer.WriteAsync(text.EncodeCsvField()).ConfigureAwait(false);
                                        }

                                        // If this is not the last column, write the comma
                                        if (i < values.Length - 1)
                                        {
                                            await writer.WriteAsync(',').ConfigureAwait(false);
                                        }
                                    }

                                    // Clear the first column flag
                                    firstColumn = false;
                                }

                                // Write and flush
                                await writer.WriteLineAsync().ConfigureAwait(false);
                                await writer.FlushAsync().ConfigureAwait(false);

                                // Clear the first row flag
                                firstRow = false;
                            }

                            writer.Close();
                            return;
                        }

                    case FileType.XLS:
                        await WriteSpreadsheet(configuration, new HSSFWorkbook()).ConfigureAwait(false);
                        return;
                    case FileType.XLSX:
                        await WriteSpreadsheet(configuration, new XSSFWorkbook()).ConfigureAwait(false);
                        return;
                }
            }

            throw new ArgumentException(Properties.Resources.InvalidConfiguration);
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
            // Ensure we have enough to start with
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException(Properties.Resources.InvalidConfiguration);
            }

            // Query the specified database
            switch (database)
            {
                case Database.Firebird:
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
                        break;
                    }

                case Database.MSSQL:
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
                        break;
                    }

                case Database.MySQL:
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
                        break;
                    }
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
            // Set up the workbook
            using var fs = new FileStream(configuration.OutputFile, FileMode.Create, FileAccess.Write);
            var sheet = workbook.CreateSheet("Output");

            // Set up the date format
            var dataFormatCustom = workbook.CreateDataFormat();
            var style = workbook.CreateCellStyle();
            style.DataFormat = dataFormatCustom.GetFormat("d/MM/yyyy");

            int i = 0;
            await foreach (var values in ExecuteQueryAsync(configuration.Database, configuration.ConnectionString, File.ReadAllText(configuration.SqlFile)).ConfigureAwait(false))
            {
                var row = sheet.CreateRow(i++);
                for (int j = 0; j < values.Length; j++)
                {
                    if (values[j] != default)
                    {
                        if (values[j] is DateTime date)
                        {
                            var cell = row.CreateCell(j);
                            cell.SetCellValue(date);
                            cell.CellStyle = style;
                        }
                        else if (values[j] is string general)
                        {
                            row.CreateCell(j).SetCellValue(general);
                        }
                        else
                        {
                            row.CreateCell(j).SetCellValue(values[j].ToString());
                        }
                    }
                }
            }

            workbook.Write(fs);
        }
    }
}
