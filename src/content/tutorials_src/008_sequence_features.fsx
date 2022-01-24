(***hide***)

(*
#frontmatter
---
title: Modelling and visualizing sequence features with BioFSharp and Plotly.NET
category: advanced
authors: Kevin Schneider
index: 1
---
*)

#r "nuget: BioFSharp, 2.0.0-beta7"
#r "nuget: BioFSharp.IO, 2.0.0-beta6"
#r "nuget: Newtonsoft.JSON, 12.0.3"
#r "nuget: DynamicObj"

open BioFSharp
open System.IO

type SequenceFeature = {
    Name: string
    //zero-based
    Start: int
    //zero-based
    End: int
    Length: int
    Abbreviation: char
    Metadata: Map<string,string>
    FeatureType: string
} with
    static member create 
        (
            name: string,
            featureStart: int,
            featureEnd: int,
            ?Abbreviation: char,
            ?Metadata: Map<string,string>,
            ?FeatureType: string
        ) =
            if featureStart < 0 || featureEnd < 0 || featureStart > featureEnd then
                failwith $"invalid feature stretch ({featureStart},{featureEnd})"
            else
                {
                    Name            = name        
                    Start           = featureStart
                    End             = featureEnd  
                    Length          = featureEnd - featureStart + 1
                    Abbreviation    = Abbreviation |> Option.defaultValue ' '
                    Metadata        = Metadata |> Option.defaultValue (Map.ofList [])
                    FeatureType     = FeatureType |> Option.defaultValue ""
                }

    static member tryGetIntersection (feature1:SequenceFeature) (feature2:SequenceFeature) =
        let s1,e1 = feature1.Start, feature1.End
        let s2,e2 = feature2.Start, feature2.End
        
        if (s2 > e1) || (s1 > e2) then
            None
        else
            Some ((max s1 s2), (min e1 e2))

type AnnotatedSequence<'T when 'T :> IBioItem> = 
    {
        Tag: string
        Sequence : seq<'T>
        Features: Map<string,SequenceFeature list>
    } 

module AnnotatedSequence =
    
    let create tag sequence (featureMap: Map<string,SequenceFeature list>) =

        let mutable hasInvalidFeatures = false
        let mutable invalidFeatures: (string*(SequenceFeature*SequenceFeature)) list = []

        let isOverlap (stretch1:int * int) (stretch2: int * int) =
            let s1, e1 = stretch1
            let s2, e2 = stretch2

            (s1 <= s2 && e1 >= s2)
            || (s1 <= s2 && e1 >= e2)
            || (s1 <= e2 && e1 >= e2)
            || (s1 >= s2 && e1 <= e2)

        featureMap
        |> Map.iter (fun key features ->
            let rec loop (featureList:SequenceFeature list) =
                match featureList with
                | f::rest -> 
                    for frest in rest do 
                        if isOverlap (f.Start, f.End) (frest.Start, frest.End) then 
                            hasInvalidFeatures <- true
                            invalidFeatures <- (key,(f,frest))::invalidFeatures
                    loop rest
                | [] -> ()
            loop features
        )
        if hasInvalidFeatures then
            failwith $"""At least one  sequence feature annotation collection contains overlapping annotations. This is not supported. Please annotate them as separate feature lists.
Offending annotations: 
{invalidFeatures}
"""         
        else
            {
                Tag = tag
                Sequence = sequence
                Features= featureMap
            }

    let addFeatures (featureKey: string) (features: SequenceFeature list) (anns: AnnotatedSequence<_>) =
        {
            anns with
                Features = 
                    if Map.containsKey featureKey anns.Features then
                        anns.Features |> Map.add featureKey (features @ anns.Features.[featureKey])
                    else
                        anns.Features |> Map.add featureKey features
                
        }

    let toStrings (anns: AnnotatedSequence<_>) =
        let sequenceString = anns.Sequence |> Seq.map (BioItem.symbol >> string) |> String.concat ""
        let emptyFormatString = [for i in 1 .. (Seq.length anns.Sequence) do yield " "] |> String.concat ""
        let featureFormats =
            anns.Features
            |> Map.map (fun key features -> 
                features
                |> Seq.fold (fun (acc:string) (feature) ->
                    let featureStretch = [for _ in 1 .. feature.Length do yield (feature.Abbreviation |> string)] |> String.concat ""
                    acc
                        .Remove(feature.Start, feature.Length)
                        .Insert(feature.Start, featureStretch)
                ) emptyFormatString
            )
        sequenceString,featureFormats

    let format (anns: AnnotatedSequence<_>) =
        let sequenceString, featureStrings = anns |> toStrings
        let longestId = 
            ["Sequence"; yield! (featureStrings |> Map.toList |> List.map fst)] 
            |> Seq.maxBy (fun x -> x.Length)
            |> fun s -> s.Length

        let ids = 
            ["Sequence"; yield! (featureStrings |> Map.toList |> List.map fst)]
            |> List.map (fun s -> s.PadRight(longestId+4))
        
        let blocks = 
            [sequenceString; yield! (featureStrings |> Map.toList |> List.map snd)]
            |> List.mapi (fun index seqString ->
                let id = ids.[index]
                let splits = 
                    seqString.ToCharArray() 
                    |> Seq.map string
                    |> Seq.chunkBySize 60 
                    |> Seq.map (String.concat "")

                let innerSplits = 
                    splits |> Seq.map (fun s -> 
                        s.ToCharArray() 
                        |> Seq.map string
                        |> Seq.chunkBySize 10 
                        |> Seq.map (String.concat "")
                )

                innerSplits 
                |> Seq.mapi (fun i strs ->  
                    let line = 
                        strs 
                        |> Seq.fold (fun acc elem -> sprintf "%s %s" acc elem) "" 
                    $"{id} {(string (((i+1)*60) - 60 + 1)).PadLeft(10)}{line}" 
                )
                |> Array.ofSeq
            )

        [for i in 0 .. blocks.[0].Length-1 do
            for b in blocks do yield b.[i]
        ]
        |> String.concat System.Environment.NewLine
        |> fun s -> $"{System.Environment.NewLine}{s}{System.Environment.NewLine}"

