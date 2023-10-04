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

    let FsLabGitHub_Organisation = @"https://github.com/fslaborg"

    module ExternalUrls =

        let Tidyverse = @"https://www.tidyverse.org"

        let MLDotNet = @"https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet"

        let SciSharp = @"https://scisharp.github.io/SciSharp/"
    
    module ApiEndpoints =
        let OpenCollective = @"https://opencollective.com/fslab/members/all.json"