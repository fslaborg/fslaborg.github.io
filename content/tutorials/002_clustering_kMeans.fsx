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
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.12"
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
    Chart.Heatmap(data,colNames=colNames,rowNames=labels)
    // required to fit the species identifier on the left side of the heatmap
    |> Chart.withMarginSize(Left=100.)
    |> Chart.withTitle "raw iris data"

// required to fit the species identifier on the left side of the heatmap(* output: 
<div id="df4aceff-3e57-41fb-8a17-84d6d51d379d"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_df4aceff3e5741fb8a1784d6d51d379d = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"heatmap","x":["sepal_length","sepal_width","petal_length","petal_width"],"y":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"],"z":[[5.5,2.4,3.8,1.1],[4.9,3.1,1.5,0.1],[7.6,3.0,6.6,2.1],[5.6,2.8,4.9,2.0],[6.1,3.0,4.9,1.8],[6.3,3.4,5.6,2.4],[6.2,2.8,4.8,1.8],[7.2,3.2,6.0,1.8],[6.9,3.2,5.7,2.3],[4.9,3.0,1.4,0.2],[5.4,3.9,1.7,0.4],[7.0,3.2,4.7,1.4],[6.1,3.0,4.6,1.4],[5.4,3.7,1.5,0.2],[7.2,3.6,6.1,2.5],[6.3,2.8,5.1,1.5],[5.4,3.4,1.7,0.2],[5.8,4.0,1.2,0.2],[5.0,3.2,1.2,0.2],[6.5,3.0,5.8,2.2],[5.6,3.0,4.1,1.3],[4.3,3.0,1.1,0.1],[6.7,3.3,5.7,2.5],[4.5,2.3,1.3,0.3],[5.7,2.5,5.0,2.0],[6.3,2.7,4.9,1.8],[5.6,3.0,4.5,1.5],[5.6,2.5,3.9,1.1],[4.9,3.1,1.5,0.1],[7.7,3.0,6.1,2.3],[6.3,2.5,5.0,1.9],[6.4,3.1,5.5,1.8],[4.7,3.2,1.6,0.2],[5.5,4.2,1.4,0.2],[5.9,3.2,4.8,1.8],[5.0,3.4,1.6,0.4],[5.4,3.4,1.5,0.4],[6.0,2.7,5.1,1.6],[5.6,2.9,3.6,1.3],[6.3,3.3,4.7,1.6],[5.1,2.5,3.0,1.1],[6.4,3.2,4.5,1.5],[5.1,3.7,1.5,0.4],[6.7,3.0,5.2,2.3],[6.2,2.9,4.3,1.3],[6.8,2.8,4.8,1.4],[7.7,2.8,6.7,2.0],[4.8,3.0,1.4,0.1],[6.2,3.4,5.4,2.3],[4.8,3.4,1.9,0.2],[6.7,2.5,5.8,1.8],[5.1,3.5,1.4,0.2],[6.7,3.0,5.0,1.7],[6.9,3.1,5.1,2.3],[5.8,2.7,5.1,1.9],[4.8,3.1,1.6,0.2],[5.5,2.6,4.4,1.2],[5.1,3.8,1.6,0.2],[5.7,4.4,1.5,0.4],[6.7,3.3,5.7,2.1],[4.4,3.0,1.3,0.2],[5.0,3.5,1.3,0.3],[6.9,3.1,5.4,2.1],[5.0,3.5,1.6,0.6],[6.5,3.2,5.1,2.0],[4.6,3.6,1.0,0.2],[6.1,2.8,4.7,1.2],[5.0,2.0,3.5,1.0],[5.8,2.7,4.1,1.0],[6.0,3.4,4.5,1.6],[4.9,2.5,4.5,1.7],[5.7,3.8,1.7,0.3],[7.9,3.8,6.4,2.0],[7.2,3.0,5.8,1.6],[5.5,3.5,1.3,0.2],[4.6,3.2,1.4,0.2],[6.4,2.9,4.3,1.3],[6.9,3.1,4.9,1.5],[6.0,2.9,4.5,1.5],[5.8,2.6,4.0,1.2],[5.2,2.7,3.9,1.4],[7.4,2.8,6.1,1.9],[5.5,2.4,3.7,1.0],[5.1,3.4,1.5,0.2],[4.8,3.4,1.6,0.2],[5.0,3.0,1.6,0.2],[5.7,2.8,4.1,1.3],[4.6,3.1,1.5,0.2],[5.9,3.0,4.2,1.5],[6.1,2.8,4.0,1.3],[5.7,2.8,4.5,1.3],[6.0,2.2,4.0,1.0],[5.4,3.0,4.5,1.5],[6.1,2.9,4.7,1.4],[5.4,3.9,1.3,0.4],[5.2,4.1,1.5,0.1],[7.3,2.9,6.3,1.8],[4.7,3.2,1.3,0.2],[6.3,2.5,4.9,1.5],[5.3,3.7,1.5,0.2],[6.6,2.9,4.6,1.3],[6.3,2.9,5.6,1.8],[4.4,3.2,1.3,0.2],[4.4,2.9,1.4,0.2],[6.5,3.0,5.5,1.8],[6.3,2.3,4.4,1.3],[4.9,3.1,1.5,0.1],[6.6,3.0,4.4,1.4],[6.4,2.8,5.6,2.1],[6.5,2.8,4.6,1.5],[5.5,2.3,4.0,1.3],[6.7,3.1,5.6,2.4],[6.4,3.2,5.3,2.3],[6.8,3.0,5.5,2.1],[5.7,2.9,4.2,1.3],[5.8,2.7,5.1,1.9],[6.0,3.0,4.8,1.8],[5.2,3.4,1.4,0.2],[6.7,3.1,4.7,1.5],[5.1,3.5,1.4,0.3],[5.5,2.5,4.0,1.3],[4.8,3.0,1.4,0.3],[5.1,3.8,1.5,0.3],[6.4,2.8,5.6,2.2],[5.0,3.4,1.5,0.2],[5.1,3.8,1.9,0.4],[5.1,3.3,1.7,0.5],[6.5,3.0,5.2,2.0],[5.6,2.7,4.2,1.3],[6.0,2.2,5.0,1.5],[7.1,3.0,5.9,2.1],[4.6,3.4,1.4,0.3],[6.1,2.6,5.6,1.4],[6.2,2.2,4.5,1.5],[5.2,3.5,1.5,0.2],[7.7,3.8,6.7,2.2],[5.7,3.0,4.2,1.2],[6.4,2.7,5.3,1.9],[5.8,2.7,3.9,1.2],[5.0,2.3,3.3,1.0],[6.7,3.1,4.4,1.4],[6.3,3.3,6.0,2.5],[4.9,2.4,3.3,1.0],[5.8,2.8,5.1,2.4],[5.0,3.3,1.4,0.2],[7.7,2.6,6.9,2.3],[5.7,2.6,3.5,1.0],[5.9,3.0,5.1,1.8],[6.8,3.2,5.9,2.3],[5.0,3.6,1.4,0.2]]}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"margin":{"l":100.0},"title":{"text":"raw iris data"}};
            var config = {"responsive":true};
            Plotly.newPlot('df4aceff-3e57-41fb-8a17-84d6d51d379d', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_df4aceff3e5741fb8a1784d6d51d379d();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_df4aceff3e5741fb8a1784d6d51d379d();
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
"3, "versicolor_0", [|5.5; 2.4; 3.8; 1.1|]
2, "setosa_1", [|4.9; 3.1; 1.5; 0.1|]
1, "virginica_2", [|7.6; 3.0; 6.6; 2.1|]
3, "virginica_3", [|5.6; 2.8; 4.9; 2.0|]
3, "virginica_4", [|6.1; 3.0; 4.9; 1.8|]
1, "virginica_5", [|6.3; 3.4; 5.6; 2.4|]
3, "virginica_6", [|6.2; 2.8; 4.8; 1.8|]
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
        Chart.Heatmap(d,colNames=colNames,rowNames=labels)
        // required to fit the species identifier on the left side of the heatmap
        |> Chart.withMarginSize(Left=100.)
        |> Chart.withTitle "clustered iris data (k-means clustering)"(* output: 
<div id="e2b599fe-4d66-47ab-ad9b-e6e48b0b41c2"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_e2b599fe4d6647abad9be6e48b0b41c2 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"heatmap","x":["sepal_length","sepal_width","petal_length","petal_width"],"y":["virginica_2","virginica_5","virginica_7","virginica_8","virginica_14","virginica_19","virginica_22","virginica_29","virginica_31","virginica_43","virginica_46","virginica_48","virginica_50","versicolor_52","virginica_53","virginica_59","virginica_62","virginica_64","virginica_72","virginica_73","versicolor_77","virginica_81","virginica_96","virginica_101","virginica_104","virginica_108","virginica_111","virginica_112","virginica_113","virginica_123","virginica_127","virginica_130","virginica_132","virginica_135","virginica_137","virginica_141","virginica_145","virginica_148","setosa_1","setosa_9","setosa_18","setosa_21","setosa_23","setosa_28","setosa_32","setosa_35","setosa_47","setosa_49","setosa_51","setosa_55","setosa_60","setosa_61","setosa_63","setosa_65","setosa_75","setosa_83","setosa_84","setosa_85","setosa_87","setosa_97","setosa_102","setosa_103","setosa_106","setosa_117","setosa_119","setosa_121","setosa_124","setosa_126","setosa_131","setosa_144","setosa_149","versicolor_0","virginica_3","virginica_4","virginica_6","versicolor_11","versicolor_12","virginica_15","versicolor_20","virginica_24","virginica_25","versicolor_26","versicolor_27","virginica_30","versicolor_34","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","versicolor_44","versicolor_45","virginica_54","versicolor_56","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","versicolor_76","versicolor_78","versicolor_79","versicolor_80","versicolor_82","versicolor_86","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","versicolor_98","versicolor_100","versicolor_105","versicolor_107","versicolor_109","versicolor_110","versicolor_114","virginica_115","virginica_116","versicolor_118","versicolor_120","versicolor_128","virginica_129","versicolor_133","versicolor_136","versicolor_138","versicolor_139","versicolor_140","versicolor_142","virginica_143","versicolor_146","virginica_147","setosa_10","setosa_13","setosa_16","setosa_17","setosa_33","setosa_36","setosa_42","setosa_57","setosa_58","setosa_71","setosa_74","setosa_94","setosa_95","setosa_99","setosa_122","setosa_125","setosa_134"],"z":[[7.6,3.0,6.6,2.1],[6.3,3.4,5.6,2.4],[7.2,3.2,6.0,1.8],[6.9,3.2,5.7,2.3],[7.2,3.6,6.1,2.5],[6.5,3.0,5.8,2.2],[6.7,3.3,5.7,2.5],[7.7,3.0,6.1,2.3],[6.4,3.1,5.5,1.8],[6.7,3.0,5.2,2.3],[7.7,2.8,6.7,2.0],[6.2,3.4,5.4,2.3],[6.7,2.5,5.8,1.8],[6.7,3.0,5.0,1.7],[6.9,3.1,5.1,2.3],[6.7,3.3,5.7,2.1],[6.9,3.1,5.4,2.1],[6.5,3.2,5.1,2.0],[7.9,3.8,6.4,2.0],[7.2,3.0,5.8,1.6],[6.9,3.1,4.9,1.5],[7.4,2.8,6.1,1.9],[7.3,2.9,6.3,1.8],[6.3,2.9,5.6,1.8],[6.5,3.0,5.5,1.8],[6.4,2.8,5.6,2.1],[6.7,3.1,5.6,2.4],[6.4,3.2,5.3,2.3],[6.8,3.0,5.5,2.1],[6.4,2.8,5.6,2.2],[6.5,3.0,5.2,2.0],[7.1,3.0,5.9,2.1],[6.1,2.6,5.6,1.4],[7.7,3.8,6.7,2.2],[6.4,2.7,5.3,1.9],[6.3,3.3,6.0,2.5],[7.7,2.6,6.9,2.3],[6.8,3.2,5.9,2.3],[4.9,3.1,1.5,0.1],[4.9,3.0,1.4,0.2],[5.0,3.2,1.2,0.2],[4.3,3.0,1.1,0.1],[4.5,2.3,1.3,0.3],[4.9,3.1,1.5,0.1],[4.7,3.2,1.6,0.2],[5.0,3.4,1.6,0.4],[4.8,3.0,1.4,0.1],[4.8,3.4,1.9,0.2],[5.1,3.5,1.4,0.2],[4.8,3.1,1.6,0.2],[4.4,3.0,1.3,0.2],[5.0,3.5,1.3,0.3],[5.0,3.5,1.6,0.6],[4.6,3.6,1.0,0.2],[4.6,3.2,1.4,0.2],[5.1,3.4,1.5,0.2],[4.8,3.4,1.6,0.2],[5.0,3.0,1.6,0.2],[4.6,3.1,1.5,0.2],[4.7,3.2,1.3,0.2],[4.4,3.2,1.3,0.2],[4.4,2.9,1.4,0.2],[4.9,3.1,1.5,0.1],[5.2,3.4,1.4,0.2],[5.1,3.5,1.4,0.3],[4.8,3.0,1.4,0.3],[5.0,3.4,1.5,0.2],[5.1,3.3,1.7,0.5],[4.6,3.4,1.4,0.3],[5.0,3.3,1.4,0.2],[5.0,3.6,1.4,0.2],[5.5,2.4,3.8,1.1],[5.6,2.8,4.9,2.0],[6.1,3.0,4.9,1.8],[6.2,2.8,4.8,1.8],[7.0,3.2,4.7,1.4],[6.1,3.0,4.6,1.4],[6.3,2.8,5.1,1.5],[5.6,3.0,4.1,1.3],[5.7,2.5,5.0,2.0],[6.3,2.7,4.9,1.8],[5.6,3.0,4.5,1.5],[5.6,2.5,3.9,1.1],[6.3,2.5,5.0,1.9],[5.9,3.2,4.8,1.8],[6.0,2.7,5.1,1.6],[5.6,2.9,3.6,1.3],[6.3,3.3,4.7,1.6],[5.1,2.5,3.0,1.1],[6.4,3.2,4.5,1.5],[6.2,2.9,4.3,1.3],[6.8,2.8,4.8,1.4],[5.8,2.7,5.1,1.9],[5.5,2.6,4.4,1.2],[6.1,2.8,4.7,1.2],[5.0,2.0,3.5,1.0],[5.8,2.7,4.1,1.0],[6.0,3.4,4.5,1.6],[4.9,2.5,4.5,1.7],[6.4,2.9,4.3,1.3],[6.0,2.9,4.5,1.5],[5.8,2.6,4.0,1.2],[5.2,2.7,3.9,1.4],[5.5,2.4,3.7,1.0],[5.7,2.8,4.1,1.3],[5.9,3.0,4.2,1.5],[6.1,2.8,4.0,1.3],[5.7,2.8,4.5,1.3],[6.0,2.2,4.0,1.0],[5.4,3.0,4.5,1.5],[6.1,2.9,4.7,1.4],[6.3,2.5,4.9,1.5],[6.6,2.9,4.6,1.3],[6.3,2.3,4.4,1.3],[6.6,3.0,4.4,1.4],[6.5,2.8,4.6,1.5],[5.5,2.3,4.0,1.3],[5.7,2.9,4.2,1.3],[5.8,2.7,5.1,1.9],[6.0,3.0,4.8,1.8],[6.7,3.1,4.7,1.5],[5.5,2.5,4.0,1.3],[5.6,2.7,4.2,1.3],[6.0,2.2,5.0,1.5],[6.2,2.2,4.5,1.5],[5.7,3.0,4.2,1.2],[5.8,2.7,3.9,1.2],[5.0,2.3,3.3,1.0],[6.7,3.1,4.4,1.4],[4.9,2.4,3.3,1.0],[5.8,2.8,5.1,2.4],[5.7,2.6,3.5,1.0],[5.9,3.0,5.1,1.8],[5.4,3.9,1.7,0.4],[5.4,3.7,1.5,0.2],[5.4,3.4,1.7,0.2],[5.8,4.0,1.2,0.2],[5.5,4.2,1.4,0.2],[5.4,3.4,1.5,0.4],[5.1,3.7,1.5,0.4],[5.1,3.8,1.6,0.2],[5.7,4.4,1.5,0.4],[5.7,3.8,1.7,0.3],[5.5,3.5,1.3,0.2],[5.4,3.9,1.3,0.4],[5.2,4.1,1.5,0.1],[5.3,3.7,1.5,0.2],[5.1,3.8,1.5,0.3],[5.1,3.8,1.9,0.4],[5.2,3.5,1.5,0.2]]}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"margin":{"l":100.0},"title":{"text":"clustered iris data (k-means clustering)"}};
            var config = {"responsive":true};
            Plotly.newPlot('e2b599fe-4d66-47ab-ad9b-e6e48b0b41c2', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_e2b599fe4d6647abad9be6e48b0b41c2();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_e2b599fe4d6647abad9be6e48b0b41c2();
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
            Chart.Scatter3D(truncData,mode=StyleParam.Mode.Markers,Name = clusterName,MultiText=label)
        )
    |> Chart.combine
    |> Chart.withTitle "isolated coordinates of clustered iris data (k-means clustering)"
    |> Chart.withXAxisStyle colNames.[0]
    |> Chart.withYAxisStyle colNames.[2]
    |> Chart.withZAxisStyle colNames.[3](* output: 
<div id="76d181ce-2c6a-4f1f-9cc1-d48c1870df72"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_76d181ce2c6a4f1f9cc1d48c1870df72 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter3d","name":"cluster 3","mode":"markers","x":[5.5,5.6,6.1,6.2,7.0,6.1,6.3,5.6,5.7,6.3,5.6,5.6,6.3,5.9,6.0,5.6,6.3,5.1,6.4,6.2,6.8,5.8,5.5,6.1,5.0,5.8,6.0,4.9,6.4,6.0,5.8,5.2,5.5,5.7,5.9,6.1,5.7,6.0,5.4,6.1,6.3,6.6,6.3,6.6,6.5,5.5,5.7,5.8,6.0,6.7,5.5,5.6,6.0,6.2,5.7,5.8,5.0,6.7,4.9,5.8,5.7,5.9],"y":[3.8,4.9,4.9,4.8,4.7,4.6,5.1,4.1,5.0,4.9,4.5,3.9,5.0,4.8,5.1,3.6,4.7,3.0,4.5,4.3,4.8,5.1,4.4,4.7,3.5,4.1,4.5,4.5,4.3,4.5,4.0,3.9,3.7,4.1,4.2,4.0,4.5,4.0,4.5,4.7,4.9,4.6,4.4,4.4,4.6,4.0,4.2,5.1,4.8,4.7,4.0,4.2,5.0,4.5,4.2,3.9,3.3,4.4,3.3,5.1,3.5,5.1],"z":[1.1,2.0,1.8,1.8,1.4,1.4,1.5,1.3,2.0,1.8,1.5,1.1,1.9,1.8,1.6,1.3,1.6,1.1,1.5,1.3,1.4,1.9,1.2,1.2,1.0,1.0,1.6,1.7,1.3,1.5,1.2,1.4,1.0,1.3,1.5,1.3,1.3,1.0,1.5,1.4,1.5,1.3,1.3,1.4,1.5,1.3,1.3,1.9,1.8,1.5,1.3,1.3,1.5,1.5,1.2,1.2,1.0,1.4,1.0,2.4,1.0,1.8],"text":["versicolor_0","virginica_3","virginica_4","virginica_6","versicolor_11","versicolor_12","virginica_15","versicolor_20","virginica_24","virginica_25","versicolor_26","versicolor_27","virginica_30","versicolor_34","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","versicolor_44","versicolor_45","virginica_54","versicolor_56","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","versicolor_76","versicolor_78","versicolor_79","versicolor_80","versicolor_82","versicolor_86","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","versicolor_98","versicolor_100","versicolor_105","versicolor_107","versicolor_109","versicolor_110","versicolor_114","virginica_115","virginica_116","versicolor_118","versicolor_120","versicolor_128","virginica_129","versicolor_133","versicolor_136","versicolor_138","versicolor_139","versicolor_140","versicolor_142","virginica_143","versicolor_146","virginica_147"],"marker":{},"line":{}},{"type":"scatter3d","name":"cluster 2","mode":"markers","x":[4.9,4.9,5.0,4.3,4.5,4.9,4.7,5.0,4.8,4.8,5.1,4.8,4.4,5.0,5.0,4.6,4.6,5.1,4.8,5.0,4.6,4.7,4.4,4.4,4.9,5.2,5.1,4.8,5.0,5.1,4.6,5.0,5.0],"y":[1.5,1.4,1.2,1.1,1.3,1.5,1.6,1.6,1.4,1.9,1.4,1.6,1.3,1.3,1.6,1.0,1.4,1.5,1.6,1.6,1.5,1.3,1.3,1.4,1.5,1.4,1.4,1.4,1.5,1.7,1.4,1.4,1.4],"z":[0.1,0.2,0.2,0.1,0.3,0.1,0.2,0.4,0.1,0.2,0.2,0.2,0.2,0.3,0.6,0.2,0.2,0.2,0.2,0.2,0.2,0.2,0.2,0.2,0.1,0.2,0.3,0.3,0.2,0.5,0.3,0.2,0.2],"text":["setosa_1","setosa_9","setosa_18","setosa_21","setosa_23","setosa_28","setosa_32","setosa_35","setosa_47","setosa_49","setosa_51","setosa_55","setosa_60","setosa_61","setosa_63","setosa_65","setosa_75","setosa_83","setosa_84","setosa_85","setosa_87","setosa_97","setosa_102","setosa_103","setosa_106","setosa_117","setosa_119","setosa_121","setosa_124","setosa_126","setosa_131","setosa_144","setosa_149"],"marker":{},"line":{}},{"type":"scatter3d","name":"cluster 1","mode":"markers","x":[7.6,6.3,7.2,6.9,7.2,6.5,6.7,7.7,6.4,6.7,7.7,6.2,6.7,6.7,6.9,6.7,6.9,6.5,7.9,7.2,6.9,7.4,7.3,6.3,6.5,6.4,6.7,6.4,6.8,6.4,6.5,7.1,6.1,7.7,6.4,6.3,7.7,6.8],"y":[6.6,5.6,6.0,5.7,6.1,5.8,5.7,6.1,5.5,5.2,6.7,5.4,5.8,5.0,5.1,5.7,5.4,5.1,6.4,5.8,4.9,6.1,6.3,5.6,5.5,5.6,5.6,5.3,5.5,5.6,5.2,5.9,5.6,6.7,5.3,6.0,6.9,5.9],"z":[2.1,2.4,1.8,2.3,2.5,2.2,2.5,2.3,1.8,2.3,2.0,2.3,1.8,1.7,2.3,2.1,2.1,2.0,2.0,1.6,1.5,1.9,1.8,1.8,1.8,2.1,2.4,2.3,2.1,2.2,2.0,2.1,1.4,2.2,1.9,2.5,2.3,2.3],"text":["virginica_2","virginica_5","virginica_7","virginica_8","virginica_14","virginica_19","virginica_22","virginica_29","virginica_31","virginica_43","virginica_46","virginica_48","virginica_50","versicolor_52","virginica_53","virginica_59","virginica_62","virginica_64","virginica_72","virginica_73","versicolor_77","virginica_81","virginica_96","virginica_101","virginica_104","virginica_108","virginica_111","virginica_112","virginica_113","virginica_123","virginica_127","virginica_130","virginica_132","virginica_135","virginica_137","virginica_141","virginica_145","virginica_148"],"marker":{},"line":{}},{"type":"scatter3d","name":"cluster 4","mode":"markers","x":[5.4,5.4,5.4,5.8,5.5,5.4,5.1,5.1,5.7,5.7,5.5,5.4,5.2,5.3,5.1,5.1,5.2],"y":[1.7,1.5,1.7,1.2,1.4,1.5,1.5,1.6,1.5,1.7,1.3,1.3,1.5,1.5,1.5,1.9,1.5],"z":[0.4,0.2,0.2,0.2,0.2,0.4,0.4,0.2,0.4,0.3,0.2,0.4,0.1,0.2,0.3,0.4,0.2],"text":["setosa_10","setosa_13","setosa_16","setosa_17","setosa_33","setosa_36","setosa_42","setosa_57","setosa_58","setosa_71","setosa_74","setosa_94","setosa_95","setosa_99","setosa_122","setosa_125","setosa_134"],"marker":{},"line":{}}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"title":{"text":"isolated coordinates of clustered iris data (k-means clustering)"},"xaxis":{"title":{"text":"sepal_length"}},"yaxis":{"title":{"text":"petal_length"}},"scene":{"zaxis":{"title":{"text":"petal_width"}}}};
            var config = {"responsive":true};
            Plotly.newPlot('76d181ce-2c6a-4f1f-9cc1-d48c1870df72', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_76d181ce2c6a4f1f9cc1d48c1870df72();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_76d181ce2c6a4f1f9cc1d48c1870df72();
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
        |> Chart.withXAxisStyle "k"
        |> Chart.withYAxisStyle "average dispersion"
        |> Chart.withTitle "iris data set average dispersion per k"(* output: 
<div id="309574c2-a0c6-4ae8-9259-804460f52c7d"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_309574c2a0c64ae89259804460f52c7d = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"lines","x":[2,3,4,5,6,7,8,9,10],"y":[2.6540379473960134,1.155001855693569,0.3912542971136334,0.2880438545938474,0.14941715486146384,0.1297071835429055,0.10556741844616122,0.09887700688288817,0.06775592067276158],"marker":{},"line":{},"error_y":{"array":[0.0,0.9506728537524636,0.12742024993820755,0.1515976434046401,0.03208299546966179,0.03062180718855102,0.042317631746049106,0.0244244060348454,0.018955502680995658]}}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"xaxis":{"title":{"text":"k"}},"yaxis":{"title":{"text":"average dispersion"}},"title":{"text":"iris data set average dispersion per k"}};
            var config = {"responsive":true};
            Plotly.newPlot('309574c2-a0c6-4ae8-9259-804460f52c7d', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_309574c2a0c64ae89259804460f52c7d();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_309574c2a0c64ae89259804460f52c7d();
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

