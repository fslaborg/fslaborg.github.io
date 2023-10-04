module Pages.DataSciencePackages

open Feliz
open Feliz.Bulma

open Literals.DataSciencePackages
open Component.ContentHelpers

let fsharpCodeBlock (rawCodeString: string) =
    Html.pre [Html.code [
        prop.className "language-fsharp"
        prop.text rawCodeString
    ]]

let private banner() =
    let logoTitles: Fable.React.ReactElement =
        Bulma.content [
            rowField [
                Bulma.image [
                    Bulma.image.is128x128
                    prop.children [
                        Html.img [
                            prop.className "is-rounded has-background-white";
                            prop.src Literals.Images.DataSciencePackages
                        ]
                    ]
                ]
                Html.div [
                    prop.style [style.fontSize (length.rem 3)]
                    prop.text "Data science packages" 
                ]
            ]
            Html.div [
                prop.className "has-text-primaryd is-size-5"; 
                prop.children [
                    Html.p "Use these packages to fuel your data science journey in F# and .NET! 🚀" 
                    Html.p [ 
                        Html.span "Want to add a package to the curated list? " 
                        Html.a [
                            prop.children [
                                Html.span "File a PR"
                                Bulma.icon [Html.i [prop.className "fa-solid fa-code-branch"]]
                            ]
                        ]
                    ]
                ]
            ]
            
        ]
    bannerContainer "primaryl" [
        logoTitles
    ]

