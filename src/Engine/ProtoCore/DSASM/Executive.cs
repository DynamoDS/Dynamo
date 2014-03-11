

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProtoCore.Exceptions;
using ProtoCore.Utils;
using ProtoCore.Lang;
using ProtoCore.RuntimeData;


namespace ProtoCore.DSASM
{
    public class Executive : IExecutive
    {
        private readonly bool enableLogging = true;
        private readonly ProtoCore.Core core;
        public ProtoCore.Core Core
        {
            get
            {
                return core;
            }
        }

        private bool isGlobScope = true;

        public Executable exe { get; set; }
        private Language executingLanguage;

        protected int pc = ProtoCore.DSASM.Constants.kInvalidPC;
        public int PC
        {
            get
            {
                return pc;
            }
        }

        private bool fepRun;
        bool terminate;

        private InstructionStream istream;
        public ProtoCore.Runtime.RuntimeMemory rmem { get; set; }

        private StackValue AX;
        private StackValue BX;
        private StackValue CX;
        private StackValue DX;
        protected StackValue EX;
        protected StackValue FX;
        private StackValue LX;

        public StackValue RX { get; set; }
        public StackValue SX { get; set; }
        public StackValue TX { get; set; }

        //public ProtoCore.AssociativeGraph.GraphNode executingGraphNode { get; private set; }
        public InterpreterProperties Properties { get; set; }

        private List<ProtoCore.AssociativeGraph.GraphNode> allSSA;

        enum DebugFlags
        {
            NONE,
            ENABLE_LOG,
            SPAWN_DEBUGGER
        }

        // Execute DS Release build
        private readonly int debugFlags = (int)DebugFlags.NONE;

        private Stack<bool> fepRunStack = new Stack<bool>();

        public int executingBlock = ProtoCore.DSASM.Constants.kInvalidIndex;

        ProtoCore.DSASM.CallingConvention.BounceType bounceType;

        public bool isExplicitCall { get; set; }

        private List<AssociativeGraph.GraphNode> deferedGraphNodes = new List<AssociativeGraph.GraphNode>();


#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
        /// <summary>
        /// Each symbol in this map is associated with a list of indices it was indexexd into
        /// It can then be refered to for every function call that requries this argument
        /// </summary>
        /// This implementation needs to be moved to the array update class
        private Dictionary<string, List<int>> symbolArrayIndexMap = new Dictionary<string,List<int>>();
#endif

        public Executive(Core core, bool isFep = false)
        {
            isExplicitCall = false;
            Validity.Assert(core != null);
            this.core = core;
            enableLogging = core.Options.Verbose;

            exe = core.DSExecutable;
            istream = null;

            fepRun = isFep;
            //executingGraphNode = null;
            //nodeExecutedSameTimes = new List<AssociativeGraph.GraphNode>();
            Properties = new InterpreterProperties();
            allSSA = new List<AssociativeGraph.GraphNode>();


            rmem = core.Rmem;

            // Execute DS View VM Log
            //
            debugFlags = (int)DebugFlags.ENABLE_LOG;

            bounceType = CallingConvention.BounceType.kImplicit;

            // Execute DS Debug watch
            //debugFlags = (int)DebugFlags.ENABLE_LOG | (int)DebugFlags.SPAWN_DEBUGGER;
        }


        private void BounceExplicit(int exeblock, int entry, Language language, StackFrame frame, List<Instruction> breakpoints)
        {
            fepRun = false;
            rmem.PushStackFrame(frame, 0, 0);

            SetupExecutive(exeblock, entry, language, breakpoints);

            bool debugRun = (0 != (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER));
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + ProtoCore.Utils.CoreUtils.GetLanguageString(language));
            }
        }


        private void BounceExplicit(int exeblock, int entry, Language language, StackFrame frame)
        {
            fepRun = false;
            rmem.PushStackFrame(frame, 0, 0);

            SetupExecutive(exeblock, entry, language);

            bool debugRun = (0 != (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER));
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + ProtoCore.Utils.CoreUtils.GetLanguageString(language));
            }
        }

        /*private void CallExplicit(int exeblock, int entry, int locals, StackFrame frame)
        {
            fepRun = true;
            rmem.PushStackFrame(frame, locals);
            SetupExecutiveForCall(exeblock, entry);
        }*/

        private void CallExplicit(int entry)
        {
            // The callsite JILFEP will have pushed the stackframe at this point
            //StackValue svClassIndex = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass);
            //Validity.Assert(AddressType.ClassIndex == svClassIndex.optype);

            //StackValue svFunctionIndex = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction);
            //Validity.Assert(AddressType.FunctionIndex == svFunctionIndex.optype);

            //int ci = (int)svClassIndex.optype;
            //int fi = (int)svFunctionIndex.optype;

            //ProcedureNode procNode = GetProcedureNode(exeblock, ci, fi);

            StackValue svFunctionBlock = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock);
            Validity.Assert(AddressType.BlockIndex == svFunctionBlock.optype);

            int exeblock = (int)svFunctionBlock.opdata;


            fepRun = true;
            SetupExecutiveForCall(exeblock, entry);
        }

        // TODO Jun: Optimization - instead of inspecting the stack, just store the 'is in function' flag in the stackframe
        // Performance would only siffer if you have so a huge number of nested language blocks
        private bool IsInsideFunction()
        {
            int fpRestore = rmem.FramePointer;
            StackValue svFrameType = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexStackFrameType);
            while (svFrameType.optype == AddressType.FrameType)
            {
                StackFrameType frametype = (StackFrameType)svFrameType.opdata;

                if (frametype == StackFrameType.kTypeFunction)
                {
                    rmem.FramePointer = fpRestore;
                    return true;
                }

                rmem.FramePointer -= StackFrame.kStackFrameSize;
                if (rmem.FramePointer >= StackFrame.kStackFrameSize)
                {
                    svFrameType = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexStackFrameType);
                }
                else
                {
                    break;
                }
            }
            rmem.FramePointer = fpRestore;
            return false;
        }


        private void RestoreRegistersFromStackFrame()
        {
            AX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterAX);
            BX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterBX);
            CX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterCX);
            DX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterDX);
            EX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterEX);
            FX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterFX);
            LX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterLX);
            //RX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterRX);
            SX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterSX);
            TX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterTX);
        }

        private void RestoreFromCall()
        {
            StackValue svExecutingBlock = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionCallerBlock);
            Validity.Assert(AddressType.BlockIndex == svExecutingBlock.optype);

            executingBlock = (int)svExecutingBlock.opdata;
            istream = exe.instrStreamList[executingBlock];

            fepRun = false;
            isGlobScope = true;

            StackFrameType callerType = (StackFrameType)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexCallerStackFrameType).opdata;
            if (callerType == StackFrameType.kTypeFunction)
            {
                fepRun = true;
                isGlobScope = false;
            }
        }

        private void RestoreFromBounce()
        {
            // Comment Jun:
            // X-lang dependency should be done for all languages 
            // as they can potentially trigger parent block updates 

            // Propagate only on lang block bounce (non fep)
            // XLangUpdateDependencyGraph requires the executingBlock to be the current running block (the block before leaving language block)
            XLangUpdateDependencyGraph(executingBlock);

            executingBlock = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionCallerBlock).opdata;
            Language currentLang = executingLanguage;

            RestoreExecutive(executingBlock);


            logVMMessage("End JIL Execution - " + ProtoCore.Utils.CoreUtils.GetLanguageString(currentLang));
        }


        private void RestoreExecutive(int exeblock)
        {
            // Jun Comment: the stackframe mpof the current language must still be present for this this method to restore the executuve
            // It must be popped off after this call
            executingLanguage = exe.instrStreamList[exeblock].language;


            // TODO Jun: Remove this check once the global bounce stackframe is pushed
            if (rmem.FramePointer >= ProtoCore.DSASM.StackFrame.kStackFrameSize)
            {
                fepRun = false;
                isGlobScope = true;

                StackFrameType callerType = (StackFrameType)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexCallerStackFrameType).opdata;
                if (callerType == StackFrameType.kTypeFunction)
                {
                    fepRun = true;
                    isGlobScope = false;
                }
            }

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            Validity.Assert(null != istream.instrList);

            pc = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexReturnAddress).opdata;
        }

        private void PushInterpreterProps(ProtoCore.InterpreterProperties properties)
        {
            core.InterpreterProps.Push(new ProtoCore.InterpreterProperties(properties));
        }

        private ProtoCore.InterpreterProperties PopInterpreterProps()
        {
            return core.InterpreterProps.Pop();
        }

        private void SetupExecutive(int exeblock, int entry, Language language)
        {
            PushInterpreterProps(Properties);
            Properties.Reset();

            if (core.ExecMode == InterpreterMode.kNormal)
            {
                exe = core.DSExecutable;
            }
            else if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                exe = core.ExprInterpreterExe;
            }
            else
            {
                Validity.Assert(false, "Invalid execution mode");
            }

            executingBlock = exeblock;

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);

            Validity.Assert(null != istream.instrList);

            if (!fepRun)
            {
                // TODO Jun: Perhaps the entrypoint now can be set from the argument 'entry' only...instead of the stream
                pc = istream.entrypoint;

                // JILFep handles function call stack frames
                rmem.FramePointer = rmem.Stack.Count;
            }
            else
            {
                pc = entry;
                isGlobScope = false;
            }

            executingLanguage = exe.instrStreamList[exeblock].language;


            string engine = ProtoCore.Utils.CoreUtils.GetLanguageString(language);
            if (Language.kAssociative == executingLanguage)
            {
                if (fepRun)
                {
                    int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                    int fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
                    UpdateMethodDependencyGraph(pc, fi, ci);
                }
                else
                {
                    if (!core.Options.IsDeltaExecution)
                    {
                        UpdateLanguageBlockDependencyGraph(pc);
                        SetupGraphEntryPoint(pc);
                    }
                    else
                    {
                        // See if we need to repond to property changed event.
                        if (UpdatePropertyChangedGraphNode())
                        {
                            SetupNextExecutableGraph(-1, -1);
                        }
                        else
                        {
                            SetupGraphEntryPoint(pc);
                        }

                        if (core.Options.ExecuteSSA)
                        {
                            ProtoCore.AssociativeEngine.Utils.SetFinalGraphNodeRuntimeDependents(Properties.executingGraphNode);
                        }
                    }
                }
            }

            if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                pc = entry;
            }

            Validity.Assert(null != rmem);
            rmem.Executable = exe;
            rmem.ClassTable = exe.classTable;
        }

        private void SetupExecutiveForCall(int exeblock, int entry)
        {
            PushInterpreterProps(Properties);
            Properties.Reset();

            if (core.ExecMode == InterpreterMode.kNormal)
            {
                exe = core.DSExecutable;
            }
            else if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                exe = core.ExprInterpreterExe;
            }
            else
            {
                Validity.Assert(false, "Invalid execution mode");
            }

            fepRun = true;
            isGlobScope = false;

            executingBlock = exeblock;


            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            Validity.Assert(null != istream.instrList);

            pc = entry;

            executingLanguage = exe.instrStreamList[exeblock].language;

            if (Language.kAssociative == executingLanguage)
            {
                int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                int fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
                UpdateMethodDependencyGraph(pc, fi, ci);
            }
        }


        public void GetCallerInformation(out int classIndex, out int functionIndex)
        {
            classIndex = Constants.kGlobalScope;
            functionIndex = Constants.kGlobalScope;

            if (rmem.FramePointer >= StackFrame.kStackFrameSize)
            {
                classIndex = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
                functionIndex = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;
            }

            if (functionIndex != Constants.kGlobalScope && classIndex == Constants.kGlobalScope)
            {
                Dictionary<string, FunctionGroup> fgps = core.FunctionTable.GlobalFuncTable[classIndex + 1];
                if (fgps[Constants.kDotArgMethodName].FunctionEndPoints[0].procedureNode.procId == functionIndex)
                {
                    int lastFramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                    if (lastFramePointer >= StackFrame.kStackFrameSize)
                    {
                        classIndex = (int)rmem.Stack[lastFramePointer + StackFrame.kFrameIndexClass].opdata;
                        functionIndex = (int)rmem.Stack[lastFramePointer + StackFrame.kFrameIndexFunction].opdata;
                    }
                }
            }
        }

        private StackValue IndexIntoArray(StackValue sv, List<StackValue> dimensions)
        {
            if (null == dimensions || dimensions.Count <= 0)
            {
                return sv;
            }

            if (!StackUtils.IsArray(sv))
            {
                sv = StackValue.Null;
            }

            StackValue ret = GetIndexedArray(sv, dimensions);
            GCRetain(ret);
            GCRelease(sv);

            return ret;
        }

        public void GCDotMethods(string name, ref StackValue sv, List<StackValue> dotCallDimensions = null, List<StackValue> arguments = null)
        {
            // Index into the resulting array
            if (name == ProtoCore.DSASM.Constants.kDotMethodName)
            {
                sv = IndexIntoArray(sv, dotCallDimensions);

                // Dot arg parameters is a special case function and must be GC'd here
                int stackPtr = rmem.Stack.Count - 1;
                GCRelease(rmem.Stack[stackPtr]);
                GCRelease(rmem.Stack[stackPtr - 1]);
                GCRelease(rmem.Stack[stackPtr - 2]);
                GCRelease(rmem.Stack[stackPtr - 3]);
                GCRelease(rmem.Stack[stackPtr - 4]);

                rmem.PopFrame(ProtoCore.DSASM.Constants.kDotCallArgCount);
            }
            else if (name == ProtoCore.DSASM.Constants.kDotArgMethodName)
            {
                if (arguments.Count == Constants.kDotCallArgCount
                    && (int)arguments[Constants.kDotArgIndexDimCount].opdata > 0)
                {
                    StackValue[] dims = rmem.GetArrayElements(arguments[Constants.kDotArgIndexArrayIndex]);
                    StackValue ret = GetIndexedArray(sv, dims.ToList());
                    GCRetain(ret);
                    GCRelease(sv);
                    sv = ret;
                }

                for (int i = 0; i < arguments.Count; ++i)
                {
                    GCRelease(arguments[i]);
                }
            }

            //return sv;
        }


#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL

        //proc GetSymbolIndexedIntoList(string symbol, out List<int> indexList)
        //    if symbolIndexMap.exists
        //        indexList = symbolIndexMap[symbol]
        //        return true
        //    end
        //    return false
        //end 

        /// <summary>
        /// Retrives the symbol in the index into list
        /// This implementation needs to be moved to the array update class
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="indexList"></param>
        /// <returns></returns>
        private bool GetSymbolIndexedIntoList(string symbol, out List<int> indexList)
        {
            indexList = null;
            if (symbolArrayIndexMap.ContainsKey(symbol))
            {
                indexList = symbolArrayIndexMap[symbol];
                return true;
            }
            return false;
        } 
#endif

        public StackValue Callr(int functionIndex, int classIndex, int depth, ref bool explicitCall, bool isDynamicCall = false, bool hasDebugInfo = false)
        {
            isGlobScope = false;
            ProcedureNode fNode = null;

            // Comment Jun: this is curently unused but required for stack alignment
            if (!isDynamicCall)
            {
                StackValue svType = rmem.Pop();
                runtimeVerify(AddressType.StaticType == svType.optype);
                int lhsStaticType = (int)svType.metaData.type;
            }


            // Pop off number of dimensions indexed into this function call
            // f()[0][1] -> 2 dimensions
            StackValue svDim = rmem.Pop();
            runtimeVerify(AddressType.ArrayDim == svDim.optype);
            int fCallDimensions = (int)svDim.opdata;


            // Jun Comment: The block where the function was declared in
            StackValue svBlockDeclaration = rmem.Pop();
            runtimeVerify(AddressType.BlockIndex == svBlockDeclaration.optype);
            int blockDeclId = (int)svBlockDeclaration.opdata;

            // Jun Comment: The block where the function is called from
            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != core.RunningBlock);
            StackValue svBlockCaller = StackValue.BuildBlockIndex(core.RunningBlock);


            bool isMember = ProtoCore.DSASM.Constants.kInvalidIndex != classIndex;
            if (isMember)
            {
                // Constructor or member function
                fNode = exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex];

                if (depth > 0)
                {
                    if (fNode.isConstructor)
                    {
                        string message = String.Format(ProtoCore.RuntimeData.WarningMessage.KCallingConstructorOnInstance, fNode.name);
                        core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kCallingConstructorOnInstance, message);
                        isGlobScope = true;
                        StackValue nullSv = StackValue.Null;
                        return nullSv;
                    }
                }
            }
            else
            {
                // Global function
                fNode = exe.procedureTable[blockDeclId].procList[functionIndex];
            }

            // Build the arg values list
            List<StackValue> arguments = new List<StackValue>();
            List<ProtoCore.ReplicationGuide> replicationGuideList = null;
            List<List<ProtoCore.ReplicationGuide>> replicationGuides = new List<List<ProtoCore.ReplicationGuide>>();

            // Retrive the param values from the stack
            int stackindex = rmem.Stack.Count - 1;

            int argtype_i = fNode.argTypeList.Count - 1;
            int argFrameSize = 0;


            List<StackValue> dotCallDimensions = new List<StackValue>();
            if (fNode.name == ProtoCore.DSASM.Constants.kDotMethodName)
            {
                int firstDotArgIndex = stackindex - (ProtoCore.DSASM.Constants.kDotCallArgCount - 1);
                StackValue svLHS = rmem.Stack[firstDotArgIndex];

                arguments.Add(svLHS);
                argFrameSize = ProtoCore.DSASM.Constants.kDotArgCount;

                int functionArgsIndex = stackindex - (ProtoCore.DSASM.Constants.kDotCallArgCount - ProtoCore.DSASM.Constants.kDotArgIndexArgCount - 1);
                StackValue svArrayPtrFunctionArgs = rmem.Stack[functionArgsIndex];
                GCRetain(svArrayPtrFunctionArgs);


                // Retrieve the indexed dimensions into the dot call
                int arrayDimIndex = stackindex - (ProtoCore.DSASM.Constants.kDotCallArgCount - ProtoCore.DSASM.Constants.kDotArgIndexArrayIndex - 1);
                StackValue svArrayPtrDimesions = rmem.Stack[arrayDimIndex];
                Validity.Assert(svArrayPtrDimesions.optype == AddressType.ArrayPointer);

                int arrayCountIndex = stackindex - (ProtoCore.DSASM.Constants.kDotCallArgCount - ProtoCore.DSASM.Constants.kDotArgIndexDimCount - 1);
                StackValue svDimensionCount = rmem.Stack[arrayCountIndex];
                Validity.Assert(svDimensionCount.optype == AddressType.Int);


                // If array dimension were provided then retrive the final pointer 
                if (svDimensionCount.opdata > 0)
                {
                    HeapElement he = rmem.Heap.Heaplist[(int)svArrayPtrDimesions.opdata];
                    Validity.Assert(he.VisibleSize == svDimensionCount.opdata);
                    for (int n = 0; n < he.VisibleSize; ++n)
                    {
                        dotCallDimensions.Add(he.Stack[n] /*(int)he.Stack[n].opdata*/);
                    }
                }
            }
            else
            {
                for (int p = 0; p < fNode.argTypeList.Count; ++p)
                {
                    // Must iterate through the args in the stack in reverse as its unknown how many replication guides were pushed
                    StackValue value = rmem.Stack[stackindex--];
                    ++argFrameSize;
                    arguments.Add(value);

                    if (fNode.name != ProtoCore.DSASM.Constants.kDotArgMethodName)
                    {
                        bool hasGuide = (AddressType.ReplicationGuide == rmem.Stack[stackindex].optype);
                        if (hasGuide)
                        {
                            replicationGuideList = new List<ProtoCore.ReplicationGuide>();

                            // Retrieve replication guides
                            value = rmem.Stack[stackindex--];
                            ++argFrameSize;
                            runtimeVerify(AddressType.ReplicationGuide == value.optype);

                            int guides = (int)value.opdata;
                            if (guides > 0)
                            {
                                for (int i = 0; i < guides; ++i)
                                {
                                    // Get the replicationguide number from the stack
                                    value = rmem.Stack[stackindex--];
                                    Validity.Assert(value.optype == AddressType.Int);
                                    int guideNumber = (int)value.opdata;

                                    // Get the replication guide property from the stack
                                    value = rmem.Stack[stackindex--];
                                    Validity.Assert(value.optype == AddressType.Boolean);
                                    bool isLongest = (int)value.opdata == 1 ? true : false;

                                    ProtoCore.ReplicationGuide guide = new ReplicationGuide(guideNumber, isLongest);
                                    replicationGuideList.Add(guide);
                                    ++argFrameSize;
                                }
                            }
                            replicationGuideList.Reverse();
                            replicationGuides.Add(replicationGuideList);
                        }
                    }
                }

                // Pop off frame information 
                rmem.Pop(argFrameSize);
            }

            replicationGuides.Reverse();
            arguments.Reverse();

            Runtime.Context runtimeContext = new Runtime.Context();

#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL

            //
            // Comment Jun: Retrieve the indices used to index in an argument

            //
            //  List<List<int>> indexIntoList
            //  foreach arg in functionNode.args
            //      
            //      Iterate over the symbols in the executing graph node
	        //      foreach symbol in executingGraphNode.dependents
            //          List<int> argIndexInto = GetSymbolIndexedIntoList(symbol)
            //          indexIntoList.push(argIndexInto)
            //      end            
            //      context.indexlist = indexIntoList
            //      sv = JILDispatch.callsite(function, args, context, ...)
            //  end
            //

            if (null != Properties.executingGraphNode 
                && null != Properties.executingGraphNode.dependentList 
                && Properties.executingGraphNode.dependentList.Count > 0 )
            {
                // Save the LHS of this graphnode
                runtimeContext.ArrayPointer = Properties.executingGraphNode.ArrayPointer;

                // Iterate over the symbols in the executing graph node
                for (int n = 0; n < Properties.executingGraphNode.dependentList.Count; n++)
                {
                    List<int> indexIntoList = new List<int>();
                    {
                        // Check if the current dependent was indexed into
                        ProtoCore.DSASM.SymbolNode argSymbol = Properties.executingGraphNode.dependentList[n].updateNodeRefList[0].nodeList[0].symbol;
                        if (symbolArrayIndexMap.ContainsKey(argSymbol.name))
                        {
                            indexIntoList = symbolArrayIndexMap[argSymbol.name];
                        }
                    }
                    runtimeContext.IndicesIntoArgMap.Add(indexIntoList);
                }
            }
#endif

            // Comment Jun: These function do not require replication guides
            // TODO Jun: Move these conditions or refactor JIL code emission so these checks dont reside here (Post R1)
            if (ProtoCore.DSASM.Constants.kDotMethodName != fNode.name
                && ProtoCore.DSASM.Constants.kDotArgMethodName != fNode.name
                && ProtoCore.DSASM.Constants.kFunctionRangeExpression != fNode.name)
            {
                // Comment Jun: If this is a non-dotarg call, cache the guides first and retrieve them on the actual function call
                // TODO Jun: Ideally, cache the replication guides in the dynamic function node
                replicationGuides = GetCachedReplicationGuides(core, arguments.Count);
            }

            bool isCallingDotArgCall = fNode.name == ProtoCore.DSASM.Constants.kDotArgMethodName;
            if (isCallingDotArgCall)
            {
                // Comment Jun: (Sept 8 2012) Check with Yuke what the intention of this is to the dotarg arguments
                for (int i = 0; i < arguments.Count; ++i)
                {
                    GCRetain(arguments[i]);
                }
            }


            // if is dynamic call, the final pointer has been resovled in the ProcessDynamicFunction function
            StackValue svThisPtr = StackValue.Null;

            if (depth > 0)
            {
                // locals are not yet in the stack so there is no need to account for that in this stack frame

                if (isDynamicCall)
                {
                    svThisPtr = rmem.Pop();
                }
                else
                {
                    svThisPtr = GetFinalPointer(depth);
                }

                // 
                if (svThisPtr.optype != AddressType.Pointer)
                {
                    string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kInvokeMethodOnInvalidObject, fNode.name);
                    core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kDereferencingNonPointer, message);
                    isGlobScope = true;
                    return StackValue.Null;
                }
            }
            else
            {
                // There is no depth but check if the function is a member function
                // If its a member function, the this pointer is required by the core to pass on to the FEP call
                if (isMember && !fNode.isConstructor && !fNode.isStatic)
                {
                    // A member function
                    // Get the this pointer as this class instance would have already been cosntructed
                    svThisPtr = rmem.GetAtRelative(rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr));

                }
                else
                {
                    // Global
                    svThisPtr = ProtoCore.DSASM.StackValue.BuildPointer(ProtoCore.DSASM.Constants.kInvalidPointer);
                }
            }

            if (svThisPtr.optype == AddressType.Pointer &&
                svThisPtr.opdata != Constants.kInvalidIndex &&
                svThisPtr.metaData.type != Constants.kInvalidIndex)
            {
                int runtimeClassIndex = (int)svThisPtr.metaData.type;
                ClassNode runtimeClass = core.ClassTable.ClassNodes[runtimeClassIndex];
                if (runtimeClass.IsMyBase(classIndex))
                {
                    classIndex = runtimeClassIndex;
                }
            }

            // Build the stackframe
            //int thisPtr = (int)svThisPtr.opdata;
            int ci = classIndex; // ProtoCore.DSASM.Constants.kInvalidIndex;   // Handled at FEP
            int fi = ProtoCore.DSASM.Constants.kInvalidIndex;   // Handled at FEP

            int returnAddr = pc + 1;

            int blockDecl = (int)svBlockDeclaration.opdata;
            int blockCaller = (int)svBlockCaller.opdata;
            int framePointer = ProtoCore.DSASM.Constants.kInvalidIndex;
            framePointer = rmem.FramePointer;


            bool isInDotArgCall = false;
            int currentClassIndex = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            int currentFunctionIndex = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
            bool isGlobalScope = ProtoCore.DSASM.Constants.kGlobalScope == currentClassIndex && ProtoCore.DSASM.Constants.kGlobalScope == currentFunctionIndex;
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                if (ProtoCore.DSASM.Constants.kGlobalScope != currentFunctionIndex)
                {
                    int currentFunctionDeclBlock = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock).opdata;
                    ProcedureNode currentProcCall = GetProcedureNode(currentFunctionDeclBlock, currentClassIndex, currentFunctionIndex);
                    isInDotArgCall = currentProcCall.name == ProtoCore.DSASM.Constants.kDotArgMethodName;
                }
            }

            if (isGlobalScope || !isInDotArgCall)
            {
                if (null != Properties.executingGraphNode)
                {
                    core.ExecutingGraphnodeUID = Properties.executingGraphNode.UID;
                }
            }

            // Get the cached callsite, creates a new one for a first-time call
            ProtoCore.CallSite callsite = core.GetCallSite(core.ExecutingGraphnodeUID, classIndex, fNode.name);
            Validity.Assert(null != callsite);



            StackFrameType type = StackFrameType.kTypeFunction;
            int blockDepth = 0;

            List<StackValue> registers = new List<StackValue>();
            SaveRegisters(registers);

            // Comment Jun: the caller type is the current type in the stackframe
            StackFrameType callerType = (fepRun) ? StackFrameType.kTypeFunction : StackFrameType.kTypeLanguage;

            // Get the execution states of the current stackframe
            int currentScopeClass = ProtoCore.DSASM.Constants.kInvalidIndex;
            int currentScopeFunction = ProtoCore.DSASM.Constants.kInvalidIndex;
            GetCallerInformation(out currentScopeClass, out currentScopeFunction);

            List<bool> execStates = new List<bool>();

            //
            // Comment Jun:
            // Storing execution states is relevant only if the current scope is a function,
            // as this mechanism is used to keep track of maintining execution states of recursive calls
            // This mechanism should also be ignored if the function call is non-recursive as it does not need to maintains state in that case
            //
            if (currentScopeFunction != ProtoCore.DSASM.Constants.kGlobalScope)
            {
                // Get the instruction stream where the current function resides in
                StackValue svCurrentFunctionBlockDecl = rmem.GetAtRelative(rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock));
                Validity.Assert(svCurrentFunctionBlockDecl.optype == AddressType.BlockIndex);
                AssociativeGraph.DependencyGraph depgraph = exe.instrStreamList[(int)svCurrentFunctionBlockDecl.opdata].dependencyGraph;

                // Allow only if this is a recursive call
                // It is recursive if the current function scope is equal to the function to call
                bool isRecursive = fNode.classScope == currentScopeClass && fNode.procId == currentScopeFunction;
                if (isRecursive)
                {
                    // Get the graphnodes of the function from the instruction stream and retrive the execution states
                    execStates = depgraph.GetExecutionStatesAtScope(currentScopeClass, currentScopeFunction);
                }
            }
            

            // Build the stackfram for the functioncall
            ProtoCore.DSASM.StackFrame stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, blockDepth, framePointer, registers, execStates);

            FunctionCounter counter = FindCounter(functionIndex, classIndex, fNode.name);
            StackValue sv = StackValue.Null;


            if (core.Options.RecursionChecking)
            {
                //Do the recursion check before call
                if (counter.times < ProtoCore.DSASM.Constants.kRecursionTheshold) //&& counter.sharedCounter < ProtoCore.DSASM.Constants.kRepetationTheshold)
                {

                    // Build a context object in JILDispatch and call the Dispatch
                    if (counter.times == 0)
                    {
                        counter.times++;
                        core.calledInFunction = true;
                    }

                    else if (counter.times >= 1)
                    {
                        if (fNode.name.ToCharArray()[0] != '%' && fNode.name.ToCharArray()[0] != '_' && !fNode.name.Equals(ProtoCore.DSASM.Constants.kDotMethodName) && core.calledInFunction)
                        {
                            counter.times++;
                        }
                    }


                    if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
                    {
                        bool isBaseCall = false;
                        core.DebugProps.SetUpCallrForDebug(core, this, fNode, pc, isBaseCall, callsite, arguments, replicationGuides, stackFrame, dotCallDimensions, hasDebugInfo);
                    }

                    sv = callsite.JILDispatch(arguments, replicationGuides, stackFrame, core, runtimeContext);
                }
                else
                {
                    FindRecursivePoints();
                    string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kMethodStackOverflow, core.recursivePoint[0].name);
                    core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kInvalidRecursion, message);

                    core.recursivePoint = new List<FunctionCounter>();
                    core.funcCounterTable = new List<FunctionCounter>();
                    sv = StackValue.Null;
                }
            }
            else
            {
                if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    bool isBaseCall = false;
                    if (core.ContinuationStruct.IsFirstCall)
                    {
                        core.DebugProps.SetUpCallrForDebug(core, this, fNode, pc, isBaseCall, callsite, arguments, replicationGuides, stackFrame, dotCallDimensions, hasDebugInfo);
                    }
                    else
                    {
                        core.DebugProps.SetUpCallrForDebug(core, this, fNode, pc, isBaseCall, callsite, core.ContinuationStruct.InitialArguments, replicationGuides, stackFrame,
                        core.ContinuationStruct.InitialDotCallDimensions, hasDebugInfo);
                    }
                }

                SX = svBlockDeclaration;
                stackFrame.SetAt(StackFrame.AbsoluteIndex.kRegisterSX, svBlockDeclaration);

                //Dispatch without recursion tracking 
                explicitCall = false;
                isExplicitCall = explicitCall;

#if __DEBUG_REPLICATE
                // TODO: This if block is currently executed only for a replicating function call in Debug Mode (including each of its continuations) 
                // This condition will no longer be required once the same Dispatch function can decide whether to perform a fast dispatch (parallel mode)
                // OR a Serial/Debug mode dispatch, in which case this same block should work for Serial mode execution w/o the Debug mode check - pratapa
                if (core.Options.IDEDebugMode)
                {
                    DebugFrame debugFrame = core.DebugProps.DebugStackFrame.Peek();

                    //if (debugFrame.IsReplicating || core.ContinuationStruct.IsContinuation)
                    if (debugFrame.IsReplicating)
                    {
                        FunctionEndPoint fep = null;
                        ContinuationStructure cs = core.ContinuationStruct;

                        if (core.Options.ExecutionMode == ProtoCore.ExecutionMode.Serial || core.Options.IDEDebugMode)
                        {
                            // This needs to be done only for the initial argument arrays (ie before the first replicating call) - pratapa
                            if(core.ContinuationStruct.IsFirstCall)
                            {
                                core.ContinuationStruct.InitialDepth = depth;
                                core.ContinuationStruct.InitialPC = pc;
                                core.ContinuationStruct.InitialArguments = arguments;
                                core.ContinuationStruct.InitialDotCallDimensions = dotCallDimensions;

                                for (int i = 0; i < arguments.Count; ++i)
                                {
                                    GCUtils.GCRetain(arguments[i], core);
                                }

                                // Hardcoded
                                core.ContinuationStruct.NextDispatchArgs.Add(StackValue.BuildInt(1));
                            }

                            // The Resolve function is currently hard-coded as a place holder to test debugging replication - pratapa
                            fep = callsite.ResolveForReplication(new ProtoCore.Runtime.Context(), arguments, replicationGuides, stackFrame, core, cs);
                            
                            // TODO: Refactor following steps into new function (ExecWithZeroRI + JILFep.Execute) to be called from here - pratapa
                            // Get final FEP by calling SelectFinalFep()
                            // Update DebugProps with final FEP
                            // Call finalFEP.CoerceParameters()
                            // Setup stackframe
                            // Push Stackframe
                            sv = callsite.ExecuteContinuation(fep, stackFrame, core);

                            core.ContinuationStruct = cs;
                            core.ContinuationStruct.IsFirstCall = true;

                        }
                    }
                    else
                        sv = callsite.JILDispatch(arguments, replicationGuides, stackFrame, core);
                }
                else
