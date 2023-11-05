---
package-name: SQLProvider
package-logo: https://api.nuget.org/v3-flatcontainer/sqlprovider/1.3.11/icon
package-nuget-link: https://www.nuget.org/packages/SQLProvider/
package-github-link: https://github.com/fsprojects/SQLProvider
package-documentation-link: https://fsprojects.github.io/SQLProvider/
package-description: An F# Type Provider providing strongly typed access to an SQL database. The type provider automatically discovers available database schema, tables, columns, rows and makes returning data easily accessible from F#.
#package-posts-link: optional
package-tags: sql, database, type provider
---

The SQLProvider will help you work with databases, but there are some requirements to be set up on your system:
SQLProvider will need database drivers set up according to your current environment

Below is a simple script example that demonstrates using SQLProvider:

```fsharp
#r "nuget: Microsoft.Data.SqlClient"
#r "nuget: SQLProvider"

open FSharp.Data.Sql
open FSharp.Data.Sql.Common

[<Literal>]
let databaseType = DatabaseProviderTypes.MSSQLSERVER // POSTGRESQL, MYSQL, ...
[<Literal>]
let compileTimeConnectionString = "Data Source=localhost;Initial Catalog=HR; Integrated Security=True"

// create a type alias with the connection string and database vendor settings
type Sql = SqlDataProvider< 
              ConnectionString = compileTimeConnectionString,
              DatabaseVendor = databaseType,
              //ResolutionPath = @"c:\\myDatabaseDrivers\", // if you use other DatabaseProviderType, you need to set this
              UseOptionTypes = Common.NullableColumnType.VALUE_OPTION
              >

let runtimeConnectionString = compileTimeConnectionString
let dbContext = Sql.GetDataContext runtimeConnectionString 

let data =
   query { 
       for d in dbContext.Dbo.Countries do
       where (d.CountryName.IsSome)
       select d.CountryName.Value
   }  |> Seq.toList

```
