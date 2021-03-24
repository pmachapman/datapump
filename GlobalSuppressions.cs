// -----------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Conglomo">
// Copyright 2019-2021 Conglomo Limited. Please see LICENSE for license details.
// </copyright>
// -----------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Of course the SQL comes from the user", Scope = "member", Target = "~M:Conglomo.DataPump.Pump.ExecuteQueryAsync(Conglomo.DataPump.Database,System.String,System.String)")]