#endif
                sv = callsite.JILDispatch(arguments, replicationGuides, stackFrame, core, runtimeContext);

                if (sv.optype == AddressType.ExplicitCall)
                {
                    //
                    // Set the interpreter properties for function calls
                    // These are used when performing GC on return 
                    // The GC occurs: 
                    //      1. In this instruction for implicit calls
                    //      2. In the return instruction
                    //
                    Properties.functionCallArguments = arguments;

                    Properties.functionCallDotCallDimensions = dotCallDimensions;

                    explicitCall = true;
                    isExplicitCall = explicitCall;
                    int entryPC = (int)sv.opdata;

                    CallExplicit(entryPC);
                }
#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
                else
                {
                    if (null != Properties.executingGraphNode)
                    {
                        Properties.executingGraphNode.ArrayPointer = sv;
                    }
                }
#endif
            }

            // If the function was called implicitly, The code below assumes this and must be executed
            if (!explicitCall)
            {
                // Restore debug properties after returning from a CALL/CALLR
                if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    core.DebugProps.RestoreCallrForNoBreak(core, fNode);
                }

                GCDotMethods(fNode.name, ref sv, dotCallDimensions, arguments);

                DecRefCounter(sv);
                if (sv.optype == AddressType.ArrayPointer)
                {
                    // GCReleasePool.Add(sv);
                }

                isGlobScope = true;
                if (fNode.name.ToCharArray()[0] != '%' && fNode.name.ToCharArray()[0] != '_')
                {
                    core.calledInFunction = false;
                    //@TODO(Gemeng, Luke): Remove me
                    //Console.WriteLine("flag changed \t" + core.calledInFunction);
                }
            }
            return sv;
        }

        private void FindRecursivePoints()
        {
            foreach (FunctionCounter c in core.funcCounterTable)
            {

                if (c.times == ProtoCore.DSASM.Constants.kRecursionTheshold || c.times == ProtoCore.DSASM.Constants.kRecursionTheshold - 1)
                {
                    core.recursivePoint.Add(c);
                }
                //else if (c.sharedCounter == ProtoCore.DSASM.Constants.kRepetationTheshold || (c.sharedCounter == ProtoCore.DSASM.Constants.kRepetationTheshold - 1 || c.sharedCounter == ProtoCore.DSASM.Constants.kRepetationTheshold + 1)
                //{
                core.recursivePoint.Add(c);
                //}
            }

        }


        private FunctionCounter FindCounter(int funcIndex, int classScope, string name)
        {
            foreach (FunctionCounter c in core.funcCounterTable)
            {

                if (c.classScope == classScope && c.functionIndex == funcIndex)
                {
                    // Comment it out. Looks this foreach loop is dead code. 
                    // - Yu Ke
                    /*
                    foreach (FunctionCounter c2 in core.funcCounterTable)
                    {
                        if (c.name.Equals(c2.name) && c2.name.ToCharArray()[0] != '%' && c2.name.ToCharArray()[0] != '_')
                        {

                            //c.sharedCounter++;
                            if (c != c2)
                            {
                                //  c2.sharedCounter++;
                            }
                        }
                    }
                    */

                    return c;
                }

            }
            FunctionCounter newC = new FunctionCounter(funcIndex, classScope, 0, name, 1);
            foreach (FunctionCounter c in core.funcCounterTable)
            {
                if (c.name.Equals(newC.name))
                {
                    //c.sharedCounter++;
                }
            }
            core.funcCounterTable.Add(newC);
            return newC;
        }

        private void logVMMessage(string msg)
        {
            if (!enableLogging)
                return;

            if (0 != (debugFlags & (int)DebugFlags.ENABLE_LOG))
            {
                if (exe.EventSink != null && exe.EventSink.PrintMessage != null)
                    exe.EventSink.PrintMessage.Invoke("VMLog: " + msg + "\n");
                if (core.Options.WebRunner && (null != core.ExecutionLog))
                    core.ExecutionLog.WriteLine(msg);
            }
        }
        private void logMessage(string msg)
        {
            if (!enableLogging)
                return;

            if (0 != (debugFlags & (int)DebugFlags.ENABLE_LOG))
            {
                if (exe.EventSink != null && exe.EventSink.PrintMessage != null)
                    exe.EventSink.PrintMessage.Invoke(msg + "\n");
                if (core.Options.WebRunner && (null != core.ExecutionLog))
                    core.ExecutionLog.WriteLine(msg);
            }
        }
        private void logWatchWindow(int blockId, int index)
        {
            if (!enableLogging)
                return;

            const string watchPrompt = "watch: ";
            if (0 != (debugFlags & (int)DebugFlags.ENABLE_LOG))
            {
                string symbol = core.DSExecutable.runtimeSymbols[blockId].symbolList[index].name;
                if (symbol.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix))
                {
                    return;
                }
                int ci = core.DSExecutable.runtimeSymbols[blockId].symbolList[index].classScope;
                if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    symbol = core.ClassTable.ClassNodes[ci].name + "::" + symbol;
                }
                string lhs = watchPrompt + symbol;

                if (null != core.DSExecutable.runtimeSymbols[blockId].symbolList[index].arraySizeList)
                {
                    lhs = lhs + "[" + "offset:" + DX + "]";
                }

                string rhs = null;
                StackValue snode = rmem.GetStackData(blockId, index, Constants.kGlobalScope);
                if (AddressType.Pointer == snode.optype)
                {
                    int type = (int)snode.metaData.type;
                    string cname = core.ClassTable.ClassNodes[type].name;

                    Int64 ptr = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata;
                    rhs = cname + ":ptr(" + ptr.ToString() + ")";
                }
                else if (AddressType.ArrayPointer == snode.optype)
                {
                    Int64 ptr = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata;
                    rhs = "Array:ptr(" + ptr + "):{" + GetArrayTrace((int)ptr, blockId, index, new HashSet<int> { (int)ptr }) + "}";
                }
                else if (AddressType.FunctionPointer == snode.optype)
                {
                    Int64 fptr = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata;
                    rhs = "fptr: " + fptr.ToString();
                }
                else if (AddressType.Int == snode.optype)
                {
                    Int64 data = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata;
                    rhs = data.ToString();
                }
                else if (AddressType.Double == snode.optype)
                {
                    double data = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata_d;
                    rhs = data.ToString("R").IndexOf('.') != -1 ? data.ToString("R") : data.ToString("R") + ".0";
                }
                else if (AddressType.Boolean == snode.optype)
                {
                    bool data = (rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata == 0) ? false : true;
                    rhs = data.ToString().ToLower();
                }
                else if (AddressType.Char == snode.optype)
                {
                    Int64 data = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata;
                    Char character = ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(data);
                    rhs = "'" + character + "'";
                }
                else if (AddressType.String == snode.optype)
                {
                    Int64 ptr = rmem.GetStackData(blockId, index, Constants.kGlobalScope).opdata;
                    rhs = UnboxString((int)ptr, blockId, index);
                }
                else if (AddressType.Null == snode.optype)
                {
                    rhs = Literal.Null;
                }

                if (exe.EventSink != null
                    && exe.EventSink.PrintMessage != null)
                {
                    exe.EventSink.PrintMessage.Invoke(lhs + " = " + rhs + "\n");
                    if (core.Options.WebRunner && (null != core.ExecutionLog))
                        core.ExecutionLog.WriteLine(lhs + " = " + rhs + "\n");
                }
            }
        }

        private string UnboxArray(StackValue snode, int blockId, int index)
        {
            String rhs = null;
            if (AddressType.ArrayPointer == snode.optype)
            {
                Int64 ptr = snode.opdata;
                rhs = "{" + GetArrayTrace((int)ptr, blockId, index, new HashSet<int> { (int)ptr }) + "}";
            }
            else if (AddressType.Int == snode.optype)
            {
                Int64 data = snode.opdata;
                rhs = data.ToString();
            }
            else if (AddressType.Double == snode.optype)
            {
                double data = snode.opdata_d;
                rhs = data.ToString("R").IndexOf('.') != -1 ? data.ToString("R") : data.ToString("R") + ".0";
            }
            else if (AddressType.Boolean == snode.optype)
            {
                bool data = snode.opdata == 0 ? false : true;
                rhs = data.ToString().ToLower();
            }
            else if (AddressType.Null == snode.optype)
            {
                rhs = Literal.Null;
            }
            else if (AddressType.Char == snode.optype)
            {
                Int64 data = snode.opdata;
                Char character = ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(data);
                rhs = "'" + character + "'";
            }
            else if (AddressType.String == snode.optype)
            {
                Int64 ptr = snode.opdata;
                rhs = UnboxString((int)ptr, blockId, index);
            }
            else if (AddressType.Pointer == snode.optype)
            {
                int type = (int)snode.metaData.type;
                string cname = core.ClassTable.ClassNodes[type].name;
                rhs = cname + ":ptr(" + snode.opdata.ToString() + ")";
            }
            return rhs;
        }

        private string UnboxString(int pointer, int blockId, int index)
        {
            HeapElement hs = rmem.Heap.Heaplist[pointer];

            string str = "";
            for (int n = 0; n < hs.VisibleSize; ++n)
            {
                if (hs.Stack[n].optype != AddressType.Char)
                    return null;
                str += ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(hs.Stack[n].opdata);
            }
            if (str == "")
                return null;

            return "\"" + str + "\"";
        }

        private string GetArrayTrace(int pointer, int blockId, int index, HashSet<int> pointers)
        {
            StringBuilder arrayelements = new StringBuilder();
            HeapElement hs = rmem.Heap.Heaplist[pointer];

            for (int n = 0; n < hs.VisibleSize; ++n)
            {
                StackValue sv = hs.Stack[n];
                if (sv.optype == AddressType.ArrayPointer)
                {
                    int ptr = (int)sv.opdata;
                    if (pointers.Contains(ptr))
                    {
                        arrayelements.Append("{...}");
                    }
                    else
                    {
                        pointers.Add(ptr);
                        arrayelements.Append("{" + GetArrayTrace(ptr, blockId, index, pointers) + "}");
                    }
                }
                else
                {
                    arrayelements.Append(UnboxArray(hs.Stack[n], blockId, index));
                }

                if (n < hs.VisibleSize - 1)
                {
                    arrayelements.Append(",");
                }
            }
            return arrayelements.ToString();
        }

        //proc SetupNextExecutableGraph
        //    Find the first dirty node and execute it
        //    foreach node in graphNodeList
        //        if node.isDirty is true
        //            node.isDirty = false
        //            pc = node.updateBlock.startpc
        //        end	
        //    end
        //end
        public void SetupNextExecutableGraph(int function, int classscope)
        {
            Validity.Assert(istream != null);
            if (istream.language != Language.kAssociative)
            {
                return;
            }

            bool isUpdated = false;
            List<AssociativeGraph.GraphNode> graphNodes = istream.dependencyGraph.GetGraphNodesAtScope(classscope, function);
            if (graphNodes != null)
            {
                foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in graphNodes)
                {
                    if (!graphNode.isDirty || !graphNode.isActive)
                    {
                        continue;
                    }
                    // Is return node or is updatable
                    if (graphNode.isReturn || graphNode.updateNodeRefList[0].nodeList.Count > 0)
                    {
                        graphNode.isDirty = false;
                        if (core.Options.ExecuteSSA)
                        {
                            ProtoCore.AssociativeEngine.Utils.SetFinalGraphNodeRuntimeDependents(graphNode);
                        }

                        // In function calls, the first graphnode in the function is executed first and was not marked 
                        // If this is the case, just move on to the next graphnode
                        if (pc == graphNode.updateBlock.endpc)
                        {
                            continue;
                        }

                        pc = graphNode.updateBlock.startpc;
                        isUpdated = true;

                        if (graphNode.forPropertyChanged)
                        {
                            Properties.updateStatus = AssociativeEngine.UpdateStatus.kPropertyChangedUpdate;
                            graphNode.forPropertyChanged = false;
                        }
                        else
                        {
                            Properties.updateStatus = AssociativeEngine.UpdateStatus.kNormalUpdate;
                        }

                        // Set the current graphnode being executed
                        Properties.executingGraphNode = graphNode;
                        core.RuntimeExpressionUID= graphNode.exprUID;

                        if (core.Options.dynamicCycleCheck)
                        {
                            if (!HasCyclicDependency(graphNode))
                            {
                                //count how many times one graphNode has been edited
                                graphNode.counter++;
                            }
                            else
                            {
                                // If the dependency cycle is not completed, keep 
                                // adding nodes
                                if (!Properties.nodeIterations.Contains(graphNode))
                                {
                                    Properties.nodeIterations.Add(graphNode);
                                }
                                // If all nodes had been added to current cycle, find 
                                // start node and end node, then clear the counters 
                                // and the cycle print out err msg
                                else
                                {
                                    if (Properties.nodeIterations.Count != 1)
                                    {
                                        handleCycle(graphNode);
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (!isUpdated)
            {
                // There were no updates, this is the end of this associative block
                pc = ProtoCore.DSASM.Constants.kInvalidPC;
            }
        }

        private void SetupGraphEntryPoint(int entrypoint)
        {
            // Find the graph where the entry point matches the graph pc
            // Set that graph node as not dirty   
            foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in istream.dependencyGraph.GraphList)
            {
                if (core.Options.IsDeltaExecution)
                {
                    // COmment Jun: start from graphnodes whose update blocks are in the range of the entry point
                    bool inStartRange = graphNode.updateBlock.startpc >= entrypoint;

                    if (graphNode.isDirty && inStartRange)
                    {
                        pc = graphNode.updateBlock.startpc;
                        graphNode.isDirty = false;
                        Properties.executingGraphNode = graphNode;
                        core.RuntimeExpressionUID = graphNode.exprUID;
                        break;
                    }
                }
                else if (graphNode.updateBlock.startpc == entrypoint)
                {
                    Properties.executingGraphNode = graphNode;
                    core.RuntimeExpressionUID = graphNode.exprUID;
                    if (graphNode.isDirty)
                    {
                        graphNode.isDirty = false;
                        //count how many times one graphNode has been edited
                        graphNode.counter++;
                        break;
                    }
                }
            }
        }

        private void handleCycle(ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            ProtoCore.AssociativeGraph.GraphNode[] CycleStartNodeAndEndNode = new ProtoCore.AssociativeGraph.GraphNode[2];

            List<AssociativeGraph.GraphNode> nodeIterations = Properties.nodeIterations;// core.InterpreterProps.Peek().nodeIterations;
            CycleStartNodeAndEndNode = FindCycleStartNodeAndEndNode(nodeIterations);

            foreach (ProtoCore.AssociativeGraph.GraphNode node in nodeIterations)
            {
                Console.WriteLine("nodes " + node.updateNodeRefList[0].nodeList[0].symbol.name);
            }

            string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kCyclicDependency, CycleStartNodeAndEndNode[0].updateNodeRefList[0].nodeList[0].symbol.name, CycleStartNodeAndEndNode[1].updateNodeRefList[0].nodeList[0].symbol.name);
            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kCyclicDependency, message);
            //BreakDependency(NodeExecutedSameTimes);
            foreach (ProtoCore.AssociativeGraph.GraphNode node in nodeIterations)
            {
                node.isCyclic = true;
                SetGraphNodeStackValueNull(node);
                node.dependentList.Clear();
            }
            Properties.nodeIterations = new List<AssociativeGraph.GraphNode>();
        }

        private void handleSSAAssignCycle(ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kCyclicDependency, graphNode.updateNodeRefList[0].nodeList[0].symbol.name, graphNode.updateNodeRefList[0].nodeList[0].symbol.name);
            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kCyclicDependency, message);

            List<AssociativeGraph.GraphNode> nodeIterations = core.InterpreterProps.Peek().nodeIterations;
            foreach (ProtoCore.AssociativeGraph.GraphNode node in nodeIterations)
            {
                node.isCyclic = true;
                SetGraphNodeStackValueNull(node);
                node.dependentList.Clear();
            }
            Properties.nodeIterations = null;
        }

        private bool HasCyclicDependency(ProtoCore.AssociativeGraph.GraphNode node)
        {

            //if (IsExecutedTooManyTimes(node, ProtoCore.DSASM.Constants.kDynamicCycleThreshold))
            if (IsExecutedTooManyTimes(node, core.Options.kDynamicCycleThreshold))
            {
                return true;
            }
            return false;
        }

        private bool IsExecutedTooManyTimes(ProtoCore.AssociativeGraph.GraphNode node, int limit)
        {
            Validity.Assert(null != node);
            if (node.counter > limit)
            {

                return true;
            }
            else
                return false;

        }

        private ProtoCore.AssociativeGraph.GraphNode[] FindCycleStartNodeAndEndNode(List<ProtoCore.AssociativeGraph.GraphNode> nodesExecutedSameTime)
        {

            ProtoCore.AssociativeGraph.GraphNode cyclicSymbolStart = null;
            ProtoCore.AssociativeGraph.GraphNode cyclicSymbolEnd = null;

            cyclicSymbolStart = nodesExecutedSameTime[0];
            cyclicSymbolEnd = nodesExecutedSameTime.Last();

            ProtoCore.AssociativeGraph.GraphNode[] StartAndEnd = new ProtoCore.AssociativeGraph.GraphNode[] { cyclicSymbolStart, cyclicSymbolEnd };
            //reset counter
            foreach (ProtoCore.AssociativeGraph.GraphNode node in nodesExecutedSameTime)
            {
                node.counter = 0;
            }

            return StartAndEnd;

        }
        private bool IsNodeModified(StackValue svGraphNode, StackValue svUpdateNode)
        {
            bool isPointerModified = AddressType.Pointer == svGraphNode.optype || AddressType.Pointer == svUpdateNode.optype;
            bool isArrayModified = AddressType.ArrayPointer == svGraphNode.optype || AddressType.ArrayPointer == svUpdateNode.optype;
            bool isDataModified = svGraphNode.opdata != svUpdateNode.opdata;
            bool isDoubleDataModified = svGraphNode.optype == AddressType.Double && svGraphNode.opdata_d != svUpdateNode.opdata_d;
            bool isTypeModified = svGraphNode.optype != AddressType.Invalid && svUpdateNode.optype != AddressType.Invalid && svGraphNode.optype != svUpdateNode.optype;

            // Jun Comment: an invalid optype means that the value was not set
            bool isInvalid = AddressType.Invalid == svGraphNode.optype || AddressType.Invalid == svUpdateNode.optype;

            return isInvalid || isPointerModified || isArrayModified || isDataModified || isDoubleDataModified || isTypeModified;
        }


        private void UpdateModifierBlockDependencyGraph(ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            int modBlkUID = graphNode.modBlkUID;
            int index = graphNode.UID;
            bool setModifierNode = true;
            if (graphNode.isCyclic)
            {
                // If the graphnode is cyclic, mark it as not first so it wont get executed 
                // Sets its cyclePoint graphnode to be not dirty so it also doesnt execute.
                // The cyclepoint is the other graphNode that the current node cycles with
                graphNode.isDirty = false;
                if (null != graphNode.cyclePoint)
                {
                    graphNode.cyclePoint.isDirty = false;
                    graphNode.cyclePoint.isCyclic = true;
                }
                setModifierNode = false;
            }

            if (modBlkUID != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                for (int i = index; i < istream.dependencyGraph.GraphList.Count; ++i)
                {
                    ProtoCore.AssociativeGraph.GraphNode node = istream.dependencyGraph.GraphList[i];
                    if (node.modBlkUID == modBlkUID)
                    {
                        node.isDirty = setModifierNode;
                    }
                }
            }
            else
            {
                graphNode.isDirty = true;
            }
        }




        private void SetGraphNodeStackValue(AssociativeGraph.GraphNode graphNode, StackValue sv)
        {
            Validity.Assert(!graphNode.isReturn);
            // TODO Jun: Expand me to handle complex ident lists
            ProtoCore.DSASM.SymbolNode symbol = graphNode.updateNodeRefList[0].nodeList[0].symbol;
            Validity.Assert(null != symbol);
            rmem.SetStackData(symbol.runtimeTableIndex, symbol.symbolTableIndex, symbol.classScope, sv);
        }

        private void SetGraphNodeStackValueNull(AssociativeGraph.GraphNode graphNode)
        {
            StackValue svNull = StackValue.Null;
            SetGraphNodeStackValue(graphNode, svNull);
        }

        private ProtoCore.AssociativeGraph.GraphNode GetFirstSSAGraphnode(int index, int exprID)
        {
            //while (istream.dependencyGraph.GraphList[index].exprUID == exprID)
            while (istream.dependencyGraph.GraphList[index].IsSSANode())
            {
                --index;
                if (index < 0)
                {
                    // In this case, the first SSA statemnt is the first graphnode
                    break;
                }

                //// This check will be deprecated on full SSA
                //if (core.Options.FullSSA)
                //{
                //    if (!istream.dependencyGraph.GraphList[index].IsSSANode())
                //    {
                //        // The next graphnode is nolonger part of the current statement 
                //        break;
                //    }
                //}

                Validity.Assert(index >= 0);
            }
            return istream.dependencyGraph.GraphList[index + 1];
        }

        private bool UpdatePropertyChangedGraphNode()
        {
            bool propertyChanged = false;
            var graphNodes = this.istream.dependencyGraph.GraphList;
            foreach (var node in graphNodes)
            {
                if (node.propertyChanged)
                {
                    propertyChanged = true;
                    int exprUID = node.exprUID;
                    int modBlkId = node.modBlkUID;
                    bool isSSAAssign = node.IsSSANode();
                    if (core.Options.ExecuteSSA)
                    {
                        UpdateDependencyGraph(exprUID, modBlkId, isSSAAssign, node.lastGraphNode, true);
                    }
                    else
                    {
                        UpdateDependencyGraph(exprUID, modBlkId, isSSAAssign, node, true);
                    }
                    node.propertyChanged = false;
                }
            }
            return propertyChanged;
        }

        private void UpdateGraph(int exprUID, int modBlkId, bool isSSAAssign)
        {
            if (null != Properties.executingGraphNode)
            {
                if (!Properties.executingGraphNode.IsSSANode())
                {
                    UpdatePropertyChangedGraphNode();
                }
            }
            UpdateDependencyGraph(exprUID, modBlkId, isSSAAssign, Properties.executingGraphNode);

            if (Properties.executingGraphNode != null)
            {
                // Remove this condition when full SSA is enabled
                bool isssa = (!Properties.executingGraphNode.IsSSANode() && Properties.executingGraphNode.DependsOnTempSSA());

                if (core.Options.ExecuteSSA)
                {
                    isssa = Properties.executingGraphNode.IsSSANode();
                }
                if (!isssa)
                {
                    for (int n = 0; n < istream.dependencyGraph.GraphList.Count; ++n)
                    {
                        ProtoCore.AssociativeGraph.GraphNode graphNode = istream.dependencyGraph.GraphList[n];

                        bool allowRedefine = true;

                        SymbolNode symbol = Properties.executingGraphNode.updateNodeRefList[0].nodeList[0].symbol;
                        bool isMember = symbol.classScope != ProtoCore.DSASM.Constants.kInvalidIndex
                            && symbol.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex;

                        if (isMember)
                        {
                            // For member vars, do not allow if not in the same scope
                            if (symbol.classScope != graphNode.classIndex || symbol.functionIndex != graphNode.procIndex)
                            {
                                allowRedefine = false;
                            }
                        }

                        if (allowRedefine)
                        {
                            // Update redefinition that this graphnode may have cauased
                            UpdateGraphNodeDependency(graphNode, Properties.executingGraphNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// To implement element based update: when an element in an array is
        /// updated, only updates the corresponding element in its updatee.
        /// 
        /// For example:
        /// 
        ///     a = b;    // b = {1, 2, 3};
        ///     b[0] = 0; // should only update a[0].
        ///     
        /// The basic idea is checking the dimension node in the executing 
        /// graph node (i.e., [0] in the executing graph node b[0] = 0), and
        /// apply thsi dimension to the dirty graph node (i.e., a = b), so when
        /// executing that dirty graph node, [0] will be applied to all POP and
        /// PUSH instructions. So for statement a = b; essentially the VM 
        /// will do
        /// 
        ///      push b[0];
        ///      pop to a[0];
        ///  
        /// Now this function only considers about the simpliest case, i.e., 
        /// only variable is on the RHS of expression because a function may
        /// involve array promotion, type conversion, replication guide and so
        /// on.   -- Yu Ke
        /// </summary>
        /// <param name="graphNode">The graph node that is to be update</param>
        /// <param name="matchingNode">Matching node</param>
        /// <param name="executingGraphNode">The executing graph node</param>
        private void UpdateDimensionsForGraphNode(
            AssociativeGraph.GraphNode graphNode,
            AssociativeGraph.GraphNode matchingNode,
            AssociativeGraph.GraphNode executingGraphNode)
        {
            Validity.Assert(graphNode != null && executingGraphNode != null);
            graphNode.updateDimensions.Clear();

            var updateDimNodes = executingGraphNode.dimensionNodeList;
            if (updateDimNodes == null)
            {
                return;
            }

            // We may take replication control into account in the future. 
            // But right now let's just skip it.
            var rcInstructions = executingGraphNode.replicationControl.Instructions;

            // Update node list can be a, b, c for the case like:
            //     a.b.c[0] = ...
            // 
            // Let's only support the simplest case now:
            //
            //    a[0] = ...
            Validity.Assert(matchingNode.updateNodeRefList != null
                && matchingNode.updateNodeRefList.Count > 0);
            var depNodes = matchingNode.updateNodeRefList[0].nodeList;
            if (depNodes == null || depNodes.Count != 1)
            {
                return;
            }


            if (graphNode.firstProc != null && graphNode.firstProc.argTypeList.Count != 0)
            {
                // Skip the case that function on RHS takes over 1 parameters --
                // there is potential replication guide which hasn't been supported
                // yet right now. 
                // 
                //     x = foo(a, b);
                //     a[0] = ...
                //
                if (graphNode.firstProc.argTypeList.Count > 1)
                {
                    return;
                }

                // Not support function parameter whose rank >= 1
                // 
                // def foo(a:int[])
                // {
                //    ...
                // }
                // a = {1, 2, 3};
                // b = a;
                // a[0] = 0;   // b[0] = foo(a[0]) doesn't work!
                //  
                if (graphNode.firstProc.argTypeList[0].rank >= 1)
                {
                    return;
                }
            }

            var depDimNodes = depNodes.Last().dimensionNodeList;
            int dimIndex = 0;

            if (depDimNodes != null)
            {
                // Try to match all dependent dimensions. For example:
                //  
                //     ... = a[0][i];
                //     a[0][j] = ...;  
                //
                // Here [i], [j] doesn't match, even they may have same value.
                // Or, 
                //  
                //     ... = a[0][1][2];
                //     a[0][1] = ...;  
                //
                // where [2] hasn't been matched yet. 
                //
                // For these cases, right now just do full update. 
                if (depDimNodes.Count > updateDimNodes.Count)
                {
                    return;
                }

                // For the case:
                //
                //     x = a[0];
                //     a[0] = 1;   
                //
                // We don't want to apply array indexing [0] to a[0] again. But we 
                // do want to apply array indexing [1] for the following case:
                //
                //    x = a[0];
                //    a[0][1] = 1;  --> x[1] = a[0][1]
                //
                // So basically we should eliminate the common part.
                for (; dimIndex < depDimNodes.Count; ++dimIndex)
                {
                    var dimSymbol1 = depDimNodes[dimIndex].symbol;
                    var dimSymbol2 = updateDimNodes[dimIndex].symbol;
                    if (!dimSymbol1.IsEqualAtScope(dimSymbol2))
                    {
                        return;
                    }
                }
            }

            for (; dimIndex < updateDimNodes.Count; ++dimIndex)
            {
                var dimNode = updateDimNodes[dimIndex];
                var dimSymbol = dimNode.symbol;

                switch (dimNode.nodeType)
                {
                    case AssociativeGraph.UpdateNodeType.kSymbol:
                        {
                            var opSymbol = StackValue.Null;
                            if (dimSymbol.classScope != Constants.kInvalidIndex &&
                                dimSymbol.functionIndex == Constants.kInvalidIndex)
                            {
                                opSymbol = StackValue.BuildMemVarIndex(dimSymbol.symbolTableIndex);
                            }
                            else
                            {
                                opSymbol = StackValue.BuildVarIndex(dimSymbol.symbolTableIndex);
                            }

                            var dimValue = GetOperandData(dimSymbol.codeBlockId,
                                                          opSymbol,
                                                          StackValue.BuildInt(dimSymbol.classScope));
                            graphNode.updateDimensions.Add(dimValue);
                            break;
                        }

                    case AssociativeGraph.UpdateNodeType.kLiteral:
                        {
                            int dimValue = Constants.kInvalidIndex;
                            if (Int32.TryParse(dimSymbol.name, out dimValue))
                            {
                                graphNode.updateDimensions.Add(StackValue.BuildInt(dimValue));
                            }
                            else
                            {
                                // No idea for this dimension, just terminate. 
                                return;
                            }
                            break;
                        }
                    default:
                        // No idea to how to handle method and other node types,
                        // just stop here at least we can get partial element
                        // based array update. 
                        return;
                }
            }
        }

        /// <summary>
        /// Check if an update is triggered;
        /// Find all graph nodes whos dependents contain this symbol;
        /// Mark those nodes as dirty
        /// </summary>
        public int UpdateDependencyGraph(
            int exprUID,
            int modBlkId,
            bool isSSAAssign,
            AssociativeGraph.GraphNode executingGraphNode,
            bool propertyChanged = false)
        {
            int nodesMarkedDirty = 0;
            if (executingGraphNode == null)
            {
                return nodesMarkedDirty;
            }

            int classIndex = executingGraphNode.classIndex;
            int procIndex = executingGraphNode.procIndex;

            var graph = istream.dependencyGraph;
            var graphNodes = graph.GetGraphNodesAtScope(classIndex, procIndex);
            if (graphNodes == null)
            {
                return nodesMarkedDirty;
            }

            //foreach (var graphNode in graphNodes)
            for (int i = 0; i < graphNodes.Count; ++i)
            {
                var graphNode = graphNodes[i];

                // If the graphnode is inactive then it is no longer executed
                if (!graphNode.isActive)
                {
                    continue;
                }
                //
                // Comment Jun: 
                //      This is clarifying the intention that if the graphnode is within the same SSA expression, we still allow update
                //
                bool allowUpdateWithinSSA = false;
                if (core.Options.ExecuteSSA)
                {
                    allowUpdateWithinSSA = true;
                    isSSAAssign = false; // Remove references to this when ssa flag is removed

                    // Do not update if its a property change and the current graphnode is the same expression
                    if (propertyChanged && graphNode.exprUID == Properties.executingGraphNode.exprUID)
                    {
                        continue;
                    }
                }
                else
                {
                    // TODO Jun: Remove this code immediatley after enabling SSA
                    bool withinSSAStatement = graphNode.UID == executingGraphNode.UID;
                    allowUpdateWithinSSA = !withinSSAStatement;
                }

                if (!allowUpdateWithinSSA || (propertyChanged && graphNode == Properties.executingGraphNode))
                {
                    continue;
                }

                foreach (var noderef in executingGraphNode.updateNodeRefList)
                {
                    ProtoCore.AssociativeGraph.GraphNode matchingNode = null;
                    if (!graphNode.DependsOn(noderef, ref matchingNode))
                    {
                        continue;
                    }

                    // Jun: only allow update to other expr id's (other statements) if this is the final SSA assignment
                    if (core.Options.ExecuteSSA && !propertyChanged)
                    {
                        if (null != Properties.executingGraphNode && Properties.executingGraphNode.IsSSANode())
                        {
                            // This is still an SSA statement, if a node of another statement depends on it, ignore it
                            if (graphNode.exprUID != Properties.executingGraphNode.exprUID)
                            {
                                // Defer this update until the final non-ssa node
                                deferedGraphNodes.Add(graphNode);
                                continue;
                            }
                        }
                    }

                    // @keyu: if we are modifying an object's property, e.g.,
                    // 
                    //    foo.id = 42;
                    //
                    // both dependent list and update list of the corresponding 
                    // graph node contains "foo" and "id", so if property "id"
                    // is changed, this graph node will be re-executed and the
                    // value of "id" is incorrectly set back to old value.
                    if (propertyChanged)
                    {
                        var depUpdateNodeRef = graphNode.dependentList[0].updateNodeRefList[0];
                        if (graphNode.updateNodeRefList.Count == 1)
                        {
                            var updateNodeRef = graphNode.updateNodeRefList[0];
                            if (depUpdateNodeRef.IsEqual(updateNodeRef))
                            {
                                continue;
                            }
                        }
                    }

                    //
                    // Comment Jun: We dont want to cycle between such statements:
                    //
                    // a1.a = 1;
                    // a1.a = 10;
                    //

                    Validity.Assert(null != matchingNode);
                    bool isLHSModification = matchingNode.isLHSNode;
                    bool isUpdateable = matchingNode.IsUpdateableBy(noderef);

                    // isSSAAssign means this is the graphnode of the final SSA assignment
                    // Overrride this if allowing within SSA update
                    // TODO Jun: Remove this code when SSA is completely enabled
                    bool allowSSADownstream = false;
                    if (core.Options.ExecuteSSA)
                    {
                        //allowSSADownstream = graphNode.UID > executingGraphNode.UID;

                        // Is within the same ssa range
                        if (exprUID == graphNode.exprUID)
                        {
                            // Make sure these are valid subscripts - Assert perhaps?
                            if (graphNode.SSASubscript != ProtoCore.DSASM.Constants.kInvalidIndex && executingGraphNode.SSASubscript != ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                allowSSADownstream = graphNode.SSASubscript > executingGraphNode.SSASubscript;
                            }
                        }
                    }


                    // Comment Jun: 
                    //      If the triggered dependent graphnode is LHS 
                    //          and... 
                    //      the triggering node (executing graphnode)
                    if (isLHSModification && !isUpdateable)
                    {
                        break;
                    }

                    // TODO Jun: Optimization - Reimplement update delta evaluation using registers
                    //if (IsNodeModified(EX, FX))
                    bool isLastSSAAssignment = (exprUID == graphNode.exprUID) && graphNode.IsLastNodeInSSA && !graphNode.isReturn;
                    if (exprUID != graphNode.exprUID && modBlkId != graphNode.modBlkUID)
                    {
                        UpdateModifierBlockDependencyGraph(graphNode);
                    }
                    else if (allowSSADownstream
                              || isSSAAssign
                                || isLastSSAAssignment
                              || (exprUID != graphNode.exprUID
                                 && modBlkId == Constants.kInvalidIndex
                                 && graphNode.modBlkUID == Constants.kInvalidIndex)
                        )
                    {
                        if (graphNode.isCyclic)
                        {
                            // If the graphnode is cyclic, mark it as not dirst so it wont get executed 
                            // Sets its cyclePoint graphnode to be not dirty so it also doesnt execute.
                            // The cyclepoint is the other graphNode that the current node cycles with
                            graphNode.isDirty = false;
                            if (null != graphNode.cyclePoint)
                            {
                                graphNode.cyclePoint.isDirty = false;
                                graphNode.cyclePoint.isCyclic = true;
                            }
                        }
                        else if (!graphNode.isDirty)
                        {
                            // If the graphnode is not cyclic, then it can be safely marked as dirty, in preparation of its execution
                            if (core.Options.EnableVariableAccumulator
                                && !isSSAAssign
                                && graphNode.IsSSANode())
                            {
                                //
                                // Comment Jun: Backtrack and firt the first graphnode of this SSA transform and mark it dirty. 
                                //              We want to execute the entire statement, not just the partial SSA nodes
                                //

                                // TODO Jun: Optimization - Statically determine the index of the starting graphnode of this SSA expression

                                // Looks we should partially execuate graph
                                // nodes otherwise we will get accumulative
                                // update. - Yu Ke 

                                /*
                                int graphNodeIndex = 0;
                                for (; graphNodeIndex < graph.GraphList.Count; graphNodeIndex++)
                                {
                                    if (graph.GraphList[graphNodeIndex].UID == graphNode.UID)
                                        break;
                                }
                                var firstGraphNode = GetFirstSSAGraphnode(graphNodeIndex - 1, graphNode.exprUID);
                                firstGraphNode.isDirty = true;
                                */
                            }

                            if (core.Options.ElementBasedArrayUpdate)
                            {
                                UpdateDimensionsForGraphNode(graphNode, matchingNode, executingGraphNode);
                            }
                            graphNode.isDirty = true;
                            graphNode.forPropertyChanged = propertyChanged;
                            nodesMarkedDirty++;
                            
                            // On debug mode:
                            //      we want to mark all ssa statements dirty for an if the lhs pointer is a new instance.
                            //      In this case, the entire line must be re-executed
                            //      
                            //  Given:
                            //      x = 1
                            //      p = p.f(x) 
                            //      x = 2
                            //
                            //  To SSA:
                            //
                            //      x = 1
                            //      t0 = p -> we want to execute from here of member function 'f' returned a new instance of 'p'
                            //      t1 = x
                            //      t2 = t0.f(t1)
                            //      p = t2
                            //      x = 2
                            if (null != executingGraphNode.lastGraphNode && executingGraphNode.lastGraphNode.reExecuteExpression)
                            {
                                executingGraphNode.lastGraphNode.reExecuteExpression = false;
                                //if (core.Options.GCTempVarsOnDebug && core.Options.IDEDebugMode)
                                {
                                    var firstGraphNode = GetFirstSSAGraphnode(i - 1, graphNode.exprUID);
                                    firstGraphNode.isDirty = true;
                                }
                            }
                        }
                    }
                }
            }
            return nodesMarkedDirty;
        }

        //
        // Comment Jun: Revised 
        //
        //  proc UpdateGraphNodeDependency(execnode)
        //      foreach node in graphnodelist 
        //          if execnode.lhs is equal to node.lhs
        //              if execnode.HasDependents() 
        //                  if execnode.Dependents() is not equal to node.Dependents()
        //                      node.RemoveDependents()
        //                  end
        //              end
        //          end
        //      end
        //  end
        //
        private void UpdateGraphNodeDependency(AssociativeGraph.GraphNode gnode, AssociativeGraph.GraphNode executingNode)
        {
            if (gnode.UID >= executingNode.UID // for previous graphnodes
                || gnode.updateNodeRefList.Count == 0
                || gnode.updateNodeRefList.Count != executingNode.updateNodeRefList.Count
                || gnode.isAutoGenerated)
            {
                return;
            }

            for (int n = 0; n < executingNode.updateNodeRefList.Count; ++n)
            {
                if (!gnode.updateNodeRefList[n].IsEqual(executingNode.updateNodeRefList[n]))
                {
                    return;
                }

                if (gnode.guid == executingNode.guid && gnode.ssaExprID == executingNode.ssaExprID)
                //if (gnode.exprUID == executingNode.exprUID)
                {
                    // These nodes are within the same expression, no redifinition can occur
                    return;
                }
            }

            //if (executingNode.dependentList.Count > 0)
            {
                // if execnode.Dependents() is not equal to node.Dependents()
                // TODO Jun: Extend this check
                bool areDependentsEqual = false;
                if (!areDependentsEqual)
                {
                    gnode.dependentList.Clear();

                    // GC all the temporaries associated with the redefined variable
                    // Given:
                    //      a = A.A()
                    //      a = 10
                    //
                    // Transforms to:
                    //        
                    //      t0 = A.A()
                    //      a = t0
                    //      a = 10      // Redefinition of 'a' will GC 't0'
                    //
                    // Another example
                    // Given:
                    //      a = {A.A()}
                    //      a = 10
                    //
                    // Transforms to:
                    //        
                    //      t0 = A.A()
                    //      t1 = {t0}
                    //      a = t1
                    //      a = 10      // Redefinition of 'a' will GC t0 and t1
                    //
                    GCAnonymousSymbols(gnode.symbolListWithinExpression);
                    gnode.symbolListWithinExpression.Clear();
                }
            }
        }

        private void UpdateLanguageBlockDependencyGraph(int entry)
        {
            int setentry = entry;
            bool isFirstGraphSet = false;
            ProtoCore.AssociativeGraph.GraphNode entryNode = null;
            foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in istream.dependencyGraph.GraphList)
            {
                graphNode.isDirty = true;
                if (!isFirstGraphSet)
                {
                    // Setting the first graph of this function to be in executed (not dirty) state
                    isFirstGraphSet = true;
                    graphNode.isDirty = false;
                    entryNode = graphNode;
                }

                if (DSASM.Constants.kInvalidIndex == setentry)
                {
                    // Set the entry point as this graph and mark this graph as executed 
                    setentry = graphNode.updateBlock.startpc;
                    graphNode.isDirty = false;
                    entryNode = graphNode;
                }
            }

            if (core.Options.ExecuteSSA)
            {
                ProtoCore.AssociativeEngine.Utils.SetFinalGraphNodeRuntimeDependents(entryNode);
            }

            pc = setentry;
        }

        private void UpdateMethodDependencyGraph(int entry, int procIndex, int classIndex)
        {
            int setentry = entry;
            bool isFirstGraphSet = false;
            ProtoCore.AssociativeGraph.GraphNode entryNode = null;

            List<AssociativeGraph.GraphNode> graphNodes = istream.dependencyGraph.GetGraphNodesAtScope(classIndex, procIndex);
            if (graphNodes != null)
            {
                foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in graphNodes)
                {
                    graphNode.isDirty = true;
                    if (!isFirstGraphSet)
                    {
                        // Setting the first graph of this function to be in executed (not dirty) state
                        isFirstGraphSet = true;
                        graphNode.isDirty = false;
                        entryNode = graphNode;
                    }

                    if (DSASM.Constants.kInvalidIndex == setentry)
                    {
                        // Set the entry point as this graph and mark this graph as executed 
                        setentry = graphNode.updateBlock.startpc;
                        graphNode.isDirty = false;
                        entryNode = graphNode;
                    }
                }
            }

            Properties.executingGraphNode = entryNode;

            if (core.Options.ExecuteSSA)
            {
                ProtoCore.AssociativeEngine.Utils.SetFinalGraphNodeRuntimeDependents(entryNode);
            }
            pc = setentry;
        }

        public void XLangSetupNextExecutableGraph(int function, int classscope)
        {
            bool isUpdated = false;

            foreach (InstructionStream instrStream in exe.instrStreamList)
            {
                if (Language.kAssociative == instrStream.language && instrStream.dependencyGraph.GraphList.Count > 0)
                {
                    foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in instrStream.dependencyGraph.GraphList)
                    {
                        if (graphNode.isDirty)
                        {
                            //if (null != graphNode.symbol && graphNode.symbol.functionIndex == function && graphNode.symbol.classScope == classscope)
                            bool isUpdateable = graphNode.updateNodeRefList[0].nodeList.Count > 0;
                            if (isUpdateable && graphNode.procIndex == function && graphNode.classIndex == classscope)
                            {
                                graphNode.isDirty = false;

                                // In function calls, the first graphnode in the function is executed first and was not marked 
                                // If this is the case, just move on to the next graphnode
                                if (pc == graphNode.updateBlock.endpc)
                                {
                                    continue;
                                }

                                pc = graphNode.updateBlock.startpc;
                                isUpdated = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (!isUpdated)
            {
                // There were no updates, this is the end of this associative block
                pc = ProtoCore.DSASM.Constants.kInvalidPC;
            }
        }

        private void XLangUpdateDependencyGraph(int currentLangBlock)
        {
            int classScope = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            int functionScope = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;

            List<ProtoCore.AssociativeGraph.UpdateNodeRef> upadatedList = new List<AssociativeGraph.UpdateNodeRef>();

            // For every instruction list in the executable
            foreach (InstructionStream xInstrStream in exe.instrStreamList)
            {
                // If the instruction list is valid, is associative and has more than 1 graph node
                if (null != xInstrStream && Language.kAssociative == xInstrStream.language && xInstrStream.dependencyGraph.GraphList.Count > 0)
                {
                    // For every graphnode in the dependency list
                    foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in xInstrStream.dependencyGraph.GraphList)
                    {
                        if (graphNode.classIndex != classScope || graphNode.procIndex != functionScope)
                        {
                            continue;
                        }

                        // To deal with the case
                        // 
                        // a;
                        // b;
                        // c;
                        // [Imperative]
                        // {
                        //     [Associative]
                        //     {
                        //         c = 1;
                        //         a = c;
                        //     }
                        // }
                        // 
                        // Here the dependency on 'c' in the inner associative
                        // language block is propagated to the outer imperative
                        // language block (see EmitLanguageBlockNode() in 
                        // imperative codegen). When the executive has executed 
                        // the inner associative language block, it will add 'c' 
                        // to // cross language dependency graph list (because 
                        // 'c' ismodified so that the outside graph node will be 
                        // UPDATED), then the outer imperative language block
                        // will be marked as dirty again and infinite loop 
                        // happens. 
                        if (graphNode.isLanguageBlock && currentLangBlock != Constants.kInvalidIndex)
                        {
                            runtimeVerify(graphNode.languageBlockId != Constants.kInvalidIndex);
                            if (graphNode.languageBlockId == currentLangBlock
                                || core.CompleteCodeBlockList[currentLangBlock].IsMyAncestorBlock(graphNode.languageBlockId))
                            {
                                continue;
                            }
                        }

                        // For every updated in the updatelist
                        foreach (ProtoCore.AssociativeGraph.UpdateNodeRef modifiedRef in istream.xUpdateList)
                        {
                            // We allow dependency check if the modified graphnode list belong to some other block
                            if (modifiedRef.block != currentLangBlock)
                            {
                                if (core.Options.AssociativeToImperativePropagation)
                                {
                                    // Comment Jun: The return check is here to handle:
                                    //  return = [Imperative]{return = 0}
                                    if (!graphNode.isReturn && graphNode.isLanguageBlock)
                                    {
                                        Validity.Assert(null != graphNode.updateNodeRefList && graphNode.updateNodeRefList.Count > 0);
                                        Validity.Assert(null != graphNode.updateNodeRefList[0].nodeList && graphNode.updateNodeRefList[0].nodeList.Count > 0);

                                        /*
                                        int updateNodeCodeBlockId = graphNode.updateNodeRefList[0].nodeList[0].symbol.codeBlockId;
                                        int modifiedNodeCodeBlockId = modifiedRef.nodeList[0].symbol.codeBlockId; 
                                        CodeBlock updateNodeCodeBlock = core.CompleteCodeBlockList[updateNodeCodeBlockId];
                                        
                                        if (updateNodeCodeBlockId == modifiedNodeCodeBlockId || updateNodeCodeBlock.IsMyAncestorBlock(modifiedNodeCodeBlockId))
                                        {
                                            continue;
                                        }
                                        */
                                    }
                                }

                                // Check if the graphnode in the associative language depends on the current updated node
                                ProtoCore.AssociativeGraph.GraphNode matchingNode = null;
                                if (!graphNode.isReturn && graphNode.DependsOn(modifiedRef, ref matchingNode))
                                {
                                    Validity.Assert(null != matchingNode);
                                    bool isLHSModification = matchingNode.isLHSNode;
                                    bool isUpdateable = matchingNode.IsUpdateableBy(modifiedRef);

                                    // Comment Jun: 
                                    //      If the triggered dependent graphnode is LHS 
                                    //          and... 
                                    //      the triggering node (executing graphnode)
                                    if (isLHSModification && !isUpdateable)
                                    {
                                        continue;
                                    }

                                    AddressType opAddr = AddressType.VarIndex;

                                    ProtoCore.DSASM.SymbolNode firstSymbolInUpdatedRef = graphNode.updateNodeRefList[0].nodeList[0].symbol;
                                    if (ProtoCore.DSASM.Constants.kInvalidIndex != firstSymbolInUpdatedRef.classScope)
                                    {
                                        if (exe.classTable.ClassNodes[firstSymbolInUpdatedRef.classScope].IsMemberVariable(firstSymbolInUpdatedRef))
                                        {
                                            opAddr = AddressType.MemVarIndex;
                                        }
                                    }

                                    // For watching nodes, may not be valid
                                    // TODO: to check whether this is required - Randy, Jun
                                    if (Constants.kInvalidIndex == firstSymbolInUpdatedRef.classScope)
                                    {
                                        if (core.DSExecutable.runtimeSymbols[firstSymbolInUpdatedRef.runtimeTableIndex].symbolList.Count <= firstSymbolInUpdatedRef.symbolTableIndex)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (firstSymbolInUpdatedRef.classScope >= 0 &&
                                        rmem.ClassTable.ClassNodes[firstSymbolInUpdatedRef.classScope].symbols.symbolList.Count <= firstSymbolInUpdatedRef.symbolTableIndex)
                                    {
                                        continue;
                                    }

                                    StackValue svSym = (opAddr == AddressType.MemVarIndex) 
                                           ? StackValue.BuildMemVarIndex(firstSymbolInUpdatedRef.symbolTableIndex)
                                           : StackValue.BuildVarIndex(firstSymbolInUpdatedRef.symbolTableIndex);
                                    StackValue svClass = StackValue.BuildClassIndex(firstSymbolInUpdatedRef.classScope);

                                    runtimeVerify(DSASM.Constants.kInvalidIndex != firstSymbolInUpdatedRef.runtimeTableIndex);
                                    StackValue svGraphNode = GetOperandData(firstSymbolInUpdatedRef.runtimeTableIndex, svSym, svClass);
                                    StackValue svPropagateNode = modifiedRef.symbolData;

                                    if (IsNodeModified(svGraphNode, svPropagateNode))
                                    {
                                        graphNode.isDirty = true;
                                        upadatedList.Add(modifiedRef);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (upadatedList.Count > 0)
            {
                foreach (ProtoCore.AssociativeGraph.UpdateNodeRef noderef in upadatedList)
                {
                    istream.xUpdateList.Remove(noderef);
                }
            }
        }

        private void ResumeRegistersFromStack()
        {
            List<StackValue> runtimeStack = rmem.Stack;
            int fp = rmem.FramePointer;

            if (runtimeStack != null && fp >= StackFrame.kStackFrameSize)
            {
                AX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterAX)];
                BX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterBX)];
                CX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterCX)];
                DX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterDX)];
                EX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterEX)];
                FX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterFX)];
                LX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterLX)];
                RX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterRX)];
                SX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterSX)];
                TX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterTX)];
            }
        }

        private void ResumeRegistersFromStackExceptRX()
        {
            List<StackValue> runtimeStack = rmem.Stack;
            int fp = rmem.FramePointer;

            if (runtimeStack != null && fp >= StackFrame.kStackFrameSize)
            {
                AX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterAX)];
                BX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterBX)];
                CX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterCX)];
                DX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterDX)];
                EX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterEX)];
                FX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterFX)];
                LX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterLX)];

                SX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterSX)];
                TX = runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterTX)];
            }
        }

        private void SaveRegistersToStack()
        {
            List<StackValue> runtimeStack = rmem.Stack;
            int fp = rmem.FramePointer;

            if (runtimeStack != null && fp >= StackFrame.kStackFrameSize)
            {
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterAX)] = AX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterBX)] = BX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterCX)] = CX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterDX)] = DX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterEX)] = EX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterFX)] = FX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterLX)] = LX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterRX)] = RX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterSX)] = SX;
                runtimeStack[rmem.GetRelative(StackFrame.kFrameIndexRegisterTX)] = TX;
            }
        }

        public void SaveRegisters(List<StackValue> registers)
        {
            if (registers != null)
            {
                if (registers.Count > 0)
                    registers.Clear();

                registers.Add(AX);
                registers.Add(BX);
                registers.Add(CX);
                registers.Add(DX);
                registers.Add(EX);
                registers.Add(FX);
                registers.Add(LX);
                registers.Add(RX);
                registers.Add(SX);
                registers.Add(TX);
            }
        }

        /// <summary>
        /// Performs type coercion of returned value and GC of arguments, this ptr and Dot methods
        /// </summary>
        /// <param name="finalFep"></param>
        /// <param name="debugFrame"></param>
        /// <param name="Arguments"></param>
        /// <param name="DotCallDimensions"></param>
        private void DebugPerformCoercionAndGC(DebugFrame debugFrame)
        {
            ProcedureNode procNode = debugFrame.FinalFepChosen != null ? debugFrame.FinalFepChosen.procedureNode : null;

            PerformCoercionAndGC(procNode, debugFrame.IsBaseCall, debugFrame.ThisPtr, debugFrame.Arguments, debugFrame.DotCallDimensions);
        }

        private void PerformCoercionAndGC(ProcedureNode procNode, bool isBaseCall, StackValue? thisPtr, List<StackValue> Arguments, List<StackValue> DotCallDimensions)
        {
            // finalFep is forced to be null for base class constructor calls
            // and for such calls 'PerformReturnTypeCoerce' is not called 
            if (!isBaseCall)
            {
                RX = ProtoCore.CallSite.PerformReturnTypeCoerce(procNode, core, RX);

                for (int i = 0; i < Arguments.Count; ++i)
                {
                    GCUtils.GCRelease(Arguments[i], core);
                }

                if (thisPtr != null)
                {
                    GCRelease((StackValue)thisPtr);
                }
                else
                {
                    //if (debugFrame.IsDotArgCall || debugFrame.IsDotCall)
                    {
                        StackValue sv = RX;
                        GCDotMethods(procNode.name, ref sv, DotCallDimensions, Arguments);
                        RX = sv;
                    }
                    DecRefCounter(RX);
                    isGlobScope = true;
                }

            }
        }

        /// <summary>
        /// Pops Debug stackframe, performs coercion and GC and pops stackframe if there's a break inside the function
        /// </summary>
        /// <param name="exeblock"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        bool RestoreDebugPropsOnReturnFromBuiltIns(ref int exeblock, ref List<Instruction> instructions)
        {
            bool waspopped = false;
            // Restore fepRun
            Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);

            DebugFrame debugFrame = core.DebugProps.DebugStackFrame.Peek();
            bool isReplicating = debugFrame.IsReplicating;

