using System;
using System.Collections.Generic;
using ProtoCore.AssociativeGraph;
using ProtoCore.CodeModel;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Runtime;
using ProtoCore.Utils;

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
#if DEBUG
                
                BeginDocument += Console.WriteLine;
                EndDocument += Console.WriteLine;
                PrintMessage += Console.WriteLine;
#endif
            }
        }
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
        /// Returns the Program counter. This is only valid when the executive is suspended
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
            GraphNode currentGraphNode = graphNode;

            if (currentGraphNode != null)
            {
                if (tempPC < currentGraphNode.updateBlock.startpc || tempPC > currentGraphNode.updateBlock.endpc)
                {
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
            else if (!fNode.Name.Contains(Constants.kSetterPrefix))
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

        public void SetUpCallrForDebug(RuntimeCore runtimeCore, DSASM.Executive exec, ProcedureNode fNode, int pc, bool isBaseCall = false,
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

            if (CoreUtils.IsDisposeMethod(fNode.Name))
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
            if (fNode.Name == Constants.kDotMethodName)
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
            bool willReplicate = callsite.WillCallReplicate(new Context(), arguments, replicationGuides, stackFrame, runtimeCore, out replicationTrials);
            
            // the inline conditional built-in is handled separately as 'WillCallReplicate' is always true in this case
            if(fNode.Name.Equals(Constants.kInlineConditionalMethodName))
            {
                // The inline conditional built-in is created only for associative blocks and needs to be handled separately as below
                InstructionStream istream = runtimeCore.DSExecutable.instrStreamList[CurrentBlockId];
                Validity.Assert(istream.language == Language.Associative);
                {
                    runtimeCore.DebugProps.InlineConditionOptions.isInlineConditional = true;
                    runtimeCore.DebugProps.InlineConditionOptions.startPc = pc;

                    runtimeCore.DebugProps.InlineConditionOptions.endPc = FindEndPCForAssocGraphNode(pc, istream, fNode, exec.Properties.executingGraphNode, runtimeCore.Options.ExecuteSSA);


                    runtimeCore.DebugProps.InlineConditionOptions.instructionStream = runtimeCore.RunningBlock;
                    debugFrame.IsInlineConditional = true;
                }
                
                // no replication case
                if (willReplicate && replicationTrials.Count == 1)
                {
                    runtimeCore.DebugProps.InlineConditionOptions.ActiveBreakPoints.AddRange(runtimeCore.Breakpoints);

                    isReplicating = false;
                    isExternalFunction = false;
                }
                else // an inline conditional call that replicates
                {
                    // Clear all breakpoints for outermost replicated call
                    if(!DebugStackFrameContains(StackFrameFlagOptions.IsReplicating))
                    {
                        ActiveBreakPoints.AddRange(runtimeCore.Breakpoints);
                        runtimeCore.Breakpoints.Clear();
                    }
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
            else if (fNode.IsExternal && fNode.Name != Constants.kDotMethodName)
            {
                // Clear all breakpoints 
                if (!DebugStackFrameContains(StackFrameFlagOptions.IsExternalFunction) && fNode.Name != Constants.kFunctionRangeExpression)
                {
                    ActiveBreakPoints.AddRange(runtimeCore.Breakpoints);
                    runtimeCore.Breakpoints.Clear();
                }

                isExternalFunction = true;
                isReplicating = false;
            }
            // Find if function call will replicate or not and if so
            // prevent stepping in by removing all breakpoints from outermost replicated call
            else if (willReplicate)
            {
                // Clear all breakpoints for outermost replicated call
                if(!DebugStackFrameContains(StackFrameFlagOptions.IsReplicating))
                {
                    ActiveBreakPoints.AddRange(runtimeCore.Breakpoints);
                    runtimeCore.Breakpoints.Clear();
                }

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
        public void RestoreCallrForNoBreak(RuntimeCore runtimeCore, ProcedureNode fNode, bool isReplicating = false)
        {
            Validity.Assert(DebugStackFrame.Count > 0);
            
            // All functions that reach this point are restored here as they have not been
            // done so in RETURN/RETC            
            DebugFrame debugFrame = DebugStackFrame.Pop();

            // Restore breakpoints which occur after returning from outermost replicating function call 
            // as well as outermost external function call
            if (!DebugStackFrameContains(StackFrameFlagOptions.IsReplicating) &&
                !DebugStackFrameContains(StackFrameFlagOptions.IsExternalFunction))
            {
                if (ActiveBreakPoints.Count > 0 && fNode.Name != Constants.kFunctionRangeExpression)
                {
                    runtimeCore.Breakpoints.AddRange(ActiveBreakPoints);
                    //if (SetUpStepOverFunctionCalls(core, fNode, ActiveBreakPoints))
                    {
                        ActiveBreakPoints.Clear();
                    }
                }
            }

            // If stepping over function call in debug mode
            if (debugFrame.HasDebugInfo && RunMode == Runmode.StepNext)
            {
                // if stepping over outermost function call
                if (!DebugStackFrameContains(StackFrameFlagOptions.IsFunctionStepOver))
                {
                    SetUpStepOverFunctionCalls(runtimeCore, fNode, debugFrame.ExecutingGraphNode, debugFrame.HasDebugInfo);
                }
            }
        }

        public void SetUpStepOverFunctionCalls(RuntimeCore runtimeCore, ProcedureNode fNode, GraphNode graphNode, bool hasDebugInfo)
        {
            int tempPC = DebugEntryPC;
            int limit = 0;  // end pc of current expression
            InstructionStream istream;

            int pc = tempPC;
            if (runtimeCore.DebugProps.InlineConditionOptions.isInlineConditional)
            {
                tempPC = InlineConditionOptions.startPc;
                limit = InlineConditionOptions.endPc;
                istream = runtimeCore.DSExecutable.instrStreamList[InlineConditionOptions.instructionStream];
            }
            else
            {
                pc = tempPC;
                istream = runtimeCore.DSExecutable.instrStreamList[runtimeCore.RunningBlock];
                if (istream.language == Language.Associative)
                {
                    limit = FindEndPCForAssocGraphNode(pc, istream, fNode, graphNode, runtimeCore.Options.ExecuteSSA);
                    //Validity.Assert(limit != ProtoCore.DSASM.Constants.kInvalidIndex);
                }
                else if (istream.language == Language.Imperative)
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
                runtimeCore.Breakpoints.Clear();
                runtimeCore.Breakpoints.AddRange(AllbreakPoints);

                pc = tempPC;
                while (++pc <= limit)
                {
                    Instruction instr = istream.instrList[pc];
                    // We still want to break at the closing brace of a function or ctor call or language block
                    if (instr.debug != null && instr.opCode != OpCode.RETURN && (instr.opCode != OpCode.RETB)) 
                    {
                        if (runtimeCore.Breakpoints.Contains(instr))
                            runtimeCore.Breakpoints.Remove(instr);
                    }
                }
            }
        }
    }

    public class ExecutionStateEventArgs : EventArgs
    {
        public enum State
        {
            Invalid = -1,
            ExecutionBegin,
            ExecutionEnd,
            ExecutionBreak,
            ExecutionResume,
        }

        public ExecutionStateEventArgs(State state)
        {
            ExecutionState = state;
        }

        public State ExecutionState { get; private set; }
    }
}
