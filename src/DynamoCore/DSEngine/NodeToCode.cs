using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using Dynamo.Utilities;
using ProtoCore.SyntaxAnalysis;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// The result of converting nodes to code. As when a node is converted to 
    /// code, its inputs and outputs may be renamed to avoid confliction, the
    /// result contains maps for inputs/new-inputs and outputs/new-outputs.
    /// </summary>
    public class NodeToCodeResult
    {
        /// <summary>
        /// AST nodes that compiled from NodeModel.
        /// </summary>
        public IEnumerable<AssociativeNode> AstNodes { get; private set; }

        /// <summary>
        /// The map between original input name and new name.
        /// </summary>
        public Dictionary<string, string> InputMap { get; private set; }

        /// <summary>
        /// The map between original output name and new name.
        /// </summary>
        public Dictionary<string, string> OutputMap { get; private set; }

        public NodeToCodeResult(
            IEnumerable<AssociativeNode> astNodes, 
            Dictionary<string, string> inputMap, 
            Dictionary<string, string> outputMap)
        {
            AstNodes = astNodes;
            InputMap = inputMap;
            OutputMap = outputMap;
        }
    }
    public class NodeToCodeUtils
    {
        /// <summary>
        /// Traverse all identifiers in depth-first order and for each 
        /// identifier apply a function to it.
        /// </summary>
        private class IdentifierVisitor : AssociativeAstVisitor 
        {
            private Action<IdentifierNode> identFunc;
            public IdentifierVisitor(Action<IdentifierNode> func)
            {
                identFunc = func;
            }

            public override void VisitIdentifierNode(IdentifierNode node)
            {
                identFunc(node);
                if (node.ArrayDimensions != null)
                    node.ArrayDimensions.Accept(this);
            }
        }

        /// <summary>
        /// Replace identfier's name with the other name.
        /// </summary>
        private class IdentifierReplacer : AssociativeAstVisitor
        {
            private string oldVariable;
            private string newVariable;

            public IdentifierReplacer(string oldVar, string newVar)
            {
                oldVariable = oldVar;
                newVariable = newVar;
            }

            public override void VisitIdentifierNode(IdentifierNode node)
            {
                if (node.Value.Equals(oldVariable))
                    node.Value = newVariable;

                if (node.ArrayDimensions != null)
                    node.ArrayDimensions.Accept(this);
            }

            public override void VisitBinaryExpressionNode(BinaryExpressionNode node)
            {
                node.RightNode.Accept(this);
            }
        }

        private class ConstantIdentifierReplacer : AstReplacer 
        {
            private string variableName;
            private AssociativeNode constantValue;

            public ConstantIdentifierReplacer(string variable, AssociativeNode newValue)
            {
                variableName = variable;
                constantValue = newValue;
            }

            public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
            {
                if (node.Value.Equals(variableName))
                    return constantValue;
                else
                    return node;
            }

            public override AssociativeNode VisitBinaryExpressionNode(BinaryExpressionNode node)
            {
                var leftNode = node.LeftNode;
                var newRightNode = node.RightNode.Accept(this);
                if (newRightNode == node.RightNode)
                {
                    return node;
                }
                else
                {
                    var newNode = new BinaryExpressionNode(node.LeftNode, newRightNode, node.Optr);
                    return newNode;
                }
            }
        }

        /// <summary>
        /// The numbering state of a variable.
        /// </summary>
        private class NumberingState
        {
            /// <summary>
            /// Variable name.
            /// </summary>
            public string Variable { get;  private set; }

            /// <summary>
            /// Variable name with number suffix.
            /// </summary>
            public string NumberedVariable
            {
                get { return Variable + ((ID == 0) ? String.Empty : ID.ToString()); }
            }

            /// <summary>
            /// Number suffix.
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Indicate it is for ASTs in a new code block
            /// </summary>
            public bool IsNewSession { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="variable"></param>
            public NumberingState(string variable)
            {
                Variable = variable;
                ID = 0;
                IsNewSession = false;
            }

            /// <summary>
            /// Increase number. 
            /// </summary>
            public void BumpID()
            {
                ID += 1;
            }
        }

        /// <summary>
        /// Generate short variable name.
        /// </summary>
        private class ShortNameGenerator
        {
            private NumberingState ns = new NumberingState(AstBuilder.StringConstants.ShortVarPrefix);

            public ShortNameGenerator()
            {

            }
            public string GetNextName()
            {
                ns.BumpID();
                return ns.NumberedVariable;
            }
        }

        private static void MarkConnectivityForNode(
            HashSet<NodeModel> selection,
            bool[,] connectivityMatrix,
            Dictionary<NodeModel, int> nodeMap,
            List<NodeModel> path)
        {
            var leave = path.Last();
            var children = leave.OutputNodes.Values.SelectMany(s => s).Select(t => t.Item2);
            if (children.Any())
            {
                foreach (var child in children)
                {
                    if (path.Contains(child))
                        continue;

                    path.Add(child);
                    MarkConnectivityForNode(selection, connectivityMatrix, nodeMap, path);
                }
            }
            else
            {
                var nodes = path.ToList();
                var idx1 = nodeMap[path.First()];

                int k = nodes.FindIndex(n => !n.IsConvertible || !selection.Contains(n));
                if (k >= 0)
                {
                    for (int i = k + 1; i < nodes.Count; i++)
                    {
                        int idx2;
                        if (!nodeMap.TryGetValue(nodes[i], out idx2))
                            continue;

                        connectivityMatrix[idx1, idx2] = false;
                        connectivityMatrix[idx2, idx1] = false;
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
        }

        /// <summary>
        /// For a selection, output partitions that each group of node can be
        /// converted to code.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static List<List<NodeModel>> GetCliques(
            IEnumerable<NodeModel> selection)
        {
            var selectionSet = new HashSet<NodeModel>(selection);
            var convertibleNodes = selection.Where(n => n.IsConvertible).ToList();
            var count = convertibleNodes.Count;

            var nodeDict = convertibleNodes.Zip(Enumerable.Range(0, count), (n, i) => new { n, i})
                                           .ToDictionary(x => x.n, x => x.i);

            // Connectivity matrix represents whether two nodes are allowed to
            // be in the same code block or not. 
            bool[,] connectivityMatrix = new bool[count, count];
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    connectivityMatrix[i, j] = true;
                }
            }
            
            // For two convertible nodes if there is path from one to the other
            // goes through some node that is unconvertible or not in the 
            // selection set, these two nodes cannot be in the same coee block,
            // otherwise there will be a circular reference. For example:
            //
            //     a -> x -> b
            //
            // And the final graph would be
            //
            //    a, b <--> x
            // 
            // So here for a node which is convertible and from selection set, 
            // find all its upstream paths, and if any path contains node that 
            // is not in the selection set or is not convertible, mark all 
            // nodes after this node along path as not reachable. 
            foreach (var node in convertibleNodes)
            {
                var path = new List<NodeModel>();
                path.Add(node);

                MarkConnectivityForNode(selectionSet, connectivityMatrix, nodeDict, path);
            }

            var partitions = new List<List<NodeModel>>();
            var visited = new HashSet<int>();

            // Now find all maximum cliques, each clique is a group of nodes 
            // that could be in the same code block
            for (int v = 0; v < count; ++v)
            {
                if (visited.Contains(v))
                    continue;

                List<int> clique = new List<int>() { v };
                visited.Add(v);

                for (int w = 0; w < count; ++w)
                {
                    if (v == w || !connectivityMatrix[v, w] || visited.Contains(w))
                        continue;

                    if (clique.All(x => connectivityMatrix[x, w]))
                    {
                        clique.Add(w);
                        visited.Add(w);
                    }
                }

                partitions.Add(clique.Select(x => nodeDict.First(p => p.Value == x).Key).ToList());
            }
            return partitions;
        }

        /// <summary>
        /// Collect input, output and renaming maps.
        ///
        /// Inputs are input variables from external node models, finally
        /// shorter names will be given for these inputs. Note when the
        /// function returns, values in inputMap are always empty strings.
        ///
        /// Outputs are variables that output to external node models. 
        /// Shorter names will be given for these outupts. But if outputs are
        /// from code block node, as variables may be re-named during variable
        /// numbering, we can't simply record orignal output variable names. So
        /// when function returns, if a variable is from output port of non 
        /// code block node, there is an entry:
        /// 
        ///     outputMap[var_node_model_guid] = "" 
        ///  
        /// If a variable, say "foo", is from code block node, there are two 
        /// entries:
        /// 
        ///     outputMap[foo_node_model_guid] = "foo"
        ///     outputMap[foo%node_model_guid] = foo_node_model_guid
        /// 
        /// We need second entry because because finally we will use outputMap
        /// to restore the connection, and if "foo" is renamed to something 
        /// else, say "foo21", we can find the value of key from the second 
        /// entry and update the first entry to:
        /// 
        ///     outputMap[foo_ndoe_mode_guid] = "foo21".
        /// 
        /// Renaming map are similar to output map. The difference is these
        /// variables used internally and they may be renamed as well. In 
        /// some sense, it is just subset of outputMap.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="inputMap"></param>
        /// <param name="outputMap"></param>
        /// <param name="renamingMap"></param>
        private static void GetInputOutputMap(
            IEnumerable<NodeModel> nodes, 
            out Dictionary<string, string> inputMap,
            out Dictionary<string, string> outputMap,
            out Dictionary<string, string> renamingMap)
        {
            outputMap = new Dictionary<string, string>();
            renamingMap = new Dictionary<string, string>();
            var inputVariableSet = new HashSet<string>();

            foreach (var node in nodes)
            {
                // Collects all inputs of selected nodes
                foreach (var inport in node.InPorts)
                {
                    if (inport.Connectors.Count == 0)
                        continue;

                    var startPort = inport.Connectors[0].Start;
                    var inputNode = startPort.Owner;
                    var portIndex = startPort.Index; 
                    var cbn = inputNode as CodeBlockNodeModel;

                    if (nodes.Contains(inputNode))
                    {
                        // internal node
                        if (cbn != null)
                        {
                            string inputVar = cbn.GetAstIdentifierForOutputIndex(portIndex).Value;
                            string originalVar = cbn.GetRawAstIdentifierForOutputIndex(portIndex).Value;

                            renamingMap[inputVar] = originalVar;
                            var key = String.Format("{0}%{1}", originalVar, cbn.GUID);
                            renamingMap[key] = inputVar;
                        }
                    }
                    else // extern node, so they should be added to inputMap
                    {
                        string inputVar = inputNode.GetAstIdentifierForOutputIndex(portIndex).Value;
                        inputVariableSet.Add(inputVar);
                    }
                }

                // Collect all outputs of selected nodes
                var thisCBN = node as CodeBlockNodeModel;
                if (thisCBN != null)
                {
                    for (int i = 0; i < thisCBN.OutPorts.Count; ++i)
                    {
                        var inputVar = thisCBN.GetAstIdentifierForOutputIndex(i).Value;
                        var originalVar = thisCBN.GetRawAstIdentifierForOutputIndex(i).Value;

                        outputMap[inputVar] = originalVar;
                        var key = String.Format("{0}%{1}", originalVar, thisCBN.GUID);
                        outputMap[key] = inputVar;
                    }
                }
                else
                {
                    for (int i = 0; i < node.OutPorts.Count; ++i)
                    {
                        var inputVar = node.GetAstIdentifierForOutputIndex(i).Value;
                        outputMap[inputVar] = string.Empty;
                    }
                }
            }

            inputMap = inputVariableSet.ToDictionary(v => v, _ => string.Empty);
        }

        /// <summary>
        /// Renumber variables used in astNode.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="numberingMap"></param>
        /// <param name="variableMap"></param>
        private static void VariableNumbering(
            AssociativeNode astNode, 
            NodeModel node,
            Dictionary<string, NumberingState> numberingMap, 
            Dictionary<string, string> renamingMap,
            Dictionary<string, string> inputMap,
            Dictionary<string, string> outputMap,
            HashSet<string> mappedVariables)
        {
            Action<IdentifierNode> func = n =>
            {
                var ident = n.Value;

                // This ident is from other node's output port, so it should be
                // renamed later on VarialbeMapping()
                if (renamingMap.ContainsKey(ident) || inputMap.ContainsKey(ident))
                    return;

                NumberingState ns; 
                if (numberingMap.TryGetValue(ident, out ns))
                {
                    // ident already defined somewhere else. So we continue to 
                    // bump its ID until numbered variable won't conflict with
                    // all existing variables. 
                    if (ns.IsNewSession)
                    {
                        ns.BumpID();
                        while (mappedVariables.Contains(ns.NumberedVariable))
                            ns.BumpID();

                        ns.IsNewSession = false;
                    }
                    // ident already defined somewhere else, but we already
                    // renumber this variable, so continue to use re-numbered
                    // one.
                }
                else
                {
                    // It is a new variable. But we need to check if some other
                    // variables are renamed to this one because of renumbering.
                    // If there is confliction, continue to bump its ID.
                    ns = new NumberingState(ident);
                    numberingMap[ident] = ns;

                    while (mappedVariables.Contains(ns.NumberedVariable))
                        ns.BumpID();
                }

                if (ns.ID != 0)
                {
                    n.Name = n.Value = ns.NumberedVariable;

                    // If this variable is numbered, and it is also output to
                    // other node, we should remap the output variable to
                    // this re-numbered variable.
                    string name = string.Format("{0}%{1}", ident, node.GUID);
                    if (renamingMap.ContainsKey(name))
                    {
                        var mappedName = renamingMap[name];
                        renamingMap[mappedName] = ns.NumberedVariable;
                    }

                    // Record in output map.
                    if (outputMap.ContainsKey(name))
                    {
                        var mappedName = outputMap[name];
                        outputMap[mappedName] = ns.NumberedVariable;
                    }
                }

                mappedVariables.Add(ns.NumberedVariable);
            };

            IdentifierVisitor visitor = new IdentifierVisitor(func);
            astNode.Accept(visitor);
        }

        /// <summary>
        /// Remap variables.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="renamingMap"></param>
        private static void VariableRemapping(
            AssociativeNode astNode, 
            Dictionary<string, string> renamingMap)
        {
            Action<IdentifierNode> func = n =>
                {
                    string newIdent;
                    if (renamingMap.TryGetValue(n.Value, out newIdent))
                        n.Value = n.Name = newIdent;
                };

            IdentifierVisitor visitor = new IdentifierVisitor(func);
            astNode.Accept(visitor);
        }

        /// <summary>
        /// Map variable to shorter name.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="shortNameMap"></param>
        /// <param name="nameGenerator"></param>
        private static void ShortNameMapping(
            AssociativeNode astNode,
            Dictionary<string, string> shortNameMap,
            ShortNameGenerator nameGenerator,
            HashSet<string> mappedVariables)
        {
            Action<IdentifierNode> func = n =>
            {
                string shortName;
                if (shortNameMap.TryGetValue(n.Value, out shortName))
                {
                    if (shortName == string.Empty)
                    {
                        shortName = nameGenerator.GetNextName();
                        while (mappedVariables.Contains(shortName))
                            shortName = nameGenerator.GetNextName();

                        shortNameMap[n.Value] = shortName;
                    }
                    n.Value = n.Name = shortName; 
                }
            };

            IdentifierVisitor visitor = new IdentifierVisitor(func);
            astNode.Accept(visitor);
        }

        /// <summary>
        /// Remove temporary assignment from the result. That is, removing 
        /// assignment "t1 = x" where t1 is a temporary variable. This kind
        /// of assginment can be safely removed, but now all nodes that 
        /// connect to "t1" should re-connect to "x". For example, "a = t1" 
        /// now should be updated to "a = x".
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private static NodeToCodeResult RemoveTemporaryAssignments(NodeToCodeResult result)
        {
            var tempVariables = result.OutputMap.Where(p => p.Key.StartsWith(Constants.kTempVarForNonAssignment))
                                                .Select(p => p.Value);
            var tempVariableSet = new HashSet<string>(tempVariables);
            
            // Check if the LHS of a binary expression is temorary variable 
            // where the RHS isn't
            Func<AssociativeNode, bool> isTempAssignment = x =>
            {
                var expr = x as BinaryExpressionNode;
                if (expr == null)
                    return false;

                var lhs = expr.LeftNode as IdentifierNode;
                var rhs = expr.RightNode as IdentifierNode;
                if (lhs == null || rhs == null)
                    return false;

                return tempVariableSet.Contains(lhs.Value) &&
                       !tempVariableSet.Contains(rhs.Value);
            };

            // Get all temporary assignments. 
            var tmpExprs = result.AstNodes
                                 .Where(isTempAssignment)
                                 .Select(n => n as BinaryExpressionNode)
                                 .Select(e => new {lhs = (e.LeftNode as IdentifierNode).Value, 
                                                   rhs = (e.RightNode as IdentifierNode).Value});
            var newAsts = result.AstNodes.Where(n => !isTempAssignment(n));

            foreach (var pair in tmpExprs)
            {
                // Update the map.
                var keys = result.OutputMap.Keys.ToList();
                for (int i = 0; i < keys.Count; ++i)
                {
                    var value = result.OutputMap[keys[i]];
                    if (value.Equals(pair.lhs))
                        result.OutputMap[keys[i]] = pair.rhs;
                }

                // Update ASTs.
                IdentifierReplacer replacer = new IdentifierReplacer(pair.lhs, pair.rhs);
                foreach (var ast in newAsts)
                {
                    ast.Accept(replacer);
                }
            }

            return new NodeToCodeResult(newAsts, result.InputMap, result.OutputMap);
        }

        /// <summary>
        /// Temporary propagation for temporary assignment like "t1 = 1", 
        /// propagate "1" to all expressions that use "t1" and the assignment
        /// node will be removed from the final result.  
        /// </summary>
        /// <param name="result"></param>
        /// <param name="outputVariables"></param>
        /// <returns></returns>
        public static NodeToCodeResult ConstantPropagationForTemp(NodeToCodeResult result, IEnumerable<string> outputVariables)
        {
            var tempVariables = result.OutputMap.Where(p => p.Key.StartsWith(Constants.kTempVarForNonAssignment))
                                                .Select(p => p.Value);

            // Check if the LHS of a binary expression is temorary variable 
            // where the RHS is constant value
            Func<AssociativeNode, bool> isConstantTempAssignment = x =>
            {
                var expr = x as BinaryExpressionNode;
                if (expr == null)
                    return false;

                var lhs = expr.LeftNode as IdentifierNode;
                return lhs != null 
                       && tempVariables.Contains(lhs.Value) 
                       && !outputVariables.Contains(lhs.Value)
                       && expr.RightNode.IsLiteral ; 
            };

            // Get all temporary assignments. 
            var tmpExprs = result.AstNodes
                                 .Where(isConstantTempAssignment)
                                 .Select(n => n as BinaryExpressionNode)
                                 .Select(e => new
                                 {
                                     lhs = (e.LeftNode as IdentifierNode).Value,
                                     rhs = e.RightNode
                                 });
            var nonTempExprs = result.AstNodes.Where(n => !isConstantTempAssignment(n));
            var finalNodes = new List<AssociativeNode>();

            var newAsts = new List<AssociativeNode>();
            // Update ASTs.
            foreach (var ast in nonTempExprs)
            {
                AssociativeNode astNode = ast;
                foreach (var pair in tmpExprs)
                {
                    ConstantIdentifierReplacer replacer = new ConstantIdentifierReplacer(pair.lhs, pair.rhs);
                    astNode = astNode.Accept(replacer);
                }
                newAsts.Add(astNode);
            }

            return new NodeToCodeResult(newAsts, result.InputMap, result.OutputMap);
        }

        /// <summary>
        /// Compile a bunch of node to AST. 
        /// </summary>
        /// <param name="astBuilder"></param>
        /// <param name="graph"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static NodeToCodeResult NodeToCode(
            AstBuilder astBuilder, 
            IEnumerable<NodeModel> graph,
            IEnumerable<NodeModel> nodes)
        {
            // The basic worflow is:
            //   1. Compile each node to get its cde in AST format
            // 
            //   2. Variable numbering to avoid confliction. For example, two 
            //      code block nodes both have assignment "y = x", we need to 
            //      rename "y" to "y1" and "y2" respectively.
            //
            //   3. Variable remapping. For example, code block node 
            //      "x = 1" connects to "a = b", where the second code block 
            //      node will have initialization "b = x_guid" where "x_guid"
            //      is because of variable mappining in the first code block 
            //      node. We should restore it to its original name.
            //   
            //      Note in step 2 we may rename "x" to "xn". 
            //
            //   4. Generate short name for long name variables. Typically they
            //      are from output ports from other nodes.
            //
            //   5. Do constant progation to optimize the generated code.
            #region Step 1 AST compilation

            var sortedGraph = AstBuilder.TopologicalSortForGraph(graph);
            var sortedNodes = sortedGraph.Where(nodes.Contains);

            var allAstNodes = astBuilder.CompileToAstNodes(sortedNodes, AstBuilder.CompilationContext.NodeToCode, false);

            #endregion

            #region Step 2 Varialbe numbering
            // In this step, we'll renumber all variables that going to be in 
            // the same code block node. That is, 
            // 
            //     "x = 1; y = x;"   and
            //     "x = 2; y = x;" 
            //
            // Will be renumbered to 
            //
            //    "x1 = 1; y1 = x1;" and
            //    "x2 = 2; y2 = x2;"

            // Map from mapped variable to its original name. These variables 
            // are from code block nodes that in the selection.
            Dictionary<string, string> renamingMap = null; ;

            // Input variable to renamed input variable map
            Dictionary<string, string> inputMap = null;

            // Output variable to renamed output variable map
            Dictionary<string, string> outputMap = null;

            // Collect all inputs/outputs/candidate renaming variables
            GetInputOutputMap(nodes, out inputMap, out outputMap, out renamingMap);

            // Vairable numbering map. The Tuple value indicates its number
            // sequence and if for the new UI node.
            var numberingMap = new Dictionary<string, NumberingState>();

            var mappedVariables = new HashSet<string>();
            foreach (var t in allAstNodes)
            {
                // Reset variable numbering map
                foreach (var p in numberingMap)
                    p.Value.IsNewSession = true;

                foreach (var astNode in t.Item2)
                    VariableNumbering(astNode, t.Item1, numberingMap, renamingMap, inputMap, outputMap, mappedVariables); 
            }

            renamingMap = renamingMap.Where(p => !p.Key.Contains("%"))
                                     .ToDictionary(p => p.Key, p => p.Value);

            outputMap = outputMap.Where(p => !p.Key.Contains("%"))
                                 .ToDictionary(p => p.Key, p => p.Value);
            #endregion

            #region Step 3 Variable remapping
            foreach (var ts in allAstNodes)
            {
                foreach (var astNode in ts.Item2)
                    VariableRemapping(astNode, renamingMap);
            }
            #endregion

            #region Step 4 Generate short name
            var nameGenerator = new ShortNameGenerator();

            // temporary variables are double mapped.
            foreach (var key in inputMap.Keys.ToList())
            {
               if (key.StartsWith(Constants.kTempVarForNonAssignment) &&
                   inputMap[key].StartsWith(Constants.kTempVarForNonAssignment))
               {
                   string shortName = nameGenerator.GetNextName();
                   while (mappedVariables.Contains(shortName))
                       shortName = nameGenerator.GetNextName();

                   var tempVar = inputMap[key];
                   inputMap[key] = shortName;
                   inputMap[tempVar] = shortName;

                   mappedVariables.Add(shortName);
               }
            }

            foreach (var key in outputMap.Keys.ToList())
            {
               if (key.StartsWith(Constants.kTempVarForNonAssignment) &&
                   outputMap[key].StartsWith(Constants.kTempVarForNonAssignment))
               {
                   string shortName = nameGenerator.GetNextName();
                   while (mappedVariables.Contains(shortName))
                       shortName = nameGenerator.GetNextName();

                   var tempVar = outputMap[key];
                   outputMap[key] = shortName;
                   outputMap[tempVar] = shortName;

                   mappedVariables.Add(shortName);
               }
            }
 
            foreach (var ts in allAstNodes)
            {
                foreach (var astNode in ts.Item2)
                {
                    ShortNameMapping(astNode, inputMap, nameGenerator, mappedVariables);
                    ShortNameMapping(astNode, outputMap, nameGenerator, mappedVariables);
                }
            }
            #endregion

            var result = new NodeToCodeResult(allAstNodes.SelectMany(x => x.Item2), inputMap, outputMap);
            result = RemoveTemporaryAssignments(result);
            return result;
        }
    }
}
