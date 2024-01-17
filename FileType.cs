// -----------------------------------------------------------------------
// <copyright file="FileType.cs" company="Conglomo">
// Copyright 2019-2024 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump;

/// <summary>
/// The file type.
/// </summary>
public enum FileType
{
    /// <summary>
    /// No file type to output.
    /// </summary>
    None = 0,

    /// <summary>
    /// A CSV file.
    /// </summary>
    CSV = 1,

    /// <summary>
    /// An XLS file.
    /// </summary>
    XLS = 2,

    /// <summary>
    /// An XLSX file.
    /// </summary>
    XLSX = 3,
}
