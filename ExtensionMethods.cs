// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Encodes a string for a CSV field.
        /// </summary>
        /// <param name="field">The field to encode.</param>
        /// <param name="separator">The separator (defaults to a comma).</param>
        /// <returns>
        /// A string suitable to output to a CSV file.
        /// </returns>
        public static string EncodeCsvField(this string field, char separator = ',')
        {
            // Set up the string builder
            StringBuilder sb = new StringBuilder(field);

            // Some fields with special characters must be embedded in double quotes
            bool embedInQuotes = false;

            // Embed in quotes to preserve leading/trailing whitespace
            if (sb.Length > 0 && (sb[0] == ' ' || sb[0] == '\t' || sb[sb.Length - 1] == ' ' || sb[sb.Length - 1] == '\t'))
            {
                embedInQuotes = true;
            }

            // If we have not yet found a reason to embed in quotes
            if (!embedInQuotes)
            {
                for (int i = 0; i < sb.Length; i++)
                {
                    // Embed in quotes to preserve commas, line-breaks etc.
                    if (sb[i] == separator || sb[i] == '\r' || sb[i] == '\n' || sb[i] == '"')
                    {
                        embedInQuotes = true;
                        break;
                    }
                }
            }

            // If the field itself has quotes, they must each be represented by a pair of consecutive quotes.
            if (embedInQuotes)
            {
                sb.Replace("\"", "\"\"");
                return "\"" + sb.ToString() + "\"";
            }
            else
            {
                return sb.ToString();
            }
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="args">The command line arguments.</param>
        /// <returns>
        /// The pump configuration completed with the command line arguments.
        /// </returns>
        public static PumpConfiguration ParseArguments(this PumpConfiguration configuration, string[] args)
        {
            if (configuration != default && args != default)
            {
                foreach (string arg in args)
                {
                    try
                    {
                        if (arg != default)
                        {
                            if (Enum.IsDefined(typeof(Database), arg) && Enum.TryParse(arg, out Database database))
                            {
                                configuration.Database = database;
                            }
                            else if (Enum.IsDefined(typeof(FileType), arg) && Enum.TryParse(arg, out FileType fileType))
                            {
                                configuration.FileType = fileType;
                            }
                            else if (arg.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                            {
                                configuration.SqlFile = arg;
                            }
                            else if (arg.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                            {
                                configuration.FileType = FileType.CSV;
                                configuration.OutputFile = arg;
                            }
                            else if (arg.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                            {
                                configuration.FileType = FileType.XLS;
                                configuration.OutputFile = arg;
                            }
                            else if (arg.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                            {
                                configuration.FileType = FileType.XLSX;
                                configuration.OutputFile = arg;
                            }
                            else if (arg.Contains(".gdb", StringComparison.OrdinalIgnoreCase)
                                    || arg.Contains(".fdb", StringComparison.OrdinalIgnoreCase))
                            {
                                // Firebird connection string
                                configuration.ConnectionString = arg;
                                configuration.Database = Database.Firebird;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore errors
                        if (!(ex is ArgumentException
                            || ex is ArgumentNullException
                            || ex is InvalidOperationException))
                        {
                            throw;
                        }
                    }
                }
            }

            return configuration ?? new PumpConfiguration();
        }

        /// <summary>
        /// Returns true if the pump configuration is valid.
        /// </summary>
        /// <param name="configuration">The pump configuration.</param>
        /// <returns>
        ///   <c>true</c> if the specified pump configuration is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(this PumpConfiguration configuration)
        {
            // Check for empty values or an invalid SQL file
            if (configuration == default
                || configuration.Database == Database.None
                || string.IsNullOrWhiteSpace(configuration.ConnectionString)
                || configuration.FileType == FileType.None
                || string.IsNullOrWhiteSpace(configuration.OutputFile)
                || string.IsNullOrWhiteSpace(configuration.SqlFile)
                || !File.Exists(configuration.SqlFile))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
