(***hide***)

(*
#frontmatter
---
title: Clustering with FSharp.Stats I: k-means
category: datascience
authors: Benedikt Venn
index: 1
---
*)

(***condition:prepare***)
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
#r "nuget: Newtonsoft.JSON"
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: FSharp.Data"

(***condition:ipynb***)
#if IPYNB
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
#r "nuget: Newtonsoft.JSON"
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.6"
#r "nuget: FSharp.Data"
#endif // IPYNB

(**

[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath={{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)


# Clustering with FSharp.Stats I: k-means

_Summary:_ This tutorial demonstrates k means clustering with FSharp.Stats and how to visualize the results with Plotly.NET.

## Introduction

Clustering methods can be used to group elements of a huge data set based on their similarity. Elements sharing similar properties cluster together and can be reported as coherent group.
k-means clustering is a frequently used technique, that segregates the given data into k clusters with similar elements grouped in each cluster, but high variation between the clusters.
The algorithm to cluster a n-dimensional dataset can be fully described in the following 4 steps:

  1. Initialize k n-dimensional centroids, that are randomly distributed over the data range.
  2. Calculate the distance of each point to all centroids and assign it to the nearest one.
  3. Reposition all centroids by calculating the average point of each cluster.
  4. Repeat step 2-3 until convergence.

### Centroid initiation

Since the random initiation of centroids may influences the result, a second initiation algorithm is proposed (_cvmax_), that extract a set of medians from the dimension with maximum variance to initialize the centroids. 

### Distance measure

While several distance metrics can be used (e.g. Manhattan distance or correlation measures) it is preferred to use Euclidean distance.
It is recommended to use a squared Euclidean distance. To not calculate the square root does not change the result but saves computation time.

<center>
<img style="max-width:75%" src="../../images/kMeans.png" class="center"></img>
</center>
<br>


For demonstration of k-means clustering, the classic iris data set is used, which consists of 150 records, each of which contains four measurements and a species identifier.

## Referencing packages

```fsharp
// Packages hosted by the Fslab community
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
// third party .net packages 
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.6"
#r "nuget: FSharp.Data"
```

*)

(**
## Loading data
*)
open FSharp.Data
open Deedle

// Retrieve data using the FSharp.Data package
let rawData = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/iris.csv"

// Use .net Core functions to convert the retrieved string to a stream
let dataAsStream = new System.IO.MemoryStream(rawData |> System.Text.Encoding.UTF8.GetBytes) 

let dataAsFrame = Frame.ReadCsv(dataAsStream,hasHeaders=true,separators=",")

dataAsFrame.Print()

(*** include-output ***)

(**

Let's take a first look at the data with heatmaps using Plotly.NET. Each of the 150 records consists of four measurements and a species identifier. 
Since the species identifier occur several times (_Iris-virginica_, _Iris-versicolor_, and _Iris-setosa_), we create unique labels by adding the rows index to the species identifier.

*)
open Plotly.NET

let colNames = ["sepal_length";"sepal_width";"petal_length";"petal_width"]

// isolate data as float [] []
let data = 
    Frame.dropCol "species" dataAsFrame
    |> Frame.toJaggedArray

//isolate labels as seq<string>
let labels = 
    Frame.getCol "species" dataAsFrame
    |> Series.values
    |> Seq.mapi (fun i s -> sprintf "%s_%i" s i)

let dataChart = 
    Chart.Heatmap(data,ColNames=colNames,RowNames=labels)
    // required to fit the species identifier on the left side of the heatmap
    |> Chart.withMarginSize(Left=100.)
    |> Chart.withTitle "raw iris data"

// required to fit the species identifier on the left side of the heatmap
(**<center>*)
(*** condition: ipynb ***)
#if IPYNB
dataChart
#endif // IPYNB

(***hide***)
dataChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>
## Clustering

The function that performs k-means clustering can be found at `FSharp.Stats.ML.Unsupervised.IterativeClustering.kmeans`. It requires four input parameters:

  1. Centroid initiation method
  2. Distance measure (from `FSharp.Stats.ML.DistanceMetrics`)
  3. Data to cluster as `float [] []`, where each entry of the outer array is a sequence of coordinates
  4. _k_, the number of clusters that are desired


*)

open FSharp.Stats
open FSharp.Stats.ML
open FSharp.Stats.ML.Unsupervised

// For random cluster initiation use randomInitFactory:
let rnd = System.Random()
let randomInitFactory : IterativeClustering.CentroidsFactory<float []> = 
    IterativeClustering.randomCentroids<float []> rnd

// For assisted cluster initiation use cvmaxFactory:
//let cvmaxFactory : IterativeClustering.CentroidsFactory<float []> = 
//    IterativeClustering.intitCVMAX

let distanceFunction = DistanceMetrics.euclideanNaNSquared
  
let kmeansResult = 
    IterativeClustering.kmeans distanceFunction randomInitFactory data 4


(**
After all centroids are set, the affiliation of a datapoint to a cluster can be determined by minimizing the distance of the respective point to each of the centroids.
A function realizing the mapping is integrated in the `kmeansResult`.

*)

let clusteredIrisData =
    Seq.zip labels data
    |> Seq.map (fun (species,dataPoint) -> 
        let clusterIndex,centroid = kmeansResult.Classifier dataPoint
        clusterIndex,species,dataPoint)

// Each datapoint is given associated with its cluster index, species identifier, and coordinates.

(*** condition: ipynb ***)
#if IPYNB
clusteredIrisData
|> Seq.take 10
|> Seq.map (fun (a,b,c) -> sprintf "%i, %A, %A" a b c)
|> String.concat "\n"
|> String.concat "<br>"
|> fun x -> x + "<br> ... "
#endif // IPYNB

(***hide***)
let printClusters=
    clusteredIrisData
    |> Seq.take 7
    |> Seq.map (fun (a,b,c) -> sprintf "%i, %A, %A" a b c)
    |> String.concat "\n"
    |> fun x -> x + "\n ... "

(*** include-value:printClusters ***)
(**

## Visualization of the clustering result as heatmap

The datapoints are sorted according to their associated cluster index and visualized in a combined heatmap.
*)

let clusterChart =
    clusteredIrisData
    //sort all data points according to their assigned cluster number
    |> Seq.sortBy (fun (clusterIndex,label,dataPoint) -> clusterIndex)
    |> Seq.unzip3
    |> fun (_,labels,d) -> 
        Chart.Heatmap(d,ColNames=colNames,RowNames=labels)
        // required to fit the species identifier on the left side of the heatmap
        |> Chart.withMarginSize(Left=100.)
        |> Chart.withTitle "clustered iris data (k-means clustering)"
(**
<center>
*)
(*** condition: ipynb ***)
#if IPYNB
clusterChart
#endif // IPYNB

(***hide***)
clusterChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>

To visualize the result in a three-dimensional chart, three of the four measurements are isolated after clustering and visualized as 3D-scatter plot.

*)

