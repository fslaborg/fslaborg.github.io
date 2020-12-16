#r "../_lib/Fornax.Core.dll"

type SiteInfo = {
    title: string
    description: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    siteContent.Add({title = "FsLab"; description = "The F# Community Incubation Projects Space for Data Science."})

    siteContent
