module Literals

module [<RequireQualifiedAccess>] Images =
    let Logo = Extensions.StaticFile.import("./img/fslab.png")

    let DefaultUserImage = Extensions.StaticFile.import ("./img/defaultUser-64x64.png")

module [<RequireQualifiedAccess>] Urls =
    
    module ApiEndpoints =
        let OpenCollective = @"https://opencollective.com/fslab/members/all.json"