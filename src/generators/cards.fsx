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

    div [Class "hero has-bg-magenta is-medium-text"] [
        div [Id "landing-page-hero-container"; Class "main-Container"] [
            div [Class "main-TextField"] [
                div [HtmlProperties.Style [CSSProperties.Width "50%"; Margin "0 auto 1rem"]] [
                    figure [Class "image is-3by1 has-ratio mb-4"] [
                        img [Src (Layout.urlPrefix + "/images/landing_test.png")]
                    ]
                ]
                div [Class "main-text"] [
                    div [Class "columns is-desktop"] [
                        div [Class "column"] [
                            !! body]
                        div [Class "column"] [
                            figure [Class "image"] [
                                img [Src (Layout.urlPrefix + "/images/landing_test.png")]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
    

let renderSecondaryCard isLeft (card:Cardloader.MainPageCard) = 
    div [Class "main-Container is-medium-text"] [
        if isLeft then 
            div [Class "columns is-reverse-columns is-desktop"] [
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
                        figure [Class "image"] [
                            img [Src (Layout.urlPrefix + card.CardImages.[0])]
                        ]
                    ]
            
                ]
            ]
        else
            div [Class "columns is-desktop"] [
                div [Class "column"] [
                    div [Class "main-ImageContainer"] [
                        figure [Class "image"] [
                            img [Src (Layout.urlPrefix + card.CardImages.[0])]
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