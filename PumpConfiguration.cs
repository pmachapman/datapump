// -----------------------------------------------------------------------
// <copyright file="PumpConfiguration.cs" company="Conglomo">
// Copyright 2019-2025 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump;

/// <summary>
/// The pump configuration.
/// </summary>
/// <remarks>
/// This is most often filled via command line arguments.
/// </remarks>
public class PumpConfiguration
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    /// <value>
    /// The connection string.
    /// </value>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database.
    /// </summary>
    /// <value>
    /// The database.
    /// </value>
    public Database Database { get; set; }

    /// <summary>
    /// Gets or sets the type of the file.
    /// </summary>
    /// <value>
    /// The output file type.
    /// </value>
    public FileType FileType { get; set; }

    /// <summary>
    /// Gets or sets the output file.
    /// </summary>
    /// <value>
    /// The output file.
    /// </value>
    public string OutputFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SQL file.
    /// </summary>
    /// <value>
    /// The SQL file.
    /// </value>
    public string SqlFile { get; set; } = string.Empty;
}
