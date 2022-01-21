(***hide***)

(*
#frontmatter
---
title: Clustering with FSharp.Stats III: DBSCAN
category: datascience
authors: Benedikt Venn
index: 2
---
*)

(***condition:prepare***)
#r "nuget: Deedle, 2.5.0"
#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: Newtonsoft.Json, 13.0.1"
#r "nuget: Plotly.NET, 2.0.0-preview.12"
#r "nuget: FSharp.Data, 4.2.7"

(***condition:ipynb***)
#if IPYNB
#r "nuget: Deedle, 2.5.0"
#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: Newtonsoft.Json, 13.0.1"
#r "nuget: Plotly.NET, 2.0.0-preview.12"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.12"
#r "nuget: FSharp.Data, 4.2.7"
#endif // IPYNB

(**

[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/{{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)


# Clustering with FSharp.Stats III: DBSCAN

_Summary:_ This tutorial demonstrates DBSCAN with FSharp.Stats and how to visualize the results with Plotly.NET.

In the previous article of this series [hierarchical clustering using FSharp.Stats](003_clustering_hierarchical.html) was introduced.

## Introduction

Clustering methods can be used to group elements of a huge data set based on their similarity. Elements sharing similar properties cluster together and can be reported as coherent group.
Density-Based Spatial Clustering of Applications with Noise (DBSCAN) was developed to identify clusters with similar density and allows the exclusion of noise points.

### Two global parameters have to be defined:

  - **ε (eps)**: radius in which the neighbourhood of each point is checked 
  - **minPts**: minimal number of data points, that must fall into the neighbourhood of a region to be defined as dense

### Data points are classified as:

  - **Core point**: Within a radius of eps there are more (or equal) data points than minPts present.
  - **Border point**: Within a radius of eps there are less data points than minPts present, but a core point is within the neighbourhood.
  - **Noise point**: None of the conditions above apply.

<img style="max-width:75%" src="../../images/dbscan.png" class="center"></img>

<br>

For demonstration of DBSCAN, the classic iris data set is used, which consists of 150 records, each of which contains four measurements and a species identifier.
In this tutorial we are going to perform DBSCAN on two- and three-dimensional data.

## Referencing packages

```fsharp
// Packages hosted by the Fslab community
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
// third party .net packages 
#r "nuget: Plotly.NET, 2.0.0-preview.12"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.12"
#r "nuget: FSharp.Data"
```

*)

(**
## Loading data
*)
open FSharp.Data
open FSharp.Stats
open Deedle

// Retrieve data using the FSharp.Data package and read it as dataframe using the Deedle package
let rawData = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/iris.csv"
let df = Frame.ReadCsvString(rawData)

df.Print()


(*** include-output ***)

(**

Let's take a first look at the data with 2D and 3D scatter plots using Plotly.NET. Each of the 150 records consists of four measurements and a species identifier. 
Since the species identifier occur several times (Iris-virginica, Iris-versicolor, and Iris-setosa), we create unique labels by adding the rows index to the species identifier.

*)
open Plotly.NET
open FSharp.Stats.ML.Unsupervised

let header2D = ["petal_length";"petal_width"]
let header3D = ["sepal_length";"petal_length";"petal_width"]

//extract petal length and petal width
let data2D = 
    Frame.sliceCols header2D df
    |> Frame.toJaggedArray

//extract sepal length, petal length, and petal width
let data3D = 
    Frame.sliceCols header3D df
    |> Frame.toJaggedArray

let labels = 
    Frame.getCol "species" df
    |> Series.values
    |> Seq.mapi (fun i s -> sprintf "%s_%i" s i)

let rawChart2D =
    let unzippedData =
        data2D
        |> Array.map (fun x -> x.[0],x.[1])
    Chart.Scatter(unzippedData,mode=StyleParam.Mode.Markers,Labels=labels)
    |> Chart.withXAxisStyle header2D.[0]
    |> Chart.withYAxisStyle header2D.[1]
    |> Chart.withTitle "rawChart2D"

let rawChart3D =
    let unzippedData =
        data3D
        |> Array.map (fun x -> x.[0],x.[1],x.[2])
    Chart.Scatter3d(unzippedData,mode=StyleParam.Mode.Markers,Labels=labels)
    |> Chart.withXAxisStyle header3D.[0]
    |> Chart.withYAxisStyle header3D.[1]
    |> Chart.withZAxisStyle header3D.[2]
    |> Chart.withTitle "rawChart3D"


(*** condition: ipynb ***)
#if IPYNB
rawChart2D
#endif // IPYNB

(***hide***)
rawChart2D |> GenericChart.toChartHTML
(***include-it-raw***)

(**
<br>
*)

(*** condition: ipynb ***)
#if IPYNB
rawChart3D
#endif // IPYNB

(***hide***)
rawChart3D |> GenericChart.toChartHTML
(***include-it-raw***)


(**
## Clustering

The function that performs DBSCAN can be found at `FSharp.Stats.ML.Unsupervised.DbScan.compute`. It requires four input parameters:

  1. Distance measure (`from FSharp.Stats.ML.DistanceMetrics`) (`seq<'T> -> seq<'T> -> float`)
  1. minPts (`int`)
  3. eps (`float`)
  4. data points as sequence of coordinate sequences (`seq<#seq<'T>>`)

The clustering result consists of a sequence of noise point coordinates and a sequence of clusters containing all related point coordinates.

*)
open FSharp.Stats.ML
open FSharp.Stats.ML.Unsupervised


let eps2D = 0.5
let eps3D = 0.7

let minPts = 20

let result2D = DbScan.compute DistanceMetrics.Array.euclidean minPts eps2D data2D

(***hide***)
let printClusters2D = result2D.ToString()
(*** include-value:printClusters2D ***)

let result3D = DbScan.compute DistanceMetrics.Array.euclidean minPts eps3D data3D

(***hide***)
let printClusters3D = result3D.ToString()

(*** include-value:printClusters3D ***)

(**
## Visualization of clustering result

To visualize the clustering result coordinates of each cluster and noise points are visualized separately and combined in a single scatter plot.

### 2D clustering result visualization

*)


//to create a chart with two dimensional data use the following function
    
let chartCluster2D = 
    result2D.Clusterlist
    |> Seq.mapi (fun i l ->
        l
        |> Seq.map (fun x -> x.[0],x.[1])
        |> Seq.distinct //more efficient visualization; no difference in plot but in point numbers
        |> Chart.Point
        |> Chart.withTraceName (sprintf "Cluster %i" i))
    |> Chart.combine

let chartNoise2D = 
    result2D.Noisepoints
    |> Seq.map (fun x -> x.[0],x.[1])  
    |> Seq.distinct //more efficient visualization; no difference in plot but in point numbers
    |> Chart.Point
    |> Chart.withTraceName "Noise"

let chartTitle2D = 
    let noiseCount   = result2D.Noisepoints |> Seq.length
    let clusterCount = result2D.Clusterlist |> Seq.length
    let clPtsCount   = result2D.Clusterlist |> Seq.sumBy Seq.length
    $"eps: %.1f{eps2D} minPts: %i{minPts} pts: %i{noiseCount + clPtsCount} cluster: %i{clusterCount} noisePts: %i{noiseCount}" 

let chart2D =
    [chartNoise2D;chartCluster2D]
    |> Chart.combine
    |> Chart.withTitle chartTitle2D
    |> Chart.withXAxisStyle header2D.[0]
    |> Chart.withYAxisStyle header2D.[1]

(*** condition: ipynb ***)
#if IPYNB
chart2D
#endif // IPYNB

(***hide***)
chart2D |> GenericChart.toChartHTML
(***include-it-raw***)

(**

### 3D clustering result visualization



*)


let chartCluster3D = 
    result3D.Clusterlist
    |> Seq.mapi (fun i l ->
        l
        |> Seq.map (fun x -> x.[0],x.[1],x.[2])
        |> Seq.distinct //faster visualization; no difference in plot but in point number
        |> fun x -> Chart.Scatter3d (x,StyleParam.Mode.Markers)
        |> Chart.withTraceName (sprintf "Cluster_%i" i))
    |> Chart.combine

let chartNoise3D =
    result3D.Noisepoints
    |> Seq.map (fun x -> x.[0],x.[1],x.[2])  
    |> Seq.distinct //faster visualization; no difference in plot but in point number
    |> fun x -> Chart.Scatter3d (x,StyleParam.Mode.Markers)
    |> Chart.withTraceName "Noise"

let chartname3D = 
    let noiseCount   = result3D.Noisepoints |> Seq.length
    let clusterCount = result3D.Clusterlist |> Seq.length
    let clPtsCount   = result3D.Clusterlist |> Seq.sumBy Seq.length
    $"eps: %.1f{eps3D} minPts: %i{minPts} pts: %i{noiseCount + clPtsCount} cluster: %i{clusterCount} noisePts: %i{noiseCount}" 
   
let chart3D = 
    [chartNoise3D;chartCluster3D]
    |> Chart.combine
    |> Chart.withTitle chartname3D
    |> Chart.withXAxisStyle header3D.[0]
    |> Chart.withYAxisStyle header3D.[1]
    |> Chart.withZAxisStyle header3D.[2]
    
//for faster computation you can use the squaredEuclidean distance and set your eps to its square
let clusteredChart3D() = DbScan.compute DistanceMetrics.Array.euclideanNaNSquared 20 (0.7**2.) data3D 


(*** condition: ipynb ***)
#if IPYNB
chart3D
#endif // IPYNB

(***hide***)
chart3D |> GenericChart.toChartHTML
(***include-it-raw***)

(**


## Limitations

  1. The selection of minPts and eps is critical and even small deviations can severely influence the final results
  2. When data points are of varying density, DBSCAN is not appropriate

## Notes

  - Please note that depending on what data you want to cluster, a column wise z-score normalization may be required. In the presented example differences in sepal width have a reduced influence because
  the absolute variation is low.

## References

  - [FSharp.Stats documentation](https://fslab.org/FSharp.Stats/Clustering.html), fslaborg, 
  - Shinde and Sankhe, Comparison of Enhanced DBSCAN Algorithms: A Review, International Journal of Engeneering Research & Technology, 2017
  - Nagaraju et al., An effective density based approach to detect complex data clusters using notion of neighborhood difference, Int. J. Autom. Comput., 2017, https://doi.org/10.1007/s11633-016-1038-7 

*)


