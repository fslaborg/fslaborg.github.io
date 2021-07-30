(**
[![Binder](https://fslab.org/images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/004_clustering_DBSCAN.ipynb)&emsp;
[![Script](https://fslab.org/images/badge-script.svg)](https://fslab.org/content/tutorials/004_clustering_DBSCAN.fsx)&emsp;
[![Notebook](https://fslab.org/images/badge-notebook.svg)](https://fslab.org/content/tutorials/004_clustering_DBSCAN.ipynb)


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
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.6"
#r "nuget: FSharp.Data"
```


## Loading data

*)
open FSharp.Data
open FSharp.Stats
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
    |> Chart.withX_AxisStyle header2D.[0]
    |> Chart.withY_AxisStyle header2D.[1]
    |> Chart.withTitle "rawChart2D"

let rawChart3D =
    let unzippedData =
        data3D
        |> Array.map (fun x -> x.[0],x.[1],x.[2])
    Chart.Scatter3d(unzippedData,mode=StyleParam.Mode.Markers,Labels=labels)
    |> Chart.withX_AxisStyle header3D.[0]
    |> Chart.withY_AxisStyle header3D.[1]
    |> Chart.withZ_AxisStyle header3D.[2]
    |> Chart.withTitle "rawChart3D"(* output: 
<div id="8a72e819-b49f-47b8-92ce-afa87260f084" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_8a72e819b49f47b892ceafa87260f084 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","x":[3.8,1.5,6.6,4.9,4.9,5.6,4.8,6.0,5.7,1.4,1.7,4.7,4.6,1.5,6.1,5.1,1.7,1.2,1.2,5.8,4.1,1.1,5.7,1.3,5.0,4.9,4.5,3.9,1.5,6.1,5.0,5.5,1.6,1.4,4.8,1.6,1.5,5.1,3.6,4.7,3.0,4.5,1.5,5.2,4.3,4.8,6.7,1.4,5.4,1.9,5.8,1.4,5.0,5.1,5.1,1.6,4.4,1.6,1.5,5.7,1.3,1.3,5.4,1.6,5.1,1.0,4.7,3.5,4.1,4.5,4.5,1.7,6.4,5.8,1.3,1.4,4.3,4.9,4.5,4.0,3.9,6.1,3.7,1.5,1.6,1.6,4.1,1.5,4.2,4.0,4.5,4.0,4.5,4.7,1.3,1.5,6.3,1.3,4.9,1.5,4.6,5.6,1.3,1.4,5.5,4.4,1.5,4.4,5.6,4.6,4.0,5.6,5.3,5.5,4.2,5.1,4.8,1.4,4.7,1.4,4.0,1.4,1.5,5.6,1.5,1.9,1.7,5.2,4.2,5.0,5.9,1.4,5.6,4.5,1.5,6.7,4.2,5.3,3.9,3.3,4.4,6.0,3.3,5.1,1.4,6.9,3.5,5.1,5.9,1.4],"y":[1.1,0.1,2.1,2.0,1.8,2.4,1.8,1.8,2.3,0.2,0.4,1.4,1.4,0.2,2.5,1.5,0.2,0.2,0.2,2.2,1.3,0.1,2.5,0.3,2.0,1.8,1.5,1.1,0.1,2.3,1.9,1.8,0.2,0.2,1.8,0.4,0.4,1.6,1.3,1.6,1.1,1.5,0.4,2.3,1.3,1.4,2.0,0.1,2.3,0.2,1.8,0.2,1.7,2.3,1.9,0.2,1.2,0.2,0.4,2.1,0.2,0.3,2.1,0.6,2.0,0.2,1.2,1.0,1.0,1.6,1.7,0.3,2.0,1.6,0.2,0.2,1.3,1.5,1.5,1.2,1.4,1.9,1.0,0.2,0.2,0.2,1.3,0.2,1.5,1.3,1.3,1.0,1.5,1.4,0.4,0.1,1.8,0.2,1.5,0.2,1.3,1.8,0.2,0.2,1.8,1.3,0.1,1.4,2.1,1.5,1.3,2.4,2.3,2.1,1.3,1.9,1.8,0.2,1.5,0.3,1.3,0.3,0.3,2.2,0.2,0.4,0.5,2.0,1.3,1.5,2.1,0.3,1.4,1.5,0.2,2.2,1.2,1.9,1.2,1.0,1.4,2.5,1.0,2.4,0.2,2.3,1.0,1.8,2.3,0.2],"mode":"markers","line":{},"marker":{},"text":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"]}];
            var layout = {"xaxis":{"title":"petal_length"},"yaxis":{"title":"petal_width"},"title":"rawChart2D"};
            var config = {};
            Plotly.newPlot('8a72e819-b49f-47b8-92ce-afa87260f084', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_8a72e819b49f47b892ceafa87260f084();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_8a72e819b49f47b892ceafa87260f084();
            }
</script>
*)
(**
<br>

<div id="768a24cd-f143-4067-a965-bde591a4f0ef" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_768a24cdf1434067a965bde591a4f0ef = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter3d","x":[5.5,4.9,7.6,5.6,6.1,6.3,6.2,7.2,6.9,4.9,5.4,7.0,6.1,5.4,7.2,6.3,5.4,5.8,5.0,6.5,5.6,4.3,6.7,4.5,5.7,6.3,5.6,5.6,4.9,7.7,6.3,6.4,4.7,5.5,5.9,5.0,5.4,6.0,5.6,6.3,5.1,6.4,5.1,6.7,6.2,6.8,7.7,4.8,6.2,4.8,6.7,5.1,6.7,6.9,5.8,4.8,5.5,5.1,5.7,6.7,4.4,5.0,6.9,5.0,6.5,4.6,6.1,5.0,5.8,6.0,4.9,5.7,7.9,7.2,5.5,4.6,6.4,6.9,6.0,5.8,5.2,7.4,5.5,5.1,4.8,5.0,5.7,4.6,5.9,6.1,5.7,6.0,5.4,6.1,5.4,5.2,7.3,4.7,6.3,5.3,6.6,6.3,4.4,4.4,6.5,6.3,4.9,6.6,6.4,6.5,5.5,6.7,6.4,6.8,5.7,5.8,6.0,5.2,6.7,5.1,5.5,4.8,5.1,6.4,5.0,5.1,5.1,6.5,5.6,6.0,7.1,4.6,6.1,6.2,5.2,7.7,5.7,6.4,5.8,5.0,6.7,6.3,4.9,5.8,5.0,7.7,5.7,5.9,6.8,5.0],"y":[3.8,1.5,6.6,4.9,4.9,5.6,4.8,6.0,5.7,1.4,1.7,4.7,4.6,1.5,6.1,5.1,1.7,1.2,1.2,5.8,4.1,1.1,5.7,1.3,5.0,4.9,4.5,3.9,1.5,6.1,5.0,5.5,1.6,1.4,4.8,1.6,1.5,5.1,3.6,4.7,3.0,4.5,1.5,5.2,4.3,4.8,6.7,1.4,5.4,1.9,5.8,1.4,5.0,5.1,5.1,1.6,4.4,1.6,1.5,5.7,1.3,1.3,5.4,1.6,5.1,1.0,4.7,3.5,4.1,4.5,4.5,1.7,6.4,5.8,1.3,1.4,4.3,4.9,4.5,4.0,3.9,6.1,3.7,1.5,1.6,1.6,4.1,1.5,4.2,4.0,4.5,4.0,4.5,4.7,1.3,1.5,6.3,1.3,4.9,1.5,4.6,5.6,1.3,1.4,5.5,4.4,1.5,4.4,5.6,4.6,4.0,5.6,5.3,5.5,4.2,5.1,4.8,1.4,4.7,1.4,4.0,1.4,1.5,5.6,1.5,1.9,1.7,5.2,4.2,5.0,5.9,1.4,5.6,4.5,1.5,6.7,4.2,5.3,3.9,3.3,4.4,6.0,3.3,5.1,1.4,6.9,3.5,5.1,5.9,1.4],"z":[1.1,0.1,2.1,2.0,1.8,2.4,1.8,1.8,2.3,0.2,0.4,1.4,1.4,0.2,2.5,1.5,0.2,0.2,0.2,2.2,1.3,0.1,2.5,0.3,2.0,1.8,1.5,1.1,0.1,2.3,1.9,1.8,0.2,0.2,1.8,0.4,0.4,1.6,1.3,1.6,1.1,1.5,0.4,2.3,1.3,1.4,2.0,0.1,2.3,0.2,1.8,0.2,1.7,2.3,1.9,0.2,1.2,0.2,0.4,2.1,0.2,0.3,2.1,0.6,2.0,0.2,1.2,1.0,1.0,1.6,1.7,0.3,2.0,1.6,0.2,0.2,1.3,1.5,1.5,1.2,1.4,1.9,1.0,0.2,0.2,0.2,1.3,0.2,1.5,1.3,1.3,1.0,1.5,1.4,0.4,0.1,1.8,0.2,1.5,0.2,1.3,1.8,0.2,0.2,1.8,1.3,0.1,1.4,2.1,1.5,1.3,2.4,2.3,2.1,1.3,1.9,1.8,0.2,1.5,0.3,1.3,0.3,0.3,2.2,0.2,0.4,0.5,2.0,1.3,1.5,2.1,0.3,1.4,1.5,0.2,2.2,1.2,1.9,1.2,1.0,1.4,2.5,1.0,2.4,0.2,2.3,1.0,1.8,2.3,0.2],"mode":"markers","line":{},"marker":{},"text":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"]}];
            var layout = {"scene":{"xaxis":{"title":"sepal_length"},"yaxis":{"title":"petal_length"},"zaxis":{"title":"petal_width"}},"title":"rawChart3D"};
            var config = {};
            Plotly.newPlot('768a24cd-f143-4067-a965-bde591a4f0ef', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_768a24cdf1434067a965bde591a4f0ef();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_768a24cdf1434067a965bde591a4f0ef();
            }
</script>

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

let result2D = DbScan.compute DistanceMetrics.Array.euclidean minPts eps2D data2D(* output: 
"{ Clusterlist =
   seq
     [seq [[|1.5; 0.1|]; [|1.4; 0.2|]; [|1.7; 0.4|]; [|1.5; 0.2|]; ...];
      seq [[|4.9; 2.0|]; [|4.9; 1.8|]; [|4.8; 1.8|]; [|5.0; 2.0|]; ...]]
  Noisepoints =
   seq [[|6.6; 2.1|]; [|3.0; 1.1|]; [|6.7; 2.0|]; [|6.4; 2.0|]; ...] }"*)
let result3D = DbScan.compute DistanceMetrics.Array.euclidean minPts eps3D data3D(* output: 
"{ Clusterlist =
   seq
     [seq
        [[|5.5; 3.8; 1.1|]; [|5.6; 4.1; 1.3|]; [|5.6; 3.9; 1.1|];
         [|5.6; 3.6; 1.3|]; ...];
      seq
        [[|4.9; 1.5; 0.1|]; [|4.9; 1.4; 0.2|]; [|5.4; 1.7; 0.4|];
         [|5.4; 1.5; 0.2|]; ...]]
  Noisepoints =
   seq
     [[|7.6; 6.6; 2.1|]; [|7.2; 6.1; 2.5|]; [|7.7; 6.1; 2.3|]; [|5.1; 3.0; 1.1|];
      ...] }"*)
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
    |> Chart.Combine

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
    |> Chart.Combine
    |> Chart.withTitle chartTitle2D
    |> Chart.withX_AxisStyle header2D.[0]
    |> Chart.withY_AxisStyle header2D.[1](* output: 
<div id="b4cfa652-577a-4184-b366-c1f9940e8b45" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_b4cfa652577a4184b366c1f9940e8b45 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","x":[6.6,3.0,6.7,6.4,6.7,3.3,6.9],"y":[2.1,1.1,2.0,2.0,2.2,1.0,2.3],"mode":"markers","marker":{},"name":"Noise"},{"type":"scatter","x":[1.5,1.4,1.7,1.5,1.7,1.2,1.1,1.3,1.6,1.6,1.5,1.4,1.9,1.3,1.7,1.3,1.4,1.5,1.9,1.7,1.6,1.0],"y":[0.1,0.2,0.4,0.2,0.2,0.2,0.1,0.3,0.2,0.4,0.4,0.1,0.2,0.2,0.3,0.4,0.3,0.3,0.4,0.5,0.6,0.2],"mode":"markers","marker":{},"name":"Cluster 0"},{"type":"scatter","x":[4.9,4.9,4.8,5.0,5.0,5.1,4.7,5.2,5.0,5.1,5.1,5.1,4.9,5.3,5.2,5.3,5.1,5.1,4.7,5.1,4.8,4.5,4.5,4.6,4.7,5.0,4.6,4.5,5.4,5.5,4.4,4.7,4.5,4.6,4.4,4.4,5.6,5.7,5.4,5.6,5.5,5.6,5.7,5.7,5.6,4.3,4.2,4.2,4.2,4.1,5.8,5.8,5.9,6.0,5.8,5.6,4.1,4.0,4.0,4.0,5.9,6.1,6.1,6.0,3.9,3.9,3.9,3.8,3.6,3.7,6.1,6.3,3.5],"y":[2.0,1.8,1.8,2.0,1.9,1.6,1.6,2.3,1.7,2.3,1.9,2.0,1.5,2.3,2.0,1.9,2.4,1.8,1.4,1.5,1.4,1.6,1.7,1.5,1.5,1.5,1.4,1.5,2.1,1.8,1.2,1.2,1.3,1.3,1.3,1.4,2.4,2.3,2.3,2.1,2.1,2.2,2.5,2.1,1.8,1.3,1.5,1.3,1.2,1.3,2.2,1.8,2.1,1.8,1.6,1.4,1.0,1.2,1.3,1.0,2.3,2.3,1.9,2.5,1.1,1.4,1.2,1.1,1.3,1.0,2.5,1.8,1.0],"mode":"markers","marker":{},"name":"Cluster 1"}];
            var layout = {"title":"eps: 0.5 minPts: 20 pts: 150 cluster: 2 noisePts: 8","xaxis":{"title":"petal_length"},"yaxis":{"title":"petal_width"}};
            var config = {};
            Plotly.newPlot('b4cfa652-577a-4184-b366-c1f9940e8b45', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_b4cfa652577a4184b366c1f9940e8b45();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_b4cfa652577a4184b366c1f9940e8b45();
            }
</script>
*)
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
    |> Chart.Combine

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
    |> Chart.Combine
    |> Chart.withTitle chartname3D
    |> Chart.withX_AxisStyle header3D.[0]
    |> Chart.withY_AxisStyle header3D.[1]
    |> Chart.withZ_AxisStyle header3D.[2]
    
//for faster computation you can use the squaredEuclidean distance and set your eps to its square
let clusteredChart3D() = DbScan.compute DistanceMetrics.Array.euclideanNaNSquared 20 (0.7**2.) data3D (* output: 
<div id="82990e6e-1e7c-4e34-8009-29a6363a4991" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_82990e6e1e7c4e34800929a6363a4991 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-latest.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter3d","x":[7.6,7.2,7.7,5.1,7.7,4.9,7.9,7.2,7.4,7.3,7.7,5.0,4.9,7.7],"y":[6.6,6.1,6.1,3.0,6.7,4.5,6.4,5.8,6.1,6.3,6.7,3.3,3.3,6.9],"z":[2.1,2.5,2.3,1.1,2.0,1.7,2.0,1.6,1.9,1.8,2.2,1.0,1.0,2.3],"mode":"markers","line":{},"marker":{},"name":"Noise"},{"type":"scatter3d","x":[5.5,5.6,5.6,5.6,5.5,5.0,5.8,5.8,5.2,5.5,5.7,5.9,6.1,6.0,5.5,5.7,5.6,5.7,5.8,5.7,5.6,6.2,6.0,6.0,5.7,5.4,6.1,6.1,6.1,6.3,6.4,6.2,5.9,6.3,6.4,6.0,6.6,5.6,6.0,6.3,6.6,6.5,6.7,6.7,6.1,6.2,6.3,6.3,6.3,6.0,5.8,5.9,6.8,5.7,6.5,5.8,6.7,6.9,6.5,6.4,7.0,6.1,6.4,6.5,6.3,6.4,6.7,6.2,6.4,6.4,6.3,6.9,6.7,6.9,6.7,6.8,6.5,6.7,6.9,6.7,6.3,6.8,7.2,7.1],"y":[3.8,4.1,3.9,3.6,4.4,3.5,4.1,4.0,3.9,3.7,4.1,4.2,4.0,4.0,4.0,4.2,4.2,4.2,3.9,3.5,4.5,4.3,4.5,4.5,4.5,4.5,4.6,4.7,4.7,4.4,4.3,4.5,4.8,4.7,4.5,4.8,4.4,4.9,5.0,4.9,4.6,4.6,4.7,4.4,4.9,4.8,5.1,4.9,5.0,5.1,5.1,5.1,4.8,5.0,5.1,5.1,5.0,4.9,5.2,5.3,4.7,5.6,5.5,5.5,5.6,5.3,5.2,5.4,5.6,5.6,5.6,5.1,5.7,5.4,5.6,5.5,5.8,5.8,5.7,5.7,6.0,5.9,6.0,5.9],"z":[1.1,1.3,1.1,1.3,1.2,1.0,1.0,1.2,1.4,1.0,1.3,1.5,1.3,1.0,1.3,1.3,1.3,1.2,1.2,1.0,1.5,1.3,1.6,1.5,1.3,1.5,1.4,1.2,1.4,1.3,1.3,1.5,1.8,1.6,1.5,1.8,1.4,2.0,1.5,1.5,1.3,1.5,1.5,1.4,1.8,1.8,1.5,1.8,1.9,1.6,1.9,1.8,1.4,2.0,2.0,2.4,1.7,1.5,2.0,1.9,1.4,1.4,1.8,1.8,1.8,2.3,2.3,2.3,2.1,2.2,2.4,2.3,2.1,2.1,2.4,2.1,2.2,1.8,2.3,2.5,2.5,2.3,1.8,2.1],"mode":"markers","line":{},"marker":{},"name":"Cluster_0"},{"type":"scatter3d","x":[4.9,4.9,5.4,5.4,5.4,5.0,4.5,4.7,5.5,5.0,5.4,5.1,4.8,4.8,5.1,4.8,5.1,4.4,5.0,5.0,4.6,5.5,4.6,5.1,5.0,4.6,5.4,5.2,4.7,5.3,4.4,5.2,5.1,4.8,5.1,5.0,5.1,5.1,4.6,5.2,5.0,4.3,5.8,5.7,5.7],"y":[1.5,1.4,1.7,1.5,1.7,1.2,1.3,1.6,1.4,1.6,1.5,1.5,1.4,1.9,1.4,1.6,1.6,1.3,1.3,1.6,1.0,1.3,1.4,1.5,1.6,1.5,1.3,1.5,1.3,1.5,1.4,1.4,1.4,1.4,1.5,1.5,1.9,1.7,1.4,1.5,1.4,1.1,1.2,1.5,1.7],"z":[0.1,0.2,0.4,0.2,0.2,0.2,0.3,0.2,0.2,0.4,0.4,0.4,0.1,0.2,0.2,0.2,0.2,0.2,0.3,0.6,0.2,0.2,0.2,0.2,0.2,0.2,0.4,0.1,0.2,0.2,0.2,0.2,0.3,0.3,0.3,0.2,0.4,0.5,0.3,0.2,0.2,0.1,0.2,0.4,0.3],"mode":"markers","line":{},"marker":{},"name":"Cluster_1"}];
            var layout = {"title":"eps: 0.7 minPts: 20 pts: 150 cluster: 2 noisePts: 14","scene":{"xaxis":{"title":"sepal_length"},"yaxis":{"title":"petal_length"},"zaxis":{"title":"petal_width"}}};
            var config = {};
            Plotly.newPlot('82990e6e-1e7c-4e34-8009-29a6363a4991', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_82990e6e1e7c4e34800929a6363a4991();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_82990e6e1e7c4e34800929a6363a4991();
            }
</script>
*)
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

