module Component.ContentHelpers

open Feliz
open Feliz.Bulma

let rowField (content : ReactElement list) = 
    Html.div [
        prop.className "row-container"
        prop.children content
    ]

let imageContainer (image: ReactElement) =
    Html.div [
        prop.className "image-container"
        prop.children image
    ]

let bannerContainer (children: ReactElement list) =
    Bulma.hero [
        Bulma.hero.isMedium
        Bulma.color.isPrimary
        prop.children [
            Bulma.heroBody [
                Bulma.container [
                    Bulma.columns [
                        for c in children do
                            Bulma.column c
                    ]
                ]
            ]
        ]
    ]

let tileContainer (children: ReactElement list) =
    Bulma.section [
        Bulma.columns [
            Bulma.columns.isCentered
            Bulma.columns.isMultiline
            prop.children [
                for c in children do
                    Bulma.column [
                        Bulma.column.isOneThirdFullHd
                        Bulma.column.isHalf
                        prop.children c
                    ]
            ]
        ]
    ]