In NextToken() method in Scanner.cs in ProtoAssociative, the following changes need to be made

Declare a new variable: 'int nestingLevel = 0;'

/* ------------------------------------------------------------------------- */
Change Case 20 to:

case 20:
	if (ch == '<') { AddCh(); goto case 37; }
	if (ch <= '"' || ch >= '$' && ch <= 65535) {AddCh(); goto case 20;}
	else if (ch == '#') {AddCh(); goto case 23;}
	else {goto case 0;}
	
/* ------------------------------------------------------------------------- */
Change Case 24 to: 

case 24:
	recEnd = pos; recKind = 31;
	nestingLevel--;
	if (nestingLevel == 0) { goto case 0; }
	if (ch <= '"' || ch >= '$' && ch <= 65535) {AddCh(); goto case 20;}
	else if (ch == '#') {AddCh(); goto case 23;}
	else {t.kind = 31; break;}
		
/* ------------------------------------------------------------------------- */
Change Case 37 to

case 37:
	recEnd = pos; recKind = 16;
	if (ch == '#') { AddCh(); nestingLevel++; goto case 20; }
	else if (ch == '=') { AddCh(); if (nestingLevel == 0) { goto case 32; } else { goto case 20; } }
	else { goto case 20; }