---
package-name: Plotly.NET
package-logo: https://api.nuget.org/v3-flatcontainer/plotly.net/2.0.0-preview.6/icon
package-nuget-link: https://www.nuget.org/packages/Plotly.NET/
package-github-link: https://www.github.com/plotly/Plotly.NET
package-documentation-link: https://plotly.github.io/Plotly.NET/
package-description: Plotly.NET provides functions for generating and rendering plotly.js charts in .NET programming languages 📈🚀.
#package-posts-link: optional
package-tags: visualization, data exploration, charting
---

Plotly.NET provides functions for generating and rendering plotly.js charts in .NET programming languages 📈🚀.

It can be used to create plotly.js charts in the following environments:

<br></br>

## 1. **interactive charts in html pages**

## 2. **interactive charts in dotnet interactive notebooks via the [Plotly.NET.Interactive](https://www.nuget.org/packages/Plotly.NET.Interactive/) package**

## 3. **static chart images via [Plotly.NET.ImageExport](https://www.nuget.org/packages/Plotly.NET.ImageExport/) package (see more [here](https://plotly.net/0_1_image-export.html))**

<br></br>

here is a basic example snippet that renders a simple point chart, either as html page or static image:

```fsharp
#r "nuget: Plotly.NET, 2.0.0-preview.6"
#r "nuget: Plotly.NET.ImageExport, 2.0.0-preview.6"

open Plotly.NET

let myChart = 
    Chart.Point(
        [
            (1.,2.)
            (2.,3.)
            (3.,4.)
            (5.,2.)
        ]
    )
    |> Chart.withTitle "Hello from Plotly.NET!"
    |> Chart.withX_AxisStyle ("X-Axis",Showline=true,Showgrid=true)
    |> Chart.withY_AxisStyle ("Y-Axis",Showline=true,Showgrid=true)

myChart |> Chart.Show //display as html in browser 

//using tatic image export
open Plotly.NET.ImageExport

myChart |> Chart.savePNG("myChart",600,600) //save chart as static png with 600x600 px
```

Here is an image of the rendered chart:

![](/images/myChart.png)