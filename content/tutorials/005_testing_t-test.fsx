(**
[![Binder](https://fslab.org/images/badge-binder.svg)](https://mybinder.org/v2/gh/fslaborg/fslaborg.github.io/gh-pages?filepath=content/tutorials/005_testing_t-test.ipynb)&emsp;
[![Script](https://fslab.org/images/badge-script.svg)](https://fslab.org/content/tutorials/005_testing_t-test.fsx)&emsp;
[![Notebook](https://fslab.org/images/badge-notebook.svg)](https://fslab.org/content/tutorials/005_testing_t-test.ipynb)

# Testing with FSharp.Stats I: t-test

## Getting started: The t-test

_I love statistical testing_ - A sentence math teachers don't hear often during their time at school. In this tutorial we aim to give you a short introduction of the theory and how to 
perform the most used statistical test: the t-test

Suppose you have measured the length of some leaves of two trees and you want to find out if the average length of the leaves is the same or if they differ from each other. 
If you knew the population distributions of all leaves hanging on both trees the task would be easy, but since we only have samples from both populations, we have to apply a statistical test.
Student's t-test can be applied to test whether two samples have the same mean (H0), or if the means are different (H1). There are two requirements to the samples that have to be fulfilled:

1. The variances of both samples have to be equal.

2. The samples have to follow a normal distribution.

_Note: Slight deviations from these requirements can be accepted but strong violations result in an inflated false positive rate. If the variances are not equal a Welch test can be performed._
_There are some tests out there to check if the variances are equal or if the sample follows a normal distribution, but their effectiveness is discussed._
_You always should consider the shape of the theoretical background distribution, instead of relying on preliminary tests rashly._


The t-test is one of the most used statistical tests in datascience. It is used to compare two samples in terms of statistical significance. 
Often a significance threshold (or &alpha; level) of 0.05 is chosen to define if a p value is defined as statistically significant. A p value describes how likely it is to observe an effect
at least as extreme as you observed (in the comparison) by chance. Low p values indicate a high confidence to state that there is a real difference and the observed difference is not caused by chance.


*)
#r "nuget: FSharp.Data"
#r "nuget: Deedle"
#r "nuget: FSharp.Stats, 0.4.2"
#r "nuget: Plotly.NET, 2.0.0-preview.12"

open FSharp.Data
open Deedle
open Plotly.NET
(**
For our purposes, we will use the housefly wing length dataset (from _Sokal et al., 1955, A morphometric analysis of DDT-resistant and non-resistant housefly strains_).
Head over to the [Getting started](001_getting-started.html#Data-access) tutorial where it is shown how to import datasets in a simple way.



*)
// We retrieve the dataset via FSharp.Data:
let rawDataHousefly = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/HouseflyWingLength.txt"

let dataHousefly : seq<float> = 
    Frame.ReadCsvString(rawDataHousefly, false, schema = "wing length (mm * 10^1)")
    |> Frame.getCol "wing length (mm * 10^1)"
    |> Series.values
    // We convert the values to mm
    |> Seq.map (fun x -> x / 10.)
(**
Let us first have a look at the sample data with help of a boxplot. As shown below, the average wingspan is around 4.5 with variability ranges between 3.5 and 5.5.



*)
let boxPlot = 
    Chart.BoxPlot(y = dataHousefly, Name = "housefly", BoxPoints = StyleParam.BoxPoints.All, Jitter = 0.2)
    |> Chart.withYAxisStyle "wing length [mm]"(* output: 
<div id="5e4b7e40-213d-4599-ab72-7c8374b8adc7" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_5e4b7e40213d4599ab727c8374b8adc7 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"box","y":[3.6,3.7,3.8,3.8,3.9,3.9,4.0,4.0,4.0,4.0,4.1,4.1,4.1,4.1,4.1,4.1,4.2,4.2,4.2,4.2,4.2,4.2,4.2,4.3,4.3,4.3,4.3,4.3,4.3,4.3,4.3,4.4,4.4,4.4,4.4,4.4,4.4,4.4,4.4,4.4,4.5,4.5,4.5,4.5,4.5,4.5,4.5,4.5,4.5,4.5,4.6,4.6,4.6,4.6,4.6,4.6,4.6,4.6,4.6,4.6,4.7,4.7,4.7,4.7,4.7,4.7,4.7,4.7,4.7,4.8,4.8,4.8,4.8,4.8,4.8,4.8,4.8,4.9,4.9,4.9,4.9,4.9,4.9,4.9,5.0,5.0,5.0,5.0,5.0,5.0,5.1,5.1,5.1,5.1,5.2,5.2,5.3,5.3,5.4,5.5],"boxpoints":"all","jitter":0.2,"name":"housefly","marker":{}}];
            var layout = {"yaxis":{"title":{"text":"wing length [mm]"}}};
            var config = {};
            Plotly.newPlot('5e4b7e40-213d-4599-ab72-7c8374b8adc7', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_5e4b7e40213d4599ab727c8374b8adc7();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_5e4b7e40213d4599ab727c8374b8adc7();
            }
</script>
*)
(**
## One-sample t-test

We want to analyze if an estimated expected value differs from the sample above. Therefore, we perform a one-sample t-test which covers exactly this situation.



<img style="max-width:75%" src="../../images/OneSampleTTest.png"></img>

Fig. 1: **The one-sample t-test** The dashed orange line depicts the distribution of our sample, the green bar the expected value to test against.


*)
open FSharp.Stats
open FSharp.Stats.Testing

// The testing module in FSharp.Stats require vectors as input types, thus we transform our array into a vector:
let vectorDataHousefly = vector dataHousefly

// The expected value of our population.
let expectedValue = 4.5

// Perform the one-sample t-test with our vectorized data and our exptected value as parameters.
let oneSampleResult = TTest.oneSample vectorDataHousefly expectedValue(* output: 
{ Statistic = 1.275624919
  DegreesOfFreedom = 99.0
  PValueLeft = 0.8974634108
  PValueRight = 0.1025365892
  PValue = 0.2050731784 }*)
(**
The function returns a `TTestStatistics` type. If contains the fields 

  - `Statistic`: defines the exact teststatistic

  - `DegreesOfFreedom`: defines the degrees of freedom

  - `PValueLeft`: the left-tailed p-value 

  - `PValueRight`: the right-tailed p-value

  - `PValue`: the two-tailed p-value

As we can see, when looking at the two-tailed p-value, our sample does _not_ differ significantly from our expected value. This matches our visual impression of the boxplot, where the sample distribution 
is centered around 4.5.


## Two-sample t-test (unpaired data)

The t-test is most often used in its two-sample variant. Here, two samples, independent from each other, are compared. It is required that both samples are normally distributed.
In this next example, we are going to see if the gender of college athletes determines the number of concussions suffered over 3 years (from: _Covassin et al., 2003, Sex Differences and the Incidence of Concussions Among Collegiate Athletes, Journal of Athletic Training_).


<img style="max-width:75%" src="../../images/TwoSampleTTest.png"></img>

Fig. 2: **The two-sample t-test** The dashed orange and green lines depict the distribution of both samples that are compared with each other.


*)
open System.Text

let rawDataAthletes = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/ConcussionsInMaleAndFemaleCollegeAthletes_adapted.tsv"

let dataAthletesAsStream = new System.IO.MemoryStream(rawDataAthletes |> Encoding.UTF8.GetBytes)

// The schema helps us setting column keys.
let dataAthletesAsFrame = Frame.ReadCsv(dataAthletesAsStream, hasHeaders = false, separators = "\t", schema = "Gender, Sports, Year, Concussion, Count")

dataAthletesAsFrame.Print()

// We need to filter out the columns and rows we don't need. Thus, we filter out the rows where the athletes suffered no concussions  
// as well as filter out the columns without the number of concussions.
let dataAthletesFemale, dataAthletesMale =
    let getAthleteGenderData gender =
        let dataAthletesOnlyConcussion =
            dataAthletesAsFrame
            |> Frame.filterRows (fun r objS -> objS.GetAs "Concussion")
        let dataAthletesGenderFrame =
            dataAthletesOnlyConcussion
            |> Frame.filterRows (fun r objS -> objS.GetAs "Gender" = gender)
        dataAthletesGenderFrame
        |> Frame.getCol "Count" 
        |> Series.values
        |> vector
    getAthleteGenderData "Female", getAthleteGenderData "Male"
    
(**
Again, let's check our data via boxplots before we proceed on comparing them.


*)
let boxPlot2 = 
    [
        Chart.BoxPlot(y = dataAthletesFemale, Name = "female college athletes", BoxPoints = StyleParam.BoxPoints.All, Jitter = 0.2)
        Chart.BoxPlot(y = dataAthletesMale, Name = "male college athletes", BoxPoints = StyleParam.BoxPoints.All, Jitter = 0.2)
    ]
    |> Chart.combine
    |> Chart.withYAxisStyle "number of concussions over 3 years"(* output: 
<div id="b686fa27-a205-402b-b3dd-ab890d9dbdc0" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_b686fa27a205402bb3ddab890d9dbdc0 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"box","y":[51.0,47.0,60.0,12.0,7.0,7.0,16.0,30.0,26.0,9.0,10.0,28.0,1.0,0.0,0.0],"boxpoints":"all","jitter":0.2,"name":"female college athletes","marker":{}},{"type":"box","y":[34.0,27.0,40.0,19.0,15.0,17.0,8.0,21.0,20.0,22.0,6.0,25.0,0.0,0.0,0.0],"boxpoints":"all","jitter":0.2,"name":"male college athletes","marker":{}}];
            var layout = {"yaxis":{"title":{"text":"number of concussions over 3 years"}}};
            var config = {};
            Plotly.newPlot('b686fa27-a205-402b-b3dd-ab890d9dbdc0', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_b686fa27a205402bb3ddab890d9dbdc0();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_b686fa27a205402bb3ddab890d9dbdc0();
            }
</script>
*)
(**
Both samples are tested against using `FSharp.Stats.Testing.TTest.twoSample` and assuming equal variances.


*)
// We test both samples against each other, assuming equal variances.
let twoSampleResult = TTest.twoSample true dataAthletesFemale dataAthletesMale(* output: 
{ Statistic = 0.5616104016
  DegreesOfFreedom = 28.0
  PValueLeft = 0.7105752703
  PValueRight = 0.2894247297
  PValue = 0.5788494593 }*)
(**
With a p value of 0.58 the t-test indicate that there's no significant difference between the number of concussions over 3 years between male and female college athletes.


## Two-sample t-test (paired data)

Paired data describes data where each value from the one sample is connected with its respective value from the other sample.  
In the next case, the endurance performance of several persons in a normal situation (control situation) is compared to their performance after ingesting a specific amount of caffeine*. 
It is the same person that performs the exercise but under different conditions. Thus, the resulting values of the persons under each condition are compared.  
Another example are time-dependent experiments: One measures, e.g., the condition of cells stressed with a high surrounding temperature in the beginning and after 30 minutes. 
The measured cells are always the same, yet their conditions might differ.
Due to the connectivity of the sample pairs the samples must be of equal length.

*Source: W.J. Pasman, M.A. van Baak, A.E. Jeukendrup, A. de Haan (1995). _The Effect of Different Dosages of Caffeine on Endurance Performance Time_, International Journal of Sports Medicine, Vol. 16, pp225-230.


*)
let rawDataCaffeine = Http.RequestString @"https://raw.githubusercontent.com/fslaborg/datasets/main/data/CaffeineAndEndurance(wide)_adapted.tsv"

let dataCaffeineAsStream = new System.IO.MemoryStream(rawDataCaffeine |> Encoding.UTF8.GetBytes)
let dataCaffeineAsFrame = Frame.ReadCsv(dataCaffeineAsStream, hasHeaders = false, separators = "\t", schema = "Subject ID, no Dose, 5 mg, 9 mg, 13 mg")

// We want to compare the subjects' performances under the influence of 13 mg caffeine and in the control situation.
let dataCaffeineNoDose, dataCaffeine13mg =
    let getVectorFromCol col = 
        dataCaffeineAsFrame
        |> Frame.getCol col
        |> Series.values
        |> vector
    getVectorFromCol "no Dose", getVectorFromCol "13 mg"

// Transforming our data into a chart.
let visualizePairedData = 
    Seq.zip dataCaffeineNoDose dataCaffeine13mg
    |> Seq.mapi (fun i (control,treatment) -> 
        let participant = "Person " + string i 
        Chart.Line(["no dose", control; "13 mg", treatment], Name = participant)
        )
    |> Chart.combine
    |> Chart.withXAxisStyle ""
    |> Chart.withYAxisStyle("endurance performance", MinMax = (0.,100.))(* output: 
<div id="1ef7316f-a759-48d0-a7ba-3c4fca7e54b0" style="width: 600px; height: 600px;"><!-- Plotly chart will be drawn inside this DIV --></div>
<script type="text/javascript">

            var renderPlotly_1ef7316fa75948d0a7ba3c4fca7e54b0 = function() {
            var fsharpPlotlyRequire = requirejs.config({context:'fsharp-plotly',paths:{plotly:'https://cdn.plot.ly/plotly-2.4.2.min'}}) || require;
            fsharpPlotlyRequire(['plotly'], function(Plotly) {

            var data = [{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[36.05,37.55],"line":{},"name":"Person 0","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[52.47,59.3],"line":{},"name":"Person 1","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[56.55,79.12],"line":{},"name":"Person 2","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[45.2,58.33],"line":{},"name":"Person 3","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[35.25,70.54],"line":{},"name":"Person 4","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[66.38,69.47],"line":{},"name":"Person 5","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[40.57,46.48],"line":{},"name":"Person 6","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[57.15,66.35],"line":{},"name":"Person 7","marker":{}},{"type":"scatter","mode":"lines","x":["no dose","13 mg"],"y":[28.34,36.2],"line":{},"name":"Person 8","marker":{}}];
            var layout = {"xaxis":{"title":{"text":""}},"yaxis":{"title":{"text":"endurance performance"},"range":[0.0,100.0]}};
            var config = {};
            Plotly.newPlot('1ef7316f-a759-48d0-a7ba-3c4fca7e54b0', data, layout, config);
});
            };
            if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) {
                var script = document.createElement("script");
                script.setAttribute("src", "https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js");
                script.onload = function(){
                    renderPlotly_1ef7316fa75948d0a7ba3c4fca7e54b0();
                };
                document.getElementsByTagName("head")[0].appendChild(script);
            }
            else {
                renderPlotly_1ef7316fa75948d0a7ba3c4fca7e54b0();
            }
</script>
*)
(**
The function for pairwise t-tests can be found at `FSharp.Stats.Testing.TTest.twoSamplePaired`. Note, that the order of the elements in each vector must be the same, so that a pairwise comparison can be performed.


*)
let twoSamplePairedResult = TTest.twoSamplePaired dataCaffeineNoDose dataCaffeine13mg(* output: 
{ Statistic = 3.252507672
  DegreesOfFreedom = 8.0
  PValueLeft = 0.9941713794
  PValueRight = 0.005828620625
  PValue = 0.01165724125 }*)
(**
The two-sample paired t-test suggests a significant difference beween caffeine and non-caffeine treatment groups with a p-value of 0.012. 


*)

