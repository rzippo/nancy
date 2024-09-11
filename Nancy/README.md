> Using the comprehensive solution `Nancy.sln` may cause issues during build.
> This is, as far as I understand, due to ambiguity with projects building the same code, like `Nancy.Expressions` and `Nancy.Expressions.Local`.
>
> As a workaround, the two solution filters `Nancy.Local.slnf` and `Nancy.NUget.slnf` load either all `Local` projects (for ease of local development) or all non-`Local` projects (which is useful to verify consistency of published packages).
