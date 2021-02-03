#r "../_lib/Fornax.Core.dll"
#if !FORNAX
#load "../loaders/pageloader.fsx"
#load "../loaders/globalloader.fsx"
#load "../loaders/cardloader.fsx"
#load "../loaders/packageloader.fsx"
#endif

#if WATCH
let urlPrefix = 
  ""
#else
let urlPrefix = 
  "/fslabsite"
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
        let cls = if p.title = active then "navbar-item is-active-link smooth-hover" else "navbar-item"
        a [Class cls; Href p.link] [!! p.title ])
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
        ]
        body [] [
          nav [Class "navbar is-fixed-top"] [
            div [Class "navbar-brand"] [
            a [Class "navbar-item"; Href "/"] [
                img [Src (urlPrefix + "/images/testlogo.png"); Alt "Logo"]
            ]
            span [Class "navbar-burger burger"; Custom ("data-target", "navbarMenu")] [
                span [] []
                span [] []
                span [] []
            ]
            ]
            div [Id "navbarMenu"; Class "navbar-menu"] menuEntries
          ]
          yield! bodyCnt
        ]
    ]

let render (ctx : SiteContents) cnt =
  cnt
  |> HtmlElement.ToString
#if WATCH
  |> injectWebsocketCode 
#endif

