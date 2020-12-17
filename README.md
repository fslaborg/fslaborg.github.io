# fslab website

Static website generated with :heart: and [fornax](https://github.com/ionide/Fornax)

This website demonstrates the aims of the fslab organisation as an incubation space for data science projects in F# and .NET:

- fslab as a home for community projects to get rejuvenated or to get more traction
- A endorsement list with both fslab and external projects for data science in .NET, categorized by position in the data science workflow
- High-quality learning material:
    - intro to F# for datascience, assuming 0 programming knowledge
    - high-level advanced tutorials, combining several endorsed tools to a data science stack

## Add a project (WIP)

To add a project, fork this repo and add a `<your-project>.md` file to the respective subfolder in `/projects`:

- `projects/A` for projects concerned with A
- `projects/B` for projects concerned with B
- `projects/C` for projects concerned with C
- `projects/D` for projects concerned with D

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