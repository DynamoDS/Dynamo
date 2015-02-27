
//#define ENABLE_INC_DEC_FIX
using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;

-->namespace

public class Parser {
-->constants
	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;
	readonly bool builtinMethodsLoaded;
    private readonly ProtoCore.Core core;

-->declarations
	
	public Parser(Scanner scanner, ProtoCore.Core coreObj, bool _builtinMethodsLoaded = false) {
		this.scanner = scanner;
		errors = new Errors();
		errors.core = coreObj;
        core = coreObj;
		builtinMethodsLoaded = _builtinMethodsLoaded;
        commentNode = new CodeBlockNode();
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
-->pragmas
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

	
-->productions

	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
-->parseRoot
	}
	
	static readonly bool[,] set = {
-->initialization
	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public ProtoCore.Core core = null;
	//public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	//public System.IO.TextWriter warningStream = Console.Out;
	//public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
-->errors
			default: s = "error " + n; break;
		}
		// errorStream.WriteLine(errMsgFormat, line, col, s);
		core.BuildStatus.LogSyntaxError(s, core.CurrentDSFileName, line, col);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		//errorStream.WriteLine(errMsgFormat, line, col, s);
		core.BuildStatus.LogSyntaxError(s, core.CurrentDSFileName, line, col);
		count++;
	}
	
	public virtual void SemErr (string s) {
		//errorStream.WriteLine(s);
		core.BuildStatus.LogSyntaxError(s, core.CurrentDSFileName);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		// TODO: Jun/Jiong expand parser warnings.
		core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kParsing, s, core.CurrentDSFileName, line, col);
		//warningStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		// TODO: Jun/Jiong expand parser warnings.
		core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kParsing, s, core.CurrentDSFileName);
		//warningStream.WriteLine(String.Format("Warning: {0}",s));
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
