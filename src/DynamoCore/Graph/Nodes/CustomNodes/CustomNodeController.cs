using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes.CustomNodes
{
    /// <summary>
    ///     Controller that synchronizes a custom node with a node definition.
    /// </summary>
    public class CustomNodeController<T> : FunctionCallNodeController<T>
        where T : CustomNodeDefinition
    {
        public CustomNodeController(T def) : base(def) { }
        
        protected override void InitializeInputs(NodeModel model)
        {
            if (Definition.DisplayParameters == null || Definition.Parameters == null)
                return;

            var inputs = Definition.DisplayParameters.Zip(Definition.Parameters, (dp, p) => Tuple.Create(dp, p.Description, p.DefaultValue)).ToList();
            var count = inputs.Count();

            if(model.InPorts.Count > count)
            {
                for (int i = model.InPorts.Count - 1; i >= count; i--)
                {
                    model.InPorts.RemoveAt(i);
                }
            }

            for (int i=0; i<inputs.Count(); i++)
            {
                var input = inputs[i];

                if(model.InPorts.Count > i)
                {
                    model.InPorts[i].Name = input.Item1;
                    model.InPorts[i].ToolTip = input.Item2;
                    model.InPorts[i].DefaultValue = input.Item3;
                }
                else
                {
                    model.InPorts.Add(new PortModel(PortType.Input, model, new PortData(input.Item1, input.Item2, input.Item3)));
                }
            }
        }

        protected override void InitializeOutputs(NodeModel model)
        {
            if (Definition.Returns.Any())
            {
                if(model.OutPorts.Count() > Definition.Returns.Count())
                {
                    for (int i = model.OutPorts.Count - 1; i >= Definition.Returns.Count(); i--)
                    {
                        model.OutPorts.RemoveAt(i);
                    }
                }

                var returns = Definition.Returns.ToList();
                for(int i=0; i<Definition.Returns.Count(); i++)
                {
                    var pair = returns[i];
                    string key = pair.Item1;
                    string tooltip = pair.Item2;

                    if (string.IsNullOrEmpty(tooltip))
                    {
                        tooltip = Properties.Resources.ToolTipReturnValue;
                    }

                    if(model.OutPorts.Count > i)
                    {
                        model.OutPorts[i].Name = key;
                        model.OutPorts[i].ToolTip = tooltip;
                    }
                    else
                    {
                        model.OutPorts.Add(new PortModel(PortType.Output, model, new PortData(key, tooltip)));
                    }
                }
            }
            else
            {
                model.OutPorts.Clear();
                model.OutPorts.Add(new PortModel(PortType.Output, model, new PortData("", Properties.Resources.ToolTipReturnValue)));
            }
        }

        protected override AssociativeNode GetFunctionApplication(NodeModel model, List<AssociativeNode> inputAstNodes)
        {
            if (!model.IsPartiallyApplied)
            {
                model.UseLevelAndReplicationGuide(inputAstNodes);
                return AstFactory.BuildFunctionCall(Definition.FunctionName, inputAstNodes);
            }

            var count = Definition.DisplayParameters.Count();
            return AstFactory.BuildFunctionObject(
                Definition.FunctionName,
                count,
                Enumerable.Range(0, count).Where(index=>model.InPorts[index].IsConnected),
                inputAstNodes);
        }

        protected override void AssignIdentifiersForFunctionCall(
            NodeModel model, AssociativeNode rhs, List<AssociativeNode> resultAst)
        {
            if (model.OutPorts.Count == 1)
                resultAst.Add(AstFactory.BuildAssignment(model.AstIdentifierForPreview, rhs));
            else
                base.AssignIdentifiersForFunctionCall(model, rhs, resultAst);
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            if (Definition == null)
            {
                var lhs = model.AstIdentifierForPreview;
                var rhs = AstFactory.BuildNullNode();
                resultAst.Add(AstFactory.BuildAssignment(lhs, rhs));
            }
            else
            {
                base.BuildOutputAst(model, inputAstNodes, resultAst);
            }
        }

        /// <summary>
        /// Synchronizes custom node with its definition.
        /// </summary>
        /// <param name="model">Custom node model</param>
        public override void SyncNodeWithDefinition(NodeModel model)
        {
            if (IsInSyncWithNode(model))
            {
                Debug.WriteLine("Custom node definition is already in sync for: " + 
                    model.Name + 
                    string.Format(", {0} returns, {1} parameters", Definition.Returns.Count(), Definition.Parameters.Count()));
                return;
            } 

            Debug.WriteLine("Syncing custom node with definition for: " + 
                model.Name + 
                string.Format(", {0} returns, {1} parameters", Definition.Returns.Count(), Definition.Parameters.Count()));

            base.SyncNodeWithDefinition(model);

            model.OnNodeModified();
        }

        /// <summary>
        /// Serializes CustomNode.
        /// </summary>
        /// <param name="nodeElement">Xml node</param>
        /// <param name="saveContext">SaveContext is used in base class.</param>
        public override void SerializeCore(XmlElement nodeElement, SaveContext saveContext)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement("ID");

            outEl.SetAttribute("value", Definition.FunctionId.ToString());
            nodeElement.AppendChild(outEl);
            nodeElement.SetAttribute("nickname", Name);
        }

        /// <summary>
        ///   Return if the custom node instance is in sync with its definition.
        ///   It may be out of sync if .dyf file is opened and updated and then
        ///   .dyn file is opened. 
        /// </summary>
        public bool IsInSyncWithNode(NodeModel model)
        {
            if (Definition == null)
                return true;

            if (Definition.DisplayParameters != null)
            {
                var paramNames = model.InPorts.Select(p => p.Name);
                if (!Definition.DisplayParameters.SequenceEqual(paramNames))
                    return false;
            }

            if (Definition.Parameters != null)
            {
                var defParamTypes = Definition.Parameters.Select(p => p.Type.ToShortString());
                var paramTypes = model.InPorts.Select(p => p.ToolTip);
                if (!defParamTypes.SequenceEqual(paramTypes))
                    return false;
            }

            if (Definition.ReturnKeys != null)
            {
                var returnKeys = model.OutPorts.Select(p => p.Name);
                if (!Definition.ReturnKeys.SequenceEqual(returnKeys))
                    return false;
            }

            if (Definition.Returns != null)
            {
                var tooltips = model.OutPorts.Select(p => p.ToolTip);
                if (!Definition.Returns.Select(r => string.IsNullOrEmpty(r.Item2) ? Properties.Resources.ToolTipReturnValue : r.Item2)
                                       .SequenceEqual(tooltips))
                    return false;
            }
            return true;
        }
    }
}
