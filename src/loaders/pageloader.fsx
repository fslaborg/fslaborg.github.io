#r "../_lib/Fornax.Core.dll"

type Page = {
    title: string
    link: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    siteContent.Add({title = "Home"; link = "/"})
    siteContent.Add({title = "Endorsed projects"; link = "/projects.html"})
    siteContent.Add({title = "Training material"; link = "/material.html"})
    siteContent.Add({title = "Contact"; link = "/contact.html"})

    siteContent
