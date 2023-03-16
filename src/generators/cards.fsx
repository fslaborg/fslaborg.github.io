#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open System
open Html

let createCardLink (card:Cardloader.MainPageCard) =
    a [Class (sprintf "button landing-page-link is-size-4"); Href (Globals.prefixUrl card.CardLink)] [
        strong [] [!! card.CardLinkText]
    ]

let getProcessedCardBody (card:Cardloader.MainPageCard) =
    card.CardBody
        .Replace("<strong>",(sprintf "<strong class='has-bg-one-fourth-%s'>" card.CardEmphasisColor))
        .Replace("<h3>","<h3 class='main-subtitle'>")

let getContentBlocks (additionalClasses:string) (cardBody:string) =
    cardBody.Split("<!---->")
    |> Array.map (fun content ->
        div [Class (sprintf "block %s" additionalClasses)] [
            !!content
        ]
    )
    |> Array.toList

//
let mainHero =
    section [Class "hero is-medium has-bg-magenta"] [
        div [(*Id "landing-page-hero-container";*) Class "hero-body"] [
            div [Class "container has-text-justified"] [
                div [Class "columns"] [
                    div [Class "column main-TextField is-7"] [
                        div [Class "media mb-4"] [
                            div [Class "media-left"] [
                                figure [Class "image is-128x128"] [
                                     img [Id "logo-square"; Class "is-rounded" ; Src "images/logo-rounded.svg"]
                                ]
                            ]
                            div [Class "media-content"] [
                                h1 [Class "main-title is-capitalized is-white is-inline-block is-strongly-emphasized-darkmagenta mb-4"] [!! "Fslab"]
                            ]
                        ]
                        div [Class "block"] [
                            h1 [Class "title is-size-3 is-capitalized is-white is-block"] [!! "The F# community project incubation space for data science"]
                        ]
                        div [Class "content is-white is-size-4"] [
                            div [Class "block is-white"] [
                                !! "Perform the whole data science cycle in F#!"
                            ]
                        ]
                    ]
                    div [Class "column is-5"] [
                        div [Class "main-ImageContainer"] [
                            figure [Class "image"] [
                                img [Src (Globals.prefixUrl "images/main_loop.svg")]
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
                        div [Class (sprintf "column main-TextField has-bg-%s is-7" card.CardColor)] [
                            h2 [Class (sprintf "title is-size-3 is-capitalized is-white is-emphasized-%s is-inline-block" card.CardEmphasisColor )] [!! card.CardTitle]
                            div [Class "content is-size-4 is-white"] [
                                yield!
                                    getProcessedCardBody card
                                    |> getContentBlocks "is-white"
                                if card.CardLinkText = "NONE" then () else createCardLink card
                            ]
                        ]
                        div [Class "column is-5"] [
                            div [Class "main-ImageContainer"] [
                                figure [Class "image"] [
                                    img [Src (Globals.prefixUrl card.CardImages.[0])]
                                ]
                            ]
                    
                        ]
                    ]
                else
                    div [Class "columns"] [
                        div [Class "column is-5"] [
                            div [Class "main-ImageContainer"] [
                                figure [Class "image"] [
                                    img [Src (Globals.prefixUrl card.CardImages.[0])]
                                ]
                            ]
                        ]
                        div [Class (sprintf "column main-TextField has-bg-%s is-7" card.CardColor)] [
                            h2 [Class (sprintf "title is-size-3 is-capitalized is-white is-emphasized-%s is-inline-block" card.CardEmphasisColor )] [!! card.CardTitle]
                            div [Class "content is-size-4 is-white"] [
                                yield!
                                    getProcessedCardBody card
                                    |> getContentBlocks "is-white"
                                if card.CardLinkText = "NONE" then () else createCardLink card
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
        div [] [
            yield mainHero
            yield! 
                cards
                |> List.sortBy (fun c -> c.CardIndex)
                |> List.mapi (fun i card ->
                    let isLeft = i%2=0
                    renderSecondaryCard isLeft card
                )
        ]
    ]


let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    printfn "[Cards-Generator] Starting generate function ..."
    generate' ctx page
    |> Layout.render ctx