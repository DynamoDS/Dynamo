using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GraphToDSCompiler;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ArrayNode = ProtoCore.AST.AssociativeAST.ArrayNode;
using Node = ProtoCore.AST.Node;

namespace Dynamo.Nodes
{
    [NodeName("Code Block")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows for code to be written")] //<--Change the descp :|
    public partial class CodeBlockNodeModel : NodeModel
    {
        private string code = "";
        private string codeToParse = "";
        private string previewVariable = null;
        private List<Statement> codeStatements = new List<Statement>();
        private List<string> inputIdentifiers = new List<string>();
        private bool shouldFocus = true;

        #region Public Methods
        public CodeBlockNodeModel()
        {
            this.ArgumentLacing = LacingStrategy.Disabled;
        }

        /// <summary>
        /// The function sets the state of the node to an erraneous state and displays 
        /// the the string errorMessage as an error bubble on top of the node.
        /// It also removes all the in ports and out ports as well. So that the user knows there is an error.
        /// </summary>
        /// <param name="errorMessage"> Error message to be displayed </param>
        public void DisplayError(string errorMessage)
        {
            //Log an error. TODO Ambi : Remove this later
            DynamoLogger.Instance.Log("Error in Code Block Node");

            //Remove all ports
            int size = InPortData.Count;
            for (int i = 0; i < size; i++)
                InPortData.RemoveAt(0);
            size = OutPortData.Count;
            for (int i = 0; i < size; i++)
                OutPortData.RemoveAt(0);
            RegisterAllPorts();

            //Set the node state in error and display the message
            Error(errorMessage);
        }

        /// <summary>
        /// Formats user text by :
        /// 1.Removes unnecessary semi colons
        /// 2. Removing whitespaces form the front and back (whitespaces -> space, tab or enter)
        /// 3.Adds a semicolon at the end if needed
        /// </summary>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        internal static string FormatUserText(string inputCode)
        {
            if (inputCode == null)
            {
                return "";
            }

            inputCode = inputCode.Replace("\r", "");
            string[] statements = inputCode.Split(';');
            inputCode = "";
            foreach (var stmnt in statements)
            {
                foreach (char c in stmnt)
                    if (!char.IsWhiteSpace(c))
                    {
                        inputCode += (stmnt + ";");
                        break;
                    }
            }

            inputCode = inputCode.Trim();

            if (inputCode.Equals(""))
                return inputCode;

            //Add the ';' if required
            if (inputCode[inputCode.Length - 1] != ';')
                return inputCode.Insert(inputCode.Length, ";");
            else
                return inputCode;
        }

        /// <summary>
        /// Returns the names of all the variables defined in this code block.
        /// </summary>
        /// <returns>List containing all the names</returns>
        public List<string> GetDefinedVariableNames()
        {
            List<string> defVarNames = new List<string>();
            foreach (var stmnt in codeStatements)
                defVarNames.AddRange(Statement.GetDefinedVariableNames(stmnt, true));
            return defVarNames;
        }
        #endregion

        #region Properties
        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                if (code == null || !code.Equals(value))
                {

                    if (value != null)
                    {
                        DisableReporting();
                        {
                            this.WorkSpace.UndoRecorder.BeginActionGroup();

                            var portConnections = new OrderedDictionary();
                            //Save the connectors so that we can recreate them at the correct positions
                            SaveAndDeleteConnectors(portConnections);

                            this.WorkSpace.UndoRecorder.RecordModificationForUndo(this);
                            code = value;
                            ProcessCode();

                            //Recreate connectors that can be reused
                            LoadAndCreateConnectors(portConnections);
                            this.WorkSpace.UndoRecorder.EndActionGroup();
                        }
                        RaisePropertyChanged("Code");
                        RequiresRecalc = true;
                        EnableReporting();
                        this.ReportPosition();
                        if (WorkSpace != null)
                            WorkSpace.Modified();
                    }
                    else
                        code = null;
                }
            }
        }

        public string CodeToParse
        {
            get { return codeToParse; }
        }
        #endregion

