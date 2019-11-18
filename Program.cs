// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// The main program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// The error code.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            if (args == default
                || !args.Any()
                || args.FirstOrDefault() == "/?"
                || args.FirstOrDefault() == "-?"
                || args.FirstOrDefault()?.ToUpperInvariant() == "-H"
                || args.FirstOrDefault()?.ToUpperInvariant() == "--HELP")
            {
                DisplayHelp();
                return 0;
            }

            // Parse the command line arguments
            PumpConfiguration configuration = new PumpConfiguration();
            configuration.ParseArguments(args);
            if (configuration.IsValid())
            {
                try
                {
                    await Pump.ExecuteAsync(configuration).ConfigureAwait(false);
                }
                catch (ArgumentException ex)
                {
                    DisplayHelp();
                    Console.WriteLine();
                    Console.WriteLine(ex);
                    return 1;
                }

                return 0;
            }
            else
            {
                // Default to an error
                DisplayHelp();
                Console.WriteLine();
                Console.WriteLine(Properties.Resources.InvalidArguments);
                return 1;
            }
        }

        /// <summary>
        /// Displays the help text.
        /// </summary>
        private static void DisplayHelp()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != default)
            {
                // Display the product name
                var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Any())
                {
                    string productName = ((AssemblyProductAttribute)attributes.First()).Product;
                    string version = string.Empty;
                    attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                    if (attributes.Any())
                    {
                        Version ver = new Version(((AssemblyFileVersionAttribute)attributes.First()).Version);
                        version = ver.Major.ToString(CultureInfo.InvariantCulture) + "." + ver.Minor.ToString(CultureInfo.InvariantCulture);
                        if (ver.Build > 0)
                        {
                            version += "." + ver.Build.ToString(CultureInfo.InvariantCulture);
                        }
                    }

                    string companyName = string.Empty;
                    attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    if (attributes.Any())
                    {
                        companyName = ((AssemblyCompanyAttribute)attributes.First()).Company;
                    }

                    Console.WriteLine((companyName + " " + productName + " " + version).Trim());
                }

                // Display the copyright message
                attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Any())
                {
                    Console.WriteLine(((AssemblyCopyrightAttribute)attributes.First()).Copyright);
                }

                // Display usage
                Console.WriteLine();
                Console.WriteLine(Properties.Resources.UsageText);
            }
        }
    }
}
