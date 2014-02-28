//#define TESTING_COMPARE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using ProtoCore.Utils;

namespace GraphToDSCompiler
{
    public class GraphCompiler
    {
        AST graph = new AST();
        private ProtoCore.Core core = null;
        public GraphCompilationStatus gcs = new GraphCompilationStatus();
        static uint tguid = 20000;

        public List<uint> ModifiedStmtGuidList { get; private set; }
        public Dictionary<string, uint> mapModifiedName { get; private set; }
        public List<string> RemovedNodeNameList { get; private set; }
        public Dictionary<string, bool> ExecutionFlagList { get; private set; }
        public List<string> UndefinedNameList { get; set; }

        /*Tron*/
        public List<uint> nodeToCodeUIDs = new List<uint>();
        /*Tron*/

        public AST Graph
        {
            get
            {
                return graph;
            }
        }
        
        public Dictionary<uint, Dictionary<int, uint>> codeBlockUIDMap = new Dictionary<uint, Dictionary<int, uint>>();

        AST statementList = new AST();




        /// <summary>
        /// Emits the DS code given the list of ast nodes
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        public string Emit(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList)
        {
            // Emit the DS code from the AST tree using ProtoCore code generator
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();
            UpdateAddedNodesInModifiedNameList();
            return code;
        }


        // TODO Jun:
        // This flag is temporary and will be removed whne the pattern rewrite basic functionality is implemented
        private bool usePatternRewrite = false;

        public static GraphCompiler CreateInstance()
        {
            return new GraphCompiler();
        }


        /// <summary>
        /// This function resets properties in the graph compiler required in preparation for a subsequent run
        /// </summary>
        public void ResetPropertiesForNextExecution()
        {
            // TODO Jun: Determine an optimal solution where we dont need to reset the entire flag list to false.
            // Perhaps the runtime VM Update can set this?
            foreach (string key in ExecutionFlagList.Keys.ToList())
            {
                ExecutionFlagList[key] = false;
            }

            // Clear the list that holds deleted variables
            RemovedNodeNameList.Clear();
            UndefinedNameList.Clear();
        }

