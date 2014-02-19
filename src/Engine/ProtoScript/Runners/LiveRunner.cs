using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GraphToDSCompiler;
using ProtoCore.DSASM.Mirror;
using System.Diagnostics;
using ProtoCore.Utils;
using System.ComponentModel;
using System.Threading;
using ProtoFFI;
using ProtoCore.AssociativeGraph;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using System.Linq;
using ProtoCore.DSASM;

namespace ProtoScript.Runners
{
    public enum EventStatus
    {
        OK,
        Error,
        Warning
    }

    /// <summary>
    /// A subtree represents a node in graph. It contains a list of AST node.
    /// </summary>
    public struct Subtree
    {
        public Guid GUID;
        public List<AssociativeNode> AstNodes;

        public Subtree(List<AssociativeNode> astNodes, System.Guid guid)
        {
            GUID = guid;
            AstNodes = astNodes;
        }
    }

    /// <summary>
    /// GraphSyncData contains three lists: Subtrees that are added, modified
    /// and deleted in a session.
    /// </summary>
    public class GraphSyncData
    {
        public List<Subtree> DeletedSubtrees
        {
            get;
            private set;
        }

        public List<Subtree> AddedSubtrees
        {
            get;
            private set;
        }

        public List<Subtree> ModifiedSubtrees
        {
            get;
            private set;
        }

        public GraphSyncData(List<Subtree> deleted, List<Subtree> added, List<Subtree> modified)
        {
            DeletedSubtrees = deleted;
            AddedSubtrees = added;
            ModifiedSubtrees = modified;
        }
    }

    public interface ILiveRunner
    {
        ProtoCore.Core Core { get; }

        #region Synchronous call
        void UpdateGraph(GraphSyncData syncData);
        void UpdateCmdLineInterpreter(string code);
        ProtoCore.Mirror.RuntimeMirror QueryNodeValue(Guid nodeId);
        ProtoCore.Mirror.RuntimeMirror InspectNodeValue(string nodeName);

        void UpdateGraph(AssociativeNode astNode);
        #endregion

        #region Asynchronous call
        void BeginUpdateGraph(GraphSyncData syncData);
        void BeginConvertNodesToCode(List<Subtree> subtrees);
        void BeginQueryNodeValue(Guid nodeGuid);
        void BeginQueryNodeValues(List<Guid> nodeGuid);
        #endregion
        
        string GetCoreDump();
        void ResetVMAndResyncGraph(List<string> libraries);
        List<LibraryMirror> ResetVMAndImportLibrary(List<string> libraries);
		void ReInitializeLiveRunner();

        // Event handlers for the notification from asynchronous call
        event NodeValueReadyEventHandler NodeValueReady;
        event GraphUpdateReadyEventHandler GraphUpdateReady;
        event NodesToCodeCompletedEventHandler NodesToCodeCompleted;
        
    }

    public partial class LiveRunner : ILiveRunner, IDisposable
    {
        /// <summary>
        ///  These are configuration parameters passed by host application to be consumed by geometry library and persistent manager implementation. 
        /// </summary>
        public class Options
        {
            /// <summary>
            /// The configuration parameters that needs to be passed to
            /// different applications.
            /// </summary>
            public Dictionary<string, object> PassThroughConfiguration;

            /// <summary>
            /// The path of the root graph/module
            /// </summary>
            public string RootModulePathName;

            /// <summary>
            /// List of search directories to resolve any file reference
            /// </summary>
            public List<string> SearchDirectories;

            /// <summary>
            /// If the Interpreter mode is true, the LiveRunner takes in code statements as input strings
            /// and not SyncData
            /// </summary>
            public bool InterpreterMode = false;
        }

        private void ResetModifiedSymbols()
        {
            this.runnerCore.Rmem.ResetModifedSymbols();
        }

        private ProtoScriptTestRunner runner;
        private ProtoRunner.ProtoVMState vmState;
        private GraphToDSCompiler.GraphCompiler graphCompiler;
        private ProtoCore.Core runnerCore = null;
        public ProtoCore.Core Core
        {
            get
            {
                return runnerCore;
            }
            private set
            {
                runnerCore = value;
            }
        }

        private ProtoCore.Options coreOptions = null;
        private Options executionOptions = null;
        private bool syncCoreConfigurations = false;
        private int deltaSymbols = 0;
        private bool isPreloadingLibraries = false;
        private int codeBlockCount = 0;
        private ProtoCore.CompileTime.Context staticContext = null;

        private Dictionary<System.Guid, Subtree> currentSubTreeList = null;

        private readonly Object operationsMutex = new object();

        private Queue<Task> taskQueue;

        private Thread workerThread;

        private bool terminating;

        public LiveRunner()
        {
            InitRunner(new Options());
        }

        public LiveRunner(Options options)
        {
            InitRunner(options);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (runnerCore != null)
                {
                    runnerCore.FFIPropertyChangedMonitor.FFIPropertyChangedEventHandler -= FFIPropertyChanged;
                    runnerCore.Cleanup();
                }

                terminating = true;

                lock (taskQueue)
                {
                    taskQueue.Clear();
                }

                // waiting for thread to finish
                if (workerThread.IsAlive)
                {
                    workerThread.Join();
                }
            }
        }

        private void InitRunner(Options options)
        {
            graphCompiler = GraphToDSCompiler.GraphCompiler.CreateInstance();
            graphCompiler.SetCore(GraphUtilities.GetCore());
            runner = new ProtoScriptTestRunner();

            executionOptions = options;
            InitOptions();
            InitCore();


            taskQueue = new Queue<Task>();

            workerThread = new Thread(new ThreadStart(TaskExecMethod));


            workerThread.IsBackground = true;
            workerThread.Start();

            staticContext = new ProtoCore.CompileTime.Context();

            currentSubTreeList = new Dictionary<Guid, Subtree>();

            terminating = false;
        }

        private void InitOptions()
        {

            // Build the options required by the core
            Validity.Assert(coreOptions == null);
            coreOptions = new ProtoCore.Options();
            coreOptions.GenerateExprID = true;
            coreOptions.IsDeltaExecution = true;
            coreOptions.BuildOptErrorAsWarning = true;

            coreOptions.WebRunner = false;
            coreOptions.ExecutionMode = ProtoCore.ExecutionMode.Serial;

            // This should have been set in the consturctor
            Validity.Assert(executionOptions != null);
        }

