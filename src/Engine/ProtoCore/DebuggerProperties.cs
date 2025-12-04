using System;
using System.Collections.Generic;
using ProtoCore.AssociativeGraph;
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
#if DEBUG
                
                BeginDocument += Console.WriteLine;
                EndDocument += Console.WriteLine;
                PrintMessage += Console.WriteLine;
#endif
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
