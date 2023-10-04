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