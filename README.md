# fslab website

Static website generated with :heart:, [fornax](https://github.com/ionide/Fornax), and [fsdocs](https://github.com/fsprojects/FSharp.Formatting)

This [website](https://fslab.org/) demonstrates the aims of the fslab organisation as an incubation space for data science projects in F# and .NET and acts as a source of high quality learning material for any kind of skill level.

<!-- TOC -->

- [Add a project to the packages site](#add-a-project-to-the-packages-site)
- [Add a tutorial, guide, or blogpost](#add-a-tutorial-guide-or-blogpost)
- [Develop](#develop)
    - [Prerequisites](#prerequisites)
    - [Technology used](#technology-used)

<!-- /TOC -->

## Add a project to the packages site

The [packages site](https://fslab.org/packages.html) is used to aggregate fslab-endorsed data science packages in one place. 

To add a package to the endorsed list, follow these steps:

1. Create new [add-package issue](https://github.com/fslaborg/issues/new/choose) by filling the issue template

2. Create the `<YOUR-PACKAGE-NAME.md` file in the `src/content/datascience-packages/` folder, which will contain the metadata about the package that is used to generate the card for it on the page.

3. Add the markdown frontmatter (starting on the very first line). Replace the `<>` placeholders with the actual correct information about your package:

    ```
    ---
    package-name: <The name of your package>
    package-logo: <link to your logo>
    package-nuget-link: <https://www.nuget.org/packages/your-package/ or other nuget source>
    package-github-link: <https://www.github.com/your-handle/your-project>
    package-documentation-link: <link to the documentation of your package>
    package-description: <short and concise description of the package>
    #package-posts-link: optional
    package-tags: <(WIP)tags to categorize your package>
    ---
    ```

4. below the frontmatter, add any kind of markdown content that will be rendered as an expandable `Read more` section. You can use the full markdown goodness here, and code snippets indicated as fsharp will get some syntax highlighting.

5. File a PR referencing the issue you created in step 1.

## Add a tutorial, guide, or blogpost

The [tutorials page](https://fslab.org/tutorials.html) contains links to a collection of tutorials in the following categories:

- `fsharp`:
    Introductory articles on F#, assuming no prior knowledge in programming and F#/.NET. There should also be a link collection to other high quality sources on those topics.

- `datascience`:
    Introduction to datascience in general and using F# for it specifically. Beginner and intermediate content on how to perform common datascience tasks with fslab-endorsed packages.

- `adcvanced`
    Deep dives on advanced topics, performing complex tasks, insights on how packages perform together, etc. More of a blog-post-style content.

To add tutorial content, follow these steps:

1. Create new [add-tutorial issue](https://github.com/fslaborg/issues/new/choose) by filling the issue template

2. In the `./src/content/tutorials_src` folder, create a new  `<YOUR_CONTENT>.fsx` file. Markdown articles will soon be supported as well.

3. Add the following frontmatter (starting on the very first line). Replace the `<>` placeholders with the actual correct information about your package:

    ```fsharp
    (***hide***)

    (*
    #frontmatter
    ---
    title: <YOUR TITLE HERE>
    category: <fsharp, datascience, or advanced>
    authors: <comma-separated list of authors>
    index: 0
    ---
    *)
    ```

4. Below the frontmatter, add the content of your article. If you are new to FSharp.Formatting, check the [docs there]() and also the documentation of the [fslab documentation template]().

5. To get a preview of how your page will look like, in `./src` run the following command(s): 

    ```shell
    dotnet tool restore
    dotnet fornax watch
    ```

    And navigate to the tutorials page.

6. File a PR referencing the issue you created in step 1.

## Develop

### Prerequisites

- currently, .NET core 3.1.XXX is used, will be upgraded to .net5 when tools are ready
- you need to install [Sass(command line)](https://sass-lang.com/install) to compile the .scss styles to the actual stylesheet

To develop the project in watcher mode, go to `/src` and run:

- `dotnet tool restore`
- `dotnet fornax watch`

This will run a webserver that serves the compiled static page(s) on localhost:8080

### Technology used

- [Fornax](https://github.com/ionide/Fornax) a F# scriptable static webpage generator
- [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting) for rendering of literate F# tutorials
- [Markdig](https://github.com/lunet-io/markdig), a markdown processor for .Net
- [Sass](https://sass-lang.com), a CSS extension language
