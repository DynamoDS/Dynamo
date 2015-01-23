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
    namespace DebugServices
    {
        public delegate void BeginDocument(string script);
        public delegate void EndDocument(string script);
        public delegate void PrintMessage(string message);
        public abstract class EventSink
        {
            public BeginDocument BeginDocument;
            public EndDocument EndDocument;
            public PrintMessage PrintMessage;
        }

        public class ConsoleEventSink : EventSink
        {
            public int delme;
            public ConsoleEventSink()
            {
                BeginDocument += Console.WriteLine;
                EndDocument += Console.WriteLine;
                PrintMessage += Console.WriteLine;
            }
        }

        internal static class StreamUtil
        {
            internal static void AddText(FileStream stream, string p)
            {
                byte[] info = new UTF8Encoding(true).GetBytes(p);
                stream.Write(info, 0, info.Length);
            }
        }

        public class FEventSink : EventSink, IDisposable
        {
            private readonly FileStream stream;

            public FEventSink(string fileName)
            {
                stream = new FileStream(fileName + ".log", FileMode.Create, FileAccess.Write, FileShare.Read);
                BeginDocument += p => StreamUtil.AddText(stream, "Begin Document: " + p);
                EndDocument += p => StreamUtil.AddText(stream, "End Document: " + p);
                PrintMessage += p => StreamUtil.AddText(stream, p);
            }

            #region IDisposable Members
            public void Dispose()
            {
                stream.Close();
            }
            #endregion
        }
    }

    public enum ExecutionMode
    {
        Parallel,
        Serial
    }

    public enum ReasonForExecutionSuspend
    {
        PreStart,
        Breakpoint,
        Exception,
        Warning,
        EndOfFile,
        NoEntryPoint,
        VMSplit

    }

    /// <summary>
    /// Represents a single replication guide entity that is associated with an argument to a function
    /// 
    /// Given:
    ///     a = f(i<1>, j<2L>)
    ///     
    ///     <1> and <2L> are each represented by a ReplicationGuide instance
    ///     
    /// </summary>
    public class ReplicationGuide
    {
        public ReplicationGuide(int guide, bool longest)
        {
            guideNumber = guide;
            isLongest = longest;
        }

        public int guideNumber { get; private set; }
        public bool isLongest {get; private set;}
    }

    public class InterpreterProperties
    {
        public GraphNode executingGraphNode { get; set; }
        public List<GraphNode> nodeIterations { get; set; }

        public List<StackValue> functionCallArguments { get; set; }
        public List<StackValue> functionCallDotCallDimensions { get; set; }

        public UpdateStatus updateStatus { get; set; }

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
            updateStatus = rhs.updateStatus;
        }

        public void Reset()
        {
            executingGraphNode = null;
            nodeIterations = new List<GraphNode>();
            functionCallArguments = new List<StackValue>();
            functionCallDotCallDimensions = new List<StackValue>();
            updateStatus = UpdateStatus.kNormalUpdate;
        }
    }

    public class Options
    {
        public Options()
        {
            ApplyUpdate = false;

            DumpByteCode = false;
            Verbose = false;
            DumpIL = false;

            GenerateSSA = true;
            ExecuteSSA = true;
            GCTempVarsOnDebug = true;

            DumpFunctionResolverLogic = false; 
            DumpOperatorToMethodByteCode = false;
            SuppressBuildOutput = false;
            BuildOptWarningAsError = false;
            BuildOptErrorAsWarning = false;
            ExecutionMode = ExecutionMode.Serial;
            IDEDebugMode = false;
            WatchTestMode = false;
            IncludeDirectories = new List<string>();

            // defaults to 6 decimal places
            //
            FormatToPrintFloatingPoints = "F6";
            RootCustomPropertyFilterPathName = @"C:\arxapiharness\Bin\AcDesignScript\CustomPropertyFilter.txt";
            CompileToLib = false;
            AssocOperatorAsMethod = true;

            EnableProcNodeSanityCheck = true;
            EnableReturnTypeCheck = true;

            RootModulePathName = Path.GetFullPath(@".");
            staticCycleCheck = true;
            dynamicCycleCheck = true;
            RecursionChecking = false;
            EmitBreakpoints = true;

            localDependsOnGlobalSet = false;
            LHSGraphNodeUpdate = true;
            TempReplicationGuideEmptyFlag = true;
            AssociativeToImperativePropagation = true;
            SuppressFunctionResolutionWarning = true;
            EnableVariableAccumulator = true;
            WebRunner = false;
            DisableDisposeFunctionDebug = true;
            GenerateExprID = true;
            IsDeltaExecution = false;
            ElementBasedArrayUpdate = false;

            IsDeltaCompile = false;

        }

        public bool ApplyUpdate { get; set; }
        public bool DumpByteCode { get; set; }
        public bool DumpIL { get; private set; }
        public bool GenerateSSA { get; set; }
        public bool ExecuteSSA { get; set; }
        public bool GCTempVarsOnDebug { get; set; }
        public bool Verbose { get; set; }
        public bool DumpOperatorToMethodByteCode { get; set; }
        public bool SuppressBuildOutput { get; set; }
        public bool BuildOptWarningAsError { get; set; }
        public bool BuildOptErrorAsWarning { get; set; }
        public bool IDEDebugMode { get; set; }      //set to true if two way mapping b/w DesignScript and JIL code is needed
        public bool WatchTestMode { get; set; }     // set to true when running automation tests for expression interpreter
        public ExecutionMode ExecutionMode { get; set; }
        public string FormatToPrintFloatingPoints { get; set; }
        public bool CompileToLib { get; set; }
        public bool AssocOperatorAsMethod { get; set; }
        public string LibPath { get; set; }
        public bool staticCycleCheck { get; set; }
        public bool dynamicCycleCheck { get; set; }
        public bool RecursionChecking { get; set; }
        public bool DumpFunctionResolverLogic { get; set; }
        public bool EmitBreakpoints { get; set; }
        public bool localDependsOnGlobalSet { get; set; }
        public bool LHSGraphNodeUpdate { get; set; }
        public bool SuppressFunctionResolutionWarning { get; set; }
        public bool WebRunner { get; set; }

        public bool TempReplicationGuideEmptyFlag { get; set; }
        public bool AssociativeToImperativePropagation { get; set; }
        public bool EnableVariableAccumulator { get; set; }
        public bool DisableDisposeFunctionDebug { get; set; }
        public bool GenerateExprID { get; set; }
        public bool IsDeltaExecution { get; set; }
        public bool ElementBasedArrayUpdate { get; set; }

        /// <summary>
        /// TODO: Aparajit: This flag is true for Delta AST compilation
        /// This will be removed once we make this the default and deprecate "deltaCompileStartPC" 
        /// which requires recompiling the entire source code for every delta execution 
        /// </summary>
        public bool IsDeltaCompile { get; set; }

        
        // This is being moved to Core.Options as this needs to be overridden for the Watch test framework runner        
        public int kDynamicCycleThreshold = 2000;
        
        public double Tolerance
        {
            get { return MathUtils.Tolerance; }
            set { MathUtils.Tolerance = value; }
        }

        public List<string> IncludeDirectories { get; set; }
        public string RootModulePathName { get; set; }

        private string rootCustomPropertyFilterPathName;
        public string RootCustomPropertyFilterPathName
        {
            get
            {
                return rootCustomPropertyFilterPathName;
            }
            set
            {
                if (value == null)
                {
                    rootCustomPropertyFilterPathName = null;
                }
                else
                {
                    var fileName = value;
                    if (File.Exists(fileName))
                    {
                        rootCustomPropertyFilterPathName = fileName;

                        StreamReader stream = null;
                        try
                        {
                            stream = new StreamReader(fileName);
                        }
                        catch (Exception ex)
                        {
                            throw new FileLoadException(string.Format("Custom property filter file {0} can't be read. Error Message:{1}", fileName, ex.Message));
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }
                    else
                    {
                        //throw new System.IO.FileNotFoundException(string.Format("Custom property filter file {0} does not exists", fileName));
                        rootCustomPropertyFilterPathName = null;
                    }
                }
            }
        }

        public bool EnableReturnTypeCheck { get; set; }

        public bool EnableProcNodeSanityCheck { get; set; }

    }

    public struct InlineConditional
    {
        public bool isInlineConditional;
        public int endPc;
        public int startPc;
        public int instructionStream;
        public List<Instruction> ActiveBreakPoints;
    }

    public enum Runmode
    {
        RunTo, StepNext, StepIn, StepOut
    }

    public class DebugFrame
    {
        public DebugFrame()
        {
            IsReplicating = false;
            IsExternalFunction = false;
            IsBaseCall = false;
            IsDotCall = false;
            IsInlineConditional = false;
            IsMemberFunction = false;
            IsDisposeCall = false;
            HasDebugInfo = false;

            FinalFepChosen = null;
            FunctionStepOver = false;
            DotCallDimensions = null;
            Arguments = null;
            ThisPtr = null;
        }

        public FunctionEndPoint FinalFepChosen { get; set; }

        // TODO: FepRun may no longer be needed as this may also be obtained from the language stack frame - pratapa
        public int FepRun { get; set; }
        public GraphNode ExecutingGraphNode { get; set; }
        public List<StackValue> DotCallDimensions { get; set; }
        public List<StackValue> Arguments { get; set; }
        public StackValue? ThisPtr { get; set; }
        
        // Flag indicating whether execution cursor is being resumed from within the lang block or function
        public bool IsResume { get; set; }
        public bool IsReplicating { get; set; }
        public bool IsExternalFunction { get; set; }
        public bool IsBaseCall { get; set; }
        public bool IsDotCall { get; set; }
        public bool IsInlineConditional { get; set; }
        public bool IsMemberFunction { get; set; }
        public bool IsDisposeCall { get; set; }
        public bool HasDebugInfo { get; set; }

        public bool FunctionStepOver { get; set; }

    }

    public class DebugProperties
    {
        public DebugProperties()
        {
            DebugStackFrame = new Stack<DebugFrame>();

            isResume = false;
            executingGraphNode = null;
            ActiveBreakPoints = new List<Instruction>();
            AllbreakPoints = null;
            FRStack = new Stack<bool>();
            FirstStackFrame = new StackFrame(1);
            
            DebugEntryPC = Constants.kInvalidIndex;
            CurrentBlockId = Constants.kInvalidIndex;
            StepOutReturnPC = Constants.kInvalidIndex;
            ReturnPCFromDispose = Constants.kInvalidIndex;
            IsPopmCall = false;
        }

        public enum BreakpointOptions
        {
            None = 0x00000000,
            EmitIdentifierBreakpoint = 0x00000001,
            EmitPopForTempBreakpoint = 0x00000002,
            EmitCallrForTempBreakpoint = 0x00000004,
            EmitInlineConditionalBreakpoint = 0x00000008,
            SuppressNullVarDeclarationBreakpoint = 0x00000010
        }

        public enum StackFrameFlagOptions
        {
            FepRun = 1,
            IsReplicating,
            IsExternalFunction,
            IsFunctionStepOver
        }

        // This field allows the code generator to selectively output DebugInfo 
        // for various parts of the code emission process. For an example, a 
        // regular identifier of variable would not generally output a DebugInfo 
        // object on the corresponding instruction. This can be temporary turned
        // on (in some very limited cases) if desired.
        // 
        // Moving forward we would introduce few more options in this enumeration 
        // to handle various cases. Note that since memory is reset when a struct 
        // is instantiated, the default value of "breakpointOptions" will be 0. 
        // Any flag introduced to "BreakpointOptions" enumeration will always be 
        // "turned off" by default. For flags that are usually turned on and only 
        // turned off in few scenarios, consider using a name that has the 
        // inversed meaning. For example function calls are always emitted, to 
        // suppress the emission in few cases, use the term along the line of 
        // "SuppressFunctionBreakpoint", which will by default absent.
        // 
        private BreakpointOptions breakpointOptions = BreakpointOptions.None;

        public BreakpointOptions breakOptions
        {
            get { return breakpointOptions; }
            set { breakpointOptions = value; }
        }

        public StackFrame FirstStackFrame { get; set; }

        // Used in Watch test framework
        public string CurrentSymbolName { get; set; }
        public bool IsPopmCall { get; set; }

        public InlineConditional InlineConditionOptions = new InlineConditional
        {
            isInlineConditional = false,
            startPc = Constants.kInvalidIndex,
            endPc = Constants.kInvalidIndex,
            instructionStream = 0,
            ActiveBreakPoints = new List<Instruction>()
        };

        public CodeRange highlightRange = new CodeRange
            {
                StartInclusive = new CodePoint
                {
                    LineNo = Constants.kInvalidIndex,
                    CharNo = Constants.kInvalidIndex
                },

                EndExclusive = new CodePoint
                {
                    LineNo = Constants.kInvalidIndex,
                    CharNo = Constants.kInvalidIndex
                }
            };

        /// <summary>
        /// Gets the Program counter. This is only valid when the executive is suspended
        /// </summary>
        public int DebugEntryPC { get; set; }
        // used by the code gen to insert the file name to the instruction

        // this is needed because in the if/for/while structure, the core.runningBlock is its parent's block id, not its own
        // we will not be able to inspect the local variable in these structures by using core.runningBlock as the current block id
        //
        // core.runningBlock is updated only at Bounce opcode
        // the instructions of if/for/while stay in their parent instruction stream but there symbols stay in their own symbol tables 
        public int CurrentBlockId { get; set; }
        public bool isResume { get; set; }
        public int StepOutReturnPC { get; set; }
        public Stack<bool> FRStack { get; set; }
        public GraphNode executingGraphNode { get; set; }
        public List<GraphNode> deferedGraphnodes { get; set; }
        public List<Instruction> ActiveBreakPoints { get; set; }

        public List<Instruction> AllbreakPoints { get; set; }
        public Runmode RunMode { get; set; }
        public int ReturnPCFromDispose { get; set; }

        public Stack<DebugFrame> DebugStackFrame { get; set; }

        public bool DebugStackFrameContains(StackFrameFlagOptions option)
        {
            foreach (DebugFrame debugFrame in DebugStackFrame)
            {
                if(option == StackFrameFlagOptions.FepRun)
                {
                    if (debugFrame.FepRun == 1)
                    {
                        return true;
                    }
                }
                else if (option == StackFrameFlagOptions.IsReplicating)
                {
                    if(debugFrame.IsReplicating)
                    {
                        return true;
                    }
                }
                else if (option == StackFrameFlagOptions.IsExternalFunction)
                {
                    if (debugFrame.IsExternalFunction)
                    {
                        return true;
                    }
                }
                else if (option == StackFrameFlagOptions.IsFunctionStepOver)
                {
                    if (debugFrame.FunctionStepOver)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int FindEndPCForAssocGraphNode(int tempPC, InstructionStream istream, ProcedureNode fNode, GraphNode graphNode, bool handleSSATemps)
        {
            int limit = Constants.kInvalidIndex;
            //AssociativeGraph.GraphNode currentGraphNode = executingGraphNode;
            GraphNode currentGraphNode = graphNode;
            //Validity.Assert(currentGraphNode != null);

            if (currentGraphNode != null)
            {
                if (tempPC < currentGraphNode.updateBlock.startpc || tempPC > currentGraphNode.updateBlock.endpc)
                {
                    //   return false;
                    return Constants.kInvalidIndex;
                }

                int i = currentGraphNode.dependencyGraphListID;
                GraphNode nextGraphNode = currentGraphNode;
                while (currentGraphNode.exprUID != Constants.kInvalidIndex 
                        && currentGraphNode.exprUID == nextGraphNode.exprUID)

                {
                    limit = nextGraphNode.updateBlock.endpc;
                    if (++i < istream.dependencyGraph.GraphList.Count)
                    {
                        nextGraphNode = istream.dependencyGraph.GraphList[i];
                    }
                    else
                    {
                        break;
                    }

                    // Is it the next statement 
                    // This check will be deprecated on full SSA
                    if (handleSSATemps)
                    {
                        if (!nextGraphNode.IsSSANode())
                        {
                            // The next graphnode is nolonger part of the current statement 
                            // This is the end pc needed to run until
                            nextGraphNode = istream.dependencyGraph.GraphList[i];
                            limit = nextGraphNode.updateBlock.endpc;
                            break;
                        }
                    }
                }
            }
            // If graph node is null in associative lang block, it either is the very first property declaration or
            // it is the very first or only function call statement ("return = f();") inside the calling function
            // Here there's most likely a DEP or RETURN respectively after the function call
            // in which case, search for the instruction and set that as the new pc limit
            else if (!fNode.name.Contains(Constants.kSetterPrefix))
            {
                while (++tempPC < istream.instrList.Count)
                {
                    Instruction instr = istream.instrList[tempPC];
                    if (instr.opCode == OpCode.DEP || instr.opCode == OpCode.RETURN)
                    {
                        limit = tempPC;
                        break;
                    }
                }
            }
            return limit;
        }

        public void SetUpBounce(DSASM.Executive exec, int exeblock, int returnAddr)
        {
            DebugFrame debugFrame = new DebugFrame();

            // TODO: Replace FepRun with StackFrameTypeinfo from Core.Rmem.Stack - pratapa
            debugFrame.FepRun = 0;
            debugFrame.IsResume = false;

            if (exec != null)
            {
                debugFrame.ExecutingGraphNode = exec.Properties.executingGraphNode;
                
            }
            else
                debugFrame.ExecutingGraphNode = null;

            DebugStackFrame.Push(debugFrame);
        }

        private void SetUpCallr(ref DebugFrame debugFrame, bool isReplicating, bool isExternalFunc, DSASM.Executive exec, int fepRun = 1)
        {
            // There is no corresponding RETURN instruction for external functions such as FFI's and dot calls
            //if (procNode.name != DSDefinitions.Kw.kw_Dispose)
            {
                debugFrame.IsExternalFunction = isExternalFunc;
                debugFrame.IsReplicating = isReplicating;

                // TODO: Replace FepRun with StackFrameTypeinfo from Core.Rmem.Stack - pratapa
                debugFrame.FepRun = fepRun;
                debugFrame.IsResume = false;
                debugFrame.ExecutingGraphNode = exec.Properties.executingGraphNode;
                
            }
        }

        public void SetUpCallrForDebug(Core core, DSASM.Executive exec, ProcedureNode fNode, int pc, bool isBaseCall = false,
            CallSite callsite = null, List<StackValue> arguments = null, List<List<ReplicationGuide>> replicationGuides = null, StackFrame stackFrame = null,
            List<StackValue> dotCallDimensions = null, bool hasDebugInfo = false, bool isMember = false, StackValue? thisPtr = null)
        {
            //ProtoCore.DSASM.Executive exec = core.CurrentExecutive.CurrentDSASMExec;

            DebugFrame debugFrame = new DebugFrame();
            debugFrame.IsBaseCall = isBaseCall;
            debugFrame.Arguments = arguments;
            debugFrame.IsMemberFunction = isMember;
            debugFrame.ThisPtr = thisPtr;
            debugFrame.HasDebugInfo = hasDebugInfo;

            if (CoreUtils.IsDisposeMethod(fNode.name))
            {
                debugFrame.IsDisposeCall = true;
                ReturnPCFromDispose = DebugEntryPC;
            }

            if (RunMode == Runmode.StepNext)
            {
                debugFrame.FunctionStepOver = true;
            }

            bool isReplicating = false;
            bool isExternalFunction = false;
            
            // callsite is set to null for a base class constructor call in CALL
            if (callsite == null)
            {
                isReplicating = false;
                isExternalFunction = false;
                
                SetUpCallr(ref debugFrame, isReplicating, isExternalFunction, exec);
                DebugStackFrame.Push(debugFrame);

                return;
            }

            // Comment Jun: A dot call does not replicate and  must be handled immediately
            if (fNode.name == Constants.kDotMethodName)
            {
                isReplicating = false;
                isExternalFunction = false;
                debugFrame.IsDotCall = true;
                debugFrame.DotCallDimensions = dotCallDimensions;
                
                SetUpCallr(ref debugFrame, isReplicating, isExternalFunction, exec);
                DebugStackFrame.Push(debugFrame);

                return;
            }

            List<List<ReplicationInstruction>> replicationTrials;
            bool willReplicate = callsite.WillCallReplicate(new Context(), arguments, replicationGuides, stackFrame, core, out replicationTrials);
            
            // the inline conditional built-in is handled separately as 'WillCallReplicate' is always true in this case
            if(fNode.name.Equals(Constants.kInlineConditionalMethodName))
            {
                // The inline conditional built-in is created only for associative blocks and needs to be handled separately as below
                InstructionStream istream = core.DSExecutable.instrStreamList[CurrentBlockId];
                Validity.Assert(istream.language == Language.kAssociative);
                {
                    core.DebugProps.InlineConditionOptions.isInlineConditional = true;
                    core.DebugProps.InlineConditionOptions.startPc = pc;

                    core.DebugProps.InlineConditionOptions.endPc = FindEndPCForAssocGraphNode(pc, istream, fNode, exec.Properties.executingGraphNode, core.Options.ExecuteSSA);


                    core.DebugProps.InlineConditionOptions.instructionStream = core.RunningBlock;
                    debugFrame.IsInlineConditional = true;
                }
                
                // no replication case
                if (willReplicate && replicationTrials.Count == 1)
                {
                    core.DebugProps.InlineConditionOptions.ActiveBreakPoints.AddRange(core.Breakpoints);

                    /*if (core.DebugProps.RunMode == Runmode.StepNext)
                    {
                        core.Breakpoints.Clear();
                    }*/

                    isReplicating = false;
                    isExternalFunction = false;
                }
                else // an inline conditional call that replicates
                {
#if !__DEBUG_REPLICATE
                    // Clear all breakpoints for outermost replicated call
                    if(!DebugStackFrameContains(StackFrameFlagOptions.IsReplicating))
                    {
                        ActiveBreakPoints.AddRange(core.Breakpoints);
                        core.Breakpoints.Clear();
                    }
#endif
                    isExternalFunction = false;
                    isReplicating = true;
                }
                SetUpCallr(ref debugFrame, isReplicating, isExternalFunction, exec, 0);
                
                DebugStackFrame.Push(debugFrame);

                return;
            }            
            // Prevent breaking inside a function that is external except for dot calls
            // by clearing all breakpoints from outermost external function call
            // This check takes precedence over the replication check
            else if (fNode.isExternal && fNode.name != Constants.kDotMethodName)
            {
                // Clear all breakpoints 
                if (!DebugStackFrameContains(StackFrameFlagOptions.IsExternalFunction) && fNode.name != Constants.kFunctionRangeExpression)
                {
                    ActiveBreakPoints.AddRange(core.Breakpoints);
                    core.Breakpoints.Clear();
                }

                isExternalFunction = true;
                isReplicating = false;
            }
            // Find if function call will replicate or not and if so
            // prevent stepping in by removing all breakpoints from outermost replicated call
            else if (willReplicate)
            {
#if !__DEBUG_REPLICATE
                // Clear all breakpoints for outermost replicated call
                if(!DebugStackFrameContains(StackFrameFlagOptions.IsReplicating))
                {
                    ActiveBreakPoints.AddRange(core.Breakpoints);
                    core.Breakpoints.Clear();
                }
#endif

                isReplicating = true;
                isExternalFunction = false;
            }
            // For all other function calls
            else
            {
                isReplicating = false;
                isExternalFunction = false;
            }

            SetUpCallr(ref debugFrame, isReplicating, isExternalFunction, exec);
            DebugStackFrame.Push(debugFrame);
        }

        /// <summary>
        /// Called only when we step over a function (including replicated and external functions) 
        /// Pops Debug stackframe and Restores breakpoints 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fNode"></param>
        /// <param name="isReplicating"></param>
        public void RestoreCallrForNoBreak(Core core, ProcedureNode fNode, bool isReplicating = false)
        {
            Validity.Assert(DebugStackFrame.Count > 0);
            
            // All functions that reach this point are restored here as they have not been
            // done so in RETURN/RETC            
            DebugFrame debugFrame = DebugStackFrame.Pop();

            // Restore breakpoints which occur after returning from outermost replicating function call 
            // as well as outermost external function call
#if !__DEBUG_REPLICATE
            if (!DebugStackFrameContains(StackFrameFlagOptions.IsReplicating) &&
                !DebugStackFrameContains(StackFrameFlagOptions.IsExternalFunction))
            {
                if (ActiveBreakPoints.Count > 0 && fNode.name != Constants.kFunctionRangeExpression)
                {
                    core.Breakpoints.AddRange(ActiveBreakPoints);
                    //if (SetUpStepOverFunctionCalls(core, fNode, ActiveBreakPoints))
                    {
                        ActiveBreakPoints.Clear();
                    }
                }
            }
#else
            if (!DebugStackFrameContains(StackFrameFlagOptions.IsExternalFunction))
            {
                if (ActiveBreakPoints.Count > 0 && fNode.name != ProtoCore.DSASM.Constants.kFunctionRangeExpression)
                {
                    core.Breakpoints.AddRange(ActiveBreakPoints);
                    //if (SetUpStepOverFunctionCalls(core, fNode, ActiveBreakPoints))
                    {
                        ActiveBreakPoints.Clear();
                    }
                }
            }
#endif

#if __DEBUG_REPLICATE
            if(!isReplicating)
#endif
            {
                // If stepping over function call in debug mode
                if (debugFrame.HasDebugInfo && RunMode == Runmode.StepNext)
                {
                    // if stepping over outermost function call
                    if (!DebugStackFrameContains(StackFrameFlagOptions.IsFunctionStepOver))
                    {
                        SetUpStepOverFunctionCalls(core, fNode, debugFrame.ExecutingGraphNode, debugFrame.HasDebugInfo);
                    }
                }
            }
        }
                
        public void SetUpStepOverFunctionCalls(Core core, ProcedureNode fNode, GraphNode graphNode, bool hasDebugInfo)
        {
            int tempPC = DebugEntryPC;
            int limit = 0;  // end pc of current expression
            InstructionStream istream;

            int pc = tempPC;
            if (core.DebugProps.InlineConditionOptions.isInlineConditional)
            {
                tempPC = InlineConditionOptions.startPc;
                limit = InlineConditionOptions.endPc;
                istream = core.DSExecutable.instrStreamList[InlineConditionOptions.instructionStream];
            }
            else
            {
                pc = tempPC;
                istream = core.DSExecutable.instrStreamList[core.RunningBlock];
                if (istream.language == Language.kAssociative)
                {
                    limit = FindEndPCForAssocGraphNode(pc, istream, fNode, graphNode, core.Options.ExecuteSSA);
                    //Validity.Assert(limit != ProtoCore.DSASM.Constants.kInvalidIndex);
                }
                else if (istream.language == Language.kImperative)
                {
                    // Check for 'SETEXPUID' instruction to check for end of expression
                    while (++pc < istream.instrList.Count)
                    {
                        Instruction instr = istream.instrList[pc];
                        if (instr.opCode == OpCode.SETEXPUID)
                        {
                            limit = pc;
                            break;
                        }
                    }
                }
            }

            // Determine if this is outermost CALLR in the expression
            // until then do not restore any breakpoints
            // If outermost CALLR, restore breakpoints after end of expression
            pc = tempPC;
            int numNestedFunctionCalls = 0;
            while (++pc <= limit)
            {
                Instruction instr = istream.instrList[pc];
                if (instr.opCode == OpCode.CALLR && instr.debug != null)
                {
                    numNestedFunctionCalls++;
                }
            }
            if (numNestedFunctionCalls == 0)
            {
                // If this is the outermost function call 
                core.Breakpoints.Clear();
                core.Breakpoints.AddRange(AllbreakPoints);

                pc = tempPC;
                while (++pc <= limit)
                {
                    Instruction instr = istream.instrList[pc];
                    // We still want to break at the closing brace of a function or ctor call or language block
                    if (instr.debug != null && instr.opCode != OpCode.RETC && instr.opCode != OpCode.RETURN && 
                        (instr.opCode != OpCode.RETB)) 
                    {
                        if (core.Breakpoints.Contains(instr))
                            core.Breakpoints.Remove(instr);
                    }
                }
            }
        }
    }

    public class ExecutionStateEventArgs : EventArgs
    {
        public enum State
        {
            kInvalid = -1,
            kExecutionBegin,
            kExecutionEnd,
            kExecutionBreak,
            kExecutionResume,
        }

        public ExecutionStateEventArgs(State state)
        {
            ExecutionState = state;
        }

        public State ExecutionState { get; private set; }
    }

    public enum ParseMode
    {
        Normal,
        AllowNonAssignment,
        None
    }

    public class Core
    {

        /// <summary>
        /// Properties in under COMPILER_GENERATED_TO_RUNTIME_DATA, are generated at compile time, and passed to RuntimeData/Exe
        /// Only Core can initialize these
        /// </summary>
#region COMPILER_GENERATED_TO_RUNTIME_DATA

        public LangVerify Langverify { get; private set; }
        public FunctionTable FunctionTable { get; private set; }

        public RuntimeData RuntimeData { get; set; }

        #endregion

        // This flag is set true when we call GraphUtilities.PreloadAssembly to load libraries in Graph UI
        public bool IsParsingPreloadedAssembly { get; set; }
        
        // THe ImportModuleHandler owned by the temporary core used in Graph UI precompilation
        // needed to detect if the same assembly is not being imported more than once
        public ImportModuleHandler ImportHandler { get; set; }
        
        // This is set to true when the temporary core is used for precompilation of CBN's in GraphUI
        public bool IsParsingCodeBlockNode { get; set; }

        // This is the AST node list of default imported libraries needed for Graph Compiler
        public CodeBlockNode ImportNodes { get; set; }

        // The root AST node obtained from parsing an expression in a Graph node in GraphUI
        public List<Node> AstNodeList { get; set; }


        public enum ErrorType
        {
            OK,
            Error,
            Warning
        }

        public struct ErrorEntry
        {
            public ErrorType Type;
            public string FileName;
            public string Message;
            public BuildData.WarningID BuildId;
            public Runtime.WarningID RuntimeId;
            public int Line;
            public int Col;
        }

        public Dictionary<ulong, ulong> codeToLocation = new Dictionary<ulong, ulong>();
        public Dictionary<ulong, ErrorEntry> LocationErrorMap = new Dictionary<ulong, ErrorEntry>();

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

        public Dictionary<Language, Executive> Executives { get; private set; }

        public Executive CurrentExecutive { get; private set; }
        public int GlobOffset { get; set; }
        public int GlobHeapOffset { get; set; }
        public int BaseOffset { get; set; }
        public int GraphNodeUID { get; set; }

        public Heap Heap { get; set; }
        public RuntimeMemory Rmem { get; set; }

        public int ClassIndex { get; set; }     // Holds the current class scope
        public int RunningBlock { get; set; }
        public int CodeBlockIndex { get; set; }
        public int RuntimeTableIndex { get; set; }


        public List<CodeBlock> CodeBlockList { get; set; }
        // The Complete Code Block list contains all the code blocks
        // unlike the codeblocklist which only stores the outer most code blocks
        public List<CodeBlock> CompleteCodeBlockList { get; set; }

        /// <summary>
        /// ForLoopBlockIndex tracks the current number of new for loop blocks created at compile time for every new compile phase
        /// It is reset for delta compilation
        /// </summary>
        public int ForLoopBlockIndex { get; set; }

        public Executable DSExecutable { get; set; }

        public List<Instruction> Breakpoints { get; set; }

        public Options Options { get; private set; }
        public BuildStatus BuildStatus { get; private set; }
        public RuntimeStatus RuntimeStatus { get; private set; }

        public TypeSystem TypeSystem { get; set; }

        // The global class table and function tables
        public ClassTable ClassTable { get; set; }
        public ProcedureTable ProcTable { get; set; }
        public ProcedureNode ProcNode { get; set; }

        // The function pointer table
        public FunctionPointerTable FunctionPointerTable { get; set; }

        //The dynamic string table and function table
        public DynamicVariableTable DynamicVariableTable { get; set; }
        public DynamicFunctionTable DynamicFunctionTable { get; set; }

        public IExecutiveProvider ExecutiveProvider { get; set; }

        public Dictionary<string, object> Configurations { get; set; }

        //Manages injected context data.
        internal ContextDataManager ContextDataManager { get; set; }

        public ParseMode ParsingMode { get; set; }

        public FFIPropertyChangedMonitor FFIPropertyChangedMonitor { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void AddContextData(Dictionary<string, Object> data)
        {
            if (data == null)
                return;

            ContextDataManager.GetInstance(this).AddData(data);
        }

        // Cached replication guides for the current call. 
        // TODO Jun: Store this in the dynamic table node
        public List<List<ReplicationGuide>> replicationGuides;

        // if CompileToLib is true, this is used to output the asm instruction to the dsASM file
        // if CompilerToLib is false, this will be set to Console.Out
        public TextWriter AsmOutput;
        public int AsmOutputIdents;

        public string CurrentDSFileName { get; set; }
        // this field is used to store the inferedtype information  when the code gen cross one langeage to another 
        // otherwize the inferedtype information will be lost
        public Type InferedType;

        public DebugProperties DebugProps;
        
        public Stack<InterpreterProperties> InterpreterProps { get; set; }

        // Continuation properties used for Serial mode execution and Debugging of Replicated calls
        public ContinuationStructure ContinuationStruct { get; set; }

        /// <summary>
        /// Gets the reason why the execution was last suspended
        /// </summary>
        public ReasonForExecutionSuspend ReasonForExecutionSuspend { get; internal set; }


        public delegate void DisposeDelegate(Core sender);
        public event DisposeDelegate Dispose;
        public event EventHandler<ExecutionStateEventArgs> ExecutionEvent;

        public int ExecutionState { get; set; }

        public bool builtInsLoaded { get; set; }
        public List<string> LoadedDLLs = new List<string>();
        public int deltaCompileStartPC { get; set; }


        // A list of graphnodes that contain a function call
        public List<GraphNode> GraphNodeCallList { get; set; }

        public int newEntryPoint { get; private set; }

        public void SetNewEntryPoint(int pc)
        {
            newEntryPoint = pc;
        }

        /// <summary>
        /// Sets the function to an inactive state where it can no longer be used by the front-end and backend
        /// </summary>
        /// <param name="functionDef"></param>
        public void SetFunctionInactive(FunctionDefinitionNode functionDef)
        {
            // DS language only supports function definition on the global and first language block scope 
            // TODO Jun: Determine if it is still required to combine function tables in the codeblocks and callsite

            // Update the functiond definition in the codeblocks
            int hash = CoreUtils.GetFunctionHash(functionDef);

            ProcedureNode procNode = null;

            foreach (CodeBlock block in CodeBlockList)
            {
                // Update the current function definition in the current block
                int index = block.procedureTable.IndexOfHash(hash);
                if (Constants.kInvalidIndex == index)
                    continue;

                procNode = block.procedureTable.procList[index];

                block.procedureTable.SetInactive(index);

                // Remove staled graph nodes
                var graph = block.instrStream.dependencyGraph;
                graph.GraphList.RemoveAll(g => g.classIndex == ClassIndex && 
                                               g.procIndex == index);
                graph.RemoveNodesFromScope(Constants.kGlobalScope, index);

                // Make a copy of all symbols defined in this function
                var localSymbols = block.symbolTable.symbolList.Values
                                        .Where(n => 
                                                n.classScope == Constants.kGlobalScope 
                                             && n.functionIndex == index)
                                        .ToList();

                foreach (var symbol in localSymbols)
                {
                    block.symbolTable.UndefineSymbol(symbol);
                }

                break;
            }

            if (null != procNode)
            {
                foreach (int cbID in procNode.ChildCodeBlocks)
                {
                    CompleteCodeBlockList.RemoveAll(x => x.codeBlockId == cbID);
                }
            }


            // Update the function definition in global function tables
            foreach (KeyValuePair<int, Dictionary<string, FunctionGroup>> functionGroupList in DSExecutable.RuntimeData.FunctionTable.GlobalFuncTable)
            {
                foreach (KeyValuePair<string, FunctionGroup> functionGroup in functionGroupList.Value)
                {
                    functionGroup.Value.FunctionEndPoints.RemoveAll(func => func.procedureNode.HashID == hash);
                }
            }
        }

        public void NotifyExecutionEvent(ExecutionStateEventArgs.State state)
        {
            switch (state)
            {
                case ExecutionStateEventArgs.State.kExecutionBegin:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.kInvalid, "Invalid Execution state being notified.");
                    break;
                case ExecutionStateEventArgs.State.kExecutionEnd:
                    if (ExecutionState == (int)ExecutionStateEventArgs.State.kInvalid) //execution never begun.
                        return;
                    break;
                case ExecutionStateEventArgs.State.kExecutionBreak:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.kExecutionBegin || ExecutionState == (int)ExecutionStateEventArgs.State.kExecutionResume, "Invalid Execution state being notified.");
                    break;
                case ExecutionStateEventArgs.State.kExecutionResume:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.kExecutionBreak, "Invalid Execution state being notified.");
                    break;
                default:
                    Validity.Assert(false, "Invalid Execution state being notified.");
                    break;
            }
            ExecutionState = (int)state;
            if (null != ExecutionEvent)
                ExecutionEvent(this, new ExecutionStateEventArgs(state));
        }

        public class CodeBlockCompilationSnapshot
        {
            public CodeBlockCompilationSnapshot(int codeBlocKId, int graphNodeCount, int endPC)
            {
                CodeBlockId = codeBlocKId;
                GraphNodeCount = graphNodeCount;
                InstructionCount = endPC;
            }

            public static List<CodeBlockCompilationSnapshot> CaptureCoreCompileState(Core core)
            {
                List<CodeBlockCompilationSnapshot> snapShots = new List<CodeBlockCompilationSnapshot>();
                if (core.CodeBlockList != null)
                {
                    foreach (var codeBlock in core.CodeBlockList)
                    {
                        int codeBlockId = codeBlock.codeBlockId;
                        InstructionStream istream = core.CodeBlockList[codeBlockId].instrStream;
                        int graphCount = istream.dependencyGraph.GraphList.Count;
                        int instructionCount = istream.instrList.Count;

                        snapShots.Add(new CodeBlockCompilationSnapshot(codeBlockId, graphCount, instructionCount));
                    }
                }
                return snapShots;
            }

            public int CodeBlockId { get; set;} 
            public int GraphNodeCount { get; set;} 
            public int InstructionCount { get; set;}
        }

        public void ResetDeltaCompileFromSnapshot(List<CodeBlockCompilationSnapshot> snapShots)
        {
            if (snapShots == null)
                throw new ArgumentNullException("snapshots");

            foreach (var snapShot in snapShots)
            {
                InstructionStream istream = CodeBlockList[snapShot.CodeBlockId].instrStream;

                int instrCount = istream.instrList.Count - snapShot.InstructionCount;
                if (instrCount > 0)
                {
                    istream.instrList.RemoveRange(snapShot.InstructionCount, instrCount);
                }

                int graphNodeCount = istream.dependencyGraph.GraphList.Count - snapShot.GraphNodeCount;
                if (graphNodeCount > 0)
                {
                    istream.dependencyGraph.GraphList.RemoveRange(snapShot.GraphNodeCount, graphNodeCount);
                }
            }
        }

        /// <summary>
        /// Reset the VM state for delta execution.
        /// </summary>
        public void ResetForDeltaExecution()
        {
            Options.ApplyUpdate = false;

            ExecMode = InterpreterMode.kNormal;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid;
            RunningBlock = 0;

            // The main codeblock never goes out of scope
            // Resetting CodeBlockIndex means getting the number of main codeblocks that dont go out of scope.
            // As of the current requirements, there is only 1 main scope, the rest are nested within.
            CodeBlockIndex = CodeBlockList.Count;
            RuntimeTableIndex = CodeBlockIndex;

            ForLoopBlockIndex = Constants.kInvalidIndex;

            // Jun this is where the temp solutions starts for implementing language blocks in delta execution
            for (int n = 1; n < CodeBlockList.Count; ++n)
            {
                CodeBlockList[n].instrStream.instrList.Clear();
            }

            // Remove inactive graphnodes in the list
            GraphNodeCallList.RemoveAll(g => !g.isActive);
            ExprInterpreterExe = null;
        }

        public void ResetForPrecompilation()
        {
            GraphNodeUID = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            
            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DynamicVariableTable();
            DynamicFunctionTable = new DynamicFunctionTable();

            if (Options.SuppressBuildOutput)
            {
                //  don't log any of the build related messages
                //  just accumulate them in relevant containers with
                //  BuildStatus object
                //
                BuildStatus = new BuildStatus(this, false, false, false);
            }
            else
            {
                BuildStatus = new BuildStatus(this, Options.BuildOptWarningAsError);
            }
            
            if (AstNodeList != null) 
                AstNodeList.Clear();

            ExpressionUID = 0;
            ForLoopBlockIndex = Constants.kInvalidIndex;
        }

        private void ResetAll(Options options)
        {
            this.RuntimeData = new ProtoCore.RuntimeData();

            Validity.AssertExpiry();
            Options = options;
            Executives = new Dictionary<Language, Executive>();
            ClassIndex = Constants.kInvalidIndex;

            FunctionTable = new FunctionTable(); 
            Langverify = new LangVerify();

            Heap = new Heap();
            Rmem = new RuntimeMemory(Heap);

            watchClassScope = Constants.kInvalidIndex;
            watchFunctionScope = Constants.kInvalidIndex;
            watchBaseOffset = 0;
            watchStack = new List<StackValue>();
            watchSymbolList = new List<SymbolNode>();
            watchFramePointer = Constants.kInvalidIndex;


            GlobOffset = 0;
            GlobHeapOffset = 0;
            BaseOffset = 0;
            GraphNodeUID = 0;
            RunningBlock = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            CodeBlockList = new List<CodeBlock>();
            CompleteCodeBlockList = new List<CodeBlock>();
            DSExecutable = new Executable();

            AssocNode = null;

            // TODO Jun/Luke type system refactoring
            // Initialize the globalClass table and type system
            ClassTable = new ClassTable();
            TypeSystem = new TypeSystem();
            TypeSystem.SetClassTable(ClassTable);
            ProcNode = null;
            ProcTable = new ProcedureTable(Constants.kGlobalScope);

            //Initialize the function pointer table
            FunctionPointerTable = new FunctionPointerTable();

            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DynamicVariableTable();
            DynamicFunctionTable = new DynamicFunctionTable();
            replicationGuides = new List<List<ReplicationGuide>>();

            startPC = Constants.kInvalidIndex;

            deltaCompileStartPC = Constants.kInvalidIndex;

            if (options.SuppressBuildOutput)
            {
                //  don't log any of the build related messages
                //  just accumulate them in relevant containers with
                //  BuildStatus object
                //
                BuildStatus = new BuildStatus(this, false, false, false);
            }
            else
            {
                BuildStatus = new BuildStatus(this, Options.BuildOptWarningAsError, null, Options.BuildOptErrorAsWarning);
            }
            RuntimeStatus = new RuntimeStatus(this);

            SSASubscript = 0;
            SSASubscript_GUID = Guid.NewGuid();
            ExpressionUID = 0;
            ModifierBlockUID = 0;
            ModifierStateSubscript = 0;

            ExprInterpreterExe = null;
            ExecMode = InterpreterMode.kNormal;

            assocCodegen = null;
            FunctionCallDepth = 0;

            // Default execution log is Console.Out.
            ExecutionLog = Console.Out;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid; //not yet started

            DebugProps = new DebugProperties();
            InterpreterProps = new Stack<InterpreterProperties>();

            ExecutiveProvider = new ExecutiveProvider();

            Configurations = new Dictionary<string, object>();

            ContinuationStruct = new ContinuationStructure();
            ParsingMode = ParseMode.Normal;
            
            IsParsingPreloadedAssembly = false;
            IsParsingCodeBlockNode = false;
            ImportHandler = null;

            deltaCompileStartPC = 0;
            builtInsLoaded = false;
            FFIPropertyChangedMonitor = new FFIPropertyChangedMonitor(this);


            ForLoopBlockIndex = Constants.kInvalidIndex;

            GraphNodeCallList = new List<GraphNode>();

            newEntryPoint = Constants.kInvalidIndex;
            cancellationPending = false;
        }

        // The unique subscript for SSA temporaries
        // TODO Jun: Organize these variables in core into proper enums/classes/struct
        public int SSASubscript { get; set; }
        public Guid SSASubscript_GUID { get; set; }

        /// <summary> 
        /// ExpressionUID is used as the unique id to identify an expression
        /// It is incremented by 1 after mapping tis current value to an expression
        /// </summary>
        public int ExpressionUID { get; set; }

        /// <summary>
        /// RuntimeExpressionUID is used by the associative engine at runtime to determine the current expression ID being executed
        /// </summary>
        public int RuntimeExpressionUID = 0;

        public int ModifierBlockUID { get; set; }
        public int ModifierStateSubscript { get; set; }

        private int tempVarId = 0;
        private int tempLanguageId = 0;

        private bool cancellationPending = false;
        public bool CancellationPending
        {
            get
            {
                return cancellationPending;
            }
        }

        // TODO Jun: Cleansify me - i dont need to be here
        public AssociativeNode AssocNode { get; set; }
        public int startPC { get; set; }


        //
        // TODO Jun: This is the expression interpreters executable. 
        //           It must be moved to its own core, whre each core is an instance of a compiler+interpreter
        //
        public Executable ExprInterpreterExe { get; set; }
        public InterpreterMode ExecMode { get; set; }
        public List<SymbolNode> watchSymbolList { get; set; }
        public int watchClassScope { get; set; }
        public int watchFunctionScope { get; set; }
        public int watchBaseOffset { get; set; }
        public List<StackValue> watchStack { get; set; }
        public int watchFramePointer { get; set; }

        public CodeGen assocCodegen { get; set; }

        // this one is to address the issue that when the execution control is in a language block
        // which is further inside a function, the compiler feprun is false, 
        // when inspecting value in that language block or the function, debugger will assume the function index is -1, 
        // name look up will fail beacuse all the local variables inside 
        // that language block and fucntion has non-zero function index 
        public int FunctionCallDepth { get; set; }
        public TextWriter ExecutionLog { get; set; }

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

        public void InitializeContextGlobals(Dictionary<string, object> context)
        {
            int globalBlock = 0;
            foreach (KeyValuePair<string, object> global in context)
            {
                int stackIndex = CodeBlockList[globalBlock].symbolTable.IndexOf(global.Key);

                if (global.Value.GetType() != typeof(Double) && global.Value.GetType() != typeof(Int32))
                    throw new NotImplementedException("Context that's aren't double are not yet supported @TODO: Jun,Sharad,Luke ASAP");

                double dValue = Convert.ToDouble(global.Value);
                StackValue svData = StackValue.BuildDouble(dValue);
                Rmem.SetGlobalStackData(stackIndex, svData);
            }
        }

        public Core(Options options)
        {
            ResetAll(options);
        }

        public SymbolNode GetSymbolInFunction(string name, int classScope, int functionScope, CodeBlock codeBlock)
        {
            Validity.Assert(functionScope != Constants.kGlobalScope);
            if (Constants.kGlobalScope == functionScope)
            {
                return null;
            }

            int symbolIndex = Constants.kInvalidIndex;

            if (classScope != Constants.kGlobalScope)
            {
                //Search local variable for the class member function
                symbolIndex = ClassTable.ClassNodes[classScope].symbols.IndexOf(name, classScope, functionScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return ClassTable.ClassNodes[classScope].symbols.symbolList[symbolIndex];
                }

                //Search class members
                symbolIndex = ClassTable.ClassNodes[classScope].symbols.IndexOf(name, classScope, Constants.kGlobalScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return ClassTable.ClassNodes[classScope].symbols.symbolList[symbolIndex];
                }
            }

            while (symbolIndex == Constants.kInvalidIndex &&
                   codeBlock != null &&
                   codeBlock.blockType != CodeBlockType.kFunction)
            {
                symbolIndex = codeBlock.symbolTable.IndexOf(name, classScope, functionScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return codeBlock.symbolTable.symbolList[symbolIndex];
                }
                else
                {
                    codeBlock = codeBlock.parent;
                }
            }

            if (symbolIndex == Constants.kInvalidIndex &&
                codeBlock != null &&
                codeBlock.blockType == CodeBlockType.kFunction)
            {
                symbolIndex = codeBlock.symbolTable.IndexOf(name, classScope, functionScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return codeBlock.symbolTable.symbolList[symbolIndex];
                }
            }

            return null;
        }

        public SymbolNode GetFirstVisibleSymbol(string name, int classscope, int function, CodeBlock codeblock)
        {
            //  
            //

            Validity.Assert(null != codeblock);
            if (null == codeblock)
            {
                return null;
            }

            int symbolIndex = Constants.kInvalidIndex;
            bool stillInsideFunction = function != Constants.kInvalidIndex;
            CodeBlock searchBlock = codeblock;
            // TODO(Jiong): Code Duplication, Consider moving this if else block inside the while loop 
            if (stillInsideFunction)
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, function);

                if (function != Constants.kInvalidIndex &&
                    searchBlock.procedureTable != null &&
                    searchBlock.procedureTable.procList.Count > function &&   // Note: This check assumes we can not define functions inside a fucntion 
                    symbolIndex == Constants.kInvalidIndex)
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, Constants.kInvalidIndex);
            }
            else
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, Constants.kInvalidIndex);
            }
            while (Constants.kInvalidIndex == symbolIndex)
            {
                // if the search block is of type function, it means our search has gone out of the function itself
                // so, we should ignore the given function index and only search its parent block's global variable
                if (searchBlock.blockType == CodeBlockType.kFunction)
                    stillInsideFunction = false;

                searchBlock = searchBlock.parent;
                if (null == searchBlock)
                {
                    return null;
                }

                // Continue searching
                if (stillInsideFunction)
                {
                    // we are still inside a function, first search the local variable defined in this function 
                    // if not found, then search the enclosing block by specifying the function index as -1
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, function);

                    // this check is to avoid unnecessary search
                    // for example if we have a for loop inside an imperative block which is further inside a function
                    // when we are searching inside the for loop or language block, there is no need to search twice
                    // we need to search twice only when we are searching directly inside the function, 
                    if (function != Constants.kInvalidIndex &&
                        searchBlock.procedureTable != null &&
                        searchBlock.procedureTable.procList.Count > function && // Note: This check assumes we can not define functions inside a fucntion 
                        symbolIndex == Constants.kInvalidIndex)

                        symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, Constants.kInvalidIndex);

                }
                else
                {
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, Constants.kInvalidIndex);
                }
            }
            return searchBlock.symbolTable.symbolList[symbolIndex];
        }

        public bool IsFunctionCodeBlock(CodeBlock cblock)
        {
            // Determine if the immediate block is a function block
            // Construct blocks are ignored
            Validity.Assert(null != cblock);
            while (null != cblock)
            {
                if (CodeBlockType.kFunction == cblock.blockType)
                {
                    return true;
                }
                else if (CodeBlockType.kLanguage == cblock.blockType)
                {
                    return false;
                }
                cblock = cblock.parent;
            }
            return false;
        }

        public ProcedureNode GetFirstVisibleProcedure(string name, List<Type> argTypeList, CodeBlock codeblock)
        {
            Validity.Assert(null != codeblock);
            if (null == codeblock)
            {
                return null;
            }

            CodeBlock searchBlock = codeblock;
            while (null != searchBlock)
            {
                if (null == searchBlock.procedureTable)
                {
                    searchBlock = searchBlock.parent;
                    continue;
                }

                // The class table is passed just to check for coercion values
                int procIndex = searchBlock.procedureTable.IndexOf(name, argTypeList);
                if (Constants.kInvalidIndex != procIndex)
                {
                    return searchBlock.procedureTable.procList[procIndex];
                }
                searchBlock = searchBlock.parent;
            }
            return null;
        }

        public CodeBlock GetCodeBlock(List<CodeBlock> blockList, int blockId)
        {
            CodeBlock codeblock = null;
            codeblock = blockList.Find(x => x.codeBlockId == blockId);
            if (codeblock == null)
            {
                foreach (CodeBlock block in blockList)
                {
                    codeblock = GetCodeBlock(block.children, blockId);
                    if (codeblock != null)
                    {
                        break;
                    }
                }
            }
            return codeblock;
        }

        public StackValue Bounce(int exeblock, int entry, Context context, StackFrame stackFrame, int locals = 0, EventSink sink = null)
        {
            if (stackFrame != null)
            {
                StackValue svThisPtr = stackFrame.ThisPtr;
                int ci = stackFrame.ClassScope;
                int fi = stackFrame.FunctionScope;
                int returnAddr = stackFrame.ReturnPC;
                int blockDecl = stackFrame.FunctionBlock;
                int blockCaller = stackFrame.FunctionCallerBlock;
                StackFrameType callerFrameType = stackFrame.CallerStackFrameType;
                StackFrameType frameType = stackFrame.StackFrameType;
                Validity.Assert(frameType == StackFrameType.kTypeLanguage);

                int depth = stackFrame.Depth;
                int framePointer = stackFrame.FramePointer;
                List<StackValue> registers = stackFrame.GetRegisters();

                Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
            }

            Language id = DSExecutable.instrStreamList[exeblock].language;
            CurrentExecutive = Executives[id];
            StackValue sv = Executives[id].Execute(exeblock, entry, context, sink);
            return sv;
        }

        public StackValue Bounce(int exeblock, int entry, Context context, List<Instruction> breakpoints, StackFrame stackFrame, int locals = 0, 
            DSASM.Executive exec = null, EventSink sink = null, bool fepRun = false)
        {
            if (stackFrame != null)
            {
                StackValue svThisPtr = stackFrame.ThisPtr;
                int ci = stackFrame.ClassScope;
                int fi = stackFrame.FunctionScope;
                int returnAddr = stackFrame.ReturnPC;
                int blockDecl = stackFrame.FunctionBlock;
                int blockCaller = stackFrame.FunctionCallerBlock;
                StackFrameType callerFrameType = stackFrame.CallerStackFrameType;
                StackFrameType frameType = stackFrame.StackFrameType;
                Validity.Assert(frameType == StackFrameType.kTypeLanguage);
                int depth = stackFrame.Depth;
                int framePointer = stackFrame.FramePointer;
                List<StackValue> registers = stackFrame.GetRegisters();

                DebugProps.SetUpBounce(exec, blockCaller, returnAddr);

                Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
            }

            Language id = DSExecutable.instrStreamList[exeblock].language;
            CurrentExecutive = Executives[id];

            StackValue sv = Executives[id].Execute(exeblock, entry, context, breakpoints, sink, fepRun);
            return sv;
        }

        private void BfsBuildSequenceTable(CodeBlock codeBlock, SymbolTable[] runtimeSymbols)
        {
            if (CodeBlockType.kLanguage == codeBlock.blockType
                || CodeBlockType.kFunction == codeBlock.blockType
                || CodeBlockType.kConstruct == codeBlock.blockType)
            {
                Validity.Assert(codeBlock.symbolTable.RuntimeIndex < RuntimeTableIndex);
                runtimeSymbols[codeBlock.symbolTable.RuntimeIndex] = codeBlock.symbolTable;
            }

            foreach (CodeBlock child in codeBlock.children)
            {
                BfsBuildSequenceTable(child, runtimeSymbols);
            }
        }

        private void BfsBuildProcedureTable(CodeBlock codeBlock, ProcedureTable[] procTable)
        {
            if (CodeBlockType.kLanguage == codeBlock.blockType || CodeBlockType.kFunction == codeBlock.blockType)
            {
                Validity.Assert(codeBlock.procedureTable.runtimeIndex < RuntimeTableIndex);
                procTable[codeBlock.procedureTable.runtimeIndex] = codeBlock.procedureTable;
            }

            foreach (CodeBlock child in codeBlock.children)
            {
                BfsBuildProcedureTable(child, procTable);
            }
        }

        private void BfsBuildInstructionStreams(CodeBlock codeBlock, InstructionStream[] istreamList)
        {
            if (null != codeBlock)
            {
                if (CodeBlockType.kLanguage == codeBlock.blockType || CodeBlockType.kFunction == codeBlock.blockType)
                {
                    Validity.Assert(codeBlock.codeBlockId < RuntimeTableIndex);
                    istreamList[codeBlock.codeBlockId] = codeBlock.instrStream;
                }

                foreach (CodeBlock child in codeBlock.children)
                {
                    BfsBuildInstructionStreams(child, istreamList);
                }
            }
        }


        public void GenerateExprExe()
        {
            // TODO Jun: Determine if we really need another executable for the expression interpreter
            Validity.Assert(null == ExprInterpreterExe);
            ExprInterpreterExe = new Executable();

            ExprInterpreterExe.RuntimeData = GenerateRuntimeData();
            // Copy all tables
            ExprInterpreterExe.classTable = DSExecutable.classTable;
            ExprInterpreterExe.procedureTable = DSExecutable.procedureTable;
            ExprInterpreterExe.runtimeSymbols = DSExecutable.runtimeSymbols;
            ExprInterpreterExe.isSingleAssocBlock = DSExecutable.isSingleAssocBlock;
            
            // Copy all instruction streams
            // TODO Jun: What method to copy all? Use that
            ExprInterpreterExe.instrStreamList = new InstructionStream[DSExecutable.instrStreamList.Length];
            for (int i = 0; i < DSExecutable.instrStreamList.Length; ++i)
            {
                if (null != DSExecutable.instrStreamList[i])
                {
                    ExprInterpreterExe.instrStreamList[i] = new InstructionStream(DSExecutable.instrStreamList[i].language, this);
                    //ExprInterpreterExe.instrStreamList[i] = new InstructionStream(DSExecutable.instrStreamList[i].language, DSExecutable.instrStreamList[i].dependencyGraph, this);
                    for (int j = 0; j < DSExecutable.instrStreamList[i].instrList.Count; ++j)
                    {
                        ExprInterpreterExe.instrStreamList[i].instrList.Add(DSExecutable.instrStreamList[i].instrList[j]);
                    }
                }
            }
        }


        public void GenerateExprExeInstructions(int blockScope)
        {
            // Append the expression instruction at the end of the current block
            for (int n = 0; n < ExprInterpreterExe.iStreamCanvas.instrList.Count; ++n)
            {
                ExprInterpreterExe.instrStreamList[blockScope].instrList.Add(ExprInterpreterExe.iStreamCanvas.instrList[n]);
            }
        }

        private RuntimeData GenerateRuntimeData()
        {
            Validity.Assert(RuntimeData != null);
            RuntimeData.FunctionTable = FunctionTable;

            return RuntimeData;
        }

        public void GenerateExecutable()
        {
            Validity.Assert(CodeBlockList.Count >= 0);

            DSExecutable.RuntimeData = GenerateRuntimeData();

            // Retrieve the class table directly since it is a global table
            DSExecutable.classTable = ClassTable;

            RuntimeTableIndex = CompleteCodeBlockList.Count;

            // Build the runtime symbols
            DSExecutable.runtimeSymbols = new SymbolTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildSequenceTable(CodeBlockList[n], DSExecutable.runtimeSymbols);
            }

            // Build the runtime procedure table
            DSExecutable.procedureTable = new ProcedureTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildProcedureTable(CodeBlockList[n], DSExecutable.procedureTable);
            }

            // Build the executable instruction streams
            DSExecutable.instrStreamList = new InstructionStream[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildInstructionStreams(CodeBlockList[n], DSExecutable.instrStreamList);
            }

            // Single associative block means the first instruction is an immediate bounce 
            // This variable is only used by the mirror to determine if the GetValue()
            // block parameter needs to be incremented or not in order to get the correct global variable
            if (DSExecutable.isSingleAssocBlock)
            {
                DSExecutable.isSingleAssocBlock = (OpCode.BOUNCE == CodeBlockList[0].instrStream.instrList[0].opCode) ? true : false;
            }
            GenerateExprExe();
        }



        public string GenerateTempVar()
        {
            tempVarId++;
            return Constants.kTempVar + tempVarId.ToString();
        }


        public string GenerateTempPropertyVar()
        {
            tempVarId++;
            return Constants.kTempPropertyVar + tempVarId.ToString();
        }

        public string GenerateTempLangageVar()
        {
            tempLanguageId++;
            return Constants.kTempLangBlock + tempLanguageId.ToString();
        }

        public bool IsTempVar(String varName)
        {
            if (String.IsNullOrEmpty(varName))
            {
                return false;
            }
            return varName[0] == '%';
        }

        public string GetModifierBlockTemp(string modifierName)
        {
            // The naming convention for auto-generated modifier block states begins with a '%'
            // followed by "<Constants.kTempModifierStateNamePrefix>_<modifier_block_name>_<index>
            string modStateTemp = Constants.kTempModifierStateNamePrefix + modifierName + ModifierStateSubscript.ToString();
            ++ModifierStateSubscript;
            return modStateTemp;
        }

        public List<int> GetAncestorBlockIdsOfBlock(int blockId)
        {
            if (blockId >= CompleteCodeBlockList.Count || blockId < 0)
            {
                return new List<int>();
            }
            CodeBlock thisBlock = CompleteCodeBlockList[blockId];

            var ancestors = new List<int>();
            CodeBlock codeBlock = thisBlock.parent;
            while (codeBlock != null)
            {
                ancestors.Add(codeBlock.codeBlockId);
                codeBlock = codeBlock.parent;
            }
            return ancestors;
        }

        public int GetCurrentBlockId()
        {
            int constructBlockId = Rmem.CurrentConstructBlockId;
            if (constructBlockId == Constants.kInvalidIndex)
                return DebugProps.CurrentBlockId;

            CodeBlock constructBlock = GetCodeBlock(CodeBlockList, constructBlockId);
            while (null != constructBlock && constructBlock.blockType == CodeBlockType.kConstruct)
            {
                constructBlock = constructBlock.parent;
            }

            if (null != constructBlock)
                constructBlockId = constructBlock.codeBlockId;

            if (constructBlockId != DebugProps.CurrentBlockId)
                return DebugProps.CurrentBlockId;
            else
                return Rmem.CurrentConstructBlockId;
        }

        public GraphNode GetExecutingGraphNode()
        {
            return ExecutingGraphnode;
        }

        public bool IsEvalutingPropertyChanged()
        {
            foreach (var prop in InterpreterProps)
            {
                if (prop.updateStatus == UpdateStatus.kPropertyChangedUpdate)
                {
                    return true;
                }
            }

            return false;
        }

        public GraphNode ExecutingGraphnode { get; set; }


        public void ResetSSASubscript(Guid guid, int subscript)
        {
            SSASubscript_GUID = guid;
            SSASubscript = subscript;
        }

        public void RequestCancellation()
        {
            if (cancellationPending)
            {
                var message = "Cancellation cannot be requested twice";
                throw new InvalidOperationException(message);
            }

            cancellationPending = true;
        }
    }
}
