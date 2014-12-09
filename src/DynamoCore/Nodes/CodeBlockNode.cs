using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Xml;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.BuildData;
using ArrayNode = ProtoCore.AST.AssociativeAST.ArrayNode;
using Node = ProtoCore.AST.Node;
using Operator = ProtoCore.DSASM.Operator;
using ProtoCore.Utils;
using Dynamo.UI;

namespace Dynamo.Nodes
{
    [NodeName("Code Block")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows for DesignScript code to be authored directly")]
    [IsDesignScriptCompatible]
    public partial class CodeBlockNodeModel : NodeModel
    {
        private readonly List<Statement> codeStatements = new List<Statement>();
        private string code = string.Empty;
        private List<string> inputIdentifiers = new List<string>();
        private List<string> tempVariables = new List<string>();
        private string previewVariable = null;
        private bool shouldFocus = true;
        public bool ShouldFocus { get { return shouldFocus; } }
        private readonly DynamoLogger logger;

        private struct Formatting
        {
            public const double InitialMargin = 0;
            public const string ToolTipForTempVariable = "Statement Output";
        }

        #region Public Methods

        public CodeBlockNodeModel(WorkspaceModel workspace)
            : base(workspace)
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public CodeBlockNodeModel(WorkspaceModel workspace, string userCode) 
            : this(workspace)
        {
            code = userCode;
            ProcessCodeDirect();
        }

        public CodeBlockNodeModel(string userCode, Guid guid, WorkspaceModel workspace, double xPos, double yPos) : base(workspace)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            this.X = xPos;
            this.Y = yPos;
            this.code = userCode;
            this.GUID = guid;
            this.shouldFocus = false;
            ProcessCodeDirect();
        }

        /// <summary>
        ///     It removes all the in ports and out ports so that the user knows there is an error.
        /// </summary>
        /// <param name="errorMessage"> Error message to be displayed </param>
        private void ProcessError()
        {
            previewVariable = null;
        }

        /// <summary>
        ///     Returns the names of all the variables defined in this code block.
        /// </summary>
        /// <returns>List containing all the names</returns>
        public List<string> GetDefinedVariableNames()
        {
            var defVarNames = new List<string>();

            // For unbound identifier, ideally if there is an input connect 
            // to it, it is defined variable. But here we have to be more
            // aggresive. For copy/paste, the connectors haven't been 
            // created yet, so if a variable is defined in other CBN, even
            // that variable is defined in this CBN, it is not included in
            // the return value. 
            defVarNames.AddRange(inputIdentifiers);

            // Then get all variabled on the LHS of the statements
            foreach (Statement stmnt in codeStatements)
            {
                defVarNames.AddRange(Statement.GetDefinedVariableNames(stmnt, true));
            }

            return defVarNames;
        }

        /// <summary>
        /// Returns the index of the port corresponding to the variable name given
        /// </summary>
        /// <param name="variableName"> Name of the variable corresponding to an input port </param>
        /// <returns> Index of the required port in the InPorts collection </returns>
        public static int GetInportIndex(CodeBlockNodeModel cbn, string variableName)
        {
            return cbn.inputIdentifiers.IndexOf(variableName);
        }

        /// <summary>
        ///  Returns the corresponding output port index for a given defined variable 
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public int GetOutportIndex(string variableName)
        {
            var svs = CodeBlockUtils.GetStatementVariables(codeStatements, true);
            for (int i = 0; i < codeStatements.Count; i++)
            {
                Statement s = codeStatements[i];
                if (CodeBlockUtils.DoesStatementRequireOutputPort(svs, i))
                {
                    List<string> varNames = Statement.GetDefinedVariableNames(s, true);
                    if (varNames.Contains(variableName))
                        return i;
                }
            }
            return -1;
        }

        #endregion

        #region Properties

        public override bool IsConvertible
        {
            get
            {
                return true;
            }
        }

        public override string AstIdentifierBase
        {
            get
            {
                return previewVariable ?? base.AstIdentifierBase;
            }
        }

        public string Code
        {
            get { return code; }

            set
            {
                if (code == null || !code.Equals(value))
                {
                    if (value != null)
                    {
                        string errorMessage = string.Empty;
                        string warningMessage = string.Empty;

                        DisableReporting();

                        using (Workspace.UndoRecorder.BeginActionGroup())
                        {
                            var inportConnections = new OrderedDictionary();
                            var outportConnections = new OrderedDictionary();
                            //Save the connectors so that we can recreate them at the correct positions
                            SaveAndDeleteConnectors(inportConnections, outportConnections);

                            if (string.IsNullOrEmpty(code))
                            {
                                Workspace.UndoRecorder.PopFromUndoGroup();
                                Workspace.UndoRecorder.RecordCreationForUndo(this);
                            }
                            else
                                Workspace.UndoRecorder.RecordModificationForUndo(this);
                            code = value;
                            ProcessCode(ref errorMessage, ref warningMessage);

                            //Recreate connectors that can be reused
                            LoadAndCreateConnectors(inportConnections, outportConnections);
                        }

                        RaisePropertyChanged("Code");
                        RequiresRecalc = true;
                        ReportPosition();

                        if (Workspace != null)
                        {
                            Workspace.Modified();
                        }

                        EnableReporting();

                        ClearRuntimeError();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Error(errorMessage);
                        }
                        else if (!string.IsNullOrEmpty(warningMessage))
                        {
                            Warning(warningMessage);
                        }
                    }
                    else
                        code = null;
                }
            }
        }

