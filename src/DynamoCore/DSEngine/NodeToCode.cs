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
            int shortVarCounter = 0;
            var nameMap = new Dictionary<string, string>();

            foreach (var longVariable in variables)
            {
                shortVarCounter++;
                string shortVariable = AstBuilder.StringConstants.ShortVarPrefix + shortVarCounter.ToString();
                nameMap[longVariable] = shortVariable;
            }
            return nameMap;
        }

        private static void MapIdentifier(AssociativeNode astNode, Dictionary<string, string> namingMap)
        {
            if (astNode is IdentifierNode)
            {
                var identNode = astNode as IdentifierNode;
                var ident = identNode.Value;
                if (namingMap.ContainsKey(ident))
                    identNode.Name = identNode.Value = namingMap[ident];

                MapIdentifier(identNode.ArrayDimensions, namingMap);
            }
            else if (astNode is IdentifierListNode)
            {
                var node = astNode as IdentifierListNode;
                MapIdentifier(node.LeftNode, namingMap);
                MapIdentifier(node.RightNode, namingMap);
            }
            else if (astNode is FunctionCallNode)
            {
                var node = astNode as FunctionCallNode;
                MapIdentifier(node.Function, namingMap);
                for (int i = 0; i < node.FormalArguments.Count; ++i)
                {
                    MapIdentifier(node.FormalArguments[i], namingMap);
                }
                MapIdentifier(node.ArrayDimensions, namingMap);
            }
            else if (astNode is ArrayNode)
            {
                var node = astNode as ArrayNode;
                MapIdentifier(node.Expr, namingMap);
            }
            else if (astNode is ExprListNode)
            {
                var node = astNode as ExprListNode;
                for (int i = 0; i < node.list.Count; ++i)
                {
                    MapIdentifier(node.list[i], namingMap);
                }
                MapIdentifier(node.ArrayDimensions, namingMap);
            }
            else if (astNode is FunctionDotCallNode)
            {
                var node = astNode as FunctionDotCallNode;
            }
            else if (astNode is InlineConditionalNode)
            {
                var node = astNode as InlineConditionalNode;
                MapIdentifier(node.ConditionExpression, namingMap);
                MapIdentifier(node.TrueExpression, namingMap);
                MapIdentifier(node.FalseExpression, namingMap);
            }
            else if (astNode is RangeExprNode)
            {
                var node = astNode as RangeExprNode;
                MapIdentifier(node.FromNode, namingMap);
                MapIdentifier(node.ToNode, namingMap);
                MapIdentifier(node.StepNode, namingMap);
                MapIdentifier(node.ArrayDimensions, namingMap);
            }
            else if (astNode is BinaryExpressionNode)
            {
                var node = astNode as BinaryExpressionNode;
                MapIdentifier(node.LeftNode, namingMap);
                MapIdentifier(node.RightNode, namingMap);
            }
        }

        public static IEnumerable<AssociativeNode> Node2Code(AstBuilder astBuilder, IEnumerable<NodeModel> nodes, bool verboseLogging)
        {
            var intermediateVariables = CollectIntermediateVarialbeNames(nodes);
            var renamingMap = GetVariableRenamingMap(intermediateVariables);
            var astNodes = astBuilder.CompileToAstNodes(nodes, false, verboseLogging, true);
            foreach (var astNode in astNodes)
            {
                MapIdentifier(astNode, renamingMap); 
            }
            return astNodes;
        }
    }
}
