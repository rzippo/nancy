# About the tutorials

This folders contains some examples of use of the library in the form of .NET notebooks (`.dib`), so you can see the code in action and edit it to try it for yourself.

> Notebooks allow to write and execute snippets of code (called "cells") and immediately see their outputs.

# Requirements

To run these notebooks you will need to install:
 
 * .NET SDK ([here](https://dotnet.microsoft.com/en-us/download))
 * Visual Studio Code ([here](https://code.visualstudio.com/)) 
 * *.NET Interactive Notebooks* extension ([here](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode))

Then you can open a notebook (a `.dib` file) from VS Code.

> Notebooks allow to write and execute snippets of code (called "cells") and immediately see their outputs.

# The `plot()` function

Plotting is _not_ a feature of Nancy, per se. As a computational library, it focuses on data structures and algorithms, leaving higher-level operations such as plotting to the applications using it.

In this case, as it is really important to see what a `Curve` looks like for tutorials and experimenting, we implemented `plot()` (with various overloads) to make is easier and immediate to try the library.

<figure>
    <img src="../img/06.png">
    <figcaption>Example of use: the call <code>plot(sc)</code> generates the image below the notebook cell.</figcaption>
</figure>

> `plot()` is designed and tested only for simple visualizations in notebooks. For other contexts and uses you may need something different.

> At the time of writing, there is no simple way to include code from *other* files into a notebook, so the definition of `plot()` must be copied over in one the first cells of each notebook.

You can use `plot()` to visualize one or more `Curve` or `Sequence` objects. Many overloads are available:

 * `plot(curve, name, limit)`: plots the given curve from `0` to `limit`, using `name` for the legend.
 * `plot(curves, names, limit)`: plots the given list of curves from `0` to `limit`, using the corresponding `names` for the legend.
 * `plot(sequence, name, limit)`: plots the given sequence, using `name` for the legend.
 * `plot(sequences, names, limit)`: plots the given list of sequences, using the corresponding `names` for the legend.

You can omit the names, in which case { *a*, *b*, *c*, ... } will be used instead. 
For `Curve`s, you can omit the time limit, in which case the function will compute a default one.

With the overloads described above, you will have to explicitly declare the lists, e.g.

```
var sc = new Curve( ... );
var ac = new Curve( ... );
plot(
    new []{sc, ac},     // Curve[]
    new []{"sc", "ac"}, // string[]
);
```

However, if none of the optional arguments is provided, you can use an even simpler syntax

```
plot(sc, ac);   // they will be called "a" and "b" in the legend
```