#if !__DEBUG_REPLICATE
            if (!isReplicating)
#endif
            {
                bool isResume = debugFrame.IsResume;

                // RestoreCallrForNoBreak and PerformReturnTypeCoerce are NOT called if this is true 
                // so these have to be explicitly called here
                if (isResume)
                {
                    debugFrame = core.DebugProps.DebugStackFrame.Pop();
                    waspopped = true;
                    if (core.DebugProps.DebugStackFrame.Count > 1)
                    {
                        DebugFrame frame = core.DebugProps.DebugStackFrame.Peek();
                        frame.IsResume = true;
                    }

#if __DEBUG_REPLICATE
                    // Return type coercion and function call GC for replicating case takes place separately 
                    // in SerialReplication() when ContinuationStruct.Done == true - pratapa
                    if(!isReplicating)
#endif
                    {
                        DebugPerformCoercionAndGC(debugFrame);
                    }

                    // Restore registers except RX on popping of function stackframe
                    ResumeRegistersFromStackExceptRX();

                    terminate = false;
                }

                // Restore return address and lang block
                /*pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;
                exeblock = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionCallerBlock).opdata;

                istream = exe.instrStreamList[exeblock];
                instructions = istream.instrList;
                executingLanguage = istream.language;

                executingGraphNode = debugFrame.ExecutingGraphNode;*/

                if (core.DebugProps.RunMode.Equals(Runmode.StepOut) && pc == core.DebugProps.StepOutReturnPC)
                {
                    core.Breakpoints.Clear();
                    core.Breakpoints.AddRange(core.DebugProps.AllbreakPoints);
                }
            }
            return waspopped;

        }

        bool RestoreDebugPropsOnReturnFromFunctionCall(ref int exeblock, ref List<Instruction> instructions, out int ci, out int fi, out bool isReplicating,
            out DebugFrame debugFrame)
        {
            //
            // TODO: Aparajit, Jun - Determine an alternative to the waspopped flag
            //
            bool waspopped = false;
            Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);

            debugFrame = core.DebugProps.DebugStackFrame.Peek();

            isReplicating = debugFrame.IsReplicating;

#if !__DEBUG_REPLICATE
            if (!isReplicating)
#endif
            {
                bool isResume = debugFrame.IsResume;

                // Comment Jun: Since we dont step into _Dispose() calls, then its debugframe should not be popped off here.
                bool isDispose = debugFrame.IsDisposeCall;

                // RestoreCallrForNoBreak and PerformReturnTypeCoerce are NOT called if this is true
                // or for base class ctor calls and therefore need to be taken care of here
                if ((isResume || debugFrame.IsBaseCall) && !isDispose)
                {
                    debugFrame = core.DebugProps.DebugStackFrame.Pop();
                    waspopped = true;

                    if (isResume)
                    {
                        if (core.DebugProps.DebugStackFrame.Count > 1)
                        {
                            DebugFrame frame = core.DebugProps.DebugStackFrame.Peek();
                            frame.IsResume = true;
                        }
                    }

#if __DEBUG_REPLICATE
                    // Return type coercion and function call GC for replicating case takes place separately 
                    // in SerialReplication() when ContinuationStruct.Done == true - pratapa
                    if (!isReplicating)
#endif
                    {
                        DebugPerformCoercionAndGC(debugFrame);
                    }

                    // Restore registers except RX on popping of function stackframe
                    ResumeRegistersFromStackExceptRX();

                    terminate = false;
                }

                Properties.executingGraphNode = debugFrame.ExecutingGraphNode;

                if (core.DebugProps.RunMode.Equals(Runmode.StepOut) && pc == core.DebugProps.StepOutReturnPC)
                {
                    core.Breakpoints.Clear();
                    core.Breakpoints.AddRange(core.DebugProps.AllbreakPoints);
                }
            }

            // Restore return address and lang block
            pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;
            exeblock = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionCallerBlock).opdata;

            istream = exe.instrStreamList[exeblock];
            instructions = istream.instrList;
            executingLanguage = istream.language;

            ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;

            int localCount = 0;
            int paramCount = 0;

            int blockId = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionBlock).opdata;

            GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

            // Pop function stackframe as this is not allowed in Ret/Retc in debug mode
            rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
            rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize + localCount + paramCount);


            ResumeRegistersFromStackExceptRX();

            //StackValue svFrameType = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexCallerStackFrameType);
            StackValue svFrameType = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexStackFrameType);
            StackFrameType frametype = (StackFrameType)svFrameType.opdata;
            if (frametype == StackFrameType.kTypeLanguage)
            {
                bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;
            }
            return waspopped;
        }

        void RestoreDebugPropsOnReturnFromLangBlock(ref int exeblock, ref List<Instruction> instructions)
        {
            // On the new stack frame, this dependency has already been executed at retb in RestoreFromBounce
            //XLangUpdateDependencyGraph(exeblock);

            Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);
            {
                // Restore fepRun
                DebugFrame debugFrame = core.DebugProps.DebugStackFrame.Pop();

                bool isResume = debugFrame.IsResume;

                if (isResume)
                {
                    if (core.DebugProps.DebugStackFrame.Count > 1)
                    {
                        DebugFrame frame = core.DebugProps.DebugStackFrame.Peek();
                        frame.IsResume = true;
                    }

                    terminate = false;

                    // Restore registers except RX on popping of language stackframe
                    ResumeRegistersFromStackExceptRX();
                }

                // Restore return address and lang block    
                pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;
                exeblock = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionCallerBlock).opdata;

                istream = exe.instrStreamList[exeblock];
                instructions = istream.instrList;
                executingLanguage = istream.language;

                Properties.executingGraphNode = debugFrame.ExecutingGraphNode;

                // Pop language stackframe as this is not allowed in Retb in debug mode
                rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
                rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize);

                ResumeRegistersFromStackExceptRX();
                bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;
            }

            if (pc < 0)
            {
                throw new ProtoCore.Exceptions.EndOfScript();
            }
        }

        /// <summary>
        /// Restores Debug properties from function call and/or from Dot call
        /// </summary>
        /// <param name="currentPC"></param>
        /// <param name="exeblock"></param>
        /// <param name="ci"></param>
        /// <param name="fi"></param>
        /// <param name="isReplicating"></param>
        /// <returns></returns>
        private bool DebugReturnFromFunctionCall(int currentPC, ref int exeblock, out int ci, out int fi, out bool isReplicating, out DebugFrame debugFrame)
        {
            DebugFrame tempFrame = null;

            tempFrame = core.DebugProps.DebugStackFrame.Peek();

            List<Instruction> instructions = istream.instrList;

            bool waspopped = RestoreDebugPropsOnReturnFromFunctionCall(ref exeblock, ref instructions, out ci, out fi, out isReplicating, out debugFrame);

            // TODO: If return from previous function calls "_Dispose", and we have stepped into it, 
            // we need to restore the caller stackframe - pratapa
            if (tempFrame.IsDisposeCall)
            {
                // TODO: If we have stepped inside _Dispose and are resuming from it - pratapa
                if (!terminate)
                {
                    // 1. Call everything after RETURNSITEGC in OpCode.RETURN/ OpCode.RETC
                    // 2. Call RestoreDebugPropsOnReturnFromFunctionCall() for caller function
                    // 3. Return address from _Dispose is one more than the correct value and therefore needs to be fixed
                }
                // TODO: This works assuming debugging inside _Dispose functions is disabled
                // ie stepping over _Dispose - pratapa
                core.DebugProps.DebugEntryPC = core.DebugProps.ReturnPCFromDispose;
                //break;
            }
            else
            {
#if __DEBUG_REPLICATE
                // When debugging replication, we must pop off the DebugFrame for the Dot call only after replication is complete
                // (after ContinuationStruct.Done == true) - pratapa
                if (!isReplicating)
#endif
                {
                    debugFrame = core.DebugProps.DebugStackFrame.Peek();
                    // If call returns to Dot Call, restore debug props for Dot call
                    if (debugFrame.IsDotCall)
                    {
                        waspopped = RestoreDebugPropsOnReturnFromBuiltIns(ref exeblock, ref instructions);
                    }
                }
                core.DebugProps.DebugEntryPC = currentPC;
            }

            return waspopped;
        }

        private bool DebugReturn(ProcedureNode procNode, int currentPC)
        {
            //
            // TODO: Aparajit, Jun - Determine an alternative to the waspopped flag
            // 
            bool waspopped = false;

            bool isReplicating = false;
            int exeblock = ProtoCore.DSASM.Constants.kInvalidIndex;
            int ci = ProtoCore.DSASM.Constants.kInvalidIndex;
            int fi = ProtoCore.DSASM.Constants.kInvalidIndex;

            DebugFrame debugFrame = null;
            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                waspopped = DebugReturnFromFunctionCall(currentPC, ref exeblock, out ci, out fi, out isReplicating, out debugFrame);

                if (!waspopped)
                {
                    core.DebugProps.RestoreCallrForNoBreak(core, procNode, isReplicating);
                }
            }

            // TODO: This call is common to both Debug as well as Serial mode of execution. Currently this will work only in Debug mode - pratapa
#if __DEBUG_REPLICATE

            if (isReplicating)
            {
                SerialReplication(procNode, ref exeblock, ci, fi, debugFrame);
                waspopped = true;
            }
