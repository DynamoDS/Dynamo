using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using ProtoCore.AssociativeGraph;
using ProtoCore.AssociativeEngine;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.CodeModel;
using ProtoCore.DebugServices;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using ProtoCore.Lang.Replication;
using ProtoCore.Runtime;
using ProtoCore.Utils;
using ProtoFFI;

using StackFrame = ProtoCore.DSASM.StackFrame;

namespace ProtoCore
{
    public class InterpreterProperties
    {
        public GraphNode executingGraphNode { get; set; }
        public List<GraphNode> nodeIterations { get; set; }

        public List<StackValue> functionCallArguments { get; set; }
        public List<StackValue> functionCallDotCallDimensions { get; set; }
        public DominantListStructure DominantStructure { get; set; }

        public InterpreterProperties()
        {
            Reset();
        }

        public InterpreterProperties(InterpreterProperties rhs)
        {
            executingGraphNode = rhs.executingGraphNode;
            nodeIterations = rhs.nodeIterations;
            functionCallArguments = rhs.functionCallArguments;
            functionCallDotCallDimensions = rhs.functionCallDotCallDimensions;
            DominantStructure = rhs.DominantStructure;
        }

        public void Reset()
        {
            executingGraphNode = null;
            nodeIterations = new List<GraphNode>();
            functionCallArguments = new List<StackValue>();
            functionCallDotCallDimensions = new List<StackValue>();
        }
    }

    /// <summary>
    /// RuntimeCore is an object that is instantiated once across the lifecycle of the runtime
    /// This is the entry point of the runtime VM and its input is a DS Executable format. 
    /// There will only be one instance of RuntimeCore regardless of how many times instances of a DSASM.Executive (runtime VM) is instantiated.
    /// Its properties will be persistent and accessible across all instances of a DSASM.Executive
    /// </summary>
    public class RuntimeCore
    {
        public RuntimeCore(Heap heap, Options options = null, Executable executable = null)
        {
            // The heap is initialized by the core and is used to allocate strings
            // Use the that heap for runtime
            Validity.Assert(heap != null);
            this.Heap = heap;
            RuntimeMemory = new RuntimeMemory(Heap);

            this.Options = options;

            InterpreterProps = new Stack<InterpreterProperties>();
            ReplicationGuides = new List<List<ReplicationGuide>>();
            AtLevels = new List<AtLevel>();
            executedAstGuids = new HashSet<Guid>();

            RunningBlock = 0;
            ExecutionState = (int)ExecutionStateEventArgs.State.Invalid; //not yet started

            ContinuationStruct = new ContinuationStructure();


            watchStack = new List<StackValue>();
            watchFramePointer = Constants.kInvalidIndex;
            WatchSymbolList = new List<SymbolNode>();

            FunctionCallDepth = 0;
            cancellationPending = false;

            watchClassScope = Constants.kInvalidIndex;

            ExecutionInstance = CurrentExecutive = new Executive(this);
            ExecutiveProvider = new ExecutiveProvider();

            RuntimeStatus = new ProtoCore.RuntimeStatus(this);
            StartPC = Constants.kInvalidPC;
            RuntimeData = new ProtoCore.RuntimeData();
            DSExecutable = executable;
            Mirror = null;
        }

        /// <summary>
        /// Setup before execution
        /// This function needs to be called before attempting to execute the RuntimeCore
        /// It will initialize the runtime execution data and configuration
        /// </summary>
        /// <param name="compileCore"></param>
        /// <param name="isCodeCompiled"></param>
        /// <param name="context"></param>
        public void SetupForExecution(ProtoCore.Core compileCore, int globalStackFrameSize)
        {
            if (globalStackFrameSize > 0)
            {
                RuntimeMemory.PushFrameForGlobals(globalStackFrameSize);
            }
            RunningBlock = 0;
            RuntimeStatus.MessageHandler = compileCore.BuildStatus.MessageHandler;
            WatchSymbolList = compileCore.watchSymbolList;
            SetProperties(compileCore.Options, compileCore.DSExecutable, compileCore.DebuggerProperties, null, compileCore.ExprInterpreterExe);
            RegisterDllTypes(compileCore.DllTypesToLoad);
            NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.ExecutionBegin);
        }

