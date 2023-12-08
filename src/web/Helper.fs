module Web.Helper

open Fable
open Fetch

let get(url: string, callback: string -> unit) =
    promise {
        let! data = fetch url []
        match data.Ok with
        | true -> 
            let! body = data.text()
            callback body
        | false -> 
            failwithf "Error. %s" data.StatusText
        return ()
    }