        private void InitCore()
        {
            Validity.Assert(coreOptions != null);

            // Comment Jun:
            // It must be guaranteed that in delta exeuction, expression id's must not be autogerated
            // expression Id's must be propagated from the graphcompiler to the DS codegenerators
            //Validity.Assert(coreOptions.IsDeltaExecution && !coreOptions.GenerateExprID);

            runnerCore = new ProtoCore.Core(coreOptions);

            SyncCoreConfigurations(runnerCore, executionOptions);


            runnerCore.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(runnerCore));
            runnerCore.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(runnerCore));
            runnerCore.FFIPropertyChangedMonitor.FFIPropertyChangedEventHandler += FFIPropertyChanged;
            vmState = null;
        }

        private void FFIPropertyChanged(FFIPropertyChangedEventArgs arg)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(new PropertyChangedTask(this, arg.hostGraphNode));
            }
        }

        private static void SyncCoreConfigurations(ProtoCore.Core core, Options options)
        {
            if (null == options)
                return;
            //update the root module path name, if set.
            if (!string.IsNullOrEmpty(options.RootModulePathName))
                core.Options.RootModulePathName = options.RootModulePathName;
            //then update the search path, if set.
            if (null != options.SearchDirectories)
                core.Options.IncludeDirectories = options.SearchDirectories;

            //Finally update the pass thru configuration values
            if (null == options.PassThroughConfiguration)
                return;
            foreach (var item in options.PassThroughConfiguration)
            {
                core.Configurations[item.Key] = item.Value;
            }
        }


        public void SetOptions(Options options)
        {
            executionOptions = options;
            syncCoreConfigurations = true; //request syncing the configuration
        }


        #region Public Live Runner Events

        public event NodeValueReadyEventHandler NodeValueReady = null;
        public event GraphUpdateReadyEventHandler GraphUpdateReady = null;
        public event NodesToCodeCompletedEventHandler NodesToCodeCompleted = null;

        #endregion

        public void BeginUpdateGraph(GraphSyncData syncData)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(new UpdateGraphTask(syncData, this));
            }
        }

        /// <summary>
        /// Async call from command-line interpreter to LiveRunner
        /// </summary>
        /// <param name="cmdLineString"></param>
        public void BeginUpdateCmdLineInterpreter(string cmdLineString)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new UpdateCmdLineInterpreterTask(cmdLineString, this));
            }
        }

        public void BeginConvertNodesToCode(List<Subtree> subtrees)
        {
            if (null == subtrees || (subtrees.Count <= 0))
                return; // Do nothing, there's no nodes to be converted.

            lock (taskQueue)
            {
                taskQueue.Enqueue(new ConvertNodesToCodeTask(subtrees, this));
            }
        }

        public void BeginQueryNodeValue(Guid nodeGuid)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new NodeValueRequestTask(nodeGuid, this));
            }
        }

        public void BeginQueryNodeValues(List<Guid> nodeGuids)
        {
            lock (taskQueue)
            {
                foreach (Guid nodeGuid in nodeGuids)
                {
                    taskQueue.Enqueue(
                        new NodeValueRequestTask(nodeGuid, this));
                }
            }
        }

        /// <summary>
        /// Query for a node value given its UID. This will block until the value is available.
        /// This uses the expression interpreter to evaluate a node variable's value.
        /// It will only serviced when all ASync calls have been completed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public ProtoCore.Mirror.RuntimeMirror QueryNodeValue(Guid nodeGuid)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {

                        //No entries and we have the lock
                        //Synchronous query to get the node

                        return InternalGetNodeValue(nodeGuid);
                    }
                }
                Thread.Sleep(0);
            }

        }



        /// <summary>
        /// Inspects the VM for the value of a node given its variable name. 
        /// As opposed to QueryNodeValue, this does not use the Expression Interpreter
        /// This will block until the value is available.
        /// It will only serviced when all ASync calls have been completed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        ///
        public ProtoCore.Mirror.RuntimeMirror InspectNodeValue(string nodeName)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        //return GetWatchValue(nodeName);
                        const int blockID = 0;
                        ProtoCore.Mirror.RuntimeMirror runtimeMirror = ProtoCore.Mirror.Reflection.Reflect(nodeName, blockID, runnerCore);
                        return runtimeMirror;
                    }
                }
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// VM Debugging API for general Debugging purposes 
        /// temporarily used by Cmmand Line REPL
        /// </summary>
        /// <returns></returns>
        public string GetCoreDump()
        {
            // Prints out the final Value of every symbol in the program
            // Traverse order:
            //  Exelist, Globals symbols

            StringBuilder globaltrace = null;

            ProtoCore.DSASM.Executive exec = runnerCore.CurrentExecutive.CurrentDSASMExec;
            ProtoCore.DSASM.Mirror.ExecutionMirror execMirror = new ProtoCore.DSASM.Mirror.ExecutionMirror(exec, runnerCore);
            ProtoCore.DSASM.Executable exe = exec.rmem.Executable;

            // Only display symbols defined in the default top-most langauge block;
            // Otherwise garbage information may be displayed.
            string formattedString = string.Empty;
            if (exe.runtimeSymbols.Length > 0)
            {
                int blockId = 0;

                ProtoCore.DSASM.SymbolTable symbolTable = exe.runtimeSymbols[blockId];

                for (int i = 0; i < symbolTable.symbolList.Count; ++i)
                {
                    //int n = symbolTable.symbolList.Count - 1;
                    //formatParams.ResetOutputDepth();
                    ProtoCore.DSASM.SymbolNode symbolNode = symbolTable.symbolList[i];

                    bool isLocal = ProtoCore.DSASM.Constants.kGlobalScope != symbolNode.functionIndex;
                    bool isStatic = (symbolNode.classScope != ProtoCore.DSASM.Constants.kInvalidIndex && symbolNode.isStatic);
                    if (symbolNode.isArgument || isLocal || isStatic || symbolNode.isTemp)
                    {
                        // These have gone out of scope, their values no longer exist
                        //return ((null == globaltrace) ? string.Empty : globaltrace.ToString());
                        continue;
                    }

                    ProtoCore.Runtime.RuntimeMemory rmem = exec.rmem;
                    StackValue sv = rmem.GetStackData(blockId, i, ProtoCore.DSASM.Constants.kGlobalScope);
                    formattedString = formattedString + string.Format("{0} = {1}\n", symbolNode.name, execMirror.GetStringValue(sv, rmem.Heap, blockId));

                    //if (null != globaltrace)
                    //{
                    //    int maxLength = 1020;
                    //    while (formattedString.Length > maxLength)
                    //    {
                    //        globaltrace.AppendLine(formattedString.Substring(0, maxLength));
                    //        formattedString = formattedString.Remove(0, maxLength);
                    //    }

                    //    globaltrace.AppendLine(formattedString);
                    //}
                }

                //formatParams.ResetOutputDepth();
            }

            //return ((null == globaltrace) ? string.Empty : globaltrace.ToString());
            return formattedString;
        }

        /// <summary>
        /// This API needs to be called for every delta AST execution
        /// </summary>
        /// <param name="syncData"></param>
        public void UpdateGraph(GraphSyncData syncData)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    if (taskQueue.Count == 0)
                    {
                        SynchronizeInternal(syncData);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Called for delta execution of AST node input
        /// </summary>
        /// <param name="astNode"></param>
        public void UpdateGraph(AssociativeNode astNode)
        {
            CodeBlockNode cNode = astNode as CodeBlockNode;
            if (cNode != null)
            {
                List<AssociativeNode> astList = cNode.Body;
                List<Subtree> addedList = new List<Subtree>();
                addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
                GraphSyncData syncData = new GraphSyncData(null, addedList, null);

                UpdateGraph(syncData);
            }
            else if (astNode is AssociativeNode)
            {
                List<AssociativeNode> astList = new List<AssociativeNode>();
                astList.Add(astNode);
                List<Subtree> addedList = new List<Subtree>();
                addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
                GraphSyncData syncData = new GraphSyncData(null, addedList, null);

                UpdateGraph(syncData);
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// This api needs to be called by a command line REPL for each DS command/expression entered to be executed
        /// </summary>
        /// <param name="code"></param>
        public void UpdateCmdLineInterpreter(string code)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        SynchronizeInternal(code);
                        return;
                    }
                }
                Thread.Sleep(0);
            }
        }

        //Secondary thread
        private void TaskExecMethod()
        {
            while (!terminating)
            {
                Task task = null;

                lock (taskQueue)
                {
                    if (taskQueue.Count > 0)
                        task = taskQueue.Dequeue();
                }

                if (task != null)
                {
                    task.Execute();
                    continue;

                }

                Thread.Sleep(50);
            }
        }



        #region Internal Implementation

        private ProtoCore.Mirror.RuntimeMirror GetWatchValue(string varname)
        {
            runnerCore.Options.IsDeltaCompile = true;
            CompileAndExecuteForDeltaExecution(GraphUtilities.GetWatchExpression(varname));

            const int blockID = 0;
            ProtoCore.Mirror.RuntimeMirror runtimeMirror = ProtoCore.Mirror.Reflection.Reflect(ProtoCore.DSASM.Constants.kWatchResultVar, blockID, runnerCore);
            return runtimeMirror;
                
        }
       

        /// <summary>
        /// This is being called currently as it uses the Expression interpreter which does not
        /// work well with delta execution. Instead we are currently inspecting into the VM using Mirrors
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(string varname)
        {
            Validity.Assert(null != vmState);

            // Comment Jun: all symbols are in the global block as there is no notion of scoping the the graphUI yet.
            const int blockID = 0;

            return vmState.LookupName(varname, blockID);
        }

        private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(Guid nodeGuid)
        {
            throw new NotImplementedException();
        }


        private bool Compile(string code, out int blockId)
        {
            Dictionary<string, bool> execFlagList = null;
            if (graphCompiler != null)
                execFlagList = graphCompiler.ExecutionFlagList;

            staticContext.SetData(code, new Dictionary<string, object>(), execFlagList);

            bool succeeded = runner.Compile(staticContext, runnerCore, out blockId);
            if (succeeded)
            {
                // Regenerate the DS executable
                runnerCore.GenerateExecutable();

                // Update the symbol tables
                // TODO Jun: Expand to accomoadate the list of symbols
                //staticContext.symbolTable = runnerCore.DSExecutable.runtimeSymbols[0];
            }
            return succeeded;
        }

        private bool Compile(List<AssociativeNode> astList, out int blockId)
        {
            bool succeeded = runner.Compile(astList, runnerCore, out blockId);
            if (succeeded)
            {
                // Regenerate the DS executable
                runnerCore.GenerateExecutable();

                // Update the symbol tables
                // TODO Jun: Expand to accomoadate the list of symbols
                staticContext.symbolTable = runnerCore.DSExecutable.runtimeSymbols[0];
            }
            return succeeded;
        }


        private ProtoRunner.ProtoVMState Execute()
        {
            // runnerCore.GlobOffset is the number of global symbols that need to be allocated on the stack
            // The argument to Reallocate is the number of ONLY THE NEW global symbols as the stack needs to accomodate this delta
            int newSymbols = runnerCore.GlobOffset - deltaSymbols;

            // If there are lesser symbols to allocate for this run, then it means nodes were deleted.
            // TODO Jun: Determine if it is safe to just leave them in the global stack 
            //           as no symbols point to this memory location in the stack anyway
            if (newSymbols >= 0)
            {
                runnerCore.Rmem.ReAllocateMemory(newSymbols);
            }

            // Store the current number of global symbols
            deltaSymbols = runnerCore.GlobOffset;

            // Initialize the runtime context and pass it the execution delta list from the graph compiler
            ProtoCore.Runtime.Context runtimeContext = new ProtoCore.Runtime.Context();

            if (graphCompiler != null)
                runtimeContext.execFlagList = graphCompiler.ExecutionFlagList;

            runner.Execute(runnerCore, runtimeContext);

            // ExecutionMirror mirror = new ExecutionMirror(runnerCore.CurrentExecutive.CurrentDSASMExec, runnerCore);

            return new ProtoRunner.ProtoVMState(runnerCore);
        }

        private bool CompileAndExecute(string code)
        {
            // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(code, out blockId);
            if (succeeded)
            {
                runnerCore.RunningBlock = blockId;
                vmState = Execute();
            }
            return succeeded;
        }

        private bool CompileAndExecute(List<AssociativeNode> astList)
        {
            // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(astList, out blockId);
            if (succeeded)
            {
                runnerCore.RunningBlock = blockId;
                vmState = Execute();
            }
            return succeeded;
        }

        private void ResetVMForExecution()
        {
            runnerCore.ResetForExecution();
        }

        private void ResetVMForDeltaExecution()
        {
            runnerCore.ResetForDeltaExecution();
        }

        /// <summary>
        /// Resets few states in the core to prepare the core for a new
        /// delta code compilation and execution
        /// </summary>
        private void ResetForDeltaASTExecution()
        {
            runnerCore.ResetForDeltaASTExecution();
        }

        /// <summary>
        /// This function resets properties in LiveRunner core and compileStateTracker required in preparation for a subsequent run
        /// </summary>
        private void RetainVMStatesForDeltaExecution()
        {
            var cblist = runnerCore.CompleteCodeBlockList;

            if (isPreloadingLibraries)
            {
                codeBlockCount = cblist.Count;
            }
            else
            {
                // In normal delta execution. Need to remove extra code blocks
                // that added in this execution.
                if (codeBlockCount > 0)
                {
                    int count = cblist.Count - codeBlockCount;
                    if (count > 0)
                    {
                        cblist.RemoveRange(codeBlockCount, count);
                    }
                }
                else
                {
                    cblist.Clear();
                }
            }
        }

        /// <summary>
        /// Compiles and executes input script in delta execution mode
        /// </summary>
        /// <param name="code"></param>
        private void CompileAndExecuteForDeltaExecution(string code)
        {
            if (coreOptions.Verbose)
            {
                System.Diagnostics.Debug.WriteLine("SyncInternal => " + code);
            }

            ResetForDeltaASTExecution();
            bool succeeded = CompileAndExecute(code);

            if (succeeded)
            {
                RetainVMStatesForDeltaExecution();
            }
        }

        private void CompileAndExecuteForDeltaExecution(List<AssociativeNode> astList)
        {
            if (coreOptions.Verbose)
            {
                string code = DebugCodeEmittedForDeltaAst(astList);
                System.Diagnostics.Debug.WriteLine(code);
            }

            ResetForDeltaASTExecution();
            bool succeeded = CompileAndExecute(astList);

            if (succeeded)
            {
                RetainVMStatesForDeltaExecution();
            }
        }

        private List<AssociativeNode> GetASTNodesDependentOnFunctionList(FunctionDefinitionNode functionNode)
        {
            // Determine if the modified function was used in any of the current nodes
            List<AssociativeNode> modifiedNodes = new List<AssociativeNode>();

            // Iterate through the vm graphnodes at the global scope that contain a function call
            //foreach (ProtoCore.AssociativeGraph.GraphNode gnode in runnerCore.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
            Validity.Assert(null != runnerCore.GraphNodeCallList);
            foreach (ProtoCore.AssociativeGraph.GraphNode gnode in runnerCore.GraphNodeCallList)
            {
                if (gnode.isActive)
                {
                    // Iterate through the current ast nodes 
                    foreach (KeyValuePair<System.Guid, Subtree> kvp in currentSubTreeList)
                    {
                        foreach (AssociativeNode assocNode in kvp.Value.AstNodes)
                        {
                            if (assocNode is BinaryExpressionNode)
                            {
                                if (gnode.exprUID == (assocNode as BinaryExpressionNode).exprUID)
                                {
                                    // Check if the procedure associatied with this graphnode matches thename and arg count of the modified proc
                                    if (null != gnode.firstProc)
                                    {
                                        if (gnode.firstProc.name == functionNode.Name
                                            && gnode.firstProc.argInfoList.Count == functionNode.Signature.Arguments.Count)
                                        {
                                            // If it does, create a new ast tree for this graphnode and append it to deltaAstList
                                            modifiedNodes.Add(assocNode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return modifiedNodes;
        }

        /// <summary>
        /// Gets the only the modified nodes from the subtree by checking of the previous cached instance
        /// </summary>
        /// <param name="subtree"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetModifiedNodes(Subtree subtree)
        {
            Subtree st;
            if (!currentSubTreeList.TryGetValue(subtree.GUID, out st) || st.AstNodes == null)
            {
                // If the subtree was not cached, it means the cache was delted
                // This means the current subtree is all modified
                return subtree.AstNodes;
            }

            // We want to process only modified statements
            // If the AST is identical to an existing AST in the same GUID, it means it was not modified
            List<AssociativeNode> modifiedASTList = new List<AssociativeNode>();
            foreach (AssociativeNode node in subtree.AstNodes)
            {
                // Check if node exists in the prev AST list
                bool nodeFound = false;
                foreach (AssociativeNode prevNode in st.AstNodes)
                {
                    if (prevNode.Equals(node))
                    {
                        nodeFound = true;
                        break;
                    }
                }

                if (!nodeFound)
                {
                    // node is modifed as it does not match any existing
                    modifiedASTList.Add(node);
                }
            }
            return modifiedASTList;
        }


        /// <summary>
        /// Get the ASTs from the previous list that no longer exist in the new list
        /// </summary>
        /// <param name="prevASTList"></param>
        /// <param name="newASTList"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetInactiveASTList(List<AssociativeNode> prevASTList, List<AssociativeNode> newASTList)
        {
            List<AssociativeNode> removedList = new List<AssociativeNode>();
            foreach (AssociativeNode prevNode in prevASTList)
            {
                bool prevNodeFoundInNewList = false;
                foreach (AssociativeNode newNode in newASTList)
                {
                    if (prevNode.Equals(newNode))
                    {
                        // prev node still exists in the new list
                        prevNodeFoundInNewList = true;
                        break;
                    }
                }

                if (!prevNodeFoundInNewList)
                {
                    removedList.Add(prevNode);
                }
            }
            return removedList;
        }

        /// <summary>
        /// Get the ASTs from the previous list that that still exist in the new list
        /// </summary>
        /// <param name="prevASTList"></param>
        /// <param name="newASTList"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetUnmodifiedASTList(List<AssociativeNode> prevASTList, List<AssociativeNode> newASTList)
        {
            List<AssociativeNode> existingList = new List<AssociativeNode>();
            foreach (AssociativeNode prevNode in prevASTList)
            {
                foreach (AssociativeNode newNode in newASTList)
                {
                    if (prevNode.Equals(newNode))
                    {
                        existingList.Add(prevNode);
                        break;
                    }
                }
            }
            return existingList;
        }

        /// <summary>
        /// Takes in a Subtree to delete or modify and marks the corresponding gragh nodes in DS inactive.
        /// This is equivalent to removing them from the VM
        /// </summary>
        /// <param name="subtree"></param>
        /// <returns></returns>
        private List<AssociativeNode> MarkGraphNodesInactive(List<AssociativeNode> modifiedASTList)
        {
            List<AssociativeNode> astNodeList = new List<AssociativeNode>();
            if (null != modifiedASTList && modifiedASTList.Count > 0)
            {
                foreach (var node in modifiedASTList)
                {
                    BinaryExpressionNode bNode = node as BinaryExpressionNode;
                    if (bNode != null)
                    {
                        BinaryExpressionNode newBNode = new BinaryExpressionNode(bNode);
                        // TODO: Aparajit - this can be made more efficient by maintaining a map in core of 
                        // graphnode vs expression UID 
                        foreach (var gnode in runnerCore.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
                        {
                            if (gnode.exprUID == bNode.exprUID)
                            {
                                gnode.isActive = false;
                            }
                        }
                        newBNode.RightNode = new NullNode();
                        astNodeList.Add(newBNode);
                    }
                }
            }
            return astNodeList;
        }


        private void DeactivateGraphnodes(List<AssociativeNode> nodeList)
        {
            if (null != nodeList)
            {
                foreach (var node in nodeList)
                {
                    BinaryExpressionNode bNode = node as BinaryExpressionNode;
                    if (bNode != null)
                    {
                        foreach (var gnode in runnerCore.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
                        {
                            if (gnode.exprUID == bNode.exprUID)
                            {
                                gnode.isActive = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the cached AST's if they were modified
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="modifiedNodes"></param>
        private void UpdateCachedSubtree(Guid guid, List<AssociativeNode> modifiedNodes)
        {
            if (null != modifiedNodes && modifiedNodes.Count > 0)
            {
                List<AssociativeNode> cachedASTList = currentSubTreeList[guid].AstNodes;
                foreach (AssociativeNode node in modifiedNodes)
                {
                    // Remove ast from cachedASTList if the current node matches an exprID
                    // cachedASTList.RemoveUnmodified()

                    if (node is BinaryExpressionNode)
                    {
                        cachedASTList.Add(node);
                    }
                }
            }
        }

        /// <summary>
        /// This method updates a redefined function
        /// </summary>
        /// <param name="subtree"></param>
        /// <returns></returns>
        private void UndefineFunctions(IEnumerable<AssociativeNode> functionDefintions)
        {
            foreach (var funcDef in functionDefintions)
            {
                runnerCore.SetFunctionInactive(funcDef as FunctionDefinitionNode);
            }
        }

        /// <summary>
        /// This is to be used for debugging only to check code emitted from delta AST input
        /// </summary>
        /// <param name="deltaAstList"></param>
        /// <returns></returns>
        private string DebugCodeEmittedForDeltaAst(List<AssociativeNode> deltaAstList)
        {
            string code = string.Empty;
            ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(deltaAstList);
            code = codeGen.GenerateCode();
            return code;
        }

        /// <summary>
        /// Re-initializes the LiveRunner to reset the VM 
        /// Used temporarily when importing libraries on-demand during delta execution
        /// Will be deprecated once this is supported by the core language
        /// </summary>
        public void ReInitializeLiveRunner()
        {
            runner = new ProtoScriptTestRunner();

            executionOptions = new Options();
            deltaSymbols = 0;

            coreOptions = null;
            InitOptions();
            InitCore();

            staticContext = new ProtoCore.CompileTime.Context();

            currentSubTreeList = new Dictionary<Guid, Subtree>();
        }

        /// <summary>
        /// This is called temporarily to reset the VM and recompile the entire graph with new import 
        /// statements whenever a node from a new library is added to the graph.
        /// TODO: It should not be needed once we have language support to insert import statements arbitrarily
        /// </summary>
        /// <param name="libraries"></param>
        /// <param name="syncData"></param>
        public void ResetVMAndResyncGraph(List<string> libraries)
        {
            // Reset VM
            ReInitializeLiveRunner();

            // generate import node for each library in input list
            List<AssociativeNode> importNodes = new List<AssociativeNode>();
            foreach (string lib in libraries)
            {
                ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
                importNode.ModuleName = lib;

                importNodes.Add(importNode);
            }
            ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(importNodes);
            string code = codeGen.GenerateCode();

            isPreloadingLibraries = true;
            UpdateCmdLineInterpreter(code);
            isPreloadingLibraries = false;
        }

        /// <summary>
        /// Resets the VM whenever a new library is imported and re-imports them
        /// Returns the list of new Library Mirrors for reflection
        /// TODO: It should not be needed once we have language support to insert import statements arbitrarily
        /// </summary>
        /// <param name="libraries"></param>
        /// <returns></returns>
        public List<LibraryMirror> ResetVMAndImportLibrary(List<string> libraries)
        {
            List<LibraryMirror> libs = new List<LibraryMirror>();

            // Reset VM
            ReInitializeLiveRunner();

            // generate import node for each library in input list
            List<AssociativeNode> importNodes = null;
            foreach (string lib in libraries)
            {
                importNodes = new List<AssociativeNode>();

                ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
                importNode.ModuleName = lib;

                importNodes.Add(importNode);

                ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(importNodes);
                string code = codeGen.GenerateCode();
                                
                int currentCI = runnerCore.ClassTable.ClassNodes.Count;

                UpdateCmdLineInterpreter(code);

                int postCI = runnerCore.ClassTable.ClassNodes.Count;

                IList<ProtoCore.DSASM.ClassNode> classNodes = new List<ProtoCore.DSASM.ClassNode>();
                for (int i = currentCI; i < postCI; ++i)
                {
                    classNodes.Add(runnerCore.ClassTable.ClassNodes[i]);
                }
                
                ProtoCore.Mirror.LibraryMirror libraryMirror = ProtoCore.Mirror.Reflection.Reflect(lib, classNodes, runnerCore);
                libs.Add(libraryMirror);
            }            

            return libs;
        }

        private void SynchronizeInternal(GraphSyncData syncData)
        {
            runnerCore.Options.IsDeltaCompile = true;

            List<AssociativeNode> deltaAstList = new List<AssociativeNode>();

            if (syncData == null)
            {
                ResetForDeltaASTExecution();
                return;
            }


            if (syncData.DeletedSubtrees != null)
            {
                foreach (var st in syncData.DeletedSubtrees)
                {
                    if (st.AstNodes != null)
                    {
                        var nullNodes = MarkGraphNodesInactive(st.AstNodes);
                        if (nullNodes != null)
                        {
                            deltaAstList.AddRange(nullNodes);
                        }
                    }
                    else
                    {
                        // Handle the case where only the GUID of the deleted subtree was provided
                        // Get the cached subtree that is now being deleted
                        Subtree removeSubTree = new Subtree();
                        if (currentSubTreeList.TryGetValue(st.GUID, out removeSubTree))
                        {
                            if (removeSubTree.AstNodes != null)
                            {
                                //List<AssociativeNode> modifiedASTList = GetModifiedNodes(removeSubTree);
                                var nullNodes = MarkGraphNodesInactive(removeSubTree.AstNodes);
                                if (nullNodes != null)
                                {
                                    deltaAstList.AddRange(nullNodes);
                                }
                            }
                        }
                    }

                    Subtree oldSubTree;
                    if (currentSubTreeList.TryGetValue(st.GUID, out oldSubTree))
                    {
                        if (oldSubTree.AstNodes != null)
                        {
                            UndefineFunctions(oldSubTree.AstNodes.Where(n => n is FunctionDefinitionNode));
                        }
                        currentSubTreeList.Remove(st.GUID);
                    }
                }
            }

            if (syncData.ModifiedSubtrees != null)
            {
                foreach (var st in syncData.ModifiedSubtrees)
                {
                    Subtree oldSubTree;
                    bool cachedTreeExists = currentSubTreeList.TryGetValue(st.GUID, out oldSubTree);

                    List<FunctionDefinitionNode> modifiedFunctions = new List<FunctionDefinitionNode>();
                    if (st.AstNodes != null)
                    {
                        // Handle modifed statements
                        //modifiedASTList = GetModifiedNodes(st);
                        //if (null != modifiedASTList && modifiedASTList.Count > 0)
                        //{
                        //    var nullNodes = MarkGraphNodesInactive(modifiedASTList);
                        //    if (nullNodes != null)
                        //    {
                        //        //deltaAstList.AddRange(nullNodes);
                        //    }
                        //    deltaAstList.AddRange(modifiedASTList);
                        //}

                        // Handle modified statements
                        List<AssociativeNode> modifiedASTList = GetModifiedNodes(st);
                        if (null != modifiedASTList && modifiedASTList.Count > 0)
                        {
                            deltaAstList.AddRange(modifiedASTList);
                        }

                        // Disable removed nodes from the cache
                        if (cachedTreeExists)
                        {
                            if (null != oldSubTree.AstNodes)
                            {
                                List<AssociativeNode> removedNodes = GetInactiveASTList(oldSubTree.AstNodes, st.AstNodes);
                                DeactivateGraphnodes(removedNodes);
                            }
                        }


                        // Handle modifed functions
                        UndefineFunctions(st.AstNodes.Where(n => n is FunctionDefinitionNode));

                        // Get the modified function list
                        foreach (AssociativeNode fnode in st.AstNodes)
                        {
                            if (fnode is FunctionDefinitionNode)
                            {
                                modifiedFunctions.Add(fnode as FunctionDefinitionNode);
                            }
                        }
                        deltaAstList.AddRange(modifiedFunctions);


                        // Handle cached subtree
                        if (cachedTreeExists)
                        {
                            if (oldSubTree.AstNodes != null)
                            {
                                UndefineFunctions(oldSubTree.AstNodes.Where(n => n is FunctionDefinitionNode));
                            }

                            // Update the current subtree list
                            if (null != oldSubTree.AstNodes)
                            {
                                List<AssociativeNode> newCachedASTList = new List<AssociativeNode>();
                                newCachedASTList.AddRange(GetUnmodifiedASTList(oldSubTree.AstNodes, st.AstNodes));
                                newCachedASTList.AddRange(modifiedASTList);

                                st.AstNodes.Clear();
                                st.AstNodes.AddRange(newCachedASTList);
                                currentSubTreeList[st.GUID] = st;
                            }
                        }

                    }

                    //Subtree oldSubTree;
                    //if (currentSubTreeList.TryGetValue(st.GUID, out oldSubTree))
                    //{
                    //    if (oldSubTree.AstNodes != null)
                    //    {
                    //        UndefineFunctions(oldSubTree.AstNodes.Where(n => n is FunctionDefinitionNode));
                    //    }

                    //    // Update the curernt subtree list
                    //    UpdateCachedSubtree(st.GUID, modifiedASTList);
                    //    //currentSubTreeList[st.GUID] = st;
                    //}

                    
                    // Get the AST's dependent on every function in the modified function list,
                    // and append them to the list of AST's to be compiled and executed
                    foreach (FunctionDefinitionNode fnode in modifiedFunctions)
                    {
                        deltaAstList.AddRange(GetASTNodesDependentOnFunctionList(fnode));
                    }
                }
            }

            if (syncData.AddedSubtrees != null)
            {
                foreach (var st in syncData.AddedSubtrees)
                {
                    if (st.AstNodes != null)
                    {
                        deltaAstList.AddRange(st.AstNodes);
                    }

                    currentSubTreeList.Add(st.GUID, st);
                }
            }
            CompileAndExecuteForDeltaExecution(deltaAstList);
        }

        private void SynchronizeInternal(string code)
        {
            runnerCore.Options.IsDeltaCompile = true;

            if (string.IsNullOrEmpty(code))
            {
                code = "";

                ResetForDeltaASTExecution();
                return;
            }
            else
            {
                CompileAndExecuteForDeltaExecution(code);
            }
        }
        #endregion
    }

    namespace Obsolete
    {
        public enum EventStatus
        {
            OK,
            Error,
            Warning
        }

        [Obsolete("This interface is obsolete, use the one that defined in ProtoScript.Runner instead", false)]
        public interface ILiveRunner
        {
            ProtoCore.Core Core { get; }

            void UpdateGraph(GraphToDSCompiler.SynchronizeData syncData);
            void BeginUpdateGraph(GraphToDSCompiler.SynchronizeData syncData);
            void BeginConvertNodesToCode(List<SnapshotNode> snapshotNodes);

            void BeginQueryNodeValue(uint nodeId);

            ProtoCore.Mirror.RuntimeMirror QueryNodeValue(uint nodeId);
            ProtoCore.Mirror.RuntimeMirror QueryNodeValue(string nodeName);
            void BeginQueryNodeValue(List<uint> nodeIds);
            
            event NodeValueReadyEventHandler NodeValueReady;
            event GraphUpdateReadyEventHandler GraphUpdateReady;
            event NodesToCodeCompletedEventHandler NodesToCodeCompleted;
        }

        [Obsolete("This class is obsolete", false)]
        public partial class LiveRunner : ILiveRunner
        {
            /// <summary>
            ///  These are configuration parameters passed by host application to be consumed by geometry library and persistent manager implementation. 
            /// </summary>
            public class Options
            {
                /// <summary>
                /// The configuration parameters that needs to be passed to
                /// different applications.
                /// </summary>
                public Dictionary<string, object> PassThroughConfiguration;

                /// <summary>
                /// The path of the root graph/module
                /// </summary>
                public string RootModulePathName;

                /// <summary>
                /// List of search directories to resolve any file reference
                /// </summary>
                public List<string> SearchDirectories;

                /// <summary>
                /// If the Interpreter mode is true, the LiveRunner takes in code statements as input strings
                /// and not SyncData
                /// </summary>
                public bool InterpreterMode = false;
            }

            private Dictionary<uint, string> GetModifiedGuidList()
            {
                // Retrieve the actual modified nodes 
                // Execution is complete, get all the modified guids 
                // Get the modified symbol names from the VM
                List<string> modifiedNames = this.runnerCore.Rmem.GetModifiedSymbolString();
                Dictionary<uint, string> modfiedGuidList = new Dictionary<uint, string>();
                foreach (string name in modifiedNames)
                {
                    // Get the uid of the modified symbol
                    if (this.graphCompiler.mapModifiedName.ContainsKey(name))
                    {
                        uint id = this.graphCompiler.mapModifiedName[name];
                        if (!modfiedGuidList.ContainsKey(id))
                        {
                            // Append the modified guid into the modified list
                            modfiedGuidList.Add(this.graphCompiler.mapModifiedName[name], name);
                        }
                    }
                }
                return modfiedGuidList;
            }

            private void ResetModifiedSymbols()
            {
                this.runnerCore.Rmem.ResetModifedSymbols();
            }

            private SynchronizeData CreateSynchronizeDataForGuidList(Dictionary<uint, string> modfiedGuidList)
            {
                Dictionary<uint, SnapshotNode> modifiedGuids = new Dictionary<uint, SnapshotNode>();
                SynchronizeData syncDataReturn = new SynchronizeData();

                if (modfiedGuidList != null)
                {
                    //foreach (uint guid in modfiedGuidList)
                    foreach (var kvp in modfiedGuidList)
                    {
                        // Get the uid recognized by the graphIDE
                        uint guid = kvp.Key;
                        string name = kvp.Value;
                        SnapshotNode sNode = new SnapshotNode(this.graphCompiler.GetRealUID(guid), SnapshotNodeType.Identifier, name);
                        if (!modifiedGuids.ContainsKey(sNode.Id))
                        {
                            modifiedGuids.Add(sNode.Id, sNode);
                        }
                    }

                    foreach (KeyValuePair<uint, SnapshotNode> kvp in modifiedGuids)
                        syncDataReturn.ModifiedNodes.Add(kvp.Value);
                }

                return syncDataReturn;
            }

            private ProtoScriptTestRunner runner;
            private ProtoRunner.ProtoVMState vmState;
            private GraphToDSCompiler.GraphCompiler graphCompiler;
            private ProtoCore.Core runnerCore = null;
            public ProtoCore.Core Core
            {
                get
                {
                    return runnerCore;
                }
                private set
                {
                    runnerCore = value;
                }
            }

            private ProtoCore.Options coreOptions = null;
            private Options executionOptions = null;
            private bool syncCoreConfigurations = false;
            private int deltaSymbols = 0;
            private ProtoCore.CompileTime.Context staticContext = null;

            private readonly Object operationsMutex = new object();

            private Queue<Task> taskQueue;

            private Thread workerThread;

            public LiveRunner()
            {
                InitRunner(new Options());
            }

            public LiveRunner(Options options)
            {
                InitRunner(options);
            }


            public GraphToDSCompiler.GraphCompiler GetCurrentGraphCompilerInstance()
            {
                return graphCompiler;
            }

            private void InitRunner(Options options)
            {
                graphCompiler = GraphToDSCompiler.GraphCompiler.CreateInstance();
                graphCompiler.SetCore(GraphUtilities.GetCore());
                runner = new ProtoScriptTestRunner();

                executionOptions = options;
                InitOptions();
                InitCore();


                taskQueue = new Queue<Task>();

                workerThread = new Thread(new ThreadStart(TaskExecMethod));


                workerThread.IsBackground = true;
                workerThread.Start();

                staticContext = new ProtoCore.CompileTime.Context();
            }

            private void InitOptions()
            {

                // Build the options required by the core
                Validity.Assert(coreOptions == null);
                coreOptions = new ProtoCore.Options();
                coreOptions.GenerateExprID = true;
                coreOptions.IsDeltaExecution = true;
                coreOptions.BuildOptErrorAsWarning = true;

                coreOptions.WebRunner = false;
                coreOptions.ExecutionMode = ProtoCore.ExecutionMode.Serial;

                // This should have been set in the consturctor
                Validity.Assert(executionOptions != null);
            }

            private void InitCore()
            {
                Validity.Assert(coreOptions != null);

                // Comment Jun:
                // It must be guaranteed that in delta exeuction, expression id's must not be autogerated
                // expression Id's must be propagated from the graphcompiler to the DS codegenerators
                //Validity.Assert(coreOptions.IsDeltaExecution && !coreOptions.GenerateExprID);

                runnerCore = new ProtoCore.Core(coreOptions);

                SyncCoreConfigurations(runnerCore, executionOptions);


                runnerCore.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(runnerCore));
                runnerCore.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(runnerCore));
                runnerCore.FFIPropertyChangedMonitor.FFIPropertyChangedEventHandler += FFIPropertyChanged;
                vmState = null;
            }

            private void FFIPropertyChanged(FFIPropertyChangedEventArgs arg)
            {
                lock (taskQueue)
                {
                    taskQueue.Enqueue(new PropertyChangedTask(this, arg.hostGraphNode));
                }
            }

            private static void SyncCoreConfigurations(ProtoCore.Core core, Options options)
            {
                if (null == options)
                    return;
                //update the root module path name, if set.
                if (!string.IsNullOrEmpty(options.RootModulePathName))
                    core.Options.RootModulePathName = options.RootModulePathName;
                //then update the search path, if set.
                if (null != options.SearchDirectories)
                    core.Options.IncludeDirectories = options.SearchDirectories;

                //Finally update the pass thru configuration values
                if (null == options.PassThroughConfiguration)
                    return;
                foreach (var item in options.PassThroughConfiguration)
                {
                    core.Configurations[item.Key] = item.Value;
                }
            }


            public void SetOptions(Options options)
            {
                executionOptions = options;
                syncCoreConfigurations = true; //request syncing the configuration
            }


            #region Public Live Runner Events

            public event NodeValueReadyEventHandler NodeValueReady = null;
            public event GraphUpdateReadyEventHandler GraphUpdateReady = null;
            public event NodesToCodeCompletedEventHandler NodesToCodeCompleted = null;

            #endregion

            /// <summary>
            /// Push new synchronization data, returns immediately and will
            /// trigger a GraphUpdateReady when the value when the execution
            /// is completed
            /// </summary>
            /// <param name="syncData"></param>
            public void BeginUpdateGraph(SynchronizeData syncData)
            {
                lock (taskQueue)
                {
                    taskQueue.Enqueue(
                        new UpdateGraphTask(syncData, this));
                }

                //Todo(Luke) add a Monitor queue to prevent having to have the 
                //work poll
            }

            /// <summary>
            /// Takes in a list of SnapshotNode objects, condensing them into one 
            /// or more SnapshotNode objects which caller can then turn into a more 
            /// compact representation of the former SnapshotNode objects.
            /// </summary>
            /// <param name="snapshotNodes">A list of source SnapshotNode objects 
            /// from which the resulting list of SnapshotNode is to be computed.
            /// </param>
            public void BeginConvertNodesToCode(List<SnapshotNode> snapshotNodes)
            {
                if (null == snapshotNodes || (snapshotNodes.Count <= 0))
                    return; // Do nothing, there's no nodes to be converted.

                lock (taskQueue)
                {
                    taskQueue.Enqueue(
                        new ConvertNodesToCodeTask(snapshotNodes, this));
                }
            }

            /// <summary>
            /// Query For a node value this will trigger a NodeValueReady callback
            /// when the value is available
            /// </summary>
            /// <param name="nodeId"></param>
            public void BeginQueryNodeValue(uint nodeId)
            {
                lock (taskQueue)
                {
                    taskQueue.Enqueue(
                        new NodeValueRequestTask(nodeId, this));
                }
            }

            /// <summary>
            /// Query For a node value this will trigger a NodeValueReady callback
            /// when the value is available
            /// This version is more efficent than calling the BeginQueryNodeValue(uint)
            /// repeatedly 
            /// </summary>
            /// <param name="nodeId"></param>
            public void BeginQueryNodeValue(List<uint> nodeIds)
            {
                lock (taskQueue)
                {
                    foreach (uint nodeId in nodeIds)
                    {
                        taskQueue.Enqueue(
                            new NodeValueRequestTask(nodeId, this));
                    }
                }
            }

            /// <summary>
            /// TODO: Deprecate - This will be replaced with the overload that takes in a Guid type
            /// Query for a node value. This will block until the value is available.
            /// This uses the expression interpreter to evaluate a node variable's value.
            /// It will only serviced when all ASync calls have been completed
            /// </summary>
            /// <param name="nodeId"></param>
            /// <returns></returns>
            public ProtoCore.Mirror.RuntimeMirror QueryNodeValue(uint nodeId)
            {
                while (true)
                {
                    lock (taskQueue)
                    {
                        //Spin waiting for the queue to be empty
                        if (taskQueue.Count == 0)
                        {

                            //No entries and we have the lock
                            //Synchronous query to get the node

                            return InternalGetNodeValue(nodeId);
                        }
                    }
                    Thread.Sleep(0);
                }

            }

            /// <summary>
            /// Query for a node value given its variable name. This will block until the value is available.
            /// This uses the expression interpreter to evaluate a node variable's value.
            /// It will only serviced when all ASync calls have been completed
            /// </summary>
            /// <param name="nodeId"></param>
            /// <returns></returns>
            public ProtoCore.Mirror.RuntimeMirror QueryNodeValue(string nodeName)
            {
                while (true)
                {
                    lock (taskQueue)
                    {
                        //Spin waiting for the queue to be empty
                        if (taskQueue.Count == 0)
                        {

                            //No entries and we have the lock
                            //Synchronous query to get the node

                            return InternalGetNodeValue(nodeName);
                        }
                    }
                    Thread.Sleep(0);
                }

            }
            
    
    public void UpdateGraph(SynchronizeData syndData)
    {
        
            
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        string code = null;
                            SynchronizeInternal(syndData, out code);
                            return;
                            
                    }
                }
                Thread.Sleep(0);
            }
    }

    //Secondary thread
    private void TaskExecMethod()
    {
        while (true)
        {
            Task task = null;
                
                lock (taskQueue)
                {
                    if (taskQueue.Count > 0)
                        task = taskQueue.Dequeue();
                }
            
                if (task != null)
                {
                    task.Execute();
                        continue;
                        
                }
            
                Thread.Sleep(50);
                
        }
        
    }

    
    
#region Internal Implementation
    
    
    private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(string varname)
    {
        Validity.Assert(null != vmState);
            
            // Comment Jun: all symbols are in the global block as there is no notion of scoping the the graphUI yet.
            const int blockID = 0;
            
            return vmState.LookupName(varname, blockID);
    }

    private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(uint nodeId)
    {
        //ProtoCore.DSASM.Constants.kInvalidIndex tells the UpdateUIDForCodeblock to look for the lastindex for given codeblock
        nodeId = graphCompiler.UpdateUIDForCodeblock(nodeId, ProtoCore.DSASM.Constants.kInvalidIndex);
        Validity.Assert(null != vmState);
        string varname = graphCompiler.GetVarName(nodeId);
        if (string.IsNullOrEmpty(varname))
        {
            return null;
        }
        return InternalGetNodeValue(varname);
    }

    private bool Compile(string code, out int blockId)
    {
        Dictionary<string, bool> execFlagList = null;
            if (graphCompiler != null)
                execFlagList = graphCompiler.ExecutionFlagList;
                    
                    staticContext.SetData(code, new Dictionary<string, object>(), execFlagList);
                    
                    bool succeeded = runner.Compile(staticContext, runnerCore, out blockId);
                    if (succeeded)
                    {
                        // Regenerate the DS executable
                        runnerCore.GenerateExecutable();
                            
                            // Update the symbol tables
                            // TODO Jun: Expand to accomoadate the list of symbols
                            //staticContext.symbolTable = runnerCore.DSExecutable.runtimeSymbols[0];
                    }
        return succeeded;
    }


    
    private ProtoRunner.ProtoVMState Execute()
    {
        // runnerCore.GlobOffset is the number of global symbols that need to be allocated on the stack
        // The argument to Reallocate is the number of ONLY THE NEW global symbols as the stack needs to accomodate this delta
        int newSymbols = runnerCore.GlobOffset - deltaSymbols;
            
            // If there are lesser symbols to allocate for this run, then it means nodes were deleted.
            // TODO Jun: Determine if it is safe to just leave them in the global stack 
            //           as no symbols point to this memory location in the stack anyway
            if (newSymbols >= 0)
            {
                runnerCore.Rmem.ReAllocateMemory(newSymbols);
            }
        
            // Store the current number of global symbols
            deltaSymbols = runnerCore.GlobOffset;
            
            // Initialize the runtime context and pass it the execution delta list from the graph compiler
            ProtoCore.Runtime.Context runtimeContext = new ProtoCore.Runtime.Context();
            
            if (graphCompiler != null)
                runtimeContext.execFlagList = graphCompiler.ExecutionFlagList;
                    
                    runner.Execute(runnerCore, runtimeContext);
                    
                    // ExecutionMirror mirror = new ExecutionMirror(runnerCore.CurrentExecutive.CurrentDSASMExec, runnerCore);
                    
                    return new ProtoRunner.ProtoVMState(runnerCore);
    }

    private bool CompileAndExecute(string code)
    {
        // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
        int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(code, out blockId);
            if (succeeded)
            {
                runnerCore.RunningBlock = blockId;
                    vmState = Execute();
            }
        return succeeded;
    }


    private void ResetVMForExecution()
    {
        runnerCore.ResetForExecution();
    }

    private void ResetVMForDeltaExecution()
    {
        runnerCore.ResetForDeltaExecution();
    }


    private void SynchronizeInternal(GraphToDSCompiler.SynchronizeData syncData, out string code)
    {
        Validity.Assert(null != runner);
            Validity.Assert(null != graphCompiler);
            
            if (syncData.AddedNodes.Count == 0 &&
                    syncData.ModifiedNodes.Count == 0 &&
                    syncData.RemovedNodes.Count == 0)
            {
                code = "";
                    ResetVMForDeltaExecution();
                    return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Begin SyncInternal: {0}", syncData);
                    GraphToDSCompiler.GraphBuilder g = new GraphBuilder(syncData, graphCompiler);
                    code = g.BuildGraphDAG();
                    
                    System.Diagnostics.Debug.WriteLine("SyncInternal => " + code);
                    
                    //List<string> deletedVars = new List<string>();
                    ResetVMForDeltaExecution();
                    
                    //Synchronize the core configuration before compilation and execution.
                    if (syncCoreConfigurations)
                    {
                        SyncCoreConfigurations(runnerCore, executionOptions);
                            syncCoreConfigurations = false;
                    }
                
                    bool succeeded = CompileAndExecute(code);
                    if (succeeded)
                    {
                        graphCompiler.ResetPropertiesForNextExecution();
                    }
            }
    }

   
#endregion
        }
    }
}
