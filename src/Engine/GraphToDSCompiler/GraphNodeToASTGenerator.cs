using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Utils;
using ProtoCore.AST.AssociativeAST;

namespace GraphToDSCompiler
{
    /// <summary>
    /// This class takes in Snapshot nodes and converts them into Proto AST nodes
    /// to be used for Node to Code conversion
    /// </summary>
    class GraphNodeToASTGenerator
    {
        public List<SnapshotNode> SnapshotNodeList { get; private set; }
        public ProtoCore.AST.Node AstRootNode { get; private set; }
        public List<AssociativeNode> AstList
        {
            get
            {
                List<AssociativeNode> astNodes = new List<AssociativeNode>();
                if (AstRootNode != null)
                {
                    CodeBlockNode rootNode = AstRootNode as CodeBlockNode;
                    if (rootNode != null)
                    {
                        foreach (var node in rootNode.Body)
                        {
                            AssociativeNode assocNode = node as AssociativeNode;
                            if(assocNode != null)
                                astNodes.Add(assocNode);
                        }
                    }
                }
                return astNodes;
            }
        }

        private GraphCompiler gc;
        private AST graph;
        public AST Graph
        {
            get
            {
                return graph;
            }
        }

        public GraphNodeToASTGenerator(List<SnapshotNode> snapshotNodeList)
        {
            SnapshotNodeList = snapshotNodeList;
        }

        public GraphNodeToASTGenerator(AST graph, GraphCompiler gc)
        {
            this.graph = graph;
            this.gc = gc;
        }

       
        public void BuildGraphFromNodes(List<SnapshotNode> snapshotNodeList)
        {
            // Instantiate SynchronizeData and GraphCompiler
            SynchronizeData syncData = new SynchronizeData();
            syncData.AddedNodes = snapshotNodeList;

            GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            GraphBuilder gb = new GraphBuilder(syncData, gc);

            gb.AddNodesToAST();
            gb.MakeConnectionsForAddedNodes();

            graph = gb.Graph;
        }

        // TODO: Deprecate
        private static IdentifierNode CreateTempIdentifierNode(Node node)
        {
            uint tguid = GraphCompiler.GetTempGUID(node.Guid);
            string tempName = GraphToDSCompiler.kw.tempPrefix + tguid;

            return new IdentifierNode(tempName);
        }

        // TODO: Deprecate
        private static BinaryExpressionNode CreateBinaryExpNode(Node node, AssociativeNode expressionNode)
        {
            BinaryExpressionNode assignmentNode = new BinaryExpressionNode();
            assignmentNode.LeftNode = CreateTempIdentifierNode(node);
            assignmentNode.LeftNode = new IdentifierNode();
            assignmentNode.Optr = ProtoCore.DSASM.Operator.assign;
            assignmentNode.RightNode = expressionNode;
            assignmentNode.Guid = node.Guid;

            return assignmentNode;
        }

        public List<AssociativeNode> SplitAST()
        {
            Validity.Assert(this.AstList != null);

            List<AssociativeNode> splitAstList = new List<AssociativeNode>();

            foreach (AssociativeNode node in this.AstList)
            {
                AssociativeNode outnode = null;
                TraverseToSplit(node, out outnode, ref splitAstList);
            }

            return splitAstList;
        }

