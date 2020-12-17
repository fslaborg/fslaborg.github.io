#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"

open Markdig

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
    CardEmphasisColor:string
    CardImages:string []
    CardIndex: int
    CardType:CardType
} with
    static member create title body color emphasisColor images index ctype = 
        {
            CardTitle = title
            CardBody = body
            CardColor = color
            CardEmphasisColor = emphasisColor 
            CardImages = images
            CardIndex = index
            CardType = ctype
        }

let contentDir = "cards"

let isSeparator (input : string) =
    input.StartsWith "---"

let markdownPipeline =
    MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseGridTables()
        .UseGenericAttributes()
        .UseEmphasisExtras()
        .UseListExtras()
        .UseCitations()
        .UseCustomContainers()
        .UseFigures()
        .Build()

///`fileContent` - content of page to parse. Usually whole content of `.md` file
///returns content of config that should be used for the page
let getCardConfig (fileContent : string) =
    let fileContent = 
        fileContent.Split '\n'
        |> Array.skip 1 //First line must be ---

    let indexOfSeperator = fileContent |> Array.findIndex isSeparator

    let splitKey (line: string) = 
        let seperatorIndex = line.IndexOf(':')
        if seperatorIndex > 0 then
            let key = line.[.. seperatorIndex - 1].Trim().ToLower()
            let value = line.[seperatorIndex + 1 ..].Trim() 
            Some(key, value)
        else 
            None

    fileContent
    |> Array.splitAt indexOfSeperator
    |> fst
    |> Seq.choose splitKey
    |> Map.ofSeq        

///`fileContent` - content of page to parse. Usually whole content of `.md` file
///returns HTML version of content of the page
let getCardContent (fileContent : string) =
    let fileContent = fileContent.Split '\n'
    let fileContent = fileContent |> Array.skip 1 //First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex isSeparator
    let content = 
        fileContent 
        |> Array.splitAt indexOfSeperator
        |> snd
        |> Array.skip 1 
        |> String.concat "\n"

    Markdown.ToHtml(content, markdownPipeline)

let trimString (str : string) =
    str.Trim().TrimEnd('"').TrimStart('"')

let loadFile (cardMarkdownPath:string) =
    let text = System.IO.File.ReadAllText cardMarkdownPath

    let config = getCardConfig text
    let content = getCardContent text

    let title = config |> Map.find "title" |> trimString
    let body = content
    let color = config |> Map.find "color" |> trimString
    let emphasisColor = config |> Map.find "emphasis" |> trimString
    let images = config |> Map.find "image" |> trimString |> fun s -> s.Split(',')
    let index = config |> Map.find "index" |> trimString |> int
    let ctype = config |> Map.find "type" |> trimString |> CardType.ofString

    printfn "%A" title

    MainPageCard.create title body color emphasisColor images index ctype

let loader (projectRoot: string) (siteContent: SiteContents) =
    let cardsPath = System.IO.Path.Combine(projectRoot, contentDir)
    System.IO.Directory.GetFiles cardsPath
    |> Array.filter (fun n -> n.EndsWith ".md")
    |> Array.map loadFile
    |> Array.iter (fun card -> 
        printfn "%A" card.CardTitle
        siteContent.Add(card))

    siteContent