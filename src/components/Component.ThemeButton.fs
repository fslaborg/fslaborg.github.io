module Component.ThemeButton

open Feliz
open Feliz.Bulma
open State.DataTheme

type private State = {
    Theme: DataTheme
} with 
    static member init() = 
        {
            Theme = DataTheme.GET()
        }

[<ReactComponent>]
let Main() =
    let state, update = React.useState(State.init)
    Bulma.navbarItem.a [
        prop.onClick (fun e ->
            e.preventDefault()
            let next = if state.Theme = Dark then Light else Dark
            DataTheme.SET next
            update {Theme = next}
        )
        prop.children [
            Bulma.icon [
                Bulma.icon.isMedium
                prop.children state.Theme.toIcon
            ]
        ]
    ]
