using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Library;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Namespace;
using ProtoCore.Utils;
using ProtoCore.BuildData;
using Newtonsoft.Json;

namespace Dynamo.Graph.Nodes.CustomNodes
{
    /// <summary>
    ///     DesignScript Custom Node instance.
    /// </summary>
    [NodeName("Custom Node")]
    [NodeDescription("FunctionDescription",typeof(Dynamo.Properties.Resources))]
    [IsMetaNode]
    [AlsoKnownAs("Dynamo.Nodes.Function")]
    public class Function 
        : FunctionCallBase<CustomNodeController<CustomNodeDefinition>, CustomNodeDefinition>
    {
        [JsonConstructor]
        private Function(string nickName, string description, string category)
            : base(new CustomNodeController<CustomNodeDefinition>(null))
        {
            ArgumentLacing = LacingStrategy.Auto;
            NickName = nickName;
            Description = description;
            Category = category;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="def">CustomNode definition.</param>
        /// <param name="nickName">Nickname.</param>
        /// <param name="description">Description.</param>
        /// <param name="category">Category.</param>
        public Function(
            CustomNodeDefinition def, string nickName, string description, string category)
            : base(new CustomNodeController<CustomNodeDefinition>(def))
        {
            ValidateDefinition(def);
            ArgumentLacing = LacingStrategy.Auto;
            NickName = nickName;
            Description = description;
            Category = category;
        }

        /// <summary>
        /// A flag used during serialization to indicate
        /// that the node is a custom node.
        /// </summary>
        public bool IsCustomNode
        {
            get { return true; }
        }

        /// <summary>
        /// The unique id of the underlying function.
        /// </summary>
        public Guid FunctionUuid
        {
            get { return Definition.FunctionId; }
        }

        /// <summary>
        /// Returns customNode definition.
        /// </summary>
        [JsonIgnore]
        public CustomNodeDefinition Definition { get { return Controller.Definition; } }
        
        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            return Controller.BuildAst(this, inputAstNodes);
        }

#region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called

            Controller.SerializeCore(element, context);

            var xmlDoc = element.OwnerDocument;

            var outEl = xmlDoc.CreateElement("Name");
            outEl.SetAttribute("value", NickName);
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Description");
            outEl.SetAttribute("value", Description);
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Inputs");
            foreach (string input in InPorts.Select(x => x.PortName))
            {
                XmlElement inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Outputs");
            foreach (string output in OutPorts.Select(x => x.PortName))
            {
                XmlElement outputEl = xmlDoc.CreateElement("Output");
                outputEl.SetAttribute("value", output);
                outEl.AppendChild(outputEl);
            }
            element.AppendChild(outEl);
        }

        /// <summary>
        ///     Complete a definition for a proxy custom node instance 
        ///     by adding input and output ports as far as we don't have
        ///     a corresponding custom node workspace
        /// </summary>
        /// <param name="nodeId">Identifier of the custom node instance</param>
        /// <param name="inputs">Number of inputs</param>
        /// <param name="outputs">Number of outputs</param>
        internal void LoadNode(Guid nodeId, int inputs, int outputs)
        {
            GUID = nodeId;
            
            // make the custom node instance be in sync 
            // with its definition if it's needed
            if (!Controller.IsInSyncWithNode(this))
            {
                Controller.SyncNodeWithDefinition(this);
            }
            else
            {
                PortData data;
                if (outputs > 0)
                {
                    // create outputs for the node
                    for (int i = 0; i < outputs; i++)
                    {
                        data = new PortData("", "Output #" + (i + 1));
                        if (OutPorts.Count > i)
                            OutPorts[i] = new PortModel(PortType.Output, this, data);
                        else
                            OutPorts.Add(new PortModel(PortType.Output, this, data));
                    }
                }

                if (inputs > 0)
                {
                    // create inputs for the node
                    for (int i = 0; i < inputs; i++)
                    {
                        data = new PortData("", "Input #" + (i + 1));
                        if (InPorts.Count > i)
                            InPorts[i] = new PortModel(PortType.Input, this, data);
                        else
                            InPorts.Add(new PortModel(PortType.Input, this, data));
                    }
                }

                RegisterAllPorts();
            }

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            List<XmlNode> childNodes = nodeElement.ChildNodes.Cast<XmlNode>().ToList();

            if (!Controller.IsInSyncWithNode(this))
            {
                Controller.SyncNodeWithDefinition(this);
                OnNodeModified();
            }
            else if (Controller.Definition == null || Controller.Definition.IsProxy)
            {
                foreach (XmlNode subNode in childNodes)
                {
                    if (subNode.Name.Equals("Outputs"))
                    {
                        var data =
                            subNode.ChildNodes.Cast<XmlNode>()
                                   .Select(
                                       (outputNode, i) =>
                                           new
                                           {
                                               data = new PortData(outputNode.Attributes[0].Value, Properties.Resources.ToolTipOutput + (i + 1)),
                                               idx = i
                                           });

                        foreach (var dataAndIdx in data)
                        {
                            if (OutPorts.Count > dataAndIdx.idx)
                                OutPorts[dataAndIdx.idx] = new PortModel(PortType.Output, this, dataAndIdx.data);
                            else
                                OutPorts.Add(new PortModel(PortType.Output, this, dataAndIdx.data));
                        }
                    }
                    else if (subNode.Name.Equals("Inputs"))
                    {
                        var data =
                            subNode.ChildNodes.Cast<XmlNode>()
                                   .Select(
                                       (inputNode, i) =>
                                           new
                                           {
                                               data = new PortData(inputNode.Attributes[0].Value, Properties.Resources.ToolTipInput + (i + 1)),
                                               idx = i
                                           });

                        foreach (var dataAndIdx in data)
                        {
                            if (InPorts.Count > dataAndIdx.idx)
                                InPorts[dataAndIdx.idx] = new PortModel(PortType.Input, this, dataAndIdx.data);
                            else
                                InPorts.Add(new PortModel(PortType.Input, this, dataAndIdx.data));
                        }
                    }

#region Legacy output support

                    else if (subNode.Name.Equals("Output"))
                    {
                        var data = new PortData(subNode.Attributes[0].Value, Properties.Resources.ToolTipFunctionOutput);

                        if (OutPorts.Any())
                            OutPorts[0] = new PortModel(PortType.Output, this, data);
                        else
                            OutPorts.Add(new PortModel(PortType.Output, this, data));
                    }

#endregion
                }

                RegisterAllPorts();
            }

            base.DeserializeCore(nodeElement, context); //Base implementation must be called

            XmlNode nameNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("Name"));
            if (nameNode != null && nameNode.Attributes != null)
                NickName = nameNode.Attributes["value"].Value;

            XmlNode descNode = childNodes.LastOrDefault(subNode => subNode.Name.Equals("Description"));
            if (descNode != null && descNode.Attributes != null)
                Description = descNode.Attributes["value"].Value;
        }

#endregion

