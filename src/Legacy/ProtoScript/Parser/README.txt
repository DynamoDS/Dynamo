In NextToken() method in Scanner.cs in ProtoScript, the following changes need to be made

Declare a new variable: 'int nestingLevel = 0;'

/* ------------------------------------------------------------------------- */
Change Case 11 to

if (ch == '#') { AddCh(); nestingLevel++; goto case 12; }
	else {goto case 12;}
		
/* ------------------------------------------------------------------------- */
Change Case 12 to

case 12:
	if (ch == '<') { AddCh(); goto case 11; }
	if (ch <= '"' || ch >= '$' && ch <= 65535) {AddCh(); goto case 12;}
	else if (ch == '#') {AddCh(); goto case 16;}
	else {goto case 0;}

/* ------------------------------------------------------------------------- */
Change Case 17 to

case 17:
	recEnd = pos; recKind = 6;
	nestingLevel--;
	if (nestingLevel == 0) { goto case 0; }
	if (ch <= '"' || ch >= '$' && ch <= 65535) {AddCh(); goto case 12;}
	else if (ch == '#') {AddCh(); goto case 16;}
	else {t.kind = 6; break;}



