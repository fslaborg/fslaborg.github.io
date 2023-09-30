module Component.Sponsors

open Feliz
open Feliz.Bulma
open Web.OpenCollective

type private State = {
    Members: Member list 
} with
    static member init() = {
        Members = []
    } 

let memberElement(mem:Member) =
    let loremPicsum = Literals.Images.DefaultUserImage
    let memberImage = defaultArg mem.Image loremPicsum
    Html.div [
        prop.className "is-flex is-flex-direction-column is-justify-content-flex-start is-align-items-center p-1"
        prop.style [style.textAlign.center]
        prop.children [
            Html.figure [
                prop.className "image is-64x64"
                prop.children [
                    Html.img [prop.src memberImage]
                ]
            ]
            Html.div mem.Name
        ]
    ]

let private memberContainer(preMembers: Member list) =
    let members = 
        preMembers
        |> Seq.distinctBy (fun x -> x.Profile)
        |> Seq.sortByDescending (fun x -> x.TotalAmountDonated)
    Html.div [
        prop.style [style.display.flex; style.justifyContent.center]
        prop.children [
            for mem in members do
                memberElement mem
        ]
    ]

[<ReactComponent>]
let Main() =
    let state, update = React.useState(State.init) 
    getSponsors(fun members -> update {state with Members = members}) |> Promise.start 
    Html.div [
        Bulma.hero [
            Bulma.heroBody [
                Bulma.title.h1 "FsLab sponsors"
                // Bulma.button.a [
                //     prop.className "m-2"
                //     prop.text "Become a sponsor!"
                // ]
                if state.Members.IsEmpty then
                    Html.div "... loading"
                else
                    memberContainer state.Members
            ]
        ]
    ]