(***hide***)

(*
#frontmatter
---
title: Perform a t-test with FSharp.Stats
category: <datascience>
authors: <Oliver Maus>
index: 0
---
*)

(**

[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath={{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)

# Getting started: The t-test

The t-test is one of the most used statistical tests in datascience. It is used to compare two samples whose values are normally distributed in terms of statistical significance.


*)

#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats, 0.4.2"
#r "nuget: Plotly.NET, 2.0.0-preview.6"

open FSharp.Data
open Deedle
open FSharp.Stats
open FSharp.Stats.Testing
open Plotly.NET

(**

For our purposes, we will use the housefly wing length dataset*.  
Head over to the [Getting started](https://fslab.org/content/tutorials/4_getting-started.html#Data-access) tutorial where it is shown how to import a more complex dataset in a simple way if you are interested.

*Source: Sokal, R.R. and P.E. Hunter. 1955. _A morphometric analysis of DDT-resistant and non-resistant housefly strains Ann_. Entomol. Soc. Amer. 48: 499-507

*)

// We retrieve the dataset via FSharp.Data:
let rawDataHousefly = Http.RequestString @"https://seattlecentral.edu/qelp/sets/057/s057.txt"

// The raw data needs to transformed into an accessible data type. We therefore split the whole string at each new line and convert every string in the resulting array into a float.
let dataHousefly = 
    rawDataHousefly
    |> fun s -> s.Split('\n')
    |> Array.map float

(**

Now, we want to look at the pricing

*)

// The Testing module in FSharp.Stats require vectors as input types, thus we transform our array into a vector:
let vectorDataHousefly = vector dataHousefly

// The expected value of our population.
let expectedValue = 45.

// 
TTest.oneSample vectorDataHousefly expectedValue

(**

Compare male and female athletes concussion

*)

open System.Text
open System.Text.RegularExpressions

let rawDataAthletes = Http.RequestString @"http://users.stat.ufl.edu/~winner/data/concussion.dat"

// In the source above, the values are seperated with a dynamic number of spaces. We replace them with tabs using Regex to make the data more easily readable.
let regexAthletes = Regex("[ ]{2,}")
let rawDataAthletesAdapted = regexAthletes.Replace(rawDataAthletes, "\t")

let dataAthletesAsStream = new System.IO.MemoryStream(rawDataAthletesAdapted |> Encoding.UTF8.GetBytes)

// The schema helps us setting column keys.
let dataAthletesAsFrame = Frame.ReadCsv(dataAthletesAsStream, hasHeaders = false, separators = "\t", schema = "Gender, Sports, Year, Concussion, Count")

// We need to filter out the columns and rows we don't need. Thus, we filter out the rows where the athletes suffered no concussions as well as filter out the columns without the number of concussions.
let dataAthletesFemale, dataAthletesMale =
    let dataAthletesOnlyConcussion =
        dataAthletesAsFrame
        |> Frame.filterRows (fun r objS -> objS.GetAs "Concussion")
    let dataAthletesFemaleFrame =
        dataAthletesOnlyConcussion
        |> Frame.filterRows (fun r objS -> objS.GetAs "Gender" = "Female")
    let dataAthletesMaleFrame =
        dataAthletesOnlyConcussion
        |> Frame.filterRows (fun r objS -> objS.GetAs "Gender" = "Male")
    dataAthletesFemaleFrame
    |> Frame.getCol "Count" 
    |> Series.values
    |> vector,
    dataAthletesMaleFrame
    |> Frame.getCol "Count" 
    |> Series.values
    |> vector
    

// We test both samples against each other, assuming equal variances.
TTest.twoSample true dataAthletesFemale dataAthletesMale

(**

Paired

*)

let rawDataCaffeine = Http.RequestString @"http://users.stat.ufl.edu/~winner/data/caffeine1.dat"
let regexCaffeine = [Regex("[ ]{2,}1"), "1"; Regex("[ ]{2,}"), "\t"; Regex("\n\t"), "\n"]
let rawDataCaffeineAdapted = 
    regexCaffeine
    |> List.fold (fun acc (reg,rep) -> reg.Replace(acc, rep)) rawDataCaffeine

let dataCaffeineAsStream = new System.IO.MemoryStream(rawDataCaffeineAdapted |> Encoding.UTF8.GetBytes)
let dataCaffeineAsFrame = Frame.ReadCsv(dataCaffeineAsStream, hasHeaders = false, separators = "\t", schema = "Subject ID, no Dose, 5 mg, 9 mg, 13 mg")

// We want to compare the subjects' performances under the influence of 13 mg caffeine and in a control situation.
let dataCaffeineNoDose, dataCaffeine13mg =
    dataCaffeineAsFrame
    |> Frame.getCol "no Dose"
    |> Series.values
    |> vector,
    dataCaffeineAsFrame
    |> Frame.getCol "13 mg"
    |> Series.values
    |> vector

// 
TTest.twoSamplePaired dataCaffeineNoDose dataCaffeine13mg


(**

Complex datasets (MAplot) -> in die Post Hoc.fsx schreiben

*)

