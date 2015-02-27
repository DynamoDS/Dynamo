using NodeList = System.Collections.Generic.List<ProtoImperative.AST.Node>;
using ProtoImperative.AST;



using System;
using System.Text;
using System.IO;
using Operator = ProtoCore.DSASM.Operator;
using UnaryOperator = ProtoCore.DSASM.UnaryOperator;
using RangeStepOperator = ProtoCore.DSASM.RangeStepOperator;

namespace Imperative {



public class Parser : ProtoCore.ParserBase{
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _char = 5;
	public const int _openbracket = 6;
	public const int _closebracket = 7;
	public const int _openparen = 8;
	public const int _closeparen = 9;
	public const int _rel_gt = 10;
	public const int _rel_lt = 11;
	public const int _rel_ge = 12;
	public const int _rel_le = 13;
	public const int _rel_eq = 14;
	public const int _rel_nq = 15;
	public const int _endline = 16;
	public const int _rangeop = 17;
	public const int _kw_funcdef = 18;
	public const int _kw_if = 19;
	public const int _kw_elseif = 20;
	public const int _kw_else = 21;
	public const int _kw_while = 22;
	public const int _kw_for = 23;
	public const int _kw_constructor = 24;
	public const int _kw_heap_alloc = 25;
	public const int _langblocktrail = 26;
	public const int _literal_true = 27;
	public const int _literal_false = 28;
	public const int _literal_null = 29;
	public const int maxT = 50;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

    private ProtoCore.Core core = null;

public override ProtoCore.NodeBase codeblock {get;set;}
	private int localVarCount = 0;
	private bool isGlobalScope = true;

	public Parser()
	{
	}

	//====================================
	// LL(k) utils
	//====================================
	private bool isArrayAccess()
	{
		Token pt = la;
		if( _ident == pt.kind ) 
		{
			pt = scanner.Peek();
            scanner.ResetPeek();
			if( _openbracket == pt.kind ) {
				return true;
			}
		}
		return false;
	}
	
	private bool isFunctionCall()
	{
		Token pt = la;
		if( _ident == pt.kind ) 
		{
			pt = scanner.Peek();
			scanner.ResetPeek();
			if( _openparen == pt.kind ) {
				return true;
			}
		}
		return false;
	}
   
	private bool hasReturnType()
    {
        Token pt = la;
        if( _ident == pt.kind ) 
        {
            pt = scanner.Peek();
            scanner.ResetPeek();
            if( _ident == pt.kind ) {
                return true;
            }
        }
        return false;
    }
    
	private bool isVariableDecl()
    {
        Token pt = la;
        if( _ident == pt.kind ) 
        {
            pt = scanner.Peek();
            scanner.ResetPeek();
            if( _ident == pt.kind ) {
                return true;
            }
        }

        if (_kw_heap_alloc == pt.kind)
        {
            pt = scanner.Peek();
            if (_ident == pt.kind)
            {
                pt = scanner.Peek();
                if (_ident == pt.kind)
                {
                    scanner.ResetPeek();
                    return true;
                }
            }
        }
        scanner.ResetPeek();
        return false;
    }

    private void ParseLanguageBlockNode(LanguageBlockNode langblock)
    {     
        if (!core.langverify.Verify(langblock.codeblock))
        {
            return;
        }

        ProtoCore.ParserBase parser = null;
        System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(langblock.codeblock.body));

        if (langblock.codeblock.language ==  ProtoCore.Language.kImperative) 
        {
            parser = new Imperative.Parser(new Imperative.Scanner(memstream), core);
        }
        else if (langblock.codeblock.language == ProtoCore.Language.kAssociative) 
        {
            parser = new Associative.Parser(new Associative.Scanner(memstream), core);
        }

