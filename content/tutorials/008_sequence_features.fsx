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

![heatmap](/img/heatmap.png)

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

AnnotatedSequence.format testSeq(* output: 
"
Sequence              1 ATGCTAGTGT CATGCTAGTG TCATGCTAGT GTCATGCTAG ATGCTAGTGT CATGCTAGTG
Feature1              1 AAAAAAAAAA A                                                     
Feature2              1 BBBBBBBBBB B                                                     
Feature3              1                                  CCCCCCCCCC CCCCCCCCCC CCCCCCCCCC
Sequence             61 TCATGCTAGT GTCATGCTAG ATGCTAGTGT CATGCTAGTG TCATGCTAGT GTCATGCTAG
Feature1             61                                                                  
Feature2             61                                             BBBBBBBBBB BBBBBBBBBB
Feature3             61 CCCCCCCCCC CCCCCCCCCC CCCCCCCCCC C                               
Sequence            121 ATGCTAGTGT CATGCTAGTG TCATGCTAGT GTCATGCTAG
Feature1            121                                            
Feature2            121 B                                          
Feature3            121                                            
"*)
(**
So with this type modelling i was able to annotate a sequence with arbitrary features and visualize their positions. This text-based representation has the same problems as my heatmap approach though: it gets quite hard to read with increasing sequence length and feature count.
Still, this is a nice pretty prionter for usage with `fsi.AddPrinter`.

# Visualizing sequence features with Plotly.NET

I took heavy inspiration from DisProt's sequence viewer, which displays feature lanes below the actual sequence as bars.

## Plotting sequences with Plotly.NET

To plot a sequence of characters on a 2D plot, we can leverage Plotly.NETs `Annotations`. 
To give the annotations points that can trigger hovertext, i added an invisible line trace behind them.

*)
#r "nuget: Plotly.NET, 2.0.0-preview.11"

open Plotly.NET
open Plotly.NET.LayoutObjects

let testSeqChart = 
    Chart.Line(
        [for i in 0..3 -> (i,1)], 
        Labels=["A";"T";"G";"C"], 
        Opacity=0.0,
        ShowLegend = false,
        Color= Color.fromKeyword Black
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

<div id="0535cf4e-cfbb-4fdb-be84-9870717f1e2b" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_0535cf4ecfbb4fdbbe849870717f1e2b = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"lines","x":[0,1,2,3],"y":[1,1,1,1],"line":{"color":"rgba(0, 0, 0, 1.0)"},"showlegend":false,"opacity":0.0,"marker":{"color":"rgba(0, 0, 0, 1.0)"},"text":["A","T","G","C"]}];
            var layout = {"annotations":[{"x":0,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":1,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":2,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":3,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"}]};
            var config = {};
            Plotly.newPlot('0535cf4e-cfbb-4fdb-be84-9870717f1e2b', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_0535cf4ecfbb4fdbbe849870717f1e2b();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_0535cf4ecfbb4fdbbe849870717f1e2b();
            }
</script>

_Fig 2: A simple sequence plot using Plotly.NET's Annotations._
<hr>
<br>

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
                Labels=annotationText, 
                Opacity=0.0,
                ShowLegend = false,
                Color= Color.fromKeyword Black
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

