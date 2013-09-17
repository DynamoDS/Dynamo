using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using ProtoCore;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Exceptions;
using ProtoCore.Lang;
using ProtoCore.Utils;
using ProtoFFI;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// If a Dynamo node doesn't override CompileToAstNodeInternal(), we 
    /// register an instance of NodeEvaluator in DesignScript's context so that 
    /// when the Dynamo node is evaluated, NodeEvaluator.Evaluate() will be 
    /// inovked.  
    ///
    /// This class won't be needed if we completely repalce FScheme evaluation 
    /// engine with DesignScript engine.
    /// </summary>
    public class NodeEvaluator
    {
        private NodeWithOneOutput node;

        public NodeEvaluator(NodeModel dynNode)
        {
            Debug.Assert(dynNode is NodeWithOneOutput);
            node = dynNode as NodeWithOneOutput;
        }

        public object Evaluate(List<object> objs)
        {
            // TODO: we need to do marhsalling and unmarhslling here.
            //
            // The output should be converted to FScheme.Value and will be 
            // marshalled or unmarshalled (depend on it is an input of custom
            // node, which only accepts FScheme.Value typed input, and an input
            // of normal node, which has been converted to DesignScript
            // function) by DesignScript FFI. 
            //
            // Let's assume that this list only contains numbers for testing.
            // And assume that returns a double value. 
            List<FScheme.Value> args = objs.Select(obj => FScheme.Value.NewNumber(Convert.ToDouble(obj))).ToList();
            FSharpList<FScheme.Value> fargs = Utils.SequenceToFSharpList<FScheme.Value>(args);
            FScheme.Value result = node.Evaluate(fargs);

            if (result.IsNumber)
            {
                return (result as FScheme.Value.Number).Item;
            }
            else if (result.IsContainer)
            {
                return (result as FScheme.Value.Container).Item;
            }

            return result;
        }
    }

    /// <summary>
    /// A linked list of list (each node in linked list is a list), and node 
    /// can be accessed through a key. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LinkedListOfList<Key, T>: IEnumerable<List<T>>
    {
        private Dictionary<Key, LinkedListNode<List<T>>> map; 
        private LinkedList<List<T>> list;

        public LinkedListOfList()
        {
            map = new Dictionary<Key, LinkedListNode<List<T>>>();
            list = new LinkedList<List<T>>();
        }

        public void AddItem(Key key, T item)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode))
            {
                listNode = new LinkedListNode<List<T>>(new List<T>());
                list.AddLast(listNode);
                map[key] = listNode;
            }
            listNode.Value.Add(item);
        }

        public bool Contains(Key key)
        {
            return map.ContainsKey(key); 
        }

        public List<T> GetItems(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode))
            {
                return null;
            }

            return listNode.Value;
        }

        public List<Key> GetKeys()
        {
            return new List<Key>(map.Keys);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    /// <summary>
    /// AstBuilder is a helper class to create different kinds of DesignScript 
    /// node like function call, expression, and so on.
    /// 
    /// Internally it keeps a mapping between Dynamo node and the corresponding
    /// DesignScript AST nodes. 
    /// </summary>
    public class AstBuilder
    {
        public Dictionary<string, object> evalContext;
        private LinkedListOfList<Guid, AssociativeNode> astNodes;

        internal class StringConstants
        {
            public const string kEvalFunction = @"Evaluate";
            public const string kEvalFunctionPrefix = @"eval_";
            public const string kParamPrefix = @"p_";
            public const string kFunctionPrefix = @"func_";
            public const string kVarPrefix = @"var_";
        }

        internal class NamingUtil
        {
            public static string NewUniqueName(string prefix = null, 
                                               string postfix = null)
            {
                return prefix + 
                       Guid.NewGuid().ToString().Replace("-", string.Empty) +
                       postfix;
            }
        }

        public AstBuilder()
        {
            evalContext = new Dictionary<string, object>();
            astNodes = new LinkedListOfList<Guid, AssociativeNode>();
        }

        public void AddNode(Guid dynamoNodeId, AssociativeNode astNode)
        {
            astNodes.AddItem(dynamoNodeId, astNode);
        }

        /// <summary>
        /// If AstBuilder has generated AST node for this Dynamo node
        /// </summary>
        /// <param name="dynamoNodeId"></param>
        /// <returns></returns>
        public bool ContainsAstNodes(Guid dynamoNodeId)
        {
            return astNodes.Contains(dynamoNodeId);
        }

#if DEBUG
        /// <summary>
        /// Dump DesignScript code from AST nodes, just for testing
        /// </summary>
        /// <returns></returns>
        public string DumpCode()
        {
            List<AssociativeNode> allAstNodes = new List<AssociativeNode>();
            foreach (var item in astNodes)
            {
                allAstNodes.AddRange(item);
            }
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(allAstNodes);
            return codegen.GenerateCode();
        }

        /// <summary>
        /// Execute AST nodes, just for testing. 
        /// </summary>
        public void Execute()
        {
            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Options.Verbose = true;
            core.RuntimeStatus.MessageHandler = new ConsoleOutputStream(); 
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            List<AssociativeNode> allAstNodes = new List<AssociativeNode>();
            foreach (var item in astNodes)
            {
                allAstNodes.AddRange(item);
            }

            DynamoLogger logger = DynamoLogger.Instance;
            try
            {
                ExecutionMirror mirror = runner.Execute(allAstNodes, core);
                List<Guid> keys = astNodes.GetKeys();
                foreach (var guid in keys)
                {
                    string varname = StringConstants.kVarPrefix + guid.ToString().Replace("-", string.Empty);
                    Obj o = mirror.GetValue(varname);
                    string value = mirror.GetStringValue(o.DsasmValue, core.Heap, 0, true);
                    logger.Log(varname + "=" + value);
                }
            }
            catch (CompileErrorsOccured e)
            {
                logger.Log(e.Message);
            }
        }
#endif

        public static NullNode BuildNullNode()
        {
            return new NullNode();
        }

        public static IntNode BuildIntNode(int value)
        {
            IntNode node = new IntNode();
            node.value = value.ToString();
            return node;
        }

        public static DoubleNode BuildDoubleNode(double value)
        {
            DoubleNode node = new DoubleNode();
            node.value = value.ToString();
            return node;
        }

        public static StringNode BuildStringNode(string str)
        {
            return new StringNode { value = str };
        }

        public static BooleanNode BuildBooleanNode(bool value)
        {
            string strValue = value ? "true" : "false";
            return new BooleanNode { value = strValue };
        }

        public static UnaryExpressionNode BuildUnaryExpression(AssociativeNode expression,
                                                               UnaryOperator uop)
        {
            UnaryExpressionNode node = new UnaryExpressionNode();
            node.Expression = expression;
            node.Operator = uop;
            return node;
        }

        public static BinaryExpressionNode BuildBinaryExpression(AssociativeNode lhs,
                                                                 AssociativeNode rhs,
                                                                 Operator op)
        {
            BinaryExpressionNode node = new BinaryExpressionNode();
            node.LeftNode = lhs;
            node.RightNode = rhs;
            node.Optr = op;
            return node;
        }

        public static InlineConditionalNode BuildConditionalNode(AssociativeNode condition,
                                                                 AssociativeNode trueExpr,
                                                                 AssociativeNode falseExpr)
        {
            InlineConditionalNode cond = new InlineConditionalNode();
            cond.ConditionExpression = condition;
            cond.TrueExpression = trueExpr;
            cond.FalseExpression = falseExpr;
            return cond;
        }

        public static AssociativeNode BuildFunctionCall(string function, 
                                                        List<AssociativeNode> arguments)
        {
            string[] dotcalls = function.Split('.');
            string functionName = dotcalls[dotcalls.Length - 1];

            FunctionCallNode funcCall = new FunctionCallNode();
            funcCall.Function = BuildIdentifier(functionName);
            funcCall.FormalArguments = arguments;
 
            if (dotcalls.Length == 1)
            {
               return funcCall;
            }
            else 
            {
                IdentifierNode lhs = BuildIdentifier(dotcalls[0]);
                return CoreUtils.GenerateCallDotNode(lhs, funcCall);
            }
        }

        public static IdentifierNode BuildIdentifier(string name)
        {
            IdentifierNode identifier = new IdentifierNode();
            identifier.Name = identifier.Value = name;

            return identifier;
        }

        public static ExprListNode BuildExprList(List<AssociativeNode> nodes)
        {
            ExprListNode exprList = new ExprListNode();
            exprList.list = nodes;
            return exprList;
        }

        public static ExprListNode BuildExprList(List<string> exprs)
        {
            List<AssociativeNode> nodes = new List<AssociativeNode>();
            foreach (var item in exprs)
            {
                nodes.Add(BuildIdentifier(item)); 
            }
            return BuildExprList(nodes);
        }

        public static BinaryExpressionNode BuildAssignment(AssociativeNode lhs, 
                                                           AssociativeNode rhs)
        {
            BinaryExpressionNode assignment = new BinaryExpressionNode();
            assignment.LeftNode = lhs;
            assignment.RightNode = rhs;
            assignment.Optr = ProtoCore.DSASM.Operator.assign;

            return assignment;
        }

        public static VarDeclNode BuildParamNode(string paramName)
        {
            VarDeclNode param = new VarDeclNode();
            param.NameNode = BuildIdentifier(paramName);

            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            type.Name = "var";
            param.ArgumentType = type;

            return param;
        }

        /// <summary>
        /// Create a function defintion for a partially applied function call. 
        /// E.g.
        /// 
        ///     foo(?, x, y, ?, z);
        ///     
        ///  ->
        ///
        ///     def foo_guid(p_1, p_2) = foo(p_1, x, y, p_2, z);
        ///     
        /// </summary>
        /// <param name="func"></param>
        /// </param>
        /// <returns></returns>
        public static FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
        {
            List<VarDeclNode> partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            for (int i = 0; i < func.FormalArguments.Count; ++i)
            {
                if (func.FormalArguments[i] == null)
                {
                    VarDeclNode param = BuildParamNode(StringConstants.kParamPrefix + paramPostfix);
                    partialArgs.Add(param);

                    func.FormalArguments[i] = param.NameNode;
                    paramPostfix++;
                }
            }

            // It is not a partial function call. 
            if (paramPostfix == 0)
            {
                return null;
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                var lhs = BuildIdentifier(ProtoCore.DSDefinitions.Kw.kw_return);
                var rhs = BuildFunctionCall(func.Function.Name, func.FormalArguments);
                var returnStmt = BuildAssignment(lhs, rhs);
                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode partialFunc = new FunctionDefinitionNode();
            partialFunc.IsExternLib = false;
            partialFunc.IsDNI = false;
            partialFunc.ExternLibName = null;
            partialFunc.Name = NamingUtil.NewUniqueName(StringConstants.kFunctionPrefix);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        /// <summary>
        /// Binary expression in DesignScript internally will be converted to
        /// a function call, so need to create a function defintion together
        /// with a function call if any operand is missing in binary expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static FunctionDefinitionNode BuildPartilFunctionForBinaryOp(BinaryExpressionNode expr)
        {
            List<VarDeclNode> partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            if (expr.LeftNode == null)
            {
                VarDeclNode param = BuildParamNode(StringConstants.kParamPrefix + paramPostfix);
                partialArgs.Add(param);
                expr.LeftNode = param.NameNode;
                paramPostfix++;
            }

            if (expr.RightNode == null)
            {
                VarDeclNode param = BuildParamNode(StringConstants.kParamPrefix + paramPostfix);
                partialArgs.Add(param);
                expr.RightNode = param.NameNode;
                paramPostfix++;
            }

            // It is not a partial function call. 
            if (paramPostfix == 0)
            {
                return null;
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                var lhs = BuildIdentifier(ProtoCore.DSDefinitions.Kw.kw_return);
                var returnStmt = BuildAssignment(lhs, expr);
                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode partialFunc = new FunctionDefinitionNode();
            partialFunc.IsExternLib = false;
            partialFunc.IsDNI = false;
            partialFunc.ExternLibName = null;
            partialFunc.Name = NamingUtil.NewUniqueName(StringConstants.kFunctionPrefix);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        /// <summary>
        /// Create a evaluation function defition node:
        /// 
        ///     def func_guid(p_1, p_2, p_3)
        ///     {
        ///         args = {p_1, p_2, p_3};
        ///         return = eval_guid.Evaluate(args);
        ///     }
        ///
        /// And create a function call to func_guid().
        /// 
        /// </summary>
        /// <param name="bulder"></param>
        /// <param name="node"></param>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public static FunctionCallNode BuildEvaluator(AstBuilder builder,
                                                      NodeModel node, 
                                                      List<AssociativeNode> inputAstNodes)
        {
            // Here we'll create a function defintion which redirect 
            // DesignScript function call back to the node's own implementation 
            // of Evaluate().
            string evaluator = NamingUtil.NewUniqueName(StringConstants.kEvalFunctionPrefix);
            builder.evalContext.Add(evaluator, new NodeEvaluator(node));

            List<VarDeclNode> arguments = new List<VarDeclNode>();
            for (int i = 0; i < inputAstNodes.Count; ++i)
            {
                VarDeclNode argument = BuildParamNode(StringConstants.kParamPrefix + i);
                arguments.Add(argument);
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                // args = {p_1, p_2, p_3};
                IdentifierNode argsVar = BuildIdentifier("args");
                ExprListNode argList = new ExprListNode();
                argList.list = arguments.Select(a => a.NameNode).ToList();
                BinaryExpressionNode assignment = BuildAssignment(argsVar, argList);
                funcBody.Body.Add(assignment);

                // return = e_guid.Evaluate(args);
                List<AssociativeNode> evaluateArgs = new List<AssociativeNode>();
                evaluateArgs.Add(argsVar);

                IdentifierNode evalInstance = BuildIdentifier(evaluator);
                FunctionCallNode eval = BuildFunctionCall(StringConstants.kEvalFunction, evaluateArgs) as FunctionCallNode;
                FunctionDotCallNode evalCall = CoreUtils.GenerateCallDotNode(evalInstance, eval, null);

                IdentifierNode ret = BuildIdentifier(ProtoCore.DSDefinitions.Kw.kw_return);
                BinaryExpressionNode returnStmt = BuildAssignment(ret, evalCall);

                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode evalFunc = new FunctionDefinitionNode();
            evalFunc.IsExternLib = false;
            evalFunc.IsDNI = false;
            evalFunc.ExternLibName = null;
            evalFunc.Name = NamingUtil.NewUniqueName(StringConstants.kFunctionPrefix);
            evalFunc.Singnature = new ArgumentSignatureNode();
            evalFunc.Singnature.Arguments = arguments;
            evalFunc.FunctionBody = funcBody;
            builder.AddNode(node.GUID, evalFunc);

            // Now make a call to this wrapper function
            return BuildFunctionCall(evalFunc.Name, inputAstNodes)  as FunctionCallNode;
        }

        public void BuildEvaluation(NodeModel node, 
                                    AssociativeNode rhs, 
                                    bool isPartial = false)
        {
            // If it is a partially applied function, need to create a function 
            // definition and function pointer.
            if (isPartial)
            {
                if (rhs is FunctionCallNode)
                {
                    FunctionCallNode funcCall = rhs as FunctionCallNode;
                    // create a function definition for it
                    var newFunc = BuildPartialFunction(funcCall);
                    AddNode(node.GUID, newFunc);

                    // create a function pointer for this node
                    rhs = BuildIdentifier(newFunc.Name);
                }
                else if (rhs is BinaryExpressionNode)
                {
                    BinaryExpressionNode expr = rhs as BinaryExpressionNode;
                    if (expr.Optr != Operator.assign)
                    {
                        var newFunc = BuildPartilFunctionForBinaryOp(expr);
                        AddNode(node.GUID, newFunc);
                        rhs = BuildIdentifier(newFunc.Name);
                    }
                }
            }

            var assignment = BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }
    }
}