#endif
            return waspopped;
        }

        private void SetupExecutive(int exeblock, int entry, Language language, List<Instruction> breakpoints)
        {
            core.ExecMode = InterpreterMode.kNormal;

            // exe need to be assigned at the constructor, 
            // for function call with replication, gc is triggered to handle the parameter and return value at FunctionEndPoint
            // gc requirs exe to be not null but at that point, Execute has not been called
            //Validity.Assert(exe == null);
            exe = core.DSExecutable;
            executingBlock = exeblock;

            core.DebugProps.CurrentBlockId = exeblock;

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            core.DebugProps.DebugEntryPC = entry;

            List<Instruction> instructions = istream.instrList;
            Validity.Assert(null != instructions);

            // Restore the previous state
            rmem = core.Rmem;
            bool _isNewFunction = rmem.FramePointer == 0;


            if (core.DebugProps.isResume)   // resume from a breakpoint, 
            {
                Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);

                DebugFrame debugFrame = core.DebugProps.DebugStackFrame.Peek();

                // TODO: The FepRun info need not be cached in DebugProps any longer
                // as it can be replaced by StackFrameType in rmem.Stack - pratapa
                fepRun = debugFrame.FepRun == 1;
                //StackFrameType stackFrameType = (StackFrameType)rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType).opdata;
                //fepRun = (stackFrameType == StackFrameType.kTypeFunction) ? true : false;

                //ResumeRegistersFromStack();

                fepRunStack = core.DebugProps.FRStack;

                Properties = PopInterpreterProps();
                Properties.executingGraphNode = core.DebugProps.executingGraphNode;
                deferedGraphNodes = core.DebugProps.deferedGraphnodes;

            }
            else
            {
                PushInterpreterProps(Properties);
                Properties.Reset();
            }

            if (false == fepRun)
            {
                if (core.DebugProps.isResume) // resume from a breakpoint, 
                {
                    pc = entry;
                }
                else
                {
                    pc = istream.entrypoint;
                    rmem.FramePointer = rmem.Stack.Count;
                }
            }
            else
            {
                pc = entry;
                isGlobScope = false;
            }
            executingLanguage = exe.instrStreamList[exeblock].language;

            if (Language.kAssociative == executingLanguage && !core.DebugProps.isResume)
            {
                if (fepRun)
                {
                    int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                    int fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
                    UpdateMethodDependencyGraph(pc, fi, ci);
                }
                else
                {
                    if (!core.Options.IsDeltaExecution)
                    {
                        UpdateLanguageBlockDependencyGraph(pc);
                    }
                    SetupGraphEntryPoint(pc);
                }
            }

            Validity.Assert(null != rmem);
            rmem.Executable = exe;
            rmem.ClassTable = exe.classTable;
        }


        private bool HandleBreakpoint(List<Instruction> breakpoints, List<Instruction> runningInstructions, int currentPC, int exeblock)
        {
            bool terminateExec = false;
            bool isBreakPoint = false;

            if ((currentPC >= 0) && (currentPC < runningInstructions.Count))
            {
                isBreakPoint = breakpoints.Contains(runningInstructions[currentPC]);
            }

            if (currentPC >= runningInstructions.Count || currentPC < 0 || isBreakPoint)
            {
                if (currentPC < 0)
                {
                    core.ReasonForExecutionSuspend = ReasonForExecutionSuspend.NoEntryPoint;
                    terminateExec = true;
                    //break;
                }
                else if (pc >= runningInstructions.Count)
                {
                    core.ReasonForExecutionSuspend = ReasonForExecutionSuspend.EndOfFile;
                    terminateExec = true;
                    //break;
                }
                else
                {
                    Validity.Assert(breakpoints.Contains(runningInstructions[currentPC]));
                    core.ReasonForExecutionSuspend = ReasonForExecutionSuspend.Breakpoint;
                    logVMMessage("Breakpoint at: " + runningInstructions[currentPC]);

                    Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);
                    {
                        DebugFrame debugFrame = core.DebugProps.DebugStackFrame.Peek();

                        // Since the first frame always belongs to the global language block
                        if (core.DebugProps.DebugStackFrame.Count > 1)
                        {
                            debugFrame.IsResume = true;
                        }
                    }
                    SaveRegistersToStack();

                    core.DebugProps.isResume = true;
                    core.DebugProps.FRStack = fepRunStack;
                    core.DebugProps.executingGraphNode = Properties.executingGraphNode;
                    core.DebugProps.deferedGraphnodes = deferedGraphNodes;

                    if (core.DebugProps.RunMode == Runmode.StepNext)
                    {
                        foreach (DebugFrame debugFrame in core.DebugProps.DebugStackFrame)
                        {
                            debugFrame.FunctionStepOver = false;
                        }
                    }

                    core.RunningBlock = executingBlock;
                    PushInterpreterProps(Properties);

                    if (core.DebugProps.FirstStackFrame != null)
                    {
                        core.DebugProps.FirstStackFrame = null;
                    }
                    throw new ProtoCore.Exceptions.DebugHalting(); 
                }
            }
            return terminateExec;
        }

        // This will be called only at the time of creation of the main interpreter in the explicit case OR
        // for every implicit function call (like in replication) OR 
        // for every implicit bounce (like in dynamic lang block in inline condition) OR
        // for a Debug Resume from a breakpoint
        public void Execute(int exeblock, int entry, List<Instruction> breakpoints, Language language = Language.kInvalid)
        {
            // TODO Jun: Call RestoreFromBounce here?
            StackValue svType = rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType);
            StackFrameType type = (StackFrameType)svType.opdata;
            if (StackFrameType.kTypeLanguage == type || StackFrameType.kTypeFunction == type)
            {
                ResumeRegistersFromStack();
                bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;
            }

            SetupExecutive(exeblock, entry, language, breakpoints);


            bool debugRun = (0 != (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER));
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + CoreUtils.GetLanguageString(language));
            }

            core.DebugProps.isResume = false;

            while (!terminate)
            {
                // This will be true only for inline conditions in Associative blocks 
                if (core.DebugProps.InlineConditionOptions.isInlineConditional == true &&
                    core.DebugProps.InlineConditionOptions.instructionStream == exeblock && core.DebugProps.InlineConditionOptions.endPc == pc)
                {
                    // turn off inline conditional flag
                    {
                        core.DebugProps.InlineConditionOptions.isInlineConditional = false;
                        core.DebugProps.InlineConditionOptions.startPc = Constants.kInvalidIndex;
                        core.DebugProps.InlineConditionOptions.endPc = Constants.kInvalidIndex;
                        core.DebugProps.InlineConditionOptions.instructionStream = 0;
                    }

                    // if no longer inside a replicated/external function call, restore breakpoints
                    if (!core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsReplicating) &&
                        !core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsExternalFunction))
                    {
                        if (core.DebugProps.InlineConditionOptions.ActiveBreakPoints.Count > 0)
                        {
                            core.Breakpoints.Clear();
                            core.Breakpoints.AddRange(core.DebugProps.InlineConditionOptions.ActiveBreakPoints);
                            core.DebugProps.InlineConditionOptions.ActiveBreakPoints.Clear();
                        }
                    }
                }

                List<Instruction> instructions = istream.instrList;

                // Execute the instruction!
                Instruction executeInstruction = instructions[pc];
                Exec(instructions[pc]);

                bool restoreInstructionStream =
                    executeInstruction.opCode == OpCode.CALLR ||
                    executeInstruction.opCode == OpCode.RETURN
                    || executeInstruction.opCode == OpCode.RETC;
                if (restoreInstructionStream && isExplicitCall)
                {
                    // The instruction stream list is updated on callr
                    instructions = istream.instrList;
                    exeblock = executingBlock;
                    core.DebugProps.CurrentBlockId = exeblock;
                }

                // Disabling support for stepping into replicating function calls temporarily - pratapa
                // Check if the current instruction is a return from a function call or constructor

                DebugFrame tempFrame = null;
                if (!isExplicitCall && (instructions[core.DebugProps.DebugEntryPC].opCode == OpCode.RETURN || instructions[core.DebugProps.DebugEntryPC].opCode == OpCode.RETC))
                {
                    int ci, fi;
                    bool isReplicating;
                    DebugFrame debugFrame = null;
                    DebugReturnFromFunctionCall(pc, ref exeblock, out ci, out fi, out isReplicating, out debugFrame);

                    instructions = istream.instrList;
                    executingBlock = exeblock;
                    core.DebugProps.CurrentBlockId = exeblock;
                }
                else if (executeInstruction.opCode == OpCode.RETB)
                {
                    tempFrame = core.DebugProps.DebugStackFrame.Peek();

                    RestoreDebugPropsOnReturnFromLangBlock(ref exeblock, ref instructions);

                    // TODO: If return from previous lang block calls "_Dispose", and we have stepped into it,
                    // we need to restore the calling stackframe - pratapa
                    if (tempFrame.IsDisposeCall)
                    {
                        // TODO: If we have stepped inside _Dispose and are resuming from it - pratapa
                        if (!terminate)
                        {
                            // 1. Call everything after GC in OpCode.RETB
                            // 2. Call RestoreDebugPropsOnReturnFromLangBlock() for caller lang block
                            // 3. Return address from _Dispose is one more than the correct value and therefore needs to be fixed
                        }
                        // TODO: This works assuming debugging inside _Dispose functions is disabled
                        // ie stepping over _Dispose - pratapa
                        core.DebugProps.DebugEntryPC = core.DebugProps.ReturnPCFromDispose;
                        break;
                    }
                    else
                    {
                        core.DebugProps.DebugEntryPC = pc;
                    }
                    // Comment Jun: On explictit bounce, only on retb we update the executing block
                    // as the block scope has already change by returning to the caller block
                    executingBlock = exeblock;
                    core.RunningBlock = exeblock;
                }
                else
                {
                    core.DebugProps.DebugEntryPC = pc;
                }

                DebugFrame frame = core.DebugProps.DebugStackFrame.Peek();
                if (frame.IsInlineConditional)
                {
                    // The reference count of RX has been decreased in RETB
                    // instruction for inline conditional statement. 
                    GCRetain(RX);
                    RestoreDebugPropsOnReturnFromBuiltIns(ref exeblock, ref instructions);
                    core.DebugProps.DebugEntryPC = pc;
                }

                core.Rmem = rmem;

                bool terminateExec = HandleBreakpoint(breakpoints, instructions, pc, exeblock);
                if (terminateExec)
                {
                    break;
                }
            }

            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("End JIL Execution - " + CoreUtils.GetLanguageString(language));
            }
        }


        public void Execute(int exeblock, int entry, Language language = Language.kInvalid)
        {
            SetupExecutive(exeblock, entry, language);

            string engine = CoreUtils.GetLanguageString(language);

            bool debugRun = IsDebugRun();
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + engine);
            }

            while (!terminate)
            {
                if (pc >= istream.instrList.Count || pc < 0)
                {
                    break;
                }
                Exec(istream.instrList[pc]);
            }

            // the exception won't handled at this level, so need to unwind
#if ENABLE_EXCEPTION_HANDLING
            if (!core.ExceptionHandlingManager.IsStackUnwinding)
            {
                // Comment Jun:
                // X-lang dependency should be done for all languages 
                // as they can potentially trigger parent block updates 

                // Comment Jun: XLang dep is only done in RestoreFromBounce 
                // Propagate only on lang block bounce (non fep)
                //if (!fepRun)
                //{
                //    XLangUpdateDependencyGraph(exeblock);
                //}



                if (!fepRun || fepRun && debugRun)
                {
                    logVMMessage("End JIL Execution - " + engine);
                }
            }
#else
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("End JIL Execution - " + engine);
            }
