#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open System
open Html
open System.IO
open System.Diagnostics

#if WATCH
let urlPrefix = 
  ""
#else
let urlPrefix = 
  "/fslabsite"
#endif

let generate' (ctx : SiteContents) (_: string) =
    
    let tutorials : Tutorialloader.TutorialDirectory = 
        ctx.TryGetValue<Tutorialloader.TutorialDirectory>()
        |> Option.defaultValue {Path="";Content=[||]}

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
            div [Class "container"] [
                div [Class "content"] [
                    ul [] (
                        tutorials.Content
                        |> Seq.map (fun t ->
                            li [] [a [Href (urlPrefix + "/" + t)] [!!t]]
                        )
                        |> List.ofSeq
                    )
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
     
    printfn "[Tutorials-Generator] Starting generate function ..."

    let tutorials : Tutorialloader.TutorialDirectory = 
        ctx.TryGetValue<Tutorialloader.TutorialDirectory>()
        |> Option.defaultValue {Path="";Content=[||]}
        
    if tutorials.Path <> "" && tutorials.Path.Contains("tutorials_src") then

        let args = 
            sprintf 
                "fsdocs build --eval --input %s --output %s --noapidocs --parameters root %s" 
                tutorials.Path 
                (tutorials.Path.Replace("_src","")) 
                (urlPrefix + "/content/tutorials/")


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

