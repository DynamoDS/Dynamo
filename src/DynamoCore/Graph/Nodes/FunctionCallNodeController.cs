using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Logging;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    ///     Controller for nodes that act as function calls.
    /// </summary>
    public abstract class FunctionCallNodeController<T> : LogSourceBase 
        where T : IFunctionDescriptor
    {
        //private T definition;

        /// <summary>
        ///     A FunctionDescriptor describing the function that this controller will call.
        /// </summary>
        public T Definition { get; set; }

        /// <summary>
        ///     Name for nodes using this controller, based on the underlying FunctionDescriptor.
        /// </summary>
        public string Name 
        {
            get
            {
                return Definition.DisplayName;
            } 
        }

        /// <summary>
        ///     ReturnKeys for multi-output functions.
        /// </summary>
        public virtual IEnumerable<string> ReturnKeys { get { return Definition.ReturnKeys; } }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        protected FunctionCallNodeController(T definition)
        {
            Definition = definition;
        }

        /// <summary>
        ///     Produces AST for a function call. Takes into account multi-outputs and partial application.
        /// </summary>
        /// <param name="model">NodeModel to produce an AST for.</param>
        /// <param name="inputAstNodes">Arguments to the function call.</param>
        public IEnumerable<AssociativeNode> BuildAst(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();
            BuildOutputAst(model, inputAstNodes, resultAst);
            return resultAst;
        }

        /// <summary>
        ///     Produces AST for a partial function application of a multi-output function.
        /// </summary>
        /// <param name="model">NodeModel to produce AST for.</param>
        /// <param name="rhs">AST representing the partial application. This will need to be used to assign all output port identifiers.</param>
        /// <param name="resultAst">Result accumulator: add all new output AST to this list.</param>
        protected virtual void BuildAstForPartialMultiOutput(
            NodeModel model, AssociativeNode rhs, List<AssociativeNode> resultAst)
        {
            var missingAmt =
                Enumerable.Range(0, model.InPorts.Count).Count(x => !model.InPorts[x].IsConnected);
            var tmp = model.AstIdentifierForPreview;
            resultAst.Add(AstFactory.BuildAssignment(tmp, rhs));
            resultAst.AddRange(
                (Definition.ReturnKeys ?? Enumerable.Empty<string>()).Select(
                    AstFactory.BuildStringNode)
                    .Select(
                        (rtnKey, index) =>
                            AstFactory.BuildAssignment(
                                model.GetAstIdentifierForOutputIndex(index),
                                AstFactory.BuildFunctionObject(
                                    "__ComposeBuffered",
                                    3,
                                    new[] { 0, 1 },
                                    new List<AssociativeNode>
                                    {
                                        AstFactory.BuildExprList(
                                            new List<AssociativeNode>
                                            {
                                                AstFactory.BuildFunctionObject(
                                                    "__GetOutput",
                                                    2,
                                                    new[] { 1 },
                                                    new List<AssociativeNode>
                                                    {
                                                        AstFactory.BuildNullNode(),
                                                        rtnKey
                                                    }),
                                                tmp
                                            }),
                                        AstFactory.BuildIntNode(missingAmt),
                                        AstFactory.BuildNullNode()
                                    }))));
        }

        private static readonly string BuiltinDictionaryTypeName = typeof(DesignScript.Builtin.Dictionary).FullName;
        private static readonly string BuiltinDictionaryGet = nameof(DesignScript.Builtin.Dictionary.ValueAtKey);

        /// <summary>
        ///     Produces AST that assigns all necessary Identifiers for the given NodeModel from
        ///     the produced function call AST.
        /// </summary>
        /// <param name="model">Model to produce AST for.</param>
        /// <param name="rhs">AST for the function call. This will need to be used to assign all output port identifiers.</param>
        /// <param name="resultAst">Result accumulator: add all new output AST to this list.</param>
        protected virtual void AssignIdentifiersForFunctionCall(NodeModel model, AssociativeNode rhs, List<AssociativeNode> resultAst)
        {
            resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));

            var keys = Definition.ReturnKeys ?? Enumerable.Empty<string>();
            resultAst.AddRange(
                from item in keys.Zip(Enumerable.Range(0, keys.Count()), (key, idx) => new { key, idx })
                let outputIdentiferNode = model.GetAstIdentifierForOutputIndex(item.idx)
                let outputIdentifier = outputIdentiferNode.ToString()
                let getValueCall = AstFactory.BuildFunctionCall(BuiltinDictionaryTypeName, BuiltinDictionaryGet,
                    new List<AssociativeNode> { model.AstIdentifierForPreview, AstFactory.BuildStringNode(item.key) })
                select
                AstFactory.BuildAssignment(outputIdentiferNode, getValueCall));
        }

        /// <summary>
        ///     Produces AST for the given NodeModel that will call the underlying
        ///     Function and assign all Identifiers for the node.
        /// </summary>
        /// <param name="model">NodeModel to produce AST for.</param>
        /// <param name="inputAstNodes">Arguments to the function call.</param>
        /// <param name="resultAst">Result accumulator: add all new output AST to this list.</param>
        protected virtual void BuildOutputAst(
            NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            AssociativeNode rhs = GetFunctionApplication(model, inputAstNodes);

            if (!model.IsPartiallyApplied || model.OutPorts.Count == 1)
                AssignIdentifiersForFunctionCall(model, rhs, resultAst);
            else
                BuildAstForPartialMultiOutput(model, rhs, resultAst);
        }

        /// <summary>
        ///     Initialize all input ports on the given node based on the underlying
        ///     function.
        /// </summary>
        /// <param name="model">Node to initialize.</param>
        protected abstract void InitializeInputs(NodeModel model);

        /// <summary>
        ///     Initialize all output ports on the given node based on the underlying
        ///     function.
        /// </summary>
        /// <param name="model">Node to initialize.</param>
        protected abstract void InitializeOutputs(NodeModel model);

        /// <summary>
        ///     Produces AST representing a function application for the given NodeModel, using the
        ///     given arguments. This should not assign any of the node's identifiers.
        /// </summary>
        /// <param name="model">Node to produce a function application for.</param>
        /// <param name="inputAstNodes">Arguments to the function application.</param>
        protected abstract AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes);
        
        /// <summary>
        ///     Deserializes Controller information from XML.
        /// </summary>
        public virtual void DeserializeCore(XmlElement element, SaveContext context) { }

        /// <summary>
        ///     Serializes Controller information from XML.
        /// </summary>
        public virtual void SerializeCore(XmlElement element, SaveContext context) { }

        /// <summary>
        ///     Synchronizes a node with this controller, based on the underlying function.
        /// </summary>
        /// <param name="model">Node to sync.</param>
        public virtual void SyncNodeWithDefinition(NodeModel model)
        {
            if (Definition == null) return;

            OnSyncWithDefinitionStart(model);
            InitializeInputs(model);
            InitializeOutputs(model);
            model.RegisterAllPorts();
            model.Name = Name;
            OnSyncWithDefintionEnd(model);
        }

        /// <summary>
        /// Event handler for the event when node starts syncing with its 
        /// definition.
        /// </summary>
        public event Action<NodeModel> SyncWithDefinitionStart;
        /// <summary>
        /// Start syncing with its definition.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnSyncWithDefinitionStart(NodeModel obj)
        {
            var handler = SyncWithDefinitionStart;
            if (handler != null) handler(obj);
        }


        /// <summary>
        /// Event handler for the event when node finishes syncing with its
        /// definition.
        /// </summary>
        public event Action<NodeModel> SyncWithDefinitionEnd;
        /// <summary>
        /// Finish syncing with its definition.
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnSyncWithDefintionEnd(NodeModel obj)
        {
            var handler = SyncWithDefinitionEnd;
            if (handler != null) handler(obj);
        }
    }
}