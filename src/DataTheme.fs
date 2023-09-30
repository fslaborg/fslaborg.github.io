module State.DataTheme

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