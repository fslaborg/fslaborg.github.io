#r "../_lib/Fornax.Core.dll"
#if WATCH
let urlPrefix = 
  ""
#else
let urlPrefix = 
  "/fslabsite"
#endif

type Page = {
    title: string
    link: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    siteContent.Add({title = "Home"; link = sprintf "%s/" urlPrefix})
    siteContent.Add({title = "Data science packages"; link = sprintf "%s/packages.html" urlPrefix})

    siteContent
