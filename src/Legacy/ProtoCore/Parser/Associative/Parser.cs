

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ProtoAssociative.DependencyPass;
using NodeList = System.Collections.Generic.List<ProtoAssociative.DependencyPass.Node>;
//using TypePatternPair = System.Collections.Generic.KeyValuePair<ProtoCore.Type, ProtoAssociative.DependencyPass.Pattern>;
using Operator = ProtoCore.DSASM.Operator;
using UnaryOperator = ProtoCore.DSASM.UnaryOperator;
using RangeStepOperator = ProtoCore.DSASM.RangeStepOperator;

namespace Associative {



public class Parser: ProtoCore.ParserBase {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _period = 5;
	public const int _pipeop = 6;
	public const int _hatop = 7;
	public const int _ampop = 8;
	public const int _addop = 9;
	public const int _mulop = 10;
	public const int _openbracket = 11;
	public const int _closebracket = 12;
	public const int _not = 13;
	public const int _neg = 14;
	public const int _pipe = 15;
	public const int _lessthan = 16;
	public const int _greaterthan = 17;
	public const int _openparen = 18;
	public const int _closeparen = 19;
	public const int _endline = 20;
	public const int _rangeop = 21;
	public const int _kw_native = 22;
	public const int _kw_class = 23;
	public const int _kw_constructor = 24;
	public const int _kw_def = 25;
	public const int _kw_external = 26;
	public const int _kw_extend = 27;
	public const int _literal_true = 28;
	public const int _literal_false = 29;
	public const int _literal_null = 30;
	public const int _langblocktrail = 31;
	public const int maxT = 59;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

    private ProtoCore.DSASM.OpKeywordData opKwData;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

    private ProtoCore.Core core = null;

public override ProtoCore.NodeBase codeblock { get; set; }
	
	private bool IsFunctionCall()
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

	private bool IsArrayAccess()
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

