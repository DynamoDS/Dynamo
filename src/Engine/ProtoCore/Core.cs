using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ProtoCore.Exceptions;
using System.Timers;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;
using Autodesk.DesignScript.Interfaces;
using ProtoCore.AssociativeGraph;
using System.Linq;

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
                BeginDocument += p => Console.WriteLine(p);
                EndDocument += p => Console.WriteLine(p);
                PrintMessage += p => Console.WriteLine(p);
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
            this.guideNumber = guide;
            this.isLongest = longest;
        }

        public int guideNumber { get; private set; }
        public bool isLongest {get; private set;}
    }

    public class InterpreterProperties
    {
        public AssociativeGraph.GraphNode executingGraphNode { get; set; }
        public List<ProtoCore.AssociativeGraph.GraphNode> nodeIterations { get; set; }

        public List<StackValue> functionCallArguments { get; set; }
        public List<StackValue> functionCallDotCallDimensions { get; set; }

        public AssociativeEngine.UpdateStatus updateStatus { get; set; }

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
            nodeIterations = new List<AssociativeGraph.GraphNode>();
            functionCallArguments = new List<StackValue>();
            functionCallDotCallDimensions = new List<StackValue>();
            updateStatus = AssociativeEngine.UpdateStatus.kNormalUpdate;
        }
    }

    public class Options
    {
        public Options()
        {

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
            ExecutionMode = ProtoCore.ExecutionMode.Serial;
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
            get { return ProtoCore.Utils.MathUtils.Tolerance; }
            set { ProtoCore.Utils.MathUtils.Tolerance = value; }
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
                    if (System.IO.File.Exists(fileName))
                    {
                        rootCustomPropertyFilterPathName = fileName;

                        System.IO.StreamReader stream = null;
                        try
                        {
                            stream = new System.IO.StreamReader(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.IO.FileLoadException(string.Format("Custom property filter file {0} can't be read. Error Message:{1}", fileName, ex.Message));
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
            IsDotArgCall = false;
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
        public AssociativeGraph.GraphNode ExecutingGraphNode { get; set; }
        public List<StackValue> DotCallDimensions { get; set; }
        public List<StackValue> Arguments { get; set; }
        public StackValue? ThisPtr { get; set; }
        
        // Flag indicating whether execution cursor is being resumed from within the lang block or function
        public bool IsResume { get; set; }
        public bool IsReplicating { get; set; }
        public bool IsExternalFunction { get; set; }
        public bool IsBaseCall { get; set; }
        public bool IsDotCall { get; set; }
        public bool IsDotArgCall { get; set; }
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
            FirstStackFrame = new ProtoCore.DSASM.StackFrame(1);
            
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

        public ProtoCore.DSASM.StackFrame FirstStackFrame { get; set; }

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

        public CodeModel.CodeRange highlightRange = new CodeModel.CodeRange
            {
                StartInclusive = new CodeModel.CodePoint
                {
                    LineNo = Constants.kInvalidIndex,
                    CharNo = Constants.kInvalidIndex
                },

                EndExclusive = new CodeModel.CodePoint
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
        public AssociativeGraph.GraphNode executingGraphNode { get; set; }
        public List<AssociativeGraph.GraphNode> deferedGraphnodes { get; set; }
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
                    if(debugFrame.IsReplicating == true)
                    {
                        return true;
                    }
                }
                else if (option == StackFrameFlagOptions.IsExternalFunction)
                {
                    if (debugFrame.IsExternalFunction == true)
                    {
                        return true;
                    }
                }
                else if (option == StackFrameFlagOptions.IsFunctionStepOver)
                {
                    if (debugFrame.FunctionStepOver == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int FindEndPCForAssocGraphNode(int tempPC, InstructionStream istream, ProcedureNode fNode, AssociativeGraph.GraphNode graphNode, bool handleSSATemps)
        {
            int limit = Constants.kInvalidIndex;
            //AssociativeGraph.GraphNode currentGraphNode = executingGraphNode;
            AssociativeGraph.GraphNode currentGraphNode = graphNode;
            //Validity.Assert(currentGraphNode != null);

            if (currentGraphNode != null)
            {
                if (tempPC < currentGraphNode.updateBlock.startpc || tempPC > currentGraphNode.updateBlock.endpc)
                {
                    //   return false;
                    return Constants.kInvalidIndex;
                }

                int i = currentGraphNode.dependencyGraphListID;
                AssociativeGraph.GraphNode nextGraphNode = currentGraphNode;
                while (currentGraphNode.exprUID != ProtoCore.DSASM.Constants.kInvalidIndex 
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

        public void SetUpCallrForDebug(ProtoCore.Core core, ProtoCore.DSASM.Executive exec, ProcedureNode fNode, int pc, bool isBaseCall = false,
            ProtoCore.CallSite callsite = null, List<StackValue> arguments = null, List<List<ProtoCore.ReplicationGuide>> replicationGuides = null, ProtoCore.DSASM.StackFrame stackFrame = null,
            List<StackValue> dotCallDimensions = null, bool hasDebugInfo = false, bool isMember = false, StackValue? thisPtr = null)
        {
            //ProtoCore.DSASM.Executive exec = core.CurrentExecutive.CurrentDSASMExec;

            DebugFrame debugFrame = new DebugFrame();
            debugFrame.IsBaseCall = isBaseCall;
            debugFrame.Arguments = arguments;
            debugFrame.IsMemberFunction = isMember;
            debugFrame.ThisPtr = thisPtr;
            debugFrame.HasDebugInfo = hasDebugInfo;

            if (fNode.name.Equals(ProtoCore.DSASM.Constants.kDotArgMethodName))
            {
                debugFrame.IsDotArgCall = true;
            }
            else if (fNode.name.Equals(ProtoCore.DSDefinitions.Keyword.Dispose))
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
            if (fNode.name == ProtoCore.DSASM.Constants.kDotMethodName)
            {
                isReplicating = false;
                isExternalFunction = false;
                debugFrame.IsDotCall = true;
                debugFrame.DotCallDimensions = dotCallDimensions;
                
                SetUpCallr(ref debugFrame, isReplicating, isExternalFunction, exec);
                DebugStackFrame.Push(debugFrame);

                return;
            }

            List<List<Lang.Replication.ReplicationInstruction>> replicationTrials;
            bool willReplicate = callsite.WillCallReplicate(new ProtoCore.Runtime.Context(), arguments, replicationGuides, stackFrame, core, out replicationTrials);
            
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
            else if (fNode.isExternal && fNode.name != ProtoCore.DSASM.Constants.kDotMethodName)
            {
                // Clear all breakpoints 
                if (!DebugStackFrameContains(StackFrameFlagOptions.IsExternalFunction) && fNode.name != ProtoCore.DSASM.Constants.kFunctionRangeExpression)
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
        public void RestoreCallrForNoBreak(ProtoCore.Core core, ProcedureNode fNode, bool isReplicating = false)
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
                if (ActiveBreakPoints.Count > 0 && fNode.name != ProtoCore.DSASM.Constants.kFunctionRangeExpression)
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
                
        public void SetUpStepOverFunctionCalls(ProtoCore.Core core, ProcedureNode fNode, AssociativeGraph.GraphNode graphNode, bool hasDebugInfo)
        {
            int tempPC = DebugEntryPC;
            int limit = 0;  // end pc of current expression
            InstructionStream istream;

            int pc = tempPC;
            if (core.DebugProps.InlineConditionOptions.isInlineConditional == true)
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
        public const int FIRST_CORE_ID = 0;

        public int ID { get; private set; }
        //recurtion
        public List<FunctionCounter> recursivePoint { get; set; }
        public List<FunctionCounter> funcCounterTable { get; set; }
        public bool calledInFunction;
        
        // This flag is set true when we call GraphUtilities.PreloadAssembly to load libraries in Graph UI
        public bool IsParsingPreloadedAssembly { get; set; }
        
        // THe ImportModuleHandler owned by the temporary core used in Graph UI precompilation
        // needed to detect if the same assembly is not being imported more than once
        public ProtoFFI.ImportModuleHandler ImportHandler { get; set; }
        
        // This is set to true when the temporary core is used for precompilation of CBN's in GraphUI
        public bool IsParsingCodeBlockNode { get; set; }

        // This is the AST node list of default imported libraries needed for Graph Compiler
        public ProtoCore.AST.AssociativeAST.CodeBlockNode ImportNodes { get; set; }

        // The root AST node obtained from parsing an expression in a Graph node in GraphUI
        public List<ProtoCore.AST.Node> AstNodeList { get; set; }

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
            public RuntimeData.WarningID RuntimeId;
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

        public Lang.FunctionTable FunctionTable { get; set; }

        public Script Script { get; set; }
        public LangVerify Langverify = new LangVerify();
        public Dictionary<Language, Executive> Executives { get; private set; }

        public Executive CurrentExecutive { get; private set; }
        public Stack<ExceptionRegistration> stackActiveExceptionRegistration { get; set; }
        public int GlobOffset { get; set; }
        public int GlobHeapOffset { get; set; }
        public int BaseOffset { get; set; }
        public int GraphNodeUID { get; set; }

        public Heap Heap { get; set; }
        public ProtoCore.Runtime.RuntimeMemory Rmem { get; set; }

        public int ClassIndex { get; set; }     // Holds the current class scope
        public int RunningBlock { get; set; }
        public int CodeBlockIndex { get; set; }
        public int RuntimeTableIndex { get; set; }


        public List<CodeBlock> CodeBlockList { get; set; }
        // The Complete Code Block list contains all the code blocks
        // unlike the codeblocklist which only stores the outer most code blocks
        public List<CodeBlock> CompleteCodeBlockList { get; set; }



        /// <summary>
        /// The delta codeblock index tracks the current number of new language blocks created at compile time for every new compile phase
        /// </summary>
        public int DeltaCodeBlockIndex { get; set; }

        // TODO Jun: Refactor this and similar indices into a logical grouping of block incrementing variables 

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        //public void AddContextData(IEnumerable<IContextData> data)
        //{
        //    if (data == null)
        //        return;

        //    if (null == mCotextManager)
        //        mCotextManager = new ContextDataManager(this);

        //    mCotextManager.AddData(data);
        //}

        // Cached replication guides for the current call. 
        // TODO Jun: Store this in the dynamic table node
        public List<List<ProtoCore.ReplicationGuide>> replicationGuides;

        // if CompileToLib is true, this is used to output the asm instruction to the dsASM file
        // if CompilerToLib is false, this will be set to Console.Out
        public TextWriter AsmOutput;
        public int AsmOutputIdents;

        public string CurrentDSFileName { get; set; }
        // this field is used to store the inferedtype information  when the code gen cross one langeage to another 
        // otherwize the inferedtype information will be lost
        public Type InferedType;

        public DebugProperties DebugProps;
        
        //public Stack<List<ProtoCore.AssociativeGraph.GraphNode>> stackNodeExecutedSameTimes { get; set; }
        //public Stack<AssociativeGraph.GraphNode> stackExecutingGraphNodes { get; set; }
        public Stack<InterpreterProperties> InterpreterProps { get; set; }

        // Continuation properties used for Serial mode execution and Debugging of Replicated calls
        public Lang.ContinuationStructure ContinuationStruct { get; set; }

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


        public bool EnableCallsiteExecutionState { get; set; }
        public CallsiteExecutionState csExecutionState { get; set; }

        public Dictionary<int, CallSite> CallsiteCache { get; set; }

        // A list of graphnodes that contain a function call
        public List<AssociativeGraph.GraphNode> GraphNodeCallList { get; set; }

        public int newEntryPoint { get; private set; }

        public void SetNewEntryPoint(int pc)
        {
            newEntryPoint = pc;
        }

        /// <summary>
        /// Sets the function to an inactive state where it can no longer be used by the front-end and backend
        /// </summary>
        /// <param name="functionDef"></param>
        public void SetFunctionInactive(ProtoCore.AST.AssociativeAST.FunctionDefinitionNode functionDef)
        {
            // DS language only supports function definition on the global and first language block scope 
            // TODO Jun: Determine if it is still required to combine function tables in the codeblocks and callsite

            // Update the functiond definition in the codeblocks
            int hash = CoreUtils.GetFunctionHash(functionDef);

            foreach (CodeBlock block in CodeBlockList)
            {
                // Update the current function definition in the current block
                int index = block.procedureTable.IndexOfHash(hash);
                if (Constants.kInvalidIndex == index)
                    continue;

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
                    // Dont remove from symbol table, but just nullify it.
                    block.symbolTable.symbolList[symbol.symbolTableIndex] = new SymbolNode();
                }

                break;
            }

            // Update the function definition in global function tables
            foreach (KeyValuePair<int, Dictionary<string, FunctionGroup>> functionGroupList in FunctionTable.GlobalFuncTable)
            {
                foreach (KeyValuePair<string, FunctionGroup> functionGroup in functionGroupList.Value)
                {
                    functionGroup.Value.FunctionEndPoints.RemoveAll(func => func.procedureNode.HashID == hash);
                }
            }
        }

        [Obsolete("This is only used in obsolete live runner")]
        public void LogErrorInGlobalMap(Core.ErrorType type, string msg, string fileName = null, int line = -1, int col = -1, 
            BuildData.WarningID buildId = BuildData.WarningID.kDefault, RuntimeData.WarningID runtimeId = RuntimeData.WarningID.kDefault)
        {
            ulong location = (((ulong)line) << 32 | ((uint)col));
            Core.ErrorEntry newError = new Core.ErrorEntry
            {
                Type = type,
                FileName = fileName,
                Message = msg,
                Line = line,
                Col = col,
                BuildId = buildId,
                RuntimeId = runtimeId
            };

            if (this.LocationErrorMap.ContainsKey(location))
            {
                ProtoCore.Core.ErrorEntry error = this.LocationErrorMap[location];

                // If there is a warning, replace it with an error
                if (error.Type == Core.ErrorType.Warning && type == Core.ErrorType.Error)
                {
                    this.LocationErrorMap[location] = newError;
                }
            }
            else
            {
                this.LocationErrorMap.Add(location, newError);
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
        /// Reset properties for recompilation
        /// Generated instructions for imported libraries are preserved. 
        /// This means they are just reloaded, not regenerated
        /// </summary>
        private void ResetDeltaCompile()
        {
            if (CodeBlockList.Count <= 0)
                return;

            var globalBlock = CodeBlockList[0];
            var instructionStream = globalBlock.instrStream;
            var graph = globalBlock.instrStream.dependencyGraph;

            // Preserve only the instructions of libraries that were previously 
            // loaded. Other instructions need to be removed and regenerated
            int count = instructionStream.instrList.Count - deltaCompileStartPC;
            instructionStream.instrList.RemoveRange(deltaCompileStartPC, count);

            // Remove graphnodes from this range
            // TODO Jun: Optimize this - determine which graphnodes need to be removed during compilation
            int removeGraphnodesFrom = Constants.kInvalidIndex;
            for (int n = 0; n < graph.GraphList.Count; ++n)
            {
                var graphNode = graph.GraphList[n];
                if (graphNode.updateBlock.startpc >= deltaCompileStartPC)
                {
                    removeGraphnodesFrom = n;
                    break;
                }
            }

            if (ProtoCore.DSASM.Constants.kInvalidIndex != removeGraphnodesFrom)
            {
                count = graph.GraphList.Count - removeGraphnodesFrom;

                int classIndex = graph.GraphList[removeGraphnodesFrom].classIndex;
                int procIndex = graph.GraphList[removeGraphnodesFrom].procIndex;

                // TODO Jun: Find the better way to remove the graphnodes from the nodemap
                // Does getting the classindex and proindex of the first node sufficient?
                graph.RemoveNodesFromScope(classIndex, procIndex);

                // Remove the graphnodes from them main list
                graph.GraphList.RemoveRange(removeGraphnodesFrom, count);
            }
            else
            {
                // @keyu: This is for the first run. Just simply remove 
                // global graph node from the map to avoid they are marked
                // as dirty at the next run.
                graph.RemoveNodesFromScope(Constants.kInvalidIndex, Constants.kInvalidIndex);
            }

            // Jun this is where the temp solutions starts for implementing language blocks in delta execution
            for (int n = 1; n < CodeBlockList.Count; ++n)
            {
                CodeBlockList[n].instrStream.instrList.Clear();
            }
        }

        // @keyu: ResetDeltaExection() resets everything so that the core will 
        // compile new code and execute it, but in some cases we dont want to 
        // compile code, just re-execute the existing code, therefore only some 
        // states need to be reset.
        public void ResetForExecution()
        {
            ExecMode = InterpreterMode.kNormal;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid;
            RunningBlock = 0;
            DeltaCodeBlockIndex = 0;
            ForLoopBlockIndex = Constants.kInvalidIndex;

            // Jun this is where the temp solutions starts for implementing language blocks in delta execution
            for (int n = 1; n < CodeBlockList.Count; ++n)
            {
                CodeBlockList[n].instrStream.instrList.Clear();
            }

            // Remove inactive graphnodes in the list
            GraphNodeCallList.RemoveAll(g => !g.isActive);
        }

        public void ResetForDeltaASTExecution()
        {
            ResetForExecution();
            ExprInterpreterExe = null;
        }

        // Comment Jun:
        // The core is reused on delta execution
        // These are properties that need to be reset on subsequent executions
        // All properties require reset except for the runtime memory
        public void ResetForDeltaExecution()
        {
            ClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;


            watchClassScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchFunctionScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchBaseOffset = 0;
            watchStack = new List<StackValue>();
            watchSymbolList = new List<SymbolNode>();
            watchFramePointer = ProtoCore.DSASM.Constants.kInvalidIndex;

            ID = FIRST_CORE_ID;

            //recurtion
            recursivePoint = new List<FunctionCounter>();
            funcCounterTable = new List<FunctionCounter>();
            calledInFunction = false;

            //GlobOffset = 0;
            GlobHeapOffset = 0;
            BaseOffset = 0;
            GraphNodeUID = 0;
            RunningBlock = 0;
            //CodeBlockList = new List<DSASM.CodeBlock>();
            CompleteCodeBlockList = new List<DSASM.CodeBlock>();
            DSExecutable = new ProtoCore.DSASM.Executable();

            AssocNode = null;


            //
            //
            // Comment Jun: Delta execution should not reset the class tables as they are preserved
            //
            //      FunctionTable = new Lang.FunctionTable();
            //      ClassTable = new DSASM.ClassTable();
            //      TypeSystem = new TypeSystem();
            //      TypeSystem.SetClassTable(ClassTable);
            //      ProcNode = null;
            //      ProcTable = new DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kGlobalScope);
            //
            //      CodeBlockList = new List<DSASM.CodeBlock>();
            //
            //

            //      CodeBlockIndex = 0;
            //      RuntimeTableIndex = 0;

            //

            // Comment Jun:
            // Disable SSA for the previous graphcompiler as it clashes with the way code recompilation behaves
            // SSA is enabled for the new graph strategy of delta compilation and execution
            Options.GenerateSSA = false;


            //Initialize the function pointer table
            FunctionPointerTable = new DSASM.FunctionPointerTable();

            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DSASM.DynamicVariableTable();
            DynamicFunctionTable = new DSASM.DynamicFunctionTable();
            replicationGuides = new List<List<ProtoCore.ReplicationGuide>>();

            ExceptionHandlingManager = new ExceptionHandlingManager();
            startPC = ProtoCore.DSASM.Constants.kInvalidIndex;

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
                BuildStatus = new BuildStatus(this, Options.BuildOptWarningAsError, null, Options.BuildOptErrorAsWarning);
            }
            RuntimeStatus = new RuntimeStatus(this);

            //SSASubscript = 0;
            ExpressionUID = 0;
            ModifierBlockUID = 0;
            ModifierStateSubscript = 0;

            ExprInterpreterExe = null;
            ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            assocCodegen = null;
            FunctionCallDepth = 0;

            // Default execution log is Console.Out.
            this.ExecutionLog = Console.Out;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid; //not yet started

            DebugProps = new DebugProperties();
            InterpreterProps = new Stack<InterpreterProperties>();
            stackActiveExceptionRegistration = new Stack<ExceptionRegistration>();

            ExecutiveProvider = new ExecutiveProvider();
            ParsingMode = ProtoCore.ParseMode.Normal;

            // Reset PC dictionary containing PC to line/col map
            if (codeToLocation != null)
                codeToLocation.Clear();

            if (LocationErrorMap != null)
                LocationErrorMap.Clear();

            if (AstNodeList != null)
                AstNodeList.Clear();

            ResetDeltaCompile();

            DeltaCodeBlockIndex = 0;
            ForLoopBlockIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public void ResetForPrecompilation()
        {
            GraphNodeUID = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            
            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DSASM.DynamicVariableTable();
            DynamicFunctionTable = new DSASM.DynamicFunctionTable();

            // If the previous compilation for import resulted in a build error, 
            // ignore it and continue compiling other import statements
            /*if (BuildStatus.ErrorCount > 0)
            {
                ImportHandler = null;
                CodeBlockList.Clear();
                CompleteCodeBlockList.Clear();
            }*/

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
            ForLoopBlockIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        private void ResetAll(Options options)
        {
            ProtoCore.Utils.Validity.AssertExpiry();
            Options = options;
            Executives = new Dictionary<ProtoCore.Language, ProtoCore.Executive>();
            FunctionTable = new Lang.FunctionTable();
            ClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            Heap = new DSASM.Heap();
            Rmem = new ProtoCore.Runtime.RuntimeMemory(Heap);

            watchClassScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchFunctionScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchBaseOffset = 0;
            watchStack = new List<StackValue>();
            watchSymbolList = new List<SymbolNode>();
            watchFramePointer = ProtoCore.DSASM.Constants.kInvalidIndex;

            ID = FIRST_CORE_ID;

            //recurtion
            recursivePoint = new List<FunctionCounter>();
            funcCounterTable = new List<FunctionCounter>();
            calledInFunction = false;

            GlobOffset = 0;
            GlobHeapOffset = 0;
            BaseOffset = 0;
            GraphNodeUID = 0;
            RunningBlock = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            CodeBlockList = new List<DSASM.CodeBlock>();
            CompleteCodeBlockList = new List<DSASM.CodeBlock>();
            DSExecutable = new ProtoCore.DSASM.Executable();

            AssocNode = null;

            // TODO Jun/Luke type system refactoring
            // Initialize the globalClass table and type system
            ClassTable = new DSASM.ClassTable();
            TypeSystem = new TypeSystem();
            TypeSystem.SetClassTable(ClassTable);
            ProcNode = null;
            ProcTable = new DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kGlobalScope);

            //Initialize the function pointer table
            FunctionPointerTable = new DSASM.FunctionPointerTable();

            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DSASM.DynamicVariableTable();
            DynamicFunctionTable = new DSASM.DynamicFunctionTable();
            replicationGuides = new List<List<ProtoCore.ReplicationGuide>>();

            ExceptionHandlingManager = new ExceptionHandlingManager();
            startPC = ProtoCore.DSASM.Constants.kInvalidIndex;

            deltaCompileStartPC = ProtoCore.DSASM.Constants.kInvalidIndex;

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
            SSASubscript_GUID = System.Guid.NewGuid();
            ExpressionUID = 0;
            ModifierBlockUID = 0;
            ModifierStateSubscript = 0;

            ExprInterpreterExe = null;
            ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            assocCodegen = null;
            FunctionCallDepth = 0;

            // Default execution log is Console.Out.
            this.ExecutionLog = Console.Out;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid; //not yet started

            DebugProps = new DebugProperties();
            //stackNodeExecutedSameTimes = new Stack<List<AssociativeGraph.GraphNode>>();
            //stackExecutingGraphNodes = new Stack<AssociativeGraph.GraphNode>();
            InterpreterProps = new Stack<InterpreterProperties>();
            stackActiveExceptionRegistration = new Stack<ExceptionRegistration>();

            ExecutiveProvider = new ExecutiveProvider();

            Configurations = new Dictionary<string, object>();

            ContinuationStruct = new Lang.ContinuationStructure();
            ParsingMode = ProtoCore.ParseMode.Normal;
            
            IsParsingPreloadedAssembly = false;
            IsParsingCodeBlockNode = false;
            ImportHandler = null;

            deltaCompileStartPC = 0;
            builtInsLoaded = false;
            FFIPropertyChangedMonitor = new FFIPropertyChangedMonitor(this);

            csExecutionState = null;
            EnableCallsiteExecutionState = false;

            // TODO: Remove check once fully implemeted
            if (EnableCallsiteExecutionState)
            {
                csExecutionState = CallsiteExecutionState.LoadState();
            }
            else
            {
                csExecutionState = new CallsiteExecutionState();
            }
            CallsiteCache = new Dictionary<int, CallSite>();
            ForLoopBlockIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            GraphNodeCallList = new List<GraphNode>();

            newEntryPoint = ProtoCore.DSASM.Constants.kInvalidIndex;
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

        public ExceptionHandlingManager ExceptionHandlingManager { get; set; }

        private int tempVarId = 0;
        private int tempLanguageId = 0;

        // TODO Jun: Cleansify me - i dont need to be here
        public AST.AssociativeAST.AssociativeNode AssocNode { get; set; }
        public int startPC { get; set; }


        //
        // TODO Jun: This is the expression interpreters executable. 
        //           It must be moved to its own core, whre each core is an instance of a compiler+interpreter
        //
        public Executable ExprInterpreterExe { get; set; }
        public ProtoCore.DSASM.InterpreterMode ExecMode { get; set; }
        public List<SymbolNode> watchSymbolList { get; set; }
        public int watchClassScope { get; set; }
        public int watchFunctionScope { get; set; }
        public int watchBaseOffset { get; set; }
        public List<StackValue> watchStack { get; set; }
        public int watchFramePointer { get; set; }

        public ProtoCore.CodeGen assocCodegen { get; set; }

        // this one is to address the issue that when the execution control is in a language block
        // which is further inside a function, the compiler feprun is false, 
        // when inspecting value in that language block or the function, debugger will assume the function index is -1, 
        // name look up will fail beacuse all the local variables inside 
        // that language block and fucntion has non-zero function index 
        public int FunctionCallDepth { get; set; }
        public System.IO.TextWriter ExecutionLog { get; set; }

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
            ProtoFFI.CLRModuleType.ClearTypes();
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

        /// <summary>
        /// Split the virutal machine, creating a new machine with the new ID
        /// </summary>
        /// <param name="newID"></param>w 
        /// <returns></returns>
        public Core Split(int newID)
        {
            Core secondCore = new Core(Options);
            secondCore.ID = newID;

            secondCore.AsmOutput = AsmOutput;
            secondCore.AsmOutputIdents = AsmOutputIdents;
            secondCore.AssocNode = AssocNode;
            secondCore.BaseOffset = BaseOffset;
            secondCore.Breakpoints = new List<Instruction>(Breakpoints);
            secondCore.BuildStatus = BuildStatus; //For now just retarget the build infomration to same output

            //@TODO(Luke) Should these be deep cloned? They will need to be fixed before we do any form of dynamic
            //code injection

            //Simple shallow copy
            secondCore.ClassIndex = ClassIndex;
            secondCore.ClassTable = ClassTable;
            secondCore.CodeBlockIndex = CodeBlockIndex;
            secondCore.CodeBlockList = CodeBlockList;
            secondCore.CompleteCodeBlockList = CompleteCodeBlockList;
            secondCore.CurrentDSFileName = CurrentDSFileName;
            secondCore.CurrentExecutive = CurrentExecutive;
            secondCore.DebugProps = DebugProps;
            secondCore.DynamicFunctionTable = DynamicFunctionTable;
            secondCore.DynamicVariableTable = DynamicVariableTable;
            secondCore.DSExecutable = DSExecutable;
            secondCore.Executives = Executives;
            secondCore.ExpressionUID = ExpressionUID;
            secondCore.FunctionPointerTable = FunctionPointerTable;
            secondCore.FunctionTable = FunctionTable;
            secondCore.GlobHeapOffset = GlobHeapOffset;
            secondCore.GlobOffset = GlobOffset;
            secondCore.InferedType = InferedType;
            secondCore.Langverify = Langverify;
            secondCore.ModifierBlockUID = ModifierBlockUID;
            secondCore.ProcNode = ProcNode;
            secondCore.ProcTable = ProcTable;
            secondCore.RunningBlock = RunningBlock;
            secondCore.RuntimeTableIndex = RuntimeTableIndex;
            secondCore.SSASubscript = SSASubscript;
            secondCore.Script = Script;
            secondCore.TypeSystem = TypeSystem;
            secondCore.tempVarId = tempVarId;
            secondCore.tempLanguageId = tempLanguageId;
            secondCore.Configurations = Configurations;

            //Deep copy
            secondCore.Heap = Heap.Clone();
            secondCore.Rmem = Rmem; //@Todo(Luke): This will have to change


            //Custom
            secondCore.ExceptionHandlingManager = new ExceptionHandlingManager();
            secondCore.ReasonForExecutionSuspend = ReasonForExecutionSuspend.VMSplit;

            secondCore.RuntimeStatus = RuntimeStatus; //@Todo(Luke): This will have to change
            secondCore.StopWatch = secondCore.StopWatch; //@Todo(Luke): This will have to change

            return secondCore;

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

            int symbolIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool stillInsideFunction = function != ProtoCore.DSASM.Constants.kInvalidIndex;
            DSASM.CodeBlock searchBlock = codeblock;
            // TODO(Jiong): Code Duplication, Consider moving this if else block inside the while loop 
            if (stillInsideFunction)
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, function);

                if (function != ProtoCore.DSASM.Constants.kInvalidIndex &&
                    searchBlock.procedureTable != null &&
                    searchBlock.procedureTable.procList.Count > function &&   // Note: This check assumes we can not define functions inside a fucntion 
                    symbolIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);
            }
            else
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);
            }
            while (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
            {
                // if the search block is of type function, it means our search has gone out of the function itself
                // so, we should ignore the given function index and only search its parent block's global variable
                if (searchBlock.blockType == DSASM.CodeBlockType.kFunction)
                    stillInsideFunction = false;

                searchBlock = searchBlock.parent;
                if (null != searchBlock)
                {
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
                        if (function != ProtoCore.DSASM.Constants.kInvalidIndex &&
                            searchBlock.procedureTable != null &&
                            searchBlock.procedureTable.procList.Count > function && // Note: This check assumes we can not define functions inside a fucntion 
                            symbolIndex == ProtoCore.DSASM.Constants.kInvalidIndex)

                            symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);

                    }
                    else
                    {
                        symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);
                    }
                }
                else
                {
                    // End of nested blocks

                    /*
                    // Not found? Look at the class scope
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != classscope)
                    {
                        // Look at the class members and base class members
                        bool hasSymbol = false;
                        ProtoCore.DSASM.AddressType addrType =  DSASM.AddressType.Invalid;
                        ProtoCore.DSASM.ClassNode cnode = classTable.list[classscope];
                        symbolIndex = cnode.GetFirstVisibleSymbol(name, classscope, function, out hasSymbol, out addrType);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolIndex)
                        {
                            if (addrType == DSASM.AddressType.StaticMemVarIndex)
                            {
                                return codeBlockList[0].symbolTable.symbolList[symbolIndex];
                            }
                            else
                            {
                                return classTable.list[classscope].symbols.symbolList[symbolIndex];
                            }
                        }

                        // Look at the class constructors and functions
                        symbolIndex = classTable.list[classscope].symbols.IndexOf(name, classscope, function);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolIndex)
                        {
                            return classTable.list[classscope].symbols.symbolList[symbolIndex];
                        }
                    }


                    // Not found? Look at the global scope
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kGlobalScope);
                    if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                    {
                        return null;
                    }
                    break;
                     * */
                    return null;
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
                if (ProtoCore.DSASM.CodeBlockType.kFunction == cblock.blockType)
                {
                    return true;
                }
                else if (ProtoCore.DSASM.CodeBlockType.kLanguage == cblock.blockType)
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

            DSASM.CodeBlock searchBlock = codeblock;
            while (null != searchBlock)
            {
                if (null == searchBlock.procedureTable)
                {
                    searchBlock = searchBlock.parent;
                    continue;
                }

                // The class table is passed just to check for coercion values
                int procIndex = searchBlock.procedureTable.IndexOf(name, argTypeList, ClassTable);
                if (ProtoCore.DSASM.Constants.kInvalidIndex != procIndex)
                {
                    return searchBlock.procedureTable.procList[procIndex];
                }
                searchBlock = searchBlock.parent;
            }
            return null;
        }

        public DSASM.CodeBlock GetCodeBlock(List<DSASM.CodeBlock> blockList, int blockId)
        {
            DSASM.CodeBlock codeblock = null;
            codeblock = blockList.Find(x => x.codeBlockId == blockId);
            if (codeblock == null)
            {
                foreach (DSASM.CodeBlock block in blockList)
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

        public StackValue Bounce(int exeblock, int entry, ProtoCore.Runtime.Context context, ProtoCore.DSASM.StackFrame stackFrame, int locals = 0, DebugServices.EventSink sink = null)
        {
            if (stackFrame != null)
            {
                StackValue svThisPtr = stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kThisPtr);
                int ci = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kClass).opdata;
                int fi = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunction).opdata;
                int returnAddr = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kReturnAddress).opdata;
                int blockDecl = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionBlock).opdata;
                int blockCaller = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
                ProtoCore.DSASM.StackFrameType callerFrameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kCallerStackFrameType).opdata;
                ProtoCore.DSASM.StackFrameType frameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kStackFrameType).opdata;
                Validity.Assert(frameType == StackFrameType.kTypeLanguage);
                
                int depth = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kStackFrameDepth).opdata;
                int framePointer = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFramePointer).opdata;
                List<StackValue> registers = stackFrame.GetRegisters();

                Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
            }

            ProtoCore.Language id = DSExecutable.instrStreamList[exeblock].language;
            CurrentExecutive = Executives[id];
            StackValue sv = Executives[id].Execute(exeblock, entry, context, sink);
            return sv;
        }

        public StackValue Bounce(int exeblock, int entry, ProtoCore.Runtime.Context context, List<Instruction> breakpoints, ProtoCore.DSASM.StackFrame stackFrame, int locals = 0, 
            DSASM.Executive exec = null, DebugServices.EventSink sink = null, bool fepRun = false)
        {
            if (stackFrame != null)
            {
                StackValue svThisPtr = stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kThisPtr);
                int ci = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kClass).opdata;
                int fi = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunction).opdata;
                int returnAddr = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kReturnAddress).opdata;
                int blockDecl = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionBlock).opdata;
                int blockCaller = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
                ProtoCore.DSASM.StackFrameType callerFrameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kCallerStackFrameType).opdata;
                ProtoCore.DSASM.StackFrameType frameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kStackFrameType).opdata;
                Validity.Assert(frameType == StackFrameType.kTypeLanguage);

                int depth = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kStackFrameDepth).opdata;
                int framePointer = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFramePointer).opdata;
                List<StackValue> registers = stackFrame.GetRegisters();

                DebugProps.SetUpBounce(exec, blockCaller, returnAddr);

                Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
            }

            ProtoCore.Language id = DSExecutable.instrStreamList[exeblock].language;
            CurrentExecutive = Executives[id];

            StackValue sv = Executives[id].Execute(exeblock, entry, context, breakpoints, sink, fepRun);
            return sv;
        }

        private void BfsBuildSequenceTable(CodeBlock codeBlock, SymbolTable[] runtimeSymbols)
        {
            if (DSASM.CodeBlockType.kLanguage == codeBlock.blockType
                || DSASM.CodeBlockType.kFunction == codeBlock.blockType
                || DSASM.CodeBlockType.kConstruct == codeBlock.blockType)
            {
                Validity.Assert(codeBlock.symbolTable.RuntimeIndex < RuntimeTableIndex);
                runtimeSymbols[codeBlock.symbolTable.RuntimeIndex] = codeBlock.symbolTable;
            }

            foreach (DSASM.CodeBlock child in codeBlock.children)
            {
                BfsBuildSequenceTable(child, runtimeSymbols);
            }
        }

        private void BfsBuildProcedureTable(CodeBlock codeBlock, ProcedureTable[] procTable)
        {
            if (DSASM.CodeBlockType.kLanguage == codeBlock.blockType || DSASM.CodeBlockType.kFunction == codeBlock.blockType)
            {
                Validity.Assert(codeBlock.procedureTable.runtimeIndex < RuntimeTableIndex);
                procTable[codeBlock.procedureTable.runtimeIndex] = codeBlock.procedureTable;
            }

            foreach (DSASM.CodeBlock child in codeBlock.children)
            {
                BfsBuildProcedureTable(child, procTable);
            }
        }

        private void BfsBuildInstructionStreams(CodeBlock codeBlock, InstructionStream[] istreamList)
        {
            if (null != codeBlock)
            {
                if (DSASM.CodeBlockType.kLanguage == codeBlock.blockType || DSASM.CodeBlockType.kFunction == codeBlock.blockType)
                {
                    Validity.Assert(codeBlock.codeBlockId < CodeBlockIndex);
                    istreamList[codeBlock.codeBlockId] = codeBlock.instrStream;
                }

                foreach (DSASM.CodeBlock child in codeBlock.children)
                {
                    BfsBuildInstructionStreams(child, istreamList);
                }
            }
        }


        public void GenerateExprExe()
        {
            // TODO Jun: Determine if we really need another executable for the expression interpreter
            Validity.Assert(null == ExprInterpreterExe);
            ExprInterpreterExe = new DSASM.Executable();

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

        public void GenerateExecutable()
        {
            Validity.Assert(CodeBlockList.Count >= 0);

            // Retrieve the class table directly since it is a global table
            DSExecutable.classTable = ClassTable;

            // Build the runtime symbols
            DSExecutable.runtimeSymbols = new DSASM.SymbolTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildSequenceTable(CodeBlockList[n], DSExecutable.runtimeSymbols);
            }

            // Build the runtime procedure table
            DSExecutable.procedureTable = new DSASM.ProcedureTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildProcedureTable(CodeBlockList[n], DSExecutable.procedureTable);
            }

            // Build the executable instruction streams
            DSExecutable.instrStreamList = new DSASM.InstructionStream[CodeBlockIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildInstructionStreams(CodeBlockList[n], DSExecutable.instrStreamList);
            }

            // Single associative block means the first instruction is an immediate bounce 
            // This variable is only used by the mirror to determine if the GetValue()
            // block parameter needs to be incremented or not in order to get the correct global variable
            if (DSExecutable.isSingleAssocBlock)
            {
                DSExecutable.isSingleAssocBlock = (DSASM.OpCode.BOUNCE == CodeBlockList[0].instrStream.instrList[0].opCode) ? true : false;
            }
            GenerateExprExe();
        }



        public string GenerateTempVar()
        {
            tempVarId++;
            return ProtoCore.DSASM.Constants.kTempVar + tempVarId.ToString();
        }


        public string GenerateTempPropertyVar()
        {
            tempVarId++;
            return ProtoCore.DSASM.Constants.kTempPropertyVar + tempVarId.ToString();
        }

        public string GenerateTempLangageVar()
        {
            tempLanguageId++;
            return ProtoCore.DSASM.Constants.kTempLangBlock + tempLanguageId.ToString();
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
            string modStateTemp = DSASM.Constants.kTempModifierStateNamePrefix + modifierName + ModifierStateSubscript.ToString();
            ++ModifierStateSubscript;
            return modStateTemp;
        }

        public List<int> GetAncestorBlockIdsOfBlock(int blockId)
        {
            if (blockId >= this.CompleteCodeBlockList.Count || blockId < 0)
            {
                return new List<int>();
            }
            CodeBlock thisBlock = this.CompleteCodeBlockList[blockId];

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
            int constructBlockId = this.Rmem.CurrentConstructBlockId;
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
                return this.Rmem.CurrentConstructBlockId;
        }

        public AssociativeGraph.GraphNode GetExecutingGraphNode()
        {
            foreach (var prop in this.InterpreterProps)
            {
                if (prop.executingGraphNode != null)
                {
                    return prop.executingGraphNode;
                }
            }

            return null;
        }

        public bool IsEvalutingPropertyChanged()
        {
            foreach (var prop in this.InterpreterProps)
            {
                if (prop.updateStatus == AssociativeEngine.UpdateStatus.kPropertyChangedUpdate)
                {
                    return true;
                }
            }

            return false;
        }

        public int ExecutingGraphnodeUID { get; set; }

        /// <summary>
        /// Retrieves an existing instance of a callsite associated with a UID
        /// It creates a new callsite if non was found
        /// </summary>
        /// <param name="core"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public CallSite GetCallSite(int uid, int classScope, string methodName)
        {
            Validity.Assert(null != FunctionTable);
            CallSite csInstance = null;

            // TODO Jun: Currently generates a new callsite for imperative and internally generated functions
            // Fix the issues that cause the cache to go out of sync when attempting to cache internal functions
            // This may require a secondary callsite cache for internal functions so they dont clash with the graphNode UID key
            bool isInternalFunction = CoreUtils.IsInternalFunction(methodName);
            bool isImperative = DSExecutable.instrStreamList[RunningBlock].language == Language.kImperative;
            if (isInternalFunction || isImperative)
            {
                csInstance = new CallSite(classScope, methodName, FunctionTable, Options.ExecutionMode);
            }
            else
            {
                if (!CallsiteCache.TryGetValue(uid, out csInstance))
                {
                    csInstance = new CallSite(classScope, methodName, FunctionTable, Options.ExecutionMode);
                    CallsiteCache.Add(uid, csInstance);
                }
            }
            return csInstance;
        }

        public void ResetSSASubscript(Guid guid, int subscript)
        {
            SSASubscript_GUID = guid;
            SSASubscript = subscript;
        }
    }
}
