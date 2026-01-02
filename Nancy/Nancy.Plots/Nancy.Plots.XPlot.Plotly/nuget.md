# Nancy.Plots.Tikz
## _Nancy extension to produce plots using XPlot.Plotly_

This package adds methods to produce plots using [XPlot.Plotly](https://www.nuget.org/packages/XPlot.Plotly).

```csharp
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots.XPlot.Plotly;

var sc = new RateLatencyServiceCurve(3, 1);
var ac = new SigmaRhoArrivalCurve(2, 2);
var html = XPlotPlots.ToXPlotHtml([sc, ac]);
File.WriteAllText("plot.html", html);
```

This package is based on the interface defined in the `Unipi.Nancy.Plots` namespace, which is shared with other `Nancy.Plots.*` packages, allowing code reuse and similar functionalities.

Checkout the [webpage](https://rzippo.github.io/nancy/) for tutorials and the full documentation, or the [tutorial notebooks](./examples/) on GitHub to play directly with code.

## Academic attribution

This software is an academic product, just like papers are. If you build on someone else's scientific ideas, you will obviously cite their paper reporting these ideas. 
This is standard academic practice. Like it or not, citations are the academic currency. 

```
If you use the Nancy library, or any software including parts of it or derived from it, 
we would appreciate it if you could cite the original paper describing it:

R. Zippo, G. Stea, "Nancy: an efficient parallel Network Calculus library", 
SoftwareX, Volume 19, July 2022, DOI: 10.1016/j.softx.2022.101178
```

The MIT license allows you to use this software for almost any purpose. However, if you use or include this software or its code (in full or in part) in your own, the fact that you are doing so in full compliance to the license does not exempt you from following standard academic practices regarding attribution and citation. 
This means that it is still your duty to ensure that users of your software:

  1. know that it use or includes our work, and 
  
  2. they can cite the above paper for correct attribution (along with your own work, possibly). 

The above two requirements are met, for open source projects, if you report the statement above in the readme of any code that uses or includes ours. 
