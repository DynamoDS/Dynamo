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

    public class DebugProperties
    {
        public DebugProperties()
        {
            CurrentBlockId = Constants.kInvalidIndex;
        }

        public int CurrentBlockId { get; set; }
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
