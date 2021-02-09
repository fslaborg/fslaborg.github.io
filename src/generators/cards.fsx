#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open System
open Html

let getProcessedCardBody (card:Cardloader.MainPageCard) =
    card.CardBody
        .Replace("<strong>",(sprintf "<strong class='has-bg-one-fourth-%s'>" card.CardEmphasisColor))
        .Replace("<h3>","<h3 class='main-subtitle'>")


let renderPrimaryCard (card:Cardloader.MainPageCard) =

    let body =
        card
        |> getProcessedCardBody

    section [Class "hero is-medium has-bg-magenta"] [
        div [(*Id "landing-page-hero-container";*) Class "hero-body"] [
            div [Class "container has-text-justified"] [
                div [Class "columns"] [
                    div [Class "column main-TextField"] [
                        h1 [Class "title is-size-3 is-capitalized is-white is-block is-strongly-emphasized-darkmagenta"] [!! card.CardTitle]
                        div [Class "content is-size-4"] [
                            !!body
                        ]
                    ]
                    div [Class "column"] [
                        div [Class "main-ImageContainer"] [
                            figure [Class "image"] [
                                img [Src (Layout.urlPrefix + "/images/main_loop.svg")]
                            ]
                        ]
                    ]
                ]
               
            ]
        ]
    ]
    

let renderSecondaryCard isLeft (card:Cardloader.MainPageCard) = 
    section [Class "hero is-medium"] [
        div [(*Id "landing-page-hero-container";*) Class "hero-body"] [
            div [Class "container has-text-justified"] [
                if isLeft then 
                    div [Class "columns is-reverse-columns"] [
                        div [Class (sprintf "column main-TextField has-bg-%s" card.CardColor)] [
                            h2 [Class (sprintf "title is-size-3 is-capitalized is-white is-emphasized-%s" card.CardEmphasisColor )] [!! card.CardTitle]
                            div [Class "content is-size-4 is-white"] [
                                !! (getProcessedCardBody card)
                            ]
                        ]
                        div [Class "column"] [
                            div [Class "main-ImageContainer"] [
                                figure [Class "image"] [
                                    img [Src (Layout.urlPrefix + card.CardImages.[0])]
                                ]
                            ]
                    
                        ]
                    ]
                else
                    div [Class "columns"] [
                        div [Class "column"] [
                            div [Class "main-ImageContainer"] [
                                figure [Class "image"] [
                                    img [Src (Layout.urlPrefix + card.CardImages.[0])]
                                ]
                            ]
                        ]
                        div [Class (sprintf "column main-TextField has-bg-%s" card.CardColor)] [
                            h2 [Class (sprintf "title is-size-3 is-capitalized is-white is-emphasized-%s" card.CardEmphasisColor )] [!! card.CardTitle]
                            div [Class "content is-size-4 is-white"] [
                                !! (getProcessedCardBody card)
                            ]
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