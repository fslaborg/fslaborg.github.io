#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "../globals.fsx"

open Markdig
open Globals

type TutorialDirectory = {
    Path: string
}

let contentDir = "content/tutorials_src"

let loader (projectRoot: string) (siteContent: SiteContents) =
    let tutorialsPath = System.IO.Path.Combine(projectRoot, contentDir)
    try 
        printfn "[Tutorial-Loader]: Adding base tutorial path at %s" tutorialsPath
        siteContent.Add {Path = tutorialsPath}
    with _ -> 
        siteContent.AddError {Path = tutorialsPath; Message = (sprintf "Uable to load base tutorial path %s" tutorialsPath); Phase = Loading}

    printfn "[Tutorial-Loader]: Done loading base tutorial path"
    siteContent
