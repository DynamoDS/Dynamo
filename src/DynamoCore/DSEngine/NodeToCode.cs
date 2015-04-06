using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.DSEngine
{
    public class NodeToCodeResult
    {
        public IEnumerable<AssociativeNode> AstNodes { get; private set; }
        public Dictionary<string, string> InputMap { get; private set; }
        public Dictionary<string, string> OutputMap { get; private set; }

        public NodeToCodeResult(IEnumerable<AssociativeNode> astNodes, 
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
        private class ShortNameGenerator
        {
            private int counter = 0;

            public string GetNextName()
            {
                counter++;
                return AstBuilder.StringConstants.ShortVarPrefix + counter.ToString();
            }
        }

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

        private static HashSet<string> GetInputs(IEnumerable<NodeModel> nodes)
        {
            HashSet<string> variableSet = new HashSet<string>();

            foreach (var node in nodes)
            {
                if (node is CodeBlockNodeModel)
                {
                    var tempVars = (node as CodeBlockNodeModel).TempVariables;
                    variableSet.UnionWith(tempVars);
                }

                string thisVar = node.AstIdentifierForPreview.ToString();
                variableSet.Add(thisVar);

                foreach (var inport in node.InPorts)
                {
                    if (inport.Connectors.Count == 0)
                        continue;

                    var inputNode = inport.Connectors[0].Start.Owner;
                    if (nodes.Contains(inputNode))
                        continue;

                    if (inputNode is CodeBlockNodeModel)
                    {
                        var cbn = inputNode as CodeBlockNodeModel;
                        int portIndex = cbn.OutPorts.IndexOf(inport.Connectors[0].Start);
                        string inputVar = cbn.GetAstIdentifierForOutputIndex(portIndex).Value;
                        variableSet.Add(inputVar);
                    }
                    else
                    {
                        string inputVar = inputNode.AstIdentifierForPreview.ToString();
                        variableSet.Add(inputVar);
                    }
                }
            }

            return variableSet;
        }

        private static Dictionary<string, string> GetVariableRenamingMap(IEnumerable<string> variables)
        {
            return variables.ToDictionary(v => v, _ => string.Empty);
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
            Dictionary<string, Tuple<int, bool>> numberingMap, 
            Dictionary<string, string> renamingMap,
            Dictionary<string, string> inputMap,
            Dictionary<string, string> outputMap)
        {
            Action<IdentifierNode> func = n =>
            {
                var ident = n.Value;

                // This ident is from other node's output port, so it should be
                // renamed later on VarialbeMapping()
                if (renamingMap.ContainsKey(ident) || inputMap.ContainsKey(ident))
                    return;

                if (!numberingMap.ContainsKey(ident))
                {
                    numberingMap[ident] = Tuple.Create(0, true);
                }
                var t = numberingMap[ident];
                if (!t.Item2)
                {
                    numberingMap[ident] = Tuple.Create(t.Item1 + 1, true);
                    t = numberingMap[ident];
                }

                if (t.Item1 != 0)
                {
                    var newIdent = ident + t.Item1.ToString();
                    n.Name = n.Value = newIdent;

                    // If this variable is numbered, and it is also output to
                    // other node, we should remap the output variable to
                    // this re-numbered variable.
                    string name = string.Format("{0}%{1}", ident, node.GUID);
                    if (renamingMap.ContainsKey(name))
                    {
                        var mappedName = renamingMap[name];
                        renamingMap[mappedName] = newIdent;
                    }

                    // Record in output map.
                    if (outputMap.ContainsKey(name))
                    {
                        var mappedName = outputMap[name];
                        outputMap[mappedName] = newIdent;
                    }
                }
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
                    var ident = n.Value;
                    if (renamingMap.ContainsKey(ident))
                    {
                        n.Value = n.Name = renamingMap[ident];
                    }
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
            ShortNameGenerator nameGenerator)
        {
            Action<IdentifierNode> func = n =>
            {
                var ident = n.Value;
                if (shortNameMap.ContainsKey(ident))
                {
                    var shortName = shortNameMap[ident];
                    if (shortName == string.Empty)
                    {
                        shortName = nameGenerator.GetNextName();
                        shortNameMap[ident] = shortName;
                    }
                    n.Value = n.Name = shortNameMap[ident];
                }
            };

            IdentifierVisitor.Visit(astNode, func);
        }

        /// <summary>
        /// Compile a bunch of NodeModel to code which is in AST format. 
        /// </summary>
        /// <param name="astBuilder"></param>
        /// <param name="nodes"></param>
        /// <param name="verboseLogging"></param>
        /// <returns></returns>
        public static NodeToCodeResult NodeToCode(
            AstBuilder astBuilder, 
            IEnumerable<NodeModel> nodes, 
            bool verboseLogging)
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
            var allAstNodes = astBuilder.CompileToAstNodesForNodeToCode(nodes, false, verboseLogging);
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
            var remappingMap = new Dictionary<string, string>();
            var outputMap = new Dictionary<string, string>();

            foreach (var node in nodes)
            {
                // Gather all input from code block node that used internally.
                foreach (var inport in node.InPorts)
                {
                    if (inport.Connectors.Count == 0)
                        continue;

                    var inputNode = inport.Connectors[0].Start.Owner;
                    if (!nodes.Contains(inputNode))
                        continue;

                    var cbn = inputNode as CodeBlockNodeModel;
                    if (cbn == null)
                        continue;

                    int portIndex = inport.Connectors[0].Start.Index;
                    string inputVar = cbn.GetAstIdentifierForOutputIndex(portIndex).Value;
                    string originalVar = cbn.GetRawAstIdentifierForOutputIndex(portIndex).Value;

                    remappingMap[inputVar] = originalVar;
                    var key = String.Format("{0}%{1}", originalVar, cbn.GUID);
                    remappingMap[key] = inputVar;
                }

                if (node is CodeBlockNodeModel)
                {
                    var cbn = node as CodeBlockNodeModel;
                    for (int i = 0; i < cbn.OutPorts.Count(); ++i)
                    {
                        string inputVar = cbn.GetAstIdentifierForOutputIndex(i).Value;
                        string originalVar = cbn.GetRawAstIdentifierForOutputIndex(i).Value;

                        outputMap[inputVar] = originalVar;
                        var key = String.Format("{0}%{1}", originalVar, cbn.GUID);
                        outputMap[key] = inputVar;
                    }
                }
                else
                {
                    for (int i = 0; i < node.OutPorts.Count(); ++i)
                    {
                        string inputVar = node.GetAstIdentifierForOutputIndex(i).Value;
                        outputMap[inputVar] = string.Empty;
                    }
                }
            }

            // Vairable numbering map. The Tuple value indicates its number
            // sequence and if for the new UI node.
            var numberingMap = new Dictionary<string, Tuple<int, bool>>();

            // Long names from extern nodes
            var longNames = GetInputs(nodes);
            var inputMap = longNames.ToDictionary(v => v, _ => string.Empty);

            foreach (var t in allAstNodes)
            {
                // Reset variable numbering map
                numberingMap = numberingMap.ToDictionary(
                    p => p.Key, 
                    p => Tuple.Create(p.Value.Item1, false));

                foreach (var astNode in t.Item2)
                    VariableNumbering(astNode, t.Item1, numberingMap, remappingMap, inputMap, outputMap); 
            }
            #endregion

            #region Step 3 Variable remapping
            foreach (var ts in allAstNodes)
            {
                foreach (var astNode in ts.Item2)
                    VariableRemapping(astNode, remappingMap); 
            }
            #endregion

            #region Step 4 Generate short name
            var nameGenerator = new ShortNameGenerator();
            foreach (var ts in allAstNodes)
            {
                foreach (var astNode in ts.Item2)
                    ShortNameMapping(astNode, inputMap, nameGenerator);
            }
            #endregion

            return new NodeToCodeResult(allAstNodes.SelectMany(x => x.Item2), inputMap, outputMap);
        }
    }
}
