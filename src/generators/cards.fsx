#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let getProcessedCardBody (card:Cardloader.MainPageCard) =
    card.CardBody
        .Replace("<strong>",(sprintf "<strong class='has-bg-one-fourth-%s'>" card.CardEmphasisColor))
        .Replace("<h3>","<h3 class='main-subtitle'>")

let splitPrimaryContent (s:string) =
    let header,columns =
       s.Split("<!---C1-->").[0],s.Split("<!---C1-->").[1]
    let col1,col2 =
        columns.Split("<!---C2-->").[0],columns.Split("<!---C2-->").[1]
    header,col1,col2

let renderPrimaryCard (card:Cardloader.MainPageCard) =

    let header, c1, c2 =
        card
        |> getProcessedCardBody
        |> splitPrimaryContent

    div [Class (sprintf "main-Container has-bg-three-fourths-%s" card.CardEmphasisColor)] [
        div [Class (sprintf "main-TextField has-bg-%s" card.CardColor)] [
            h2 [Class (sprintf "main-title has-bg-%s" card.CardEmphasisColor )] [!! card.CardTitle]
            div [Class "container"] [
                div [ ] [
                    div [Id "carousel-demo"; Class "carousel"; HtmlProperties.Style [] ] [
                        div [Class "item-1"] [
                            figure [Class "image is-3y1 has-ratio"] [
                                img [Src "https://picsum.photos/1500/500?grayscale"]
                            ]
                        ]
                        div [Class "item-2" ] [
                            figure [Class "image is-3by1 has-ratio"] [
                                img [Src "https://picsum.photos/1500/500?grayscale"]
                            ]
                        ]
                        div [Class "item-3" ] [
                            figure [Class "image is-3by1 has-ratio"] [
                                img [Src "https://picsum.photos/1500/500?grayscale"]
                            ]
                        ]
                        div [Class "item-3" ] [
                            figure [Class "image is-3by1 has-ratio"] [
                                img [Src "https://picsum.photos/1500/500?grayscale"]
                            ]
                        ]
                        div [Class "item-3" ] [
                            figure [Class "image is-3by1 has-ratio"] [
                                img [Src "https://picsum.photos/1500/500?grayscale"]
                            ]
                        ]
                        div [Class "item-3" ] [
                            figure [Class "image is-3by1 has-ratio"] [
                                img [Src "https://picsum.photos/1500/500?grayscale"]
                            ]
                        ]
                    ]
                ]
            ]
            div [Class "main-text"] [
                !! header
                div [Class "columns"] [
                    div [Class "column"] [!! c1]
                    div [Class "column"] [!! c2]
                ]
            ]
        ]
    ]
    

let renderSecondaryCard isLeft (card:Cardloader.MainPageCard) = 
    if isLeft then 
        div [Class (sprintf "main-Container has-bg-three-fourths-%s" card.CardBGColor)] [
            div [Class "columns is-reverse-columns"] [
                div [Class "column"] [
                    div [Class (sprintf "main-TextField has-bg-%s" card.CardColor)] [
                        h2 [Class (sprintf "main-title is-emphasized-%s" card.CardEmphasisColor )] [!! card.CardTitle]
                        div [Class "main-text"] [
                            !! (getProcessedCardBody card)
                        ]
                    ]
                ]
                div [Class "column"] [
                    div [Class "main-ImageContainer"] [
                        a [Href "https://github.com/fslaborg"; Target "_blank"] [
                            figure [Class "image"] [
                                img [Src (Layout.urlPrefix + card.CardImages.[0])]
                            ]
                        ]
                    ]
                
                ]
            ]
        ]
    else
        div [Class (sprintf "main-Container has-bg-three-fourths-%s" card.CardBGColor)] [
            div [Class "columns"] [
                div [Class "column"] [
                    div [Class "main-ImageContainer"] [
                        a [Href "https://github.com/fslaborg"; Target "_blank"] [
                            figure [Class "image"] [
                                img [Src (Layout.urlPrefix + card.CardImages.[0])]
                            ]
                        ]
                    ]
                ]
                div [Class "column"] [
                    div [Class (sprintf "main-TextField has-bg-%s" card.CardColor)] [
                        h2 [Class (sprintf "main-title is-emphasized-%s" card.CardEmphasisColor )] [!! card.CardTitle]
                        div [Class "main-text"] [
                            !! (getProcessedCardBody card)
                        ]
                    ]
                
                ]
            ]
        ]
    
    
let generate' (ctx : SiteContents) (_: string) =
    
    let cards : Cardloader.MainPageCard list = 
        ctx.TryGetValues<Cardloader.MainPageCard>()
        |> Option.defaultValue Seq.empty
        |> Seq.toList
        
    Layout.layout ctx "Home" [
        div [] (
            cards
            |> List.sortBy (fun c -> c.CardIndex)
            |> List.mapi (fun i card ->
                let isLeft = (i+1)%2=0
                match card.CardType with
                | Cardloader.CardType.Main ->
                    renderPrimaryCard card
                | Cardloader.CardType.Secondary ->
                    renderSecondaryCard isLeft card
            )
        )
    ]


let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    printfn "[Cards-Generator] Starting generate function ..."
    generate' ctx page
    |> Layout.render ctx