	private bool IsReplicationGuideIdent()
	{
		Token pt = la;
		if( _ident == pt.kind ) 
		{
			pt = scanner.Peek();
			if( _lessthan == pt.kind ) 
			{
				pt = scanner.Peek();
				if( _number == pt.kind ) 
				{
					pt = scanner.Peek();					
					if( _greaterthan == pt.kind ) 
					{
						scanner.ResetPeek();
						return true;
					}
				}
			}
		}
		scanner.ResetPeek();
		return false;
	}

    
	private string GetArithmeticFunction(Operator op)
	{
	    return "";
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
            parser = new Imperative.Parser(new Imperative.Scanner(memstream), core);
        else if (langblock.codeblock.language == ProtoCore.Language.kAssociative) 
            parser = new Associative.Parser(new Associative.Scanner(memstream), core);
        else
            return;

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

/*--------------------------------------------------------------------------*/

	
	public Parser(Scanner scanner, ProtoCore.Core coreObj) {
		this.scanner = scanner;
		errors = new Errors();
        opKwData = new ProtoCore.DSASM.OpKeywordData();
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

	
	void HydrogenParser() {
		codeblock = new CodeBlockNode(); 
		Node node = null; 
		while (StartOf(1)) {
			if (la.kind == 1 || la.kind == 20 || la.kind == 48) {
				Statement(out node);
			} else if (la.kind == 25 || la.kind == 26) {
				functiondecl(out node);
			} else if (la.kind == 23) {
				classdecl(out node);
			} else {
				LanguageBlock(out node);
			}
			if (null != node)
			(codeblock as CodeBlockNode).Body.Add(node); 
			
		}
	}

	void Statement(out Node node) {
		while (!(StartOf(2))) {SynErr(60); Get();}
		node = null; 
		if (la.kind == 1) {
			FunctionalStatement(out node);
		} else if (la.kind == 48) {
			ForLoop(out node);
		} else if (la.kind == 20) {
			Get();
		} else SynErr(61);
	}

	void functiondecl(out Node node, ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic) {
		FunctionDefinitionNode f = new FunctionDefinitionNode(); 
		string methodName;  
		Node argumentSignature; 
		ProtoCore.Type returnType;  
		Node pattern; 
		string externLibName = ""; 
		bool isExternLib = false; 
		bool isDNI = false; 								
		
		if (la.kind == 26) {
			Get();
			isExternLib = true; 
			if (la.kind == 22) {
				Get();
				isDNI = true; 
			}
			ExternalLibraryReference(out externLibName);
		}
		Expect(25);
		MethodSignature(out methodName, out argumentSignature, out pattern, out returnType);
		f = new FunctionDefinitionNode(); 
		f.IsExternLib = isExternLib; 
		f.IsDNI = isDNI; 
		f.ExternLibName = externLibName; 
		f.Name = methodName; 
		f.Name = methodName; 
		f.Pattern = pattern; 
		f.ReturnType = returnType; 
		f.access = access;
		f.Singnature = argumentSignature as ArgumentSignatureNode; 
		Node functionBody = null; 
		
		if (la.kind == 20) {
			Get();
		} else if (la.kind == 32) {
			FunctionalMethodBodyMultiLine(out functionBody);
		} else SynErr(62);
		f.FunctionBody = functionBody as CodeBlockNode; 
		node = f;	
		
	}

	void classdecl(out Node node) {
		ClassDeclNode classnode = new ClassDeclNode(); 
		Expect(23);
		Expect(1);
		classnode.name = t.val; 
		if (la.kind == 27) {
			Get();
			Expect(1);
			classnode.superClass = new List<string>();
			classnode.superClass.Add(t.val); 
			
			while (la.kind == 1) {
				Get();
				classnode.superClass.Add(t.val); 
			}
		}
		Expect(32);
		while (StartOf(3)) {
			ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic; 
			if (la.kind == 38 || la.kind == 39 || la.kind == 40) {
				AccessSpecifier(out access);
			}
			if (la.kind == 24) {
				Node constr = null; 
				constructordecl(out constr, access);
				classnode.funclist.Add(constr); 
			} else if (la.kind == 25 || la.kind == 26) {
				Node funcnode; 
				functiondecl(out funcnode, access);
				classnode.funclist.Add(funcnode); 
			} else if (la.kind == 1) {
				Node varnode = null; 
				vardecl(out varnode, access);
				classnode.varlist.Add(varnode); 
				Expect(20);
			} else SynErr(63);
		}
		Expect(33);
		node = classnode; 
	}

	void LanguageBlock(out Node node) {
		node = null; 
		LanguageBlockNode langblock = new LanguageBlockNode(); 
		
		Expect(11);
		Expect(1);
		if( 0 == t.val.CompareTo("Imperative")) {
		langblock.codeblock.language = ProtoCore.Language.kImperative;
		}
		else if( 0 == t.val.CompareTo("Associative")) {
		langblock.codeblock.language = ProtoCore.Language.kAssociative; 
		}
		
		while (la.kind == 34) {
			Get();
			string key; 
			Expect(1);
			key = t.val; 
			Expect(35);
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
		Expect(12);
		Expect(31);
		langblock.codeblock.body = t.val.Substring(2,t.val.Length-4);									
		node = langblock;
		                                ParseLanguageBlockNode(langblock);
		
	}

	void FunctionalStatement(out Node node) {
		while (!(la.kind == 0 || la.kind == 1)) {SynErr(64); Get();}
		node = null; 
		Node leftNode; 
		IdentifierList(out leftNode);
		Expect(35);
		Node rightNode = null; 
		if (la.kind == 11) {
			LanguageBlock(out rightNode);
			BinaryExpressionNode expressionNode = new BinaryExpressionNode(); 
			expressionNode.LeftNode = leftNode; 
			expressionNode.RightNode = rightNode; 
			expressionNode.Optr = Operator.assign; 
			node = expressionNode; 
		} else if (StartOf(4)) {
			Expression(out rightNode);
			BinaryExpressionNode expressionNode = new BinaryExpressionNode(); 
			expressionNode.LeftNode = leftNode; 
			expressionNode.RightNode = rightNode; 
			expressionNode.Optr = Operator.assign; 
			node = expressionNode; 
			while (!(la.kind == 0 || la.kind == 20)) {SynErr(65); Get();}
			Expect(20);
		} else if (la.kind == 41) {
			Get();
			ModifierStackNode mstack = new ModifierStackNode(); 
			string name = ""; 
			Expression(out rightNode);
			if (la.kind == 42) {
				Get();
				Expect(1);
				name = t.val; 
			}
			BinaryExpressionNode expressionNode = new BinaryExpressionNode();
			expressionNode.RightNode = rightNode;
			expressionNode.LeftNode = leftNode; 
			expressionNode.Optr = Operator.assign;
			mstack.AddElementNode(expressionNode, name); 
			
			while (!(la.kind == 0 || la.kind == 20)) {SynErr(66); Get();}
			Expect(20);
			while (StartOf(5)) {
				name = ""; 
				bool bHasOperator = false; 
				Operator op = Operator.add;  
				if (StartOf(6)) {
					bHasOperator = true; 
					BinaryOps(out op);
				}
				Expression(out rightNode);
				if (la.kind == 42) {
					Get();
					Expect(1);
					name = t.val; 
				}
				if(!bHasOperator)
				{
				  expressionNode = new BinaryExpressionNode();
				  expressionNode.RightNode = rightNode;
				  expressionNode.LeftNode = leftNode; 
				  expressionNode.Optr = Operator.assign;
				  mstack.AddElementNode(expressionNode, name);
				}
				else
				{ 
				  BinaryExpressionNode expressionNode2 = new BinaryExpressionNode(); 
				  expressionNode2.LeftNode = leftNode;
				  expressionNode2.RightNode = rightNode;
				  expressionNode2.Optr = op;
				  expressionNode = new BinaryExpressionNode();
				  expressionNode.RightNode = expressionNode2;
				  expressionNode.LeftNode = leftNode; 
				  expressionNode.Optr = Operator.assign;
				  mstack.AddElementNode(expressionNode, name);
				}
				
				while (!(la.kind == 0 || la.kind == 20)) {SynErr(67); Get();}
				Expect(20);
			}
			node = mstack; 
			Expect(33);
		} else SynErr(68);
	}

	void ForLoop(out Node forLoop) {
		Node node;
		ForLoopNode loop = new ForLoopNode(); 
		NodeList forBody = null; 
		
		Expect(48);
		Expect(18);
		IdentifierList(out node);
		loop.id = node; 
		Expect(49);
		Expression(out node);
		loop.expression = node; 
		Expect(19);
		Expect(32);
		StatementList(out forBody);
		loop.body = forBody; 
		Expect(33);
		forLoop = loop; 
	}

	void StatementList(out NodeList nodelist) {
		nodelist = new NodeList(); 
		while (la.kind == 1 || la.kind == 20 || la.kind == 48) {
			Node node = null; 
			Statement(out node);
			nodelist.Add(node); 
		}
	}

	void AccessSpecifier(out ProtoCore.DSASM.AccessSpecifier access) {
		access = ProtoCore.DSASM.AccessSpecifier.kPublic; 
		if (la.kind == 38) {
			Get();
		} else if (la.kind == 39) {
			Get();
			access = ProtoCore.DSASM.AccessSpecifier.kPrivate; 
		} else if (la.kind == 40) {
			Get();
			access = ProtoCore.DSASM.AccessSpecifier.kProtected; 
		} else SynErr(69);
	}

	void constructordecl(out Node constrNode, ProtoCore.DSASM.AccessSpecifier access) {
		ConstructorDefinitionNode constr = null;									
		string methodName;  
		Node argumentSignature; 
		ProtoCore.Type returnType;  
		Node pattern;								
		
		Expect(24);
		MethodSignature(out methodName, out argumentSignature, out pattern, out returnType);
		constr = new ConstructorDefinitionNode(); 
		constr.Name = methodName; 
		constr.Pattern = pattern; 
		constr.ReturnType = returnType;
		constr.Signature = argumentSignature as ArgumentSignatureNode;
		constr.access = access; 
		Node functionBody = null; 
		
		if (la.kind == 36) {
			Get();
			Node bnode; 
			BaseConstructorCall(out bnode);
			constr.baseConstr = bnode as FunctionCallNode; 
		}
		FunctionalMethodBodyMultiLine(out functionBody);
		constr.FunctionBody = functionBody as CodeBlockNode; 
		constrNode = constr; 
	}

	void vardecl(out Node node, ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic) {
		IdentifierNode tNode = null; 
		VarDeclNode varDeclNode = new VarDeclNode(); 
		varDeclNode.memregion = ProtoCore.DSASM.MemoryRegion.kMemStack;
		varDeclNode.access = access;
		
		if (IsArrayAccess()) {
			arrayIdent(out node);
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
			
		} else SynErr(70);
		Expect(36);
		Expect(1);
		ProtoCore.Type argtype = new ProtoCore.Type(); argtype.Name = t.val; argtype.rank = 0; 
		if (la.kind == 11) {
			argtype.IsIndexable = true; 
			Get();
			Expect(12);
			argtype.rank = 1; 
			if (la.kind == 11 || la.kind == 21 || la.kind == 35) {
				if (la.kind == 21) {
					Get();
					Expect(11);
					Expect(12);
					argtype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
				} else {
					while (la.kind == 11) {
						Get();
						Expect(12);
						argtype.rank++; 
					}
				}
			}
		}
		varDeclNode.ArgumentType = argtype; 
		if (la.kind == 35) {
			Get();
			Node rhsNode; 
			Expression(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = tNode;
			bNode.RightNode = rhsNode;
			bNode.Optr = Operator.assign;
			varDeclNode.NameNode = bNode;		
			
		}
		node = varDeclNode; 
		//if(!isGlobalScope) {
		//    localVarCount++;
		//}
		
	}

	void MethodSignature(out string methodName, out Node argumentSign, out Node pattern, out ProtoCore.Type returnType) {
		Expect(1);
		methodName = t.val; 
		Node argumentSignature = null;
		                          returnType = new ProtoCore.Type(); 
		                          returnType.Name = "var";
		                          returnType.UID = 0;
		
		                          // TODO Jun: Luke made changes to array representation, handle this
		                          //returnType.IsArray = false;
		                      
		if (la.kind == 36) {
			TypeRestriction(out returnType);
		}
		ArgumentSignatureDefinition(out argumentSignature);
		pattern = null; 
		if (la.kind == 15) {
			PatternExpression(out pattern);
		}
		argumentSign = argumentSignature; 
	}

	void BaseConstructorCall(out Node bnode) {
		FunctionCallNode f = new FunctionCallNode(); 
		NodeList args = null; 
		Expect(37);
		if (la.kind == 1) {
			Ident(out bnode);
			f.Function = bnode; 
		}
		Arguments(out args);
		f.FormalArguments = args; 
		bnode = f; 
	}

	void FunctionalMethodBodyMultiLine(out Node funcBody) {
		CodeBlockNode functionBody = new CodeBlockNode(); 
		NodeList body = new NodeList(); 
		
		Expect(32);
		StatementList(out body);
		functionBody = new CodeBlockNode(); 
		functionBody.Body =body;  
		Expect(33);
		funcBody = functionBody; 
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

	void Arguments(out NodeList nodes) {
		Expect(18);
		nodes = new List<Node>(); 
		if (StartOf(4)) {
			Node t; 
			Expression(out t);
			nodes.Add(t); 
			while (WeakSeparator(34,4,7) ) {
				Expression(out t);
				nodes.Add(t); 
			}
		}
		Expect(19);
	}

	void ExternalLibraryReference(out string libname) {
		Expect(18);
		Expect(4);
		libname = t.val; 
		libname = libname.Remove(0, 1); 
		libname = libname.Remove(libname.Length-1, 1); 
		Expect(19);
	}

	void TypeRestriction(out ProtoCore.Type type) {
		Expect(36);
		ClassReference(out type);
		type.rank = 0; 
		if (la.kind == 11) {
			type.IsIndexable = true; 
			Get();
			Expect(12);
			type.rank = 1; 
			if (la.kind == 11 || la.kind == 21) {
				if (la.kind == 21) {
					Get();
					Expect(11);
					Expect(12);
					type.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
				} else {
					while (la.kind == 11) {
						Get();
						Expect(12);
						type.rank++; 
					}
				}
			}
		}
	}

	void ArgumentSignatureDefinition(out Node argumentSign) {
		ArgumentSignatureNode argumentSignature = new ArgumentSignatureNode(); 
		Expect(18);
		if (la.kind == 1) {
			Node arg;
			ArgDecl(out arg);
			argumentSignature.AddArgument(arg as VarDeclNode); 
			while (la.kind == 34) {
				Get();
				ArgDecl(out arg);
				argumentSignature.AddArgument(arg as VarDeclNode); 
			}
		}
		Expect(19);
		argumentSign = argumentSignature; 
	}

	void PatternExpression(out Node pattern) {
		Node p = null; 
		Expect(15);
		Expression(out p);
		pattern = p; 
	}

	void ArgDecl(out Node node, ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic) {
		IdentifierNode tNode = null; 
		VarDeclNode varDeclNode = new VarDeclNode(); 
		varDeclNode.memregion = ProtoCore.DSASM.MemoryRegion.kMemStack;
		varDeclNode.access = access;
		
		if (IsArrayAccess()) {
			arrayIdent(out node);
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
			
		} else SynErr(71);
		ProtoCore.Type argtype = new ProtoCore.Type(); argtype.Name = "var"; argtype.rank = 0; argtype.UID = 0; 
		if (la.kind == 36) {
			Get();
			Expect(1);
			argtype.Name = t.val; 
			if (la.kind == 11) {
				argtype.IsIndexable = true; 
				Get();
				Expect(12);
				argtype.rank = 1; 
				if (la.kind == 11 || la.kind == 21 || la.kind == 35) {
					if (la.kind == 21) {
						Get();
						Expect(11);
						Expect(12);
						argtype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 11) {
							Get();
							Expect(12);
							argtype.rank++; 
						}
					}
				}
			}
		}
		varDeclNode.ArgumentType = argtype; 
		if (la.kind == 35) {
			Get();
			Node rhsNode; 
			Expression(out rhsNode);
			BinaryExpressionNode bNode = new BinaryExpressionNode();
			bNode.LeftNode = tNode;
			bNode.RightNode = rhsNode;
			bNode.Optr = Operator.assign;
			varDeclNode.NameNode = bNode;		
			
		}
		node = varDeclNode; 
	}

	void arrayIdent(out Node node) {
		Ident(out node);
		IdentifierNode var = node as IdentifierNode; 
		if (la.kind == 11) {
			ArrayNode array = null; 
			arrayIndices(out array);
			var.ArrayDimensions = array; 
		}
	}

	void Expression(out Node node) {
		node = null; 
		if (la.kind == 13 || la.kind == 57) {
			UnaryExpression(out node);
		} else if (StartOf(8)) {
			LogicalExpression(out node);
		} else SynErr(72);
		while (la.kind == 43) {
			TernaryOp(ref node);
		}
	}

	void BinaryOps(out Operator op) {
		op = Operator.none; 
		if (la.kind == 14 || la.kind == 50) {
			AddOp(out op);
		} else if (la.kind == 51 || la.kind == 52) {
			MulOp(out op);
		} else if (StartOf(9)) {
			ComparisonOp(out op);
		} else if (la.kind == 55 || la.kind == 56) {
			LogicalOp(out op);
		} else SynErr(73);
	}

	void AddOp(out Operator op) {
		op = Operator.add; 
		if (la.kind == 50) {
			Get();
		} else if (la.kind == 14) {
			Get();
			op = Operator.sub; 
		} else SynErr(74);
	}

	void MulOp(out Operator op) {
		op = Operator.mul; 
		if (la.kind == 51) {
			Get();
		} else if (la.kind == 52) {
			Get();
			op = Operator.div; 
		} else SynErr(75);
	}

	void ComparisonOp(out Operator op) {
		op = Operator.none; 
		switch (la.kind) {
		case 17: {
			Get();
			op = Operator.gt; 
			break;
		}
		case 44: {
			Get();
			op = Operator.ge; 
			break;
		}
		case 16: {
			Get();
			op = Operator.lt; 
			break;
		}
		case 45: {
			Get();
			op = Operator.le; 
			break;
		}
		case 46: {
			Get();
			op = Operator.eq; 
			break;
		}
		case 47: {
			Get();
			op = Operator.nq; 
			break;
		}
		default: SynErr(76); break;
		}
	}

	void LogicalOp(out Operator op) {
		op = Operator.and; 
		if (la.kind == 55) {
			Get();
		} else if (la.kind == 56) {
			Get();
			op = Operator.or; 
		} else SynErr(77);
	}

	void ClassReference(out ProtoCore.Type type) {
		type = new ProtoCore.Type(); 
		string name; 
		Expect(1);
		name = t.val; 
		type.Name = name; 
		type.UID = 0; 
	}

	void IdentifierList(out Node node) {
		node = null; 
		NameReference(out node);
		while (la.kind == 5) {
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

	void UnaryExpression(out Node node) {
		node = null; 
		UnaryOperator op; 
		Node exprNode; 
		unaryop(out op);
		Expression(out exprNode);
		UnaryExpressionNode unary = new UnaryExpressionNode(); 
		unary.Operator = op;
		unary.Expression = exprNode;
		node = unary;
		
	}

	void LogicalExpression(out Node node) {
		ComparisonExpression(out node);
		while (la.kind == 55 || la.kind == 56) {
			Operator op;
			LogicalOp(out op);
			Node expr2; 
			Expression(out expr2);
			BinaryExpressionNode binaryNode = new BinaryExpressionNode();
			binaryNode.LeftNode = node;
			binaryNode.RightNode = expr2;
			binaryNode.Optr = op;
			node = binaryNode;
			
		}
	}

	void TernaryOp(ref Node node) {
		InlineConditionalNode inlineConNode = new InlineConditionalNode(); 
		Expect(43);
		inlineConNode.ConditionExpression = node; node = null; 
		Expression(out node);
		inlineConNode.TrueExpression = node; 
		Expect(36);
		node = null; 
		Expression(out node);
		inlineConNode.FalseExpression = node; 
		node = inlineConNode; 
	}

	void unaryop(out UnaryOperator op) {
		op = UnaryOperator.None; 
		if (la.kind == 13) {
			Get();
			op = UnaryOperator.Not; 
		} else if (la.kind == 57) {
			Get();
			op = UnaryOperator.Negate; 
		} else SynErr(78);
	}

	void ComparisonExpression(out Node node) {
		RangeExpr(out node);
		while (StartOf(9)) {
			Operator op; 
			ComparisonOp(out op);
			Node expr2; 
			RangeExpr(out expr2);
			BinaryExpressionNode binaryNode = new BinaryExpressionNode();
			binaryNode.LeftNode = node;
			binaryNode.RightNode = expr2;
			binaryNode.Optr = op;
			node = binaryNode;
			
		}
	}

	void RangeExpr(out Node node) {
		ArithmeticExpression(out node);
		if (la.kind == 21) {
			RangeExprNode rnode = new RangeExprNode(); 
			rnode.FromNode = node;
			
			Get();
			ArithmeticExpression(out node);
			rnode.ToNode = node; 
			if (la.kind == 21) {
				RangeStepOperator op; 
				Get();
				rangeStepOperator(out op);
				rnode.stepoperator = op; 
				ArithmeticExpression(out node);
				rnode.StepNode = node; 
			}
			node = rnode; 
		}
	}

	void ArithmeticExpression(out Node node) {
		Term(out node);
		while (la.kind == 14 || la.kind == 50) {
			Operator op; 
			AddOp(out op);
			Node expr2; 
			Term(out expr2);
			BinaryExpressionNode binaryNode = new BinaryExpressionNode();
			binaryNode.LeftNode = node;
			binaryNode.RightNode = expr2;
			binaryNode.Optr = op;
			node = binaryNode;
			
		}
	}

	void rangeStepOperator(out RangeStepOperator op) {
		op = RangeStepOperator.stepsize; 
		if (la.kind == 57 || la.kind == 58) {
			if (la.kind == 58) {
				Get();
				op = RangeStepOperator.num; 
			} else {
				Get();
				op = RangeStepOperator.approxsize; 
			}
		}
	}

	void BitOp(out Operator op) {
		op = Operator.bitwiseand; 
		if (la.kind == 53) {
			Get();
		} else if (la.kind == 54) {
			Get();
			op = Operator.bitwisexor; 
		} else if (la.kind == 15) {
			Get();
			op = Operator.bitwiseor; 
		} else SynErr(79);
	}

	void Term(out Node node) {
		interimfactor(out node);
		while (la.kind == 51 || la.kind == 52) {
			Operator op; 
			MulOp(out op);
			Node expr2; 
			interimfactor(out expr2);
			BinaryExpressionNode binaryNode = new BinaryExpressionNode();
			binaryNode.LeftNode = node;
			binaryNode.RightNode = expr2;
			binaryNode.Optr = op;
			node = binaryNode;
			
		}
	}

	void interimfactor(out Node node) {
		Factor(out node);
		while (la.kind == 15 || la.kind == 53 || la.kind == 54) {
			Operator op; 
			BitOp(out op);
			Node expr2; 
			Factor(out expr2);
			BinaryExpressionNode binaryNode = new BinaryExpressionNode();
			binaryNode.LeftNode = node;
			binaryNode.RightNode = expr2;
			binaryNode.Optr = op;
			node = binaryNode;
			
		}
	}

	void Factor(out Node node) {
		node = null; 
		if (IsReplicationGuideIdent()) {
			ReplicationGuideIdent(out node);
		} else if (la.kind == 2 || la.kind == 3 || la.kind == 14) {
			Number(out node);
		} else if (la.kind == 28) {
			Get();
			node = new BooleanNode() { value = ProtoCore.DSASM.Literal.True }; 
		} else if (la.kind == 29) {
			Get();
			node = new BooleanNode() { value = ProtoCore.DSASM.Literal.False }; 
		} else if (la.kind == 30) {
			Get();
			node = new NullNode(); 
		} else if (la.kind == 32) {
			ArrayExprList(out node);
		} else if (la.kind == 18) {
			Get();
			Expression(out node);
			Expect(19);
		} else if (la.kind == 1) {
			IdentifierList(out node);
		} else SynErr(80);
	}

	void Number(out Node node) {
		node = null; String localvalue = String.Empty; 
		if (la.kind == 14) {
			Get();
			localvalue = "-"; 
		}
		if (la.kind == 2) {
			Get();
			node = new IntNode() { value = localvalue + t.val }; 
		} else if (la.kind == 3) {
			Get();
			node = new DoubleNode() { value = localvalue + t.val }; 
		} else SynErr(81);
	}

	void ReplicationGuideIdent(out Node node) {
		Ident(out node);
		IdentifierNode identNode = node as IdentifierNode; 
		NodeList guides = new NodeList();
		Expect(16);
		Expect(2);
		Node numNode = new IdentifierNode() { Value = t.val }; 
		Expect(17);
		guides.Add(numNode); 
		while (la.kind == 16) {
			Get();
			Expect(2);
			numNode = new IdentifierNode() { Value = t.val }; 
			Expect(17);
			guides.Add(numNode); 
		}
		identNode.ReplicationGuides = guides; 
		node = identNode; 
	}

	void ArrayExprList(out Node node) {
		Expect(32);
		ExprListNode exprlist = new ExprListNode(); 
		if (StartOf(4)) {
			Expression(out node);
			exprlist.list.Add(node); 
			while (la.kind == 34) {
				Get();
				Expression(out node);
				exprlist.list.Add(node); 
			}
		}
		Expect(33);
		node = exprlist; 
	}

	void arrayIndices(out ArrayNode array) {
		Expect(11);
		Node exp = null; 
		if (StartOf(4)) {
			Expression(out exp);
		}
		array = new ArrayNode();
		array.Expr = exp; 
		array.Type = null;
		
		Expect(12);
		while (la.kind == 11) {
			Get();
			exp = null; 
			if (StartOf(4)) {
				Expression(out exp);
			}
			ArrayNode array2 = new ArrayNode();
			array2.Expr = exp; 
			array2.Type = null;
			array.Type = array2;
			array = array2;
			
			Expect(12);
		}
	}

	void NameReference(out Node node) {
		node = null; 
		if (IsFunctionCall()) {
			FunctionCall(out node);
		} else if (la.kind == 1) {
			arrayIdent(out node);
		} else SynErr(82);
	}

	void FunctionCall(out Node node) {
		Ident(out node);
		NodeList args = null; 
		Arguments(out args);
		FunctionCallNode f = new FunctionCallNode(); 
		f.FormalArguments = args;
		f.Function = node;
		node = f; 
		
	}



	public override void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		HydrogenParser();
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
		{T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, T,x,x,T, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, T,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,T,T,x, T,T,T,x, x,x,x,x, x,x,x,x, T,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,T,T, T,x,x,T, T,T,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,T,T, T,x,x,T, T,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, T,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x}

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
			case 5: s = "period expected"; break;
			case 6: s = "pipeop expected"; break;
			case 7: s = "hatop expected"; break;
			case 8: s = "ampop expected"; break;
			case 9: s = "addop expected"; break;
			case 10: s = "mulop expected"; break;
			case 11: s = "openbracket expected"; break;
			case 12: s = "closebracket expected"; break;
			case 13: s = "not expected"; break;
			case 14: s = "neg expected"; break;
			case 15: s = "pipe expected"; break;
			case 16: s = "lessthan expected"; break;
			case 17: s = "greaterthan expected"; break;
			case 18: s = "openparen expected"; break;
			case 19: s = "closeparen expected"; break;
			case 20: s = "endline expected"; break;
			case 21: s = "rangeop expected"; break;
			case 22: s = "kw_native expected"; break;
			case 23: s = "kw_class expected"; break;
			case 24: s = "kw_constructor expected"; break;
			case 25: s = "kw_def expected"; break;
			case 26: s = "kw_external expected"; break;
			case 27: s = "kw_extend expected"; break;
			case 28: s = "literal_true expected"; break;
			case 29: s = "literal_false expected"; break;
			case 30: s = "literal_null expected"; break;
			case 31: s = "langblocktrail expected"; break;
			case 32: s = "\"{\" expected"; break;
			case 33: s = "\"}\" expected"; break;
			case 34: s = "\",\" expected"; break;
			case 35: s = "\"=\" expected"; break;
			case 36: s = "\":\" expected"; break;
			case 37: s = "\"base\" expected"; break;
			case 38: s = "\"public\" expected"; break;
			case 39: s = "\"private\" expected"; break;
			case 40: s = "\"protected\" expected"; break;
			case 41: s = "\"#{\" expected"; break;
			case 42: s = "\"=>\" expected"; break;
			case 43: s = "\"?\" expected"; break;
			case 44: s = "\">=\" expected"; break;
			case 45: s = "\"<=\" expected"; break;
			case 46: s = "\"==\" expected"; break;
			case 47: s = "\"!=\" expected"; break;
			case 48: s = "\"for\" expected"; break;
			case 49: s = "\"in\" expected"; break;
			case 50: s = "\"+\" expected"; break;
			case 51: s = "\"*\" expected"; break;
			case 52: s = "\"/\" expected"; break;
			case 53: s = "\"&\" expected"; break;
			case 54: s = "\"^\" expected"; break;
			case 55: s = "\"&&\" expected"; break;
			case 56: s = "\"||\" expected"; break;
			case 57: s = "\"~\" expected"; break;
			case 58: s = "\"#\" expected"; break;
			case 59: s = "??? expected"; break;
			case 60: s = "this symbol not expected in Statement"; break;
			case 61: s = "invalid Statement"; break;
			case 62: s = "invalid functiondecl"; break;
			case 63: s = "invalid classdecl"; break;
			case 64: s = "this symbol not expected in FunctionalStatement"; break;
			case 65: s = "this symbol not expected in FunctionalStatement"; break;
			case 66: s = "this symbol not expected in FunctionalStatement"; break;
			case 67: s = "this symbol not expected in FunctionalStatement"; break;
			case 68: s = "invalid FunctionalStatement"; break;
			case 69: s = "invalid AccessSpecifier"; break;
			case 70: s = "invalid vardecl"; break;
			case 71: s = "invalid ArgDecl"; break;
			case 72: s = "invalid Expression"; break;
			case 73: s = "invalid BinaryOps"; break;
			case 74: s = "invalid AddOp"; break;
			case 75: s = "invalid MulOp"; break;
			case 76: s = "invalid ComparisonOp"; break;
			case 77: s = "invalid LogicalOp"; break;
			case 78: s = "invalid unaryop"; break;
			case 79: s = "invalid BitOp"; break;
			case 80: s = "invalid Factor"; break;
			case 81: s = "invalid Number"; break;
			case 82: s = "invalid NameReference"; break;

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