namespace App

open Feliz
open Feliz.Router
open Feliz.Bulma

type View =

    /// <summary>
    /// This function wraps subpages into a wrapper with common components, such as
    /// Navbar and footer.
    /// </summary>
    static member View(body : ReactElement) =
        Html.div [
            Component.Navbar.Main()
            body
            Component.Footer.Main()
        ]
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
                View.View (
                    match currentPage with
                    | Page.Main -> Pages.Main.Main()
                    | Page.DataSciencePackages -> Pages.DataSciencePackages.Main()
                    | Page.NotFound | _ -> Html.h1 "Not found"
                )
            ]
        ]