---
package-name: RProvider
package-logo: https://api.nuget.org/v3-flatcontainer/rprovider/2.0.1-beta2/icon
package-nuget-link: https://www.nuget.org/packages/RProvider/
package-github-link: https://www.github.com/fslaborg/RProvider
package-documentation-link: https://fslab.org/RProvider/
package-description: An F# Type Provider providing strongly typed access to the R statistical language. The type provider automatically discovers available R packages and makes them easily accessible from F#, so you can easily call powerful packages and visualization libraries from code running on the .NET platform.
#package-posts-link: optional
package-tags: R, interop, type provider, visualisation, statistics
---

The R Provider discovers R packages that are available in your R installation and makes 
them available as .NET namespaces underneath the parent namespace RProvider. For example, 
the stats package is available as RProvider.stats. If you open the namespaces you want to 
use, functions and values will be available as R.name.

There are three requirements to be set up on your system:

* dotnet SDK 5.0 or greater; and
* R statistical language. *Note: on Windows, there is currently a bug in R preventing us from supporting R versions greater than 4.0.2.*
* R_HOME environment variable set to the R home directory. This can usually be identified by running the command 'R RHOME'.

Below is a simple script example that demonstrates using R statistical functions,
and using R graphics functions to create charts:

```fsharp
#r "nuget: RProvider,2.0.1"

open RProvider
open RProvider.graphics
open RProvider.grDevices
open RProvider.datasets

// use R to calculate the mean of a list
R.mean([1;2;3;4])

// Calculate sin using the R 'sin' function
// (converting results to 'float') and plot it
[ for x in 0.0 .. 0.1 .. 3.14 -> 
    R.sin(x).GetValue<float>() ]
|> R.plot

// Plot the data from the standard 'Nile' data set
R.plot(R.Nile)```
