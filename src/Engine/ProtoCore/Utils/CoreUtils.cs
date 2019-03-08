using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using System.Collections.Generic;
using System.Linq;

namespace ProtoCore.Utils
{
    public static class CoreUtils
    {


        public static void InsertPredefinedAndBuiltinMethods(Core core, CodeBlockNode root)
        {
            if (DSASM.InterpreterMode.Normal == core.Options.RunMode)
            {
                InsertPredefinedMethod(core, root);
                InsertBuiltInMethods(core, root);
            }
        }
        private static FunctionDefinitionNode GenerateBuiltInMethodSignatureNode(Lang.BuiltInMethods.BuiltInMethod method)
        {
            FunctionDefinitionNode fDef = new FunctionDefinitionNode();
            fDef.Name = Lang.BuiltInMethods.GetMethodName(method.ID);
            fDef.ReturnType = method.ReturnType;
            fDef.IsExternLib = true;
            fDef.IsBuiltIn = true;
            fDef.BuiltInMethodId = method.ID;
            fDef.Signature = new ArgumentSignatureNode();
            fDef.MethodAttributes = method.MethodAttributes;

            foreach (KeyValuePair<string, Type> param in method.Parameters)
            {
                VarDeclNode arg = new VarDeclNode();
                arg.NameNode = new IdentifierNode { Name = param.Key, Value = param.Key };
                arg.ArgumentType = param.Value;
                fDef.Signature.AddArgument(arg);
            }

            return fDef;
        }

        private static void InsertBuiltInMethods(Core core, CodeBlockNode root)
        {
            Lang.BuiltInMethods builtInMethods = new Lang.BuiltInMethods(core);
            foreach (Lang.BuiltInMethods.BuiltInMethod method in builtInMethods.Methods)
            {
                root.Body.Add(GenerateBuiltInMethodSignatureNode(method));
            }
        }

        private static void InsertBinaryOperationMethod(Core core, CodeBlockNode root, Operator op, PrimitiveType r, PrimitiveType op1, PrimitiveType op2, int retRank = 0, int op1rank = 0, int op2rank = 0)
        {
            FunctionDefinitionNode funcDefNode = new FunctionDefinitionNode();
            funcDefNode.Access = CompilerDefinitions.AccessModifier.Public;
            funcDefNode.IsAssocOperator = true;
            funcDefNode.IsBuiltIn = true;
            funcDefNode.Name = Op.GetOpFunction(op);
            funcDefNode.ReturnType = new Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r, rank = retRank };
            ArgumentSignatureNode args = new ArgumentSignatureNode();
            args.AddArgument(new VarDeclNode()
            {
                Access = CompilerDefinitions.AccessModifier.Public,
                NameNode = AstFactory.BuildIdentifier(DSASM.Constants.kLHS),
                ArgumentType = new Type { Name = core.TypeSystem.GetType((int)op1), UID = (int)op1, rank = op1rank }
            });
            args.AddArgument(new VarDeclNode()
            {
                Access = CompilerDefinitions.AccessModifier.Public,
                NameNode = AstFactory.BuildIdentifier(DSASM.Constants.kRHS),
                ArgumentType = new Type { Name = core.TypeSystem.GetType((int)op2), UID = (int)op2, rank = op2rank }
            });
            funcDefNode.Signature = args;

            CodeBlockNode body = new CodeBlockNode();

            var lhs = AstFactory.BuildIdentifier(DSASM.Constants.kLHS);
            var rhs = AstFactory.BuildIdentifier(DSASM.Constants.kRHS);
            var binaryExpr = AstFactory.BuildBinaryExpression(lhs, rhs, op);
            body.Body.Add(AstFactory.BuildReturnStatement(binaryExpr));

            funcDefNode.FunctionBody = body;
            root.Body.Add(funcDefNode);
        }

