
//#define ENABLE_INC_DEC_FIX
using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoCore.Properties;

namespace ProtoCore.DesignScriptParser {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _float = 3;
	public const int _textstring = 4;
	public const int _char = 5;
	public const int _period = 6;
	public const int _postfixed_replicationguide = 7;
	public const int _list_at = 8;
	public const int _dominant_list_at = 9;
	public const int _openbracket = 10;
	public const int _closebracket = 11;
	public const int _openparen = 12;
	public const int _closeparen = 13;
	public const int _not = 14;
	public const int _neg = 15;
	public const int _pipe = 16;
	public const int _lessthan = 17;
	public const int _greaterthan = 18;
	public const int _lessequal = 19;
	public const int _greaterequal = 20;
	public const int _equal = 21;
	public const int _notequal = 22;
	public const int _endline = 23;
	public const int _rangeop = 24;
	public const int _kw_class = 25;
	public const int _kw_constructor = 26;
	public const int _kw_def = 27;
	public const int _kw_extend = 28;
	public const int _kw_if = 29;
	public const int _kw_elseif = 30;
	public const int _kw_else = 31;
	public const int _kw_while = 32;
	public const int _kw_for = 33;
	public const int _kw_import = 34;
	public const int _kw_prefix = 35;
	public const int _kw_from = 36;
	public const int _kw_break = 37;
	public const int _kw_continue = 38;
	public const int _kw_static = 39;
	public const int _kw_local = 40;
	public const int _literal_true = 41;
	public const int _literal_false = 42;
	public const int _literal_null = 43;
	public const int maxT = 64;
	public const int _inlinecomment = 65;
	public const int _blockcomment = 66;

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

public Node root { get; set; }
    public CodeBlockNode commentNode { get; set; }
    public ProtoFFI.ImportModuleHandler ImportModuleHandler { get; set; }
    
    //used for isModifier check
    private string leftVar { get; set; }
    private bool isModifier = false;
    private bool withinModifierCheckScope = false;
    private bool isLeftVarIdentList = false;
    //end

    private int localVarCount = 0;
    private bool isGlobalScope = true;

    private bool isInClass = false;
    private bool disableKwCheck = false;

    private bool isLeft = false; // check if it is left hand side of the assignment expression

    // This is used by GraphIDE mode parsing when determining how many statements the parser has processed
    private int stmtsParsed = 0;


