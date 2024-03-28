# About the tutorials

This folders contains some examples of use of the library in the form of .NET notebooks (`.dib`), so you can see the code in action and edit it to try it for yourself.

> Notebooks allow to write and execute snippets of code (called "cells") and immediately see their outputs.

# Requirements

To run these notebooks you will need to install:
 
 * .NET SDK ([here](https://dotnet.microsoft.com/en-us/download))
 * Visual Studio Code ([here](https://code.visualstudio.com/)) 
 * *Polyglot Notebooks* extension ([here](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode))

Then you can open a notebook (a `.dib` file) from VS Code.

> Notebooks allow to write and execute snippets of code (called "cells") and immediately see their outputs.

# The `Plots.Plot()` method

The `Nancy` package is the computational library focused on data structures and algorithms.
The `Nancy.Interactive` package is the one that provide plotting functionality for .NET notebooks.
This is how you import it into a notebook.

```csharp
#r "nuget: Unipi.Nancy.Interactive"

using Unipi.Nancy.Interactive;
```

You can then plot your curves using `Plots.Plot()`.
This method has various overloads, to make it easier and immediate to use.

<figure>
    <img src="../img/06.png">
    <figcaption>Example of use: the call to <code>Plots.Plot()</code> generates the image below the notebook cell.</figcaption>
</figure>

> `Plots.Plot()` is designed and tested only for simple visualizations in notebooks. For other contexts and uses you may need something different.
> `Plots.GetPlot()` and `Plots.GetPlotHtml()` return objects than can be used to render the plot elsewhere, e.g. in a browser page.
> For TikZ plots, see `ToTikzPlot()`. 

You can use `Plots.Plot()` to visualize one or more `Curve` or `Sequence` objects. Many overloads are available:

 * `Plots.Plot(curve, name, limit)`: plots the given curve from `0` to `limit`, using `name` for the legend.
 * `Plots.Plot(curves, names, limit)`: plots the given list of curves from `0` to `limit`, using the corresponding `names` for the legend.
 * `Plots.Plot(sequence, name, limit)`: plots the given sequence, using `name` for the legend.
 * `Plots.Plot(sequences, names, limit)`: plots the given list of sequences, using the corresponding `names` for the legend.

Names can be omitted.
For the overloads for single curve or sequence will, the default name will capture the name of the variable passed as argument, e.g. `Plots.Plot(beta)` will show "beta" in the legend.
For the overloads for multiple curves or sequences, names *f*, *g*, *h*, and so on, will be used instead. 

For `Curve`s, you can omit the time limit, in which case the method will compute a default one.

With the overloads described above, you can either explicitly declare the lists or arrays (both are fine), e.g.

```
var sc = new Curve( ... );
var ac = new Curve( ... );
Plots.Plot(
    new Curve[]{sc, ac},
    new string[]{"sc", "ac"}, // string[]
);
```

or use [collection expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions), e.g.

```
Plots.Plot([sc, ac],["sc", "ac"]);
```

However, if none of the optional arguments is provided, you can use an even simpler syntax

```
Plots.Plot(sc, ac);   // they will be called "f" and "g" in the legend
```