(**
# Modelling and visualizing sequence features with BioFSharp and Plotly.NET

### Table of contents

- [Assigning secondary structure for proteins based on .pdb files](#Assigning-secondary-structure-for-proteins-based-on-pdb-files)
- [Comparing structural annotations](#Comparing-structural-annotations)
- [Generalizing sequence features]()
    - [Implementing the Sequence feature](#Implementing-the-Sequence-feature)
    - [Implementing the Annotated Sequence](#Implementing-the-Annotated-Sequence)
- [Visualizing sequence features with Plotly.NET](#Visualizing-sequence-features-with-Plotly-NET)
    - [Plotting sequences with Plotly.NET](#Plotting-sequences-with-Plotly-NET)
    - [A sequence feature view plot for AnnotatedSequence](#A-sequence-feature-view-plot-for-AnnotatedSequence)


## Assigning secondary structure for proteins based on .pdb files

I recently started to work with a lot of structural protein data with the aim of extracting features based on the proteins secondary structures.

This involved assigning secondary structures for `.pdb` files, which is a file format that contains positional information about each atom in a polipeptide chain.
As in many bioinformatic fields, tried-and-tested algorithms for this are several decades old but seem to be still the gold standard. 
The algorithm that pops up most is [**DSSP**](https://swift.cmbi.umcn.nl/gv/dssp/) (Dictionary of Protein Secondary Structure). You can clearly see the age in every ounce of that website.

DSSP was originally used to assign all secondary structures for the [PDB (Protein Data bank)](https://www.rcsb.org/). I cannot find a source if that is still the case though. 
`.pdb` files obtained from PDB usually already contain a section with the assigned structures, but this is not true for example for the output of alpha fold, which only predicts the raw atom positions without any structure assignment.

Using dssp is straight forward, it can be installed directly via apt on ubuntu, and there is a biocontainer available [here](https://biocontainers.pro/tools/dssp)

dssp itself is also very easy to use. Once in the container, simply run

```bash
dssp -i <.pdb file> -o <dssp file>
```

The output format of DSSP is weird, but writing parsers is not too hard. It contains metadata sections indicated by the line start, which are not very interesting fort my purposes.
The structure assignments are contained in a fixed-column data format further down the file. 

Here is an example of how it looks like:

```no-highlight
#  RESIDUE AA STRUCTURE BP1 BP2  ACC     N-H-->O    O-->H-N    N-H-->O    O-->H-N    TCO  KAPPA ALPHA  PHI   PSI    X-CA   Y-CA   Z-CA            CHAIN
  1    1 A M              0   0  235      0, 0.0     4,-0.1     0, 0.0     0, 0.0   0.000 360.0 360.0 360.0  58.6   -7.4   17.5   38.1               
  2    2 A Y     >  +     0   0  202      2,-0.1     4,-0.6     3,-0.1     0, 0.0   0.539 360.0  69.8-121.3  -9.6   -8.6   17.8   34.5               
  3    3 A Y  H  > S+     0   0  209      1,-0.2     4,-1.1     2,-0.2     3,-0.3   0.865  91.2  58.3 -82.3 -33.1   -5.5   19.3   32.5               
  4    4 A F  H  > S+     0   0  193      1,-0.2     4,-1.7     2,-0.2    -1,-0.2   0.861 101.0  56.5 -67.1 -33.2   -3.2   16.2   32.7               
  5    5 A S  H  > S+     0   0   91      1,-0.2     4,-1.5     2,-0.2    -1,-0.2   0.835 104.5  51.5 -71.2 -31.9   -5.6   13.8   31.0               
  6    6 A R  H  X S+     0   0  204     -4,-0.6     4,-1.4    -3,-0.3    -1,-0.2   0.816 108.4  51.3 -75.1 -30.1   -6.0   16.0   27.8               
  7    7 A V  H  X S+     0   0   96     -4,-1.1     4,-1.8     2,-0.2    -2,-0.2   0.924 110.2  49.0 -72.1 -41.8   -2.2   16.3   27.3               
  8    8 A A  H  X S+     0   0   54     -4,-1.7     4,-1.9     1,-0.2    -2,-0.2   0.860 109.1  52.8 -65.7 -36.0   -1.8   12.5   27.5               
  9    9 A A  H  X S+     0   0   65     -4,-1.5     4,-1.7     2,-0.2    -1,-0.2   0.885 108.6  50.6 -67.1 -36.6   -4.6   11.9   25.0               
 10   10 A R  H  X S+     0   0  206     -4,-1.4     4,-1.6     2,-0.2    -2,-0.2   0.888 110.4  48.5 -68.7 -39.2   -2.9   14.3   22.5               
 11   11 A T  H  X S+     0   0   84     -4,-1.8     4,-1.6     2,-0.2    -2,-0.2   0.894 109.7  52.6 -70.1 -36.5    0.5   12.5   22.7               
etc.
```

Writing a parser for that section was straight forward. I added it to [BioFSharp.IO]() if you are interested in using it yourself.

## Comparing structural annotations

Without going into too much detail, one of the things I am interested in is how the structural assignments of DSSP relate to other structural annotations for it.
An example would be **intrinsically disordered stretches**, parts of the chain that do not have a structure, but this disorder is actually crucial for the proteins function.

You can read more about disorder in protein structures [here](https://en.wikipedia.org/wiki/Intrinsically_disordered_proteins). An awesome ressource for disorder annotations is [DisProt](https://disprot.org/). You can download its annotations in an easily usable tsv format (no custom parsing yay).
With these two annotations at hand, i started scripting with BioFSharp and Plotly.NET to get visual comparisons of both features (DSSP structure and disprot annotation).

My first attempts involved chunking the sequence and annotations by 60 and creating a annotated heatmap, assigning color based on the one character code of the structure. It achieved the goal, but was very hard to read, especially for large sequences.
I wont even include the source code for this, because it obviously sucks:

<br>
<hr>

![heatmap]({{root}}images/sequence_features_heatmap.png))

_Fig1: My first pitiful attempt at visualizing sequence features_
<hr>
<br>

At this point, i thought i was pretty much near the goal of my project (i calculated some fancy metrics downstream from the features that do not belong in this post),
and therefore content with the visualization. But as often happens in any kind of project - especially in academia - the scale of the project increased and i wanted to include more features in my calculations.

One was the secondary structure assigment of [Stride](http://webclu.bio.wzw.tum.de/stride/) - basically an improved version of DSSP. Also, i wanted to look at different disprot annotations individually.
At this point, a generic solution for both handling sequence features as well as their visualization was needed.

Stride is not as straight-forward to use as DSSP. I ended up creating my own docker container that builds it from [source](http://webclu.bio.wzw.tum.de/stride/install.html):

```dockerfile
FROM biocontainers/biocontainers:vdebian-buster-backports_cv1

USER root
RUN apt-get update && (apt-get install -t buster-backports -y gcc || apt-get install -y gcc) && (apt-get install -t buster-backports -y make || apt-get install -y make) && apt-get clean && apt-get purge

WORKDIR /bin/stride
COPY ./stride.tar.gz /bin/stride

ENV DEBIAN_FRONTEND noninteractive
RUN tar -zxf stride.tar.gz
RUN make
ENV PATH="/bin/stride/:${PATH}"

WORKDIR /data
USER biodocker
```

## Implementing the Sequence feature

A sequence feature in its most basic form just need start and end index within the sequence. They are usually abbreviated by a one-character code in most visualizations, and DSSP as well as Stride use on-letter codes for their assignment. 
I added additional metadata such as the name, type, and length of the feature, as well as arbitrary metadata. The full implementation in BioFSharp can be seen [here]()

```
type SequenceFeature = 
    {
        Name: string
        //zero-based
        Start: int
        //zero-based
        End: int
        Length: int
        Abbreviation: char
        Metadata: Map<string,string>
        FeatureType: string
    }

```

## Implementing the Annotated Sequence

An annotated sequence is a sequence which has feature annotations. I decided to model these as a Map of sequence features, where the key represents the feature type, and the list contains the individual feature stretches of that type.
The sequence can also be tagged with a string to give it an identifier:

```
type AnnotatedSequence<'T when 'T :> IBioItem> = 
    {
        Tag: string
        Sequence : seq<'T>
        Features: Map<string,SequenceFeature list>
    } 

```

The full implementation with all additional functions can be found in BioFSharp [here]()

Based on this type, i first created a pretty printer in the fasta style to see if i was going in the right direction:

*)
open BioFSharp

let testSeq = 
    AnnotatedSequence.create
        "Test"
        ("ATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAG" |> BioArray.ofNucleotideString)
        (Map.ofList [
            "Feature1", [SequenceFeature.create("F1",0,10,'A')]
            "Feature2", [SequenceFeature.create("F2",0,10,'B'); SequenceFeature.create("F2",100,120,'B')]
            "Feature3", [SequenceFeature.create("F3",30,90,'C')]

        ])

AnnotatedSequence.format testSeq
(***include-it***)

(**
So with this type modelling i was able to annotate a sequence with arbitrary features and visualize their positions. This text-based representation has the same problems as my heatmap approach though: it gets quite hard to read with increasing sequence length and feature count.
Still, this is a nice pretty prionter for usage with `fsi.AddPrinter`.

# Visualizing sequence features with Plotly.NET

I took heavy inspiration from DisProt's sequence viewer, which displays feature lanes below the actual sequence as bars.

## Plotting sequences with Plotly.NET

To plot a sequence of characters on a 2D plot, we can leverage Plotly.NETs `Annotations`. 
To give the annotations points that can trigger hovertext, i added an invisible line trace behind them.
*)
#r "nuget: Plotly.NET, 2.0.0-preview.16"

open Plotly.NET
open Plotly.NET.LayoutObjects

let testSeqChart = 
    Chart.Line(
        [for i in 0..3 -> (i,1)], 
        MultiText=["A";"T";"G";"C"], 
        Opacity=0.0,
        ShowLegend = false,
        LineColor= Color.fromKeyword Black
    )
    |> Chart.withAnnotations (
        ["A";"T";"G";"C"]
        |> Seq.mapi (fun x text ->
                Annotation.init(
                    x,1,
                    Text=(string text),
                    ShowArrow=false,
                    Font = Font.init(Size=16.)
                )
        )
    )

(**
<hr>
*)

(***hide***)
testSeqChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 2: A simple sequence plot using Plotly.NET's Annotations._
<hr>
<br>
*)

(**
With some additional styling, we can make this look pretty good already:

- Remove the Y axis
- Mirror the X Axis
- Add spike lines per default (very usefull later when combining with the feature traces)

*)

type Chart with
    static member SequencePlot
        (
            annotationText: #seq<string>,
            ?FontSize: float
        ) =
            let fontSize = defaultArg FontSize 16.

            Chart.Line(
                [for i in 0..((Seq.length annotationText) - 1) -> (i,1)], 
                MultiText=annotationText, 
                Opacity=0.0,
                ShowLegend = false,
                LineColor= Color.fromKeyword Black
            )
            |> Chart.withXAxis(
                LinearAxis.init(
                    Visible=true, 
                    ShowLine= true, 
                    ShowTickLabels = true, 
                    ShowSpikes= true, 
                    ZeroLine = false, 
                    Range= StyleParam.Range.MinMax(0.,60.), // as default, show the first 60 characters. Double click to zoom out.
                    Title = Title.init("Sequence index (0-based)", Font=Font.init(Size=fontSize)),
                    TickFont = Font.init(Size=fontSize),
                    Ticks = StyleParam.TickOptions.Inside,
                    Mirror = StyleParam.Mirror.AllTicks
                )
            )        
            |> Chart.withYAxis(
                LinearAxis.init(Visible=false, ShowLine= false, ShowTickLabels = false, ShowGrid = false, ZeroLine=false)
            )
            |> Chart.withAnnotations (
                annotationText
                |> Seq.mapi (fun x text ->
                    Annotation.init(
                        x,1,
                        Text=(string text),
                        ShowArrow=false,
                        Font = Font.init(Size=fontSize)
                    )
                )
            )

let seqPlot = 
    Chart.SequencePlot(testSeq.Sequence |> Seq.map (BioItem.symbol >> string))
    |> Chart.withSize(1000)

(**
<hr>
*)

(***hide***)
seqPlot |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 3: A better styled version of the Sequence plot._
<hr>
<br>
*)

(**
## A sequence feature view plot for AnnotatedSequence

Now we need to add the feature traces. While Plotly.NET supports shapes to draw on a Plot, these have the disadvantage of not triggering hover events (at least to my knowledge).

So i decided to render each feature as a horizontal Bar trace, setting its `Base` property (the Bar start) to the feature start, and the length accordingly.

Using `Chart.SingleStack` in shared axis mode together with the previous sequence plot, this has the additional advantage that spikelines of the sequence plot span over the features (try hovering over the sequence below)

*)

let featureTraceTestPlot = 
    [
        Chart.SequencePlot(testSeq.Sequence |> Seq.map (BioItem.symbol >> string))
        [
            Chart.Bar(["Feature1", 20], Base=10, ShowLegend = false)
            Chart.Bar(["Feature1", 20], Base=41, ShowLegend = false)
            Chart.Bar(["Feature2", 50], Base=20, ShowLegend = false)
        ]
        |> Chart.combine
    ]
    |> Chart.SingleStack(Pattern=StyleParam.LayoutGridPattern.Coupled)
    |> Chart.withSize(1000)

(**
<hr>
*)

(***hide***)
featureTraceTestPlot |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 4: Bar traces with different bases can be used in a stacked chart to indicate features mapping to the sequence position of the sequence plot on top._
<hr>
<br>
*)

(**

That looks exactly like i wanted it to turn out!

The rest is now a matter of styling. here is what i did additionally (in words):

- render all features with the same color, unless indicated otherwise by a color mapping function
- As seen on the plot above, When there are multiple features in a single lane, they get rendered with a y offset. This can be overcome by setting the barmode of the chart layout to `Overlay`
- Add a x axis range slider to give more exploratory power

And here is the final result (in code):

*)


type Chart with
    static member SequenceFeatureView
        (
            annotatedSequence: AnnotatedSequence<_>,
            ?FontSize: float,
            ?ColorMapping: seq<(string*Color)>,
            ?ShowRangeSlider: bool
        ) =
            let showRangeSlider = defaultArg ShowRangeSlider true
            let sequenceString = annotatedSequence.Sequence |> Seq.map (BioItem.symbol >> string)

            let featureColorMap = 
                ColorMapping
                |> Option.defaultValue Seq.empty
                |> Map.ofSeq

            let featurePlots =
                annotatedSequence.Features
                |> Map.toSeq
                |> Seq.map (fun (featureName,features) ->
                    features
                    |> List.map (fun f ->
                        Chart.Bar(
                            [featureName,f.Length-1], 
                            Width=0.8, 
                            Base=f.Start, 
                            Text = $"({f.Start}-{f.End}):  {f.Abbreviation}", 
                            TextPosition = StyleParam.TextPosition.Inside,
                            ShowLegend = false,
                            MarkerColor = (Map.tryFind featureName featureColorMap |> Option.defaultValue (Color.fromKeyword Black))
                        )
                    
                    )
                )
                |> Seq.concat

            [
                Chart.SequencePlot(sequenceString, ?FontSize = FontSize)
                |> Chart.withYAxis(
                    LinearAxis.init(Domain = StyleParam.Range.MinMax(0.81,1.))
                )

                featurePlots
                |> Chart.combine
                |> Chart.withYAxis(
                    LinearAxis.init(ShowGrid=true, FixedRange = false, Domain = StyleParam.Range.MinMax(0.,0.79))
                )
            ]
            |> Chart.SingleStack(Pattern = StyleParam.LayoutGridPattern.Coupled)
            |> fun c -> 
                if showRangeSlider then
                    c
                    |> Chart.withXAxisRangeSlider(
                        RangeSlider.init(BorderColor=Color.fromKeyword Gray, BorderWidth=1.)
                    )
                else
                    c
            |> Chart.withConfig(
                Config.init(ModeBarButtonsToAdd=[
                    StyleParam.ModeBarButton.ToggleSpikelines
                ])
            )
            |> Chart.withLayout(
                Layout.init(
                    BarMode = StyleParam.BarMode.Overlay
                )
            )
            |> Chart.withTitle $"Sequence feature view for {annotatedSequence.Tag}"

(**
Here is what it looks like with a big test sequence:
*)

let bigTestSeq = 
    AnnotatedSequence.create
        "test sequence"
        ("ATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTCATGCTAGTGTC" |> BioArray.ofNucleotideString)
        (Map.ofList [
            "Feature 1", [SequenceFeature.create("F",1,33,'X');  SequenceFeature.create("F",50,60,'D')]
            "Feature 2", [SequenceFeature.create("F",0,30,'L');  SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,100,'L')]
            "Feature 3", [SequenceFeature.create("F",8,83,'X');  SequenceFeature.create("F",84,100,'D')]
            "Feature 4", [SequenceFeature.create("F",80,85,'L'); SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,79,'L')]            
            "Feature 5", [SequenceFeature.create("F",1,33,'X');  SequenceFeature.create("F",50,60,'D')]
            "Feature 6", [SequenceFeature.create("F",0,30,'L');  SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,100,'L')]
            "Feature 7", [SequenceFeature.create("F",8,83,'X');  SequenceFeature.create("F",84,100,'D')]
            "Feature 8", [SequenceFeature.create("F",80,85,'L'); SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,79,'L')]
            "Feature 9", [SequenceFeature.create("F",1,33,'X');  SequenceFeature.create("F",50,60,'D')]
            "Feature 10",[SequenceFeature.create("F",0,30,'L');  SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,100,'L')]
            "Feature 11",[SequenceFeature.create("F",8,83,'X');  SequenceFeature.create("F",84,100,'D')]
            "Feature 12",[SequenceFeature.create("F",80,85,'L'); SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,79,'L')]            
            "Feature 13",[SequenceFeature.create("F",1,33,'X');  SequenceFeature.create("F",50,60,'D')]
            "Feature 14",[SequenceFeature.create("F",0,30,'L');  SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,100,'L')]
            "Feature 15",[SequenceFeature.create("F",8,83,'X');  SequenceFeature.create("F",84,100,'D')]
            "Feature 16",[SequenceFeature.create("F",80,85,'L'); SequenceFeature.create("F",40,50,'E'); SequenceFeature.create("F",52,79,'L')]
        ])

let finalChart =
    Chart.SequenceFeatureView(
        bigTestSeq,
        ColorMapping = ["Feature 10", Color.fromKeyword DarkSalmon] // show feature 10 in a different color
    )
    |> Chart.withSize(1000)

(**
<hr>
*)

(***hide***)
finalChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 5: The final result of my feature view plotting efforts._
<hr>
*)