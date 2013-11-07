using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System.Windows;

namespace Dynamo.Nodes
{
    [NodeName("Code Block")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Allows for code to be written")] //<--Change the descp :|
    public partial class CodeBlockNodeModel : NodeModel
    {
        private string code = "";
        private string previewVariable = null;
        private List<Statement> codeStatements = new List<Statement>();
        private bool shouldFocus = true;

        #region Public Methods
        public CodeBlockNodeModel()
        {
            this.ArgumentLacing = LacingStrategy.Disabled;
        }

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
        /// 1. Removing whitespaces form the front and back (whitespaces -> space, tab or enter)
        /// 2.Adds a semicolon at the end
        /// </summary>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        internal static string FormatUserText(string inputCode)
        {
            if (inputCode == null)
            {
                return "";
            }

            inputCode = inputCode.Trim();

            if (inputCode.Equals(""))
                return inputCode;

            //Add the ';' if required
            if (inputCode[inputCode.Length-1] != ';')
                return inputCode.Insert(inputCode.Length, ";");
            else
                return inputCode;
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
                    code = value;
                    if (value != null)
                    {
                        DisableReporting();
                        ProcessCode();
                        RaisePropertyChanged("Code");
                        RequiresRecalc = true;
                        EnableReporting();
                        this.ReportPosition();
                        if (WorkSpace != null)
                            WorkSpace.Modified();
                    }
                }
            }
        }

