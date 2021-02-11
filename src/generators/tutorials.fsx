#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open System
open Html
open System.IO
open System.Diagnostics

#if WATCH
let urlPrefix = 
  "http://localhost:8080/"
#else
let urlPrefix = 
  "/fslabsite/"
#endif

let renderContentTable (content:Tutorialloader.Tutorial []) =
    table [Class "table content-table"] [
        thead [] [
            th [] [!!"Title"]
            th [] [!!"Authors"]
        ]
        tbody [] (
            content
            |> Array.sortBy (fun t -> t.Index)
            |> Array.map (fun t ->
                tr [] [
                    td [] [
                        a [Class "button tutorial-link"; Href t.LinkPath] [!!t.Title]
                    ]
                    td [] [!!t.Authors]
                ]
            )
            |> Array.toList
        )
    ]


let generate' (ctx : SiteContents) (_: string) =
    
    let tutorials : Tutorialloader.TutorialContent = 
        ctx.TryGetValue<Tutorialloader.TutorialContent>()
        |> Option.defaultValue {Path="";FSharpContent = [||]; DatascienceContent = [||]; AdvancedContent = [||]}

    Layout.layout ctx "Tutorials and Blogposts" [
        section [Class "hero is-medium has-bg-darkmagenta"] [
            div [Class "hero-body"] [
                div [Class "container"] [
                    div [Class "media mb-4"] [
                        div [Class "media-left"] [
                            figure [Class "image is-128x128"] [
                                 img [Id "package-header-img"; Class "is-rounded"; Src "images/skills.svg"]
                            ]
                        ]
                        div [Class "media-content"] [
                            h1 [Class "main-title is-white"] [!!"FsLab tutorials and blogposts"]
                        ]
                    ]
                    h3 [Class "subtitle is-white"] [!!"From zero to hero in data science using F#! 🚀"]
                    h3 [Class "subtitle is-white"] [
                        !!"FsLab guides you through downloading and setting up F# for data science. We will support you in learning basics of reading and writing F# syntax and solving problems by examples that work with your own environment."
                    ]
                ]
            ]
        ]
        section [Class "hero is-medium"] [
            div [Class "columns mt-4"] [
                div [Class "column has-text-centered"] [
                    h1 [Class "title is-darkmagenta"] [!!"Getting started with F#"]
                    renderContentTable tutorials.FSharpContent
                ]
                div [Class "column has-text-centered"] [
                    h1 [Class "title is-darkmagenta"] [!!"Getting started with data science in F#"]
                    renderContentTable tutorials.DatascienceContent
                ]
                div [Class "column has-text-centered"] [
                    h1 [Class "title is-darkmagenta"] [!!"Advanced tutorials and blogposts"]
                    renderContentTable tutorials.AdvancedContent
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
     
    printfn "[Tutorials-Generator] Starting generate function ..."

    let tutorials : Tutorialloader.TutorialContent = 
        ctx.TryGetValue<Tutorialloader.TutorialContent>()
        |> Option.defaultValue {Path="";FSharpContent = [||]; DatascienceContent = [||]; AdvancedContent = [||]}
        
    if tutorials.Path <> "" && tutorials.Path.Contains("tutorials_src") then

        let args = 
            sprintf 
                "fsdocs build --eval --input %s --output %s --noapidocs --parameters root %s" 
                tutorials.Path 
                (tutorials.Path.Replace("_src","")) 
                (urlPrefix)

        let psi = ProcessStartInfo()
        psi.FileName <- "dotnet"
        psi.Arguments <- args
        psi.CreateNoWindow <- true
        psi.WindowStyle <- ProcessWindowStyle.Hidden
        psi.UseShellExecute <- true
        try
            printfn "[Tutorials-Generator]: dotnet %s" args
            let proc = Process.Start psi
            proc.WaitForExit()
            printfn  "[Tutorials-Generator]: generated tutorial documents"
            generate' ctx page
            |> Layout.render ctx
        with
        | ex ->
            printfn "Error during fsdocs execution."
            printfn "EX: %s" ex.Message
            ""

    else 
        printfn "Error during fsdocs execution."
        ""