        try
        {
            parser.errorStream = new System.IO.StringWriter();
            parser.Parse();

            if (parser.errorStream.ToString() != String.Empty)
                core.BuildStatus.LogSyntaxError(parser.errorStream.ToString());
            core.BuildStatus.errorCount += parser.errorCount;  

            langblock.codeBlockNode = parser.codeblock;
        }
        catch (ProtoCore.BuildHaltException e)
        {
            System.Console.WriteLine(e.errorMsg);
        }  
    }



	public Parser(Scanner scanner, ProtoCore.Core coreObj) {
		this.scanner = scanner;
		errors = new Errors();
        core = coreObj;
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

	
	void DSImperative() {
		Node node = null; 
		codeblock = new CodeBlockNode();
		
		while (StartOf(1)) {
			if (StartOf(2)) {
				stmt(out node);
			} else {
				functiondecl(out node);
			}
			if (null != node)	
			(codeblock as CodeBlockNode).Body.Add(node); 
			
		}
	}

	void stmt(out Node node) {
		node = null; 
		if (isFunctionCall()) {
			functioncall(out node);
			Expect(16);
		} else if (la.kind == 1) {
			assignstmt(out node);
		} else if (la.kind == 19) {
			ifstmt(out node);
		} else if (la.kind == 22) {
			whilestmt(out node);
		} else if (la.kind == 23) {
			forloop(out node);
		} else if (la.kind == 6) {
			languageblock(out node);
		} else if (la.kind == 16) {
			Get();
		} else SynErr(51);
	}

	void functiondecl(out Node node) {
		FunctionDefinitionNode funcDecl = new FunctionDefinitionNode(); 
		ProtoCore.Type rtype = new ProtoCore.Type(); rtype.Name = "var"; rtype.UID = 0; 
		Expect(18);
		Expect(1);
		funcDecl.Name = t.val; 
		if (la.kind == 36) {
			Get();
			ReturnType(out rtype);
		}
		funcDecl.ReturnType = rtype; 
		Expect(8);
		if (la.kind == 1 || la.kind == 25) {
			ArgumentSignatureNode args = new ArgumentSignatureNode(); 
			Node argdecl; 
			ArgDecl(out argdecl);
			args.AddArgument(argdecl as VarDeclNode); 
			while (la.kind == 30) {
				Get();
				ArgDecl(out argdecl);
				args.AddArgument(argdecl as VarDeclNode); 
			}
			funcDecl.Signature = args; 
		}
		Expect(9);
		isGlobalScope = false; 
		Expect(32);
		funcDecl.FunctionBody = new CodeBlockNode(); 
		NodeList body = new NodeList();
		
		stmtlist(out body);
		Expect(33);
		funcDecl.localVars = localVarCount;
		funcDecl.FunctionBody.Body = body;
		node = funcDecl; 
		
		isGlobalScope = true;
		localVarCount=  0;
		
	}

	void languageblock(out Node node) {
		node = null; 
		LanguageBlockNode langblock = new LanguageBlockNode(); 
		
		Expect(6);
		Expect(1);
		if( 0 == t.val.CompareTo("Imperative")) {
		langblock.codeblock.language = ProtoCore.Language.kImperative;
		}
		else if( 0 == t.val.CompareTo("Associative")) {
		langblock.codeblock.language = ProtoCore.Language.kAssociative; 
		}
		
		while (la.kind == 30) {
			Get();
			string key; 
			Expect(1);
			key = t.val; 
			Expect(31);
			Expect(4);
			if ("fingerprint" == key)
			{
			langblock.codeblock.fingerprint = t.val; 
			langblock.codeblock.fingerprint = langblock.codeblock.fingerprint.Remove(0,1); 
			langblock.codeblock.fingerprint = langblock.codeblock.fingerprint.Remove(langblock.codeblock.fingerprint.Length-1,1); 
			}
			else if ("version" == key)
			{
			langblock.codeblock.version = t.val; 
			langblock.codeblock.version = langblock.codeblock.version.Remove(0,1); 
			langblock.codeblock.version = langblock.codeblock.version.Remove(langblock.codeblock.version.Length-1,1);
			}
			
		}
		Expect(7);
		Expect(26);
		langblock.codeblock.body = t.val.Substring(2,t.val.Length-4); 											
		node = langblock;
		                                ParseLanguageBlockNode(langblock);
		
	}

	void functioncall(out Node node) {
		Expect(1);
		IdentifierNode function = new IdentifierNode() { Value = t.val, Name = t.val }; 
		NodeList arglist = new NodeList(); 
		Expect(8);
		if (StartOf(3)) {
			Node argNode; 
			expr(out argNode);
			arglist.Add(argNode); 
			while (la.kind == 30) {
				Get();
				expr(out argNode);
				arglist.Add(argNode); 
			}
		}
		Expect(9);
		FunctionCallNode funcNode = new FunctionCallNode(); 
		funcNode.Function = function;
		funcNode.FormalArguments = arglist;
		node = funcNode; 
		
	}

	void assignstmt(out Node node) {
		node = null; 
		Node lhsNode = null; 
		if (isArrayAccess()) {
			arrayident(out lhsNode);
		} else if (la.kind == 1) {
			Get();
			int ltype = (0 == String.Compare(t.val, "return")) ? (int)ProtoCore.PrimitiveType.kTypeReturn : (int)ProtoCore.PrimitiveType.kTypeVar;
			lhsNode = new ProtoImperative.AST.IdentifierNode() 
			{ 
			Value = t.val,
			  type = ltype,
			  datatype = (ProtoCore.PrimitiveType)ltype
			}; 
			
		} else SynErr(52);
		Expect(31);
		Node rhsNode; 
		if (StartOf(3)) {
			expr(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = lhsNode;
			bNode.RightNode = rhsNode;
			bNode.Optr = Operator.assign;
			node = bNode;		
			
			Expect(16);
		} else if (la.kind == 6) {
			languageblock(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = lhsNode;
			bNode.RightNode = rhsNode;
			bNode.Optr = Operator.assign;
			node = bNode;		
			
		} else SynErr(53);
	}

	void ifstmt(out Node node) {
		IfStmtNode ifStmtNode = new IfStmtNode(); 
		NodeList body = null; 
		Expect(19);
		Expect(8);
		expr(out node);
		ifStmtNode.IfExprNode = node; 
		Expect(9);
		if (StartOf(2)) {
			Node singleStmt; 
			stmt(out singleStmt);
			ifStmtNode.IfBody.Add(singleStmt); 
		} else if (la.kind == 32) {
			Get();
			stmtlist(out body);
			ifStmtNode.IfBody = body; 
			Expect(33);
		} else SynErr(54);
		while (la.kind == 20) {
			ElseIfBlock elseifBlock = new ElseIfBlock(); 
			Get();
			Expect(8);
			expr(out node);
			elseifBlock.Expr = node; 
			Expect(9);
			if (StartOf(2)) {
				Node singleStmt = null; 
				stmt(out singleStmt);
				elseifBlock.Body.Add(singleStmt); 
			} else if (la.kind == 32) {
				Get();
				stmtlist(out body);
				elseifBlock.Body = body; 
				Expect(33);
			} else SynErr(55);
			ifStmtNode.ElseIfList.Add(elseifBlock); 
		}
		if (la.kind == 21) {
			Get();
			if (StartOf(2)) {
				Node singleStmt = null; 
				stmt(out singleStmt);
				ifStmtNode.ElseBody.Add(singleStmt); 
			} else if (la.kind == 32) {
				Get();
				stmtlist(out body);
				ifStmtNode.ElseBody = body; 
				Expect(33);
			} else SynErr(56);
		}
		node = ifStmtNode; 
	}

	void whilestmt(out Node node) {
		WhileStmtNode whileStmtNode = new WhileStmtNode(); 
		NodeList body = null; 
		Expect(22);
		Expect(8);
		expr(out node);
		whileStmtNode.Expr = node; 
		Expect(9);
		Expect(32);
		stmtlist(out body);
		whileStmtNode.Body = body; 
		Expect(33);
		node = whileStmtNode; 
	}

	void forloop(out Node forloop) {
		Node node;
		ForLoopNode loopNode = new ForLoopNode();
		NodeList body = null;   
		
		Expect(23);
		Expect(8);
		arrayident(out node);
		loopNode.id = node; 
		Expect(34);
		expr(out node);
		loopNode.expression = node; 
		Expect(9);
		if (StartOf(2)) {
			Node singleStmt = null; 
			stmt(out singleStmt);
			loopNode.body.Add(singleStmt); 
		} else if (la.kind == 32) {
			Get();
			stmtlist(out body);
			loopNode.body = body; 
			Expect(33);
		} else SynErr(57);
		forloop = loopNode; 
	}

	void stmtlist(out NodeList nodelist) {
		nodelist = new NodeList(); 
		while (StartOf(2)) {
			Node node = null; 
			stmt(out node);
			nodelist.Add(node); 
		}
	}

	void arrayident(out Node node) {
		Ident(out node);
		if (la.kind == 6) {
			Get();
			IdentifierNode var = node as IdentifierNode; 
			node = null; 
			if (StartOf(3)) {
				expr(out node);
			}
			ArrayNode array = new ArrayNode();
			array.Expr = node; 
			array.Type = null;
			var.ArrayDimensions = array; 
			
			Expect(7);
			while (la.kind == 6) {
				Get();
				node = null; 
				if (StartOf(3)) {
					expr(out node);
				}
				ArrayNode array2 = new ArrayNode();
				array2.Expr = node; 
				array2.Type = null;
				array.Type = array2;
				array = array2;
				
				Expect(7);
			}
			node = var; 
		}
	}

	void expr(out Node node) {
		node = null; 
		if (la.kind == 47 || la.kind == 48) {
			unaryexpr(out node);
		} else if (StartOf(4)) {
			binexpr(out node);
		} else SynErr(58);
		while (la.kind == 35) {
			TernaryOp(ref node);
		}
	}

	void unaryexpr(out Node node) {
		node = null; 
		UnaryOperator op; 
		Node exprNode; 
		unaryop(out op);
		expr(out exprNode);
		UnaryExpressionNode unary = new UnaryExpressionNode(); 
		unary.Operator = op;
		unary.Expression = exprNode;
		node = unary;
		
	}

	void binexpr(out Node node) {
		node = null;
		logicalexpr(out node);
		while (la.kind == 44 || la.kind == 45) {
			Operator op; 
			logicalop(out op);
			Node rhsNode = null; 
			expr(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			node = bNode;
			
		}
	}

	void TernaryOp(ref Node node) {
		InlineConditionalNode inlineConNode = new InlineConditionalNode(); 
		Expect(35);
		inlineConNode.ConditionExpression = node; node = null; 
		expr(out node);
		inlineConNode.TrueExpression = node; 
		Expect(36);
		node = null; 
		expr(out node);
		inlineConNode.FalseExpression = node; 
		node = inlineConNode; 
	}

	void identifierList(out Node node) {
		node = null; 
		NameReference(out node);
		while (la.kind == 37) {
			Get();
			Node rnode = null; 
			NameReference(out rnode);
			IdentifierListNode bnode = new IdentifierListNode(); 
			bnode.LeftNode = node; 
			bnode.Optr = Operator.dot; 
			bnode.RightNode = rnode; 
			node = bnode; 
			
		}
	}

	void NameReference(out Node node) {
		node = null; 
		if (isFunctionCall()) {
			functioncall(out node);
		} else if (la.kind == 1) {
			arrayident(out node);
		} else SynErr(59);
	}

	void unaryop(out UnaryOperator op) {
		op = UnaryOperator.None; 
		if (la.kind == 47) {
			Get();
			op = UnaryOperator.Not; 
		} else if (la.kind == 48) {
			Get();
			op = UnaryOperator.Negate; 
		} else SynErr(60);
	}

	void logicalexpr(out Node node) {
		node = null;
		RangeExpr(out node);
		while (StartOf(5)) {
			Operator op; 
			relop(out op);
			Node rhsNode = null; 
			RangeExpr(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			node = bNode;
			
		}
	}

	void logicalop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 44) {
			Get();
			op = Operator.and; 
		} else if (la.kind == 45) {
			Get();
			op = Operator.or; 
		} else SynErr(61);
	}

	void RangeExpr(out Node node) {
		rel(out node);
		if (la.kind == 17) {
			RangeExprNode rnode = new RangeExprNode(); 
			rnode.FromNode = node;
			
			Get();
			rel(out node);
			rnode.ToNode = node; 
			if (la.kind == 17) {
				RangeStepOperator op; 
				Get();
				rangeStepOperator(out op);
				rnode.stepoperator = op; 
				rel(out node);
				rnode.StepNode = node; 
			}
			node = rnode; 
		}
	}

	void relop(out Operator op) {
		op = Operator.none; 
		switch (la.kind) {
		case 10: {
			Get();
			op = Operator.gt; 
			break;
		}
		case 11: {
			Get();
			op = Operator.lt; 
			break;
		}
		case 12: {
			Get();
			op = Operator.ge; 
			break;
		}
		case 13: {
			Get();
			op = Operator.le; 
			break;
		}
		case 14: {
			Get();
			op = Operator.eq; 
			break;
		}
		case 15: {
			Get();
			op = Operator.nq; 
			break;
		}
		default: SynErr(62); break;
		}
	}

	void rel(out Node node) {
		node = null;
		term(out node);
		while (la.kind == 38 || la.kind == 46) {
			Operator op; 
			addop(out op);
			Node rhsNode; 
			term(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			node = bNode;
			
		}
	}

	void rangeStepOperator(out RangeStepOperator op) {
		op = RangeStepOperator.stepsize; 
		if (la.kind == 48 || la.kind == 49) {
			if (la.kind == 49) {
				Get();
				op = RangeStepOperator.num; 
			} else {
				Get();
				op = RangeStepOperator.approxsize; 
			}
		}
	}

	void term(out Node node) {
		node = null;
		interimfactor(out node);
		while (la.kind == 39 || la.kind == 40) {
			Operator op; 
			mulop(out op);
			Node rhsNode; 
			interimfactor(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			node = bNode;
			
		}
	}

	void addop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 46) {
			Get();
			op = Operator.add; 
		} else if (la.kind == 38) {
			Get();
			op = Operator.sub; 
		} else SynErr(63);
	}

	void interimfactor(out Node node) {
		node = null;
		factor(out node);
		while (la.kind == 41 || la.kind == 42 || la.kind == 43) {
			Operator op; 
			bitop(out op);
			Node rhsNode; 
			factor(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			node = bNode;
			
		}
	}

	void mulop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 39) {
			Get();
			op = Operator.mul; 
		} else if (la.kind == 40) {
			Get();
			op = Operator.div; 
		} else SynErr(64);
	}

	void factor(out Node node) {
		node = null; 
		if (la.kind == 2 || la.kind == 3 || la.kind == 38) {
			num(out node);
		} else if (isFunctionCall()) {
			functioncall(out node);
		} else if (isArrayAccess()) {
			arrayident(out node);
		} else if (la.kind == 27) {
			Get();
			node = new BooleanNode() { value = ProtoCore.DSASM.Literal.True }; 
		} else if (la.kind == 28) {
			Get();
			node = new BooleanNode() { value = ProtoCore.DSASM.Literal.False }; 
		} else if (la.kind == 29) {
			Get();
			node = new NullNode(); 
		} else if (la.kind == 32) {
			Get();
			ExprListNode exprlist = new ExprListNode(); 
			if (StartOf(3)) {
				expr(out node);
				exprlist.list.Add(node); 
				while (la.kind == 30) {
					Get();
					expr(out node);
					exprlist.list.Add(node); 
				}
			}
			Expect(33);
			node = exprlist; 
		} else if (la.kind == 8) {
			Get();
			expr(out node);
			Expect(9);
		} else if (la.kind == 1) {
			identifierList(out node);
		} else SynErr(65);
	}

	void bitop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 41) {
			Get();
			op = Operator.bitwiseand; 
		} else if (la.kind == 42) {
			Get();
			op = Operator.bitwiseor; 
		} else if (la.kind == 43) {
			Get();
			op = Operator.bitwisexor; 
		} else SynErr(66);
	}