        private void ValidateDefinition(CustomNodeDefinition def)
        {
            if (def.IsProxy)
            {
                this.Error(Properties.Resources.CustomNodeNotLoaded);
            } 
            else
            {
                this.ClearRuntimeError();
            }
        }


        /// <summary>
        ///     Validates passed Custom Node definition and synchronizes node with it.
        /// </summary>
        /// <param name="def">Custom Node definition.</param>
        public void ResyncWithDefinition(CustomNodeDefinition def)
        {
            ValidateDefinition(def);
            Controller.Definition = def;
            Controller.SyncNodeWithDefinition(this);
        }
    }

    /// <summary>
    ///     Represents function entry point.
    ///     It contains functionality to manage expressions and applies input values to function logic.
    /// </summary>
    [NodeName("Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("SymbolNodeDescription",typeof(Properties.Resources))]
    [NodeSearchTags("SymbolSearchTags", typeof(Properties.Resources))]    
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.Symbol")]
    public class Symbol : NodeModel
    {
        private string inputSymbol = String.Empty;
        private string nickName = String.Empty;
        private ElementResolver workspaceElementResolver;

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "InputNode";
            }
        }

        /// <summary>
        ///     Responsible for resolving 
        ///     a partial class name to its fully resolved name
        /// </summary>
        public ElementResolver ElementResolver { get; set; }
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        public Symbol()
        {
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", Properties.Resources.ToolTipSymbol)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            InputSymbol = String.Empty;

            ElementResolver = new ElementResolver();
        }

        /// <summary>
        ///     Represents string input. 
        /// </summary>
        public string InputSymbol
        {
            get { return inputSymbol; }
            set
            {
                inputSymbol = value;
                nickName = inputSymbol;
                ClearRuntimeError();

                var type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);
                AssociativeNode defaultValue = null;

                string comment = null;

                if (!string.IsNullOrEmpty(nickName))
                {
                    // three cases:
                    //    x = default_value
                    //    x : type
                    //    x : type = default_value
                    IdentifierNode identifierNode;
                    if (TryParseInputExpression(inputSymbol, out identifierNode, out defaultValue, out comment))
                    {
                        nickName = identifierNode.Value;

                        if (identifierNode.datatype.UID == Constants.kInvalidIndex)
                        {
                            string warningMessage = String.Format(
                                Properties.Resources.WarningCannotFindType,
                                identifierNode.datatype.Name);
                            this.Warning(warningMessage);
                        }
                        else
                        {
                            type = identifierNode.datatype;
                        }
                    }
                }

                Parameter = new TypedParameter(nickName, type, defaultValue, null, comment);

                OnNodeModified();
                RaisePropertyChanged("InputSymbol");
            }
        }

        /// <summary>
        ///     Returns tuple of Input parameter and its type.
        /// </summary>
        public TypedParameter Parameter
        {
            get;
            private set;
        }

        /// <summary>
        ///     Returns <see cref="IdentifierNode"/> by passed output index.
        /// </summary>
        /// <param name="outputIndex">Output index.</param>
        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return
                AstFactory.BuildIdentifier(
                    string.IsNullOrEmpty(nickName) ? AstIdentifierBase : nickName + "__" + AstIdentifierBase);
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement("Symbol");
            outEl.SetAttribute("value", InputSymbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (var subNode in
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == "Symbol"))
            {
                InputSymbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private bool TryParseInputExpression(string inputSymbol, 
                                             out IdentifierNode identifier, 
                                             out AssociativeNode defaultValue,
                                             out string comment)
        {
            identifier = null;
            defaultValue = null;
            comment = null;

            var parseString = InputSymbol;
            parseString += ";";
            
            // During loading of symbol node from file, the elementResolver from the workspace is unavailable
            // in which case, a local copy of the ER obtained from the symbol node is used
            var resolver = workspaceElementResolver ?? ElementResolver;
            var parseParam = new ParseParam(this.GUID, parseString, resolver);

            if (EngineController.CompilationServices.PreCompileCodeBlock(ref parseParam) &&
                parseParam.ParsedNodes.Any())
            {
                var parsedComments = parseParam.ParsedComments;
                if (parsedComments.Any())
                {
                    comment = String.Join("\n", parsedComments.Select(c => (c as CommentNode).Value));
                }

                var node = parseParam.ParsedNodes.First() as BinaryExpressionNode;
                if (node != null)
                {
                    var leftIdent = node.LeftNode as IdentifierNode;
                    var rightIdent = node.RightNode as IdentifierNode;

                    // "x" will be compiled to "temp_guid = x";
                    if (leftIdent != null && leftIdent.Value.StartsWith(Constants.kTempVarForNonAssignment))
                    {
                        identifier = rightIdent;
                    }
                    // "x:int" will be compiled to "x:int = tTypedIdent0";
                    else if (rightIdent != null && rightIdent.Value.StartsWith(Constants.kTempVarForTypedIdentifier))
                    {
                        identifier = leftIdent;
                    }
                    else
                    {
                        identifier = leftIdent;
                    }

                    if (inputSymbol.Contains('='))
                        defaultValue = node.RightNode;

                    if (parseParam.Errors.Any())
                    {
                        this.Error(parseParam.Errors.First().Message);
                    }
                    else if (parseParam.Warnings.Any())
                    {
                        var warnings = parseParam.Warnings.Where(w => w.ID != WarningID.IdUnboundIdentifier);
                        if (warnings.Any())
                        {
                            this.Warning(parseParam.Warnings.First().Message);
                        }
                    }

                    return identifier != null;
                }
            }

            return false;
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;
            workspaceElementResolver = updateValueParams.ElementResolver;

            if (name == "InputSymbol")
            {
                InputSymbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }
    }

    /// <summary>
    ///     Represents function output.
    ///     It contains functionality to manage expressions and store function's node value to pass. 
    /// </summary>
    [NodeName("Output")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("OutputNodeDescription",typeof(Dynamo.Properties.Resources))]    
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.Output")]
    public class Output : NodeModel
    {
        private string symbol = string.Empty;
        private string outputIdentifier = string.Empty;
        private string description = string.Empty;
        private ElementResolver workspaceElementResolver;

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "OutputNode";
            }
        }

        /// <summary>
        /// Element resolver 
        /// </summary>
        public ElementResolver  ElementResolver { get; set;}

        /// <summary>
        /// Create output node.
        /// </summary>
        public Output()
        {
            InPorts.Add(new Nodes.PortModel(PortType.Input, this, new PortData("", "")));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        /// <summary>
        /// Text in output node.
        /// </summary>
        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                ClearRuntimeError();

                string comment = string.Empty;
                IdentifierNode identNode;
                if (!TryParseOutputExpression(symbol, out identNode, out comment))
                {
                    outputIdentifier = symbol;
                }
                else
                {
                    outputIdentifier = identNode.Value;
                    description = comment;
                }

                OnNodeModified();
                RaisePropertyChanged("Symbol");
            }
        }

        /// <summary>
        /// Output name and its description tuple.
        /// </summary>
        public Tuple<string, string> Return
        {
            get
            {
                return new Tuple<string, string>(outputIdentifier, description);
            }
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            AssociativeNode assignment;
            if (null == inputAstNodes || inputAstNodes.Count == 0)
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, AstFactory.BuildNullNode());
            else
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0]);

            return new[] { assignment };
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement("Symbol");
            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (var subNode in 
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == "Symbol"))
            {
                Symbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;
            workspaceElementResolver = updateValueParams.ElementResolver;

            if (name == "Symbol")
            {
                Symbol = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }

        private bool TryParseOutputExpression(string expression, out IdentifierNode outputIdentifier, out string comment)
        {
            outputIdentifier = null;
            comment = null;

            var resolver = workspaceElementResolver ?? ElementResolver;
            var parseParam = new ParseParam(GUID, expression + ";", resolver);

            EngineController.CompilationServices.PreCompileCodeBlock(ref parseParam);
            if (parseParam.ParsedNodes.Any())
            {
                var parsedComments = parseParam.ParsedComments;
                if (parsedComments.Any())
                {
                    comment = String.Join("\n", parsedComments.Select(c => (c as CommentNode).Value));
                }

                if (parseParam.ParsedNodes.Count() > 1)
                {
                    this.Warning(Properties.Resources.WarningInvalidOutput);
                }

                var node = parseParam.ParsedNodes.First() as BinaryExpressionNode;
                if (node != null)
                {
                    var leftIdent = node.LeftNode as IdentifierNode;
                    var rightIdent = node.RightNode as IdentifierNode;

                    // "x" will be compiled to "temp_guid = x;"
                    if (leftIdent != null && leftIdent.Value.StartsWith(Constants.kTempVarForNonAssignment))
                    {
                        outputIdentifier = rightIdent;
                    }
                    else
                    {
                        outputIdentifier = leftIdent;
                    }
                    return outputIdentifier != null;
                }
            }

            return false;
        }
    }
}