#endif
        }

        public SymbolNode GetGlobalSymbolNode(string variable)
        {
            IEnumerable<SymbolNode> symbols = exe.runtimeSymbols[0].GetNodeForName(variable);
            foreach (var symbol in symbols)
            {
                if (symbol.functionIndex == Constants.kGlobalScope &&
                    symbol.classScope == Constants.kGlobalScope)
                {
                    return symbol;
                }
            }
            return null;
        }

        protected SymbolNode GetSymbolNode(int blockId, int classIndex, int symbolIndex)
        {
            if (DSASM.Constants.kGlobalScope == classIndex)
            {
                return exe.runtimeSymbols[blockId].symbolList[symbolIndex];
            }
            else
            {
                return exe.classTable.ClassNodes[classIndex].symbols.symbolList[symbolIndex];
            }
        }

        private StackValue GetOperandData(StackValue op1)
        {
            StackValue op2 = StackValue.BuildClassIndex(Constants.kInvalidIndex);
            return GetOperandData(-1, op1, op2);
        }

        private StackValue GetOperandData(int blockId, StackValue opSymbol, StackValue opClass, int offset = 0)
        {
            StackValue data;
            switch (opSymbol.optype)
            {
                case AddressType.Int:
                case AddressType.Double:
                case AddressType.Boolean:
                case AddressType.Char:
                case AddressType.BlockIndex:
                case AddressType.LabelIndex:
                case AddressType.ArrayDim:
                case AddressType.Pointer:
                case AddressType.ArrayPointer:
                case AddressType.ReplicationGuide:
                case AddressType.Null:
                case AddressType.Dynamic:
                case AddressType.DefaultArg:
                case AddressType.FunctionPointer:
                    data = opSymbol;
                    data.metaData.type = core.TypeSystem.GetType(opSymbol);
                    break;
                case AddressType.StaticType:
                    data = opSymbol;
                    break;
                case AddressType.Register:
                    switch ((Registers)opSymbol.opdata)
                    {
                        case Registers.AX:
                            data = AX;
                            break;
                        case Registers.BX:
                            data = BX;
                            break;
                        case Registers.CX:
                            data = CX;
                            break;
                        case Registers.DX:
                            data = DX;
                            break;
                        case Registers.EX:
                            data = EX;
                            break;
                        case Registers.FX:
                            data = FX;
                            break;
                        case Registers.LX:
                            data = LX;
                            break;
                        case Registers.RX:
                            data = RX;
                            break;
                        case Registers.SX:
                            data = SX;
                            break;
                        case Registers.TX:
                            data = TX;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case AddressType.VarIndex:
                    data = rmem.GetStackData(blockId, (int)opSymbol.opdata, (int)opClass.opdata, offset);
                    break;

                case AddressType.MemVarIndex:
                    data = rmem.GetMemberData((int)opSymbol.opdata, (int)opClass.opdata);
                    break;

                case AddressType.StaticMemVarIndex:
                    data = rmem.GetStackData(blockId, (int)opSymbol.opdata, Constants.kGlobalScope);
                    break;

                case AddressType.ThisPtr:
                    data = rmem.GetAtRelative(rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr));
                    break;

                default:
                    throw new NotImplementedException();
            }
            return data;
        }

        private void PopToW(int blockId, StackValue op1, StackValue op2, StackValue opVal)
        {
            int symbolIndex = (int)op1.opdata;
            int classIndex = (int)op2.opdata;
            SymbolNode symbol = GetSymbolNode(blockId, classIndex, symbolIndex);
            int offset = symbol.index;
            core.watchStack[offset] = opVal;
        }

        private void PushW(int block, StackValue op1, StackValue op2)
        {
            int symbol = (int)op1.opdata;
            int scope = (int)op2.opdata;
            SymbolNode node = null;
            if (Constants.kGlobalScope == scope)
            {
                node = core.DSExecutable.runtimeSymbols[block].symbolList[symbol];
            }
            else
            {
                node = core.ClassTable.ClassNodes[scope].symbols.symbolList[symbol];
            }

            int offset = node.index;
            //For watch symbol, use watching stack.
            if (core.watchSymbolList.Contains(node))
            {
                rmem.Push(core.watchStack[offset]);
            }
            else
            {
                rmem.Push(GetOperandData(block, op1, op2));
            }
        }

        protected StackValue PopTo(int blockId, StackValue op1, StackValue op2, StackValue opVal)
        {
            StackValue opPrev = StackValue.Null;
            switch (op1.optype)
            {
                case AddressType.VarIndex:
                case AddressType.MemVarIndex:
                    opPrev = rmem.SetStackData(blockId, (int)op1.opdata, (int)op2.opdata, opVal);

                    if (IsDebugRun())
                    {
                        logWatchWindow(blockId, (int)op1.opdata);
                        System.Console.ReadLine();
                    }

                    if (isGlobScope && Constants.kGlobalScope == op2.opdata)
                    {
                        logWatchWindow(blockId, (int)op1.opdata);
                    }
                    break;

                case AddressType.StaticMemVarIndex:
                    opPrev = rmem.SetStackData(blockId, (int)op1.opdata, Constants.kGlobalScope, opVal);

                    if (IsDebugRun())
                    {
                        logWatchWindow(blockId, (int)op1.opdata);
                        System.Console.ReadLine();
                    }

                    logWatchWindow(blockId, (int)op1.opdata);
                    break;
                case AddressType.Register:
                    {
                        StackValue data = opVal;
                        switch ((Registers)op1.opdata)
                        {
                            case Registers.AX:
                                opPrev = AX;
                                AX = data;
                                break;
                            case Registers.BX:
                                opPrev = BX;
                                BX = data;
                                break;
                            case Registers.CX:
                                opPrev = CX;
                                CX = data;
                                break;
                            case Registers.DX:
                                opPrev = DX;
                                DX = data;
                                break;
                            case Registers.EX:
                                opPrev = EX;
                                EX = data;
                                break;
                            case Registers.FX:
                                opPrev = FX;
                                FX = data;
                                break;
                            case Registers.RX:
                                opPrev = RX;
                                RX = data;
                                break;
                            case Registers.SX:
                                opPrev = SX;
                                SX = data;
                                break;
                            case Registers.TX:
                                opPrev = TX;
                                TX = data;
                                break;
                            case Registers.LX:
                                opPrev = LX;
                                LX = data;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    }
                default:
                    throw new NotImplementedException();

            }
            return opPrev;
        }

        private StackValue PopToIndexedArrayW(int blockId, int symbol, int classIndex, List<StackValue> dimlist, StackValue data)
        {
            int symindex = DSASM.Constants.kInvalidIndex;
            SymbolNode symbolnode = GetSymbolNode(blockId, classIndex, symbol);

            Type t = symbolnode.staticType;
            if (t.rank < 0)
            {
                t.rank = Constants.kArbitraryRank;
                t.IsIndexable = true;
            }

            StackValue value = core.watchStack[symindex];
            if (value.optype == AddressType.ArrayPointer)
            {
                StackValue oldValue;

                if (t.UID == (int)PrimitiveType.kTypeVar && t.rank < 0)
                {
                    oldValue = ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
                }
                else
                {
                    int lhsRepCount = 0;
                    foreach (var dim in dimlist)
                    {
                        if (dim.optype == AddressType.ArrayPointer)
                        {
                            lhsRepCount++;
                        }
                    }

                    if (t.rank > 0)
                    {
                        t.rank = t.rank - dimlist.Count;
                        t.rank += lhsRepCount;

                        if (t.rank > 0)
                        {
                            t.IsIndexable = true;
                        }
                        else if (t.rank == 0)
                        {
                            t.IsIndexable = false;
                        }
                        else if (t.rank < 0)
                        {
                            string message = String.Format(RuntimeData.WarningMessage.kSymbolOverIndexed, symbolnode.name);
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kOverIndexing, message);
                        }
                    }

                    oldValue = ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
                }

                return oldValue;
            }
            else if (value.optype == AddressType.String)
            {
                t.UID = (int)ProtoCore.PrimitiveType.kTypeChar;
                t.rank = 0;
                t.IsIndexable = false;

                return ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
            }
            else
            {
                if (symbolnode.staticType.rank == 0)
                {
                    rmem.SetAtSymbol(symbolnode, StackValue.Null);
                    return value;
                }
                else
                {
                    StackValue array = rmem.BuildNullArray(0);
                    GCRetain(array);
                    rmem.SetAtSymbol(symbolnode, array);
                    ArrayUtils.SetValueForIndex(array, 0, value, core);
                    return ArrayUtils.SetValueForIndices(array, dimlist, data, t, core);
                }
            }
        }

        protected StackValue PopToIndexedArray(int blockId, int symbol, int classIndex, List<StackValue> dimlist, StackValue data)
        {
            SymbolNode symbolnode = GetSymbolNode(blockId, classIndex, symbol);
            Validity.Assert(symbolnode != null);

            StackValue value = rmem.GetAtRelative(symbolnode);
            if (value.optype == AddressType.Invalid)
            {
                value = StackValue.Null;
            }

            Type t = symbolnode.staticType;
            if (t.rank < 0)
            {
                t.rank = Constants.kArbitraryRank;
                t.IsIndexable = true;
            }

            StackValue ret = StackValue.Null;
            if (value.optype == AddressType.ArrayPointer)
            {
                if (t.UID != (int)PrimitiveType.kTypeVar || t.rank >= 0)
                {
                    int lhsRepCount = 0;
                    foreach (var dim in dimlist)
                    {
                        if (dim.optype == AddressType.ArrayPointer)
                        {
                            lhsRepCount++;
                        }
                    }

                    if (t.rank > 0)
                    {
                        t.rank = t.rank - dimlist.Count;
                        t.rank += lhsRepCount;

                        if (t.rank > 0)
                        {
                            t.IsIndexable = true;
                        }
                        else if (t.rank == 0)
                        {
                            t.IsIndexable = false;
                        }
                        else if (t.rank < 0)
                        {
                            string message = String.Format(RuntimeData.WarningMessage.kSymbolOverIndexed, symbolnode.name);
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kOverIndexing, message);
                        }
                    }

                }

                ret = ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
            }
            else if (value.optype == AddressType.String)
            {
                t.UID = (int)ProtoCore.PrimitiveType.kTypeChar;
                t.rank = 0;
                t.IsIndexable = false;

                ret = ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
            }
            else
            {
                if (symbolnode.staticType.rank == 0)
                {
                    rmem.SetAtSymbol(symbolnode, StackValue.Null);
                    return value;
                }
                else
                {
                    StackValue array = rmem.BuildNullArray(0);
                    GCRetain(array);
                    rmem.SetAtSymbol(symbolnode, array);
                    if (!StackUtils.IsNull(value))
                    {
                        ArrayUtils.SetValueForIndex(array, 0, value, core);
                    }
                    ret = ArrayUtils.SetValueForIndices(array, dimlist, data, t, core);
                }
            }

            if (IsDebugRun())
            {
                logWatchWindow(blockId, symbolnode.symbolTableIndex);
                System.Console.ReadLine();
            }

            if (isGlobScope)
            {
                logWatchWindow(blockId, symbolnode.symbolTableIndex);
            }
            return ret;
        }

        private bool IsDebugRun()
        {
            return (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER) != 0;
        }

        private void SetOperandData(StackValue opdest, Object stackdata = null, int blockId = Constants.kInvalidIndex)
        {
            switch (opdest.optype)
            {
                case AddressType.VarIndex:
                case AddressType.MemVarIndex:
                    Validity.Assert(false);
                    rmem.SetStackData(0, (int)opdest.opdata, -1, stackdata);

                    if (IsDebugRun())
                    {
                        logWatchWindow(DSASM.Constants.kInvalidIndex, (int)opdest.opdata);
                        System.Console.ReadLine();
                    }

                    if (isGlobScope)
                    {
                        logWatchWindow(DSASM.Constants.kInvalidIndex, (int)opdest.opdata);
                    }
                    break;
                case AddressType.StaticMemVarIndex:
                    rmem.SetStackData(blockId, (int)opdest.opdata, -1, stackdata);

                    if (IsDebugRun())
                    {
                        logWatchWindow(DSASM.Constants.kInvalidIndex, (int)opdest.opdata);
                        System.Console.ReadLine();
                    }

                    if (isGlobScope)
                    {
                        logWatchWindow(0, (int)opdest.opdata);
                    }
                    break;
                case AddressType.Register:
                    {
                        StackValue data;
                        if (null == stackdata)
                        {
                            data = rmem.Pop();
                        }
                        else
                        {
                            data = (StackValue)stackdata;
                        }

                        switch ((Registers)opdest.opdata)
                        {
                            case Registers.AX:
                                AX = data;
                                break;
                            case Registers.BX:
                                BX = data;
                                break;
                            case Registers.CX:
                                CX = data;
                                break;
                            case Registers.DX:
                                DX = data;
                                break;
                            case Registers.EX:
                                EX = data;
                                break;
                            case Registers.FX:
                                FX = data;
                                break;
                            case Registers.LX:
                                LX = data;
                                break;
                            case Registers.RX:
                                RX = data;
                                break;
                            case Registers.SX:
                                SX = data;
                                break;
                            case Registers.TX:
                                TX = data;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private void Nullify(ref StackValue op1, ref StackValue op2)
        {
            if (AddressType.Null == op1.optype || AddressType.Null == op2.optype)
            {
                op1.optype = AddressType.Null;
                op2.optype = AddressType.Null;
            }
        }

        protected void runtimeVerify(bool condition, string msg = "Dsasm runtime error. Exiting...\n")
        {
            // TODO Jun: hook this up to a runtime error handler            
            if (!condition)
                throw new ProtoCore.Exceptions.RuntimeException(msg);
        }

        private StackValue GetFinalPointer(int depth, bool isDotFunctionBody = false)
        {
            DSASM.RTSymbol[] rtSymbols = new DSASM.RTSymbol[depth];
            bool isInvalidIdentList = false;
            for (int i = depth - 1; i >= 0; --i)
            {
                // Get the symbol
                rtSymbols[i].Sv = rmem.Pop();

                AddressType optype = rtSymbols[i].Sv.optype;
                if (!isDotFunctionBody
                    && optype != AddressType.Pointer
                    && optype != AddressType.ArrayPointer
                    && optype != AddressType.Dynamic
                    && optype != AddressType.ClassIndex)
                {
                    isInvalidIdentList = true;
                }

                if (isDotFunctionBody && i != 0)
                {
                    StackValue dimSv = rmem.Pop();
                    dimSv.optype = AddressType.ArrayDim;
                    rmem.Push(dimSv);
                }

                if (AddressType.ArrayDim == rmem.Stack[rmem.Stack.Count - 1].optype)
                {
                    // Get the number of demension pushed
                    StackValue svDim = rmem.Pop();
                    int dimensions = (int)svDim.opdata;

                    if (dimensions > 0)
                    {
                        if (isDotFunctionBody && i != 0)
                        {
                            //push its dimension value
                            StackValue dimValArraySv = rmem.Pop();
                            foreach (StackValue dimValSv in core.Heap.Heaplist[(int)dimValArraySv.opdata].Stack)
                            {
                                rmem.Push(dimValSv);
                            }
                        }
                        // Pop off each dimension
                        rtSymbols[i].Dimlist = new int[dimensions];
                        for (int j = dimensions - 1; j >= 0; --j)
                        {
                            svDim = rmem.Pop();
                            if (AddressType.Int != svDim.optype)
                            {
                                isInvalidIdentList = true;
                            }
                            rtSymbols[i].Dimlist[j] = (int)svDim.opdata;
                        }
                    }
                    else if (isDotFunctionBody && i != 0)
                    {
                        rmem.Pop(); //pop the rhsDimExprList (arrayPointer)
                    }
                }
            }

            if (isInvalidIdentList)
            {
                return StackValue.Null;
            }

            if (isDotFunctionBody)
            {
                if (rtSymbols[0].Sv.optype == AddressType.Int) // static, class UID
                {
                    // if static, the opdata of rtSymbols[0] is not used, no need to bother that 
                    int type = (int)rtSymbols[0].Sv.opdata;
                    rtSymbols[0].Sv.metaData.type = type;
                    rtSymbols[0].Sv.optype = AddressType.ClassIndex;
                }
                rtSymbols[1].Sv.optype = AddressType.Dynamic;
            }


            if (1 == depth)
            {
                return GetIndexedArray(rtSymbols[0].Sv, rtSymbols[0].Dimlist);
            }

            // Get first stackvalue of the first elemnt in the ident list
            // Get its indexed value
            rtSymbols[0].Sv = GetIndexedArray(rtSymbols[0].Sv, rtSymbols[0].Dimlist);

            //If the value of the first identifier is null, return null stack value
            if (rtSymbols[0].Sv.optype == AddressType.Null)
            {
                return rtSymbols[0].Sv;
            }

            int index = -1;
            int ptr = (int)rtSymbols[0].Sv.opdata;

            // Traverse the heap until the last pointer
            int n;
            int classsccope = (int)rtSymbols[0].Sv.metaData.type;
            for (n = 1; n < rtSymbols.Length; ++n)
            {
                // Index into the current pointer
                // 'index' is the index of the member variable

                // class f {
                //   x : var; y : var // index of x = 0, y = 1
                // }

                //resolve dynamic reference
                if (AddressType.Dynamic == rtSymbols[n].Sv.optype)
                {
                    classsccope = (int)rtSymbols[n - 1].Sv.metaData.type;
                    bool succeeded = ProcessDynamicVariable((rtSymbols[n].Dimlist != null), ref rtSymbols[n].Sv, classsccope);
                    //if the identifier is unbounded. Push null
                    if (!succeeded)
                    {
                        return StackValue.Null;
                    }
                }

                if (rtSymbols[n].Sv.optype == AddressType.StaticMemVarIndex)
                {
                    StackValue op2 = StackValue.BuildClassIndex(Constants.kInvalidIndex);
                    rtSymbols[n].Sv = GetOperandData(0, rtSymbols[n].Sv, op2);
                }
                else
                {
                    index = (int)rtSymbols[n].Sv.opdata;
                    rtSymbols[n].Sv = core.Heap.Heaplist[ptr].Stack[index];
                }

                // Once a pointer to the member is retrieved, get its indexed value
                rtSymbols[n].Sv = GetIndexedArray(rtSymbols[n].Sv, rtSymbols[n].Dimlist);
                ptr = (int)rtSymbols[n].Sv.opdata;
            }

            // Check the last pointer
            StackValue opVal = rtSymbols[n - 1].Sv;
            AddressType addrtype = opVal.optype;
            if (AddressType.Pointer == addrtype || AddressType.Invalid == addrtype)
            {
                /*
                  if lookahead is Not a pointer then
                      move to that pointer and get its value at stack index 0 (or further if array)
                      push that
                  else 
                      push the current ptr
                  end
                */

                // Determine if we still need to move one more time on the heap
                // Peek into the pointed data using nextPtr. 
                // If nextPtr is not a pointer (a primitive) then return the data at nextPtr
                int nextPtr = (int)opVal.opdata;
                bool isActualData =
                        AddressType.Pointer != core.Heap.Heaplist[nextPtr].Stack[0].optype
                    && AddressType.ArrayPointer != core.Heap.Heaplist[nextPtr].Stack[0].optype
                    && AddressType.Invalid != core.Heap.Heaplist[nextPtr].Stack[0].optype; // Invalid is an uninitialized member

                if (isActualData)
                {
                    // Move one more and get the value at the first heapstack
                    opVal = core.Heap.Heaplist[nextPtr].Stack[0];
                }
            }
            return opVal;
        }

        private StackValue GetIndexedArray(StackValue svPtr, int[] dimList)
        {
            // 'svPtr' is the array pointer that is to be indexed into
            if (null == dimList || dimList.Length <= 0)
            {
                return svPtr;
            }

            if (svPtr.optype != AddressType.ArrayPointer)
            {
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, RuntimeData.WarningMessage.kArrayOverIndexed);
                return StackValue.Null;
            }

            //
            // Comment Jun: It is possible that the type is not an array, and in such cases just take the value
            // Here is such as case:
            //      a = 100;
            //      z = 0;
            //      for (i in a)
            //      {
            //          z = z + i;
            //      }
            //
            // In this example, 'a' is assumed to be an array by the forloop and is compiled into: i = a[auto_counter]
            // In this case, just return 'a' and there is no need to traverse the heap
            //

            if (AddressType.ArrayPointer != svPtr.optype)
            {
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, RuntimeData.WarningMessage.kArrayOverIndexed);
                return StackValue.Null;
            }

            int ptr = (int)svPtr.opdata;
            int dimensions = dimList.Length;
            for (int n = 0; n < dimensions - 1; ++n)
            {
                // TODO Jun: This means that variables are coerced to 32-bit when used as an array index
                try
                {
                    StackValue array = core.Heap.Heaplist[ptr].GetValue(dimList[n], core);
                    if (array.optype != AddressType.ArrayPointer)
                    {
                        core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, RuntimeData.WarningMessage.kArrayOverIndexed);
                        return StackValue.Null;
                    }
                    ptr = (int)array.opdata;
                }
                catch (ArgumentOutOfRangeException)
                {
                    core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, RuntimeData.WarningMessage.kArrayOverIndexed);
                    return StackValue.Null;
                }
            }
            StackValue sv = StackValue.Null;
            try
            {
                sv = core.Heap.Heaplist[ptr].GetValue(dimList[dimensions - 1], core);
            }
            catch (ArgumentOutOfRangeException)
            {
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, RuntimeData.WarningMessage.kArrayOverIndexed);
                sv = StackValue.Null;
            }
            catch (IndexOutOfRangeException)
            {
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kIndexOutOfRange, RuntimeData.WarningMessage.kIndexOutOfRange);
                return StackValue.Null;
            }
            return sv;
        }

        public StackValue GetIndexedArray(StackValue array, List<StackValue> indices)
        {
            return ArrayUtils.GetValueFromIndices(array, indices, core);
        }

        public StackValue GetIndexedArrayW(int dimensions, int blockId, StackValue op1, StackValue op2)
        {
            var dims = new List<StackValue>();
            for (int n = dimensions - 1; n >= 0; --n)
            {
                dims.Insert(0, rmem.Pop());
            }

            int symbolIndex = (int)op1.opdata;
            int classIndex = (int)op2.opdata;

            SymbolNode symbolNode = GetSymbolNode(blockId, classIndex, symbolIndex);
            int stackindex = symbolNode.index;
            string varname = symbolNode.name;

            StackValue thisArray;
            if (core.ExecMode == InterpreterMode.kExpressionInterpreter && core.watchSymbolList.Contains(symbolNode))
            {
                thisArray = core.watchStack[symbolNode.index];
            }
            else
            {
                if (op1.optype == AddressType.MemVarIndex)
                {
                    int thisptr = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr).opdata;
                    thisArray = rmem.Heap.Heaplist[thisptr].Stack[stackindex];
                }
                else
                {
                    thisArray = rmem.GetAtRelative(symbolNode);
                }
            }

            if (AddressType.ArrayPointer != thisArray.optype)
            {
                if (varname.StartsWith(ProtoCore.DSASM.Constants.kForLoopExpression))
                {
                    return thisArray;
                }

                string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            StackValue result;
            try
            {
                result = GetIndexedArray(thisArray, dims);
            }
            catch (ArgumentOutOfRangeException)
            {
                string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            return result;
        }

        public StackValue GetIndexedArray(List<StackValue> dims, int blockId, StackValue op1, StackValue op2)
        {
            int symbolIndex = (int)op1.opdata;
            int classIndex = (int)op2.opdata;

            SymbolNode symbolNode = GetSymbolNode(blockId, classIndex, symbolIndex);
            int stackindex = rmem.GetStackIndex(symbolNode);
            string varname = symbolNode.name;

            StackValue thisArray;
            if (op1.optype == AddressType.MemVarIndex)
            {
                int thisptr = (int)rmem.GetAtRelative(rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr)).opdata;

                // For member variables, the stackindex is absolute and needs no offset as this is found in the heap
                stackindex = symbolNode.index;
                thisArray = rmem.Heap.Heaplist[thisptr].Stack[stackindex];
            }
            else
            {
                thisArray = rmem.GetAtRelative(symbolNode);
            }

            if (AddressType.ArrayPointer != thisArray.optype)
            {
                if (varname.StartsWith(ProtoCore.DSASM.Constants.kForLoopExpression))
                {
                    return thisArray;
                }

                string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            StackValue result;
            try
            {
                result = GetIndexedArray(thisArray, dims);
            }
            catch (ArgumentOutOfRangeException)
            {
                string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            return result;
        }

        private bool ProcessDynamicVariable(bool isArray, ref StackValue svPtr, int classIndex)
        {
            int variableDynamicIndex = (int)svPtr.opdata;
            DSASM.DyanmicVariableNode dynamicVariableNode = core.DynamicVariableTable.variableTable[variableDynamicIndex];
            string name = dynamicVariableNode.variableName;
            int contextClassIndex = dynamicVariableNode.classIndex;
            int contextProcIndex = dynamicVariableNode.procIndex;

            SymbolNode node = null;
            bool isStatic = false;
            //if (classscope == ProtoCore.DSASM.Constants.kGlobalScope)
            //{
            //    classscope = dynamicVariableNode.classIndex;
            //    int blockId = dynamicVariableNode.codeBlockId;
            //    int symbolIndex = dynamicVariableNode.symbolIndex;
            //    if (blockId != (int)ProtoCore.DSASM.Constants.kInvalidIndex && symbolIndex != (int)ProtoCore.DSASM.Constants.kInvalidIndex)
            //    {
            //        svPtr = rmem.GetStackData(blockId, symbolIndex, classscope);
            //        return true;
            //        //if (Constants.kInvalidIndex == classscope)
            //        //{
            //        //    node = core.executable.runtimeSymbols[blockId].symbolList[symbolIndex];
            //        //}
            //        //else
            //        //{
            //        //    node = core.classTable.list[classscope].symbols.symbolList[symbolIndex];
            //        //}
            //    }
            //    return false;
            //    //if (node != null)
            //    //{
            //    //    svPtr.metaData.type = node.datatype.UID;
            //    //}
            //}
            //if (node == null)
            //{
            if (!((int)ProtoCore.PrimitiveType.kTypeVoid == classIndex
                || ProtoCore.DSASM.Constants.kInvalidIndex == classIndex
                || core.ClassTable.ClassNodes[classIndex].symbols == null))
            {
                bool hasThisSymbol;
                ProtoCore.DSASM.AddressType addressType;

                int symbolIndex = core.ClassTable.ClassNodes[classIndex].GetSymbolIndex(name, contextClassIndex, contextProcIndex, core.RunningBlock, core, out hasThisSymbol, out addressType);
                if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolIndex)
                {
                    if (addressType == AddressType.StaticMemVarIndex)
                    {
                        node = core.CodeBlockList[0].symbolTable.symbolList[symbolIndex];
                        isStatic = true;
                    }
                    else
                    {
                        node = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symbolIndex];
                    }
                }
            }
            //}

            if (null == node)
            {
                return false;
            }
            svPtr.opdata = node.symbolTableIndex;
            svPtr.optype = isArray ? AddressType.ArrayPointer : (isStatic ? AddressType.StaticMemVarIndex : AddressType.Pointer);
            return true;
        }

        private bool ProcessDynamicFunction(Instruction instr)
        {
            int fptr = ProtoCore.DSASM.Constants.kInvalidIndex;
            int functionDynamicIndex = (int)instr.op1.opdata;
            int classIndex = (int)instr.op2.opdata;
            int depth = (int)instr.op3.opdata;
            bool isDotMemFuncBody = functionDynamicIndex == ProtoCore.DSASM.Constants.kInvalidIndex;
            bool isFunctionPointerCall = false;
            if (isDotMemFuncBody)
            {
                functionDynamicIndex = (int)rmem.Pop().opdata;
            }

            DSASM.DynamicFunctionNode dynamicFunctionNode = core.DynamicFunctionTable.functionTable[functionDynamicIndex];

            if (isDotMemFuncBody)
            {
                classIndex = dynamicFunctionNode.classIndex;
            }

            string procName = dynamicFunctionNode.functionName;
            List<ProtoCore.Type> arglist = dynamicFunctionNode.argList;
            if (procName == ProtoCore.DSASM.Constants.kFunctionPointerCall && depth == 0)
            {
                isFunctionPointerCall = true;
                classIndex = ProtoCore.DSASM.Constants.kGlobalScope;
                StackValue fpSv = rmem.Pop();
                if (fpSv.optype != AddressType.FunctionPointer)
                {
                    rmem.Pop(arglist.Count); //remove the arguments
                    return false;
                }
                fptr = (int)fpSv.opdata;
            }



            //retrieve the function arguments
            List<StackValue> argSvList = new List<StackValue>();
            if (isDotMemFuncBody)
            {
                arglist = new List<Type>();
                StackValue argArraySv = rmem.Pop();
                for (int i = 0; i < ArrayUtils.GetElementSize(argArraySv, core); ++i)
                {
                    StackValue sv = core.Heap.Heaplist[(int)argArraySv.opdata].Stack[i];
                    argSvList.Add(sv); //actual arguments
                    ProtoCore.Type paramType = new ProtoCore.Type();
                    paramType.UID = (int)sv.metaData.type;
                    paramType.IsIndexable = sv.optype == AddressType.ArrayPointer;
                    if (paramType.IsIndexable)
                    {
                        StackValue paramSv = sv;
                        while (paramSv.optype == AddressType.ArrayPointer)
                        {
                            paramType.rank++;
                            int arrayHeapPtr = (int)paramSv.opdata;
                            if (core.Heap.Heaplist[arrayHeapPtr].VisibleSize > 0)
                            {
                                paramSv = core.Heap.Heaplist[arrayHeapPtr].Stack[0];
                                paramType.UID = (int)paramSv.metaData.type;
                            }
                            else
                            {
                                paramType.UID = (int)ProtoCore.PrimitiveType.kTypeArray;
                                break;
                            }
                        }
                    }
                    arglist.Add(paramType); //build arglist
                }
                argSvList.Reverse();
            }
            else
            {
                for (int i = 0; i < arglist.Count; i++)
                {
                    StackValue argSv = rmem.Pop();
                    argSvList.Add(argSv);
                }
            }
            int lefttype = DSASM.Constants.kGlobalScope;
            bool isLeftClass = false;
            if (isDotMemFuncBody && rmem.Stack.Last().optype == AddressType.Int) //constructor or static function
            {
                //in this case, ptr won't be used
                lefttype = (int)rmem.Pop().opdata;
                isLeftClass = true;
            }
            else if (depth > 0)
            {
                //resolve the identifier list            
                StackValue pSv = GetFinalPointer(depth);
                //push the resolved stack value to stack
                rmem.Push(pSv);
                lefttype = (int)pSv.metaData.type;
            }

            int type = lefttype;

            if (depth > 0)
            {
                // check whether it is function pointer, this checking is done at runtime to handle the case
                // when turning on converting dot operator to function call
                if (!((int)ProtoCore.PrimitiveType.kTypeVoid == type
                    || ProtoCore.DSASM.Constants.kInvalidIndex == type
                    || core.ClassTable.ClassNodes[type].symbols == null))
                {
                    bool hasThisSymbol;
                    ProtoCore.DSASM.AddressType addressType;
                    SymbolNode node = null;
                    bool isStatic = false;
                    int symbolIndex = core.ClassTable.ClassNodes[type].GetSymbolIndex(procName, type, DSASM.Constants.kGlobalScope, core.RunningBlock, core, out hasThisSymbol, out addressType);
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolIndex)
                    {
                        if (addressType == AddressType.StaticMemVarIndex)
                        {
                            node = core.CodeBlockList[0].symbolTable.symbolList[symbolIndex];
                            isStatic = true;
                        }
                        else
                        {
                            node = core.ClassTable.ClassNodes[type].symbols.symbolList[symbolIndex];
                        }
                    }
                    if (node != null)
                    {
                        isFunctionPointerCall = true;
                        StackValue fpSv = new StackValue();
                        fpSv.opdata = node.symbolTableIndex;
                        fpSv.optype = isStatic ? AddressType.StaticMemVarIndex : AddressType.Pointer;
                        if (fpSv.optype == AddressType.StaticMemVarIndex)
                        {
                            StackValue op2 = new StackValue();
                            op2.optype = AddressType.ClassIndex;
                            op2.opdata = Constants.kInvalidIndex;

                            fpSv = GetOperandData(0, fpSv, op2);
                        }
                        else
                        {
                            int ptr = (int)rmem.Stack.Last().opdata;
                            fpSv = core.Heap.Heaplist[ptr].Stack[(int)fpSv.opdata];
                        }
                        //assuming the dimension is zero, as funtion call with nonzero dimension is not supported yet

                        // Check the last pointer
                        AddressType addrtype = fpSv.optype;
                        if (AddressType.Pointer == addrtype || AddressType.Invalid == addrtype)
                        {
                            /*
                              if lookahead is Not a pointer then
                                  move to that pointer and get its value at stack index 0 (or further if array)
                                  push that
                              else 
                                  push the current ptr
                              end
                            */

                            // Determine if we still need to move one more time on the heap
                            // Peek into the pointed data using nextPtr. 
                            // If nextPtr is not a pointer (a primitive) then return the data at nextPtr
                            int nextPtr = (int)fpSv.opdata;
                            bool isActualData =
                                    AddressType.Pointer != core.Heap.Heaplist[nextPtr].Stack[0].optype
                                && AddressType.ArrayPointer != core.Heap.Heaplist[nextPtr].Stack[0].optype
                                && AddressType.Invalid != core.Heap.Heaplist[nextPtr].Stack[0].optype; // Invalid is an uninitialized member

                            if (isActualData)
                            {
                                // Move one more and get the value at the first heapstack
                                fpSv = core.Heap.Heaplist[nextPtr].Stack[0];
                            }
                        }
                        if (fpSv.optype != AddressType.FunctionPointer)
                        {
                            rmem.Pop(); //remove final pointer
                            return false;
                        }
                        fptr = (int)fpSv.opdata;
                    }
                }
            }

            ProtoCore.DSASM.ProcedureNode procNode = null;
            if (isFunctionPointerCall)
            {
                ProtoCore.DSASM.FunctionPointerNode fptrNode;
                if (core.FunctionPointerTable.functionPointerDictionary.TryGetByFirst(fptr, out fptrNode))
                {
                    int blockId = fptrNode.blockId;
                    int procId = fptrNode.procId;
                    int classId = fptrNode.classScope;

                    if (Constants.kGlobalScope == classId)
                    {
                        procName = exe.procedureTable[blockId].procList[procId].name;
                        CodeBlock codeblock = core.GetCodeBlock(core.CodeBlockList, blockId);
                        procNode = core.GetFirstVisibleProcedure(procName, arglist, codeblock);
                    }
                    else
                    {
                        procNode = exe.classTable.ClassNodes[classId].vtable.procList[procId];
                    }
                    type = classId;
                }
                else
                {
                    procNode = null;
                }
            }
            else
            {
                // This is a member function of the previous type
                if (ProtoCore.DSASM.Constants.kInvalidIndex != type)
                {
                    int realType;
                    bool isAccessible;
                    ProtoCore.DSASM.ProcedureNode memProcNode = core.ClassTable.ClassNodes[type].GetMemberFunction(procName, arglist, classIndex, out isAccessible, out realType);

                    if (memProcNode == null)
                    {
                        string property;
                        if (CoreUtils.TryGetPropertyName(procName, out property))
                        {
                            string classname = core.ClassTable.ClassNodes[type].name;
                            string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kPropertyOfClassNotFound, classname, property);
                            core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure, message);
                        }
                        else
                        {
                            string message = String.Format(ProtoCore.RuntimeData.WarningMessage.kMethodResolutionFailure, procName);
                            core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure, message);
                        }
                    }
                    else
                    {
                        procNode = memProcNode;
                        type = realType;

                        // if the proc node is not accessible, that error will be handled by
                        // callr() later on. 
                    }
                }
            }

            if (null != procNode)
            {
                if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                {
                    if (isLeftClass || (isFunctionPointerCall && depth > 0)) //constructor or static function or function pointer call
                    {
                        rmem.Pop(); //remove the array dimension for "isLeftClass" or final pointer for "isFunctionPointerCall"
                        instr.op3.opdata = 0; //depth = 0
                    }
                    //push back the function arguments
                    for (int i = argSvList.Count - 1; i >= 0; i--)
                    {
                        rmem.Push(argSvList[i]);
                    }
                    //push value-not-provided default argument
                    for (int i = arglist.Count; i < procNode.argInfoList.Count; i++)
                    {
                        rmem.Push(StackValue.BuildDefaultArgument());
                    }

                    // Push the function declaration block  
                    StackValue opblock = StackValue.BuildBlockIndex(procNode.runtimeIndex);
                    rmem.Push(opblock);

                    int dimensions = 0;
                    StackValue opdim = StackValue.BuildArrayDimension(dimensions);
                    rmem.Push(opdim);

                    //Modify the operand data
                    instr.op1.opdata = procNode.procId;
                    instr.op1.optype = AddressType.FunctionIndex;
                    instr.op2.opdata = type;

                    return true;
                }

            }
            if (!(isFunctionPointerCall && depth == 0))
            {
                rmem.Pop(); //remove the array dimension for "isLeftClass" or final pointer
            }
            return false;
        }

        // helper method for GC
        public void GCRelease(StackValue sv)
        {
            if ((core.ExecMode != InterpreterMode.kExpressionInterpreter))
            {
                rmem.Heap.GCRelease(new StackValue[] { sv }, this);
            }
        }

        // this method only decrement the reference counter, it does not gc them
        public void DecRefCounter(StackValue sv)
        {
            if ((core.ExecMode != InterpreterMode.kExpressionInterpreter))
            {
                rmem.Heap.DecRefCount(sv);
            }
        }

        public void GCRetain(StackValue sv)
        {
            if ((core.ExecMode != InterpreterMode.kExpressionInterpreter))
            {
                rmem.Heap.IncRefCount(sv);
            }
        }

        // GC for local code blocks if,for,while
        public void GCCodeBlock(int blockId, int functionIndex = DSASM.Constants.kGlobalScope, int classIndex = DSASM.Constants.kInvalidIndex)
        {
            foreach (ProtoCore.DSASM.SymbolNode sn in exe.runtimeSymbols[blockId].symbolList.Values)
            {   
                bool allowGC = sn.classScope == classIndex 
                    && sn.functionIndex == functionIndex 
                    && !sn.name.Equals(Constants.kWatchResultVar);
                    /*&& !CoreUtils.IsSSATemp(sn.name)*/

                if (core.Options.GCTempVarsOnDebug && core.Options.ExecuteSSA)
                {
                    if (core.Options.IDEDebugMode)
                    {
                        allowGC = sn.classScope == classIndex 
                            && sn.functionIndex == functionIndex 
                            && !sn.name.Equals(Constants.kWatchResultVar)
                            && !CoreUtils.IsSSATemp(sn.name);
                    }
                }

                if (allowGC)
                {
                    int offset = sn.index;
                    int n = offset;
                    if (sn.absoluteFunctionIndex != DSASM.Constants.kGlobalScope)
                    {
                        // Comment Jun: We only want the relative offset if a variable is in a function
                        n = rmem.GetRelative(rmem.GetStackIndex(offset));
                    }
                    if (n >= 0)
                    {
                        if (core.Options.ExecuteSSA)
                        { 
                            // if this block is not the outer most one, gc all the local variables 
                            // if this block is the outer most one, gc the temp variables only
                            if (blockId != 0)
                            {
                                GCRelease(rmem.Stack[n]);
                            }
                            else if (sn.isTemp)
                            {
                                GCRelease(rmem.Stack[n]);
                            }
                        }
                        else
                        {
                            // if this block is not the outer most one, gc all the local variables 
                            // if this block is the outer most one, gc the temp variables only
                            if (blockId != 0)
                            {
                                GCRelease(rmem.Stack[n]);
                            }
                            else if (sn.isTemp)
                            {
                                GCRelease(rmem.Stack[n]);
                            }
                        }

                        if (blockId != 0)
                        {
                            StackValue sv = rmem.Stack[n];
                            sv.optype = AddressType.Invalid;
                            rmem.Stack[n] = sv;
                        }
                    }
                }
            }
        }

        public void GCAnonymousSymbols(List<SymbolNode> symbolList, bool isLastNodeSetter = false)
        {
            //foreach (SymbolNode symbol in symbolList)
            for(int i = 0; i < symbolList.Count; ++i)
            {
                //
                // Comment Jun: We want to prevent GC of temp var if it is being assigned to a property
                // This is a current issue of property setters where a GCRetain is not called on the RHS 
                // i.e. 
                //  a.b = p -> GCRetain is not called when p is popped to a.b
                //
                if (isLastNodeSetter && i == symbolList.Count - 1)
                {
                    break;
                }

                SymbolNode symbol = symbolList[i];

                int offset = symbol.index;
                int n = offset;
                if (symbol.absoluteFunctionIndex != DSASM.Constants.kGlobalScope)
                {
                    // Comment Jun: We only want the relative offset if a variable is in a function
                    n = rmem.GetRelative(rmem.GetStackIndex(offset));
                }

                if (n >= 0)
                {
                    GCRelease(rmem.Stack[n]);
                }
            }
        }

        public void ReturnSiteGC(int blockId, int classIndex, int functionIndex)
        {
            ProcedureNode pn = null;
            SymbolTable st = null;
            List<StackValue> ptrList = new List<StackValue>();
            if (DSASM.Constants.kInvalidIndex == classIndex)
            {
                pn = exe.procedureTable[blockId].procList[functionIndex];
                st = core.CompleteCodeBlockList[blockId].symbolTable;
            }
            else
            {
                pn = exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex];
                st = exe.classTable.ClassNodes[classIndex].symbols;
            }

            foreach (SymbolNode symbol in st.symbolList.Values)
            {
                bool allowGC = symbol.functionIndex == functionIndex
                    && !symbol.name.Equals(ProtoCore.DSASM.Constants.kWatchResultVar);

                if (core.Options.GCTempVarsOnDebug && core.Options.ExecuteSSA)
                {
                    if (core.Options.IDEDebugMode)
                    {
                        allowGC = symbol.functionIndex == functionIndex
                            && !symbol.name.Equals(ProtoCore.DSASM.Constants.kWatchResultVar)
                            && !CoreUtils.IsSSATemp(symbol.name);
                    }
                }

                if (allowGC)
                {
                    StackValue sv = rmem.GetAtRelative(symbol);
                    if (AddressType.Pointer == sv.optype || AddressType.ArrayPointer == sv.optype)
                    {
                        ptrList.Add(sv);
                    }
                }
            }

            foreach (CodeBlock cb in core.CompleteCodeBlockList[blockId].children)
            {
                if (cb.blockType == CodeBlockType.kConstruct)
                    GCCodeBlock(cb.codeBlockId, functionIndex, classIndex);
            }


            if (ptrList.Count > 0)
            {
                core.Rmem.Heap.GCRelease(ptrList.ToArray(), this);
            }
        }

        public void Modify_istream_instrList_FromSetValue(int pc, StackValue op)
        {
            istream.instrList[pc].op1 = op;
        }

        public void Modify_istream_instrList_FromSetValue(int blockId, int pc, StackValue op)
        {
            exe.instrStreamList[blockId].instrList[pc].op1 = op;
        }

        public void Modify_istream_entrypoint_FromSetValue(int pc)
        {
            istream.entrypoint = pc;
        }

        public void Modify_istream_entrypoint_FromSetValue(int blockId, int pc)
        {
            exe.instrStreamList[blockId].entrypoint = pc;
        }

        public AssociativeGraph.GraphNode GetLastGraphNode(string varName)
        {
            return istream.dependencyGraph.GraphList.Last(x =>
                    null != x.updateNodeRefList && x.updateNodeRefList.Count > 0
                && null != x.updateNodeRefList[0].nodeList && x.updateNodeRefList[0].nodeList.Count > 0
                && x.updateNodeRefList[0].nodeList[0].symbol.name == varName);
        }


        public AssociativeGraph.GraphNode GetFirstGraphNode(string varName, out int blockId)
        {
            blockId = 0;
            for (int n = 0; n < exe.instrStreamList.Length; ++n)
            {
                InstructionStream stream = exe.instrStreamList[n];
                for (int i = 0; i < stream.dependencyGraph.GraphList.Count; ++i)
                {
                    AssociativeGraph.GraphNode node = stream.dependencyGraph.GraphList[i];
                    if (
                        null != node.updateNodeRefList
                        && node.updateNodeRefList.Count > 0
                        && null != node.updateNodeRefList[0].nodeList
                        && node.updateNodeRefList[0].nodeList.Count > 0
                        && node.updateNodeRefList[0].nodeList[0].symbol.name == varName)
                    {
                        blockId = n;
                        return node;
                    }
                }
            }
            return null;
        }

        public ProcedureNode GetProcedureNode(int blockId, int classIndex, int functionIndex)
        {
            if (ProtoCore.DSASM.Constants.kGlobalScope != classIndex)
            {
                return exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex];
            }
            return exe.procedureTable[blockId].procList[functionIndex];
        }

        private void GetLocalAndParamCount(int blockId, int classIndex, int functionIndex, out int localCount, out int paramCount)
        {
            localCount = paramCount = 0;

            if (ProtoCore.DSASM.Constants.kGlobalScope != classIndex)
            {
                localCount = exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex].localCount;
                paramCount = exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex].argTypeList.Count;
            }
            else
            {
                localCount = exe.procedureTable[blockId].procList[functionIndex].localCount;
                paramCount = exe.procedureTable[blockId].procList[functionIndex].argTypeList.Count;
            }
        }

        /*
        public int GetPreviousFramePointer(int offset)
        {
            int ptr = (int)rmem.GetAtRelative(offset, ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr).opdata;
            int ci = (int)rmem.GetAtRelative(offset, ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            int fi = (int)rmem.GetAtRelative(offset, ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
            int blockId = (int)rmem.GetAtRelative(offset, ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock).opdata;

            int localVars = 0;
            int paramCount = 0;
            GetLocalAndParamCount(blockId, ci, fi, out localVars, out paramCount);
            int localCount = localVars + paramCount;


            return rmem.Stack.Count - ProtoCore.DSASM.StackFrame.kStackFrameSize - localCount - paramCount - offset;
        }
         * */

        public List<List<ProtoCore.ReplicationGuide>> GetCachedReplicationGuides(Core core, int argumentCount)
        {
            int index = core.replicationGuides.Count - argumentCount;
            if (index >= 0)
            {
                var replicationGuides = core.replicationGuides.GetRange(index, argumentCount);
                core.replicationGuides.RemoveRange(index, argumentCount);
                return replicationGuides;
            }
            return new List<List<ProtoCore.ReplicationGuide>>();
        }

        private void NONE_Handler(Instruction instruction)
        {

        }

        private void END_Handler(Instruction instruction)
        {

        }
        private void ALLOC_Handler(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        private void ALLOCC_Handler(Instruction instruction)
        {
            fepRunStack.Push(fepRun);
            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op1.optype);
            int type = (int)instruction.op1.opdata;
            int ptr = ProtoCore.DSASM.Constants.kInvalidPointer;
            lock (core.Heap.cslock)
            {
                ptr = core.Heap.Allocate(exe.classTable.ClassNodes[type].size);
            }

            MetaData metadata;
            metadata.type = type;
            StackValue pointer = StackValue.BuildPointer(ptr, metadata);
            rmem.SetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr, pointer);

            // Increase reference count here to avoid that in the 
            // constructor we use thisptr so that the executive will
            // thinks it is a temporary object and GC it. 
            GCRetain(pointer);

            ++pc;
            return;
        }

        private void ALLOCM_Handler(Instruction instruction)
        {
            runtimeVerify(ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype);
            int offset = (int)instruction.op1.opdata;
            int ptr = -1;

            lock (core.Heap.cslock)
            {
                ptr = core.Heap.Allocate(ProtoCore.DSASM.Constants.kPointerSize);
            }

            int thisptr = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr).opdata;

            lock (core.Heap.cslock)
            {
                core.Heap.Heaplist[thisptr].Stack[offset] = StackValue.BuildPointer(ptr);
            }
            ++pc;
            return;
        }

        private void PUSH_Handler(Instruction instruction)
        {
            int dimensions = 0;
            bool objectIndexing = false;

            int blockId = DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.Pointer == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.FunctionPointer == instruction.op1.optype)
            {

                // TODO: Jun this is currently unused but required for stack alignment
                StackValue svType = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.StaticType == svType.optype);

                StackValue svDim = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                blockId = (int)svBlock.opdata;

                objectIndexing = true;
            }

            bool elementBasedUpdate = core.Options.ElementBasedArrayUpdate
                                    && Properties.executingGraphNode != null
                                    && Properties.executingGraphNode.updateDimensions.Count > 0;
            // At present element based array update only supports single 
            // variable on the RHS of expression. So for graph node
            //
            //    x = foo(a[i]);
            //    a[i][0] = 1;
            // 
            // there are two dependent node 'a', 'i'. To avoid dimension '[0]'
            // applied to 'i', double check to ensure it is the first dependent
            // node 'a'. 
            if (objectIndexing && elementBasedUpdate)
            {
                SymbolNode symbolNode = GetSymbolNode(blockId, (int)instruction.op2.opdata, (int)instruction.op1.opdata);
                AssociativeGraph.UpdateNode firstDepNode = Properties.executingGraphNode.dependentList[0].updateNodeRefList[0].nodeList[0];
                elementBasedUpdate = firstDepNode.symbol.IsEqualAtScope(symbolNode);
            }

            if (0 == dimensions && !elementBasedUpdate || !objectIndexing)
            {
                int fp = core.Rmem.FramePointer;
                if (core.ExecMode == InterpreterMode.kExpressionInterpreter && instruction.op1.optype == AddressType.ThisPtr)
                    core.Rmem.FramePointer = core.watchFramePointer;
                StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                if (core.ExecMode == InterpreterMode.kExpressionInterpreter && instruction.op1.optype == AddressType.ThisPtr)
                    core.Rmem.FramePointer = fp;
                rmem.Push(opdata1);
            }
            else
            {
                // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                runtimeVerify(ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype);

                runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);

                var dims = new List<StackValue>();

                for (int n = 0; n < dimensions; n++)
                {
                    dims.Add(rmem.Pop());
                }
                dims.Reverse();

                if (elementBasedUpdate)
                {
                    dims.AddRange(Properties.executingGraphNode.updateDimensions);
                }

                StackValue sv = GetIndexedArray(dims, blockId, instruction.op1, instruction.op2);
                rmem.Push(sv);
            }

            ++pc;
            return;
        }


        private void PUSHW_Handler(Instruction instruction)
        {
            int dimensions = 0;
            int blockId = DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.Pointer == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.FunctionPointer == instruction.op1.optype)
            {

                // TODO: Jun this is currently unused but required for stack alignment
                StackValue svType = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.StaticType == svType.optype);

                StackValue svDim = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                blockId = (int)svBlock.opdata;
            }

            int fp = core.Rmem.FramePointer;
            if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
                core.Rmem.FramePointer = core.watchFramePointer;

            if (0 == dimensions)
            {
                PushW(blockId, instruction.op1, instruction.op2);
            }
            else
            {
                // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                runtimeVerify(ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype);
                runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);
                StackValue sv = GetIndexedArrayW(dimensions, blockId, instruction.op1, instruction.op2);
                rmem.Push(sv);
            }

            if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
                core.Rmem.FramePointer = fp;

            ++pc;
            return;
        }

        private void PUSHINDEX_Handler(Instruction instruction)
        {
            if (instruction.op1.optype == AddressType.ArrayDim)
            {
                int dimensions = (int)instruction.op1.opdata;

                if (dimensions > 0)
                {
                    List<StackValue> dims = new List<StackValue>();
                    for (int i = 0; i < dimensions; ++i)
                    {
                        dims.Add(rmem.Pop());
                    }
                    dims.Reverse();

                    StackValue arrayPointer = rmem.Pop();
                    StackValue sv = GetIndexedArray(arrayPointer, dims);

                    GCRetain(sv);
                    GCRetain(arrayPointer);
                    GCRelease(arrayPointer);
                    DecRefCounter(sv);

                    rmem.Push(sv);
                }
            }
            else if (instruction.op1.optype == AddressType.ReplicationGuide)
            {
                int guides = (int)instruction.op1.opdata;

                List<ProtoCore.ReplicationGuide> argGuides = new List<ProtoCore.ReplicationGuide>();
                for (int i = 0; i < guides; ++i)
                {
                    StackValue svGuideProperty = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.Boolean == svGuideProperty.optype);
                    bool isLongest = (int)svGuideProperty.opdata == 1 ? true : false;

                    StackValue svGuide = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.Int == svGuide.optype);
                    int guideNumber = (int)svGuide.opdata;

                    argGuides.Add(new ProtoCore.ReplicationGuide(guideNumber, isLongest));
                }

                argGuides.Reverse();
                core.replicationGuides.Add(argGuides);
            }

            ++pc;
            return;
        }
        private void PUSHG_Handler(Instruction instruction)
        {
            if (core.Options.TempReplicationGuideEmptyFlag)
            {
                int dimensions = 0;
                int guides = 0;
                int blockId = DSASM.Constants.kInvalidIndex;
                if (ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.Pointer == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.FunctionPointer == instruction.op1.optype)
                {

                    // TODO: Jun this is currently unused but required for stack alignment
                    StackValue svType = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.StaticType == svType.optype);

                    StackValue svDim = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                    dimensions = (int)svDim.opdata;

                    StackValue svBlock = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                    blockId = (int)svBlock.opdata;

                }

                if (0 == dimensions)
                {
                    StackValue svNumGuides = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.ReplicationGuide == svNumGuides.optype);
                    guides = (int)svNumGuides.opdata;

                    List<ProtoCore.ReplicationGuide> argGuides = new List<ProtoCore.ReplicationGuide>();
                    for (int i = 0; i < guides; ++i)
                    {
                        StackValue svGuideProperty = rmem.Pop();
                        runtimeVerify(ProtoCore.DSASM.AddressType.Boolean == svGuideProperty.optype);
                        bool isLongest = (int)svGuideProperty.opdata == 1 ? true : false;

                        StackValue svGuide = rmem.Pop();
                        runtimeVerify(ProtoCore.DSASM.AddressType.Int == svGuide.optype);
                        int guideNumber = (int)svGuide.opdata;

                        argGuides.Add(new ProtoCore.ReplicationGuide(guideNumber, isLongest));
                    }

                    argGuides.Reverse();
                    core.replicationGuides.Add(argGuides);

                    StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                    rmem.Push(opdata1);
                }
                else
                {
                    // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                    runtimeVerify(ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                        || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                        || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype);

                    runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);

                    var dims = new List<StackValue>();
                    for (int n = 0; n < dimensions; ++n)
                    {
                        dims.Insert(0, rmem.Pop());
                    }

                    StackValue sv = GetIndexedArray(dims, blockId, instruction.op1, instruction.op2);


                    StackValue svNumGuides = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.ReplicationGuide == svNumGuides.optype);
                    guides = (int)svNumGuides.opdata;

                    rmem.Push(sv);
                }
            }
            else
            {
                int dimensions = 0;
                int guides = 0;
                int blockId = DSASM.Constants.kInvalidIndex;
                if (ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.Pointer == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype
                    || ProtoCore.DSASM.AddressType.FunctionPointer == instruction.op1.optype)
                {

                    // TODO: Jun this is currently unused but required for stack alignment
                    StackValue svType = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.StaticType == svType.optype);

                    StackValue svDim = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                    dimensions = (int)svDim.opdata;

                    StackValue svBlock = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                    blockId = (int)svBlock.opdata;

                    StackValue svNumGuides = rmem.Pop();
                    runtimeVerify(ProtoCore.DSASM.AddressType.ReplicationGuide == svNumGuides.optype);
                    guides = (int)svNumGuides.opdata;

                }

                if (0 == dimensions)
                {
                    List<ProtoCore.ReplicationGuide> argGuides = new List<ProtoCore.ReplicationGuide>();
                    for (int i = 0; i < guides; ++i)
                    {
                        StackValue svGuideProperty = rmem.Pop();
                        runtimeVerify(ProtoCore.DSASM.AddressType.Boolean == svGuideProperty.optype);
                        bool isLongest = (int)svGuideProperty.opdata == 1 ? true : false;

                        StackValue svGuide = rmem.Pop();
                        runtimeVerify(ProtoCore.DSASM.AddressType.Int == svGuide.optype);
                        int guideNumber = (int)svGuide.opdata;

                        argGuides.Add(new ProtoCore.ReplicationGuide(guideNumber, isLongest));
                    }

                    argGuides.Reverse();
                    core.replicationGuides.Add(argGuides);

                    StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                    rmem.Push(opdata1);
                }
                else
                {
                    // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                    runtimeVerify(ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                        || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype
                        || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype);

                    runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);

                    var dims = new List<StackValue>();
                    for (int n = 0; n < dimensions; ++n)
                    {
                        dims.Insert(0, rmem.Pop());
                    }

                    StackValue sv = GetIndexedArray(dims, blockId, instruction.op1, instruction.op2);

                    rmem.Push(sv);
                }
            }

            ++pc;
            return;
        }

        private void PUSHB_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PushConstructBlockId((int)instruction.op1.opdata);
            }
            ++pc;
            return;
        }

        private void POPB_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PopConstructBlockId();
            }
            ++pc;
            return;
        }

        private void PUSHM_Handler(Instruction instruction)
        {
            int blockId = (int)instruction.op3.opdata;

            if (instruction.op1.optype == AddressType.StaticMemVarIndex)
            {
                rmem.Push(StackValue.BuildBlockIndex(blockId));
                rmem.Push(instruction.op1);
            }
            else if (instruction.op1.optype == AddressType.ClassIndex)
            {
                rmem.Push(StackValue.BuildClassIndex((int)instruction.op1.opdata));
            }
            else
            {
                StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                rmem.Push(opdata1);
            }

            ++pc;
            return;
        }
        private void PUSHLIST_Handler(Instruction instruction)
        {
            bool isDotFunctionBody = false;
            if (instruction.op1.optype == AddressType.Dynamic)
            {
                isDotFunctionBody = true;
            }
            else
            {
                runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op1.optype);
            }
            int depth = (int)instruction.op1.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);
            int scope = (int)instruction.op2.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == instruction.op3.optype);
            int blockId = (int)instruction.op3.opdata;

            StackValue sv = GetFinalPointer(depth, isDotFunctionBody);
            rmem.Push(sv);

            ++pc;
            return;
        }

        private void PUSH_VARSIZE_Handler(Instruction instruction)
        {
            // TODO Jun: This is a temporary solution to retrieving the array size until lib files are implemented
            runtimeVerify(ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype);
            int symbolIndex = (int)instruction.op1.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == instruction.op2.optype);
            int blockId = (int)instruction.op2.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op3.optype);
            int classIndex = (int)instruction.op3.opdata;

            SymbolNode snode = GetSymbolNode(blockId, classIndex, symbolIndex);
            runtimeVerify(null != snode);

            int stackindex = rmem.GetStackIndex(snode);
            StackValue array = rmem.GetAtRelative(snode);

            if (AddressType.ArrayPointer != array.optype && snode.datatype.IsIndexable)
            {
                array = core.Heap.Heaplist[(int)array.opdata].Stack[0];
            }

            StackValue key = StackValue.Null;
            HeapElement he = ArrayUtils.GetHeapElement(array, core);
            if (he != null)
            {
                if (he.VisibleSize > 0 || (he.Dict != null && he.Dict.Count > 0))
                {
                    key = StackValue.BuildArrayKey(0, (int)array.opdata);
                }
            }
            else if (!StackUtils.IsNull(array))
            {
                key = StackValue.BuildArrayKey(Constants.kInvalidIndex, Constants.kInvalidIndex);
            }
            rmem.Push(key);

            ++pc;
            return;
        }

        protected StackValue POP_helper(Instruction instruction, out int blockId, out int dimensions)
        {
            dimensions = 0;
            blockId = DSASM.Constants.kInvalidIndex;
            int staticType = (int)ProtoCore.PrimitiveType.kTypeVar;
            int rank = ProtoCore.DSASM.Constants.kUndefinedRank;
            bool objectIndexing = false;
            if (AddressType.VarIndex == instruction.op1.optype
                || AddressType.Pointer == instruction.op1.optype
                || AddressType.ArrayPointer == instruction.op1.optype)
            {

                StackValue svType = rmem.Pop();
                runtimeVerify(AddressType.StaticType == svType.optype);
                staticType = (int)svType.metaData.type;
                rank = (int)svType.opdata;

                StackValue svDim = rmem.Pop();
                runtimeVerify(AddressType.ArrayDim == svDim.optype);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(AddressType.BlockIndex == svBlock.optype);
                blockId = (int)svBlock.opdata;

                objectIndexing = true;
                // TODO(Jun/Jiong): Find a more reliable way to update the current block Id
                //  eg: [Imperative] { a = 10; if (a > 10) c = a; else c = 10; m = a; } 
                //  when the execution cursor is at "m = a;", the user should not be allowed to inspect the value of c 
                //  because it is in the inner scope, 3, of the current block, 1. 
                //  for now, since the pop instruction in "m = a" has not been executed, the currentBlockId has not been updated 
                //  from 3 to 1, the user will be able to inspect the value of c 
                //core.DebugProps.CurrentBlockId = blockId;
            }

            bool isSSANode = Properties.executingGraphNode != null && Properties.executingGraphNode.IsSSANode();
            StackValue svData;

            // The returned stackvalue is used by watch test framework - pratapa
            StackValue tempSvData = StackValue.Null;

            bool elementBasedUpdate = core.Options.ElementBasedArrayUpdate
                                    && Properties.executingGraphNode != null
                                    && Properties.executingGraphNode.updateDimensions.Count > 0;
            if (0 == dimensions && !elementBasedUpdate || !objectIndexing)
            {
                runtimeVerify(AddressType.ClassIndex == instruction.op2.optype);

                svData = rmem.Pop();
                StackValue coercedValue;

                if (isSSANode)
                {
                    coercedValue = svData;
                    // Double check to avoid the case like
                    //    %tvar = obj;
                    //    %tSSA = %tvar;
                    blockId = core.RunningBlock;
                    string symbol = core.DSExecutable.runtimeSymbols[blockId].symbolList[(int)instruction.op1.opdata].name;

                    //if (!CoreUtils.IsSSATemp(symbol))
                    {
                        GCRetain(coercedValue);
                    }
                }
                else
                {
                    coercedValue = TypeSystem.Coerce(svData, staticType, rank, core);
                    GCRetain(coercedValue);
                }

                FX = coercedValue;
                tempSvData = coercedValue;
                EX = PopTo(blockId, instruction.op1, instruction.op2, coercedValue);

                if (core.Options.ExecuteSSA)
                {
                    if (!isSSANode)
                    {
                        if (EX.optype == AddressType.Pointer && coercedValue.optype == AddressType.Pointer)
                        {
                            if (EX.opdata != coercedValue.opdata)
                            {
                                if (null != Properties.executingGraphNode)
                                {
                                    Properties.executingGraphNode.reExecuteExpression = true;
                                }
                            }
                        }
                    }
                }

                //if (!isSSANode && instruction.op1.optype != AddressType.Register)
                if (instruction.op1.optype != AddressType.Register)
                {
                    GCRelease(EX);
                }

            }
            else
            {
                runtimeVerify(AddressType.VarIndex == instruction.op1.optype);

                List<StackValue> dimList = new List<StackValue>();

#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
                List<int> indexIntoList = new List<int>();
                for (int i = 0; i < dimensions; ++i)
                {
                    StackValue svIndex = rmem.Pop();
                    dimList.Add(svIndex);
                    indexIntoList.Add((int)svIndex.opdata);
                }
                indexIntoList.Reverse();
                dimList.Reverse();

#else
                for (int i = 0; i < dimensions; ++i)
                {
                    dimList.Add(rmem.Pop());
                }
                dimList.Reverse();
#endif

                // Get the original value of variable. Test framework will add
                // svData below too a map and do comparsion. But for element
                // based array update, svData is just the value of element, not
                // the whole array, so we have to get the original value of 
                // array. 
                List<StackValue> partialDimList = new List<StackValue>(dimList);
                if (elementBasedUpdate)
                {
                    dimList.AddRange(Properties.executingGraphNode.updateDimensions);
                }


                // Comment Jun: Store the indices into the symbol into the map                
                ProtoCore.DSASM.SymbolNode symbol = core.DSExecutable.runtimeSymbols[blockId].symbolList[(int)instruction.op1.opdata];

#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
                ProtoCore.AssociativeEngine.ArrayUpdate.UpdateSymbolArrayIndex(symbol.name, indexIntoList, symbolArrayIndexMap);
#endif


                svData = rmem.Pop();
                FX = svData;
                tempSvData = svData;
                EX = PopToIndexedArray(blockId, (int)instruction.op1.opdata, (int)instruction.op2.opdata, dimList, svData);

                if (elementBasedUpdate)
                {
                    if (partialDimList.Count == 0)  // array promotion
                    {
                        tempSvData = GetOperandData(blockId,
                                                    instruction.op1,
                                                    instruction.op2);
                    }
                    else
                    {
                        tempSvData = GetIndexedArray(partialDimList,
                                                     blockId,
                                                     instruction.op1,
                                                     instruction.op2);
                    }
                }

                if (instruction.op1.optype != AddressType.Register)
                {
                    GCRelease(EX);
                }
            }

            if (!isSSANode && rmem.Heap.IsTemporaryPointer(svData))
            {
                GCRelease(svData);
            }

            ++pc;
            return tempSvData;
        }

        protected virtual void POP_Handler(Instruction instruction)
        {
            int blockId = DSASM.Constants.kInvalidIndex;
            int dimensions = 0;
            POP_helper(instruction, out blockId, out dimensions);
        }

        private void POPW_Handler(Instruction instruction)
        {
            int dimensions = 0;
            int blockId = DSASM.Constants.kInvalidIndex;
            int staticType = (int)ProtoCore.PrimitiveType.kTypeVar;
            int rank = ProtoCore.DSASM.Constants.kUndefinedRank;
            if (ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.Pointer == instruction.op1.optype
                || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op1.optype)
            {

                StackValue svType = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.StaticType == svType.optype);
                staticType = (int)svType.metaData.type;
                rank = (int)svType.opdata;

                StackValue svDim = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                blockId = (int)svBlock.opdata;


                // TODO(Jun/Jiong): Find a more reliable way to update the current block Id
                //  eg: [Imperative] { a = 10; if (a > 10) c = a; else c = 10; m = a; } 
                //  when the execution cursor is at "m = a;", the user should not be allowed to inspect the value of c 
                //  because it is in the inner scope, 3, of the current block, 1. 
                //  for now, since the pop instruction in "m = a" has not been executed, the currentBlockId has not been updated 
                //  from 3 to 1, the user will be able to inspect the value of c 
                //core.DebugProps.CurrentBlockId = blockId;
            }

            StackValue svData;
            if (0 == dimensions)
            {
                runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);

                svData = rmem.Pop();
                StackValue coercedValue = TypeSystem.Coerce(svData, staticType, rank, core);
                GCRetain(coercedValue);
                FX = coercedValue;

                PopToW(blockId, instruction.op1, instruction.op2, coercedValue);
            }
            else
            {
                runtimeVerify(ProtoCore.DSASM.AddressType.VarIndex == instruction.op1.optype);

                List<StackValue> dimList = new List<StackValue>();
                for (int i = 0; i < dimensions; ++i)
                {
                    dimList.Insert(0, rmem.Pop());
                }

                svData = rmem.Pop();
                FX = svData;
                EX = PopToIndexedArray(blockId, (int)instruction.op1.opdata, (int)instruction.op2.opdata, dimList, svData);
                if (instruction.op1.optype != AddressType.Register)
                {
                    GCRelease(EX);
                }
            }

            if (rmem.Heap.IsTemporaryPointer(svData))
            {
                GCRelease(svData);
            }
            ++pc;
            return;
        }

        private void POPG_Handler(Instruction instruction)
        {
            StackValue svNumGuides = rmem.Pop();
            runtimeVerify(ProtoCore.DSASM.AddressType.ReplicationGuide == svNumGuides.optype);
            int guides = (int)svNumGuides.opdata;

            List<ProtoCore.ReplicationGuide> argGuides = new List<ProtoCore.ReplicationGuide>();
            for (int i = 0; i < guides; ++i)
            {
                StackValue svGuideProperty = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.Boolean == svGuideProperty.optype);
                bool isLongest = (int)svGuideProperty.opdata == 1 ? true : false;

                StackValue svGuide = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.Int == svGuide.optype);
                int guideNumber = (int)svGuide.opdata;

                argGuides.Add(new ProtoCore.ReplicationGuide(guideNumber, isLongest));
            }

            argGuides.Reverse();
            core.replicationGuides.Add(argGuides);

            ++pc;
            return;
        }

        protected StackValue POPM_Helper(Instruction instruction, out int blockId, out int classIndex)
        {
            classIndex = Constants.kInvalidIndex;

            runtimeVerify(ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype ||
                          ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype);

            StackValue svType = rmem.Pop();
            runtimeVerify(ProtoCore.DSASM.AddressType.StaticType == svType.optype);
            int staticType = (int)svType.metaData.type;
            int rank = (int)svType.opdata;

            StackValue svDim = rmem.Pop();
            runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
            int dimensions = (int)svDim.opdata;

            StackValue svBlock = rmem.Pop();
            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
            blockId = (int)svBlock.opdata;

            List<StackValue> dimList = new List<StackValue>();
            for (int i = 0; i < dimensions; ++i)
            {
                dimList.Insert(0, rmem.Pop());
            }

            bool isSSANode = Properties.executingGraphNode != null && Properties.executingGraphNode.IsSSANode();
            StackValue svData = rmem.Pop();

            // The returned stackvalue is used by watch test framework - pratapa
            StackValue tempSvData = svData;

            svData.metaData.type = core.TypeSystem.GetType(svData);

            // TODO(Jun/Jiong): Find a more reliable way to update the current block Id
            //core.DebugProps.CurrentBlockId = blockId;

            if (ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype)
            {
                FX = svData;

                if (0 == dimensions)
                {
                    StackValue coercedValue = TypeSystem.Coerce(svData, staticType, rank, core);
                    FX = coercedValue;

                    tempSvData = coercedValue;

                    EX = PopTo(blockId, instruction.op1, instruction.op2, coercedValue);
                    GCRelease(EX);
                }
                else
                {
                    EX = PopToIndexedArray(blockId, (int)instruction.op1.opdata, Constants.kGlobalScope, dimList, svData);
                    GCRelease(EX);
                }

                if (rmem.Heap.IsTemporaryPointer(svData))
                {
                    GCRelease(svData);
                }

                ++pc;
                return tempSvData;
            }

            int symbolIndex = (int)instruction.op1.opdata;
            classIndex = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            int stackIndex = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symbolIndex].index;

            //==================================================
            //  1. If allocated... bypass auto allocation
            //  2. If pointing to a class, just point to the class directly, do not allocate a new pointer
            //==================================================

            StackValue svThis = rmem.GetAtRelative(rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr));
            runtimeVerify(AddressType.Pointer == svThis.optype);
            int thisptr = (int)svThis.opdata;
            StackValue svProperty = core.Heap.Heaplist[thisptr].Stack[stackIndex];

            StackValue svOldData = svData;
            Type targetType = new Type { UID = (int)PrimitiveType.kTypeVar, rank = Constants.kArbitraryRank, IsIndexable = true };
            if (staticType != (int)PrimitiveType.kTypeFunctionPointer)
            {
                if (dimensions == 0)
                {
                    StackValue coercedType = TypeSystem.Coerce(svData, staticType, rank, core);
                    GCRetain(coercedType);
                    svData = coercedType;
                }
                else
                {
                    SymbolNode symbolnode = GetSymbolNode(blockId, classIndex, symbolIndex);
                    targetType = symbolnode.staticType;
                    if (targetType.rank < 0)
                    {
                        targetType.rank = Constants.kArbitraryRank;
                        targetType.IsIndexable = true;
                    }

                    if (svProperty.optype == AddressType.ArrayPointer)
                    {
                        if (targetType.UID != (int)PrimitiveType.kTypeVar || targetType.rank >= 0)
                        {
                            int lhsRepCount = 0;
                            foreach (var dim in dimList)
                            {
                                if (dim.optype == AddressType.ArrayPointer)
                                {
                                    lhsRepCount++;
                                }
                            }

                            if (targetType.rank > 0)
                            {
                                targetType.rank = targetType.rank - dimList.Count;
                                targetType.rank += lhsRepCount;

                                if (targetType.rank > 0)
                                {
                                    targetType.IsIndexable = true;
                                }
                                else if (targetType.rank == 0)
                                {
                                    targetType.IsIndexable = false;
                                }
                                else if (targetType.rank < 0)
                                {
                                    string message = String.Format(RuntimeData.WarningMessage.kSymbolOverIndexed, symbolnode.name);
                                    core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kOverIndexing, message);
                                }
                            }

                        }
                    }
                }
            }

            if (AddressType.Pointer == svProperty.optype || (AddressType.ArrayPointer == svProperty.optype && dimensions == 0))
            {
                // The data to assign is already a pointer
                if (AddressType.Pointer == svData.optype || AddressType.ArrayPointer == svData.optype)
                {
                    // Assign the src pointer directily to this property
                    lock (core.Heap.cslock)
                    {
                        // TODO Jun: Implement gcUpdate here
                        GCRelease(svProperty);
                        core.Heap.Heaplist[thisptr].Stack[stackIndex] = svData;
                    }
                }
                else
                {
                    GCRelease(svProperty);

                    lock (core.Heap.cslock)
                    {
                        int ptr = core.Heap.Allocate(DSASM.Constants.kPointerSize);
                        core.Heap.Heaplist[ptr].Stack[0] = svData;

                        StackValue svNewProperty = StackValue.BuildPointer(ptr);
                        core.Heap.Heaplist[thisptr].Stack[stackIndex] = svNewProperty;
                        GCRetain(svNewProperty);

                        exe.classTable.ClassNodes[classIndex].symbols.symbolList[stackIndex].heapIndex = ptr;
                    }
                }
            }
            else if ((AddressType.ArrayPointer == svProperty.optype) && (dimensions > 0))
            {
                lock (core.Heap.cslock)
                {
                    FX = svData;
                    EX = ArrayUtils.SetValueForIndices(svProperty, dimList, svData, targetType, core);
                    GCRelease(EX);
                }
            }
            else // This property has NOT been allocated
            {
                if (AddressType.Pointer == svData.optype || AddressType.ArrayPointer == svData.optype)
                {
                    lock (core.Heap.cslock)
                    {
                        core.Heap.Heaplist[thisptr].Stack[stackIndex] = svData;
                    }
                }
                else
                {
                    lock (core.Heap.cslock)
                    {
                        int ptr = core.Heap.Allocate(DSASM.Constants.kPointerSize);
                        core.Heap.Heaplist[ptr].Stack[0] = svData;

                        StackValue svNewProperty = StackValue.BuildPointer(ptr);
                        core.Heap.Heaplist[thisptr].Stack[stackIndex] = svNewProperty;
                        GCRetain(svNewProperty);

                        exe.classTable.ClassNodes[classIndex].symbols.symbolList[stackIndex].heapIndex = ptr;
                    }
                }
            }

            if (!isSSANode && rmem.Heap.IsTemporaryPointer(svOldData))
            {
                GCRelease(svOldData);
            }

            ++pc;