	void num(out Node node) {
		node = null; String localvalue = String.Empty; 
		if (la.kind == 38) {
			Get();
			localvalue = "-"; 
		}
		if (la.kind == 2) {
			Get();
			node = new IntNode() { value = localvalue + t.val }; 
		} else if (la.kind == 3) {
			Get();
			node = new DoubleNode() { value = localvalue + t.val }; 
		} else SynErr(67);
	}

	void Ident(out Node node) {
		Expect(1);
		int ltype = (0 == String.Compare(t.val, "return")) ? (int)ProtoCore.PrimitiveType.kTypeReturn : (int)ProtoCore.PrimitiveType.kTypeVar;
		IdentifierNode var = new IdentifierNode() 
		{
		// TODO Jun: Move the primitive types into a class table 
		Value = t.val, 
		Name = t.val, 
		type = ltype,
		datatype = (ProtoCore.PrimitiveType)ltype 
		}; 
		node = var;
		
	}

	void ArgDecl(out Node node) {
		IdentifierNode tNode = null; 
		VarDeclNode varDeclNode = new ProtoImperative.AST.VarDeclNode(); 
		varDeclNode.memregion = ProtoCore.DSASM.MemoryRegion.kMemStack;
		
		if (la.kind == 25) {
			Get();
			varDeclNode.memregion = ProtoCore.DSASM.MemoryRegion.kMemHeap; 
		}
		if (isArrayAccess()) {
			arrayident(out node);
			tNode = node as IdentifierNode; 
			varDeclNode.NameNode = tNode;
			
		} else if (la.kind == 1) {
			Get();
			tNode = new IdentifierNode() 
			{ 
			   Value = t.val, 
			   Name = t.val, 
			   type = (int)ProtoCore.PrimitiveType.kTypeVar, 
			   datatype = ProtoCore.PrimitiveType.kTypeVar 
			}; 
			varDeclNode.NameNode = tNode;
			
		} else SynErr(68);
		ProtoCore.Type argtype = new ProtoCore.Type(); argtype.Name = "var"; argtype.rank = 0; argtype.UID = 0; 
		if (la.kind == 36) {
			Get();
			Expect(1);
			argtype.Name = t.val; 
			if (la.kind == 6) {
				argtype.IsIndexable = true; 
				Get();
				Expect(7);
				argtype.rank = 1; 
				if (la.kind == 6 || la.kind == 17 || la.kind == 31) {
					if (la.kind == 17) {
						Get();
						Expect(6);
						Expect(7);
						argtype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 6) {
							Get();
							Expect(7);
							argtype.rank++; 
						}
					}
				}
			}
		}
		varDeclNode.ArgumentType = argtype; 
		if (la.kind == 31) {
			Get();
			Node rhsNode; 
			expr(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = tNode;
			bNode.RightNode = rhsNode;
			bNode.Optr = Operator.assign;
			varDeclNode.NameNode = bNode;		
			
		}
		node = varDeclNode; 
		if(!isGlobalScope) {
		   localVarCount++;
		}
		
	}

