using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Xml;
using GraphToDSCompiler;
using ProtoCore.AST.AssociativeAST;

using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.BuildData;
using ArrayNode = ProtoCore.AST.AssociativeAST.ArrayNode;
using Node = ProtoCore.AST.Node;
using Operator = ProtoCore.DSASM.Operator;
using ProtoCore.Utils;
using System.Text;

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
        private readonly DynamoLogger logger;

        private struct Formatting
        {
            public const double InitialMargin = 7;
            public const double VerticalMargin = 26;
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
            get { return (State == ElementState.Error) ? null : previewVariable; }
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

                        ClearError();
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
        public List<string> TempVariables
        {
            get { return tempVariables; }
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

        private void ProcessCodeDirect()
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

            ClearError();
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
                if (GraphToDSCompiler.GraphUtilities.PreCompileCodeBlock(parseParam))
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
            this.identifier = null; // Reset preview identifier for regeneration.
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
            // Get all defined variables and their locations
            var definedVars = codeStatements.Select(s => new KeyValuePair<Variable, int>(s.FirstDefinedVariable, s.StartLine))
                                            .Where(pair => pair.Key != null)
                                            .Select(pair => new KeyValuePair<string, int>(pair.Key.Name, pair.Value))
                                            .OrderBy(pair => pair.Key)
                                            .GroupBy(pair => pair.Key);

            // Calc each variable's last location of definition
            var locationMap = new Dictionary<string, int>();
            foreach (var defs in definedVars)
            {
                var name = defs.FirstOrDefault().Key;
                var loc = defs.Select(p => p.Value).Max<int>();
                locationMap[name] = loc;
            }

            // Create output ports
            var allDefs = locationMap.OrderBy(p => p.Value);
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
                portCoordsY += visualIndex * Formatting.VerticalMargin;
                OutPortData.Add(new PortData(string.Empty, tooltip)
                {
                    VerticalMargin = portCoordsY - prevPortBottom
                });

                // Since we compute the "delta" between the top of the current 
                // port to the bottom of the previous port, we need to record 
                // down the bottom coordinate value before proceeding to the next 
                // port.
                // 
                prevPortBottom = portCoordsY + Formatting.VerticalMargin;
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
                    && !identNode.Equals(this.identifier))
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

    internal class CodeBlockParser
    {
        private ProtoCore.Core core;

        public CodeBlockParser(ProtoCore.Core core)
        {
            this.core = core;
        }

        /// <summary>
        /// Pre-compiles DS code in code block node, 
        /// checks for syntax, converts non-assignments to assignments,
        /// stores list of AST nodes, errors and warnings
        /// Evaluates and stores list of unbound identifiers
        /// </summary>
        /// <param name="parseParams"></param>
        /// <returns></returns>
        public bool PreCompileCodeBlock(ParseParam parseParams)
        {
            string postfixGuid = parseParams.PostfixGuid.ToString().Replace("-", "_");

            // Parse code to generate AST and add temporaries to non-assignment nodes
            IEnumerable<ProtoCore.AST.Node> astNodes = ParseUserCode(parseParams.OriginalCode, postfixGuid);

            // Catch the syntax errors and errors for unsupported 
            // language constructs thrown by compile expression
            if (core.BuildStatus.ErrorCount > 0)
            {
                parseParams.AppendErrors(core.BuildStatus.Errors);
                parseParams.AppendWarnings(core.BuildStatus.Warnings);
                return false;
            }
            parseParams.AppendParsedNodes(astNodes);

            // Compile the code to get the resultant unboundidentifiers  
            // and any errors or warnings that were caught by the compiler and cache them in parseParams
            return CompileCodeBlockAST(parseParams);
        }

        private bool CompileCodeBlockAST(ParseParam parseParams)
        {
            Dictionary<int, List<VariableLine>> unboundIdentifiers = new Dictionary<int, List<VariableLine>>();
            IEnumerable<ProtoCore.BuildData.WarningEntry> warnings = null;

            ProtoCore.BuildStatus buildStatus = null;
            try
            {
                BuildCore(true);
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                CodeBlockNode codeblock = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
                List<AssociativeNode> nodes = new List<AssociativeNode>();
                foreach (var i in parseParams.ParsedNodes)
                {
                    AssociativeNode assocNode = i as AssociativeNode;

                    if (assocNode != null)
                        nodes.Add(NodeUtils.Clone(assocNode));
                }
                codeblock.Body.AddRange(nodes);

                buildStatus = CompilerUtils.PreCompile(string.Empty, core, codeblock, out blockId);

                parseParams.AppendErrors(buildStatus.Errors);
                parseParams.AppendWarnings(buildStatus.Warnings);

                if (buildStatus.ErrorCount > 0)
                {
                    return false;
                }
                warnings = buildStatus.Warnings;

                // Get the unboundIdentifiers from the warnings
                GetInputLines(parseParams.ParsedNodes, warnings, unboundIdentifiers);
                foreach (KeyValuePair<int, List<VariableLine>> kvp in unboundIdentifiers)
                {
                    foreach (VariableLine vl in kvp.Value)
                        parseParams.AppendUnboundIdentifier(vl.variable);
                }

                return true;
            }
            catch (Exception)
            {
                buildStatus = null;
                return false;
            }
        }
 
        private IEnumerable<ProtoCore.AST.Node> ParseUserCode(string expression, string postfixGuid)
        {
            IEnumerable<ProtoCore.AST.Node> astNodes = new List<ProtoCore.AST.Node>();

            if (expression == null)
                return astNodes;

            expression = expression.Replace("\r\n", "\n");

            bool parseSuccess = false;
            try
            {
                return ParseUserCodeCore(expression, postfixGuid, ref parseSuccess);
            }
            catch
            {
                // For modifier blocks, language blocks, etc. that are currently ignored
                if (parseSuccess)
                    return astNodes;

                // Reset core above as we don't wish to propagate these errors - pratapa
                core.ResetForPrecompilation();

                // Use manual parsing for invalid functional associative statement errors like for "a+b;"
                return ParseNonAssignments(expression, postfixGuid);
            }
        }

        private IEnumerable<ProtoCore.AST.Node> ParseUserCodeCore(string expression, string postfixGuid, ref bool parseSuccess)
        {
            List<ProtoCore.AST.Node> astNodes = new List<ProtoCore.AST.Node>();

            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode = null;
            ProtoCore.AST.Node codeBlockNode = Parse(expression, out commentNode);
            parseSuccess = true;
            List<ProtoCore.AST.Node> nodes = ParserUtils.GetAstNodes(codeBlockNode);
            Validity.Assert(nodes != null);

            int index = 0;
            foreach (var node in nodes)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode n = node as ProtoCore.AST.AssociativeAST.AssociativeNode;
                ProtoCore.Utils.Validity.Assert(n != null);

                // Append the temporaries only if it is not a function def or class decl
                bool isFunctionOrClassDef = n is FunctionDefinitionNode || n is ClassDeclNode;

                // Handle non Binary expression nodes separately
                if (n is ProtoCore.AST.AssociativeAST.ModifierStackNode)
                {
                    core.BuildStatus.LogSemanticError("Modifier Blocks are not supported currently.");
                }
                else if (n is ProtoCore.AST.AssociativeAST.ImportNode)
                {
                    core.BuildStatus.LogSemanticError("Import statements are not supported in CodeBlock Nodes.");
                }
                else if (isFunctionOrClassDef)
                {
                    // Add node as it is
                    astNodes.Add(node);
                }
                else
                {
                    // Handle temporary naming for temporary Binary exp. nodes and non-assignment nodes
                    BinaryExpressionNode ben = node as BinaryExpressionNode;
                    if (ben != null && ben.Optr == ProtoCore.DSASM.Operator.assign)
                    {
                        ModifierStackNode mNode = ben.RightNode as ModifierStackNode;
                        if (mNode != null)
                        {
                            core.BuildStatus.LogSemanticError("Modifier Blocks are not supported currently.");
                        }
                        IdentifierNode lNode = ben.LeftNode as IdentifierNode;
                        if (lNode != null && lNode.Value == ProtoCore.DSASM.Constants.kTempProcLeftVar)
                        {
                            string name = string.Format("temp_{0}_{1}", index++, postfixGuid);
                            BinaryExpressionNode newNode = new BinaryExpressionNode(new IdentifierNode(name), ben.RightNode);
                            astNodes.Add(newNode);
                        }
                        else
                        {
                            // Add node as it is
                            astNodes.Add(node);
                        }
                    }
                    else
                    {
                        // These nodes are non-assignment nodes
                        string name = string.Format("temp_{0}_{1}", index++, postfixGuid);
                        BinaryExpressionNode newNode = new BinaryExpressionNode(new IdentifierNode(name), n);
                        astNodes.Add(newNode);
                    }
                }
            }
            return astNodes;
        }

        private IEnumerable<ProtoCore.AST.Node> ParseNonAssignments(string expression, string postfixGuid)
        {
            List<string> compiled = new List<string>();

            string[] expr = GetStatementsString(expression);
            foreach (string s in expr)
                compiled.Add(s);

            for (int i = 0; i < compiled.Count(); i++)
            {
                if (compiled[i].StartsWith("\n"))
                {
                    string newlines = string.Empty;
                    int lastPosButOne = 0;
                    string original = compiled[i];
                    for (int j = 0; j < original.Length; j++)
                    {
                        if (!original[j].Equals('\n'))
                        {
                            lastPosButOne = j;
                            break;
                        }
                        else
                            newlines += original[j];
                    }
                    string newStatement = original.Substring(lastPosButOne);

                    if (!IsNotAssigned(newStatement))
                    {
                        string name = string.Format("temp_{0}_{1}", i, postfixGuid);
                        newStatement = name + " = " + newStatement;
                    }
                    compiled[i] = newlines + newStatement;
                }
                else
                {
                    if (!IsNotAssigned(compiled[i]))
                    {
                        string name = string.Format("temp_{0}_{1}", i, postfixGuid);
                        compiled[i] = name + " = " + compiled[i];
                    }
                }
            }
            StringBuilder newCode = new StringBuilder();
            compiled.ForEach(x => newCode.Append(x));
            CodeBlockNode commentNode = null;

            try
            {
                ProtoCore.AST.Node codeBlockNode = Parse(newCode.ToString(), out commentNode);
                return ParserUtils.GetAstNodes(codeBlockNode);
            }
            catch (Exception)
            {
                return new List<ProtoCore.AST.Node>();
            }
        }

        /*Given a block of code that has only usual statements and Modifier Stacks*/
        private static string[] GetStatementsString(string input)
        {
            var expr = new List<string>();

            expr.AddRange(GetBinaryStatementsList(input));
            return expr.ToArray();
        }

        /*attempt*/
        /*Given a block of code that has only usual binary statements*/
        private static List<string> GetBinaryStatementsList(string input)
        {
            var expr = new List<string>();
            int index = 0;
            int oldIndex = 0;
            do
            {
                index = input.IndexOf(";", oldIndex);
                if (index != -1)
                {
                    string sub;
                    if (index < input.Length - 1)
                    {
                        if (input[index + 1].Equals('\n'))
                            index += 1;
                    }
                    sub = input.Substring(oldIndex, index - oldIndex + 1);
                    expr.Add(sub);
                    //index++;
                    oldIndex = index + 1;
                }
            } while (index != -1);
            return expr;
        }

        private bool IsNotAssigned(string code)
        {
            code = code.Trim(';', ' ');
            if (string.IsNullOrEmpty(code))
                return true;
            bool hasLHS = code.Contains("=");
            return hasLHS;
        }

        private void GetInputLines(IEnumerable<ProtoCore.AST.Node> astNodes, 
                                   IEnumerable<ProtoCore.BuildData.WarningEntry> warnings,
                                   Dictionary<int, List<VariableLine>> inputLines)
        {
            List<VariableLine> warningVLList = GetVarLineListFromWarning(warnings);

            if (warningVLList.Count == 0)
                return;

            int stmtNumber = 1;
            foreach (var node in astNodes)
            {
                // Only binary expression need warnings. 
                // Function definition nodes do not have input and output ports
                if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                {
                    List<VariableLine> variableLineList = new List<VariableLine>();
                    foreach (var warning in warningVLList)
                    {
                        if (warning.line >= node.line && warning.line <= node.endLine)
                            variableLineList.Add(warning);
                    }

                    if (variableLineList.Count > 0)
                    {
                        inputLines.Add(stmtNumber, variableLineList);
                    }
                    stmtNumber++;
                }
            }
        }

        private List<VariableLine> GetVarLineListFromWarning(IEnumerable<ProtoCore.BuildData.WarningEntry> warnings)
        {
            List<VariableLine> result = new List<VariableLine>();
            foreach (ProtoCore.BuildData.WarningEntry warningEntry in warnings)
            {
                if (warningEntry.ID == ProtoCore.BuildData.WarningID.kIdUnboundIdentifier)
                {
                    result.Add(new VariableLine()
                    {
                        variable = warningEntry.Message.Split(' ')[1].Replace("'", ""),
                        line = warningEntry.Line
                    });
                }
            }
            return result;
        }
    }
}
