"use strict";(self.webpackChunknancy_docs=self.webpackChunknancy_docs||[]).push([[4981],{3905:(e,t,n)=>{n.d(t,{Zo:()=>s,kt:()=>d});var a=n(7294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function i(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function o(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?i(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):i(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,a,r=function(e,t){if(null==e)return{};var n,a,r={},i=Object.keys(e);for(a=0;a<i.length;a++)n=i[a],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);for(a=0;a<i.length;a++)n=i[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var u=a.createContext({}),c=function(e){var t=a.useContext(u),n=t;return e&&(n="function"==typeof e?e(t):o(o({},t),e)),n},s=function(e){var t=c(e.components);return a.createElement(u.Provider,{value:t},e.children)},p={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},m=a.forwardRef((function(e,t){var n=e.components,r=e.mdxType,i=e.originalType,u=e.parentName,s=l(e,["components","mdxType","originalType","parentName"]),m=c(n),d=r,y=m["".concat(u,".").concat(d)]||m[d]||p[d]||i;return n?a.createElement(y,o(o({ref:t},s),{},{components:n})):a.createElement(y,o({ref:t},s))}));function d(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var i=n.length,o=new Array(i);o[0]=m;var l={};for(var u in t)hasOwnProperty.call(t,u)&&(l[u]=t[u]);l.originalType=e,l.mdxType="string"==typeof e?e:r,o[1]=l;for(var c=2;c<i;c++)o[c]=n[c];return a.createElement.apply(null,o)}return a.createElement.apply(null,n)}m.displayName="MDXCreateElement"},1667:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>u,contentTitle:()=>o,default:()=>p,frontMatter:()=>i,metadata:()=>l,toc:()=>c});var a=n(7462),r=(n(7294),n(3905));const i={hide_title:!0,sidebar_position:3,title:"Operators"},o="Operators",l={unversionedId:"tutorials/first-tutorial/operators",id:"tutorials/first-tutorial/operators",title:"Operators",description:"The library implements the DNC operators for all representable curves, which include",source:"@site/docs/tutorials/first-tutorial/operators.md",sourceDirName:"tutorials/first-tutorial",slug:"/tutorials/first-tutorial/operators",permalink:"/nancy/docs/tutorials/first-tutorial/operators",draft:!1,tags:[],version:"current",sidebarPosition:3,frontMatter:{hide_title:!0,sidebar_position:3,title:"Operators"},sidebar:"tutorialSidebar",previous:{title:"Common types of curves",permalink:"/nancy/docs/tutorials/first-tutorial/common-types"},next:{title:"Simple computations",permalink:"/nancy/docs/tutorials/first-tutorial/Examples/simple-computations"}},u={},c=[{value:"Immutability",id:"immutability",level:2}],s={toc:c};function p(e){let{components:t,...n}=e;return(0,r.kt)("wrapper",(0,a.Z)({},s,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"operators"},"Operators"),(0,r.kt)("p",null,"The library implements the DNC operators for all representable curves, which include"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#minimumcurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Minimum"))," and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#maximumcurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Maximum"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#additioncurve"},(0,r.kt)("inlineCode",{parentName:"a"},"Addition"))," and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#subtractioncurve-boolean"},(0,r.kt)("inlineCode",{parentName:"a"},"Subtraction")),", the latter with option to have the result non-negative (default) or not"),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#convolutioncurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Convolution"))," and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#deconvolutioncurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Deconvolution"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#verticaldeviationcurve-curve"},(0,r.kt)("inlineCode",{parentName:"a"},"Vertical")),"- and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#horizontaldeviationcurve-curve"},(0,r.kt)("inlineCode",{parentName:"a"},"HorizontalDeviation"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#maxplusconvolutioncurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"MaxPlusConvolution"))," and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#maxplusdeconvolutioncurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"MaxPlusDeconvolution"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#lowerpseudoinverse"},(0,r.kt)("inlineCode",{parentName:"a"},"Lower")),"- and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#upperpseudoinverse"},(0,r.kt)("inlineCode",{parentName:"a"},"UpperPseudoInverse"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#subadditiveclosurecomputationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Sub")),"- and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#superadditiveclosurecomputationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"SuperAdditiveClosure"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#compositioncurve-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Composition")))),(0,r.kt)("p",null,"Common manipulations are also included:"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#delaybyrational"},(0,r.kt)("inlineCode",{parentName:"a"},"DelayBy"))," and ",(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#anticipatebyrational"},(0,r.kt)("inlineCode",{parentName:"a"},"AnticipateBy"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#verticalshiftrational-boolean"},(0,r.kt)("inlineCode",{parentName:"a"},"VerticalShift"))),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"/docs/nancy/Unipi.Nancy.MinPlusAlgebra/Curve#cutrational-rational-boolean-boolean-computationsettings"},(0,r.kt)("inlineCode",{parentName:"a"},"Cut"))," over finite interval")),(0,r.kt)("h2",{id:"immutability"},"Immutability"),(0,r.kt)("p",null,"Note that all the data structures used in Nancy are ",(0,r.kt)("em",{parentName:"p"},"immutable"),".\nThis means that methods will not change the operands, instead they will create and return a new object that contains the result."),(0,r.kt)("p",null,"The main benefit of this approach is that you can safely use objects in multiple contexts and concurrently, and never worry about concurrent writes to your variables."))}p.isMDXComponent=!0}}]);