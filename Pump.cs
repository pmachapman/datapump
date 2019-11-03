// -----------------------------------------------------------------------
// <copyright file="Pump.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    using System;

    /// <summary>
    /// The main data pump.
    /// </summary>
    public static class Pump
    {
        /// <summary>
        /// Executes the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="ArgumentException">Thrown when the configration is invalid.</exception>
        public static void Execute(PumpConfiguration configuration)
        {
            if (configuration != default && configuration.IsValid())
            {
                // TODO: Execute the data pump configuration
            }
            else
            {
                throw new ArgumentException(Properties.Resources.InvalidConfiguration);
            }
        }
    }
}
