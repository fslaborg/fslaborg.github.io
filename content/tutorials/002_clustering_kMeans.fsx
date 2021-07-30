(**
[![Binder](https://fslab.org/images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/002_clustering_kMeans.ipynb)&emsp;
[![Script](https://fslab.org/images/badge-script.svg)](https://fslab.org/content/tutorials/002_clustering_kMeans.fsx)&emsp;
[![Notebook](https://fslab.org/images/badge-notebook.svg)](https://fslab.org/content/tutorials/002_clustering_kMeans.ipynb)


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

<img style="max-width:75%" src="../../images/kMeans.png"></img>

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
Since the species identifier occur several times (_Iris-virginica_, _Iris-versicolor_, and _Iris-setosa_), we create unique labels by adding the rows index to the species identifier.


*)
open Plotly.NET

let colNames = ["sepal_length";"sepal_width";"petal_length";"petal_width"]

// isolate data as float [] []
let data = 
    Frame.dropCol "species" df
    |> Frame.toJaggedArray

//isolate labels as seq<string>
let labels = 
    Frame.getCol "species" df
    |> Series.values
    |> Seq.mapi (fun i s -> sprintf "%s_%i" s i)

let dataChart = 
    Chart.Heatmap(data,ColNames=colNames,RowNames=labels)
    // required to fit the species identifier on the left side of the heatmap
    |> Chart.withMarginSize(Left=100.)
    |> Chart.withTitle "raw iris data"

// required to fit the species identifier on the left side of the heatmap(* output: 
<div id="548d7375-0efc-451e-be46-0431614d282b" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_548d73750efc451ebe460431614d282b = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"heatmap","z":[[5.5,2.4,3.8,1.1],[4.9,3.1,1.5,0.1],[7.6,3.0,6.6,2.1],[5.6,2.8,4.9,2.0],[6.1,3.0,4.9,1.8],[6.3,3.4,5.6,2.4],[6.2,2.8,4.8,1.8],[7.2,3.2,6.0,1.8],[6.9,3.2,5.7,2.3],[4.9,3.0,1.4,0.2],[5.4,3.9,1.7,0.4],[7.0,3.2,4.7,1.4],[6.1,3.0,4.6,1.4],[5.4,3.7,1.5,0.2],[7.2,3.6,6.1,2.5],[6.3,2.8,5.1,1.5],[5.4,3.4,1.7,0.2],[5.8,4.0,1.2,0.2],[5.0,3.2,1.2,0.2],[6.5,3.0,5.8,2.2],[5.6,3.0,4.1,1.3],[4.3,3.0,1.1,0.1],[6.7,3.3,5.7,2.5],[4.5,2.3,1.3,0.3],[5.7,2.5,5.0,2.0],[6.3,2.7,4.9,1.8],[5.6,3.0,4.5,1.5],[5.6,2.5,3.9,1.1],[4.9,3.1,1.5,0.1],[7.7,3.0,6.1,2.3],[6.3,2.5,5.0,1.9],[6.4,3.1,5.5,1.8],[4.7,3.2,1.6,0.2],[5.5,4.2,1.4,0.2],[5.9,3.2,4.8,1.8],[5.0,3.4,1.6,0.4],[5.4,3.4,1.5,0.4],[6.0,2.7,5.1,1.6],[5.6,2.9,3.6,1.3],[6.3,3.3,4.7,1.6],[5.1,2.5,3.0,1.1],[6.4,3.2,4.5,1.5],[5.1,3.7,1.5,0.4],[6.7,3.0,5.2,2.3],[6.2,2.9,4.3,1.3],[6.8,2.8,4.8,1.4],[7.7,2.8,6.7,2.0],[4.8,3.0,1.4,0.1],[6.2,3.4,5.4,2.3],[4.8,3.4,1.9,0.2],[6.7,2.5,5.8,1.8],[5.1,3.5,1.4,0.2],[6.7,3.0,5.0,1.7],[6.9,3.1,5.1,2.3],[5.8,2.7,5.1,1.9],[4.8,3.1,1.6,0.2],[5.5,2.6,4.4,1.2],[5.1,3.8,1.6,0.2],[5.7,4.4,1.5,0.4],[6.7,3.3,5.7,2.1],[4.4,3.0,1.3,0.2],[5.0,3.5,1.3,0.3],[6.9,3.1,5.4,2.1],[5.0,3.5,1.6,0.6],[6.5,3.2,5.1,2.0],[4.6,3.6,1.0,0.2],[6.1,2.8,4.7,1.2],[5.0,2.0,3.5,1.0],[5.8,2.7,4.1,1.0],[6.0,3.4,4.5,1.6],[4.9,2.5,4.5,1.7],[5.7,3.8,1.7,0.3],[7.9,3.8,6.4,2.0],[7.2,3.0,5.8,1.6],[5.5,3.5,1.3,0.2],[4.6,3.2,1.4,0.2],[6.4,2.9,4.3,1.3],[6.9,3.1,4.9,1.5],[6.0,2.9,4.5,1.5],[5.8,2.6,4.0,1.2],[5.2,2.7,3.9,1.4],[7.4,2.8,6.1,1.9],[5.5,2.4,3.7,1.0],[5.1,3.4,1.5,0.2],[4.8,3.4,1.6,0.2],[5.0,3.0,1.6,0.2],[5.7,2.8,4.1,1.3],[4.6,3.1,1.5,0.2],[5.9,3.0,4.2,1.5],[6.1,2.8,4.0,1.3],[5.7,2.8,4.5,1.3],[6.0,2.2,4.0,1.0],[5.4,3.0,4.5,1.5],[6.1,2.9,4.7,1.4],[5.4,3.9,1.3,0.4],[5.2,4.1,1.5,0.1],[7.3,2.9,6.3,1.8],[4.7,3.2,1.3,0.2],[6.3,2.5,4.9,1.5],[5.3,3.7,1.5,0.2],[6.6,2.9,4.6,1.3],[6.3,2.9,5.6,1.8],[4.4,3.2,1.3,0.2],[4.4,2.9,1.4,0.2],[6.5,3.0,5.5,1.8],[6.3,2.3,4.4,1.3],[4.9,3.1,1.5,0.1],[6.6,3.0,4.4,1.4],[6.4,2.8,5.6,2.1],[6.5,2.8,4.6,1.5],[5.5,2.3,4.0,1.3],[6.7,3.1,5.6,2.4],[6.4,3.2,5.3,2.3],[6.8,3.0,5.5,2.1],[5.7,2.9,4.2,1.3],[5.8,2.7,5.1,1.9],[6.0,3.0,4.8,1.8],[5.2,3.4,1.4,0.2],[6.7,3.1,4.7,1.5],[5.1,3.5,1.4,0.3],[5.5,2.5,4.0,1.3],[4.8,3.0,1.4,0.3],[5.1,3.8,1.5,0.3],[6.4,2.8,5.6,2.2],[5.0,3.4,1.5,0.2],[5.1,3.8,1.9,0.4],[5.1,3.3,1.7,0.5],[6.5,3.0,5.2,2.0],[5.6,2.7,4.2,1.3],[6.0,2.2,5.0,1.5],[7.1,3.0,5.9,2.1],[4.6,3.4,1.4,0.3],[6.1,2.6,5.6,1.4],[6.2,2.2,4.5,1.5],[5.2,3.5,1.5,0.2],[7.7,3.8,6.7,2.2],[5.7,3.0,4.2,1.2],[6.4,2.7,5.3,1.9],[5.8,2.7,3.9,1.2],[5.0,2.3,3.3,1.0],[6.7,3.1,4.4,1.4],[6.3,3.3,6.0,2.5],[4.9,2.4,3.3,1.0],[5.8,2.8,5.1,2.4],[5.0,3.3,1.4,0.2],[7.7,2.6,6.9,2.3],[5.7,2.6,3.5,1.0],[5.9,3.0,5.1,1.8],[6.8,3.2,5.9,2.3],[5.0,3.6,1.4,0.2]],"x":["sepal_length","sepal_width","petal_length","petal_width"],"y":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"]}];
            var layout = {"margin":{"l":100.0},"title":"raw iris data"};
            var config = {};
            Plotly.newPlot('548d7375-0efc-451e-be46-0431614d282b', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_548d73750efc451ebe460431614d282b();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_548d73750efc451ebe460431614d282b();
            }
</script>
*)
(**
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

// Each datapoint is given associated with its cluster index, species identifier, and coordinates.(* output: 
"1, "versicolor_0", [|5.5; 2.4; 3.8; 1.1|]
2, "setosa_1", [|4.9; 3.1; 1.5; 0.1|]
3, "virginica_2", [|7.6; 3.0; 6.6; 2.1|]
4, "virginica_3", [|5.6; 2.8; 4.9; 2.0|]
4, "virginica_4", [|6.1; 3.0; 4.9; 1.8|]
3, "virginica_5", [|6.3; 3.4; 5.6; 2.4|]
4, "virginica_6", [|6.2; 2.8; 4.8; 1.8|]
 ... "*)
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
        |> Chart.withTitle "clustered iris data (k-means clustering)"(* output: 
<div id="6969e4b1-1a94-4412-adb9-7fc984bdb8b5" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_6969e4b11a944412adb97fc984bdb8b5 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"heatmap","z":[[5.5,2.4,3.8,1.1],[5.6,3.0,4.1,1.3],[5.6,3.0,4.5,1.5],[5.6,2.5,3.9,1.1],[5.6,2.9,3.6,1.3],[5.1,2.5,3.0,1.1],[6.2,2.9,4.3,1.3],[5.5,2.6,4.4,1.2],[5.0,2.0,3.5,1.0],[5.8,2.7,4.1,1.0],[4.9,2.5,4.5,1.7],[5.8,2.6,4.0,1.2],[5.2,2.7,3.9,1.4],[5.5,2.4,3.7,1.0],[5.7,2.8,4.1,1.3],[5.9,3.0,4.2,1.5],[6.1,2.8,4.0,1.3],[5.7,2.8,4.5,1.3],[6.0,2.2,4.0,1.0],[5.4,3.0,4.5,1.5],[6.3,2.3,4.4,1.3],[5.5,2.3,4.0,1.3],[5.7,2.9,4.2,1.3],[5.5,2.5,4.0,1.3],[5.6,2.7,4.2,1.3],[5.7,3.0,4.2,1.2],[5.8,2.7,3.9,1.2],[5.0,2.3,3.3,1.0],[4.9,2.4,3.3,1.0],[5.7,2.6,3.5,1.0],[4.9,3.1,1.5,0.1],[4.9,3.0,1.4,0.2],[5.4,3.9,1.7,0.4],[5.4,3.7,1.5,0.2],[5.4,3.4,1.7,0.2],[5.8,4.0,1.2,0.2],[5.0,3.2,1.2,0.2],[4.3,3.0,1.1,0.1],[4.5,2.3,1.3,0.3],[4.9,3.1,1.5,0.1],[4.7,3.2,1.6,0.2],[5.5,4.2,1.4,0.2],[5.0,3.4,1.6,0.4],[5.4,3.4,1.5,0.4],[5.1,3.7,1.5,0.4],[4.8,3.0,1.4,0.1],[4.8,3.4,1.9,0.2],[5.1,3.5,1.4,0.2],[4.8,3.1,1.6,0.2],[5.1,3.8,1.6,0.2],[5.7,4.4,1.5,0.4],[4.4,3.0,1.3,0.2],[5.0,3.5,1.3,0.3],[5.0,3.5,1.6,0.6],[4.6,3.6,1.0,0.2],[5.7,3.8,1.7,0.3],[5.5,3.5,1.3,0.2],[4.6,3.2,1.4,0.2],[5.1,3.4,1.5,0.2],[4.8,3.4,1.6,0.2],[5.0,3.0,1.6,0.2],[4.6,3.1,1.5,0.2],[5.4,3.9,1.3,0.4],[5.2,4.1,1.5,0.1],[4.7,3.2,1.3,0.2],[5.3,3.7,1.5,0.2],[4.4,3.2,1.3,0.2],[4.4,2.9,1.4,0.2],[4.9,3.1,1.5,0.1],[5.2,3.4,1.4,0.2],[5.1,3.5,1.4,0.3],[4.8,3.0,1.4,0.3],[5.1,3.8,1.5,0.3],[5.0,3.4,1.5,0.2],[5.1,3.8,1.9,0.4],[5.1,3.3,1.7,0.5],[4.6,3.4,1.4,0.3],[5.2,3.5,1.5,0.2],[5.0,3.3,1.4,0.2],[5.0,3.6,1.4,0.2],[7.6,3.0,6.6,2.1],[6.3,3.4,5.6,2.4],[7.2,3.2,6.0,1.8],[6.9,3.2,5.7,2.3],[7.2,3.6,6.1,2.5],[6.5,3.0,5.8,2.2],[6.7,3.3,5.7,2.5],[7.7,3.0,6.1,2.3],[7.7,2.8,6.7,2.0],[6.7,2.5,5.8,1.8],[6.7,3.3,5.7,2.1],[6.9,3.1,5.4,2.1],[7.9,3.8,6.4,2.0],[7.2,3.0,5.8,1.6],[7.4,2.8,6.1,1.9],[7.3,2.9,6.3,1.8],[6.7,3.1,5.6,2.4],[6.8,3.0,5.5,2.1],[7.1,3.0,5.9,2.1],[7.7,3.8,6.7,2.2],[6.3,3.3,6.0,2.5],[7.7,2.6,6.9,2.3],[6.8,3.2,5.9,2.3],[5.6,2.8,4.9,2.0],[6.1,3.0,4.9,1.8],[6.2,2.8,4.8,1.8],[7.0,3.2,4.7,1.4],[6.1,3.0,4.6,1.4],[6.3,2.8,5.1,1.5],[5.7,2.5,5.0,2.0],[6.3,2.7,4.9,1.8],[6.3,2.5,5.0,1.9],[6.4,3.1,5.5,1.8],[5.9,3.2,4.8,1.8],[6.0,2.7,5.1,1.6],[6.3,3.3,4.7,1.6],[6.4,3.2,4.5,1.5],[6.7,3.0,5.2,2.3],[6.8,2.8,4.8,1.4],[6.2,3.4,5.4,2.3],[6.7,3.0,5.0,1.7],[6.9,3.1,5.1,2.3],[5.8,2.7,5.1,1.9],[6.5,3.2,5.1,2.0],[6.1,2.8,4.7,1.2],[6.0,3.4,4.5,1.6],[6.4,2.9,4.3,1.3],[6.9,3.1,4.9,1.5],[6.0,2.9,4.5,1.5],[6.1,2.9,4.7,1.4],[6.3,2.5,4.9,1.5],[6.6,2.9,4.6,1.3],[6.3,2.9,5.6,1.8],[6.5,3.0,5.5,1.8],[6.6,3.0,4.4,1.4],[6.4,2.8,5.6,2.1],[6.5,2.8,4.6,1.5],[6.4,3.2,5.3,2.3],[5.8,2.7,5.1,1.9],[6.0,3.0,4.8,1.8],[6.7,3.1,4.7,1.5],[6.4,2.8,5.6,2.2],[6.5,3.0,5.2,2.0],[6.0,2.2,5.0,1.5],[6.1,2.6,5.6,1.4],[6.2,2.2,4.5,1.5],[6.4,2.7,5.3,1.9],[6.7,3.1,4.4,1.4],[5.8,2.8,5.1,2.4],[5.9,3.0,5.1,1.8]],"x":["sepal_length","sepal_width","petal_length","petal_width"],"y":["versicolor_0","versicolor_20","versicolor_26","versicolor_27","versicolor_38","versicolor_40","versicolor_44","versicolor_56","versicolor_67","versicolor_68","virginica_70","versicolor_79","versicolor_80","versicolor_82","versicolor_86","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_105","versicolor_110","versicolor_114","versicolor_120","versicolor_128","versicolor_136","versicolor_138","versicolor_139","versicolor_142","versicolor_146","setosa_1","setosa_9","setosa_10","setosa_13","setosa_16","setosa_17","setosa_18","setosa_21","setosa_23","setosa_28","setosa_32","setosa_33","setosa_35","setosa_36","setosa_42","setosa_47","setosa_49","setosa_51","setosa_55","setosa_57","setosa_58","setosa_60","setosa_61","setosa_63","setosa_65","setosa_71","setosa_74","setosa_75","setosa_83","setosa_84","setosa_85","setosa_87","setosa_94","setosa_95","setosa_97","setosa_99","setosa_102","setosa_103","setosa_106","setosa_117","setosa_119","setosa_121","setosa_122","setosa_124","setosa_125","setosa_126","setosa_131","setosa_134","setosa_144","setosa_149","virginica_2","virginica_5","virginica_7","virginica_8","virginica_14","virginica_19","virginica_22","virginica_29","virginica_46","virginica_50","virginica_59","virginica_62","virginica_72","virginica_73","virginica_81","virginica_96","virginica_111","virginica_113","virginica_130","virginica_135","virginica_141","virginica_145","virginica_148","virginica_3","virginica_4","virginica_6","versicolor_11","versicolor_12","virginica_15","virginica_24","virginica_25","virginica_30","virginica_31","versicolor_34","versicolor_37","versicolor_39","versicolor_41","virginica_43","versicolor_45","virginica_48","versicolor_52","virginica_53","virginica_54","virginica_64","versicolor_66","versicolor_69","versicolor_76","versicolor_77","versicolor_78","versicolor_93","versicolor_98","versicolor_100","virginica_101","virginica_104","versicolor_107","virginica_108","versicolor_109","virginica_112","virginica_115","virginica_116","versicolor_118","virginica_123","virginica_127","virginica_129","virginica_132","versicolor_133","virginica_137","versicolor_140","virginica_143","virginica_147"]}];
            var layout = {"margin":{"l":100.0},"title":"clustered iris data (k-means clustering)"};
            var config = {};
            Plotly.newPlot('6969e4b1-1a94-4412-adb9-7fc984bdb8b5', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_6969e4b11a944412adb97fc984bdb8b5();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_6969e4b11a944412adb97fc984bdb8b5();
            }
</script>
*)
(**
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
    |> Chart.withZ_AxisStyle colNames.[3](* output: 
<div id="a6064658-19cd-4c81-8da1-36560544baee" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_a606465819cd4c818da136560544baee = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter3d","x":[5.5,5.6,5.6,5.6,5.6,5.1,6.2,5.5,5.0,5.8,4.9,5.8,5.2,5.5,5.7,5.9,6.1,5.7,6.0,5.4,6.3,5.5,5.7,5.5,5.6,5.7,5.8,5.0,4.9,5.7],"y":[3.8,4.1,4.5,3.9,3.6,3.0,4.3,4.4,3.5,4.1,4.5,4.0,3.9,3.7,4.1,4.2,4.0,4.5,4.0,4.5,4.4,4.0,4.2,4.0,4.2,4.2,3.9,3.3,3.3,3.5],"z":[1.1,1.3,1.5,1.1,1.3,1.1,1.3,1.2,1.0,1.0,1.7,1.2,1.4,1.0,1.3,1.5,1.3,1.3,1.0,1.5,1.3,1.3,1.3,1.3,1.3,1.2,1.2,1.0,1.0,1.0],"mode":"markers","name":"cluster 1","line":{},"marker":{},"text":["versicolor_0","versicolor_20","versicolor_26","versicolor_27","versicolor_38","versicolor_40","versicolor_44","versicolor_56","versicolor_67","versicolor_68","virginica_70","versicolor_79","versicolor_80","versicolor_82","versicolor_86","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_105","versicolor_110","versicolor_114","versicolor_120","versicolor_128","versicolor_136","versicolor_138","versicolor_139","versicolor_142","versicolor_146"]},{"type":"scatter3d","x":[4.9,4.9,5.4,5.4,5.4,5.8,5.0,4.3,4.5,4.9,4.7,5.5,5.0,5.4,5.1,4.8,4.8,5.1,4.8,5.1,5.7,4.4,5.0,5.0,4.6,5.7,5.5,4.6,5.1,4.8,5.0,4.6,5.4,5.2,4.7,5.3,4.4,4.4,4.9,5.2,5.1,4.8,5.1,5.0,5.1,5.1,4.6,5.2,5.0,5.0],"y":[1.5,1.4,1.7,1.5,1.7,1.2,1.2,1.1,1.3,1.5,1.6,1.4,1.6,1.5,1.5,1.4,1.9,1.4,1.6,1.6,1.5,1.3,1.3,1.6,1.0,1.7,1.3,1.4,1.5,1.6,1.6,1.5,1.3,1.5,1.3,1.5,1.3,1.4,1.5,1.4,1.4,1.4,1.5,1.5,1.9,1.7,1.4,1.5,1.4,1.4],"z":[0.1,0.2,0.4,0.2,0.2,0.2,0.2,0.1,0.3,0.1,0.2,0.2,0.4,0.4,0.4,0.1,0.2,0.2,0.2,0.2,0.4,0.2,0.3,0.6,0.2,0.3,0.2,0.2,0.2,0.2,0.2,0.2,0.4,0.1,0.2,0.2,0.2,0.2,0.1,0.2,0.3,0.3,0.3,0.2,0.4,0.5,0.3,0.2,0.2,0.2],"mode":"markers","name":"cluster 2","line":{},"marker":{},"text":["setosa_1","setosa_9","setosa_10","setosa_13","setosa_16","setosa_17","setosa_18","setosa_21","setosa_23","setosa_28","setosa_32","setosa_33","setosa_35","setosa_36","setosa_42","setosa_47","setosa_49","setosa_51","setosa_55","setosa_57","setosa_58","setosa_60","setosa_61","setosa_63","setosa_65","setosa_71","setosa_74","setosa_75","setosa_83","setosa_84","setosa_85","setosa_87","setosa_94","setosa_95","setosa_97","setosa_99","setosa_102","setosa_103","setosa_106","setosa_117","setosa_119","setosa_121","setosa_122","setosa_124","setosa_125","setosa_126","setosa_131","setosa_134","setosa_144","setosa_149"]},{"type":"scatter3d","x":[7.6,6.3,7.2,6.9,7.2,6.5,6.7,7.7,7.7,6.7,6.7,6.9,7.9,7.2,7.4,7.3,6.7,6.8,7.1,7.7,6.3,7.7,6.8],"y":[6.6,5.6,6.0,5.7,6.1,5.8,5.7,6.1,6.7,5.8,5.7,5.4,6.4,5.8,6.1,6.3,5.6,5.5,5.9,6.7,6.0,6.9,5.9],"z":[2.1,2.4,1.8,2.3,2.5,2.2,2.5,2.3,2.0,1.8,2.1,2.1,2.0,1.6,1.9,1.8,2.4,2.1,2.1,2.2,2.5,2.3,2.3],"mode":"markers","name":"cluster 3","line":{},"marker":{},"text":["virginica_2","virginica_5","virginica_7","virginica_8","virginica_14","virginica_19","virginica_22","virginica_29","virginica_46","virginica_50","virginica_59","virginica_62","virginica_72","virginica_73","virginica_81","virginica_96","virginica_111","virginica_113","virginica_130","virginica_135","virginica_141","virginica_145","virginica_148"]},{"type":"scatter3d","x":[5.6,6.1,6.2,7.0,6.1,6.3,5.7,6.3,6.3,6.4,5.9,6.0,6.3,6.4,6.7,6.8,6.2,6.7,6.9,5.8,6.5,6.1,6.0,6.4,6.9,6.0,6.1,6.3,6.6,6.3,6.5,6.6,6.4,6.5,6.4,5.8,6.0,6.7,6.4,6.5,6.0,6.1,6.2,6.4,6.7,5.8,5.9],"y":[4.9,4.9,4.8,4.7,4.6,5.1,5.0,4.9,5.0,5.5,4.8,5.1,4.7,4.5,5.2,4.8,5.4,5.0,5.1,5.1,5.1,4.7,4.5,4.3,4.9,4.5,4.7,4.9,4.6,5.6,5.5,4.4,5.6,4.6,5.3,5.1,4.8,4.7,5.6,5.2,5.0,5.6,4.5,5.3,4.4,5.1,5.1],"z":[2.0,1.8,1.8,1.4,1.4,1.5,2.0,1.8,1.9,1.8,1.8,1.6,1.6,1.5,2.3,1.4,2.3,1.7,2.3,1.9,2.0,1.2,1.6,1.3,1.5,1.5,1.4,1.5,1.3,1.8,1.8,1.4,2.1,1.5,2.3,1.9,1.8,1.5,2.2,2.0,1.5,1.4,1.5,1.9,1.4,2.4,1.8],"mode":"markers","name":"cluster 4","line":{},"marker":{},"text":["virginica_3","virginica_4","virginica_6","versicolor_11","versicolor_12","virginica_15","virginica_24","virginica_25","virginica_30","virginica_31","versicolor_34","versicolor_37","versicolor_39","versicolor_41","virginica_43","versicolor_45","virginica_48","versicolor_52","virginica_53","virginica_54","virginica_64","versicolor_66","versicolor_69","versicolor_76","versicolor_77","versicolor_78","versicolor_93","versicolor_98","versicolor_100","virginica_101","virginica_104","versicolor_107","virginica_108","versicolor_109","virginica_112","virginica_115","virginica_116","versicolor_118","virginica_123","virginica_127","virginica_129","virginica_132","versicolor_133","virginica_137","versicolor_140","virginica_143","virginica_147"]}];
            var layout = {"title":"isolated coordinates of clustered iris data (k-means clustering)","scene":{"xaxis":{"title":"sepal_length"},"yaxis":{"title":"petal_length"},"zaxis":{"title":"petal_width"}}};
            var config = {};
            Plotly.newPlot('a6064658-19cd-4c81-8da1-36560544baee', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_a606465819cd4c818da136560544baee();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_a606465819cd4c818da136560544baee();
            }
</script>
*)
(**
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
        |> Chart.withTitle "iris data set average dispersion per k"(* output: 
<div id="19356474-99cb-4c82-9f9a-dcf0dba1ac43" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_1935647499cb4c829f9adcf0dba1ac43 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","x":[2,3,4,5,6,7,8,9,10],"y":[2.6540379473960134,0.564893306317678,0.34102450719761757,0.238212140944339,0.14591591315756597,0.15986090339424835,0.14994624085880273,0.11166928411799741,0.08933049886109387],"mode":"lines","line":{},"marker":{},"error_y":{"array":[0.0,0.000727037053283333,0.11860892454845733,0.09867045906502948,0.028958222315139556,0.042695681878844656,0.042758246455209335,0.04317559308663152,0.044036544869561774]}}];
            var layout = {"xaxis":{"title":"k"},"yaxis":{"title":"average dispersion"},"title":"iris data set average dispersion per k"};
            var config = {};
            Plotly.newPlot('19356474-99cb-4c82-9f9a-dcf0dba1ac43', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_1935647499cb4c829f9adcf0dba1ac43();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_1935647499cb4c829f9adcf0dba1ac43();
            }
</script>
*)
(**
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
  
The next article in this series covers [hierarchical clustering using FSharp.Stats](003_clustering_hierarchical.html).


*)

