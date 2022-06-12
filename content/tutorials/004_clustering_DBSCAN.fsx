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
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.12"
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
    Chart.Scatter(unzippedData,mode=StyleParam.Mode.Markers,MultiText=labels)
    |> Chart.withXAxisStyle header2D.[0]
    |> Chart.withYAxisStyle header2D.[1]
    |> Chart.withTitle "rawChart2D"

let rawChart3D =
    let unzippedData =
        data3D
        |> Array.map (fun x -> x.[0],x.[1],x.[2])
    Chart.Scatter3D(unzippedData,mode=StyleParam.Mode.Markers,MultiText=labels)
    |> Chart.withXAxisStyle header3D.[0]
    |> Chart.withYAxisStyle header3D.[1]
    |> Chart.withZAxisStyle header3D.[2]
    |> Chart.withTitle "rawChart3D"(* output: 
<div id="6a00c162-64fb-4e82-b3d1-51004cece8d0"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_6a00c16264fb4e82b3d151004cece8d0 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"markers","x":[3.8,1.5,6.6,4.9,4.9,5.6,4.8,6.0,5.7,1.4,1.7,4.7,4.6,1.5,6.1,5.1,1.7,1.2,1.2,5.8,4.1,1.1,5.7,1.3,5.0,4.9,4.5,3.9,1.5,6.1,5.0,5.5,1.6,1.4,4.8,1.6,1.5,5.1,3.6,4.7,3.0,4.5,1.5,5.2,4.3,4.8,6.7,1.4,5.4,1.9,5.8,1.4,5.0,5.1,5.1,1.6,4.4,1.6,1.5,5.7,1.3,1.3,5.4,1.6,5.1,1.0,4.7,3.5,4.1,4.5,4.5,1.7,6.4,5.8,1.3,1.4,4.3,4.9,4.5,4.0,3.9,6.1,3.7,1.5,1.6,1.6,4.1,1.5,4.2,4.0,4.5,4.0,4.5,4.7,1.3,1.5,6.3,1.3,4.9,1.5,4.6,5.6,1.3,1.4,5.5,4.4,1.5,4.4,5.6,4.6,4.0,5.6,5.3,5.5,4.2,5.1,4.8,1.4,4.7,1.4,4.0,1.4,1.5,5.6,1.5,1.9,1.7,5.2,4.2,5.0,5.9,1.4,5.6,4.5,1.5,6.7,4.2,5.3,3.9,3.3,4.4,6.0,3.3,5.1,1.4,6.9,3.5,5.1,5.9,1.4],"y":[1.1,0.1,2.1,2.0,1.8,2.4,1.8,1.8,2.3,0.2,0.4,1.4,1.4,0.2,2.5,1.5,0.2,0.2,0.2,2.2,1.3,0.1,2.5,0.3,2.0,1.8,1.5,1.1,0.1,2.3,1.9,1.8,0.2,0.2,1.8,0.4,0.4,1.6,1.3,1.6,1.1,1.5,0.4,2.3,1.3,1.4,2.0,0.1,2.3,0.2,1.8,0.2,1.7,2.3,1.9,0.2,1.2,0.2,0.4,2.1,0.2,0.3,2.1,0.6,2.0,0.2,1.2,1.0,1.0,1.6,1.7,0.3,2.0,1.6,0.2,0.2,1.3,1.5,1.5,1.2,1.4,1.9,1.0,0.2,0.2,0.2,1.3,0.2,1.5,1.3,1.3,1.0,1.5,1.4,0.4,0.1,1.8,0.2,1.5,0.2,1.3,1.8,0.2,0.2,1.8,1.3,0.1,1.4,2.1,1.5,1.3,2.4,2.3,2.1,1.3,1.9,1.8,0.2,1.5,0.3,1.3,0.3,0.3,2.2,0.2,0.4,0.5,2.0,1.3,1.5,2.1,0.3,1.4,1.5,0.2,2.2,1.2,1.9,1.2,1.0,1.4,2.5,1.0,2.4,0.2,2.3,1.0,1.8,2.3,0.2],"text":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"],"marker":{},"line":{}}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"xaxis":{"title":{"text":"petal_length"}},"yaxis":{"title":{"text":"petal_width"}},"title":{"text":"rawChart2D"}};
            var config = {"responsive":true};
            Plotly.newPlot('6a00c162-64fb-4e82-b3d1-51004cece8d0', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_6a00c16264fb4e82b3d151004cece8d0();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_6a00c16264fb4e82b3d151004cece8d0();
            }
</script>
*)
(**
<br>

<div id="fadd55b0-fdfd-41a5-a540-b72cb24a5ce9"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_fadd55b0fdfd41a5a540b72cb24a5ce9 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter3d","mode":"markers","x":[5.5,4.9,7.6,5.6,6.1,6.3,6.2,7.2,6.9,4.9,5.4,7.0,6.1,5.4,7.2,6.3,5.4,5.8,5.0,6.5,5.6,4.3,6.7,4.5,5.7,6.3,5.6,5.6,4.9,7.7,6.3,6.4,4.7,5.5,5.9,5.0,5.4,6.0,5.6,6.3,5.1,6.4,5.1,6.7,6.2,6.8,7.7,4.8,6.2,4.8,6.7,5.1,6.7,6.9,5.8,4.8,5.5,5.1,5.7,6.7,4.4,5.0,6.9,5.0,6.5,4.6,6.1,5.0,5.8,6.0,4.9,5.7,7.9,7.2,5.5,4.6,6.4,6.9,6.0,5.8,5.2,7.4,5.5,5.1,4.8,5.0,5.7,4.6,5.9,6.1,5.7,6.0,5.4,6.1,5.4,5.2,7.3,4.7,6.3,5.3,6.6,6.3,4.4,4.4,6.5,6.3,4.9,6.6,6.4,6.5,5.5,6.7,6.4,6.8,5.7,5.8,6.0,5.2,6.7,5.1,5.5,4.8,5.1,6.4,5.0,5.1,5.1,6.5,5.6,6.0,7.1,4.6,6.1,6.2,5.2,7.7,5.7,6.4,5.8,5.0,6.7,6.3,4.9,5.8,5.0,7.7,5.7,5.9,6.8,5.0],"y":[3.8,1.5,6.6,4.9,4.9,5.6,4.8,6.0,5.7,1.4,1.7,4.7,4.6,1.5,6.1,5.1,1.7,1.2,1.2,5.8,4.1,1.1,5.7,1.3,5.0,4.9,4.5,3.9,1.5,6.1,5.0,5.5,1.6,1.4,4.8,1.6,1.5,5.1,3.6,4.7,3.0,4.5,1.5,5.2,4.3,4.8,6.7,1.4,5.4,1.9,5.8,1.4,5.0,5.1,5.1,1.6,4.4,1.6,1.5,5.7,1.3,1.3,5.4,1.6,5.1,1.0,4.7,3.5,4.1,4.5,4.5,1.7,6.4,5.8,1.3,1.4,4.3,4.9,4.5,4.0,3.9,6.1,3.7,1.5,1.6,1.6,4.1,1.5,4.2,4.0,4.5,4.0,4.5,4.7,1.3,1.5,6.3,1.3,4.9,1.5,4.6,5.6,1.3,1.4,5.5,4.4,1.5,4.4,5.6,4.6,4.0,5.6,5.3,5.5,4.2,5.1,4.8,1.4,4.7,1.4,4.0,1.4,1.5,5.6,1.5,1.9,1.7,5.2,4.2,5.0,5.9,1.4,5.6,4.5,1.5,6.7,4.2,5.3,3.9,3.3,4.4,6.0,3.3,5.1,1.4,6.9,3.5,5.1,5.9,1.4],"z":[1.1,0.1,2.1,2.0,1.8,2.4,1.8,1.8,2.3,0.2,0.4,1.4,1.4,0.2,2.5,1.5,0.2,0.2,0.2,2.2,1.3,0.1,2.5,0.3,2.0,1.8,1.5,1.1,0.1,2.3,1.9,1.8,0.2,0.2,1.8,0.4,0.4,1.6,1.3,1.6,1.1,1.5,0.4,2.3,1.3,1.4,2.0,0.1,2.3,0.2,1.8,0.2,1.7,2.3,1.9,0.2,1.2,0.2,0.4,2.1,0.2,0.3,2.1,0.6,2.0,0.2,1.2,1.0,1.0,1.6,1.7,0.3,2.0,1.6,0.2,0.2,1.3,1.5,1.5,1.2,1.4,1.9,1.0,0.2,0.2,0.2,1.3,0.2,1.5,1.3,1.3,1.0,1.5,1.4,0.4,0.1,1.8,0.2,1.5,0.2,1.3,1.8,0.2,0.2,1.8,1.3,0.1,1.4,2.1,1.5,1.3,2.4,2.3,2.1,1.3,1.9,1.8,0.2,1.5,0.3,1.3,0.3,0.3,2.2,0.2,0.4,0.5,2.0,1.3,1.5,2.1,0.3,1.4,1.5,0.2,2.2,1.2,1.9,1.2,1.0,1.4,2.5,1.0,2.4,0.2,2.3,1.0,1.8,2.3,0.2],"text":["versicolor_0","setosa_1","virginica_2","virginica_3","virginica_4","virginica_5","virginica_6","virginica_7","virginica_8","setosa_9","setosa_10","versicolor_11","versicolor_12","setosa_13","virginica_14","virginica_15","setosa_16","setosa_17","setosa_18","virginica_19","versicolor_20","setosa_21","virginica_22","setosa_23","virginica_24","virginica_25","versicolor_26","versicolor_27","setosa_28","virginica_29","virginica_30","virginica_31","setosa_32","setosa_33","versicolor_34","setosa_35","setosa_36","versicolor_37","versicolor_38","versicolor_39","versicolor_40","versicolor_41","setosa_42","virginica_43","versicolor_44","versicolor_45","virginica_46","setosa_47","virginica_48","setosa_49","virginica_50","setosa_51","versicolor_52","virginica_53","virginica_54","setosa_55","versicolor_56","setosa_57","setosa_58","virginica_59","setosa_60","setosa_61","virginica_62","setosa_63","virginica_64","setosa_65","versicolor_66","versicolor_67","versicolor_68","versicolor_69","virginica_70","setosa_71","virginica_72","virginica_73","setosa_74","setosa_75","versicolor_76","versicolor_77","versicolor_78","versicolor_79","versicolor_80","virginica_81","versicolor_82","setosa_83","setosa_84","setosa_85","versicolor_86","setosa_87","versicolor_88","versicolor_89","versicolor_90","versicolor_91","versicolor_92","versicolor_93","setosa_94","setosa_95","virginica_96","setosa_97","versicolor_98","setosa_99","versicolor_100","virginica_101","setosa_102","setosa_103","virginica_104","versicolor_105","setosa_106","versicolor_107","virginica_108","versicolor_109","versicolor_110","virginica_111","virginica_112","virginica_113","versicolor_114","virginica_115","virginica_116","setosa_117","versicolor_118","setosa_119","versicolor_120","setosa_121","setosa_122","virginica_123","setosa_124","setosa_125","setosa_126","virginica_127","versicolor_128","virginica_129","virginica_130","setosa_131","virginica_132","versicolor_133","setosa_134","virginica_135","versicolor_136","virginica_137","versicolor_138","versicolor_139","versicolor_140","virginica_141","versicolor_142","virginica_143","setosa_144","virginica_145","versicolor_146","virginica_147","virginica_148","setosa_149"],"marker":{},"line":{}}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"xaxis":{"title":{"text":"sepal_length"}},"yaxis":{"title":{"text":"petal_length"}},"scene":{"zaxis":{"title":{"text":"petal_width"}}},"title":{"text":"rawChart3D"}};
            var config = {"responsive":true};
            Plotly.newPlot('fadd55b0-fdfd-41a5-a540-b72cb24a5ce9', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_fadd55b0fdfd41a5a540b72cb24a5ce9();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_fadd55b0fdfd41a5a540b72cb24a5ce9();
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
    |> Chart.withYAxisStyle header2D.[1](* output: 
<div id="b35d2f51-ed75-45af-85d9-44e571b8dffc"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_b35d2f51ed7545af85d944e571b8dffc = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"markers","x":[6.6,3.0,6.7,6.4,6.7,3.3,6.9],"y":[2.1,1.1,2.0,2.0,2.2,1.0,2.3],"marker":{},"line":{},"name":"Noise"},{"type":"scatter","mode":"markers","x":[1.5,1.4,1.7,1.5,1.7,1.2,1.1,1.3,1.6,1.6,1.5,1.4,1.9,1.3,1.7,1.3,1.4,1.5,1.9,1.7,1.6,1.0],"y":[0.1,0.2,0.4,0.2,0.2,0.2,0.1,0.3,0.2,0.4,0.4,0.1,0.2,0.2,0.3,0.4,0.3,0.3,0.4,0.5,0.6,0.2],"marker":{},"line":{},"name":"Cluster 0"},{"type":"scatter","mode":"markers","x":[4.9,4.9,4.8,5.0,5.0,5.1,4.7,5.2,5.0,5.1,5.1,5.1,4.9,5.3,5.2,5.3,5.1,5.1,4.7,5.1,4.8,4.5,4.5,4.6,4.7,5.0,4.6,4.5,5.4,5.5,4.4,4.7,4.5,4.6,4.4,4.4,5.6,5.7,5.4,5.6,5.5,5.6,5.7,5.7,5.6,4.3,4.2,4.2,4.2,4.1,5.8,5.8,5.9,6.0,5.8,5.6,4.1,4.0,4.0,4.0,5.9,6.1,6.1,6.0,3.9,3.9,3.9,3.8,3.6,3.7,6.1,6.3,3.5],"y":[2.0,1.8,1.8,2.0,1.9,1.6,1.6,2.3,1.7,2.3,1.9,2.0,1.5,2.3,2.0,1.9,2.4,1.8,1.4,1.5,1.4,1.6,1.7,1.5,1.5,1.5,1.4,1.5,2.1,1.8,1.2,1.2,1.3,1.3,1.3,1.4,2.4,2.3,2.3,2.1,2.1,2.2,2.5,2.1,1.8,1.3,1.5,1.3,1.2,1.3,2.2,1.8,2.1,1.8,1.6,1.4,1.0,1.2,1.3,1.0,2.3,2.3,1.9,2.5,1.1,1.4,1.2,1.1,1.3,1.0,2.5,1.8,1.0],"marker":{},"line":{},"name":"Cluster 1"}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"title":{"text":"eps: 0.5 minPts: 20 pts: 150 cluster: 2 noisePts: 8"},"xaxis":{"title":{"text":"petal_length"}},"yaxis":{"title":{"text":"petal_width"}}};
            var config = {"responsive":true};
            Plotly.newPlot('b35d2f51-ed75-45af-85d9-44e571b8dffc', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_b35d2f51ed7545af85d944e571b8dffc();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_b35d2f51ed7545af85d944e571b8dffc();
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
        |> fun x -> Chart.Scatter3D (x,StyleParam.Mode.Markers)
        |> Chart.withTraceName (sprintf "Cluster_%i" i))
    |> Chart.combine

let chartNoise3D =
    result3D.Noisepoints
    |> Seq.map (fun x -> x.[0],x.[1],x.[2])  
    |> Seq.distinct //faster visualization; no difference in plot but in point number
    |> fun x -> Chart.Scatter3D (x,StyleParam.Mode.Markers)
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
let clusteredChart3D() = DbScan.compute DistanceMetrics.Array.euclideanNaNSquared 20 (0.7**2.) data3D (* output: 
<div id="3f02a07c-ee3f-44f7-97b7-0a33f84a6e6c"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_3f02a07cee3f44f797b70a33f84a6e6c = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.6.3.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter3d","mode":"markers","x":[7.6,7.2,7.7,5.1,7.7,4.9,7.9,7.2,7.4,7.3,7.7,5.0,4.9,7.7],"y":[6.6,6.1,6.1,3.0,6.7,4.5,6.4,5.8,6.1,6.3,6.7,3.3,3.3,6.9],"z":[2.1,2.5,2.3,1.1,2.0,1.7,2.0,1.6,1.9,1.8,2.2,1.0,1.0,2.3],"marker":{},"line":{},"name":"Noise"},{"type":"scatter3d","mode":"markers","x":[5.5,5.6,5.6,5.6,5.5,5.0,5.8,5.8,5.2,5.5,5.7,5.9,6.1,6.0,5.5,5.7,5.6,5.7,5.8,5.7,5.6,6.2,6.0,6.0,5.7,5.4,6.1,6.1,6.1,6.3,6.4,6.2,5.9,6.3,6.4,6.0,6.6,5.6,6.0,6.3,6.6,6.5,6.7,6.7,6.1,6.2,6.3,6.3,6.3,6.0,5.8,5.9,6.8,5.7,6.5,5.8,6.7,6.9,6.5,6.4,7.0,6.1,6.4,6.5,6.3,6.4,6.7,6.2,6.4,6.4,6.3,6.9,6.7,6.9,6.7,6.8,6.5,6.7,6.9,6.7,6.3,6.8,7.2,7.1],"y":[3.8,4.1,3.9,3.6,4.4,3.5,4.1,4.0,3.9,3.7,4.1,4.2,4.0,4.0,4.0,4.2,4.2,4.2,3.9,3.5,4.5,4.3,4.5,4.5,4.5,4.5,4.6,4.7,4.7,4.4,4.3,4.5,4.8,4.7,4.5,4.8,4.4,4.9,5.0,4.9,4.6,4.6,4.7,4.4,4.9,4.8,5.1,4.9,5.0,5.1,5.1,5.1,4.8,5.0,5.1,5.1,5.0,4.9,5.2,5.3,4.7,5.6,5.5,5.5,5.6,5.3,5.2,5.4,5.6,5.6,5.6,5.1,5.7,5.4,5.6,5.5,5.8,5.8,5.7,5.7,6.0,5.9,6.0,5.9],"z":[1.1,1.3,1.1,1.3,1.2,1.0,1.0,1.2,1.4,1.0,1.3,1.5,1.3,1.0,1.3,1.3,1.3,1.2,1.2,1.0,1.5,1.3,1.6,1.5,1.3,1.5,1.4,1.2,1.4,1.3,1.3,1.5,1.8,1.6,1.5,1.8,1.4,2.0,1.5,1.5,1.3,1.5,1.5,1.4,1.8,1.8,1.5,1.8,1.9,1.6,1.9,1.8,1.4,2.0,2.0,2.4,1.7,1.5,2.0,1.9,1.4,1.4,1.8,1.8,1.8,2.3,2.3,2.3,2.1,2.2,2.4,2.3,2.1,2.1,2.4,2.1,2.2,1.8,2.3,2.5,2.5,2.3,1.8,2.1],"marker":{},"line":{},"name":"Cluster_0"},{"type":"scatter3d","mode":"markers","x":[4.9,4.9,5.4,5.4,5.4,5.0,4.5,4.7,5.5,5.0,5.4,5.1,4.8,4.8,5.1,4.8,5.1,4.4,5.0,5.0,4.6,5.5,4.6,5.1,5.0,4.6,5.4,5.2,4.7,5.3,4.4,5.2,5.1,4.8,5.1,5.0,5.1,5.1,4.6,5.2,5.0,4.3,5.8,5.7,5.7],"y":[1.5,1.4,1.7,1.5,1.7,1.2,1.3,1.6,1.4,1.6,1.5,1.5,1.4,1.9,1.4,1.6,1.6,1.3,1.3,1.6,1.0,1.3,1.4,1.5,1.6,1.5,1.3,1.5,1.3,1.5,1.4,1.4,1.4,1.4,1.5,1.5,1.9,1.7,1.4,1.5,1.4,1.1,1.2,1.5,1.7],"z":[0.1,0.2,0.4,0.2,0.2,0.2,0.3,0.2,0.2,0.4,0.4,0.4,0.1,0.2,0.2,0.2,0.2,0.2,0.3,0.6,0.2,0.2,0.2,0.2,0.2,0.2,0.4,0.1,0.2,0.2,0.2,0.2,0.3,0.3,0.3,0.2,0.4,0.5,0.3,0.2,0.2,0.1,0.2,0.4,0.3],"marker":{},"line":{},"name":"Cluster_1"}];
            var layout = {"width":600,"height":600,"template":{"layout":{"title":{"x":0.05},"font":{"color":"rgba(42, 63, 95, 1.0)"},"paper_bgcolor":"rgba(255, 255, 255, 1.0)","plot_bgcolor":"rgba(229, 236, 246, 1.0)","autotypenumbers":"strict","colorscale":{"diverging":[[0.0,"#8e0152"],[0.1,"#c51b7d"],[0.2,"#de77ae"],[0.3,"#f1b6da"],[0.4,"#fde0ef"],[0.5,"#f7f7f7"],[0.6,"#e6f5d0"],[0.7,"#b8e186"],[0.8,"#7fbc41"],[0.9,"#4d9221"],[1.0,"#276419"]],"sequential":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]],"sequentialminus":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]},"hovermode":"closest","hoverlabel":{"align":"left"},"coloraxis":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"geo":{"showland":true,"landcolor":"rgba(229, 236, 246, 1.0)","showlakes":true,"lakecolor":"rgba(255, 255, 255, 1.0)","subunitcolor":"rgba(255, 255, 255, 1.0)","bgcolor":"rgba(255, 255, 255, 1.0)"},"mapbox":{"style":"light"},"polar":{"bgcolor":"rgba(229, 236, 246, 1.0)","radialaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""},"angularaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","ticks":""}},"scene":{"xaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"yaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true},"zaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","gridwidth":2.0,"zerolinecolor":"rgba(255, 255, 255, 1.0)","backgroundcolor":"rgba(229, 236, 246, 1.0)","showbackground":true}},"ternary":{"aaxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"baxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"caxis":{"ticks":"","linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)"},"bgcolor":"rgba(229, 236, 246, 1.0)"},"xaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"yaxis":{"title":{"standoff":15},"ticks":"","automargin":true,"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","zerolinecolor":"rgba(255, 255, 255, 1.0)","zerolinewidth":2.0},"annotationdefaults":{"arrowcolor":"#2a3f5f","arrowhead":0,"arrowwidth":1},"shapedefaults":{"line":{"color":"rgba(42, 63, 95, 1.0)"}},"colorway":["rgba(99, 110, 250, 1.0)","rgba(239, 85, 59, 1.0)","rgba(0, 204, 150, 1.0)","rgba(171, 99, 250, 1.0)","rgba(255, 161, 90, 1.0)","rgba(25, 211, 243, 1.0)","rgba(255, 102, 146, 1.0)","rgba(182, 232, 128, 1.0)","rgba(255, 151, 255, 1.0)","rgba(254, 203, 82, 1.0)"]},"data":{"bar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}},"errorx":{"color":"rgba(42, 63, 95, 1.0)"},"errory":{"color":"rgba(42, 63, 95, 1.0)"}}],"barpolar":[{"marker":{"line":{"color":"rgba(229, 236, 246, 1.0)","width":0.5},"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"carpet":[{"aaxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"},"baxis":{"linecolor":"rgba(255, 255, 255, 1.0)","gridcolor":"rgba(255, 255, 255, 1.0)","endlinecolor":"rgba(42, 63, 95, 1.0)","minorgridcolor":"rgba(255, 255, 255, 1.0)","startlinecolor":"rgba(42, 63, 95, 1.0)"}}],"choropleth":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"contourcarpet":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"heatmap":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"heatmapgl":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram":[{"marker":{"pattern":{"fillmode":"overlay","size":10,"solidity":0.2}}}],"histogram2d":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"histogram2dcontour":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"mesh3d":[{"colorbar":{"outlinewidth":0.0,"ticks":""}}],"parcoords":[{"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"pie":[{"automargin":true}],"scatter":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatter3d":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}},"line":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattercarpet":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergeo":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattergl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scattermapbox":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolar":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterpolargl":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"scatterternary":[{"marker":{"colorbar":{"outlinewidth":0.0,"ticks":""}}}],"surface":[{"colorbar":{"outlinewidth":0.0,"ticks":""},"colorscale":[[0.0,"#0d0887"],[0.1111111111111111,"#46039f"],[0.2222222222222222,"#7201a8"],[0.3333333333333333,"#9c179e"],[0.4444444444444444,"#bd3786"],[0.5555555555555556,"#d8576b"],[0.6666666666666666,"#ed7953"],[0.7777777777777778,"#fb9f3a"],[0.8888888888888888,"#fdca26"],[1.0,"#f0f921"]]}],"table":[{"cells":{"fill":{"color":"rgba(235, 240, 248, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}},"header":{"fill":{"color":"rgba(200, 212, 227, 1.0)"},"line":{"color":"rgba(255, 255, 255, 1.0)"}}}]}},"title":{"text":"eps: 0.7 minPts: 20 pts: 150 cluster: 2 noisePts: 14"},"xaxis":{"title":{"text":"sepal_length"}},"yaxis":{"title":{"text":"petal_length"}},"scene":{"zaxis":{"title":{"text":"petal_width"}}}};
            var config = {"responsive":true};
            Plotly.newPlot('3f02a07c-ee3f-44f7-97b7-0a33f84a6e6c', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_3f02a07cee3f44f797b70a33f84a6e6c();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_3f02a07cee3f44f797b70a33f84a6e6c();
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

