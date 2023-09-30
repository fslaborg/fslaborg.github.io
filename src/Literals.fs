module Literals

module [<RequireQualifiedAccess>] Images =
    let Logo = Extensions.StaticFile.import("./img/fslab.png")
    let LogoRounded = Extensions.StaticFile.import("./img/logo-rounded.svg")
    let MainLoop = Extensions.StaticFile.import("./img/main_loop.svg")
    let DefaultUserImage = Extensions.StaticFile.import ("./img/defaultUser-64x64.png")

module [<RequireQualifiedAccess>] Urls =
    
    module ApiEndpoints =
        let OpenCollective = @"https://opencollective.com/fslab/members/all.json"