        // The following methods are used to insert methods to the bottom of the AST and convert operator to these method calls 
        // to support replication on operators 
        private static void InsertUnaryOperationMethod(Core core, CodeBlockNode root, UnaryOperator op, PrimitiveType r, PrimitiveType operand)
        {
            FunctionDefinitionNode funcDefNode = new FunctionDefinitionNode();
            funcDefNode.Access = CompilerDefinitions.AccessModifier.Public;
            funcDefNode.IsAssocOperator = true;
            funcDefNode.IsBuiltIn = true;
            funcDefNode.Name = Op.GetUnaryOpFunction(op);
            funcDefNode.ReturnType = new Type() { Name = core.TypeSystem.GetType((int)r), UID = (int)r };
            ArgumentSignatureNode args = new ArgumentSignatureNode();
            args.AddArgument(new VarDeclNode()
            {
                Access = CompilerDefinitions.AccessModifier.Public,
                NameNode = AstFactory.BuildIdentifier("%param"),
                ArgumentType = new Type { Name = core.TypeSystem.GetType((int)operand), UID = (int)operand }
            });
            funcDefNode.Signature = args;

            CodeBlockNode body = new CodeBlockNode();
            IdentifierNode param = AstFactory.BuildIdentifier("%param");
            body.Body.Add(AstFactory.BuildReturnStatement(new UnaryExpressionNode() { Expression = param, Operator = op }));
            funcDefNode.FunctionBody = body;
            root.Body.Add(funcDefNode);
        }

