namespace App

open Feliz
open Feliz.Router
open Feliz.Bulma

type View =
    /// <summary>
    /// A React component that uses Feliz.Router
    /// to determine what to show based on the current URL
    /// </summary>
    [<ReactComponent>]
    static member Main() =
        let (currentPage, updatePage) = React.useState(Router.currentUrl() |> Page.fromUrl)
        React.router [
            router.onUrlChanged (Page.fromUrl >> updatePage)
            router.children [
                match currentPage with
                | Page.Main -> Html.h1 "Index"
                | Page.NotFound -> Html.h1 "Not found"
            ]
        ]