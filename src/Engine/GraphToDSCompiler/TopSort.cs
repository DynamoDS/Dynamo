using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using ProtoCore.Utils;

namespace GraphToDSCompiler
{
    class TopSort
    {
        private static int NOT_VISTITED = 0;
        private static int VISITING = 1;
        private static int VISITED = 2;

        public static List<Node> sort(AST graph)
        {
            return DFS(graph);
        }

        public static List<Node> sort(Node node, AST statementList)
        {
            return DFS(node, statementList);
        }

        static List<Node> DFS(AST graph)
        {
            List<Node> nodes = graph.GetNodes();
            List<Node> sotedNodes = new List<Node>();
            Dictionary<Node, int> nodeStateMap = new Dictionary<Node, int>();
            IEnumerable iter = nodes;
            foreach (Node node in iter)
            {
                if (IsNotVisited(node, nodeStateMap))
                {
                    DFSVisit(node, nodeStateMap, sotedNodes);
                }
            }
            return sotedNodes;
        }

        static List<Node> DFS(Node node, AST statementList)
        {
            List<Node> sotedNodes = new List<Node>();
            Dictionary<Node, int> nodeStateMap = new Dictionary<Node, int>();
            if (IsNotVisited(node, nodeStateMap))
            {
                DFSVisit(node, nodeStateMap, sotedNodes, statementList);
            }
            return sotedNodes;
        }

        private static Boolean IsNotVisited(Node node, Dictionary<Node, int> nodeStateMap)
        {
            if (!nodeStateMap.ContainsKey(node))
            {
                return true;
            }
            int state = nodeStateMap[node];
            return NOT_VISTITED == state;
        }

        static void DFSVisit(Node node, Dictionary<Node, int> nodeStateMap, List<Node> list)
        {
            nodeStateMap.Add(node, VISITING);
            List<Node> nodes = node.GetChildren();
            IEnumerable iter = nodes;
            foreach (Node nodeI in iter)
            {
                if (IsNotVisited(nodeI, nodeStateMap))
                    DFSVisit(nodeI, nodeStateMap, list);
            }
            nodeStateMap[node] = VISITED;
            list.Add(node);
        }