	void ReturnType(out ProtoCore.Type type) {
		ProtoCore.Type rtype = new ProtoCore.Type(); 
		Expect(1);
		rtype.Name = t.val; rtype.rank = 0; 
		if (la.kind == 6) {
			rtype.IsIndexable = true; 
			Get();
			Expect(7);
			rtype.rank = 1; 
			if (la.kind == 6 || la.kind == 17) {
				if (la.kind == 17) {
					Get();
					Expect(6);
					Expect(7);
					rtype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
				} else {
					while (la.kind == 6) {
						Get();
						Expect(7);
						rtype.rank++; 
					}
				}
			}
		}
		type = rtype; 
	}



	public override void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		DSImperative();
		Expect(0);

	}
	
    public override System.IO.TextWriter errorStream {
        get
        {
            return errors.errorStream;
        }
        set 
        {
            errors.errorStream = value;
        }
    }

    public override int errorCount {
        get
        {
            return errors.count;
        }
    }

	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, T,x,T,T, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, T,x,x,T, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,x,T, T,x,x,x},
		{x,T,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
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
			case 6: s = "openbracket expected"; break;
			case 7: s = "closebracket expected"; break;
			case 8: s = "openparen expected"; break;
			case 9: s = "closeparen expected"; break;
			case 10: s = "rel_gt expected"; break;
			case 11: s = "rel_lt expected"; break;
			case 12: s = "rel_ge expected"; break;
			case 13: s = "rel_le expected"; break;
			case 14: s = "rel_eq expected"; break;
			case 15: s = "rel_nq expected"; break;
			case 16: s = "endline expected"; break;
			case 17: s = "rangeop expected"; break;
			case 18: s = "kw_funcdef expected"; break;
			case 19: s = "kw_if expected"; break;
			case 20: s = "kw_elseif expected"; break;
			case 21: s = "kw_else expected"; break;
			case 22: s = "kw_while expected"; break;
			case 23: s = "kw_for expected"; break;
			case 24: s = "kw_constructor expected"; break;
			case 25: s = "kw_heap_alloc expected"; break;
			case 26: s = "langblocktrail expected"; break;
			case 27: s = "literal_true expected"; break;
			case 28: s = "literal_false expected"; break;
			case 29: s = "literal_null expected"; break;
			case 30: s = "\",\" expected"; break;
			case 31: s = "\"=\" expected"; break;
			case 32: s = "\"{\" expected"; break;
			case 33: s = "\"}\" expected"; break;
			case 34: s = "\"in\" expected"; break;
			case 35: s = "\"?\" expected"; break;
			case 36: s = "\":\" expected"; break;
			case 37: s = "\".\" expected"; break;
			case 38: s = "\"-\" expected"; break;
			case 39: s = "\"*\" expected"; break;
			case 40: s = "\"/\" expected"; break;
			case 41: s = "\"&\" expected"; break;
			case 42: s = "\"|\" expected"; break;
			case 43: s = "\"^\" expected"; break;
			case 44: s = "\"&&\" expected"; break;
			case 45: s = "\"||\" expected"; break;
			case 46: s = "\"+\" expected"; break;
			case 47: s = "\"!\" expected"; break;
			case 48: s = "\"~\" expected"; break;
			case 49: s = "\"#\" expected"; break;
			case 50: s = "??? expected"; break;
			case 51: s = "invalid stmt"; break;
			case 52: s = "invalid assignstmt"; break;
			case 53: s = "invalid assignstmt"; break;
			case 54: s = "invalid ifstmt"; break;
			case 55: s = "invalid ifstmt"; break;
			case 56: s = "invalid ifstmt"; break;
			case 57: s = "invalid forloop"; break;
			case 58: s = "invalid expr"; break;
			case 59: s = "invalid NameReference"; break;
			case 60: s = "invalid unaryop"; break;
			case 61: s = "invalid logicalop"; break;
			case 62: s = "invalid relop"; break;
			case 63: s = "invalid addop"; break;
			case 64: s = "invalid mulop"; break;
			case 65: s = "invalid factor"; break;
			case 66: s = "invalid bitop"; break;
			case 67: s = "invalid num"; break;
			case 68: s = "invalid ArgDecl"; break;

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