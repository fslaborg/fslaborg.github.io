#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "../globals.fsx"

open Markdig
open Globals
open System.IO

type TutorialCategory =
    | FSharp
    | Datascience
    | Advanced
    | Hidden

    static member ofString (str:string) =
        match str.ToLower() with
        | "fsharp" -> FSharp
        | "datascience" -> Datascience
        | "advanced" -> Advanced
        | "hidden" -> Hidden
        | _ -> failwithf "Tutorial category %s does not exist" str

    static member toString (tc:TutorialCategory) = 
        match tc with
        | FSharp        -> "fsharp"
        | Datascience   -> "datascience"
        | Advanced      -> "advanced"
        | Hidden        -> "hidden"


type Tutorial = {
    OriginalFileName:string
    LinkPath: string
    Title:string
    Authors: string
    Category: TutorialCategory
    Index: int
} with
    static member create og path title authors category index = 
        {
            OriginalFileName = og
            LinkPath = path
            Title = title
            Authors = authors
            Category = category
            Index = index
        }

type TutorialContent = {
    Path: string
    FSharpContent: Tutorial []
    DatascienceContent: Tutorial []
    AdvancedContent: Tutorial []
} with 
    static member create path fscontent dscontent advcontent =
        {
            Path = path
            FSharpContent =  fscontent
            DatascienceContent = dscontent
            AdvancedContent = advcontent
        } 


let loadTutorialFile (tutorialFilePath:string) =

    try

        let text = System.IO.File.ReadAllText tutorialFilePath

        let config = ScriptProcessing.getFrontMatter text

        let title =  config |> Map.find "title"
        let authors = config |> Map.find "authors"
        let category = config |> Map.find "category" |> TutorialCategory.ofString
        let index = config |> Map.find "index" |> int
        let link = sprintf "content/tutorials/%s.html" (Path.GetFileNameWithoutExtension(tutorialFilePath))

        printfn "[Tutorial-Loader]: (%s) Registered file %s for " (TutorialCategory.toString category) tutorialFilePath
        
        Some (Tutorial.create (Path.GetFileName(tutorialFilePath)) link title authors category index)

    with e as exn ->
        printfn "[Tutorial-Loader]: ERROR loading tutorial file %s:" tutorialFilePath
        printfn "[Tutorial-Loader]: EXN: %s:" e.Message
        None

let contentDir = "content/tutorials_src"

let loader (projectRoot: string) (siteContent: SiteContents) =

    let tutorialsPath = System.IO.Path.Combine(projectRoot, contentDir)

    let tutorialFiles = 
        Directory.GetFiles(tutorialsPath)
        |> Array.filter Predicates.isTutorialFile
        |> Array.choose loadTutorialFile

    let content =
        TutorialContent.create
            tutorialsPath
            (tutorialFiles |> Array.filter (fun t -> t.Category = FSharp )) 
            (tutorialFiles |> Array.filter (fun t -> t.Category = Datascience )) 
            (tutorialFiles |> Array.filter (fun t -> t.Category = Advanced )) 

    try 
        printfn "[Tutorial-Loader]: Adding tutorial content for tutorial root path at %s" tutorialsPath
        siteContent.Add content
    with _ -> 
        siteContent.AddError {Path = tutorialsPath; Message = (sprintf "Uable to load base tutorial path %s" tutorialsPath); Phase = Loading}

    printfn "[Tutorial-Loader]: Done loading base tutorial path"
    siteContent
