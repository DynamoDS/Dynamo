using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using Dynamo.Utilities;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Mirror;
using Dynamo.Core;

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

    /// <summary>
    /// An undo helper class that is used in node-to-code conversion. It helps to 
    /// avoid recording redundant user actions (e.g. deletion right after creation 
    /// will not be recorded). This typically happens when there are multiple code 
    /// block nodes being created as part of node-to-code conversion, during which 
    /// some connectors will be created and later on deleted all within the same 
    /// conversion process.
    /// </summary>
    public class NodeToCodeUndoHelper
    {
        private List<Tuple<ModelBase, UndoRedoRecorder.UserAction>> recordedActions;

        public NodeToCodeUndoHelper()
        {
            recordedActions = new List<Tuple<ModelBase, UndoRedoRecorder.UserAction>>();
        }

        /// <summary>
        /// Add a creation action.
        /// </summary>
        /// <param name="model"></param>
        public void RecordCreation(ModelBase model)
        {
            recordedActions.Add(Tuple.Create(model, UndoRedoRecorder.UserAction.Creation));
        }

        /// <summary>
        /// Add a deletion action. If a creation action for the same model has
        /// been added, that creation action will be removed and this deletion
        /// action won't be added.
        /// </summary>
        /// <param name="model"></param>
        public void RecordDeletion(ModelBase model)
        {
            for (int i = 0; i < recordedActions.Count; ++i)
            {
                var recordedAction = recordedActions[i];
                if (recordedAction.Item1.GUID.Equals(model.GUID) &&
                    recordedAction.Item2 == UndoRedoRecorder.UserAction.Creation)
                {
                    recordedActions.RemoveAt(i);
                    return;
                }
            }
            recordedActions.Add(Tuple.Create(model, UndoRedoRecorder.UserAction.Deletion));
        }

        /// <summary>
        /// Record all actions in recorder.
        /// </summary>
        /// <param name="recorder"></param>
        public void ApplyActions(UndoRedoRecorder recorder)
        {
            using (recorder.BeginActionGroup())
            {
                foreach (var item in recordedActions)
                {
                    var model = item.Item1;
                    var action = item.Item2;

                    if (action == UndoRedoRecorder.UserAction.Creation)
                        recorder.RecordCreationForUndo(model);
                    else if (action == UndoRedoRecorder.UserAction.Deletion)
                        recorder.RecordDeletionForUndo(model);
                }
            }
        }

        /// <summary>
        /// Return the count of recorded actions.
        /// </summary>
        /// <returns></returns>
        public int ActionCount()
        {
            return recordedActions.Count();
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
            private ProtoCore.Core core;

            public IdentifierVisitor(Action<IdentifierNode> func, ProtoCore.Core core)
            {
                identFunc = func;
                this.core = core;
            }

            public override void VisitIdentifierNode(IdentifierNode node)
            {
                identFunc(node);
                if (node.ArrayDimensions != null)
                    node.ArrayDimensions.Accept(this);
            }

            public override void VisitIdentifierListNode(IdentifierListNode node)
            {
                if ((node.LeftNode is IdentifierNode ||
                    node.LeftNode is IdentifierListNode) &&
                    node.RightNode is FunctionCallNode)
                {
                    var lhs = node.LeftNode.ToString();
                    if (core.ClassTable.IndexOf(lhs) < 0)
                    {
                        node.LeftNode.Accept(this);
                    }
                    node.RightNode.Accept(this);
                    return;
                }
                base.VisitIdentifierListNode(node);
            }
        }

        /// <summary>
        /// Replace an identifier with a constant value.
        /// </summary>
        private class IdentifierReplacer : AstReplacer 
        {
            private string variableName;
            private AssociativeNode constantValue;

            public IdentifierReplacer(string variable, AssociativeNode newValue)
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
                if (node.Optr != Operator.assign)
                {
                    var newLeftNode = node.LeftNode.Accept(this);
                    if (node.LeftNode != newLeftNode)
                        node.LeftNode = newLeftNode;
                }

                var newRightNode = node.RightNode.Accept(this);
                if (newRightNode != node.RightNode)
                    node.RightNode = newRightNode;

                return node;
           }
        }

        /// <summary>
        /// Replace a fully qualified function call with short name. 
        /// </summary>
        private class UnqualifiedNameReplacer : AstReplacer
        {
            private ProtoCore.Core core;

            public UnqualifiedNameReplacer(ProtoCore.Core core)
            {
                this.core = core;
            }

            public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
            {
                if ((node.LeftNode is IdentifierNode || node.LeftNode is IdentifierListNode)
                    && node.RightNode is FunctionCallNode)
                {
                    var fullName = node.LeftNode.ToString();
                    if (core.ClassTable.IndexOf(fullName) >= 0 && fullName.Contains("."))
                    {
                        node.LeftNode = AstFactory.BuildIdentifier(fullName.Split('.').Last());
                    }

                    node.RightNode.Accept(this);
                }

                return node;
            }
        }

        /// <summary>
        /// Check if an identifier is used.
        /// </summary>
        private class IdentifierFinder : AssociativeAstVisitor<bool>
        {
            private string identifier = string.Empty;
            private bool forDefinition = false;

            public IdentifierFinder(string identifier, bool isForDefinition)
            {
                this.identifier = identifier;
                this.forDefinition = isForDefinition;
            }

            public override bool VisitIdentifierNode(IdentifierNode node)
            {
                if (node.Value.Equals(identifier))
                    return true;

                if (forDefinition)
                    return false;

                return node.ArrayDimensions != null ? node.ArrayDimensions.Accept(this) : false;
            }

            public override bool VisitGroupExpressionNode(GroupExpressionNode node)
            {
                if (forDefinition)
                    return false;

                return node.Expression.Accept(this);
            }

            public override bool VisitIdentifierListNode(IdentifierListNode node)
            {
                if (forDefinition)
                    return false;

                return node.LeftNode.Accept(this) || node.RightNode.Accept(this);
            }

            public override bool VisitFunctionCallNode(FunctionCallNode node)
            {
                if (forDefinition)
                    return false;

                if (node.FormalArguments.Any(f => f.Accept(this)))
                    return true;

                return node.ArrayDimensions != null ? node.ArrayDimensions.Accept(this) : false;
            }

            public override bool VisitInlineConditionalNode(InlineConditionalNode node)
            {
                if (forDefinition)
                    return false;

                return node.ConditionExpression.Accept(this) ||
                       node.TrueExpression.Accept(this) ||
                       node.FalseExpression.Accept(this);
            }

            public override bool VisitBinaryExpressionNode(BinaryExpressionNode node)
            {
                if (forDefinition && node.Optr == Operator.assign)
                    return node.LeftNode.Accept(this);

                return node.Optr == Operator.assign 
                    ? node.RightNode.Accept(this)
                    : node.LeftNode.Accept(this) || node.RightNode.Accept(this);
            }

            public override bool VisitUnaryExpressionNode(UnaryExpressionNode node)
            {
                if (forDefinition)
                    return false;

                return node.Expression.Accept(this);
            }

            public override bool VisitRangeExprNode(RangeExprNode node)
            {
                if (forDefinition)
                    return false;

                if (node.FromNode.Accept(this) || node.ToNode.Accept(this))
                    return true;

                return node.StepNode != null ? node.StepNode.Accept(this) : false;
            }

            public override bool VisitExprListNode(ExprListNode node)
            {
                if (forDefinition)
                    return false;

                if (node.list.Any(e => e.Accept(this)))
                    return true;

                return node.ArrayDimensions != null ? node.ArrayDimensions.Accept(this) : false;
            }

            public override bool VisitArrayNode(ArrayNode node)
            {
                if (forDefinition)
                    return false;

                if (node.Expr.Accept(this))
                    return true;

                return node.Type != null ? node.Type.Accept(this) : false;
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
            var leaf = path.Last();
            var children = leaf.OutputNodes.Values.SelectMany(s => s).Select(t => t.Item2);
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

                // Find the index of the first node that is either unconvertible
                // or not in the selection.
                int k = nodes.FindIndex(n => !n.IsConvertible || !selection.Contains(n));
                if (k >= 0)
                {
                    // Then mark all nodes after this node as unreachable from 
                    // root node.
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
                var path = new List<NodeModel> { node };
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
            inputMap = new Dictionary<string, string>();
            outputMap = new Dictionary<string, string>();
            renamingMap = new Dictionary<string, string>();

            foreach (var node in nodes)
            {
                // Collects all inputs of selected nodes.
                foreach (var inport in node.InPorts)
                {
                    if (inport.Connectors.Count == 0)
                        continue;

                    var startPort = inport.Connectors.First().Start;
                    var inputNode = startPort.Owner;
                    var portIndex = startPort.Index;

                    if (!nodes.Contains(inputNode) || !(inputNode is CodeBlockNodeModel))
                    {
                        string inputVar = inputNode.GetAstIdentifierForOutputIndex(portIndex).Value;
                        inputMap[inputVar] = string.Empty;
                    }
                }

                // For all outputs from non code block node, simply put them 
                // in output map because they are unique. Otherwise, need to
                // save to renaming map because they would be renumbered.
                var thisCBN = node as CodeBlockNodeModel;
                if (thisCBN != null)
                {
                    for (int i = 0; i < thisCBN.OutPorts.Count; ++i)
                    {
                        var mappedInputVar = thisCBN.GetAstIdentifierForOutputIndex(i).Value;
                        var originalVar = thisCBN.GetRawAstIdentifierForOutputIndex(i).Value;

                        renamingMap[mappedInputVar] = originalVar;
                        var key = String.Format("{0}%{1}", originalVar, thisCBN.GUID);
                        renamingMap[key] = mappedInputVar;

                        outputMap[mappedInputVar] = originalVar;
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
        }

        /// <summary>
        /// Check if this identifier is a temporary variable that output from
        /// other code block node. 
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="renamingMap"></param>
        /// <returns></returns>
        private static bool IsTempVarFromCodeBlockNode(string ident,
            Dictionary<string, string> renamingMap)
        {
            if (!ident.StartsWith(Constants.kTempVarForNonAssignment))
                return false;

            string renamedVariable = string.Empty;
            // ident is: Constants.TempVarForNonAssignment_guid
            if (!renamingMap.TryGetValue(ident, out renamedVariable))
                return false;

            return renamedVariable.StartsWith(Constants.kTempVarForNonAssignment);
        }

        /// <summary>
        /// Renumber variables used in astNode.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="numberingMap"></param>
        /// <param name="variableMap"></param>
        private static void VariableNumbering(
            ProtoCore.Core core,
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

                // This ident is from external non-code block node's output 
                // port, or it is from a temporary variable in code block node,
                // it is not necessary to do renumbering because the name is 
                // unique.
                if (inputMap.ContainsKey(ident) || /*IsTempVarFromCodeBlockNode(ident, renamingMap)*/ renamingMap.ContainsKey(ident))
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
                    var numberedVar = ns.NumberedVariable;
                    n.Name = n.Value = numberedVar;

                    // If this variable is numbered, and it is also output to
                    // other node, we should remap the output variable to
                    // this re-numbered variable.
                    string name = string.Format("{0}%{1}", ident, node.GUID);
                    if (renamingMap.ContainsKey(name))
                    {
                        var mappedName = renamingMap[name];
                        renamingMap[mappedName] = numberedVar;

                        // Record in output map.
                        if (outputMap.ContainsKey(mappedName))
                            outputMap[mappedName] = numberedVar;
                    }
                }

                // If this one is not the variable that going to be renamed, then
                // add to mapped variable set
                if (!renamingMap.ContainsKey(ns.NumberedVariable))
                    mappedVariables.Add(ns.NumberedVariable);
            };

            IdentifierVisitor visitor = new IdentifierVisitor(func, core);
            astNode.Accept(visitor);
        }

        /// <summary>
        /// Remap variables.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="renamingMap"></param>
        private static void VariableRemapping(
            ProtoCore.Core core,
            AssociativeNode astNode, 
            Dictionary<string, string> renamingMap,
            Dictionary<string, string> outuptMap,
            HashSet<string> mappedVariable)
        {
            Action<IdentifierNode> func = n =>
                {
                    string newIdent;
                    if (renamingMap.TryGetValue(n.Value, out newIdent))
                    {
                        var ident = n.Value;
                        n.Value = n.Name = newIdent;
                        outuptMap[ident] = newIdent;
                        mappedVariable.Add(newIdent);
                    }
                };

            IdentifierVisitor visitor = new IdentifierVisitor(func, core);
            astNode.Accept(visitor);
        }

        /// <summary>
        /// Map variable to shorter name.
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="shortNameMap"></param>
        /// <param name="nameGenerator"></param>
        private static void ShortNameMapping(
            ProtoCore.Core core,
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

            IdentifierVisitor visitor = new IdentifierVisitor(func, core);
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
        public static NodeToCodeResult ConstantPropagationForTemp(NodeToCodeResult result, IEnumerable<string> outputVariables)
        {
            var tempVars = new HashSet<string>(
                result.OutputMap.Where(p => p.Key.StartsWith(Constants.kTempVarForNonAssignment))
                                .Select(p => p.Value));
            
            var nonTempAsts = new List<AssociativeNode>();
            var tempAssignments = new List<Tuple<IdentifierNode, AssociativeNode>>();

            foreach (var ast in result.AstNodes)
	        {
                var expr = ast as BinaryExpressionNode;
                if (expr == null || expr.Optr != Operator.assign)
                {
                    nonTempAsts.Add(ast);
                    continue;
                }
		
                var lhs = expr.LeftNode as IdentifierNode;
                var rhs = expr.RightNode;
                if (lhs == null 
                    || !tempVars.Contains(lhs.Value) 
                    || outputVariables.Contains(lhs.Value) 
                    || !(rhs.IsLiteral || rhs is IdentifierNode))
                {
                    nonTempAsts.Add(ast);
                    continue;
                }

                IdentifierFinder finder = new IdentifierFinder(lhs.Value, false);
                bool isReferenced = result.AstNodes.Any(n => n.Accept(finder));

                bool isRhsDefined = false;
                if (rhs is IdentifierNode)
                {
                    var defFinder = new IdentifierFinder((rhs as IdentifierNode).Value, true);
                    isRhsDefined = result.AstNodes.Any(n => n.Accept(defFinder));
                }

                if (isReferenced || isRhsDefined)
                    tempAssignments.Add(Tuple.Create(lhs, rhs));
                else
                    nonTempAsts.Add(ast);
	        }

            foreach (var pair in tempAssignments)
            {
                // Update the map.
                var keys = result.OutputMap.Keys.ToList();
                for (int i = 0; i < keys.Count; ++i)
                {
                    var value = result.OutputMap[keys[i]];
                    if (value.Equals(pair.Item1.Value))
                    {
                        var rhs = pair.Item2 as IdentifierNode;
                        if (rhs != null)
                            result.OutputMap[keys[i]] = rhs.Value; 
                    }
                }

                // Update ASTs.
                IdentifierReplacer replacer = new IdentifierReplacer(pair.Item1.Value, pair.Item2);
                foreach (var ast in nonTempAsts)
                {
                    ast.Accept(replacer);
                }
            }

            return new NodeToCodeResult(nonTempAsts, result.InputMap, result.OutputMap);
        }

        /// <summary>
        /// Replace fully qualified class name with unqualified name. 
        /// 
        /// For example, replace
        /// 
        ///     Autodesk.Geometry.Point.ByCoordinates(...)
        ///     
        /// with
        /// 
        ///     Point.ByCoordinates(...)
        /// </summary>
        /// <param name="asts">Input ASTs</param>
        public static void ReplaceWithUnqualifiedName(ProtoCore.Core core, IEnumerable<AssociativeNode> asts)
        {
            UnqualifiedNameReplacer replacer = new UnqualifiedNameReplacer(core);
            foreach (var ast in asts)
            {
                ast.Accept(replacer);
            }
        }

        /// <summary>
        /// Compile a set of nodes to ASTs. 
        ///
        /// Note: 
        /// 1. Nodes should be a clique with regarding to convertibility and 
        ///    selection state. That is, these nodes can be safely to be 
        ///    converted into a single code block node. It shouldn't have 
        ///    unconvertible or unselected node on any path (if there is) that 
        ///    connects any two of these nodes, otherwise there will be 
        ///    circular references between unconvertible/unselected node and
        ///    code block node.
        ///    
        ///    To split arbitary node set into cliques, use 
        ///    NodeToCodeUtils.GetCliques().
        ///
        /// 2. WorkspaceNodes are all nodes in current workspace. We need the
        ///    whole graph so that each to-be-converted node will have correct
        ///    order in the final code block node.
        /// </summary>
        /// <param name="core">Library core</param>
        /// <param name="astBuilder">Ast builder</param>
        /// <param name="workspaceNodes">The whole workspace nodes</param>
        /// <param name="nodes">Selected node that can be converted to a single code block node</param>
        /// <returns></returns>
        public static NodeToCodeResult NodeToCode(
            ProtoCore.Core core,
            AstBuilder astBuilder, 
            IEnumerable<NodeModel> workspaceNodes,
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

            var sortedGraph = AstBuilder.TopologicalSortForGraph(workspaceNodes);
            var sortedNodes = sortedGraph.Where(nodes.Contains);

            var allAstNodes = astBuilder.CompileToAstNodes(sortedNodes, AstBuilder.CompilationContext.NodeToCode, false);

            #endregion

            #region Step 2 Varialbe numbering
           
            // External inputs will be in input map
            // Internal non-cbn will be input map & output map
            // Internal cbn will be in renaming map and output map

            // Map from mapped variable to its original name. These variables 
            // are from code block nodes that in the selection.
            Dictionary<string, string> renamingMap = null; ;

            // Input variable to renamed input variable map
            Dictionary<string, string> inputMap = null;

            // Output variable to renamed output variable map
            Dictionary<string, string> outputMap = null;

            // Collect all inputs/outputs/candidate renaming variables
            GetInputOutputMap(nodes, out inputMap, out outputMap, out renamingMap);

            // Variable numbering map. Value field indicates current current
            // numbering value of the variable. For example, there are variables
            // t1, t2, ... tn and the ID of variable t's NumberingState is n.
            var numberingMap = new Dictionary<string, NumberingState>();

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
            var mappedVariables = new HashSet<string>();
            foreach (var t in allAstNodes)
            {
                // Reset variable numbering map
                foreach (var p in numberingMap)
                    p.Value.IsNewSession = true;

                foreach (var astNode in t.Item2)
                    VariableNumbering(core, astNode, t.Item1, numberingMap, renamingMap, inputMap, outputMap, mappedVariables); 
            }

            renamingMap = renamingMap.Where(p => !p.Key.Contains("%"))
                                     .ToDictionary(p => p.Key, p => p.Value);
            #endregion

            #region Step 3 Variable remapping
            foreach (var ts in allAstNodes)
            {
                foreach (var astNode in ts.Item2)
                    VariableRemapping(core, astNode, renamingMap, outputMap, mappedVariables);
            }
            #endregion

            #region Step 4 Generate short name
            var nameGenerator = new ShortNameGenerator();

            // temporary variables are double mapped.
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
                    ShortNameMapping(core, astNode, inputMap, nameGenerator, mappedVariables);
                    foreach (var kvp in inputMap)
                    {
                        if (kvp.Value != String.Empty && outputMap.ContainsKey(kvp.Key))
                            outputMap[kvp.Key] = kvp.Value;
                    }
                    ShortNameMapping(core, astNode, outputMap, nameGenerator, mappedVariables);
                }
            }
            #endregion

            var result = new NodeToCodeResult(allAstNodes.SelectMany(x => x.Item2), inputMap, outputMap);
            return result;
        }
    }
}
