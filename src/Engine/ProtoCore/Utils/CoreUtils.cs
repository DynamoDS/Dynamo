﻿
using ProtoCore.DSASM;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;

namespace ProtoCore.Utils
{
    public static class CoreUtils
    {
        public static void InsertPredefinedAndBuiltinMethods(Core core, ProtoCore.AST.Node root, bool builtinMethodsLoaded)
        {
            if (DSASM.InterpreterMode.kNormal == core.ExecMode)
            {
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
        ident.datatype = TypeSystem.BuildPrimitiveTypeObject(type, 0);
        return ident;
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
        fDef.MethodAttributes = method.MethodAttributes;

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

    private static void InsertBinaryOperationMethod(Core core, ProtoCore.AST.Node root, Operator op, PrimitiveType r, PrimitiveType op1, PrimitiveType op2, int retRank = 0, int op1rank = 0, int op2rank = 0)
    {
        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
        funcDefNode.access = ProtoCore.Compiler.AccessSpecifier.kPublic;
        funcDefNode.IsAssocOperator = true;
        funcDefNode.IsBuiltIn = true;
        funcDefNode.Name = Op.GetOpFunction(op);
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r, rank = retRank};
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.Compiler.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kLHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)op1), UID = (int)op1, rank = op1rank}
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.Compiler.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, ProtoCore.DSASM.Constants.kRHS),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)op2), UID = (int)op2, rank = op2rank}
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
        funcDefNode.access = ProtoCore.Compiler.AccessSpecifier.kPublic;
        funcDefNode.IsAssocOperator = true;
        funcDefNode.IsBuiltIn = true;
        funcDefNode.Name = Op.GetUnaryOpFunction(op);
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r };
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.Compiler.AccessSpecifier.kPublic,
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
        funcDefNode.access = ProtoCore.Compiler.AccessSpecifier.kPublic;
        funcDefNode.Name = ProtoCore.DSASM.Constants.kInlineCondition; 
        funcDefNode.ReturnType = new ProtoCore.Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r };
        ProtoCore.AST.AssociativeAST.ArgumentSignatureNode args = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.Compiler.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%condition"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)condition), UID = (int)condition }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.Compiler.AccessSpecifier.kPublic,
            NameNode = BuildAssocIdentifier(core, "%trueExp"),
            ArgumentType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)r), UID = (int)r }
        });
        args.AddArgument(new ProtoCore.AST.AssociativeAST.VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
            access = ProtoCore.Compiler.AccessSpecifier.kPublic,
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
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeString, PrimitiveType.kTypeVar);
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.kTypeString, PrimitiveType.kTypeVar, PrimitiveType.kTypeString);

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

        public static void LogWarning(this Interpreter dsi, ProtoCore.Runtime.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
        {
            ProtoCore.Core core = dsi.runtime.Core;
            core.RuntimeStatus.LogWarning(id, msg, fileName, line, col);
        }

        public static void LogSemanticError(this Interpreter dsi, string msg, string fileName = null, int line = -1, int col = -1)
        {
            ProtoCore.Core core = dsi.runtime.Core;
            core.BuildStatus.LogSemanticError(msg, fileName, line, col);
        }

        public static void LogWarning(this Core core, ProtoCore.Runtime.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
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
                    ProtoCore.AST.AssociativeAST.IdentifierNode currentNode = (iNode.RightNode as ProtoCore.AST.AssociativeAST.IdentifierNode);
                    stringList.Add(currentNode.ToString());

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
            stringList.Add(leftNode.ToString());

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

        public static bool StartsWithSingleUnderscore(string name)
        {
            Validity.Assert(null != name);
            return name.StartsWith(ProtoCore.DSASM.Constants.kSingleUnderscore);
        }

        public static bool StartsWithDoubleUnderscores(string name)
        {
            Validity.Assert(null != name);
            return name.StartsWith(ProtoCore.DSASM.Constants.kDoubleUnderscores);
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
            string sGUID = core.SSASubscript_GUID.ToString();
            sGUID = sGUID.Replace("-", string.Empty);
            string SSATemp = ProtoCore.DSASM.Constants.kSSATempPrefix + core.SSASubscript.ToString() + "_" + sGUID;
            ++core.SSASubscript;
            return SSATemp;
        }

        public static bool IsSSATemp(string ssaVar)
        {
            // Jun Comment: The current convention for auto generated SSA variables begin with '%'
            // This ensures that the variables is compiler generated as the '%' symbol cannot be used as an identifier and will fail compilation
            Validity.Assert(!string.IsNullOrEmpty(ssaVar));
            return ssaVar.StartsWith(ProtoCore.DSASM.Constants.kSSATempPrefix);
        }

        public static bool IsTempVarProperty(string varname)
        {
            Validity.Assert(!string.IsNullOrEmpty(varname));
            return varname.StartsWith(ProtoCore.DSASM.Constants.kTempPropertyVar);
        }

        public static bool IsCompilerGenerated(string varname)
        {
            Validity.Assert(!string.IsNullOrEmpty(varname));
            return varname.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix);
        }

        public static bool IsInternalFunction(string methodName)
        {
            Validity.Assert(!string.IsNullOrEmpty(methodName));
            return methodName.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix) || methodName.StartsWith(ProtoCore.DSDefinitions.Keyword.Dispose);
        }

        public static bool IsDisposeMethod(string methodName)
        {
            Validity.Assert(!string.IsNullOrEmpty(methodName));
            return methodName.Equals(ProtoCore.DSDefinitions.Keyword.Dispose);
        }

        public static bool IsGetTypeMethod(string methodName)
        {
            Validity.Assert(!string.IsNullOrEmpty(methodName));
            return methodName.Equals(ProtoCore.DSDefinitions.Keyword.GetType);
        }

        public static bool IsPropertyTemp(string varname)
        {
            Validity.Assert(!string.IsNullOrEmpty(varname));
            return varname.StartsWith(ProtoCore.DSASM.Constants.kTempPropertyVar);
        }

        public static bool IsDefaultArgTemp(string varname)
        {
            Validity.Assert(null != varname);
            return varname.StartsWith(ProtoCore.DSASM.Constants.kTempDefaultArg);
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


            FunctionCallNode funCallNode = new FunctionCallNode();
            IdentifierNode funcName = new IdentifierNode { Value = Constants.kDotArgMethodName, Name = Constants.kDotArgMethodName };
            funCallNode.Function = funcName;
            funCallNode.Name = Constants.kDotArgMethodName;

            NodeUtils.CopyNodeLocation(funCallNode, lhs);
            int rhsIdx = ProtoCore.DSASM.Constants.kInvalidIndex;
            string lhsName = string.Empty;
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
                DynamicFunction func;
                if (core.DynamicFunctionTable.TryGetFunction(rhsName, 0, Constants.kInvalidIndex, out func))
                {
                    rhsIdx = func.Index;
                }
                else
                {
                    func = core.DynamicFunctionTable.AddNewFunction(rhsName, 0, Constants.kInvalidIndex);
                    rhsIdx = func.Index;
                }
            }

            // The first param to the dot arg (the pointer or the class name)
            funCallNode.FormalArguments.Add(lhs);

            // The second param which is the dynamic table index of the function to call
            var rhs = new IntNode(rhsIdx);
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
            var dimNode = new IntNode(dimCount);
            funCallNode.FormalArguments.Add(dimNode);

            if (argNum >= 0)
            {
                funCallNode.FormalArguments.Add(argList);
                funCallNode.FormalArguments.Add(new IntNode(argNum));
            }

            var funDotCallNode = new FunctionDotCallNode(rhsCall);
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

            int functionIndex = core.ClassTable.ClassNodes[classIndex].vtable.IndexOfExact(functionName, argTypeList, procNode.isAutoGeneratedThisProc);
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

        /// <summary>
        /// Retrieves the string format of the identifier list from left to right, leaving out any symbols after the last identifier.
        /// Given: A.B()
        ///     Return: "A.B"
        /// Given: A.B.C()[0]
        ///     Return: "A.B.C"
        /// Given: A.B().C
        ///     Return: "A.B"
        /// Given: A.B[0].C
        ///     Return: "A.B[0].C"
        /// </summary>
        /// <param name="identList"></param>
        /// <returns></returns>
        public static string GetIdentifierStringUntilFirstParenthesis(ProtoCore.AST.AssociativeAST.IdentifierListNode identList)
        {
            Validity.Assert(null != identList);
            string identListString = identList.ToString();
            int removeIndex = identListString.IndexOf('(');
            if (removeIndex > 0)
            {
                identListString = identListString.Remove(removeIndex);
            }
            return identListString;
        }

        /// <summary>
        /// Traverses the identifierlist argument until class name resolution succeeds or fails.
        /// </summary>
        /// <param name="classTable"></param>
        /// <param name="identList"></param>
        /// <returns></returns>
        public static string[] GetResolvedClassName(ProtoCore.DSASM.ClassTable classTable, ProtoCore.AST.AssociativeAST.IdentifierListNode identList)
        {
            string[] classNames = classTable.GetAllMatchingClasses(ProtoCore.Utils.CoreUtils.GetIdentifierStringUntilFirstParenthesis(identList));

            // Failed to find the first time
            // Attempt to remove identifiers in the identifierlist until we find a class or not
            while (0 == classNames.Length)
            {
                // Move to the left node
                AssociativeNode leftNode = identList.LeftNode;
                if (leftNode is IdentifierListNode)
                {
                    identList = leftNode as IdentifierListNode;
                    classNames = classTable.GetAllMatchingClasses(ProtoCore.Utils.CoreUtils.GetIdentifierStringUntilFirstParenthesis(identList));
                }
                if (leftNode is IdentifierNode)
                {
                    IdentifierNode identNode = leftNode as IdentifierNode;
                    classNames = classTable.GetAllMatchingClasses(identNode.Name);
                    break;
                }
                else
                {
                    break;
                }
            }
            return classNames;
        }

        /// <summary>
        /// Parses designscript code and outputs ProtoAST
        /// </summary>
        /// <param name="core"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<AssociativeNode> BuildASTList(ProtoCore.Core core, string code)
        {
            Validity.Assert(null != core);
            List<AssociativeNode> astList = new List<AssociativeNode>();
            var cbn = ProtoCore.Utils.ParserUtils.Parse(code) as CodeBlockNode;
            astList.AddRange(cbn.Body);
            return astList;
        }


        /// <summary>
        /// Parses designscript code and outputs ProtoAST
        /// </summary>
        /// <param name="core"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<AssociativeNode> BuildASTList(ProtoCore.Core core, List<string> codeList)
        {
            List<AssociativeNode> astList = new List<AssociativeNode>();
            foreach (string code in codeList)
            {
                astList.AddRange(BuildASTList(core, code));
            }
            return astList;
        }
    }
}
