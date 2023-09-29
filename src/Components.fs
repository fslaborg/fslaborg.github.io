namespace App

open Feliz
open Feliz.Router
open Feliz.Bulma

module ThemeSwitch =

    let getDataTheme() =
        Browser.Dom.document.documentElement.getAttribute("data-theme")

    type DataTheme =
    | Dark 
    | Light
        static member ofString (str: string) =
            if isNull str then Light
            else
                match str.ToLower() with
                | "dark" -> Dark
                | "light" | _ -> Light
        member this.setOppositeTheme(setter: DataTheme -> unit) =
            let prev = getDataTheme()
            Browser.Dom.console.log(prev)
            let next = if this = Dark then Light else Dark
            Browser.Dom.document.documentElement.setAttribute("data-theme", (string next).ToLower() )
            setter next

        member this.isDark = this = Dark

    type State = {
        Theme: DataTheme
    } with
        static member init() = 
            let dt = getDataTheme()
            Browser.Dom.console.log("[DATA-THEME]", dt)
            { Theme = DataTheme.ofString dt }

    let button() = 
        let state, update = React.useState(State.init)
        Bulma.button.button [
            let t = match state.Theme with | Dark -> "dark" | Light -> "light"
            prop.text t
            prop.onClick <| fun _ ->
                let setter = fun (theme: DataTheme) ->
                    update {state with Theme = theme}
                state.Theme.setOppositeTheme(setter)
        ]

type Components =
    /// <summary>
    /// The simplest possible React component.
    /// Shows a header with the text Hello World
    /// </summary>
    [<ReactComponent>]
    static member HelloWorld() = Html.h1 "Hello World"

    /// <summary>
    /// A stateful React component that maintains a counter
    /// </summary>
    [<ReactComponent>]
    static member Counter() =
        let (count, setCount) = React.useState(0)
        Html.div [
            Html.h1 count
            Html.button [
                prop.onClick (fun _ -> setCount(count + 1))
                prop.text "Increment"
            ]
            ThemeSwitch.button()
        ]

    /// <summary>
    /// A React component that uses Feliz.Router
    /// to determine what to show based on the current URL
    /// </summary>
    [<ReactComponent>]
    static member Router() =
        let (currentUrl, updateUrl) = React.useState(Router.currentUrl())
        React.router [
            router.onUrlChanged updateUrl
            router.children [
                match currentUrl with
                | [ ] -> Html.h1 "Index"
                | [ "hello" ] -> Components.HelloWorld()
                | [ "counter" ] -> Components.Counter()
                | otherwise -> Html.h1 "Not found"
            ]
        ]