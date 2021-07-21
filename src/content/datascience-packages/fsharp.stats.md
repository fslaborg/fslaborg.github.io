---
package-name: FSharp.Stats
package-logo: https://api.nuget.org/v3-flatcontainer/fsharp.stats.msf/0.3.0-beta/icon
package-nuget-link: https://www.nuget.org/packages/FSharp.Stats/
package-github-link: https://www.github.com/fslaborg/FSharp.Stats
package-documentation-link: https://fslab.org/FSharp.Stats/
package-description: statistical testing, linear algebra, machine learning, fitting and signal processing in F#.
#package-posts-link: optional
package-tags: statistics, linear algebra, machine learning, fitting, signal processing
---

FSharp.Stats is a multipurpose project for statistical testing, linear algebra, machine learning, fitting and signal processing.

Here is a simple basic example for getting general statistics of a sequence of numbers sampled from a normal distribution:

```fsharp
#r "nuget: FSharp.Stats"
open FSharp.Stats

// initialize a normal distribution with mean 25 and standard deviation 0.1
let normalDistribution = Distributions.Continuous.normal 25. 0.1

// draw independently 30 times from the given distribution 
let sample = Array.init 30 (fun _ -> normalDistribution.Sample())

let mean = Seq.mean sample
let stDev = Seq.stDev sample
let cv = Seq.cv sample
```
