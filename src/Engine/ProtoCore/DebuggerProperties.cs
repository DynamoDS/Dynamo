using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoCore.AssociativeGraph;
using ProtoCore.CodeModel;
using ProtoCore.DSASM;

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

            FinalFepChosen = null;
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

    }

    public class DebugProperties
    {
        public DebugProperties()
        {
            DebugStackFrame = new Stack<DebugFrame>();
            executingGraphNode = null;
            CurrentBlockId = Constants.kInvalidIndex;
        }

        public enum StackFrameFlagOptions
        {
            FepRun = 1,
            IsReplicating,
            IsExternalFunction,
            IsFunctionStepOver
        }

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

        public int CurrentBlockId { get; set; }
        public GraphNode executingGraphNode { get; set; }
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
