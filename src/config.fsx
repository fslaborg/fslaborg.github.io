#r "_lib/Fornax.Core.dll"
#load "globals.fsx"

open Config
open Globals

let config = {
    Generators = [
        //{Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        {Script = "cards.fsx"; Trigger = Once; OutputFile = NewFileName "index.html" }
        {Script = "packages.fsx"; Trigger = Once; OutputFile = NewFileName "packages.html"}
        {Script = "sass.fsx"; Trigger = OnFileExt ".scss"; OutputFile = ChangeExtension "css" }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate Predicates.staticPredicate; OutputFile = SameFileName }
        {Script = "material.fsx"; Trigger = Once; OutputFile = NewFileName "material.html" }
        {Script = "projects.fsx"; Trigger = Once; OutputFile = NewFileName "projects.html" }
        {Script = "contact.fsx"; Trigger = Once; OutputFile = NewFileName "contact.html" }
    ]
}