[<RequireQualifiedAccess>]
module private FsCards =
    let FSharpStats() = 
        let content =
            Bulma.content [
                Html.p "FSharp.Stats is a multipurpose project for statistical testing, linear algebra, machine learning, fitting and signal processing."
                Html.p "Here is a simple basic example for getting general statistics of a sequence of numbers sampled from a normal distribution:"
                fsharpCodeBlock """#r "nuget: FSharp.Stats"
open FSharp.Stats

// initialize a normal distribution with mean 25 and standard deviation 0.1
let normalDistribution = Distributions.Continuous.normal 25. 0.1

// draw independently 30 times from the given distribution 
let sample = Array.init 30 (fun _ -> normalDistribution.Sample())

let mean = Seq.mean sample
let stDev = Seq.stDev sample
let cv = Seq.cv sample"""
            ]
        Component.DataSciencePackageCard.Main(
            "FSharp.Stats",
            "Statistical testing, linear algebra, machine learning, fitting and signal processing in F#.",
            FSharpStats_URLS,
            content
        ) 

    let CyjsNET() =
        let content = Bulma.content [
            Html.p "This package provides a light-weighted layer on top of the famous Cytoscape.js library. It brings all the graph visualization capabilities directly into .NET."
            Html.p "Here is a small snippet that creates a basic styled graph:"
            fsharpCodeBlock """#r "nuget: Cyjs.NET"

open Cyjs.NET
open Elements

let myFirstStyledGraph =     
    CyGraph.initEmpty ()
    |> CyGraph.withElements [
            node "n1" [ CyParam.label "FsLab"  ]
            node "n2" [ CyParam.label "ML" ]
 
            edge  "e1" "n1" "n2" []
        ]
    |> CyGraph.withStyle "node"     
            [
                CyParam.content =. CyParam.label
                CyParam.color "#A00975"
            ]
    |> CyGraph.withSize(800, 400)  
    |> CyGraph.show // displays the graph in a browser"""
        ]
        Component.DataSciencePackageCard.Main("Cyjs.NET",".NET interface for Cytoscape.js written in F# for graph visualization.",Literals.DataSciencePackages.CyjsNET_URLS,content)

    let flips() =
        let content = Bulma.content [
            Html.p [
                Html.span "Flips is an F# library for modeling and solving Linear Programming (LP) and Mixed-Integer Programming (MIP) problems. It is inspired by the work of the PuLP library for Python and the excellent Gurobi Python library. It builds on the work of the outstanding "
                Html.a [prop.src "https://github.com/google/or-tools"; prop.target "_blank"; prop.text "Google OR-Tools library"]
                Html.span " and the "
                Html.a [prop.src "https://optano.com/en/modeling/"; prop.target "_blank"; prop.text "OPTANO"]
                Html.span " library."
            ]
        ]
        Component.DataSciencePackageCard.Main("flips", "Fsharp LInear Programming System.", Literals.DataSciencePackages.flips_URLS, content)

    let PlotlyNet() =
        let content = Bulma.content [
            Html.p "Plotly.NET provides functions for generating and rendering plotly.js charts in .NET programming languages 📈🚀."
            Html.p "It can be used to create plotly.js charts in the following environments:"
            Html.ol [
                Html.li "interactive charts in html pages"
                Html.li [
                    Html.span "interactive charts in dotnet interactive notebooks via the "
                    Html.a [prop.src "https://www.nuget.org/packages/Plotly.NET.Interactive/"; prop.target "_blank"; prop.text "Plotly.NET.Interactive"]
                    Html.span " package."
                ]
                Html.li [
                    Html.span "static chart images via "
                    Html.a [prop.src "https://www.nuget.org/packages/Plotly.NET.ImageExport/"; prop.target "_blank"; prop.text "Plotly.NET.ImageExport"]
                    Html.span " package."
                ]
            ]
            Html.p "Here is a basic example snippet that renders a simple point chart, either as html page or static image:"
            fsharpCodeBlock """#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: Plotly.NET.ImageExport, 2.0.0-preview.16"

open Plotly.NET

let myChart = 
    Chart.Point(
        [
            (1.,2.)
            (2.,3.)
            (3.,4.)
            (5.,2.)
        ]
    )
    |> Chart.withTitle "Hello from Plotly.NET!"
    |> Chart.withX_AxisStyle ("X-Axis",Showline=true,Showgrid=true)
    |> Chart.withY_AxisStyle ("Y-Axis",Showline=true,Showgrid=true)

myChart |> Chart.Show //display as html in browser 

//using tatic image export
open Plotly.NET.ImageExport

//save chart as static png with 600x600 px
myChart |> Chart.savePNG("myChart",Width=600,Height=600)  
"""
        ]
        Component.DataSciencePackageCard.Main("Plotly.NET", "Plotly.NET provides functions for generating and rendering plotly.js charts in .NET programming languages 📈🚀.", Literals.DataSciencePackages.PlotlyNET_URLS, content)

    let RProvider() =
        let content = Bulma.content [
            Html.p "The R Provider discovers R packages that are available in your R installation and makes them available as .NET namespaces underneath the parent namespace RProvider."
            Html.p "For example, the stats package is available as RProvider.stats. If you open the namespaces you want to use, functions and values will be available as R.name."
            Html.p "There are three requirements to be set up on your system:"
            Html.ol [
                Html.li (Html.b ".NET SDK 5.0 or greater")
                Html.li [
                    Html.b "R statistical language. "
                    Html.i "Note: on Windows, there is currently a bug in R preventing us from supporting R versions greater than 4.0.2."
                ]
                Html.li [
                    Html.b "R_HOME environment variable set to the R home directory."
                    Html.span "This can usually be identified by running the command 'R RHOME'."
                ]
            ]
            Html.p "Below is a simple script example that demonstrates using R statistical functions, and using R graphics functions to create charts:"
            fsharpCodeBlock """#r "nuget: RProvider,2.0.1"

open RProvider
open RProvider.graphics
open RProvider.grDevices
open RProvider.datasets

// use R to calculate the mean of a list
R.mean([1;2;3;4])

// Calculate sin using the R 'sin' function
// (converting results to 'float') and plot it
[ for x in 0.0 .. 0.1 .. 3.14 -> 
    R.sin(x).GetValue<float>() ]
|> R.plot

// Plot the data from the standard 'Nile' data set
R.plot(R.Nile)"""
        ]
        let description = "An F# Type Provider providing strongly typed access to the R statistical language. The type provider automatically discovers available R packages and makes them easily accessible from F#, so you can easily call powerful packages and visualization libraries from code running on the .NET platform."
        Component.DataSciencePackageCard.Main(
            "RProvider", 
            description, 
            Literals.DataSciencePackages.RProvider_URLS,
            content)

    let Deedle() =
        let content = Bulma.content [
            Html.p "Deedle implements efficient and robust frame and series data structures for accessing and manipulating structured data."
            Html.p "It supports handling of missing values, aggregations, grouping, joining, statistical functions and more. For frames and series with ordered indices (such as time series), automatic alignment is also available."
            Html.p [
                Html.span "Here is a short snippet on how to read and manipulate an online data source (HTTP requests are done with "
                Html.a [prop.href Literals.DataSciencePackages.FSharpData_URLS.GitHub; prop.target "_blank"; prop.text "FSharp.Data"]
                Html.span ")."
            ]
            Html.p "It reads the boston housing data set csv file from an online data source."
            fsharpCodeBlock """#r "nuget: FSharp.Data"
#r "nuget: Deedle"

open FSharp.Data
open Deedle

let rawData = Http.RequestString @"https://raw.githubusercontent.com/dotnet/machinelearning/master/test/data/housing.txt"

// get a frame containing the values of houses at the charles river only
let df = 
    Frame.ReadCsvString(rawData, separators="\t")
    |> Frame.sliceCols ["MedianHomeValue"; "CharlesRiver"]
    |> Frame.filterRowValues (fun s -> s.GetAs<bool>("CharlesRiver"))

df.Print()"""
        ]
        let description = "Deedle is an easy to use library for data and time series manipulation and for scientific programming."
        Component.DataSciencePackageCard.Main("Deedle", description, Literals.DataSciencePackages.Deedle_URLS, content)

    let FSharpData() =
        let content = Bulma.content [
            Html.p "FSharp.Data is a multipurpose project for data access from many different file formats. Most of this is done via type providers."
            Html.p [
                Html.span "We recommend using the "
                Html.code "Http"
                Html.span " module provided by FSharp.data to download data sources via Http and then convert them to deedle data frames via "
                Html.code "Frame.ReadCsvString"
                Html.span ":"
            ]
            fsharpCodeBlock """#r "nuget: FSharp.Data"
#r "nuget: Deedle"

open FSharp.Data
open Deedle

let rawData = Http.RequestString @"https://raw.githubusercontent.com/dotnet/machinelearning/master/test/data/housing.txt"

// get a frame containing the values of houses at the charles river only
let df = 
    Frame.ReadCsvString(rawData, separators="\t")
    |> Frame.sliceCols ["MedianHomeValue"; "CharlesRiver"]
    |> Frame.filterRowValues (fun s -> s.GetAs<bool>("CharlesRiver"))

df.Print()"""
        ]
        let description = "The FSharp.Data package contains type providers and utilities to access common data formats (CSV, HTML, JSON and XML in your F# applications and scripts. It also contains helpers for parsing CSV, HTML and JSON files and for sending HTTP requests."
        Component.DataSciencePackageCard.Main("FSharp.Data",description, Literals.DataSciencePackages.FSharpData_URLS, content)

let Main() =
    Html.div [
        prop.style [style.backgroundColor "var(--scheme-main)"]
        prop.children [
            banner()
            Bulma.container [
                FsCards.CyjsNET()
                FsCards.FSharpStats()
                FsCards.flips()
                FsCards.PlotlyNet()
                FsCards.RProvider()
                FsCards.Deedle()
                FsCards.FSharpData()
            ]
        ]
    ]