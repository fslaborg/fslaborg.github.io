module Pages.Main

open Feliz
open Feliz.Bulma

let private mainBanner() =
    let logoTitles =
        Bulma.column [
            Bulma.column.isHalf
            prop.children [
                Bulma.content [
                    prop.className "is-flex is-flex-direction-row pl-5"
                    prop.children [
                        Bulma.image [
                            Bulma.image.is128x128
                            prop.children [
                                Html.img [prop.className "is-rounded";prop.src Literals.Images.LogoRounded]
                            ]
                        ]
                        Html.div [
                            prop.className "is-stronger"
                            prop.style [style.fontSize (length.rem 5)]
                            prop.text "FsLab"
                        ]
                    ]
                ]
            ]
        ]
    let schema =
        Bulma.column [
            Bulma.column.isHalf
            prop.children [
                Bulma.image [
                    Html.img [
                        prop.src Literals.Images.MainLoop
                    ]
                ]
            ]
        ]
    Bulma.hero [
        Bulma.hero.isMedium
        Bulma.color.isPrimary
        prop.children [
            Bulma.heroBody [
                Bulma.container [
                    Bulma.columns [
                        logoTitles
                        schema
                    ]
                ]
            ]
        ]
    ]

let Main() =
    Html.div [
        mainBanner()
        Component.Sponsors.Main()
    ]