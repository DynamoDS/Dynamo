using ProtoCore.DSASM;
using System.Collections.Generic;

namespace ProtoCore.Utils
{
    public static class CoreUtils
    {
        public static void InsertPredefinedAndBuiltinMethods(Core core, ProtoCore.AST.Node root, bool builtinMethodsLoaded)
        {
            if (DSASM.InterpreterMode.kNormal == core.ExecMode)
            {
                if (!builtinMethodsLoaded)
                {
                    InsertDotMethod(core, root);
                }
                if (core.Options.AssocOperatorAsMethod)
                {
                    ProtoCore.Utils.CoreUtils.InsertPredefinedMethod(core, root, builtinMethodsLoaded);
                }
                ProtoCore.Utils.CoreUtils.InsertBuiltInMethods(core, root, builtinMethodsLoaded);
            }
        }

        
    public static ProtoCore.AST.AssociativeAST.IdentifierNode BuildAssocIdentifier(Core core, string name, ProtoCore.PrimitiveType type = ProtoCore.PrimitiveType.kTypeVar)
    {
        var ident = new ProtoCore.AST.AssociativeAST.IdentifierNode();
        ident.Name = ident.Value = name;
        ident.datatype = TypeSystem.BuildPrimitiveTypeObject(type, false);
        return ident;
    }

    private static void InsertDotMethod(Core core, ProtoCore.AST.Node root)
    {
        InsertDotMemFuncMethod(core, root);
    }

    private static ProtoCore.AST.AssociativeAST.FunctionDefinitionNode GenerateBuiltInMethodSignatureNode(ProtoCore.Lang.BuiltInMethods.BuiltInMethod method)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode fDef = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        fDef.Name = ProtoCore.Lang.BuiltInMethods.GetMethodName(method.ID);
        fDef.ReturnType = method.ReturnType;
        fDef.IsExternLib = true;
        fDef.IsBuiltIn = true;
        fDef.BuiltInMethodId = method.ID;
        fDef.Signature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();

        foreach (KeyValuePair<string, ProtoCore.Type> param in method.Parameters)
        {
            ProtoCore.AST.AssociativeAST.VarDeclNode arg = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            arg.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode { Name = param.Key, Value = param.Key };
            arg.ArgumentType = param.Value;
            fDef.Signature.AddArgument(arg);
        }

        return fDef;
    }
        
