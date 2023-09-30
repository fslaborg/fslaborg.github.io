module Literals

module [<RequireQualifiedAccess>] Images =
    let Logo = Extensions.StaticFile.import("./img/fslab.png")

module [<RequireQualifiedAccess>] Urls =
    let x = 0