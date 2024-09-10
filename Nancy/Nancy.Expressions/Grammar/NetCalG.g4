grammar NetCalG;

@header {
#pragma warning disable 3021
}

equivalenceCatalog: (equivalence NEWLINE)+;

equivalence: '{' NEWLINE 
                (hypothesis SEMICOLON NEWLINE)* 
             '}' '==>' equivalenceExpression;

equivalenceExpression: curveExpression EQUAL curveExpression;

hypothesis
    : placeholder IN set (property)*
    | placeholder CONV placeholder WELL_DEFINED
    | placeholder relationalOperator placeholder
    ;

set: U_SET | RATIONALS;

placeholder: ID;

property
    : NON_DECREASING                                        #nonDecreasingProperty
    | NON_NEGATIVE                                          #nonNegativeProperty
    | SUBADDITIVE                                           #subadditiveProperty
    | CONVEX                                                #convexProperty
    | CONCAVE                                               #concaveProperty
    | LEFT_CONTINUOUS                                       #leftContinuousProperty
    | RIGHT_CONTINUOUS                                      #rightContinuousProperty
    | ZERO_AT_ZERO                                          #zeroAtZeroProperty
    | ULTIMATELY_CONSTANT                                   #ultimatelyConstant
    ;

curveExpression
    : curveExpression CONV curveExpression                  #convolutionExpression
    | curveExpression DECONV curveExpression                #deconvolutionExpression
    | curveExpression MAXPLUSCONV curveExpression           #maxPlusConvolutionExpression
    | curveExpression MAXPLUSDECONV curveExpression         #maxPlusDeconvolutionExpression
    | curveExpression MIN curveExpression                   #minimumExpression
    | curveExpression MAX curveExpression                   #maximumExpression
    | curveExpression ADD curveExpression                   #additionExpression
    | curveExpression SUB curveExpression                   #subtractionExpression
    | curveExpression COMP curveExpression                  #compositionExpression
    | constantCurve                                         #constantCurveExpression
    | '(' curveExpression ')'                               #parenthesizedExpression
    ;

constantCurve: placeholder;

relationalOperator : GREATER_OR_EQUAL | LESS_THAN_OR_EQUAL;

CONV                : '*' | 'conv' | '\\otimes';
DECONV              : 'deconv' | '\\oslash';
MAXPLUSCONV         : 'maxpconv' | '\\overline{\\otimes}';
MAXPLUSDECONV       : 'maxpdeconv' | '\\overline{\\oslash}';
ADD                 : '+';
SUB                 : '-';
MIN                 : 'min' | '/\\' | '\\wedge';
MAX                 : 'max' | '\\/' | '\\vee';
COMP                : '°';

NON_DECREASING      : 'non-decreasing';
NON_NEGATIVE        : 'non-negative';
SUBADDITIVE         : 'subadditive';
CONVEX              : 'convex';
CONCAVE             : 'concave';
LEFT_CONTINUOUS     : 'left-continuous';
RIGHT_CONTINUOUS    : 'right-continuous';
ZERO_AT_ZERO        : 'zero-at-zero';
ULTIMATELY_CONSTANT : 'ultimately-constant';
WELL_DEFINED        : 'well-defined';

U_SET               : 'U';
RATIONALS           : 'Q';

GREATER_OR_EQUAL    : '>=';
LESS_THAN_OR_EQUAL  : '<=';
EQUAL               : '=';

IN                  : 'in';
ID                  : LETTER (LETTER | DIGIT)*;
SEMICOLON           : ';';
NEWLINE             : '\r'? '\n' ;
WHITESPACE          : [ \t]+ -> skip ;


fragment LETTER 	: 'a'..'z' | 'A'..'Z' |	'_'	;

fragment DIGIT  	: '0'..'9';
