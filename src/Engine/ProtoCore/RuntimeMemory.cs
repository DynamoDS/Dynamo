
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

            public void PushFrame(int size)
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
                PushFrame(size);
                startFramePointer = Stack.Count;
            }

            public void PopFrame(int size)
            {
                for (int n = 0; n < size; ++n)
                {
                    int last = Stack.Count - 1;
                    Stack.RemoveAt(last);
                }
            }

            public void PushStackFrame(StackValue svThisPtr, int classIndex, int funcIndex, int pc, int functionBlockDecl, int functionBlockCaller, StackFrameType callerType, StackFrameType type, int depth, int fp, List<StackValue> registers, int locsize, int executionStates)
            {
                // TODO Jun: Performance
                // Push frame should only require adjusting the frame index instead of pushing dummy elements
                PushFrame(locsize);
                Push(StackValue.BuildInt(fp));
                PushRegisters(registers);
                Push(StackValue.BuildInt(executionStates));
                Push(StackValue.BuildInt(0));
                Push(StackValue.BuildInt(depth));
                Push(StackValue.BuildFrameType((int)type));
                Push(StackValue.BuildFrameType((int)callerType));
                Push(StackValue.BuildBlockIndex(functionBlockCaller));
                Push(StackValue.BuildBlockIndex(functionBlockDecl));
                Push(StackValue.BuildInt(pc));
                Push(StackValue.BuildInt(funcIndex));
                Push(StackValue.BuildInt(classIndex));
                Push(svThisPtr);
                FramePointer = Stack.Count;
            }

            public void PushStackFrame(StackFrame stackFrame)
            {
                // TODO Jun: Performance
                // Push frame should only require adjusting the frame index instead of pushing dummy elements
                Validity.Assert(StackFrame.kStackFrameSize == stackFrame.Frame.Length);

                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kFramePointer]);

                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterTX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterSX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterRX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterLX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterFX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterEX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterDX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterCX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterBX]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kRegisterAX]);

                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kExecutionStates]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kLocalVariables]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kStackFrameDepth]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kStackFrameType]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kCallerStackFrameType]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kFunctionCallerBlock]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kFunctionBlock]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kReturnAddress]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kFunction]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kClass]);
                Push(stackFrame.Frame[(int)StackFrame.AbsoluteIndex.kThisPtr]);

                FramePointer = Stack.Count;
            }

            public bool ValidateStackFrame()
            {
                return Stack[GetRelative(StackFrame.kFrameIndexThisPtr)].IsPointer
                    && Stack[GetRelative(StackFrame.kFrameIndexClass)].IsInteger
                    && Stack[GetRelative(StackFrame.kFrameIndexFunction)].IsInteger
                    && Stack[GetRelative(StackFrame.kFrameIndexReturnAddress)].IsInteger
                    && Stack[GetRelative(StackFrame.kFrameIndexFunctionBlock)].IsBlockIndex
                    && Stack[GetRelative(StackFrame.kFrameIndexFunctionCallerBlock)].IsBlockIndex
                    && Stack[GetRelative(StackFrame.kFrameIndexCallerStackFrameType)].IsFrameType
                    && Stack[GetRelative(StackFrame.kFrameIndexStackFrameType)].IsFrameType
                    && Stack[GetRelative(StackFrame.kFrameIndexLocalVariables)].IsInteger
                    && Stack[GetRelative(StackFrame.kFrameIndexExecutionStates)].IsInteger
                    && Stack[GetRelative(StackFrame.kFrameIndexStackFrameDepth)].IsInteger
                    && Stack[GetRelative(StackFrame.kFrameIndexFramePointer)].IsInteger;
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
                return relativeOffset >= 0
                    ? Stack[relativeOffset]
                    : Stack[framePointer + relativeOffset];
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

            /// <summary>
            /// Set the value for symbol on specified frame. 
            /// </summary>
            /// <param name="symbol"></param>
            /// <param name="data"></param>
            /// <param name="framePointer"></param>
            public void SetSymbolValueOnFrame(SymbolNode symbol, StackValue data, int framePointer)
            {
                int index = GetStackIndex(symbol, framePointer);
                Stack[index] = data;
            }

            // TO BE DELETED
            public int GetStackIndex(int offset)
            {
                int depth = (int)GetAtRelative(StackFrame.kFrameIndexStackFrameDepth).opdata;
                int blockOffset = depth * StackFrame.kStackFrameSize;

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
                    int depth = (int)GetAtRelative(StackFrame.kFrameIndexStackFrameDepth, framePointer).opdata;
                    int blockOffset = depth * StackFrame.kStackFrameSize;
                    offset -= blockOffset;
                }

                return offset >= 0 ? offset : framePointer + offset;
            }

            public void SetGlobalStackData(int globalOffset, StackValue svData)
            {
                Validity.Assert(globalOffset >= 0);
                Validity.Assert(Stack.Count > 0);
                Stack[globalOffset] = svData;
            }
            
            public StackValue GetMemberData(int symbolindex, int scope, Executable exe)
            {
                StackValue thisptr = CurrentStackFrame.ThisPtr;

                // Get the heapstck offset
                int offset = exe.classTable.ClassNodes[scope].symbols.symbolList[symbolindex].index;

                var heapElement = Heap.GetHeapElement(thisptr); 
                if (null == heapElement.Stack || heapElement.Stack.Length == 0)
                    return StackValue.Null;

                StackValue sv = heapElement.Stack[offset];
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
                Validity.Assert(nextPtr.opdata >= 0);
                heapElement = Heap.GetHeapElement(nextPtr);

                if (null != heapElement.Stack && heapElement.Stack.Length > 0)
                {
                    StackValue data = heapElement.Stack[0];
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
                    var stackFrames = GetStackFrames();
                    if (stackFrames.Any())
                    {
                        return stackFrames.First();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Get all stack frames. 
            /// </summary>
            /// <returns></returns>
            public IEnumerable<StackFrame> GetStackFrames()
            {
                var fp = FramePointer;
                
                // Note the first stack frame starts after the space for global 
                // variables, so here we need to check the frame pointer is 
                // large enought to contain a stack frame and all global variables
                while (fp - StackFrame.kStackFrameSize >= startFramePointer)
                {
                    var frame = new StackValue[StackFrame.kStackFrameSize];
                    for (int sourceIndex = fp - StackFrame.kStackFrameSize; sourceIndex < fp; sourceIndex++)
                    {
                        int destIndex = fp - sourceIndex - 1;
                        frame[destIndex] = Stack[sourceIndex];
                    }
                    var stackFrame = new StackFrame(frame);
                    fp = stackFrame.FramePointer;
                    yield return stackFrame;
                }
            }

            /// <summary>
            /// Mark and sweep garbage collection.
            /// </summary>
            /// <param name="gcRootPointers"></param>
            /// <param name="exe"></param>
            public void GC(List<StackValue> gcRootPointers, DSASM.Executive exe)
            {
                Heap.GCMarkAndSweep(gcRootPointers.ToList(), exe);
            }
        }
    }
}

