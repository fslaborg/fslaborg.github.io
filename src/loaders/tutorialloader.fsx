#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "../globals.fsx"

open Markdig
open Globals
open System.IO

type TutorialDirectory = {
    Path: string
    Content: string []
}

let contentDir = "content/tutorials_src"

let loader (projectRoot: string) (siteContent: SiteContents) =
    let tutorialsPath = System.IO.Path.Combine(projectRoot, contentDir)
    let files = 
        Directory.GetFiles(tutorialsPath)
        |> Array.filter Predicates.isTutorialFile
        |> Array.map (fun f -> sprintf "content/tutorials/%s.html" (Path.GetFileNameWithoutExtension(f)))
    try 
        printfn "[Tutorial-Loader]: Adding base tutorial path at %s" tutorialsPath
        siteContent.Add {Path = tutorialsPath; Content=files}
    with _ -> 
        siteContent.AddError {Path = tutorialsPath; Message = (sprintf "Uable to load base tutorial path %s" tutorialsPath); Phase = Loading}

    printfn "[Tutorial-Loader]: Done loading base tutorial path"
    siteContent
