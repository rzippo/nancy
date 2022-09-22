using System.Runtime.CompilerServices;

#if BIG_RATIONAL
[assembly: InternalsVisibleTo("Unipi.Nancy.Tests")]
#endif
#if LONG_RATIONAL
[assembly: InternalsVisibleTo("Unipi.Nancy.Tests.LongRational")]
#endif