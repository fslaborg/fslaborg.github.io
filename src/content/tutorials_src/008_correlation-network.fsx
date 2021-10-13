(***hide***)

(*
#frontmatter
---
title: Correlation network
category: advanced
authors: Heinrich Lukas Weil    
index: 1
---
*)

(***condition:prepare***)
#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
#r "nuget: Cyjs.NET"

(***condition:ipynb***)
#if IPYNB
#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats"
#r "nuget: Cyjs.NET"
#endif // IPYNB


(**
[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/{{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)

# Correlation network


_Summary:_ This tutorial demonstrates an example workflow using different FsLab libraries. The aim is to create a correlation network, finding a threhold for which to filter and visualizing the result.


## Introduction



In this tutorial, a simple workflow will be presented for how to create and visualize a correlation network from experimental gene expression data. For this, 4 FsLab libraries will be used:

0. [FSharp.Data](https://fsprojects.github.io/FSharp.Data/) for retreiving the data file
1. [Deedle](https://github.com/fslaborg/Deedle) for reading a frame containing the data
2. & 3. [FSharp.Stats](https://fslab.org/FSharp.Stats/) to calculate correlations and finding a critical threshold
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

In this tutorial, an multi experiment ecoli gene expression dataset is used.  

`FSharp.Data` and `Deedle` are used to load the data into the fsi.

*)

open FSharp.Data
open Deedle

// Load the data 
let rawData = Http.RequestString @"https://raw.githubusercontent.com/HLWeil/datasets/main/data/ecoliGeneExpression.tsv"

// Create a deedle frame and index the rows with the values of the "Key" column.
let rawFrame : Frame<string,string> = 
    Frame.ReadCsvString(rawData, separators = "\t")
    |> Frame.take 1000
    |> Frame.indexRows "Key"

(***hide***)
rawFrame.Print()


(*** include-output ***)

(** 

## Create a correlation network

Networks can be represented in many different ways. One representation which is computationally efficient in many approaches is the adjacency matrix. 
Here every node is represented by an index and the strength of the connection between nodes is the value in the matrix at the position of their indices.

In our case, the nodes of our network are genes in Escherichia coli (or bakers yeast). In a correlation network, the strength of this connection is the correlation. 
The correlation between these genes is calculated over the expression of these genes over different experiments. For this we use the pearson correlation.

*)

open FSharp.Stats

// Get the rows as a matrix
let rows = 
    rawFrame 
    |> Frame.toJaggedArray 
    |> Matrix.ofJaggedArray

// Create a correlation network by computing the pearson correlation between every tow rows
let correlationNetwork = 
    Correlation.Matrix.rowWisePearson rows

(***hide***)
correlationNetwork

(*** include-output ***)
(** 

## Critical threshold finding

Thresholding correlation networks

*)

// Calculate the critical threshold
// let thr,_ = Testing.RMT.compute 0.9 0.01 0.05 correlationNetwork
let thr = 0.8

// Set all correlations less strong than the critical threshold to 0
let filteredNetwork = 
    correlationNetwork
    |> Matrix.map (fun v -> if (abs v) > thr then v else 0.)

(*** include-value:clustering ***)

(** 

## Data visualization

Finally, the clustering results can be visualized to check for replicate clustering. For this we use `Cyjs.NET`, an FsLab library which makes use of the `Cytoscape.js` network visualization tool.

Further information about styling the graphs can be found [here](https://fslab.org/Cyjs.NET/).
*)


open Cyjs.NET


// The styled vertices. The size is based on the degree of this vertex, so that more heavily connected nodes are emphasized
let cytoVertices = 
    rawFrame.RowKeys
    |> Seq.toList
    |> List.mapi (fun i v -> 
        let degree = 
            Matrix.getRow filteredNetwork i 
            |> Seq.filter ((<>) 0.)
            |> Seq.length
        let styling = [CyParam.label v; CyParam.width degree]
        Elements.node (string i) styling
    )

// Styled edges
let cytoEdges = 
    let len = filteredNetwork.Dimensions |> fst
    [
        for i = 0 to len - 1 do
            for j = i + 1 to len - 1 do
                let v = filteredNetwork.[i,j]
                if v <> 0. then yield i,j,v
    ]
    |> List.mapi (fun i (v1,v2,weight) -> 
        let styling = [CyParam.weight (weight)]
        Elements.edge ("e" + string i) (string v1) (string v2) styling
    )

// Resulting cytograph
let cytoGraph = 

    CyGraph.initEmpty ()
    |> CyGraph.withElements cytoVertices
    |> CyGraph.withElements cytoEdges
    |> CyGraph.withStyle "node" 
        [
            CyParam.content =. CyParam.label
            CyParam.width =. CyParam.width
        ]
    |> CyGraph.withStyle "edge" 
        [
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
