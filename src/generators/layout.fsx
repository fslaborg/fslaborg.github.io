#r "../_lib/Fornax.Core.dll"
#if !FORNAX
#load "../loaders/pageloader.fsx"
#load "../loaders/globalloader.fsx"
#load "../loaders/cardloader.fsx"
#load "../loaders/packageloader.fsx"
#load "../loaders/tutorialloader.fsx"
#endif

#if WATCH
let urlPrefix = 
  ""
#else
let urlPrefix = 
  "https://fslaborg.github.io"
#endif


open Html 

let injectWebsocketCode (webpage:string) =
    let websocketScript =
        """
        <script type="text/javascript">
          var wsUri = "ws://localhost:8080/websocket";
      function init()
      {
        websocket = new WebSocket(wsUri);
        websocket.onclose = function(evt) { onClose(evt) };
      }
      function onClose(evt)
      {
        console.log('closing');
        websocket.close();
        document.location.reload();
      }
      window.addEventListener("load", init, false);
      </script>
        """
    let head = "<head>"
    let index = webpage.IndexOf head
    webpage.Insert ( (index + head.Length + 1),websocketScript)

let getBgColorForActiveItem (siteTitle:string) =
    match siteTitle with
    | "Home" -> "is-active-link-magenta"
    | "Data science packages" -> "is-active-link-lightmagenta"
    | "Tutorials and Blogposts" -> "is-active-link-darkmagenta"
    | _ -> siteTitle

let createFooterIconLink (iconClass:string) (text:string) (link:string) = 
    div [Class "icon-text is-white"] [
        span [Class"icon"] [
            i [Class iconClass] []
        ]
        span [] [a [Class "footer-link"; Href link] [!! text]]
    ]


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
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "icon"; Type "image/png"; Sizes "32x32"; Href "/images/favicon.png"]
            link [Rel "stylesheet"; Href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css"]
            link [Rel "stylesheet"; Href "https://fonts.googleapis.com/css?family=Nunito+Sans"]
            link [Rel "stylesheet"; Href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.9.1/css/bulma.min.css"]
            link [Rel "stylesheet"; Href "https://cdn.jsdelivr.net/npm/bulma-carousel@4.0.4/dist/css/bulma-carousel.min.css"]
            link [Rel "stylesheet"; Type "text/css"; Href (urlPrefix + "/style/style.css")]
            link [Rel "stylesheet"; Type "text/css"; Href "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/css/bulma-collapsible.min.css"]
            
            script [ Defer true; Src "https://kit.fontawesome.com/0d3e0ea7a6.js"; CrossOrigin "anonymous"][]
            
            script [ Defer true; Type "text/javascript"; Src "https://cdn.jsdelivr.net/npm/@creativebulma/bulma-collapsible@1.0.4/dist/js/bulma-collapsible.min.js"][]
            script [ Defer true; Type "text/javascript"; Src ("https://cdn.jsdelivr.net/npm/bulma-carousel@4.0.4/dist/js/bulma-carousel.min.js") ] []
            
            script [ Defer true; Type "text/javascript"; Src (urlPrefix + "/js/prism.js") ] []
            script [ Defer true; Type "text/javascript"; Src (urlPrefix + "/js/slider.js") ] []
            script [ Defer true; Type "text/javascript"; Src (urlPrefix + "/js/navbar.js") ] []
        ]
        body [] [
            nav [Class "navbar is-fixed-top"] [
                div [Class "navbar-brand"] [
                    a [Class "navbar-item"; Href "/"] [
                        img [Src (urlPrefix + "/images/favicon.png"); Alt "Logo"]
                    ]
                    a [
                        Class "navbar-burger"; 
                        Custom ("data-target", "navMenu"); 
                        Custom ("aria-label", "menu"); 
                        HtmlProperties.Role "button"
                        Custom ("aria-expanded", "false")
                    ] [
                        span [HtmlProperties.Custom ("aria-hidden","true")] []
                        span [HtmlProperties.Custom ("aria-hidden","true")] []
                        span [HtmlProperties.Custom ("aria-hidden","true")] []
                    ]
                ]
                div [Id "navMenu"; Class "navbar-menu"] menuEntries
            ]
            yield! bodyCnt
        ]
        footer [Class "footer has-bg-darkmagenta"] [
            div [Class "container"] [
                div [Class "columns"] [
                    div [Class "column is-4 m-4"] [
                        div [Class "block"] [
                            h3 [Class "subtitle is-white"] [!!"FsLab - the project incubation space for data science in F#"]
                        ]
                        div [Class "block"] [
                            p [] [!!"FsLab is only possible due to the joined forces of F# open source contributors."]
                        ]
                        div [Class "block"] [
                            p [] [!!"This website is created and maintained by individual FsLab open source contributors."]
                        ]
                        div [Class "block"] [
                            createFooterIconLink "fas fa-code-branch" "website source code" "https://github.com/fslaborg/fslabsite"
                            createFooterIconLink "far fa-handshake" "fslab contributors" "https://github.com/orgs/fslaborg/people"
                        ]
                    ]
                    div [Class "column is-4 m-4"] [
                        div [Class "block"] [
                            h3 [Class "subtitle is-white"] [!!"More"]
                        ]
                        div [Class "block"] [
                            div [Class "block"] [
                                createFooterIconLink "fab fa-github" "the fslab organistation on github" "https://github.com/fslaborg?type=source"
                                createFooterIconLink "fab fa-twitter" "fslab on twitter" "https://twitter.com/fslaborg"
                            ]
                            div [Class "block"] [
                                createFooterIconLink "fas fa-cubes" "endorsed packages" "/packages.html"
                                createFooterIconLink "fas fa-plus" "add a package to the list" "https://github.com/fslaborg/fslabsite#add-a-project-to-the-packages-site"
                                
                            ]
                            div [Class "block"] [
                                createFooterIconLink "fas fa-graduation-cap" "tutorials and learning resources" "/tutorials.html"
                                createFooterIconLink "fas fa-plus" "add tutorial content" "https://github.com/fslaborg/fslabsite#add-a-tutorial-guide-or-blogpost"
                            ]
                        ]
                    ]
                    div [Class "column is-4 m-4"] [
                        div [Class "block"] [
                            h3 [Class "subtitle is-white"] [!!"External resources"]
                        ]
                        div [Class "block"] [
                            ul [] [
                                li [] [a [Href "https://github.com/fsprojects?type=source"; Class "footer-link"] [!!"fsprojects - general F# project incubation space"]]
                                li [] [a [Href "https://fsharp.org/"; Class "footer-link"] [!!"fsharp.org"]]
                                li [] [a [Href "https://scisharp.github.io/SciSharp/"; Class "footer-link"] [!!"SciSharp STACK"]]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let render (ctx : SiteContents) cnt =
  cnt
  |> HtmlElement.ToString
#if WATCH
  |> injectWebsocketCode 
#endif

