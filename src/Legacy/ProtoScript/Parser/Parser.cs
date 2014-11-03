
using System;

namespace ProtoScript {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _char = 5;
	public const int _langblocktrail = 6;
	public const int _openbracket = 7;
	public const int _closebracket = 8;
	public const int maxT = 11;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public ProtoCore.Script script{ get; set; }

	public Parser()
	{
	}


//================================================
// Coco-defined Language agnostic
//================================================



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void ProtoScript() {
		script = new ProtoCore.Script(); 
		while (la.kind == 7) {
			ProtoCore.LanguageCodeBlock codeblock = new ProtoCore.LanguageCodeBlock(); 
			Get();
			Expect(1);
            if (0 == t.val.CompareTo(ProtoCore.DSASM.kw.imperative))
            {
			    codeblock.language = ProtoCore.Language.kImperative;
			}
            else if (0 == t.val.CompareTo(ProtoCore.DSASM.kw.associative))
            {
			    codeblock.language = ProtoCore.Language.kAssociative;
			}
			
			while (la.kind == 9) {
				Get();
				string key; 
				Expect(1);
				key = t.val; 
				Expect(10);
				Expect(4);
				if ("fingerprint" == key)
				{	
				codeblock.fingerprint = t.val; 
				codeblock.fingerprint = codeblock.fingerprint.Remove(0,1); 
				codeblock.fingerprint = codeblock.fingerprint.Remove(codeblock.fingerprint.Length-1,1); 
				}
				else if ("version" == key)
				{
				codeblock.version = t.val; 
				codeblock.version = codeblock.version.Remove(0,1); 
				codeblock.version = codeblock.version.Remove(codeblock.version.Length-1,1); 
				}
				
			}
			Expect(8);
			Expect(6);
			codeblock.body = t.val; 
			codeblock.body = codeblock.body.Remove(0, 2);
			codeblock.body = codeblock.body.Remove(codeblock.body.Length - 2, 2);
			
			script.codeblockList.Add(codeblock); 
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		ProtoScript();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x}

	};
} // end Parser


public class Errors {
	public int count;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "float expected"; break;
			case 4: s = "textstring expected"; break;
			case 5: s = "char expected"; break;
			case 6: s = "langblocktrail expected"; break;
			case 7: s = "openbracket expected"; break;
			case 8: s = "closebracket expected"; break;
			case 9: s = "\",\" expected"; break;
			case 10: s = "\"=\" expected"; break;
			case 11: s = "??? expected"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}