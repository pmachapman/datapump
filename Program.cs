// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The main program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            if (args != default && args.FirstOrDefault() == "/?")
            {
                DisplayHelp();
            }

            // Parse the command line arguments
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
                    attributes = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
                    if (attributes.Any())
                    {
                        string version = ((AssemblyVersionAttribute)attributes.First()).Version;
                        Console.WriteLine(productName + " " + version);
                    }
                    else
                    {
                        Console.WriteLine(productName);
                    }
                }

                // Display the copyright message
                attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Any())
                {
                    Console.WriteLine(((AssemblyCopyrightAttribute)attributes.First()).Copyright);
                }

                // Display usage
                Console.WriteLine(Properties.Resources.UsageText);

                return;
            }
        }
    }
}
