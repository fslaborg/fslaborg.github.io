# fslab website

Static website generated with :heart:, [fornax](https://github.com/ionide/Fornax), and [fsdocs](https://github.com/fsprojects/FSharp.Formatting)

This [website](https://fslab.org/) demonstrates the aims of the fslab organisation as an incubation space for data science projects in F# and .NET and acts as a source of high quality learning material for any kind of skill level.

<!-- TOC -->

- [fslab website](#fslab-website)
  - [Add a project to the packages site](#add-a-project-to-the-packages-site)
  - [Add a tutorial, guide, or blogpost](#add-a-tutorial-guide-or-blogpost)
  - [Develop](#develop)
    - [Prerequisites](#prerequisites)
    - [Technology used](#technology-used)

<!-- /TOC -->

## Add a project to the packages site

The [packages site](https://fslab.org/packages.html) is used to aggregate fslab-endorsed data science packages in one place. 

The main goal of fslab is to streamline the datascience environment of F#. 

In a first step we want to collect and foster open source datascience packages and create a save haven for F# open source maintainers and developers

We want to encourage interaction between these libraries to move forward together, 
as well as prevent fslab from becoming just an address to dump a project url for easy promotion. Therefore, while we don't want to set up strict requirements - especially in this critical first phase of fslab - 
we kindly ask you to meet at least **one of the following requirements** are with your package:

- 1. Additionally to the package documentation itself, **tutorials and guides** on how to get started with and interact with other fslab packages will be included at the time of joining the list. See the [next chapter](#add-a-tutorial-guide-or-blogpost) on how to do that.

- 2. The github repository that holds the source code of the package will **move to the fslab github organization** (with no changes to repo access besides access for fslab admins). This increases the weight of fslab as a center of gravity for open source F# developers and would be the best choice for projects that are in need for new maintainers.

- 3. The **documentation** of the project will be changed or created in the [**fslab documentation theme**](). This ensures fostering a certain fslab 'brand awareness'

To add a package to the endorsed list, follow these steps:

1. Create new [add-package issue](https://github.com/fslaborg/fslaborg.github.io/issues/new/choose) by filling the issue template. 

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

please refer to the FsLab [blog](https://github.com/fslaborg/blog?tab=readme-ov-file#add-content) repository

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
- [Markdig](https://github.com/lunet-io/markdig), a markdown processor for .Net
- [Sass](https://sass-lang.com), a CSS extension language
