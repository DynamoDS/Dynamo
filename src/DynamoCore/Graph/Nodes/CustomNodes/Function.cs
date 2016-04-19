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
        public Function(
            CustomNodeDefinition def, string nickName, string description, string category)
            : base(new CustomNodeController<CustomNodeDefinition>(def))
        {
            ValidateDefinition(def);
            ArgumentLacing = LacingStrategy.Shortest;
            NickName = nickName;
            Description = description;
            Category = category;
        }

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
            foreach (string input in InPortData.Select(x => x.NickName))
            {
                XmlElement inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            element.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Outputs");
            foreach (string output in OutPortData.Select(x => x.NickName))
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
                        if (OutPortData.Count > i)
                            OutPortData[i] = data;
                        else
                            OutPortData.Add(data);
                    }
                }

                if (inputs > 0)
                {
                    // create inputs for the node
                    for (int i = 0; i < inputs; i++)
                    {
                        data = new PortData("", "Input #" + (i + 1));
                        if (InPortData.Count > i)
                            InPortData[i] = data;
                        else
                            InPortData.Add(data);
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
                            if (OutPortData.Count > dataAndIdx.idx)
                                OutPortData[dataAndIdx.idx] = dataAndIdx.data;
                            else
                                OutPortData.Add(dataAndIdx.data);
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
                            if (InPortData.Count > dataAndIdx.idx)
                                InPortData[dataAndIdx.idx] = dataAndIdx.data;
                            else
                                InPortData.Add(dataAndIdx.data);
                        }
                    }

                    #region Legacy output support

                    else if (subNode.Name.Equals("Output"))
                    {
                        var data = new PortData(subNode.Attributes[0].Value, Properties.Resources.ToolTipFunctionOutput);

                        if (OutPortData.Any())
                            OutPortData[0] = data;
                        else
                            OutPortData.Add(data);
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
            if (def == null)
            {
                throw new ArgumentNullException("def");
            }

            if (def.IsProxy)
            {
                this.Error(Properties.Resources.CustomNodeNotLoaded);
            } 
            else
            {
                this.ClearRuntimeError();
            }
        }

        public void ResyncWithDefinition(CustomNodeDefinition def)
        {
            ValidateDefinition(def);
            Controller.Definition = def;
            Controller.SyncNodeWithDefinition(this);
        }
    }

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
        public ElementResolver  ElementResolver { get; set;}
        private ElementResolver workspaceElementResolver;

        public Symbol()
        {
            OutPortData.Add(new PortData("", Properties.Resources.ToolTipSymbol));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            InputSymbol = String.Empty;

            ElementResolver = new ElementResolver();
        }

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

        public TypedParameter Parameter
        {
            get;
            private set;
        }

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
        /// Element resolver 
        /// </summary>
        public ElementResolver  ElementResolver { get; set;}

        /// <summary>
        /// Create output node.
        /// </summary>
        public Output()
        {
            InPortData.Add(new PortData("", ""));

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
                if (node == null)
                {
                    if (parseParam.Errors.Any())
                    {
                        this.Error(Properties.Resources.WarningInvalidOutput);
                    }
                }
                else
                {
                    var leftIdent = node.LeftNode as IdentifierNode;
                    var rightIdent = node.RightNode as IdentifierNode;

                    // "x" will be compiled to "temp_guid = x;"
                    if (leftIdent != null && leftIdent.Value.StartsWith(Constants.kTempVarForNonAssignment))
                    {
                        outputIdentifier = rightIdent;
                    }
                    // "x:int" will be compiled to "x:int = tTypedIdent0;"
                    else if (rightIdent != null && rightIdent.Value.StartsWith(Constants.kTempVarForTypedIdentifier))
                    {
                        outputIdentifier = leftIdent;
                    }
                    else
                    {
                        if (parseParam.Errors.Any())
                        {
                            this.Error(parseParam.Errors.First().Message);
                        }
                        else
                        {
                            this.Warning(Properties.Resources.WarningInvalidOutput);
                        }
                        outputIdentifier = leftIdent;
                    }
                    return outputIdentifier != null;
                }
            }

            return false;
        }
    }
}
