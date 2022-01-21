(***hide***)

(*
#frontmatter
---
title: Replicate quality control
category: advanced
authors: Heinrich Lukas Weil    
index: 0
---
*)

(***condition:prepare***)
#r "nuget: FSharp.Data, 4.2.7"
#r "nuget: Deedle, 2.5.0"
#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: Cyjs.NET, 0.0.4"

(***condition:ipynb***)
#if IPYNB
#r "nuget: FSharp.Data, 4.2.7"
#r "nuget: Deedle, 2.5.0"
#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: Cyjs.NET, 0.0.4"
#endif // IPYNB


(**
[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/{{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)

# Replicate quality control


_Summary:_ This tutorial demonstrates an example workflow using different FsLab libraries. The aim is to check the quality of replicate measurements by clustering the samples.


## Introduction

In biology and other sciences, experimental procedures are often repeated several times in the same conditions. These resulting samples are called replicates. 
Replicates are especially useful to check for the reproducibility of the results and to boost their trustability.

One metric for the quality of the measurements is rather easy in principle. Samples received from a similar procedure should also result in similar measurements. 
Therefore just checking if replicates are more similar than other samples can already hand to the experimenter some implications about the quality of his samples.
This is especially useful when considering that usually - as the ground truth is unknown - this trustability is difficult to measure. 

In this tutorial, a simple workflow will be presented for how to visualize the clustering of replicates in an experiment. For this, 3 FsLab libraries will be used:

0. [FSharp.Data](https://fsprojects.github.io/FSharp.Data/) for retreiving the data file
1. [Deedle](https://github.com/fslaborg/Deedle) for reading a frame containing the data
2. & 3. [FSharp.Stats](https://fslab.org/FSharp.Stats/) to impute missing values and cluster the samples
4. [CyJS.NET](https://fslab.org/Cyjs.NET/) to visualize the results


## Referencing packages

```fsharp
#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
#r "nuget: Cyjs.NET"

do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))
```

## Loading Data 

In this tutorial, an in silico generated dataset is used.  

`FSharp.Data` and `Deedle` are used to load the data into the fsi.

*)

open FSharp.Data
open Deedle

// Load the data 
let rawData = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/InSilicoGeneExpression.csv"

// Create a deedle frame and index the rows with the values of the "Key" column.
let rawFrame : Frame<string,string> = 
    Frame.ReadCsvString(rawData)
    |> Frame.indexRows "Key"

(***hide***)
rawFrame.Print()


(*** include-output ***)

(** 

## Data imputation

Missing data is a constant companion of many data scientists. And it's not the best company, as missing values [can introduce a substantial amount of bias, make the handling and analysis of the data more arduous, and create reductions in efficiency](https://en.wikipedia.org/wiki/Imputation_(statistics)).

To tackle this, missing values can be substituted in a step called `imputation`. Different approaches for this exist. Here a k-nearest neighbour imputation is shown, which works as follows: 
For each observation with missing values, the k most similar other observations are chosen. Then the missing value of this observation is substituted by the mean of these values in the neighbouring observations.

*)

open FSharp.Stats
open FSharp.Stats.ML

// Select the imputation method: kNearestImpute where the 2 nearest observations are considered
let kn : Impute.MatrixBaseImputation<float[],float> = Impute.kNearestImpute 2

// Impute the missing values using the "imputeBy" function. The values of the deedle frame are first transformed into the input type of this function.
let imputedData = 
    rawFrame 
    |> Frame.toJaggedArray 
    |> Impute.imputeBy kn Ops.isNan

// Creating a new frame from the old keys and the new imputed data
let imputedFrame = 
    Frame.ofJaggedArray imputedData
    |> Frame.indexRowsWith rawFrame.RowKeys
    |> Frame.indexColsWith rawFrame.ColumnKeys

(***hide***)
imputedFrame.Print()

(*** include-output ***)
(** 

## Hierarchical clustering

To sort the level of closeness between samples, we perform a hierarchical clustering. Details about this can be found [here](003_clustering_hierarchical.html) and [here](https://fslab.org/FSharp.Stats/Clustering.html#Hierarchical-clustering).

*)

open FSharp.Stats.ML.Unsupervised

// Retreive the sample columns from the frame
let samples = 
    imputedFrame
    |> Frame.getNumericCols
    |> Series.observations
    |> Seq.map (fun (k,vs) -> 
        k,
        vs
        |> Series.values
    )

// Run the hierarchical clustering on the samples
// The clustering is performed on labeled samples (name,values) so that these labels later appear in the cluster tree
let clustering = 
    HierarchicalClustering.generate 
        (fun (name1,values1) (name2,values2) -> DistanceMetrics.euclidean values1 values2) // perform the distance calculation only on the values, not the labels
        HierarchicalClustering.Linker.wardLwLinker
        samples
    |> HierarchicalClustering.mapClusterLeaftags fst // only keep the labels in the cluster tree

(*** include-value:clustering ***)

(** 

## Data visualization

Finally, the clustering results can be visualized to check for replicate clustering. For this we use `Cyjs.NET`, an FsLab library which makes use of the `Cytoscape.js` network visualization tool.

Further information about styling the graphs can be found [here](https://fslab.org/Cyjs.NET/).
*)


open Cyjs.NET

// Function for flattening the cluster tree to an edgelist
let hClustToEdgeList (f : int -> 'T) (hClust : HierarchicalClustering.Cluster<'T>) =
    let rec loop (d,nodeLabel) cluster=
        match cluster with
        | HierarchicalClustering.Node (id,dist,_,c1,c2) ->
            let t = f id
            loop (dist,t) c1
            |> List.append (loop (dist,t) c2)
            |> List.append [nodeLabel,t,d] 
        | HierarchicalClustering.Leaf (_,_,label)-> [(nodeLabel,label,d)]
    loop (0., f 0) hClust

let rawEdgeList = hClustToEdgeList (string) clustering

// The styled vertices, samnples are coloured based on the condition they belong to. So replicates of one condition have the same colour
let cytoVertices = 
    rawEdgeList
    |> List.collect (fun (v1,v2,w) ->
        [v1;v2]
    )
    |> List.distinct
    |> List.map (fun v -> 
        let label,color,size = 
            match v.Split '_' with
            | [|"Condition0";_|] -> "Condition0", "#6FB1FC","40"
            | [|"Condition1";_|] -> "Condition1", "#EDA1ED","40"
            | [|"Condition2";_|] -> "Condition2", "#F5A45D","40"
            | _ -> "","#DDDDDD","10"

        let styling = [CyParam.label label; CyParam.color color; CyParam.width size]
        Elements.node (v) styling
    )

// Helper function to transform the distances between samples to weights
let distanceToWeight = 
    let max = rawEdgeList |> List.map (fun (a,b,c) -> c) |> List.max
    fun distance -> 1. - (distance / max)   


// Styled edges
let cytoEdges = 
    rawEdgeList
    |> List.mapi (fun i (v1,v2,weight) -> 
        let styling = [CyParam.weight (distanceToWeight weight)]
        Elements.edge ("e" + string i) v1 v2 styling
    )

// Resulting cytograph
let cytoGraph = 

    CyGraph.initEmpty ()
    |> CyGraph.withElements cytoVertices
    |> CyGraph.withElements cytoEdges
    |> CyGraph.withStyle "node" 
        [
            CyParam.content =. CyParam.label
            CyParam.shape =. CyParam.shape
            CyParam.color =. CyParam.color
            CyParam.width =. CyParam.width
        ]
    |> CyGraph.withLayout (Layout.initCose (id))  

(** 

```fsharp
// Send the cytograph to the browser
cytoGraph
|> CyGraph.show
```

*)

(***hide***)
cytoGraph
|> CyGraph.withSize(600, 400) 
|> HTML.toEmbeddedHTML

(*** include-it-raw ***)


(** 

## Interpretation

As can be seen in the graph, replicates of one condition cluster together. This is a good sign for the quality of the experiment. 
If one replicate of a condition does not behave this way, it can be considered an outlier.
If the replicates don't cluster together at all, there might be some problems with the experiment.

*)
