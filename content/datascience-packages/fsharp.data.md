---
package-name: FSharp.Data
package-logo: https://api.nuget.org/v3-flatcontainer/fsharp.data/4.1.1/icon
package-nuget-link: https://www.nuget.org/packages/FSharp.Data
package-github-link: https://github.com/fsprojects/FSharp.Data
package-documentation-link: https://fsprojects.github.io/FSharp.Data/
package-description: The FSharp.Data package contains type providers and utilities to accesscommon data formats (CSV, HTML, JSON and XML in your F# applications and scripts. It also contains  helpers for parsing CSV, HTML and JSON files and for sending HTTP requests.
#package-posts-link: optional
package-tags: data access, type providers, http
---

FSharp.Data is a multipurpose project for data access from many different file formats. Most of this is done via type providers.

We recommend using the `Http` module provided by FSharp.data to download data sources via Http and then convert them to deedle data frames via `Frame.ReadCsvString`:

```fsharp
#r "nuget: FSharp.Data"
#r "nuget: Deedle"

open FSharp.Data
open Deedle

let rawData = Http.RequestString @"https://raw.githubusercontent.com/dotnet/machinelearning/master/test/data/housing.txt"

// get a frame containing the values of houses at the charles river only
let df = 
    Frame.ReadCsvString(rawData, separators="\t")
    |> Frame.sliceCols ["MedianHomeValue"; "CharlesRiver"]
    |> Frame.filterRowValues (fun s -> s.GetAs<bool>("CharlesRiver"))

df.Print()
```
