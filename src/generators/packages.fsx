#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let codeBlock content = code [] [!!content]

let generate' (ctx : SiteContents) (_: string) =
    
    let packages : Packageloader.DataSciencePackage list= 
        ctx.TryGetValues<Packageloader.DataSciencePackage>()
        |> Option.defaultValue Seq.empty
        |> Seq.toList

    Layout.layout ctx "Data science packages" [
        section [Class "hero is-medium has-bg-lightmagenta"] [
            div [Class "hero-body"] [
                div [Class "container"] [
                    div [Class "media"] [
                        div [Class "media-left"] [
                            figure [Class "image"] [
                                 img [Id "package-header-img"; Class "is-rounded"; Src "images/packages-square.svg"]
                            ]
                        ]
                        div [Id "package-title-container"; Class "media-content"] [
                            h1 [Class "title is-darkmagenta"] [!!"Data science packages"]
                            h3 [Class "subtitle is-magenta"] [!!"Data science packages"]
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
                                    p [Class "title"] [a [] [!! package.PackageName]]
                                    p [Class "subtitle is-6"] [!! package.PackageDescription]
                                ]
                            ]
                            codeBlock package.PackageNugetLink
                            if package.PackageTags.IsSome then
                                div [Class"tags"] (
                                    package.PackageTags.Value
                                    |> Array.map (fun tag -> span [Class "has-bg-magenta tag"] [!!tag])
                                    |> Array.toList
                                )
                        ]
                        div [Class "card-footer"] [
                            a [Class "card-footer-item"; Href package.PackageGithubLink] [!!"Github"]
                            a [Class "card-footer-item"; Href package.PackageDocumentationLink] [!!"Docs"]
                            a [Class "card-footer-item"; Href package.PackageNugetLink] [!!"Nuget"]
                            if package.PackagePostsLink.IsSome then
                                a [Class "card-footer-item"; Href package.PackagePostsLink.Value] [!!"Posts"]
                        ]

                    ]
                ]
            )
        )
    ]


let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page
  |> Layout.render ctx