#if SUPPORT_DS_PROPERTYCHANGED_EVENT
            SymbolNode propertySymbol = GetSymbolNode(blockId, classIndex, symbolIndex);
            ProtoFFI.FFIPropertyChangedMonitor.GetInstance().DSObjectPropertyChanged(this, thisptr, propertySymbol.name, null);
#endif
            return svData;
        }

        protected virtual void POPM_Handler(Instruction instruction)
        {
#if USE_DEPRECATED_POPM
                        runtimeVerify(ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op1.optype);
                        
                        StackValue svDim = rmem.Pop();
                        runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                        int dimensions = (int)svDim.opdata;

                        StackValue svBlock = rmem.Pop();
                        runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                        int blockId = (int)svBlock.opdata;
                        
                        int symbol = (int)instruction.op1.opdata;
                        int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;

                      
                        //==================================================
                        //  1. If allocated... bypass auto allocation
                        //  2. If pointing to a class, just point to the class directly, do not allocate a new pointer
                        //==================================================

                        int thisptr = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr).opdata;


                        StackValue data = rmem.Pop();
                        if (AddressType.Invalid != core.heap.heaplist[thisptr].stack[symbol].optype)
                        {
                            // This property already points to a heap index
                            if (AddressType.Pointer == data.optype)
                            {
                                // Assign the src pointer directily to this property
                                lock (core.heap.cslock)
                                {
                                    core.heap.heaplist[thisptr].stack[symbol] = data;
                                }
                            }
                            else
                            {
                                // Modify the data that it points to
                                StackValue valPtr = core.heap.heaplist[thisptr].stack[symbol];
                                runtimeVerify(AddressType.Pointer == valPtr.optype);
                                core.heap.heaplist[(int)valPtr.opdata].stack[0] = data;
                            }
                        }
                        else
                        {
                            // This property has NOT been allocated
                            if (AddressType.Pointer == data.optype || AddressType.ArrayPointer == data.optype)
                            {
                                // Assign the src pointer directily to this property
                                lock (core.heap.cslock)
                                {
                                    core.heap.heaplist[thisptr].stack[symbol] = data;
                                }
                            }
                            else
                            {
                                // Allocate a pointer for this property
                                int ptr = ProtoCore.DSASM.Constants.kInvalidPointer;
                                lock (core.heap.cslock)
                                {
                                    ptr = core.heap.Allocate(DSASM.Constants.kPointerSize);

                                    MetaData mdata;
                                    mdata.type = ci;
                                    data.metaData = mdata;
                                    core.heap.heaplist[ptr].stack[0] = data;

                                    core.heap.heaplist[thisptr].stack[symbol] = StackValue.BuildPointer(ptr);
                                }
                                exe.classTable.list[ci].symbols.symbolList[symbol].heapIndex = ptr;
                            }
                        }
                    
                        ++pc;  setPC(pc);
                        return;
#else
            int blockId;
            int ci;
            POPM_Helper(instruction, out blockId, out ci);
