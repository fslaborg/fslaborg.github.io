module Component.DataSciencePackageCard

open Feliz
open Feliz.Bulma

open Literals.DataSciencePackages
open Component.ContentHelpers

open Fable.Core.JsInterop

importSideEffects "prismjs/components/prism-fsharp.js"
importSideEffects "prismjs/themes/prism-twilight.css"

let private Prism : obj = importDefault "prismjs"

type private CardBuilder = 
    {
        Name: string
        Description: string
        Urls: DataSciencePackagesUrls
        Content: ReactElement

    }     
    static member create(name: string, description: string, urls, content) = {
        Name        = name
        Description = description     
        Urls        = urls
        Content     = content
    }

module private CardBuilder =
    let header(this: CardBuilder, state, update) =
        Bulma.cardHeader [
            Bulma.cardHeaderTitle.div [
                Bulma.media [
                    Bulma.mediaLeft [
                        Bulma.image [
                            Bulma.image.is48x48
                            prop.children [Html.img [prop.src this.Urls.Logo]]
                        ]
                    ]
                    Bulma.mediaContent [
                        prop.style [style.overflow.initial]
                        prop.children [
                            Html.div [ prop.className "title has-text-primary is-size-2"; prop.text this.Name ]
                        ]
                    ]
                ]
            ]
            Bulma.cardHeaderIcon.a [
                prop.onClick (fun _ -> update (not state))
                prop.children [
                    Bulma.icon [Html.i [
                        prop.style [style.custom("transition", "transform .25s ease")]
                        prop.classes [ "fa-solid"; "fa-angle-down"; if state then "fa-rotate-180"]
                    ]]
                ]
            ]
        ]
    let footerItem(url, icon: string, text: string) = 
        Bulma.cardFooterItem.div [
            prop.className "p-0"
            prop.children [
                Bulma.button.a [
                    prop.href url
                    prop.classes [ "is-ghost"]
                    prop.style [style.height (length.percent 100); style.width (length.percent 100); style.borderRadius 0]
                    prop.children [
                        Bulma.icon [Html.i [prop.className icon]]
                        Html.span text
                    ]
                ]
            ]
        ]
    let footer(this : CardBuilder) =
        Bulma.cardFooter [
            footerItem(this.Urls.GitHub,"fa-brands fa-github", "GitHub")
            footerItem(this.Urls.Docs,"fa-solid fa-book", "Docs")
            footerItem(this.Urls.Nuget,"fa-solid fa-cubes", "Nuget")
        ]

    let content(this: CardBuilder, state, update) =
        Bulma.cardContent [
            Bulma.content [
                Html.p this.Description
                Html.blockquote $"""#r "nuget: {this.Name}" """
            ]
            if state then
                Html.div [
                    this.Content
                    Bulma.button.a [
                        Bulma.button.isSmall
                        prop.className "is-ghost"
                        prop.text "show less"
                        prop.onClick (fun _ -> update false)
                    ]
                ]
        ]

[<ReactComponent>] 
let Main(name, description, urls, content) =
    let this = CardBuilder.create(name, description, urls, content)
    let state, update = React.useState(false)
    React.useEffect(fun _ -> Prism?highlightAll())
    Bulma.section [
        prop.key $"{this.Name}"
        prop.children [
            Bulma.card [
                prop.className "datascience-package-card"
                prop.children [
                    CardBuilder.header(this,state, update)
                    CardBuilder.content(this, state, update)
                    CardBuilder.footer(this)
                ]
            ]
        ]
    ]