    private string GetEscapedString(string s)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int i = 0;
        while (i < s.Length)
        {
            if (s[i] == '\\' && (i + 1) < s.Length)
            {
                i = i + 1;
                switch (s[i])
                {
                    case '\\':
                        sb.Append('\\');
                        break;
                    case '"':
                        sb.Append('"');
                        break;
                    case 'a':
                        sb.Append('\a');
                        break;
                    case 'b':
                        sb.Append('\b');
                        break;
                    case 'f':
                        sb.Append('\f');
                        break;
                    case 'n':
                        sb.Append('\n');
                        break;
                    case 'r':
                        sb.Append('\r');
                        break;
                    case 't':
                        sb.Append('\t');
                        break;
                    case 'v':
                        sb.Append('\v');
                        break;
                    case '0':
                        sb.Append('\0');
                        break;
                    default:
                        i = i - 1;
                        sb.Append(s[i]);
                        break;
                }
            }
			else if (s[i] == '\\' && i == s.Length - 1) {
                 SynErr(Resources.EOF_expected);
                }
            else
            {
                sb.Append(s[i]);
            }
            i = i + 1;
        }
        return sb.ToString();
    }

    private bool IsIdentList()
    {
        Token pt = la;
        if (_ident == pt.kind)
        {
            pt = scanner.Peek();
            if ("." == pt.val)
            {
                pt = scanner.Peek();
                scanner.ResetPeek();
                if (_ident == pt.kind)
                    return true;
            }
        }
        scanner.ResetPeek();
        return false;
    }

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
        scanner.ResetPeek();
        return false;
    }

	private bool IsFunctionCallStatement()
	{
        Token pt = la;
        while (pt.kind != _EOF)
        {
			if( _ident == pt.kind ) 
			{
				pt = scanner.Peek();
				if( _openparen == pt.kind )
				{
					scanner.ResetPeek();
					return true;
				}
				else if( _period == pt.kind )
				{
					pt = scanner.Peek();
					continue;
				}
			}
			else
			{
				break;
			}			
		}

        scanner.ResetPeek();
        return false;
	}

	private bool IsNonAssignmentStatement()
    {
		if (core.ParsingMode != ParseMode.AllowNonAssignment)
            return false;

		if (IsTypedVariable())
            return false;

        Token pt = la;
        while (pt.kind != _EOF)
        {
            if (pt.val == ";" && la.val == ";")
            {
                scanner.ResetPeek();
                return false;
            }
            else if (pt.val == "=")
            {
                scanner.ResetPeek();
                return false;
            }
			else if (pt.val == ";")
                break;

            pt = scanner.Peek();
        }

        scanner.ResetPeek();
        return true;
    }

    private bool IsAssignmentStatement()
    {
        Token pt = la;
        while (pt.kind != _EOF)
        {
            if (pt.val == ";")
            {
                scanner.ResetPeek();
                return false;
            }
            else if (pt.val == "=")
            {
                scanner.ResetPeek();
                return true;
            }

            pt = scanner.Peek();
        }

        scanner.ResetPeek();
        return false;
    }

    private bool IsVariableDeclaration()
    {
        Token t = la;
        if (_ident == t.kind) {
            t = scanner.Peek();
            if (":" == t.val) {
                t = scanner.Peek();
                if (_ident == t.kind) {
                    t = scanner.Peek();
                    scanner.ResetPeek();
                    return (_endline == t.kind || "[" == t.val);
                }
                scanner.ResetPeek();
                return false;
            }
            else if (_endline == t.kind) {
                scanner.ResetPeek();
                return true;
            }

            scanner.ResetPeek();
            return false;
        }
        return false;
    }
   
    private bool IsReplicationGuide()
    {
        bool isRepGuide = false;
        Token pt = la;
        if (_lessthan == pt.kind)
        {
            pt = scanner.Peek();
            if (_number == pt.kind || _postfixed_replicationguide == pt.kind)
            {
                pt = scanner.Peek();
                if (_greaterthan == pt.kind)
                {
                    isRepGuide = true;
                }
            }

        }
        scanner.ResetPeek();
        return isRepGuide;
    }

    private bool IsPostfixedReplicationGuide()
    {
        bool isPostFixedRepGuide = false;
        Token pt = la;
        if (_postfixed_replicationguide == pt.kind)
        {
            pt = scanner.Peek();
            if (_greaterthan == pt.kind)
            {
                isPostFixedRepGuide = true;
            }
        }

        scanner.ResetPeek();
        return isPostFixedRepGuide;
    }

    private bool IsNumber()
    {
        Token pt = la;

        if (pt.val == "-") {
            pt = scanner.Peek();
            scanner.ResetPeek();
        }

        return ((_number == pt.kind) || (_float == pt.kind));
    }

    private bool IsLocalVariable()
    {
        Token pt = la;
        bool isLocal = false;

        if (_ident == pt.kind) 
        {
            pt = scanner.Peek();
            if (":" == pt.val)
            { 
                pt = scanner.Peek();
                if (_kw_local == pt.kind) 
                {
                    isLocal = true;
                }
            }
        }
        scanner.ResetPeek();
        return isLocal;
    }

    private bool IsTypedVariable()
    {
        Token pt = la;
        bool isTyped = false;

        if (_ident == pt.kind) 
        {
            pt = scanner.Peek();
            if (":" == pt.val)
            { 
                pt = scanner.Peek();
                if (_ident == pt.kind) 
                {
                    isTyped = true;
                }
            }
        }
        scanner.ResetPeek();
        return isTyped;
    }

    private bool IsLocallyTypedVariable()
    {
        Token pt = la;
        bool isLocallyTyped = false;

        if (_ident == pt.kind) 
        {
            pt = scanner.Peek();
            if (":" == pt.val)
            { 
                pt = scanner.Peek();
                if (_kw_local == pt.kind) 
                {
                    pt = scanner.Peek();
                    if (_ident == pt.kind) 
                    {
                        isLocallyTyped = true;
                    }
                }
            }
        }
        scanner.ResetPeek();
        return isLocallyTyped;
    }
	
	private bool IsFullClosure()
    {
        Token pt = la;
        int closureCount = 0;

        while (true)
        {
            pt = scanner.Peek();
            if (pt.val == "(") { closureCount++; continue; }
            if (pt.val == ")") { closureCount--; continue; }
			if ((pt.kind == 0)||(pt.kind == _endline)) break;
		}
        scanner.ResetPeek();
        return (closureCount > 0) ? false : true;
    }

    private bool HasMoreAssignmentStatements()
    {
        Token pt = la;

        if (pt.kind != _ident)
            return false;

        bool gotAssignmentToken = false;
        bool isAssignmentStatement = false;

        while (true) {
            pt = scanner.Peek();
            if (pt.kind == 0) {
                break;
            }
            else if (pt.val == "=") {
                isAssignmentStatement = true;
                break;
            }
            else if (pt.kind == _endline) {
                isAssignmentStatement = gotAssignmentToken;
                break;
            }
            else if (pt.val == "{")
                break;
        }

        scanner.ResetPeek();
        return isAssignmentStatement;
    }

    private bool NotDefaultArg()
    {
        Token pt = la;          
        if (pt.val == ",")
            pt = scanner.Peek();
        //pt should be ident now
        if (pt.kind == _ident)
        {         
            pt = scanner.Peek();    //':'
            if (pt.val == ":")
            {
                pt = scanner.Peek();    //type
                if (pt.kind == _ident)
                {
                    pt = scanner.Peek();
                    scanner.ResetPeek();
                    if (pt.val == "=")
                         return false;
                }
            }
			if (pt.val == "=")
			{
				scanner.ResetPeek();
				return false;
			}
        }
        scanner.ResetPeek();        
        return true;    
    }

    private AssociativeNode GenerateBinaryOperatorMethodCallNode(Operator op, ProtoCore.AST.AssociativeAST.AssociativeNode op1, ProtoCore.AST.AssociativeAST.AssociativeNode op2)
    {
        FunctionCallNode funCallNode = new FunctionCallNode();
        IdentifierNode funcName = new IdentifierNode { Value = ProtoCore.DSASM.Op.GetOpFunction(op), Name = ProtoCore.DSASM.Op.GetOpFunction(op) };
        funCallNode.Function = funcName;
        funCallNode.Name = ProtoCore.DSASM.Op.GetOpFunction(op);
        funCallNode.FormalArguments.Add(op1); funCallNode.FormalArguments.Add(op2);

        NodeUtils.SetNodeLocation(funCallNode, op1, op2);
        return funCallNode;
    }

 	private AssociativeNode GenerateUnaryOperatorMethodCallNode(UnaryOperator op, ProtoCore.AST.AssociativeAST.AssociativeNode operand)
    {
        FunctionCallNode funCallNode = new FunctionCallNode();
        IdentifierNode funcName = new IdentifierNode { Value = ProtoCore.DSASM.Op.GetUnaryOpFunction(op), Name = ProtoCore.DSASM.Op.GetUnaryOpFunction(op) };
        funCallNode.Function = funcName;
        funCallNode.Name = ProtoCore.DSASM.Op.GetUnaryOpFunction(op);
        funCallNode.FormalArguments.Add(operand);

        NodeUtils.CopyNodeLocation(funCallNode, operand);
        return funCallNode;
    }

    private AST.ImperativeAST.IdentifierNode BuildImperativeIdentifier(string name, ProtoCore.PrimitiveType type = ProtoCore.PrimitiveType.Var)
    {
        var ident = new AST.ImperativeAST.IdentifierNode();
        ident.Name = ident.Value = name;
        ident.DataType = TypeSystem.BuildPrimitiveTypeObject(type, 0);
        return ident;
    }

    private bool IsKeyWord(string identName, bool checkReturn = false, bool checkThis = true)
    {
        if (identName == ProtoCore.DSDefinitions.Keyword.Return && !checkReturn)
        {
            return false;
        }

        if (checkThis && identName == ProtoCore.DSDefinitions.Keyword.This)
        {
            return true;
        }

        foreach (string kw in ProtoCore.DSDefinitions.Keyword.KeywordList)
        {
            if (kw == identName)
                return true;
        }
        return false;
    }

	 // use by associative
	 private bool IsNotAttributeFunctionClass()
	 {
		if (la.val == "[")
		{
		    Token t = scanner.Peek();
            while (t.val != "]" && t.kind != _EOF)
            {
                t = scanner.Peek();
            }
          
            if (t.val == "]")
            {
                Token next = scanner.Peek(); 
                scanner.ResetPeek();
                return (next.val == "{");
            }
            else
            {
                scanner.ResetPeek();
                return false;
            }
		}

		if (la.kind != _kw_class && la.kind != _kw_def)
			return true;
		return false;
	 }

	 // used by imperative
	 private bool IsNotAttributeFunction()
	 {
	    if (la.val == "[")
		{
		    Token t = scanner.Peek();
            while (t.val != "]" && t.kind != _EOF)
            {
                t = scanner.Peek();
               
            }
          
            if (t.val == "]")
            {
                Token next = scanner.Peek(); 
                scanner.ResetPeek();
                return (next.val == "{");
            }
            else
            {
                scanner.ResetPeek();
                return false;
            }
		}

        return la.kind != _kw_def;
	 }

	 
	 //Experiment for user-defined synErr message
	 private void SynErr (string s) {
		if (errDist >= minErrDist) 
		core.BuildStatus.LogSyntaxError(s, core.CurrentDSFileName, la.line, la.col);
		errors.count++;
		errDist = 0;
	 }



	
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
				if (la.kind == 65) {
				CommentNode cNode = new CommentNode(la.col, la.line, la.val, CommentNode.CommentType.Inline); 
				commentNode.Body.Add(cNode); 
				}
				if (la.kind == 66) {
				CommentNode cNode = new CommentNode(la.col, la.line, la.val, CommentNode.CommentType.Block); 
				commentNode.Body.Add(cNode); 
				}

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

	
	void DesignScriptParser() {
		Node node = null; 
		Hydrogen(out node);
		if (!core.IsParsingPreloadedAssembly && !core.IsParsingCodeBlockNode && !builtinMethodsLoaded)
		{
		   CoreUtils.InsertPredefinedAndBuiltinMethods(core, node as CodeBlockNode);
		   root = node;
		}
		else
		{
		   root = node;
		} 
		
	}

	void Hydrogen(out Node codeBlockNode) {
		ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = new ProtoCore.AST.AssociativeAST.CodeBlockNode();  
		NodeUtils.SetNodeStartLocation(codeblock, t); 
		ProtoCore.AST.AssociativeAST.AssociativeNode node = null; 
		ProtoFFI.ImportModuleHandler imh = null;
		if (core.IsParsingPreloadedAssembly)
		{
		  imh = core.ImportHandler;
		}
		else
		{
		   imh = this.ImportModuleHandler;
		}
		bool rootImport = null == imh;  
		
		while (la.kind == 34) {
			ProtoCore.AST.AssociativeAST.AssociativeNode importNode = null; 
			Import_Statement(out importNode);
			if (null != importNode)
			  (codeblock as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(importNode);
			
		}
		imh = null;
		if (core.IsParsingPreloadedAssembly)
		{
		  imh = core.ImportHandler;
		}
		else
		{
		   imh = ImportModuleHandler;
		}
		if(null != core.ContextDataManager)
		{
		  if (imh == null)
		      imh = new ProtoFFI.ImportModuleHandler(core);
		  ProtoCore.AST.AssociativeAST.AssociativeNode importNode = null;
		  importNode = core.ContextDataManager.Compile(imh);
		  if (null != importNode)
		      (codeblock as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(importNode);
		}
		
		if (rootImport && null != imh && imh.RootImportNode.CodeNode.Body.Count != 0)
		{
		  (codeblock as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(imh.RootImportNode);
		}
		
		if (rootImport && core.IsParsingPreloadedAssembly && !builtinMethodsLoaded)
		{
		   CoreUtils.InsertPredefinedAndBuiltinMethods(core, codeblock);
		  core.ImportNodes = codeblock;
		}
		
		while (StartOf(1)) {
			if (IsNotAttributeFunctionClass()) {
				Associative_Statement(out node);
			} else {
				if (la.kind == 27) {
					Associative_functiondecl(out node);
				} else {
					Associative_classdecl(out node);
				}
			}
			if (null != node)
			{
			   (codeblock as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(node); 
			   
			   stmtsParsed++;
			}
			
		}
		if (la.val == "if")
		  SynErr(String.Format(Resources.UseInlineConditional, la.val)); 
		if ((la.val == "for")||(la.val == "while"))
		  SynErr(String.Format(Resources.ValidForImperativeBlockOnly, la.val)); 
		codeBlockNode = codeblock;
		
		// We look ahead (la) here instead of looking at the current token (t)
		// because when we get here at the end of a language block, "t" would 
		// have been pointing to the ending token of the last statement in the 
		// language block. What we really need here is the closing bracket '}'
		// character, and that's conveniently residing in the look ahead token.
		// 
		NodeUtils.SetNodeEndLocation(codeblock, la); 
		
	}

	void Import_Statement(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		while (!(la.kind == 0 || la.kind == 34)) {SynErr(65); Get();}
		string moduleName = "", typeName = "", alias = "";
		
		Expect(34);
		Expect(12);
		if (la.kind == 4) {
			Get();
			moduleName = t.val;
		} else if (la.kind == 1) {
			Get();
			typeName = t.val;
			
			Expect(36);
			Expect(4);
			moduleName = t.val; 
		} else SynErr(66);
		Expect(13);
		if (la.kind == 35) {
			Get();
			Expect(1);
		}
		if (la.kind != _endline)
		  SynErr(Resources.SemiColonExpected);
		
		Expect(23);
		if (moduleName == null) {
		   node = null;
		   return;
		}
		
		ProtoFFI.ImportModuleHandler imh = null;
		if (core.IsParsingPreloadedAssembly)
		{
		   if (core.ImportHandler == null)
		   {
		       core.ImportHandler = new ProtoFFI.ImportModuleHandler(core);
		
		   }
		   imh = core.ImportHandler;
		}
		else
		{
		   if (this.ImportModuleHandler == null)
		   {
		
		   this.ImportModuleHandler = new ProtoFFI.ImportModuleHandler(core);
		}
		   imh = this.ImportModuleHandler;
		}
		
		node = imh.Import(moduleName, typeName, alias);
		
	}

	void Associative_Statement(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		while (!(StartOf(2))) {SynErr(67); Get();}
		if (!IsFullClosure()) SynErr(Resources.CloseBracketExpected); 
		node = null; 
		if (IsNonAssignmentStatement()) {
			Associative_NonAssignmentStatement(out node);
		} else if (IsFunctionCallStatement()) {
			Associative_FunctionCallStatement(out node);
		} else if (la.kind == 1 || la.kind == 12 || la.kind == 44) {
			Associative_FunctionalStatement(out node);
		} else if (la.kind == 10) {
			Associative_LanguageBlock(out node);
		} else if (StartOf(3)) {
			if (core.ParsingMode == ParseMode.AllowNonAssignment) {
				if (StartOf(4)) {
					Associative_Expression(out node);
				}
				Expect(23);
			} else {
				if (la.val != ";")
				   SynErr(Resources.SemiColonExpected);
				
				Get();
			}
		} else SynErr(68);
	}

	void Associative_functiondecl(out ProtoCore.AST.AssociativeAST.AssociativeNode node, ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public, bool isStatic = false) {
		ProtoCore.AST.AssociativeAST.FunctionDefinitionNode f = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode(); 
		string methodName;  
		ProtoCore.AST.AssociativeAST.AssociativeNode argumentSignature; 
		ProtoCore.Type returnType;  
		string externLibName = ""; 
		bool isExternLib = false; 
		
		Expect(27);
		NodeUtils.SetNodeLocation(f, t); 
		Associative_MethodSignature(out methodName, out argumentSignature, out returnType);
		if (isExternLib &&  "var" == returnType.Name){
		   errors.Warning(String.Format("External function {0} does not have a return type defined. Defaulting to var.", methodName));
		}
		
		
		f.IsExternLib = isExternLib; 
		f.ExternLibName = externLibName; 
		f.Name = methodName; 
		f.ReturnType = returnType; 
		f.Access = access;
		f.Signature = argumentSignature as ProtoCore.AST.AssociativeAST.ArgumentSignatureNode; 
		f.IsStatic = isStatic;
		ProtoCore.AST.AssociativeAST.AssociativeNode functionBody = null; 
		
		Associative_FunctionDefinitionBody(out functionBody);
		f.FunctionBody = functionBody as ProtoCore.AST.AssociativeAST.CodeBlockNode; 
		node = f;   
		
	}

	void Associative_classdecl(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = new ProtoCore.AST.AssociativeAST.ClassDeclNode(); 
		NodeUtils.SetNodeLocation(classnode, la); 
		Expect(25);
		Expect(1);
		classnode.ClassName = t.val; 
		isInClass = true;
		if (IsKeyWord(t.val, true))
		{
		    errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
		}
		
		if (la.kind == 28) {
			Get();
			Expect(1);
			classnode.BaseClass = t.val;
			
		}
		Expect(44);
		while (StartOf(5)) {
			ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public; 
			if (la.kind == 47 || la.kind == 48 || la.kind == 49) {
				Associative_AccessSpecifier(out access);
			}
			if (la.kind == 26) {
				ProtoCore.AST.AssociativeAST.AssociativeNode constr = null; 
				Associative_constructordecl(out constr, access);
				if (String.IsNullOrEmpty(constr.Name))
				{
				   constr.Name= classnode.ClassName;
				}
				classnode.Procedures.Add(constr); 
				
			} else if (la.kind == 1 || la.kind == 23 || la.kind == 39) {
				bool isStatic = false; 
				if (la.kind == 39) {
					Get();
					isStatic = true; 
				}
				if (la.kind == 1) {
					ProtoCore.AST.AssociativeAST.AssociativeNode varnode = null; 
					Associative_memvardecl(out varnode, access, isStatic);
					classnode.Variables.Add(varnode); 
					if (la.val != ";")
					   SynErr(Resources.SemiColonExpected);  
					
					Expect(23);
					NodeUtils.SetNodeEndLocation(varnode, t); 
				} else if (la.kind == 23) {
					Get();
				} else SynErr(69);
			} else SynErr(70);
		}
		Expect(45);
		isInClass = false; classnode.endLine = t.line; classnode.endCol = t.col; 
		node = classnode; 
	}

	void Associative_NonAssignmentStatement(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		while (!(StartOf(6))) {SynErr(71); Get();}
		node = null; 
		ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = null;
		 
		Associative_Expression(out rightNode);
		ProtoCore.AST.AssociativeAST.BinaryExpressionNode expressionNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode();
		ProtoCore.AST.AssociativeAST.IdentifierNode leftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode();
		leftNode.Value = leftNode.Name = Constants.kTempProcLeftVar;
		
		var unknownType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
		leftNode.datatype = unknownType;
		leftNode.line = rightNode.line;
		leftNode.col = rightNode.col;
		leftNode.endLine = rightNode.endLine;
		leftNode.endCol = rightNode.endCol;
		
		expressionNode.LeftNode = leftNode;
		expressionNode.RightNode = rightNode;
		expressionNode.Optr = Operator.assign;
		NodeUtils.UpdateBinaryExpressionLocation(expressionNode);
		node = expressionNode;
		
		if (la.val != ";")
		   SynErr(Resources.SemiColonExpected);  
		
		Expect(23);
		NodeUtils.SetNodeEndLocation(node, t); 
	}

	void Associative_FunctionCallStatement(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		while (!(StartOf(6))) {SynErr(72); Get();}
		node = null; 
		ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = null;
		 
		Associative_Expression(out rightNode);
		bool allowIdentList = core.Options.GenerateSSA && rightNode is ProtoCore.AST.AssociativeAST.IdentifierListNode;
		
		//Try to make a false binary expression node.
		if (rightNode is ProtoCore.AST.AssociativeAST.FunctionDotCallNode 
		   || rightNode is ProtoCore.AST.AssociativeAST.FunctionCallNode
		   || allowIdentList)
		
		{
		ProtoCore.AST.AssociativeAST.BinaryExpressionNode expressionNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode();
		ProtoCore.AST.AssociativeAST.IdentifierNode leftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode();
		leftNode.Value = leftNode.Name = Constants.kTempProcLeftVar;
		
		var unknownType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
		leftNode.datatype = unknownType;
		leftNode.line = rightNode.line;
		leftNode.col = rightNode.col;
		leftNode.endLine = rightNode.endLine;
		leftNode.endCol = rightNode.endCol;
		
		expressionNode.LeftNode = leftNode;
		expressionNode.RightNode = rightNode;
		expressionNode.Optr = Operator.assign;
		NodeUtils.UpdateBinaryExpressionLocation(expressionNode);
		node = expressionNode;
		}
		
		if (la.val != ";")
		   SynErr(Resources.SemiColonExpected);  
		
		Expect(23);
		NodeUtils.SetNodeEndLocation(node, t); 
	}

	void Associative_FunctionalStatement(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		while (!(StartOf(7))) {SynErr(73); Get();}
		node = null; 
		ProtoCore.AST.AssociativeAST.AssociativeNode leftNode = null; 
		ProtoCore.AST.AssociativeAST.BinaryExpressionNode expressionNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(); 
		isLeft = true; 
		Associative_DecoratedIdentifier(out leftNode);
		isLeft = false;
		NodeUtils.CopyNodeLocation(expressionNode, leftNode);
		node = leftNode; 
		
		if (la.kind == 23) {
			Get();
			if (core.ParsingMode != ParseMode.AllowNonAssignment)
			{
			   expressionNode.LeftNode = leftNode;
			   expressionNode.RightNode = null;
			   expressionNode.Optr = Operator.assign;
			   NodeUtils.UpdateBinaryExpressionLocation(expressionNode);
			   node = expressionNode;
			}
			
		} else if (la.kind == 51) {
			Get();
			ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = null;
			bool isLeftMostNode = false; 
			if (leftVar == null) 
			{
			   if (node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
			   {
			       isLeftVarIdentList = true;
			       leftVar = ProtoCore.Utils.CoreUtils.GenerateIdentListNameString(node);
			   }
			   else
			   {
			       isLeftVarIdentList = false;
			       leftVar = node.Name; 
			   }
			   isLeftMostNode = true;
			   withinModifierCheckScope = true;
			} 
			
			if (HasMoreAssignmentStatements()) {
				Associative_FunctionalStatement(out rightNode);
				expressionNode.LeftNode = leftNode; 
				expressionNode.RightNode = rightNode; 
				NodeUtils.SetNodeEndLocation(expressionNode, rightNode);
				expressionNode.Optr = Operator.assign; 
				expressionNode.isMultipleAssign = true;
				node = expressionNode; 
				
			} else if (la.kind == 10) {
				withinModifierCheckScope = false; 
				
				Associative_LanguageBlock(out rightNode);
				NodeUtils.SetNodeEndLocation(expressionNode, t);
				expressionNode.LeftNode = leftNode;
				expressionNode.RightNode = rightNode;
				expressionNode.Optr = Operator.assign;
				node = expressionNode; 
				
			} else if (StartOf(4)) {
				Associative_Expression(out rightNode);
				expressionNode.LeftNode = leftNode;
				expressionNode.RightNode = rightNode;
				expressionNode.Optr = Operator.assign;
				NodeUtils.UpdateBinaryExpressionLocation(expressionNode);
				
				if (rightNode is ProtoCore.AST.AssociativeAST.ExprListNode)
				   expressionNode.RightNode.Name = leftVar;
				
				if (la.kind != _endline)
				  SynErr(Resources.SemiColonExpected);
				
				Expect(23);
				NodeUtils.SetNodeEndLocation(expressionNode, t); node = expressionNode; 
			} else SynErr(74);
			if (isLeftMostNode) 
			{
			   leftVar = null;
			   if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
			   {
			       node.IsModifier = isModifier;   
			   }
			   isModifier = false;
			   withinModifierCheckScope = false;
			   isLeftVarIdentList = false;                                  
			}  
			
		} else if (StartOf(8)) {
			SynErr(Resources.SemiColonExpected); 
		} else SynErr(75);
	}

	void Associative_LanguageBlock(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode(); 
		
		Expect(10);
		NodeUtils.SetNodeLocation(langblock, t); 
		Expect(1);
		if( 0 == t.val.CompareTo(ProtoCore.DSASM.kw.imperative)) {
		   langblock.codeblock.Language = ProtoCore.Language.Imperative;
		}
		else if( 0 == t.val.CompareTo(ProtoCore.DSASM.kw.associative)) {
		   langblock.codeblock.Language = ProtoCore.Language.Associative; 
		}
		else {
		   langblock.codeblock.Language = ProtoCore.Language.NotSpecified;
		   errors.SemErr(t.line, t.col, String.Format(Resources.InvalidLanguageBlockIdentifier, t.val));
		}
		
		Expect(11);
		Expect(44);
		Node codeBlockNode = null; 
		if (langblock.codeblock.Language == ProtoCore.Language.Associative ||
langblock.codeblock.Language == ProtoCore.Language.NotSpecified) {
			Hydrogen(out codeBlockNode);
		} else if (langblock.codeblock.Language == ProtoCore.Language.Imperative ) {
			Imperative(out codeBlockNode);
		} else SynErr(76);
		if (langblock.codeblock.Language == ProtoCore.Language.NotSpecified) {
			int openCurlyBraceCount = 0, closeCurlyBraceCount = 0; 
			ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlockInvalid = new ProtoCore.AST.AssociativeAST.CodeBlockNode(); 
			ProtoCore.AST.AssociativeAST.AssociativeNode validBlockInInvalid = null; 
			while (closeCurlyBraceCount <= openCurlyBraceCount) {
				if (la.kind == 10) {
					Associative_LanguageBlock(out validBlockInInvalid);
					codeBlockInvalid.Body.Add(validBlockInInvalid); 
				} else if (la.kind == 44) {
					Get();
					openCurlyBraceCount++; 
				} else if (la.kind == 45) {
					Get();
					closeCurlyBraceCount++; 
				} else if (la.kind == 0) {
					Get();
					Expect(45);
					break; 
				} else if (StartOf(8)) {
					Get(); 
				} else SynErr(77);
			}
			codeBlockNode = codeBlockInvalid; 
		} else if (la.kind == 45) {
			Get();
		} else SynErr(78);
		langblock.CodeBlockNode = codeBlockNode; 
		node = langblock; 
	}

	void Associative_Expression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		Associative_LogicalExpression(out node);
		while (la.kind == 52) {
			Associative_TernaryOp(ref node);
		}
	}

	void Associative_StatementList(out List<ProtoCore.AST.AssociativeAST.AssociativeNode> nodelist) {
		nodelist = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>(); 
		while (StartOf(9)) {
			ProtoCore.AST.AssociativeAST.AssociativeNode node = null; 
			Associative_Statement(out node);
			if (null != node) nodelist.Add(node); 
		}
		if (la.val == "if")
		   SynErr(String.Format(Resources.UseInlineConditional, la.val)); 
		if ((la.val == "for")||(la.val == "while"))
		    SynErr(String.Format(Resources.ValidForImperativeBlockOnly, la.val));
		
	}

	void Associative_AccessSpecifier(out ProtoCore.CompilerDefinitions.AccessModifier access) {
		access = ProtoCore.CompilerDefinitions.AccessModifier.Public; 
		if (la.kind == 47) {
			Get();
		} else if (la.kind == 48) {
			Get();
			access = ProtoCore.CompilerDefinitions.AccessModifier.Private; 
		} else if (la.kind == 49) {
			Get();
			access = ProtoCore.CompilerDefinitions.AccessModifier.Protected; 
		} else SynErr(79);
	}

	void Associative_constructordecl(out ProtoCore.AST.AssociativeAST.AssociativeNode constrNode, ProtoCore.CompilerDefinitions.AccessModifier access) {
		ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode constr = new ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode(); ;                                 
		string methodName;  
		ProtoCore.AST.AssociativeAST.AssociativeNode argumentSignature; 
		
		Expect(26);
		NodeUtils.SetNodeStartLocation(constr, t); 
		Associative_CtorSignature(out methodName, out argumentSignature);
		var returnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank);
		
		constr.Name = methodName; 
		constr.ReturnType = returnType;
		constr.Signature = argumentSignature as ProtoCore.AST.AssociativeAST.ArgumentSignatureNode;
		constr.Access = access; 
		ProtoCore.AST.AssociativeAST.AssociativeNode functionBody = null; 
		
		if (la.kind == 46) {
			Get();
			ProtoCore.AST.AssociativeAST.AssociativeNode bnode; 
			Associative_BaseConstructorCall(out bnode);
			constr.BaseConstructor = bnode as ProtoCore.AST.AssociativeAST.FunctionCallNode; 
		}
		Associative_FunctionDefinitionBody(out functionBody);
		constr.FunctionBody = functionBody as ProtoCore.AST.AssociativeAST.CodeBlockNode; 
		NodeUtils.SetNodeEndLocation(constr, functionBody); 
		constrNode = constr; 
	}

	void Associative_memvardecl(out ProtoCore.AST.AssociativeAST.AssociativeNode node, ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public, bool isStatic = false) {
		ProtoCore.AST.AssociativeAST.IdentifierNode tNode = null; 
		ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode(); 
		varDeclNode.Access = access;
		varDeclNode.IsStatic = isStatic;
		
		Expect(1);
		if (IsKeyWord(t.val, true))
		{
		   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
		}
		NodeUtils.SetNodeLocation(varDeclNode, t);
		tNode = AstFactory.BuildIdentifier(t.val);
		NodeUtils.SetNodeLocation(tNode, t);
		varDeclNode.NameNode = tNode;
		
		if (la.kind == 46) {
			Get();
			Expect(1);
			ProtoCore.Type argtype = new ProtoCore.Type(); argtype.Name = t.val; argtype.rank = 0; 
			if (la.kind == 10) {
				Get();
				Expect(11);
				argtype.rank = 1; 
				if (la.kind == 10 || la.kind == 24) {
					if (la.kind == 24) {
						Get();
						Expect(10);
						Expect(11);
						argtype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 10) {
							Get();
							Expect(11);
							argtype.rank++; 
						}
					}
				}
			}
			string oldName = tNode.Name;
			string oldValue = tNode.Value;
			
			// Here since the variable has an explicitly specified type 
			// the "IdentifierNode" should really be "TypedIdentifierNode"
			// (which is used to indicate the identifier that has explicit 
			// type specified).
			tNode = new ProtoCore.AST.AssociativeAST.TypedIdentifierNode()
			{
			   Name = oldName,
			   Value = oldValue
			};
			
			argtype.UID = core.TypeSystem.GetType(argtype.Name);
			tNode.datatype = argtype;
			varDeclNode.NameNode = tNode;
			varDeclNode.ArgumentType = argtype;
			
		}
		node = varDeclNode; 
		//if(!isGlobalScope) {
		//    localVarCount++;
		//}
		
	}

	void Imperative(out Node codeBlockNode) {
		ProtoCore.AST.ImperativeAST.ImperativeNode node = null; 
		ProtoCore.AST.ImperativeAST.CodeBlockNode codeblock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
		NodeUtils.SetNodeStartLocation(codeblock, t);
		
		while (StartOf(10)) {
			Imperative_stmt(out node);
			if (null != node)   
			   codeblock.Body.Add(node); 
			
		}
		codeBlockNode = codeblock;
		
		// We look ahead (la) here instead of looking at the current token (t)
		// because when we get here at the end of a language block, "t" would 
		// have been pointing to the ending token of the last statement in the 
		// language block. What we really need here is the closing bracket '}'
		// character, and that's conveniently residing in the look ahead token.
		// 
		NodeUtils.SetNodeEndLocation(codeblock, la);
		
	}

	void Associative_CtorSignature(out string ctorName, out ProtoCore.AST.AssociativeAST.AssociativeNode argumentSign) {
		ctorName = null;  
		if (la.kind == 1) {
			Get();
			ctorName = t.val; 
			if (IsKeyWord(ctorName, true))
			{
			    errors.SemErr(t.line, t.col, String.Format(Resources.keywordCannotBeUsedAsConstructorName, t.val));
			}
			
		}
		ProtoCore.AST.AssociativeAST.AssociativeNode argumentSignature = null;
		Associative_ArgumentSignatureDefinition(out argumentSignature);
		argumentSign = argumentSignature; 
	}

	void Associative_BaseConstructorCall(out ProtoCore.AST.AssociativeAST.AssociativeNode bnode) {
		ProtoCore.AST.AssociativeAST.FunctionCallNode f = new ProtoCore.AST.AssociativeAST.FunctionCallNode(); 
		List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = null; 
		if (la.val != "base")
		{
		   SynErr(Resources.BaseIsExpectedToCallBaseConstructor); 
		}
		else
		{
		   Get();
		   NodeUtils.SetNodeLocation(f, t);
		}
		
		if (la.kind == 6) {
			Get();
			Associative_Ident(out bnode);
			f.Function = bnode; 
		}
		Associative_Arguments(out args);
		f.FormalArguments = args; 
		bnode = f; 
	}

	void Associative_FunctionDefinitionBody(out ProtoCore.AST.AssociativeAST.AssociativeNode funcBody) {
		ProtoCore.AST.AssociativeAST.CodeBlockNode functionBody = new ProtoCore.AST.AssociativeAST.CodeBlockNode(); 
		List<ProtoCore.AST.AssociativeAST.AssociativeNode> body = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>(); 
		NodeUtils.SetNodeStartLocation(functionBody, la);
		
		Expect(44);
		Associative_StatementList(out body);
		functionBody.Body =body;  
		Expect(45);
		NodeUtils.SetNodeEndLocation(functionBody, t); 
		funcBody = functionBody; 
	}

	void Associative_Ident(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Expect(1);
		if (!disableKwCheck && IsKeyWord(t.val, false, false))
		{
		   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
		}
		int ltype = (0 == String.Compare(t.val, "return")) ? (int)ProtoCore.PrimitiveType.Return : (int)ProtoCore.PrimitiveType.Var;
		if (ltype == (int)ProtoCore.PrimitiveType.Return && la.val != "=")
		{
		    SynErr(String.Format(Resources.InvalidReturnStatement, la.val));
		}
		
		var identNode = AstFactory.BuildIdentifier(t.val);
		identNode.datatype = TypeSystem.BuildPrimitiveTypeObject((ProtoCore.PrimitiveType)ltype);
		node = identNode;
		NodeUtils.SetNodeLocation(node, t);
		
	}

	void Associative_Arguments(out List<ProtoCore.AST.AssociativeAST.AssociativeNode> nodes) {
		Expect(12);
		if (!IsFullClosure()) SynErr(Resources.CloseBracketExpected); 
		nodes = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>(); 
		if (StartOf(4)) {
			ProtoCore.AST.AssociativeAST.AssociativeNode t; 
			Associative_Expression(out t);
			nodes.Add(t); 
			while (WeakSeparator(50,4,11) ) {
				Associative_Expression(out t);
				nodes.Add(t); 
			}
		}
		Expect(13);
	}

	void Associative_MethodSignature(out string methodName, out ProtoCore.AST.AssociativeAST.AssociativeNode argumentSign, out ProtoCore.Type returnType) {
		Expect(1);
		methodName = t.val; 
		if (IsKeyWord(t.val, true))
		{
		   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
		}
		ProtoCore.AST.AssociativeAST.AssociativeNode argumentSignature = null;
		returnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank);
		
		if (la.kind == 46) {
			Associative_TypeRestriction(out returnType);
		}
		Associative_ArgumentSignatureDefinition(out argumentSignature);
		argumentSign = argumentSignature; 
	}

	void Associative_TypeRestriction(out ProtoCore.Type type) {
		Expect(46);
		Associative_ClassReference(out type);
		type.rank = 0; 
		if (la.kind == 10) {
			Get();
			Expect(11);
			type.rank = 1; 
			if (la.kind == 10 || la.kind == 24) {
				if (la.kind == 24) {
					Get();
					Expect(10);
					Expect(11);
					type.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
				} else {
					while (la.kind == 10) {
						Get();
						Expect(11);
						type.rank++; 
					}
				}
			}
		}
	}

	void Associative_ArgumentSignatureDefinition(out ProtoCore.AST.AssociativeAST.AssociativeNode argumentSign) {
		ProtoCore.AST.AssociativeAST.ArgumentSignatureNode argumentSignature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode(); 
		Expect(12);
		NodeUtils.SetNodeLocation(argumentSignature, t); 
		ProtoCore.AST.AssociativeAST.AssociativeNode arg;
		if (la.kind == 1) {
			if (NotDefaultArg()) { 
			Associative_ArgDecl(out arg);
			argumentSignature.AddArgument(arg as ProtoCore.AST.AssociativeAST.VarDeclNode); 
			while (la.kind == 50) {
				if (NotDefaultArg()) { 
				ExpectWeak(50, 12);
				Associative_ArgDecl(out arg);
				argumentSignature.AddArgument(arg as ProtoCore.AST.AssociativeAST.VarDeclNode); 
				} else break; 
			}
			} 
		}
		if (la.kind == 1 || la.kind == 50) {
			if (la.kind == 50) {
				Get();
			}
			Associative_DefaultArgDecl(out arg);
			argumentSignature.AddArgument(arg as ProtoCore.AST.AssociativeAST.VarDeclNode); 
			while (la.kind == 50) {
				Get();
				Associative_DefaultArgDecl(out arg);
				argumentSignature.AddArgument(arg as ProtoCore.AST.AssociativeAST.VarDeclNode); 
			}
		}
		Expect(13);
		argumentSign = argumentSignature; 
	}

	void Associative_ArgDecl(out ProtoCore.AST.AssociativeAST.AssociativeNode node, ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public) {
		ProtoCore.AST.AssociativeAST.IdentifierNode tNode = null; 
		ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode(); 
		varDeclNode.Access = access;
		
		Expect(1);
		if (IsKeyWord(t.val, true))
		{
		   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
		}
		tNode = AstFactory.BuildIdentifier(t.val);
		NodeUtils.SetNodeLocation(tNode, t);
		varDeclNode.NameNode = tNode;
		NodeUtils.CopyNodeLocation(varDeclNode, tNode);
		
		ProtoCore.Type argtype = new ProtoCore.Type(); argtype.Name = "var"; argtype.rank = 0; argtype.UID = 0; 
		if (la.kind == 46) {
			Get();
			Expect(1);
			argtype.Name = t.val; 
			if (la.kind == 10) {
				Get();
				Expect(11);
				argtype.rank = 1; 
				if (la.kind == 10 || la.kind == 24) {
					if (la.kind == 24) {
						Get();
						Expect(10);
						Expect(11);
						argtype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 10) {
							Get();
							Expect(11);
							argtype.rank++; 
						}
					}
				}
			}
		}
		varDeclNode.ArgumentType = argtype; 
		node = varDeclNode; 
	}

	void Associative_DefaultArgDecl(out ProtoCore.AST.AssociativeAST.AssociativeNode node, ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public) {
		Associative_ArgDecl(out node);
		ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = node as ProtoCore.AST.AssociativeAST.VarDeclNode; 
		Expect(51);
		ProtoCore.AST.AssociativeAST.AssociativeNode rhsNode; 
		Associative_Expression(out rhsNode);
		ProtoCore.AST.AssociativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode();
		NodeUtils.CopyNodeLocation(bNode, varDeclNode);
		bNode.LeftNode = varDeclNode.NameNode;
		bNode.RightNode = rhsNode;
		bNode.Optr = Operator.assign;
		varDeclNode.NameNode = bNode;
		
		node = varDeclNode; 
	}

	void Associative_ClassReference(out ProtoCore.Type type) {
		type = new ProtoCore.Type(); 
		string name; 
		Expect(1);
		name = t.val; 
		type.Name = name; 
		type.UID = 0; 
	}

	void TypedIdentifierList(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Expect(1);
		node = new IdentifierNode(t.val); 
		while (la.kind == 6) {
			Get();
			ProtoCore.AST.AssociativeAST.AssociativeNode rnode = null; 
			Expect(1);
			rnode = new IdentifierNode(t.val);
			      
			ProtoCore.AST.AssociativeAST.IdentifierListNode bnode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
			bnode.LeftNode = node;
			bnode.Optr = Operator.dot;
			bnode.RightNode = rnode;
			node = bnode;
			NodeUtils.SetNodeLocation(bnode, bnode.LeftNode, bnode.RightNode); 
			
		}
	}

	void Associative_DecoratedIdentifier(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		if (IsLocallyTypedVariable()) {
			Expect(1);
			if (IsKeyWord(t.val, true))
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
			}
			var typedVar = new ProtoCore.AST.AssociativeAST.TypedIdentifierNode();
			typedVar.Name = typedVar.Value = t.val;
			NodeUtils.SetNodeLocation(typedVar, t);
			
			Expect(46);
			Expect(40);
			typedVar.IsLocal = true;
			
			Expect(1);
			int type = core.TypeSystem.GetType(t.val); 
			if (type == ProtoCore.DSASM.Constants.kInvalidIndex)
			{
			   var unknownType = new ProtoCore.Type();
			   unknownType.UID = ProtoCore.DSASM.Constants.kInvalidIndex;
			   unknownType.Name = t.val; 
			   typedVar.datatype = unknownType;
			}
			else
			{
			   typedVar.datatype = core.TypeSystem.BuildTypeObject(type, 0);
			}
			
			if (la.kind == 10) {
				var datatype = typedVar.datatype; 
				Get();
				Expect(11);
				datatype.rank = 1; 
				if (la.kind == 10 || la.kind == 24) {
					if (la.kind == 24) {
						Get();
						Expect(10);
						Expect(11);
						datatype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 10) {
							Get();
							Expect(11);
							datatype.rank++; 
						}
					}
				}
				typedVar.datatype = datatype; 
			}
			node = typedVar; 
		} else if (IsLocalVariable()) {
			Expect(1);
			if (IsKeyWord(t.val, true))
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
			}
			var identNode = new ProtoCore.AST.AssociativeAST.IdentifierNode();
			identNode.Name = identNode.Value = t.val;
			NodeUtils.SetNodeLocation(identNode, t);
			
			Expect(46);
			Expect(40);
			identNode.IsLocal = true;
			
			node = identNode; 
		} else if (IsTypedVariable()) {
			Expect(1);
			if (IsKeyWord(t.val, true))
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
			}
			var typedVar = new ProtoCore.AST.AssociativeAST.TypedIdentifierNode();
			typedVar.Name = typedVar.Value = t.val;
			NodeUtils.SetNodeLocation(typedVar, t);
			
			Expect(46);
			string strIdent = string.Empty;
			int type = ProtoCore.DSASM.Constants.kInvalidIndex;
			
			if (IsIdentList()) {
				TypedIdentifierList(out node);
				strIdent = node.ToString(); 
			} else if (la.kind == 1) {
				Get();
				strIdent = t.val; 
			} else SynErr(80);
			type = core.TypeSystem.GetType(strIdent);
			typedVar.TypeAlias = strIdent;
			if (type == ProtoCore.DSASM.Constants.kInvalidIndex)
			{
			var unknownType = new ProtoCore.Type();
			unknownType.UID = ProtoCore.DSASM.Constants.kInvalidIndex;
			unknownType.Name = strIdent; 
			typedVar.datatype = unknownType;
			}
			else
			{
			typedVar.datatype = core.TypeSystem.BuildTypeObject(type, 0);
			}
			
			if (la.kind == 10) {
				var datatype = typedVar.datatype; 
				Get();
				Expect(11);
				datatype.rank = 1; 
				if (la.kind == 10 || la.kind == 24) {
					if (la.kind == 24) {
						Get();
						Expect(10);
						Expect(11);
						datatype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 10) {
							Get();
							Expect(11);
							datatype.rank++; 
						}
					}
				}
				typedVar.datatype = datatype; 
			}
			node = typedVar; 
		} else if (la.kind == 1 || la.kind == 12 || la.kind == 44) {
			Associative_IdentifierList(out node);
		} else SynErr(81);
	}

	void Associative_IdentifierList(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null;
		if (isInClass && IsIdentList())
		{
		   disableKwCheck = true;
		}
		
		Associative_NameReference(out node);
		disableKwCheck = false; 
		ProtoCore.AST.AssociativeAST.AssociativeNode inode = node; 
		
		while (la.kind == 6) {
			Get();
			ProtoCore.AST.AssociativeAST.AssociativeNode rnode = null; 
			Associative_NameReference(out rnode);
			if ((inode is ProtoCore.AST.AssociativeAST.IdentifierNode) &&
			   (inode as ProtoCore.AST.AssociativeAST.IdentifierNode).Name == ProtoCore.DSDefinitions.Keyword.This &&
			   (rnode is ProtoCore.AST.AssociativeAST.FunctionCallNode))
			{
			   node = rnode;
			   return;
			}
			
			
			ProtoCore.AST.AssociativeAST.IdentifierListNode bnode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
			bnode.LeftNode = node;
			bnode.Optr = Operator.dot;
			bnode.RightNode = rnode;
			node = bnode;
			NodeUtils.SetNodeLocation(bnode, bnode.LeftNode, bnode.RightNode);
			
			if (!core.Options.GenerateSSA)
			{
			bool isNeitherIdentOrFunctionCall = !(rnode is ProtoCore.AST.AssociativeAST.IdentifierNode || rnode is ProtoCore.AST.AssociativeAST.FunctionCallNode);
			if (isLeft || isNeitherIdentOrFunctionCall)
			{
			   node = inode;
			}
			else 
			{
			   if (rnode is ProtoCore.AST.AssociativeAST.IdentifierNode)
			   {
			       ProtoCore.AST.AssociativeAST.FunctionCallNode rcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
			       rcall.Function = rnode;
			       rcall.Function.Name = ProtoCore.DSASM.Constants.kGetterPrefix + rcall.Function.Name;
			       bnode.RightNode = rcall;
			
			       NodeUtils.SetNodeLocation(rcall, rnode, rnode);
			       node = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(bnode.LeftNode, rcall, core);
			   }
			   else
			   {
			       string rhsName = null;
			       ProtoCore.AST.AssociativeAST.ExprListNode dimList = null;
			       int dim = 0;
			       if (rnode is ProtoCore.AST.AssociativeAST.FunctionCallNode)
			       {
			           ProtoCore.AST.AssociativeAST.FunctionCallNode rhsFNode = rnode as ProtoCore.AST.AssociativeAST.FunctionCallNode;
			           node = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(node, rhsFNode, core);
			       }
			   }
			}
			}
			
		}
		{
		   if (!isModifier && withinModifierCheckScope)
		   {
		       if (isLeftVarIdentList)
		       {
		           if (inode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
		           {
		               isModifier = false;
		               if (node is ProtoCore.AST.AssociativeAST.FunctionDotCallNode)
		               {
		                   ProtoCore.AST.AssociativeAST.FunctionDotCallNode fdotCall = node as ProtoCore.AST.AssociativeAST.FunctionDotCallNode;
		                   string checkVar = ProtoCore.Utils.CoreUtils.GenerateIdentListNameString(fdotCall.GetIdentList());
		                   isModifier = (leftVar == checkVar);
		               }
		           }
		       }
		       else if (inode is ProtoCore.AST.AssociativeAST.IdentifierNode)
		       {
		           isModifier = (leftVar == inode.Name);
		       }   
		
		       // The LHS is an identifier
		       else
		       {
		           // It is a modifier if the lhs is:
		           //   1. the same as the current node
		           //   2. the current node starts with the lhs identifier
		           isModifier = (leftVar == inode.Name);
		           if (!isModifier)
		           {
		               string rhsString = ProtoCore.Utils.CoreUtils.GenerateIdentListNameString(inode);
		
		               isModifier = rhsString.StartsWith(leftVar);
		           }
		       }
		   }
		}
		
	}

	void Associative_LogicalExpression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Associative_ComparisonExpression(out node);
		while (la.kind == 57 || la.kind == 58) {
			Operator op;
			Associative_LogicalOp(out op);
			ProtoCore.AST.AssociativeAST.AssociativeNode expr2; 
			Associative_ComparisonExpression(out expr2);
			node = GenerateBinaryOperatorMethodCallNode(op, node, expr2);
			
		}
	}

	void Associative_TernaryOp(ref ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		ProtoCore.AST.AssociativeAST.InlineConditionalNode inlineConNode = new ProtoCore.AST.AssociativeAST.InlineConditionalNode(); 
		Expect(52);
		inlineConNode.ConditionExpression = node; node = null; 
		Associative_Expression(out node);
		inlineConNode.TrueExpression = node; 
		Expect(46);
		node = null; 
		Associative_Expression(out node);
		inlineConNode.FalseExpression = node;
		node = inlineConNode;
		NodeUtils.SetNodeLocation(inlineConNode, inlineConNode.ConditionExpression, inlineConNode.FalseExpression);
		
	}

	void Associative_UnaryExpression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		if (StartOf(13)) {
			Associative_NegExpression(out node);
		} else if (la.kind == 14 || la.kind == 59) {
			Associative_BitUnaryExpression(out node);
		} else SynErr(82);
	}

	void Associative_NegExpression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		UnaryOperator op; 
		ProtoCore.AST.AssociativeAST.AssociativeNode exprNode = null; 
		Associative_negop(out op);
		Associative_IdentifierList(out exprNode);
		node = GenerateUnaryOperatorMethodCallNode(op, exprNode);
		
	}

	void Associative_BitUnaryExpression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		UnaryOperator op; 
		ProtoCore.AST.AssociativeAST.AssociativeNode exprNode; 
		Associative_unaryop(out op);
		Associative_Factor(out exprNode);
		node = GenerateUnaryOperatorMethodCallNode(op, exprNode);
		
	}

	void Associative_unaryop(out UnaryOperator op) {
		op = UnaryOperator.None; 
		if (la.kind == 14) {
			Get();
			op = UnaryOperator.Not;    
			#if ENABLE_BIT_OP          
		} else if (la.kind == 59) {
			Get();
			op = UnaryOperator.Negate; 
			#endif                     
		} else SynErr(83);
	}

	void Associative_Factor(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		if (IsNumber()) {
			Associative_Number(out node);
		} else if (la.kind == 41) {
			Get();
			node = new ProtoCore.AST.AssociativeAST.BooleanNode(true);
			NodeUtils.SetNodeLocation(node, t);
			
		} else if (la.kind == 42) {
			Get();
			node = new ProtoCore.AST.AssociativeAST.BooleanNode(false);
			NodeUtils.SetNodeLocation(node, t);
			
		} else if (la.kind == 43) {
			Get();
			node = new ProtoCore.AST.AssociativeAST.NullNode();
			NodeUtils.SetNodeLocation(node, t);
			
		} else if (la.kind == 5) {
			Associative_Char(out node);
		} else if (la.kind == 4) {
			Associative_String(out node);
		} else if (la.kind == 1 || la.kind == 12 || la.kind == 44) {
			Associative_IdentifierList(out node);
		} else if (StartOf(14)) {
			Associative_UnaryExpression(out node);
		} else SynErr(84);
	}

	void Associative_negop(out UnaryOperator op) {
		op = UnaryOperator.None; 
		if (la.kind == 1 || la.kind == 12 || la.kind == 44) {
		} else if (la.kind == 15) {
			Get();
			op = UnaryOperator.Neg; 
		} else SynErr(85);
	}

	void Associative_ComparisonExpression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Associative_RangeExpr(out node);
		while (StartOf(15)) {
			Operator op; 
			Associative_ComparisonOp(out op);
			ProtoCore.AST.AssociativeAST.AssociativeNode expr2; 
			Associative_RangeExpr(out expr2);
			node = GenerateBinaryOperatorMethodCallNode(op, node, expr2);
			
		}
	}

	void Associative_LogicalOp(out Operator op) {
		op = Operator.and; 
		if (la.kind == 57) {
			Get();
		} else if (la.kind == 58) {
			Get();
			op = Operator.or; 
		} else SynErr(86);
	}

	void Associative_ComparisonOp(out Operator op) {
		op = Operator.none; 
		switch (la.kind) {
		case 18: {
			Get();
			op = Operator.gt; 
			break;
		}
		case 20: {
			Get();
			op = Operator.ge; 
			break;
		}
		case 17: {
			Get();
			op = Operator.lt; 
			break;
		}
		case 19: {
			Get();
			op = Operator.le; 
			break;
		}
		case 21: {
			Get();
			op = Operator.eq; 
			break;
		}
		case 22: {
			Get();
			op = Operator.nq; 
			break;
		}
		default: SynErr(87); break;
		}
	}

	void Associative_RangeExpr(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Associative_ArithmeticExpression(out node);
		if (la.kind == 24) {
			ProtoCore.AST.AssociativeAST.RangeExprNode rnode = new ProtoCore.AST.AssociativeAST.RangeExprNode(); 
			rnode.From = node; NodeUtils.SetNodeStartLocation(rnode, node);
			bool hasRangeAmountOperator = false;
			
			Get();
			if (la.kind == 60) {
				Associative_rangeAmountOperator(out hasRangeAmountOperator);
			}
			rnode.HasRangeAmountOperator = hasRangeAmountOperator; 
			Associative_ArithmeticExpression(out node);
			rnode.To = node; 
			NodeUtils.SetNodeEndLocation(rnode, node);
			
			if (la.kind == 24) {
				RangeStepOperator op; 
				Get();
				Associative_rangeStepOperator(out op);
				rnode.StepOperator = op; 
				Associative_ArithmeticExpression(out node);
				rnode.Step = node;
				NodeUtils.SetNodeEndLocation(rnode, node);
				
			}
			node = rnode; 
		}
	}

	void Associative_ArithmeticExpression(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Associative_Term(out node);
		while (la.kind == 15 || la.kind == 53) {
			Operator op; 
			Associative_AddOp(out op);
			ProtoCore.AST.AssociativeAST.AssociativeNode expr2; 
			Associative_Term(out expr2);
			node = GenerateBinaryOperatorMethodCallNode(op, node, expr2);
			
		}
	}

	void Associative_rangeAmountOperator(out bool hasRangeAmountOperator) {
		hasRangeAmountOperator = false; 
		Expect(60);
		hasRangeAmountOperator = true; 
	}

	void Associative_rangeStepOperator(out RangeStepOperator op) {
		op = RangeStepOperator.StepSize; 
		if (la.kind == 59 || la.kind == 60) {
			if (la.kind == 60) {
				Get();
				op = RangeStepOperator.Number; 
			} else {
				Get();
				op = RangeStepOperator.ApproximateSize; 
			}
		}
	}

	void Associative_AddOp(out Operator op) {
		op = Operator.add; 
		if (la.kind == 53) {
			Get();
		} else if (la.kind == 15) {
			Get();
			op = Operator.sub; 
		} else SynErr(88);
	}

	void Associative_MulOp(out Operator op) {
		op = Operator.mul; 
		if (la.kind == 54) {
			Get();
		} else if (la.kind == 55) {
			Get();
			op = Operator.div; 
		} else if (la.kind == 56) {
			Get();
			op = Operator.mod; 
		} else SynErr(89);
	}

	void Associative_Term(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Associative_Factor(out node);
		while (la.kind == 54 || la.kind == 55 || la.kind == 56) {
			Operator op; 
			Associative_MulOp(out op);
			ProtoCore.AST.AssociativeAST.AssociativeNode expr2; 
			Associative_Factor(out expr2);
			node = GenerateBinaryOperatorMethodCallNode(op, node, expr2);
			
		}
	}

	void Associative_Level(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		int sign = 1;
		
		if (la.kind == 1) {
			Get();
			if(t.val[0] != 'L')
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.kInvalidListLevelName, t.val));
			}
			var level = t.val.Remove(0, 1);
			Int64 value;
			if (Int64.TryParse(level, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
			   node = new ProtoCore.AST.AssociativeAST.IntNode(-1*value);
			}
			else
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.kInvalidListLevelName, t.val));
			}
			
			NodeUtils.SetNodeLocation(node, t);
			
		} else if (la.kind == 2 || la.kind == 15) {
			if (la.kind == 15) {
				Get();
				sign = -1; 
				
			}
			Expect(2);
			Int64 value;
			if (Int64.TryParse(t.val, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
			   node = new ProtoCore.AST.AssociativeAST.IntNode(sign*value);
			}
			else
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.kInvalidListLevelName, t.val));
			}
			
			NodeUtils.SetNodeLocation(node, t);
			
		} else SynErr(90);
	}

	void Associative_Number(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		int sign = 1;
		int line = ProtoCore.DSASM.Constants.kInvalidIndex; 
		int col = ProtoCore.DSASM.Constants.kInvalidIndex; 
		
		if (la.kind == 15) {
			Get();
			sign = -1; 
			line = t.line; 
			col = t.col; 
			
		}
		if (la.kind == 2) {
			Get();
			Int64 value;
			if (Int64.TryParse(t.val, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
			   node = new ProtoCore.AST.AssociativeAST.IntNode(value * sign);
			}
			else
			{
			   node = new ProtoCore.AST.AssociativeAST.NullNode();
			}
			
			if (ProtoCore.DSASM.Constants.kInvalidIndex == line
			   &&  ProtoCore.DSASM.Constants.kInvalidIndex == col)
			{
			   NodeUtils.SetNodeLocation(node, t);
			}
			else
			{
			   node.line = line; node.col = col;
			node.endLine = t.line; node.endCol = t.col;
			}
			
		} else if (la.kind == 3) {
			Get();
			double value;
			if (Double.TryParse(t.val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
			   node = new ProtoCore.AST.AssociativeAST.DoubleNode(value * sign);
			}
			else
			{
			   node = new ProtoCore.AST.AssociativeAST.NullNode();
			}
			
			if (ProtoCore.DSASM.Constants.kInvalidIndex == line
			   &&  ProtoCore.DSASM.Constants.kInvalidIndex == col)
			{
			   NodeUtils.SetNodeLocation(node, t);
			}
			else
			{
			   node.line = line; node.col = col;
			}
			
		} else SynErr(91);
	}

	void Associative_Char(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		Expect(5);
		if (t.val.Length <= 2) {
		   errors.SemErr(t.line, t.col, Resources.EmptyCharacterLiteral);
		}
		
		node = new ProtoCore.AST.AssociativeAST.CharNode() 
		{ 
		   Value = t.val.Substring(1, t.val.Length - 2),
		   line = t.line,
		   col = t.col
		}; 
		
	}

	void Associative_String(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		node = null; 
		Expect(4);
		node = new ProtoCore.AST.AssociativeAST.StringNode() 
		{ 
		   Value = GetEscapedString(t.val.Length <= 2 ? "" : t.val.Substring(1, t.val.Length - 2)),
		}; 
		NodeUtils.SetNodeLocation(node, t);
		
	}

	void Associative_ArrayExprList(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		Expect(44);
		ProtoCore.AST.AssociativeAST.ExprListNode exprlist = new ProtoCore.AST.AssociativeAST.ExprListNode(); 
		NodeUtils.SetNodeStartLocation(exprlist, t); 
		if (StartOf(4)) {
			Associative_Expression(out node);
			exprlist.Exprs.Add(node); 
			while (la.kind == 50) {
				Get();
				Associative_Expression(out node);
				exprlist.Exprs.Add(node); 
			}
		}
		Expect(45);
		NodeUtils.SetNodeEndLocation(exprlist, t); 
		node = exprlist; 
	}

	void Associative_NameReference(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		ProtoCore.AST.AssociativeAST.ArrayNameNode nameNode = null; 
		ProtoCore.AST.AssociativeAST.GroupExpressionNode groupExprNode = null;
		
		if (la.kind == 12) {
			Get();
			Associative_Expression(out node);
			if (node is ProtoCore.AST.AssociativeAST.ArrayNameNode)
			{
			   nameNode = node as ProtoCore.AST.AssociativeAST.ArrayNameNode;
			}
			else
			{
			   groupExprNode = new ProtoCore.AST.AssociativeAST.GroupExpressionNode();
			   groupExprNode.Expression = node;
			   nameNode = groupExprNode;
			}
			
			Expect(13);
		} else if (IsFunctionCall()) {
			if (isLeft)
			{
			  errors.SemErr(la.line, la.col, Resources.FunctionCallCannotBeAtLeftSide);
			} 
			
			Associative_FunctionCall(out node);
			nameNode = node as ProtoCore.AST.AssociativeAST.ArrayNameNode; 
			
		} else if (la.kind == 1) {
			Associative_Ident(out node);
			nameNode = node as ProtoCore.AST.AssociativeAST.ArrayNameNode; 
			
		} else if (la.kind == 44) {
			Associative_ArrayExprList(out node);
			nameNode = node as ProtoCore.AST.AssociativeAST.ArrayNameNode;
			
		} else SynErr(92);
		if (la.kind == 10) {
			ProtoCore.AST.AssociativeAST.ArrayNode array = new ProtoCore.AST.AssociativeAST.ArrayNode(); 
			
			Get();
			if (StartOf(4)) {
				bool tmpIsLeft = isLeft; 
				isLeft = false;
				
				Associative_Expression(out node);
				isLeft = tmpIsLeft; 
				array.Expr = node; 
				array.Type = nameNode.ArrayDimensions;
				NodeUtils.SetNodeLocation(array, t);
				nameNode.ArrayDimensions = array; 
				
				
			}
			Expect(11);
			while (la.kind == 10) {
				Get();
				if (StartOf(4)) {
					bool tmpIsLeft = isLeft; 
					isLeft = false;
					
					Associative_Expression(out node);
					isLeft = tmpIsLeft; 
					ProtoCore.AST.AssociativeAST.ArrayNode array2 = new ProtoCore.AST.AssociativeAST.ArrayNode();
					array2.Expr = node; 
					array2.Type = null;
					NodeUtils.SetNodeLocation(array2, t);
					array.Type = array2;
					array = array2;
					
				}
				Expect(11);
			}
			if (groupExprNode != null)
			{
			   var expr = groupExprNode.Expression;
			   if (expr is ProtoCore.AST.AssociativeAST.IdentifierListNode)
			   {
			       var rightNode = (expr as ProtoCore.AST.AssociativeAST.IdentifierListNode).RightNode;
			       if (rightNode is ProtoCore.AST.AssociativeAST.ArrayNameNode)
			       {
			           var rightMostArrayNameNode = rightNode as ProtoCore.AST.AssociativeAST.ArrayNameNode;
			           if (rightMostArrayNameNode.ArrayDimensions == null)
			           {
			               rightMostArrayNameNode.ArrayDimensions = groupExprNode.ArrayDimensions;
			           }
			           else 
			           {
			               rightMostArrayNameNode.ArrayDimensions.Type = groupExprNode.ArrayDimensions; 
			           }
			           groupExprNode.ArrayDimensions = null;
			       }
			   }
			   else if (expr is ProtoCore.AST.AssociativeAST.RangeExprNode)
			   {    
			       var rangeExprNode = expr as ProtoCore.AST.AssociativeAST.RangeExprNode; 
			       if (rangeExprNode.ArrayDimensions == null)
			       {
			           rangeExprNode.ArrayDimensions = groupExprNode.ArrayDimensions;
			       }
			       else 
			       {
			           rangeExprNode.ArrayDimensions.Type = groupExprNode.ArrayDimensions; 
			       }
			       groupExprNode.ArrayDimensions = null;
			   }
			   else if (expr is ProtoCore.AST.AssociativeAST.ExprListNode)
			   {    
			       var exprListNode = expr as ProtoCore.AST.AssociativeAST.ExprListNode; 
			       if (exprListNode.ArrayDimensions == null)
			       {
			           exprListNode.ArrayDimensions = groupExprNode.ArrayDimensions;
			       }
			       else 
			       {
			           exprListNode.ArrayDimensions.Type = groupExprNode.ArrayDimensions; 
			       }
			       groupExprNode.ArrayDimensions = null;
			   }
			   else if (expr is ProtoCore.AST.AssociativeAST.FunctionDotCallNode)
			   {
			       var dotCall = expr as ProtoCore.AST.AssociativeAST.FunctionDotCallNode;
			       var arrayExpr = (dotCall.Arguments[2] as ProtoCore.AST.AssociativeAST.ExprListNode);
			       var dimCount = (dotCall.Arguments[3] as ProtoCore.AST.AssociativeAST.IntNode);
			
			       var dims = dimCount.Value;
			       var newdims = dims;
			
			       if (arrayExpr != null)
			       {
			           var newarray = groupExprNode.ArrayDimensions;
			           while (newarray != null)
			           {
			               arrayExpr.Exprs.Add(newarray.Expr);
			               newdims++;
			               newarray = (newarray.Type as ProtoCore.AST.AssociativeAST.ArrayNode);
			           }
			           
			           (dotCall.Arguments[3] as ProtoCore.AST.AssociativeAST.IntNode).Value = newdims;
			       }
			       groupExprNode.ArrayDimensions = null;
			   }
			}
			
		}
		if (la.kind == 8 || la.kind == 9) {
			AssociativeNode levelNode = null;
			bool isDominant = false;
			
			if (la.kind == 8) {
				Get();
				Associative_Level(out levelNode);
			} else {
				Get();
				Associative_Level(out levelNode);
				isDominant = true; 
			}
			IntNode listAtLevel = levelNode as IntNode;
			if (listAtLevel != null)
			{
			   var atLevel = new AtLevelNode { Level = listAtLevel.Value, IsDominant = isDominant };
			   nameNode.AtLevel = atLevel; 
			}
			
		}
		if (IsReplicationGuide()) {
			var guides = new List<AssociativeNode>();
			Expect(17);
			string repguide = String.Empty;
			bool isLongest = false;
			ReplicationGuideNode repGuideNode = null;
			
			if (IsPostfixedReplicationGuide()) {
				Expect(7);
				isLongest = true; 
			} else if (la.kind == 2) {
				Get();
				isLongest = false; 
			} else SynErr(93);
			repguide = t.val;
			if (isLongest)
			{
			   repguide = repguide.Remove(repguide.Length - 1);
			}
			var numNode = new IdentifierNode() { Value = repguide };
			repGuideNode = new ReplicationGuideNode();
			repGuideNode.RepGuide = numNode;
			repGuideNode.IsLongest = isLongest;
			NodeUtils.SetNodeLocation(numNode, t); 
			
			Expect(18);
			guides.Add(repGuideNode); 
			while (la.kind == 17) {
				Get();
				if (IsPostfixedReplicationGuide()) {
					Expect(7);
					isLongest = true; 
				} else if (la.kind == 2) {
					Get();
					isLongest = false; 
				} else SynErr(94);
				repguide = t.val;
				if (isLongest)
				{
				   repguide = repguide.Remove(repguide.Length - 1);
				}
				numNode = AstFactory.BuildIdentifier(repguide);
				repGuideNode = new ReplicationGuideNode();
				repGuideNode.RepGuide = numNode;
				repGuideNode.IsLongest = isLongest;
				NodeUtils.SetNodeLocation(numNode, t); 
				
				Expect(18);
				guides.Add(repGuideNode); 
			}
			nameNode.ReplicationGuides = guides; 
			if (groupExprNode != null)
			{
			   var expr = groupExprNode.Expression;
			   if (expr is IdentifierListNode)
			   {
			       var rightNode = (expr as IdentifierListNode).RightNode;
			       if (rightNode is ArrayNameNode)
			       {
			           var rightMostArrayNameNode = rightNode as ArrayNameNode;
			           if (rightMostArrayNameNode.ReplicationGuides == null)
			           {
			               rightMostArrayNameNode.ReplicationGuides = guides;
			           }
			           else
			           {
			               rightMostArrayNameNode.ReplicationGuides.InsertRange(0, guides);
			           }
			           groupExprNode.ReplicationGuides = null;
			       }
			   }
			   else if (expr is FunctionDotCallNode)
			   {
			       var functionCall = (expr as FunctionDotCallNode).FunctionCall;
			       var function = (functionCall.Function as ArrayNameNode);
			       if (function.ReplicationGuides == null)
			       {
			           function.ReplicationGuides = guides;
			       }
			       else
			       {
			           function.ReplicationGuides.InsertRange(0, guides);
			       }
			       groupExprNode.ReplicationGuides = null;
			   }
			}
			
		}
		if (groupExprNode != null && groupExprNode.ArrayDimensions == null && (groupExprNode.ReplicationGuides == null || groupExprNode.ReplicationGuides.Count == 0))
		{
		   node = groupExprNode.Expression;
		}
		else
		{
		   node = nameNode; 
		}
		
	}

	void Associative_FunctionCall(out ProtoCore.AST.AssociativeAST.AssociativeNode node) {
		ProtoCore.AST.AssociativeAST.FunctionCallNode f = new ProtoCore.AST.AssociativeAST.FunctionCallNode(); 
		Associative_Ident(out node);
		NodeUtils.SetNodeStartLocation(f, t); 
		List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = null; 
		Associative_Arguments(out args);
		f.FormalArguments = args;
		f.Function = node;
		NodeUtils.SetNodeEndLocation(f, t);
		node = f; 
		
	}

	void Imperative_stmt(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		if (la.kind == 29) {
			Imperative_ifstmt(out node);
		} else if (la.kind == 32) {
			Imperative_whilestmt(out node);
		} else if (la.kind == 33) {
			Imperative_forloop(out node);
		} else if (la.kind == 10) {
			Imperative_languageblock(out node);
		} else if (la.kind == 37) {
			Get();
			if (la.kind != _endline)
			   SynErr(Resources.SemiColonExpected);
			
			Expect(23);
			node = new ProtoCore.AST.ImperativeAST.BreakNode(); NodeUtils.SetNodeLocation(node, t); 
		} else if (la.kind == 38) {
			Get();
			if (la.kind != _endline)
			  SynErr(Resources.SemiColonExpected);
			
			Expect(23);
			node = new ProtoCore.AST.ImperativeAST.ContinueNode(); NodeUtils.SetNodeLocation(node, t); 
		} else if (IsAssignmentStatement() || IsVariableDeclaration()) {
			Imperative_assignstmt(out node);
		} else if (StartOf(4)) {
			Imperative_expr(out node);
			if (la.kind != _endline)
			  SynErr(Resources.SemiColonExpected);
			
			Expect(23);
		} else if (la.kind == 23) {
			if (la.kind != _endline)
			  SynErr(Resources.SemiColonExpected);
			
			Get();
		} else SynErr(95);
	}

	void Imperative_languageblock(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		ProtoCore.AST.ImperativeAST.LanguageBlockNode langblock = new ProtoCore.AST.ImperativeAST.LanguageBlockNode(); 
		
		Expect(10);
		NodeUtils.SetNodeLocation(langblock, t); 
		Expect(1);
		if( 0 == t.val.CompareTo(ProtoCore.DSASM.kw.imperative)) {
		   langblock.codeblock.Language = ProtoCore.Language.Imperative;
		}
		else if( 0 == t.val.CompareTo(ProtoCore.DSASM.kw.associative)) {
		   langblock.codeblock.Language = ProtoCore.Language.Associative; 
		}
		else {
		   langblock.codeblock.Language = ProtoCore.Language.NotSpecified; 
		   errors.SemErr(t.line, t.col, String.Format(Resources.InvalidLanguageBlockIdentifier, t.val));
		}
		
		Expect(11);
		Expect(44);
		Node codeBlockNode = null; 
		if (langblock.codeblock.Language == ProtoCore.Language.Associative ||
langblock.codeblock.Language == ProtoCore.Language.NotSpecified) {
			Hydrogen(out codeBlockNode);
		} else if (langblock.codeblock.Language == ProtoCore.Language.Imperative ) {
			Imperative(out codeBlockNode);
		} else SynErr(96);
		if (langblock.codeblock.Language == ProtoCore.Language.NotSpecified) {
			int openCurlyBraceCount = 0, closeCurlyBraceCount = 0; 
			ProtoCore.AST.ImperativeAST.CodeBlockNode codeBlockInvalid = new ProtoCore.AST.ImperativeAST.CodeBlockNode(); 
			ProtoCore.AST.ImperativeAST.ImperativeNode validBlockInInvalid = null; 
			while (closeCurlyBraceCount <= openCurlyBraceCount) {
				if (la.kind == 10) {
					Imperative_languageblock(out validBlockInInvalid);
					codeBlockInvalid.Body.Add(validBlockInInvalid); 
				} else if (la.kind == 44) {
					Get();
					openCurlyBraceCount++; 
				} else if (la.kind == 45) {
					Get();
					closeCurlyBraceCount++; 
				} else if (la.kind == 0) {
					Get();
					Expect(45);
					break; 
				} else if (StartOf(8)) {
					Get(); 
				} else SynErr(97);
			}
			codeBlockNode = codeBlockInvalid; 
		} else if (la.kind == 45) {
			Get();
		} else SynErr(98);
		langblock.CodeBlockNode = codeBlockNode; 
		node = langblock; 
	}

	void Imperative_ifstmt(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		ProtoCore.AST.ImperativeAST.IfStmtNode ifStmtNode = new ProtoCore.AST.ImperativeAST.IfStmtNode(); 
		List<ProtoCore.AST.ImperativeAST.ImperativeNode> body = null; 
		Expect(29);
		NodeUtils.SetNodeLocation(ifStmtNode, t); 
		Expect(12);
		Imperative_expr(out node);
		ifStmtNode.IfExprNode = node; 
		Expect(13);
		NodeUtils.SetNodeStartLocation(ifStmtNode.IfExprNode, ifStmtNode);
		NodeUtils.SetNodeEndLocation(ifStmtNode.IfExprNode, t);
		NodeUtils.SetNodeStartLocation(ifStmtNode.IfBodyPosition, la);
		
		if (la.kind == 44) {
			Get();
			Imperative_stmtlist(out body);
			ifStmtNode.IfBody = body; 
			Expect(45);
		} else if (StartOf(10)) {
			ProtoCore.AST.ImperativeAST.ImperativeNode singleStmt; 
			Imperative_stmt(out singleStmt);
			ifStmtNode.IfBody.Add(singleStmt); 
		} else SynErr(99);
		NodeUtils.SetNodeEndLocation(ifStmtNode.IfBodyPosition, t); 
		while (la.kind == 30) {
			ProtoCore.AST.ImperativeAST.ElseIfBlock elseifBlock = new ProtoCore.AST.ImperativeAST.ElseIfBlock(); 
			Get();
			int line = t.line; int col = t.col; 
			Expect(12);
			Imperative_expr(out node);
			Expect(13);
			elseifBlock.Expr = node;
			elseifBlock.Expr.line = line;
			elseifBlock.Expr.col = col;
			NodeUtils.SetNodeEndLocation(elseifBlock.Expr, t);
			NodeUtils.SetNodeStartLocation(elseifBlock.ElseIfBodyPosition, la);
			
			if (la.kind == 44) {
				Get();
				Imperative_stmtlist(out body);
				elseifBlock.Body = body; 
				Expect(45);
			} else if (StartOf(10)) {
				ProtoCore.AST.ImperativeAST.ImperativeNode singleStmt = null; 
				Imperative_stmt(out singleStmt);
				elseifBlock.Body.Add(singleStmt); 
			} else SynErr(100);
			NodeUtils.SetNodeEndLocation(elseifBlock.ElseIfBodyPosition, t); 
			ifStmtNode.ElseIfList.Add(elseifBlock); 
		}
		if (la.kind == 31) {
			Get();
			NodeUtils.SetNodeStartLocation(ifStmtNode.ElseBodyPosition, la); 
			if (la.kind == 44) {
				Get();
				Imperative_stmtlist(out body);
				ifStmtNode.ElseBody = body; 
				Expect(45);
			} else if (StartOf(10)) {
				ProtoCore.AST.ImperativeAST.ImperativeNode singleStmt = null; 
				Imperative_stmt(out singleStmt);
				ifStmtNode.ElseBody.Add(singleStmt); 
			} else SynErr(101);
			NodeUtils.SetNodeEndLocation(ifStmtNode.ElseBodyPosition, t); 
		}
		node = ifStmtNode; 
	}

	void Imperative_whilestmt(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		ProtoCore.AST.ImperativeAST.WhileStmtNode whileStmtNode = new ProtoCore.AST.ImperativeAST.WhileStmtNode(); 
		List<ProtoCore.AST.ImperativeAST.ImperativeNode> body = null; 
		Expect(32);
		NodeUtils.SetNodeStartLocation(whileStmtNode, t); 
		Expect(12);
		Imperative_expr(out node);
		Expect(13);
		whileStmtNode.Expr = node; 
		NodeUtils.SetNodeStartLocation(whileStmtNode.Expr, whileStmtNode);
		NodeUtils.SetNodeEndLocation(whileStmtNode.Expr, t);
		
		Expect(44);
		Imperative_stmtlist(out body);
		whileStmtNode.Body = body; 
		Expect(45);
		NodeUtils.SetNodeEndLocation(whileStmtNode, t);
		node = whileStmtNode;
		
	}

	void Imperative_forloop(out ProtoCore.AST.ImperativeAST.ImperativeNode forloop) {
		ProtoCore.AST.ImperativeAST.ImperativeNode node;
		ProtoCore.AST.ImperativeAST.ForLoopNode loopNode = new ProtoCore.AST.ImperativeAST.ForLoopNode();
		List<ProtoCore.AST.ImperativeAST.ImperativeNode> body = null;   
		
		Expect(33);
		NodeUtils.SetNodeLocation(loopNode, t); loopNode.KwForLine = t.line; loopNode.KwForCol = t.col; 
		Expect(12);
		int idLine = la.line; int idCol = la.col; 
		Imperative_Ident(out node);
		loopNode.LoopVariable = node; loopNode.LoopVariable.line = idLine; loopNode.LoopVariable.col = idCol; 
		Expect(61);
		loopNode.KwInLine = t.line; loopNode.KwInCol = t.col; int exprLine = la.line; int exprCol = la.col; 
		Imperative_expr(out node);
		loopNode.Expression = node; if (loopNode.Expression != null) {  loopNode.Expression.line = exprLine; loopNode.Expression.col = exprCol; } 
		Expect(13);
		if (la.kind == 44) {
			Get();
			Imperative_stmtlist(out body);
			loopNode.Body = body; 
			Expect(45);
			NodeUtils.SetNodeEndLocation(loopNode, t); 
		} else if (StartOf(10)) {
			ProtoCore.AST.ImperativeAST.ImperativeNode singleStmt = null; 
			Imperative_stmt(out singleStmt);
			loopNode.Body.Add(singleStmt); 
		} else SynErr(102);
		forloop = loopNode;
		
	}

	void Imperative_assignstmt(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		ProtoCore.AST.ImperativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode();
		ProtoCore.AST.ImperativeAST.ImperativeNode lhsNode = null; 
		NodeUtils.SetNodeLocation(bNode, la);
		
		Imperative_decoratedIdentifier(out lhsNode);
		node = lhsNode; 
		if (la.kind == 23) {
			Get();
			bNode.LeftNode = lhsNode;
			bNode.RightNode = null;
			bNode.Optr = Operator.assign;
			NodeUtils.SetNodeEndLocation(bNode, t);
			node = bNode; 
			
		} else if (la.kind == 51) {
			Get();
			ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode = null; 
			if (HasMoreAssignmentStatements()) {
				Imperative_assignstmt(out rhsNode);
			} else if (StartOf(4)) {
				Imperative_expr(out rhsNode);
				if (la.kind != _endline)
				  SynErr(Resources.SemiColonExpected);
				
				Expect(23);
			} else if (la.kind == 10) {
				Imperative_languageblock(out rhsNode);
			} else SynErr(103);
			bNode.LeftNode = lhsNode;
			bNode.RightNode = rhsNode;
			bNode.Optr = Operator.assign;
			NodeUtils.SetNodeEndLocation(bNode, t);
			node = bNode;       
			
		} else if (StartOf(8)) {
			SynErr("';' is expected"); 
		} else SynErr(104);
	}

	void Imperative_expr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		Imperative_binexpr(out node);
		while (la.kind == 52) {
			Imperative_TernaryOp(ref node);
		}
	}

	void Imperative_stmtlist(out List<ProtoCore.AST.ImperativeAST.ImperativeNode> nodelist) {
		nodelist = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>(); 
		while (StartOf(10)) {
			ProtoCore.AST.ImperativeAST.ImperativeNode node = null; 
			Imperative_stmt(out node);
			if (node != null) nodelist.Add(node); 
		}
	}

	void Imperative_decoratedIdentifier(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		if (IsLocallyTypedVariable()) {
			Expect(1);
			if (IsKeyWord(t.val, true))
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
			}
			var typedVar = new ProtoCore.AST.ImperativeAST.TypedIdentifierNode();
			typedVar.Name = typedVar.Value = t.val;
			NodeUtils.SetNodeLocation(typedVar, t);
			
			Expect(46);
			Expect(40);
			typedVar.IsLocal = true;
			
			Expect(1);
			int type = core.TypeSystem.GetType(t.val); 
			if (type == ProtoCore.DSASM.Constants.kInvalidIndex)
			{
			   var unknownType = new ProtoCore.Type();
			   unknownType.UID = ProtoCore.DSASM.Constants.kInvalidIndex;
			   unknownType.Name = t.val; 
			   typedVar.DataType = unknownType;
			}
			else
			{
			   typedVar.DataType = core.TypeSystem.BuildTypeObject(type, 0);
			}
			
			if (la.kind == 10) {
				var datatype = typedVar.DataType; 
				Get();
				Expect(11);
				datatype.rank = 1; 
				if (la.kind == 10 || la.kind == 24) {
					if (la.kind == 24) {
						Get();
						Expect(10);
						Expect(11);
						datatype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 10) {
							Get();
							Expect(11);
							datatype.rank++; 
						}
					}
				}
				typedVar.DataType = datatype; 
			}
			node = typedVar; 
		} else if (IsLocalVariable()) {
			Expect(1);
			if (IsKeyWord(t.val, true))
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
			}
			var identNode = new ProtoCore.AST.ImperativeAST.IdentifierNode();
			identNode.Name = identNode.Value = t.val;
			NodeUtils.SetNodeLocation(identNode, t);
			
			Expect(46);
			Expect(40);
			identNode.IsLocal = true;
			
			node = identNode; 
		} else if (IsTypedVariable()) {
			Expect(1);
			if (IsKeyWord(t.val, true))
			{
			   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
			}
			var typedVar = new ProtoCore.AST.ImperativeAST.TypedIdentifierNode();
			typedVar.Name = typedVar.Value = t.val;
			NodeUtils.SetNodeLocation(typedVar, t);
			
			Expect(46);
			Expect(1);
			int type = core.TypeSystem.GetType(t.val); 
			if (type == ProtoCore.DSASM.Constants.kInvalidIndex)
			{
			   var unknownType = new ProtoCore.Type();
			   unknownType.UID = ProtoCore.DSASM.Constants.kInvalidIndex;
			   unknownType.Name = t.val; 
			   typedVar.DataType = unknownType;
			}
			else
			{
			   typedVar.DataType = core.TypeSystem.BuildTypeObject(type, 0);
			}
			
			if (la.kind == 10) {
				var datatype = typedVar.DataType; 
				Get();
				Expect(11);
				datatype.rank = 1; 
				if (la.kind == 10 || la.kind == 24) {
					if (la.kind == 24) {
						Get();
						Expect(10);
						Expect(11);
						datatype.rank = ProtoCore.DSASM.Constants.nDimensionArrayRank; 
					} else {
						while (la.kind == 10) {
							Get();
							Expect(11);
							datatype.rank++; 
						}
					}
				}
				typedVar.DataType = datatype; 
			}
			node = typedVar; 
		} else if (la.kind == 1 || la.kind == 12 || la.kind == 44) {
			Imperative_IdentifierList(out node);
		} else SynErr(105);
	}

	void Imperative_IdentifierList(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		if (isInClass && IsIdentList())
		disableKwCheck = true;
		
		Imperative_NameReference(out node);
		disableKwCheck = false; 
		while (la.kind == 6) {
			Get();
			ProtoCore.AST.ImperativeAST.ImperativeNode rnode = null; 
			Imperative_NameReference(out rnode);
			ProtoCore.AST.ImperativeAST.IdentifierListNode bnode = new ProtoCore.AST.ImperativeAST.IdentifierListNode(); 
			bnode.LeftNode = node; 
			bnode.Optr = Operator.dot; 
			bnode.RightNode = rnode; 
			NodeUtils.SetNodeLocation(bnode, bnode.LeftNode, bnode.RightNode);
			if (bnode.RightNode is ProtoCore.AST.ImperativeAST.FunctionCallNode)
			{
			   // We want the entire "Point.Project()" to be highlighted, 
			   // not just "Project()". So if the RHS is a function call node,
			   // then the identifier list should be extended to include both 
			   // LeftNode and RightNode (which is the entire 'bnode' here).
			   NodeUtils.CopyNodeLocation(bnode.RightNode, bnode);
			}
			node = bnode; 
			
		}
	}

	void Imperative_Ident(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		Expect(1);
		if (!disableKwCheck && IsKeyWord(t.val, false, false))
		{
		   errors.SemErr(t.line, t.col, String.Format(Resources.keywordCantBeUsedAsIdentifier, t.val));
		}
		int ltype = (0 == String.Compare(t.val, "return")) ? (int)ProtoCore.PrimitiveType.Return : (int)ProtoCore.PrimitiveType.Var;
		if (ltype == (int)ProtoCore.PrimitiveType.Return && la.val != "=")
		{
		   SynErr(String.Format(Resources.InvalidReturnStatement, la.val)); 
		}        
		node = BuildImperativeIdentifier(t.val, (ProtoCore.PrimitiveType)ltype);
		NodeUtils.SetNodeLocation(node, t);
		
	}

	void Imperative_binexpr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null;
		Imperative_logicalexpr(out node);
		while (la.kind == 57 || la.kind == 58) {
			Operator op; 
			Imperative_logicalop(out op);
			ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode = null; 
			Imperative_logicalexpr(out rhsNode);
			ProtoCore.AST.ImperativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			NodeUtils.CopyNodeLocation(bNode, node);
			node = bNode;
			
		}
	}

	void Imperative_TernaryOp(ref ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		ProtoCore.AST.ImperativeAST.InlineConditionalNode inlineConNode = new ProtoCore.AST.ImperativeAST.InlineConditionalNode(); 
		Expect(52);
		inlineConNode.ConditionExpression = node; node = null; 
		Imperative_expr(out node);
		inlineConNode.TrueExpression = node; 
		Expect(46);
		node = null; 
		Imperative_expr(out node);
		inlineConNode.FalseExpression = node; 
		node = inlineConNode; 
	}

	void Imperative_NameReference(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		ProtoCore.AST.ImperativeAST.ArrayNameNode nameNode = null;
		ProtoCore.AST.ImperativeAST.GroupExpressionNode groupExprNode = null;
		
		if (la.kind == 12) {
			Get();
			Imperative_expr(out node);
			Expect(13);
			if (node is ProtoCore.AST.ImperativeAST.ArrayNameNode)
			{
			   nameNode = node as ProtoCore.AST.ImperativeAST.ArrayNameNode;
			}
			else
			{
			   groupExprNode = new ProtoCore.AST.ImperativeAST.GroupExpressionNode();
			   groupExprNode.Expression = node;
			   nameNode = groupExprNode; 
			}
			
		} else if (IsFunctionCall()) {
			Imperative_functioncall(out node);
			nameNode = node as ProtoCore.AST.ImperativeAST.ArrayNameNode;
			
		} else if (la.kind == 1) {
			Imperative_Ident(out node);
			nameNode = node as ProtoCore.AST.ImperativeAST.ArrayNameNode;
			
		} else if (la.kind == 44) {
			Imperative_ExprList(out node);
			nameNode = node as ProtoCore.AST.ImperativeAST.ArrayNameNode;
			
		} else SynErr(106);
		if (la.kind == 10) {
			ProtoCore.AST.ImperativeAST.ArrayNode array = new ProtoCore.AST.ImperativeAST.ArrayNode();
			
			Get();
			if (StartOf(4)) {
				Imperative_expr(out node);
				array.Expr = node; 
				array.Type = nameNode.ArrayDimensions;
				NodeUtils.SetNodeLocation(array, t);
				nameNode.ArrayDimensions = array; 
				
			}
			Expect(11);
			while (la.kind == 10) {
				Get();
				if (StartOf(4)) {
					Imperative_expr(out node);
					ProtoCore.AST.ImperativeAST.ArrayNode array2 = new ProtoCore.AST.ImperativeAST.ArrayNode();
					array2.Expr = node; 
					array2.Type = null;
					NodeUtils.SetNodeLocation(array2, t);
					array.Type = array2;
					array = array2;
					
				}
				Expect(11);
			}
			if (groupExprNode != null)
			{
			   var expr = groupExprNode.Expression;
			   if (expr is ProtoCore.AST.ImperativeAST.RangeExprNode)
			   {    
			       var rangeExprNode = expr as ProtoCore.AST.ImperativeAST.RangeExprNode; 
			       if (rangeExprNode.ArrayDimensions == null)
			       {
			           rangeExprNode.ArrayDimensions = groupExprNode.ArrayDimensions;
			       }
			       else 
			       {
			           rangeExprNode.ArrayDimensions.Type = groupExprNode.ArrayDimensions; 
			       }
			       groupExprNode.ArrayDimensions = null;
			   }
			   else if (expr is ProtoCore.AST.ImperativeAST.ExprListNode)
			   {    
			       var exprListNode = expr as ProtoCore.AST.ImperativeAST.ExprListNode; 
			       if (exprListNode.ArrayDimensions == null)
			       {
			           exprListNode.ArrayDimensions = groupExprNode.ArrayDimensions;
			       }
			       else 
			       {
			           exprListNode.ArrayDimensions.Type = groupExprNode.ArrayDimensions; 
			       }
			       groupExprNode.ArrayDimensions = null;
			   }
			}
			
		}
		if (groupExprNode != null && groupExprNode.ArrayDimensions == null)
		{
		   node = groupExprNode.Expression;
		}
		else
		{
		   node = nameNode;
		}
		
	}

	void Imperative_unaryexpr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		if (la.kind == 15) {
			Imperative_negexpr(out node);
		} else if (la.kind == 14 || la.kind == 59) {
			Imperative_bitunaryexpr(out node);
		} else SynErr(107);
	}

	void Imperative_negexpr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		UnaryOperator op; 
		ProtoCore.AST.ImperativeAST.ImperativeNode exprNode = null; 
		Imperative_negop(out op);
		Imperative_IdentifierList(out exprNode);
		ProtoCore.AST.ImperativeAST.UnaryExpressionNode unary = new ProtoCore.AST.ImperativeAST.UnaryExpressionNode(); 
		unary.Operator = op;
		unary.Expression = exprNode;
		NodeUtils.SetNodeLocation(unary, t);
		node = unary;
		
	}

	void Imperative_bitunaryexpr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		UnaryOperator op; 
		ProtoCore.AST.ImperativeAST.ImperativeNode exprNode; 
		Imperative_unaryop(out op);
		
		Imperative_factor(out exprNode);
		ProtoCore.AST.ImperativeAST.UnaryExpressionNode unary = new ProtoCore.AST.ImperativeAST.UnaryExpressionNode(); 
		unary.Operator = op;                            
		unary.Expression = exprNode;
		NodeUtils.SetNodeLocation(unary, t);
		node = unary;
		
	}

	void Imperative_unaryop(out UnaryOperator op) {
		op = UnaryOperator.None; 
		if (la.kind == 14) {
			Get();
			op = UnaryOperator.Not; 
			#if ENABLE_BIT_OP       
		} else if (la.kind == 59) {
			Get();
			op = UnaryOperator.Negate; 
			#endif                     
		} else SynErr(108);
	}

	void Imperative_factor(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		if (IsNumber()) {
			Imperative_num(out node);
		} else if (la.kind == 5) {
			Imperative_Char(out node);
		} else if (la.kind == 4) {
			Imperative_String(out node);
		} else if (la.kind == 41) {
			Get();
			node = new ProtoCore.AST.ImperativeAST.BooleanNode(true);
			NodeUtils.SetNodeLocation(node, t); 
			
		} else if (la.kind == 42) {
			Get();
			node = new ProtoCore.AST.ImperativeAST.BooleanNode(false); 
			NodeUtils.SetNodeLocation(node, t); 
			
		} else if (la.kind == 43) {
			Get();
			node = new ProtoCore.AST.ImperativeAST.NullNode(); 
			NodeUtils.SetNodeLocation(node, t); 
			
		} else if (la.kind == 1 || la.kind == 12 || la.kind == 44) {
			Imperative_IdentifierList(out node);
		} else if (la.kind == 14 || la.kind == 15 || la.kind == 59) {
			Imperative_unaryexpr(out node);
		} else SynErr(109);
	}

	void Imperative_negop(out UnaryOperator op) {
		Expect(15);
		op = UnaryOperator.Neg; 
	}

	void Imperative_logicalexpr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null;
		Imperative_RangeExpr(out node);
		while (StartOf(15)) {
			Operator op; 
			Imperative_relop(out op);
			ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode = null; 
			Imperative_RangeExpr(out rhsNode);
			ProtoCore.AST.ImperativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			NodeUtils.SetNodeLocation(bNode, bNode.LeftNode, bNode.RightNode);
			node = bNode;
			
		}
	}

	void Imperative_logicalop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 57) {
			Get();
			op = Operator.and; 
		} else if (la.kind == 58) {
			Get();
			op = Operator.or; 
		} else SynErr(110);
	}

	void Imperative_RangeExpr(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		Imperative_rel(out node);
		if (la.kind == 24) {
			ProtoCore.AST.ImperativeAST.RangeExprNode rnode = new ProtoCore.AST.ImperativeAST.RangeExprNode(); 
			rnode.From = node;
			NodeUtils.SetNodeStartLocation(rnode, rnode.From);
			bool hasAmountOperator = false;
			
			Get();
			if (la.kind == 60) {
				Imperative_rangeAmountOperator(out hasAmountOperator);
			}
			rnode.HasRangeAmountOperator = hasAmountOperator; 
			Imperative_rel(out node);
			rnode.To = node;
			NodeUtils.SetNodeEndLocation(rnode, rnode.To);
			
			if (la.kind == 24) {
				RangeStepOperator op; 
				Get();
				Imperative_rangeStepOperator(out op);
				rnode.StepOperator = op; 
				Imperative_rel(out node);
				rnode.Step = node;
				NodeUtils.SetNodeEndLocation(rnode, rnode.Step);
				
			}
			node = rnode; 
		}
	}

	void Imperative_relop(out Operator op) {
		op = Operator.none; 
		switch (la.kind) {
		case 18: {
			Get();
			op = Operator.gt; 
			break;
		}
		case 17: {
			Get();
			op = Operator.lt; 
			break;
		}
		case 20: {
			Get();
			op = Operator.ge; 
			break;
		}
		case 19: {
			Get();
			op = Operator.le; 
			break;
		}
		case 21: {
			Get();
			op = Operator.eq; 
			break;
		}
		case 22: {
			Get();
			op = Operator.nq; 
			break;
		}
		default: SynErr(111); break;
		}
	}

	void Imperative_rel(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null;
		Imperative_term(out node);
		while (la.kind == 15 || la.kind == 53) {
			Operator op; 
			Imperative_addop(out op);
			ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode; 
			Imperative_term(out rhsNode);
			ProtoCore.AST.ImperativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			NodeUtils.SetNodeLocation(bNode, bNode.LeftNode, bNode.RightNode);
			node = bNode;
			
		}
	}

	void Imperative_rangeAmountOperator(out bool hasAmountOperator) {
		hasAmountOperator = false; 
		Expect(60);
		hasAmountOperator = true; 
	}

	void Imperative_rangeStepOperator(out RangeStepOperator op) {
		op = RangeStepOperator.StepSize; 
		if (la.kind == 59 || la.kind == 60) {
			if (la.kind == 60) {
				Get();
				op = RangeStepOperator.Number; 
			} else {
				Get();
				op = RangeStepOperator.ApproximateSize; 
			}
		}
	}

	void Imperative_term(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null;
		#if ENABLE_BIT_OP 
		Imperative_interimfactor(out node);
		#else             
		Imperative_factor(out node);
		#endif            
		while (la.kind == 54 || la.kind == 55 || la.kind == 56) {
			Operator op; 
			Imperative_mulop(out op);
			ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode; 
			#if ENABLE_BIT_OP 
			Imperative_interimfactor(out rhsNode);
			#else             
			Imperative_factor(out rhsNode);
			#endif            
			ProtoCore.AST.ImperativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			NodeUtils.CopyNodeLocation(bNode, node);
			node = bNode;
			
		}
	}

	void Imperative_addop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 53) {
			Get();
			op = Operator.add; 
		} else if (la.kind == 15) {
			Get();
			op = Operator.sub; 
		} else SynErr(112);
	}

	void Imperative_interimfactor(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null;
		Imperative_factor(out node);
		while (la.kind == 16 || la.kind == 62 || la.kind == 63) {
			Operator op; 
			Imperative_bitop(out op);
			ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode; 
			Imperative_factor(out rhsNode);
			ProtoCore.AST.ImperativeAST.BinaryExpressionNode bNode = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode();
			bNode.LeftNode = node;
			bNode.RightNode = rhsNode;
			bNode.Optr = op;
			NodeUtils.CopyNodeLocation(bNode, node);
			node = bNode;
			
		}
	}

	void Imperative_mulop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 54) {
			Get();
			op = Operator.mul; 
		} else if (la.kind == 55) {
			Get();
			op = Operator.div; 
		} else if (la.kind == 56) {
			Get();
			op = Operator.mod; 
		} else SynErr(113);
	}

	void Imperative_bitop(out Operator op) {
		op = Operator.none; 
		if (la.kind == 62) {
			Get();
			op = Operator.bitwiseand; 
		} else if (la.kind == 16) {
			Get();
			op = Operator.bitwiseor; 
		} else if (la.kind == 63) {
			Get();
			op = Operator.bitwisexor; 
		} else SynErr(114);
	}

	void Imperative_Char(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		Expect(5);
		if (t.val.Length <= 2) {
		   errors.SemErr(t.line, t.col, Resources.EmptyCharacterLiteral);
		}
		
		node = new ProtoCore.AST.ImperativeAST.CharNode() 
		{ 
		   Value = t.val.Substring(1, t.val.Length - 2),
		   line = t.line,
		   col = t.col
		}; 
		
	}

	void Imperative_String(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		Expect(4);
		node = new ProtoCore.AST.ImperativeAST.StringNode() 
		{ 
		   Value = GetEscapedString(t.val.Length <= 2 ? "" : t.val.Substring(1, t.val.Length - 2)), 
		   line = t.line,
		   col = t.col
		}; 
		
	}

	void Imperative_num(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		node = null; 
		int sign = 1;
		int line = ProtoCore.DSASM.Constants.kInvalidIndex; int col = ProtoCore.DSASM.Constants.kInvalidIndex;
		
		if (la.kind == 15) {
			Get();
			sign = -1;
			line = t.line; 
			col = t.col; 
			
		}
		if (la.kind == 2) {
			Get();
			Int64 value;
			if (Int64.TryParse(t.val, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
			   node = new ProtoCore.AST.ImperativeAST.IntNode(value * sign);
			}
			else
			{
			   node = new ProtoCore.AST.ImperativeAST.NullNode();
			}
			
			if (ProtoCore.DSASM.Constants.kInvalidIndex != line)
			{
			   node.line = line; node.col = col; 
			}
			else
			{
			   NodeUtils.SetNodeLocation(node, t); 
			}
			
		} else if (la.kind == 3) {
			Get();
			double value;
			if (Double.TryParse(t.val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
			   node = new ProtoCore.AST.ImperativeAST.DoubleNode(value * sign);
			}
			else
			{
			   node = new ProtoCore.AST.ImperativeAST.NullNode();
			}
			
			if (ProtoCore.DSASM.Constants.kInvalidIndex != line){
			   node.line = line; node.col = col; }
			else{
			   NodeUtils.SetNodeLocation(node, t); }
			
		} else SynErr(115);
	}

	void Imperative_functioncall(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		Expect(1);
		ProtoCore.AST.ImperativeAST.IdentifierNode function = new ProtoCore.AST.ImperativeAST.IdentifierNode() { Value = t.val, Name = t.val }; 
		NodeUtils.SetNodeLocation(function, t); 
		List<ProtoCore.AST.ImperativeAST.ImperativeNode> arglist = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>(); 
		Expect(12);
		if (StartOf(4)) {
			ProtoCore.AST.ImperativeAST.ImperativeNode argNode; 
			Imperative_expr(out argNode);
			arglist.Add(argNode); 
			while (la.kind == 50) {
				Get();
				Imperative_expr(out argNode);
				arglist.Add(argNode); 
			}
		}
		Expect(13);
		ProtoCore.AST.ImperativeAST.FunctionCallNode funcNode = new ProtoCore.AST.ImperativeAST.FunctionCallNode(); 
		funcNode.Function = function;
		funcNode.FormalArguments = arglist;
		NodeUtils.SetNodeStartLocation(funcNode, function);
		NodeUtils.SetNodeEndLocation(funcNode, t);
		node = funcNode; 
		
	}

	void Imperative_ExprList(out ProtoCore.AST.ImperativeAST.ImperativeNode node) {
		Expect(44);
		ProtoCore.AST.ImperativeAST.ExprListNode exprlist = new ProtoCore.AST.ImperativeAST.ExprListNode();
		NodeUtils.SetNodeStartLocation(exprlist, t);
		
		if (StartOf(4)) {
			Imperative_expr(out node);
			exprlist.Exprs.Add(node); 
			while (la.kind == 50) {
				Get();
				Imperative_expr(out node);
				exprlist.Exprs.Add(node); 
			}
		}
		Expect(45);
		NodeUtils.SetNodeEndLocation(exprlist, t);
		node = exprlist;
		
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		DesignScriptParser();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{T,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{T,T,T,T, T,T,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{T,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,T,x,T, x,T,T,T, T,T,x,x, x,T,T,x, x,T,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, T,T,x,x, x,T,T,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{T,T,T,T, T,T,x,x, x,x,T,x, T,x,T,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,x,x, x,x,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

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
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "float expected"; break;
			case 4: s = "textstring expected"; break;
			case 5: s = "char expected"; break;
			case 6: s = "period expected"; break;
			case 7: s = "postfixed_replicationguide expected"; break;
			case 8: s = "list_at expected"; break;
			case 9: s = "dominant_list_at expected"; break;
			case 10: s = "openbracket expected"; break;
			case 11: s = "closebracket expected"; break;
			case 12: s = "openparen expected"; break;
			case 13: s = "closeparen expected"; break;
			case 14: s = "not expected"; break;
			case 15: s = "neg expected"; break;
			case 16: s = "pipe expected"; break;
			case 17: s = "lessthan expected"; break;
			case 18: s = "greaterthan expected"; break;
			case 19: s = "lessequal expected"; break;
			case 20: s = "greaterequal expected"; break;
			case 21: s = "equal expected"; break;
			case 22: s = "notequal expected"; break;
			case 23: s = "endline expected"; break;
			case 24: s = "rangeop expected"; break;
			case 25: s = "kw_class expected"; break;
			case 26: s = "kw_constructor expected"; break;
			case 27: s = "kw_def expected"; break;
			case 28: s = "kw_extend expected"; break;
			case 29: s = "kw_if expected"; break;
			case 30: s = "kw_elseif expected"; break;
			case 31: s = "kw_else expected"; break;
			case 32: s = "kw_while expected"; break;
			case 33: s = "kw_for expected"; break;
			case 34: s = "kw_import expected"; break;
			case 35: s = "kw_prefix expected"; break;
			case 36: s = "kw_from expected"; break;
			case 37: s = "kw_break expected"; break;
			case 38: s = "kw_continue expected"; break;
			case 39: s = "kw_static expected"; break;
			case 40: s = "kw_local expected"; break;
			case 41: s = "literal_true expected"; break;
			case 42: s = "literal_false expected"; break;
			case 43: s = "literal_null expected"; break;
			case 44: s = "\"{\" expected"; break;
			case 45: s = "\"}\" expected"; break;
			case 46: s = "\":\" expected"; break;
			case 47: s = "\"public\" expected"; break;
			case 48: s = "\"private\" expected"; break;
			case 49: s = "\"protected\" expected"; break;
			case 50: s = "\",\" expected"; break;
			case 51: s = "\"=\" expected"; break;
			case 52: s = "\"?\" expected"; break;
			case 53: s = "\"+\" expected"; break;
			case 54: s = "\"*\" expected"; break;
			case 55: s = "\"/\" expected"; break;
			case 56: s = "\"%\" expected"; break;
			case 57: s = "\"&&\" expected"; break;
			case 58: s = "\"||\" expected"; break;
			case 59: s = "\"~\" expected"; break;
			case 60: s = "\"#\" expected"; break;
			case 61: s = "\"in\" expected"; break;
			case 62: s = "\"&\" expected"; break;
			case 63: s = "\"^\" expected"; break;
			case 64: s = "??? expected"; break;
			case 65: s = "this symbol not expected in Import_Statement"; break;
			case 66: s = "invalid Import_Statement"; break;
			case 67: s = "this symbol not expected in Associative_Statement"; break;
			case 68: s = "invalid Associative_Statement"; break;
			case 69: s = "invalid Associative_classdecl"; break;
			case 70: s = "invalid Associative_classdecl"; break;
			case 71: s = "this symbol not expected in Associative_NonAssignmentStatement"; break;
			case 72: s = "this symbol not expected in Associative_FunctionCallStatement"; break;
			case 73: s = "this symbol not expected in Associative_FunctionalStatement"; break;
			case 74: s = "invalid Associative_FunctionalStatement"; break;
			case 75: s = "invalid Associative_FunctionalStatement"; break;
			case 76: s = "invalid Associative_LanguageBlock"; break;
			case 77: s = "invalid Associative_LanguageBlock"; break;
			case 78: s = "invalid Associative_LanguageBlock"; break;
			case 79: s = "invalid Associative_AccessSpecifier"; break;
			case 80: s = "invalid Associative_DecoratedIdentifier"; break;
			case 81: s = "invalid Associative_DecoratedIdentifier"; break;
			case 82: s = "invalid Associative_UnaryExpression"; break;
			case 83: s = "invalid Associative_unaryop"; break;
			case 84: s = "invalid Associative_Factor"; break;
			case 85: s = "invalid Associative_negop"; break;
			case 86: s = "invalid Associative_LogicalOp"; break;
			case 87: s = "invalid Associative_ComparisonOp"; break;
			case 88: s = "invalid Associative_AddOp"; break;
			case 89: s = "invalid Associative_MulOp"; break;
			case 90: s = "invalid Associative_Level"; break;
			case 91: s = "invalid Associative_Number"; break;
			case 92: s = "invalid Associative_NameReference"; break;
			case 93: s = "invalid Associative_NameReference"; break;
			case 94: s = "invalid Associative_NameReference"; break;
			case 95: s = "invalid Imperative_stmt"; break;
			case 96: s = "invalid Imperative_languageblock"; break;
			case 97: s = "invalid Imperative_languageblock"; break;
			case 98: s = "invalid Imperative_languageblock"; break;
			case 99: s = "invalid Imperative_ifstmt"; break;
			case 100: s = "invalid Imperative_ifstmt"; break;
			case 101: s = "invalid Imperative_ifstmt"; break;
			case 102: s = "invalid Imperative_forloop"; break;
			case 103: s = "invalid Imperative_assignstmt"; break;
			case 104: s = "invalid Imperative_assignstmt"; break;
			case 105: s = "invalid Imperative_decoratedIdentifier"; break;
			case 106: s = "invalid Imperative_NameReference"; break;
			case 107: s = "invalid Imperative_unaryexpr"; break;
			case 108: s = "invalid Imperative_unaryop"; break;
			case 109: s = "invalid Imperative_factor"; break;
			case 110: s = "invalid Imperative_logicalop"; break;
			case 111: s = "invalid Imperative_relop"; break;
			case 112: s = "invalid Imperative_addop"; break;
			case 113: s = "invalid Imperative_mulop"; break;
			case 114: s = "invalid Imperative_bitop"; break;
			case 115: s = "invalid Imperative_num"; break;

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
		core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.Parsing, s, core.CurrentDSFileName, line, col);
		//warningStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		// TODO: Jun/Jiong expand parser warnings.
		core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.Parsing, s, core.CurrentDSFileName);
		//warningStream.WriteLine(String.Format("Warning: {0}",s));
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}