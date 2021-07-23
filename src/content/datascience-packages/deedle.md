---
package-name: Deedle
package-logo: https://api.nuget.org/v3-flatcontainer/deedle/2.3.0/icon
package-nuget-link: https://www.nuget.org/packages/Deedle/
package-github-link: https://www.github.com/fslaborg/Deedle
package-documentation-link: https://fslab.org/Deedle/
package-description: Deedle is an easy to use library for data and time series manipulation and for scientific programming.
#package-posts-link: optional
package-tags: dataframe, data exploration, data access
---

Deedle implements efficient and robust frame and series data structures for accessing and manipulating structured data. 

It supports handling of missing values, aggregations, grouping, joining, statistical functions and more. For frames and series with ordered indices (such as time series), automatic alignment is also available.

Here is a short snippet on how to read and manipulate an online data source (HTTP requests are done with [FSharp.Data](https://github.com/fsprojects/FSharp.Data/)).

It reads the boston housing data set csv file from an online data source and 

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

