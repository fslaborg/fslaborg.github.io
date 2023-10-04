module Literals

module [<RequireQualifiedAccess>] Images =
    let Logo = Extensions.StaticFile.import("./img/fslab.png")
    let LogoRounded = Extensions.StaticFile.import("./img/logo-rounded.svg")
    let MainLoop = Extensions.StaticFile.import("./img/main_loop.svg")
    let OurMission = Extensions.StaticFile.import("./img/mission.svg")
    let DataSciencePackages = Extensions.StaticFile.import("./img/packages.svg")
    let BuildYourSkills = Extensions.StaticFile.import("./img/skills.svg")
    let Interoperability = Extensions.StaticFile.import("./img/interoperability.svg")
    let DefaultUserImage = Extensions.StaticFile.import ("./img/defaultUser-64x64.png")

module DataSciencePackages =
    type DataSciencePackagesUrls = {
        GitHub: string
        Docs: string
        Nuget: string
        Logo: string
    } with
        static member create (github, docs, nuget, logo) = {
            GitHub  = github
            Docs    = docs
            Nuget   = nuget
            Logo    = logo
        }
    let FSharpStats_URLS = DataSciencePackagesUrls.create(
        @"https://www.github.com/fslaborg/FSharp.Stats", 
        @"https://fslab.org/FSharp.Stats/", 
        @"https://www.nuget.org/packages/FSharp.Stats/",
        @"https://api.nuget.org/v3-flatcontainer/fsharp.stats.msf/0.3.0-beta/icon"
    )

    let CytoscapeNET_URLS = DataSciencePackagesUrls.create(
        @"https://github.com/fslaborg/Cytoscape.NET", 
        @"https://fslab.org/Cytoscape.NET/", 
        @"https://www.nuget.org/packages/Cytoscape.NET/",
        @"https://api.nuget.org/v3-flatcontainer/cytoscape.net/0.2.0/icon"
    )

    let flips_URLS = DataSciencePackagesUrls.create(
        @"https://www.github.com/fslaborg/flips",
        @"https://flipslibrary.com/#/",
        @"https://www.nuget.org/packages/flips/",
        @"https://api.nuget.org/v3-flatcontainer/flips/2.4.4/icon"
    )

    let PlotlyNET_URLS = DataSciencePackagesUrls.create(
        @"https://www.github.com/plotly/Plotly.NET",
        @"https://plotly.github.io/Plotly.NET/",
        @"https://www.nuget.org/packages/Plotly.NET/",
        @"https://api.nuget.org/v3-flatcontainer/plotly.net/2.0.0-preview.16/icon"
    )

    let RProvider_URLS = DataSciencePackagesUrls.create(
        @"https://www.github.com/fslaborg/RProvider",
        @"https://fslab.org/RProvider/",
        @"https://www.nuget.org/packages/RProvider/",
        @"https://api.nuget.org/v3-flatcontainer/rprovider/2.0.1-beta2/icon"
    )

    let Deedle_URLS = DataSciencePackagesUrls.create(
        @"https://www.github.com/fslaborg/Deedle",
        @"https://fslab.org/Deedle/",
        @"https://www.nuget.org/packages/Deedle/",
        @"https://api.nuget.org/v3-flatcontainer/deedle/2.3.0/icon"
    )

    let FSharpData_URLS = DataSciencePackagesUrls.create(
        @"https://github.com/fsprojects/FSharp.Data",
        @"https://fsprojects.github.io/FSharp.Data/",
        @"https://www.nuget.org/packages/FSharp.Data",
        @"https://api.nuget.org/v3-flatcontainer/fsharp.data/4.1.1/icon"
    )

module [<RequireQualifiedAccess>] Urls =

    module GitHub =
        let FsLabOrganisation = @"https://github.com/fslaborg"
        let SourceCode = @"https://github.com/fslaborg/fslaborg.github.io"
        let Contributors = @"https://github.com/orgs/fslaborg/people"
        let AddPackage = @"https://github.com/fslaborg/fslaborg.github.io#add-a-project-to-the-packages-site"
        let AddBlogpost = @"https://github.com/fslaborg/fslaborg.github.io#add-a-tutorial-guide-or-blogpost"

    module Socials =

        let TwitterX = @"https://twitter.com/fslaborg"

    module External =

        let Tidyverse = @"https://www.tidyverse.org"

        let MLDotNet = @"https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet"

        let SciSharp = @"https://scisharp.github.io/SciSharp/"

        let FsProjects = @"https://github.com/fsprojects"

        let FSharpOrg = @"https://fsharp.org"
    
    module ApiEndpoints =
        let OpenCollective = @"https://opencollective.com/fslab/members/all.json"


module Branding =

    let [<Literal>] Title = "FsLab"

    let [<Literal>] Slogan = "The Community Driven Toolkit For Datascience In F#"