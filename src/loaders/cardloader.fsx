#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "../globals.fsx"

open Markdig
open Globals

type CardType =
    | Main
    | Secondary

    static member ofString (str:string) =
        match str with
        |"main"|"Main" -> Main
        |"secondary"| "Secondary" -> Secondary
        | _ -> failwithf "%s is not a valid card type" str 
        
type MainPageCard = {
    CardTitle:string
    CardBody:string
    CardColor:string
    CardBGColor:string
    CardEmphasisColor:string
    CardImages:string []
    CardIndex: int
    CardType:CardType
} with
    static member create title body color bgcolor emphasisColor images index ctype = 
        {
            CardTitle = title
            CardBody = body
            CardColor = color
            CardBGColor = bgcolor
            CardEmphasisColor = emphasisColor 
            CardImages = images
            CardIndex = index
            CardType = ctype
        }

let contentDir = "cards"

let trimString (str : string) =
    str.Trim().TrimEnd('"').TrimStart('"')

let loadFile (cardMarkdownPath:string) =
    let text = System.IO.File.ReadAllText cardMarkdownPath

    let config = MarkdownProcessing.getFrontMatter text
    let content = MarkdownProcessing.getMarkdownContent text

    let title = config |> Map.find "title" |> trimString
    let body = content
    let color = config |> Map.find "color" |> trimString
    let bgcolor = config |> Map.find "bg-color" |> trimString
    let emphasisColor = config |> Map.find "emphasis" |> trimString
    let images = config |> Map.find "image" |> trimString |> fun s -> s.Split(',')
    let index = config |> Map.find "index" |> trimString |> int
    let ctype = config |> Map.find "type" |> trimString |> CardType.ofString

    printfn "%A" title

    MainPageCard.create title body color bgcolor emphasisColor images index ctype

let loader (projectRoot: string) (siteContent: SiteContents) =
    let cardsPath = System.IO.Path.Combine(projectRoot, contentDir)
    System.IO.Directory.GetFiles cardsPath
    |> Array.filter (fun n -> n.EndsWith ".md")
    |> Array.map loadFile
    |> Array.iter (fun card -> 
        printfn "%A" card.CardTitle
        siteContent.Add(card))

    siteContent