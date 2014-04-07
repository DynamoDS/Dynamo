
using System;
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore
{
    namespace Runtime
    {
        public class RuntimeMemory
        {
            public Executable Executable { get; set; }

            // TODO Jun: Handle classes. This is part of the classes in global scope refactor
            public ClassTable ClassTable { get; set; }

            public int FramePointer { get; set; }
            public List<StackValue> Stack { get; set; }

            public List<int> ConstructBlockIds { get; private set; }

            public Heap Heap { get; private set; }

            // Keep track of modified symbols during an execution cycle
            // TODO Jun: Turn this into a multi-key dictionary where the keys are: name, classindex and procindex
            public Dictionary<string, SymbolNode> mapModifiedSymbols { get; private set; }

            public RuntimeMemory()
            {
                FramePointer = 0;
                Executable = null;
                Stack = new List<StackValue>();
                Heap = null;
                mapModifiedSymbols = new Dictionary<string, SymbolNode>();
            }

            public RuntimeMemory(Heap heap)
            {
                FramePointer = 0;
                Executable = null;
                Stack = new List<StackValue>();
                ConstructBlockIds = new List<int>();
                Heap = heap;
                StackRestorer = new StackAlignToFramePointerRestorer();
                mapModifiedSymbols = new Dictionary<string, SymbolNode>();
            }

            private void UpdateModifiedSymbols(SymbolNode symbol)
            {
                Validity.Assert(null != symbol);
                Validity.Assert(!string.IsNullOrEmpty(symbol.name));
                Validity.Assert(null != mapModifiedSymbols);

                // TODO Jun: Turn this into a multi-key dictionary where the keys are: name, classindex and procindex
                string key = symbol.name;
                if (!mapModifiedSymbols.ContainsKey(key))
                {
                    mapModifiedSymbols.Add(key, symbol);
                }
            }

            public void ResetModifedSymbols()
            {
                Validity.Assert(null != mapModifiedSymbols);
                mapModifiedSymbols.Clear();
            }

            public List<string> GetModifiedSymbolString()
            {
                List<string> nameList = new List<string>();
                foreach (KeyValuePair<string, SymbolNode> kvp in mapModifiedSymbols)
                {
                    nameList.Add(kvp.Key);
                }
                return nameList;
            }

            public void ReAllocateMemory(int delta)
            {
                //
                // Comment Jun:
                // This modifies the current stack and heap to accomodate delta statements
                PushGlobFrame(delta);
            }

            public int GetRelative(int index)
            {
                return index >= 0 ? index : FramePointer + index;
            }

            public int GetRelative(int offset, int index)
            {
                return index >= 0 ? index : (FramePointer - offset) + index;
            }

            public void PushGlobFrame(int globsize)
            {
                for (int n = 0; n < globsize; ++n)
                {
                    Stack.Add(StackValue.Null);
                }
            }

            private void PushFrame(int size)
            {
                for (int n = 0; n < size; ++n)
                {
                    Stack.Add(StackValue.Null);
                }
            }

            private void PushFrame(List<StackValue> stackData)
            {
                if (null != stackData && stackData.Count > 0)
                {
                    Stack.AddRange(stackData);
                }
            }

            public void PushStackFrame(int ptr, int classIndex, int funcIndex, int pc, int functionBlockDecl, int functionBlockCaller, StackFrameType callerType, StackFrameType type, int depth, int fp, List<StackValue> registers, int locsize, int executionStates)
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
                Push(StackValue.BuildPointer(ptr));
                FramePointer = Stack.Count;
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


            public void PushStackFrame(StackFrame stackFrame, int localSize, int executionStates)
            {
                // TODO Jun: Performance
                // Push frame should only require adjusting the frame index instead of pushing dummy elements
                Validity.Assert(StackFrame.kStackFrameSize == stackFrame.Frame.Length);

                PushFrame(localSize);
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
                bool isValid =
                    //(
                        AddressType.Pointer == Stack[GetRelative(StackFrame.kFrameIndexThisPtr)].optype
                    //|| AddressType.Invalid == stack[GetRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr)].optype 
                    //|| AddressType.ClassIndex == stack[GetRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr)].optype 
                    //)

                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexClass)].optype
                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexFunction)].optype
                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexReturnAddress)].optype
                    && AddressType.BlockIndex == Stack[GetRelative(StackFrame.kFrameIndexFunctionBlock)].optype
                    && AddressType.BlockIndex == Stack[GetRelative(StackFrame.kFrameIndexFunctionCallerBlock)].optype
                    && AddressType.FrameType == Stack[GetRelative(StackFrame.kFrameIndexCallerStackFrameType)].optype
                    && AddressType.FrameType == Stack[GetRelative(StackFrame.kFrameIndexStackFrameType)].optype
                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexLocalVariables)].optype
                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexExecutionStates)].optype
                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexStackFrameDepth)].optype
                    && AddressType.Int == Stack[GetRelative(StackFrame.kFrameIndexFramePointer)].optype;

                return isValid;
            }


            private void PushRegisters(List<StackValue> registers)
            {
                for (int i = 0; i < registers.Count; ++i)
                {
                    Stack.Add(registers[registers.Count - 1 - i]);
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

            public void Pop(int size)
            {
                for (int n = 0; n < size; ++n)
                {
                    int last = Stack.Count - 1;
                    Stack.RemoveAt(last);
                }
            }

            public void SetAtRelative(int offset, StackValue data)
            {

                int n = GetRelative(offset);
                Stack[n] = data;
            }

            public void SetAtSymbol(SymbolNode symbol, StackValue data)
            {

                int n = GetRelative(GetStackIndex(symbol));
                Stack[n] = data;
            }

            public StackValue GetAtRelative(int relativeOffset)
            {
                int n = GetRelative(relativeOffset);
                return Stack[n];
            }

            public StackValue GetAtRelative(SymbolNode symbol)
            {
                int n = GetRelative(GetStackIndex(symbol));
                return Stack[n];
            }

            public StackValue GetAtRelative(int relativeOffset, int index)
            {
                int n = GetRelative(relativeOffset);
                return Stack[n];
            }

            public void SetData(int blockId, int symbolindex, int scope, Object data)
            {
                MemoryRegion region = Executable.runtimeSymbols[blockId].symbolList[symbolindex].memregion;

                if (MemoryRegion.kMemStack == region)
                {
                    SetStackData(blockId, symbolindex, scope, data);
                }
                else if (MemoryRegion.kMemHeap == region)
                {
                    throw new NotImplementedException("{BEC8F306-6704-4C90-A1A2-5BD871871022}");
                }
                else if (MemoryRegion.kMemStatic == region)
                {
                    Validity.Assert(false, "static region not yet supported, {17C22575-2361-4BAE-AA9E-9076CD1E52D9}");
                }
                else
                {
                    Validity.Assert(false, "unsupported memory region, {AF92A869-6F9F-421D-84F8-BC2FC56C07F4}");
                }
            }

            public int GetStackIndex(int offset)
            {
                int depth = (int)GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexStackFrameDepth).opdata;
                int blockOffset = depth * StackFrame.kStackFrameSize;

                offset -= blockOffset;
                return offset;
            }

            public int GetStackIndex(SymbolNode symbolNode)
            {
                int offset = symbolNode.index;

                int depth = 0;
                int blockOffset = 0;
                // TODO Jun: the property 'localFunctionIndex' must be deprecated and just use 'functionIndex'.
                // The GC currenlty has an issue of needing to reset 'functionIndex' at codegen
                bool isGlobal = Constants.kInvalidIndex == symbolNode.absoluteClassScope && Constants.kGlobalScope == symbolNode.absoluteFunctionIndex;
                if (!isGlobal)
                {
                    depth = (int)GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexStackFrameDepth).opdata;
                    blockOffset = depth * StackFrame.kStackFrameSize;
                }
                offset -= blockOffset;
                return offset;
            }

            public StackValue SetStackData(int blockId, int symbol, int classScope, Object data)
            {
                int offset = Constants.kInvalidIndex;
                SymbolNode symbolNode = null;
                if (Constants.kInvalidIndex == classScope)
                {
                    symbolNode = Executable.runtimeSymbols[blockId].symbolList[symbol];
                    offset = GetStackIndex(symbolNode);
                }
                else
                {
                    symbolNode = ClassTable.ClassNodes[classScope].symbols.symbolList[symbol];
                    offset = GetStackIndex(symbolNode);
                }

                Validity.Assert(null != symbolNode);
                UpdateModifiedSymbols(symbolNode);

                int n = GetRelative(offset);
                StackValue opPrev = Stack[n];
                Stack[n] = (null == data) ? Pop() : (StackValue)data;
                return opPrev;
            }


            public void SetGlobalStackData(int globalOffset, StackValue svData)
            {
                Validity.Assert(globalOffset >= 0);
                Validity.Assert(Stack.Count > 0);
                Stack[globalOffset] = svData;
            }

            public StackValue GetData(int blockId, int symbolindex, int scope)
            {
                MemoryRegion region = DSASM.MemoryRegion.kInvalidRegion;
                if (Constants.kGlobalScope == scope)
                {
                    region = Executable.runtimeSymbols[blockId].symbolList[symbolindex].memregion;
                }
                else
                {
                    region = ClassTable.ClassNodes[scope].symbols.symbolList[symbolindex].memregion;
                }

                if (MemoryRegion.kMemStack == region)
                {
                    return GetStackData(blockId, symbolindex, scope);
                }
                else if (MemoryRegion.kMemHeap == region)
                {
                    //return GetHeapData(symbolindex);
                    throw new NotImplementedException("{69604961-DE03-440A-97EB-0390B1B0E510}");
                }
                else if (MemoryRegion.kMemStatic == region)
                {
                    Validity.Assert(false, "static region not yet supported, {63EA5434-D2E2-40B6-A816-0046F573236F}");
                }

                Validity.Assert(false, "unsupported memory region, {DCA48F13-EEE1-4374-B301-C96870D44C6B}");
                return StackValue.BuildInt(0);
            }

            public StackValue GetStackData(int blockId, int symbolindex, int classscope, int offset = 0)
            {
                SymbolNode symbolNode = null;
                if (Constants.kInvalidIndex == classscope)
                {
                    symbolNode = Executable.runtimeSymbols[blockId].symbolList[symbolindex];
                }
                else
                {
                    symbolNode = ClassTable.ClassNodes[classscope].symbols.symbolList[symbolindex];
                }

                Validity.Assert(null != symbolNode);
                int n = GetRelative(offset, GetStackIndex(symbolNode));
                if (n > Stack.Count - 1)
                {
                    return StackValue.Null;
                }

                return Stack[n];
            }

            public StackValue GetMemberData(int symbolindex, int scope)
            {

                int thisptr = (int)GetAtRelative(GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr)).opdata;

                // Get the heapstck offset
                int offset = ClassTable.ClassNodes[scope].symbols.symbolList[symbolindex].index;

                if (null == Heap.Heaplist[thisptr].Stack || Heap.Heaplist[thisptr].Stack.Length == 0)
                    return StackValue.Null;

                StackValue sv = Heap.Heaplist[thisptr].Stack[offset];
                Validity.Assert(AddressType.Pointer == sv.optype || AddressType.ArrayPointer == sv.optype || AddressType.Invalid == sv.optype);

                // Not initialized yet
                if (sv.optype == AddressType.Invalid)
                {
                    sv.optype = AddressType.Null;
                    sv.opdata_d = sv.opdata = 0;
                    return sv;
                }
                else if (sv.optype == AddressType.ArrayPointer)
                {
                    return sv;
                }

                int nextPtr = (int)sv.opdata;
                Validity.Assert(nextPtr >= 0);
                if (null != Heap.Heaplist[nextPtr].Stack && Heap.Heaplist[nextPtr].Stack.Length > 0)
                {
                    bool isActualData =
                            AddressType.Pointer != Heap.Heaplist[nextPtr].Stack[0].optype
                        && AddressType.ArrayPointer != Heap.Heaplist[nextPtr].Stack[0].optype
                        && AddressType.Invalid != Heap.Heaplist[nextPtr].Stack[0].optype; // Invalid is an uninitialized member

                    if (isActualData)
                    {
                        return Heap.Heaplist[nextPtr].Stack[0];
                    }
                }
                return sv;
            }

            public StackValue GetPrimitive(StackValue op)
            {
                if (AddressType.Pointer != op.optype)
                {
                    return op;
                }
                int ptr = (int)op.opdata;
                while (AddressType.Pointer == Heap.Heaplist[ptr].Stack[0].optype)
                {
                    ptr = (int)Heap.Heaplist[ptr].Stack[0].opdata;
                }
                return Heap.Heaplist[ptr].Stack[0];
            }

            public StackValue[] GetArrayElements(StackValue array)
            {
                int ptr = (int)array.opdata;
                HeapElement hs = Heap.Heaplist[ptr];
                StackValue[] arrayElements = new StackValue[hs.VisibleSize];
                for (int n = 0; n < hs.VisibleSize; ++n)
                {
                    arrayElements[n] = hs.Stack[n];
                }

                return arrayElements;
            }

            public int GetArraySize(StackValue array)
            {
                if (array.optype != AddressType.ArrayPointer)
                {
                    return Constants.kInvalidIndex;
                }
                int ptr = (int)array.opdata;
                return Heap.Heaplist[ptr].Stack.Length;
            }

            public StackValue BuildArray(StackValue[] arrayElements)
            {
                int size = arrayElements.Length;
                lock (Heap.cslock)
                {
                    int ptr = Heap.Allocate(size);
                    for (int n = size - 1; n >= 0; --n)
                    {
                        StackValue sv = arrayElements[n];
                        Heap.IncRefCount(sv);
                        Heap.Heaplist[ptr].Stack[n] = sv;
                    }
                    return StackValue.BuildArrayPointer(ptr);
                }
            }

            public StackValue BuildNullArray(int size)
            {
                lock (Heap.cslock)
                {
                    int ptr = Heap.Allocate(size);
                    for (int n = 0; n < size; ++n)
                    {
                        Heap.Heaplist[ptr].Stack[n] = StackValue.Null;
                    }
                    return StackValue.BuildArrayPointer(ptr);
                }
            }

            public StackValue BuildArrayFromStack(int size)
            {
                lock (Heap.cslock)
                {
                    int ptr = Heap.Allocate(size);
                    for (int n = size - 1; n >= 0; --n)
                    {
                        StackValue sv = Pop();
                        Heap.IncRefCount(sv);
                        Heap.Heaplist[ptr].Stack[n] = sv;
                    }
                    return StackValue.BuildArrayPointer(ptr);
                }
            }

            public bool IsHeapActive(StackValue sv)
            {
                if (!StackUtils.IsReferenceType(sv))
                {
                    return false;
                }

                return Heap.Heaplist[(int)sv.opdata].Active;
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
        }
    }
}