module [<AutoOpen>] Routing

[<RequireQualifiedAccess>]
type Page =
| Main
| NotFound
    static member fromUrl (url: string list) =
        match url with
        | [ ] -> Main
        | [ "404" ] |_ -> NotFound

    member this.toUrl() =
        match this with
        | Main -> []
        | NotFound -> [ "404" ]