let clusterChart3D =
    //group clusters
    clusteredIrisData
    |> Seq.groupBy (fun (clusterIndex,label,dataPoint) -> clusterIndex)
    //for each cluster generate a scatter plot
    |> Seq.map (fun (clusterIndex,cluster) -> 
        cluster
        |> Seq.unzip3
        |> fun (clusterIndex,label,data) -> 
            let clusterName = sprintf "cluster %i" (Seq.head clusterIndex)
            //for 3 dimensional representation isolate sepal length, petal length, and petal width
            let truncData = data |> Seq.map (fun x -> x.[0],x.[2],x.[3]) 
            Chart.Scatter3d(truncData,mode=StyleParam.Mode.Markers,Name = clusterName,Labels=label)
        )
    |> Chart.Combine
    |> Chart.withTitle "isolated coordinates of clustered iris data (k-means clustering)"
    |> Chart.withX_AxisStyle colNames.[0]
    |> Chart.withY_AxisStyle colNames.[2]
    |> Chart.withZ_AxisStyle colNames.[3]

(**
<center>
*)
(*** condition: ipynb ***)
#if IPYNB
clusterChart3D
#endif // IPYNB

(***hide***)
clusterChart3D |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>

### Optimal cluster number

The identification of the optimal cluster number _k_ in terms of the average squared distance of each point to its centroid 
can be realized by performing the clustering over a range of _k_'s multiple times and taking the _k_ according to the elbow criterion.
Further more robust and advanced cluster number determination techniques can be found [here](https://fslab.org/FSharp.Stats/Clustering.html#Determining-the-optimal-number-of-clusters).

*)

let getBestkMeansClustering bootstraps k =
    let dispersions =
        Array.init bootstraps (fun _ -> 
            IterativeClustering.kmeans distanceFunction randomInitFactory data k
            )
        |> Array.map (fun clusteringResult -> IterativeClustering.DispersionOfClusterResult clusteringResult)
    Seq.mean dispersions,Seq.stDev dispersions

let iterations = 10

let maximalK = 10

let bestKChart = 
    [2 .. maximalK] 
    |> List.map (fun k -> 
        let mean,stdev = getBestkMeansClustering iterations k
        k,mean,stdev
        )
    |> List.unzip3
    |> fun (ks,means,stdevs) -> 
        Chart.Line(ks,means)
        |> Chart.withYErrorStyle(stdevs)
        |> Chart.withX_AxisStyle "k"
        |> Chart.withY_AxisStyle "average dispersion"
        |> Chart.withTitle "iris data set average dispersion per k"
(**
<center>
*)
(*** condition: ipynb ***)
#if IPYNB
bestKChart
#endif // IPYNB

(***hide***)
bestKChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**

</center>

## Limitations

  1. Outlier have a strong influence on the positioning of the centroids. 
  2. Determining the correct number of clusters in advance is critical. Often it is chosen according to the number of classes present in the dataset which isn't in the spirit of clustering procedures.

## Notes

  - Please note that depending on what data you want to cluster, a column wise z-score normalization may be required. In the presented example differences in sepal width have a reduced influence because
  the absolute variation is low.

## References

  - FSharp.Stats documentation, fslaborg, https://fslab.org/FSharp.Stats/Clustering.html
  - Shraddha and Saganna, A Review On K-means Data Clustering Approach, International Journal of Information & Computation Technology, Vol:4 No:17, 2014
  - Moth'd Belal, A New Algorithm for Cluster Initialization, International Journal of Computer and Information Engineering, Vol:1 No:4, 2007
  - Singh et al., K-means with Three different Distance Metrics, International Journal of Computer Applications, 2013, DOI:10.5120/11430-6785
  - Kodinariya and Makwana, Review on Determining of Cluster in K-means Clustering, International Journal of Advance Research in Computer Science and Management Studies, 2013

## Further reading
  
Examples are taken from [FSharp.Stats documentation](https://fslab.org/FSharp.Stats/Clustering.html) that covers various techniques for an optimal cluster number determination.
  
The next article in this series covers [hierarchical clustering using FSharp.Stats](https://fslab.org/content/tutorials/003_clustering_hierarchical.html).

*)