        #region Protected Methods
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);
            XmlElementHelper helper = new XmlElementHelper(nodeElement);
            helper.SetAttribute("CodeText", code);
            helper.SetAttribute("ShouldFocus", shouldFocus);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            XmlElementHelper helper = new XmlElementHelper(nodeElement as XmlElement);
            Code = helper.ReadString("CodeText");
            shouldFocus = helper.ReadBoolean("ShouldFocus");
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Code")
            {
                this.Code = value;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("CodeText", code);
            helper.SetAttribute("ShouldFocus", shouldFocus);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                shouldFocus = helper.ReadBoolean("ShouldFocus");
                code = helper.ReadString("CodeText");
                ProcessCode();
                RaisePropertyChanged("Code");
                RequiresRecalc = true;
                if (WorkSpace != null)
                    WorkSpace.Modified();
            }
        }

        public override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            //var unboundIdentifiers = new List<string>();

            CodeBlockNode commentNode;
            CodeBlockNode codeBlock = null;
            string finalCode = CodeToParse;

            // Define unbound variables if necessary
            if (this.inputIdentifiers != null && this.inputIdentifiers.Count > 0)
            {
                if (null == inputAstNodes || inputAstNodes.Count != inputIdentifiers.Count)
                {
                    throw new ArgumentException("Invalid input AST nodes.");
                }

                StringBuilder initStatements = new StringBuilder();
                for (int i = 0; i < inputIdentifiers.Count; ++i)
                {
                    var astNode = inputAstNodes[i];
                    if (astNode != null && astNode is IdentifierNode)
                    {
                        var unboundVar = inputIdentifiers[i];
                        var inputVar = GraphUtilities.ASTListToCode(new List<AssociativeNode> { astNode });
                        if (!string.Equals(unboundVar, inputVar))
                        {
                            initStatements.Append(unboundVar);
                            initStatements.Append(" = ");
                            initStatements.Append(inputVar);
                            initStatements.Append(";");
                        }
                    }
                }
                initStatements.Append(codeToParse);
                finalCode = initStatements.ToString();
            }

            try
            {
                codeBlock = GraphUtilities.Parse(finalCode, out commentNode) as CodeBlockNode;
            }
            catch (Exception ex)
            {
                this.State = ElementState.ERROR;
                DynamoLogger.Instance.Log("Failed to build AST for code block node. Error: " + ex.Message);
            }

            return codeBlock != null ? codeBlock.Body : null;
        }

        public override IdentifierNode GetAstIdentifierForPortIndex(int portIndex)
        {
            if (this.State == ElementState.ERROR)
                return null;

            int statementIndex = -1;
            while (portIndex >= 0)
            {
                statementIndex++;
                if (RequiresOutPort(codeStatements[statementIndex], statementIndex))
                    portIndex--;
            }

            List<string> unboundIdentifiers = new List<string>();
            List<ProtoCore.AST.Node> resultNodes = new List<Node>();
            List<ProtoCore.BuildData.ErrorEntry> errors;
            List<ProtoCore.BuildData.WarningEntry> warnings;
            GraphToDSCompiler.GraphUtilities.Parse(ref codeToParse, out resultNodes, out errors, out  warnings, unboundIdentifiers);
            BinaryExpressionNode indexedStatement = resultNodes[statementIndex] as BinaryExpressionNode;
            return indexedStatement.LeftNode as IdentifierNode;
        }

        public override string VariableToPreview
        {
            get
            {
                return (State == ElementState.ERROR) ? null : previewVariable;
            }
        }
        #endregion

        #region Private Methods
        private void ProcessCode()
        {

            //Format user test
            code = CodeBlockNodeModel.FormatUserText(code);

            //New code => Revamp everything
            codeStatements.Clear();

            if (Code.Equals("")) //If its null then remove all the ports
            {
                SetPorts(new List<string>());
                return;
            }

            //Parse the text and assign each AST node to a statement instance
            codeToParse = code;
            List<string> unboundIdentifiers = new List<string>();
            List<ProtoCore.AST.Node> resultNodes = new List<Node>();
            List<ProtoCore.BuildData.ErrorEntry> errors;
            List<ProtoCore.BuildData.WarningEntry> warnings;

            if (GraphToDSCompiler.GraphUtilities.Parse(ref codeToParse, out resultNodes, out errors, out  warnings, unboundIdentifiers) && resultNodes != null)
            {
                //Create an instance of statement for each code statement written by the user
                foreach (Node node in resultNodes)
                {
                    Statement tempStatement;
                    try
                    {
                        //Create and save a statement variable from the astnodes generated
                        tempStatement = Statement.CreateInstance(node);
                        codeStatements.Add(tempStatement);
                    }
                    catch (Exception e)
                    {
                        DisplayError(e.Message);
                        previewVariable = null;
                    }

                    var binaryStatement = node as BinaryExpressionNode;
                    if (binaryStatement != null && binaryStatement.Optr == ProtoCore.DSASM.Operator.assign)
                    {
                        var lhsIdent = binaryStatement.LeftNode as IdentifierNode;
                        if (lhsIdent != null)
                        {
                            previewVariable = lhsIdent.Name;
                            // previewVariable = GraphToDSCompiler.GraphUtilities.ASTListToCode(new List<AssociativeNode> { lhsIdent});
                        }
                    }
                }
            }
            else
            {
                if (errors == null)
                    DisplayError("Errors not getting sent from compiler to UI");
                //Found errors. Get the error message strings and use it to call the DisplayError function

                if (errors != null)
                {
                    string errorMessage = "";
                    int i = 0;
                    for (; i < errors.Count - 1; i++)
                        errorMessage += (errors[i].Message + "\n");
                    errorMessage += errors[i].Message;
                    DisplayError(errorMessage);
                }
                return;
            }

            //Make sure variables have not been declared in other Code block nodes.
            string redefinedVariable = this.WorkSpace.GetRedefinedVariable(this);
            if (redefinedVariable != null)
            {
                DisplayError(redefinedVariable + " is already defined");
                return;
            }

            SetPorts(unboundIdentifiers); //Set the input and output ports based on the statements
        }

        /// <summary>
        /// Creates the inport and outport data based on the statements generated form the user code
        /// </summary>
        /// <param name="unboundIdentifiers"> List of unbound identifiers to be used an inputs</param>
        private void SetPorts(List<string> unboundIdentifiers)
        {
            this.inputIdentifiers = unboundIdentifiers;

            InPortData.Clear();
            OutPortData.Clear();
            if (codeStatements.Count == 0 || codeStatements == null)
            {
                RegisterAllPorts();
                return;
            }

            SetInputPorts(unboundIdentifiers);

            //Since output ports need to be aligned with the statements, calculate the margins
            //needed based on the statement lines and add them to port data.
            List<double> verticalMargin = CalculateMarginInPixels();
            SetOutputPorts(verticalMargin);

            RegisterAllPorts();
        }

        /// <summary>
        /// Creates the output ports with the necessary margins for port alignment
        /// </summary>
        /// <param name="verticalMargin"> Distance between the consequtive output ports </param>
        private void SetOutputPorts(List<double> verticalMargin)
        {
            int outportCount = 0;
            for (int i = 0; i < codeStatements.Count; i++)
            {
                Statement s = codeStatements[i];
                if (RequiresOutPort(s, i))
                {
                    string nickName = Statement.GetDefinedVariableNames(s, true)[0];

                    if (nickName.StartsWith("temp") && nickName.Length > 9) // Do a better check
                        nickName = "Statement Output"; //Set tool tip incase of random var name

                    OutPortData.Add(new PortData(">", nickName, typeof(object))
                    {
                        VerticalMargin = verticalMargin[outportCount]
                    });
                    outportCount++;
                }
            }
        }

        /// <summary>
        /// Set a port for each different unbound identifier
        /// </summary>
        private void SetInputPorts(List<string> unboundIdentifier)
        {
            foreach (string name in unboundIdentifier)
            {
                string portName = name;
                if (portName.Length > 24)
                    portName = portName.Remove(21) + "...";
                InPortData.Add(new PortData(portName, name, typeof(object)));
            }
        }

        /// <summary>
        /// Based on the start line of ech statement and type, it returns a list of
        /// top margins required for the ports
        /// </summary>
        private List<double> CalculateMarginInPixels()
        {
            List<double> result = new List<double>();
            int currentOffset = 1; //Used to mark the line immediately after the last output port line
            double initialMarginRequired = 4, margin;
            for (int i = 0; i < codeStatements.Count; i++)
            {
                //Dont calculate margin for ports that dont require a port
                if (!RequiresOutPort(codeStatements[i], i))
                    continue;

                //Margin = diff between this line and prev port line x port height
                if (codeStatements[i].StartLine - currentOffset >= 0)
                {
                    margin = (codeStatements[i].StartLine - currentOffset) * 20;
                    currentOffset = codeStatements[i].StartLine + 1;
                }
                else
                {
                    margin = 0.0;
                    currentOffset += 1;
                }
                result.Add(margin + initialMarginRequired);
                initialMarginRequired = 0;
            }
            return result;
        }

        /// <summary>
        /// Checks wheter an outport is required for a given statement. An outport is not required
        /// if there are no defined variables or if any of the defined variables have been
        /// declared again later on in the code block
        /// </summary>
        /// <param name="s"> Statement to check the port</param>
        /// <param name="pos"> Position of the statement in codeStatements</param>
        /// <returns></returns>
        private bool RequiresOutPort(Statement s, int pos)
        {
            List<string> defVariables = Statement.GetDefinedVariableNames(s, true);

            //Check if defined variables exist
            if (defVariables.Count == 0)
                return false;

            //Check if variable has been redclared later on in the CBN
            foreach (string varName in defVariables)
            {
                for (int i = pos + 1; i < codeStatements.Count; i++)
                {
                    List<string> laterDefVariables = Statement.GetDefinedVariableNames(codeStatements[i], true);
                    if (laterDefVariables.Contains(varName))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Deletes all the connections and saves their data (the start and end port)
        /// so that they can be recreated if needed.
        /// </summary>
        /// <param name="portConnections">A list of connections that will be destroyed</param>
        private void SaveAndDeleteConnectors(OrderedDictionary portConnections)
        {
            for (int i = 0; i < OutPorts.Count; i++)
            {
                var portModel = OutPorts[i];
                string portName = portModel.ToolTipContent;
                if (portModel.ToolTipContent.Equals("Statement Output"))
                    portName += i.ToString();
                if (portModel.Connectors.Count != 0)
                {
                    portConnections.Add(portName, new List<PortModel>());
                    foreach (var connector in portModel.Connectors)
                    {
                        (portConnections[portName] as List<PortModel>).Add(connector.End);
                        this.WorkSpace.UndoRecorder.RecordDeletionForUndo(connector);
                    }
                }
                else
                {
                    portConnections.Add(portName, null);
                }
            }

            //Delete the connectors
            foreach (var outport in OutPorts)
                DestroyConnectors(outport);

            //Clear out all the port models
            for (int i = OutPorts.Count - 1; i >= 0; i--)
                OutPorts.RemoveAt(i);
        }

        /// <summary>
        /// Now that the portData has been set for the new ports, we recreate the connections we
        /// so mercilessly destroyed, restoring peace and balance to the world once again.
        /// </summary>
        /// <param name="portConnections"> List of the connections that were killed</param>
        private void LoadAndCreateConnectors(OrderedDictionary portConnections)
        {
            List<int> undefinedIndices = new List<int>();
            for (int i = 0; i < OutPortData.Count; i++)
            {
                string varName = OutPortData[i].ToolTipString;
                if (portConnections.Contains(varName) && portConnections[varName] != null)
                {
                    foreach (var endPortModel in (portConnections[varName] as List<PortModel>))
                    {
                        PortType p;
                        NodeModel endNode = endPortModel.Owner;
                        var connector = ConnectorModel.Make(this, endNode, i,
                            endNode.GetPortIndex(endPortModel, out p), PortType.INPUT);
                        this.WorkSpace.Connectors.Add(connector);
                        this.WorkSpace.UndoRecorder.RecordCreationForUndo(connector);
                    }
                    portConnections[varName] = null;
                }
                else
                    undefinedIndices.Add(i);
            }

            for (int i = 0; i < undefinedIndices.Count; i++)
            {
                int index = undefinedIndices[i];
                if (index < portConnections.Count && portConnections[index] != null)
                {
                    foreach (var endPortModel in (portConnections[index] as List<PortModel>))
                    {
                        PortType p;
                        NodeModel endNode = endPortModel.Owner;
                        var connector = ConnectorModel.Make(this, endNode, index,
                            endNode.GetPortIndex(endPortModel, out p), PortType.INPUT);
                        this.WorkSpace.Connectors.Add(connector);
                        this.WorkSpace.UndoRecorder.RecordCreationForUndo(connector);
                    }
                    portConnections[index] = null;
                    undefinedIndices.Remove(index);
                    i--;
                }
            }


            List<List<PortModel>> unusedConnections = new List<List<PortModel>>();
            foreach (List<PortModel> portModelList in portConnections.Values.Cast<List<PortModel>>())
            {
                if (portModelList == null)
                    continue;
                unusedConnections.Add(portModelList);
            }
            while (undefinedIndices.Count > 0 && unusedConnections.Count != 0)
            {
                foreach (var endPortModel in unusedConnections[0])
                {
                    PortType p;
                    NodeModel endNode = endPortModel.Owner;
                    var connector = ConnectorModel.Make(this, endNode, undefinedIndices[0],
                        endNode.GetPortIndex(endPortModel, out p), PortType.INPUT);
                    this.WorkSpace.Connectors.Add(connector);
                    this.WorkSpace.UndoRecorder.RecordCreationForUndo(connector);
                }
                undefinedIndices.RemoveAt(0);
                unusedConnections.RemoveAt(0);
            }
        }
        #endregion

    }


    public class Statement
    {
        #region Enums
        public enum State
        {
            Normal,
            Warning,
            Error
        }
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

        private List<Variable> definedVariables = new List<Variable>();
        private List<Variable> referencedVariables = new List<Variable>();
        private List<Statement> subStatements = new List<Statement>();

        #region Public Methods
        public static Statement CreateInstance(Node astNode)
        {
            if (astNode == null)
                throw new ArgumentNullException();

            return new Statement(astNode);
        }

        public static void GetReferencedVariables(Node astNode, List<Variable> refVariableList)
        {
            //DFS Search to find all identifier nodes
            if (astNode == null)
                return;
            if (astNode is FunctionCallNode)
            {
                FunctionCallNode currentNode = astNode as FunctionCallNode;
                foreach (var node in currentNode.FormalArguments)
                {
                    GetReferencedVariables(node, refVariableList);
                }

            }
            else if (astNode is IdentifierNode)
            {
                Variable resultVariable = new Variable(astNode as IdentifierNode);
                refVariableList.Add(resultVariable);
                GetReferencedVariables((astNode as IdentifierNode).ArrayDimensions, refVariableList);
            }
            else if (astNode is ArrayNode)
            {
                ArrayNode currentNode = astNode as ArrayNode;
                GetReferencedVariables(currentNode.Expr, refVariableList);
                GetReferencedVariables(currentNode.Type, refVariableList);
            }
            else if (astNode is ExprListNode)
            {
                ExprListNode currentNode = astNode as ExprListNode;
                foreach (var node in currentNode.list)
                {
                    GetReferencedVariables(node, refVariableList);
                }
            }
            else if (astNode is FunctionDotCallNode)
            {
                FunctionDotCallNode currentNode = astNode as FunctionDotCallNode;
                GetReferencedVariables(currentNode.FunctionCall, refVariableList);
            }
            else if (astNode is InlineConditionalNode)
            {
                InlineConditionalNode currentNode = astNode as InlineConditionalNode;
                GetReferencedVariables(currentNode.ConditionExpression, refVariableList);
                GetReferencedVariables(currentNode.TrueExpression, refVariableList);
                GetReferencedVariables(currentNode.FalseExpression, refVariableList);
            }
            else if (astNode is RangeExprNode)
            {
                RangeExprNode currentNode = astNode as RangeExprNode;
                GetReferencedVariables(currentNode.FromNode, refVariableList);
                GetReferencedVariables(currentNode.ToNode, refVariableList);
                GetReferencedVariables(currentNode.StepNode, refVariableList);
            }
            else if (astNode is BinaryExpressionNode)
            {
                BinaryExpressionNode currentNode = astNode as BinaryExpressionNode;
                GetReferencedVariables(currentNode.RightNode, refVariableList);
            }
            else
            {
                //Its could be something like a literal
                //Or node not completely implemented YET
                return;
            }
        }

        /// <summary>
        /// Returns the names of the variables that have been referenced in the statement
        /// </summary>
        /// <param name="s"> Statement whose variable names to be got.</param>
        /// <param name="onlyTopLevel"> Bool to check if required to return reference variables in sub statements as well</param>
        /// <returns></returns>
        public static List<string> GetReferencedVariableNames(Statement s, bool onlyTopLevel)
        {
            List<string> names = new List<string>();
            foreach (Variable refVar in s.referencedVariables)
                names.Add(refVar.Name);
            if (!onlyTopLevel)
            {
                foreach (Statement subStatement in s.subStatements)
                    names.AddRange(GetReferencedVariableNames(subStatement, onlyTopLevel));
            }
            return names;
        }

        /// <summary>
        /// Returns the names of the variables that have been declared in the statement
        /// </summary>
        /// <param name="s"> Statement whose variable names to be got.</param>
        /// <param name="onlyTopLevel"> Bool to check if required to return reference variables in sub statements as well</param>
        /// <returns></returns>
        public static List<string> GetDefinedVariableNames(Statement s, bool onlyTopLevel)
        {
            List<string> names = new List<string>();
            foreach (Variable refVar in s.definedVariables)
                names.Add(refVar.Name);
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
                BinaryExpressionNode currentNode = astNode as BinaryExpressionNode;
                if (currentNode.Optr != ProtoCore.DSASM.Operator.assign)
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
        #endregion

        #region Properties
        public int StartLine { get; private set; }
        public int EndLine { get; private set; }
        public Variable FirstDefinedVariable { get { return definedVariables.FirstOrDefault(); } }
        public State CurrentState { get; private set; }
        public StatementType CurrentType { get; private set; }

        #endregion

        #region Private Methods
        private Statement(Node astNode)
        {
            StartLine = astNode.line;
            EndLine = astNode.endLine;
            CurrentType = GetStatementType(astNode);

            if (astNode is BinaryExpressionNode)
            {
                //First get all the defined variables
                while (astNode is BinaryExpressionNode)
                {
                    BinaryExpressionNode binExprNode = astNode as BinaryExpressionNode;
                    IdentifierNode assignedVar = binExprNode.LeftNode as IdentifierNode;
                    definedVariables.Add(new Variable(assignedVar));
                    astNode = binExprNode.RightNode;
                }

                //Then get the referenced variables
                List<Variable> refVariableList = new List<Variable>();
                GetReferencedVariables(astNode, refVariableList);
                referencedVariables = refVariableList;
            }
            else if (astNode is FunctionDefinitionNode)
            {
                FunctionDefinitionNode currentNode = astNode as FunctionDefinitionNode;
                if (currentNode.FunctionBody.endLine != -1)
                    EndLine = currentNode.FunctionBody.endLine;
                foreach (Node node in currentNode.FunctionBody.Body)
                {
                    subStatements.Add(new Statement(node));
                }
            }
            else
                throw new ArgumentException("Must be func def or assignment");

            Variable.SetCorrectColumn(referencedVariables, this.CurrentType, this.StartLine);
        }
        #endregion
    }

    public class Variable
    {
        public int Row { get; private set; }
        public int StartColumn { get; private set; }
        public int EndColumn
        {
            get
            {
                return StartColumn + Name.Length;
            }
        }
        public string Name { get; private set; }

        #region Private Methods
        private void MoveColumnBack(int line)
        {
            //Move the column of the variable back only if it is on the same line
            //as the fake variable
            if (Row == line)
            {
                StartColumn -= 13;
            }
        }
        #endregion

        #region Public Methods
        public Variable(IdentifierNode identNode)
        {
            if (identNode == null)
                throw new ArgumentNullException();
            Name = identNode.Value;
            if (identNode.ArrayDimensions != null)
                ;//  Implement!
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
                foreach (var singleVar in refVar)
                    singleVar.MoveColumnBack(line);
            }
        }
        #endregion
    }
}
