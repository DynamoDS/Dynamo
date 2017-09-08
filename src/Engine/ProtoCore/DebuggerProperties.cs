using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                BeginDocument += Console.WriteLine;
                EndDocument += Console.WriteLine;
                PrintMessage += Console.WriteLine;
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