<div id="c2c12cd3-2db9-4e92-a114-59e04599a49e" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_c2c12cd32db94e92a11459e04599a49e = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"lines","x":[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159],"y":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1],"line":{"color":"rgba(0, 0, 0, 1.0)"},"showlegend":false,"opacity":0.0,"marker":{"color":"rgba(0, 0, 0, 1.0)"},"text":["A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G"]}];
            var layout = {"xaxis":{"visible":true,"title":{"text":"Sequence index (0-based)","font":{"size":16.0}},"range":[0.0,60.0],"ticks":"inside","mirror":"allticks","showticklabels":true,"showspikes":true,"tickfont":{"size":16.0},"showline":true,"zeroline":false},"yaxis":{"visible":false,"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"annotations":[{"x":0,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":1,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":2,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":3,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":4,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":5,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":6,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":7,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":8,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":9,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":10,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":11,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":12,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":13,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":14,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":15,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":16,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":17,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":18,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":19,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":20,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":21,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":22,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":23,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":24,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":25,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":26,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":27,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":28,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":29,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":30,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":31,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":32,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":33,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":34,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":35,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":36,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":37,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":38,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":39,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":40,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":41,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":42,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":43,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":44,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":45,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":46,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":47,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":48,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":49,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":50,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":51,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":52,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":53,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":54,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":55,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":56,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":57,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":58,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":59,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":60,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":61,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":62,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":63,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":64,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":65,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":66,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":67,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":68,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":69,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":70,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":71,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":72,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":73,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":74,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":75,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":76,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":77,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":78,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":79,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":80,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":81,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":82,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":83,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":84,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":85,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":86,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":87,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":88,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":89,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":90,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":91,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":92,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":93,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":94,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":95,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":96,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":97,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":98,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":99,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":100,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":101,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":102,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":103,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":104,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":105,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":106,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":107,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":108,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":109,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":110,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":111,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":112,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":113,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":114,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":115,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":116,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":117,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":118,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":119,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":120,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":121,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":122,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":123,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":124,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":125,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":126,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":127,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":128,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":129,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":130,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":131,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":132,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":133,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":134,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":135,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":136,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":137,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":138,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":139,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":140,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":141,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":142,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":143,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":144,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":145,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":146,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":147,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":148,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":149,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":150,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":151,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":152,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":153,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":154,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":155,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":156,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":157,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":158,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":159,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"}],"width":1000};
            var config = {};
            Plotly.newPlot('c2c12cd3-2db9-4e92-a114-59e04599a49e', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_c2c12cd32db94e92a11459e04599a49e();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_c2c12cd32db94e92a11459e04599a49e();
            }
</script>

_Fig 3: A better styled version of the Sequence plot._
<hr>
<br>

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

<div id="8c523ebf-6029-41f1-9fe9-3da90a569e56" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_8c523ebf602941f19fe93da90a569e56 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"lines","x":[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159],"y":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1],"line":{"color":"rgba(0, 0, 0, 1.0)"},"showlegend":false,"opacity":0.0,"marker":{"color":"rgba(0, 0, 0, 1.0)"},"text":["A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G"],"xaxis":"x","yaxis":"y"},{"type":"bar","showlegend":false,"x":[20],"y":["Feature1"],"base":10,"orientation":"h","marker":{"pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[20],"y":["Feature1"],"base":41,"orientation":"h","marker":{"pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[50],"y":["Feature2"],"base":20,"orientation":"h","marker":{"pattern":{}},"xaxis":"x","yaxis":"y2"}];
            var layout = {"xaxis":{"visible":true,"title":{"text":"Sequence index (0-based)","font":{"size":16.0}},"range":[0.0,60.0],"ticks":"inside","mirror":"allticks","showticklabels":true,"showspikes":true,"tickfont":{"size":16.0},"showline":true,"zeroline":false},"yaxis":{"visible":false,"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false},"annotations":[{"x":0,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":1,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":2,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":3,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":4,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":5,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":6,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":7,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":8,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":9,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":10,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":11,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":12,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":13,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":14,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":15,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":16,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":17,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":18,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":19,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":20,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":21,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":22,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":23,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":24,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":25,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":26,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":27,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":28,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":29,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":30,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":31,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":32,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":33,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":34,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":35,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":36,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":37,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":38,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":39,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":40,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":41,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":42,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":43,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":44,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":45,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":46,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":47,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":48,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":49,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":50,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":51,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":52,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":53,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":54,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":55,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":56,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":57,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":58,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":59,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":60,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":61,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":62,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":63,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":64,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":65,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":66,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":67,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":68,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":69,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":70,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":71,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":72,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":73,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":74,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":75,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":76,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":77,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":78,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":79,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":80,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":81,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":82,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":83,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":84,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":85,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":86,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":87,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":88,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":89,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":90,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":91,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":92,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":93,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":94,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":95,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":96,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":97,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":98,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":99,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":100,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":101,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":102,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":103,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":104,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":105,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":106,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":107,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":108,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":109,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":110,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":111,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":112,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":113,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":114,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":115,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":116,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":117,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":118,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":119,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":120,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":121,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":122,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":123,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":124,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":125,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":126,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":127,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":128,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":129,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":130,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":131,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":132,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":133,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":134,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":135,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":136,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":137,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":138,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":139,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":140,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":141,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":142,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":143,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":144,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":145,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":146,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":147,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":148,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":149,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":150,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":151,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":152,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":153,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":154,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":155,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":156,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":157,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":158,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":159,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"}],"xaxis2":{},"yaxis2":{},"grid":{"rows":2,"columns":1,"pattern":"coupled"},"width":1000};
            var config = {};
            Plotly.newPlot('8c523ebf-6029-41f1-9fe9-3da90a569e56', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_8c523ebf602941f19fe93da90a569e56();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_8c523ebf602941f19fe93da90a569e56();
            }
</script>

_Fig 4: Bar traces with different bases can be used in a stacked chart to indicate features mapping to the sequence position of the sequence plot on top._
<hr>
<br>

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
                            Color = (Map.tryFind featureName featureColorMap |> Option.defaultValue (Color.fromKeyword Black))
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

<div id="fa565fde-7446-47f7-aa57-ca97124bb344" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_fa565fde744647f7aa57ca97124bb344 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"lines","x":[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109],"y":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1],"line":{"color":"rgba(0, 0, 0, 1.0)"},"showlegend":false,"opacity":0.0,"marker":{"color":"rgba(0, 0, 0, 1.0)"},"text":["A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C","A","T","G","C","T","A","G","T","G","T","C"],"xaxis":"x","yaxis":"y"},{"type":"bar","showlegend":false,"x":[32],"y":["Feature 1"],"base":1,"width":0.8,"text":"(1-33):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 1"],"base":50,"width":0.8,"text":"(50-60):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[30],"y":["Feature 10"],"base":0,"width":0.8,"text":"(0-30):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(233, 150, 122, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 10"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(233, 150, 122, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[48],"y":["Feature 10"],"base":52,"width":0.8,"text":"(52-100):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(233, 150, 122, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[75],"y":["Feature 11"],"base":8,"width":0.8,"text":"(8-83):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[16],"y":["Feature 11"],"base":84,"width":0.8,"text":"(84-100):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[5],"y":["Feature 12"],"base":80,"width":0.8,"text":"(80-85):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 12"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[27],"y":["Feature 12"],"base":52,"width":0.8,"text":"(52-79):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[32],"y":["Feature 13"],"base":1,"width":0.8,"text":"(1-33):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 13"],"base":50,"width":0.8,"text":"(50-60):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[30],"y":["Feature 14"],"base":0,"width":0.8,"text":"(0-30):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 14"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[48],"y":["Feature 14"],"base":52,"width":0.8,"text":"(52-100):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[75],"y":["Feature 15"],"base":8,"width":0.8,"text":"(8-83):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[16],"y":["Feature 15"],"base":84,"width":0.8,"text":"(84-100):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[5],"y":["Feature 16"],"base":80,"width":0.8,"text":"(80-85):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 16"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[27],"y":["Feature 16"],"base":52,"width":0.8,"text":"(52-79):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[30],"y":["Feature 2"],"base":0,"width":0.8,"text":"(0-30):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 2"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[48],"y":["Feature 2"],"base":52,"width":0.8,"text":"(52-100):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[75],"y":["Feature 3"],"base":8,"width":0.8,"text":"(8-83):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[16],"y":["Feature 3"],"base":84,"width":0.8,"text":"(84-100):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[5],"y":["Feature 4"],"base":80,"width":0.8,"text":"(80-85):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 4"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[27],"y":["Feature 4"],"base":52,"width":0.8,"text":"(52-79):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[32],"y":["Feature 5"],"base":1,"width":0.8,"text":"(1-33):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 5"],"base":50,"width":0.8,"text":"(50-60):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[30],"y":["Feature 6"],"base":0,"width":0.8,"text":"(0-30):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 6"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[48],"y":["Feature 6"],"base":52,"width":0.8,"text":"(52-100):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[75],"y":["Feature 7"],"base":8,"width":0.8,"text":"(8-83):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[16],"y":["Feature 7"],"base":84,"width":0.8,"text":"(84-100):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[5],"y":["Feature 8"],"base":80,"width":0.8,"text":"(80-85):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 8"],"base":40,"width":0.8,"text":"(40-50):  E","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[27],"y":["Feature 8"],"base":52,"width":0.8,"text":"(52-79):  L","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[32],"y":["Feature 9"],"base":1,"width":0.8,"text":"(1-33):  X","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"},{"type":"bar","showlegend":false,"x":[10],"y":["Feature 9"],"base":50,"width":0.8,"text":"(50-60):  D","textposition":"inside","orientation":"h","marker":{"color":"rgba(0, 0, 0, 1.0)","pattern":{}},"xaxis":"x","yaxis":"y2"}];
            var layout = {"xaxis":{"visible":true,"title":{"text":"Sequence index (0-based)","font":{"size":16.0}},"range":[0.0,60.0],"ticks":"inside","mirror":"allticks","showticklabels":true,"showspikes":true,"tickfont":{"size":16.0},"showline":true,"zeroline":false,"rangeslider":{"bordercolor":"rgba(128, 128, 128, 1.0)","borderwidth":1.0,"yaxis":{}}},"yaxis":{"visible":false,"showticklabels":false,"showline":false,"showgrid":false,"zeroline":false,"domain":[0.81,1.0]},"annotations":[{"x":0,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":1,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":2,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":3,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":4,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":5,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":6,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":7,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":8,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":9,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":10,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":11,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":12,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":13,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":14,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":15,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":16,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":17,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":18,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":19,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":20,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":21,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":22,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":23,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":24,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":25,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":26,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":27,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":28,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":29,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":30,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":31,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":32,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":33,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":34,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":35,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":36,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":37,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":38,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":39,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":40,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":41,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":42,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":43,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":44,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":45,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":46,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":47,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":48,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":49,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":50,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":51,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":52,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":53,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":54,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":55,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":56,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":57,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":58,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":59,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":60,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":61,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":62,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":63,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":64,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":65,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":66,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":67,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":68,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":69,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":70,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":71,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":72,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":73,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":74,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":75,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":76,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":77,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":78,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":79,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":80,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":81,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":82,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":83,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":84,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":85,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":86,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":87,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":88,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":89,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":90,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":91,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":92,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":93,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":94,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":95,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":96,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":97,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":98,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":99,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":100,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":101,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":102,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"},{"x":103,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":104,"y":1,"font":{"size":16.0},"showarrow":false,"text":"A"},{"x":105,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":106,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":107,"y":1,"font":{"size":16.0},"showarrow":false,"text":"G"},{"x":108,"y":1,"font":{"size":16.0},"showarrow":false,"text":"T"},{"x":109,"y":1,"font":{"size":16.0},"showarrow":false,"text":"C"}],"xaxis2":{},"yaxis2":{"fixedrange":false,"showgrid":true,"domain":[0.0,0.79]},"grid":{"rows":2,"columns":1,"pattern":"coupled"},"barmode":"overlay","title":{"text":"Sequence feature view for test sequence"},"width":1000};
            var config = {"modeBarButtonsToAdd":["toggleSpikelines"]};
            Plotly.newPlot('fa565fde-7446-47f7-aa57-ca97124bb344', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_fa565fde744647f7aa57ca97124bb344();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_fa565fde744647f7aa57ca97124bb344();
            }
</script>

_Fig 5: The final result of my feature view plotting efforts._
<hr>

*)

