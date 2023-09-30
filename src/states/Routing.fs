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
        let l =
            match this with
            | Main -> []
            | NotFound -> [ "404" ]
        let lHash = "#"::l
        let sb = System.Text.StringBuilder()
        for seq in lHash do
            if seq.StartsWith("?") then 
                sb.Append seq |> ignore
            else
                sb.Append(sprintf "/%s" seq) |> ignore
        sb.ToString()
        

    member this.PageName =
        match this with
        | Main -> "Home"
        | NotFound -> "404"