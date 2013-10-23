using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Code Block")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Allows for code to be written")] //<--Change the descp :|
    public partial class CodeBlockNodeModel : NodeModel
    {
        private string code = "Your Code Goes Here";
        private List<Statement> codeStatements = new List<Statement>();

        #region Public Methods
        public CodeBlockNodeModel()
        {
            codeStatements = new List<Statement>();
            code = "Your Code Goes Here";
            this.ArgumentLacing = LacingStrategy.Disabled;
        }

        public void DisplayError()
        {
            DynamoLogger.Instance.Log("Error in Code Block Node");
            this.State = ElementState.ERROR;
        }

        /// <summary>
        /// Formats user text by :
        /// 1. Removing whitespaces form the front and back (whitespaces -> space, tab or enter)
        /// 2.Adds a semicolon at the end
        /// </summary>
        /// <param name="inputCode"></param>
        /// <returns></returns>
        public string FormatUserText(string inputCode)
        {
            inputCode = inputCode.Trim();

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
                        if (WorkSpace != null)
                            WorkSpace.Modified();
                    }
                }
            }
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
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element as XmlElement);
                Code = helper.ReadString("CodeText");
            }
        }

        protected override AssociativeNode BuildAstNode(DSEngine.IAstBuilder builder, List<AssociativeNode> inputAstNodes)
        {
            return builder.Build(this, inputAstNodes);
        }

        protected override AssociativeNode GetIndexedOutputNode(int index)
        {
            Dictionary<int, List<GraphToDSCompiler.VariableLine>> unboundIdentifiers;
            unboundIdentifiers = new Dictionary<int, List<GraphToDSCompiler.VariableLine>>();
            List<ProtoCore.AST.Node> resultNodes;
            GraphToDSCompiler.GraphUtilities.ParseCodeBlockNodeStatements(Code, unboundIdentifiers, out resultNodes);
            BinaryExpressionNode indexedStatement = resultNodes[index] as BinaryExpressionNode;
            return indexedStatement.LeftNode as AssociativeNode;
        }
        #endregion

        #region Private Methods
        private void ProcessCode()
        {
            //New code => Revamp everything
            codeStatements.Clear();

            if (Code.Equals("") || Code.Equals("Your Code Goes Here")) //If its null then remove all the ports
            {
                SetPorts();
                return;
            }

            //Addition of ';' to end of code
            code = FormatUserText(code);

            //Parse the text and assign each AST node to a statement instance
            List<string> compiledCode;

            //To allow for statements like a+b; which are not handled by the parser, enter a fake assigned
            //variable and compute. Cannot handle comments as of now
            GraphToDSCompiler.GraphUtilities.CompileExpression(Code, out compiledCode);
            string fakeVariableName = "temp" + this.GUID.ToString().Remove(7);
            string codeToParse = "";
            for (int i = 0; i < compiledCode.Count; i++)
            {
                string singleExpression = compiledCode[i];
                singleExpression = singleExpression.Replace("%t", fakeVariableName);
                //singleExpression = singleExpression.Replace("\r\n","\n");
                codeToParse += singleExpression;
            }

            Dictionary<int, List<GraphToDSCompiler.VariableLine>> unboundIdentifiers;
            unboundIdentifiers = new Dictionary<int, List<GraphToDSCompiler.VariableLine>>();
            List<ProtoCore.AST.Node> resultNodes;
            if (GraphToDSCompiler.GraphUtilities.ParseCodeBlockNodeStatements(codeToParse, unboundIdentifiers, out resultNodes))
            {
                //Create an instance of statement for each code statement written by the user
                foreach (Node node in resultNodes)
                {
                    Statement tempStatement;
                    {
                        tempStatement = Statement.CreateInstance(node, this.GUID);
                    }
                    codeStatements.Add(tempStatement);
                }
            }
            else
            {
                DisplayError();
            }

            SetPorts(); //Set the input and output ports based on the statements
        }

        private void SetPorts()
        {
            InPortData.Clear();
            OutPortData.Clear();
            if (codeStatements.Count == 0 || codeStatements == null)
            {
                RegisterAllPorts();
                return;
            }

            SetInputPorts();

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
        /// Set a port for each different input parameter
        /// </summary>
        private void SetInputPorts()
        {
            List<string> uniqueInputs = new List<string>();
            foreach (var singleStatement in codeStatements)
            {
                List<string> inputNames = Statement.GetReferencedVariableNames(singleStatement, true);
                foreach (string name in inputNames)
                {
                    if (!uniqueInputs.Contains(name))
                        uniqueInputs.Add(name);
                }
            }
            foreach (string name in uniqueInputs)
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

    //NOT TESTED - currently happening
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