        private void TraverseToSplit(AssociativeNode node, out AssociativeNode outNode, ref List<AssociativeNode> splitList)
        {
            if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode ben = node as BinaryExpressionNode;
                
                BinaryExpressionNode newNode = new BinaryExpressionNode();
                AssociativeNode lNode = null;
                TraverseToSplit(ben.LeftNode, out lNode, ref splitList);
                newNode.LeftNode = lNode;
                newNode.Optr = ben.Optr;

                AssociativeNode rNode = null;
                TraverseToSplit(ben.RightNode, out rNode, ref splitList);
                newNode.RightNode = rNode;

                if (ben.Optr == ProtoCore.DSASM.Operator.assign)
                {
                    if (NotEnlisted(splitList, newNode))
                    {
                        splitList.Add(newNode);
                    }
                    outNode = lNode;
                }
                else
                    outNode = newNode;
                
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode funcCallNode = node as FunctionCallNode;
                AssociativeNode statement = null;
                foreach (AssociativeNode argNode in funcCallNode.FormalArguments)
                    TraverseToSplit(argNode, out statement, ref splitList);
                for (int i=0; i<funcCallNode.FormalArguments.Count; i++)
                {
                    AssociativeNode argNode = funcCallNode.FormalArguments[i];
                    if (argNode is BinaryExpressionNode)
                        funcCallNode.FormalArguments[i] = (argNode as BinaryExpressionNode).LeftNode;
                }
                //if (statement is BinaryExpressionNode)
                //{
                //    splitList.Add(statement);
                //}
                outNode = funcCallNode;
            }
            else if (node is FunctionDotCallNode)
            {
                FunctionDotCallNode funcDotNode = node as FunctionDotCallNode;
                AssociativeNode statement = null;
                TraverseToSplit(funcDotNode.FunctionCall, out statement, ref splitList);
                funcDotNode.FunctionCall = (statement as FunctionCallNode);
                TraverseToSplit(funcDotNode.DotCall.FormalArguments[0], out statement, ref splitList);
                if (statement is BinaryExpressionNode)
                    funcDotNode.DotCall.FormalArguments[0] = (statement as BinaryExpressionNode).LeftNode;
                else
                    funcDotNode.DotCall.FormalArguments[0] = statement;
                outNode = funcDotNode;
            }
            else if (node is ProtoCore.AST.AssociativeAST.ImportNode)
            {
                outNode = node;
                splitList.Add(outNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ArrayIndexerNode)
            {
                ArrayIndexerNode arrIdxNode = node as ArrayIndexerNode;
                AssociativeNode statement = null;
                TraverseToSplit(arrIdxNode.Array, out statement, ref splitList);
                arrIdxNode.Array = statement;
                outNode = arrIdxNode;
            }
            else if (node is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                ExprListNode exprListNode = node as ExprListNode;
                AssociativeNode statement = null;
                //for (int i=0; i<exprListNode.list.Count; i++)
                foreach (AssociativeNode listNode in exprListNode.list)
                {
                    TraverseToSplit(listNode, out statement, ref splitList);
                }
                for (int i = 0; i < exprListNode.list.Count; i++)
                {
                    AssociativeNode argNode = exprListNode.list[i];
                    if (argNode is BinaryExpressionNode)
                        exprListNode.list[i] = (argNode as BinaryExpressionNode).LeftNode;
                }
                outNode = exprListNode;
            }
            //else if (node is ProtoCore.AST.AssociativeAST.ArrayNode)
            //{
            //    k
            //}
            else if (node is ProtoCore.AST.AssociativeAST.RangeExprNode)
            {
                RangeExprNode rangeExprNode = node as RangeExprNode;
                AssociativeNode statement = null;
                TraverseToSplit(rangeExprNode.FromNode, out statement, ref splitList);
                TraverseToSplit(rangeExprNode.StepNode, out statement, ref splitList);
                TraverseToSplit(rangeExprNode.ToNode, out statement, ref splitList);
                if (rangeExprNode.FromNode is BinaryExpressionNode)
                {
                    rangeExprNode.FromNode = (rangeExprNode.FromNode as BinaryExpressionNode).LeftNode;
                }
                if (rangeExprNode.StepNode is BinaryExpressionNode)
                {
                    rangeExprNode.StepNode = (rangeExprNode.StepNode as BinaryExpressionNode).LeftNode;
                }
                if (rangeExprNode.ToNode is BinaryExpressionNode)
                {
                    rangeExprNode.ToNode = (rangeExprNode.ToNode as BinaryExpressionNode).LeftNode;
                }
                outNode = rangeExprNode;
            }
            else
            {
                outNode = node;
            }
        }

        private bool NotEnlisted(List<AssociativeNode> splitList, BinaryExpressionNode newNode)
        {
            bool result = true;
            foreach (AssociativeNode node in splitList)
            {
                if (node is BinaryExpressionNode)
                {
                    BinaryExpressionNode ben = node as BinaryExpressionNode;
                    //if ((ben.LeftNode.ID == newNode.LeftNode.ID || ben.LeftNode.Name == newNode.LeftNode.Name) &&
                    //        ben.Optr == newNode.Optr && (ben.RightNode.ID == newNode.RightNode.ID || ben.RightNode.Name == newNode.RightNode.Name))
                    if (ben.Equals(newNode))
                        result = false;
                }
            }
            return result;
        }

