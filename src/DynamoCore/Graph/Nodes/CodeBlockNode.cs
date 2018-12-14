using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Connectors;
using Dynamo.Migration;
using Dynamo.Properties;
using Dynamo.Utilities;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.Namespace;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;
using ProtoCore.AST;
using ArrayNode = ProtoCore.AST.AssociativeAST.ArrayNode;
using Node = ProtoCore.AST.Node;
using Operator = ProtoCore.DSASM.Operator;
using Newtonsoft.Json;
using ProtoCore.DSASM;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    ///     Represents codeblock node's functionality.
    /// </summary>
    [NodeName("Code Block")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("CodeBlockDescription", typeof(Dynamo.Properties.Resources))]
    [NodeSearchTags("CodeBlockSearchTags", typeof(Dynamo.Properties.Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.CodeBlockNodeModel")]
    public class CodeBlockNodeModel : NodeModel
    {
        private readonly List<Statement> codeStatements = new List<Statement>();
        private string code = string.Empty;
        private List<string> inputIdentifiers = new List<string>();
        private List<string> inputPortNames = new List<string>();
        private string previewVariable;
        private readonly LibraryServices libraryServices;

        private bool shouldFocus = true;

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "CodeBlockNode";
            }
        }
        /// <summary>
        ///     Indicates whether code block should not be in focus upon undo/redo actions on node
        /// </summary>
        [JsonIgnore]
        public bool ShouldFocus
        {
            get { return shouldFocus; }
            internal set { shouldFocus = value; }
        }

        /// <summary>
        ///     Returns <see cref="ElementResolver"/> for CodeBlock node
        /// </summary>
        [JsonIgnore]
        public ElementResolver ElementResolver { get; set; }

        /// <summary>
        ///     Indicates whether node is input node.
        ///     Used to bind visibility of UI for user selection.
        /// </summary>
        public override bool IsInputNode
        {
            get { return false; }
        }

        /// <summary>
        ///     Indicates whether node is an output node.
        ///     Used to bind visibility of UI for user selection.
        /// </summary>
        public override bool IsOutputNode
        {
            get { return false; }
        }

        #region Public Methods

        /// <summary>
        ///     Initilizes a new instance of the <see cref="CodeBlockNodeModel"/> class
        /// </summary>
        /// <param name="libraryServices"><see cref="LibraryServices"/> object to manage
        ///  builtin libraries as well as imported libraries</param>
        public CodeBlockNodeModel(LibraryServices libraryServices)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            this.libraryServices = libraryServices;
            this.ElementResolver = new ElementResolver();

            ProcessCodeDirect();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeBlockNodeModel"/> class
        /// </summary>
        /// <param name="code">Code block content</param>
        /// <param name="x">X coordinate of the code block</param>
        /// <param name="y">Y coordinate of the code block</param>
        /// <param name="libraryServices"><see cref="LibraryServices"/> object to manage
        ///  builtin libraries as well as imported libraries</param>
        /// <param name="resolver">Responsible for resolving 
        /// a partial class name to its fully resolved name</param>
        public CodeBlockNodeModel(string code, double x, double y, LibraryServices libraryServices, ElementResolver resolver)
            : this(code, Guid.NewGuid(), x, y, libraryServices, resolver) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeBlockNodeModel"/> class
        /// </summary>
        /// <param name="userCode">Code block content</param>
        /// <param name="guid">Identifier of the code block</param>
        /// <param name="xPos">X coordinate of the code block</param>
        /// <param name="yPos">Y coordinate of the code block</param>
        /// <param name="libraryServices"><see cref="LibraryServices"/> object to manage
        ///  builtin libraries as well as imported libraries</param>
        /// <param name="resolver">Responsible for resolving 
        /// a partial class name to its fully resolved name</param>
        public CodeBlockNodeModel(string userCode, Guid guid, double xPos, double yPos, LibraryServices libraryServices, ElementResolver resolver)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            X = xPos;
            Y = yPos;
            this.libraryServices = libraryServices;
            this.ElementResolver = resolver;
            GUID = guid;
            ShouldFocus = false;
            this.code = userCode;

            ProcessCodeDirect();
        }

        /// <summary>
        ///     It removes all the in ports and out ports so that the user knows there is an error.
        /// </summary>
        private void ProcessError()
        {
            previewVariable = null;
        }

        /// <summary>
        ///     Returns the names of all the variables defined in this code block.
        /// </summary>
        /// <returns>List containing all the names</returns>
        internal List<string> GetDefinedVariableNames()
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
                defVarNames.AddRange(Statement.GetDefinedVariableNames(stmnt));
            }

            return defVarNames;
        }

        /// <summary>
        /// Returns the index of the port corresponding to the variable name given
        /// </summary>
        /// <param name="cbn"></param>
        /// <param name="variableName"> Name of the variable corresponding to an input port </param>
        /// <returns> Index of the required port in the InPorts collection </returns>
        internal static int GetInportIndex(CodeBlockNodeModel cbn, string variableName)
        {
            return cbn.inputIdentifiers.IndexOf(variableName);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     If this node is allowed to be converted to AST node in nodes to code conversion.
        /// </summary>
        public override bool IsConvertible
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Code block node displays the value
        ///     of the left hand side variable of last statement.
        /// </summary>
        public override string AstIdentifierBase
        {
            get
            {
                return previewVariable ?? base.AstIdentifierBase;
            }
        }

        /// <summary>
        ///     Returns string content of CodeBlock node.
        /// </summary>
        public string Code
        {
            get { return code; }
            private set { code = value; }
        }

        /// <summary>
        /// Sets string content of CodeBlock node.
        /// </summary>
        /// <param name="newCode">New content of the code block</param>
        /// <param name="workspaceElementResolver"><see cref="ElementResolver"/> object</param>
        public void SetCodeContent(string newCode, ElementResolver workspaceElementResolver)
        {
            if (code != null && code.Equals(newCode))
                return;

            if (newCode == null)
                code = null;
            else
            {
                string errorMessage = string.Empty;
                string warningMessage = string.Empty;

                var inportConnections = new OrderedDictionary();
                var outportConnections = new OrderedDictionary();

                // disable node modification evnets while mutating the code
                this.OnRequestSilenceModifiedEvents(true);

                //Save the connectors so that we can recreate them at the correct positions.
                SaveAndDeleteConnectors(inportConnections, outportConnections);

                code = newCode;
                ProcessCode(ref errorMessage, ref warningMessage, workspaceElementResolver);

                //Recreate connectors that can be reused
                LoadAndCreateConnectors(inportConnections, outportConnections, SaveContext.None);

                RaisePropertyChanged("Code");

                ReportPosition();

                ClearErrorsAndWarnings();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Error(errorMessage);
                }
                else if (!string.IsNullOrEmpty(warningMessage))
                {
                    // Build warnings must persist so that they are not cleared by runtime warnings
                    Warning(warningMessage, isPersistent: true);
                }

                this.OnRequestSilenceModifiedEvents(false);

                // Mark node for update
                OnNodeModified();
            }
        }


        /// <summary>
        /// Code statement of CBN
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Statement> CodeStatements => codeStatements;

        #endregion

        #region Protected Methods

        /// <summary>
        /// If a CBN is in Error state, it will have no code but will have output ports
        /// from the last successful compilation if any. 
        /// In this case it should continue to be in Error state.
        /// </summary>
        protected override void SetNodeStateBasedOnConnectionAndDefaults()
        {
            if (!CodeStatements.Any() && OutPorts.Any())
                State = ElementState.Error;
            else
                base.SetNodeStateBasedOnConnectionAndDefaults();
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;
            ElementResolver workspaceElementResolver = updateValueParams.ElementResolver;

            if (name != "Code")
                return base.UpdateValueCore(updateValueParams);

            value = CodeBlockUtils.FormatUserText(value);

            if (!value.Equals(Code))
                SetCodeContent(value, workspaceElementResolver);

            return true;
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("CodeText", code);
            helper.SetAttribute("ShouldFocus", shouldFocus);

            // add input port names to port info
            var childNodes = element.ChildNodes.Cast<XmlElement>().ToList();
            var inPorts = childNodes.Where(node => node.Name.Equals("PortInfo"));
            foreach (var tuple in inPorts.Zip(InPorts, Tuple.Create))
            {
                tuple.Item1.SetAttribute("name", tuple.Item2.Name);
            }

            //write output port line number info
            foreach (var t in OutPorts)
            {
                XmlElement outportInfo = element.OwnerDocument.CreateElement("OutPortInfo");
                outportInfo.SetAttribute("LineIndex", t.LineIndex.ToString(CultureInfo.InvariantCulture));
                element.AppendChild(outportInfo);
            }
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            var helper = new XmlElementHelper(nodeElement);
            shouldFocus = helper.ReadBoolean("ShouldFocus");
            code = helper.ReadString("CodeText");

            var inportConnections = new OrderedDictionary();
            var outportConnections = new OrderedDictionary();

            //before the refactor here: https://github.com/DynamoDS/Dynamo/pull/7301
            //we didn't actually make new portModels we just updated them, 
            //but after this PR we remove the data property of ports,
            //so now new models are created instead,
            //so we have to delete and create new connectors to go along with those ports.
            SaveAndDeleteConnectors(inportConnections, outportConnections);

            var childNodes = nodeElement.ChildNodes.Cast<XmlElement>().ToList();
            var inputPortHelpers =
                childNodes.Where(node => node.Name.Equals("PortInfo")).Select(x => new XmlElementHelper(x));


            // set the inputPorts and outputPorts incase this node is in an error state after processing code.
            // read and set input port info.
            inputPortNames =
                inputPortHelpers.Select(x => x.ReadString("name", String.Empty))
                    .Where(y => !string.IsNullOrEmpty(y))
                    .ToList();
            SetInputPorts();

            // if we're in an error state - clear the output ports before we try adding more.
            if (IsInErrorState)
            {
                OutPorts.RemoveAll((p) => { return true; });
            }
            var outputPortHelpers =
                childNodes.Where(node => node.Name.Equals("OutPortInfo")).Select(x => new XmlElementHelper(x));
            var lineNumbers = outputPortHelpers.Select(x => x.ReadInteger("LineIndex")).ToList();
            foreach (var line in lineNumbers)
            {
                var tooltip = string.Format(Resources.CodeBlockTempIdentifierOutputLabel, line);
                OutPorts.Add(new PortModel(PortType.Output, this, new PortData(string.Empty, tooltip)
                {
                    LineIndex = line, // Logical line index.
                    Height = Configurations.CodeBlockPortHeightInPixels
                }));
            }

            ProcessCodeDirect();
            //Recreate connectors that can be reused
            LoadAndCreateConnectors(inportConnections, outportConnections, SaveContext.Undo);
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            //Do not build if the node is in error.
            if (State == ElementState.Error)
            {
                return Enumerable.Empty<AssociativeNode>();
            }

            var identMapper = new IdentifierInPlaceMapper(libraryServices.LibraryManagementCore, ShouldBeRenamed, LocalizeIdentifier);
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
                        if (context != CompilationContext.NodeToCode)
                            identNode.Accept(identMapper);
                        return AstFactory.BuildAssignment(identNode, rhs);
                    });
                resultNodes.AddRange(initStatments);
            }

            foreach (var astNode in codeStatements.Select(stmnt => NodeUtils.Clone(stmnt.AstNode)))
            {
                if (context != CompilationContext.NodeToCode)
                {
                    (astNode as AssociativeNode).Accept(identMapper);
                }
                resultNodes.Add(astNode as AssociativeNode);
            }

            return resultNodes;
        }

        private Statement GetStatementForOutput(int portIndex)
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
            var svs = CodeBlockUtils.GetStatementVariablesForOutports(codeStatements);
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

            return statement;
        }

        /// <summary>
        /// For code block nodes, each output identifier of an output port is mapped.
        /// For an example, "p = 1" would have its internal identifier renamed to 
        /// "pXXXX", where "XXXX" is the GUID of the code block node. This mapping is 
        /// done to ensure the uniqueness of the output variable name.
        /// </summary>
        /// <param name="portIndex">Output port index</param>
        /// <param name="forRawName">Set this parameter to true to retrieve the 
        /// original identifier name "p". If this parameter is false, the mapped 
        /// identifer name "pXXXX" is returned instead.</param>
        /// <returns></returns>
        private IdentifierNode GetAstIdentifierForOutputIndexInternal(int portIndex, bool forRawName)
        {
            var statement = GetStatementForOutput(portIndex);
            if (statement == null)
                return null;

            var binExprNode = statement.AstNode as BinaryExpressionNode;
            if (binExprNode == null || (binExprNode.LeftNode == null))
                return null;

            var identNode = binExprNode.LeftNode as IdentifierNode;
            if (identNode == null)
                return null;

            var mappedIdent = NodeUtils.Clone(identNode);

            if (!forRawName)
            {
                var identMapper = new IdentifierInPlaceMapper(libraryServices.LibraryManagementCore, ShouldBeRenamed, LocalizeIdentifier);
                mappedIdent.Accept(identMapper);
            }

            return mappedIdent as IdentifierNode;
        }

        /// <summary>
        ///     Fetches the ProtoAST Identifier for a given output index.
        /// </summary>
        /// <param name="outputIndex">Index of the output port.</param>
        /// <returns>Identifier corresponding to the given output port.</returns>
        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return GetAstIdentifierForOutputIndexInternal(outputIndex, false);
        }

        /// <summary>
        ///     Fetches the raw ProtoAST Identifier for a given index.
        /// </summary>
        /// <param name="portIndex">Index of the port.</param>
        /// <returns>Identifier corresponding to the given port</returns>
        public IdentifierNode GetRawAstIdentifierForOutputIndex(int portIndex)
        {
            return GetAstIdentifierForOutputIndexInternal(portIndex, true);
        }

        /// <summary>
        /// Returns possible type of the output at specified output port.
        /// </summary>
        /// <param name="index">Index of the port</param>
        /// <returns>The type</returns>
        public override ProtoCore.Type GetTypeHintForOutput(int index)
        {
            ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);
            var statement = GetStatementForOutput(index);
            if (statement == null)
                return type;

            BinaryExpressionNode expr = statement.AstNode as BinaryExpressionNode;
            if (expr == null || expr.Optr != Operator.assign)
                return type;

            var core = libraryServices.LibraryManagementCore;

            if (expr.RightNode is IdentifierListNode)
            {
                var identListNode = expr.RightNode as IdentifierListNode;
                var funcNode = identListNode.RightNode as FunctionCallNode;
                if (funcNode == null)
                    return type;

                string fullyQualifiedName = CoreUtils.GetIdentifierExceptMethodName(identListNode);
                if (string.IsNullOrEmpty(fullyQualifiedName))
                    return type;

                var classIndex = core.ClassTable.IndexOf(fullyQualifiedName);
                if (classIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                    return type;

                var targetClass = core.ClassTable.ClassNodes[classIndex];
                var func = targetClass.GetFirstMemberFunctionBy(funcNode.Function.Name);
                type = func.ReturnType;
                return type;
            }
            else if (expr.RightNode is FunctionCallNode)
            {
                var functionCallNode = expr.RightNode as FunctionCallNode;
                ProtoCore.FunctionGroup funcGroup;
                var funcTable = core.FunctionTable.GlobalFuncTable[0];
                if (funcTable.TryGetValue(functionCallNode.Function.Name, out funcGroup))
                {
                    var func = funcGroup.FunctionEndPoints.FirstOrDefault();
                    if (func != null)
                        return func.procedureNode.ReturnType;
                }
            }
            else if (expr.RightNode is IntNode)
                return TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer);
            else if (expr.RightNode is StringNode)
                return TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String);
            else if (expr.RightNode is DoubleNode)
                return TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double);
            else if (expr.RightNode is StringNode)
                return TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String);

            return type;
        }

        #endregion

        #region Private Methods

        internal void ProcessCodeDirect()
        {
            var errorMessage = string.Empty;
            var warningMessage = string.Empty;

            ProcessCode(ref errorMessage, ref warningMessage);
            RaisePropertyChanged("Code");

            ClearErrorsAndWarnings();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Error(errorMessage);
            }
            else if (!string.IsNullOrEmpty(warningMessage))
            {
                // Build warnings must persist so that they are not cleared by runtime warnings
                Warning(warningMessage, isPersistent: true);
            }

            // Mark node for update
            OnNodeModified();
        }

        private void ProcessCode(ref string errorMessage, ref string warningMessage,
            ElementResolver workspaceElementResolver = null)
        {
            code = CodeBlockUtils.FormatUserText(code);
            codeStatements.Clear();

            if (string.IsNullOrEmpty(Code))
                previewVariable = null;

            try
            {
                // During loading of CBN from file, the elementResolver from the workspace is unavailable
                // in which case, a local copy of the ER obtained from the CBN is used
                var resolver = workspaceElementResolver ?? this.ElementResolver;
                var parseParam = new ParseParam(GUID, code, resolver);
                var priorNames = libraryServices.GetPriorNames();

                if (CompilerUtils.PreCompileCodeBlock(libraryServices.LibraryManagementCore, ref parseParam, priorNames))
                {
                    if (parseParam.ParsedNodes != null)
                    {
                        // Create an instance of statement for each code statement written by the user
                        foreach (var parsedNode in parseParam.ParsedNodes)
                        {
                            // Create a statement variable from the generated nodes
                            codeStatements.Add(Statement.CreateInstance(parsedNode));
                        }
                    }
                }

                if (parseParam.Errors.Any())
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
                    var warnings =
                        parseParam.Warnings.Where(
                            w =>
                                w.ID != WarningID.IdUnboundIdentifier
                                    && w.ID != WarningID.FunctionAlreadyDefined);

                    if (warnings.Any())
                    {
                        warningMessage = string.Join("\n", warnings.Select(m => m.Message));
                    }
                }

                if (parseParam.UnboundIdentifiers != null)
                {
                    inputIdentifiers = new List<string>();
                    inputPortNames = new List<string>();

                    var definedVariables = new HashSet<string>(CodeBlockUtils.GetStatementVariables(codeStatements).SelectMany(s => s));
                    foreach (var kvp in parseParam.UnboundIdentifiers)
                    {
                        if (!definedVariables.Contains(kvp.Value))
                        {
                            inputIdentifiers.Add(kvp.Value);
                            inputPortNames.Add(kvp.Key);
                        }
                    }
                }
                else
                {
                    inputIdentifiers.Clear();
                    inputPortNames.Clear();
                }

                // Set preview variable after gathering input identifiers. As a variable
                // will be renamed only if it is not the preview variable and is either a
                // variable defined in code block node or in on the right hand side of 
                // expression as an input variable, a variable may not be renamed properly
                // if SetPreviewVariable() is called before gathering input identifiers.
                SetPreviewVariable(parseParam.ParsedNodes);
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

        private static bool IsTempIdentifier(string name)
        {
            return name.StartsWith(Constants.kTempVarForNonAssignment);
        }

        private void SetPreviewVariable(IEnumerable<Node> parsedNodes)
        {
            previewVariable = null;
            if (parsedNodes == null || (!parsedNodes.Any()))
                return;

            IdentifierNode identifierNode = null;
            foreach (var statement in parsedNodes.Reverse().OfType<BinaryExpressionNode>())
            {
                identifierNode = statement.LeftNode as IdentifierNode;
                if (identifierNode != null) // Found the identifier...
                {
                    break;
                }
            }

            if (identifierNode == null)
                return;

            var duplicatedNode = new IdentifierNode(identifierNode);
            var identMapper = new IdentifierInPlaceMapper(libraryServices.LibraryManagementCore, ShouldBeRenamed, LocalizeIdentifier);
            duplicatedNode.Accept(identMapper);

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
            // This extension method is used instead because 
            // observableCollection has very odd behavior when cleared - 
            // there is no way to reference the cleared items and so they 
            // cannot be cleaned up properly

            InPorts.RemoveAll((p) => { return true; });

            // Generate input port data list from the unbound identifiers.
            var inportData = CodeBlockUtils.GenerateInputPortData(inputPortNames);
            foreach (var portData in inportData)
                InPorts.Add(new PortModel(PortType.Input, this, portData));
        }

        internal void SetErrorStatePortData(List<string> inputPortNames, List<int> outputPortIndexes)
        {
            if (inputPortNames != null)
            {
                this.inputPortNames = inputPortNames;
            }

            SetInputPorts();

            if (outputPortIndexes != null)
            {
                foreach (var outputPortIndex in outputPortIndexes)
                {
                  var tooltip = string.Format(Resources.CodeBlockTempIdentifierOutputLabel, outputPortIndex);
                  OutPorts.Add(new PortModel(PortType.Output, this, new PortData(string.Empty, tooltip)
                  {
                    LineIndex = outputPortIndex, // Logical line index.
                    Height = Configurations.CodeBlockPortHeightInPixels
                  }));
                }
            }

            SetOutputPorts();
        }

        private void SetOutputPorts()
        {
            var allDefs = CodeBlockUtils.GetDefinitionLineIndexMap(codeStatements);

            if (allDefs.Any() == false)
                return;

            // This extension method is used instead because 
            // observableCollection has very odd behavior when cleared - 
            // there is no way to reference the cleared items and so they 
            // cannot be cleaned up properly
            
            // Clear out all the output port models
            OutPorts.RemoveAll((p) => { return true; });

            foreach (var def in allDefs)
            {
                var tooltip = IsTempIdentifier(def.Key) ? string.Format(Resources.CodeBlockTempIdentifierOutputLabel, def.Value) : def.Key;

                OutPorts.Add(new PortModel(PortType.Output, this, new PortData(string.Empty, tooltip)
                {
                    LineIndex = def.Value - 1, // Logical line index.
                    Height = Configurations.CodeBlockPortHeightInPixels
                }));
            }
        }


        /// <summary>
        ///     Deletes all the connections and saves their data (the start and end port)
        ///     so that they can be recreated if needed.
        /// </summary>
        /// <param name="inportConnections">A list of connections that will be destroyed</param>
        /// <param name="outportConnections"></param>
        private void SaveAndDeleteConnectors(IDictionary inportConnections, IDictionary outportConnections)
        {
            //----------------------------Inputs---------------------------------
            foreach (var portModel in InPorts)
            {
                var portName = portModel.ToolTip;
                if (portModel.Connectors.Count != 0)
                {
                    inportConnections.Add(portName, new List<ConnectorModel>());
                    foreach (var connector in portModel.Connectors)
                    {
                        (inportConnections[portName] as List<ConnectorModel>).Add(connector);
                    }
                }
                else
                    inportConnections.Add(portName, null);
            }

            //Delete the connectors
            foreach (PortModel inport in InPorts)
                inport.DestroyConnectors();

            //----------------------------Outputs---------------------------------
            for (int i = 0; i < OutPorts.Count; i++)
            {
                PortModel portModel = OutPorts[i];
                string portName = portModel.ToolTip;
                if (portModel.Connectors.Count != 0)
                {
                    outportConnections.Add(portName, new List<ConnectorModel>());
                    foreach (ConnectorModel connector in portModel.Connectors)
                    {
                        (outportConnections[portName] as List<ConnectorModel>).Add(connector);
                    }
                }
                else
                    outportConnections.Add(portName, null);
            }

            //Delete the connectors
            foreach (PortModel outport in OutPorts)
                outport.DestroyConnectors();

        }

        /// <summary>
        ///     Now that the portData has been set for the new ports, we recreate the connections we
        ///     so mercilessly destroyed, restoring peace and balance to the world once again.
        /// </summary>
        /// <param name="inportConnections"></param>
        /// <param name="outportConnections"> List of the connections that were killed</param>
        /// <param name="context">context this operation is being performed in</param>
        private void LoadAndCreateConnectors(OrderedDictionary inportConnections, OrderedDictionary outportConnections, SaveContext context)
        {
            //----------------------------Inputs---------------------------------
            /* Input Port connections are matched only if the name is the same */
            for (int i = 0; i < InPorts.Count; i++)
            {
                string varName = InPorts[i].ToolTip;
                if (inportConnections.Contains(varName))
                {
                    if (inportConnections[varName] != null)
                    {
                        foreach (var oldConnector in (inportConnections[varName] as List<ConnectorModel>))
                        {
                            var startPortModel = oldConnector.Start;
                            NodeModel startNode = startPortModel.Owner;
                            var connector = ConnectorModel.Make(
                                startNode,
                                this,
                                startPortModel.Index,
                                i);
                            //during an undo operation we should set the new input connector
                            //to have the same id as the old connector.
                            if (context == SaveContext.Undo)
                            {
                                connector.GUID = oldConnector.GUID;
                            }
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
            for (int i = 0; i < OutPorts.Count; i++)
            {
                // If a code block is in an error state the indexes are not always
                // known (after the code block node is loaded in an error state), 
                // so matching the connector by name can result in the port being 
                // on the wrong line, just store the index to match in step 2 next
                if (IsInErrorState)
                {
                    undefinedIndices.Add(i);
                    continue;
                }

                // Attempting to match the connector by name failed, 
                // store the index to match in step 2 next
                string varName = OutPorts[i].ToolTip;
                if (!outportConnections.Contains(varName))
                {
                    undefinedIndices.Add(i);
                    continue;
                }

                // Attempting to match the connector by name succeeded, 
                // create the connector using the matched port index
                if (outportConnections[varName] != null)
                {
                    foreach (var oldConnector in (outportConnections[varName] as List<ConnectorModel>))
                    {
                        var endPortModel = oldConnector.End;
                        NodeModel endNode = endPortModel.Owner;
                        var connector = ConnectorModel.Make(this, endNode, i, endPortModel.Index);
                        
                        // During an undo operation we should set the new output connector
                        // to have the same id as the old connector.
                        if (context == SaveContext.Undo)
                        {
                            connector.GUID = oldConnector.GUID;
                        }
                    }

                    outportConnections[varName] = null;
                }
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
                    foreach (PortModel endPortModel in (outportConnections[index] as List<ConnectorModel>).Select(connector => connector.End))
                    {
                        NodeModel endNode = endPortModel.Owner;
                        var connector = ConnectorModel.Make(this, endNode, index, endPortModel.Index);
                    }
                    // we do not match the guid here as these ports did not exist before 
                    //...so these should be brand new connectors.
                    outportConnections[index] = null;
                    undefinedIndices.Remove(index);
                    i--;
                }
            }

            /*
             *Step 3:
             *   The final step. Now that the priorties are finished, the 
             *   function tries to reuse any existing connections by attaching 
             *   them to any ports that have not already been given connections
             */
            List<List<PortModel>> unusedConnections =
                outportConnections.Values.Cast<List<ConnectorModel>>()
                    .Where(connectorList => connectorList != null).Select(list => list.Select(x => x.End).ToList()).ToList();


            while (undefinedIndices.Count > 0 && unusedConnections.Count != 0)
            {
                foreach (PortModel endPortModel in unusedConnections[0])
                {
                    NodeModel endNode = endPortModel.Owner;
                    ConnectorModel connector = ConnectorModel.Make(
                        this,
                        endNode,
                        undefinedIndices[0],
                        endPortModel.Index);

                    // we do not match the guid here as these ports did not exist before 
                    //...so these should be brand new connectors.
                }
                undefinedIndices.RemoveAt(0);
                unusedConnections.RemoveAt(0);
            }
        }

        private bool ShouldBeRenamed(string ident)
        {
            return !ident.Equals(AstIdentifierForPreview.Value) && GetDefinedVariableNames().Contains(ident);
        }

        private string LocalizeIdentifier(string identifierName)
        {
            return string.Format("{0}_{1}", identifierName, AstIdentifierGuid);
        }

        private class ImperativeIdentifierInPlaceMapper : ImperativeAstReplacer
        {
            private ProtoCore.Core core;
            private Func<string, bool> cond;
            private Func<string, string> mapper;

            public ImperativeIdentifierInPlaceMapper(ProtoCore.Core core, Func<string, bool> cond, Func<string, string> mapper)
            {
                this.core = core;
                this.cond = cond;
                this.mapper = mapper;
            }

            public override ProtoCore.AST.ImperativeAST.ImperativeNode VisitIdentifierNode(ProtoCore.AST.ImperativeAST.IdentifierNode node)
            {
                var variable = node.Value;
                if (cond(variable))
                    node.Value = node.Name = mapper(variable);

                return base.VisitIdentifierNode(node);
            }

            public override ProtoCore.AST.ImperativeAST.ImperativeNode VisitIdentifierListNode(ProtoCore.AST.ImperativeAST.IdentifierListNode node)
            {
                node.LeftNode = node.LeftNode.Accept(this);

                var rightNode = node.RightNode;
                while (rightNode != null)
                {
                    if (rightNode is ProtoCore.AST.ImperativeAST.FunctionCallNode)
                    {
                        var funcCall = rightNode as ProtoCore.AST.ImperativeAST.FunctionCallNode;
                        funcCall.FormalArguments = VisitNodeList(funcCall.FormalArguments);
                        if (funcCall.ArrayDimensions != null)
                        {
                            funcCall.ArrayDimensions = funcCall.ArrayDimensions.Accept(this) as ProtoCore.AST.ImperativeAST.ArrayNode;
                        }
                        break;
                    }
                    else if (rightNode is ProtoCore.AST.ImperativeAST.IdentifierListNode)
                    {
                        rightNode = (rightNode as ProtoCore.AST.ImperativeAST.IdentifierListNode).RightNode;
                    }
                    else
                    {
                        break;
                    }
                }

                return node;
            }
        }

        private class IdentifierInPlaceMapper : AstReplacer
        {
            private ProtoCore.Core core;
            private Func<string, string> mapper;
            private Func<string, bool> cond;

            public IdentifierInPlaceMapper(ProtoCore.Core core, Func<string, bool> cond, Func<string, string> mapper)
            {
                this.core = core;
                this.cond = cond;
                this.mapper = mapper;
            }

            public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
            {
                var variable = node.Value;
                if (cond(variable))
                    node.Value = node.Name = mapper(variable);

                return base.VisitIdentifierNode(node);
            }

            public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
            {
                if (node == null)
                    return null;

                node.LeftNode = node.LeftNode.Accept(this);

                var rightNode = node.RightNode;
                while (rightNode != null)
                {
                    if (rightNode is FunctionCallNode)
                    {
                        var funcCall = rightNode as FunctionCallNode;
                        funcCall.FormalArguments = VisitNodeList(funcCall.FormalArguments);
                        if (funcCall.ArrayDimensions != null)
                        {
                            funcCall.ArrayDimensions = funcCall.ArrayDimensions.Accept(this) as ArrayNode;
                        }
                        break;
                    }
                    else if (rightNode is IdentifierListNode)
                    {
                        rightNode = (rightNode as IdentifierListNode).RightNode;
                    }
                    else
                    {
                        break;
                    }
                }

                return node;
            }

            public override AssociativeNode VisitFunctionDefinitionNode(FunctionDefinitionNode node)
            {
                // Not applying renaming to function defintion node, otherwise
                // function defintion would depend on variables that defined in
                // code block node, and there are implicit dependency between
                // code block node that uses this funciton and code block node
                // that defines this function.
                return node;
            }

            public override AssociativeNode VisitLanguageBlockNode(LanguageBlockNode node)
            {
                var impCbn = node.CodeBlockNode as ProtoCore.AST.ImperativeAST.ImperativeNode;
                if (impCbn != null)
                {
                    var replacer = new ImperativeIdentifierInPlaceMapper(core, cond, mapper);
                    impCbn.Accept(replacer);
                }
                return node;
            }
        }

        #endregion


        [NodeMigration(version: "1.9.0.0")]
        public static NodeMigrationData Migrate_2_0_0(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            var node = data.MigratedNodes.ElementAt(0);

            var codeTextAttr = node.Attributes["CodeText"];
            if (codeTextAttr == null)
            {
                return migrationData;
            }

            codeTextAttr.Value = ParserUtils.TryMigrateDeprecatedListSyntax(codeTextAttr.Value);

            migrationData.AppendNode(node);
            return migrationData;
        }
    }

    /// <summary>
    /// Statements are used in CBN in order to create output ports.
    /// </summary>
    public class Statement
    {
        #region Enums

        #region State enum

        /// <summary>
        /// Describes statement state.
        /// E.g. normal, warning or error.
        /// </summary>
        public enum State
        {
            Normal,
            Warning,
            Error
        }

        #endregion

        #region StatementType enum

        /// <summary>
        /// Describes statement type.
        /// Used in order to set correct column to variable.
        /// </summary>
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

        /// <summary>
        /// Creates Statement from node
        /// </summary>
        /// <param name="parsedNode"><see cref="Node"/></param>
        /// <returns>Statement</returns>
        public static Statement CreateInstance(Node parsedNode)
        {
            if (parsedNode == null)
                throw new ArgumentNullException();

            return new Statement(parsedNode);
        }

        /// <summary>
        /// Returns variables from AST nodes.
        /// E.g. a+5. Here "a" is variable.
        /// </summary>
        /// <param name="astNode"><see cref="Node"/></param>
        /// <param name="refVariableList">list of variables</param>
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
                foreach (AssociativeNode node in currentNode.Exprs)
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
                GetReferencedVariables(currentNode.From, refVariableList);
                GetReferencedVariables(currentNode.To, refVariableList);
                GetReferencedVariables(currentNode.Step, refVariableList);
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
        /// Returns the names of the variables that have been declared in the statement
        /// </summary>
        /// <param name="s"> Statement whose variable names to be queried.</param>
        /// <returns></returns>
        public static List<string> GetDefinedVariableNames(Statement s)
        {
            return s.definedVariables.Select(defVar => defVar.Name).ToList();
        }

        /// <summary>
        /// Returns the names of the variables that have been declared in the statement
        /// for code block node output ports. 
        /// Example: "a[0] = x; a[1] = y;" will return 2 output ports, one for each list index.
        /// </summary>
        /// <param name="s"> Statement whose variable names to be queried.</param>
        /// <returns></returns>
        internal static List<string> GetDefinedVariableNamesForOutports(Statement s)
        {
            return s.definedVariables.Select(outVar => outVar.NameWithIndex).ToList();
        }

        /// <summary>
        /// Returns statement type.
        /// </summary>
        /// <param name="astNode"><see cref="Node"/></param>
        /// <returns>StatementType</returns>
        private static StatementType GetStatementType(Node astNode)
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

        private static IdentifierNode GetDefinedIdentifier(Node leftNode)
        {
            var lhs = leftNode as TypedIdentifierNode;
            if (lhs != null)
                return new IdentifierNode((IdentifierNode) leftNode);

            var identiferNode = leftNode as IdentifierNode;
            if (identiferNode != null)
            {
                return identiferNode;
            }
            if (leftNode is IdentifierListNode || leftNode is FunctionCallNode)
                return null;

            throw new ArgumentException("Left node type incorrect");
        }
        #endregion

        #region Properties

        /// <summary>
        /// Returns the index of the Startline.
        /// E.g. a+5 StartLine will be 1.
        /// </summary>
        public int StartLine { get; private set; }

        /// <summary>
        /// Returns the index of the EndLine.
        /// E.g.
        /// a+5
        /// +6+3;
        /// Endline will be 2.
        /// </summary>
        public int EndLine { get; private set; }

        public Variable FirstDefinedVariable
        {
            get { return definedVariables.FirstOrDefault(); }
        }

        /// <summary>
        /// Returns the State of the Statement.
        /// E.g. normal, warning or error.
        /// </summary>
        public State CurrentState { get; private set; }

        /// <summary>
        /// Returns the type of the statement.
        /// E.g. expression, literal etc.
        /// </summary>
        public StatementType CurrentType { get; private set; }

        /// <summary>
        /// <see cref="Node"/>
        /// </summary>
        public Node AstNode { get; private set; }

        #endregion

        #region Private Methods

        private Statement(Node parsedNode)
        {
            StartLine = parsedNode.line;
            EndLine = parsedNode.endLine;
            CurrentType = GetStatementType(parsedNode);
            AstNode = parsedNode;

            if (parsedNode is BinaryExpressionNode)
            {
                //First get all the defined variables
                while (parsedNode is BinaryExpressionNode)
                {
                    var binaryExpression = parsedNode as BinaryExpressionNode;
                    IdentifierNode assignedVar = GetDefinedIdentifier(binaryExpression.LeftNode);
                    if (assignedVar != null)
                    {
                        definedVariables.Add(new Variable(assignedVar));
                    }
                    parsedNode = (parsedNode as BinaryExpressionNode).RightNode;
                }

                //Then get the referenced variables
                List<Variable> refVariableList = new List<Variable>();
                GetReferencedVariables(parsedNode, refVariableList);
                referencedVariables = refVariableList;
            }

            Variable.SetCorrectColumn(referencedVariables, CurrentType, StartLine);
        }

        #endregion
    }

    /// <summary>
    /// Represents variable in CBN.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Returns the index of row.
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Returns the index of start column.
        /// </summary>
        public int StartColumn { get; private set; }

        /// <summary>
        /// Returns the index of end column.
        /// </summary>
        public int EndColumn
        {
            get { return StartColumn + Name.Length; }
        }

        /// <summary>
        /// This returns the name of the variable.
        /// E.g. 
        /// a = 5;
        /// Name will be "a".
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This returns the name of the list including its index. 
        /// E.g. for "a[0] = 5;", NameWithIndex will be "a[0]".
        /// It simply returns the name of the variable otherwise.
        /// E.g. for "a = 5;" NameWithIndex will be "a".
        /// </summary>
        public string NameWithIndex { get; private set; }

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

        /// <summary>
        /// Creates Variable
        /// </summary>
        /// <param name="identNode"><see cref="IdentifierNode"/></param>
        public Variable(IdentifierNode identNode)
        {
            if (identNode == null)
                throw new ArgumentNullException();

            Name = identNode.Name;
            NameWithIndex = identNode.ToString();
            Row = identNode.line;
            StartColumn = identNode.col;
        }
        
        /// <summary>
        /// Moves column index back only if variable is not an expression.
        /// </summary>
        /// <param name="refVar">list of variables</param>
        /// <param name="type">statement type</param>
        /// <param name="line">line index</param>
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
