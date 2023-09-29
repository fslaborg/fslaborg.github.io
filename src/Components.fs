namespace App

open Feliz
open Feliz.Router
open Feliz.Bulma

[<AutoOpen>]
module Aux =
    let log x= Browser.Dom.console.log(x)

module ThemeSwitch =

    open Fable.Core.JsInterop

    let getDataTheme() =
        Browser.Dom.document.documentElement.getAttribute("data-theme")

    let setDataTheme(theme: string) =
        Browser.Dom.document.documentElement.setAttribute("data-theme", theme.ToLower() )
    let getDefault() =
        let m : bool = Browser.Dom.window?matchMedia("(prefers-color-scheme: dark)")?matches
        // Browser.Dom.console.log(m)
        m

    type DataTheme =
    | Dark 
    | Light
        static member ofString (str: string) =
            match str.ToLower() with
            | "dark" -> Dark
            | "light" | _ -> Light
        member this.setOppositeTheme(setter: DataTheme -> unit) =
            let next = if this = Dark then Light else Dark
            string next |> setDataTheme
            setter next

        static member getDefault() =
            let isDark = getDefault()
            if isDark then Dark else Light

        static member GET() =
            let dtStr = getDataTheme()
            if isNull dtStr then
                DataTheme.getDefault()
            else
                let dt: DataTheme = DataTheme.ofString dtStr
                dt

        member this.isDark = this = Dark

    type State = {
        Theme: DataTheme
    } with
        static member init() = 
            let dt = DataTheme.GET()
            setDataTheme (string dt)
            Browser.Dom.console.log("[DATA-THEME]", dt)
            { Theme = dt }

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