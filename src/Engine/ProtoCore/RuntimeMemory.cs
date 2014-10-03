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
                int n = GetRelative(relativeOffset);
                return Stack[n];
            }
            
            public void SetAtRelative(int offset, StackValue data)
            {
                int n = GetRelative(offset);
                Stack[n] = data;
            }

            public StackValue GetSymbolValue(SymbolNode symbol)
            {
                int index = GetStackIndex(symbol);
                return Stack[index];
            }

            public void SetSymbolValue(SymbolNode symbol, StackValue data)
            {
                int index = GetStackIndex(symbol);
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
            /// Return stack index of the corresponding symbol node in runtime
            /// stack.
            /// </summary>
            /// <param name="symbolNode"></param>
            /// <returns></returns>
            private int GetStackIndex(SymbolNode symbolNode)
            {
                int offset = symbolNode.index;

                int depth = 0;
                int blockOffset = 0;
                // TODO Jun: the property 'localFunctionIndex' must be deprecated and just use 'functionIndex'.
                // The GC currenlty has an issue of needing to reset 'functionIndex' at codegen
                bool isGlobal = Constants.kInvalidIndex == symbolNode.absoluteClassScope && 
                                Constants.kGlobalScope == symbolNode.absoluteFunctionIndex;
                if (!isGlobal)
                {
                    depth = (int)GetAtRelative(StackFrame.kFrameIndexStackFrameDepth).opdata;
                    blockOffset = depth * StackFrame.kStackFrameSize;
                }
                offset -= blockOffset;

                int index = GetRelative(offset);
                return index;
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
                while (fp >= StackFrame.kStackFrameSize)
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
