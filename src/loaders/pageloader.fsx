#r "../_lib/Fornax.Core.dll"
#load "../globals.fsx"

type Page = {
    title: string
    link: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    siteContent.Add({title = "Home"; link = Globals.prefixUrl ""})
    siteContent.Add({title = "Data science packages"; link = Globals.prefixUrl "packages.html"})
    siteContent.Add({title = "Blog"; link = sprintf "https://fslab.org/blog"})

    siteContent
