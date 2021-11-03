(**
[![Binder](https://fslab.org/images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/003_clustering_hierarchical.ipynb)&emsp;
[![Script](https://fslab.org/images/badge-script.svg)](https://fslab.org/content/tutorials/003_clustering_hierarchical.fsx)&emsp;
[![Notebook](https://fslab.org/images/badge-notebook.svg)](https://fslab.org/content/tutorials/003_clustering_hierarchical.ipynb)

# Clustering with FSharp.Stats II: hierarchical clustering

_Summary:_ This tutorial demonstrates hierarchical clustering with FSharp.Stats and how to visualize the results with Plotly.NET.

In the previous article of this series [k-means clustering using FSharp.Stats](002_clustering_kMeans.html) was introduced.

## Introduction

Clustering methods can be used to group elements of a huge data set based on their similarity. Elements sharing similar properties cluster together and can be reported as coherent group.
Many clustering algorithms require a predefined cluster number, that has to be provided by the experimenter.
Hierarchical clustering (hClust) does not require such cluster number definition. Instead, hierarchical clustering results in a tree structure, that has a single cluster (node) on its root and recursively splits up into clusters of 
elements that are more similar to each other than to elements of other clusters. For generating multiple clustering results with different number of clusters, 
the clustering has to performed only once. Subsequently a cluster number can be defined to split up the clustering tree in the desired number of clusters.
The clustering tree is often represented as dendrogram.

### There are two types of hClust:

  - Agglomerative (bottom-up): Each data point is in its own cluster and the nearest ones are merged recursively. It is referred as agglomerative hierarchical clustering.

  - Divisive (top-down): All data points are in the same cluster and you divide the cluster into two that are far away from each other.

  - The presented implementation is an agglomerative type.

### Distance measures

There are several distance metrics, that can be used as distance function. The commonly used one probably is Euclidean distance.

### Linker

When the distance between two clusters is calculated, there are several linkage types to choose from:

  - **complete linkage**: maximal pairwise distance between the clusters (prone to break large clusters)

  - **single linkage**: minimal pairwise distance between the clusters (sensitive to outliers)

  - **centroid linkage**: distance between the two cluster centroids

  - **average linkage**: average pairwise distance between the clusters (sensitive to cluster shape and size)

  - **median linkage**: median pairwise distance between the clusters


<img style="max-width:100%" src="../../images/hClust.png"></img>

<br>


For demonstration of hierarchical clustering, the classic iris data set is used, which consists of 150 records, each of which contains four measurements and a species identifier.

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


## Loading data

*)
open FSharp.Data
open Deedle

// Retrieve data using the FSharp.Data package and read it as dataframe using the Deedle package
let rawData = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/iris.csv"
let df = Frame.ReadCsvString(rawData)

df.Print()(* output: 
sepal_length sepal_width petal_length petal_width species    
0   -> 5.5          2.4         3.8          1.1         versicolor 
1   -> 4.9          3.1         1.5          0.1         setosa     
2   -> 7.6          3           6.6          2.1         virginica  
3   -> 5.6          2.8         4.9          2           virginica  
4   -> 6.1          3           4.9          1.8         virginica  
5   -> 6.3          3.4         5.6          2.4         virginica  
6   -> 6.2          2.8         4.8          1.8         virginica  
7   -> 7.2          3.2         6            1.8         virginica  
8   -> 6.9          3.2         5.7          2.3         virginica  
9   -> 4.9          3           1.4          0.2         setosa     
10  -> 5.4          3.9         1.7          0.4         setosa     
11  -> 7            3.2         4.7          1.4         versicolor 
12  -> 6.1          3           4.6          1.4         versicolor 
13  -> 5.4          3.7         1.5          0.2         setosa     
14  -> 7.2          3.6         6.1          2.5         virginica  
:      ...          ...         ...          ...         ...        
135 -> 7.7          3.8         6.7          2.2         virginica  
136 -> 5.7          3           4.2          1.2         versicolor 
137 -> 6.4          2.7         5.3          1.9         virginica  
138 -> 5.8          2.7         3.9          1.2         versicolor 
139 -> 5            2.3         3.3          1           versicolor 
140 -> 6.7          3.1         4.4          1.4         versicolor 
141 -> 6.3          3.3         6            2.5         virginica  
142 -> 4.9          2.4         3.3          1           versicolor 
143 -> 5.8          2.8         5.1          2.4         virginica  
144 -> 5            3.3         1.4          0.2         setosa     
145 -> 7.7          2.6         6.9          2.3         virginica  
146 -> 5.7          2.6         3.5          1           versicolor 
147 -> 5.9          3           5.1          1.8         virginica  
148 -> 6.8          3.2         5.9          2.3         virginica  
149 -> 5            3.6         1.4          0.2         setosa*)
(**
Let's take a first look at the data with heatmaps using Plotly.NET. Each of the 150 records consists of four measurements and a species identifier. 
Since the species identifier occur several times (Iris-virginica, Iris-versicolor, and Iris-setosa), we create unique labels by adding the rows index to the species identifier.


*)
open Plotly.NET

let colNames = ["sepal_length";"sepal_width";"petal_length";"petal_width"]

// isolate data as float [] []
let data = 
    Frame.dropCol "species" df
    |> Frame.toJaggedArray
    

// isolate labels as seq<string>
let labels = 
    Frame.getCol "species" df
    |> Series.values
    |> Seq.mapi (fun i s -> sprintf "%s_%i" s i)
    |> Array.ofSeq

let dataChart = 
    Chart.Heatmap(data,ColNames=colNames,RowNames=labels)
    // required to fit the species identifier on the left side of the heatmap
    |> Chart.withMarginSize(Left=100.)
    |> Chart.withTitle "raw iris data"(* output: 
<div id="1e19d0fa-1b78-472c-9986-2b54983ae620" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_1e19d0fa1b78472c99862b54983ae620 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"heatmap","z":[[5.5,2.4,3.8,1.1],[4.9,3.1,1.5,0.1],[7.6,3.0,6.6,2.1],[5.6,2.8,4.9,2.0],[6.1,3.0,4.9,1.8],[6.3,3.4,5.6,2.4],[6.2,2.8,4.8,1.8],[7.2,3.2,6.0,1.8],[6.9,3.2,5.7,2.3],[4.9,3.0,1.4,0.2],[5.4,3.9,1.7,0.4],[7.0,3.2,4.7,1.4],[6.1,3.0,4.6,1.4],[5.4,3.7,1.5,0.2],[7.2,3.6,6.1,2.5],[6.3,2.8,5.1,1.5],[5.4,3.4,1.7,0.2],[5.8,4.0,1.2,0.2],[5.0,3.2,1.2,0.2],[6.5,3.0,5.8,2.2],[5.6,3.0,4.1,1.3],[4.3,3.0,1.1,0.1],[6.7,3.3,5.7,2.5],[4.5,2.3,1.3,0.3],[5.7,2.5,5.0,2.0],[6.3,2.7,4.9,1.8],[5.6,3.0,4.5,1.5],[5.6,2.5,3.9,1.1],[4.9,3.1,1.5,0.1],[7.7,3.0,6.1,2.3],[6.3,2.5,5.0,1.9],[6.4,3.1,5.5,1.8],[4.7,3.2,1.6,0.2],[5.5,4.2,1.4,0.2],[5.9,3.2,4.8,1.8],[5.0,3.4,1.6,0.4],[5.4,3.4,1.5,0.4],[6.0,2.7,5.1,1.6],[5.6,2.9,3.6,1.3],[6.3,3.3,4.7,1.6],[5.1,2.5,3.0,1.1],[6.4,3.2,4.5,1.5],[5.1,3.7,1.5,0.4],[6.7,3.0,5.2,2.3],[6.2,2.9,4.3,1.3],[6.8,2.8,4.8,1.4],[7.7,2.8,6.7,2.0],[4.8,3.0,1.4,0.1],[6.2,3.4,5.4,2.3],[4.8,3.4,1.9,0.2],[6.7,2.5,5.8,1.8],[5.1,3.5,1.4,0.2],[6.7,3.0,5.0,1.7],[6.9,3.1,5.1,2.3],[5.8,2.7,5.1,1.9],[4.8,3.1,1.6,0.2],[5.5,2.6,4.4,1.2],[5.1,3.8,1.6,0.2],[5.7,4.4,1.5,0.4],[6.7,3.3,5.7,2.1],[4.4,3.0,1.3,0.2],[5.0,3.5,1.3,0.3],[6.9,3.1,5.4,2.1],[5.0,3.5,1.6,0.6],[6.5,3.2,5.1,2.0],[4.6,3.6,1.0,0.2],[6.1,2.8,4.7,1.2],[5.0,2.0,3.5,1.0],[5.8,2.7,4.1,1.0],[6.0,3.4,4.5,1.6],[4.9,2.5,4.5,1.7],[5.7,3.8,1.7,0.3],[7.9,3.8,6.4,2.0],[7.2,3.0,5.8,1.6],[5.5,3.5,1.3,0.2],[4.6,3.2,1.4,0.2],[6.4,2.9,4.3,1.3],[6.9,3.1,4.9,1.5],[6.0,2.9,4.5,1.5],[5.8,2.6,4.0,1.2],[5.2,2.7,3.9,1.4],[7.4,2.8,6.1,1.9],[5.5,2.4,3.7,1.0],[5.1,3.4,1.5,0.2],[4.8,3.4,1.6,0.2],[5.0,3.0,1.6,0.2],[5.7,2.8,4.1,1.3],[4.6,3.1,1.5,0.2],[5.9,3.0,4.2,1.5],[6.1,2.8,4.0,1.3],[5.7,2.8,4.5,1.3],[6.0,2.2,4.0,1.0],[5.4,3.0,4.5,1.5],[6.1,2.9,4.7,1.4],[5.4,3.9,1.3,0.4],[5.2,4.1,1.5,0.1],[7.3,2.9,6.3,1.8],[4.7,3.2,1.3,0.2],[6.3,2.5,4.9,1.5],[5.3,3.7,1.5,0.2],[6.6,2.9,4.6,1.3],[6.3,2.9,5.6,1.8],[4.4,3.2,1.3,0.2],[4.4,2.9,1.4,0.2],[6.5,3.0,5.5,1.8],[6.3,2.3,4.4,1.3],[4.9,3.1,1.5,0.1],[6.6,3.0,4.4,1.4],[6.4,2.8,5.6,2.1],[6.5,2.8,4.6,1.5],[5.5,2.3,4.0,1.3],[6.7,3.1,5.6,2.4],[6.4,3.2,5.3,2.3],[6.8,3.0,5.5,2.1],[5.7,2.9,4.2,1.3],[5.8,2.7,5.1,1.9],[6.0,3.0,4.8,1.8],[5.2,3.4,1.4,0.2],[6.7,3.1,4.7,1.5],[5.1,3.5,1.4,0.3],[5.5,2.5,4.0,1.3],[4.8,3.0,1.4,0.3],[5.1,3.8,1.5,0.3],[6.4,2.8,5.6,2.2],[5.0,3.4,1.5,0.2],[5.1,3.8,1.9,0.4],[5.1,3.3,1.7,0.5],[6.5,3.0,5.2,2.0],[5.6,2.7,4.2,1.3],[6.0,2.2,5.0,1.5],[7.1,3.0,5.9,2.1],[4.6,3.4,1.4,0.3],[6.1,2.6,5.6,1.4],[6.2,2.2,4.5,1.5],[5.2,3.5,1.5,0.2],[7.7,3.8,6.7,2.2],[5.7,3.0,4.2,1.2],[6.4,2.7,5.3,1.9],[5.8,2.7,3.9,1.2],[5.0,2.3,3.3,1.0],[6.7,3.1,4.4,1.4],[6.3,3.3,6.0,2.5],[4.9,2.4,3.3,1.0],[5.8,2.8,5.1,2.4],[5.0,3.3,1.4,0.2],[7.7,2.6,6.9,2.3],[5.7,2.6,3.5,1.0],[5.9,3.0,5.1,1.8],[6.8,3.2,5.9,2.3],[5.0,3.6,1.4,0.2]],"x":["sepal_length","sepal_width","petal_length","petal_width"],"y":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"]}];
            var layout = {"margin":{"l":100.0},"title":"raw iris data"};
            var config = {};
            Plotly.newPlot('1e19d0fa-1b78-472c-9986-2b54983ae620', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_1e19d0fa1b78472c99862b54983ae620();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_1e19d0fa1b78472c99862b54983ae620();
            }
</script>
*)
(**
## Clustering

The function that performs hierarchical clustering can be found at `FSharp.Stats.ML.Unsupervised.HierarchicalClustering.generate`. It requires three input parameters:

  1. Distance measure working on `'T` (from `FSharp.Stats.ML.DistanceMetrics`)
  2. Linkage type
  3. Data to cluster as `'T`


*)
open FSharp.Stats.ML
open FSharp.Stats.ML.Unsupervised

let distanceMeasure = DistanceMetrics.euclideanNaNSquared

let linker = HierarchicalClustering.Linker.centroidLwLinker

// calculates the clustering and reports a single root cluster (node), 
// that may recursively contains further nodes
let clusterResultH = 
    HierarchicalClustering.generate distanceMeasure linker data

// If a desired cluster number is specified, the following function cuts the cluster according
// to the depth, that results in the respective number of clusters (here 3). Only leaves are reported.
let threeClusters = HierarchicalClustering.cutHClust 3 clusterResultH
(**
Every cluster leaf contains its raw values and an index that indicates the position of the respective data 
point in the raw data. The index can be retrieved from leaves using HierarchicalClustering.getClusterId.


*)
// Detailed information for 3 clusters are given
let inspectThreeClusters =
    threeClusters
    |> List.map (fun cluster -> 
        cluster
        |> List.map (fun leaf -> 
            labels.[HierarchicalClustering.getClusterId leaf]
            )
        )(* output: 
Cluster0: [versicolor_44; versicolor_76; versicolor_89; versicolor_12; versicolor_93 ...]<br>Cluster1: [setosa_16; setosa_36; setosa_74; setosa_42; setosa_122 ...]<br>Cluster2: [virginica_72; virginica_135; virginica_14; virginica_7; virginica_73 ...]*)
(**
To break up the tree structure but maintain the clustering order, the cluster tree has to be flattened.


*)
// To recursevely flatten the cluster tree into leaves only, use flattenHClust.
// A leaf list is reported, that does not contain any cluster membership, 
// but is sorted by the clustering result.
let hLeaves = 
    clusterResultH
    |> HierarchicalClustering.flattenHClust
    
// Takes the sorted cluster result and reports a tuple of label and data value.
let dataSortedByClustering =    
    hLeaves
    |> Seq.choose (fun c -> 
        let label  = labels.[HierarchicalClustering.getClusterId c]
        let values = HierarchicalClustering.tryGetLeafValue c
        match values with
        | None -> None
        | Some x -> Some (label,x)
        )
(**
The visualization again is performed using a Plotly.NET heatmap. 
        

*)
let hClusteredDataHeatmap = 
    let (hlable,hdata) =
        dataSortedByClustering
        |> Seq.unzip
    Chart.Heatmap(hdata,ColNames=colNames,RowNames=hlable)
    // required to fit the species identifier on the left side of the heatmap
    |> Chart.withMarginSize(Left=100.)
    |> Chart.withTitle "Clustered iris data (hierarchical clustering)"(* output: 
<div id="dc85d794-9610-45ad-b669-f7edf700db27" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_dc85d794961045adb669f7edf700db27 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"heatmap","z":[[6.2,2.9,4.3,1.3],[6.4,2.9,4.3,1.3],[6.1,2.8,4.0,1.3],[6.1,3.0,4.6,1.4],[6.1,2.9,4.7,1.4],[6.0,2.9,4.5,1.5],[6.1,2.8,4.7,1.2],[6.3,3.3,4.7,1.6],[6.4,3.2,4.5,1.5],[6.0,3.4,4.5,1.6],[7.0,3.2,4.7,1.4],[6.9,3.1,4.9,1.5],[6.7,3.1,4.7,1.5],[6.8,2.8,4.8,1.4],[6.7,3.0,5.0,1.7],[6.6,2.9,4.6,1.3],[6.5,2.8,4.6,1.5],[6.6,3.0,4.4,1.4],[6.7,3.1,4.4,1.4],[6.3,2.8,5.1,1.5],[6.0,2.7,5.1,1.6],[6.3,2.5,4.9,1.5],[6.2,2.8,4.8,1.8],[6.3,2.7,4.9,1.8],[6.3,2.5,5.0,1.9],[6.1,3.0,4.9,1.8],[6.0,3.0,4.8,1.8],[5.9,3.2,4.8,1.8],[5.9,3.0,5.1,1.8],[5.8,2.7,5.1,1.9],[5.8,2.7,5.1,1.9],[5.7,2.5,5.0,2.0],[5.6,2.8,4.9,2.0],[5.8,2.8,5.1,2.4],[6.3,2.3,4.4,1.3],[6.2,2.2,4.5,1.5],[6.0,2.2,5.0,1.5],[5.6,2.9,3.6,1.3],[5.7,2.6,3.5,1.0],[5.5,2.4,3.8,1.1],[5.5,2.4,3.7,1.0],[5.6,2.5,3.9,1.1],[5.5,2.3,4.0,1.3],[5.5,2.5,4.0,1.3],[5.2,2.7,3.9,1.4],[5.8,2.6,4.0,1.2],[5.8,2.7,3.9,1.2],[5.8,2.7,4.1,1.0],[5.7,2.8,4.1,1.3],[5.6,2.7,4.2,1.3],[5.7,2.9,4.2,1.3],[5.7,3.0,4.2,1.2],[5.6,3.0,4.1,1.3],[5.5,2.6,4.4,1.2],[5.7,2.8,4.5,1.3],[5.9,3.0,4.2,1.5],[5.6,3.0,4.5,1.5],[5.4,3.0,4.5,1.5],[6.0,2.2,4.0,1.0],[4.9,2.5,4.5,1.7],[5.0,2.3,3.3,1.0],[4.9,2.4,3.3,1.0],[5.1,2.5,3.0,1.1],[5.0,2.0,3.5,1.0],[7.9,3.8,6.4,2.0],[7.7,3.8,6.7,2.2],[7.2,3.6,6.1,2.5],[7.2,3.2,6.0,1.8],[7.2,3.0,5.8,1.6],[7.1,3.0,5.9,2.1],[7.4,2.8,6.1,1.9],[7.3,2.9,6.3,1.8],[7.7,3.0,6.1,2.3],[7.6,3.0,6.6,2.1],[7.7,2.8,6.7,2.0],[7.7,2.6,6.9,2.3],[6.7,2.5,5.8,1.8],[6.1,2.6,5.6,1.4],[6.7,3.0,5.2,2.3],[6.9,3.1,5.1,2.3],[6.9,3.1,5.4,2.1],[6.8,3.0,5.5,2.1],[6.5,3.2,5.1,2.0],[6.5,3.0,5.2,2.0],[6.4,3.1,5.5,1.8],[6.5,3.0,5.5,1.8],[6.3,2.9,5.6,1.8],[6.4,2.7,5.3,1.9],[6.4,2.8,5.6,2.1],[6.4,2.8,5.6,2.2],[6.5,3.0,5.8,2.2],[6.7,3.3,5.7,2.5],[6.7,3.1,5.6,2.4],[6.9,3.2,5.7,2.3],[6.8,3.2,5.9,2.3],[6.7,3.3,5.7,2.1],[6.3,3.4,5.6,2.4],[6.2,3.4,5.4,2.3],[6.4,3.2,5.3,2.3],[6.3,3.3,6.0,2.5],[5.4,3.4,1.7,0.2],[5.4,3.4,1.5,0.4],[5.5,3.5,1.3,0.2],[5.1,3.7,1.5,0.4],[5.1,3.8,1.5,0.3],[5.1,3.8,1.6,0.2],[5.4,3.7,1.5,0.2],[5.3,3.7,1.5,0.2],[5.0,3.5,1.3,0.3],[5.0,3.6,1.4,0.2],[5.2,3.4,1.4,0.2],[5.2,3.5,1.5,0.2],[5.1,3.5,1.4,0.2],[5.1,3.5,1.4,0.3],[5.1,3.4,1.5,0.2],[5.0,3.4,1.5,0.2],[5.0,3.3,1.4,0.2],[5.0,3.4,1.6,0.4],[5.1,3.3,1.7,0.5],[5.0,3.5,1.6,0.6],[5.1,3.8,1.9,0.4],[4.6,3.2,1.4,0.2],[4.6,3.1,1.5,0.2],[4.7,3.2,1.3,0.2],[4.6,3.4,1.4,0.3],[4.9,3.0,1.4,0.2],[4.8,3.0,1.4,0.3],[4.8,3.0,1.4,0.1],[4.9,3.1,1.5,0.1],[4.9,3.1,1.5,0.1],[4.9,3.1,1.5,0.1],[5.0,3.0,1.6,0.2],[4.7,3.2,1.6,0.2],[4.8,3.1,1.6,0.2],[5.0,3.2,1.2,0.2],[4.8,3.4,1.9,0.2],[4.8,3.4,1.6,0.2],[4.4,3.0,1.3,0.2],[4.4,2.9,1.4,0.2],[4.4,3.2,1.3,0.2],[4.3,3.0,1.1,0.1],[4.6,3.6,1.0,0.2],[5.5,4.2,1.4,0.2],[5.2,4.1,1.5,0.1],[5.4,3.9,1.3,0.4],[5.4,3.9,1.7,0.4],[5.7,3.8,1.7,0.3],[5.8,4.0,1.2,0.2],[5.7,4.4,1.5,0.4],[4.5,2.3,1.3,0.3]],"x":["sepal_length","sepal_width","petal_length","petal_width"],"y":["versicolor_44","versicolor_76","versicolor_89","versicolor_12","versicolor_93","versicolor_78","versicolor_66","versicolor_39","versicolor_41","versicolor_69","versicolor_11","versicolor_77","versicolor_118","versicolor_45","versicolor_52","versicolor_100","versicolor_109","versicolor_107","versicolor_140","virginica_15","versicolor_37","versicolor_98","virginica_6","virginica_25","virginica_30","virginica_4","virginica_116","versicolor_34","virginica_147","virginica_54","virginica_115","virginica_24","virginica_3","virginica_143","versicolor_105","versicolor_133","virginica_129","versicolor_38","versicolor_146","versicolor_0","versicolor_82","versicolor_27","versicolor_110","versicolor_120","versicolor_80","versicolor_79","versicolor_138","versicolor_68","versicolor_86","versicolor_128","versicolor_114","versicolor_136","versicolor_20","versicolor_56","versicolor_90","versicolor_88","versicolor_26","versicolor_92","versicolor_91","virginica_70","versicolor_139","versicolor_142","versicolor_40","versicolor_67","virginica_72","virginica_135","virginica_14","virginica_7","virginica_73","virginica_130","virginica_81","virginica_96","virginica_29","virginica_2","virginica_46","virginica_145","virginica_50","virginica_132","virginica_43","virginica_53","virginica_62","virginica_113","virginica_64","virginica_127","virginica_31","virginica_104","virginica_101","virginica_137","virginica_108","virginica_123","virginica_19","virginica_22","virginica_111","virginica_8","virginica_148","virginica_59","virginica_5","virginica_48","virginica_112","virginica_141","setosa_16","setosa_36","setosa_74","setosa_42","setosa_122","setosa_57","setosa_13","setosa_99","setosa_61","setosa_149","setosa_117","setosa_134","setosa_51","setosa_119","setosa_83","setosa_124","setosa_144","setosa_35","setosa_126","setosa_63","setosa_125","setosa_75","setosa_87","setosa_97","setosa_131","setosa_9","setosa_121","setosa_47","setosa_1","setosa_28","setosa_106","setosa_85","setosa_32","setosa_55","setosa_18","setosa_49","setosa_84","setosa_60","setosa_103","setosa_102","setosa_21","setosa_65","setosa_33","setosa_95","setosa_94","setosa_10","setosa_71","setosa_17","setosa_58","setosa_23"]}];
            var layout = {"margin":{"l":100.0},"title":"Clustered iris data (hierarchical clustering)"};
            var config = {};
            Plotly.newPlot('dc85d794-9610-45ad-b669-f7edf700db27', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_dc85d794961045adb669f7edf700db27();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_dc85d794961045adb669f7edf700db27();
            }
</script>
*)
(**
## Limitations

  1. There is no strong guidance on which distance function and linkage type should be used. It often is chosen arbitrarily according to the user's experience.
  2. The visual interpretation of the dendrogram is difficult, since swapping the direction of some bifurcations may totally disturbe the visual impression.

## Notes

  - Please note that depending on what data you want to cluster, a column wise z-score normalization may be required. In the presented example differences in sepal width have a reduced influence because
  the absolute variation is low.

## References

  - Vijaya et al., A Review on Hierarchical Clustering Algorithms, Journal of Engineering and Applied Sciences, 2017
  - Rani and Rohil, A Study of Hierarchical Clustering Algorithm, International Journal of Information and Computation Technology, 2013
  - FSharp.Stats documentation, fslaborg, https://fslab.org/FSharp.Stats/Clustering.html

## Further reading

Examples are taken from [FSharp.Stats documentation](https://fslab.org/FSharp.Stats/Clustering.html) that covers various techniques for an optimal cluster number determination.

The next article in this series covers [DBSCAN using FSharp.Stats](004_clustering_DBSCAN.html).


*)

