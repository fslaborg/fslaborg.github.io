#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (_: string) =
    Layout.layout ctx "Endorsed projects" [
        div [Class "section"] [
            h1 [Class "title"] [!! "Endorsed projects"]
        ]
    ]
    

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page
  |> Layout.render ctx