        private List<Node> FindRoots()
        {
            List<Node> rootNodes = new List<Node>();

            Validity.Assert(Graph != null);
            List<Node> graphNodes = Graph.GetNodes();
            foreach (Node node in graphNodes)
            {
                if(node.GetParents().Count() == 0)
                    rootNodes.Add(node);
            }
            return rootNodes;
        }
        
        public void AddNodesToAST()
        {
            AstRootNode = new CodeBlockNode();

            List<Node> rootNodes = FindRoots();
            Validity.Assert(rootNodes.Count() > 0);

            // TODO Jun: Check with Aparajit, the relevance of this assert...
            //Validity.Assert(GraphUtilities.ImportNodes != null);
            
            // TODO: Aparajit: To check if the AST nodes for preloaded assemblies and built-ins need to be added to this AST
            /*foreach(AssociativeNode node in GraphUtilities.ImportNodes.Body)
            {
                (AstRootNode as CodeBlockNode).Body.Add(node);
            }*/

            // First emit all the ImportNode's
            foreach (Node rNode in rootNodes)
            {
                ImportNode importNode = rNode as ImportNode;
                if (importNode != null)
                {
                    AssociativeNode outNode = null;
                    EmitImportNode(importNode, out outNode);

                    (AstRootNode as CodeBlockNode).Body.Add(outNode);
                }
                else if (importNode == null)
                {
                    AssociativeNode outNode = null;
                    DFSTraverse(rNode, out outNode);

                    (AstRootNode as CodeBlockNode).Body.Add(outNode);
                }
            }

        }       

        private void DFSTraverse(Node rootNode, out AssociativeNode outnode)
        {
            AssociativeNode node = null;
            if (rootNode is LiteralNode)
            {
                EmitLiteralNode(rootNode as LiteralNode, out node);
            }
            else if (rootNode is IdentNode)
            {
                EmitIdentifierNode(rootNode as IdentNode, out node);
            }
            else if (rootNode is Operator)
            {
                EmitBinaryExpNode(rootNode as Operator, out node);
            }
            else if (rootNode is Block)
            {
                // Create BinaryExpressionNode
                EmitBlockNode(rootNode as Block, out node);
            }
            else if (rootNode is Func)
            {
                EmitFunctionNode(rootNode as Func, out node);
            }
            

            outnode = node;
        }

        #region Emit ProtoAST Functions

        /// <summary>
        /// TODO: Deprecate
        /// Emit IntNode or DoubleNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outnode"></param>
        private void EmitLiteralNode(LiteralNode node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);
            Validity.Assert(node.children.Count == 0);

            // Create temp identifier and assign it to the literal (IntNode or DoubleNode) to create a BinaryExpressionNode
            // Return the temp IdentifierNode
            //BinaryExpressionNode expressionNode = new BinaryExpressionNode();
            AssociativeNode rightNode = null;
            
            int number;
            double real;
            bool flag;
            string val = node.ToScript();
            // If LiternalNode is double
            if(Double.TryParse(val, out real))
            {
                rightNode = new DoubleNode() { value = node.ToScript() };
            }
            // If LiteralNode type is Int
            else if (Int32.TryParse(val, out number))
            {
            
                rightNode = new IntNode() { value = val };            
            }            
            // If LiteralNode is bool
            else if (Boolean.TryParse(val, out flag))
            {
                rightNode = new BooleanNode() { value = node.ToScript() };
            }

            /*IdentifierNode ident = CreateTempIdentifierNode(node);
            expressionNode.RightNode = rightNode;
            expressionNode.LeftNode = ident;
            expressionNode.Optr = ProtoCore.DSASM.Operator.assign;*/

            //(AstRootNode as CodeBlockNode).Body.Add(expressionNode);

            //outnode = expressionNode;
            outnode = CreateBinaryExpNode(node, rightNode);
        }


        private void EmitIdentifierNode(IdentNode node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);

