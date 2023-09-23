#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let referenceCodeBlock packageNugetName = code [Class "package-reference"] [
    !! (sprintf "#r \"nuget: %s\"" packageNugetName)
]

let generate' (ctx : SiteContents) (_: string) =
    
    let packages : Packageloader.DataSciencePackage list= 
        ctx.TryGetValues<Packageloader.DataSciencePackage>()
        |> Option.defaultValue Seq.empty
        |> Seq.toList

    Layout.layout ctx "Data science packages" [
        section [Class "hero is-medium has-bg-lightmagenta"] [
            div [Class "hero-body"] [
                div [Class "container"] [
                    div [Class "media mb-4"] [
                        div [Class "media-left"] [
                            figure [Class "image is-128x128"] [
                                 img [Id "package-header-img"; Class "is-rounded"; Src "images/packages.svg"]
                            ]
                        ]
                        div [Id "package-title-container"; Class "media-content"] [
                            h1 [Class "main-title is-darkmagenta"] [!!"Data science packages"]
                        ]
                    ]
                    h3 [Class "subtitle is-magenta"] [!!"Use these packages to fuel your data science journey in F# and .NET! 🚀"]
                    h3 [Class "subtitle is-magenta"] [
                        !!"Want to add a package to the curated list?"
                        a [] [
                            !!"File a PR"
                            Href ("https://github.com/fslaborg/fslaborg.github.io/tree/main/src/content/datascience-packages")
                            span [Class "icon"] [i [Class "fa fa-code-branch"] []]
                        ]
                    ]
                ]
            ]
        ]
        div [Class "section"] (
            packages
            |> List.map (fun package ->
                div [Class "Container package-container"] [
                    div [Class "card"] [
                        //header [Class "card-header"] [!! package.PackageName]
                        div [Class "card-content"] [
                            div [Class "media"] [
                                div [Class "media-left"] [
                                    figure [Class "image is-48x48"] [
                                        img [Src package.PackageLogoLink]
                                    ]
                                ]
                                div [Class "media-content"] [
                                    p [Class "title"] [a [Href package.PackageGithubLink] [!! package.PackageName]]
                                ]
                            ]
                            p [Class "subtitle is-6"] [!! package.PackageDescription]
                            referenceCodeBlock package.PackageName
                            if package.PackageTags.IsSome then
                                div [Class"tags"] (
                                    package.PackageTags.Value
                                    |> Array.map (fun tag -> span [Class "has-bg-magenta tag"] [!!tag])
                                    |> Array.toList
                                )
                            if package.PackageMore.IsSome then 
                                a [
                                    Href (sprintf "#%s-collapse" package.PackageName)
                                    HtmlProperties.Custom("data-action","collapse")
                                    HtmlProperties.Custom("aria-label","more")
                                    Class "collapsible-trigger"
                                ] [
                                    !! "Read More"
                                    span [Class "icon"] [
                                        i [Class "fas fa-angle-down"] []
                                    ]
                                ]
                                div [Id (sprintf "%s-collapse" package.PackageName); Class "is-collapsible"] [
                                    hr []
                                    !!package.PackageMore.Value
                                ]
                        ]
                        div [Class "card-footer"] [
                            a [Class "card-footer-item"; Href package.PackageGithubLink] [
                                span [Class "icon"] [i [Class "fas fa-code-branch"] []]
                                !!"Github"
                            ]
                            a [Class "card-footer-item"; Href package.PackageDocumentationLink] [
                                span [Class "icon"] [i [Class "fas fa-book"] []]
                                !!"Docs"
                            ]
                            a [Class "card-footer-item"; Href package.PackageNugetLink] [
                                span [Class "icon"] [i [Class "fas fa-cubes"] []]
                                !!"Nuget"
                            ]
                            if package.PackagePostsLink.IsSome then
                                a [Class "card-footer-item"; Href package.PackagePostsLink.Value] [
                                    span [Class "icon"] [i [Class "fas fa-blog"] [] ]
                                    !!"Posts"
                                ]
                        ]

                    ]
                ]
            )
        )
    ]


let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    printfn "[Packages-Generator] Starting generate function ..."
    generate' ctx page
    |> Layout.render ctx
