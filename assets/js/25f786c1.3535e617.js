"use strict";(self.webpackChunknancy_docs=self.webpackChunknancy_docs||[]).push([[5910],{3905:(a,e,t)=>{t.d(e,{Zo:()=>o,kt:()=>k});var n=t(7294);function s(a,e,t){return e in a?Object.defineProperty(a,e,{value:t,enumerable:!0,configurable:!0,writable:!0}):a[e]=t,a}function m(a,e){var t=Object.keys(a);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(a);e&&(n=n.filter((function(e){return Object.getOwnPropertyDescriptor(a,e).enumerable}))),t.push.apply(t,n)}return t}function p(a){for(var e=1;e<arguments.length;e++){var t=null!=arguments[e]?arguments[e]:{};e%2?m(Object(t),!0).forEach((function(e){s(a,e,t[e])})):Object.getOwnPropertyDescriptors?Object.defineProperties(a,Object.getOwnPropertyDescriptors(t)):m(Object(t)).forEach((function(e){Object.defineProperty(a,e,Object.getOwnPropertyDescriptor(t,e))}))}return a}function r(a,e){if(null==a)return{};var t,n,s=function(a,e){if(null==a)return{};var t,n,s={},m=Object.keys(a);for(n=0;n<m.length;n++)t=m[n],e.indexOf(t)>=0||(s[t]=a[t]);return s}(a,e);if(Object.getOwnPropertySymbols){var m=Object.getOwnPropertySymbols(a);for(n=0;n<m.length;n++)t=m[n],e.indexOf(t)>=0||Object.prototype.propertyIsEnumerable.call(a,t)&&(s[t]=a[t])}return s}var i=n.createContext({}),l=function(a){var e=n.useContext(i),t=e;return a&&(t="function"==typeof a?a(e):p(p({},e),a)),t},o=function(a){var e=l(a.components);return n.createElement(i.Provider,{value:e},a.children)},c={inlineCode:"code",wrapper:function(a){var e=a.children;return n.createElement(n.Fragment,{},e)}},N=n.forwardRef((function(a,e){var t=a.components,s=a.mdxType,m=a.originalType,i=a.parentName,o=r(a,["components","mdxType","originalType","parentName"]),N=l(t),k=s,h=N["".concat(i,".").concat(k)]||N[k]||c[k]||m;return t?n.createElement(h,p(p({ref:e},o),{},{components:t})):n.createElement(h,p({ref:e},o))}));function k(a,e){var t=arguments,s=e&&e.mdxType;if("string"==typeof a||s){var m=t.length,p=new Array(m);p[0]=N;var r={};for(var i in e)hasOwnProperty.call(e,i)&&(r[i]=e[i]);r.originalType=a,r.mdxType="string"==typeof a?a:s,p[1]=r;for(var l=2;l<m;l++)p[l]=t[l];return n.createElement.apply(null,p)}return n.createElement.apply(null,t)}N.displayName="MDXCreateElement"},9069:(a,e,t)=>{t.r(e),t.d(e,{assets:()=>i,contentTitle:()=>p,default:()=>c,frontMatter:()=>m,metadata:()=>r,toc:()=>l});var n=t(7462),s=(t(7294),t(3905));const m={hide_title:!0},p="PlotEndStrategy",r={unversionedId:"nancy/Unipi.Nancy.TikzPlot/PlotEndStrategy",id:"nancy/Unipi.Nancy.TikzPlot/PlotEndStrategy",title:"PlotEndStrategy",description:"Different strategies to compute the x-axis right boundary.",source:"@site/docs/nancy/Unipi.Nancy.TikzPlot/PlotEndStrategy.md",sourceDirName:"nancy/Unipi.Nancy.TikzPlot",slug:"/nancy/Unipi.Nancy.TikzPlot/PlotEndStrategy",permalink:"/nancy/docs/nancy/Unipi.Nancy.TikzPlot/PlotEndStrategy",draft:!1,tags:[],version:"current",frontMatter:{hide_title:!0},sidebar:"docsSidebar",previous:{title:"LegendStrategy",permalink:"/nancy/docs/nancy/Unipi.Nancy.TikzPlot/LegendStrategy"},next:{title:"TikzLayoutSettings",permalink:"/nancy/docs/nancy/Unipi.Nancy.TikzPlot/TikzLayoutSettings"}},i={},l=[{value:"Fields",id:"fields",level:2},{value:"<strong>OnePeriodEach</strong>",id:"oneperiodeach",level:3},{value:"<strong>TwoPeriodsEach</strong>",id:"twoperiodseach",level:3}],o={toc:l};function c(a){let{components:e,...t}=a;return(0,s.kt)("wrapper",(0,n.Z)({},o,t,{components:e,mdxType:"MDXLayout"}),(0,s.kt)("h1",{id:"plotendstrategy"},"PlotEndStrategy"),(0,s.kt)("p",null,"Different strategies to compute the x-axis right boundary."),(0,s.kt)("pre",null,(0,s.kt)("code",{parentName:"pre",className:"language-csharp"},"public enum PlotEndStrategy\n")),(0,s.kt)("p",null,"Inheritance ",(0,s.kt)("a",{parentName:"p",href:"https://docs.microsoft.com/en-us/dotnet/api/system.object"},"Object")," \u2192 ",(0,s.kt)("a",{parentName:"p",href:"https://docs.microsoft.com/en-us/dotnet/api/system.valuetype"},"ValueType")," \u2192 ",(0,s.kt)("a",{parentName:"p",href:"https://docs.microsoft.com/en-us/dotnet/api/system.enum"},"Enum")," \u2192 ",(0,s.kt)("a",{parentName:"p",href:"/docs/nancy/Unipi.Nancy.TikzPlot/PlotEndStrategy"},"PlotEndStrategy"),(0,s.kt)("br",null),"\nImplements ",(0,s.kt)("a",{parentName:"p",href:"https://docs.microsoft.com/en-us/dotnet/api/system.icomparable"},"IComparable"),", ",(0,s.kt)("a",{parentName:"p",href:"https://docs.microsoft.com/en-us/dotnet/api/system.iformattable"},"IFormattable"),", ",(0,s.kt)("a",{parentName:"p",href:"https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible"},"IConvertible")),(0,s.kt)("h2",{id:"fields"},"Fields"),(0,s.kt)("h3",{id:"oneperiodeach"},(0,s.kt)("strong",{parentName:"h3"},"OnePeriodEach")),(0,s.kt)("p",null,"The boundary is chosen such that, for each function, at least a full period is plotted.\nIn math, ",(0,s.kt)("span",{parentName:"p",className:"math math-inline"},(0,s.kt)("span",{parentName:"span",className:"katex"},(0,s.kt)("span",{parentName:"span",className:"katex-mathml"},(0,s.kt)("math",{parentName:"span",xmlns:"http://www.w3.org/1998/Math/MathML"},(0,s.kt)("semantics",{parentName:"math"},(0,s.kt)("mrow",{parentName:"semantics"},(0,s.kt)("mo",{parentName:"mrow"},"\u22c1"),(0,s.kt)("mo",{parentName:"mrow",stretchy:"false"},"{"),(0,s.kt)("msub",{parentName:"mrow"},(0,s.kt)("mi",{parentName:"msub"},"T"),(0,s.kt)("mi",{parentName:"msub"},"i")),(0,s.kt)("mo",{parentName:"mrow"},"+"),(0,s.kt)("msub",{parentName:"mrow"},(0,s.kt)("mi",{parentName:"msub"},"d"),(0,s.kt)("mi",{parentName:"msub"},"i")),(0,s.kt)("mo",{parentName:"mrow",stretchy:"false"},"}")),(0,s.kt)("annotation",{parentName:"semantics",encoding:"application/x-tex"},"\\bigvee\\{ T_i + d_i \\}")))),(0,s.kt)("span",{parentName:"span",className:"katex-html","aria-hidden":"true"},(0,s.kt)("span",{parentName:"span",className:"base"},(0,s.kt)("span",{parentName:"span",className:"strut",style:{height:"1em",verticalAlign:"-0.25em"}}),(0,s.kt)("span",{parentName:"span",className:"mop op-symbol small-op",style:{position:"relative",top:"0em"}},"\u22c1"),(0,s.kt)("span",{parentName:"span",className:"mopen"},"{"),(0,s.kt)("span",{parentName:"span",className:"mord"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal",style:{marginRight:"0.13889em"}},"T"),(0,s.kt)("span",{parentName:"span",className:"msupsub"},(0,s.kt)("span",{parentName:"span",className:"vlist-t vlist-t2"},(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.3117em"}},(0,s.kt)("span",{parentName:"span",style:{top:"-2.55em",marginLeft:"-0.1389em",marginRight:"0.05em"}},(0,s.kt)("span",{parentName:"span",className:"pstrut",style:{height:"2.7em"}}),(0,s.kt)("span",{parentName:"span",className:"sizing reset-size6 size3 mtight"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal mtight"},"i")))),(0,s.kt)("span",{parentName:"span",className:"vlist-s"},"\u200b")),(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.15em"}},(0,s.kt)("span",{parentName:"span"})))))),(0,s.kt)("span",{parentName:"span",className:"mspace",style:{marginRight:"0.2222em"}}),(0,s.kt)("span",{parentName:"span",className:"mbin"},"+"),(0,s.kt)("span",{parentName:"span",className:"mspace",style:{marginRight:"0.2222em"}})),(0,s.kt)("span",{parentName:"span",className:"base"},(0,s.kt)("span",{parentName:"span",className:"strut",style:{height:"1em",verticalAlign:"-0.25em"}}),(0,s.kt)("span",{parentName:"span",className:"mord"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal"},"d"),(0,s.kt)("span",{parentName:"span",className:"msupsub"},(0,s.kt)("span",{parentName:"span",className:"vlist-t vlist-t2"},(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.3117em"}},(0,s.kt)("span",{parentName:"span",style:{top:"-2.55em",marginLeft:"0em",marginRight:"0.05em"}},(0,s.kt)("span",{parentName:"span",className:"pstrut",style:{height:"2.7em"}}),(0,s.kt)("span",{parentName:"span",className:"sizing reset-size6 size3 mtight"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal mtight"},"i")))),(0,s.kt)("span",{parentName:"span",className:"vlist-s"},"\u200b")),(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.15em"}},(0,s.kt)("span",{parentName:"span"})))))),(0,s.kt)("span",{parentName:"span",className:"mclose"},"}"))))),"."),(0,s.kt)("h3",{id:"twoperiodseach"},(0,s.kt)("strong",{parentName:"h3"},"TwoPeriodsEach")),(0,s.kt)("p",null,"The boundary is chosen such that, for each function, at least two full periods are plotted.\nIn math, ",(0,s.kt)("span",{parentName:"p",className:"math math-inline"},(0,s.kt)("span",{parentName:"span",className:"katex"},(0,s.kt)("span",{parentName:"span",className:"katex-mathml"},(0,s.kt)("math",{parentName:"span",xmlns:"http://www.w3.org/1998/Math/MathML"},(0,s.kt)("semantics",{parentName:"math"},(0,s.kt)("mrow",{parentName:"semantics"},(0,s.kt)("mo",{parentName:"mrow"},"\u22c1"),(0,s.kt)("mo",{parentName:"mrow",stretchy:"false"},"{"),(0,s.kt)("msub",{parentName:"mrow"},(0,s.kt)("mi",{parentName:"msub"},"T"),(0,s.kt)("mi",{parentName:"msub"},"i")),(0,s.kt)("mo",{parentName:"mrow"},"+"),(0,s.kt)("mn",{parentName:"mrow"},"2"),(0,s.kt)("mo",{parentName:"mrow"},"\u22c5"),(0,s.kt)("msub",{parentName:"mrow"},(0,s.kt)("mi",{parentName:"msub"},"d"),(0,s.kt)("mi",{parentName:"msub"},"i")),(0,s.kt)("mo",{parentName:"mrow",stretchy:"false"},"}")),(0,s.kt)("annotation",{parentName:"semantics",encoding:"application/x-tex"},"\\bigvee\\{ T_i + 2 \\cdot d_i \\}")))),(0,s.kt)("span",{parentName:"span",className:"katex-html","aria-hidden":"true"},(0,s.kt)("span",{parentName:"span",className:"base"},(0,s.kt)("span",{parentName:"span",className:"strut",style:{height:"1em",verticalAlign:"-0.25em"}}),(0,s.kt)("span",{parentName:"span",className:"mop op-symbol small-op",style:{position:"relative",top:"0em"}},"\u22c1"),(0,s.kt)("span",{parentName:"span",className:"mopen"},"{"),(0,s.kt)("span",{parentName:"span",className:"mord"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal",style:{marginRight:"0.13889em"}},"T"),(0,s.kt)("span",{parentName:"span",className:"msupsub"},(0,s.kt)("span",{parentName:"span",className:"vlist-t vlist-t2"},(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.3117em"}},(0,s.kt)("span",{parentName:"span",style:{top:"-2.55em",marginLeft:"-0.1389em",marginRight:"0.05em"}},(0,s.kt)("span",{parentName:"span",className:"pstrut",style:{height:"2.7em"}}),(0,s.kt)("span",{parentName:"span",className:"sizing reset-size6 size3 mtight"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal mtight"},"i")))),(0,s.kt)("span",{parentName:"span",className:"vlist-s"},"\u200b")),(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.15em"}},(0,s.kt)("span",{parentName:"span"})))))),(0,s.kt)("span",{parentName:"span",className:"mspace",style:{marginRight:"0.2222em"}}),(0,s.kt)("span",{parentName:"span",className:"mbin"},"+"),(0,s.kt)("span",{parentName:"span",className:"mspace",style:{marginRight:"0.2222em"}})),(0,s.kt)("span",{parentName:"span",className:"base"},(0,s.kt)("span",{parentName:"span",className:"strut",style:{height:"0.6444em"}}),(0,s.kt)("span",{parentName:"span",className:"mord"},"2"),(0,s.kt)("span",{parentName:"span",className:"mspace",style:{marginRight:"0.2222em"}}),(0,s.kt)("span",{parentName:"span",className:"mbin"},"\u22c5"),(0,s.kt)("span",{parentName:"span",className:"mspace",style:{marginRight:"0.2222em"}})),(0,s.kt)("span",{parentName:"span",className:"base"},(0,s.kt)("span",{parentName:"span",className:"strut",style:{height:"1em",verticalAlign:"-0.25em"}}),(0,s.kt)("span",{parentName:"span",className:"mord"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal"},"d"),(0,s.kt)("span",{parentName:"span",className:"msupsub"},(0,s.kt)("span",{parentName:"span",className:"vlist-t vlist-t2"},(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.3117em"}},(0,s.kt)("span",{parentName:"span",style:{top:"-2.55em",marginLeft:"0em",marginRight:"0.05em"}},(0,s.kt)("span",{parentName:"span",className:"pstrut",style:{height:"2.7em"}}),(0,s.kt)("span",{parentName:"span",className:"sizing reset-size6 size3 mtight"},(0,s.kt)("span",{parentName:"span",className:"mord mathnormal mtight"},"i")))),(0,s.kt)("span",{parentName:"span",className:"vlist-s"},"\u200b")),(0,s.kt)("span",{parentName:"span",className:"vlist-r"},(0,s.kt)("span",{parentName:"span",className:"vlist",style:{height:"0.15em"}},(0,s.kt)("span",{parentName:"span"})))))),(0,s.kt)("span",{parentName:"span",className:"mclose"},"}"))))),"."))}c.isMDXComponent=!0}}]);