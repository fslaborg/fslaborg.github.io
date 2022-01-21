(***hide***)

(*
#frontmatter
---
title: q values
category: advanced
authors: Benedikt Venn
index: 3
---
*)

(***condition:prepare***)
#r "nuget: FSharpAux, 1.1.0"
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: FSharp.Data, 4.2.7"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.16"

(***condition:ipynb***)
#if IPYNB
#r "nuget: FSharpAux, 1.1.0"
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.15"
#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: FSharp.Data, 4.2.7"
#endif // IPYNB

open FSharp.Data
open Plotly.NET
open Plotly.NET.StyleParam
open Plotly.NET.LayoutObjects

// Extension of chart module for more automated chart styling
module Chart = 
    let myAxis name = LinearAxis.init(Title=Title.init name,Mirror=StyleParam.Mirror.All,Ticks=StyleParam.TickOptions.Inside,ShowGrid=false,ShowLine=true)
    let withAxisTitles x y chart = 
        chart 
        |> Chart.withTemplate ChartTemplates.lightMirrored
        |> Chart.withXAxis (myAxis x) 
        |> Chart.withYAxis (myAxis y)
(**
[![Binder]({{root}}images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/{{fsdocs-source-basename}}.ipynb)&emsp;
[![Script]({{root}}images/badge-script.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.fsx)&emsp;
[![Notebook]({{root}}images/badge-notebook.svg)]({{root}}content/tutorials/{{fsdocs-source-basename}}.ipynb)

# q values

_Summary:_ This blog post provides insight into the definition, calculation, and interpretation of q-values. _[Benedikt Venn](https://github.com/bvenn)_, 21 Jan 2022


### Table of contents

- [Introduction](#Introduction)
- [The multiple testing problem](#The-multiple-testing-problem)
- [False discovery rate](#False-discovery-rate)
    - [q value](#q value)
    - [Variants](#Variants)
- [Quality plots](#Quality-plots)
- [Definitions and Notes](#Definitions-and-Notes)
- [FAQ](#FAQ)
- [References](#References)

*)

let asd = 
    Chart.Point([1,2])

(***hide***)
asd |> GenericChart.toChartHTML
(***include-it-raw***)



(**
## Referencing packages

```fsharp
#r "nuget: FSharpAux, 1.1.0"             //required for auxiliary functions
#r "nuget: Plotly.NET, 2.0.0-preview.16" //required for charting
#r "nuget: FSharp.Stats, 0.4.3"          //required for all calculations
#r "nuget: FSharp.Data, 4.2.7"           //required to read the pvalue set

open Plotly.NET
open Plotly.NET.StyleParam
open Plotly.NET.LayoutObjects

// Extension of chart module for more automated chart styling
module Chart = 
    let myAxis name = LinearAxis.init(Title=Title.init name,Mirror=StyleParam.Mirror.All,Ticks=StyleParam.TickOptions.Inside,ShowGrid=false,ShowLine=true)
    let withAxisTitles x y chart = 
        chart 
        |> Chart.withTemplate ChartTemplates.lightMirrored
        |> Chart.withXAxis (myAxis x) 
        |> Chart.withYAxis (myAxis y)

```

## Introduction

<b>High throughput techniques</b> like microarrays with its successor RNA-Seq and mass spectrometry proteomics lead to an huge data amount.
Thousands of features (e.g. transcripts or proteins) are measured simultaneously. <b>Differential expression analysis</b> aims to identify features, that change significantly
between two conditions. A common experimental setup is the analysis of which genes are over- or underexpressed between e.g. a wild type and a mutant.

Hypothesis tests aim to identify differences between two or more samples. The most common statistical test is the <b>t test</b> that tests a difference of means. Hypothesis tests report 
a p value, that correspond the probability of obtaining results at least as extreme as the observed results, assuming that the null hypothesis is correct. In other words:

_<center>If there is no effect (no mean difference), a p value of 0.05 indicates that in 5 % of the tests a false positive is reported.</center>_

<hr>

Consider two population distributions that follow a normal distribution. Both have the <b>same</b> mean and standard deviation.
*)



open FSharpAux
open FSharp.Stats

let distributionA = Distributions.Continuous.normal 10.0 1.0
let distributionB = Distributions.Continuous.normal 10.0 1.0

let distributionChartAB = 
    [
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionA.PDF x),"distA")
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionB.PDF x),"distB")
    ]
    |> Chart.combine
    |> Chart.withAxisTitles "variable X" "relative count"
    |> Chart.withSize (900.,600.)
    |> Chart.withTitle "null hypothesis"


(**<center>*)
(***hide***)
distributionChartAB |> GenericChart.toChartHTML
(***include-it-raw***)

asd |> GenericChart.toChartHTML
(***include-it-raw***)

let k = Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionA.PDF x),"distA") |> GenericChart.toChartHTML

distributionA.PDF 2.
(***include-it-raw***)


(**
</center>

Samples with sample size 5 are randomly drawn from both population distributions.
Both samples are tested <b>if a mean difference exist</b> using a two sample t test where equal variances of the underlying population distribution are assumed.

*)

let getSample n (dist: Distributions.Distribution<float,float>) =
    Vector.init n (fun _ -> dist.Sample())
    
let sampleA = getSample 5 distributionA
let sampleB = getSample 5 distributionB

let pValue = (Testing.TTest.twoSample true sampleA sampleB).PValue

(***hide***)
pValue
(***include-it***)

(**
10,000 tests are performed, each with new randomly drawn samples. This corresponds to an experiment in which <b>none of the features changed</b> 
Note, that the mean intensities are arbitrary and must not be the same for all features! In the presented case all feature intensities are in average 10.
The same simulation can be performed with pairwise comparisons from distributions that differ for each feature, but are the same within the feature.
<b>The resulting p values are uniformly distributed between 0 and 1</b>

<br>

<center><img style="max-width:50%" src="../../images/qvalue_01.svg"></img></center>

_Fig 1: p value distribution of the null hypothesis._
<hr>
<br>

*)
(***hide***)
let nullDist = 
    Array.init 10000 (fun x -> 
        let sA = getSample 5 distributionA
        let sB = getSample 5 distributionB
        (Testing.TTest.twoSample true sA sB).PValue
        )


let nullDistributionChart = 
    nullDist 
    |> Distributions.Frequency.create 0.025 
    |> Map.toArray 
    |> Array.map (fun (k,c) -> k,float c) 
    |> Chart.StackedColumn 
    |> Chart.withTraceName "alt"
    |> Chart.withAxisTitles "pvalue" "frequency"

let thresholdLine =
    Shape.init(ShapeType.Line,0.05,0.05,0.,300.)

(**

Samples are called significantly different, if their p value is below a certain significance threshold ($\alpha$ level). While "the lower the better", a common threshold
is a p value of 0.05 or 0.01. In the presented case in average $10,000 * 0.05 = 500$ tests are <b>significant (red box), even though the populations do not differ</b>. They are called <b>false 
positives (FP)</b>. Now lets repeat the same experiment, but this time sample 70% of the time from null features (no difference) and <b>add 30% samples of truly 
differing</b> distributions. Therefore a third populations is generated, that differ in mean, but has an equal standard deviation:

*)

let distributionC = Distributions.Continuous.normal 11.5 1.0

(***hide***)
let distributionChartAC = 
    [
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionA.PDF x),"distA")
        Chart.Area([5. .. 0.01 .. 15.] |> List.map (fun x -> x,distributionC.PDF x),"distC")
    ]
    |> Chart.combine
    |> Chart.withAxisTitles  "variable X" "relative count"
    |> Chart.withSize (1000.,600.)
    |> Chart.withTitle "alternative hypothesis"

//distributionChartAC |> GenericChart.toChartHTML

(**

<center><img style="max-width:50%" src="../../images/qvalue_02.svg"></img></center>

_Fig 2: p value distribution of the alternative hypothesis. Blue coloring indicate p values deriving from distribution A and B (null). 
Orange coloring indicate p values deriving from distribution A and C (truly differing)._


The pvalue distribution of the tests resulting from truly differing populations are <b>right skewed</b>, while the null tests again show a homogeneous distribution between 0 and 1. 
Many, but not all of the tests that come from the truly differing populations are below 0.05, and therefore would be reported as significant.
In average 350 null features would be reported as significant even though they derive from null features (blue bars, 10,000 x 0.7 x 0.05 = 350).


##The multiple testing problem

The hypothesis testing framework with the p value definition given above was <b>developed for performing just one test. If many tests are performed, like in modern high throughput studies, the probability to obtain a 
false positive result increases.</b> The probability of at least one false positive is called Familywise error rate (FWER) and can be determined by $FWER=1-(1-\alpha)^m$ where 
$\alpha$ corresponds to the significance threshold (here 0.05) and $m$ is the number of performed tests.

*)

(***hide***)

let bonferroniLine = 
    Shape.init(ShapeType.Line,0.,35.,0.05,0.05,Line=Line.init(Dash=DrawingStyle.Dash))

let fwer = 
    [1..35]
    |> List.map (fun x -> 
        x,(1. - (1. - 0.05)**(float x))
        )
    |> Chart.Point
    |> Chart.withYAxisStyle("",MinMax=(0.,1.))
    |> Chart.withAxisTitles "#tests" "p(at least one FP)" 
    |> Chart.withShape bonferroniLine
    |> Chart.withTitle "FWER"

(*** condition: ipynb ***)
#if IPYNB
fwer
#endif // IPYNB

(**<center>*)
(***hide***)
fwer |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>

_Fig 3: Family wise error rate depending on number of performed tests. The black dashed line indicates the Bonferroni corrected FWER by $p^* = \frac{\alpha}{m}$ ._


When 10,000 null features are tested with a p value threshold of 0.05, in average 500 tests are reported significant even if there is not a single comparisons in which the 
population differ. **If some of the features are in fact different, the number of false positives consequently decreases (remember, the p value is defined for tests of null features).**

Why the interpretation of high throughput data based on p values is difficult: The more features are measured, the more false positives you can expect. If 100 differentially 
expressed genes are identified by p value thresholding, without further information about the magnitude of expected changes and the total number of measured transcripts, the 
data is useless. 

The p value threshold has no straight-forward interpretation when many tests are performed. Of course you could restrict the family wise error rate to 0.05, regardless 
how many tests are performed. This is realized by dividing the $\alpha$ significance threshold by the number of tests, which is known as Bonferroni correction: $p^* = \frac{\alpha}{m}$.
This correction drastically limit the false positive rate, but in an experiment with a huge count of expected changes, it additionally would result in many false negatives. The 
FWER should be chosen if the costs of follow up studies to tests the candidates are dramatic or there is a huge waste of time to potentially study false positives.

##False discovery rate

A more reasonable measure of significance with a simple interpretation is the so called false discovery rate (FDR). **It describes the rate of expected false positives within the 
overall reported significant features.** The goal is to identify as many sig. features as possible while incurring a relatively low proportion of false positives.
Consequently a set of reported significant features together with the <b>FDR describes the confidence of this set</b>, without the requirement to 
somehow incorporate the uncertainty that is introduced by the total number of tests performed. In the simulated case of 7,000 null tests and 3,000 tests resulting from truly 
differing distributions above, the FDR can be calculated exactly. Therefore at e.g. a p value of 0.05 the number of false positives (blue in red box) are divided by the number 
of significant reported tests (false positives + true positives). 




<br>
<hr>

<center><img style="max-width:75%" src="../../images/qvalue_03.svg"></img></center>

_Fig 4: p value distribution of the alternative hypothesis._
<hr>
<br>

Given the conditions described in the first chapter, the FDR of this experiment with a p value threshold of 0.05 is 0.173. Out of the 2019 reported significant comparisons, in average 350 
are expected to be false positives, which gives an straight forward interpretation of the data confidence. In real-world experiments the proportion of null tests and tests 
deriving from an actual difference is of course unknown. **The proportion of null tests however tends to be distributed equally in the p value histogram.** By identification of 
the average null frequency, a proportion of FP and TP can be determined and the FDR can be defined. This frequency estimate is called $\pi_0$, which leads to an FDR definition of:



<br>


<center><img style="max-width:75%" src="../../images/qvalue_04.svg"></img></center>

_Fig 5: FDR calculation on simulated data._

<br>

*)



(**



###q value

Consequently for each presented p value a corresponding FDR can be calculated. The minimum local FDR at each p value is called q value. 

$$\hat q(p_i) = min_{t \geq p_i} \hat{FDR}(p_i)$$


Since the q value is not monotonically increasing, it is smoothed by assigning the lowest FDR of all p values, that are equal or higher the current one.

**By defining $\pi_0$, all other parameters can be calculated from the given p value distribution to determine the all q values.** The most prominent 
FDR-controlling method is known as Benjamini-Hochberg correction. It sets $\pi_0$ as 1, assuming that all features are null. In studies with an expected high proportion of true 
positives, a $\pi_0$ of 1 is too conservative, since there definitely are true positives in the data. 

A better estimation of $\pi_0$ is given in the following:

<b>True positives are assumed to be right skewed while null tests are distributed equally between 0 and 1</b>. Consequently the right flat region of the p value histogram tends to correspond 
to the true frequency of null comparisons (Fig 5). As <b>real world example</b> 9856 genes were measured in triplicates under two conditions (control and treatment). The p value distribution of two 
sample t tests looks as follows:


*)

let examplePVals = 
    //let rawData = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/pvalExample.txt"
    let rawData = Http.RequestString @"https://raw.githubusercontent.com/bvenn/datasets/main/data/pvalExample.txt"
    rawData.Split '\n'
    |> Array.tail
    |> Array.map float

(***hide***)

//number of tests
let m =  
    examplePVals
    |> Array.length
    |> float

let nullLine =
    Shape.init(ShapeType.Line,0.,1.,1.,1.,Line=Line.init(Dash=DrawingStyle.Dash))

let empLine =
    Shape.init(ShapeType.Line,0.,1.,0.4,0.4,Line=Line.init(Dash=DrawingStyle.DashDot,Color=Color.fromHex "#FC3E36"))

let exampleDistribution = 
    [
        [
        examplePVals
        |> Distributions.Frequency.create 0.025
        |> Map.toArray 
        |> Array.map (fun (k,c) -> k,float c / (m * 0.025))
        |> Chart.Column
        |> Chart.withTraceName "density"
        |> Chart.withAxisTitles "p value" "density"
        |> Chart.withShapes [nullLine;empLine]

        examplePVals
        |> Distributions.Frequency.create 0.025
        |> Map.toArray 
        |> Array.map (fun (k,c) -> k,float c)
        |> Chart.Column
        |> Chart.withTraceName "gene count"
        |> Chart.withAxisTitles "p value" "gene count"
        ]
    ]
    |> Chart.Grid()
    |> Chart.withSize(1100.,550.)


(*** condition: ipynb ***)
#if IPYNB
exampleDistribution
#endif // IPYNB

(**<center>*)
(***hide***)
exampleDistribution |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>

_Fig 6: p value distributions of real world example. The frequency is given on the right, its density on the left. The black dashed line indicates the distribution, if all features
were null. The red dash-dotted line indicates the visual estimated pi0._

<br>
<hr>



By performing t tests for all comparisons 3743 (38 %) of the genes lead to a pvalue lower than 0.05.
By eye, you would estimate $\pi_0$ as 0.4, indicating, only a small fraction of the genes are unaltered (null). After q value calculations, you would filter for a specific FDR (e.g. 0.05) and 
end up with an p value threshold of 0.04613, indicating a FDR of max. 0.05 in the final reported 3642 genes. 

```no-highlight
pi0     = 0.4
m       = 9856
D(p)    = number of sig. tests at given p
FP(p)   = p*0.4*9856
FDR(p)  = FP(p) / D(p)
```

FDR(0.04613) = 0.4995 
<br>


*)
(***hide***)
let pi0 = 0.4

let getD p = 
    examplePVals 
    |> Array.sumBy (fun x -> if x <= p then 1. else 0.) 

let getFP p = p * pi0 * m

let getFDR p = (getFP p) / (getD p)

let qvaluesNotSmoothed = 
    examplePVals
    |> Array.sort
    |> Array.map (fun x -> 
        x, getFDR x)
    |> Chart.Line 
    |> Chart.withTraceName "not smoothed"
let qvaluesSmoothed = 
    let pValsSorted =
        examplePVals
        |> Array.sortDescending
    let rec loop i lowest acc  = 
        if i = pValsSorted.Length then 
            acc |> List.rev
        else 
            let p = pValsSorted.[i]
            let q = getFDR p
            if q > lowest then  
                loop (i+1) lowest ((p,lowest)::acc)
            else loop (i+1) q ((p,q)::acc)
    loop 0 1. []
    |> Chart.Line
    |> Chart.withTraceName "smoothed"
let eXpos = examplePVals |> Array.filter (fun x -> x <= 0.046135) |> Array.length
let p2qValeChart =
    [qvaluesNotSmoothed;qvaluesSmoothed]
    |> Chart.combine
    |> Chart.withYAxisStyle("",MinMax=(0.,1.))
    |> Chart.withAxisTitles "p value" "q value"
    |> Chart.withShape empLine
    |> Chart.withTitle (sprintf "#[genes with q value < 0.05] = %i" eXpos)


(*** condition: ipynb ***)
#if IPYNB
p2qValeChart
#endif // IPYNB

(**<center>*)
(***hide***)
p2qValeChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>

_Fig 7: FDR calculation on experiment data. Please zoom into the very first part of the curve to inspect the monotonicity._
<hr>



###The automatic detection of $\pi_0$ is facilitated as follows


For a range of $\lambda$ in e.g. $\{0.0  ..  0.05  ..  0.95\}$, calculate $\hat \pi_0 (\lambda) = \frac {\#[p_j > \lambda]}{m(1 - \lambda)}$

*)


let pi0Est = 
    [|0. .. 0.05 .. 0.95|]
    |> Array.map (fun lambda -> 
        let num = 
            examplePVals 
            |> Array.sumBy (fun x -> if x > lambda then 1. else 0.) 
        let den = float examplePVals.Length * (1. - lambda)
        lambda, num/den
        )

(***hide***)
let pi0EstChart = 
    pi0Est 
    |> Chart.Point
    |> Chart.withYAxisStyle("",MinMax=(0.,1.))
    |> Chart.withXAxisStyle("",MinMax=(0.,1.))
    |> Chart.withAxisTitles "$\lambda$" "$\hat \pi_0(\lambda)$"
    |> Chart.withMathTex(true)
    |> Chart.withConfig(
        Config.init(
            Responsive=true, 
            ModeBarButtonsToAdd=[
                ModeBarButton.DrawLine
                ModeBarButton.DrawOpenPath
                ModeBarButton.EraseShape
                ]
            )
        )


(*** condition: ipynb ***)
#if IPYNB
pi0EstChart
#endif // IPYNB

(**<center>*)
(***hide***)
pi0EstChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
</center>

_Fig 8: pi0 estimation._
<hr>

The resulting diagram shows, that with increasing $\lambda$ its function value $\hat \pi_0(\lambda)$ tends to $\pi_0$. The calculation <b>relates the actual proportion of tests greater than $\lambda$ to the proportion of $\lambda$ range the corresponding p values are in</b>.
In Storey & Tibshirani 2003 this curve is fitted with a <b>cubic spline</b>. A weighting of the knots by $(1 - \lambda)$ is recommended 
but not specified in the final publication. Afterwards the function value at $\hat \pi_0(1)$ is defined as final estimator of $\pi_0$. This is often referred to as the _smoother method_.

Another method (_bootstrap method_) (Storey et al., 2004), that does not depend on fitting is based on <b>bootstrapping</b> and was introduced in Storey et al. (2004). It is implemented in FSharp.Stats:

  1. Determine the minimal $\hat \pi_0 (\lambda)$ and call it $min \hat \pi_0$ . 

  2. For each $\lambda$, bootstrap the p values (e.g. 100 times) and calculate the mean squared error (MSE) from the difference of resulting $\hat \pi_0^b$ to $min  \hat \pi_0$. The minimal MSE indicates the best $\lambda$. With $\lambda$ 
defined, $\pi_0$ can be determined. <b>Note: When bootstrapping an data set of size n, n elements are drawn with replacement.</b>



*)


(***hide***)
let getpi0Bootstrap (lambda:float[]) (pValues:float[]) =
    let rnd = System.Random()
    let m = pValues.Length |> float
    let getpi0hat lambda pVals=
        let hits = 
            pVals 
            |> Array.sumBy (fun x -> if x > lambda then 1. else 0.) 
        hits / (m * (1. - lambda))
    
    //calculate MSE for each lambda
    let getMSE lambda =
        let mse = 
            //generate 100 bootstrap samples of p values and calculate the MSE at given lambda
            Array.init 100 (fun b -> 
                Array.sampleWithReplacement rnd pValues pValues.Length  
                |> getpi0hat lambda
                )
        mse
    lambda
    |> Array.map (fun l -> l,getMSE l)
    

let minimalpihat = 
    //FSharp.Stats.Testing.MultipleTesting.Qvalues.pi0hats  [|0. .. 0.05 .. 0.96|] examplePVals |> Array.minBy snd |> snd
    0.3686417749

let minpiHatShape = 
    Shape.init(ShapeType.Line,0.,1.,minimalpihat,minimalpihat,Line=Line.init(Dash=DrawingStyle.Dash))

let bootstrappedPi0 =
    getpi0Bootstrap [|0. .. 0.05 .. 0.95|] examplePVals
    |> Array.map (fun (l,x) -> 
        Chart.BoxPlot(x=Array.init x.Length (fun _ -> l),y=x,Fillcolor=Color.fromHex"#1F77B4",MarkerColor=Color.fromHex"#1F77B4",Name=sprintf "%.2f" l))
    |> Chart.combine
    |> Chart.withYAxisStyle("",MinMax=(0.,1.))
    |> Chart.withAxisTitles "$\lambda$" "$\hat \pi_0$"
    |> Chart.withMathTex(true)
    |> Chart.withShape minpiHatShape
    |> Chart.withConfig(
        Config.init(
            Responsive=true, 
            ModeBarButtonsToAdd=[
                ModeBarButton.DrawLine
                ModeBarButton.DrawOpenPath
                ModeBarButton.EraseShape
                ]
            )
        )


(*** condition: ipynb ***)
#if IPYNB
bootstrappedPi0
#endif // IPYNB

bootstrappedPi0 |> GenericChart.toChartHTML
(***include-it-raw***)

(**


_Fig 9: Bootstrapping for pi0 estimation. The dashed line indicates the minimal pi0 from Fig. 8.
The bootstrapped pi0 distribution that shows the least variation to the dashed line is the optimal. In the presented example it is either 0.8 or 0.85._
<hr>

For an $\lambda$, range of $\{0.0  ..  0.05  ..  0.95\}$ the bootstrapping method determines either 0.8 or 0.85 as optimal $\lambda$ and therefore $optimal  \hat \pi_0$ is either $0.3703$ or $0.3686$.

The <b>automated estimation</b> of $\pi_0$ based on bootstrapping is implemented in `FSharp.Stats.Testing.MultipleTesting.Qvalues`.

*)
open Testing.MultipleTesting

let pi0Stats = Qvalues.pi0BootstrapWithLambda [|0.0 .. 0.05 .. 0.95|] examplePVals


(*** condition: ipynb ***)
#if IPYNB
pi0Stats
#endif // IPYNB

(***hide***)
pi0Stats 
(***include-it***)


(**
Subsequent to $\pi_0$ estimation the <b>q values can be determined</b> from a list of p values.
*)

let qValues = Qvalues.ofPValues pi0Stats examplePVals

(*** condition: ipynb ***)
#if IPYNB
qValues
#endif // IPYNB

(***hide***)
qValues 
(***include-it***)

(**
###Variants

A robust variant of q value determination exists, that is more conservative for small p values when
the total number of p values is low. Here the number of false positives is divided by the number of 
total discoveries multiplied by the FWER at the current p value. The correction takes into account 
the probability of a false positive being reported in the first place.

Especially when the population distributions do not follow a perfect normal distribution or the p value distribution looks strange, 
the usage of the robust version is recommended.

<center>

$qval = {\#FP \over \#Discoveries}$ 

$qval_{robust} = {\#FP \over \#Discoveries \times (1-(1-p)^m)}$ 

</center>

*)

let qvaluesRobust = 
    Testing.MultipleTesting.Qvalues.ofPValuesRobust pi0Stats examplePVals

let qChart =    
    [
        Chart.Line(Array.sortBy fst (Array.zip examplePVals qValues),Name="qValue")
        Chart.Line(Array.sortBy fst (Array.zip examplePVals qvaluesRobust),Name="qValueRobust")
    ]
    |> Chart.combine
    |> Chart.withAxisTitles "p value" "q value"

(*** condition: ipynb ***)
#if IPYNB
qChart
#endif // IPYNB

(***hide***)
qChart |> GenericChart.toChartHTML
(***include-it-raw***)


(**
_Fig 10: Comparison of q values and robust q values, that is more conservative at low p values._


##Quality plots

*)

let pi0Line = 
    Shape.init(ShapeType.Line,0.,1.,pi0Stats,pi0Stats,Line=Line.init(Dash=DrawingStyle.Dash))

// relates the q value to each p value
let p2q = 
    Array.zip examplePVals qValues
    |> Array.sortBy fst
    |> Chart.Line
    |> Chart.withShape pi0Line
    |> Chart.withAxisTitles "p value" "q value"

// shows the p values distribution for an visual inspection of pi0 estimation
let pValueDistribution =
    let frequencyBins = 0.025 
    let m = examplePVals.Length |> float
    examplePVals 
    |> Distributions.Frequency.create frequencyBins 
    |> Map.toArray 
    |> Array.map (fun (k,c) -> k,float c / frequencyBins / m) 
    |> Chart.StackedColumn 
    |> Chart.withTraceName "p values"
    |> Chart.withAxisTitles "p value" "frequency density"
    |> Chart.withShape pi0Line

// shows pi0 estimation in relation to lambda
let pi0Estimation = 
    //Testing.MultipleTesting.Qvalues.pi0hats [|0. .. 0.05 .. 0.96|] examplePVals
    [|0. .. 0.05 .. 0.95|]
    |> Array.map (fun lambda -> 
        let num =   
            examplePVals 
            |> Array.sumBy (fun x -> if x > lambda then 1. else 0.)
        let den = float examplePVals.Length * (1. - lambda)
        lambda, num/den
        )
    |> Chart.Point
    |> Chart.withAxisTitles "$\lambda$" "$\hat \pi_0(\lambda)$"
    |> Chart.withMathTex(true)



(*** condition: ipynb ***)
#if IPYNB
p2q
#endif // IPYNB

(***hide***)
p2q |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 11: p value relation to q values. At a p value of 1 the q value is equal to pi0 (black dashed line)._
*)

(*** condition: ipynb ***)
#if IPYNB
pValueDistribution
#endif // IPYNB

(***hide***)
pValueDistribution |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 12: p value density distribution. The dashed line indicates pi0 estimated by Storeys bootstrapping method._
*)

(*** condition: ipynb ***)
#if IPYNB
pi0Estimation
#endif // IPYNB

(***hide***)
pi0Estimation |> GenericChart.toChartHTML
(***include-it-raw***)

(**
_Fig 13: Visual pi0 estimation._
*)

(**
##Definitions and Notes
  - Benjamini-Hochberg (BH) correction is equal to q values with $\pi_0 = 1$
  - Storey & Tibshirani (2003):
    - _"The 0.05 q-value cut-off is arbitrary, and we do not recommend that this value necessarily be used."_
    - _"The q-value for a particular feature is the expected proportion of false positives occurring up through that feature on the list."_
    - _"The precise definition of the q-value for a particular feature is the following. The q-value for a particular feature is the minimum false discovery rate that can be attained when calling all features up through that one on the list significant."_
    - _"The Benjamini & Hochberg (1995) methodology also forces one to choose an acceptable FDR level before any data are seen, which is often going to be impractical."_
  - To improve the q value estimation if the effects are asymmetric, meaning that negative effects are stronger than positives, or vice versa a method was published in 2014 by Orr et al.. They estimate a global $m_0$ and then split the p values 
  in two groups before calculating q values for each p value set. The applicability of this strategy however is questionable, as the number of up- and downregulated features must be equal, which is not the case in most biological experimental setups.
  - The distinction of FDR and pFDR (positive FDR) is not crucial in the presented context, because in high throughput experiments with m>>100: Pr(R > 0) ~ 1 (Storey & Tibshirani, 2003, Appendix Remark A).
  - The local FDR (lFDR) is sometimes referred to as the probability that for the current p value the null hypothesis is true (Storey 2011).
  - If you have found typos, errors, or misleading statements, please feel free to file a pull request or contact me.


##FAQ
  - Why are q values lower than their associated p values?
    - q values are not necessarily greater than their associated p values. q values can maximal be pi0. The definition of p values is not the same as for q values! A q
    value defines what proportion of the reported discoveries may be false.

  - Which cut off should I use?
    - _"The 0.05 q-value cut-off is arbitrary, and we do not recommend that this value necessarily be used."_ (Storey 2003). It depends on your experimental design and the number of false positives you are willing to accept.
    If there are _20 discoveries_, you may argue to accept if _2_ of them are false positives (FDR=0.1). On the other hand, if there are _10,000 discoveries_ with _1,000 false positives_ (FDR=0.1) you may should reduce the FDR. Thereby the 
    proportion of false positives decreases. Of course in this case the number of positives will decrease as well. It all breaks down to the matter of willingness to accept a certain number of false positives within your study. 
    Studies, that aim to identify the presence of an specific protein of interest, the FDR should be kept low, because it inflates the risk, that this particular candidate is a false positive.
    If confirmatory follow up studies are cheap, you can increase the FDR, if they are **expensive**, you should restrict the number of false positives to **avoid unpleasant discussions with your supervisor**. 
    
  - In my study gene RBCM has an q value of 0.03. Does that indicate, there is a 3% chance, that it is an false positive?
    - No, actually the change that this particular gene is an false positive may actually be higher, because there may be genes that are much more significant than MSH2. The q value indicates, 
    that 3% of the genes that are as or more extreme than RBCM are false positives (Storey 2003).

  - Should I use the default or robust version for my study?

  - When should I use q values over BH correction, or other multiple testing variants?
    - There is no straight forward answer to this question. If you are able to define a confident pi0 estimate by eye when inspecting the p value distribution, then the q value approach may be feasible.
    If you struggle in defining pi0, because the p value distribution has an odd shape or there are too few p values on which you base your estimate, it is better to choose the more conservative BH correction, or even
    consider other methodologies.

##References
  - Storey JD, Tibshirani R, Statistical significance for genomewide studies, 2003 [DOI: 10.1073/pnas.1530509100](https://www.pnas.org/content/100/16/9440)
  - Storey JD, Taylor JE, Siegmund D, Strong Control, Conservative Point Estimation and Simultaneous Conservative Consistency of False Discovery Rates: A Unified Approach, 2004, [http://www.jstor.org/stable/3647634](https://www.jstor.org/stable/3647634?seq=1#metadata_info_tab_contents).
  - Storey JD, Princeton University, 2011, [preprint](http://genomics.princeton.edu/storeylab/papers/Storey_FDR_2011.pdf)
  - Orr M, Liu P, Nettleton D, An improved method for computing q-values when the distribution of effect sizes is asymmetric, 2014, [doi: 10.1093/bioinformatics/btu432](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC4609005/)
  - NettletonD et al., Estimating the Number of True Null Hypotheses from a Histogram of p Values., 2006, http://www.jstor.org/stable/27595607.
  - Benjamini Y, Hochberg Y, On the Adaptive Control of the False Discovery Rate in Multiple Testing With Independent Statistics, 2000, [doi:10.3102/10769986025001060](https://journals.sagepub.com/doi/10.3102/10769986025001060)

*)