        private static void InsertPredefinedMethod(Core core, CodeBlockNode root)
        {
            InsertBinaryOperationMethod(core, root, Operator.add, PrimitiveType.Var, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.sub, PrimitiveType.Var, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.div, PrimitiveType.Var, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.mul, PrimitiveType.Var, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.mod, PrimitiveType.Var, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.eq, PrimitiveType.Bool, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.nq, PrimitiveType.Bool, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.ge, PrimitiveType.Bool, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.gt, PrimitiveType.Bool, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.le, PrimitiveType.Bool, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.lt, PrimitiveType.Bool, PrimitiveType.Var, PrimitiveType.Var);
            InsertBinaryOperationMethod(core, root, Operator.and, PrimitiveType.Bool, PrimitiveType.Bool, PrimitiveType.Bool);
            InsertBinaryOperationMethod(core, root, Operator.or, PrimitiveType.Bool, PrimitiveType.Bool, PrimitiveType.Bool);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Not, PrimitiveType.Bool, PrimitiveType.Bool);
            InsertUnaryOperationMethod(core, root, UnaryOperator.Neg, PrimitiveType.Var, PrimitiveType.Var);
        }


        public static string GetLanguageString(Language language)
        {
            string languageString = string.Empty;
            if (Language.Associative == language)
            {
                languageString = DSASM.kw.associative;
            }
            else if (Language.Imperative == language)
            {
                languageString = DSASM.kw.imperative;
            }
            return languageString;
        }

        public static void LogWarning(this Interpreter dsi, Runtime.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
        {
            RuntimeCore runtimeCore = dsi.runtime.RuntimeCore;
            runtimeCore.RuntimeStatus.LogWarning(id, msg, fileName, line, col);
        }

        public static void LogSemanticError(this Interpreter dsi, string msg, string fileName = null, int line = -1, int col = -1)
        {
            // Consider renaming this function as there is no such thing as a semantic error at runtime
            RuntimeCore runtimeCore = dsi.runtime.RuntimeCore;
            runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.Default, msg, fileName, line, col);
        }

        public static void LogWarning(this Core core, BuildData.WarningID id, string msg, string fileName = null, int line = -1, int col = -1)
        {
            core.BuildStatus.LogWarning(id, msg, fileName, line, col);
        }

        public static void LogSemanticError(this Core core, string msg, string fileName = null, int line = -1, int col = -1)
        {
            core.BuildStatus.LogSemanticError(msg, fileName, line, col);
        }


        public static string GenerateIdentListNameString(AssociativeNode node)
        {
            IdentifierListNode iNode;
            AssociativeNode leftNode = node;
            List<string> stringList = new List<string>();
            while (leftNode is IdentifierListNode)
            {
                iNode = leftNode as IdentifierListNode;
                leftNode = iNode.LeftNode;
                if (iNode.RightNode is IdentifierNode)
                {
                    IdentifierNode currentNode = (iNode.RightNode as IdentifierNode);
                    stringList.Add(currentNode.ToString());

                }
                else if (iNode.RightNode is FunctionCallNode)
                {
                    FunctionCallNode fCall = iNode.RightNode as FunctionCallNode;
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
            return propertyName.StartsWith(DSASM.Constants.kGetterPrefix);
        }

        public static bool IsSetter(string propertyName)
        {
            Validity.Assert(null != propertyName);
            return propertyName.StartsWith(DSASM.Constants.kSetterPrefix);
        }

        public static bool StartsWithSingleUnderscore(string name)
        {
            Validity.Assert(null != name);
            return name.StartsWith(DSASM.Constants.kSingleUnderscore);
        }

        public static bool StartsWithDoubleUnderscores(string name)
        {
            Validity.Assert(null != name);
            return name.StartsWith(DSASM.Constants.kDoubleUnderscores);
        }

        public static bool TryGetOperator(string methodName, out Operator op)
        {
            Validity.Assert(null != methodName);
            if (!methodName.StartsWith(DSASM.Constants.kInternalNamePrefix))
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
                propertyName = methodName.Substring(DSASM.Constants.kGetterPrefix.Length);
                return true;
            }
            else if (IsSetter(methodName))
            {
                propertyName = methodName.Substring(DSASM.Constants.kSetterPrefix.Length);
                return true;
            }

            propertyName = null;
            return false;
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

        public static string BuildSSATemp(Core core)
        {
            // Jun Comment: The current convention for auto generated SSA variables begin with '%'
            // This ensures that the variables is compiler generated as the '%' symbol cannot be used as an identifier and will fail compilation
            string sGUID = core.SSASubscript_GUID.ToString();
            sGUID = sGUID.Replace("-", string.Empty);
            string SSATemp = DSASM.Constants.kSSATempPrefix + core.SSASubscript.ToString() + "_" + sGUID;
            ++core.SSASubscript;
            return SSATemp;
        }

        public static bool IsSSATemp(string ssaVar)
        {
            // Jun Comment: The current convention for auto generated SSA variables begin with '%'
            // This ensures that the variables is compiler generated as the '%' symbol cannot be used as an identifier and will fail compilation
            Validity.Assert(!string.IsNullOrEmpty(ssaVar));
            return ssaVar.StartsWith(DSASM.Constants.kSSATempPrefix);
        }

        public static bool IsInternalFunction(string methodName)
        {
            Validity.Assert(!string.IsNullOrEmpty(methodName));
            return methodName.StartsWith(DSASM.Constants.kInternalNamePrefix) || methodName.StartsWith(DSDefinitions.Keyword.Dispose);
        }

        public static bool IsDisposeMethod(string methodName)
        {
            Validity.Assert(!string.IsNullOrEmpty(methodName));
            return methodName.Equals(DSDefinitions.Keyword.Dispose);
        }

        public static bool IsDotMethod(string methodName)
        {
            return methodName.Equals(Constants.kDotMethodName);
        }

        public static bool IsGetTypeMethod(string methodName)
        {
            Validity.Assert(!string.IsNullOrEmpty(methodName));
            return methodName.Equals(DSDefinitions.Keyword.GetType);
        }

        public static bool IsPropertyTemp(string varname)
        {
            Validity.Assert(!string.IsNullOrEmpty(varname));
            return varname.StartsWith(DSASM.Constants.kTempPropertyVar);
        }

        public static bool IsDefaultArgTemp(string varname)
        {
            Validity.Assert(null != varname);
            return varname.StartsWith(DSASM.Constants.kTempDefaultArg);
        }

        public static FunctionDotCallNode GenerateCallDotNode(AssociativeNode lhs,
            FunctionCallNode rhsCall, Core core = null)
        {
            // The function name to call
            string rhsName = rhsCall.Function.Name;
            int argNum = rhsCall.FormalArguments.Count;
            ExprListNode argList = new ExprListNode();
            foreach (AssociativeNode arg in rhsCall.FormalArguments)
            {
                // The function arguments
                argList.Exprs.Add(arg);
            }

            var arguments = new List<AssociativeNode>();

            int rhsIdx = DSASM.Constants.kInvalidIndex;
            string lhsName = string.Empty;
            if (lhs is IdentifierNode)
            {
                lhsName = (lhs as IdentifierNode).Name;
                if (lhsName == DSDefinitions.Keyword.This)
                {
                    lhs = new ThisPointerNode();
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
            arguments.Add(lhs);

            // The second param which is the dynamic table index of the function to call
            arguments.Add(new IntNode(rhsIdx));

            // The array dimensions
            ExprListNode arrayDimExperList = new ExprListNode();
            int dimCount = 0;
            if (rhsCall.Function is IdentifierNode)
            {
                // Number of dimensions
                IdentifierNode fIdent = rhsCall.Function as IdentifierNode;
                if (fIdent.ArrayDimensions != null)
                {
                    arrayDimExperList = CoreUtils.BuildArrayExprList(fIdent.ArrayDimensions);
                    dimCount = arrayDimExperList.Exprs.Count;
                }
                else if (rhsCall.ArrayDimensions != null)
                {
                    arrayDimExperList = CoreUtils.BuildArrayExprList(rhsCall.ArrayDimensions);
                    dimCount = arrayDimExperList.Exprs.Count;
                }
                else
                {
                    arrayDimExperList = new ExprListNode();
                }
            }

            arguments.Add(arrayDimExperList);

            // Number of dimensions
            var dimNode = new IntNode(dimCount);
            arguments.Add(dimNode);

            if (argNum >= 0)
            {
                arguments.Add(argList);
                arguments.Add(new IntNode(argNum));
            }

            var funDotCallNode = new FunctionDotCallNode(rhsCall, arguments);
            // funDotCallNode.FunctionCall.Function = rhsCall.Function;
            NodeUtils.SetNodeEndLocation(funDotCallNode, rhsCall);
            return funDotCallNode;
        }


        public static ExprListNode BuildArrayExprList(AssociativeNode arrayNode)
        {
            ExprListNode exprlist = new ExprListNode();
            while (arrayNode is ArrayNode)
            {
                ArrayNode array = arrayNode as ArrayNode;
                exprlist.Exprs.Add(array.Expr);
                arrayNode = array.Type;
            }
            return exprlist;
        }

        public static void CopyDebugData(AST.Node nodeTo, AST.Node nodeFrom)
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
        /// Returns the has id of a function signature given the name and argument types
        /// </summary>
        /// <param name="functionDef"></param>
        /// <returns></returns>
        public static int GetFunctionHash(FunctionDefinitionNode functionDef)
        {
            Validity.Assert(null != functionDef);
            string functionDescription = functionDef.Name;
            foreach (VarDeclNode argNode in functionDef.Signature.Arguments)
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
        public static string GetIdentifierStringUntilFirstParenthesis(AST.Node node)
        {
            dynamic identList = node as IdentifierListNode;
            if(identList == null)
            {
                identList = node as AST.ImperativeAST.IdentifierListNode;
            }
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
        /// Retrieves the string format of the identifier list from left to right, leaving out any symbols after the last identifier.
        /// Given: A.B()
        ///     Return: "A"
        /// Given: A.B.C()[0]
        ///     Return: "A.B"
        /// Given: A.B().C
        ///     Return: "A"
        /// Given: A.B[0].C
        ///     Return: "A.B[0].C"
        /// Given: A().B (global function)
        ///     Return: empty string
        /// Given: A.B[0].C()
        ///     Return: "A.B[0]"
        /// </summary>
        /// <param name="identList"></param>
        /// <returns></returns>
        public static string GetIdentifierExceptMethodName(IdentifierListNode identList)
        {
            Validity.Assert(null != identList);

            var leftNode = identList.LeftNode;
            var rightNode = identList.RightNode;

            var intermediateNodes = new List<AssociativeNode>();
            if (!(rightNode is FunctionCallNode))
            {
                intermediateNodes.Insert(0, rightNode);
            }

            while (leftNode is IdentifierListNode)
            {
                rightNode = ((IdentifierListNode)leftNode).RightNode;
                if (rightNode is FunctionCallNode)
                {
                    intermediateNodes.Clear();
                }
                else
                {
                    intermediateNodes.Insert(0, rightNode);
                }
                leftNode = ((IdentifierListNode)leftNode).LeftNode;

            }
            if (leftNode is FunctionCallNode)
            {
                intermediateNodes.Clear();
                return "";
            }
            intermediateNodes.Insert(0, leftNode);

            return CreateNodeByCombiningIdentifiers(intermediateNodes).ToString();
        }

        /// <summary>
        /// Retrieves the string format of the identifier list from left to right, leaving out any symbols after the last identifier.
        /// Given: A.B()
        ///     Return: "A"
        /// Given: A.B.C()[0]
        ///     Return: "A.B"
        /// Given: A.B().C
        ///     Return: "A"
        /// Given: A.B[0].C
        ///     Return: "A.B[0].C"
        /// Given: A().B (global function)
        ///     Return: empty string
        /// Given: A.B[0].C()
        ///     Return: "A.B[0]"
        /// </summary>
        /// <param name="identList"></param>
        /// <returns></returns>
        public static string GetIdentifierExceptMethodName(AST.ImperativeAST.IdentifierListNode identList)
        {
            Validity.Assert(null != identList);

            var leftNode = identList.LeftNode;
            var rightNode = identList.RightNode;

            var intermediateNodes = new List<AST.ImperativeAST.ImperativeNode>();
            if (!(rightNode is AST.ImperativeAST.FunctionCallNode))
            {
                intermediateNodes.Insert(0, rightNode);
            }

            while (leftNode is AST.ImperativeAST.IdentifierListNode)
            {
                rightNode = ((AST.ImperativeAST.IdentifierListNode)leftNode).RightNode;
                if (rightNode is AST.ImperativeAST.FunctionCallNode)
                {
                    intermediateNodes.Clear();
                }
                else
                {
                    intermediateNodes.Insert(0, rightNode);
                }
                leftNode = ((AST.ImperativeAST.IdentifierListNode)leftNode).LeftNode;

            }
            if (leftNode is AST.ImperativeAST.FunctionCallNode)
            {
                intermediateNodes.Clear();
                return "";
            }
            intermediateNodes.Insert(0, leftNode);

            return CreateNodeByCombiningIdentifiers(intermediateNodes).ToString();
        }

        /// <summary>
        /// Inspects the input identifier list to match all class names with the class used in it
        /// </summary>
        /// <param name="classTable"></param>
        /// <param name="identifierList">single identifier or identifier list</param>
        /// <returns>list of fully resolved class names</returns>
        public static string[] GetResolvedClassName(ClassTable classTable, AssociativeNode identifierList)
        {
            var identListNode = identifierList as IdentifierListNode;
            var identifierNode = identifierList as IdentifierNode;
            Validity.Assert(identListNode != null || identifierNode != null);

            string partialName = identListNode != null ?
                GetIdentifierStringUntilFirstParenthesis(identListNode) : identifierList.Name;

            string[] classNames = classTable.GetAllMatchingClasses(partialName);

            // Failed to find the first time
            // Attempt to remove identifiers in the identifierlist until we find a class or not
            while (0 == classNames.Length)
            {
                // Move to the left node
                AssociativeNode leftNode = identListNode != null ? identListNode.LeftNode : identifierNode;
                if (leftNode is IdentifierListNode)
                {
                    identListNode = leftNode as IdentifierListNode;
                    classNames = classTable.GetAllMatchingClasses(GetIdentifierStringUntilFirstParenthesis(identListNode));
                }
                if (leftNode is IdentifierNode)
                {
                    classNames = classTable.GetAllMatchingClasses(leftNode.Name);
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
        /// Given a partial class name, get assembly to which the class belongs
        /// </summary>
        /// <param name="classTable"> class table in Core </param>
        /// <param name="className"> class name </param>
        /// <returns> assembly to which the class belongs </returns>
        public static string GetAssemblyFromClassName(ClassTable classTable, string className)
        {
            //throw new NotImplementedException();
            var ci = classTable.IndexOf(className);

            if (ci == DSASM.Constants.kInvalidIndex)
                return string.Empty;

            var classNode = classTable.ClassNodes[ci];
            return classNode.ExternLib;
        }

        /// <summary>
        /// Given a name or string of names, this creates an IdentifierNode or IdentifierListNode
        /// e.g. Creates an IdentifierNode from A and IdentifierListNode from A.B
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AssociativeNode CreateNodeFromString(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string[] strIdentList = name.Split('.');

            if (strIdentList.Length == 1)
            {
                return new IdentifierNode(strIdentList[0]);
            }

            var newIdentList = new IdentifierListNode
            {
                LeftNode = new IdentifierNode(strIdentList[0]),
                RightNode = new IdentifierNode(strIdentList[1]),
                Optr = Operator.dot
            };
            for (var n = 2; n < strIdentList.Length; ++n)
            {
                var subIdentList = new IdentifierListNode
                {
                    LeftNode = newIdentList,
                    RightNode = new IdentifierNode(strIdentList[n]),
                    Optr = Operator.dot
                };
                newIdentList = subIdentList;
            }

            return newIdentList;
        }

        public static AssociativeNode CreateNodeByCombiningIdentifiers(IList<AssociativeNode> nodeList)
        {
            int count = nodeList.Count;
            if (count == 0)
                return null;

            if (count == 1)
            {
                return nodeList[0];
            }

            var newIdentList = new IdentifierListNode
            {
                LeftNode = nodeList[0],
                RightNode = nodeList[1],
                Optr = Operator.dot
            };

            for (var n = 2; n < count; ++n)
            {
                var subIdentList = new IdentifierListNode
                {
                    LeftNode = newIdentList,
                    RightNode = nodeList[n],
                    Optr = Operator.dot
                };
                newIdentList = subIdentList;
            }

            return newIdentList;
        }

        private static AST.ImperativeAST.ImperativeNode CreateNodeByCombiningIdentifiers(IList<AST.ImperativeAST.ImperativeNode> nodeList)
        {
            int count = nodeList.Count;
            if (count == 0)
                return null;

            if (count == 1)
            {
                return nodeList[0];
            }

            var newIdentList = new AST.ImperativeAST.IdentifierListNode
            {
                LeftNode = nodeList[0],
                RightNode = nodeList[1],
                Optr = Operator.dot
            };

            for (var n = 2; n < count; ++n)
            {
                var subIdentList = new AST.ImperativeAST.IdentifierListNode
                {
                    LeftNode = newIdentList,
                    RightNode = nodeList[n],
                    Optr = Operator.dot
                };
                newIdentList = subIdentList;
            }

            return newIdentList;
        }

        /// <summary>
        /// Parses designscript code and outputs ProtoAST
        /// </summary>
        /// <param name="core"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<AssociativeNode> BuildASTList(Core core, string code)
        {
            Validity.Assert(null != core);
            List<AssociativeNode> astList = new List<AssociativeNode>();
            var cbn = ParserUtils.Parse(code) as CodeBlockNode;
            astList.AddRange(cbn.Body);
            return astList;
        }


        /// <summary>
        /// Parses designscript code and outputs ProtoAST
        /// </summary>
        /// <param name="core"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<AssociativeNode> BuildASTList(Core core, List<string> codeList)
        {
            List<AssociativeNode> astList = new List<AssociativeNode>();
            foreach (string code in codeList)
            {
                astList.AddRange(BuildASTList(core, code));
            }
            return astList;
        }

        /// <summary>
        /// Returns the Codeblock given the blockId
        /// </summary>
        /// <param name="blockList"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public static CodeBlock GetCodeBlock(List<CodeBlock> blockList, int blockId)
        {
            CodeBlock codeblock = null;
            codeblock = blockList.Find(x => x.codeBlockId == blockId);
            if (codeblock == null)
            {
                foreach (CodeBlock block in blockList)
                {
                    codeblock = GetCodeBlock(block.children, blockId);
                    if (codeblock != null)
                    {
                        break;
                    }
                }
            }
            return codeblock;
        }
        
        /// <summary>
        /// Returns the CLR object for a given mirror data
        /// </summary>
        public static object GetDataOfValue(Mirror.MirrorData value)
        {
            if (value.IsCollection)
            {
                return value.GetElements().Select(GetDataOfValue).ToList();
            }

            if (!value.IsPointer)
            {
                var data = value.Data;

                if (data != null)
                {
                    return data;
                }
            }
            else if (value.IsDictionary)
            {
                var dict = (DesignScript.Builtin.Dictionary)value.Data;
                return dict.Keys.Zip(dict.Values, (key, val) => new { key, val }).ToDictionary(ns => ns.key, ns => ns.val);
            }

            return value.StringData;
        }

        public static bool IsNonStaticPropertyLookupOnClass(ProcedureNode procCallNode, string className)
        {
            return procCallNode.ArgumentInfos.Count == 1
                   && procCallNode.ArgumentInfos[0].Name == Constants.kThisPointerArgName
                   && procCallNode.ArgumentTypes[0].Name == className;
        }

        public static ProcedureNode GetFunctionByName(string name, CodeBlock codeBlock)
        {
            if (null == codeBlock)
            {
                return null;
            }

            CodeBlock searchBlock = codeBlock;
            while (null != searchBlock)
            {
                if (null == searchBlock.procedureTable)
                {
                    searchBlock = searchBlock.parent;
                    continue;
                }

                // The class table is passed just to check for coercion values
                var procNode = searchBlock.procedureTable.GetFunctionsByName(name).FirstOrDefault();
                if (procNode != null)
                    return procNode;

                searchBlock = searchBlock.parent;
            }
            return null;
        }

        public static ProcedureNode GetFunctionBySignature(string name, List<Type> argTypeList, CodeBlock codeblock)
        {
            if (null == codeblock)
            {
                return null;
            }

            CodeBlock searchBlock = codeblock;
            while (null != searchBlock)
            {
                if (null == searchBlock.procedureTable)
                {
                    searchBlock = searchBlock.parent;
                    continue;
                }

                // The class table is passed just to check for coercion values
                int procIndex = searchBlock.procedureTable.IndexOf(name, argTypeList);
                if (Constants.kInvalidIndex != procIndex)
                {
                    return searchBlock.procedureTable.Procedures[procIndex];
                }
                searchBlock = searchBlock.parent;
            }
            return null;
        }

        /// <summary>
        /// Performs addition on 2 StackValues
        /// This is used by the VM when adding strings
        /// </summary>
        /// <param name="sv1"></param>
        /// <param name="sv2"></param>
        /// <returns></returns>
        public static StackValue AddStackValueString(StackValue sv1, StackValue sv2, RuntimeCore runtimeCore)
        {
            Validity.Assert(sv1.IsString || sv2.IsString);

            if (sv1.IsString && sv2.IsString)
            {
                return StringUtils.ConcatString(sv2, sv1, runtimeCore);
            }
            else if (sv1.IsString || sv2.IsString)
            {
                StackValue newSV;
                if (sv1.IsNull || sv2.IsNull)
                {
                    return StackValue.BuildNull();
                }
                else if (sv1.IsString)
                {
                    newSV = StringUtils.ConvertToString(sv2, runtimeCore, runtimeCore.RuntimeMemory);
                    return StringUtils.ConcatString(newSV, sv1, runtimeCore);
                }
                else if (sv2.IsString)
                {
                    newSV = StringUtils.ConvertToString(sv1, runtimeCore, runtimeCore.RuntimeMemory);
                    return StringUtils.ConcatString(sv2, newSV, runtimeCore);
                }
            }
            return StackValue.BuildNull();
        }
    }
}