#endif
        }

        private void POPLIST_Handler(Instruction instruction)
        {
            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op1.optype);
            int depth = (int)instruction.op1.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op2.optype);
            int scope = (int)instruction.op2.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == instruction.op3.optype);
            int blockId = (int)instruction.op3.opdata;
            // TODO(Jun/Jiong): Find a more reliable way to update the current block Id
            //core.DebugProps.CurrentBlockId = blockId;
            DSASM.RTSymbol[] listInfo = new RTSymbol[depth];
            for (int n = 0; n < depth; ++n)
            {
                listInfo[n].Sv = rmem.Pop();
                if (listInfo[n].Sv.optype == AddressType.StaticMemVarIndex)
                {
                    StackValue block = rmem.Pop();
                    Validity.Assert(block.optype == AddressType.BlockIndex);
                    listInfo[n].BlockId = (int)block.opdata;
                }
                int dim = (int)rmem.Pop().opdata;
                if (dim == 0)
                    listInfo[n].Dimlist = null;
                else
                    listInfo[n].Dimlist = new int[dim];

                for (int d = 0; d < dim; ++d)
                {
                    listInfo[n].Dimlist[d] = (int)rmem.Pop().opdata;
                }
            }

            // Handle depth until one before the last pointer
            StackValue finalPointer = StackValue.Null; 
            int classsccope = (int)listInfo.Last().Sv.metaData.type;
            for (int n = listInfo.Length - 1; n >= 1; --n)
            {
                if (n == listInfo.Length - 1)
                    finalPointer = listInfo[n].Sv;
                else
                {
                    //resolve dynamic reference
                    if (listInfo[n].Sv.optype == AddressType.Dynamic)
                    {
                        classsccope = (int)listInfo[n + 1].Sv.metaData.type;
                        bool succeeded = ProcessDynamicVariable((listInfo[n].Dimlist != null), ref listInfo[n].Sv, classsccope);
                        //if the identifier is unbounded. Push null
                        if (!succeeded)
                        {
                            finalPointer = StackValue.Null;
                            break;
                        }
                    }

                    if (listInfo[n].Sv.optype == AddressType.StaticMemVarIndex)
                        finalPointer = listInfo[n].Sv = GetOperandData(blockId, listInfo[n].Sv, new StackValue());
                    else
                        finalPointer = listInfo[n].Sv = core.Heap.Heaplist[(int)finalPointer.opdata].Stack[(int)listInfo[n].Sv.opdata];
                }
                if (listInfo[n].Dimlist != null)
                {
                    for (int d = listInfo[n].Dimlist.Length - 1; d >= 0; --d)
                    {
                        finalPointer = listInfo[n].Sv = core.Heap.Heaplist[(int)finalPointer.opdata].GetValue(listInfo[n].Dimlist[d], core);
                    }
                }
            }

            // Handle the last pointer
            StackValue tryPointer = StackValue.Null;
            StackValue data = rmem.Pop();
            GCRetain(data);
            if (finalPointer.optype != AddressType.Null)
            {
                if (listInfo[0].Sv.optype == AddressType.Dynamic)
                {
                    classsccope = (int)listInfo[1].Sv.metaData.type;
                    bool succeeded = ProcessDynamicVariable((listInfo[0].Dimlist != null), ref listInfo[0].Sv, classsccope);
                    //if the identifier is unbounded. Push null
                    if (!succeeded)
                    {
                        tryPointer = StackValue.Null;
                    }
                    else
                    {
                        if (listInfo[0].Sv.optype == AddressType.StaticMemVarIndex)
                        {
                            SetOperandData(listInfo[0].Sv, data, listInfo[0].BlockId);
                            ++pc;
                            return;
                        }
                        tryPointer = core.Heap.Heaplist[(int)finalPointer.opdata].Stack[listInfo[0].Sv.opdata];
                    }
                }
                else if (listInfo[0].Sv.optype == AddressType.StaticMemVarIndex)
                {
                    SetOperandData(listInfo[0].Sv, data, listInfo[0].BlockId);
                    ++pc;
                    return;
                }
                else
                {
                    tryPointer = core.Heap.Heaplist[(int)finalPointer.opdata].Stack[listInfo[0].Sv.opdata];
                }
            }
            else
            {
                tryPointer = StackValue.Null;
            }

            if (listInfo[0].Dimlist != null)
            {
                finalPointer = tryPointer;
                for (int d = listInfo[0].Dimlist.Length - 1; d >= 1; --d)
                    finalPointer = core.Heap.Heaplist[(int)finalPointer.opdata].GetValue(listInfo[0].Dimlist[d], core);
                tryPointer = core.Heap.Heaplist[(int)finalPointer.opdata].GetValue(listInfo[0].Dimlist[0], core);
            }

            if (tryPointer.optype == AddressType.Null)
            { //do nothing
            }
            else if (core.Heap.Heaplist[(int)tryPointer.opdata].Stack.Length == 1 &&
                core.Heap.Heaplist[(int)tryPointer.opdata].Stack[0].optype != AddressType.Pointer &&
                core.Heap.Heaplist[(int)tryPointer.opdata].Stack[0].optype != AddressType.ArrayPointer)
            {
                // TODO Jun:
                // Spawn GC here

                lock (core.Heap.cslock)
                {
                    // Setting a primitive
                    DX = core.Heap.Heaplist[(int)tryPointer.opdata].Stack[0];
                    core.Heap.Heaplist[(int)tryPointer.opdata].Stack[0] = data;
                    // GCPointer(DX); No need to spawn GC here, the data been replaced is not a pointer or array
                }
            }
            else if (finalPointer.optype == AddressType.Pointer || data.optype == AddressType.Null)
            {
                if (data.optype == AddressType.Null)
                {
                    int ptr = core.Heap.Allocate(1);
                    lock (core.Heap.cslock)
                    {
                        core.Heap.Heaplist[ptr].Stack = new[] { data };
                        data = StackValue.BuildPointer(ptr);
                        // TODO Jun/Jiong,  write test case for this
                        GCRetain(data);
                    }
                }
                lock (core.Heap.cslock)
                {
                    // Setting a pointer
                    int idx = (int)listInfo[0].Sv.opdata;
                    DX = ArrayUtils.GetValueFromIndex(finalPointer, idx, core);
                    GCRelease(DX);
                    core.Heap.Heaplist[(int)finalPointer.opdata].Stack[listInfo[0].Sv.opdata] = data;
                }
            }
            else
            {
                // TODO Jun:
                // Spawn GC here
                runtimeVerify(finalPointer.optype == AddressType.ArrayPointer);
                lock (core.Heap.cslock)
                {
                    // Setting an array
                    DX = core.Heap.Heaplist[(int)finalPointer.opdata].GetValue(listInfo[0].Dimlist[0], core);
                    GCRelease(DX);
                    core.Heap.Heaplist[(int)finalPointer.opdata].SetValue(listInfo[0].Dimlist[0], data);
                }
            }

            //List<int> pointerList = new List<int>();
            //for (int n = 0; n < depth; ++n)
            //{
            //    StackValue sv = rmem.Pop();
            //    runtimeVerify(ProtoCore.DSASM.AddressType.Pointer == sv.optype);
            //    pointerList.Add((int)sv.opdata);

            //    // TODO Jun: Support popping to a list of indexed arrays
            //    // a.b[0].c[1] = 10;
            //    StackValue svDim = rmem.Pop();
            //    runtimeVerify(AddressType.ArrayDim == svDim.optype);
            //    int dimensions = (int)svDim.opdata;
            //    for (int i = 0; i < dimensions; ++i)
            //    {
            //        Validity.Assert(false, "To implement: Indexing into function calls.");
            //    }
            //}

            //pointerList.Reverse();

            //if (ProtoCore.DSASM.Constants.kGlobalScope == scope)
            //{
            //    StackValue data = rmem.Pop();

            //    int n = 0;
            //    int ptr = pointerList[0];
            //    for (n = 1; n < pointerList.Count - 1; ++n)
            //    {
            //        ptr = (int)core.heap.heaplist[ptr].stack[pointerList[n]].opdata;
            //    }

            //    // If the src data is a pointer then make the last variable in the resolution op point to that.
            //    // Otherwise move to the pointed location and assign the source data (a primitive)
            //    if (AddressType.Pointer == data.optype)
            //    {
            //        // TODO Jun:
            //        // Spawn GC here
            //        lock (core.heap.cslock)
            //        {
            //            core.heap.heaplist[ptr].stack[pointerList[n - 1]] = data;
            //        }
            //    }
            //    else
            //    {
            //        ptr = (int)core.heap.heaplist[ptr].stack[pointerList[n]].opdata;
            //        lock (core.heap.cslock)
            //        {
            //            core.heap.heaplist[ptr].stack[0] = data;
            //        }
            //    }
            //}

            ++pc;
            return;
        }
        private void MOV_Handler(Instruction instruction)
        {
            StackValue opClass = StackValue.BuildClassIndex(Constants.kGlobalScope);

            int dimensions = 0;
            int blockId = DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.AddressType.VarIndex == instruction.op2.optype
                || ProtoCore.DSASM.AddressType.MemVarIndex == instruction.op2.optype
                || ProtoCore.DSASM.AddressType.Pointer == instruction.op2.optype
                || ProtoCore.DSASM.AddressType.ArrayPointer == instruction.op2.optype)
            {
                StackValue svDim = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.ArrayDim == svDim.optype);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == svBlock.optype);
                blockId = (int)svBlock.opdata;
            }

            StackValue opdata1 = GetOperandData(blockId, instruction.op2, opClass);
            SetOperandData(instruction.op1, opdata1);

            ++pc;
            return;
        }

        private void ADD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            // Need to optmize these if-elses to a table. 
            if ((AddressType.Int == opdata1.optype) && (AddressType.Int == opdata2.optype))
            {
                opdata2 = StackValue.BuildInt(opdata1.opdata + opdata2.opdata);

            }
            else if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                opdata2 = StackValue.BuildDouble(opdata1.opdata_d + opdata2.opdata_d);
            }
            else if ((StackUtils.IsString(opdata1) && (AddressType.Char == opdata2.optype || StackUtils.IsString(opdata2))) ||
                     (StackUtils.IsString(opdata2) && (AddressType.Char == opdata1.optype || StackUtils.IsString(opdata1))))
            {
                opdata2 = StringUtils.ConcatString(opdata2, opdata1, rmem);
            }
            else if (((AddressType.Char == opdata1.optype) && (AddressType.Char == opdata2.optype || StackUtils.IsString(opdata2))) || ((AddressType.Char == opdata2.optype) && (AddressType.Char == opdata1.optype || StackUtils.IsString(opdata1))))
            {
                if (opdata1.optype == AddressType.Char)
                {
                    (ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(opdata1.opdata)).ToString();
                }
                if (opdata2.optype == AddressType.Char)
                {
                    (ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(opdata2.opdata)).ToString();
                }
                opdata2 = StringUtils.ConcatString(opdata2, opdata1, rmem);
            }
            else if ((StackUtils.IsString(opdata1)) || (StackUtils.IsString(opdata2)))
            {
                StackValue newSV;
                if (StackUtils.IsString(opdata1))
                {
                    newSV = StringUtils.ConvertToString(opdata2, core, rmem);
                    opdata2 = StringUtils.ConcatString(newSV, opdata1, rmem);
                }
                else if (StackUtils.IsString(opdata2))
                {
                    newSV = StringUtils.ConvertToString(opdata1, core, rmem);
                    opdata2 = StringUtils.ConcatString(opdata2, newSV, rmem);
                }
                else
                {
                    opdata2 = StringUtils.ConcatString(opdata2, opdata1, rmem);
                }
            }
            else if (opdata2.optype == AddressType.ArrayKey && opdata1.optype == AddressType.Int)
            {
                if (opdata1.opdata == 1)
                {
                    opdata2 = ArrayUtils.GetNextKey(opdata2, core);
                }
                else
                {
                    opdata2 = StackValue.Null;
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void ADDD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if ((AddressType.Int == opdata1.optype) && (AddressType.Int == opdata2.optype))
            {
                opdata2 = StackValue.BuildInt(opdata1.opdata + opdata2.opdata);
            }
            else if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                opdata2 = StackValue.BuildDouble(opdata1.opdata_d + opdata2.opdata_d);
            }
            else
            {
                opdata2.opdata_d = opdata2.opdata = 0;
                opdata2.optype = AddressType.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void SUB_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if ((AddressType.Int == opdata1.optype) && (AddressType.Int == opdata2.optype))
            {
                opdata2 = StackValue.BuildInt(opdata2.opdata - opdata1.opdata);
            }
            else if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                opdata2 = StackValue.BuildDouble(opdata2.opdata_d - opdata1.opdata_d);
            }
            else
            {
                opdata1 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void SUBD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if ((AddressType.Int == opdata1.optype) && (AddressType.Int == opdata2.optype))
            {
                opdata2 = StackValue.BuildInt(opdata2.opdata - opdata1.opdata);
            }
            else if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                opdata2 = StackValue.BuildDouble(opdata2.opdata_d - opdata1.opdata_d);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void MUL_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if ((AddressType.Int == opdata1.optype) && (AddressType.Int == opdata2.optype))
            {
                opdata2 = StackValue.BuildInt(opdata1.opdata * opdata2.opdata);
            }
            else if (((AddressType.Int == opdata1.optype) || (AddressType.Double == opdata1.optype)) &&
                     ((AddressType.Int == opdata2.optype) || (AddressType.Double == opdata2.optype)))
            {
                opdata2 = StackValue.BuildDouble(opdata1.opdata_d * opdata2.opdata_d);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void MULD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if ((AddressType.Int == opdata1.optype) && (AddressType.Int == opdata2.optype))
            {
                opdata2 = StackValue.BuildInt(opdata1.opdata * opdata2.opdata);
            }
            else if (((AddressType.Int == opdata1.optype) || (AddressType.Double == opdata1.optype)) &&
                     ((AddressType.Int == opdata2.optype) || (AddressType.Double == opdata2.optype)))
            {
                opdata2 = StackValue.BuildDouble(opdata1.opdata_d * opdata2.opdata_d);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void DIV_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            //division is always carried out as a double
            if (((AddressType.Int == opdata1.optype) || (AddressType.Double == opdata1.optype)) &&
                ((AddressType.Int == opdata2.optype) || (AddressType.Double == opdata2.optype)))
            {
                opdata2 = StackValue.BuildDouble(opdata2.opdata_d / opdata1.opdata_d);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void DIVD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (((AddressType.Int == opdata1.optype) || (AddressType.Double == opdata1.optype)) &&
                ((AddressType.Int == opdata2.optype) || (AddressType.Double == opdata2.optype)))
            {
                opdata2 = StackValue.BuildDouble(opdata2.opdata_d / opdata1.opdata_d);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void MOD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if ((opdata1.optype == AddressType.Int) && (opdata2.optype == AddressType.Int))
            {
                opdata2 = StackValue.BuildInt(opdata2.opdata % opdata1.opdata);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void BITAND_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.optype == AddressType.Int && opdata2.optype == AddressType.Int)
            {
                opdata2.opdata &= opdata1.opdata;
                opdata2.opdata_d = opdata2.opdata;
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }

        private void BITOR_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.optype == AddressType.Int && opdata2.optype == AddressType.Int)
            {
                opdata2.opdata |= opdata1.opdata;
                opdata2.opdata_d = opdata2.opdata;
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void BITXOR_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.optype == AddressType.Int && opdata2.optype == AddressType.Int)
            {
                opdata2.opdata ^= opdata1.opdata;
                opdata2.opdata_d = opdata2.opdata;
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void NEGATE_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            if (opdata1.optype == AddressType.Int)
            {
                opdata1.opdata = ~opdata1.opdata;
                opdata1.opdata_d = opdata1.opdata;
            }
            else
            {
                opdata1 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata1);

            ++pc;
            return;
        }
        private void NEG_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            opdata1.opdata = -opdata1.opdata;
            opdata1.opdata_d = -opdata1.opdata_d;

            SetOperandData(instruction.op1, opdata1);
            ++pc;
            return;
        }
        private void AND_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata1 = opdata1.AsBoolean(core);
            opdata2 = opdata2.AsBoolean(core);
            if (opdata1.optype == AddressType.Null || opdata2.optype == AddressType.Null)
            {
                opdata2 = StackValue.Null;
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata2.opdata != 0L && opdata1.opdata != 0L);
            }
            //Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void OR_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata1 = opdata1.AsBoolean(core);
            opdata2 = opdata2.AsBoolean(core);
            if (opdata1.optype == AddressType.Null || opdata2.optype == AddressType.Null)
            {
                opdata2 = StackValue.Null;
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata2.opdata != 0L || opdata1.opdata != 0L);
            }

            //Nullify(ref opdata2, ref opdata1);
            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void NOT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            opdata1 = opdata1.AsBoolean(core);
            if (opdata1.optype != AddressType.Null)
            {
                opdata1 = StackValue.BuildBoolean(opdata1.opdata == 0L ? true : false);
            }

            //Nullify(ref opdata1);
            SetOperandData(instruction.op1, opdata1);

            ++pc;
            return;
        }
        private void EQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (AddressType.Boolean == opdata1.optype || AddressType.Boolean == opdata2.optype)
            {
                opdata1 = opdata1.AsBoolean(core);
                opdata2 = opdata2.AsBoolean(core);
                if (StackUtils.IsNull(opdata1) || StackUtils.IsNull(opdata2))
                {
                    opdata2 = StackValue.Null;
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.opdata == opdata2.opdata);
                }
            }
            else if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
                {
                    opdata1 = opdata1.AsDouble();
                    opdata2 = opdata2.AsDouble();
                    opdata2 = StackValue.BuildBoolean(MathUtils.Equals(opdata1.opdata_d, opdata2.opdata_d));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.opdata == opdata2.opdata);
                }
            }
            else if (StackUtils.IsString(opdata1) && StackUtils.IsString(opdata2))
            {
                int diffIndex = StringUtils.CompareString(opdata2, opdata1, core);
                opdata2 = StackValue.BuildBoolean(diffIndex == 0);
            }
            else if (opdata1.optype == opdata2.optype)
            {
                opdata2 = StackValue.BuildBoolean(opdata1.opdata == opdata2.opdata);
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(false);
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void EQD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata2.opdata_d = (opdata2.opdata_d.Equals(opdata1.opdata_d)) ? 1 : 0;
            opdata2.opdata_d = opdata2.opdata;

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void NQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (AddressType.Boolean == opdata1.optype || AddressType.Boolean == opdata2.optype)
            {
                opdata1 = opdata1.AsBoolean(core);
                opdata2 = opdata2.AsBoolean(core);
                opdata2 = StackValue.BuildBoolean(opdata1.opdata != opdata2.opdata);
            }
            else if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
                {
                    opdata1 = opdata1.AsDouble();
                    opdata2 = opdata2.AsDouble();
                    opdata2 = StackValue.BuildBoolean(!MathUtils.Equals(opdata1.opdata_d, opdata2.opdata_d));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.opdata != opdata2.opdata);
                }
            }
            else if (StackUtils.IsString(opdata1) && StackUtils.IsString(opdata2))
            {
                int diffIndex = StringUtils.CompareString(opdata1, opdata2, core);
                opdata2 = StackValue.BuildBoolean(diffIndex != 0);
            }
            else if (opdata1.optype == opdata2.optype)
            {
                opdata2 = StackValue.BuildBoolean(opdata1.opdata != opdata2.opdata);
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(true); ;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void NQD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata2.opdata_d = !MathUtils.Equals(opdata2.opdata_d, opdata1.opdata_d) ? 1 : 0;
            opdata2.opdata_d = opdata2.opdata;

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void GT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
                {
                    opdata1 = opdata1.AsDouble();
                    opdata2 = opdata2.AsDouble();
                    opdata2 = StackValue.BuildBoolean(MathUtils.IsGreaterThan(opdata2.opdata_d, opdata1.opdata_d));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata2.opdata > opdata1.opdata);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);
            ++pc;
            return;
        }
        private void GTD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata2.opdata = (opdata2.opdata_d > opdata1.opdata_d) ? 1 : 0;
            opdata2.opdata_d = opdata2.opdata;

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void LT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
                {
                    opdata1 = opdata1.AsDouble();
                    opdata2 = opdata2.AsDouble();
                    opdata2 = StackValue.BuildBoolean(MathUtils.IsLessThan(opdata2.opdata_d, opdata1.opdata_d));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata2.opdata < opdata1.opdata);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void LTD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata2.opdata_d = (opdata2.opdata_d < opdata1.opdata_d) ? 1 : 0;
            opdata2.opdata_d = opdata2.opdata;

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void GE_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
                {
                    opdata1 = opdata1.AsDouble();
                    opdata2 = opdata2.AsDouble();
                    opdata2 = StackValue.BuildBoolean(MathUtils.IsGreaterThanOrEquals(opdata2.opdata_d, opdata1.opdata_d));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata2.opdata >= opdata1.opdata);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void GED_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata2.opdata_d = (MathUtils.IsGreaterThanOrEquals(opdata2.opdata_d, opdata1.opdata_d)) ? 1 : 0;
            opdata2.opdata_d = opdata2.opdata;

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void LE_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (StackUtils.IsNumeric(opdata1) && StackUtils.IsNumeric(opdata2))
            {
                if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
                {
                    opdata1 = opdata1.AsDouble();
                    opdata2 = opdata2.AsDouble();
                    opdata2 = StackValue.BuildBoolean(MathUtils.IsLessThanOrEquals(opdata2.opdata_d, opdata1.opdata_d));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata2.opdata <= opdata1.opdata);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }
        private void LED_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata2.opdata = MathUtils.IsLessThanOrEquals(opdata2.opdata_d, opdata1.opdata_d) ? 1 : 0;
            opdata2.opdata_d = opdata2.opdata;

            SetOperandData(instruction.op1, opdata2);

            ++pc;
            return;
        }

        private void ALLOCA_Handler(Instruction instruction)
        {
            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op1.optype
                          || ProtoCore.DSASM.AddressType.Register == instruction.op1.optype);

            int size = DSASM.Constants.kInvalidIndex;

            if (ProtoCore.DSASM.AddressType.Int == instruction.op1.optype)
            {
                size = (int)instruction.op1.opdata; //Number of the elements in the array
            }
            else
            {
                StackValue arraySize = GetOperandData(instruction.op1);
                runtimeVerify(arraySize.optype == AddressType.Int);
                size = (int)arraySize.opdata;
            }

            runtimeVerify(DSASM.Constants.kInvalidIndex != size);
            StackValue pointer = rmem.BuildArrayFromStack(size);
            if (ProtoCore.DSASM.AddressType.String == instruction.op2.optype)
            {
                pointer = StackValue.BuildString(pointer.opdata);
            }
            rmem.Push(pointer);

            ++pc;
            return;
        }
        private void BOUNCE_Handler(Instruction instruction)
        {
            // We disallow language blocks inside watch window currently - pratapa
            Validity.Assert(DSASM.InterpreterMode.kExpressionInterpreter != Core.ExecMode);

            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == instruction.op1.optype);
            int blockId = (int)instruction.op1.opdata;

            // Comment Jun: On a bounce, update the debug property to reflect this.
            // Before the explicit bounce, this was done in Execute() which is now no longer the case
            // as Execute is only called once during first bounce and succeeding bounce reuse the same interpreter
            core.DebugProps.CurrentBlockId = blockId;

            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op2.optype);
            int entrypoint = (int)instruction.op2.opdata;

            ProtoCore.Runtime.Context context = new ProtoCore.Runtime.Context();
            // TODO(Jun/Jiong): Considering store the orig block id to stack frame
            int origRunningBlock = core.RunningBlock;
            core.RunningBlock = blockId;

            core.Rmem = rmem;
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PushConstructBlockId(blockId);
            }

#if ENABLE_EXCEPTION_HANDLING
                core.stackActiveExceptionRegistration.Push(core.ExceptionHandlingManager.CurrentActiveRegistration);
#endif

            int ci = ProtoCore.DSASM.Constants.kInvalidIndex;
            int fi = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (rmem.Stack.Count >= ProtoCore.DSASM.StackFrame.kStackFrameSize)
            {
                StackValue sci = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass);
                StackValue sfi = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction);
                if (sci.optype == AddressType.Int && sfi.optype == AddressType.Int)
                {
                    ci = (int)sci.opdata;
                    fi = (int)sfi.opdata;
                }
            }

#if ENABLE_EXCEPTION_HANDLING
            core.ExceptionHandlingManager.SwitchContextTo(blockId, fi, ci, pc);
#endif

            StackValue svThisPtr = ProtoCore.DSASM.StackValue.BuildPointer(ProtoCore.DSASM.Constants.kInvalidPointer);
            int returnAddr = pc + 1;

            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != executingBlock);
            //int blockDecl = executingBlock;
            int blockDecl = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock).opdata;
            int blockCaller = executingBlock;

            StackFrameType type = StackFrameType.kTypeLanguage;
            int depth = (int)rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameDepth).opdata;
            int framePointer = core.Rmem.FramePointer;

            // Comment Jun: Use the register TX to store explicit/implicit bounce state
            bounceType = ProtoCore.DSASM.CallingConvention.BounceType.kExplicit;
            TX = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kExplicit);

            List<StackValue> registers = new List<StackValue>();
            SaveRegisters(registers);

            StackFrameType callerType = (fepRun) ? StackFrameType.kTypeFunction : StackFrameType.kTypeLanguage;


            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                // Comment Jun: Temporarily disable debug mode on bounce
                //Validity.Assert(false); 

                //Validity.Assert(core.Breakpoints != null);
                //blockDecl = blockCaller = core.DebugProps.CurrentBlockId;

                core.DebugProps.SetUpBounce(this, blockCaller, returnAddr);

                StackFrame stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth + 1, framePointer, registers, null);
                ProtoCore.Language bounceLangauge = exe.instrStreamList[blockId].language;
                BounceExplicit(blockId, 0, bounceLangauge, stackFrame, core.Breakpoints);
            }
            else //if (core.Breakpoints == null)
            {
                StackFrame stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth + 1, framePointer, registers, null);

                ProtoCore.Language bounceLangauge = exe.instrStreamList[blockId].language;
                BounceExplicit(blockId, 0, bounceLangauge, stackFrame);
            }

            return;
        }

        private void CALL_Handler(Instruction instruction)
        {
            PushInterpreterProps(Properties);

            isGlobScope = false;

            runtimeVerify(ProtoCore.DSASM.AddressType.FunctionIndex == instruction.op1.optype);
            int fi = (int)instruction.op1.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);
            int ci = (int)instruction.op2.opdata;

            StackValue svDim = rmem.Pop();
            int dim = (int)svDim.opdata;

            StackValue svBlock = rmem.Pop();
            int blockId = (int)svBlock.opdata;
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                rmem.PushConstructBlockId(blockId);
            }
            SX = svBlock;

            ProcedureNode fNode = null;
            if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                fNode = core.ClassTable.ClassNodes[ci].vtable.procList[fi];
            }
            else
            {
                fNode = exe.procedureTable[blockId].procList[fi];
            }

            // Disabling support for stepping into replicating function calls temporarily 
            // This CALL instruction has a corresponding RETC instruction
            // and for debugger purposes for every RETURN/RETC where we restore the states,
            // we need a corresponding SetUpCallr to save the states. Therefore this call here - pratapa
            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                bool isBaseCall = true;
                core.DebugProps.SetUpCallrForDebug(core, this, fNode, pc, isBaseCall);
            }

            StackValue svThisPointer = StackValue.BuildInvalid();
            int pcoffset = 0;

            // It is to specially handle calling base constructor 
            // from derive class and calling setter. 
            //
            // For the first case, we want to call base constructor 
            // but dont want to allocate memory from the heap again 
            // (instruction ALLOC), so we want to skip the first 
            // instruction in base constructor. Besides, at the end 
            // of base constructor instruction RETC doesnt actually 
            // returns from a function as here we just simulate 
            // function call.

            if ((ProtoCore.DSASM.AddressType.Int == instruction.op3.optype) &&
               (instruction.op3.opdata >= 0))
            {
                // thisptr should be the pointer to the instance of derive class
                svThisPointer = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr);
                // how many instruction offset? basically it should be 1 to skip ALLOCC
                pcoffset = (int)instruction.op3.opdata;
                // Validity.Assert(pcoffset == 1);

                // To simulate CALLR. We have to retrive the param values from the
                // stack and reverse these values and save back to the stack. Otherwise
                // in base constructor all params will be in reverse order
                List<StackValue> argvalues = new List<StackValue>();
                int stackindex = rmem.Stack.Count - 1;
                for (int idx = 0; idx < fNode.argTypeList.Count; ++idx)
                {
                    StackValue value = rmem.Stack[stackindex--];
                    GCRetain(value);
                    argvalues.Add(value);

                    // Probably it is useless in calling base constructor
                    bool hasGuide = (AddressType.ReplicationGuide == rmem.Stack[stackindex].optype);
                    if (hasGuide)
                    {
                        var replicationGuideList = new List<int>();

                        // Retrieve replication guides
                        value = rmem.Stack[stackindex--];
                        runtimeVerify(AddressType.ReplicationGuide == value.optype);

                        int guides = (int)value.opdata;
                        if (guides > 0)
                        {
                            for (int i = 0; i < guides; ++i)
                            {
                                value = rmem.Stack[stackindex--];
                                replicationGuideList.Add((int)value.opdata);
                            }
                        }
                        replicationGuideList.Reverse();
                    }
                }
                rmem.Pop(fNode.argTypeList.Count);
                for (int idx = 0; idx < fNode.argTypeList.Count; ++idx)
                {
                    rmem.Push(argvalues[idx]);
                }
            }

            // TODO: Jun: To set these at compile time. They are being hardcoded to 0 temporarily as
            // class methods are always defined only at the global scope
            int blockDecl = 0;
            int blockCaller = 0;


            // Comment Jun: the caller type is the current type in the stackframe
            StackFrameType callerType = (fepRun) ? StackFrameType.kTypeFunction : StackFrameType.kTypeLanguage;

            StackValue svCallConvention = StackValue.BuildCallingConversion((int)CallingConvention.CallType.kExplicitBase);
            TX = svCallConvention;


            // On implicit call, the SX is set in JIL Fep
            // On explicit call, the SX should be directly set here
            SX = StackValue.BuildInt(blockDecl);

            List<StackValue> registers = new List<StackValue>();
            SaveRegisters(registers);

            // Comment Jun: the depth is always 0 for a function call as we are reseting this for each function call
            // This is only incremented for every language block bounce
            int depth = 0;

            StackFrameType type = StackFrameType.kTypeFunction;
            rmem.PushStackFrame(svThisPointer, ci, fi, pc + 1, blockDecl, blockCaller, callerType, type, depth, rmem.FramePointer, registers, fNode.localCount, 0);


            // Now let's go to the function
            pc = fNode.pc + pcoffset;
            fepRunStack.Push(false);

            // A standard call instruction must reset the graphnodes for associative
            if (Language.kAssociative == executingLanguage)
            {
                UpdateMethodDependencyGraph(pc, fi, ci);
            }

            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                rmem.PopConstructBlockId();
            }
            return;
        }
        private void CALLC_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PushConstructBlockId(-1);
            }
            throw new NotImplementedException();

            //runtimeVerify(ProtoCore.DSASM.AddressType.FunctionIndex == instruction.op1.optype);
            //int fi = (int)instruction.op1.opdata;

            //runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);
            //int type = (int)instruction.op2.opdata;

            //rmem.PushFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize);
            //rmem.PushFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize);

            //// Set fi and pc locations
            //rmem.SetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass, StackValue.BuildInt(type));
            //rmem.SetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction, StackValue.BuildInt(fi));
            //rmem.SetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexReturnAddress, StackValue.BuildInt(pc + 1));

            //pc = exe.classTable.list[type].vtable.procList[fi].pc;
            //return;
        }

        protected virtual void CALLR_Handler(Instruction instruction)
        {
            bool isDynamicCall = false;
            Instruction instr = new Instruction();
            //a new copy of instruction. this will be modified if it is dynamic call
            instr.op1 = instruction.op1;
            instr.op2 = instruction.op2;
            instr.op3 = instruction.op3;
            instr.debug = instruction.debug;
            instr.opCode = instruction.opCode;

            //core.DebugProps.IsInFunction = true;

            if (instr.op1.optype == AddressType.Dynamic)
            {
                isDynamicCall = true;
                bool succeeded = ProcessDynamicFunction(instr);
                if (!succeeded)
                {
                    RX = StackValue.Null;
                    ++pc;
                    return;
                }
            }

            // runtime verification
            runtimeVerify(ProtoCore.DSASM.AddressType.FunctionIndex == instr.op1.optype);
            int functionIndex = (int)instr.op1.opdata;
            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instr.op2.optype);
            int classIndex = (int)instr.op2.opdata;
            StackValue valDepth = instr.op3;
            runtimeVerify(ProtoCore.DSASM.AddressType.Int == valDepth.optype);
            int depth = (int)valDepth.opdata;

            // chain up exception registration
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            int stackIndex = rmem.Stack.Count - 3;
            if (rmem.Stack[stackIndex].optype == AddressType.BlockIndex)
            {
                blockId = (int)rmem.Stack[stackIndex].opdata;
            }

#if ENABLE_EXCEPTION_HANDLING
            core.stackActiveExceptionRegistration.Push(core.ExceptionHandlingManager.CurrentActiveRegistration);
            core.ExceptionHandlingManager.SwitchContextTo(blockId, functionIndex, classIndex, pc);
#endif

            ++core.FunctionCallDepth;

            bool explicitCall = false;

            //
            // Comment Jun: 
            //      This solution is implemented to be able to capture Exceptions thrown from the callsite that cannot be handled by the runtime
            //      It will allow execution to continue in graph mode while nullifying the RX register
            //      This is a temporary solution until unhandle exceptions in the callsite are addressed
            //
            // TODO: 
            //      Luke/Jun to fix the error propagation from the callsite
            //
            if (!core.Options.IsDeltaExecution)
            {
                RX = Callr(functionIndex, classIndex, depth, ref explicitCall, isDynamicCall, instr.debug != null);
            }
            else
            {
                //
                // Comment Jun:
                //      Running in graph mode, nullify the result and continue.
                //      The only affected downstream operations are the ones connected to the graph associated with this call
                try
                {
                    RX = Callr(functionIndex, classIndex, depth, ref explicitCall, isDynamicCall, instr.debug != null);
                }
                catch (ReplicationCaseNotCurrentlySupported e)
                {
                    core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kReplicationWarning, e.Message);
                    RX = StackValue.Null;
                }
            }

            --core.FunctionCallDepth;

            if (!explicitCall)
            {
#if ENABLE_EXCEPTION_HANDLING
                core.ExceptionHandlingManager.CurrentActiveRegistration = core.stackActiveExceptionRegistration.Pop();

                if (core.ExceptionHandlingManager.IsStackUnwinding)
                {
                    int newpc = ProtoCore.DSASM.Constants.kInvalidIndex;
                    if (core.ExceptionHandlingManager.CanHandleIt(ref newpc))
                    {
                        LX = RX;
                        pc = newpc;
                        core.ExceptionHandlingManager.SetHandled();
                    }
                    else
                    {
                        // Clean up stack
                        isGlobScope = true;
                        runtimeVerify(rmem.ValidateStackFrame());
                        pc = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexReturnAddress).opdata;

                        ReturnSiteGC(blockId, classIndex, functionIndex);

                        rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
                        int localCount, paramCount;
                        GetLocalAndParamCount(blockId, classIndex, functionIndex, out localCount, out paramCount);
                        rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize + localCount + paramCount);

                        if (fepRunStack.Count > 0)
                        {
                            terminate = fepRunStack.Pop();
                        }
                        else if (fepRun)
                        {
                            terminate = true;
                        }
                    }
                }
                else
                {
                    ++pc; setPC(pc);
                }
#else
                ++pc;
#endif
            }
            return;
        }
        private void RETC_Handler(Instruction instruction)
        {
            runtimeVerify(rmem.ValidateStackFrame());

            RX = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr);

            int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            int fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;

            pc = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexReturnAddress).opdata;

            // block id is used in ReturnSiteGC to get the procedure node if it is not a member function 
            // not meaningful here, because we are inside a constructor
            int blockId = (int)SX.opdata;

            if (core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                ReturnSiteGC(blockId, ci, fi);
            }


            RestoreFromCall();
            core.RunningBlock = executingBlock;

            // If we're returning from a block to a function, the instruction stream needs to be restored.
            StackValue sv = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterTX);
            Validity.Assert(AddressType.CallingConvention == sv.optype);
            CallingConvention.CallType callType = (CallingConvention.CallType)sv.opdata;
            bool explicitCall = CallingConvention.CallType.kExplicit == callType || CallingConvention.CallType.kExplicitBase == callType;
            isExplicitCall = explicitCall;

            if (!core.Options.IDEDebugMode || core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                int localCount = 0;
                int paramCount = 0;
                GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

                rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
                rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize + localCount + paramCount);

                if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    RestoreRegistersFromStackFrame();

                    bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;
                }
            }


            terminate = !explicitCall;

            Properties = PopInterpreterProps();


            ProcedureNode procNode = GetProcedureNode(blockId, ci, fi);
            if (explicitCall)
            {
                //RX = CallSite.PerformReturnTypeCoerce(procNode, core, RX);

                bool wasDebugPropsPopped = DebugReturn(procNode, pc);



                // Comment Jun: For explicit calls, we need to manually GC decrement the arguments.
                //  These arguments were GC incremented on callr
                if (!wasDebugPropsPopped)
                //if (!core.Options.IDEDebugMode || core.ExecMode == ExecutionMode.kExpressionInterpreter)
                {
                    for (int i = 0; i < Properties.functionCallArguments.Count; ++i)
                    {
                        GCUtils.GCRelease(Properties.functionCallArguments[i], core);
                    }
                }

                if (CallingConvention.CallType.kExplicitBase != callType)
                {
                    //if (!core.Options.IDEDebugMode || core.ExecMode == ExecutionMode.kExpressionInterpreter)
                    if (!wasDebugPropsPopped)
                    {
                        DecRefCounter(RX);
                    }
                }
            }
            return;
        }

        private void RETB_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PopConstructBlockId();
            }

            if (!core.Options.IsDeltaExecution || (core.Options.IsDeltaExecution && 0 != core.RunningBlock))
            {
                GCCodeBlock(core.RunningBlock);
            }

            if (ProtoCore.DSASM.CallingConvention.BounceType.kExplicit == bounceType)
            {
                RestoreFromBounce();
                core.RunningBlock = executingBlock;
            }

            if (ProtoCore.DSASM.CallingConvention.BounceType.kImplicit == bounceType)
            {
                pc = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexReturnAddress).opdata;
                terminate = true;
            }


            ProtoCore.DSASM.StackFrameType type = StackFrameType.kTypeLanguage;

            // Comment Jun: Just want to see if this is the global rerb, in which case we dont retrieve anything
            //if (executingBlock > 0)
            {
                StackValue svCallerType = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexCallerStackFrameType);
                type = (ProtoCore.DSASM.StackFrameType)svCallerType.opdata;
            }

            // Pop the frame as we are adding stackframes for language blocks as well - pratapa
            // Do not do this for the final Retb 
            //if (core.RunningBlock != 0)
            if (!core.Options.IDEDebugMode || core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
                rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize);

                if (bounceType == CallingConvention.BounceType.kExplicit)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    RestoreRegistersFromStackFrame();

                    bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;

#if ENABLE_EXCEPTION_HANDLING
                    core.ExceptionHandlingManager.CurrentActiveRegistration = core.stackActiveExceptionRegistration.Pop();
                    if (core.ExceptionHandlingManager.IsStackUnwinding)
                    {
                    #region __MERGE_WITH_STACKUNWIND
                        // The excecution of last langage block is interrupted
                        // abnormally because of stack unwinding, so we need to 
                        // run GC to reclaim those allocated memory.
                        GCCodeBlock(core.RunningBlock);

                        int newpc = ProtoCore.DSASM.Constants.kInvalidIndex;
                        if (core.ExceptionHandlingManager.CanHandleIt(ref newpc))
                        {
                            LX = RX;
                            pc = newpc;
                            core.ExceptionHandlingManager.SetHandled();
                        }
                        // else cannot handle in this scope, so in the next
                        // loop of Execute(), current executive will be ;
                        // ended and returns to the last scope, continues
                        // stack unwinding

                        int origRunningBlock = executingBlock;
                        core.RunningBlock = origRunningBlock;
#endregion
                    }
                    else
                    {
                        DecRefCounter(RX);
                    }
#else
                    DecRefCounter(RX);
