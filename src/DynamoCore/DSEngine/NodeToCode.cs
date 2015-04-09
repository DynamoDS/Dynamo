using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

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
        private class IdentifierVisitor
        {
            public static void Visit(AssociativeNode astNode, Action<IdentifierNode> func)
            {
                if (astNode is IdentifierNode)
                {
                    var identNode = astNode as IdentifierNode;
                    func(identNode);
                    Visit(identNode.ArrayDimensions, func);
                }
                else if (astNode is IdentifierListNode)
                {
                    var node = astNode as IdentifierListNode;
                    Visit(node.LeftNode, func);
                    Visit(node.RightNode, func);
                }
                else if (astNode is FunctionCallNode)
                {
                    var node = astNode as FunctionCallNode;
                    for (int i = 0; i < node.FormalArguments.Count; ++i)
                    {
                        Visit(node.FormalArguments[i], func);
                    }
                    Visit(node.ArrayDimensions, func);
                }
                else if (astNode is ArrayNode)
                {
                    var node = astNode as ArrayNode;
                    Visit(node.Expr, func);
                    Visit(node.Type, func);
                }
                else if (astNode is ExprListNode)
                {
                    var node = astNode as ExprListNode;
                    for (int i = 0; i < node.list.Count; ++i)
                    {
                        Visit(node.list[i], func);
                    }
                    Visit(node.ArrayDimensions, func);
                }
                else if (astNode is FunctionDotCallNode)
                {
                    var node = astNode as FunctionDotCallNode;
                }
                else if (astNode is InlineConditionalNode)
                {
                    var node = astNode as InlineConditionalNode;
                    Visit(node.ConditionExpression, func);
                    Visit(node.TrueExpression, func);
                    Visit(node.FalseExpression, func);
                }
                else if (astNode is RangeExprNode)
                {
                    var node = astNode as RangeExprNode;
                    Visit(node.FromNode, func);
                    Visit(node.ToNode, func);
                    Visit(node.StepNode, func);
                    Visit(node.ArrayDimensions, func);
                }
                else if (astNode is BinaryExpressionNode)
                {
                    var node = astNode as BinaryExpressionNode;
                    Visit(node.LeftNode, func);
                    Visit(node.RightNode, func);
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
            public int ID { get; set; }

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
                    for (int i = 0; i < thisCBN.OutPorts.Count(); ++i)
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
                    for (int i = 0; i < node.OutPorts.Count(); ++i)
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

            IdentifierVisitor.Visit(astNode, func);
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

            IdentifierVisitor.Visit(astNode, func);
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

            IdentifierVisitor.Visit(astNode, func);
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

            var allAstNodes = astBuilder.CompileToAstNodes(sortedNodes, AstBuilder.CompilationContext.ForNodeToCode, false);

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

            return new NodeToCodeResult(allAstNodes.SelectMany(x => x.Item2), inputMap, outputMap);
        }
    }
}
