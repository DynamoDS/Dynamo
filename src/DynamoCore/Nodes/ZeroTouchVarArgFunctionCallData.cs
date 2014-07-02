using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    public class ZeroTouchVarArgFunctionCallData : ZeroTouchFunctionCallData
    {
        public ZeroTouchVarArgFunctionCallData(FunctionDescriptor zeroTouchDef) : base(zeroTouchDef) { }

        public override void InitializeNode(NodeModel model)
        {
            VarInputController = new ZeroTouchVarInputController(this, model);
            base.InitializeNode(model);
        }

        protected override void InitializeParameters(NodeModel model)
        {
            foreach (var arg in Definition.Parameters.Take(Definition.Parameters.Count() - 1))
            {
                model.InPortData.Add(
                    new PortData(
                        arg.Name,
                        string.IsNullOrEmpty(arg.Type) ? "var" : arg.Type,
                        arg.DefaultValue));
            }
            VarInputController.AddInputToModel();
        }

        public override void SetupNodeUI(dynNodeView view)
        {
            VarInputController.SetupNodeUI(view);
        }

        public override void Save(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.Save(xmlDoc, nodeElement, context);
            VarInputController.SaveNode(xmlDoc, nodeElement, context);
        }

        protected override void Load(XmlNode nodeElement)
        {
            base.Load(nodeElement);
            VarInputController.LoadNode(nodeElement);
        }

        public override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            VarInputController.DeserializeCore(element, context);
        }

        public override bool HandleModelEventCore(string eventName)
        {
            return VarInputController.HandleModelEventCore(eventName)
                || base.HandleModelEventCore(eventName);
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            // All inputs are provided, then we should pack all inputs that
            // belong to variable input parameter into a single array. 
            if (!model.IsPartiallyApplied)
            {
                var paramCount = Definition.Parameters.Count();
                var packId = "__var_arg_pack_" + model.GUID;
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(packId),
                        AstFactory.BuildExprList(inputAstNodes.Skip(paramCount - 1).ToList())));

                inputAstNodes =
                    inputAstNodes.Take(paramCount - 1)
                        .Concat(new[] { AstFactory.BuildIdentifier(packId) })
                        .ToList();
            }

            base.BuildOutputAst(model, inputAstNodes, resultAst);

            //string function = Definition.Name;
            //AssociativeNode rhs;

            //// All inputs are provided, then we should pack all inputs that
            //// belong to variable input parameter into a single array. 
            //if (!model.HasUnconnectedInput())
            //{
            //    var paramCount = Definition.Parameters.Count();
            //    var packId = "__var_arg_pack_" + model.GUID;
            //    resultAst.Add(
            //        AstFactory.BuildAssignment(
            //            AstFactory.BuildIdentifier(packId),
            //            AstFactory.BuildExprList(inputAstNodes.Skip(paramCount - 1).ToList())));

            //    inputAstNodes =
            //        inputAstNodes.Take(paramCount - 1)
            //            .Concat(new[] { AstFactory.BuildIdentifier(packId) })
            //            .ToList();
            //}

            //switch (Definition.Type)
            //{
            //    case FunctionType.Constructor:
            //    case FunctionType.StaticMethod:
            //        if (model.HasUnconnectedInput())
            //        {
            //            var functionNode = new IdentifierListNode
            //            {
            //                LeftNode = new IdentifierNode(Definition.ClassName),
            //                RightNode = new IdentifierNode(Definition.Name)
            //            };
            //            rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
            //        }
            //        else
            //        {
            //            rhs = AstFactory.BuildFunctionCall(
            //                Definition.ClassName,
            //                Definition.Name,
            //                inputAstNodes);
            //        }
            //        break;

            //    case FunctionType.StaticProperty:

            //        var staticProp = new IdentifierListNode
            //        {
            //            LeftNode = new IdentifierNode(Definition.ClassName),
            //            RightNode = new IdentifierNode(Definition.Name)
            //        };
            //        rhs = staticProp;
            //        break;

            //    case FunctionType.InstanceProperty:

            //        // Only handle getter here. Setter could be handled in CBN.
            //        rhs = new NullNode();
            //        if (inputAstNodes != null && inputAstNodes.Count >= 1)
            //        {
            //            var thisNode = inputAstNodes[0];
            //            if (thisNode != null && !(thisNode is NullNode))
            //            {
            //                var insProp = new IdentifierListNode
            //                {
            //                    LeftNode = inputAstNodes[0],
            //                    RightNode = new IdentifierNode(Definition.Name)
            //                };
            //                rhs = insProp;
            //            }
            //        }

            //        break;

            //    case FunctionType.InstanceMethod:

            //        rhs = new NullNode();
            //        if (inputAstNodes != null && inputAstNodes.Count >= 1)
            //        {
            //            var thisNode = inputAstNodes[0];
            //            inputAstNodes.RemoveAt(0); // remove this pointer

            //            if (thisNode != null && !(thisNode is NullNode))
            //            {
            //                var memberFunc = new IdentifierListNode
            //                {
            //                    LeftNode = thisNode,
            //                    RightNode = AstFactory.BuildFunctionCall(function, inputAstNodes)
            //                };
            //                rhs = memberFunc;
            //            }
            //        }

            //        break;

            //    default:
            //        if (model.HasUnconnectedInput())
            //        {
            //            var functionNode = new IdentifierNode(function);
            //            rhs = CreateFunctionObject(model, functionNode, inputAstNodes);
            //        }
            //        else
            //        {
            //            rhs = AstFactory.BuildFunctionCall(function, inputAstNodes);
            //        }
            //        break;
            //}

            //if (model.OutPortData.Count == 1)
            //{
            //    resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));

            //    var outputIdentiferNode = model.GetAstIdentifierForOutputIndex(0);
            //    string outputIdentifier = outputIdentiferNode.ToString();
            //    string thisIdentifier = model.AstIdentifierForPreview.ToString();
            //    if (!string.Equals(outputIdentifier, thisIdentifier))
            //    {
            //        resultAst.Add(
            //            AstFactory.BuildAssignment(outputIdentiferNode, model.AstIdentifierForPreview));
            //    }
            //}
            //else
            //{
            //    var undefinedOutputs = Definition.ReturnKeys == null || !Definition.ReturnKeys.Any();

            //    if (undefinedOutputs || !model.IsPartiallyApplied)
            //    {
            //        resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));
            //    }
            //    else
            //    {
            //        var missingAmt = Enumerable.Range(0, model.InPortData.Count).Count(x => !model.HasInput(x));
            //        var tmp =
            //            AstFactory.BuildIdentifier("__partial_" + model.GUID.ToString().Replace('-', '_'));
            //        resultAst.Add(AstFactory.BuildAssignment(tmp, rhs));
            //        resultAst.AddRange(
            //            Definition.ReturnKeys.Select(AstFactory.BuildStringNode)
            //                .Select(
            //                    (rtnKey, index) =>
            //                        AstFactory.BuildAssignment(
            //                            model.GetAstIdentifierForOutputIndex(index),
            //                            AstFactory.BuildFunctionObject(
            //                                "__ComposeBuffered",
            //                                3,
            //                                new[] { 0, 1 },
            //                                new List<AssociativeNode>
            //                                {
            //                                    AstFactory.BuildExprList(
            //                                        new List<AssociativeNode>
            //                                        {
            //                                            AstFactory.BuildFunctionObject(
            //                                                "__GetOutput",
            //                                                2,
            //                                                new[] { 1 },
            //                                                new List<AssociativeNode>
            //                                                {
            //                                                    AstFactory.BuildNullNode(),
            //                                                    rtnKey
            //                                                }),
            //                                            tmp
            //                                        }),
            //                                    AstFactory.BuildIntNode(missingAmt),
            //                                    AstFactory.BuildNullNode()
            //                                }))));
            //    }
            //}
        }

        public class ZeroTouchVarInputController : VariableInputNodeController
        {
            private readonly ZeroTouchVarArgFunctionCallData data;

            public ZeroTouchVarInputController(ZeroTouchVarArgFunctionCallData def, NodeModel model)
                : base(model)
            {
                data = def;
            }

            protected override string GetInputName(int index)
            {
                return data.Definition.Parameters.Last().Name.TrimEnd('s') + index;
            }

            protected override string GetInputTooltip(int index)
            {
                var type = data.Definition.Parameters.Last().Type;
                return (string.IsNullOrEmpty(type) ? "var" : type);
            }
        }

        public VariableInputNodeController VarInputController { get; private set; }
    }

    [NodeName("Function Node w/ VarArgs"), NodeDescription("DesignScript Builtin Functions"),
     IsInteractive(false), IsVisibleInDynamoLibrary(false), NodeSearchable(false), IsMetaNode]
    public class DSVarArgFunction : DSFunctionBase
    {
        public DSVarArgFunction(FunctionDescriptor descriptor)
            : base(new ZeroTouchVarArgFunctionCallData(descriptor)) { }

        public DSVarArgFunction() : this(null) { }
    }
}