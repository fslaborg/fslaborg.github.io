(***hide***)

(*
#frontmatter
---
title: F# Introduction III: Library Setup
category: fsharp
authors: Jonathan Ott, Kevin Schneider
index: 2
---
*)
(**
# F# Introduction III: Library Setup

This guide shows an example setup for a library. This is not the only way on how to do this, but merely a possibility. As always, this guide is meant as a starting point to be expanded upon. 
The installation of .NET 5.0 or dotnet SDK 3.1 LTS is required. It is also recommended to use [GitHub](https://github.com/) when following this example.

## Initializing the repository

* An easy way to initialize a repository is by creating a new one using GitHub and cloning it.
    * You can automatically add a readme, a .gitignore with many entries for Visual Studio already added and a license of choice.
    ![]({{root}}images/InitRepo.png)
* After you cloned the initialized repository, it should look like this:
```
│   .git
│   .vs
|   .gitignore
│   LICENSE
|   .README.md
```

## Initializing the library

* The stock library template is just fine (change framework if you know what you are doing):
    `dotnet new classlib -lang F# -n "YourNameHere" --framework net5.0 -o src/YourNameHere`
* Add an entry for the 'pkg' folder to your `.gitignore`
* Create a `RELEASE_NOTES.md` file in the project root, make sure to add at least one version header like this:
    ```
    ### 0.0.1 - 1/27/2021
    ```
* Add a solution to your projekt with `dotnet new sln --name YourNameHere`
* After you completed the previous steps your folder should look like this:
    ```
    │   .git
    │   .vs
    |   .gitignore
    │   LICENSE
    |   .README.md
    |   RELEASE_NOTES.md
    |   YourNameHere.sln
    └───src
        └───YourNameHere
            |   bin
            |   obj
            │   Library.fs
            │   YourNameHere.fsproj
    ```
*)