        public void SetProperties(Options runtimeOptions, Executable executable, DebugProperties debugProps = null, ProtoCore.Runtime.Context context = null, Executable exprInterpreterExe = null)
        {
            this.Context = context;
            this.DSExecutable = executable;
            this.Options = runtimeOptions;
            this.DebugProps = debugProps;
            this.ExprInterpreterExe = exprInterpreterExe;
        }

        /// <summary>
        /// Register imported dll types
        /// These types are initialzed from Importing dlls
        /// </summary>
        /// <param name="dllTypes"></param>
        public void RegisterDllTypes(List<System.Type> dllTypes)
        {
            foreach (System.Type type in dllTypes)
            {
                FFIExecutionManager.Instance.RegisterExtensionApplicationType(this, type);
            }
        }
        public RuntimeData RuntimeData { get; set; }
        public IExecutiveProvider ExecutiveProvider { get; set; }
        public Executive ExecutionInstance { get; private set; }
        public Executive CurrentExecutive { get; private set; }
        public int StartPC { get; private set; }

        // Execution properties
        public Executable DSExecutable { get; private set; }
        public Executable ExprInterpreterExe { get; private set; }
        public Options Options { get; private set; }
        public RuntimeStatus RuntimeStatus { get; set; }
        public Stack<InterpreterProperties> InterpreterProps { get; set; }
        public ProtoCore.Runtime.Context Context { get; set; }

        // Memory
        public Heap Heap { get; set; }
        public RuntimeMemory RuntimeMemory { get; set; }

        public delegate void DisposeDelegate(RuntimeCore sender);
        public event DisposeDelegate Dispose;
        public event EventHandler<ExecutionStateEventArgs> ExecutionEvent;
        public int ExecutionState { get; set; }

        // this one is to address the issue that when the execution control is in a language block
        // which is further inside a function, the compiler feprun is false, 
        // when inspecting value in that language block or the function, debugger will assume the function index is -1, 
        // name look up will fail beacuse all the local variables inside 
        // that language block and fucntion has non-zero function index 
        public int FunctionCallDepth { get; set; }

        /// <summary>
        /// The currently executing blockID
        /// </summary>
        public int RunningBlock { get; set; }

        /// <summary>
        /// RuntimeExpressionUID is used by the associative engine at runtime to determine the current expression ID being executed
        /// </summary>
        public int RuntimeExpressionUID = 0;

        // Cached replication guides for the current call. 
        // TODO Jun: Store this in the dynamic table node
        public List<List<ReplicationGuide>> ReplicationGuides;

        // Cached at levels for the current call.
        public List<AtLevel> AtLevels;

        private HashSet<Guid> executedAstGuids; 
        // GUIDs of executed ASTs.
        public IEnumerable<Guid> ExecutedAstGuids
        {
            get { return executedAstGuids; }
        }

        
        public ProtoCore.DSASM.Mirror.ExecutionMirror Mirror { get; set; }

        private bool cancellationPending = false;
        public bool CancellationPending
        {
            get
            {
                return cancellationPending;
            }
        }

#region DEBUGGER_PROPERTIES

        public int watchClassScope { get; set; }

        public DebugProperties DebugProps { get; set; }
        public List<Instruction> Breakpoints { get; set; }

        // Continuation properties used for Serial mode execution and Debugging of Replicated calls
        public ContinuationStructure ContinuationStruct { get; set; }
        /// <summary>
        /// Returns the reason why the execution was last suspended
        /// </summary>
        public ReasonForExecutionSuspend ReasonForExecutionSuspend { get; internal set; }


        public List<StackValue> watchStack { get; set; }
        public int watchFramePointer { get; set; }

        public List<SymbolNode> WatchSymbolList { get; set; }
#endregion 
        
        private Dictionary<Guid, List<StackValue>> callsiteGCRoots = new Dictionary<Guid, List<StackValue>>();

        public IEnumerable<StackValue> CallSiteGCRoots
        {
            get { return callsiteGCRoots.Values.SelectMany(x => x);  }
        }

        public void AddCallSiteGCRoot(Guid callSiteID, StackValue sv)
        {
            if (!sv.IsReferenceType)
                return;

            if (!callsiteGCRoots.ContainsKey(callSiteID))
                callsiteGCRoots[callSiteID] = new List<StackValue>();

            List<StackValue> svs = callsiteGCRoots[callSiteID];
            svs.Add(sv); 
        }