        /// <summary>
        /// Temporary variables that generated in code.
        /// </summary>
        public IEnumerable<string> TempVariables
        {
            get { return tempVariables; }
        }

        public IEnumerable<Statement> CodeStatements
        {
            get { return codeStatements; }
        }

        #endregion

        #region Protected Methods

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);
            var helper = new XmlElementHelper(nodeElement);
            helper.SetAttribute("CodeText", code);
            helper.SetAttribute("ShouldFocus", shouldFocus);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            var helper = new XmlElementHelper(nodeElement as XmlElement);
            code = helper.ReadString("CodeText");
            ProcessCodeDirect();
            shouldFocus = helper.ReadBoolean("ShouldFocus");
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Code")
            {
                //Remove the UpdateValue's recording
                this.Workspace.UndoRecorder.PopFromUndoGroup();

                //Since an empty Code Block Node should not exist, this checks for such instances.
                // If an empty Code Block Node is found, it is deleted. Since the creation and deletion of 
                // an empty Code Block Node should not be recorded, this method also checks and removes
                // any unwanted recordings
                value = CodeBlockUtils.FormatUserText(value);
                if (value == "")
                {
                    if (this.Code == "")
                    {
                        this.Workspace.UndoRecorder.PopFromUndoGroup();
                        Dynamo.Selection.DynamoSelection.Instance.Selection.Remove(this);
                        this.Workspace.Nodes.Remove(this);
                    }
                    else
                    {
                        this.Workspace.RecordAndDeleteModels(new System.Collections.Generic.List<ModelBase>() { this });
                    }
                }
                else
                {
                    if (!value.Equals(this.Code))
                        Code = value;
                }
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("CodeText", code);
            helper.SetAttribute("ShouldFocus", shouldFocus);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            if (context == SaveContext.Undo)
            {
                var helper = new XmlElementHelper(element);
                shouldFocus = helper.ReadBoolean("ShouldFocus");
                code = helper.ReadString("CodeText");
                ProcessCodeDirect();
            }
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            //Do not build if the node is in error.
            if (this.State == ElementState.Error)
            {
                return null;
            }

            var resultNodes = new List<AssociativeNode>();

            // Define unbound variables if necessary
            if (inputIdentifiers != null && 
                inputAstNodes != null && 
                inputIdentifiers.Count == inputAstNodes.Count)
            {
                var initStatments = inputIdentifiers.Zip(inputAstNodes,
                    (ident, rhs) =>
                    {
                        var identNode = AstFactory.BuildIdentifier(ident);
                        MapIdentifiers(identNode);
                        return AstFactory.BuildAssignment(identNode, rhs);
                    });
                resultNodes.AddRange(initStatments);
            }

            foreach (var stmnt in codeStatements)
            {
                var astNode = ProtoCore.Utils.NodeUtils.Clone(stmnt.AstNode);
                MapIdentifiers(astNode);
                resultNodes.Add(astNode as ProtoCore.AST.AssociativeAST.AssociativeNode);
            }

            return resultNodes;
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int portIndex)
        {
            if (State == ElementState.Error)
                return null;

            // Here the "portIndex" is back mapped to the corresponding "Statement" 
            // object. However, not all "Statement" objects produce an output port,
            // so "portIndex" cannot be used directly to index into "codeStatements" 
            // list. This loop goes through "codeStatements", decrementing "portIndex"
            // along the way to determine the right "Statement" object matching the 
            // port index.
            // 
            Statement statement = null;
            var svs = CodeBlockUtils.GetStatementVariables(codeStatements, true);
            for (int stmt = 0, port = 0; stmt < codeStatements.Count; stmt++)
            {
                if (CodeBlockUtils.DoesStatementRequireOutputPort(svs, stmt))
                {
                    if (port == portIndex)
                    {
                        statement = codeStatements[stmt];
                        break;
                    }

                    port = port + 1;
                }
            }

            if (statement == null)
                return null;

            var binExprNode = statement.AstNode as BinaryExpressionNode;
            if (binExprNode == null || (binExprNode.LeftNode == null))
                return null;

            var identNode = binExprNode.LeftNode as IdentifierNode;
            var mappedIdent = ProtoCore.Utils.NodeUtils.Clone(identNode);
            MapIdentifiers(mappedIdent);
            return mappedIdent as IdentifierNode;
        }

        #endregion

        #region Private Methods

        internal void ProcessCodeDirect()
        {
            string errorMessage = string.Empty;
            string warningMessage = string.Empty;

            ProcessCode(ref errorMessage, ref warningMessage);
            RaisePropertyChanged("Code");
            RequiresRecalc = true;

            if (Workspace != null)
            {
                Workspace.Modified();
            }

            ClearRuntimeError();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Error(errorMessage);
            }
            else if (!string.IsNullOrEmpty(warningMessage))
            {
                Warning(warningMessage);
            }
        }

        private void ProcessCode(ref string errorMessage, ref string warningMessage)
        {
            code = CodeBlockUtils.FormatUserText(code);
            codeStatements.Clear();

            if (string.IsNullOrEmpty(Code))
                previewVariable = null;

            try
            {
                ParseParam parseParam = new ParseParam(this.GUID, code);
                if (Workspace.DynamoModel.EngineController.TryParseCode(ref parseParam))
                {
                    if (parseParam.ParsedNodes != null)
                    {
                        // Create an instance of statement for each code statement written by the user
                        foreach (var parsedNode in parseParam.ParsedNodes)
                        {
                            // Create a statement variable from the generated nodes
                            codeStatements.Add(Statement.CreateInstance(parsedNode));
                        }

                        SetPreviewVariable(parseParam.ParsedNodes);
                    }
                }

                if (parseParam.Errors != null && parseParam.Errors.Any())
                {
                    errorMessage = string.Join("\n", parseParam.Errors.Select(m => m.Message));
                    ProcessError();
                    CreateInputOutputPorts();
                    return;
                }

                if (parseParam.Warnings != null)
                {
                    // Unbound identifiers in CBN will have input slots.
                    // 
                    // To check function redefinition, we need to check other
                    // CBN to find out if it has been defined yet. Now just
                    // skip this warning.
                    var warnings = parseParam.Warnings.Where((w) =>
                    {
                        return w.ID != WarningID.kIdUnboundIdentifier
                            && w.ID != WarningID.kFunctionAlreadyDefined;
                    });

                    if (warnings.Any())
                    {
                        warningMessage = string.Join("\n", warnings.Select(m => m.Message));
                    }
                }

                if (parseParam.UnboundIdentifiers != null)
                    inputIdentifiers = new List<string>(parseParam.UnboundIdentifiers);
                else
                    inputIdentifiers.Clear();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                previewVariable = null;
                ProcessError();
                return;
            }

            // Set the input and output ports based on the statements
            CreateInputOutputPorts();
        }

        private void SetPreviewVariable(IEnumerable<Node> parsedNodes)
        {
            this.previewVariable = null;
            if (parsedNodes == null || (!parsedNodes.Any()))
                return;

            IdentifierNode identifierNode = null;
            foreach(var parsedNode in parsedNodes.Reverse())
            {
                var statement = parsedNode as BinaryExpressionNode;
                if (null == statement)
                    continue;

                identifierNode = statement.LeftNode as IdentifierNode;
                if (identifierNode != null) // Found the identifier...
                {
                    // ... that is not a temporary variable, take it!
                    if (!tempVariables.Contains(identifierNode.Value))
                        break;
                }
            }

            if (identifierNode == null)
                return;

            var duplicatedNode = new IdentifierNode(identifierNode);
            MapIdentifiers(duplicatedNode);

            // Of course, if we just needed "duplicatedNode.Value" we would not 
            // have to clone the original "IdentifierNode". In addition to 
            // renaming the variable, we also need to keep the array indexer 
            // (e.g. the "previewVariable" should be "arr[2][3]" instead of just
            // "arr") to obtain the correct value for that particular array 
            // element. The best way to keep these array indexers, naturally, is
            // to use "IdentifierNode.ToString" method, as in:
            // 
            //      previewVariable = duplicatedNode.ToString();
            // 
            // But the problem now is, "ILiveRunner.InspectNodeValue" method can 
            // only return a valid RuntimeMirror if "previewVariable" contains 
            // variable name (i.e. "arr") and nothing else (e.g. "arr[2][3]").
            // For now, simply set the "previewVariable" to just the array name,
            // instead of the full expression with array indexers.
            // 
            previewVariable = duplicatedNode.Value;
        }

        /// <summary>
        /// Creates the inport and outport data based on 
        /// the statements generated from the user code.
        /// </summary>
        /// 
        private void CreateInputOutputPorts()
        {
            InPortData.Clear();
            OutPortData.Clear();
            if ((codeStatements == null || (codeStatements.Count == 0))
                && (inputIdentifiers == null || (inputIdentifiers.Count == 0)))
            {
                RegisterAllPorts();
                return;
            }

            SetInputPorts();
            SetOutputPorts();

            RegisterAllPorts();
        }

        private void SetInputPorts()
        {
            // Generate input port data list from the unbound identifiers.
            var inportData = CodeBlockUtils.GenerateInputPortData(this.inputIdentifiers);
            foreach (var portData in inportData)
                InPortData.Add(portData);
        }

        private void SetOutputPorts()
        {
            var allDefs = CodeBlockUtils.GetDefinitionLineIndexMap(codeStatements);

            if (allDefs.Any() == false)
                return;

            double prevPortBottom = 0.0;
            var map = CodeBlockUtils.MapLogicalToVisualLineIndices(this.code);
            foreach (var def in allDefs)
            {
                // Map the given logical line index to its corresponding visual 
                // line index. Do note that "def.Value" here is the line number 
                // supplied by the paser, which uses 1-based line indexing so we 
                // have to remove one from the line index.
                // 
                var logicalIndex = def.Value - 1;
                var visualIndex = map.ElementAt(logicalIndex);

                string tooltip = def.Key;
                if (tempVariables.Contains(def.Key))
                    tooltip = Formatting.ToolTipForTempVariable;

                double portCoordsY = Formatting.InitialMargin;
                portCoordsY += visualIndex * Configurations.CodeBlockPortHeightInPixels;
                
                OutPortData.Add(new PortData(string.Empty, tooltip)
                {
                    VerticalMargin = portCoordsY - prevPortBottom,
                    Height = Configurations.CodeBlockPortHeightInPixels
                });

                // Since we compute the "delta" between the top of the current 
                // port to the bottom of the previous port, we need to record 
                // down the bottom coordinate value before proceeding to the next 
                // port.
                // 
                prevPortBottom = portCoordsY + Configurations.CodeBlockPortHeightInPixels;
            }
        }

        /// <summary>
        ///     Deletes all the connections and saves their data (the start and end port)
        ///     so that they can be recreated if needed.
        /// </summary>
        /// <param name="portConnections">A list of connections that will be destroyed</param>
        private void SaveAndDeleteConnectors(OrderedDictionary inportConnections, OrderedDictionary outportConnections)
        {
            //----------------------------Inputs---------------------------------
            for (int i = 0; i < InPorts.Count; i++)
            {
                PortModel portModel = InPorts[i];
                string portName = portModel.ToolTipContent;
                if (portModel.Connectors.Count != 0)
                {
                    inportConnections.Add(portName, new List<PortModel>());
                    foreach (ConnectorModel connector in portModel.Connectors)
                    {
                        (inportConnections[portName] as List<PortModel>).Add(connector.Start);
                        Workspace.UndoRecorder.RecordDeletionForUndo(connector);
                    }
                }
                else
                    inportConnections.Add(portName, null);
            }

            //Delete the connectors
            foreach (PortModel inport in InPorts)
                inport.DestroyConnectors();

            //Clear out all the port models
            for (int i = InPorts.Count - 1; i >= 0; i--)
                InPorts.RemoveAt(i);


            //----------------------------Outputs---------------------------------
            for (int i = 0; i < OutPorts.Count; i++)
            {
                PortModel portModel = OutPorts[i];
                string portName = portModel.ToolTipContent;
                if (portModel.ToolTipContent.Equals(Formatting.ToolTipForTempVariable))
                    portName += i.ToString(CultureInfo.InvariantCulture);
                if (portModel.Connectors.Count != 0)
                {
                    outportConnections.Add(portName, new List<PortModel>());
                    foreach (ConnectorModel connector in portModel.Connectors)
                    {
                        (outportConnections[portName] as List<PortModel>).Add(connector.End);
                        Workspace.UndoRecorder.RecordDeletionForUndo(connector);
                    }
                }
                else
                    outportConnections.Add(portName, null);
            }

            //Delete the connectors
            foreach (PortModel outport in OutPorts)
                outport.DestroyConnectors();

            //Clear out all the port models
            for (int i = OutPorts.Count - 1; i >= 0; i--)
                OutPorts.RemoveAt(i);
        }

        /// <summary>
        ///     Now that the portData has been set for the new ports, we recreate the connections we
        ///     so mercilessly destroyed, restoring peace and balance to the world once again.
        /// </summary>
        /// <param name="outportConnections"> List of the connections that were killed</param>
        private void LoadAndCreateConnectors(OrderedDictionary inportConnections, OrderedDictionary outportConnections)
        {
            //----------------------------Inputs---------------------------------
            /* Input Port connections are matched only if the name is the same */
            for (int i = 0; i < InPortData.Count; i++)
            {
                string varName = InPortData[i].ToolTipString;
                if (inportConnections.Contains(varName))
                {
                    if (inportConnections[varName] != null)
                    {
                        foreach (var startPortModel in (inportConnections[varName] as List<PortModel>))
                        {
                            PortType p;
                            NodeModel startNode = startPortModel.Owner;
                            ConnectorModel connector = this.Workspace.AddConnection(startNode, this,
                                startNode.GetPortIndexAndType(startPortModel, out p), i);
                            this.Workspace.UndoRecorder.RecordCreationForUndo(connector);
                        }
                        outportConnections[varName] = null;
                    }
                }
            }

            //----------------------------Outputs--------------------------------
            /*The matching is done in three parts:
             *Step 1:
             *   First, it tries to match the connectors wrt to the defined 
             *   variable name. Hence it first checks to see if any of the old 
             *   variable names are present. If so, if there were any connectors 
             *   presnt then it makes the new connectors. As it iterates through 
             *   the new ports, it also finds the ports that didnt exist before
             */
            List<int> undefinedIndices = new List<int>();
            for (int i = 0; i < OutPortData.Count; i++)
            {
                string varName = OutPortData[i].ToolTipString;
                if (outportConnections.Contains(varName))
                {
                    if (outportConnections[varName] != null)
                    {
                        foreach (var endPortModel in (outportConnections[varName] as List<PortModel>))
                        {
                            PortType p;
                            NodeModel endNode = endPortModel.Owner;
                            var connector = this.Workspace.AddConnection(this, endNode, i,
                                endNode.GetPortIndexAndType(endPortModel, out p), PortType.INPUT);
                            this.Workspace.UndoRecorder.RecordCreationForUndo(connector);
                        }
                        outportConnections[varName] = null;
                    }
                }
                else
                    undefinedIndices.Add(i);
            }

            /*
             *Step 2:
             *   The second priority is to match the connections to the previous 
             *   indices. For all the ports that were not previously defined, it 
             *   now checks if that "numbered" port had any connections 
             *   previously, ie, if the old third port had 2 connections, then 
             *   these would go to the new 3rd port (if it is not a variable that
             *   was defined before)
             */
            for (int i = 0; i < undefinedIndices.Count; i++)
            {
                int index = undefinedIndices[i];
                if (index < outportConnections.Count && outportConnections[index] != null)
                {
                    foreach (PortModel endPortModel in (outportConnections[index] as List<PortModel>))
                    {
                        PortType p;
                        NodeModel endNode = endPortModel.Owner;
                        var connector = this.Workspace.AddConnection(this, endNode, index,
                            endNode.GetPortIndexAndType(endPortModel, out p), PortType.INPUT);
                        Workspace.UndoRecorder.RecordCreationForUndo(connector);
                    }
                    outportConnections[index] = null;
                    undefinedIndices.Remove(index);
                    i--;
                }
            }

            /*
             *Step 2:
             *   The final step. Now that the priorties are finished, the 
             *   function tries to reuse any existing connections by attaching 
             *   them to any ports that have not already been given connections
             */
            List<List<PortModel>> unusedConnections = new List<List<PortModel>>();
            foreach (List<PortModel> portModelList in outportConnections.Values.Cast<List<PortModel>>())
            {
                if (portModelList == null)
                    continue;
                unusedConnections.Add(portModelList);
            }
            while (undefinedIndices.Count > 0 && unusedConnections.Count != 0)
            {
                foreach (PortModel endPortModel in unusedConnections[0])
                {
                    PortType p;
                    NodeModel endNode = endPortModel.Owner;
                    ConnectorModel connector = this.Workspace.AddConnection(
                        this,
                        endNode,
                        undefinedIndices[0],
                        endNode.GetPortIndexAndType(endPortModel, out p),
                        PortType.INPUT);
                    Workspace.UndoRecorder.RecordCreationForUndo(connector);
                }
                undefinedIndices.RemoveAt(0);
                unusedConnections.RemoveAt(0);
            }
        }

        private void MapIdentifiers(Node astNode)
        {
            if (astNode == null)
            {
                return;
            }

            var definedVars = GetDefinedVariableNames();

            if (astNode is IdentifierNode)
            {
                var identNode = astNode as IdentifierNode;
                var ident = identNode.Value;
                if ((inputIdentifiers.Contains(ident) || definedVars.Contains(ident))
                    && !tempVariables.Contains(ident)
                    && !identNode.Equals(this.AstIdentifierForPreview))
                {
                    identNode.Name = identNode.Value = LocalizeIdentifier(ident);
                }

                MapIdentifiers(identNode.ArrayDimensions);
            }
            else if (astNode is IdentifierListNode)
            {
                var node = astNode as IdentifierListNode;
                MapIdentifiers(node.LeftNode);
                MapIdentifiers(node.RightNode);
            }
            else if (astNode is FunctionCallNode)
            {
                var node = astNode as FunctionCallNode;
                MapIdentifiers(node.Function);
                for (int i = 0; i < node.FormalArguments.Count; ++i)
                {
                    MapIdentifiers(node.FormalArguments[i]);
                }
                MapIdentifiers(node.ArrayDimensions);
            }
            else if (astNode is ArrayNode)
            {
                var node = astNode as ArrayNode;
                MapIdentifiers(node.Expr);
            }
            else if (astNode is ExprListNode)
            {
                var node = astNode as ExprListNode;
                for (int i = 0; i < node.list.Count; ++i)
                {
                    MapIdentifiers(node.list[i]);
                }
                MapIdentifiers(node.ArrayDimensions);
            }
            else if (astNode is FunctionDotCallNode)
            {
                var node = astNode as FunctionDotCallNode;
            }
            else if (astNode is InlineConditionalNode)
            {
                var node = astNode as InlineConditionalNode;
                MapIdentifiers(node.ConditionExpression);
                MapIdentifiers(node.TrueExpression);
                MapIdentifiers(node.FalseExpression);
            }
            else if (astNode is RangeExprNode)
            {
                var node = astNode as RangeExprNode;
                MapIdentifiers(node.FromNode);
                MapIdentifiers(node.ToNode);
                MapIdentifiers(node.StepNode);
                MapIdentifiers(node.ArrayDimensions);
            }
            else if (astNode is BinaryExpressionNode)
            {
                var node = astNode as BinaryExpressionNode;
                MapIdentifiers(node.LeftNode);
                MapIdentifiers(node.RightNode);
            }
            else
            {
            }
        }

        private string LocalizeIdentifier(string identifierName)
        {
            var guid = this.GUID.ToString().Replace("-", string.Empty);
            return string.Format("{0}_{1}", identifierName, guid);
        }

        #endregion
    }

    public class Statement
    {
        #region Enums

        #region State enum

        public enum State
        {
            Normal,
            Warning,
            Error
        }

        #endregion

        #region StatementType enum

        public enum StatementType
        {
            None,
            Expression,
            Literal,
            Collection,
            AssignmentVar,
            FuncDeclaration
        }

        #endregion

        #endregion

        private readonly List<Variable> definedVariables = new List<Variable>();
        private readonly List<Variable> referencedVariables;
        private readonly List<Statement> subStatements = new List<Statement>();

        #region Public Methods
        public static Statement CreateInstance(Node parsedNode)
        {
            if (parsedNode == null)
                throw new ArgumentNullException();

            return new Statement(parsedNode);
        }

        public static void GetReferencedVariables(Node astNode, List<Variable> refVariableList)
        {
            //DFS Search to find all identifier nodes
            if (astNode == null)
                return;
            if (astNode is FunctionCallNode)
            {
                var currentNode = astNode as FunctionCallNode;
                foreach (AssociativeNode node in currentNode.FormalArguments)
                    GetReferencedVariables(node, refVariableList);
            }
            else if (astNode is IdentifierNode)
            {
                var resultVariable = new Variable(astNode as IdentifierNode);
                refVariableList.Add(resultVariable);
                GetReferencedVariables((astNode as IdentifierNode).ArrayDimensions, refVariableList);
            }
            else if (astNode is ArrayNode)
            {
                var currentNode = astNode as ArrayNode;
                GetReferencedVariables(currentNode.Expr, refVariableList);
                GetReferencedVariables(currentNode.Type, refVariableList);
            }
            else if (astNode is ExprListNode)
            {
                var currentNode = astNode as ExprListNode;
                foreach (AssociativeNode node in currentNode.list)
                    GetReferencedVariables(node, refVariableList);
            }
            else if (astNode is FunctionDotCallNode)
            {
                var currentNode = astNode as FunctionDotCallNode;
                GetReferencedVariables(currentNode.FunctionCall, refVariableList);
            }
            else if (astNode is InlineConditionalNode)
            {
                var currentNode = astNode as InlineConditionalNode;
                GetReferencedVariables(currentNode.ConditionExpression, refVariableList);
                GetReferencedVariables(currentNode.TrueExpression, refVariableList);
                GetReferencedVariables(currentNode.FalseExpression, refVariableList);
            }
            else if (astNode is RangeExprNode)
            {
                var currentNode = astNode as RangeExprNode;
                GetReferencedVariables(currentNode.FromNode, refVariableList);
                GetReferencedVariables(currentNode.ToNode, refVariableList);
                GetReferencedVariables(currentNode.StepNode, refVariableList);
            }
            else if (astNode is BinaryExpressionNode)
            {
                var currentNode = astNode as BinaryExpressionNode;
                GetReferencedVariables(currentNode.RightNode, refVariableList);
            }
            else
            {
                //Its could be something like a literal
                //Or node not completely implemented YET
            }
        }

        /// <summary>
        ///     Returns the names of the variables that have been referenced in the statement
        /// </summary>
        /// <param name="s"> Statement whose variable names to be got.</param>
        /// <param name="onlyTopLevel"> Bool to check if required to return reference variables in sub statements as well</param>
        /// <returns></returns>
        public static List<string> GetReferencedVariableNames(Statement s, bool onlyTopLevel)
        {
            var names = s.referencedVariables.Select(refVar => refVar.Name).ToList();
            if (!onlyTopLevel)
            {
                foreach (Statement subStatement in s.subStatements)
                    names.AddRange(GetReferencedVariableNames(subStatement, onlyTopLevel));
            }
            return names;
        }

        /// <summary>
        ///     Returns the names of the variables that have been declared in the statement
        /// </summary>
        /// <param name="s"> Statement whose variable names to be got.</param>
        /// <param name="onlyTopLevel"> Bool to check if required to return reference variables in sub statements as well</param>
        /// <returns></returns>
        public static List<string> GetDefinedVariableNames(Statement s, bool onlyTopLevel)
        {
            var names = s.definedVariables.Select(refVar => refVar.Name).ToList();
            if (!onlyTopLevel)
            {
                foreach (Statement subStatement in s.subStatements)
                    names.AddRange(GetReferencedVariableNames(subStatement, onlyTopLevel));
            }
            return names;
        }

        public static StatementType GetStatementType(Node astNode)
        {
            if (astNode is FunctionDefinitionNode)
                return StatementType.FuncDeclaration;
            if (astNode is BinaryExpressionNode)
            {
                var currentNode = astNode as BinaryExpressionNode;
                if (currentNode.Optr != Operator.assign)
                    throw new ArgumentException();
                if (!(currentNode.LeftNode.Name.StartsWith("temp") && currentNode.LeftNode.Name.Length > 10))
                    return StatementType.Expression;
                if (currentNode.RightNode is IdentifierNode)
                    return StatementType.AssignmentVar;
                if (currentNode.RightNode is ExprListNode)
                    return StatementType.Collection;
                if (currentNode.RightNode is DoubleNode || currentNode.RightNode is IntNode)
                    return StatementType.Literal;
                if (currentNode.RightNode is StringNode)
                    return StatementType.Literal;
            }
            return StatementType.None;
        }

        public static IdentifierNode GetDefinedIdentifier(Node leftNode)
        {
            if (leftNode is IdentifierNode)
                return leftNode as IdentifierNode;
            else if (leftNode is IdentifierListNode)
                return GetDefinedIdentifier((leftNode as IdentifierListNode).LeftNode);
            else if (leftNode is FunctionCallNode)
                return null;
            else
                throw new ArgumentException("Left node type incorrect");
        }
        #endregion

        #region Properties

        public int StartLine { get; private set; }
        public int EndLine { get; private set; }

        public Variable FirstDefinedVariable
        {
            get { return definedVariables.FirstOrDefault(); }
        }

        public State CurrentState { get; private set; }
        public StatementType CurrentType { get; private set; }
        public Node AstNode { get; private set; }

        #endregion

        #region Private Methods

        private Statement(Node parsedNode)
        {
            StartLine = parsedNode.line;
            EndLine = parsedNode.endLine;
            CurrentType = GetStatementType(parsedNode);
            this.AstNode = parsedNode;

            if (parsedNode is BinaryExpressionNode)
            {
                //First get all the defined variables
                while (parsedNode is BinaryExpressionNode)
                {
                    IdentifierNode assignedVar = GetDefinedIdentifier((parsedNode as BinaryExpressionNode).LeftNode);
                    if (assignedVar != null)
                        definedVariables.Add(new Variable(assignedVar));
                    parsedNode = (parsedNode as BinaryExpressionNode).RightNode;
                }

                //Then get the referenced variables
                List<Variable> refVariableList = new List<Variable>();
                GetReferencedVariables(parsedNode, refVariableList);
                referencedVariables = refVariableList;
            }
            else if (parsedNode is FunctionDefinitionNode)
            {
                // Handle function definitions in CBN
            }
            else
                throw new ArgumentException("Must be func def or assignment");

            Variable.SetCorrectColumn(referencedVariables, CurrentType, StartLine);
        }

        #endregion
    }


    public class Variable
    {
        public int Row { get; private set; }
        public int StartColumn { get; private set; }

        public int EndColumn
        {
            get { return StartColumn + Name.Length; }
        }

        public string Name { get; private set; }

        #region Private Methods

        private void MoveColumnBack(int line)
        {
            //Move the column of the variable back only if it is on the same line
            //as the fake variable
            if (Row == line)
                StartColumn -= 13;
        }

        #endregion

        #region Public Methods

        public Variable(IdentifierNode identNode)
        {
            if (identNode == null)
                throw new ArgumentNullException();

            Name = identNode.ToString();
            Row = identNode.line;
            StartColumn = identNode.col;
        }

        public Variable(string name, int line)
        {
            Name = name;
            Row = line;
        }

        public static void SetCorrectColumn(List<Variable> refVar, Statement.StatementType type, int line)
        {
            if (refVar == null)
                return;
            if (type != Statement.StatementType.Expression)
            {
                foreach (Variable singleVar in refVar)
                    singleVar.MoveColumnBack(line);
            }
        }

        #endregion
    }
}
