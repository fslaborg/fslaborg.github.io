module Component.Navbar

open Feliz
open Feliz.Bulma

let private icon = 
    Bulma.icon [
        Bulma.icon.isMedium
        prop.children [ Html.img [
            prop.href <| Page.Main.toUrl()
            prop.src <| Literals.Images.Logo
            prop.alt "FsLab logo"
        ]]
    ]

let private navbarBrand(isActive: bool, update: bool -> unit) = 
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            icon
        ]
        Bulma.navbarBurger [
            prop.ariaLabel "menu"
            prop.ariaExpanded isActive
            prop.role "button"
            prop.onClick (fun _ -> update (not isActive))
            prop.children [
                Html.span [prop.ariaHidden true]
                Html.span [prop.ariaHidden true]
                Html.span [prop.ariaHidden true]
            ]
        ]
    ]

open Feliz.Router
let private getActivePage() =
    Router.currentUrl() |> Page.fromUrl


let private pageLink(page:Routing.Page) =
    Bulma.navbarItem.a [
        let activePage = getActivePage()
        if (page = activePage) then Bulma.navbarItem.isActive
        prop.href <| page.toUrl()
        prop.text page.PageName
    ]

let private navbarStart() = 
    Bulma.navbarStart.div [
        pageLink Page.Main
    ]

let private navbarEnd() = 
    Bulma.navbarEnd.div [
        Component.ThemeButton.Main()
    ]

let private navbarMenu(isActive:bool) =
    Bulma.navbarMenu [
        if isActive then Bulma.navbarMenu.isActive
        prop.children [
            navbarStart()
            navbarEnd()
        ]
    ]

[<ReactComponent>]
let Main() =
    let state, update = React.useState(false)
    Bulma.navbar [
        Bulma.color.isPrimary
        Bulma.navbar.hasShadow
        Bulma.navbar.isFixedTop
        prop.children [
            navbarBrand(state, update)
            navbarMenu state
        ]
    ]