	private static void InsertBuiltInMethods(Core core, ProtoCore.AST.Node root, bool builtinMethodsLoaded)
    {
        if (!builtinMethodsLoaded)
        {
            ProtoCore.Lang.BuiltInMethods builtInMethods = new Lang.BuiltInMethods(core);
            foreach (ProtoCore.Lang.BuiltInMethods.BuiltInMethod method in builtInMethods.Methods)
			{
				(root as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(GenerateBuiltInMethodSignatureNode(method));
			}
		}
    }

    private static void InsertDotMemVarMethod(Core core, ProtoCore.AST.Node root)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        funcDefNode.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
        funcDefNode.Name = ProtoCore.DSASM.Constants.kDotArgMethodName;
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeVar), UID = (int)PrimitiveType.kTypeVar };
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kLHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeVar), UID = (int)PrimitiveType.kTypeVar }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kRHS), 
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeInt), UID = (int)PrimitiveType.kTypeInt }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%rhsDimExprList"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeInt), UID = (int)PrimitiveType.kTypeInt, IsIndexable = true, rank = 1 }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%rhsDim"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeInt), UID = (int)PrimitiveType.kTypeInt }
        });
        funcDefNode.Signature = args;

        ProtoCore.AST.AssociativeAST.CodeBlockNode body = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
        ProtoCore.AST.AssociativeAST.IdentifierNode _return = BuildAssocIdentifier(core, ProtoCore.DSDefinitions.Keyword.Return, ProtoCore.PrimitiveType.kTypeReturn);

        ProtoCore.AST.AssociativeAST.DotFunctionBodyNode dotNode = new ProtoCore.AST.AssociativeAST.DotFunctionBodyNode(args.Arguments[0].NameNode, args.Arguments[1].NameNode, args.Arguments[2].NameNode, args.Arguments[3].NameNode);
        body.Body.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode() { LeftNode = _return, Optr = ProtoCore.DSASM.Operator.assign, RightNode = dotNode});
        funcDefNode.FunctionBody = body;
        (root as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(funcDefNode); 
    }

    private static void InsertDotMemFuncMethod(Core core, ProtoCore.AST.Node root)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        funcDefNode.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
        funcDefNode.Name = ProtoCore.DSASM.Constants.kDotArgMethodName;
        funcDefNode.IsBuiltIn = true;
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeVar), UID = (int)PrimitiveType.kTypeVar,
        rank = DSASM.Constants.kArbitraryRank, IsIndexable = true};

        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kLHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeVar), UID = (int)PrimitiveType.kTypeVar }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kRHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeInt), UID = (int)PrimitiveType.kTypeInt }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%rhsDimExprList"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeVar), UID = (int)PrimitiveType.kTypeVar, IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%rhsDim"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeInt), UID = (int)PrimitiveType.kTypeInt }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%rhsArgList"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeVar), UID = (int)PrimitiveType.kTypeVar, IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%rhsArgNum"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeInt), UID = (int)PrimitiveType.kTypeInt }
        });
        funcDefNode.Signature = args;

        ProtoCore.AST.AssociativeAST.CodeBlockNode body = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
        ProtoCore.AST.AssociativeAST.IdentifierNode _return = BuildAssocIdentifier(core, ProtoCore.DSDefinitions.Keyword.Return, ProtoCore.PrimitiveType.kTypeReturn);

        ProtoCore.AST.AssociativeAST.DotFunctionBodyNode dotNode = new ProtoCore.AST.AssociativeAST.DotFunctionBodyNode(args.Arguments[0].NameNode, args.Arguments[1].NameNode, args.Arguments[2].NameNode, args.Arguments[3].NameNode, args.Arguments[4].NameNode, args.Arguments[5].NameNode);
        body.Body.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode() { LeftNode = _return, Optr = ProtoCore.DSASM.Operator.assign, RightNode = dotNode});
        funcDefNode.FunctionBody = body;
        (root as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(funcDefNode); 
    }
 

    private static void InsertBinaryOperationMethod(Core core, ProtoCore.AST.Node root, Operator op, PrimitiveType r, PrimitiveType op1, PrimitiveType op2, int retRank = 0, int op1rank = 0, int op2rank = 0)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        funcDefNode.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
        funcDefNode.IsAssocOperator = true;
        funcDefNode.IsBuiltIn = true;
        funcDefNode.Name = Op.GetOpFunction(op);
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r, rank = retRank, IsIndexable = (retRank > 0)};
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kLHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)op1), UID = (int)op1, rank = op1rank, IsIndexable = (op1rank > 0)}
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kRHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)op2), UID = (int)op2, rank = op2rank, IsIndexable = (op2rank > 0)}
        });
        funcDefNode.Signature = args;

        ProtoCore.AST.AssociativeAST.CodeBlockNode body = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
        ProtoCore.AST.AssociativeAST.IdentifierNode _return = BuildAssocIdentifier(core, ProtoCore.DSDefinitions.Keyword.Return, ProtoCore.PrimitiveType.kTypeReturn);

        ProtoCore.AST.AssociativeAST.IdentifierNode lhs = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kLHS);
        ProtoCore.AST.AssociativeAST.IdentifierNode rhs = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kRHS);
        body.Body.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode() { LeftNode = _return, Optr = ProtoCore.DSASM.Operator.assign, RightNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode() { LeftNode = lhs, RightNode = rhs, Optr = op } });
        funcDefNode.FunctionBody = body;
        (root as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(funcDefNode);
    }

	// The following methods are used to insert methods to the bottom of the AST and convert operator to these method calls 
	// to support replication on operators 
	private static void InsertUnaryOperationMethod(Core core, ProtoCore.AST.Node root, UnaryOperator op, PrimitiveType r, PrimitiveType operand)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        funcDefNode.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
        funcDefNode.IsAssocOperator = true;
        funcDefNode.IsBuiltIn = true;
        funcDefNode.Name = Op.GetUnaryOpFunction(op);
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r };
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%param"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)operand), UID = (int)operand }
        });
        funcDefNode.Signature = args;

        ProtoCore.AST.AssociativeAST.CodeBlockNode body = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
        ProtoCore.AST.AssociativeAST.IdentifierNode _return = BuildAssocIdentifier(core, ProtoCore.DSDefinitions.Keyword.Return, ProtoCore.PrimitiveType.kTypeReturn);
        ProtoCore.AST.AssociativeAST.IdentifierNode param = BuildAssocIdentifier(core, "%param");
        body.Body.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode() { LeftNode = _return, Optr = ProtoCore.DSASM.Operator.assign, RightNode = new ProtoCore.AST.AssociativeAST.UnaryExpressionNode() { Expression = param, Operator = op } });
        funcDefNode.FunctionBody = body;
        (root as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(funcDefNode);
    }

	private static void InsertInlineConditionOperationMethod(Core core, ProtoCore.AST.Node root, PrimitiveType condition, PrimitiveType r)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        funcDefNode.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
        funcDefNode.Name = ProtoCore.DSASM.Constants.kInlineCondition; 
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r };
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%condition"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)condition), UID = (int)condition }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%trueExp"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)r), UID = (int)r }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%falseExp"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)r), UID = (int)r }
        });
        funcDefNode.Signature = args;

        ProtoCore.AST.AssociativeAST.CodeBlockNode body = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
        ProtoCore.AST.AssociativeAST.IdentifierNode _return = BuildAssocIdentifier(core, ProtoCore.DSDefinitions.Keyword.Return, ProtoCore.PrimitiveType.kTypeReturn);
        ProtoCore.AST.AssociativeAST.IdentifierNode con = BuildAssocIdentifier(core, "%condition");
        ProtoCore.AST.AssociativeAST.IdentifierNode t = BuildAssocIdentifier(core, "%trueExp");
        ProtoCore.AST.AssociativeAST.IdentifierNode f = BuildAssocIdentifier(core, "%falseExp");

        body.Body.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode() { LeftNode = _return, Optr = Operator.assign, RightNode = new ProtoCore.AST.AssociativeAST.InlineConditionalNode() { ConditionExpression = con, TrueExpression = t, FalseExpression = f } });
        funcDefNode.FunctionBody = body;
        (root as ProtoCore.AST.AssociativeAST.CodeBlockNode).Body.Add(funcDefNode);
    }

    private static void InsertPredefinedMethod(Core core, ProtoCore.AST.Node root, bool builtinMethodsLoaded)
    {
        if (!builtinMethodsLoaded)
        {
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeChar, PrimitiveType.kTypeChar);


            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeString, PrimitiveType.kTypeString);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeVar, PrimitiveType.kTypeString, PrimitiveType.kTypeChar);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeVar, PrimitiveType.kTypeChar, PrimitiveType.kTypeString);
            //			InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeString, PrimitiveType.kTypeVar);
            //          InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeVar, PrimitiveType.kTypeString);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeString, PrimitiveType.kTypeVar, 0, 0, ProtoCore.DSASM.Constants.kArbitraryRank);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeVar, PrimitiveType.kTypeString, 0, ProtoCore.DSASM.Constants.kArbitraryRank, 0);

            InsertBinaryOperationMethod(core, root, Operator.sub, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.sub, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.sub, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.sub, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            //InsertBinaryOperationMethod(core, root, Operator.div, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            //InsertBinaryOperationMethod(core, root, Operator.div, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt);
            //InsertBinaryOperationMethod(core, root, Operator.div, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.div, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.mul, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.mul, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.mul, PrimitiveType.kTypeDouble, PrimitiveType.kTypeInt, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.mul, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.mod, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);

            InsertBinaryOperationMethod(core, root, Operator.bitwiseand, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.bitwiseand, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);
            InsertBinaryOperationMethod(core, root, Operator.bitwiseor, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.bitwiseor, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);
            InsertBinaryOperationMethod(core, root, Operator.bitwisexor, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.bitwisexor, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);

            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.kTypeBool, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.kTypeBool, PrimitiveType.kTypeString, PrimitiveType.kTypeString);
            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);
            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeVar);
            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.kTypeBool, PrimitiveType.kTypeVar, PrimitiveType.kTypeBool);

            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.kTypeBool, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.kTypeBool, PrimitiveType.kTypeString, PrimitiveType.kTypeString);
            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);
            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeVar);
            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.kTypeBool, PrimitiveType.kTypeVar, PrimitiveType.kTypeBool);

            InsertBinaryOperationMethod(core, root, Operator.ge, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.ge, PrimitiveType.kTypeBool, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.gt, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.gt, PrimitiveType.kTypeBool, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.le, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.le, PrimitiveType.kTypeBool, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.lt, PrimitiveType.kTypeBool, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertBinaryOperationMethod(core, root, Operator.lt, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.and, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertBinaryOperationMethod(core, root, Operator.or, PrimitiveType.kTypeBool, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);

            InsertUnaryOperationMethod(core, root, UnaryOperator.Neg, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Neg, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Negate, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Negate, PrimitiveType.kTypeDouble, PrimitiveType.kTypeDouble);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Not, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);

            InsertBinaryOperationMethod(core, root, Operator.and, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);
            InsertBinaryOperationMethod(core, root, Operator.or, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool, PrimitiveType.kTypeBool);

            InsertUnaryOperationMethod(core, root, UnaryOperator.Decrement, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Increment, PrimitiveType.kTypeInt, PrimitiveType.kTypeInt);
        }
    }
	

        public static string GetLanguageString(Language language)
        {
            string languageString = string.Empty;
            if (Language.kAssociative == language)
            {
                languageString = ProtoCore.DSASM.kw.associative;
            }
            else if (Language.kImperative == language)
            {
                languageString = ProtoCore.DSASM.kw.imperative;
            }
            else if (Language.kOptions == language)
            {
                languageString = ProtoCore.DSASM.kw.options;
            }
            return languageString;
        }

        public static void LogWarning(this Interpreter dsi, ProtoCore.RuntimeData.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
        {
            ProtoCore.Core core = dsi.runtime.Core;
            core.RuntimeStatus.LogWarning(id, msg, fileName, line, col);
        }

        public static void LogSemanticError(this Interpreter dsi, string msg, string fileName = null, int line = -1, int col = -1)
        {
            ProtoCore.Core core = dsi.runtime.Core;
            core.BuildStatus.LogSemanticError(msg, fileName, line, col);
        }

        public static void LogWarning(this Core core, ProtoCore.RuntimeData.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
        {
            core.RuntimeStatus.LogWarning(id, msg, fileName, line, col);
        }

        public static void LogWarning(this Core core, ProtoCore.BuildData.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
        {
            core.BuildStatus.LogWarning(id, msg, fileName, line, col);
        }

        public static void LogSemanticError(this Core core, string msg, string fileName = null, int line = -1, int col = -1)
        {
            core.BuildStatus.LogSemanticError(msg, fileName, line, col);
        }


        public static string GenerateIdentListNameString(ProtoCore.AST.AssociativeAST.AssociativeNode node)
        {
            ProtoCore.AST.AssociativeAST.IdentifierListNode iNode;
            ProtoCore.AST.AssociativeAST.AssociativeNode leftNode = node;
            List<string> stringList = new List<string>();
            while (leftNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                iNode = leftNode as ProtoCore.AST.AssociativeAST.IdentifierListNode;
                leftNode = iNode.LeftNode;
                if (iNode.RightNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                {
                    stringList.Add((iNode.RightNode as ProtoCore.AST.AssociativeAST.IdentifierNode).Value);
                }
                else if (iNode.RightNode is ProtoCore.AST.AssociativeAST.FunctionCallNode)
                {
                    ProtoCore.AST.AssociativeAST.FunctionCallNode fCall = iNode.RightNode as ProtoCore.AST.AssociativeAST.FunctionCallNode;
                    stringList.Add(fCall.Function.Name);
                }
                else
                {
                    return string.Empty;
                }
            }
            stringList.Add(leftNode.Name);

            stringList.Reverse();

            string retString = string.Empty;
            foreach (string s in stringList)
            {
                retString += s;
                retString += '.';
            }

            // Remove the last dot
            retString = retString.Remove(retString.Length - 1);

            return retString;
        }

        public static bool IsAutoGeneratedVar(string name)
        {
            Validity.Assert(null != name);
            return name.StartsWith("%");
        }

        public static bool IsGetter(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return propertyName.StartsWith(ProtoCore.DSASM.Constants.kGetterPrefix);
        }

        public static bool IsSetter(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return propertyName.StartsWith(ProtoCore.DSASM.Constants.kSetterPrefix);
        }

        public static bool TryGetOperator(string methodName, out Operator op)
        {
            Validity.Assert(null != methodName);
            if (!methodName.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix))
            {
                op = Operator.none;
                return false;
            }

            string realMethodName = methodName.Substring(Constants.kInternalNamePrefix.Length);
            return System.Enum.TryParse(realMethodName, out op);
        }

        public static string GetOperatorString(DSASM.Operator optr)
        {
            return Op.GetOpSymbol(optr);
        }

        public static bool TryGetPropertyName(string methodName, out string propertyName)
        {
            Validity.Assert(null != methodName);
            if (IsGetter(methodName))
            {
                propertyName = methodName.Substring(ProtoCore.DSASM.Constants.kGetterPrefix.Length);
                return true;
            }
            else if (IsSetter(methodName))
            {
                propertyName = methodName.Substring(ProtoCore.DSASM.Constants.kSetterPrefix.Length);
                return true;
            }

            propertyName = null;
            return false;
        }

        public static bool IsGlobalInstanceSetter(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return propertyName.StartsWith(ProtoCore.DSASM.Constants.kGlobalInstanceNamePrefix) && propertyName.Contains(ProtoCore.DSASM.Constants.kSetterPrefix);
        }

        public static bool GetGlobalInstanceSetterName(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return propertyName.StartsWith(ProtoCore.DSASM.Constants.kGlobalInstanceNamePrefix) && propertyName.Contains(ProtoCore.DSASM.Constants.kSetterPrefix);
        }

        public static bool IsInternalMethod(string methodName)
        {
            Validity.Assert(null != methodName);
            return methodName.StartsWith(Constants.kInternalNamePrefix);
        }

        public static bool IsGetterSetter(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return IsGetter(propertyName) || IsSetter(propertyName);
        }


        public static bool IsGlobalInstanceGetterSetter(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return propertyName.StartsWith(ProtoCore.DSASM.Constants.kGlobalInstanceNamePrefix) && IsGetterSetter(propertyName);
        }

        public static string GetMangledFunctionName(string className, string functionName)
        {
            string name = ProtoCore.DSASM.Constants.kGlobalInstanceNamePrefix + className + ProtoCore.DSASM.Constants.kGlobalInstanceFunctionPrefix + functionName;
            return name;
        }

        public static string GetMangledFunctionName(int classIndex, string functionName, Core core)
        {
            Validity.Assert(classIndex < core.ClassTable.ClassNodes.Count);
            ClassNode cnode = core.ClassTable.ClassNodes[classIndex];
            string name = ProtoCore.DSASM.Constants.kGlobalInstanceNamePrefix + cnode.name + ProtoCore.DSASM.Constants.kGlobalInstanceFunctionPrefix + functionName;
            return name;
        }

        public static string BuildSSATemp(Core core)
        {
            // Jun Comment: The current convention for auto generated SSA variables begin with '%'
            // This ensures that the variables is compiler generated as the '%' symbol cannot be used as an identifier and will fail compilation
            string SSATemp = ProtoCore.DSASM.Constants.kSSATempPrefix + core.SSASubscript.ToString();
            ++core.SSASubscript;
            return SSATemp;
        }

        public static bool IsSSATemp(string ssaVar)
        {
            // Jun Comment: The current convention for auto generated SSA variables begin with '%'
            // This ensures that the variables is compiler generated as the '%' symbol cannot be used as an identifier and will fail compilation
            Validity.Assert(null != ssaVar);
            return ssaVar.StartsWith(ProtoCore.DSASM.Constants.kSSATempPrefix);
        }

        public static bool IsTempVarProperty(string varname)
        {
            Validity.Assert(null != varname);
            return varname.StartsWith(ProtoCore.DSASM.Constants.kTempPropertyVar);
        }

        public static bool IsCompilerGenerated(string varname)
        {
            // Jun Comment: Help function to determine if its a compiler generated temp
            Validity.Assert(null != varname);
            return varname.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix);
        }

        public static bool IsInternalFunction(string methodName)
        {
            // Jun Comment: Help function to determine if its a compiler generated temp
            Validity.Assert(null != methodName);
            return methodName.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix) || methodName.StartsWith(ProtoCore.DSDefinitions.Keyword.Dispose);
        }

        public static bool IsPropertyTemp(string varname)
        {
            // Jun Comment: Help function to determine if its a compiler generated temp specifically for a property
            Validity.Assert(null != varname);
            return varname.StartsWith(ProtoCore.DSASM.Constants.kTempPropertyVar);
        }

        public static ProtoCore.AST.AssociativeAST.FunctionDotCallNode GenerateCallDotNode(ProtoCore.AST.AssociativeAST.AssociativeNode lhs, 
            ProtoCore.AST.AssociativeAST.FunctionCallNode rhsCall, Core core = null)
        {
            // The function name to call
            string rhsName = rhsCall.Function.Name;
            int argNum = rhsCall.FormalArguments.Count;
            ProtoCore.AST.AssociativeAST.ExprListNode argList = new ProtoCore.AST.AssociativeAST.ExprListNode();
            foreach (ProtoCore.AST.AssociativeAST.AssociativeNode arg in rhsCall.FormalArguments)
            {
                // The function arguments
                argList.list.Add(arg);
            }


            ProtoCore.AST.AssociativeAST.FunctionCallNode funCallNode = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            ProtoCore.AST.AssociativeAST.IdentifierNode funcName = new ProtoCore.AST.AssociativeAST.IdentifierNode { Value = ProtoCore.DSASM.Constants.kDotArgMethodName, Name = ProtoCore.DSASM.Constants.kDotArgMethodName };
            funCallNode.Function = funcName;
            funCallNode.Name = ProtoCore.DSASM.Constants.kDotArgMethodName;
            NodeUtils.CopyNodeLocation(funCallNode, lhs);
            int rhsIdx = ProtoCore.DSASM.Constants.kInvalidIndex;
            string lhsName = null;
            if (lhs is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                lhsName = (lhs as ProtoCore.AST.AssociativeAST.IdentifierNode).Name;
                if (lhsName == ProtoCore.DSDefinitions.Keyword.This)
                {
                    lhs = new ProtoCore.AST.AssociativeAST.ThisPointerNode();
                }
            }

            if (core != null)
            {
                if (argNum >= 0)
                {
                    ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(rhsName, new List<ProtoCore.Type>());
                    core.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);
                    rhsIdx = core.DynamicFunctionTable.functionTable.Count - 1;
                }
                else
                {
                    DSASM.DyanmicVariableNode dynamicVariableNode = new DSASM.DyanmicVariableNode(rhsName);
                    core.DynamicVariableTable.variableTable.Add(dynamicVariableNode);
                    rhsIdx = core.DynamicVariableTable.variableTable.Count - 1;
                }
            }

            // The first param to the dot arg (the pointer or the class name)
            ProtoCore.AST.AssociativeAST.IntNode rhs = new ProtoCore.AST.AssociativeAST.IntNode() { value = rhsIdx.ToString() };
            funCallNode.FormalArguments.Add(lhs);


            // The second param which is the dynamic table index of the function to call
            funCallNode.FormalArguments.Add(rhs);


            // The array dimensions
            ProtoCore.AST.AssociativeAST.ExprListNode arrayDimExperList = new ProtoCore.AST.AssociativeAST.ExprListNode();
            int dimCount = 0;
            if (rhsCall.Function is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                // Number of dimensions
                ProtoCore.AST.AssociativeAST.IdentifierNode fIdent = rhsCall.Function as ProtoCore.AST.AssociativeAST.IdentifierNode;
                if (fIdent.ArrayDimensions != null)
                {
                    arrayDimExperList = ProtoCore.Utils.CoreUtils.BuildArrayExprList(fIdent.ArrayDimensions);
                    dimCount = arrayDimExperList.list.Count;
                }
                else if (rhsCall.ArrayDimensions != null)
                {
                    arrayDimExperList = ProtoCore.Utils.CoreUtils.BuildArrayExprList(rhsCall.ArrayDimensions);
                    dimCount = arrayDimExperList.list.Count;
                }
                else
                {
                    arrayDimExperList = new ProtoCore.AST.AssociativeAST.ExprListNode();
                }
            }

            funCallNode.FormalArguments.Add(arrayDimExperList);

            // Number of dimensions
            ProtoCore.AST.AssociativeAST.IntNode dimNode = new ProtoCore.AST.AssociativeAST.IntNode() { value = dimCount.ToString() };
            funCallNode.FormalArguments.Add(dimNode);

            if (argNum >= 0)
            {
                funCallNode.FormalArguments.Add(argList);
                funCallNode.FormalArguments.Add(new ProtoCore.AST.AssociativeAST.IntNode() { value = argNum.ToString() });
            }


            ProtoCore.AST.AssociativeAST.FunctionDotCallNode funDotCallNode = new ProtoCore.AST.AssociativeAST.FunctionDotCallNode(rhsCall);
            funDotCallNode.DotCall = funCallNode;
            funDotCallNode.FunctionCall.Function = rhsCall.Function;

            // Consider the case of "myClass.Foo(a, b)", we will have "DotCall" being 
            // equal to "myClass" (in terms of its starting line/column), and "rhsCall" 
            // matching with the location of "Foo(a, b)". For execution cursor to cover 
            // this whole statement, the final "DotCall" function call node should 
            // range from "lhs.col" to "rhs.col".
            // 
            NodeUtils.SetNodeEndLocation(funDotCallNode.DotCall, rhsCall);
            NodeUtils.CopyNodeLocation(funDotCallNode, funDotCallNode.DotCall);


            return funDotCallNode;
        }


        public static ProtoCore.AST.AssociativeAST.ExprListNode BuildArrayExprList(ProtoCore.AST.AssociativeAST.AssociativeNode arrayNode)
        {
            ProtoCore.AST.AssociativeAST.ExprListNode exprlist = new ProtoCore.AST.AssociativeAST.ExprListNode();
            while (arrayNode is ProtoCore.AST.AssociativeAST.ArrayNode)
            {
                ProtoCore.AST.AssociativeAST.ArrayNode array = arrayNode as ProtoCore.AST.AssociativeAST.ArrayNode;
                exprlist.list.Add(array.Expr);
                arrayNode = array.Type;
            }
            return exprlist;
        }


        // Comment Jun: 
        // Instead of this method, consider storing the name mangled methods original class name and varname
        public static string GetClassDeclarationName(ProcedureNode procNode, Core core)
        {
            string mangledName = procNode.name;
            mangledName = mangledName.Remove(0, ProtoCore.DSASM.Constants.kGlobalInstanceNamePrefix.Length);

            int start = mangledName.IndexOf(ProtoCore.DSASM.Constants.kGlobalInstanceFunctionPrefix);
            mangledName = mangledName.Remove(start);
            return mangledName;

            if (ProtoCore.DSASM.Constants.kInvalidIndex == procNode.classScope)
            {
                return string.Empty;
            }

            Validity.Assert(core.ClassTable.ClassNodes.Count > procNode.classScope);
            return core.ClassTable.ClassNodes[procNode.classScope].name;
        }



        public static ProcedureNode GetClassAndProcFromGlobalInstance(ProcedureNode procNode, Core core, out int classIndex, List<Type> argTypeList)
        {
            string className = ProtoCore.Utils.CoreUtils.GetClassDeclarationName(procNode, core);
            classIndex = core.ClassTable.IndexOf(className);


            int removelength = 0;
            if (ProtoCore.Utils.CoreUtils.IsGlobalInstanceGetterSetter(procNode.name))
            {
                if (ProtoCore.Utils.CoreUtils.IsGlobalInstanceSetter(procNode.name))
                {
                    removelength = procNode.name.IndexOf(ProtoCore.DSASM.Constants.kSetterPrefix);
                }
                else
                {
                    removelength = procNode.name.IndexOf(ProtoCore.DSASM.Constants.kGetterPrefix);
                }
            }
            else
            {
                removelength = procNode.name.IndexOf(ProtoCore.DSASM.Constants.kGlobalInstanceFunctionPrefix);
                removelength += ProtoCore.DSASM.Constants.kGlobalInstanceFunctionPrefix.Length;
            }

            string functionName = procNode.name.Remove(0, removelength);
            //ProtoCore.DSASM.ProcedureNode tmpProcNode = core.ClassTable.list[classIndex].GetFirstMemberFunction(functionName, procNode.argTypeList.Count - 1);

            int functionIndex = core.ClassTable.ClassNodes[classIndex].vtable.IndexOfExact(functionName, argTypeList);
            ProtoCore.DSASM.ProcedureNode tmpProcNode = core.ClassTable.ClassNodes[classIndex].vtable.procList[functionIndex];

            return tmpProcNode;
        }

        public static bool Compare(ProtoCore.AST.Node node1, ProtoCore.AST.Node node2)
        {
            return node1.Equals(node2);
        }

        public static bool Compare(string s1, string s2, Core core)
        {
            System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(s1));
            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
            ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, core);
            p.Parse();
            ProtoCore.AST.Node s1Root = p.root;

            memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(s2));
            s = new ProtoCore.DesignScriptParser.Scanner(s2);
            p = new ProtoCore.DesignScriptParser.Parser(s, core);
            p.Parse();
            ProtoCore.AST.Node s2Root = p.root;

            bool areEqual = s1Root.Equals(s2Root);
            return areEqual;
        }

        public static void CopyDebugData(ProtoCore.AST.Node nodeTo, ProtoCore.AST.Node nodeFrom)
        {
            if (null != nodeTo && null != nodeFrom)
            {
                nodeTo.col = nodeFrom.col;
                nodeTo.endCol = nodeFrom.endCol;
                nodeTo.endLine = nodeFrom.endLine;
                nodeTo.line = nodeFrom.line;
            }
        }


        /// <summary>
        /// Gets the has id of a function signature given the name and argument types
        /// </summary>
        /// <param name="functionDef"></param>
        /// <returns></returns>
        public static int GetFunctionHash(ProtoCore.AST.AssociativeAST.FunctionDefinitionNode functionDef)
        {
            Validity.Assert(null != functionDef);
            string functionDescription = functionDef.Name;
            foreach (ProtoCore.AST.AssociativeAST.VarDeclNode argNode in functionDef.Signature.Arguments)
            {
                functionDescription += argNode.ArgumentType.ToString();
            }
            return functionDescription.GetHashCode();
        }
    }
}
