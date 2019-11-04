// -----------------------------------------------------------------------
// <copyright file="FileType.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    /// <summary>
    /// The file type.
    /// </summary>
    public enum FileType : int
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
}
