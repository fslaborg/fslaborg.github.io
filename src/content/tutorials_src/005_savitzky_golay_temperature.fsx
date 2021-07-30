(***hide***)

(*
#frontmatter
---
title: Smoothing data with the Savitzky-Golay filter
category: datascience
authors: Kevin Frey
index: 4
---
*)

(***condition:prepare***)
#r "nuget: Fsharp.Data"
#r "nuget: FSharp.Stats"
#r "nuget: Plotly.NET, 2.0.0-preview.6"

(***condition:ipynb***)
#if IPYNB
#r "nuget: FSharp.Data"
#r "nuget: FSharp.Stats"
#r "nuget: Newtonsoft.JSON"
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.6"
#endif // IPYNB

(**
[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/{{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)


# Smoothing data with the Savitzky-Golay filter

_Summary:_ This tutorial demonstrates how to access a public dataset for temperature data with [FSharp.Data](https://fsprojects.github.io/FSharp.Data/), how to smoothe the data points with 
the Savitzky-Golay filter from [FSharp.Stats](https://fslab.org/FSharp.Stats/) and finally how to visualize the results with [Plotly.NET](https://plotly.net).

## Introduction: 

The Savitzky-Golay is a type of low-pass filter, particularly suited for smoothing noisy data. The main idea behind this approach is to make for each point a 
least-square fit with a polynomial of high order over a odd-sized window centered at the point. One advantage of the Savitzky-Golay filter is that, portions 
of high frequencies are not simply cut off, but are preserved due to the polynomial regression. This allows the filter to preserve properties of the distribution 
such as relative maxima, minima and dispersion, which are usually distorted by flattening or shifting by conventional methods such as moving average.

This is useful when trying to identify general trends in highly fluctuating data sets, or to smooth out noise to improve the ability to find minima and maxima of the data trend.
To showcase this we will plot a temperature dataset from the ["Deutsche Wetterdienst"](https://www.dwd.de/DE/leistungen/klimadatendeutschland/klimadatendeutschland.html), 
a german organization for climate data. We will do this for both the original data points and a smoothed version.

<center>

![windowed polynomial regression](https://upload.wikimedia.org/wikipedia/commons/8/89/Lissage_sg3_anim.gif)

The image shows the moving window for polynomial regression used in the Savitzky-Golay filter [@wikipedia](https://upload.wikimedia.org/wikipedia/commons/8/89/Lissage_sg3_anim.gif)

</center>

## Referencing packages

```fsharp
// Packages hosted by the Fslab community
#r "nuget: FSharp.Stats"
// third party .net packages 
#r "nuget: FSharp.Data"
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.6"
```

*)


(**
## Loading data

We will start by retrieving the data. This is done with the [FSharp.Data](https://fsprojects.github.io/FSharp.Data/) package 
and will return a single string in the original format.
*)

// Get data from Deutscher Wetterdienst
// Explanation for Abbreviations: https://www.dwd.de/DE/leistungen/klimadatendeutschland/beschreibung_tagesmonatswerte.html
let rawData = FSharp.Data.Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/WeatherDataAachen-Orsbach_daily_1year.txt"

// print first 1000 characters to console.
rawData.[..1000] |> printfn "%s"

(*** include-output ***)

(**

Currently the data set is not in a format, that is easily parsable. Normally you would try to use 
the Deedle package to read in the data into a a [Deedle](https://fslab.org/Deedle/) data frame. As this is not possible here, we will do some ugly formatting.

## Data Formatting/Parsing
*)

open System
open System.Text.RegularExpressions

/// Tuple of 4 data arrays representing the measured temperature for over a year.
let processedData = 
    // First separate the huge string in lines
    rawData.Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)
    // Skip the first 5 rows until the real data starts, also skip the last row (length-2) to remove a "</pre>" at the end
    |> fun arr -> arr.[5..arr.Length-2]
    |> Array.map (fun data -> 
        // Regex pattern that will match groups of whitespace
        let whitespacePattern = @"\s+"
        // This is needed to tell regex to replace hits with a tabulator
        let matchEval = MatchEvaluator(fun _ -> @"\t" )
        // The original data columns are separated by different amounts of whitespace.
        // Therefore, we need a flexible string parsing option to replace any amount of whitespace with a single tabulator.
        // This is done with the regex pattern above and the fsharp core library "System.Text.RegularExpressions" 
        let tabSeparated = Regex.Replace(data, whitespacePattern, matchEval)
        tabSeparated
        // Split each row by tabulator will return rows with an equal amount of values, which we can access.
        |> fun dataStr -> dataStr.Split([|@"\t"|], StringSplitOptions.RemoveEmptyEntries)
        |> fun dataArr -> 
            // Second value is the date of measurement, which we will parse to the DateTime type
            DateTime.ParseExact(dataArr.[1], "yyyyMMdd", Globalization.CultureInfo.InvariantCulture),
            // 5th value is minimal temperature at that date.
            float dataArr.[4],
            // 6th value is average temperature over 24 timepoints at that date.
            float dataArr.[5],
            // 7th value is maximal temperature at that date.
            float dataArr.[6]
    )
    // Sort by date
    |> Array.sortBy (fun (day,tn,tm,tx) -> day)
    // Unzip the array of value tuples, to make the different values easier accessible
    |> fun arr -> 
        arr |> Array.map (fun (day,tn,tm,tx) -> day.ToShortDateString()),
        arr |> Array.map (fun (day,tn,tm,tx) -> tm),
        arr |> Array.map (fun (day,tn,tm,tx) -> tx),
        arr |> Array.map (fun (day,tn,tm,tx) -> tn)

(*** include-value:processedData ***)

(**
## Exploring the data set with Plotly.NET

Next we create a create chart function with [Plotly.NET](https://plotly.net) to produce a visual representation of our data set.
*)

open Plotly.NET

// Because our data set is already rather wide we want to move the legend from the right side of the plot
// to the right center. As this function is not defined for fsharp we will use the underlying js bindings (https://plotly.com/javascript/legend/#positioning-the-legend-inside-the-plot).
// Declarative style in F# using underlying DynamicObj
// https://plotly.net/#Declarative-style-in-F-using-the-underlying
let legend = 
    let tmp = Legend()
    tmp?yanchor <- "top"
    tmp?y <- 0.99
    tmp?xanchor <- "left"
    tmp?x <- 0.5
    tmp

/// This function will take 'processedData' as input and return a range chart with a line for the average temperature
/// and a different colored area for the range between minimal and maximal temperature at that date.
let createTempChart (days,tm,tmUpper,tmLower) =
    Chart.Range(
        // data arrays
        days, tm, tmUpper, tmLower,
        StyleParam.Mode.Lines_Markers,
        Color="#3D1244",
        RangeColor="#F99BDE",
        // Name for line in legend
        Name="Average temperature over 24 timepoints each day",
        // Name for lower point when hovering over chart
        LowerName="Min temp",
        // Name for upper point when hovering over chart
        UpperName="Max temp"
    )
    // Configure the chart with the legend from above
    |> Chart.withLegend legend
    // Add name to y axis
    |> Chart.withY_AxisStyle("daily temperature [°C]")
    |> Chart.withSize (1000.,600.)

/// Chart for original data set 
let rawChart =
    processedData 
    |> createTempChart

(**<center>*)
(***hide***)
rawChart |> GenericChart.toChartHTML
(***include-it-raw***)
(**</center>*)

(**

As you can see the data looks chaotic and is difficult to analyze. Trends are hidden in daily 
temperature fluctuations and correlating events with temperature can get difficult. So next we want to
smooth the data to clearly see temperature trends.

## Savitzky-Golay filter

We will use the `Signal.Filtering.savitzkyGolay` function from [FSharp.Stats](https://fslab.org/FSharp.Stats/).

Parameters:

- windowSize (`int`) the length of the window. Must be an odd integer number.
- order (`int`) the order of the polynomial used in the filtering. Must be less then `windowSize` - 1.
- deriv (`int`) the order of the derivative to compute (default = 0 means only smoothing)
- rate (`int`) this factor will influence amplitude when using Savitzky-Golay for derivation
- data (`float array`) the values of the time history of the signal.

*)

open FSharp.Stats

let smootheTemp ws order (days,tm,tmUpper,tmLower) =
    let tm' = Signal.Filtering.savitzkyGolay ws order 0 1 tm
    let tmUpper' = Signal.Filtering.savitzkyGolay ws order 0 1 tmUpper
    let tmLower' = Signal.Filtering.savitzkyGolay ws order 0 1 tmLower
    days,tm',tmUpper',tmLower'

let smoothedChart =
    processedData
    |> smootheTemp 31 4
    |> createTempChart 

(**<center>*)
(***hide***)
smoothedChart |> GenericChart.toChartHTML
(***include-it-raw***)
(**</center>*)