"use strict";(self.webpackChunknancy_docs=self.webpackChunknancy_docs||[]).push([[5598],{3905:(t,e,r)=>{r.d(e,{Zo:()=>u,kt:()=>f});var n=r(7294);function o(t,e,r){return e in t?Object.defineProperty(t,e,{value:r,enumerable:!0,configurable:!0,writable:!0}):t[e]=r,t}function i(t,e){var r=Object.keys(t);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(t);e&&(n=n.filter((function(e){return Object.getOwnPropertyDescriptor(t,e).enumerable}))),r.push.apply(r,n)}return r}function a(t){for(var e=1;e<arguments.length;e++){var r=null!=arguments[e]?arguments[e]:{};e%2?i(Object(r),!0).forEach((function(e){o(t,e,r[e])})):Object.getOwnPropertyDescriptors?Object.defineProperties(t,Object.getOwnPropertyDescriptors(r)):i(Object(r)).forEach((function(e){Object.defineProperty(t,e,Object.getOwnPropertyDescriptor(r,e))}))}return t}function l(t,e){if(null==t)return{};var r,n,o=function(t,e){if(null==t)return{};var r,n,o={},i=Object.keys(t);for(n=0;n<i.length;n++)r=i[n],e.indexOf(r)>=0||(o[r]=t[r]);return o}(t,e);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(t);for(n=0;n<i.length;n++)r=i[n],e.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(t,r)&&(o[r]=t[r])}return o}var s=n.createContext({}),c=function(t){var e=n.useContext(s),r=e;return t&&(r="function"==typeof t?t(e):a(a({},e),t)),r},u=function(t){var e=c(t.components);return n.createElement(s.Provider,{value:e},t.children)},p={inlineCode:"code",wrapper:function(t){var e=t.children;return n.createElement(n.Fragment,{},e)}},d=n.forwardRef((function(t,e){var r=t.components,o=t.mdxType,i=t.originalType,s=t.parentName,u=l(t,["components","mdxType","originalType","parentName"]),d=c(r),f=o,y=d["".concat(s,".").concat(f)]||d[f]||p[f]||i;return r?n.createElement(y,a(a({ref:e},u),{},{components:r})):n.createElement(y,a({ref:e},u))}));function f(t,e){var r=arguments,o=e&&e.mdxType;if("string"==typeof t||o){var i=r.length,a=new Array(i);a[0]=d;var l={};for(var s in e)hasOwnProperty.call(e,s)&&(l[s]=e[s]);l.originalType=t,l.mdxType="string"==typeof t?t:o,a[1]=l;for(var c=2;c<i;c++)a[c]=r[c];return n.createElement.apply(null,a)}return n.createElement.apply(null,r)}d.displayName="MDXCreateElement"},1825:(t,e,r)=>{r.r(e),r.d(e,{assets:()=>c,contentTitle:()=>l,default:()=>d,frontMatter:()=>a,metadata:()=>s,toc:()=>u});var n=r(7462),o=(r(7294),r(3905)),i=r(4996);const a={hide_title:!0,sidebar_position:1,title:"Introduction"},l="Introduction",s={unversionedId:"tutorials/introduction",id:"tutorials/introduction",title:"Introduction",description:"",source:"@site/docs/tutorials/introduction.mdx",sourceDirName:"tutorials",slug:"/tutorials/introduction",permalink:"/nancy/docs/tutorials/introduction",draft:!1,tags:[],version:"current",sidebarPosition:1,frontMatter:{hide_title:!0,sidebar_position:1,title:"Introduction"},sidebar:"tutorialSidebar",next:{title:"Installation",permalink:"/nancy/docs/tutorials/getting-started/installation"}},c={},u=[],p={toc:u};function d(t){let{components:e,...r}=t;return(0,o.kt)("wrapper",(0,n.Z)({},p,r,{components:e,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"introduction"},"Introduction"),(0,o.kt)("p",null,"Nancy is a flexible C# library for Network Calculus computations.\nIt implements min-plus and max-plus operators, and handles ultimately pseudo-periodic piecewise affine curves."),(0,o.kt)("figure",null,(0,o.kt)("img",{src:(0,i.Z)("/img/tutorials/generic example.png"),alt:"Plot of a generic NC curve"}),(0,o.kt)("figcaption",null,"Example of a generic NC curve, supported by this library")),(0,o.kt)("p",null,"It has been developed by ",(0,o.kt)("a",{parentName:"p",href:"https://rzippo.github.io/publications/"},"Raffaele Zippo")," and ",(0,o.kt)("a",{parentName:"p",href:"http://www.iet.unipi.it/g.stea/"},"Giovanni Stea")," (University of Pisa, Italy), partly during a joint research project with Arm LTD, Cambridge (UK).\nThe authors would like to thank the Arm staff involved in that project, and particularly Dr. Matteo Andreozzi, for useful discussions and suggestions."),(0,o.kt)("p",null,"You can use this library to quickly write, compute and plot Network Calculus expressions, or to build larger analysis tools with its performant computations.\nNancy is released under the MIT license."),(0,o.kt)("p",null,"For a first introduction on how it works, see the ",(0,o.kt)("a",{parentName:"p",href:"/nancy/docs/tutorials/first-tutorial/upp-types"},"First Tutorial")," section."),(0,o.kt)("p",null,"To try it by yourself, see the ",(0,o.kt)("a",{parentName:"p",href:"/nancy/docs/tutorials/getting-started/installation"},"Getting Started")," section."))}d.isMDXComponent=!0}}]);