In NextToken() method in Scanner.cs in ProtoImperative, the following changes need to be made

Declare a new variable: 'int nestingLevel = 0;'

/* ------------------------------------------------------------------------- */
Change Case 21 to

case 21:
                if (ch == '<') { AddCh(); goto case 24; }
				if (ch <= '"' || ch >= '$' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch == '#') {AddCh(); goto case 25;}
				else {goto case 0;}
	
/* ------------------------------------------------------------------------- */
Change Case 24 to

case 24:
				recEnd = pos; recKind = 11;
				if (ch == '=') {AddCh(); goto case 16;}
                else if (ch == '#') { AddCh(); nestingLevel++;  goto case 21; }
                else { if (nestingLevel == 0) { goto case 0; } else { goto case 21; } }

/* ------------------------------------------------------------------------- */
Change Case 26 to

case 26:
				recEnd = pos; recKind = 26;
                nestingLevel--;
                if (nestingLevel == 0) { goto case 0; }
				if (ch <= '"' || ch >= '$' && ch <= 65535) {AddCh(); goto case 21;}
				else if (ch == '#') {AddCh(); goto case 25;}
				else {t.kind = 26; break;}