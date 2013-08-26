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
    public class dynNodeEvalutor
    {
        private dynNodeWithOneOutput _node;

        public dynNodeEvalutor(dynNodeModel node)
        {
            Debug.Assert(node is dynNodeWithOneOutput);
            _node = node as dynNodeWithOneOutput;
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
    internal class dynLinkedListOfList<Key, T>: IEnumerable<List<T>>
    {
        private Dictionary<Key, LinkedListNode<List<T>>> NodeMap; 
        private LinkedList<List<T>> NodeList;

        public dynLinkedListOfList()
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

    public class dynAstBuilder
    {
        public Dictionary<string, object> EvalContext;
        private dynLinkedListOfList<Guid, AssociativeNode> AstNodes;
        private dynLinkedListOfList<Guid, String> Sources;

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

        public dynAstBuilder()
        {
            EvalContext = new Dictionary<string, object>();
            AstNodes = new dynLinkedListOfList<Guid, AssociativeNode>();
            Sources = new dynLinkedListOfList<Guid, string>();
        }

        public void AddNode(Guid dynamoNodeId, AssociativeNode astNode)
        {
            AstNodes.AddItem(dynamoNodeId, astNode);
        }

        public bool ContainsAstNodes(Guid dynamoNodeId)
        {
            return AstNodes.Contains(dynamoNodeId);
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

        public BinaryExpressionNode BuildAssignment(AssociativeNode lhs, 
                                                    AssociativeNode rhs)
        {
            BinaryExpressionNode assignment = new BinaryExpressionNode();
            assignment.LeftNode = lhs;
            assignment.RightNode = rhs;
            assignment.Optr = ProtoCore.DSASM.Operator.assign;

            return assignment;
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
                    VarDeclNode param = new VarDeclNode();
                    param.NameNode = BuildIdentifier(StringConstants.kParamPrefix + paramPostfix);
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
        /// <param name="dynNode"></param>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public FunctionCallNode BuildEvaluator(dynNodeModel dynNode, 
                                               List<AssociativeNode> inputAstNodes)
        {
            // Here we'll create a function defintion which redirect 
            // DesignScript function call back to the node's own implementation 
            // of Evaluate().
            string evaluator = NamingUtil.NewUniqueName(StringConstants.kEvalFunctionPrefix);
            this.EvalContext.Add(evaluator, new dynNodeEvalutor(dynNode));

            List<VarDeclNode> arguments = new List<VarDeclNode>();
            for (int i = 0; i < inputAstNodes.Count; ++i)
            {
                VarDeclNode argument = new VarDeclNode();
                string argumentName = StringConstants.kParamPrefix + i;
                argument.NameNode = BuildIdentifier(argumentName);
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
            AddNode(dynNode.GUID, evalFunc);

            // Now make a call to this wrapper function
            return BuildFunctionCall(evalFunc.Name, inputAstNodes);
        }

        public void BuildEvaluation(dynNodeModel dynNode, 
                                    AssociativeNode rhs, 
                                    bool isPartial = false)
        {
            // If it is a partially applied function, need to create a function 
            // definition and function pointer.
            if (isPartial)
            {
                FunctionCallNode funcCall = rhs as FunctionCallNode;
                if (funcCall != null)
                {
                    // create a function definition for it
                    var newFunc = BuildPartialFunction(funcCall);
                    AddNode(dynNode.GUID, newFunc);

                    // create a function pointer for this node
                    rhs = BuildIdentifier(newFunc.Name);
                }
            }

            var assignment = BuildAssignment(dynNode.AstIdentifier, rhs);
            AddNode(dynNode.GUID, assignment);
        }
    }
}
