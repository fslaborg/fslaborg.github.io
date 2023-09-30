module Components.ThemeSwitch

open Feliz
open Feliz.Bulma
open State.DataTheme

type private State = {
    /// This is just for ui noc actual styling
    Theme: DataTheme
} with
    static member init() = 
        let dt = DataTheme.GET()
        DataTheme.SET dt
        Browser.Dom.console.log("[DATA-THEME]", dt)
        { Theme = dt }

[<ReactComponent>]
let Main() = 
    let state, update = React.useState(State.init)
    Bulma.button.button [
        let t = match state.Theme with | Dark -> "dark" | Light -> "light"
        prop.text t
        prop.onClick <| fun _ ->
            let next = if state.Theme.isDark then Light else Dark
            DataTheme.SET(next)
            update {state with Theme = next} // This is just for ui
    ]