        public bool VariableAlreadyDeclared(Statement stmnt)
        {
            string varName = stmnt.DefinedVariable.Name;
            foreach (var node in this.WorkSpace.Nodes)
            {
                if (node is CodeBlockNodeModel)
                {
                    foreach (var x in (node as CodeBlockNodeModel).codeStatements)
                    {
                        if (x == stmnt)
                            continue;
                        if (x.DefinedVariable.Name.Equals(varName))
                            return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Protected Methods
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);
            XmlElementHelper helper = new XmlElementHelper(nodeElement);
            helper.SetAttribute("CodeText", code);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            XmlElementHelper helper = new XmlElementHelper(nodeElement as XmlElement);
            Code = helper.ReadString("CodeText");
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
                XmlElementHelper helper = new XmlElementHelper(element as XmlElement);
                Code = helper.ReadString("CodeText");
                shouldFocus = helper.ReadBoolean("ShouldFocus");
            }
        }

        protected override void BuildAstNode(DSEngine.IAstBuilder builder, List<AssociativeNode> inputAstNodes)
        {
            builder.Build(this, inputAstNodes);
        }

        protected override AssociativeNode GetIndexedOutputNode(int index)
        {
            if (this.State == ElementState.ERROR)
                return null;
            List<string> unboundIdentifiers = new List<string>();
            List<ProtoCore.AST.Node> resultNodes = new List<Node>();
            List<ProtoCore.BuildData.ErrorEntry> errors;
            List<ProtoCore.BuildData.WarningEntry> warnings;
            GraphToDSCompiler.GraphUtilities.Parse(code, out resultNodes, out errors, out  warnings, unboundIdentifiers);
            BinaryExpressionNode indexedStatement = resultNodes[index] as BinaryExpressionNode;
            return indexedStatement.LeftNode as AssociativeNode;
        }

        public override string VariableToPreview
        {
            get
            {
                return previewVariable;
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

            if (Code.Equals("") || Code.Equals("Your Code Goes Here")) //If its null then remove all the ports
            {
                SetPorts(new List<string>());
                return;
            }

            //Parse the text and assign each AST node to a statement instance
            List<string> unboundIdentifiers = new List<string>();
            List<ProtoCore.AST.Node> resultNodes = new List<Node>();
            List<ProtoCore.BuildData.ErrorEntry> errors;
            List<ProtoCore.BuildData.WarningEntry> warnings;
            if(GraphToDSCompiler.GraphUtilities.Parse(code,out resultNodes,out errors,out  warnings, unboundIdentifiers) && resultNodes!=null)
            {
                //Create an instance of statement for each code statement written by the user
                foreach (Node node in resultNodes)
                {
                    Statement tempStatement;
                    {
                        tempStatement = Statement.CreateInstance(node, this.GUID);
                    }
                    codeStatements.Add(tempStatement);

                    var binaryStatement = node as BinaryExpressionNode;
                    if (binaryStatement != null && binaryStatement.Optr == ProtoCore.DSASM.Operator.assign)
                    {
                        var lhsIdent = binaryStatement.LeftNode as IdentifierNode;
                        if (lhsIdent != null)
                        {
                            previewVariable = lhsIdent.Name;
                        }
                    }
                }
            }
            else
            {
                string errorMessage = "";
                int i=0;
                for (; i < errors.Count - 1; i++)
                    errorMessage += (errors[i].Message + "\n");
                errorMessage += errors[i].Message;
                DisplayError(errorMessage);
                return;
            }

            foreach (var singleStatement in codeStatements)
            {
                if (VariableAlreadyDeclared(singleStatement))
                {
                    string varName = singleStatement.DefinedVariable.Name;
                    string errorMessage = varName + " is already declared.";
                    DisplayError(errorMessage);
                    return;
                }
            }

            SetPorts(unboundIdentifiers); //Set the input and output ports based on the statements
        }

        private void SetPorts(List<string> unboundIdentifiers)
        {
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

        private void SetOutputPorts(List<double> verticalMargin)
        {
            int outportCount = 0;
            for (int i = 0; i < codeStatements.Count; i++)
            {
                Statement s = codeStatements[i];
                if (s.DefinedVariable != null)
                {
                    OutPortData.Add(new PortData(">", "Output", typeof(object))
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
                InPortData.Add(new PortData(name, "Input", typeof(object)));
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
                Statement.StatementType sType = codeStatements[i].CurrentType;
                if (sType == Statement.StatementType.FuncDeclaration) //FuncDec doesnt have an output
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

        private List<Variable> referencedVariables = new List<Variable>();
        private List<Statement> subStatements = new List<Statement>();

        #region Public Methods
        public static Statement CreateInstance(Node astNode, Guid nodeGuid)
        {
            if (astNode == null)
                throw new ArgumentNullException();

            return new Statement(astNode, nodeGuid);
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

        public static StatementType GetStatementType(Node astNode, Guid nodeGuid)
        {
            if (astNode is FunctionDefinitionNode)
                return StatementType.FuncDeclaration;
            if (astNode is BinaryExpressionNode)
            {
                BinaryExpressionNode currentNode = astNode as BinaryExpressionNode;
                string fakeVariableName = "temp" + nodeGuid.ToString().Remove(7);
                if (!(currentNode.LeftNode is IdentifierNode) || currentNode.Optr != ProtoCore.DSASM.Operator.assign)
                    throw new ArgumentException();
                if (!currentNode.LeftNode.Name.Equals(fakeVariableName))
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

        public Variable DefinedVariable { get; private set; }

        public State CurrentState { get; private set; }
        public StatementType CurrentType { get; private set; }

        #endregion

        #region Private Methods
        private Statement(Node astNode, Guid nodeGuid)
        {
            StartLine = astNode.line;
            EndLine = astNode.endLine;
            CurrentType = GetStatementType(astNode, nodeGuid);

            if (astNode is BinaryExpressionNode)
            {
                BinaryExpressionNode binExprNode = astNode as BinaryExpressionNode;
                if (binExprNode.Optr != ProtoCore.DSASM.Operator.assign)
                    throw new ArgumentException("Binary Expr Node is not an assignment!");
                if (!(binExprNode.LeftNode is IdentifierNode))
                    throw new ArgumentException("LHS invalid");

                IdentifierNode assignedVar = binExprNode.LeftNode as IdentifierNode;
                string fakeVariableName = "temp" + nodeGuid.ToString().Remove(7);
                if (assignedVar.Name.Equals(fakeVariableName))
                {
                    DefinedVariable = new Variable(">", assignedVar.line);
                }
                else
                {
                    DefinedVariable = new Variable(assignedVar);
                }

                List<Variable> refVariableList = new List<Variable>();
                GetReferencedVariables(binExprNode.RightNode, refVariableList);
                referencedVariables = refVariableList;
            }
            else if (astNode is FunctionDefinitionNode)
            {
                FunctionDefinitionNode currentNode = astNode as FunctionDefinitionNode;
                DefinedVariable = null;
                if (currentNode.FunctionBody.endLine != -1)
                    EndLine = currentNode.FunctionBody.endLine;
                foreach (Node node in currentNode.FunctionBody.Body)
                {
                    subStatements.Add(new Statement(node, nodeGuid));
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