        public void RemoveCallSiteGCRoot(Guid callSiteID)
        {
            callsiteGCRoots.Remove(callSiteID);
        }

        public void ResetForDeltaExecution()
        {
            RunningBlock = 0;
            ExecutionState = (int)ExecutionStateEventArgs.State.Invalid;
            StartPC = Constants.kInvalidPC;
        }

        protected void OnDispose()
        {
            if (Dispose != null)
            {
                Dispose(this);
            }
        }

        public void Cleanup()
        {
            OnDispose();
            CLRModuleType.ClearTypes();
        }

        public void NotifyExecutionEvent(ExecutionStateEventArgs.State state)
        {
            switch (state)
            {
                case ExecutionStateEventArgs.State.ExecutionBegin:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.Invalid, "Invalid Execution state being notified.");
                    break;
                case ExecutionStateEventArgs.State.ExecutionEnd:
                    if (ExecutionState == (int)ExecutionStateEventArgs.State.Invalid) //execution never begun.
                        return;
                    break;
                case ExecutionStateEventArgs.State.ExecutionBreak:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.ExecutionBegin || ExecutionState == (int)ExecutionStateEventArgs.State.ExecutionResume, "Invalid Execution state being notified.");
                    break;
                case ExecutionStateEventArgs.State.ExecutionResume:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.ExecutionBreak, "Invalid Execution state being notified.");
                    break;
                default:
                    Validity.Assert(false, "Invalid Execution state being notified.");
                    break;
            }
            ExecutionState = (int)state;
            if (null != ExecutionEvent)
                ExecutionEvent(this, new ExecutionStateEventArgs(state));
        }
        
        public int GetCurrentBlockId()
        {
            int constructBlockId = RuntimeMemory.CurrentConstructBlockId;
            if (constructBlockId == Constants.kInvalidIndex)
                return DebugProps.CurrentBlockId;

            CodeBlock constructBlock = ProtoCore.Utils.CoreUtils.GetCodeBlock(DSExecutable.CodeBlocks, constructBlockId);
            while (null != constructBlock && constructBlock.blockType == CodeBlockType.Construct)
            {
                constructBlock = constructBlock.parent;
            }

            if (null != constructBlock)
                constructBlockId = constructBlock.codeBlockId;

            if (constructBlockId != DebugProps.CurrentBlockId)
                return DebugProps.CurrentBlockId;
            else
                return RuntimeMemory.CurrentConstructBlockId;
        }

        //STop
        public Stopwatch StopWatch;
        public void StartTimer()
        {
            StopWatch = new Stopwatch();
            StopWatch.Start();
        }
        public TimeSpan GetCurrentTime()
        {
            TimeSpan ts = StopWatch.Elapsed;
            return ts;
        }

        /// <summary>
        /// This function determines what the starting pc should be for the next execution session
        /// The StartPC takes precedence if set. Otherwise, the entry pc in the global codeblock is the entry point
        /// StartPC is assumed to be reset to kInvalidPC after each execution session
        /// </summary>
        public void SetupStartPC()
        {
            if (StartPC == Constants.kInvalidPC && DSExecutable.CodeBlocks.Count > 0)
            {
                StartPC = DSExecutable.CodeBlocks[0].instrStream.entrypoint;
            }
        }

        /// <summary>
        /// Sets a new entry point pc
        /// This can be overrided by another call to SetStartPC
        /// </summary>
        /// <param name="pc"></param>
        public void SetStartPC(int pc)
        {
            StartPC = pc;
        }

        /// <summary>
        /// Record the GUID of executed graph node.
        /// </summary>
        /// <param name="graphNode"></param>
        public void RecordExtecutedGraphNode(GraphNode graphNode)
        {
            if (graphNode != null && !graphNode.guid.Equals(Guid.Empty))
            {
                executedAstGuids.Add(graphNode.guid);
            }
        }

        /// <summary>
        /// Clear all recorded AST guids
        /// </summary>
        public void RemoveExecutedAstGuids()
        {
            executedAstGuids.Clear();
        }
    }
}
