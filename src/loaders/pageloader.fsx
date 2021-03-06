#r "../_lib/Fornax.Core.dll"
#if WATCH
let urlPrefix = 
  ""
#else
let urlPrefix = 
  "https://fslab.org"
#endif

type Page = {
    title: string
    link: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    siteContent.Add({title = "Home"; link = sprintf "%s/" urlPrefix})
    siteContent.Add({title = "Data science packages"; link = sprintf "%s/packages.html" urlPrefix})
    siteContent.Add({title = "Tutorials and Blogposts"; link = sprintf "%s/tutorials.html" urlPrefix})

    siteContent
