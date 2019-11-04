﻿// -----------------------------------------------------------------------
// <copyright file="Database.cs" company="Conglomo">
// Copyright 2019 Conglomo Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Conglomo.DataPump
{
    /// <summary>
    /// The database type.
    /// </summary>
    public enum Database : int
    {
        /// <summary>
        /// No database selected
        /// </summary>
        None = 0,

        /// <summary>
        /// The Firebird database.
        /// </summary>
        Firebird = 1,

        /// <summary>
        /// The Microsoft SQL Server database.
        /// </summary>
        MSSQL = 2,

        /// <summary>
        /// The MySQL database.
        /// </summary>
        MySQL = 3,
    }
}