#endif
                }
            }
            else
            {
                DecRefCounter(RX);
            }

            if (type == StackFrameType.kTypeFunction)
            {
                // Comment Jun: 
                // Consider moving this to a restore to function method

                // If this language block was called explicitly, only then do we need to restore the instruction stream
                if (bounceType == CallingConvention.BounceType.kExplicit)
                {
                    // If we're returning from a block to a function, the instruction stream needs to be restored.
                    StackValue sv = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterTX);
                    Validity.Assert(AddressType.CallingConvention == sv.optype);
                    CallingConvention.CallType callType = (CallingConvention.CallType)sv.opdata;
                    if (CallingConvention.CallType.kExplicit == callType)
                    {
                        int callerblock = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock).opdata;
                        istream = exe.instrStreamList[callerblock];
                    }
                }
            }
            Properties = PopInterpreterProps();
            return;
        }

        private void RETCN_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PopConstructBlockId();
            }

            StackValue op1 = instruction.op1;
            runtimeVerify(op1.optype == AddressType.BlockIndex);
            int blockId = (int)op1.opdata;


            CodeBlock codeBlock = core.CompleteCodeBlockList[blockId];
            runtimeVerify(codeBlock.blockType == CodeBlockType.kConstruct);
            GCCodeBlock(blockId);
            pc++;
            return;
        }

        private void RETURN_Handler(Instruction instruction)
        {
            isGlobScope = true;

            runtimeVerify(rmem.ValidateStackFrame());

            int ptr = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexThisPtr).opdata;
            int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
            int fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;

            int blockId = (int)SX.opdata;

            StackValue svBlockDecl = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterSX);
            Validity.Assert(AddressType.BlockIndex == svBlockDecl.optype);
            blockId = (int)svBlockDecl.opdata;

            ProcedureNode procNode = GetProcedureNode(blockId, ci, fi);

            if (core.Options.ExecuteSSA)
            {
                if (core.Options.GCTempVarsOnDebug && core.Options.IDEDebugMode)
                {
                    // GC anonymous variables in the return stmt
                    if (null != Properties.executingGraphNode && !Properties.executingGraphNode.IsSSANode())
                    {
                        GCAnonymousSymbols(Properties.executingGraphNode.symbolListWithinExpression);
                        Properties.executingGraphNode.symbolListWithinExpression.Clear();
                    }
                }
            }

            pc = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexReturnAddress).opdata;
            executingBlock = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionCallerBlock).opdata;

            if (core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                ReturnSiteGC(blockId, ci, fi);
            }

            RestoreFromCall();
            core.RunningBlock = executingBlock;


            // If we're returning from a block to a function, the instruction stream needs to be restored.
            StackValue sv = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexRegisterTX);
            Validity.Assert(AddressType.CallingConvention == sv.optype);
            CallingConvention.CallType callType = (CallingConvention.CallType)sv.opdata;
            bool explicitCall = CallingConvention.CallType.kExplicit == callType;
            isExplicitCall = explicitCall;


            List<bool> execStateRestore = new List<bool>();
            if (!core.Options.IDEDebugMode || core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                // Get stack frame size
                int localCount = 0;
                int paramCount = 0;
                GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

                // Retrieve the execution execution states 
                int execstates = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexExecutionStates).opdata;
                if (execstates > 0)
                {
                    int offset = ProtoCore.DSASM.StackFrame.kStackFrameSize + localCount + paramCount;
                    for (int n = 0; n < execstates; ++n)
                    {
                        int relativeIndex = -offset - n - 1; 
                        StackValue svState = rmem.GetAtRelative(relativeIndex);
                        Validity.Assert(svState.optype == AddressType.Boolean);
                        execStateRestore.Add(svState.opdata == 0 ? false : true);
                    }
                }

                // Pop the stackframe
                rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;

                // Get the size of the stackframe and all variable size contents (local, args and exec states)
                int stackFrameSize = ProtoCore.DSASM.StackFrame.kStackFrameSize + localCount + paramCount + execstates;
                rmem.PopFrame(stackFrameSize);

                if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    RestoreRegistersFromStackFrame();

                    bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;
                }
            }


            terminate = !explicitCall;

            // Comment Jun: Dispose calls are always implicit and need to terminate
            // TODO Jun: This instruction should not know about dispose
            bool isDispose = procNode.name.Equals(ProtoCore.DSDefinitions.Keyword.Dispose);
            if (isDispose)
            {
                terminate = true;
            }

            // Let the return graphNode always be active 
            if (null != Properties.executingGraphNode)
            {
                Properties.executingGraphNode.isDirty = true;
            }

            Properties = PopInterpreterProps();

            if (explicitCall)
            {
                bool wasDebugPropsPopped = false;
                if (!isDispose)
                {
                    wasDebugPropsPopped = DebugReturn(procNode, pc);

                }

                // This condition should only be reached in the following cases:
                // 1. Debug StepOver or External Function call in non-replicating mode
                // 2. Normal execution in Serial (explicit call), non-replicating mode
                if (!wasDebugPropsPopped)
                {
                    RX = CallSite.PerformReturnTypeCoerce(procNode, core, RX);

                    // Comment Jun: For explicit calls, we need to manually GC decrement the arguments passed into the function 
                    // These arguments were GC incremented on callr
                    for (int i = 0; i < Properties.functionCallArguments.Count; ++i)
                    {
                        GCUtils.GCRelease(Properties.functionCallArguments[i], core);
                    }

                    StackValue svRet = RX;
                    GCDotMethods(procNode.name, ref svRet, Properties.functionCallDotCallDimensions, Properties.functionCallArguments);
                    DecRefCounter(svRet);

                    RX = svRet;

                }
            }
            
            // Restore the execution states
            if (execStateRestore.Count > 0)
            {
                // Now that the stack frame is popped off, we can retrieve the returned scope
                // Get graphnodes at the current scope after the return
                int currentScopeClass = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                int currentScopeFunction = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;

                bool isReturningFromRecursiveCall = procNode.procId == currentScopeFunction;
                if (isReturningFromRecursiveCall)
                {
                    // Since there are execution states retrieved from the stack frame,
                    // this means that we must be returning to a function and not the global scope
                    Validity.Assert(currentScopeFunction != ProtoCore.DSASM.Constants.kGlobalScope);

                    // Get the instruction stream where the current function resides in
                    StackValue svCurrentFunctionBlockDecl = rmem.GetAtRelative(rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock));
                    Validity.Assert(svCurrentFunctionBlockDecl.optype == AddressType.BlockIndex);
                    AssociativeGraph.DependencyGraph depgraph = exe.instrStreamList[(int)svCurrentFunctionBlockDecl.opdata].dependencyGraph;

                    List<AssociativeGraph.GraphNode> graphNodesInScope = depgraph.GetGraphNodesAtScope(currentScopeClass, currentScopeFunction);
                    Validity.Assert(execStateRestore.Count == graphNodesInScope.Count);
                    for (int n = 0; n < execStateRestore.Count; ++n)
                    {
                        graphNodesInScope[n].isDirty = execStateRestore[n];
                    }
                }
            }

            return;
        }

        private void SerialReplication(ProcedureNode procNode, ref int exeblock, int ci, int fi, DebugFrame debugFrame = null)
        {
            // TODO: Decide where to insert this common code block for Serial mode and Debugging - pratapa
            if (core.Options.ExecutionMode == ProtoCore.ExecutionMode.Serial || core.Options.IDEDebugMode)
            {
                RX = CallSite.PerformReturnTypeCoerce(procNode, core, RX);

                core.ContinuationStruct.RunningResult.Add(RX);
                core.ContinuationStruct.Result = RX;

                pc = core.ContinuationStruct.InitialPC;

                if (core.ContinuationStruct.Done)
                {
                    RX = HeapUtils.StoreArray(core.ContinuationStruct.RunningResult.ToArray(), null, core);
                    GCUtils.GCRetain(RX, core);

                    core.ContinuationStruct.RunningResult.Clear();
                    core.ContinuationStruct.IsFirstCall = true;

                    if (core.Options.IDEDebugMode)
                    {
                        // If stepping over function call in debug mode
                        if (core.DebugProps.RunMode == Runmode.StepNext)
                        {
                            // if stepping over outermost function call
                            if (!core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsFunctionStepOver))
                            {
                                core.DebugProps.SetUpStepOverFunctionCalls(core, procNode, debugFrame.ExecutingGraphNode, debugFrame.HasDebugInfo);
                            }
                        }
                        // The DebugFrame passed here is the previous one that was popped off before this call
                        // In the case of Dot call the debugFrame obtained here is the one for the member function
                        // for both Break and non Break cases - pratapa
                        DebugPerformCoercionAndGC(debugFrame);

                        // If call returns to Dot Call, restore debug props for Dot call
                        debugFrame = core.DebugProps.DebugStackFrame.Peek();
                        if (debugFrame.IsDotCall)
                        {
                            List<Instruction> instructions = istream.instrList;
                            bool wasPopped = RestoreDebugPropsOnReturnFromBuiltIns(ref exeblock, ref instructions);
                            if (wasPopped)
                            {
                                executingBlock = exeblock;
                                core.DebugProps.CurrentBlockId = exeblock;
                            }
                            else
                            {
                                core.DebugProps.RestoreCallrForNoBreak(core, procNode, false);
                            }
                            DebugPerformCoercionAndGC(debugFrame);
                        }

                        //core.DebugProps.DebugEntryPC = currentPC;
                    }
                    // Perform return type coercion, GC and/or GC for Dot methods for Non-debug, Serial mode replication case
                    else
                    {
                        // If member function
                        // 1. Release array arguments to Member function
                        // 2. Release this pointer
                        bool isBaseCall = false;
                        StackValue? thisPtr = null;
                        if (thisPtr != null)
                        {
                            // Replicating member function
                            PerformCoercionAndGC(null, false, thisPtr, core.ContinuationStruct.InitialArguments, core.ContinuationStruct.InitialDotCallDimensions);

                            // Perform coercion and GC for Dot call
                            ProcedureNode dotCallprocNode = null;
                            List<StackValue> dotCallArgs = new List<StackValue>();
                            List<StackValue> dotCallDimensions = new List<StackValue>();
                            PerformCoercionAndGC(dotCallprocNode, false, null, dotCallArgs, dotCallDimensions);
                        }
                        else
                        {
                            PerformCoercionAndGC(procNode, isBaseCall, null, core.ContinuationStruct.InitialArguments, core.ContinuationStruct.InitialDotCallDimensions);
                        }
                    }

                    pc++;
                    return;

                }
                else
                {
                    // Jump back to Callr to call ResolveForReplication and recompute fep with next argument
                    core.ContinuationStruct.IsFirstCall = false;

                    ReturnToCallSiteForReplication(procNode, ci, fi);
                    return;
                }

            }
        }

        private void ReturnToCallSiteForReplication(ProcedureNode procNode, int ci, int fi)
        {
            // Jump back to Callr to call ResolveForReplication and recompute fep with next argument
            // Need to push new arguments, then cache block, dim and type, push them before calling callr - pratapa

            // This functionality has to be common for both Serial mode execution and the debugger - pratap
            List<StackValue> nextArgs = core.ContinuationStruct.NextDispatchArgs;

            foreach (var arg in nextArgs)
            {
                rmem.Push(arg);
            }

            // TODO: Currently functions can be defined only in the global and level 1 blocks (BlockIndex = 0 or 1)
            // Ideally the procNode.runtimeIndex should capture this information but this needs to be tested - pratapa
            //rmem.Push(StackValue.BuildNode(AddressType.BlockIndex, core.DebugProps.CurrentBlockId));
            rmem.Push(StackValue.BuildBlockIndex(procNode.runtimeIndex));

            // The function call dimension for the subsequent feps are assumed to be 0 for now
            // This is not being used currently except for stack alignment - pratapa
            rmem.Push(StackValue.BuildArrayDimension(0));

            // This is unused in Callr() but needed for stack alignment
            rmem.Push(StackValue.BuildStaticType((int)PrimitiveType.kTypeVar));

            bool explicitCall = true;
            Callr(fi, ci, core.ContinuationStruct.InitialDepth, ref explicitCall);
        }

        private void JMP_Handler(Instruction instruction)
        {
            pc = (int)instruction.op1.opdata;
            return;
        }
        private void CJMP_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            if (opdata1.optype == ProtoCore.DSASM.AddressType.Double)
            {
                if (0 == opdata1.opdata_d)
                {
                    pc = (int)GetOperandData(instruction.op3).opdata;
                }
                else
                {
                    pc = (int)GetOperandData(instruction.op2).opdata;
                }
            }
            else
            {
                if (opdata1.optype == ProtoCore.DSASM.AddressType.Pointer)
                {
                    pc = (int)GetOperandData(instruction.op2).opdata;
                }
                else if (0 == opdata1.opdata)
                {
                    pc = (int)GetOperandData(instruction.op3).opdata;
                }
                else
                {
                    pc = (int)GetOperandData(instruction.op2).opdata;
                }
            }

            return;
        }
        private void JMP_EQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
            {
                if (Math.Equals(opdata1.opdata_d, opdata2.opdata))
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            else
            {
                if (opdata1.opdata == opdata2.opdata)
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            return;
        }
        private void JMP_GT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            bool isGT = false;
            if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
            {
                isGT = opdata1.opdata_d > opdata2.opdata_d;
            }
            else
            {
                isGT = opdata1.opdata > opdata2.opdata;
            }

            if (isGT)
            {
                pc = (int)instruction.op3.opdata;
            }
            else
            {
                ++pc;
            }
            return;
        }
        private void JMP_GTEQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
            {
                if (MathUtils.IsGreaterThanOrEquals(opdata1.opdata_d, opdata2.opdata_d))
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            else
            {
                if (opdata1.opdata >= opdata2.opdata)
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            return;
        }
        private void JMP_LT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
            {
                if (opdata1.opdata_d < opdata2.opdata_d)
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            else
            {
                if (opdata1.opdata < opdata2.opdata)
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            return;
        }
        private void JMP_LTEQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
            {
                if (MathUtils.IsLessThanOrEquals(opdata1.opdata_d, opdata2.opdata_d))
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            else
            {
                if (opdata1.opdata <= opdata2.opdata)
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            return;
        }
        private void JMP_NEQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (AddressType.Double == opdata1.optype || AddressType.Double == opdata2.optype)
            {
                if (!MathUtils.Equals(opdata1.opdata_d, opdata2.opdata_d))
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            else
            {
                if (opdata1.opdata != opdata2.opdata)
                {
                    pc = (int)instruction.op3.opdata;
                }
                else
                {
                    ++pc;
                }
            }
            return;
        }
        private void JLZ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            if (opdata1.opdata_d < 0)
            {
                pc = (int)instruction.op2.opdata;
            }
            else
            {
                ++pc;
            }

            return;
        }
        private void JGZ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            if (opdata1.opdata_d > 0)
            {
                pc = (int)instruction.op2.opdata;
            }
            else
            {
                ++pc;
            }

            return;
        }
        private void JZ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            if (MathUtils.Equals(opdata1.opdata_d, 0))
            {
                pc = (int)instruction.op2.opdata;
            }
            else
            {
                ++pc;
            }

            return;
        }

        private void CAST_Handler(Instruction instruction)
        {
            ++pc;
            return;
        }

        //instruction dep(block, symbol)
        //    def sv = stack.get(block,symbol)
        //    if sv is not equal to _dx (i.e. is dirty)
        //        // An update is triggered
        //        // Find all graph nodes whos dependents contain this symbol
        //        // Mark those nodes as dirty
        //        for n = 0 to graphNodeList.size n++
        //            def index = graphNodeList[n].Contains(block,symbol)
        //            if index is valid
        //                graphNodeList[n].isDirty = true
        //                break
        //            end
        //        end
        //    end
        //    SetupDependencyGraph()
        //end


        private void DEP_Handler(Instruction instruction)
        {
            // This expression ID of this instruction
            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op1.optype);
            int exprID = (int)instruction.op1.opdata;


            // The SSA assignment flag
            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op2.optype);
            bool isSSA = (1 == (int)instruction.op2.opdata) ? true : false;

            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op3.optype);
            int modBlkID = (int)instruction.op3.opdata;


            // The current function and class scope
            int ci = DSASM.Constants.kInvalidIndex;
            int fi = DSASM.Constants.kGlobalScope;
            bool isInFunction = core.FunctionCallDepth > 0;


            StackValue svType = rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexStackFrameType);
            ProtoCore.DSASM.StackFrameType type = (ProtoCore.DSASM.StackFrameType)svType.opdata;

            isInFunction = IsInsideFunction();


            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);
                {
                    isInFunction = core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun);
                }
            }

            if (isInFunction)
            {
                ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
            }

            if (null != Properties.executingGraphNode)
            {
                // Append this modified graph into the x-lang list
                if (!isSSA && (Properties.executingGraphNode.updateNodeRefList.Count > 0))
                {
                    if (!istream.xUpdateList.Contains(Properties.executingGraphNode.updateNodeRefList[0]))
                    {
                        istream.xUpdateList.Add(Properties.executingGraphNode.updateNodeRefList[0]);
                    }
                }
                if (core.Options.ExecuteSSA)
                {
                    if (core.Options.GCTempVarsOnDebug && core.Options.IDEDebugMode)
                    {
                        if (!Properties.executingGraphNode.IsSSANode())
                        {
                            bool isSetter = Properties.executingGraphNode.updateNodeRefList[0].nodeList.Count > 1;

                            GCAnonymousSymbols(Properties.executingGraphNode.symbolListWithinExpression, isSetter);
                            Properties.executingGraphNode.symbolListWithinExpression.Clear();
                        }
                    }
                }

                if (core.Options.ExecuteSSA)
                {
                    if (!Properties.executingGraphNode.IsSSANode())
                    {
                        foreach (AssociativeGraph.GraphNode gnode in deferedGraphNodes)
                        {
                            gnode.isDirty = true;
                        }
                        deferedGraphNodes.Clear();
                    }
                }
            }
            UpdateGraph(exprID, modBlkID, isSSA);

            // Get the next graph to be executed
            SetupNextExecutableGraph(fi, ci);


            return;
        }

        private void PUSHDEP_Handler(Instruction instruction)
        {
            // The symbol block
            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == instruction.op1.optype);
            int block = (int)instruction.op1.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.Int == instruction.op2.optype);
            int depth = (int)instruction.op2.opdata;

            // The symbol and its class index
            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op3.optype);
            int classIndex = (int)instruction.op3.opdata;

            // Get the identifier list
            List<StackValue> symbolList = new List<StackValue>();
            for (int n = 0; n < depth; ++n)
            {
                // TODO Jun: use the proper ID for this
                StackValue sv = rmem.Pop();
                runtimeVerify(sv.optype == AddressType.Int);
                symbolList.Add(sv);
            }
            symbolList.Reverse();

            // TODO Jun: use the proper ID for this
            runtimeVerify(AddressType.Int == symbolList[0].optype);
            int symindex = (int)symbolList[0].opdata;

            if (ProtoCore.DSASM.Constants.kInvalidIndex != symindex)
            {
                ProtoCore.DSASM.SymbolNode symnode = null;
                if (ProtoCore.DSASM.Constants.kInvalidIndex != classIndex)
                {
                    symnode = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symindex];
                }
                else
                {
                    symnode = core.DSExecutable.runtimeSymbols[block].symbolList[symindex];
                }

                ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                updateNode.symbol = symnode;
                updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;

                // Build the first symbol of the modified ref
                ProtoCore.AssociativeGraph.UpdateNodeRef modifiedRef = new AssociativeGraph.UpdateNodeRef();
                modifiedRef.nodeList.Add(updateNode);
                modifiedRef.block = symnode.runtimeTableIndex;

                // Update the current type
                classIndex = symnode.datatype.UID;

                // Build the rest of the list of symbols of the modified ref
                for (int n = 1; n < symbolList.Count; ++n)
                {
                    // TODO Jun: This should be a memvarindex address type
                    runtimeVerify(symbolList[n].optype == AddressType.Int);
                    symindex = (int)symbolList[n].opdata;

                    // Get the symbol and append it to the modified ref
                    updateNode = new AssociativeGraph.UpdateNode();
                    updateNode.symbol = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symindex];
                    updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;

                    runtimeVerify(null != updateNode.symbol);

                    modifiedRef.nodeList.Add(updateNode);

                    // Update the current type
                    classIndex = symnode.datatype.UID;
                }

                // Get the current value of symbol
                StackValue svSym;
                if (DSASM.Constants.kInvalidIndex != symnode.classScope
                    && DSASM.Constants.kInvalidIndex == symnode.functionIndex)
                {
                    svSym = StackValue.BuildMemVarIndex(symnode.symbolTableIndex);
                }
                else
                {
                    svSym = StackValue.BuildVarIndex(symnode.symbolTableIndex);
                }
                modifiedRef.symbolData = GetOperandData(block, svSym, instruction.op3);

                bool addNewModifiedRef = true;
                for (int i = 0; i < istream.xUpdateList.Count; ++i)
                {
                    var udpatedRef = istream.xUpdateList[i];
                    if (modifiedRef.IsEqual(istream.xUpdateList[i]))
                    {
                        istream.xUpdateList[i].symbolData = modifiedRef.symbolData;
                        addNewModifiedRef = false;
                        break;
                    }
                }
                if (addNewModifiedRef)
                {
                    istream.xUpdateList.Add(modifiedRef);
                }
            }

            ++pc;
            return;
        }


        //
        //  instruction depx()
        //      foreach dependencygraph in exe.instructionstreams
        //          foreach graphnode in dependencygraph
        //              foreach updatenode in updatelist
        //                  if graphnode.dependsOn(updatenode.symbol)
        //                      if graphnode.symdata is equal to updatenode.symdata
        //                          graphnode.isdirty = true
        //                      end
        //                  end
        //              end
        //          end
        //      end
        //      updatelist.clear()
        //  end
        //

        private void DEPX_Handler(Instruction instruction)
        {
            //XLangUpdateDependencyGraph();

            //// Clear the propagation list
            //istream.xUpdateList.Clear();

            runtimeVerify(Language.kAssociative == istream.language);

            // The current function and class scope
            int ci = DSASM.Constants.kInvalidIndex;
            int fi = DSASM.Constants.kGlobalScope;
            if (fepRun)
            {
                ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;
            }

            // Set the next graph to be executed
            SetupNextExecutableGraph(fi, ci);

            return;
        }

        private void THROW_Handler(Instruction instruction)
        {
#if ENABLE_EXCEPTION_HANDLING
            runtimeVerify(ProtoCore.DSASM.AddressType.BlockIndex == instruction.op1.optype);
            int blockId = (int)instruction.op1.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.ClassIndex == instruction.op2.optype);
            int classScope = (int)instruction.op2.opdata;

            runtimeVerify(ProtoCore.DSASM.AddressType.FunctionIndex == instruction.op3.optype);
            int functionScope = (int)instruction.op3.opdata;

            StackValue exceptionValue = LX;
            ProtoCore.Exceptions.ExceptionContext context = new Exceptions.ExceptionContext();
            context.pc = pc;
            context.codeBlockId = blockId;
            context.functionScope = functionScope;
            context.classScope = classScope;
            switch (exceptionValue.optype)
            {
                case AddressType.Int:
                    context.typeUID = (int)ProtoCore.PrimitiveType.kTypeInt;
                    break;
                case AddressType.Double:
                    context.typeUID = (int)ProtoCore.PrimitiveType.kTypeDouble;
                    break;
                case AddressType.Boolean:
                    context.typeUID = (int)ProtoCore.PrimitiveType.kTypeBool;
                    break;
                case AddressType.Pointer:
                    context.typeUID = (int)exceptionValue.metaData.type;
                    break;
                default:
                    context.typeUID = (int)ProtoCore.PrimitiveType.kTypeVar;
                    break;
            }
            // Walk through exception chain, a.k.a. 1st hand exception
            // handling
            core.ExceptionHandlingManager.HandleFirstHandException(context);

            // The exception can be handled in current scope, so simply jmp to 
            // the corresponding catch block
            int newpc = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (core.ExceptionHandlingManager.CanHandleIt(ref newpc))
            {
                pc = newpc;
                core.ExceptionHandlingManager.SetHandled();
            }
            else
            {
                RX = LX;
            }

            if (core.ExceptionHandlingManager.IsStackUnwinding)
            {
                while (core.stackActiveExceptionRegistration.Count > 1)
                {
                    StackValue svType = rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType);
                    StackFrameType type = (StackFrameType)svType.opdata;
                    if (StackFrameType.kTypeLanguage == type)
                    {
                        RestoreFromBounce();
                        rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
                        rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize);


                        // Restoring the registers require the current frame pointer of the stack frame 
                        RestoreRegistersFromStackFrame();

                        bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;

                        core.ExceptionHandlingManager.CurrentActiveRegistration = core.stackActiveExceptionRegistration.Pop();

            #region __MERGE_WITH_STACKUNWIND

                        // The excecution of last langage block is interrupted
                        // abnormally because of stack unwinding, so we need to 
                        // run GC to reclaim those allocated memory.
                        GCCodeBlock(core.RunningBlock);

                        newpc = ProtoCore.DSASM.Constants.kInvalidIndex;
                        if (core.ExceptionHandlingManager.CanHandleIt(ref newpc))
                        {
                            LX = RX;
                            pc = newpc;
                            core.ExceptionHandlingManager.SetHandled();
                            break;
                        }
                        // else cannot handle in this scope, so in the next
                        // loop of Execute(), current executive will be ;
                        // ended and returns to the last scope, continues
                        // stack unwinding

                        int origRunningBlock = executingBlock;
                        core.RunningBlock = origRunningBlock;
#endregion
                    }
                    else
                    {
                        RestoreFromCall();

                        int ci = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                        int fi = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFunction).opdata;

                        int localCount = 0;
                        int paramCount = 0;
                        GetLocalAndParamCount(executingBlock, ci, fi, out localCount, out paramCount);

                        rmem.FramePointer = (int)rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexFramePointer).opdata;
                        rmem.PopFrame(ProtoCore.DSASM.StackFrame.kStackFrameSize + localCount + paramCount);
                    }
                }
            }
            return;
#else
            throw new NotImplementedException();
#endif
        }

        private void SETEXPUID_Handler(Instruction instruction)
        {
            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                if (core.DebugProps.RunMode == Runmode.StepNext)
                {
                    if (!core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsFunctionStepOver))
                    {
                        // if ec is at end of an expression in imperative lang block
                        // we force restore the breakpoints                     
                        core.Breakpoints.Clear();
                        core.Breakpoints.AddRange(core.DebugProps.AllbreakPoints);
                    }
                }
            }
            pc++;
            return;
        }

        private void Exec(Instruction instruction)
        {
            switch (instruction.opCode)
            {
                case OpCode.ALLOC:
                    {
                        ALLOC_Handler(instruction);
                        return;
                    }

                case OpCode.ALLOCC:
                    {
                        ALLOCC_Handler(instruction);
                        return;
                    }

                case OpCode.ALLOCM:
                    {
                        ALLOCM_Handler(instruction);
                        return;
                    }

                case OpCode.PUSH:
                    {
                        PUSH_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHW:
                    {
                        PUSHW_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHINDEX:
                    {
                        PUSHINDEX_Handler(instruction);
                        return;
                    }
                case OpCode.PUSHG:
                    {
                        PUSHG_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHB:
                    {
                        PUSHB_Handler(instruction);
                        return;
                    }

                case OpCode.POPB:
                    {
                        POPB_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHM:
                    {
                        PUSHM_Handler(instruction);
                        return;
                    }
                case OpCode.PUSHLIST:
                    {
                        PUSHLIST_Handler(instruction);
                        return;
                    }
                case OpCode.PUSH_ARRAYKEY:
                    {
                        PUSH_VARSIZE_Handler(instruction);
                        return;
                    }

                case OpCode.POP:
                    {
                        POP_Handler(instruction);
                        return;
                    }

                case OpCode.POPW:
                    {
                        POPW_Handler(instruction);
                        return;
                    }

                case OpCode.POPG:
                    {
                        POPG_Handler(instruction);
                        return;
                    }

                case OpCode.POPM:
                    {
                        POPM_Handler(instruction);
                        return;
                    }

                case OpCode.POPLIST:
                    {
                        POPLIST_Handler(instruction);
                        return;
                    }

                case OpCode.MOV:
                    {
                        MOV_Handler(instruction);
                        return;
                    }

                case OpCode.ADD:
                    {
                        ADD_Handler(instruction);
                        return;
                    }

                case OpCode.ADDD:
                    {
                        ADDD_Handler(instruction);
                        return;
                    }

                case OpCode.SUB:
                    {
                        SUB_Handler(instruction);
                        return;
                    }

                case OpCode.SUBD:
                    {
                        SUBD_Handler(instruction);
                        return;
                    }

                case OpCode.MUL:
                    {
                        MUL_Handler(instruction);
                        return;
                    }

                case OpCode.MULD:
                    {
                        MULD_Handler(instruction);
                        return;
                    }

                case OpCode.DIV:
                    {
                        DIV_Handler(instruction);
                        return;
                    }

                case OpCode.DIVD:
                    {
                        DIVD_Handler(instruction);
                        return;
                    }

                case OpCode.MOD:
                    {
                        MOD_Handler(instruction);
                        return;
                    }
#if ENABLE_BIT_OP
                case OpCode.BITAND:
                    {
                        BITAND_HANDLER(instruction);
                        return;
                    }

                case OpCode.BITOR:
                    {
                        BITOR_HANDLER(instruction);
                        return;
                    }
                case OpCode.BITXOR:
                    {
                        BITXOR_HANDLER(instruction);
                        return;
                    }
                    case OpCode.NEGATE:
                    {
                        NEGATE_HAndler(instruction);
                        return;
#endif
                case OpCode.NEG:
                    {
                        NEG_Handler(instruction);
                        return;
                    }

                case OpCode.AND:
                    {
                        AND_Handler(instruction);
                        return;
                    }

                case OpCode.OR:
                    {
                        OR_Handler(instruction);
                        return;
                    }

                case OpCode.NOT:
                    {
                        NOT_Handler(instruction);
                        return;
                    }

                case OpCode.EQ:
                    {
                        EQ_Handler(instruction);
                        return;
                    }

                case OpCode.EQD:
                    {
                        EQD_Handler(instruction);
                        return;
                    }

                case OpCode.NQ:
                    {
                        NQ_Handler(instruction);
                        return;
                    }

                case OpCode.NQD:
                    {
                        NQD_Handler(instruction);
                        return;
                    }

                case OpCode.GT:
                    {
                        GT_Handler(instruction);
                        return;
                    }

                case OpCode.GTD:
                    {
                        GTD_Handler(instruction);
                        return;
                    }

                case OpCode.LT:
                    {
                        LT_Handler(instruction);
                        return;
                    }

                case OpCode.LTD:
                    {
                        LTD_Handler(instruction);
                        return;
                    }

                case OpCode.GE:
                    {
                        GE_Handler(instruction);
                        return;
                    }

                case OpCode.GED:
                    {
                        GED_Handler(instruction);
                        return;
                    }

                case OpCode.LE:
                    {
                        LE_Handler(instruction);
                        return;
                    }

                case OpCode.LED:
                    {
                        LED_Handler(instruction);
                        return;
                    }

                case OpCode.ALLOCA:
                    {
                        ALLOCA_Handler(instruction);
                        return;
                    }

                case OpCode.BOUNCE:
                    {
                        BOUNCE_Handler(instruction);
                        return;
                    }

                case OpCode.CALL:
                    {
                        CALL_Handler(instruction);
                        return;
                    }

                case OpCode.CALLC:
                    {
                        CALLC_Handler(instruction);
                        return;
                    }

                case OpCode.CALLR:
                    {
                        CALLR_Handler(instruction);
                        return;
                    }

                case OpCode.RETC:
                    {
                        RETC_Handler(instruction);
                        return;
                    }

                case OpCode.RETB:
                    {
                        RETB_Handler(instruction);
                        return;
                    }

                case OpCode.RETCN:
                    {
                        RETCN_Handler(instruction);
                        return;
                    }

                case OpCode.RETURN:
                    {
                        RETURN_Handler(instruction);
                        return;
                    }

                case OpCode.JMP:
                    {
                        JMP_Handler(instruction);
                        return;
                    }

                case OpCode.CJMP:
                    {
                        CJMP_Handler(instruction);
                        return;
                    }

                case OpCode.JMP_EQ:
                    {
                        JMP_EQ_Handler(instruction);
                        return;
                    }

                case OpCode.JMP_GT:
                    {
                        JMP_GT_Handler(instruction);
                        return;
                    }

                case OpCode.JMP_GTEQ:
                    {
                        JMP_GTEQ_Handler(instruction);
                        return;
                    }

                case OpCode.JMP_LT:
                    {
                        JMP_LT_Handler(instruction);
                        return;
                    }

                case OpCode.JMP_LTEQ:
                    {
                        JMP_LTEQ_Handler(instruction);
                        return;
                    }

                case OpCode.JMP_NEQ:
                    {
                        JMP_NEQ_Handler(instruction);
                        return;
                    }

                case OpCode.JLZ:
                    {
                        JLZ_Handler(instruction);
                        return;
                    }

                case OpCode.JGZ:
                    {
                        JGZ_Handler(instruction);
                        return;
                    }
                case OpCode.JZ:
                    {
                        JZ_Handler(instruction);
                        return;
                    }

                case OpCode.CAST:
                    {
                        CAST_Handler(instruction);
                        return;
                    }

                case OpCode.DEP:
                    {
                        DEP_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHDEP:
                    {
                        PUSHDEP_Handler(instruction);
                        return;
                    }

                case OpCode.DEPX:
                    {
                        DEPX_Handler(instruction);
                        return;
                    }

                case OpCode.THROW:
                    {
                        THROW_Handler(instruction);
                        return;
                    }

                case OpCode.SETEXPUID:
                    {
                        SETEXPUID_Handler(instruction);
                        return;
                    }
                default: //Unknown OpCode
                    throw new NotImplementedException("Unknown Op code, NIE Marker: {D6028708-CD47-4D0B-97FC-E681BD65DB5C}");
            }
        }
    }
}
