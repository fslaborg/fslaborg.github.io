module Component.Footer

open Feliz
open Feliz.Bulma

[<ReactComponent>]
let Main() =
    Html.div [
        prop.text "Footer"
    ]