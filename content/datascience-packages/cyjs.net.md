---
package-name: Cyjs.NET
package-logo: https://api.nuget.org/v3-flatcontainer/cyjs.net/0.0.3/icon
package-nuget-link: https://www.nuget.org/packages/Cyjs.NET
package-github-link: https://www.github.com/fslaborg/Cyjs.NET
package-documentation-link: https://fslab.org/Cyjs.NET/
package-description: .NET interface for Cytoscape.js written in F# for graph visualization.
#package-posts-link: optional
package-tags: cytoscape, graph visualization
---

This package provides a light-weighted layer on top of the famous Cytoscape.js library. It brings all the graph visualization capabilities directly into .NET.

Here is a small snippet that creates a basic styled graph:

```fsharp
#r "nuget: Cyjs.NET"

open Cyjs.NET
open Elements

let myFirstStyledGraph =     
    CyGraph.initEmpty ()
    |> CyGraph.withElements [
            node "n1" [ CyParam.label "FsLab"  ]
            node "n2" [ CyParam.label "ML" ]
 
            edge  "e1" "n1" "n2" []
        ]
    |> CyGraph.withStyle "node"     
            [
                CyParam.content =. CyParam.label
                CyParam.color "#A00975"
            ]
    |> CyGraph.withSize(800, 400)  
    |> CyGraph.show // displays the graph in a browser
```

Here is an image of the rendered graph:

![](/images/cygraph.png)