#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"
#if !FORNAX
#load "../loaders/pageloader.fsx"
#load "../loaders/globalloader.fsx"
#load "../loaders/cardloader.fsx"
#load "../loaders/packageloader.fsx"
#load "../loaders/tutorialloader.fsx"
#endif

open FsLab.Fornax
open Html 

let getBgColorForActiveItem (siteTitle:string) =
    match siteTitle with
    | "Home" -> "is-active-link-magenta"
    | "Data science packages" -> "is-active-link-lightmagenta"
    | _ -> siteTitle

let layout (ctx : SiteContents) active bodyCnt =
    let pages = ctx.TryGetValues<Pageloader.Page> () |> Option.defaultValue Seq.empty
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl =
        siteInfo
        |> Option.map (fun si -> si.title)
        |> Option.defaultValue ""

    let menuEntries =
        pages
        |> Seq.map (fun p ->
            let cls = if p.title = active then (sprintf "navbar-item %s smooth-hover" (getBgColorForActiveItem active)) else "navbar-item"
            a [Class cls; Href p.link] [!! p.title ]
        )
        |> Seq.toList

    html [Class "has-navbar-fixed-top"] [
        head [] [
            yield! Components.DefaultHeadTags ttl
          
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/css/bulma-collapsible.min.css"]
            script [ Defer true; Src "https://kit.fontawesome.com/0d3e0ea7a6.js"; CrossOrigin "anonymous"] []
            script [ Defer true; Type "text/javascript"; Src "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/js/bulma-collapsible.min.js"] []
            script [ Defer true; Type "text/javascript"; Src ("https://cdn.jsdelivr.net/npm/bulma-carousel@4.0.4/dist/js/bulma-carousel.min.js") ] []
            script [ Defer true; Type "text/javascript"; Src (Globals.prefixUrl "js/prism.js") ] []
            script [ Defer true; Type "text/javascript"; Src (Globals.prefixUrl "js/slider.js") ] []
            script [ Defer true; Type "text/javascript"; Src (Globals.prefixUrl "js/navbar.js") ] []
        ]
        body [] [

            Components.Navbar(
                LogoSource = (Globals.prefixUrl "images/favicon.png"),
                LogoLink = "https://fslab.org",
                MenuEntries = menuEntries,
                SocialLinks = [
                    a [Class "navbar-item is-magenta"; Href "https://twitter.com/fslaborg"] [Components.Icon "fab fa-twitter"]
                    a [Class "navbar-item is-magenta"; Href "https://github.com/fslaborg"] [Components.Icon "fab fa-github"]
                ]
            )
            yield! bodyCnt
        ]
        Components.Footer()
    ]

let render (ctx : SiteContents) cnt =
  cnt
  |> HtmlElement.ToString
#if WATCH
  |> Globals.injectWebsocketCode 
#endif

