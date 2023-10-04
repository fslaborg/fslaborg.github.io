module Component.Footer

open Feliz
open Feliz.Bulma


module FooterCols =
    let summary() =
        Bulma.column [
            Bulma.column.isOneThird
            prop.children [
                Bulma.content [
                    Bulma.title.h5 [
                        Bulma.color.hasTextWhite
                        prop.text (sprintf "%s - %s" Literals.Branding.Title Literals.Branding.Slogan)
                    ]
                    Html.p "FsLab is only possible due to the joined forces of F# open source contributors."
                    Html.p "This website is created and maintained by individual FsLab open source contributors."
                    Html.ul [
                        prop.style [style.listStyleType.none]
                        prop.children [
                            Html.li [
                                Bulma.iconText [
                                    Bulma.icon (Html.i [prop.className "fa-solid fa-code-branch"])
                                    Html.a [prop.href Literals.Urls.GitHub.SourceCode; prop.text "Website Source Code"]
                                ]
                            ]
                            Html.li [
                                Bulma.iconText [
                                    Bulma.icon (Html.i [prop.className "fa-solid fa-handshake-simple"])
                                    Html.a [prop.href Literals.Urls.GitHub.Contributors; prop.text $"{Literals.Branding.Title} Contributers"]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let contactContribution() =
        let content = [
            Bulma.title.h5 [
                Bulma.color.hasTextWhite
                prop.text "Contact"
            ]
            Html.ul [
                prop.style [style.listStyleType.none]
                prop.children [
                    Html.li [
                        Bulma.iconText [
                            Bulma.icon (Html.i [prop.className "fa-brands fa-github"])
                            Html.a [prop.href Literals.Urls.GitHub.FsLabOrganisation; prop.text $"GitHub"]
                        ]
                    ]
                    Html.li [
                        Bulma.iconText [
                            Bulma.icon (Html.i [prop.className "fa-brands fa-x-twitter"])
                            Html.a [prop.href Literals.Urls.Socials.TwitterX; prop.text $"Twitter / X"]
                        ]
                    ]
                ]
            ]
            Bulma.title.h5 [
                Bulma.color.hasTextWhite
                prop.text "Contribution"
            ]
            Html.ul [
                prop.style [style.listStyleType.none]
                prop.children [
                    Html.li [
                        Bulma.iconText [
                            Bulma.icon (Html.i [prop.className "fa-solid fa-plus"])
                            Html.a [prop.href Literals.Urls.GitHub.AddPackage; prop.text $"Add a package"]
                        ]
                    ]
                    Html.li [
                        Bulma.iconText [
                            Bulma.icon (Html.i [prop.className "fa-solid fa-plus"])
                            Html.a [prop.href Literals.Urls.GitHub.AddBlogpost; prop.text $"Add a blogpost"]
                        ]
                    ]
                ]
            ]
        ]
        Bulma.column [
            Bulma.column.isOneThird
            prop.children [
                Bulma.content content
            ]
        ]

    let ressources() =
        let content = [
            Bulma.title.h5 [
                Bulma.color.hasTextWhite
                prop.text "Ressources"
            ]
            Html.ul [
                prop.style [style.listStyleType.none]
                prop.children [
                    Html.li [
                        Bulma.iconText [
                            Bulma.icon (Html.i [prop.className "fa-solid fa-cubes"])
                            Html.a [prop.href <| Page.DataSciencePackages.toUrl(); prop.text $"Data Science Packages"]
                        ]
                    ]
                    Html.li [
                        Bulma.iconText [
                            Bulma.icon (Html.i [prop.className "fa-solid fa-graduation-cap"])
                            Html.a [prop.href <| Page.Blog.toUrl(); prop.text $"Learning Ressources"]
                        ]
                    ]
                ]
            ]
            Bulma.title.h6 [
                Bulma.color.hasTextWhite
                prop.text "External"
            ]
            Html.ul [
                prop.style [style.listStyleType.disc]
                prop.children [
                    Html.li [
                        Html.a [prop.href Literals.Urls.External.FsProjects; prop.text $"FsProjects"]
                        Html.span " - a general F# Community Space for open community project incubation."
                    ]
                    Html.li [
                        Html.a [prop.href Literals.Urls.External.FSharpOrg; prop.text $"fsharp.org"]
                    ]
                ]
            ]
        ]
        Bulma.column [
            Bulma.column.isOneThird
            prop.children [
                Bulma.content content
            ]
        ]

[<ReactComponent>]
let Main() =
    Bulma.footer [
        prop.className "has-background-primaryd has-text-white"
        prop.children [
            Bulma.container [
                Bulma.columns [
                    Bulma.columns.isCentered
                    prop.children [
                        FooterCols.summary()
                        FooterCols.contactContribution()
                        FooterCols.ressources()
                    ]
                ]
            ]
        ]
    ]