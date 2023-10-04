module Pages.DataSciencePackages

open Feliz
open Feliz.Bulma

open Component.ContentHelpers

let private mainBanner() =
    let logoTitles: Fable.React.ReactElement =
        Bulma.content [
            rowField [
                Bulma.image [
                    Bulma.image.is128x128
                    prop.children [
                        Html.img [
                            prop.className "is-rounded has-background-white";
                            prop.src Literals.Images.DataSciencePackages
                        ]
                    ]
                ]
                Html.div [
                    prop.style [style.fontSize (length.rem 3)]
                    prop.text "Data science packages" 
                ]
            ]
            Html.div [
                prop.className "has-text-primaryd is-size-5"; 
                prop.children [
                    Html.p "Use these packages to fuel your data science journey in F# and .NET! 🚀" 
                    Html.p [ 
                        Html.span "Want to add a package to the curated list? " 
                        Html.a [
                            prop.children [
                                Html.span "File a PR"
                                Bulma.icon [Html.i [prop.className "fa-solid fa-code-branch"]]
                            ]
                        ]
                    ]
                ]
            ]
            
        ]
    bannerContainer "primaryl" [
        logoTitles
    ]

let Main() =
    Html.div [
        mainBanner()
    ]