        //
        // TODO Jun: Re-evaluate the topsort implementation
        //
        static void DFSVisit(Node node, Dictionary<Node, int> nodeStateMap, List<Node> list, AST statementList)
        {
            nodeStateMap.Add(node, VISITING);
            List<Node> nodes = node.GetChildren();
            Dictionary<int, Node> nodeDic = node.GetChildrenWithIndices();
            IEnumerable iter = nodes;
            int j = 0;
            foreach (Node nodeI in iter)
            {
                if (node is IdentNode && nodeI is LiteralNode)
                {
                    BuildIdentToLiteralStatement(node, nodeI, statementList);
                }
                else if (node is IdentNode && nodeI is IdentNode)
                {
                    BuildIdentToIdentStatement(node, nodeI, statementList);
                }
                else if (node is IdentNode && nodeI is Block)
                {
                    Block blocknode = (Block)nodeI;
                    if (GraphUtilities.AnalyzeString(blocknode.Name) == SnapshotNodeType.Literal)
                    {
                        LiteralNode literal = new LiteralNode(blocknode.content, nodeI.Guid);
                        BuildIdentToLiteralStatement(node, literal, statementList);
                    }
                    else
                    {
                        j = BuildIdentToCodeBlockIdentStatement(node, nodeI, nodes, statementList, j);
                    }
                }
                else if (node is Operator && nodeI is Block)
                {
                    j = BuildOperatorToBlockStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if (node is Func && nodeI is Block)
                {
                    j = BuildFuncToBlockStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if (node is Func && nodeI is IdentNode)
                {
                    j = BuildFuncToIdentStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if (node is IdentNode && nodeI is Operator)
                {
                    BuildIdentToOperatorStatement(node, statementList, nodeI);
                }
                else if (node is Operator && nodeI is Operator)
                {
                    j = BuildOperatorToOperatorStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if (node is IdentNode && nodeI is Func)
                {
                    BuildIdentToFuncStatement(node, nodeI, statementList);
                }
                else if (node is Func && nodeI is Func)
                {
                    j = BuildFuncToFuncStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if (node is Operator && nodeI is Func)
                {
                    j = BuildOperatorToOperatorStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if (node is Func && nodeI is Operator)
                {
                    j = BuildFuncToFuncStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if ((node is Operator && nodeI is ArrayNode) || (node is Operator && nodeI is LiteralNode))
                { 
                    j = BuildOperatorToOperatorStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if ((node is Func && nodeI is ArrayNode) || (node is Func && nodeI is LiteralNode))
                {
                    j = BuildFuncToFuncStatement(node, nodeI, nodeDic, statementList, j);
                }
                else if ((node is Block && nodeI is Block))
                {
                    BuildBlockToBlockStatement(node, nodeI, statementList);
                }
                else if ((node is Block && nodeI is Func))
                {
                    BuildBlockToFuncStatement(node, nodeI, statementList);
                }
                else if ((node is Block && nodeI is Operator))
                {
                    BuildBlockToFuncStatement(node, nodeI, statementList);
                }
                else if ((node is Block && nodeI is IdentNode))
                {
                    BuildBlockToIdentStatement(node, nodeI, statementList);
                }
                /*Block to Operator*/
                else if (node is Block && nodeI is Operator)
                {
                    //BuildBlockToOperatorStatement(node, nodeI, statementList);
                }
                //else if ((node is Block && nodeI is Func))
                //{
                //    BuildBlockToBlockStatement(node, nodeI, statementList);
                //}
                else
                {
                    if (node is Operator)
                    {
                        if (nodes.IndexOf(nodeI, j) == 0)
                        {
                            Assignment a = (Assignment)statementList.GetNode(node.Guid);
                            ((BinExprNode)a.right).left = nodeI;
                            ++j;
                        }
                        else
                        {
                            Assignment a = (Assignment)statementList.GetNode(node.Guid);
                            ((BinExprNode)a.right).right = nodeI;
                        }
                    }
                    else if (node is Func)
                    {
                        Assignment a = (Assignment)statementList.GetNode(node.Guid);
                        FunctionCall f = ((FunctionCall)a.right);
                        f.parameters[nodes.IndexOf(nodeI, j)] = nodeI;
                        j = 0;
                    }
                }

                if (IsNotVisited(nodeI, nodeStateMap))
                {
                    DFSVisit(nodeI, nodeStateMap, list, statementList);
                }
            }
            nodeStateMap[node] = VISITED;
            list.Add(node);
        }

        private static void BuildBlockToIdentStatement(Node node, Node nodeIdent, AST statementList)
        {
            Validity.Assert(node is Block);

            if (node.children.Count > 1)
            {
                statementList.AddNode(node);
            }
            else
            {
                IdentNode identNode = nodeIdent as IdentNode;
                Validity.Assert(null != identNode);

                Block block = node as Block;

                string lhs = string.Empty;
                string content = string.Empty;
                bool isSingleIdent = string.IsNullOrEmpty(block.LHS);
                if (isSingleIdent)
                {
                    lhs = block.TrimName();
                }
                else
                {
                    lhs = block.LHS;
                }

                // Create the cnontents of the new block. 
                content = lhs + '=' + identNode.Name;

                // Comment Jun: Remove the current codeblock first
                // Codeblock removal is guid dependent
                Node nodeToRemove = statementList.GetNode(node.Guid);
                int index = statementList.nodeList.IndexOf(nodeToRemove);
                statementList.RemoveNode(nodeToRemove);

                // Comment Jun: Create a new block using the current guid
                // This new codeblock represents the new contents
                Block block2 = new Block(content, node.Guid);
                statementList.nodeList.Insert(index, block2);
                statementList.nodeMap.Add(block2.Guid, block2);
                statementList.AddNode(block2);


                // Comment Jun: Reflect the new changes to the original block
                (node as Block).SetData(block2.LHS, content);
            }
        }

        private static void BuildBlockToFuncStatement(Node node, Node nodeFunc, AST statementList)
        {
            Validity.Assert(node is Block);

            if (node.children.Count > 1)
            {
                statementList.AddNode(node);
            }
            else
            {
                Assignment funcCall = (Assignment)statementList.GetNode(nodeFunc.Guid);

                Block block = node as Block;

                string lhs = string.Empty;
                string content = string.Empty;
                bool isSingleIdent = string.IsNullOrEmpty(block.LHS);
                if (isSingleIdent)
                {
                    lhs = block.TrimName();

                    // Create the cnontents of the new block. 
                    // The rhs of the block is the rhs of the function call statement
                    content = lhs + '=' + funcCall.left.Name;
                }
                else
                {
                    lhs = block.LHS;
                    content = block.Name;
                }


                // Comment Jun: Remove the current codeblock first
                // Codeblock removal is guid dependent
                Node nodeToRemove = statementList.GetNode(node.Guid);
                int index = statementList.nodeList.IndexOf(nodeToRemove);
                statementList.RemoveNode(nodeToRemove);
                
                // Comment Jun: Create a new block using the current guid
                // This new codeblock represents the new contents
                // Comment Tron: Insert the new node into the index where the removed node was originally
                Block block2 = new Block(content, node.Guid);
                statementList.nodeList.Insert(index, block2);
                statementList.nodeMap.Add(block2.Guid, block2);

                // Comment Jun: Reflect the new changes to the original block
                (node as Block).SetData(block2.LHS, content);
            }
        }

        private static void BuildBlockToBlockStatement(Node node, Node nodeI, AST statementList)
        {
            if (node.children.Count > 1)
            {
                statementList.AddNode(node);
            }
            else
            {
                string content = string.Empty;
                Block block = nodeI as Block;
                Validity.Assert(block != null);
                Assignment a1 = null;
                Node n1 = statementList.GetNode(node.Guid);
                if (n1 is Assignment)
                {
                    a1 = n1 as Assignment;

                    //
                    // Comment Jun:
                    // This condition basically checks if the single line codeblock is either a full expression or a single ident
                    // For now, in order to check for single ident, we check if the LHS if empty
                    // This needs refinement
                    bool isSingleIdent = string.IsNullOrEmpty((((Block)node).LHS));
                    if (isSingleIdent)
                    {
                        // This single line codeblock is a single identifier
                        a1.right.Name = block.LHS != "" ? block.LHS : block.Name.Replace(";", "").Trim();
                        content = a1.ToScript();
                    }
                    else
                    {
                        // This single line codeblock is a full expression
                        content = ((Block)node).Name;
                    }

                    // Comment Jun: Create a new block that represents the new contents
                    Block block2 = new Block(content, node.Guid);
                    int index = statementList.nodeList.IndexOf(a1);
                    statementList.RemoveNode(a1);


                    statementList.nodeList.Insert(index, block2);
                    statementList.nodeMap.Add(block2.Guid, block2);

                    // Comment Jun: Reflect the new changes to the original block
                    (node as Block).SetData(block2.LHS, content);
                }
            }
        }

        private static int BuildIdentToCodeBlockIdentStatement(Node node, Node nodeI, List<Node> nodes, AST statementList, int j)
        {
            Block block = nodeI as Block;
            Validity.Assert(block != null);

            // Comment Jun: Check if the codeblock is a single ident
            string lhs = block.LHS;
            if (string.IsNullOrEmpty(lhs))
            {
                lhs = block.TrimName();
            }

            Assignment a = new Assignment((IdentNode)node, new IdentNode(lhs, block.Guid));
            if (statementList.nodeMap.ContainsKey(a.Guid))
            {
                int index = statementList.nodeList.IndexOf(statementList.GetNode(a.Guid));
                statementList.RemoveNode(statementList.GetNode(a.Guid));

                statementList.nodeList.Insert(index, a);
                statementList.nodeMap.Add(a.Guid, a);
            }
            else
            {
                statementList.AddNode(a);
            }
                
            return ++j;
        }

        private static int BuildFuncToBlockStatement(Node node, Node nodeI, Dictionary<int, Node> nodeDic, AST statementList, int j)
        {
            Assignment a1 = (Assignment)statementList.GetNode(node.Guid);
            Block block = nodeI as Block;
            List<int> keys = GetKeysFromValue(nodeDic, nodeI);
            FunctionCall f = ((FunctionCall)a1.right);
            if (keys.Count > 1)
            {
                do
                {
                    f.parameters[keys[j]] = new IdentNode(block.LHS, block.Guid); 
                    j++;
                } while (j < keys.Count);
            }
            else
            {
                f.parameters[keys[j]] = new IdentNode(block.LHS, block.Guid);
            }
            j = 0;
            return j;
        }

        private static int BuildFuncToIdentStatement(Node node, Node nodeI, Dictionary<int, Node> nodeDic, AST statementList, int j)
        {
            Assignment a1 = (Assignment)statementList.GetNode(node.Guid);
            IdentNode identNode = nodeI as IdentNode;
            Validity.Assert(null != identNode);

            List<int> keys = GetKeysFromValue(nodeDic, nodeI);
            FunctionCall f = ((FunctionCall)a1.right);
            if (keys.Count > 1)
            {
                do
                {
                    f.parameters[keys[j]] = identNode;
                    j++;
                } while (j < keys.Count);
            }
            else
            {
                f.parameters[keys[j]] = identNode;
            }
            j = 0;
            return j;
        }

        private static int BuildOperatorToBlockStatement(Node node, Node nodeI, Dictionary<int, Node> nodeDic, AST statementList, int j)
        {

            Assignment a1 = (Assignment)statementList.GetNode(node.Guid);
            Block block = nodeI as Block;
            List<int> keys = GetKeysFromValue(nodeDic, nodeI);
            if (keys.Count > 1)
            {
                if (keys[j] == 0)
                {
                    ((BinExprNode)a1.right).left = block;
                    if (keys.Count == 2)
                        ++j;
                }
                else
                {
                    ((BinExprNode)a1.right).right = block;
                    if (keys.Count == 2)
                        ++j;
                }
            }
            else
            {
                if (keys[0] == 0)
                {
                    ((BinExprNode)a1.right).left = block;
                    if (keys.Count == 2)
                        ++j;
                }
                else
                {
                    ((BinExprNode)a1.right).right = block;
                    if (keys.Count == 2)
                        ++j;
                }
            }
            return j;
        }

        private static void BuildIdentToIdentStatement(Node node, Node nodeI, AST statementList)
        {
            Assignment a = new Assignment((IdentNode)node, (IdentNode)nodeI);
            if (statementList.nodeMap.ContainsKey(node.Guid))
            {
                Node nodeToRemove = statementList.GetNode(a.Guid);
                int index = statementList.nodeList.IndexOf(nodeToRemove);
                statementList.RemoveNode(nodeToRemove);

                statementList.nodeList.Insert(index, a);
                statementList.nodeMap.Add(a.Guid, a);
            }
            else
                statementList.AddNode(a);
        }

        private static void BuildIdentToLiteralStatement(Node node, Node nodeI, AST statementList)
        {
            Assignment a2 = (Assignment)statementList.GetNode(nodeI.Guid);
            Assignment a = new Assignment((IdentNode)node, (IdentNode)a2.left);
            if (statementList.nodeMap.ContainsKey(a.Guid))
            {
                Node nodeToRemove = statementList.GetNode(a.Guid);
                int index = statementList.nodeList.IndexOf(nodeToRemove);
                statementList.RemoveNode(nodeToRemove);

                statementList.nodeList.Insert(index, a);
                statementList.nodeMap.Add(a.Guid, a);
            }
            else
                statementList.AddNode(a);
        }

        private static void BuildIdentToFuncStatement(Node node, Node nodeI, AST statementList)
        {
            Assignment a2 = (Assignment)statementList.GetNode(nodeI.Guid);
            Assignment a = new Assignment((IdentNode)node, (IdentNode)a2.left);
            if (statementList.nodeMap.ContainsKey(a.Guid))
            {
                Node nodeToRemove = statementList.GetNode(a.Guid);
                int index = statementList.nodeList.IndexOf(nodeToRemove);
                statementList.RemoveNode(nodeToRemove);

                statementList.nodeList.Insert(index, a);
                statementList.nodeMap.Add(a.Guid, a);
            }
            else
                statementList.AddNode(a);
        }

        private static void BuildIdentToOperatorStatement(Node node, AST statementList, Node nodeI)
        {
            Assignment a2 = (Assignment)statementList.GetNode(nodeI.Guid);
            Assignment a = new Assignment((IdentNode)node, (IdentNode)a2.left);
            if (statementList.nodeMap.ContainsKey(a.Guid))
            {
                Node nodeToRemove = statementList.GetNode(a.Guid);
                int index = statementList.nodeList.IndexOf(nodeToRemove);
                statementList.RemoveNode(nodeToRemove);

                statementList.nodeList.Insert(index, a);
                statementList.nodeMap.Add(a.Guid, a);
            }
            else
                statementList.AddNode(a);
        }
        public static List<int> GetKeysFromValue(Dictionary<int, Node> dict, Node node)
        {
            List<int> ks = new List<int>();
            foreach (int k in dict.Keys)
            {
                if (dict[k].Equals(node)) 
                {
                    ks.Add(k); 
                }
            }
            return ks;
        }
        private static int BuildOperatorToOperatorStatement(Node node, Node nodeI, Dictionary<int, Node> nodeDic, AST statementList, int j)
        {
            Assignment a1 = (Assignment)statementList.GetNode(node.Guid);
            Assignment a2 = (Assignment)statementList.GetNode(nodeI.Guid);
            List<int> keys = GetKeysFromValue(nodeDic, nodeI);
            if (keys.Count > 1)
            {
                if (keys[j] == 0)
                {
                    ((BinExprNode)a1.right).left = a2.left; if (keys.Count == 2) ++j;
                }
                else
                {
                    ((BinExprNode)a1.right).right = a2.left; if (keys.Count == 2) ++j;
                }
            }
            else
            {
                if (keys[0] == 0)
                {
                    ((BinExprNode)a1.right).left = a2.left; if (keys.Count == 2) ++j;
                }
                else
                {
                    ((BinExprNode)a1.right).right = a2.left; if (keys.Count == 2) ++j;
                }
            }
            return j;
        }

        private static int BuildFuncToFuncStatement(Node node, Node nodeI, Dictionary<int, Node> nodeDic, AST statementList, int j)
        {
            Assignment a1 = (Assignment)statementList.GetNode(node.Guid);
            Assignment a2 = (Assignment)statementList.GetNode(nodeI.Guid);
            List<int> keys = GetKeysFromValue(nodeDic, nodeI);
            FunctionCall f = ((FunctionCall)a1.right);
            if (keys.Count > 1)
                do
                {
                    f.parameters[keys[j]] = a2.left; j++;
                } while (j < keys.Count);
            else f.parameters[keys[j]] = a2.left;
            j = 0;
            return j;
        }


    }
}
