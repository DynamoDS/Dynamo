using System.Collections.Generic;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore
{
    namespace Runtime
    {
        public class RuntimeMemory
        {
            public int FramePointer
            {
                get; set;
            }

            public List<StackValue> Stack
            {
                get; private set;
            }

            public List<int> ConstructBlockIds
            {
                get; private set;
            }

            public Heap Heap
            {
                get; private set;
            }

            public int GlobOffset
            {
                get;
                private set;
            }

            /// <summary>
            /// Where the first stack frame starts. Usually the stack below this
            /// pointer is reserved for global variables. 
            /// </summary>
            private int startFramePointer = 0;

            public RuntimeMemory(Heap heap)
            {
                FramePointer = 0;
                Stack = new List<StackValue>();
                ConstructBlockIds = new List<int>();
                Heap = heap;
                StackRestorer = new StackAlignToFramePointerRestorer();
            }

            public void Push(StackValue data)
            {
                Stack.Add(data);
            }

            public StackValue Pop()
            {
                int last = Stack.Count - 1;
                StackValue value = Stack[last];
                Stack.RemoveAt(last);
                return value;
            }

            /// <summary>
            /// Reserve specified number of stack slots for local variables
            /// and initialize them to Null.
            /// </summary>
            /// <param name="size"></param>
            public void PushFrameForLocals(int size)
            {
                for (int n = 0; n < size; ++n)
                {
                    Stack.Add(StackValue.Null);
                }
            }

            /// <summary>
            /// Reserve stack for global variables. 
            /// </summary>
            /// <param name="size"></param>
            public void PushFrameForGlobals(int size)
            {
                GlobOffset = size;
                PushFrameForLocals(size);
                startFramePointer = Stack.Count;
            }

            /// <summary>
            /// Remove the specified number of items from the stack.
            /// </summary>
            /// <param name="size"></param>
            public void PopFrame(int size)
            {
                Stack.RemoveRange(Stack.Count - size, size);
            }

            public void PushStackFrame(StackFrame stackFrame)
            {
                Validity.Assert(StackFrame.StackFrameSize == stackFrame.Frame.Length);
                for (int i = StackFrame.StackFrameSize - 1; i >= 0; i--)
                {
                    Push(stackFrame.Frame[i]);
                }
                FramePointer = Stack.Count;
            }

            public bool ValidateStackFrame()
            {
                return Stack[GetRelative(StackFrame.FrameIndexThisPtr)].IsPointer
                    && Stack[GetRelative(StackFrame.FrameIndexClassIndex)].IsClassIndex
                    && Stack[GetRelative(StackFrame.FrameIndexFunctionIndex)].IsFunctionIndex
                    && Stack[GetRelative(StackFrame.FrameIndexReturnAddress)].IsInteger
                    && Stack[GetRelative(StackFrame.FrameIndexFunctionBlockIndex)].IsBlockIndex
                    && Stack[GetRelative(StackFrame.FrameIndexCallerBlockIndex)].IsBlockIndex
                    && Stack[GetRelative(StackFrame.FrameIndexCallerStackFrameType)].IsFrameType
                    && Stack[GetRelative(StackFrame.FrameIndexStackFrameType)].IsFrameType
                    && Stack[GetRelative(StackFrame.FrameIndexLocalVariableCount)].IsInteger
                    && Stack[GetRelative(StackFrame.FrameIndexExecutionStates)].IsInteger
                    && Stack[GetRelative(StackFrame.FrameIndexStackFrameDepth)].IsInteger
                    && Stack[GetRelative(StackFrame.FrameIndexFramePointer)].IsInteger
                    && Stack[GetRelative(StackFrame.FrameIndexBlockIndex)].IsBlockIndex;
            }

            private void PushRegisters(List<StackValue> registers)
            {
                for (int i = 0; i < registers.Count; ++i)
                {
                    Stack.Add(registers[registers.Count - 1 - i]);
                }
            }

            public int GetRelative(int index)
            {
                return index >= 0 ? index : FramePointer + index;
            }

            public StackValue GetAtRelative(int relativeOffset)
            {
                return GetAtRelative(relativeOffset, FramePointer);
            }

            private StackValue GetAtRelative(int relativeOffset, int framePointer)
            {
                int index = relativeOffset >= 0 ? relativeOffset : framePointer + relativeOffset;
                return Stack[index];
            }
            
            public void SetAtRelative(int offset, StackValue data)
            {
                int n = GetRelative(offset);
                Stack[n] = data;
            }

            /// <summary>
            /// Return the value of symbol on current stack frame.
            /// </summary>
            /// <param name="symbol"></param>
            /// <returns></returns>
            public StackValue GetSymbolValue(SymbolNode symbol)
            {
                return GetSymbolValueOnFrame(symbol, FramePointer);
            }

            /// <summary>
            /// Return the value of symbol on specified frame. 
            /// </summary>
            /// <param name="symbol"></param>
            /// <param name="framePointer"></param>
            /// <returns></returns>
            public StackValue GetSymbolValueOnFrame(SymbolNode symbol, int framePointer)
            {
                int index = GetStackIndex(symbol, framePointer);
                return Stack[index];
            }

            /// <summary>
            /// Set the value for symbol on current stack frame.
            /// </summary>
            /// <param name="symbol"></param>
            /// <param name="data"></param>
            public void SetSymbolValue(SymbolNode symbol, StackValue data)
            {
                int index = GetStackIndex(symbol, FramePointer);
                Stack[index] = data;
            }

            // TO BE DELETED
            public int GetStackIndex(int offset)
            {
                int depth = (int)GetAtRelative(StackFrame.FrameIndexStackFrameDepth).IntegerValue;
                int blockOffset = depth * StackFrame.StackFrameSize;

                offset -= blockOffset;
                return offset;
            }

            /// <summary>
            /// Return stack index of symbol for specified frame.
            /// </summary>
            /// <param name="symbol"></param>
            /// <param name="framePointer"></param>
            /// <returns></returns>
            private int GetStackIndex(SymbolNode symbol, int framePointer)
            {
                int offset = symbol.index;

                if (symbol.absoluteClassScope != Constants.kGlobalScope ||
                    symbol.absoluteFunctionIndex != Constants.kGlobalScope)
                {
                    int depth = (int)GetAtRelative(StackFrame.FrameIndexStackFrameDepth, framePointer).IntegerValue;
                    int blockOffset = depth * StackFrame.StackFrameSize;
                    offset -= blockOffset;
                }

                return offset >= 0 ? offset : framePointer + offset;
            }
            
            public StackValue GetMemberData(int symbolindex, int scope, Executable exe)
            {
                StackValue thisptr = CurrentStackFrame.ThisPtr;

                // Get the heapstck offset
                int offset = exe.classTable.ClassNodes[scope].Symbols.symbolList[symbolindex].index;

                var obj = Heap.ToHeapObject<DSObject>(thisptr);
                if (!obj.Values.Any())
                    return StackValue.Null;

                StackValue sv = obj.GetValueFromIndex(offset, null);
                Validity.Assert(sv.IsPointer || sv.IsArray|| sv.IsInvalid);

                // Not initialized yet
                if (sv.IsInvalid)
                {
                    return StackValue.Null;
                }
                else if (sv.IsArray)
                {
                    return sv;
                }

                StackValue nextPtr = sv;
                obj = Heap.ToHeapObject<DSObject>(nextPtr);

                if (obj.Values.Any()) 
                {
                    StackValue data = obj.GetValueFromIndex(0, null);
                    bool isActualData = !data.IsPointer && !data.IsArray && !data.IsInvalid; 
                    if (isActualData)
                    {
                        return data;
                    }
                }
                return sv;
            }

            private StackAlignToFramePointerRestorer StackRestorer { get; set; }

            public void AlignStackForExprInterpreter()
            {
                StackRestorer.Align(this);
            }

            public void RestoreStackForExprInterpreter()
            {
                StackRestorer.Restore();
            }

            /// <summary>
            /// Push the block ID of the block that will be executed
            /// </summary>
            /// <param name="id"></param>
            public void PushConstructBlockId(int id)
            {
                ConstructBlockIds.Add(id);
            }

            public void PopConstructBlockId()
            {
                if (ConstructBlockIds.Count > 0)
                    ConstructBlockIds.RemoveAt(ConstructBlockIds.Count - 1);
            }

            public int CurrentConstructBlockId
            {
                get
                {
                    if (ConstructBlockIds.Count > 0)
                        return ConstructBlockIds[ConstructBlockIds.Count - 1];
                    else
                        return DSASM.Constants.kInvalidIndex;
                }
            }

            /// <summary>
            /// Current stack frame.
            /// </summary>
            public StackFrame CurrentStackFrame
            {
                get
                {
                    var fp = FramePointer;

                    // Note the first stack frame starts after the space for global 
                    // variables, so here we need to check the frame pointer is 
                    // large enought to contain a stack frame and all global variables
                    if (fp - StackFrame.StackFrameSize >= startFramePointer)
                    {
                        var frame = new StackValue[StackFrame.StackFrameSize];
                        for (int sourceIndex = fp - 1, destIndex = 0; sourceIndex >= fp - StackFrame.StackFrameSize; sourceIndex--, destIndex++)
                        {
                            frame[destIndex] = Stack[sourceIndex];
                        }
                        var stackFrame = new StackFrame(frame);
                        return stackFrame;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
