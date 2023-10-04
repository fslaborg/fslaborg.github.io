module Pages.Main

open Feliz
open Feliz.Bulma

open Component.ContentHelpers

let private mainBanner() =
    let logoTitles =
        Bulma.content [
            rowField [
                Bulma.image [
                    Bulma.image.is128x128
                    prop.children [
                        Html.img [prop.className "is-rounded";prop.src Literals.Images.LogoRounded]
                    ]
                ]
                Html.div [
                    prop.className "is-stronger"
                    prop.text Literals.Branding.Title
                ]
            ]
            Bulma.title.h3 Literals.Branding.Slogan
            Bulma.title.h5 [
                prop.text "Perform the whole data science cycle in F#!"
            ] 
        ]
    let schema =
        imageContainer <|
            Bulma.image [
                Html.img [
                    prop.src Literals.Images.MainLoop
                ]
            ]
    bannerContainer "primary" [
        logoTitles
        schema
    ]

module FsLabTiles =

    let private tileBuilder (content: ReactElement list) = 
        Bulma.box [
            prop.className "content-tile"
            prop.children [
                Bulma.content content
            ]
        ]

    let tileLinkButton(url: string, text: string) =
        Bulma.button.a [
            Bulma.color.isLink
            Bulma.button.isLarge
            prop.href url
            prop.text text
        ]

    let tileIcon(iconSrc: string) = 
        Bulma.image [
            prop.className "tile-icon"
            Bulma.image.is64x64
            prop.style [style.custom("marginLeft", "auto")]
            prop.children [ Html.img [prop.src iconSrc] ]
        ]

    let ourMission() =
        tileBuilder [
            rowField [
                Html.h2 "Our Mission"
                tileIcon Literals.Images.OurMission
            ]
            Html.h3 "Curating the FsLab Stack"
            Html.div [
                Html.span "We strive to provide exceptional tools for conducting data science in F#. "
                Html.span "Therefore, we develop and curate the "
                Html.a [ prop.href Literals.Urls.GitHub.FsLabOrganisation; prop.target "_blank";  prop.text "FsLab stack"]
                Html.span " - a collection of mature, high-quality packages that form a cohesive one-stop solution. Think "
                Html.a [ prop.href Literals.Urls.External.Tidyverse; prop.target "_blank";  prop.text "tidyverse"]
                Html.span " for F#/.NET."
            ]
            Html.h3 "Fostering an F# Data Science Community"
            Html.span "Whether you've never written a line of code before or you're already a skilled data scientist experimenting with .NET, we aim to have you covered with up-to-date "
            Html.a [ prop.href <| Page.Blog.toUrl(); prop.target "_blank";  prop.text "blog posts"]
            Html.span ", documentation, and "
            Html.a [ prop.href <| Page.DataSciencePackages.toUrl(); prop.target "_blank";  prop.text Page.DataSciencePackages.PageName]
            Html.span ". We provide a safe haven for any data science project in search of new maintainers and/or contributors."
        ]

    let dataSciencePackages() = 
        tileBuilder [
            rowField [
                Html.h2 "F#-First Data Science Packages"
                tileIcon Literals.Images.DataSciencePackages
            ]
            Html.p "Here you find our recommended collection of open source F# packages for your data analysis toolchain."
            Html.p "Together with your editor or notebook these packages allow you to rapidly develop scalable, high-performance analytics and visualizations using succinct, type-safe, production-ready code."
            Html.p "Just pick and play."
            tileLinkButton(Page.DataSciencePackages.toUrl(), "Explore Packages")
        ]

    let buildYourSkills() =
        tileBuilder [
            rowField [
                Html.h2 "Build Your Skills"
                tileIcon Literals.Images.BuildYourSkills
            ]
            Html.p "From zero to hero in data science using F# with our blogs and tutorials."
            Html.p "FsLab guides you through downloading and setting up F# for data science."
            Html.p "We support you in learning basics of reading and writing F# syntax and solving problems by examples that work with your own environment."
            tileLinkButton(Page.Blog.toUrl(), "Get Started")
        ]

    let interoperability() =
        tileBuilder [
            rowField [
                Html.h2 "Interoperability Is Key"
                tileIcon Literals.Images.Interoperability
            ]
            Html.p "Connect and collaborate."
            Html.p "With Fslab you are part of an amazing .NET Open-Source ecosystem for data science, machine learning and AI. We recommend always using the best tools for your analytic problem at hand. Therefore, FsLab endorses partner projects from the ecosystem."
            Html.p [
                Html.span "Visit "
                Html.a [ prop.href Literals.Urls.External.MLDotNet; prop.target "_blank";  prop.text "ML.NET"]
                Html.span ", an open source and cross-platform machine learning framework, and "
                Html.a [ prop.href Literals.Urls.External.SciSharp; prop.target "_blank";  prop.text "SciSharp STACK"]
                Html.span " which brings all major machine learning frameworks from Python to .NET ecosystem."
            ]
            Html.p "Make friends with other communities and projects and give yourself maximum flexibility for the future."
        ]

let infoTiles() = 
    tileContainer [ 
        FsLabTiles.ourMission()
        FsLabTiles.dataSciencePackages()
        FsLabTiles.buildYourSkills()
        FsLabTiles.interoperability()
    ]

let Main() =
    Html.div [
        mainBanner()
        infoTiles()
        Component.Sponsors.Main()
    ]