grammar WarehouseFilterGrammar;

parse
 : expression EOF
 ;

expression
 : methodCall                                     #methodCallExpression  
 | LPAREN expression RPAREN                       #parenExpression
 | left=expression op=comparator right=expression #comparatorExpression
 | left=expression op=binary right=expression     #binaryExpression
 | bool                                           #boolExpression
 | NOT expression                                 #notExpression
 | IDENTIFIER                                     #identifierExpression
 | DECIMAL                                        #decimalExpression
 | STRING                                         #stringExpression
 ;

methodCall
 : IDENTIFIER LPAREN argumentList? RPAREN
 ;

comparator
 : GT | GE | LT | LE | EQ | NE
 ;

binary
 : AND | OR
 ;

bool
 : TRUE | FALSE
 ;

argumentList
 : expression (COMMA expression)?
 ;


AND        : 'AND' | '&';
OR         : 'OR' | '|';
NOT        : 'NOT' | '!';
TRUE       : 'TRUE' | 'true';
FALSE      : 'FALSE' | 'false';
GT         : '>' ;
GE         : '>=' ;
NE         : '!=' ;
LT         : '<' ;
LE         : '<=' ;
EQ         : '=' ;
LPAREN     : '(' ;
RPAREN     : ')' ;
DECIMAL    : '-'? [0-9]+ ( '.' [0-9]+ )? ;
IDENTIFIER : [a-zA-Z_] [a-zA-Z_0-9]* ( '.' [a-zA-Z_0-9]+)? ;
WS         : [ \r\t\u000C\n]+ -> skip;
Q          : '"';
STRING     : Q ~["\r\n]* Q;
COMMA      : ',';