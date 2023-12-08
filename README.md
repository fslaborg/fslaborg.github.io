# FsLab - Website

Static website generated with :heart:, [Fable](http://fable.io/) and [Feliz](https://github.com/Zaid-Ajaj/Feliz).

This [website](https://fslab.org/) demonstrates the aims of the fslab organisation as an incubation space for data science projects in F# and .NET and acts as a source of high quality learning material for any kind of skill level.

<!-- TOC -->
- [Contribution](#contribution)
    - [Add a project to the packages site](#add-a-project-to-the-packages-site)
    - [Add a tutorial, guide, or blogpost](#add-a-tutorial-guide-or-blogpost)
- [Develop](#develop)
    - [Requirements](#requirements)
    - [Editor](#editor)
    - [Setup](#setup)
    - [Commands](#commands)
<!-- /TOC -->

# Contribution

## Add a project to the packages site

WIP

## Add a tutorial, guide, or blogpost

This was moved to another repo?

# Develop 

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) v7.0 or higher
* [node.js](https://nodejs.org) v18+ LTS

## Editor

To write and edit your code, you can use either VS Code + [Ionide](http://ionide.io/), Emacs with [fsharp-mode](https://github.com/fsharp/emacs-fsharp-mode), [Rider](https://www.jetbrains.com/rider/) or Visual Studio.

## Setup

1. `dotnet tool restore`, install fable as local dotnet tool.
2. `npm install`, install all js dependencies.

## Commands

### Run

Then to start development mode with hot module reloading, run:

```bash
npm start
```

This will start the development server after compiling the project, once it is finished, navigate to http://localhost:5173/ to view the application .

### Bundle/Build

To build the application and make ready for production:

```bash
npm run build
```

This command builds the application and puts the generated files into the `/docs` directory (can be overwritten in `vite.config.js`).