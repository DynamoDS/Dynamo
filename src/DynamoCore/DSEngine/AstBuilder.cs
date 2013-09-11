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
    /// A linked list of list (each node in linked list is a list), and node 
    /// can be accessed through a key. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LinkedListOfList<Key, T> : IEnumerable<List<T>>
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

        public void Clears(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                listNode.Value.Clear();
            }
        }

        public void Removes(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                map.Remove(key);
                list.Remove(listNode);
            }
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
    /// Generate ast nodes
    /// </summary>
    public interface IAstBuilder
    {
        AssociativeNode Build(NodeModel node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.DSFunction node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs);
    }

    /// <summary>
    /// AstBuilder is a factory class to create different kinds of AST nodes.
    /// 
    /// Internally it keeps a mapping between Dynamo node and the corresponding
    /// DesignScript AST nodes. 
    /// </summary>
    public class AstBuilder : IAstBuilder
    {
        internal class StringConstants
        {
            public const string kParamPrefix = @"p_";
            public const string kFunctionPrefix = @"func_";
            public const string kVarPrefix = @"var_";
        }

        public static AstBuilder Instance = new AstBuilder();
        private LinkedListOfList<Guid, AssociativeNode> astNodes;

        private AstBuilder()
        {
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

        public void RemoveAstNodes(Guid dynamoNodeId)
        {
            astNodes.Removes(dynamoNodeId); 
        }

        public void ClearAstNodes(Guid dynamoNodeId)
        {
            astNodes.Clears(dynamoNodeId);
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

        #region IAstBuilder interface
        public AssociativeNode Build(NodeModel node, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildNullNode();
        }

        public AssociativeNode Build(DSFunction node, List<AssociativeNode> inputs)
        {
            string function = node.FunctionName;
            return AstFactory.BuildFunctionCall(function, inputs);
        }

        public AssociativeNode Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildDoubleNode(node.Value);
        }

        public AssociativeNode Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildBooleanNode(node.Value);
        }

        public AssociativeNode Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs)
        {
            return AstFactory.BuildStringNode(node.Value);
        }
        #endregion

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
        private FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
        {
            List<VarDeclNode> partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            for (int i = 0; i < func.FormalArguments.Count; ++i)
            {
                if (func.FormalArguments[i] == null)
                {
                    VarDeclNode param = AstFactory.BuildParamNode(AstBuilder.StringConstants.kParamPrefix + paramPostfix);
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
                var lhs = AstFactory.BuildIdentifier(ProtoCore.DSDefinitions.Keyword.Return);
                var rhs = AstFactory.BuildFunctionCall(func.Function.Name, func.FormalArguments);
                var returnStmt = AstFactory.BuildAssignment(lhs, rhs);
                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode partialFunc = new FunctionDefinitionNode();
            partialFunc.IsExternLib = false;
            partialFunc.IsDNI = false;
            partialFunc.ExternLibName = null;
            partialFunc.Name = StringConstants.kFunctionPrefix + Guid.NewGuid().ToString().Replace("-", string.Empty);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        public void BuildEvaluation(NodeModel node, AssociativeNode rhs, bool isPartial = false)
        {
            // If it is a partially applied function, need to create a function 
            // definition and function pointer.
            if (isPartial && rhs is FunctionCallNode)
            {
                FunctionCallNode funcCall = rhs as FunctionCallNode;
                // create a function definition for it
                var newFunc = BuildPartialFunction(funcCall);
                AddNode(node.GUID, newFunc);

                // create a function pointer for this node
                rhs = AstFactory.BuildIdentifier(newFunc.Name);
            }

            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }
    }
}
