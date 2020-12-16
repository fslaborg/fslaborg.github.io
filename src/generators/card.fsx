#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (_: string) =
    
    let cards : Cardloader.MainPageCard list= 
        ctx.TryGetValues<Cardloader.MainPageCard>()
        |> Option.defaultValue Seq.empty
        |> Seq.toList

    Layout.layout ctx "Home" [
        section [Class "section"] (
            cards
            |> List.mapi (fun i card ->
                if (i+1)%2 = 0 then
                    div [Class (sprintf "card-is-%s main-Container" card.CardColor)] [
                        div [Class "columns"] [
                            div [Class "column"] [
                                div [Class "main-TextField is-skewed-left"] [
                                    h2 [Class (sprintf "main-title is-emphasized-half-%s" card.CardEmphasisColor )] [!! card.CardTitle]
                                    div [Class "main-text"] [
                                        !! card.CardBody.Replace("<strong>",(sprintf "<strong class='is-emphasized-one-third-%s'>" card.CardEmphasisColor))
                                    ]
                                ]
                            ]
                            div [Class "column"] [
                                div [Class "main-ImageContainer"] [
                                    a [Href "https://github.com/fslaborg"; Target "_blank"] [
                                        figure [Class "image is-square"] [
                                            img [Src card.CardImage]
                                        ]
                                    ]
                                ]
                            ]
                        
                        ]
                    ]
                else
                    div [Class (sprintf "card-is-%s main-Container" card.CardColor)] [
                        div [Class "columns"] [
                            div [Class "column"] [
                                div [Class "main-ImageContainer"] [
                                    a [Href "https://github.com/fslaborg"; Target "_blank"] [
                                        figure [Class "image is-square"] [
                                            img [Src card.CardImage]
                                        ]
                                    ]
                                ]
                            ]
                            div [Class "column"] [
                                div [Class "main-TextField is-skewed-right"] [
                                    h2 [Class (sprintf "main-title is-emphasized-half-%s" card.CardEmphasisColor )] [!! card.CardTitle]
                                    div [Class "main-text"] [
                                        !! card.CardBody.Replace("<strong>",(sprintf "<strong class='is-emphasized-one-third-%s'>" card.CardEmphasisColor))
                                    ]
                                ]
                            ]
                        
                        ]
                    ]
            )
        )
    ]


let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page
  |> Layout.render ctx