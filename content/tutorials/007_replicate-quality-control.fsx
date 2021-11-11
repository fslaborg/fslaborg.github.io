(**
[![Binder](https://fslab.org/images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/007_replicate-quality-control.ipynb)&emsp;
[![Script](https://fslab.org/images/badge-script.svg)](https://fslab.org/content/tutorials/007_replicate-quality-control.fsx)&emsp;
[![Notebook](https://fslab.org/images/badge-notebook.svg)](https://fslab.org/content/tutorials/007_replicate-quality-control.ipynb)

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
    |> Frame.indexRows "Key"(* output: 
Condition0_1     Condition0_2     Condition0_3     Condition1_1     Condition1_2     Condition1_3     Condition2_1     Condition2_2     Condition2_3     
Gene0  -> <missing>        <missing>        859.507048737706 892.488061131967 1018.39682842723 <missing>        1103.47465251202 1157.72940330711 1065.74060396554 
Gene1  -> 874.831680800388 750.248739657293 885.186911420285 928.994516057073 853.081858812674 793.574297701139 1065.97949919587 1131.14376992316 <missing>        
Gene2  -> 838.556912459832 852.727407339623 899.295260312015 860.880771705626 932.199854945633 976.124808642915 1207.93463145272 <missing>        1277.61049813247 
Gene3  -> 578.81785907921  678.347549342628 602.246497320338 <missing>        643.093516693419 <missing>        <missing>        873.194740469258 849.451122811244 
Gene4  -> 842.094396445274 965.835426665507 867.369051645365 928.252271146921 881.501122913359 <missing>        1054.1287942036  1171.60939846118 1038.00577431047 
Gene5  -> 1020.09691148753 1074.40387941314 1007.97548739644 1016.85273907176 1137.89035971088 1053.22697953361 1375.66060145121 1231.83519162636 <missing>        
Gene6  -> 1101.31859349101 1035.34541804719 1073.81849597601 1173.85556819786 1135.11445658909 1135.37014907268 1312.46683931566 <missing>        1446.11026206729 
Gene7  -> 1293.5505187121  1279.24240353059 1210.69733913481 1418.83379683658 1315.66861035805 1340.02039414327 1547.14933515457 1542.7958833035  1506.05595369617 
Gene8  -> 932.451969005451 943.887579432217 1064.35286448003 993.708988922016 1020.75857130752 1141.8439164254  1302.84589494999 <missing>        1255.55859138653 
Gene9  -> <missing>        676.324726723238 627.516574042796 <missing>        761.340619105394 629.926834913104 913.833973465211 <missing>        935.953193020724 
Gene10 -> <missing>        631.593373387394 <missing>        760.573132248994 667.777800663737 708.579020598344 897.652044466068 951.352427593893 913.498402976585 
Gene11 -> 925.558844182864 921.275745566886 962.402042467281 919.912081924523 915.817349871239 <missing>        1133.39251694594 1215.95999339307 1256.12477917909 
Gene12 -> 877.088860898347 910.78457773273  977.60432138044  906.470100177092 895.635227528066 974.138600172186 1268.46086169626 1130.47280685606 1209.22769248572 
Gene13 -> 961.944859271178 938.498328719872 913.620698710984 866.402054828119 906.174954450491 985.197102336624 1124.13083729644 1212.131809912   1159.25077296929 
Gene14 -> 992.748362800567 1000.19176657168 983.841273992796 1102.47481181182 1189.49999801473 1098.43197409232 1287.05128248142 1193.33301922455 <missing>        
:         ...              ...              ...              ...              ...              ...              ...              ...              ...              
Gene85 -> 1141.84782692372 966.934334466062 960.874704416693 1083.08973656227 1034.99233568037 1165.59614687963 1296.69301350431 1265.91311513118 <missing>        
Gene86 -> 709.392263094341 599.356231019707 627.809442533116 656.618659654456 693.650796020343 659.473041695018 852.316184386208 824.655405167439 925.919147678158 
Gene87 -> 899.380276781729 884.17260562817  948.935506964083 1089.52324524629 1029.65752699391 1003.33640042116 1298.44005521355 1280.59977722652 1241.62816145067 
Gene88 -> 1144.81738980837 1063.96798295389 1035.41837813454 1089.09832802759 <missing>        1201.22372291913 1403.27831423481 1289.29148231194 1310.98966263453 
Gene89 -> <missing>        1412.27615611229 1213.87168101546 1464.29266169994 1410.28662129759 1460.0814182168  1633.25421505257 1622.26533282371 1614.55971113835 
Gene90 -> 1380.11337491127 1298.94445749242 1388.80183781222 1380.04885949936 1464.39067804687 <missing>        1602.07971635588 1572.12371268329 1588.54863749936 
Gene91 -> 1104.4688200858  1044.62985634541 1038.52005763965 1118.68807975572 1079.12934076147 1110.47798847764 1357.74586648515 1341.72499952814 <missing>        
Gene92 -> 643.08246230498  713.608735692286 664.349154855986 804.005009357483 735.028682825565 715.584678733244 988.804126972969 <missing>        935.68406770922  
Gene93 -> 1066.41860092441 <missing>        1100.26729085649 1129.70531336959 1190.5905781972  1160.86694672781 1370.99671365522 1326.95197016386 1386.621278477   
Gene94 -> 842.568958216624 920.226880055378 824.776105482268 1000.45119277737 999.622914936843 926.209950863813 1179.08167586582 1186.13260877293 1151.99017404333 
Gene95 -> <missing>        776.852408021102 809.406302005126 866.770357211847 777.940357407953 868.312695316338 1039.8775076931  1072.75143664128 1095.87599329151 
Gene96 -> 736.95604192717  831.823622542142 790.627443848724 887.289779057259 781.461038651523 867.660284443528 932.981758186553 1042.45392008744 1149.56046307528 
Gene97 -> 979.147873857094 1033.86834107186 1010.60740727841 974.152107390665 1141.69306792765 1093.41903358936 1185.3882808851  1198.15294047248 1389.92530724036 
Gene98 -> 759.570384027805 661.370466127403 647.949680836865 831.520407212324 787.268819663521 855.916145637134 1018.7813832175  959.437629515552 1013.48894708169 
Gene99 -> <missing>        1170.12951077744 1103.50236480969 1212.87417318883 1219.28343069146 1184.0055742792  1340.62297540036 <missing>        1405.19376312643*)
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
    |> Frame.indexColsWith rawFrame.ColumnKeys(* output: 
Condition0_1      Condition0_2     Condition0_3      Condition1_1     Condition1_2       Condition1_3       Condition2_1     Condition2_2       Condition2_3      
Gene0  -> 815.0863716692485 834.177804837712 859.507048737706  892.488061131967 1018.39682842723   993.467661383195   1103.47465251202 1157.72940330711   1065.74060396554  
Gene1  -> 874.831680800388  750.248739657293 885.186911420285  928.994516057073 853.081858812674   793.574297701139   1065.97949919587 1131.14376992316   1097.76308399039  
Gene2  -> 838.556912459832  852.727407339623 899.295260312015  860.880771705626 932.199854945633   976.124808642915   1207.93463145272 1106.54977345808   1277.61049813247  
Gene3  -> 578.81785907921   678.347549342628 602.246497320338  650.900998152141 643.093516693419   662.8620463286804  871.917379913417 873.194740469258   849.451122811244  
Gene4  -> 842.094396445274  965.835426665507 867.369051645365  928.252271146921 881.501122913359   1009.0399679729114 1054.1287942036  1171.60939846118   1038.00577431047  
Gene5  -> 1020.09691148753  1074.40387941314 1007.97548739644  1016.85273907176 1137.89035971088   1053.22697953361   1375.66060145121 1231.83519162636   1258.40412114039  
Gene6  -> 1101.31859349101  1035.34541804719 1073.81849597601  1173.85556819786 1135.11445658909   1135.37014907268   1312.46683931566 1315.53219258206   1446.11026206729  
Gene7  -> 1293.5505187121   1279.24240353059 1210.69733913481  1418.83379683658 1315.66861035805   1340.02039414327   1547.14933515457 1542.7958833035    1506.05595369617  
Gene8  -> 932.451969005451  943.887579432217 1064.35286448003  993.708988922016 1020.75857130752   1141.8439164254    1302.84589494999 1219.183035179655  1255.55859138653  
Gene9  -> 735.2133784999505 676.324726723238 627.516574042796  650.900998152141 761.340619105394   629.926834913104   913.833973465211 845.062060254252   935.953193020724  
Gene10 -> 748.63186994741   631.593373387394 683.5207771870655 760.573132248994 667.777800663737   708.579020598344   897.652044466068 951.352427593893   913.498402976585  
Gene11 -> 925.558844182864  921.275745566886 962.402042467281  919.912081924523 915.817349871239   979.6678512544049  1133.39251694594 1215.95999339307   1256.12477917909  
Gene12 -> 877.088860898347  910.78457773273  977.60432138044   906.470100177092 895.635227528066   974.138600172186   1268.46086169626 1130.47280685606   1209.22769248572  
Gene13 -> 961.944859271178  938.498328719872 913.620698710984  866.402054828119 906.174954450491   985.197102336624   1124.13083729644 1212.131809912     1159.25077296929  
Gene14 -> 992.748362800567  1000.19176657168 983.841273992796  1102.47481181182 1189.49999801473   1098.43197409232   1287.05128248142 1193.33301922455   1353.28807884254  
:         ...               ...              ...               ...              ...                ...                ...              ...                ...               
Gene85 -> 1141.84782692372  966.934334466062 960.874704416693  1083.08973656227 1034.99233568037   1165.59614687963   1296.69301350431 1265.91311513118   1269.106757436075 
Gene86 -> 709.392263094341  599.356231019707 627.809442533116  656.618659654456 693.650796020343   659.473041695018   852.316184386208 824.655405167439   925.919147678158  
Gene87 -> 899.380276781729  884.17260562817  948.935506964083  1089.52324524629 1029.65752699391   1003.33640042116   1298.44005521355 1280.59977722652   1241.62816145067  
Gene88 -> 1144.81738980837  1063.96798295389 1035.41837813454  1089.09832802759 1095.4373870826148 1201.22372291913   1403.27831423481 1289.29148231194   1310.98966263453  
Gene89 -> 1336.03815552525  1412.27615611229 1213.87168101546  1464.29266169994 1410.28662129759   1460.0814182168    1633.25421505257 1622.26533282371   1614.55971113835  
Gene90 -> 1380.11337491127  1298.94445749242 1388.80183781222  1380.04885949936 1464.39067804687   1407.121461949345  1602.07971635588 1572.12371268329   1588.54863749936  
Gene91 -> 1104.4688200858   1044.62985634541 1038.52005763965  1118.68807975572 1079.12934076147   1110.47798847764   1357.74586648515 1341.72499952814   1258.40412114039  
Gene92 -> 643.08246230498   713.608735692286 664.349154855986  804.005009357483 735.028682825565   715.584678733244   988.804126972969 994.4828305630385  935.68406770922   
Gene93 -> 1066.41860092441  1131.85550949513 1100.26729085649  1129.70531336959 1190.5905781972    1160.86694672781   1370.99671365522 1326.95197016386   1386.621278477    
Gene94 -> 842.568958216624  920.226880055378 824.776105482268  1000.45119277737 999.622914936843   926.209950863813   1179.08167586582 1186.13260877293   1151.99017404333  
Gene95 -> 808.7914067089065 776.852408021102 809.406302005126  866.770357211847 777.940357407953   868.312695316338   1039.8775076931  1072.75143664128   1095.87599329151  
Gene96 -> 736.95604192717   831.823622542142 790.627443848724  887.289779057259 781.461038651523   867.660284443528   932.981758186553 1042.45392008744   1149.56046307528  
Gene97 -> 979.147873857094  1033.86834107186 1010.60740727841  974.152107390665 1141.69306792765   1093.41903358936   1185.3882808851  1198.15294047248   1389.92530724036  
Gene98 -> 759.570384027805  661.370466127403 647.949680836865  831.520407212324 787.268819663521   855.916145637134   1018.7813832175  959.437629515552   1013.48894708169  
Gene99 -> 1162.515191125715 1170.12951077744 1103.50236480969  1212.87417318883 1219.28343069146   1184.0055742792    1340.62297540036 1368.7660061523002 1405.19376312643*)
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
    |> HierarchicalClustering.mapClusterLeaftags fst // only keep the labels in the cluster tree(* output: 
Node
  (16, 7262.367644, 9,
   Node
     (15, 1317.505683, 6,
      Node
        (14, 765.0062545, 3,
         Node
           (13, 750.1592066, 2, Leaf (3, 1, "Condition1_1"),
            Leaf (5, 1, "Condition1_3")), Leaf (4, 1, "Condition1_2")),
      Node
        (11, 744.3808016, 3,
         Node
           (9, 684.1625739, 2, Leaf (0, 1, "Condition0_1"),
            Leaf (1, 1, "Condition0_2")), Leaf (2, 1, "Condition0_3"))),
   Node
     (12, 745.2649938, 3,
      Node
        (10, 701.3010992, 2, Leaf (6, 1, "Condition2_1"),
         Leaf (7, 1, "Condition2_2")), Leaf (8, 1, "Condition2_3")))*)
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


```
No value returned by any evaluator
```

## Interpretation

As can be seen in the graph, replicates of one condition cluster together. This is a good sign for the quality of the experiment. 
If one replicate of a condition does not behave this way, it can be considered an outlier.
If the replicates don't cluster together at all, there might be some problems with the experiment.


*)