            Dictionary<int, Node> nodes = node.GetChildrenWithIndices();
            Validity.Assert(nodes.Count <= 1);

            // Create BinaryExpressionNode with the lhs as the IdentifierNode and the rhs as the temp Node emitted from the child
            // Return the IdentifierNode
            BinaryExpressionNode expressionNode = new BinaryExpressionNode();

            if (nodes.Count > 0)
            {
                AssociativeNode tmpNode = null;
                DFSTraverse(nodes[0], out tmpNode);
                expressionNode.RightNode = tmpNode;
            }
            else
            {
                // Create IdentifierNode and assign it to null
                expressionNode.RightNode = new NullNode();
            }
            IdentifierNode leftNode = new IdentifierNode(node.Name);
            expressionNode.LeftNode = leftNode;
            expressionNode.Optr = ProtoCore.DSASM.Operator.assign;
            expressionNode.Guid = node.Guid;

            Validity.Assert(gc != null);
            gc.HandleNewNode(expressionNode);
            //(AstRootNode as CodeBlockNode).Body.Add(expressionNode);
            outnode = expressionNode;
        }

        private void EmitBinaryExpNode(Operator node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);

            Dictionary<int, Node> nodes = node.GetChildrenWithIndices();
            Validity.Assert(nodes.Count <= 2);

            BinaryExpressionNode expressionNode = new BinaryExpressionNode();

            // Create operator from input node
            switch (node.Name)
            {
                case "=":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.assign;
                    break;
                case "+":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.add;
                    break;
                case "-":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.sub;
                    break;
                case "*":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.mul;
                    break;
                case "/":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.div;
                    break;
                case "%":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.mod;
                    break;
                case "==":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.eq;
                    break;
                case "!=":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.nq;
                    break;
                case ">=":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.ge;
                    break;
                case ">":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.gt;
                    break;
                case "<=":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.le;
                    break;
                case "<":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.lt;
                    break;
                case "&&":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.and;
                    break;
                case "||":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.or;
                    break;
                case "&":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.bitwiseand;
                    break;
                case "|":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.bitwiseor;
                    break;
                case "^":
                    expressionNode.Optr = ProtoCore.DSASM.Operator.bitwisexor;
                    break;
                default: break;
            }

            AssociativeNode identNode1 = new NullNode();
            AssociativeNode identNode2 = new NullNode();

            if (nodes.Count == 2)
            {
                // Create BinaryExpressionNode from identNode1, identNode2 and operator                
                DFSTraverse(nodes[0], out identNode1);
                DFSTraverse(nodes[1], out identNode2);
                
            }
            else if (nodes.Count == 1)
            {
                // Create BinaryExpressionNode from identNode1, null 
                DFSTraverse(nodes[0], out identNode1);
            }
            expressionNode.LeftNode = identNode1;
            expressionNode.RightNode = identNode2;

            //(AstRootNode as CodeBlockNode).Body.Add(expressionNode);
            BinaryExpressionNode assignmentNode = new BinaryExpressionNode();
            assignmentNode.LeftNode = new IdentifierNode(node.tempName);
            assignmentNode.Optr = ProtoCore.DSASM.Operator.assign;
            assignmentNode.RightNode = expressionNode;
            assignmentNode.Guid = node.Guid;

            Validity.Assert(gc != null);
            gc.HandleNewNode(assignmentNode);

            outnode = assignmentNode;
        }


        /// <summary>
        /// Create BinaryExpressionNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outnode"></param>
        private void EmitBlockNode(Block node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);
            
            // TODO: Confirm that these children are returned in the order that they
            // appear in the code
            Dictionary<int, Node> childNodes = node.GetChildrenWithIndices();

            // Parse program statement in node.Name
            string code = node.Name + ";";
            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode = null;
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlockNode = (ProtoCore.AST.AssociativeAST.CodeBlockNode)GraphUtilities.Parse(code, out commentNode);
            Validity.Assert(codeBlockNode != null);

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = codeBlockNode.Body;
            Validity.Assert(astList.Count == 1);

            if (astList[0] is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                ProtoCore.AST.AssociativeAST.BinaryExpressionNode ben = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode();
                ben.LeftNode = astList[0];
                ben.Optr = ProtoCore.DSASM.Operator.assign;
                ProtoCore.AST.AssociativeAST.AssociativeNode statement = null;
                foreach (KeyValuePair<int, Node> kvp in childNodes)
                    DFSTraverse(kvp.Value, out statement);
                ben.RightNode = statement;
                astList[0] = ben;
            }

            //I don't know what I am doing
            if (astList[0] is BinaryExpressionNode)
            {
                BinaryExpressionNode tempBen = astList[0] as BinaryExpressionNode;
                if (tempBen.LeftNode is IdentifierNode)
                {
                    IdentifierNode identitiferNode = tempBen.LeftNode as IdentifierNode;
                    if (identitiferNode.ArrayDimensions != null)
                    {
                        ArrayIndexerNode arrIndex = new ArrayIndexerNode();
                        arrIndex.ArrayDimensions = identitiferNode.ArrayDimensions;
                        arrIndex.Array = identitiferNode;
                        tempBen.LeftNode = arrIndex;
                    }
                }
                if (tempBen.RightNode is IdentifierNode)
                {
                    IdentifierNode identitiferNode = tempBen.RightNode as IdentifierNode;
                    if (identitiferNode.ArrayDimensions != null)
                    {
                        ArrayIndexerNode arrIndex = new ArrayIndexerNode();
                        arrIndex.ArrayDimensions = identitiferNode.ArrayDimensions;
                        arrIndex.Array = identitiferNode;
                        tempBen.RightNode = arrIndex;
                    }
                }
                astList[0] = tempBen;
            }
            //it should be correct, if not, debug?

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode bNode = astList[0] as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
            Validity.Assert(bNode != null);
            bNode.Guid = node.Guid;
            //bNode.SplitFromUID = node.splitFomUint;

            // Child nodes are arguments to expression in bNode.RightNode. 
            // Match child nodes with IdentifierNode's in bNode.RightNode - pratapa
            foreach (Node n in childNodes.Values)
            {
                AssociativeNode argNode = null;
                DFSTraverse(n, out argNode);

                // DFS traverse the bNode.RightNode and check for IdentifierNode's
                // and if their names match with the names of argNode, then replace
                // the IdentifierNode in bNode.RightNode with argNode
                BinaryExpressionNode ben = argNode as BinaryExpressionNode;
                Validity.Assert(ben != null);
                //ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(ben);
                AstCodeBlockTraverse sourceGen = new AstCodeBlockTraverse(ben);
                ProtoCore.AST.AssociativeAST.AssociativeNode right = bNode.RightNode;
                sourceGen.DFSTraverse(ref right);
                bNode.RightNode = right;
            }

            //(AstRootNode as CodeBlockNode).Body.Add(expressionNode);

            Validity.Assert(gc != null);
            gc.HandleNewNode(bNode);

            outnode = bNode;
        }

        private void EmitFunctionNode(Func node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);

            AssociativeNode fNode = new NullNode();
            string funcQualifier = node.Name;
            if (node.isRange)
            {
                EmitRangeExpNode(node, out fNode);
            }
            else if (!funcQualifier.Contains("."))
            {
                if (node.isProperty)
                {
                    Dictionary<int, Node> nodes = node.GetChildrenWithIndices();
                    Validity.Assert(nodes.Count == 1);
                    for (int i = 0; i < nodes.Count; ++i)
                    {
                        AssociativeNode instanceNode = null;
                        DFSTraverse(nodes[i], out instanceNode);
                        
                        EmitFunctionCallNode(node, out fNode);
                        ((fNode as FunctionCallNode).Function as IdentifierNode).Value = ProtoCore.DSASM.Constants.kGetterPrefix + ((fNode as FunctionCallNode).Function as IdentifierNode).Value;
                        //string className = (node.Name.Split('.'))[0];
                        //IdentifierNode inode = new IdentifierNode(className);

                        fNode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(instanceNode, fNode as FunctionCallNode);
                    }
                }
                else if (node.isMemberFunction)
                {
                    Dictionary<int, Node> nodes = node.GetChildrenWithIndices();                    
                    
                    AssociativeNode instanceNode = null;
                    DFSTraverse(nodes[0], out instanceNode);

                    EmitFunctionCallNode(node, out fNode);
                    //string className = (node.Name.Split('.'))[0];
                    //IdentifierNode inode = new IdentifierNode(className);

                    fNode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(instanceNode, fNode as FunctionCallNode);                    
                }
                else
                {
                    // Create AssociativeAST.FunctionCallNode for global and built-in functions
                    EmitFunctionCallNode(node, out fNode);
                }
                
            }
            else
            {
                // Create FunctionDotCallNode for ctors, static and instance methods
                EmitFunctionDotCallNode(node, out fNode);
            }

            BinaryExpressionNode assignmentNode = new BinaryExpressionNode();
            assignmentNode.LeftNode = new IdentifierNode(node.tempName);
            assignmentNode.Optr = ProtoCore.DSASM.Operator.assign;
            assignmentNode.RightNode = fNode;
            assignmentNode.Guid = node.Guid;

            Validity.Assert(gc != null);
            gc.HandleNewNode(assignmentNode);

            outnode = assignmentNode;
        }

        private void EmitFunctionDotCallNode(Func node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);

            AssociativeNode fNode = null;
            EmitFunctionCallNode(node, out fNode);

            string className = (node.Name.Split('.'))[0];
            IdentifierNode inode = new IdentifierNode(className);
            
            outnode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, fNode as FunctionCallNode);
        }

        private void EmitFunctionCallNode(Func node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);

            FunctionCallNode fNode = new FunctionCallNode();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            
            if (node.Name.Contains("."))
            {
                string funcName = (node.Name.Split('.'))[1];
                fNode.Function = new IdentifierNode(funcName);
            }
            //else if (node.GetChildrenWithIndices().Count != 0)
            //{

            //}
            else
                fNode.Function = new IdentifierNode(node.Name);

            
            for (int i = 0; i < node.numParameters; i++)
                args.Add(new NullNode());

            if (args.Count > 0)
            {
                Dictionary<int, Node> nodes = node.GetChildrenWithIndices();
                for (int i = 0; i < nodes.Count; ++i)
                {
                    AssociativeNode argNode = null;
                    DFSTraverse(nodes[i], out argNode);
                    args.RemoveAt(i);
                    args.Insert(i, argNode);
                }
            }

            fNode.FormalArguments = args;

            outnode = fNode;
        }

        private void EmitRangeExpNode(Func node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);
            Validity.Assert(node.isRange);

            RangeExprNode rangeNode = new RangeExprNode();
            
            // Set FromNode, ToNode, stepOperator and StepNode for rangeNode
            Dictionary<int, Node> nodes = node.GetChildrenWithIndices();
            //int numParams = node.numParameters;

            AssociativeNode startNode = null;
            AssociativeNode endNode = null;
            AssociativeNode stepNode = null;

            if (nodes.Count >= 1)
            {                
                DFSTraverse(nodes[0], out startNode);
            }
            if (nodes.Count >= 2)
            {
                DFSTraverse(nodes[1], out endNode);
            }
            if (nodes.Count >= 3)
            {
                DFSTraverse(nodes[2], out stepNode);
            }
            rangeNode.FromNode = startNode;
            rangeNode.ToNode = endNode;
            rangeNode.StepNode = stepNode;
            
            if (node.Name == "Range.ByIncrementValue")
            {
                rangeNode.stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize;
            }
            else if (node.Name == "Range.ByIntervals")
            {
                rangeNode.stepoperator = ProtoCore.DSASM.RangeStepOperator.num;
            }
            else
            {
                rangeNode.stepoperator = ProtoCore.DSASM.RangeStepOperator.approxsize;
            }

            outnode = rangeNode;
            //outnode = CreateBinaryExpNode(node, rangeNode);
        }

        private void EmitImportNode(ImportNode node, out AssociativeNode outnode)
        {
            Validity.Assert(node != null);

            ProtoCore.AST.AssociativeAST.ImportNode importNode = null;

            importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
            importNode.ModuleName = node.ModuleName;

            //(AstRootNode as CodeBlockNode).Body.Add(importNode);
            outnode = importNode;
        }

        #endregion
    }
}