        /// <summary>
        ///  This function updates the nodes of a codeblock that was split given the original codeblocks' uid
        /// </summary>
        /// <param name="codeblock"></param>
        private void UpdateCodeBlockNodeDirtyFlags(SnapshotNode codeblock)
        {
            Validity.Assert(codeblock.Type == SnapshotNodeType.CodeBlock);
            string name = string.Empty;

            // The codeblock is multiline if its uid exisits in the codeblock map
            // A better approach maybe the IDE flagging this as a snapshot node property

            // Comment Jun:
            // If its a single line codeblock, then just get its name and update the flag
            // If its a multi-line codeblock, then get all the associated split codeblocks, get all the names, and update the flag

            bool isMultiLine = codeBlockUIDMap.ContainsKey(codeblock.Id);
            if (isMultiLine)
            {
                // This function will nto be necessary if the snapshot node stored the number of outputslots as a property
                int outputSlots = codeblock.GetNumOutputs();
                for (int index = 0; index < outputSlots; index++)
                {
                    name = GetVarName(codeblock.Id, index, SnapshotNodeType.CodeBlock);
                    if (name != null && name.Length > 0)
                    {
                        if (ExecutionFlagList.Keys.Contains(name))
                        {
                            ExecutionFlagList[name] = true;
                        }
                    }
                }
            }
            else
            {
                name = GetVarName(codeblock.Id);
                if (name != null && name.Length > 0)
                {
                    if (ExecutionFlagList.Keys.Contains(name))
                    {
                        ExecutionFlagList[name] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the ExecutionFlagList (list of statements to execute), given a modified list.
        /// where: If a statment needs to be executed (identifier key), its value must be true
        /// </summary>
        /// <param name="nodesToModify"></param>
        public void UpdateDirtyFlags(List<SnapshotNode> nodesToModify)
        {
            if (nodesToModify != null)
            {
                // For every node that was modified, do the following:
                foreach (SnapshotNode sNode in nodesToModify)
                {
                    if (sNode.Type == SnapshotNodeType.CodeBlock)
                    {
                        UpdateCodeBlockNodeDirtyFlags(sNode);
                    }
                    else
                    {
                        // This is not a codeblock node
                        // Check if varname associated with this node exisits
                        string name = GetVarName(sNode.Id);
                        if (name != null && name.Length > 0)
                        {
                            // 
                            if (ExecutionFlagList.Keys.Contains(name))
                            {
                                ExecutionFlagList[name] = true;
                            }
                        }
                    }
                }
            }
        }

        public string GetVarName(uint uid)
        {
            string name = "";
            Node n = graph.GetNode(uid);
            if (null != n)
            {
                if (n is Func)
                {
                    name = ((Func)n).tempName;
                }
                else if (n is Operator)
                {
                    name = ((Operator)n).tempName;
                }
                else if (n is LiteralNode)
                {
                    name = ((LiteralNode)n).tempName;
                }
                else if (n is Block)
                {
                    name = ((Block)n).LHS;
                }
                else
                {
                    name = n.Name;
                }
            }
            else
            {
            }
            return name;
        }


        public string GetVarName(uint uid, int index, SnapshotNodeType type)
        {
            Validity.Assert(type == SnapshotNodeType.CodeBlock);

            // The graph would have already been rewritten (split) for codeblock nodes
            Dictionary<int, uint> indexUIDMap = new Dictionary<int, uint>();
            bool foundUID = codeBlockUIDMap.TryGetValue(uid, out indexUIDMap);
            Validity.Assert(foundUID);

            uint slotGUID = 0;
            bool foundNewUID = indexUIDMap.TryGetValue(index, out slotGUID);
            Validity.Assert(foundNewUID);

            Node node = graph.GetNode(slotGUID);
            Block block = node as Block;
            Validity.Assert(block != null);

            return block.LHS;

        }

        public bool CreateArrayNode(uint guid, string content)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(content))
                throw new ArgumentException("Invalid argument value!", "content");
            ArrayNode ar = new ArrayNode(content, guid);
            graph.AddNode(ar);
            return true;
        }
        public bool CreateRangeNode(uint guid, string range, int args, int argType, string tempName)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(range))
                throw new ArgumentException("Invalid argument value!", "opSymbol");
            Func op = new Func(range, guid, args);
            op.isRange = true;
            op.isStatic = true;
            op.argTypeRange = argType;
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }
        public bool CreateRangeNode(uint guid, string range, int args, int argType, string tempName,string replicationGuides)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(range))
                throw new ArgumentException("Invalid argument value!", "opSymbol");
            Func op = new Func(range, guid, args,replicationGuides);
            op.isRange = true;
            op.isStatic = true;
            op.argTypeRange = argType;
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }
        public bool CreateOperatorNode(uint guid, string opSymbol, string tempName)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(opSymbol))
                throw new ArgumentException("Invalid argument value!", "opSymbol");

            Operator op = new Operator(opSymbol, guid);
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }
        public bool CreateOperatorNode(uint guid, string opSymbol, string tempName, string replicationGuide)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(opSymbol))
                throw new ArgumentException("Invalid argument value!", "opSymbol");

            Operator op = new Operator(opSymbol, guid, replicationGuide);
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }
        public bool CreateFunctionNode(uint guid, string functionName, int args, string tempName)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(functionName))
                throw new ArgumentException("Invalid argument value!", "functionName");

            Func op = new Func(functionName, guid, args);
            op.tempName = tempName;
            op.isStatic = true;
            graph.AddNode(op);
            return true;
        }

        public bool CreateFunctionNode(uint guid, string functionName, int args, string tempName, string replicationGuides)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(functionName))
                throw new ArgumentException("Invalid argument value!", "functionName");
            Func op = new Func(functionName, guid, args, replicationGuides);
            op.isStatic = true;
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }
        public bool CreateMethodNode(uint guid, string functionName, int args, string tempName, bool isMemberFunction = false)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(functionName))
                throw new ArgumentException("Invalid argument value!", "methodName");
            Func op = new Func(functionName, guid, args);
            op.isStatic = false;
            op.isMemberFunction = isMemberFunction;
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }
        public bool CreateMethodNode(uint guid, string functionName, int args, string tempName, string replicationGuides, bool isMemberFunction = false)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(functionName))
                throw new ArgumentException("Invalid argument value!", "methodName");
            Func op = new Func(functionName, guid, args, replicationGuides);
            op.isStatic = false;
            op.isMemberFunction = isMemberFunction;
            op.tempName = tempName;
            graph.AddNode(op);
            return true;
        }

        
        private bool IsStaticCall(string dotcall)
        {
            Validity.Assert(!string.IsNullOrEmpty(dotcall));

            // TODO Jun: Determine if this check needs to be done at the UI level
            // Determine if there are instances where the UI shouldnt know about class properties
            char[] delimiter = {'.'};
            string[] tokens = dotcall.Split(delimiter);
            
            Validity.Assert(tokens.Length > 0);
            if (tokens.Length == 1)
            {
                // This is just a property
                return false;
            }

            string classname = tokens[0];
            string propertyname = tokens[1];

            // TODO Jun: Optimization
            // IsStaticMemberVariable is an expensive call, consider optimizing that function or cache property name
            int classindex = core.ClassTable.IndexOf(classname);
            ProtoCore.DSASM.ClassNode cnode = core.ClassTable.ClassNodes[classindex];

            // Check if the proeprty is a member variable
            return cnode.IsStaticMemberVariable(propertyname);
        }

        public bool CreatePropertyNode(uint guid,string propertyName,string tempName)
        {
            if (guid < 0)
            {
                throw new ArgumentException("Invalid argument value!", "guid");
            }
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Invalid argument value!", "propertyName");
            }


            // Handle static calls
            // TODO Jun: Determine if this check needs to be done at the UI level
            Validity.Assert(null != core);
            if (IsStaticCall(propertyName))
            {
                CreateCodeblockNode(guid, tempName + "=" + propertyName);
            }
            else
            {
                Func op = new Func(propertyName, guid);
                op.isStatic = false;
                op.isProperty = true;
                op.tempName = tempName;
                graph.AddNode(op);
            }
            return true;
        }
        public bool CreateImportNode(uint guid, string library)
        {
            if (guid < 0)
            {
                throw new ArgumentException("Invalid argument value!", "guid");
            }
            if (String.IsNullOrEmpty(library))
            {
                throw new ArgumentException("Invalid argument value!", "identifier");
            }

            ImportNode id = new ImportNode(library, guid);
            graph.AddNode(id);
            return true; // If failed, return 'false'.
        }

        public bool CreateIdentifierNode(uint guid, string identifier)
        {
            if (guid < 0)
                throw new ArgumentException("Invalid argument value!", "guid");
            if (String.IsNullOrEmpty(identifier))
                throw new ArgumentException("Invalid argument value!", "identifier");

            IdentNode id = new IdentNode(identifier, guid);
            graph.AddNode(id);
            return true; // If failed, return 'false'.
        }

        public bool CreateLiteralNode(uint guid, object value)
        {
            //String guida = guid.ToString();
            /* switch (value.GetType().ToString())
             {
                 case "System.Boolean": break;
                 case "System.Double":
                     graph.AddNode(new DoubleNode(value.ToString(), guida));
                     break;
                 case "System.Int32":
                     graph.AddNode(new IntNode(value.ToString(), guida));
                     break;
                 case "System.String": break;

                 default:
                     throw new ArgumentException("Invalid argument!", "type");
             }*/
            graph.AddNode(new LiteralNode(value.ToString(), guid));

            return true; // If failed, return 'false'.
        }

        public bool CreateCodeblockNode(uint guid, string code)
        {
            graph.AddNode(new Block(code, guid));
            RewriteCodeBlock(graph);
            return true;
        }


        public bool CreateCodeblockNode(SnapshotNode ssn)
        {
            graph.AddNode(new Block(ssn.Content, ssn.Id, ssn.Assignments));
            RewriteCodeBlock(graph);

            return true;
        }

        public bool DeleteAllEdges(uint guidToDelete)
        {
            Node nodeToDelete = graph.GetNode(guidToDelete);
            graph.RemoveAllEdges(nodeToDelete);
            return true;
        }

        public bool DeleteEdge(uint outputNode, int outputIndex, uint inputNode, int inputIndex)
        {
            uint from = inputNode;
            uint to = outputNode;
            graph.RemoveEdge("", "", from, to);
            return true;
        }

        public void ResetUndefinedVariables()
        {
            foreach (var variables in UndefinedNameList)
            {
                mapModifiedName.Remove(variables); 
            }
            UndefinedNameList.Clear();
        }

        public void UndefineVariablesForNodes(List<string> variables, uint nodeId)
        {
            if (variables == null || variables.Count() == 0)
                return;

            this.UndefinedNameList.AddRange(variables);

            Node node = null;
            Dictionary<int, uint> slotUIDMap = new Dictionary<int, uint>();

            // @keyu: here is the tricky part...if a variable is undefined in
            // a node in current run, e.g, a node "a = 1;" is modified to ";", 
            // as the new node doesn't contain a variable, its preview window
            // won't be updated so that the stale value will be kept there. 
            // 
            // We should put the undefined value to mapModifiedName list so
            // that the host will get node's id from this map and further query
            // the preview value of that node. If there are some valid 
            // statements in that node, that's total fine -- the preview window
            // will display the value of last statement. Otherwise the preview
            // window will be dismissed and won't display any stale value.
            //
            // But, here we takes too much assumptions of what will the host 
            // (graph ui) behave. We may decouple this association. 
            //
            // Aslo refer to defect IDE-1882.
            if (this.codeBlockUIDMap.TryGetValue(nodeId, out slotUIDMap))
            {
                uint slotUID = slotUIDMap.GetEnumerator().Current.Value;
                if (slotUID != 0)
                {
                    node = graph.GetNode(slotUID);
                }
            }
            else
            {
                node = graph.GetNode(nodeId);
            }

            if (node != null)
            {
                uint uid = GetRealUID(node.Guid);
                foreach (var variable in variables)
                {
                    mapModifiedName[variable] = uid;
                }
            }
        }

        public bool RemoveNodes(uint guidToDelete, bool removeFromRemovedNodes)
        {
            bool result = false;
            Node nodeToDelete = graph.GetNode(guidToDelete);
            string name = GetVarName(nodeToDelete.Guid);
            if (nodeToDelete != null)
            {
                if (removeFromRemovedNodes) 
                {
                    RemovedNodeNameList.Add(name);
                }
                HandleRemovedNode(nodeToDelete);

                result = graph.RemoveNode(nodeToDelete);
                UpdateRemovedNodesInModifiedNameList(name);
            }
            else 
            { 
                List<uint> guidsToDelete = UpdateUIDForCodeblock(guidToDelete);
                bool ans = false;
                foreach (uint guid in guidsToDelete)
                {
                    nodeToDelete = graph.GetNode(guid);
                    if (nodeToDelete != null)
                    {
                        name = GetVarName(nodeToDelete.Guid);
                        if (removeFromRemovedNodes)
                        {
                            RemovedNodeNameList.Add(name);
                        }
                        HandleRemovedNode(nodeToDelete);
                        ans = graph.RemoveNode(nodeToDelete);
                        UpdateRemovedNodesInModifiedNameList(name);
                    }
                }
                result = ans;
            }
            
            //char[] delimit = { '.', '[' };
            //name = name.Split(delimit)[0];
            //GraphUtilities.RemoveGlobalSymbols(name);

            return result;
        }

        public bool ReplaceNodesFromAList(List<Node> l1)
        {
            foreach (Node n in l1)
            {
                Node toBeReplaced=graph.GetNode(n.Guid);
                graph.ReplaceNode(n,toBeReplaced);
            }
            return true;
        }
        public bool UpdateNodes(SnapshotNode node)
        {
            uint guidToUpdate = node.Id;
            string content = node.Content;

            Node nodeToUpdate = graph.GetNode(guidToUpdate);
            nodeToUpdate.Name = content;

            // TODO Jun: Check with Luke/Chirag what the codeblock node can contain
            if (node.Type == SnapshotNodeType.CodeBlock || node.Type == SnapshotNodeType.Literal)
            {
                (nodeToUpdate as Block).content = content;
            }

            return true;
        }

        public bool ConnectNodes(uint outputNode, int outputIndex, uint inputNode, int inputIndex)
        {
            uint from = UpdateUIDForCodeblock(inputNode, inputIndex);
            uint to = outputNode;
            to = UpdateUIDForCodeblock(to, outputIndex);
            if (!graph.HasEdge(from, to, inputIndex))
            {
                try
                {
                    graph.AddEdge(" ", " ", from, to, inputIndex, outputIndex);
                }
                catch (HasCycleException h)
                {
                    gcs.LogError(h.Message);
                }
            }
            return true;
        }



        public bool ConnectNodes(uint outputNode, int outputIndex, uint inputNode, int inputIndex, string mapping)
        {
            //uint from = UpdateUIDForCodeblock(inputNode, inputIndex);
            uint from = GetUidOfRHSIdentifierInCodeBlock(inputNode, inputIndex, mapping);
            uint to = outputNode;
            to = UpdateUIDForCodeblock(to, outputIndex);
            if (!graph.HasEdge(from, to, inputIndex))
                try
                {
                    graph.AddEdge(" ", " ", from, to, inputIndex, outputIndex);
                    Node n=graph.GetNode(to);
                    if (n is Block)
                    {
                        Block node = (Block)n;
                        /*
                        if (mapping.Equals(""))
                        {
                        }
                        else
                        {
                            string s = from.ToString() + outputIndex.ToString() + to.ToString() + inputIndex.ToString();
                            if (!node.outputIndexMap.ContainsKey(s))
                                node.outputIndexMap.Add(s, mapping);
                            if (!node.stats.ContainsKey(mapping))
                                node.stats.Add(mapping, new Block(mapping, to + (uint)mapping.Length + (uint)mapping[0]));
                        }*/
                    }
                }
                catch (HasCycleException h)
                {
                    gcs.LogError(h.Message);
                }

            return true;
        }

        /// <summary>
        /// Given an codeblock node 'toCodeBlock', determine which stmt uid in the codeblock 'index' is connected to
        /// 'index' is the slot connection into the codeblock
        /// </summary>
        /// <param name="toCodeBlock"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint UpdateUIDForCodeblock(uint toCodeBlock, int index)
        {
            uint connectedToCodeBlockSubUID = toCodeBlock;
            Dictionary<int, uint> indexUIDMap = new Dictionary<int, uint>();

            // Get the codeblock map
            bool foundUID = codeBlockUIDMap.TryGetValue(toCodeBlock, out indexUIDMap);
            if (foundUID)
            {
                uint slotGUID = 0;
                if (index == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    index = indexUIDMap.Count - 1;
                }

                // Find the UID in the codeblock map where 'index' is connected to
                bool foundNewUID = indexUIDMap.TryGetValue(index, out slotGUID);
                if (foundNewUID)
                {
                    if (slotGUID != 0)
                    {
                        connectedToCodeBlockSubUID = slotGUID;
                    }
                }
            }
            return connectedToCodeBlockSubUID;
        }


        /// <summary>
        /// Given an codeblock node 'toCodeBlock', determine which stmt uid in the codeblock 'identifier' is connected to
        /// 'index' is the slot connection into the codeblock
        /// </summary>
        /// <param name="toCodeBlock"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint GetUidOfRHSIdentifierInCodeBlock(uint toCodeBlock, int index, string identifier)
        {
            uint connectedToCodeBlockSubUID = toCodeBlock;
            Dictionary<int, uint> indexUIDMap = new Dictionary<int, uint>();

            // Get the codeblock map
            bool foundUID = codeBlockUIDMap.TryGetValue(toCodeBlock, out indexUIDMap);
            if (foundUID)
            {
                // Find the UID in the codeblock map where 'identifier' is connected to
                // For every node in the codeblock, check if 'identifier' exisits in the rhs of the node contents
                foreach (KeyValuePair<int, uint> kvp in indexUIDMap)
                {
                    uint subUID = kvp.Value;                    
                    Block block = Graph.GetNode(subUID) as Block;
                    Validity.Assert(null != block);

                    if (1 == block.assignmentData.Count)
                    {
                        if (block.assignmentData[0].References.Contains(identifier))
                        {
                            connectedToCodeBlockSubUID = subUID;
                            return connectedToCodeBlockSubUID;
                            //break;
                        }
                    }
                    else
                    {
                        // If no assignment data - reverto to just peeking into the index map to see which uid 'index' is connected to
                        // This will be deprecated once AssignmetnData is serialized in all Snapshot nodes
                        return UpdateUIDForCodeblock(toCodeBlock, index);
                    }
                }
            }
            return UpdateUIDForCodeblock(toCodeBlock, index);
        }

        private List<uint> UpdateUIDForCodeblock(uint guidToDelete)
        {
            Dictionary<int, uint> indexUIDMap = new Dictionary<int, uint>();
            List<uint> Guids = new List<uint>();
            bool foundUID = codeBlockUIDMap.TryGetValue(guidToDelete, out indexUIDMap);
            if (foundUID)
            {
                var flattenedList = indexUIDMap.Values.ToList();
                Guids = flattenedList.ToList();
            }
            return Guids;
        }
        public string PrintGraph()
        {
            return BuildScript(graph);
        }

        /**/
        public List<uint> ConnectionToUID(List<Connection> input)
        {
            List<uint> result = new List<uint>();
            foreach (Connection inputConnection in input)
            {
                result.Add(inputConnection.OtherNode);
            }
            return result;
        }

        /*Tron: Use for node to code function
         *For each connected component of the graph, generate a respective string of code
         */
        public List<SnapshotNode> ToCode(AST graph, GraphCompiler originalGC, List<SnapshotNode> inputs)
        {
            List<SnapshotNode> result = new List<SnapshotNode>();
            string liststat = "";
            List<Node> li = TopSort.sort(graph);
            tguid = 20000;
            List<string> listIslands = new List<string>();
            List<Node> islandNodes = new List<Node>();
            int countIslands = 0;
            statementList = new AST();
            ModifiedStmtGuidList.Clear();
            List<string> importIslands = new List<string>();
            IEnumerable iter = li;
            List<Node> islandNodeList = new List<Node>();
            //List<List<Node>> listing = new List<List<Node>>();
            List<Node> listing = new List<Node>();
            foreach (Node node in iter)
            {
                if (node != null)
                {
                    if (node is ImportNode)
                    {
                        importIslands.Add(node.ToScript() + ProtoCore.DSASM.Constants.termline);
                    }
                    else if (node.IsIsland)
                    {
                        countIslands++;
                        if (node is ArrayNode)
                        {
                            BuildArrayNodeStatement(node, statementList);
                            if (!islandNodes.Contains(node))
                                islandNodes.Add(node);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is LiteralNode)
                        {
                            BuildLiteralNodeStatement(node, statementList);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is Func)
                        {
                            BuildFunctionCallStatement(node, statementList);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is Operator)
                        {
                            BuildOperatorStatement(node, statementList);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is Block)
                        {
                            BuildBlockStatement(node, statementList);
                            islandNodes.Add(node);
                            //string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            //if (!listIslands.Contains(island))
                              //  listIslands.Add(island);
                        }
                        else if (node is IdentNode)
                        {
                            // comment Jun:
                            // An island identifier node is handled by emitting a null as its rhs
                            statementList.AddNode(node);
                            string contents = statementList.GetNode(node.Guid).ToScript() + "=" + ProtoCore.DSASM.Literal.Null + ProtoCore.DSASM.Constants.termline;
                            listIslands.Add(contents);
                        }
                        else
                        {
                            statementList.AddNode(node);
                            string island = node.ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        islandNodeList.Add(node);
                        listing = listing.ToList().Union<Node>(BuildStatement(node, statementList)).ToList();
                        HandleNewNode(node);
                    }
                    else if (node.IsLeaf)
                    {
                        if (node is ArrayNode)
                        {
                            BuildArrayNodeStatement(node, statementList);
                        }
                        else if (node is LiteralNode)
                        {
                            BuildLiteralNodeStatement(node, statementList);
                        }
                        else if (node is Func)
                        {
                            BuildFunctionCallStatement(node, statementList);
                        }
                        else if (node is Operator)
                        {
                            BuildOperatorStatement(node, statementList);
                        }
                        else if (node is Block)
                        {
                            BuildBlockStatement(node, statementList);
                        }
                        else if (node is IdentNode)
                        {
                            statementList.AddNode(node);
                            string contents = statementList.GetNode(node.Guid).ToScript() + "=" + ProtoCore.DSASM.Literal.Null + ProtoCore.DSASM.Constants.termline;
                            listIslands.Add(contents);
                        }
                        HandleNewNode(node);
                    }
                    else if (node.IsRoot && !node.IsIsland)
                    {
                        if (node is Operator)
                        {
                            BuildOperatorStatement(node, statementList);
                        }
                        else if (node is Func)
                        {
                            BuildFunctionCallStatement(node, statementList);
                        }
                        else if (node is Block)
                        {
                            BuildBlockStatement(node, statementList);
                        }
                        //liststat = BuildStatement(node, statementList);
                        //finalScript=finalScript.Union(BuildStatement(node, statementList)).ToList();
                        
                        //comment out for NodeToCode function
                        //listing.Add(BuildStatement(node, statementList));
                        listing = BuildStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is Operator)
                    {
                        BuildOperatorStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is Func)
                    {
                        BuildFunctionCallStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is ArrayNode)
                    {
                        BuildArrayNodeStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is LiteralNode)
                    {
                        BuildLiteralNodeStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is Block)
                    {
                        BuildBlockStatement(node, statementList);
                        listing = BuildStatement(node, statementList);
                        HandleNewNode(node);
                    }
                }
            }
            StringBuilder builder = new StringBuilder();
            foreach (string island in importIslands)// Loop through all strings
            {
                builder.Append(island);             // Append string to StringBuilder
            }
            foreach (string island in listIslands)  // Loop through all strings
            {
                builder.Append(island);             // Append string to StringBuilder
            }

            /*N2C*/
            #region get connected components of the graph
            List<Node> nodeToCodeInputList = new List<Node>();
            List<List<Node>> listingAlternate = new List<List<Node>>();
            List<List<Node>> listingAlt2 = new List<List<Node>>();
            if (nodeToCodeUIDs.Count != 0)
            {
                foreach (uint nodeID in nodeToCodeUIDs)
                {
                    if (graph.GetNode(nodeID) != null)
                    {
                        nodeToCodeInputList.Add(graph.GetNode(nodeID));
                    }
                    else
                    {
                        if (this.codeBlockUIDMap[nodeID] != null)
                        {
                            foreach (KeyValuePair<int, uint> pair in this.codeBlockUIDMap[nodeID])
                            {
                                nodeToCodeInputList.Add(graph.GetNode(pair.Value));
                            }
                        }
                    }
                }
                //listingAlternate = GetConnectedComponents(nodeToCodeInputList, graph);
                listingAlt2 = GetConnectedComponents_02(nodeToCodeInputList, graph);
            }
            #endregion

            //foreach (List<Node> n1 in listing)
            //{
            //    foreach (Node n2 in n1)
            //        if (!finalScript.Contains(n2))
            //            finalScript.Add(n2);
            //        else
            //        {
            //            finalScript.Remove(n2);
            //            finalScript.Add(n2);
            //        }
            //}
            listing = SortCodeBlocks(listing);
            List<List<Node>> finalList = new List<List<Node>>();
            foreach (List<Node> l1 in listingAlt2)
            {
                List<Node> temp = new List<Node>();
                foreach (Node n1 in l1)
                    foreach (Node listingNode in listing)
                        if (listingNode.Guid == n1.Guid)
                            temp.Add(listingNode);
                if (temp.Count != 0)
                    finalList.Add(temp);
            }            
            islandNodeList = islandNodeList.Union(islandNodes).ToList();
            //islandNodeList = SortCodeBlocks(islandNodeList);
            //if (islandNodeList.Count != 0)
            //    finalList.Add(islandNodeList);

            #region generate code and snapshot node
            uint id = 0;
            foreach (List<Node> nodeList in finalList)
            {
                string output = "";
                List<Node> tempList = SortCodeBlocks(nodeList);
                //tempList.Reverse();
                foreach (Node node in tempList)
                {
                    if (nodeToCodeUIDs.Contains(node.Guid))
                    {
                        if (node.ToCode() != null)
                            output += node.ToCode().Replace(";", "") + ProtoCore.DSASM.Constants.termline;
                    }
                    else
                    {
                        foreach (KeyValuePair<uint, Dictionary<int, uint>> pair in this.codeBlockUIDMap)
                        {
                            if (pair.Value.ContainsValue(node.Guid))
                            {
                                output += node.ToCode().Replace(";", "") + ProtoCore.DSASM.Constants.termline; //ensure only one semicolon at the end of a statement, by request from UI
                            }
                        }
                    }    
                }
                output = output.TrimEnd('\n');
                SnapshotNode ssn = new SnapshotNode();
                ssn.Id = id++;
                ssn.Content = output;
                ssn.Type = SnapshotNodeType.CodeBlock;
                ssn.InputList = new List<Connection>();

                foreach (SnapshotNode inputNode in inputs)
                {
                    foreach (Node subTreeNode in nodeList)
                    {
                        if (inputNode.Id == subTreeNode.Guid)
                        {
                            foreach (Connection c1 in inputNode.InputList)
                            {
                                if (!IsInternalConnection(c1, this))                //the connection is not internal, return it back to UI
                                {
                                    Connection newInputConnection = new Connection();
                                    newInputConnection.OtherNode = c1.OtherNode;
                                    newInputConnection.OtherIndex = c1.OtherIndex;
                                    newInputConnection.IsImplicit = c1.IsImplicit;
                                    string[] tokens = graph.GetNode(c1.OtherNode).Name.Split('=');
                                    newInputConnection.LocalName = tokens[0];
                                    ssn.InputList.Add(newInputConnection);
                                }
                            }
                        }
                        else if (codeBlockUIDMap.ContainsKey(inputNode.Id))         //inputNode was split
                        {
                            if (codeBlockUIDMap[inputNode.Id].ContainsValue(subTreeNode.Guid))
                            {
                                foreach (Connection c1 in inputNode.InputList)
                                {
                                    if (!IsInternalConnection(c1, this))
                                    {
                                        int indexSlot = 0;
                                        foreach (KeyValuePair<int, uint> pair in codeBlockUIDMap[inputNode.Id])
                                        {
                                            if (pair.Value == subTreeNode.Guid)
                                                indexSlot = pair.Key;
                                        }
                                        if (c1.OtherIndex == indexSlot)
                                        {
                                            Connection newInputConnection = new Connection();
                                            newInputConnection.OtherNode = c1.OtherNode;
                                            newInputConnection.OtherIndex = c1.OtherIndex;
                                            foreach (KeyValuePair<uint, Dictionary<int, uint>> pair in originalGC.codeBlockUIDMap)
                                            {
                                                if (pair.Value.ContainsValue(c1.OtherNode))                     //this means if the other node was split, return the original Id that was sent to us by the UI
                                                {
                                                    newInputConnection.OtherNode = pair.Key;
                                                    newInputConnection.OtherIndex = pair.Value.First(x => x.Value == c1.OtherNode).Key;
                                                }
                                            }
                                            newInputConnection.IsImplicit = c1.IsImplicit;
                                            string[] tokens = graph.GetNode(c1.OtherNode).Name.Split('=');
                                            newInputConnection.LocalName = tokens[0];
                                            ssn.InputList.Add(newInputConnection);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                ssn.OutputList = new List<Connection>();
                foreach (SnapshotNode inputNode in inputs)
                {
                    foreach (Node subTreeNode in nodeList)
                    {
                        //if (subTreeNode.Name.Split('=')[0] == outputCnt.LocalName)
                        if (inputNode.Id == subTreeNode.Guid)
                        {
                            foreach (Connection c1 in inputNode.OutputList)
                            {
                                if (!IsInternalConnection(c1, this))
                                {
                                    Connection newOutputConnection = new Connection();
                                    newOutputConnection.OtherNode = c1.OtherNode;
                                    newOutputConnection.OtherIndex = c1.OtherIndex;
                                    newOutputConnection.IsImplicit = c1.IsImplicit;
                                    newOutputConnection.LocalName = c1.LocalName;
                                    ssn.OutputList.Add(newOutputConnection);
                                }
                            }
                        }

                        else if (codeBlockUIDMap.ContainsKey(inputNode.Id))         //inputNode was split
                        {
                            if (codeBlockUIDMap[inputNode.Id].ContainsValue(subTreeNode.Guid))
                            {
                                foreach (Connection c1 in inputNode.OutputList)
                                {
                                    if (!IsInternalConnection(c1, this))
                                    {
                                        int indexSlot = 0;
                                        foreach (KeyValuePair<int, uint> pair in codeBlockUIDMap[inputNode.Id])
                                        {
                                            if (pair.Value == subTreeNode.Guid)
                                            {
                                                indexSlot = pair.Key;
                                                break;
                                            }
                                        }
                                        if (c1.LocalIndex == indexSlot)
                                        {
                                            Connection newOutputConnection = new Connection();
                                            newOutputConnection.OtherNode = c1.OtherNode;
                                            newOutputConnection.OtherIndex = c1.OtherIndex;
                                            foreach (KeyValuePair<uint, Dictionary<int, uint>> pair in originalGC.codeBlockUIDMap)
                                            {
                                                if (pair.Value.ContainsValue(c1.OtherNode))                     //this means if the other node was split, return the original Id that was sent to us by the UI
                                                {
                                                    newOutputConnection.OtherNode = pair.Key;
                                                    newOutputConnection.OtherIndex = pair.Value.First(x => x.Value == c1.OtherNode).Key;
                                                }
                                            }
                                            newOutputConnection.IsImplicit = c1.IsImplicit;
                                            newOutputConnection.LocalName = c1.LocalName;
                                            ssn.OutputList.Add(newOutputConnection);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                result.Add(ssn);
            }
            #endregion

            #region remove _temp_xxx name
            //Dictionary<string, string> tempReplaceValue = new Dictionary<string, string>();
            //for (int i = 0; i < result.Count; i++)
            //{
            //    SnapshotNode ssn = result[i];
            //    string[] statements = ssn.Content.Split(';');
            //    for (int j = 0; j < statements.Length; j++)
            //    {
            //        string statement = statements[j];
            //        string lhsTempName = statement.Split('=')[0].Replace("\n", "").Trim();
            //        foreach (Node astNode in graph.nodeList)
            //        {
            //            if (astNode.Name.Split('=')[0].Trim() == lhsTempName && lhsTempName.StartsWith("_temp_"))
            //            {
            //                //this means in the ast there is some statement _temp_abc = something
            //                //which means the _temp_xxx is generated, not typed in by users
            //                tempReplaceValue.Add(lhsTempName, statement.Split('=')[1].Replace("\n", "").Trim());
            //            }
            //        }
            //    }
            //}
            //for (int i = 0; i < result.Count; i++)
            //{
            //    SnapshotNode ssn = result[i];
            //    foreach (KeyValuePair<string, string> pair in tempReplaceValue)
            //    {
            //        ssn.Content.Replace(pair.Key, pair.Value);
            //    }
            //}
            #endregion

            #region replace _temp_ name with more elegant name
            //int tempId = 0;
            //for (int i = 0; i < result.Count; i++)
            //{
            //    SnapshotNode ssn = result[i];
            //    string[] statements = ssn.Content.Split(';');
            //    for (int j = 0; j < statements.Length; j++)
            //    {
            //        string statement = statements[j];
            //        string lhsTempName = statement.Split('=')[0].Replace("\n", "").Trim();
            //        foreach (Node astNode in graph.nodeList)
            //        {
            //            if (astNode.Name.Split('=')[0].Trim() == lhsTempName && lhsTempName.StartsWith("_temp_"))
            //            {
            //                string newTempName = "temp_" + tempId++;
            //                for (int k = 0; k < result.Count; k++)
            //                {
            //                    result[k].Content = result[k].Content.Replace(lhsTempName, newTempName);
            //                }
            //            }
            //        }
            //        if (statement.Split('=').Length > 1)
            //        {
            //            string rhsTempName = statement.Split('=')[1].Replace("\n", "").Trim();
            //            foreach (Node astNode in graph.nodeList)
            //            {
            //                if (astNode.Name.Split('=')[0].Trim() == rhsTempName && rhsTempName.StartsWith("_temp_"))
            //                {
            //                    string newTempName = "temp_" + tempId++;
            //                    for (int k = 0; k < result.Count; k++)
            //                    {
            //                        result[k].Content = result[k].Content.Replace(rhsTempName, newTempName);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion

            #region return to original input connections
            for (int i = 0; i < result.Count; i++)
            {
                SnapshotNode ssn = result[i];
                for (int j = 0; j < ssn.InputList.Count; j++)
                {
                    Connection inputConnection = ssn.InputList[j];
                    foreach (KeyValuePair<uint, Dictionary<int, uint>> kvp in originalGC.codeBlockUIDMap)
                    {
                        foreach (KeyValuePair<int, uint> kvp2 in kvp.Value)
                        {
                            if (kvp2.Value == inputConnection.OtherNode)
                            {
                                Connection oldInputConnection = new Connection();
                                oldInputConnection.OtherNode = kvp.Key;
                                oldInputConnection.OtherIndex = inputConnection.OtherIndex;
                                oldInputConnection.IsImplicit = inputConnection.IsImplicit;
                                oldInputConnection.LocalName = inputConnection.LocalName;
                                oldInputConnection.LocalIndex = inputConnection.LocalIndex;
                                ssn.InputList.Remove(inputConnection);
                                ssn.InputList.Insert(j, oldInputConnection);
                            }
                        }
                    }
                }

                for (int j = 0; j < ssn.OutputList.Count; j++)
                {
                    Connection outputConnection = ssn.OutputList[j];
                    foreach (KeyValuePair<uint, Dictionary<int, uint>> kvp in originalGC.codeBlockUIDMap)
                    {
                        foreach (KeyValuePair<int, uint> kvp2 in kvp.Value)
                        {
                            if (kvp2.Value == outputConnection.OtherNode)
                            {
                                Connection oldInputConnection = new Connection();
                                oldInputConnection.OtherNode = kvp.Key;
                                oldInputConnection.OtherIndex = outputConnection.OtherIndex;
                                oldInputConnection.IsImplicit = outputConnection.IsImplicit;
                                oldInputConnection.LocalName = outputConnection.LocalName;
                                oldInputConnection.LocalIndex = outputConnection.LocalIndex;
                                ssn.OutputList.Remove(outputConnection);
                                ssn.OutputList.Insert(j, oldInputConnection);
                            }
                        }
                    }
                }
            }
            #endregion
            /*Chirag's
            foreach (var value in finalScript)
            {
                if (nodeToCodeUIDs.Contains((value as Node).Guid))
                    if (value.ToCode() != null)
                        liststat += value.ToCode() + ProtoCore.DSASM.Constants.termline;
            }
            liststat = builder.ToString() + liststat;
            //liststat += builder.ToString();
            //GraphUtilities.runningUID = GraphToDSCompiler.Constants.UIDStart;
            //result.AddRange(liststat.Split(';'));
            */
            UpdateAddedNodesInModifiedNameList();

            return result;
        }

        private bool IsInternalConnection(Connection c1, GraphCompiler oriGC)
        {
            bool result = false;
            if (nodeToCodeUIDs.Contains(c1.OtherNode))
            {
                result = true;
            }  
            else
            {
                foreach (KeyValuePair<uint, Dictionary<int, uint>> kvp in oriGC.codeBlockUIDMap)
                {
                    if (nodeToCodeUIDs.Contains(kvp.Key) && kvp.Value.ContainsValue(c1.OtherNode))
                        result = true;
                }
            }
            return result;
        }
        /*end*/
        #region find number of connected subgraphs in a given graph
        private List<List<Node>> GetConnectedComponents(List<Node> input, AST graph)
        {
            List<Node> inputCopy = new List<Node>(input);
            List<List<Node>> result = new List<List<Node>>();
            foreach (Node node in input)
            {
                if (inputCopy.Contains(node))
                {
                    //List<Node> dfsFromNode = DFS_Children(node, graph, inputCopy);
                    //dfsFromNode.AddRange(DFS_Parent(node, graph, inputCopy));

                    List<Node> dfsFromNode = DFS_Parent(node, graph, inputCopy);
                    if (dfsFromNode.Count != 1)
                    {
                        dfsFromNode.Remove(node);
                    }
                    List<Node> temp = new List<Node>(dfsFromNode);
                    foreach (Node parentNode in dfsFromNode)
                    {
                        List<Node> dfsFromParentNode = DFS_Children(parentNode, graph, inputCopy);
                        foreach (Node nodeInParentDFSTree in dfsFromParentNode)
                        {
                            if (!dfsFromNode.Contains(nodeInParentDFSTree))
                                //temp.Remove(nodeInParentDFSTree);
                                //temp.Add(nodeInParentDFSTree);
                                temp = new List<Node>(dfsFromParentNode);
                        }
                    }
                    dfsFromNode = new List<Node>(temp);
                    
                    foreach (Node dfsNode in dfsFromNode)
                    {
                        inputCopy.Remove(graph.GetNode(dfsNode.Guid));
                        inputCopy.Remove(dfsNode);
                    }
                    result.Add(dfsFromNode);

                    //foreach (KeyValuePair<int, Node> kvp in node.children)
                    //{
                    //    List<Node> dfsFromNode = DFS(kvp.Value, graph, inputCopy);
                    //    foreach (Node dfsNode in dfsFromNode)
                    //    {
                    //        inputCopy.Remove(dfsNode);
                    //    }
                    //    result.Add(dfsFromNode);
                    //}
                }
            }
            return result;
        }

        private List<Node> DFS_Children(Node node, AST Graph, List<Node> input)
        {
            List<Node> result = new List<Node>();
            if (input.Count == 0)
                return result;
            foreach (KeyValuePair<int, Node> kvp in node.children)
                result.AddRange(DFS_Children(kvp.Value, graph, input));
            //if (input.Contains(node))
            if (nodesToGuid(input).Contains(node.Guid))
                result.Add(node);
            return result;
        }

        private List<Node> DFS_Parent(Node node, AST graph, List<Node> input)
        {
            List<Node> result = new List<Node>();
            if (input.Count == 0)
                return result;
            //if (input.Contains(node))
            if (nodesToGuid(input).Contains(node.Guid))
                result.Add(node);
            foreach (Node parentNode in node.GetParents())
                result.AddRange(DFS_Parent(parentNode, graph, input));
            return result;
        }

        public List<uint> nodesToGuid(List<Node> inputList)
        {
            List<uint> result = new List<uint>();
            foreach (Node node in inputList)
            {
                result.Add(node.Guid);
            }
            return result;
        }
        #endregion

        #region new function for splitting a graph into connected subgraphs
        private List<List<Node>> SplitConnectedSubgraphs(List<Node> input, AST graph)
        {
            List<List<Node>> result = new List<List<Node>>();
            Dictionary<Node, bool> visitedMap = new Dictionary<Node, bool>();
            foreach (Node node in graph.nodeList)
            {
                visitedMap.Add(node, false);
            }
            return result;
        }

        private List<Node> BFS(Node node, AST graph, List<Node> input)
        {
            List<uint> UIDlist = this.nodesToGuid(input);
            List<Node> result = new List<Node>();
            Queue<Node> mainQ = new Queue<Node>();
            mainQ.Enqueue(node);
            result.Add(node);
            foreach (Node parentNode in node.GetParents())
            {
                if (UIDlist.Contains(parentNode.Guid))
                {
                    mainQ.Enqueue(parentNode);
                    if (!result.Contains(parentNode))
                    {
                        result.Add(parentNode);
                    }
                    input.Remove(parentNode);
                }
            }
            foreach (KeyValuePair<int, Node> childPair in node.children)
            {
                if (UIDlist.Contains(childPair.Value.Guid))
                {
                    mainQ.Enqueue(childPair.Value);
                    if (!result.Contains(childPair.Value))
                    {
                        result.Add(childPair.Value);
                    }
                    input.Remove(childPair.Value);
                }                
            }
            while (mainQ.Count != 0)
            {
                Node topNode = mainQ.Dequeue();
                foreach (Node parentNode in topNode.GetParents())
                {
                    if (!result.Contains(parentNode) && UIDlist.Contains(parentNode.Guid))
                    {
                        mainQ.Enqueue(parentNode);
                        if (UIDlist.Contains(parentNode.Guid))
                        {
                            result.Add(parentNode);
                            input.Remove(parentNode);
                        }
                    }
                }
                foreach (KeyValuePair<int, Node> sonNodePair in topNode.children)
                {
                    if (!result.Contains(sonNodePair.Value) && UIDlist.Contains(sonNodePair.Value.Guid))
                    {
                        mainQ.Enqueue(sonNodePair.Value);
                        if (UIDlist.Contains(sonNodePair.Value.Guid))
                        {
                            result.Add(sonNodePair.Value);
                            input.Remove(sonNodePair.Value);
                        }
                    }
                }
            }
            return result;
        }

        private List<Node> DFS(Node node, AST graph, List<Node> input)
        {
            List<Node> result = new List<Node>();
            //foreach (Node neighborNode in node.GetParents())
            for (int i=0; i<node.GetParents().Count; i++)
            {
                Node neighborNode = node.GetParents()[i];
                KeyValuePair<int, Node> item = neighborNode.children.FirstOrDefault(x => x.Value == node);
                {
                    neighborNode.children.Remove(item.Key);
                    //item.Value.GetParents().Remove(neighborNode);
                    item.Value.GetParents().RemoveAll(x => x == neighborNode);
                    result.AddRange(DFS(neighborNode, graph, input));
                    item.Value.GetParents().Add(neighborNode);
                    neighborNode.children.Add(item.Key, item.Value);
                }
            }
            //foreach (KeyValuePair<int, Node> neighborNode in node.children)
            for (int i=0; i<node.children.Count; i++)
            {
                KeyValuePair<int, Node> neighborNode = node.children.ElementAt(i);
                neighborNode.Value.GetParents().Remove(neighborNode.Value);
                List<Node> parentNodeList = new List<Node>(neighborNode.Value.GetParents());
                foreach (Node parentNode in parentNodeList)
                {
                    parentNode.RemoveChild(neighborNode.Value);
                }
                result.AddRange(DFS(neighborNode.Value, graph, input));
                foreach (Node parentNode in parentNodeList)
                {
                    parentNode.children.Add(neighborNode.Key, neighborNode.Value);
                }
                neighborNode.Value.GetParents().Add(neighborNode.Value);

            }
            result.Add(node);
            return result;
        }
        private List<List<Node>> GetConnectedComponents_02(List<Node> input, AST graph)
        {
            List<Node> inputCopy = new List<Node>(input);
            List<List<Node>> result = new List<List<Node>>();
            foreach (Node node in input)
            {
                if (inputCopy.Contains(node))
                {
                    List<Node> dfsFromNode = BFS(node, graph, inputCopy);                    
                    //dfsFromNode = new List<Node>(temp);

                    foreach (Node dfsNode in dfsFromNode)
                    {
                        inputCopy.Remove(graph.GetNode(dfsNode.Guid));
                        inputCopy.Remove(dfsNode);
                    }
                    // at this moment, im sure that input only contains nodes in graph, and by that i mean reference
                    List<uint> temp = new List<uint>();
                    foreach (Node bfsNode in dfsFromNode)
                    {
                        temp.Add(bfsNode.Guid);
                    }
                    dfsFromNode.Clear();
                    foreach (uint id in temp)
                    {
                        dfsFromNode.Add(graph.GetNode(id));
                    }
                    dfsFromNode = TopSort.sort(graph).Intersect<Node>(dfsFromNode).ToList();
                    result.Add(dfsFromNode);
                }
            }
            return result;
        }
        #endregion

        public SnapshotNode JoinNode(SnapshotNode nodeA, SnapshotNode nodeB)
        {
            SnapshotNode newNode = new SnapshotNode();
            newNode.Id = Math.Min(nodeA.Id, nodeB.Id);
            newNode.Type = SnapshotNodeType.CodeBlock;
            newNode.Content = nodeA.Content + nodeB.Content;
            newNode.InputList = nodeA.InputList.Union<Connection>(nodeB.InputList).ToList();
            newNode.OutputList = nodeA.OutputList.Union<Connection>(nodeB.OutputList).ToList();
            return newNode;
        }
        /*end*/

        public string GetGraphString()
        {
            return graph.ToScript();
        }

        /// <summary>
        /// Appends a newly added node into the ExecutionFlagList and setting it to initally true
        /// Updating the modifed nodes occur in UpdateDirtyFlags
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="ident"></param>
        private void HandleNewNode(Node node)
        {
            uint guid = node.Guid;
            string ident = string.Empty;
            if (node is Block)
            {
                // TODO Jun: We just want to check if this node is an identifier or a codeblock with a single identifier: [a;]
                // The string check will no longer be required once the graphIDE sends the relevant snapshot node data
                Block tblock = node as Block;
                ident = tblock.LHS;
                if (string.IsNullOrEmpty(ident))
                {
                    ident = (node as Block).TrimName();
                }
            }
            else
            {
                ident = GetVarName(guid);
            }

            if (ident.Equals(""))
            {
                return;
            }

            Validity.Assert(ident != null && ident.Length > 0);

            if (!ExecutionFlagList.Keys.Contains(ident))
            {
                // The node does not exist yet, add it and mark it as dirty
                bool executeFlag = true;
                #if TESTING_COMPARE
                if (node is Block)
                {
                    executeFlag = (node as Block).wasModified;
                }
                #endif
                ExecutionFlagList.Add(ident, executeFlag);
                AppendToStatementList(guid); 
                if (ExecutionFlagList[ident])
                {
                    AppendToStatementList(guid);
                }
                else
                {
                    RemoveFromStatementList(guid);
                }
            }
        }

        public void HandleNewNode(ProtoCore.AST.AssociativeAST.BinaryExpressionNode node)
        {
            uint guid = node.Guid;

            ProtoCore.AST.AssociativeAST.IdentifierNode identNode = node.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
            string ident = "";
            if (identNode != null)
            {
                ident = identNode.Value;
            }
            else if (node.LeftNode is ProtoCore.AST.AssociativeAST.ArrayIndexerNode)
            {
                ident = ((node.LeftNode as ProtoCore.AST.AssociativeAST.ArrayIndexerNode).Array as ProtoCore.AST.AssociativeAST.IdentifierNode).Value;
            }
            //Validity.Assert(identNode != null);
            //string ident = identNode.Value;

            Validity.Assert(!string.IsNullOrEmpty(ident));
            if (!ExecutionFlagList.Keys.Contains(ident))
            {
                // The node does not exist yet, add it and mark it as dirty
                ExecutionFlagList.Add(ident, true);
                AppendToStatementList(guid);
                if (ExecutionFlagList[ident])
                {
                    AppendToStatementList(guid);
                }
                else
                {
                    RemoveFromStatementList(guid);
                }
            }
        }

        /// <summary>
        /// This function handles all nodes in the modified guid list and appends them on the modified name list
        /// </summary>
        public void UpdateAddedNodesInModifiedNameList()
        {
            foreach (uint id in ModifiedStmtGuidList)
            {
                string key = GetVarName(id);
                if (!mapModifiedName.ContainsKey(key))
                {
                    mapModifiedName.Add(key, id);
                }
            }
        }

        /// <summary>
        /// This function removes nodes from the modified name list if they no longer exist in the modified guid list
        /// </summary>
        public void UpdateRemovedNodesInModifiedNameList(string varname)
        {
            if (mapModifiedName.ContainsKey(varname))
            {
                mapModifiedName.Remove(varname);
            }
        }

        private void AppendToStatementList(uint guid)
        {
            if (!ModifiedStmtGuidList.Contains(guid))
            {
                ModifiedStmtGuidList.Add(guid);
            }
        }

        /// <summary>
        /// This function handles nodes that must be removed from the list of nodes 
        /// This is called when a node becomes an island
        /// They need not be processed as they have no conncections (are islands)
        /// </summary>
        /// <param name="node"></param>
        private void HandleRemovedNode(Node node)
        {
            uint guid = node.Guid;
            string ident = GetVarName(guid);

            bool isValidCodeBlock = node is Block && ((Block)node).content != null;
            if (isValidCodeBlock)
            {
                // Comment Jun: Anything useful to do here?
                Validity.Assert(isValidCodeBlock && ident != null && ident.Length > 0);
            }

            if (ExecutionFlagList.Keys.Contains(ident))
            {
                // The node exisits, we want to remove it
                ExecutionFlagList.Remove(ident);
                RemoveFromStatementList(guid);
            }
        }


        private void RemoveFromStatementList(uint guid)
        {
            if (ModifiedStmtGuidList.Contains(guid))
            {
                ModifiedStmtGuidList.Remove(guid);
            }
        }

        private string BuildScriptFromIR(AST graph)
        {
            return "";
        }

        private uint GetCodeBlockParentUID(uint childCodeBlockUID)
        {
            //Block cbn = statementList.GetNode(childCodeBlockUID) as Block;
            Block cbn = graph.GetNode(childCodeBlockUID) as Block;
            Validity.Assert(null != cbn);
            return (cbn as Block).splitFomUint;
        }      

        /// <summary>
        /// Given a uid, get its associated uid used by the graphIDE
        /// The codeblock is an instance where each statement has a uid but the IDE only recognizes the main UID of the codeblock
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public uint GetRealUID(uint uid)
        {
            if (IsCodeBlock(uid))
            {
                return GetCodeBlockParentUID(uid);
            }
            return uid;
        }

        /*
        proc Rewrite(graph)
            foreach node in graph
	            if node is CodeBlock 
		            RewriteCodeBlock(node)
	            end
            end
        end
         */

        private void RewriteCodeBlock(AST graph)
        {
            List<int> nodesToRemove = new List<int>();
            List<uint> guidListOfNodesToRemove = new List<uint>();

            List<Node> rewrittenNodeList = new List<Node>();
            Dictionary<uint, Node> rewrittenUIDMap = new Dictionary<uint, Node>();

            foreach (Node node in graph.nodeList)
            {
                if (node != null)
                {
                    if (node is Block)
                    {
                        List<Block> listBlocks = RewriteCodeBlock(node as Block);
                        if (listBlocks != null && listBlocks.Count > 1)
                        {
                            // Add the new range of split blocks
                            //rewrittenNodeList.AddRange(listBlocks);

                            // Add the uids of every split block
                            foreach (Block toAdd in listBlocks)
                            {
                                if (rewrittenUIDMap.ContainsKey(toAdd.Guid))
                                {
                                    rewrittenNodeList.RemoveAll((x) => x.Guid == toAdd.Guid);
                                    rewrittenNodeList.Add(toAdd);
                                    rewrittenUIDMap[toAdd.Guid] = toAdd;
                                }
                                else
                                {
                                    rewrittenNodeList.Add(toAdd);
                                    rewrittenUIDMap.Add(toAdd.Guid, toAdd);
                                }
                            }
                        }
                        else
                        {
                            if (((Block)node).splitFomUint ==0)
                            ((Block)node).splitFomUint = node.Guid;
                            if (!rewrittenNodeList.Contains(node))
                            rewrittenNodeList.Add(node);
                            if (rewrittenUIDMap.ContainsKey(node.Guid)) 
                                rewrittenUIDMap[node.Guid] = node;
                            else
                            rewrittenUIDMap.Add(node.Guid, node);
                        }
                    }
                    else
                    {
                        if (!rewrittenNodeList.Contains(node))
                        rewrittenNodeList.Add(node);
                        if (rewrittenUIDMap.ContainsKey(node.Guid)) 
                            rewrittenUIDMap[node.Guid] = node;
                        else
                            rewrittenUIDMap.Add(node.Guid, node);
                    }
                }
            }

            graph.nodeList = new List<Node>(rewrittenNodeList);
            graph.nodeMap = new Dictionary<uint, Node>(rewrittenUIDMap);
        }

        /*
        proc RewriteCodeBlock(Node codeblock)
		
		    // Create new codeblocks for every line of code in the current codeblock CodeBlock[] newBlockList = new CodeBlock[node.lines.length]
            for n = 0 to node.lines.length
	            newBlockList[n].code = node.lines[n]
	            newBlockList[n].uid = generateUID(codeblock.uid)
            end
		
		    // At this point, determine which parents of the current codeblock node need to be connected to each splitted node

		    // Iterate through each parent of the current code block
		    foreach parentNode in codeblock.parents 

			    // Iterate through each child of the parent
			    for n = 0 to parentNode.children.length
			
				    // Check if the child is this codeblock
				    if parentNode.children[n] is equal to codeblock

                        // index ‘n’ is the current output slot of the codeblock		
                        newBlockList[n].parent.push(parentNode)

					    // Rewire the parent’s child to this new codeblock
					    parentNode.children[n] = newBlockList[n]

				    end
				    n++;
			    end
		    end			
        end

         */


        /*private List<Block> RewriteCodeBlock(Block codeblock)
        {
            Validity.Assert(codeblock != null);

            if (codeblock.Name == null || codeblock.Name.Length <= 0)
            {
                return null;
            }


            // Comment Jun: Im just trying to find the number of times ';' occurs
            // Make this more efficient by turning this into a function in a utils class
            string dummy = codeblock.Name.Replace(";", "");
            if ((codeblock.Name.Length - dummy.Length) <= 1)
            {
                // Single line codeblocks need not be split
                return null;
            }

            string[] token = new string[1];
            token[0] = ProtoCore.DSASM.Constants.termline;
            StringSplitOptions opt = new StringSplitOptions();
            string[] contents = codeblock.Name.Split(token, opt);
            int length = contents.Length;


            // The map of the new uid and its associated connecting slot
            Dictionary<int, uint> slotIndexToUIDMap = new Dictionary<int, uint>();

            // Create new codeblocks for every line of code in the current codeblock CodeBlock[] newBlockList = new CodeBlock[node.lines.length]
            List<Block> newBlockList = new List<Block>();
            for (int n = 0; n < length; n++)
            {
                // TODO Jun: Check with IDE why the semicolon is inconsitent
                string code = contents[n];
                if (code.Length > 0 && code[0] != ';')
                {
                    uint newGuid = GraphUtilities.GenerateUID();

                    List<AssignmentStatement> assignmentData = new List<AssignmentStatement>();
                    if (codeblock.assignmentData.Count > 0)
                    {
                        // This assignemnt data list must contain only 1 element
                        // This element is associated witht he current line in the codeblock
                        assignmentData.Add(codeblock.assignmentData[n]);
                    }
                    
                    Block newBlock = new Block(code, newGuid, assignmentData);
                    newBlock.splitFomUint = codeblock.Guid;
                    newBlockList.Add(newBlock);
                    slotIndexToUIDMap.Add(n, newGuid);
                }
            }

            // At this point, determine which parents of the current codeblock node need to be connected to each splitted node

            // Iterate through each parent of the current code block
            List<Node> parents = codeblock.GetParents();
            foreach (Node parentNode in parents)
            {
                // Iterate through each child of the parent
                for (int childIndex = 0; childIndex < parentNode.children.Count; childIndex++)
                {
                    Node child = null;
                    bool foundIndex = parentNode.children.TryGetValue(childIndex, out child);

                    if (foundIndex)
                    {
                        // Check if the child is this codeblock
                        if (child.Guid == codeblock.Guid)
                        {
                            int fromIndex = parentNode.childConnections[childIndex].from;

                            // Set the new codeblock's parent
                            newBlockList[fromIndex].AddParent(parentNode);

                            // Rewire the parent’s child to this new codeblock
                            parentNode.RemoveChild(childIndex);

                            parentNode.AddChild(newBlockList[fromIndex], childIndex, fromIndex);
                        }
                    }
                }
            }
            if (codeBlockUIDMap.ContainsKey(codeblock.Guid))
            {
                codeBlockUIDMap[codeblock.Guid] = slotIndexToUIDMap;
            }
            else
            {
                codeBlockUIDMap.Add(codeblock.Guid, slotIndexToUIDMap);
            }
            return newBlockList;
        }*/

        private List<Block> RewriteCodeBlock(Block codeblock)
        {
            Validity.Assert(codeblock != null);

            if (codeblock.Name == null || codeblock.Name.Length <= 0 )
            {
                return null;
            }

            // Comment Aparajit: This check is erroneous and should be removed
            // It's there since in some Nunit tests, comment strings are being passed in here
            string dummy = codeblock.Name.Replace(";", "");
            if ((codeblock.Name.Length - dummy.Length) <= 1)
            {
                return null;
            }

            //if (!codeblock.Name.EndsWith(";"))
              //  codeblock.Name += ";";

            List<ProtoCore.AST.Node> nodes = GraphUtilities.ParseCodeBlock(codeblock.Name);
            if (nodes.Count <= 1)
            {
                // Single line codeblocks need not be split
                return null;
            }

            int length = nodes.Count;


            // The map of the new uid and its associated connecting slot
            Dictionary<int, uint> slotIndexToUIDMap = new Dictionary<int, uint>();

            // Create new codeblocks for every line of code in the current codeblock CodeBlock[] newBlockList = new CodeBlock[node.lines.length]
            List<Block> newBlockList = new List<Block>();
            for (int n = 0; n < length; n++)
            {
                ProtoCore.AST.Node node = nodes[n];
                Validity.Assert(node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode);

                string code = ProtoCore.Utils.ParserUtils.ExtractStatementFromCode(codeblock.Name, node);
                if (code.Length > 0)
                {
                    uint newGuid = GraphUtilities.GenerateUID();

                    List<AssignmentStatement> assignmentData = new List<AssignmentStatement>();
                    if (codeblock.assignmentData.Count > 0)
                    {
                        // This assignemnt data list must contain only 1 element
                        // This element is associated witht he current line in the codeblock
                        assignmentData.Add(codeblock.assignmentData[n]);
                    }

                    Block newBlock = new Block(code, newGuid, assignmentData);
                    newBlock.splitFomUint = codeblock.Guid;
                    newBlockList.Add(newBlock);
                    slotIndexToUIDMap.Add(n, newGuid);
                }
            }

            // At this point, determine which parents of the current codeblock node need to be connected to each splitted node

            // Iterate through each parent of the current code block
            List<Node> parents = codeblock.GetParents();
            foreach (Node parentNode in parents)
            {
                // Iterate through each child of the parent
                for (int childIndex = 0; childIndex < parentNode.children.Count; childIndex++)
                {
                    Node child = null;
                    bool foundIndex = parentNode.children.TryGetValue(childIndex, out child);

                    if (foundIndex)
                    {
                        // Check if the child is this codeblock
                        if (child.Guid == codeblock.Guid)
                        {
                            int fromIndex = parentNode.childConnections[childIndex].from;

                            // Set the new codeblock's parent
                            newBlockList[fromIndex].AddParent(parentNode);

                            // Rewire the parent’s child to this new codeblock
                            parentNode.RemoveChild(childIndex);

                            parentNode.AddChild(newBlockList[fromIndex], childIndex, fromIndex);
                        }
                    }
                }
            }
            if (codeBlockUIDMap.ContainsKey(codeblock.Guid))
            {
                codeBlockUIDMap[codeblock.Guid] = slotIndexToUIDMap;
            }
            else
            {
                codeBlockUIDMap.Add(codeblock.Guid, slotIndexToUIDMap);
            }
            return newBlockList;
        }

        public static uint GenerateUID(uint temp,uint t)
        {
            uint f256 = t; // Right now wer're here.
            uint mask = f256 << 24;
            uint temptStList = temp| mask;
            return temptStList;
        }

        private string BuildScriptFromGraph(AST graph)
        {
            string liststat = "";
            //RewriteCodeBlock(graph);
            List<Node> li = TopSort.sort(graph);
            //li = SortCodeBlocks(li);
            tguid = 20000;
            //IEnumerable iter = li;
            List<string> listIslands = new List<string>();
            List<Node> islandNodes = new List<Node>();
            int countIslands = 0;
            statementList = new AST();
            ModifiedStmtGuidList.Clear();
            List<string> importIslands = new List<string>();
            IEnumerable iter = li;
            List<Node> finalScript = new List<Node>();
            List<List<Node>> listing = new List<List<Node>>();
            foreach (Node node in iter)
            {
                if (node != null)
                {
                    if (node is ImportNode)
                    {
                        importIslands.Add(node.ToScript() + ProtoCore.DSASM.Constants.termline);
                    }
                    else if (node.IsIsland)
                    {
                        countIslands++;
                        if (node is ArrayNode)
                        {
                            BuildArrayNodeStatement(node, statementList);
                            if (!islandNodes.Contains(node))
                                islandNodes.Add(node);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is LiteralNode)
                        {
                            BuildLiteralNodeStatement(node, statementList);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is Func)
                        {
                            BuildFunctionCallStatement(node, statementList);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is Operator)
                        {
                            BuildOperatorStatement(node, statementList);
                            string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        else if (node is Block)
                        {
                            BuildBlockStatement(node, statementList);
                            if (!islandNodes.Contains(node))
                            islandNodes.Add(node);
                            /*string island = statementList.GetNode(node.Guid).ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);*/
                        }
                        else if (node is IdentNode)
                        {
                            // comment Jun:
                            // An island identifier node is handled by emitting a null as its rhs
                            statementList.AddNode(node);
                            string contents = statementList.GetNode(node.Guid).ToScript() + "=" + ProtoCore.DSASM.Literal.Null + ProtoCore.DSASM.Constants.termline;
                            listIslands.Add(contents);
                        }
                        else
                        {
                            statementList.AddNode(node);
                            string island = node.ToScript() + ProtoCore.DSASM.Constants.termline;
                            if (!listIslands.Contains(island))
                                listIslands.Add(island);
                        }
                        HandleNewNode(node);
                    }
                    else if (node.IsLeaf)
                    {
                        if (node is ArrayNode)
                        {
                            BuildArrayNodeStatement(node, statementList);
                        }
                        else if (node is LiteralNode)
                        {
                            BuildLiteralNodeStatement(node, statementList);
                        }
                        else if (node is Func)
                        {
                            BuildFunctionCallStatement(node, statementList);
                        }
                        else if (node is Operator)
                        {
                            BuildOperatorStatement(node, statementList);
                        }
                        else if (node is Block)
                        {
                            BuildBlockStatement(node, statementList);
                        }
                        else if (node is IdentNode)
                        {
                            statementList.AddNode(node);
                            string contents = statementList.GetNode(node.Guid).ToScript() + "=" + ProtoCore.DSASM.Literal.Null + ProtoCore.DSASM.Constants.termline;
                            listIslands.Add(contents);
                        }
                        HandleNewNode(node);
                    }
                    else if (node.IsRoot && !node.IsIsland)
                    {
                        if (node is Operator)
                        {
                            BuildOperatorStatement(node, statementList);
                        }
                        else if (node is Func)
                        {
                            BuildFunctionCallStatement(node, statementList);
                        }
                        else if (node is Block)
                        {
                            BuildBlockStatement(node, statementList);
                        }
                        else if (node is IdentNode)
                        {
                            BuildIdentStatement(node, statementList);
                        }
                        //liststat = BuildStatement(node, statementList);
                        //finalScript=finalScript.Union(BuildStatement(node, statementList)).ToList();
                        
                        //comment out for new node to code
                        listing.Add(BuildStatement(node, statementList));
                        HandleNewNode(node);
                    }
                    else if (node is Operator)
                    {
                        BuildOperatorStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is Func)
                    {
                        BuildFunctionCallStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is ArrayNode)
                    {
                        BuildArrayNodeStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is LiteralNode)
                    {
                        BuildLiteralNodeStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is Block)
                    {
                        BuildBlockStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else if (node is IdentNode)
                    {
                        BuildIdentStatement(node, statementList);
                        HandleNewNode(node);
                    }
                    else
                    {
                        HandleNewNode(node);
                    }
                }
            }
            StringBuilder builder = new StringBuilder();
            foreach (string island in importIslands) // Loop through all strings
            {
                builder.Append(island); // Append string to StringBuilder
            }

            // nullify deleted variables or variables that undefined in this
            // run. 
            HashSet<string> nullifyVariables = new HashSet<string>(RemovedNodeNameList);
            nullifyVariables.UnionWith(this.UndefinedNameList);

            foreach (string variable in nullifyVariables)
            {
                if (!String.IsNullOrEmpty(variable))
                {
                    builder.Append(variable + " = null;\n");
                }
            }

            //for_taking_care_of_nulls
            const string tempNullAssign = Constants.kwTempNull + "=" + ProtoCore.DSDefinitions.Keyword.Null + ";\n";
            builder.Append(tempNullAssign);
            foreach (string island in listIslands) // Loop through all strings
            {
                builder.Append(island); // Append string to StringBuilder
            }

            foreach (List<Node> n1 in listing)
            {
                foreach (Node n2 in n1)
                {
                    if (!nodesToGuid(finalScript).Contains(n2.Guid))
                    {
                        finalScript.Add(n2);
                    }
                    else
                    {
                        finalScript.RemoveAll(x=>x.Guid == n2.Guid);
                        finalScript.Add(n2);
                    }
                }
            }

            finalScript = finalScript.Union(islandNodes).ToList();
            finalScript = SortCodeBlocks(finalScript);

            foreach (var value in finalScript)
            {
                liststat += value.ToScript() + ProtoCore.DSASM.Constants.termline;
            }
            liststat = builder.ToString() + liststat;
            //GraphUtilities.runningUID = GraphToDSCompiler.Constants.UIDStart;


            UpdateAddedNodesInModifiedNameList();

            return liststat;
        }

        private void BuildIdentStatement(Node node, AST statementList)
        {
            IdentNode identNode = node as IdentNode;
            Assignment a = new Assignment(identNode, identNode);
            statementList.AddNode(a);
        }

        private List<Node> SortCodeBlocks(List<Node> list)
        {
            List<Node> li = new List<Node>();
            Dictionary<int, uint> d=new Dictionary<int,uint>();
            foreach (Node n in list)
            {
                if (n is Block)
                {
                    codeBlockUIDMap.TryGetValue(((Block)n).splitFomUint, out d);
                    List<uint> uints = new List<uint>();
                    if (d != null && d.Count != 0)
                    {
                        uints = new List<uint>(d.Values);
                        foreach (uint id in uints)
                        {
                            Node n1 = list.FirstOrDefault(item => item.Guid == id);
                            if (!li.Contains(n1) && n1 != null)
                            {
                                li.Add(n1);
                            }
                        }
                    }
                    if (!li.Contains(n))
                    {
                        li.Add(n);
                    }
                }
                else
                {
                    li.Add(n);
                }
            }
            return li;
        }

        public string BuildScript(AST graph)
        {
            string statements = "";
            if (usePatternRewrite)
            {/*
                // Transform the graph into the graph IR
                GraphTransform graphTransform = new GraphTransform(graph);
                graph = graphTransform.ToFinalGraph();
                statements = BuildScriptFromIR(graph);*/
            }
            else
            {
                statements = BuildScriptFromGraph(graph);
            }
            return statements;
        }

        private void BuildBlockStatement(Node node, AST statementList)
        {
            if (node.children.Count > 1 || node.IsLeaf || node.IsIsland)
            {
                statementList.AddNode(node);
            }
            else
            {
                Block n = node as Block;
                Node a;

                // TODO Jun: why is this check needed?

                // This checks if the current node is a full expression
                // If a codeblock contains LHS then it is a full expression
                if (!string.IsNullOrEmpty(n.LHS))
                {
                    // This is a full expression
                    // such as: [a = 2;]
                    a = new Assignment(new IdentNode(n.LHS, node.Guid), new IdentNode(n.LHS, node.Guid));
                }
                else
                {   
                    // This is a single identifier
                    // such as: [a;]
                    string s = n.Name;
                    s = s.TrimEnd(';');
                    a = new Assignment(new IdentNode(s, node.Guid), new IdentNode(s, node.Guid));
                }
                statementList.AddNode(a);
            }
        }
        private void BuildLiteralNodeStatement(Node node, AST statementList)
        {
            LiteralNode n = (LiteralNode)node;
            tguid = GetTempGUID(n.Guid);
            string name = GraphToDSCompiler.kw.tempPrefix + tguid;
            n.tempName = name;
            Node a = new Assignment(new IdentNode(name, tguid), n);
            statementList.AddNode(a);
        }

        private void BuildArrayNodeStatement(Node node, AST statementList)
        {
            Expr e = (Expr)node;
            tguid = GetTempGUID(e.Guid);

            Assignment a = new Assignment(new IdentNode(GraphToDSCompiler.kw.tempPrefix + tguid, tguid), e);
            statementList.AddNode(a);
        }
        private void BuildFunctionCallStatement(Node node, AST statementList)
        {
            Func n = (Func)node;
            tguid = GetTempGUID(n.Guid);
            FunctionCall f = new FunctionCall(n);
            Assignment a = new Assignment(new IdentNode(n.tempName, tguid), f);
            statementList.AddNode(a);
        }

        private void BuildOperatorStatement(Node node, AST statementList)
        {
            Operator n = (Operator)node;
            Expr e1 = null;
            Expr e2 = null;

            tguid = GetTempGUID(n.Guid);

            BinExprNode b = new BinExprNode(e1, n, e2);

            Assignment a = new Assignment(new IdentNode(n.tempName, tguid), b);


            statementList.AddNode(a);
        }
        
        public static uint GetTempGUID(uint tempt)
        {
            return (GraphToDSCompiler.Constants.TempMask | tempt);
        }
        
        List<Node> BuildStatement(Node node, AST statementList)
        {
            //statementList.RemoveNode(node);
            List<Node> l = TopSort.sort(node, statementList);
            //if (node is Block)
            //{
            //    
            //    statementList.AddNode(node);
            //}
            List<uint> guid = new List<uint>();
            IEnumerable iter = l;
            string liststat = "";
            foreach (Node nodus in iter)
            {
                if (nodus != null)
                {
                    guid.Add(nodus.Guid);
                }
            }

            List<Node> nodes = new List<Node>(statementList.GetNodes());
            foreach (var value in nodes)
            {
                liststat += value.ToScript() + ProtoCore.DSASM.Constants.termline;
            }

            return nodes;
        }
        public bool IsCodeBlock(uint id)
        {
            return (graph.GetNode(id) is Block); 
        }

        private GraphCompiler()
        {
            // Making the constructor private so we can't "new" it directly.
            ExecutionFlagList = new Dictionary<string, bool>();
            ModifiedStmtGuidList = new List<uint>();
            RemovedNodeNameList = new List<string>();
            UndefinedNameList = new List<string>();
            mapModifiedName = new Dictionary<string, uint>();
        }

        public void SetCore(ProtoCore.Core core)
        {
            Validity.Assert(null == this.core);
            this.core = core;
        }

        /// <summary>
        /// return the number of nodes on UI
        /// </summary>
        /// <returns></returns>
        public int GetNumUINodes()
        {
            int count = graph.nodeList.Count;
            foreach (KeyValuePair<uint, Dictionary<int, uint>> kvp in codeBlockUIDMap)
            {
                foreach (KeyValuePair<int, uint> kvp2 in kvp.Value)
                {
                    if (graph.nodeMap.ContainsKey(kvp2.Value))
                    {
                        count = count - kvp.Value.Count + 1;
                        break;
                    }
                }
            }
            foreach (Node node in graph.nodeList)
            {
                if (node is ImportNode)
                    count -= 1;
            }
            return count;
        }

        static void Main(string[] args)
        {
        }


        
    }
}