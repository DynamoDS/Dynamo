using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.DSEngine
{
    public class Nodes2CodeUtils
    {
        private class VariableShortNameGenerator
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
                    Visit(node.Function, func);
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

        private static HashSet<string> CollectIntermediateVarialbeNames(IEnumerable<NodeModel> nodes)
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
                        string inputVar = cbn.GetRawAstIdentifierForOutputIndex(portIndex).Value;
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

        private static void MapIdentifier(AssociativeNode astNode, Dictionary<string, string> namingMap, VariableShortNameGenerator nameGenerator)
        {
            if (astNode is IdentifierNode)
            {
                var identNode = astNode as IdentifierNode;
                var ident = identNode.Value;
                if (namingMap.ContainsKey(ident))
                {
                    // hasn't been initialized yet
                    if (namingMap[ident] == string.Empty)
                    {
                        namingMap[ident] = nameGenerator.GetNextName();
                    }
                    identNode.Name = identNode.Value = namingMap[ident];
                }

                MapIdentifier(identNode.ArrayDimensions, namingMap, nameGenerator);
            }
            else if (astNode is IdentifierListNode)
            {
                var node = astNode as IdentifierListNode;
                MapIdentifier(node.LeftNode, namingMap, nameGenerator);
                MapIdentifier(node.RightNode, namingMap, nameGenerator);
            }
            else if (astNode is FunctionCallNode)
            {
                var node = astNode as FunctionCallNode;
                MapIdentifier(node.Function, namingMap, nameGenerator);
                for (int i = 0; i < node.FormalArguments.Count; ++i)
                {
                    MapIdentifier(node.FormalArguments[i], namingMap, nameGenerator);
                }
                MapIdentifier(node.ArrayDimensions, namingMap, nameGenerator);
            }
            else if (astNode is ArrayNode)
            {
                var node = astNode as ArrayNode;
                MapIdentifier(node.Expr, namingMap, nameGenerator);
            }
            else if (astNode is ExprListNode)
            {
                var node = astNode as ExprListNode;
                for (int i = 0; i < node.list.Count; ++i)
                {
                    MapIdentifier(node.list[i], namingMap, nameGenerator);
                }
                MapIdentifier(node.ArrayDimensions, namingMap, nameGenerator);
            }
            else if (astNode is FunctionDotCallNode)
            {
                var node = astNode as FunctionDotCallNode;
            }
            else if (astNode is InlineConditionalNode)
            {
                var node = astNode as InlineConditionalNode;
                MapIdentifier(node.ConditionExpression, namingMap, nameGenerator);
                MapIdentifier(node.TrueExpression, namingMap, nameGenerator);
                MapIdentifier(node.FalseExpression, namingMap, nameGenerator);
            }
            else if (astNode is RangeExprNode)
            {
                var node = astNode as RangeExprNode;
                MapIdentifier(node.FromNode, namingMap, nameGenerator);
                MapIdentifier(node.ToNode, namingMap, nameGenerator);
                MapIdentifier(node.StepNode, namingMap, nameGenerator);
                MapIdentifier(node.ArrayDimensions, namingMap, nameGenerator);
            }
            else if (astNode is BinaryExpressionNode)
            {
                var node = astNode as BinaryExpressionNode;
                MapIdentifier(node.LeftNode, namingMap, nameGenerator);
                MapIdentifier(node.RightNode, namingMap, nameGenerator);
            }
        }

        /// <summary>
        /// Renumber variables used in astNode.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="variableNumberingMap"></param>
        /// <param name="variableMap"></param>
        private static void VariableNumbering(
            AssociativeNode astNode, 
            Dictionary<string, Tuple<int, bool>> variableNumberingMap, 
            Dictionary<string, string> variableMap)
        {
            Action<IdentifierNode> func = node =>
                {
                    var ident = node.Value;
                    if (!variableNumberingMap.ContainsKey(ident))
                    {
                        variableNumberingMap[ident] = Tuple.Create(0, true);
                    }
                    var tuple = variableNumberingMap[ident];
                    if (!tuple.Item2)
                    {
                        variableNumberingMap[ident] = Tuple.Create(tuple.Item1 + 1, true);
                        tuple = variableNumberingMap[ident];
                    }

                    if (tuple.Item1 != 0)
                    {
                        var newIdent = ident + tuple.Item1.ToString();
                        node.Name = node.Value = newIdent;
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
        public static IEnumerable<AssociativeNode> Node2Code(
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
            //   4. Do constant progation to optimize the generated code.
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

            // Map from mapped variable to its original name. Typcically
            // these variables are from code block node.
            var renamingMap = new Dictionary<string, string>();

            // Vairable numbering map. The Tuple value indicates its number
            // sequence and if for the new UI node.
            var numberingMap = new Dictionary<string, Tuple<int, bool>>();

            foreach (var astNodes in allAstNodes)
            {
                // Reset variable numbering map
                numberingMap = numberingMap.ToDictionary(
                    p => p.Key, 
                    p => Tuple.Create(p.Value.Item1, false));

                foreach (var astNode in astNodes)
                    VariableNumbering(astNode, numberingMap, renamingMap); 
            }
            #endregion

            // var nameGenerator = new VariableShortNameGenerator();
            // var intermediateVariables = CollectIntermediateVarialbeNames(nodes);
            // var renamingMap = GetVariableRenamingMap(intermediateVariables);

            return allAstNodes.SelectMany(x => x);
        }
    }
}
