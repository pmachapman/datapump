// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.IO;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
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
            if (configuration != default && args != default && (args.Length == 3 || args.Length == 4))
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
                            else if (arg.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                            {
                                configuration.SqlFile = arg;
                            }
                            else if (arg.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                            {
                                // We only support CSV files at present
                                configuration.OutputFile = arg;
                            }
                            else if (arg.Contains(".gdb", StringComparison.OrdinalIgnoreCase)
                                    || arg.Contains(".fdb", StringComparison.OrdinalIgnoreCase))
                            {
                                // Firebird connection string
                                configuration.Database = Database.Firebird;
                                configuration.ConnectionString = arg;
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
