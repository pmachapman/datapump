# Conglomo Data Pump
An application for extracting data from a database using a SQL query, and writing that query to a spreadsheet file.

This program can be used in batch files to automate data export from your database, or to extract data from a one off query.

## Supported databases

* Firebird
* Microsoft SQL Server
* MySQL

## Supported output files

* CSV
* Excel 97-2003
* Excel 2007+

## Usage

`DATAPUMP.EXE [database type] [file type] [connection string] [sql file] [output file]`

## Notes

* The database and file type parameters usually do not need to be entered
* The SQL file should end in .sql
* The SQL file should contain only one SELECT statement, and no other queries or commands
* The output file should end in the correct file extension (.csv, .xls, .xlsx)
* If the connection string contains a clear identifier to the datbase file type (i.e. .fdb, .gdb, .mdf), you may not need to specify the database type
* Parameters can be entered in any order, so long as they are correctly formed
* The error code 0 will be emitted on success, 1 on an error

## Examples

`DATAPUMP "User=SYSDBA;Password=masterkey;Database=C:\db\mydb.gdb;DataSource=localhost;" "c:\data\get all companies.sql" companies.xlsx`

`DATAPUMP Firebird CSV "User=SYSDBA;Password=masterkey;Database=C:\db\mydb.gdb;DataSource=localhost;" c:\data\getcompanies.sql "c:\data\my companies.csv"`

`DATAPUMP MySQL XLSX "Server=localhost;Database=reports;User=root;Password=;" c:\data\names.sql c:\data\names.xlsx`

`DATAPUMP MSSQL XLS "Data Source=localhost;Initial Catalog=reports;User Id=sa;Password=;" c:\data\names.sql c:\data\names.xls`

`DATAPUMP "Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=C:\db\website.mdf;Integrated Security=SSPI;MultipleActiveResultSets=true;AttachDBFilename=C:\db\website.mdf" c:\data\names.sql c:\data\names.xlsx`
