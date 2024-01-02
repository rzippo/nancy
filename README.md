# Nancy
## _A computational library for Deterministic Network Calculus_

[![Nuget](https://img.shields.io/nuget/v/Unipi.Nancy)](https://www.nuget.org/packages/Unipi.Nancy/)
[![DOI:10.1016/j.softx.2022.101178](http://img.shields.io/badge/DOI-10.1016/j.softx.2022.101178-B31B1B.svg)](https://doi.org/10.1016/j.softx.2022.101178)

Nancy is a C# library implementing (min,+) and (max,+) operators for ultimately pseudo-periodic piecewise affine curves.

<figure>
    <img src="./img/01.png" alt="Plot of a generic NC curve"/>
</figure>

See the [webpage](https://rzippo.github.io/nancy/) for tutorials and the full documentation.
See the [tutorial notebooks](./examples/) to play directly with code.

Prebuilt package on [NuGet](https://www.nuget.org/packages/Unipi.Nancy/).

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

## Language and requirements

Nancy is a .NET 8.0 library, written in C# 12.
Both SDK and runtime for .NET are cross-platform, and can be downloaded from [here](https://dotnet.microsoft.com/en-us/download).

## References

The algorithms implemented by Nancy are discussed in the following works:

* [An Algorithmic Toolbox for Network Calculus](https://inria.hal.science/inria-00123643v2), Anne Bouillard and Éric Thierry, 2007
* [An Algorithmic Toolbox for Network Calculus](https://doi.org/10.1007/s10626-007-0028-x), Anne Bouillard and Éric Thierry, 2008
* [Deterministic Network Calculus: From Theory to Practical Implementation](http://dx.doi.org/10.1002/9781119440284), Anne Bouillard and Marc Boyer and Euriell Le Corronc, 2018
* [Computationally efficient worst-case analysis of flow-controlled networks with Network Calculus](https://doi.org/10.1109/TIT.2023.3244276), Raffaele Zippo and Giovanni Stea, 2023
* [Extending the Network Calculus Algorithmic Toolbox for Ultimately Pseudo-Periodic Functions: Pseudo-Inverse and Composition](https://doi.org/10.1007/s10626-022-00373-5), Raffaele Zippo and Paul Nikolaus and Giovanni Stea, 2023
* [Isospeed: Improving (min,+) Convolution by Exploiting (min,+)/(max,+) Isomorphism](https://doi.org/10.4230/LIPIcs.ECRTS.2023.12), Raffaele Zippo and Paul Nikolaus and Giovanni Stea, 2023
