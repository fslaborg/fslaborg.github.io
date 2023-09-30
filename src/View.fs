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
            Component.Sponsors.Main()
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
                    | Page.Main -> 
                        Html.section [
                            Bulma.button.button [
                                prop.className "is-primaryd"
                                prop.text "Hello"
                            ]
                        ]
                    | Page.DataSciencePackages -> Html.h1 "Welcome! you found our awesome collection of data packages."
                    | Page.NotFound | _ -> Html.h1 "Not found"
                )
            ]
        ]