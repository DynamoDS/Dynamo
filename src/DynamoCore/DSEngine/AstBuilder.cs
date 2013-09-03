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
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace Dynamo.DSEngine
{
    public class NodeEvaluator
    {
        private NodeWithOneOutput _node;

        public NodeEvaluator(NodeModel node)
        {
            Debug.Assert(node is NodeWithOneOutput);
            _node = node as NodeWithOneOutput;
        }

        public object Evaluate(List<object> objs)
        {
            // TODO: we need to do marhsalling and unmarhslling here.
            //
            // The input could be normal object as well as FScheme.Value.
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
            FScheme.Value result = _node.Evaluate(fargs);

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
    /// A linked list of list. The node can be accessed throuh a key. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LinkedListOfList<Key, T>: IEnumerable<List<T>>
    {
        private Dictionary<Key, LinkedListNode<List<T>>> NodeMap; 
        private LinkedList<List<T>> NodeList;

        public LinkedListOfList()
        {
            NodeMap = new Dictionary<Key, LinkedListNode<List<T>>>();
            NodeList = new LinkedList<List<T>>();
        }

        public void AddItem(Key key, T item)
        {
            LinkedListNode<List<T>> node;
            if (!NodeMap.TryGetValue(key, out node))
            {
                node = new LinkedListNode<List<T>>(new List<T>());
                NodeList.AddLast(node);
                NodeMap[key] = node;
            }
            node.Value.Add(item);
        }

        public bool Contains(Key key)
        {
            return NodeMap.ContainsKey(key); 
        }

        public List<T> GetItems(Key key)
        {
            LinkedListNode<List<T>> node;
            if (!NodeMap.TryGetValue(key, out node))
            {
                return null;
            }

            return node.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return NodeList.GetEnumerator();
        }

        IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        {
            return NodeList.GetEnumerator();
        }
    }

    public class AstBuilder
    {
        public Dictionary<string, object> EvalContext;
        private LinkedListOfList<Guid, AssociativeNode> AstNodes;
        private LinkedListOfList<Guid, System.String> Sources;

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
            EvalContext = new Dictionary<string, object>();
            AstNodes = new LinkedListOfList<Guid, AssociativeNode>();
            Sources = new LinkedListOfList<Guid, string>();
        }

        public void AddNode(Guid dynamoNodeId, AssociativeNode astNode)
        {
            AstNodes.AddItem(dynamoNodeId, astNode);
        }

        public bool ContainsAstNodes(Guid dynamoNodeId)
        {
            return AstNodes.Contains(dynamoNodeId);
        }

        public string GenerateSourceCode()
        {
            List<AssociativeNode> allAstNodes = new List<AssociativeNode>();
            foreach (var item in AstNodes)
            {
                allAstNodes.AddRange(item);
            }
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(allAstNodes);
            return codegen.GenerateCode();
        }

        public BinaryExpressionNode BuildBinaryExpression(AssociativeNode lhs,
                                                          AssociativeNode rhs,
                                                          Operator op)
        {
            BinaryExpressionNode node = new BinaryExpressionNode();
            node.LeftNode = lhs;
            node.RightNode = rhs;
            node.Optr = op;
            return node;
        }

        public FunctionCallNode BuildFunctionCall(string function, 
                                                  List<AssociativeNode> arguments)
        {
            FunctionCallNode funcCall = new FunctionCallNode();
            funcCall.Function = BuildIdentifier(function);
            funcCall.FormalArguments = arguments;

            return funcCall;
        }

        public IdentifierNode BuildIdentifier(string name)
        {
            IdentifierNode identifier = new IdentifierNode();
            identifier.Name = identifier.Value = name;

            return identifier;
        }

        public ExprListNode BuildExprList(List<AssociativeNode> nodes)
        {
            ExprListNode exprList = new ExprListNode();
            exprList.list = nodes;
            return exprList;
        }

        public BinaryExpressionNode BuildAssignment(AssociativeNode lhs, 
                                                    AssociativeNode rhs)
        {
            BinaryExpressionNode assignment = new BinaryExpressionNode();
            assignment.LeftNode = lhs;
            assignment.RightNode = rhs;
            assignment.Optr = ProtoCore.DSASM.Operator.assign;

            return assignment;
        }

        protected VarDeclNode BuildParamNode(string paramName)
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
        public FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
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

        public FunctionDefinitionNode BuildPartilFunctionForBinaryOp(BinaryExpressionNode expr)
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
        /// <param name="node"></param>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public FunctionCallNode BuildEvaluator(NodeModel node, 
                                               List<AssociativeNode> inputAstNodes)
        {
            // Here we'll create a function defintion which redirect 
            // DesignScript function call back to the node's own implementation 
            // of Evaluate().
            string evaluator = NamingUtil.NewUniqueName(StringConstants.kEvalFunctionPrefix);
            this.EvalContext.Add(evaluator, new NodeEvaluator(node));

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
                FunctionCallNode eval = BuildFunctionCall(StringConstants.kEvalFunction, evaluateArgs);
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
            AddNode(node.GUID, evalFunc);

            // Now make a call to this wrapper function
            return BuildFunctionCall(evalFunc.Name, inputAstNodes);
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
