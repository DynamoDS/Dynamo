using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Exceptions;
using ProtoCore.Utils;
using ProtoCore.RuntimeData;
using System.Diagnostics;

namespace ProtoCore.DSASM
{
    public class Executive : IExecutive
    {
        private readonly bool enableLogging = true;
        private readonly Core core;
        public Core Core
        {
            get
            {
                return core;
            }
        }

        public Executable exe { get; set; }
        private Language executingLanguage;

        protected int pc = Constants.kInvalidPC;
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
        public Runtime.RuntimeMemory rmem { get; set; }

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

        enum DebugFlags
        {
            NONE,
            ENABLE_LOG,
            SPAWN_DEBUGGER
        }

        // Execute DS Release build
        private readonly int debugFlags = (int)DebugFlags.NONE;

        private Stack<bool> fepRunStack = new Stack<bool>();

        public int executingBlock = Constants.kInvalidIndex;

        CallingConvention.BounceType bounceType;

        public bool IsExplicitCall { get; set; }

        public List<AssociativeGraph.GraphNode> deferedGraphNodes {get; private set;}
        
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
            IsExplicitCall = false;
            Validity.Assert(core != null);
            this.core = core;
            enableLogging = core.Options.Verbose;

            exe = core.DSExecutable;
            istream = null;

            fepRun = isFep;
            //executingGraphNode = null;
            //nodeExecutedSameTimes = new List<AssociativeGraph.GraphNode>();
            Properties = new InterpreterProperties();

            rmem = core.Rmem;

            // Execute DS View VM Log
            //
            debugFlags = (int)DebugFlags.ENABLE_LOG;

            bounceType = CallingConvention.BounceType.kImplicit;

            deferedGraphNodes = new List<AssociativeGraph.GraphNode>();

            // Execute DS Debug watch
            //debugFlags = (int)DebugFlags.ENABLE_LOG | (int)DebugFlags.SPAWN_DEBUGGER;
        }

        /// <summary>
        /// Determines if the runtime is not inside a function 
        /// Will also return true if within a nested language block
        /// </summary>
        /// <returns></returns>
        private bool IsGlobalScope()
        {
            return rmem.CurrentStackFrame == null || 
                (rmem.CurrentStackFrame.ClassScope == Constants.kInvalidIndex && rmem.CurrentStackFrame.FunctionScope == Constants.kInvalidIndex);
        }

        private void BounceExplicit(int exeblock, int entry, Language language, StackFrame frame, List<Instruction> breakpoints)
        {
            fepRun = false;
            rmem.PushStackFrame(frame);

            SetupExecutive(exeblock, entry, language, breakpoints);

            bool debugRun = (0 != (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER));
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + CoreUtils.GetLanguageString(language));
            }
        }


        private void BounceExplicit(int exeblock, int entry, Language language, StackFrame frame)
        {
            fepRun = false;
            rmem.PushStackFrame(frame);

            SetupExecutive(exeblock, entry);

            bool debugRun = (0 != (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER));
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + CoreUtils.GetLanguageString(language));
            }
        }

        private void CallExplicit(int entry)
        {
            StackValue svFunctionBlock = rmem.GetAtRelative(StackFrame.kFrameIndexFunctionBlock);
            Validity.Assert(svFunctionBlock.IsBlockIndex);

            int exeblock = (int)svFunctionBlock.opdata;

            fepRun = true;
            SetupExecutiveForCall(exeblock, entry);
        }

        // TODO Jun: Optimization - instead of inspecting the stack, just store the 'is in function' flag in the stackframe
        // Performance would only siffer if you have so a huge number of nested language blocks
        private bool IsInsideFunction()
        {
            int fpRestore = rmem.FramePointer;
            StackValue svFrameType = rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType);
            while (svFrameType.IsFrameType)
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
                    svFrameType = rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType);
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
            AX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterAX);
            BX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterBX);
            CX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterCX);
            DX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterDX);
            EX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterEX);
            FX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterFX);
            LX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterLX);
            //RX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterRX);
            SX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterSX);
            TX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterTX);
        }

        private void RestoreFromCall()
        {
            StackValue svExecutingBlock = rmem.GetAtRelative(StackFrame.kFrameIndexFunctionCallerBlock);
            Validity.Assert(svExecutingBlock.IsBlockIndex);

            executingBlock = (int)svExecutingBlock.opdata;
            istream = exe.instrStreamList[executingBlock];

            fepRun = false;

            StackFrameType callerType = (StackFrameType)rmem.GetAtRelative(StackFrame.kFrameIndexCallerStackFrameType).opdata;
            if (callerType == StackFrameType.kTypeFunction)
            {
                fepRun = true;
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

            executingBlock = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionCallerBlock).opdata;
            Language currentLang = executingLanguage;

            RestoreExecutive(executingBlock);


            logVMMessage("End JIL Execution - " + CoreUtils.GetLanguageString(currentLang));
        }


        private void RestoreExecutive(int exeblock)
        {
            // Jun Comment: the stackframe mpof the current language must still be present for this this method to restore the executuve
            // It must be popped off after this call
            executingLanguage = exe.instrStreamList[exeblock].language;


            // TODO Jun: Remove this check once the global bounce stackframe is pushed
            if (rmem.FramePointer >= StackFrame.kStackFrameSize)
            {
                fepRun = false;
                StackFrameType callerType = (StackFrameType)rmem.GetAtRelative(StackFrame.kFrameIndexCallerStackFrameType).opdata;
                if (callerType == StackFrameType.kTypeFunction)
                {
                    fepRun = true;
                }
            }

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            Validity.Assert(null != istream.instrList);

            pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;
        }

        private void PushInterpreterProps(InterpreterProperties properties)
        {
            core.InterpreterProps.Push(new InterpreterProperties(properties));
        }

        private InterpreterProperties PopInterpreterProps()
        {
            return core.InterpreterProps.Pop();
        }

        private void SetupEntryPoint()
        {
            int ci = Constants.kInvalidIndex;
            int fi = Constants.kInvalidIndex;
            if (!IsGlobalScope())
            {
                ci = rmem.CurrentStackFrame.ClassScope;
                fi = rmem.CurrentStackFrame.FunctionScope;
            }

            if (fepRun)
            {
                UpdateMethodDependencyGraph(pc, fi, ci);
            }
            else
            {
                if (!core.Options.IsDeltaExecution)
                {
                    pc = SetupGraphNodesForEntry(pc);
                    SetupGraphEntryPoint(pc, IsGlobalScope());
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
                        SetupGraphEntryPoint(pc, IsGlobalScope());
                    }
                }
            }
        }

        private void SetupExecutive(int exeblock, int entry)
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
            }

            executingLanguage = exe.instrStreamList[exeblock].language;

            if (Language.kAssociative == executingLanguage)
            {
                SetupEntryPoint();
            }

            if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                pc = entry;
            }

            Validity.Assert(null != rmem);
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
            executingBlock = exeblock;


            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            Validity.Assert(null != istream.instrList);

            pc = entry;

            executingLanguage = exe.instrStreamList[exeblock].language;

            if (Language.kAssociative == executingLanguage)
            {
                int ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
                int fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;
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
        }

        private StackValue IndexIntoArray(StackValue sv, List<StackValue> dimensions)
        {
            if (null == dimensions || dimensions.Count <= 0)
            {
                return sv;
            }

            if (!sv.IsArray)
            {
                sv = StackValue.Null;
            }

            StackValue ret = GetIndexedArray(sv, dimensions);
            GCRetain(ret);
            GCRelease(sv);

            return ret;
        }

        private void PopArgumentsFromStack(int argumentCount,
                                           ref List<StackValue> arguments,
                                           ref List<List<ReplicationGuide>> replicationGuides)
        {
            int argFrameSize = 0;
            int stackindex = rmem.Stack.Count - 1;

            for (int p = 0; p < argumentCount; ++p)
            {
                // Must iterate through the args in the stack in reverse as 
                // its unknown how many replication guides were pushed
                StackValue value = rmem.Stack[stackindex--];
                ++argFrameSize;
                arguments.Add(value);

                bool hasGuide = rmem.Stack[stackindex].IsReplicationGuide;
                if (hasGuide)
                {
                    var replicationGuideList = new List<ReplicationGuide>();

                    // Retrieve replication guides
                    value = rmem.Stack[stackindex--];
                    ++argFrameSize;
                    runtimeVerify(value.IsReplicationGuide);

                    int guides = (int)value.opdata;
                    for (int i = 0; i < guides; ++i)
                    {
                        // Get the replicationguide number from the stack
                        value = rmem.Stack[stackindex--];
                        Validity.Assert(value.IsInteger);
                        int guideNumber = (int)value.opdata;

                        // Get the replication guide property from the stack
                        value = rmem.Stack[stackindex--];
                        Validity.Assert(value.IsBoolean);
                        bool isLongest = value.IsBoolean && value.RawBooleanValue;

                        var guide = new ReplicationGuide(guideNumber, isLongest);
                        replicationGuideList.Add(guide);
                        ++argFrameSize;
                    }

                    replicationGuideList.Reverse();
                    replicationGuides.Add(replicationGuideList);
                }
            }

            rmem.PopFrame(argFrameSize);
        }

        public void GCDotMethods(string name, ref StackValue sv, List<StackValue> dotCallDimensions = null, List<StackValue> arguments = null)
        {
            // Index into the resulting array
            if (name == Constants.kDotMethodName)
            {
                sv = IndexIntoArray(sv, dotCallDimensions);

                // Dot arg parameters is a special case function and must be GC'd here
                int stackPtr = rmem.Stack.Count - 1;
                GCRelease(rmem.Stack[stackPtr]);
                GCRelease(rmem.Stack[stackPtr - 1]);
                GCRelease(rmem.Stack[stackPtr - 2]);
                GCRelease(rmem.Stack[stackPtr - 3]);
                GCRelease(rmem.Stack[stackPtr - 4]);

                rmem.PopFrame(Constants.kDotCallArgCount);
            }
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

        public StackValue Callr(int functionIndex, 
                                int classIndex, 
                                int depth, 
                                ref bool explicitCall, 
                                bool isDynamicCall = false, 
                                bool hasDebugInfo = false)
        {
            // This is curently unused but required for stack alignment
            if (!isDynamicCall)
            {
                StackValue svType = rmem.Pop();
                runtimeVerify(svType.IsStaticType);
            }

            ProcedureNode fNode = null;

            // Pop off number of dimensions indexed into this function call
            // f()[0][1] -> 2 dimensions
            StackValue svDim = rmem.Pop();
            runtimeVerify(svDim.IsArrayDimension);

            // Jun Comment: The block where the function was declared in
            StackValue svBlockDeclaration = rmem.Pop();
            runtimeVerify(svBlockDeclaration.IsBlockIndex);
            int blockDeclId = (int)svBlockDeclaration.opdata;

            bool isCallingMemberFunction = Constants.kInvalidIndex != classIndex;
            if (isCallingMemberFunction)
            {
                fNode = exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex];

                if (depth > 0 && fNode.isConstructor)
                {
                    string message = String.Format(StringConstants.KCallingConstructorOnInstance, fNode.name);
                    core.RuntimeStatus.LogWarning(WarningID.kCallingConstructorOnInstance, message);
                    return StackValue.Null;
                }
            }
            else
            {
                // Global function
                fNode = exe.procedureTable[blockDeclId].procList[functionIndex];
            }

            // Build the arg values list
            var arguments = new List<StackValue>();
            var replicationGuides = new List<List<ReplicationGuide>>();

            // Retrive the param values from the stack
            int stackindex = rmem.Stack.Count - 1;

            List<StackValue> dotCallDimensions = new List<StackValue>();
            if (fNode.name.Equals(Constants.kDotMethodName))
            {
                int firstDotArgIndex = stackindex - (Constants.kDotCallArgCount - 1);
                StackValue svLHS = rmem.Stack[firstDotArgIndex];
                arguments.Add(svLHS);

                int functionArgsIndex = stackindex - (Constants.kDotCallArgCount - Constants.kDotArgIndexArgCount - 1);
                StackValue svArrayPtrFunctionArgs = rmem.Stack[functionArgsIndex];
                GCRetain(svArrayPtrFunctionArgs);

                // Retrieve the indexed dimensions into the dot call
                int arrayDimIndex = stackindex - (Constants.kDotCallArgCount - Constants.kDotArgIndexArrayIndex - 1);
                StackValue svArrayPtrDimesions = rmem.Stack[arrayDimIndex];
                Validity.Assert(svArrayPtrDimesions.IsArray);

                int arrayCountIndex = stackindex - (Constants.kDotCallArgCount - Constants.kDotArgIndexDimCount - 1);
                StackValue svDimensionCount = rmem.Stack[arrayCountIndex];
                Validity.Assert(svDimensionCount.IsInteger);

                // If array dimension were provided then retrive the final pointer 
                if (svDimensionCount.opdata > 0)
                {
                    HeapElement he = rmem.Heap.GetHeapElement(svArrayPtrDimesions);
                    Validity.Assert(he.VisibleSize == svDimensionCount.opdata);
                    dotCallDimensions.AddRange(he.VisibleItems);
                }
            }
            else
            {
                PopArgumentsFromStack(fNode.argTypeList.Count, ref arguments, ref replicationGuides);
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
                        SymbolNode argSymbol = Properties.executingGraphNode.dependentList[n].updateNodeRefList[0].nodeList[0].symbol;
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
            if (Constants.kDotMethodName != fNode.name
                && Constants.kFunctionRangeExpression != fNode.name)
            {
                // Comment Jun: If this is a non-dot call, cache the guides first and retrieve them on the actual function call
                // TODO Jun: Ideally, cache the replication guides in the dynamic function node
                replicationGuides = GetCachedReplicationGuides(core, arguments.Count);
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
                if (!svThisPtr.IsPointer)
                {
                    string message = String.Format(StringConstants.kInvokeMethodOnInvalidObject, fNode.name);
                    core.RuntimeStatus.LogWarning(WarningID.kDereferencingNonPointer, message);
                    return StackValue.Null;
                }
            }
            else
            {
                // There is no depth but check if the function is a member function
                // If its a member function, the this pointer is required by the core to pass on to the FEP call
                if (isCallingMemberFunction && !fNode.isConstructor && !fNode.isStatic)
                {
                    // A member function
                    // Get the this pointer as this class instance would have already been cosntructed
                    svThisPtr = rmem.CurrentStackFrame.ThisPtr;
                }
                else
                {
                    // Global
                    svThisPtr = StackValue.BuildPointer(Constants.kInvalidPointer);
                }
            }

            if (svThisPtr.IsPointer &&
                svThisPtr.opdata != Constants.kInvalidIndex &&
                svThisPtr.metaData.type != Constants.kInvalidIndex)
            {
                int runtimeClassIndex = svThisPtr.metaData.type;
                ClassNode runtimeClass = core.ClassTable.ClassNodes[runtimeClassIndex];
                if (runtimeClass.IsMyBase(classIndex))
                {
                    classIndex = runtimeClassIndex;
                }
            }

            // Build the stackframe
            //int thisPtr = (int)svThisPtr.opdata;
            int ci = classIndex; // Constants.kInvalidIndex;   // Handled at FEP
            int fi = Constants.kInvalidIndex;   // Handled at FEP

            int returnAddr = pc + 1;

            int blockDecl = (int)svBlockDeclaration.opdata;

            if (null != Properties.executingGraphNode)
            {
                core.ExecutingGraphnode = Properties.executingGraphNode;
            }

            // Get the cached callsite, creates a new one for a first-time call
            CallSite callsite = core.GetCallSite(core.ExecutingGraphnode, classIndex, fNode.name);
            Validity.Assert(null != callsite);

            List<StackValue> registers = new List<StackValue>();
            SaveRegisters(registers);

            // Get the execution states of the current stackframe
            int currentScopeClass = Constants.kInvalidIndex;
            int currentScopeFunction = Constants.kInvalidIndex;
            GetCallerInformation(out currentScopeClass, out currentScopeFunction);


            // Handle execution states at the FEP
            var stackFrame = new StackFrame(svThisPtr, 
                                            ci, 
                                            fi, 
                                            returnAddr, 
                                            blockDecl, 
                                            core.RunningBlock, 
                                            fepRun ? StackFrameType.kTypeFunction : StackFrameType.kTypeLanguage, 
                                            StackFrameType.kTypeFunction, 
                                            0, 
                                            rmem.FramePointer, 
                                            registers, 
                                            null);

            FunctionCounter counter = FindCounter(functionIndex, classIndex, fNode.name);
            StackValue sv = StackValue.Null;


            if (core.Options.RecursionChecking)
            {
                //Do the recursion check before call
                if (counter.times < Constants.kRecursionTheshold) //&& counter.sharedCounter < Constants.kRepetationTheshold)
                {

                    // Build a context object in JILDispatch and call the Dispatch
                    if (counter.times == 0)
                    {
                        counter.times++;
                        core.calledInFunction = true;
                    }

                    else if (counter.times >= 1)
                    {
                        if (fNode.name.ToCharArray()[0] != '%' && fNode.name.ToCharArray()[0] != '_' && !fNode.name.Equals(Constants.kDotMethodName) && core.calledInFunction)
                        {
                            counter.times++;
                        }
                    }


                    if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
                    {
                        core.DebugProps.SetUpCallrForDebug(core, this, fNode, pc, false, callsite, arguments, replicationGuides, stackFrame, dotCallDimensions, hasDebugInfo);
                    }

                    sv = callsite.JILDispatch(arguments, replicationGuides, stackFrame, core, runtimeContext);
                }
                else
                {
                    FindRecursivePoints();
                    string message = String.Format(StringConstants.kMethodStackOverflow, core.recursivePoint[0].name);
                    core.RuntimeStatus.LogWarning(WarningID.kInvalidRecursion, message);

                    core.recursivePoint = new List<FunctionCounter>();
                    core.funcCounterTable = new List<FunctionCounter>();
                    sv = StackValue.Null;
                }
            }
            else
            {
                if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    if (core.ContinuationStruct.IsFirstCall)
                    {
                        core.DebugProps.SetUpCallrForDebug(core, 
                                                           this, 
                                                           fNode, 
                                                           pc, 
                                                           false, 
                                                           callsite, 
                                                           arguments, 
                                                           replicationGuides, 
                                                           stackFrame, 
                                                           dotCallDimensions, 
                                                           hasDebugInfo);
                    }
                    else
                    {
                        core.DebugProps.SetUpCallrForDebug(core, 
                                                           this, 
                                                           fNode, 
                                                           pc, 
                                                           false, 
                                                           callsite, 
                                                           core.ContinuationStruct.InitialArguments, 
                                                           replicationGuides, 
                                                           stackFrame,
                                                           core.ContinuationStruct.InitialDotCallDimensions, 
                                                           hasDebugInfo);
                    }
                }

                SX = svBlockDeclaration;
                stackFrame.SX = svBlockDeclaration;

                //Dispatch without recursion tracking 
                explicitCall = false;
                IsExplicitCall = explicitCall;

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

                if (sv.IsExplicitCall)
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
                    IsExplicitCall = explicitCall;
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
                if (sv.IsArray)
                {
                    // GCReleasePool.Add(sv);
                }

                if (fNode.name.ToCharArray()[0] != '%' && fNode.name.ToCharArray()[0] != '_')
                {
                    core.calledInFunction = false;
                }
            }
            return sv;
        }

        private StackValue CallrForMemberFunction(int classIndex,
                                                  int procIndex,
                                                  bool hasDebugInfo,
                                                  ref bool isExplicitCall)
        {
            var arrayDim = rmem.Pop();
            Validity.Assert(arrayDim.IsArrayDimension);

            var blockIndex = rmem.Pop();
            Validity.Assert(blockIndex.IsBlockIndex);

            ClassNode classNode = exe.classTable.ClassNodes[classIndex];
            ProcedureNode procNode = classNode.vtable.procList[procIndex];

            // Get all arguments and replications 
            var arguments = new List<StackValue>();
            var repGuides = new List<List<ReplicationGuide>>();
            PopArgumentsFromStack(procNode.argTypeList.Count,
                                  ref arguments,
                                  ref repGuides);
            arguments.Reverse();
            repGuides = GetCachedReplicationGuides(core, arguments.Count + 1);

            StackValue lhs = rmem.Pop();
            StackValue thisObject = lhs;
            bool isValidThisPointer = true;
            if (lhs.IsArray)
            {
                isValidThisPointer = ArrayUtils.GetFirstNonArrayStackValue(lhs, ref thisObject, core);
                arguments.Insert(0, lhs);
            }

            if (!isValidThisPointer || (!thisObject.IsPointer && !thisObject.IsArray))
            {
                core.RuntimeStatus.LogWarning(WarningID.kDereferencingNonPointer,
                                              StringConstants.kDeferencingNonPointer);
                return StackValue.Null;
            }

            var registers = new List<StackValue>();
            SaveRegisters(registers);

            var stackFrame = new StackFrame(thisObject,         // thisptr 
                                            classIndex,         // function class index
                                            procIndex,          // function index
                                            pc + 1,             // return address
                                            0,                  // member function always declared in block 0 */
                                            core.RunningBlock,  // caller block
                                            fepRun ? StackFrameType.kTypeFunction : StackFrameType.kTypeLanguage,
                                            StackFrameType.kTypeFunction,   // frame type
                                            0,                              // block depth
                                            rmem.FramePointer,
                                            registers,
                                            new List<bool>());

            var callsite = core.GetCallSite(core.ExecutingGraphnode,
                                            classIndex,
                                            procNode.name);
            Validity.Assert(null != callsite);

            bool setDebugProperty = core.Options.IDEDebugMode &&
                                    core.ExecMode != InterpreterMode.kExpressionInterpreter &&
                                    procNode != null;

            if (setDebugProperty)
            {
                core.DebugProps.SetUpCallrForDebug(core,
                                                   this,
                                                   procNode,
                                                   pc,
                                                   false,
                                                   callsite,
                                                   arguments,
                                                   repGuides,
                                                   stackFrame,
                                                   null,
                                                   hasDebugInfo);
            }

            SX = StackValue.BuildBlockIndex(0);
            stackFrame.SX = SX;

            StackValue sv = callsite.JILDispatch(arguments,
                                                 repGuides,
                                                 stackFrame,
                                                 core,
                                                 new Runtime.Context());

            isExplicitCall = sv.IsExplicitCall;
            if (isExplicitCall)
            {
                Properties.functionCallArguments = arguments;
                Properties.functionCallDotCallDimensions = new List<StackValue>();

                int entryPC = (int)sv.opdata;
                CallExplicit(entryPC);
            }

            return sv;
        }

        private void FindRecursivePoints()
        {
            foreach (FunctionCounter c in core.funcCounterTable)
            {

                if (c.times == Constants.kRecursionTheshold || c.times == Constants.kRecursionTheshold - 1)
                {
                    core.recursivePoint.Add(c);
                }
                //else if (c.sharedCounter == Constants.kRepetationTheshold || (c.sharedCounter == Constants.kRepetationTheshold - 1 || c.sharedCounter == Constants.kRepetationTheshold + 1)
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
       
        private void logWatchWindow(int blockId, int index)
        {
            if (!enableLogging)
                return;

            const string watchPrompt = "watch: ";
            if (0 != (debugFlags & (int)DebugFlags.ENABLE_LOG))
            {
                SymbolNode symbol = exe.runtimeSymbols[blockId].symbolList[index];
                string symbolName = symbol.name;

                if (symbolName.StartsWith(Constants.kInternalNamePrefix))
                {
                    return;
                }
                int ci = symbol.classScope;
                if (ci != Constants.kInvalidIndex)
                {
                    symbolName = core.ClassTable.ClassNodes[ci].name + "::" + symbolName;
                }
                string lhs = watchPrompt + symbolName;

                if (null != exe.runtimeSymbols[blockId].symbolList[index].arraySizeList)
                {
                    lhs = lhs + "[" + "offset:" + DX + "]";
                }

                string rhs = null;
                StackValue snode = rmem.GetSymbolValue(symbol);
                if (snode.IsPointer)
                {
                    int type = snode.metaData.type;
                    string cname = core.ClassTable.ClassNodes[type].name;
                    rhs = cname + ":ptr(" + snode.opdata + ")";
                }
                else if (snode.IsArray)
                {
                    int rawPtr = (int)snode.RawIntValue;
                    rhs = "Array:ptr(" + rawPtr + "):{" + GetArrayTrace(snode, blockId, index, new HashSet<int> { rawPtr } ) + "}";
                }
                else if (snode.IsFunctionPointer)
                {
                    rhs = "fptr: " + snode.opdata;
                }
                else if (snode.IsInteger)
                {
                    rhs = snode.opdata.ToString();
                }
                else if (snode.IsDouble)
                {
                    double data = snode.RawDoubleValue;
                    rhs = data.ToString("R").IndexOf('.') != -1 ? data.ToString("R") : data.ToString("R") + ".0";
                }
                else if (snode.IsBoolean)
                {
                    rhs = snode.RawBooleanValue.ToString().ToLower();
                }
                else if (snode.IsChar)
                {
                    Char character = EncodingUtils.ConvertInt64ToCharacter(snode.RawIntValue);
                    rhs = "'" + character + "'";
                }
                else if (snode.IsString)
                {
                    rhs = UnboxString(snode);
                }
                else if (snode.IsNull)
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
            if (snode.IsArray)
            {
                rhs = "{" + GetArrayTrace(snode, blockId, index, new HashSet<int> { (int)snode.opdata}) + "}";
            }
            else if (snode.IsInteger)
            {
                Int64 data = snode.opdata;
                rhs = data.ToString();
            }
            else if (snode.IsDouble)
            {
                double data = snode.RawDoubleValue;
                rhs = data.ToString("R").IndexOf('.') != -1 ? data.ToString("R") : data.ToString("R") + ".0";
            }
            else if (snode.IsBoolean)
            {
                bool data = snode.opdata == 0 ? false : true;
                rhs = data.ToString().ToLower();
            }
            else if (snode.IsNull)
            {
                rhs = Literal.Null;
            }
            else if (snode.IsChar)
            {
                Int64 data = snode.opdata;
                Char character = EncodingUtils.ConvertInt64ToCharacter(data);
                rhs = "'" + character + "'";
            }
            else if (snode.IsString)
            {
                rhs = UnboxString(snode);
            }
            else if (snode.IsPointer)
            {
                int type = snode.metaData.type;
                string cname = core.ClassTable.ClassNodes[type].name;
                rhs = cname + ":ptr(" + snode.opdata.ToString() + ")";
            }
            return rhs;
        }

        private string UnboxString(StackValue pointer)
        {
            HeapElement hs = rmem.Heap.GetHeapElement(pointer);

            string str = "";
            foreach (var item in hs.VisibleItems)
            {
                if (!item.IsChar)
                    return null;
                str += EncodingUtils.ConvertInt64ToCharacter(item.opdata);
            }

            if (str == "")
                return null;

            return "\"" + str + "\"";
        }

        private string GetArrayTrace(StackValue pointer, int blockId, int index, HashSet<int> pointers)
        {
            StringBuilder arrayelements = new StringBuilder();
            HeapElement hs = rmem.Heap.GetHeapElement(pointer); 

            for (int n = 0; n < hs.VisibleSize; ++n)
            {
                StackValue sv = hs.Stack[n];
                if (sv.IsArray)
                {
                    int ptr = (int)sv.opdata;
                    if (pointers.Contains(ptr))
                    {
                        arrayelements.Append("{...}");
                    }
                    else
                    {
                        pointers.Add(ptr);
                        arrayelements.Append("{" + GetArrayTrace(sv, blockId, index, pointers) + "}");
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
                foreach (AssociativeGraph.GraphNode graphNode in graphNodes)
                {
                    if (!graphNode.isDirty || !graphNode.isActive)
                    {
                        continue;
                    }

                    // Is return node or is updatable
                    if (graphNode.isReturn || graphNode.updateNodeRefList[0].nodeList.Count > 0)
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

                        if (graphNode.forPropertyChanged)
                        {
                            Properties.updateStatus = AssociativeEngine.UpdateStatus.kPropertyChangedUpdate;
                            graphNode.forPropertyChanged = false;
                        }
                        else
                        {
                            Properties.updateStatus = AssociativeEngine.UpdateStatus.kNormalUpdate;
                        }

                        // Clear runtime warning for the first run in delta
                        // execution.
                        if (core.Options.IsDeltaExecution && 
                            (Properties.executingGraphNode == null ||
                             Properties.executingGraphNode.OriginalAstID != graphNode.OriginalAstID))
                        {
                            core.RuntimeStatus.ClearWarningsForAst(graphNode.OriginalAstID);
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
                                        HandleCycle();
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
                pc = Constants.kInvalidPC;
            }
        }

        /// <summary>
        /// Sets up the first graph to be executed
        /// </summary>
        /// <param name="entrypoint"></param>
        /// <param name="isGlobalScope"></param>
        private void SetupGraphEntryPoint(int entrypoint, bool isGlobalScope)
        { 
            List<AssociativeGraph.GraphNode> graphNodeList = null;
            if (core.Options.ApplyUpdate && isGlobalScope)
            {
                graphNodeList = istream.dependencyGraph.GetGraphNodesAtScope(Constants.kInvalidIndex, Constants.kInvalidIndex);

                Validity.Assert(graphNodeList.Count > 0);

                // The default entry point on ApplyUpdate is the first graphNode
                entrypoint = graphNodeList[0].updateBlock.startpc;
            }
            else
            {
                graphNodeList = istream.dependencyGraph.GraphList;
            }

            foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in graphNodeList)
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

            if (core.Options.IsDeltaExecution)
            {
                core.RuntimeStatus.ClearWarningsForAst(Properties.executingGraphNode.OriginalAstID);
            }
        }

        private void HandleCycle()
        {
            List<AssociativeGraph.GraphNode> nodeIterations = Properties.nodeIterations;
            var CycleStartNodeAndEndNode = FindCycleStartNodeAndEndNode(nodeIterations);

            if (enableLogging)
            {
                foreach (AssociativeGraph.GraphNode node in nodeIterations)
                {
                    Console.WriteLine("nodes " + node.updateNodeRefList[0].nodeList[0].symbol.name);
                }
            }

            string message = String.Format(StringConstants.kCyclicDependency, CycleStartNodeAndEndNode[0].updateNodeRefList[0].nodeList[0].symbol.name, CycleStartNodeAndEndNode[1].updateNodeRefList[0].nodeList[0].symbol.name);
            core.RuntimeStatus.LogWarning(WarningID.kCyclicDependency, message);
            //BreakDependency(NodeExecutedSameTimes);
            foreach (AssociativeGraph.GraphNode node in nodeIterations)
            {
                node.isCyclic = true;
                SetGraphNodeStackValueNull(node);
                node.dependentList.Clear();
            }
            Properties.nodeIterations = new List<AssociativeGraph.GraphNode>();
        }

        private bool HasCyclicDependency(AssociativeGraph.GraphNode node)
        {
            return IsExecutedTooManyTimes(node, core.Options.kDynamicCycleThreshold);
        }

        private bool IsExecutedTooManyTimes(AssociativeGraph.GraphNode node, int limit)
        {
            Validity.Assert(null != node);
            return (node.counter > limit);
        }

        private AssociativeGraph.GraphNode[] FindCycleStartNodeAndEndNode(List<AssociativeGraph.GraphNode> nodesExecutedSameTime)
        {
            AssociativeGraph.GraphNode cyclicSymbolStart = null;
            AssociativeGraph.GraphNode cyclicSymbolEnd = null;

            cyclicSymbolStart = nodesExecutedSameTime[0];
            cyclicSymbolEnd = nodesExecutedSameTime.Last();

            var StartAndEnd = new [] { cyclicSymbolStart, cyclicSymbolEnd };
            //reset counter
            foreach (AssociativeGraph.GraphNode node in nodesExecutedSameTime)
            {
                node.counter = 0;
            }

            return StartAndEnd;

        }
        private bool IsNodeModified(StackValue svGraphNode, StackValue svUpdateNode)
        {
            bool isPointerModified = svGraphNode.IsPointer || svUpdateNode.IsPointer;
            bool isArrayModified = svGraphNode.IsArray || svUpdateNode.IsArray;
            bool isDataModified = svGraphNode.opdata != svUpdateNode.opdata;
            bool isDoubleDataModified = svGraphNode.IsDouble && svGraphNode.RawDoubleValue != svUpdateNode.ToDouble().RawDoubleValue;
            bool isTypeModified = !svGraphNode.IsInvalid && !svUpdateNode.IsInvalid && svGraphNode.optype != svUpdateNode.optype;

            // Jun Comment: an invalid optype means that the value was not set
            bool isInvalid = svGraphNode.IsInvalid || svUpdateNode.IsInvalid;

            return isInvalid || isPointerModified || isArrayModified || isDataModified || isDoubleDataModified || isTypeModified;
        }


        private void SetGraphNodeStackValue(AssociativeGraph.GraphNode graphNode, StackValue sv)
        {
            Validity.Assert(!graphNode.isReturn);
            // TODO Jun: Expand me to handle complex ident lists
            SymbolNode symbol = graphNode.updateNodeRefList[0].nodeList[0].symbol;
            Validity.Assert(null != symbol);
            rmem.SetSymbolValue(symbol, sv);
        }

        private void SetGraphNodeStackValueNull(AssociativeGraph.GraphNode graphNode)
        {
            StackValue svNull = StackValue.Null;
            SetGraphNodeStackValue(graphNode, svNull);
        }

        private bool UpdatePropertyChangedGraphNode()
        {
            bool propertyChanged = false;
            var graphNodes = istream.dependencyGraph.GraphList;
            foreach (var node in graphNodes)
            {
                if (node.propertyChanged)
                {
                    propertyChanged = true;
                    int exprUID = node.exprUID;
                    int modBlkId = node.modBlkUID;
                    bool isSSAAssign = node.IsSSANode();
                    List<AssociativeGraph.GraphNode> reachableGraphNodes = null;
                    bool recursiveSearch = false;
                    if (core.Options.ExecuteSSA)
                    {
                        reachableGraphNodes = AssociativeEngine.Utils.UpdateDependencyGraph(
                            node.lastGraphNode, this, exprUID, modBlkId, isSSAAssign, core.Options.ExecuteSSA, executingBlock, recursiveSearch, propertyChanged);
                    }
                    else
                    {
                        reachableGraphNodes = AssociativeEngine.Utils.UpdateDependencyGraph(
                            node, this, exprUID, modBlkId, isSSAAssign, core.Options.ExecuteSSA, executingBlock, recursiveSearch, propertyChanged);
                    }

                    // Mark reachable nodes as dirty
                    Validity.Assert(reachableGraphNodes != null);
                    foreach (AssociativeGraph.GraphNode gnode in reachableGraphNodes)
                    {
                        gnode.isDirty = true;
                    }

                    node.propertyChanged = false;
                }
            }
            return propertyChanged;
        }
     
        private int UpdateGraph(int exprUID, int modBlkId, bool isSSAAssign)
        {
            if (null != Properties.executingGraphNode)
            {
                if (!Properties.executingGraphNode.IsSSANode())
                {
                    UpdatePropertyChangedGraphNode();
                }
            }

            // Find reachable graphnodes
            List<AssociativeGraph.GraphNode> reachableGraphNodes = AssociativeEngine.Utils.UpdateDependencyGraph(
                Properties.executingGraphNode, this, exprUID, modBlkId, isSSAAssign, core.Options.ExecuteSSA, executingBlock, false);

            // Mark reachable nodes as dirty
            Validity.Assert(reachableGraphNodes != null);
            foreach (AssociativeGraph.GraphNode gnode in reachableGraphNodes)
            {
                gnode.isDirty = true;
            }

            // Get all redefined graphnodes
            int classScope = Constants.kInvalidIndex;
            int functionScope = Constants.kInvalidIndex;
            GetCallerInformation(out classScope, out functionScope);
            var nodesInScope = istream.dependencyGraph.GetGraphNodesAtScope(classScope, functionScope);
            List<AssociativeGraph.GraphNode> redefinedNodes = AssociativeEngine.Utils.GetRedefinedGraphNodes(core, Properties.executingGraphNode, nodesInScope, classScope, functionScope);
            Validity.Assert(redefinedNodes != null);
            foreach(AssociativeGraph.GraphNode gnode in redefinedNodes)
            {
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

                // Handle deactivated graphnodes
                GCSymbols(gnode.symbolListWithinExpression);
#if GC_MARK_AND_SWEEP
                foreach (var symbol in gnode.symbolListWithinExpression)
                {
                    rmem.SetSymbolValue(symbol, StackValue.Null);
                }
#endif
                gnode.isActive = false;
            }
            return reachableGraphNodes.Count;
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
                    if (!dimSymbol1.Equals(dimSymbol2))
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
                            int dimValue;
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
        /// Sets graphnodes dirty flag to true
        /// Returns the entry point
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private int SetupGraphNodesForEntry(int entry)
        {
            int setentry = entry;
            bool isFirstGraphSet = false;
            foreach (AssociativeGraph.GraphNode graphNode in istream.dependencyGraph.GraphList)
            {
                graphNode.isDirty = true;
                if (!isFirstGraphSet)
                {
                    // Setting the first graph of this function to be in executed (not dirty) state
                    isFirstGraphSet = true;
                    graphNode.isDirty = false;
                }

                if (Constants.kInvalidIndex == setentry)
                {
                    // Set the entry point as this graph and mark this graph as executed 
                    setentry = graphNode.updateBlock.startpc;
                    graphNode.isDirty = false;
                }
            }
            return setentry;
        }

        private void UpdateMethodDependencyGraph(int entry, int procIndex, int classIndex)
        {
            int setentry = entry;
            bool isFirstGraphSet = false;
            AssociativeGraph.GraphNode entryNode = null;

            StackValue svFunctionBlock = rmem.GetAtRelative(StackFrame.kFrameIndexFunctionBlock);
            Validity.Assert(svFunctionBlock.IsBlockIndex);
            int langBlockDecl = (int)svFunctionBlock.opdata;
            ProcedureNode procNode = GetProcedureNode(langBlockDecl, classIndex, procIndex);

            List<AssociativeGraph.GraphNode> graphNodes = procNode.GraphNodeList;//istream.dependencyGraph.GetGraphNodesAtScope(classIndex, procIndex);
            if (graphNodes != null)
            {
                foreach (AssociativeGraph.GraphNode graphNode in graphNodes)
                {
                    graphNode.isActive = graphNode.isDirty = true;
                    if (!isFirstGraphSet)
                    {
                        // Get the first graphnode of this function
                        if (graphNode.ProcedureOwned)
                        {
                            // Setting the first graph of this function to be in executed (not dirty) state
                            isFirstGraphSet = true;
                            graphNode.isDirty = false;
                            entryNode = graphNode;
                        }
                    }

                    if (Constants.kInvalidIndex == setentry)
                    {
                        // Set the entry point as this graph and mark this graph as executed 
                        setentry = graphNode.updateBlock.startpc;
                        graphNode.isDirty = false;
                        entryNode = graphNode;
                    }
                }
            }

            Properties.executingGraphNode = entryNode;

            pc = setentry;
        }

        public void XLangSetupNextExecutableGraph(int function, int classscope)
        {
            bool isUpdated = false;

            foreach (InstructionStream instrStream in exe.instrStreamList)
            {
                if (Language.kAssociative == instrStream.language && instrStream.dependencyGraph.GraphList.Count > 0)
                {
                    foreach (AssociativeGraph.GraphNode graphNode in instrStream.dependencyGraph.GraphList)
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
                pc = Constants.kInvalidPC;
            }
        }

        private void XLangUpdateDependencyGraph(int currentLangBlock)
        {
            int classScope = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
            int functionScope = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;

            List<AssociativeGraph.UpdateNodeRef> upadatedList = new List<AssociativeGraph.UpdateNodeRef>();

            // For every instruction list in the executable
            foreach (InstructionStream xInstrStream in exe.instrStreamList)
            {
                // If the instruction list is valid, is associative and has more than 1 graph node
                if (null != xInstrStream && Language.kAssociative == xInstrStream.language && xInstrStream.dependencyGraph.GraphList.Count > 0)
                {
                    // For every graphnode in the dependency list
                    foreach (AssociativeGraph.GraphNode graphNode in xInstrStream.dependencyGraph.GraphList)
                    {
                        // Do not check for xlang dependencies from within the same block
                        if (graphNode.languageBlockId == executingBlock)
                        {
                            continue;
                        }

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
                            if (graphNode.languageBlockId == currentLangBlock
                                || core.CompleteCodeBlockList[currentLangBlock].IsMyAncestorBlock(graphNode.languageBlockId))
                            {
                                continue;
                            }
                        }

                        // For every updated in the updatelist
                        foreach (AssociativeGraph.UpdateNodeRef modifiedRef in istream.xUpdateList)
                        {
                            // We allow dependency check if the modified graphnode list belong to some other block
                            if (modifiedRef.block != currentLangBlock)
                            {
                                // Check if the graphnode in the associative language depends on the current updated node
                                AssociativeGraph.GraphNode matchingNode = null;
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

                                    SymbolNode firstSymbolInUpdatedRef = graphNode.updateNodeRefList[0].nodeList[0].symbol;
                                    if (Constants.kInvalidIndex != firstSymbolInUpdatedRef.classScope)
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
                                        if (exe.runtimeSymbols[firstSymbolInUpdatedRef.runtimeTableIndex].symbolList.Count <= firstSymbolInUpdatedRef.symbolTableIndex)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (firstSymbolInUpdatedRef.classScope >= 0 &&
                                        core.ClassTable.ClassNodes[firstSymbolInUpdatedRef.classScope].symbols.symbolList.Count <= firstSymbolInUpdatedRef.symbolTableIndex)
                                    {
                                        continue;
                                    }

                                    StackValue svSym = (opAddr == AddressType.MemVarIndex) 
                                           ? StackValue.BuildMemVarIndex(firstSymbolInUpdatedRef.symbolTableIndex)
                                           : StackValue.BuildVarIndex(firstSymbolInUpdatedRef.symbolTableIndex);
                                    StackValue svClass = StackValue.BuildClassIndex(firstSymbolInUpdatedRef.classScope);

                                    runtimeVerify(Constants.kInvalidIndex != firstSymbolInUpdatedRef.runtimeTableIndex);
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
                foreach (AssociativeGraph.UpdateNodeRef noderef in upadatedList)
                {
                    istream.xUpdateList.Remove(noderef);
                }
            }
        }

        private void ResumeRegistersFromStack()
        {
            int fp = rmem.FramePointer;
            if (fp >= StackFrame.kStackFrameSize)
            {
                AX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterAX);
                BX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterBX);
                CX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterCX);
                DX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterDX);
                EX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterEX);
                FX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterFX);
                LX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterLX);
                RX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterRX);
                SX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterSX);
                TX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterTX);
            }
        }

        private void ResumeRegistersFromStackExceptRX()
        {
            int fp = rmem.FramePointer;
            if (fp >= StackFrame.kStackFrameSize)
            {
                AX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterAX);
                BX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterBX);
                CX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterCX);
                DX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterDX);
                EX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterEX);
                FX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterFX);
                LX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterLX);
                SX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterSX);
                TX = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterTX);
            }
       }

        private void SaveRegistersToStack()
        {
            int fp = rmem.FramePointer;
            if (fp >= StackFrame.kStackFrameSize)
            {
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterAX, AX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterBX, BX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterCX, CX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterDX, DX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterEX, EX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterFX, FX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterLX, LX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterRX, RX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterSX, SX);
                rmem.SetAtRelative(StackFrame.kFrameIndexRegisterTX, TX);
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
                RX = CallSite.PerformReturnTypeCoerce(procNode, core, RX);

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
                    StackValue sv = RX;
                    GCDotMethods(procNode.name, ref sv, DotCallDimensions, Arguments);
                    RX = sv;
                    DecRefCounter(RX);
                }
            }
        }

        /// <summary>
        /// Pops Debug stackframe, performs coercion and GC and pops stackframe if there's a break inside the function
        /// </summary>
        /// <param name="exeblock"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        bool RestoreDebugPropsOnReturnFromBuiltIns()
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

            ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
            fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;

            int localCount;
            int paramCount;
            int blockId = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionBlock).opdata;
            GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

            // Get execution states
            List<bool> execStateRestore = new List<bool>();
            execStateRestore = RetrieveExecutionStatesFromStack(localCount, paramCount);

            // Pop function stackframe as this is not allowed in Ret/Retc in debug mode
            rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;

            //int execstates = (int)rmem.GetAtRelative(StackFrame.kFrameIndexExecutionStates).opdata;
            rmem.PopFrame(StackFrame.kStackFrameSize + localCount + paramCount + execStateRestore.Count); 


            ResumeRegistersFromStackExceptRX();

            //StackValue svFrameType = rmem.GetAtRelative(StackFrame.kFrameIndexCallerStackFrameType);
            StackValue svFrameType = rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType);
            StackFrameType frametype = (StackFrameType)svFrameType.opdata;
            if (frametype == StackFrameType.kTypeLanguage)
            {
                bounceType = (CallingConvention.BounceType)TX.opdata;
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
                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                rmem.PopFrame(StackFrame.kStackFrameSize);

                ResumeRegistersFromStackExceptRX();
                bounceType = (CallingConvention.BounceType)TX.opdata;
            }

            if (pc < 0)
            {
                throw new EndOfScript();
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
            var tempFrame = core.DebugProps.DebugStackFrame.Peek();

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
                        waspopped = RestoreDebugPropsOnReturnFromBuiltIns();
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

            bool isReplicating;
            int exeblock = Constants.kInvalidIndex;
            int ci;
            int fi;

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
            }
            executingLanguage = exe.instrStreamList[exeblock].language;

            if (Language.kAssociative == executingLanguage && !core.DebugProps.isResume)
            {
                SetupEntryPoint();
            }

            Validity.Assert(null != rmem);
        }


        private bool HandleBreakpoint(List<Instruction> breakpoints, List<Instruction> runningInstructions, int currentPC)
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
                    throw new DebugHalting(); 
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
                bounceType = (CallingConvention.BounceType)TX.opdata;
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
                if (core.DebugProps.InlineConditionOptions.isInlineConditional &&
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
                if (restoreInstructionStream && IsExplicitCall)
                {
                    // The instruction stream list is updated on callr
                    instructions = istream.instrList;
                    exeblock = executingBlock;
                    core.DebugProps.CurrentBlockId = exeblock;
                }

                // Disabling support for stepping into replicating function calls temporarily - pratapa
                // Check if the current instruction is a return from a function call or constructor

                DebugFrame tempFrame = null;
                if (!IsExplicitCall && (instructions[core.DebugProps.DebugEntryPC].opCode == OpCode.RETURN || instructions[core.DebugProps.DebugEntryPC].opCode == OpCode.RETC))
                {
                    int ci, fi;
                    bool isReplicating;
                    DebugFrame debugFrame;
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
                    RestoreDebugPropsOnReturnFromBuiltIns();
                    core.DebugProps.DebugEntryPC = pc;
                }

                core.Rmem = rmem;

                bool terminateExec = HandleBreakpoint(breakpoints, instructions, pc);
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
            SetupExecutive(exeblock, entry);

            string engine = CoreUtils.GetLanguageString(language);

            bool debugRun = IsDebugRun();
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + engine);
            }

            while (!terminate)
            {
                if(core.CancellationPending)
                {
                    throw new ExecutionCancelledException();
                }

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

        protected SymbolNode GetSymbolNode(int blockId, int classIndex, int symbolIndex)
        {
            if (Constants.kGlobalScope == classIndex)
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

        private StackValue GetOperandData(int blockId, StackValue opSymbol, StackValue opClass)
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
                    SymbolNode symbol = GetSymbolNode(blockId, (int)opClass.opdata, (int)opSymbol.opdata);
                    data = rmem.GetSymbolValue(symbol);
                    break;

                case AddressType.MemVarIndex:
                    data = rmem.GetMemberData((int)opSymbol.opdata, (int)opClass.opdata, exe);
                    break;

                case AddressType.StaticMemVarIndex:
                    SymbolNode staticMember = GetSymbolNode(blockId, Constants.kGlobalScope, (int)opSymbol.opdata);
                    data = rmem.GetSymbolValue(staticMember);
                    break;

                case AddressType.ThisPtr:
                    data = rmem.CurrentStackFrame.ThisPtr;
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
            SymbolNode node;
            if (Constants.kGlobalScope == scope)
            {
                node = exe.runtimeSymbols[block].symbolList[symbol];
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

                    SymbolNode symbol = GetSymbolNode(blockId, (int)op2.opdata, (int)op1.opdata);
                    opPrev = rmem.GetSymbolValue(symbol);
                    rmem.SetSymbolValue(symbol, opVal);
                    core.UpdatedSymbols.Add(symbol);

                    if (IsDebugRun())
                    {
                        logWatchWindow(blockId, (int)op1.opdata);
                        System.Console.ReadLine();
                    }

                    if (Constants.kGlobalScope == op2.opdata)
                    {
                        logWatchWindow(blockId, (int)op1.opdata);
                    }
                    break;

                case AddressType.StaticMemVarIndex:
                    var staticMember = GetSymbolNode( blockId, Constants.kGlobalScope, (int)op1.opdata);
                    opPrev = rmem.GetSymbolValue(staticMember);
                    rmem.SetSymbolValue(staticMember, opVal);
                    core.UpdatedSymbols.Add(staticMember);

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

        protected StackValue PopToIndexedArray(int blockId, int symbol, int classIndex, List<StackValue> dimlist, StackValue data)
        {
            SymbolNode symbolnode = GetSymbolNode(blockId, classIndex, symbol);
            Validity.Assert(symbolnode != null);

            StackValue value = rmem.GetSymbolValue(symbolnode);
            if (value.IsInvalid)
            {
                value = StackValue.Null;
            }

            Type t = symbolnode.staticType;
            StackValue ret = StackValue.Null;
            if (value.IsArray)
            {
                if (t.UID != (int)PrimitiveType.kTypeVar || t.rank >= 0)
                {
                    int lhsRepCount = 0;
                    foreach (var dim in dimlist)
                    {
                        if (dim.IsArray)
                        {
                            lhsRepCount++;
                        }
                    }

                    if (t.rank > 0)
                    {
                        t.rank = t.rank - dimlist.Count;
                        t.rank += lhsRepCount;

                        if (t.rank < 0)
                        {
                            string message = String.Format(StringConstants.kSymbolOverIndexed, symbolnode.name);
                            core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, message);
                        }
                    }

                }

                ret = ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
            }
            else if (value.IsString)
            {
                t = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeChar, 0);
                ret = ArrayUtils.SetValueForIndices(value, dimlist, data, t, core);
            }
            else
            {
                if (symbolnode.staticType.rank == 0)
                {
                    rmem.SetSymbolValue(symbolnode, StackValue.Null);
                    return value;
                }
                else
                {
                    StackValue array = rmem.Heap.AllocateArray(Enumerable.Empty<StackValue>(), null);
                    GCRetain(array);
                    rmem.SetSymbolValue(symbolnode, array);
                    if (!value.IsNull)
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

            if (IsGlobalScope())
            {
                logWatchWindow(blockId, symbolnode.symbolTableIndex);
            }
            return ret;
        }

        private bool IsDebugRun()
        {
            return (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER) != 0;
        }

        private void SetOperandData(StackValue opdest, StackValue stackData, int blockId = Constants.kInvalidIndex)
        {
            switch (opdest.optype)
            {
                case AddressType.VarIndex:
                case AddressType.MemVarIndex:
                    Validity.Assert(false);

                    SymbolNode symbol = GetSymbolNode(0, Constants.kGlobalScope, (int)opdest.opdata);
                    rmem.SetSymbolValue(symbol, stackData);

                    if (IsDebugRun())
                    {
                        logWatchWindow(Constants.kInvalidIndex, (int)opdest.opdata);
                        System.Console.ReadLine();
                    }

                    if (IsGlobalScope())
                    {
                        logWatchWindow(Constants.kInvalidIndex, (int)opdest.opdata);
                    }
                    break;
                case AddressType.StaticMemVarIndex:
                    SymbolNode staticMember = GetSymbolNode(0, Constants.kGlobalScope, (int)opdest.opdata);
                    rmem.SetSymbolValue(staticMember, stackData);

                    if (IsDebugRun())
                    {
                        logWatchWindow(Constants.kInvalidIndex, (int)opdest.opdata);
                        System.Console.ReadLine();
                    }

                    if (IsGlobalScope())
                    {
                        logWatchWindow(0, (int)opdest.opdata);
                    }
                    break;
                case AddressType.Register:
                {
                    StackValue data = stackData;
                        
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

        protected void runtimeVerify(bool condition, string msg = "Dsasm runtime error. Exiting...\n")
        {
            // TODO Jun: hook this up to a runtime error handler            
            if (!condition)
                throw new RuntimeException(msg);
        }

        private StackValue GetFinalPointer(int depth, bool isDotFunctionBody = false)
        {
            RTSymbol[] rtSymbols = new RTSymbol[depth];
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

                if (rmem.Stack[rmem.Stack.Count - 1].IsArrayDimension)
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
                            HeapElement he = core.Heap.GetHeapElement(dimValArraySv);
                            foreach (StackValue dimValSv in he.Stack)
                            {
                                rmem.Push(dimValSv);
                            }
                        }
                        // Pop off each dimension
                        rtSymbols[i].Dimlist = new int[dimensions];
                        for (int j = dimensions - 1; j >= 0; --j)
                        {
                            svDim = rmem.Pop();
                            if (!svDim.IsInteger)
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
                if (rtSymbols[0].Sv.IsInteger) // static, class UID
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
            if (rtSymbols[0].Sv.IsNull)
            {
                return rtSymbols[0].Sv;
            }

            int index = -1;
            StackValue ptr = rtSymbols[0].Sv;

            // Traverse the heap until the last pointer
            int n;
            int classsccope = rtSymbols[0].Sv.metaData.type;
            for (n = 1; n < rtSymbols.Length; ++n)
            {
                // Index into the current pointer
                // 'index' is the index of the member variable

                // class f {
                //   x : var; y : var // index of x = 0, y = 1
                // }

                //resolve dynamic reference
                if (rtSymbols[n].Sv.IsDynamic)
                {
                    classsccope = rtSymbols[n - 1].Sv.metaData.type;
                    bool succeeded = ProcessDynamicVariable((rtSymbols[n].Dimlist != null), ref rtSymbols[n].Sv, classsccope);
                    //if the identifier is unbounded. Push null
                    if (!succeeded)
                    {
                        return StackValue.Null;
                    }
                }

                if (rtSymbols[n].Sv.IsStaticVariableIndex)
                {
                    StackValue op2 = StackValue.BuildClassIndex(Constants.kInvalidIndex);
                    rtSymbols[n].Sv = GetOperandData(0, rtSymbols[n].Sv, op2);
                }
                else
                {
                    index = (int)rtSymbols[n].Sv.opdata;
                    rtSymbols[n].Sv = core.Heap.GetHeapElement(ptr).Stack[index];
                }

                // Once a pointer to the member is retrieved, get its indexed value
                rtSymbols[n].Sv = GetIndexedArray(rtSymbols[n].Sv, rtSymbols[n].Dimlist);
                ptr = rtSymbols[n].Sv;
            }

            // Check the last pointer
            StackValue opVal = rtSymbols[n - 1].Sv;
            if (opVal.IsPointer || opVal.IsInvalid)
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
                StackValue nextPtr = opVal;
                var data = core.Heap.GetHeapElement(nextPtr).Stack[0];

                bool isActualData = !data.IsPointer && !data.IsArray && !data.IsInvalid;
                if (isActualData)
                {
                    // Move one more and get the value at the first heapstack
                    opVal = data;
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

            if (!svPtr.IsArray)
            {
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
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

            if (!svPtr.IsArray)
            {
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                return StackValue.Null;
            }

            int dimensions = dimList.Length;
            for (int n = 0; n < dimensions - 1; ++n)
            {
                // TODO Jun: This means that variables are coerced to 32-bit when used as an array index
                try
                {
                    StackValue array = core.Heap.GetHeapElement(svPtr).GetValue(dimList[n], core);
                    if (!array.IsArray)
                    {
                        core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                        return StackValue.Null;
                    }
                    svPtr = array;
                }
                catch (ArgumentOutOfRangeException)
                {
                    core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                    return StackValue.Null;
                }
            }
            StackValue sv;
            try
            {
                sv = core.Heap.GetHeapElement(svPtr).GetValue(dimList[dimensions - 1], core);
            }
            catch (ArgumentOutOfRangeException)
            {
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                sv = StackValue.Null;
            }
            catch (IndexOutOfRangeException)
            {
                core.RuntimeStatus.LogWarning(WarningID.kIndexOutOfRange, StringConstants.kIndexOutOfRange);
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
                if (op1.IsMemberVariableIndex)
                {
                    StackValue thisptr = rmem.GetAtRelative(StackFrame.kFrameIndexThisPtr);
                    thisArray = rmem.Heap.GetHeapElement(thisptr).Stack[stackindex];
                }
                else
                {
                    thisArray = rmem.GetSymbolValue(symbolNode);
                }
            }

            if (!thisArray.IsArray)
            {
                if (varname.StartsWith(Constants.kForLoopExpression))
                {
                    return thisArray;
                }

                string message = String.Format(StringConstants.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            StackValue result;
            try
            {
                result = GetIndexedArray(thisArray, dims);
            }
            catch (ArgumentOutOfRangeException)
            {
                string message = String.Format(StringConstants.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            return result;
        }

        public StackValue GetIndexedArray(List<StackValue> dims, int blockId, StackValue op1, StackValue op2)
        {
            int symbolIndex = (int)op1.opdata;
            int classIndex = (int)op2.opdata;

            SymbolNode symbolNode = GetSymbolNode(blockId, classIndex, symbolIndex);
            string varname = symbolNode.name;

            StackValue thisArray;
            if (op1.IsMemberVariableIndex)
            {
                thisArray = rmem.GetMemberData(symbolIndex, classIndex, exe);
           }
            else
            {
                thisArray = rmem.GetSymbolValue(symbolNode);
            }

            if (!thisArray.IsArray && !thisArray.IsString)
            {
                if (varname.StartsWith(Constants.kForLoopExpression))
                {
                    return thisArray;
                }

                string message = String.Format(StringConstants.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            StackValue result;
            try
            {
                result = GetIndexedArray(thisArray, dims);

                // If the source object is a string and the result is an array
                // of character, wrap it into a string. 
                if (result.IsArray && thisArray.IsString)
                {
                    result = StackValue.BuildString(result.opdata);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                string message = String.Format(StringConstants.kSymbolOverIndexed, varname);
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, message);
                return StackValue.Null;
            }

            return result;
        }

        private bool ProcessDynamicVariable(bool isArray, ref StackValue svPtr, int classIndex)
        {
            int variableDynamicIndex = (int)svPtr.opdata;
            var dynamicVariableNode = core.DynamicVariableTable.variableTable[variableDynamicIndex];

            SymbolNode node = null;
            bool isStatic = false;

            if (!((int)PrimitiveType.kTypeVoid == classIndex
                || Constants.kInvalidIndex == classIndex
                || core.ClassTable.ClassNodes[classIndex].symbols == null))
            {
                bool hasThisSymbol;
                AddressType addressType;

                string name = dynamicVariableNode.variableName;
                int contextClassIndex = dynamicVariableNode.classIndex;
                int contextProcIndex = dynamicVariableNode.procIndex;
                int symbolIndex = core.ClassTable.ClassNodes[classIndex].GetSymbolIndex(name, contextClassIndex, contextProcIndex, core.RunningBlock, core, out hasThisSymbol, out addressType);
                if (Constants.kInvalidIndex != symbolIndex)
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

            if (null == node)
            {
                return false;
            }
            svPtr.opdata = node.symbolTableIndex;
            svPtr.optype = isArray ? AddressType.ArrayPointer : (isStatic ? AddressType.StaticMemVarIndex : AddressType.Pointer);
            return true;
        }

        private bool ResolveDynamicFunction(Instruction instr, out bool isMemberFunctionPointer)
        {
            isMemberFunctionPointer = false;
            int fptr = Constants.kInvalidIndex;
            int functionDynamicIndex = (int)instr.op1.opdata;
            int classIndex = (int)instr.op2.opdata;
            int depth = (int)instr.op3.opdata;
            bool isDotMemFuncBody = functionDynamicIndex == Constants.kInvalidIndex;
            bool isFunctionPointerCall = false;
            if (isDotMemFuncBody)
            {
                functionDynamicIndex = (int)rmem.Pop().opdata;
            }

            var dynamicFunction = core.DynamicFunctionTable.GetFunctionAtIndex(functionDynamicIndex);

            if (isDotMemFuncBody)
            {
                classIndex = dynamicFunction.ClassIndex;
            }

            string procName = dynamicFunction.Name;
            int argumentNumber = dynamicFunction.ArgumentNumber;
            List<Type> arglist = new List<Type>();
            for (int i = 0; i < argumentNumber; ++i)
            {
                arglist.Add(new Type());
            }

            if (procName == Constants.kFunctionPointerCall && depth == 0)
            {
                isFunctionPointerCall = true;
                classIndex = Constants.kGlobalScope;
                StackValue fpSv = rmem.Pop();
                if (!fpSv.IsFunctionPointer)
                {
                    rmem.PopFrame(argumentNumber); //remove the arguments
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
                    StackValue sv = core.Heap.GetHeapElement(argArraySv).Stack[i];
                    argSvList.Add(sv); //actual arguments
                    Type paramType = new Type();
                    paramType.UID = sv.metaData.type;
                    paramType.rank = 0;
                    if (sv.IsArray)
                    {
                        StackValue paramSv = sv;
                        while (paramSv.IsArray)
                        {
                            paramType.rank++;
                            var he = core.Heap.GetHeapElement(paramSv);

                            if (he.VisibleItems.Any())
                            {
                                paramSv = he.VisibleItems.First();
                                paramType.UID = paramSv.metaData.type;
                            }
                            else
                            {
                                paramType.UID = (int)PrimitiveType.kTypeArray;
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
                for (int i = 0; i < argumentNumber; i++)
                {
                    StackValue argSv = rmem.Pop();
                    argSvList.Add(argSv);
                }
            }
            int lefttype = Constants.kGlobalScope;
            bool isLeftClass = false;
            if (isDotMemFuncBody && rmem.Stack.Last().IsInteger) //constructor or static function
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
                lefttype = pSv.metaData.type;
            }

            int type = lefttype;

            if (depth > 0)
            {
                // check whether it is function pointer, this checking is done at runtime to handle the case
                // when turning on converting dot operator to function call
                if (!((int)PrimitiveType.kTypeVoid == type
                    || Constants.kInvalidIndex == type
                    || core.ClassTable.ClassNodes[type].symbols == null))
                {
                    bool hasThisSymbol;
                    AddressType addressType;
                    SymbolNode node = null;
                    bool isStatic = false;
                    int symbolIndex = core.ClassTable.ClassNodes[type].GetSymbolIndex(procName, type, Constants.kGlobalScope, core.RunningBlock, core, out hasThisSymbol, out addressType);
                    if (Constants.kInvalidIndex != symbolIndex)
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
                        if (fpSv.IsStaticVariableIndex)
                        {
                            StackValue op2 = new StackValue();
                            op2.optype = AddressType.ClassIndex;
                            op2.opdata = Constants.kInvalidIndex;

                            fpSv = GetOperandData(0, fpSv, op2);
                        }
                        else
                        {
                            StackValue ptr = rmem.Stack.Last();
                            fpSv = core.Heap.GetHeapElement(ptr).Stack[(int)fpSv.opdata];
                        }
                        //assuming the dimension is zero, as funtion call with nonzero dimension is not supported yet

                        // Check the last pointer
                        if (fpSv.IsPointer || fpSv.IsInvalid)
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
                            var data = core.Heap.GetHeapElement(fpSv).Stack[0];

                            bool isActualData = !data.IsPointer && 
                                                !data.IsArray && 
                                                !data.IsInvalid; 

                            if (isActualData)
                            {
                                // Move one more and get the value at the first heapstack
                                fpSv = data; 
                            }
                        }
                        if (!fpSv.IsFunctionPointer)
                        {
                            rmem.Pop(); //remove final pointer
                            return false;
                        }
                        fptr = (int)fpSv.opdata;
                    }
                }
            }

            ProcedureNode procNode = null;
            if (isFunctionPointerCall)
            {
                FunctionPointerNode fptrNode;
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
                        isMemberFunctionPointer = !procNode.isConstructor && !procNode.isStatic;                        
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
                if (Constants.kInvalidIndex != type)
                {
                    int realType;
                    bool isAccessible;
                    ProcedureNode memProcNode = core.ClassTable.ClassNodes[type].GetMemberFunction(procName, arglist, classIndex, out isAccessible, out realType);

                    if (memProcNode == null)
                    {
                        string property;
                        if (CoreUtils.TryGetPropertyName(procName, out property))
                        {
                            string classname = core.ClassTable.ClassNodes[type].name;
                            string message = String.Format(StringConstants.kPropertyOfClassNotFound, classname, property);
                            core.RuntimeStatus.LogWarning(WarningID.kMethodResolutionFailure, message);
                        }
                        else
                        {
                            string message = String.Format(StringConstants.kMethodResolutionFailure, procName);
                            core.RuntimeStatus.LogWarning(WarningID.kMethodResolutionFailure, message);
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

            if (null != procNode && Constants.kInvalidIndex != procNode.procId)
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
                rmem.Heap.GCRelease(new [] { sv }, this);
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
        public void GCCodeBlock(int blockId, int functionIndex = Constants.kGlobalScope, int classIndex = Constants.kInvalidIndex)
        {
            foreach (SymbolNode sn in exe.runtimeSymbols[blockId].symbolList.Values)
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
                    if (sn.absoluteFunctionIndex != Constants.kGlobalScope)
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

        private void GCSymbols(List<SymbolNode> symbolList)
        {
            foreach (var symbol in symbolList)
            {
                StackValue sv = rmem.GetSymbolValue(symbol);
                GCRelease(sv);
            }
        }

        public void ReturnSiteGC(int blockId, int classIndex, int functionIndex)
        {
            SymbolTable st;
            List<StackValue> ptrList = new List<StackValue>();
            if (Constants.kInvalidIndex == classIndex)
            {
                st = core.CompleteCodeBlockList[blockId].symbolTable;
            }
            else
            {
                st = exe.classTable.ClassNodes[classIndex].symbols;
            }

            foreach (SymbolNode symbol in st.symbolList.Values)
            {
                bool allowGC = symbol.functionIndex == functionIndex
                    && !symbol.name.Equals(Constants.kWatchResultVar);

                if (core.Options.GCTempVarsOnDebug && core.Options.ExecuteSSA)
                {
                    if (core.Options.IDEDebugMode)
                    {
                        allowGC = symbol.functionIndex == functionIndex
                            && !symbol.name.Equals(Constants.kWatchResultVar)
                            && !CoreUtils.IsSSATemp(symbol.name);
                    }
                }

                if (allowGC)
                {
                    StackValue sv = rmem.GetSymbolValue(symbol);
                    if (sv.IsPointer || sv.IsArray)
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
            if (Constants.kGlobalScope != classIndex)
            {
                return exe.classTable.ClassNodes[classIndex].vtable.procList[functionIndex];
            }
            return exe.procedureTable[blockId].procList[functionIndex];
        }

        private void GetLocalAndParamCount(int blockId, int classIndex, int functionIndex, out int localCount, out int paramCount)
        {
            localCount = paramCount = 0;

            if (Constants.kGlobalScope != classIndex)
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

        public List<List<ReplicationGuide>> GetCachedReplicationGuides(Core core, int argumentCount)
        {
            int index = core.replicationGuides.Count - argumentCount;
            if (index >= 0)
            {
                var replicationGuides = core.replicationGuides.GetRange(index, argumentCount);
                core.replicationGuides.RemoveRange(index, argumentCount);
                return replicationGuides;
            }
            return new List<List<ReplicationGuide>>();
        }

        #region Opcode Handlers

        private void ALLOC_Handler()
        {
            throw new NotImplementedException();
        }

        private void ALLOCC_Handler(Instruction instruction)
        {
            fepRunStack.Push(fepRun);
            runtimeVerify(instruction.op1.IsClassIndex);
            int type = (int)instruction.op1.opdata;
            MetaData metadata;
            metadata.type = type;
            StackValue pointer = core.Heap.AllocatePointer(exe.classTable.ClassNodes[type].size, metadata);
            rmem.SetAtRelative(StackFrame.kFrameIndexThisPtr, pointer);

            // Increase reference count here to avoid that in the 
            // constructor we use thisptr so that the executive will
            // thinks it is a temporary object and GC it. 
            GCRetain(pointer);

            ++pc;
        }

        private void PUSH_Handler(Instruction instruction)
        {
            int dimensions = 0;
            bool objectIndexing = false;

            int blockId = Constants.kInvalidIndex;
            StackValue op1 = instruction.op1;

            if (op1.IsVariableIndex ||
                op1.IsMemberVariableIndex ||
                op1.IsPointer ||
                op1.IsArray ||
                op1.IsStaticVariableIndex ||
                op1.IsFunctionPointer)
            {

                // TODO: Jun this is currently unused but required for stack alignment
                StackValue svType = rmem.Pop();
                runtimeVerify(svType.IsStaticType);

                StackValue svDim = rmem.Pop();
                runtimeVerify(svDim.IsArrayDimension);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(svBlock.IsBlockIndex);
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
                elementBasedUpdate = firstDepNode.symbol.Equals(symbolNode);
            }

            if (0 == dimensions && !elementBasedUpdate || !objectIndexing)
            {
                int fp = core.Rmem.FramePointer;
                if (core.ExecMode == InterpreterMode.kExpressionInterpreter && instruction.op1.IsThisPtr)
                    core.Rmem.FramePointer = core.watchFramePointer;
                StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                if (core.ExecMode == InterpreterMode.kExpressionInterpreter && instruction.op1.IsThisPtr)
                    core.Rmem.FramePointer = fp;
                rmem.Push(opdata1);
            }
            else
            {
                // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                runtimeVerify(op1.IsVariableIndex ||
                    op1.IsMemberVariableIndex ||
                    op1.IsArray);

                runtimeVerify(instruction.op2.IsClassIndex);

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
        }

        private void PUSHW_Handler(Instruction instruction)
        {
            int dimensions = 0;
            int blockId = Constants.kInvalidIndex;

            StackValue op1 = instruction.op1;
            StackValue op2 = instruction.op2;
    
            if (op1.IsVariableIndex ||
                op1.IsMemberVariableIndex ||
                op1.IsPointer ||
                op1.IsArray ||
                op1.IsStaticVariableIndex ||
                op1.IsFunctionPointer)
            {

                // TODO: Jun this is currently unused but required for stack alignment
                StackValue svType = rmem.Pop();
                runtimeVerify(svType.IsStaticType);

                StackValue svDim = rmem.Pop();
                runtimeVerify(svDim.IsArrayDimension);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(svBlock.IsBlockIndex);
                blockId = (int)svBlock.opdata;
            }

            int fp = core.Rmem.FramePointer;
            if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
                core.Rmem.FramePointer = core.watchFramePointer;

            if (0 == dimensions)
            {
                PushW(blockId, op1, op2);
            }
            else
            {
                // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                runtimeVerify(op1.IsVariableIndex || op1.IsMemberVariableIndex || op1.IsArray);
                runtimeVerify(op2.IsClassIndex);
                StackValue sv = GetIndexedArrayW(dimensions, blockId, op1, op2);
                rmem.Push(sv);
            }

            if (core.ExecMode == InterpreterMode.kExpressionInterpreter)
                core.Rmem.FramePointer = fp;

            ++pc;
        }

        private void PUSHINDEX_Handler(Instruction instruction)
        {
            if (instruction.op1.IsArrayDimension)
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
            else if (instruction.op1.IsReplicationGuide)
            {
                int guides = (int)instruction.op1.opdata;

                List<ReplicationGuide> argGuides = new List<ReplicationGuide>();
                for (int i = 0; i < guides; ++i)
                {
                    StackValue svGuideProperty = rmem.Pop();
                    runtimeVerify(svGuideProperty.IsBoolean);
                    bool isLongest = (int)svGuideProperty.opdata == 1;

                    StackValue svGuide = rmem.Pop();
                    runtimeVerify(svGuide.IsInteger);
                    int guideNumber = (int)svGuide.opdata;

                    argGuides.Add(new ReplicationGuide(guideNumber, isLongest));
                }

                argGuides.Reverse();
                core.replicationGuides.Add(argGuides);
            }

            ++pc;
        }

        private void PUSHG_Handler(Instruction instruction)
        {
            if (core.Options.TempReplicationGuideEmptyFlag)
            {
                int dimensions = 0;
                int guides = 0;
                int blockId = Constants.kInvalidIndex;

                StackValue op1 = instruction.op1;
                if (op1.IsVariableIndex ||
                    op1.IsMemberVariableIndex ||
                    op1.IsPointer ||
                    op1.IsArray ||
                    op1.IsStaticVariableIndex ||
                    op1.IsFunctionPointer)
                {

                    // TODO: Jun this is currently unused but required for stack alignment
                    StackValue svType = rmem.Pop();
                    runtimeVerify(svType.IsStaticType);

                    StackValue svDim = rmem.Pop();
                    runtimeVerify(svDim.IsArrayDimension);
                    dimensions = (int)svDim.opdata;

                    StackValue svBlock = rmem.Pop();
                    runtimeVerify(svBlock.IsBlockIndex);
                    blockId = (int)svBlock.opdata;

                }

                if (0 == dimensions)
                {
                    StackValue svNumGuides = rmem.Pop();
                    runtimeVerify(svNumGuides.IsReplicationGuide);
                    guides = (int)svNumGuides.opdata;

                    List<ReplicationGuide> argGuides = new List<ReplicationGuide>();
                    for (int i = 0; i < guides; ++i)
                    {
                        StackValue svGuideProperty = rmem.Pop();
                        runtimeVerify(svGuideProperty.IsBoolean);
                        bool isLongest = (int)svGuideProperty.opdata == 1;

                        StackValue svGuide = rmem.Pop();
                        runtimeVerify(svGuide.IsInteger);
                        int guideNumber = (int)svGuide.opdata;

                        argGuides.Add(new ReplicationGuide(guideNumber, isLongest));
                    }

                    argGuides.Reverse();
                    core.replicationGuides.Add(argGuides);

                    StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                    rmem.Push(opdata1);
                }
                else
                {
                    // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData

                    runtimeVerify(op1.IsVariableIndex ||
                                  op1.IsMemberVariableIndex ||
                                  op1.IsArray);

                    runtimeVerify(instruction.op2.IsClassIndex);

                    var dims = new List<StackValue>();
                    for (int n = 0; n < dimensions; ++n)
                    {
                        dims.Insert(0, rmem.Pop());
                    }

                    StackValue sv = GetIndexedArray(dims, blockId, instruction.op1, instruction.op2);


                    StackValue svNumGuides = rmem.Pop();
                    runtimeVerify(svNumGuides.IsReplicationGuide);
                    guides = (int)svNumGuides.opdata;

                    rmem.Push(sv);
                }
            }
            else
            {
                int dimensions = 0;
                int guides = 0;
                int blockId = Constants.kInvalidIndex;

                StackValue op1 = instruction.op1;
                if (op1.IsVariableIndex ||
                    op1.IsMemberVariableIndex ||
                    op1.IsPointer ||
                    op1.IsArray ||
                    op1.IsStaticVariableIndex ||
                    op1.IsFunctionPointer)
                {

                    // TODO: Jun this is currently unused but required for stack alignment
                    StackValue svType = rmem.Pop();
                    runtimeVerify(svType.IsStaticType);

                    StackValue svDim = rmem.Pop();
                    runtimeVerify(svDim.IsArrayDimension);
                    dimensions = (int)svDim.opdata;

                    StackValue svBlock = rmem.Pop();
                    runtimeVerify(svBlock.IsBlockIndex);
                    blockId = (int)svBlock.opdata;

                    StackValue svNumGuides = rmem.Pop();
                    runtimeVerify(svNumGuides.IsReplicationGuide);
                    guides = (int)svNumGuides.opdata;

                }

                if (0 == dimensions)
                {
                    List<ReplicationGuide> argGuides = new List<ReplicationGuide>();
                    for (int i = 0; i < guides; ++i)
                    {
                        StackValue svGuideProperty = rmem.Pop();
                        runtimeVerify(svGuideProperty.IsBoolean);
                        bool isLongest = (int)svGuideProperty.opdata == 1;

                        StackValue svGuide = rmem.Pop();
                        runtimeVerify(svGuide.IsInteger);
                        int guideNumber = (int)svGuide.opdata;

                        argGuides.Add(new ReplicationGuide(guideNumber, isLongest));
                    }

                    argGuides.Reverse();
                    core.replicationGuides.Add(argGuides);

                    StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                    rmem.Push(opdata1);
                }
                else
                {
                    // TODO Jun: This entire block that handles arrays shoudl be integrated with getOperandData
                    runtimeVerify(op1.IsVariableIndex ||
                                  op1.IsMemberVariableIndex ||
                                  op1.IsArray);

                    runtimeVerify(instruction.op2.IsClassIndex);

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
        }

        private void PUSHB_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PushConstructBlockId((int)instruction.op1.opdata);
            }
            ++pc;
        }

        private void POPB_Handler()
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PopConstructBlockId();
            }
            ++pc;
        }

        private void PUSHM_Handler(Instruction instruction)
        {
            int blockId = (int)instruction.op3.opdata;

            if (instruction.op1.IsStaticVariableIndex)
            {
                rmem.Push(StackValue.BuildBlockIndex(blockId));
                rmem.Push(instruction.op1);
            }
            else if (instruction.op1.IsClassIndex)
            {
                rmem.Push(StackValue.BuildClassIndex((int)instruction.op1.opdata));
            }
            else
            {
                StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                rmem.Push(opdata1);
            }

            ++pc;
        }
        private void PUSHLIST_Handler(Instruction instruction)
        {
            bool isDotFunctionBody = false;
            if (instruction.op1.IsDynamic)
            {
                isDotFunctionBody = true;
            }
            else
            {
                runtimeVerify(instruction.op1.IsInteger);
            }
            int depth = (int)instruction.op1.opdata;

            runtimeVerify(instruction.op2.IsClassIndex);

            runtimeVerify(instruction.op3.IsBlockIndex);

            StackValue sv = GetFinalPointer(depth, isDotFunctionBody);
            rmem.Push(sv);

            ++pc;
        }

        private void PUSH_VARSIZE_Handler(Instruction instruction)
        {
            // TODO Jun: This is a temporary solution to retrieving the array size until lib files are implemented
            runtimeVerify(instruction.op1.IsVariableIndex);
            int symbolIndex = (int)instruction.op1.opdata;

            runtimeVerify(instruction.op2.IsBlockIndex);
            int blockId = (int)instruction.op2.opdata;

            runtimeVerify(instruction.op3.IsClassIndex);
            int classIndex = (int)instruction.op3.opdata;

            SymbolNode snode = GetSymbolNode(blockId, classIndex, symbolIndex);
            runtimeVerify(null != snode);

            StackValue svArrayToIterate = rmem.GetSymbolValue(snode);

            // Check if the array to iterate is a valid array
            StackValue key = StackValue.Null;
            if (svArrayToIterate.IsArray)
            {
                HeapElement he = ArrayUtils.GetHeapElement(svArrayToIterate, core);
                Validity.Assert(he != null);
                bool arrayHasElement = he.VisibleItems.Any();
                bool dictionaryHasElement = he.Dict != null && he.Dict.Count > 0;
                if (arrayHasElement || dictionaryHasElement)
                {
                    key = StackValue.BuildArrayKey(svArrayToIterate, 0);
                }
                else
                {
                    // svArrayToIterate has no elements 
                    key = StackValue.Null;
                }
            }
            else
            {
                // Handle the case if svArrayToIterate is not an array
                if (svArrayToIterate.IsNull)
                {
                    key = StackValue.Null;
                }
                else
                {
                    // svArrayToIterate is not an array and is non-null, build the default key
                    key = StackValue.BuildArrayKey(Constants.kInvalidIndex, Constants.kInvalidIndex);
                }
            }
            rmem.Push(key); 
            ++pc;
        }

        protected StackValue POP_helper(Instruction instruction, out int blockId, out int dimensions)
        {
            dimensions = 0;
            blockId = Constants.kInvalidIndex;
            int staticType = (int)PrimitiveType.kTypeVar;
            int rank = Constants.kArbitraryRank;
            bool objectIndexing = false;

            if (instruction.op1.IsVariableIndex ||
                instruction.op1.IsPointer ||
                instruction.op1.IsArray)
            {

                StackValue svType = rmem.Pop();
                runtimeVerify(svType.IsStaticType);
                staticType = svType.metaData.type;
                rank = (int)svType.opdata;

                StackValue svDim = rmem.Pop();
                runtimeVerify(svDim.IsArrayDimension);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(svBlock.IsBlockIndex);
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
                runtimeVerify(instruction.op2.IsClassIndex);

                svData = rmem.Pop();
                StackValue coercedValue;

                if (isSSANode)
                {
                    coercedValue = svData;
                    // Double check to avoid the case like
                    //    %tvar = obj;
                    //    %tSSA = %tvar;
                    blockId = core.RunningBlock;

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

                tempSvData = coercedValue;
                EX = PopTo(blockId, instruction.op1, instruction.op2, coercedValue);

                if (core.Options.ExecuteSSA)
                {
                    if (!isSSANode)
                    {
                        if (EX.IsPointer && coercedValue.IsPointer)
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

                if (!instruction.op1.IsRegister)
                {
                    GCRelease(EX);
                }

            }
            else
            {
                runtimeVerify(instruction.op1.IsVariableIndex);

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

#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
                ProtoCore.AssociativeEngine.ArrayUpdate.UpdateSymbolArrayIndex(symbol.name, indexIntoList, symbolArrayIndexMap);
#endif

                svData = rmem.Pop();
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

                if (!instruction.op1.IsRegister)
                {
                    GCRelease(EX);
                }
            }

            if (!isSSANode && svData.IsReferenceType)
            {
                var dataHeapElement = rmem.Heap.GetHeapElement(svData);
                if (dataHeapElement.Active && dataHeapElement.Refcount == 0)
                {
                    GCRelease(svData);
                }
            }

            ++pc;
            return tempSvData;
        }

        protected virtual void POP_Handler(Instruction instruction)
        {
            int blockId;
            int dimensions;
            POP_helper(instruction, out blockId, out dimensions);
        }

        private void POPW_Handler(Instruction instruction)
        {
            int dimensions = 0;
            int blockId = Constants.kInvalidIndex;
            int staticType = (int)PrimitiveType.kTypeVar;
            int rank = Constants.kArbitraryRank;
            if (instruction.op1.IsVariableIndex ||
                instruction.op1.IsPointer ||
                instruction.op1.IsArray)
            {

                StackValue svType = rmem.Pop();
                runtimeVerify(svType.IsStaticType);
                staticType = svType.metaData.type;
                rank = (int)svType.opdata;

                StackValue svDim = rmem.Pop();
                runtimeVerify(svDim.IsArrayDimension);
                dimensions = (int)svDim.opdata;

                StackValue svBlock = rmem.Pop();
                runtimeVerify(svBlock.IsBlockIndex);
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
                runtimeVerify(instruction.op2.IsClassIndex);

                svData = rmem.Pop();
                StackValue coercedValue = TypeSystem.Coerce(svData, staticType, rank, core);
                GCRetain(coercedValue);

                PopToW(blockId, instruction.op1, instruction.op2, coercedValue);
            }
            else
            {
                runtimeVerify(instruction.op1.IsVariableIndex);

                List<StackValue> dimList = new List<StackValue>();
                for (int i = 0; i < dimensions; ++i)
                {
                    dimList.Insert(0, rmem.Pop());
                }

                svData = rmem.Pop();
                EX = PopToIndexedArray(blockId, (int)instruction.op1.opdata, (int)instruction.op2.opdata, dimList, svData);
                if (!instruction.op1.IsRegister)
                {
                    GCRelease(EX);
                }
            }

            if (svData.IsReferenceType)
            {
                var dataHeapElement = rmem.Heap.GetHeapElement(svData);
                if (dataHeapElement.Active && dataHeapElement.Refcount == 0)
                {
                    GCRelease(svData);
                }
            }

            ++pc;
        }

        private void POPG_Handler()
        {
            StackValue svNumGuides = rmem.Pop();
            runtimeVerify(svNumGuides.IsReplicationGuide);
            int guides = (int)svNumGuides.opdata;

            List<ReplicationGuide> argGuides = new List<ReplicationGuide>();
            for (int i = 0; i < guides; ++i)
            {
                StackValue svGuideProperty = rmem.Pop();
                runtimeVerify(svGuideProperty.IsBoolean);
                bool isLongest = (int)svGuideProperty.opdata == 1;

                StackValue svGuide = rmem.Pop();
                runtimeVerify(svGuide.IsInteger);
                int guideNumber = (int)svGuide.opdata;

                argGuides.Add(new ReplicationGuide(guideNumber, isLongest));
            }

            argGuides.Reverse();
            core.replicationGuides.Add(argGuides);

            ++pc;
        }

        protected StackValue POPM_Helper(Instruction instruction, out int blockId, out int classIndex)
        {
            classIndex = Constants.kInvalidIndex;

            StackValue op1 = instruction.op1;
            runtimeVerify(op1.IsMemberVariableIndex || op1.IsStaticVariableIndex);

            StackValue svType = rmem.Pop();
            runtimeVerify(svType.IsStaticType);
            int staticType = svType.metaData.type;
            int rank = (int)svType.opdata;

            StackValue svDim = rmem.Pop();
            runtimeVerify(svDim.IsArrayDimension);
            int dimensions = (int)svDim.opdata;

            StackValue svBlock = rmem.Pop();
            runtimeVerify(svBlock.IsBlockIndex);
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

            if (instruction.op1.IsStaticVariableIndex)
            {
                if (0 == dimensions)
                {
                    StackValue coercedValue = TypeSystem.Coerce(svData, staticType, rank, core);
                    GCRetain(coercedValue);

                    tempSvData = coercedValue;

                    EX = PopTo(blockId, instruction.op1, instruction.op2, coercedValue);
                    GCRelease(EX);
                }
                else
                {
                    EX = PopToIndexedArray(blockId, (int)instruction.op1.opdata, Constants.kGlobalScope, dimList, svData);
                    GCRelease(EX);
                }

                if (svData.IsReferenceType)
                {
                    var dataHeapElement = rmem.Heap.GetHeapElement(svData);
                    if (dataHeapElement.Active && dataHeapElement.Refcount == 0)
                    {
                        GCRelease(svData);
                    }
                }

                ++pc;
                return tempSvData;
            }

            int symbolIndex = (int)instruction.op1.opdata;
            classIndex = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
            int stackIndex = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symbolIndex].index;

            //==================================================
            //  1. If allocated... bypass auto allocation
            //  2. If pointing to a class, just point to the class directly, do not allocate a new pointer
            //==================================================

            StackValue svThis = rmem.CurrentStackFrame.ThisPtr;
            runtimeVerify(svThis.IsPointer);
            StackValue svProperty = core.Heap.GetHeapElement(svThis).Stack[stackIndex];

            StackValue svOldData = svData;
            Type targetType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar);
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

                    if (svProperty.IsArray)
                    {
                        if (targetType.UID != (int)PrimitiveType.kTypeVar || targetType.rank >= 0)
                        {
                            int lhsRepCount = 0;
                            foreach (var dim in dimList)
                            {
                                if (dim.IsArray)
                                {
                                    lhsRepCount++;
                                }
                            }

                            if (targetType.rank > 0)
                            {
                                targetType.rank = targetType.rank - dimList.Count;
                                targetType.rank += lhsRepCount;

                                if (targetType.rank < 0)
                                {
                                    string message = String.Format(StringConstants.kSymbolOverIndexed, symbolnode.name);
                                    core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, message);
                                }
                            }

                        }
                    }
                }
            }

            if (svProperty.IsPointer || (svProperty.IsArray && dimensions == 0))
            {
                // The data to assign is already a pointer
                if (svData.IsPointer || svData.IsArray)
                {
                    // Assign the src pointer directily to this property
                    GCRelease(svProperty);
                    core.Heap.GetHeapElement(svThis).Stack[stackIndex] = svData;
                }
                else
                {
                    GCRelease(svProperty);

                    StackValue svNewProperty = core.Heap.AllocatePointer(new [] { svData });
                    core.Heap.GetHeapElement(svThis).Stack[stackIndex] = svNewProperty;
                    GCRetain(svNewProperty);

                    exe.classTable.ClassNodes[classIndex].symbols.symbolList[stackIndex].heapIndex = (int)svNewProperty.opdata;
                }
            }
            else if (svProperty.IsArray && (dimensions > 0))
            {
                EX = ArrayUtils.SetValueForIndices(svProperty, dimList, svData, targetType, core);
                GCRelease(EX);
            }
            else // This property has NOT been allocated
            {
                if (svData.IsPointer || svData.IsArray)
                {
                    core.Heap.GetHeapElement(svThis).Stack[stackIndex] = svData;
                }
                else
                {
                    StackValue svNewProperty = core.Heap.AllocatePointer(new [] {svData});
                    core.Heap.GetHeapElement(svThis).Stack[stackIndex] = svNewProperty;
                    GCRetain(svNewProperty);

                    exe.classTable.ClassNodes[classIndex].symbols.symbolList[stackIndex].heapIndex = (int)svNewProperty.opdata;
                }
            }

            if (!isSSANode && svOldData.IsReferenceType)
            {
                var dataHeapElement = rmem.Heap.GetHeapElement(svOldData);
                if (dataHeapElement.Active && dataHeapElement.Refcount == 0)
                {
                    GCRelease(svOldData);
                }
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
            int blockId;
            int ci;
            POPM_Helper(instruction, out blockId, out ci);
        }

        private void POPLIST_Handler(Instruction instruction)
        {
            runtimeVerify(instruction.op1.IsInteger);
            int depth = (int)instruction.op1.opdata;

            runtimeVerify(instruction.op2.IsInteger);

            runtimeVerify(instruction.op3.IsBlockIndex);
            int blockId = (int)instruction.op3.opdata;
            // TODO(Jun/Jiong): Find a more reliable way to update the current block Id
            //core.DebugProps.CurrentBlockId = blockId;
            RTSymbol[] listInfo = new RTSymbol[depth];
            for (int n = 0; n < depth; ++n)
            {
                listInfo[n].Sv = rmem.Pop();
                if (listInfo[n].Sv.IsStaticVariableIndex)
                {
                    StackValue block = rmem.Pop();
                    Validity.Assert(block.IsBlockIndex);
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
            int classsccope = listInfo.Last().Sv.metaData.type;
            for (int n = listInfo.Length - 1; n >= 1; --n)
            {
                if (n == listInfo.Length - 1)
                    finalPointer = listInfo[n].Sv;
                else
                {
                    //resolve dynamic reference
                    if (listInfo[n].Sv.IsDynamic)
                    {
                        classsccope = listInfo[n + 1].Sv.metaData.type;
                        bool succeeded = ProcessDynamicVariable((listInfo[n].Dimlist != null), ref listInfo[n].Sv, classsccope);
                        //if the identifier is unbounded. Push null
                        if (!succeeded)
                        {
                            finalPointer = StackValue.Null;
                            break;
                        }
                    }

                    if (listInfo[n].Sv.IsStaticVariableIndex)
                        finalPointer = listInfo[n].Sv = GetOperandData(blockId, listInfo[n].Sv, new StackValue());
                    else
                        finalPointer = listInfo[n].Sv = core.Heap.GetHeapElement(finalPointer).Stack[(int)listInfo[n].Sv.opdata];
                }
                if (listInfo[n].Dimlist != null)
                {
                    for (int d = listInfo[n].Dimlist.Length - 1; d >= 0; --d)
                    {
                        finalPointer = listInfo[n].Sv = core.Heap.GetHeapElement(finalPointer).GetValue(listInfo[n].Dimlist[d], core);
                    }
                }
            }

            // Handle the last pointer
            StackValue tryPointer = StackValue.Null;
            StackValue data = rmem.Pop();
            GCRetain(data);
            if (!finalPointer.IsNull)
            {
                if (listInfo[0].Sv.IsDynamic)
                {
                    classsccope = listInfo[1].Sv.metaData.type;
                    bool succeeded = ProcessDynamicVariable((listInfo[0].Dimlist != null), ref listInfo[0].Sv, classsccope);
                    //if the identifier is unbounded. Push null
                    if (!succeeded)
                    {
                        tryPointer = StackValue.Null;
                    }
                    else
                    {
                        if (listInfo[0].Sv.IsStaticVariableIndex)
                        {
                            SetOperandData(listInfo[0].Sv, data, listInfo[0].BlockId);
                            ++pc;
                            return;
                        }
                        tryPointer = core.Heap.GetHeapElement(finalPointer).Stack[listInfo[0].Sv.opdata];
                    }
                }
                else if (listInfo[0].Sv.IsStaticVariableIndex)
                {
                    SetOperandData(listInfo[0].Sv, data, listInfo[0].BlockId);
                    ++pc;
                    return;
                }
                else
                {
                    tryPointer = core.Heap.GetHeapElement(finalPointer).Stack[listInfo[0].Sv.opdata];
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
                    finalPointer = core.Heap.GetHeapElement(finalPointer).GetValue(listInfo[0].Dimlist[d], core);
                tryPointer = core.Heap.GetHeapElement(finalPointer).GetValue(listInfo[0].Dimlist[0], core);
            }

            if (tryPointer.IsNull)
            { //do nothing
            }
            else if (core.Heap.GetHeapElement(tryPointer).Stack.Length == 1 &&
                !core.Heap.GetHeapElement(tryPointer).Stack[0].IsPointer &&
                !core.Heap.GetHeapElement(tryPointer).Stack[0].IsArray)
            {
                // TODO Jun:
                // Spawn GC here

                // Setting a primitive
                DX = core.Heap.GetHeapElement(tryPointer).Stack[0];
                core.Heap.GetHeapElement(tryPointer).Stack[0] = data;
            }
            else if (finalPointer.IsPointer || data.IsNull)
            {
                if (data.IsNull)
                {
                    StackValue ptr = core.Heap.AllocatePointer(new [] { data });
                    GCRetain(data);
                }

                // Setting a pointer
                int idx = (int)listInfo[0].Sv.opdata;
                DX = ArrayUtils.GetValueFromIndex(finalPointer, idx, core);
                GCRelease(DX);
                core.Heap.GetHeapElement(finalPointer).Stack[listInfo[0].Sv.opdata] = data;
            }
            else
            {
                // TODO Jun:
                // Spawn GC here
                runtimeVerify(finalPointer.IsArray);

                // Setting an array
                DX = core.Heap.GetHeapElement(finalPointer).GetValue(listInfo[0].Dimlist[0], core);
                GCRelease(DX);
                core.Heap.GetHeapElement(finalPointer).SetValue(listInfo[0].Dimlist[0], data);
            }

            ++pc;
        }

        private void MOV_Handler(Instruction instruction)
        {
            int blockId = Constants.kInvalidIndex;
            if (instruction.op2.IsVariableIndex ||
                instruction.op2.IsMemberVariableIndex ||
                instruction.op2.IsPointer ||
                instruction.op2.IsArray)
            {
                StackValue svDim = rmem.Pop();
                runtimeVerify(svDim.IsArrayDimension);

                StackValue svBlock = rmem.Pop();
                runtimeVerify(svBlock.IsBlockIndex);
                blockId = (int)svBlock.opdata;
            }

            StackValue opClass = StackValue.BuildClassIndex(Constants.kGlobalScope);
            StackValue opdata1 = GetOperandData(blockId, instruction.op2, opClass);
            SetOperandData(instruction.op1, opdata1);

            ++pc;
        }

        private void ADD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            // Need to optmize these if-elses to a table. 
            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata1.RawIntValue + opdata2.RawIntValue);

            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;

                opdata2 = StackValue.BuildDouble(value1 + value2);
            }
            else if ((opdata1.IsChar || opdata1.IsString) &&
                     (opdata2.IsChar || opdata2.IsString))
            {
                opdata2 = StringUtils.ConcatString(opdata2, opdata1, core);
            }
            else if (opdata1.IsString || opdata2.IsString)
            {
                StackValue newSV;
                if (opdata1.IsString)
                {
                    newSV = StringUtils.ConvertToString(opdata2, core, rmem);
                    opdata2 = StringUtils.ConcatString(newSV, opdata1, core);
                }
                else if (opdata2.IsString)
                {
                    newSV = StringUtils.ConvertToString(opdata1, core, rmem);
                    opdata2 = StringUtils.ConcatString(opdata2, newSV, core);
                }
            }
            else if (opdata2.IsArrayKey && opdata1.IsInteger)
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
        }
        
        private void SUB_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata2.RawIntValue - opdata1.RawIntValue);
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                double value2 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                opdata2 = StackValue.BuildDouble(value1 - value2);
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }
        
        private void MUL_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata1.opdata * opdata2.opdata);
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                opdata2 = StackValue.BuildDouble(value1 * value2);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }
        
        private void DIV_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            //division is always carried out as a double
            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double lhs = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                double rhs = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                opdata2 = StackValue.BuildDouble(lhs / rhs);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }
        
        private void MOD_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata2.RawIntValue % opdata1.RawIntValue);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }
       
        private void NEG_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            if (opdata1.IsInteger)
            {
                opdata1 = StackValue.BuildInt(-opdata1.RawIntValue);
            }
            else if (opdata1.IsDouble)
            {
                opdata1 = StackValue.BuildDouble(-opdata1.RawDoubleValue);
            }
            else 
            {
                opdata1 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata1);
            ++pc;
        }

        private void AND_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata1 = opdata1.ToBoolean(core);
            opdata2 = opdata2.ToBoolean(core);
            if (opdata1.IsNull || opdata2.IsNull)
            {
                opdata2 = StackValue.Null;
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata2.opdata != 0L && opdata1.opdata != 0L);
            }
            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }

        private void OR_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            opdata1 = opdata1.ToBoolean(core);
            opdata2 = opdata2.ToBoolean(core);
            if (opdata1.IsNull || opdata2.IsNull)
            {
                opdata2 = StackValue.Null;
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata2.opdata != 0L || opdata1.opdata != 0L);
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }

        private void NOT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            opdata1 = opdata1.ToBoolean(core);
            if (!opdata1.IsNull)
            {
                opdata1 = StackValue.BuildBoolean(opdata1.opdata == 0L);
            }

            SetOperandData(instruction.op1, opdata1);

            ++pc;
        }

        private void EQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsBoolean || opdata2.IsBoolean)
            {
                opdata1 = opdata1.ToBoolean(core);
                opdata2 = opdata2.ToBoolean(core);
                if (opdata1.IsNull || opdata2.IsNull) 
                {
                    opdata2 = StackValue.Null;
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.RawBooleanValue == opdata2.RawBooleanValue);
                }
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                    double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                    opdata2 = StackValue.BuildBoolean(MathUtils.Equals(value1, value2));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.RawIntValue == opdata2.RawIntValue);
                }
            }
            else if (opdata1.IsString && opdata2.IsString)
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
        }
        
        private void NQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsBoolean || opdata2.IsBoolean)
            {
                opdata1 = opdata1.ToBoolean(core);
                opdata2 = opdata2.ToBoolean(core);
                opdata2 = StackValue.BuildBoolean(opdata1.opdata != opdata2.opdata);
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                    double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                    opdata2 = StackValue.BuildBoolean(!MathUtils.Equals(value1, value2));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.opdata != opdata2.opdata);
                }
            }
            else if (opdata1.IsString && opdata2.IsString)
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
        }
        
        private void GT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                var value1 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                var value2 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                opdata2 = StackValue.BuildBoolean(value1 > value2);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);
            ++pc;
        }
        
        private void LT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                double value2 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                opdata2 = StackValue.BuildBoolean(MathUtils.IsLessThan(value1, value2));
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            SetOperandData(instruction.op1, opdata2);

            ++pc;
        }
        
        private void GE_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double lhs = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                    double rhs = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                    opdata2 = StackValue.BuildBoolean(MathUtils.IsGreaterThanOrEquals(lhs, rhs));
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
        }
        
        private void LE_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op2);
            StackValue opdata2 = GetOperandData(instruction.op1);

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double lhs = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                    double rhs = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                    opdata2 = StackValue.BuildBoolean(MathUtils.IsLessThanOrEquals(lhs, rhs));
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
        }

        private void ALLOCA_Handler(Instruction instruction)
        {
            StackValue op1 = instruction.op1;
            runtimeVerify(op1.IsInteger || op1.IsRegister);

            int size;
            if (op1.IsInteger)
            {
                size = (int)op1.opdata; //Number of the elements in the array
            }
            else
            {
                StackValue arraySize = GetOperandData(op1);
                runtimeVerify(arraySize.IsInteger);
                size = (int)arraySize.opdata;
            }

            runtimeVerify(Constants.kInvalidIndex != size);

            StackValue[] svs = new StackValue[size];
            for (int i = size - 1; i >= 0; i--)
            {
                StackValue value = rmem.Pop();
                GCRetain(value);
                svs[i] = value;
            }
            StackValue pointer = rmem.Heap.AllocateArray(svs);

            if (instruction.op2.IsString)
            {
                pointer = StackValue.BuildString(pointer.opdata);
            }
            rmem.Push(pointer);

            if (instruction.op3.IsReplicationGuide)
            {
                Validity.Assert(instruction.op3.RawIntValue == 0);
                core.replicationGuides.Add(new List<ReplicationGuide> {});
            }

            ++pc;
        }

        private void BOUNCE_Handler(Instruction instruction)
        {
            // We disallow language blocks inside watch window currently - pratapa
            Validity.Assert(InterpreterMode.kExpressionInterpreter != Core.ExecMode);

            runtimeVerify(instruction.op1.IsBlockIndex);
            int blockId = (int)instruction.op1.opdata;

            // Comment Jun: On a bounce, update the debug property to reflect this.
            // Before the explicit bounce, this was done in Execute() which is now no longer the case
            // as Execute is only called once during first bounce and succeeding bounce reuse the same interpreter
            core.DebugProps.CurrentBlockId = blockId;

            runtimeVerify(instruction.op2.IsInteger);

            // TODO(Jun/Jiong): Considering store the orig block id to stack frame
            core.RunningBlock = blockId;

            core.Rmem = rmem;
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PushConstructBlockId(blockId);
            }

#if ENABLE_EXCEPTION_HANDLING
                core.stackActiveExceptionRegistration.Push(core.ExceptionHandlingManager.CurrentActiveRegistration);
#endif

            int ci = Constants.kInvalidIndex;
            int fi = Constants.kInvalidIndex;
            if (rmem.Stack.Count >= StackFrame.kStackFrameSize)
            {
                StackValue sci = rmem.GetAtRelative(StackFrame.kFrameIndexClass);
                StackValue sfi = rmem.GetAtRelative(StackFrame.kFrameIndexFunction);
                if (sci.IsInteger && sfi.IsInteger)
                {
                    ci = (int)sci.opdata;
                    fi = (int)sfi.opdata;
                }
            }

#if ENABLE_EXCEPTION_HANDLING
            core.ExceptionHandlingManager.SwitchContextTo(blockId, fi, ci, pc);
#endif

            StackValue svThisPtr;
            if (rmem.CurrentStackFrame == null)
            {
                svThisPtr = StackValue.BuildPointer(Constants.kInvalidPointer);
            }
            else
            {
                svThisPtr = rmem.CurrentStackFrame.ThisPtr;
            }
            int returnAddr = pc + 1;

            Validity.Assert(Constants.kInvalidIndex != executingBlock);
            //int blockDecl = executingBlock;
            int blockDecl = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionBlock).opdata;
            int blockCaller = executingBlock;

            StackFrameType type = StackFrameType.kTypeLanguage;
            int depth = (int)rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameDepth).opdata;
            int framePointer = core.Rmem.FramePointer;

            // Comment Jun: Use the register TX to store explicit/implicit bounce state
            bounceType = CallingConvention.BounceType.kExplicit;
            TX = StackValue.BuildCallingConversion((int)CallingConvention.BounceType.kExplicit);

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
                Language bounceLangauge = exe.instrStreamList[blockId].language;
                BounceExplicit(blockId, 0, bounceLangauge, stackFrame, core.Breakpoints);
            }
            else //if (core.Breakpoints == null)
            {
                StackFrame stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth + 1, framePointer, registers, null);

                Language bounceLangauge = exe.instrStreamList[blockId].language;
                BounceExplicit(blockId, 0, bounceLangauge, stackFrame);
            }
        }

        private void CALL_Handler(Instruction instruction)
        {
            PushInterpreterProps(Properties);

            runtimeVerify(instruction.op1.IsFunctionIndex);
            int fi = (int)instruction.op1.opdata;

            runtimeVerify(instruction.op2.IsClassIndex);
            int ci = (int)instruction.op2.opdata;

            rmem.Pop();

            StackValue svBlock = rmem.Pop();
            int blockId = (int)svBlock.opdata;
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                rmem.PushConstructBlockId(blockId);
            }
            SX = svBlock;

            ProcedureNode fNode;
            if (ci != Constants.kInvalidIndex)
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
                core.DebugProps.SetUpCallrForDebug(core, this, fNode, pc, true);
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

            if ((instruction.op3.IsInteger) &&
               (instruction.op3.opdata >= 0))
            {
                // thisptr should be the pointer to the instance of derive class
                svThisPointer = rmem.GetAtRelative(StackFrame.kFrameIndexThisPtr);
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
                    bool hasGuide = rmem.Stack[stackindex].IsReplicationGuide;
                    if (hasGuide)
                    {
                        var replicationGuideList = new List<int>();

                        // Retrieve replication guides
                        value = rmem.Stack[stackindex--];
                        runtimeVerify(value.IsReplicationGuide);

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
                rmem.PopFrame(fNode.argTypeList.Count);
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
        }

        private void CALLC_Handler()
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PushConstructBlockId(-1);
            }
            throw new NotImplementedException();
        }

        protected virtual void CALLR_Handler(Instruction instruction)
        {
            bool isDynamicCall = instruction.op1.IsDynamic;
            bool isMemberFunctionPointer = false;
            Instruction instr = instruction;

            if (isDynamicCall)
            {
                //a new copy of instruction. this will be modified if it is 
                // dynamic call
                instr = new Instruction();
                instr.op1 = instruction.op1;
                instr.op2 = instruction.op2;
                instr.op3 = instruction.op3;
                instr.debug = instruction.debug;
                instr.opCode = instruction.opCode;

                bool succeeded = ResolveDynamicFunction(instr, out isMemberFunctionPointer);
                if (!succeeded)
                {
                    RX = StackValue.Null;
                    ++pc;
                    return;
                }
            }

            runtimeVerify(instr.op1.IsFunctionIndex);
            int functionIndex = (int)instr.op1.opdata;

            runtimeVerify(instr.op2.IsClassIndex);
            int classIndex = (int)instr.op2.opdata;

            runtimeVerify(instr.op3.IsInteger);
            int depth = (int)instr.op3.opdata;

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
            if (isMemberFunctionPointer)
            {
                RX = CallrForMemberFunction(classIndex, functionIndex, instr.debug != null, ref explicitCall);
            }
            else if (!core.Options.IsDeltaExecution)
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
                    core.RuntimeStatus.LogWarning(WarningID.kReplicationWarning, e.Message);
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
                    int newpc = Constants.kInvalidIndex;
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
                        pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;

                        ReturnSiteGC(blockId, classIndex, functionIndex);

                        rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                        int localCount, paramCount;
                        GetLocalAndParamCount(blockId, classIndex, functionIndex, out localCount, out paramCount);
                        rmem.PopFrame(StackFrame.kStackFrameSize + localCount + paramCount);

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
        }

        private void RETC_Handler()
        {
            runtimeVerify(rmem.ValidateStackFrame());

            RX = rmem.GetAtRelative(StackFrame.kFrameIndexThisPtr);

            int ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
            int fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;

            pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;

            // block id is used in ReturnSiteGC to get the procedure node if it is not a member function 
            // not meaningful here, because we are inside a constructor
            int blockId = (int)SX.opdata;

            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                ReturnSiteGC(blockId, ci, fi);
            }


            RestoreFromCall();
            core.RunningBlock = executingBlock;

            // If we're returning from a block to a function, the instruction stream needs to be restored.
            StackValue sv = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterTX);
            Validity.Assert(sv.IsCallingConvention);
            CallingConvention.CallType callType = (CallingConvention.CallType)sv.opdata;
            bool explicitCall = CallingConvention.CallType.kExplicit == callType || CallingConvention.CallType.kExplicitBase == callType;
            IsExplicitCall = explicitCall;

            List<bool> execStateRestore = new List<bool>();
            if (!core.Options.IDEDebugMode || core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                int localCount;
                int paramCount;
                GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

                execStateRestore = RetrieveExecutionStatesFromStack(localCount, paramCount);

                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                rmem.PopFrame(StackFrame.kStackFrameSize + localCount + paramCount + execStateRestore.Count);

                if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    RestoreRegistersFromStackFrame();

                    bounceType = (CallingConvention.BounceType)TX.opdata;
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

            // This resotring execution states is only permitted if the current scope is still in a function
            //if (currentScopeFunction != Constants.kGlobalScope)
            {
                RestoreGraphNodeExecutionStates(procNode, execStateRestore);
            }
        }

        private void RETB_Handler()
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PopConstructBlockId();
            }

            if (!core.Options.IsDeltaExecution || (core.Options.IsDeltaExecution && 0 != core.RunningBlock))
            {
                GCCodeBlock(core.RunningBlock);
            }

            if (CallingConvention.BounceType.kExplicit == bounceType)
            {
                RestoreFromBounce();
                core.RunningBlock = executingBlock;
            }

            if (CallingConvention.BounceType.kImplicit == bounceType)
            {
                pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;
                terminate = true;
            }


            StackFrameType type;

            // Comment Jun: Just want to see if this is the global rerb, in which case we dont retrieve anything
            //if (executingBlock > 0)
            {
                StackValue svCallerType = rmem.GetAtRelative(StackFrame.kFrameIndexCallerStackFrameType);
                type = (StackFrameType)svCallerType.opdata;
            }

            // Pop the frame as we are adding stackframes for language blocks as well - pratapa
            // Do not do this for the final Retb 
            //if (core.RunningBlock != 0)
            if (!core.Options.IDEDebugMode || core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                rmem.PopFrame(StackFrame.kStackFrameSize);

                if (bounceType == CallingConvention.BounceType.kExplicit)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    RestoreRegistersFromStackFrame();

                    bounceType = (CallingConvention.BounceType)TX.opdata;

#if ENABLE_EXCEPTION_HANDLING
                    core.ExceptionHandlingManager.CurrentActiveRegistration = core.stackActiveExceptionRegistration.Pop();
                    if (core.ExceptionHandlingManager.IsStackUnwinding)
                    {
                    #region __MERGE_WITH_STACKUNWIND
                        // The excecution of last langage block is interrupted
                        // abnormally because of stack unwinding, so we need to 
                        // run GC to reclaim those allocated memory.
                        GCCodeBlock(core.RunningBlock);

                        int newpc = Constants.kInvalidIndex;
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
                    StackValue sv = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterTX);
                    Validity.Assert(sv.IsCallingConvention);
                    CallingConvention.CallType callType = (CallingConvention.CallType)sv.opdata;
                    if (CallingConvention.CallType.kExplicit == callType)
                    {
                        int callerblock = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionBlock).opdata;
                        istream = exe.instrStreamList[callerblock];
                    }
                }
            }
            Properties = PopInterpreterProps();
        }

        private void RETCN_Handler(Instruction instruction)
        {
            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                core.Rmem.PopConstructBlockId();
            }

            StackValue op1 = instruction.op1;
            runtimeVerify(op1.IsBlockIndex);
            int blockId = (int)op1.opdata;


            CodeBlock codeBlock = core.CompleteCodeBlockList[blockId];
            runtimeVerify(codeBlock.blockType == CodeBlockType.kConstruct);
            GCCodeBlock(blockId);
            pc++;
        }

        private List<bool> RetrieveExecutionStatesFromStack(int localSize, int paramSize)
        {
            // Retrieve the execution execution states 
            List<bool> execStateRestore = new List<bool>();
            int execstates = (int)rmem.GetAtRelative(StackFrame.kFrameIndexExecutionStates).opdata;
            if (execstates > 0)
            {
                int offset = StackFrame.kStackFrameSize + localSize + paramSize;
                for (int n = 0; n < execstates; ++n)
                {
                    int relativeIndex = -offset - n - 1;
                    StackValue svState = rmem.GetAtRelative(relativeIndex);
                    Validity.Assert(svState.IsBoolean);
                    execStateRestore.Add(svState.opdata == 0 ? false : true);
                }
            }
            return execStateRestore;
        }

        private void RestoreGraphNodeExecutionStates(ProcedureNode procNode, List<bool> execStateRestore)
        {
            if (execStateRestore.Count > 0 )
            {
                Validity.Assert(execStateRestore.Count == procNode.GraphNodeList.Count);
                for (int n = 0; n < execStateRestore.Count; ++n)
                {
                    procNode.GraphNodeList[n].isDirty = execStateRestore[n];
                }
            }
        }

        private void RETURN_Handler()
        {
            runtimeVerify(rmem.ValidateStackFrame());

            int ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
            int fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;

            int blockId = (int)SX.opdata;

            StackValue svBlockDecl = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterSX);
            Validity.Assert(svBlockDecl.IsBlockIndex);
            blockId = (int)svBlockDecl.opdata;

            ProcedureNode procNode = GetProcedureNode(blockId, ci, fi);

            if (core.Options.ExecuteSSA)
            {
                if (core.Options.GCTempVarsOnDebug && core.Options.IDEDebugMode)
                {
                    // GC anonymous variables in the return stmt
                    if (null != Properties.executingGraphNode && !Properties.executingGraphNode.IsSSANode())
                    {
                        GCSymbols(Properties.executingGraphNode.symbolListWithinExpression);
                        Properties.executingGraphNode.symbolListWithinExpression.Clear();
                    }
                }
            }

            pc = (int)rmem.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;
            executingBlock = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunctionCallerBlock).opdata;

            if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                ReturnSiteGC(blockId, ci, fi);
            }

            RestoreFromCall();
            core.RunningBlock = executingBlock;


            // If we're returning from a block to a function, the instruction stream needs to be restored.
            StackValue sv = rmem.GetAtRelative(StackFrame.kFrameIndexRegisterTX);
            Validity.Assert(sv.IsCallingConvention);
            CallingConvention.CallType callType = (CallingConvention.CallType)sv.opdata;
            bool explicitCall = CallingConvention.CallType.kExplicit == callType;
            IsExplicitCall = explicitCall;


            List<bool> execStateRestore = new List<bool>();
            if (!core.Options.IDEDebugMode || core.ExecMode == InterpreterMode.kExpressionInterpreter)
            {
                // Get stack frame size
                int localCount;
                int paramCount;
                GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

                execStateRestore = RetrieveExecutionStatesFromStack(localCount, paramCount);

                // Pop the stackframe
                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;

                // Get the size of the stackframe and all variable size contents (local, args and exec states)
                int stackFrameSize = StackFrame.kStackFrameSize + localCount + paramCount + execStateRestore.Count;
                rmem.PopFrame(stackFrameSize);

                if (core.ExecMode != InterpreterMode.kExpressionInterpreter)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    RestoreRegistersFromStackFrame();

                    bounceType = (CallingConvention.BounceType)TX.opdata;
                }
            }


            terminate = !explicitCall;

            // Comment Jun: Dispose calls are always implicit and need to terminate
            // TODO Jun: This instruction should not know about dispose
            bool isDispose = CoreUtils.IsDisposeMethod(procNode.name);
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

            RestoreGraphNodeExecutionStates(procNode, execStateRestore);
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
                    RX = core.Heap.AllocateArray(core.ContinuationStruct.RunningResult, null);
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
                            bool wasPopped = RestoreDebugPropsOnReturnFromBuiltIns();
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
        }

        private void CJMP_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            if (opdata1.IsDouble)
            {
                if (opdata1.RawDoubleValue.Equals(0))
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
                if (opdata1.IsPointer)
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
        }

        private void JMP_EQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (opdata1.IsDouble || opdata2.IsDouble)
            {
                double lhs = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                double rhs = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;

                if (Math.Equals(lhs, rhs))
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
        }

        private void JMP_GT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            bool isGT;
            if (opdata1.IsDouble || opdata2.IsDouble)
            {
                double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                isGT = value1 > value2;
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
        }

        private void JMP_GTEQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (opdata1.IsDouble || opdata2.IsDouble)
            {
                double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                if (MathUtils.IsGreaterThanOrEquals(value1, value2))
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
        }

        private void JMP_LT_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (opdata1.IsDouble || opdata2.IsDouble)
            {
                var value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                var value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                if (value1 < value2)
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
        }

        private void JMP_LTEQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (opdata1.IsDouble || opdata2.IsDouble)
            {
                double value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                double value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                if (MathUtils.IsLessThanOrEquals(value1, value2))
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
        }

        private void JMP_NEQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            StackValue opdata2 = GetOperandData(instruction.op2);

            if (opdata1.IsDouble || opdata2.IsDouble)
            {
                var value1 = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
                var value2 = opdata2.IsDouble ? opdata2.RawDoubleValue : opdata2.RawIntValue;
                if (!MathUtils.Equals(value1, value2))
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
        }

        private void JLZ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            var opvalue = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;

            if (opvalue < 0)
            {
                pc = (int)instruction.op2.opdata;
            }
            else
            {
                ++pc;
            }
        }

        private void JGZ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);
            var value = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;

            if (value > 0)
            {
                pc = (int)instruction.op2.opdata;
            }
            else
            {
                ++pc;
            }
        }

        private void JZ_Handler(Instruction instruction)
        {
            StackValue opdata1 = GetOperandData(instruction.op1);

            var opvalue = opdata1.IsDouble ? opdata1.RawDoubleValue : opdata1.RawIntValue;
            if (MathUtils.Equals(opvalue, 0))
            {
                pc = (int)instruction.op2.opdata;
            }
            else
            {
                ++pc;
            }
        }

        private void CAST_Handler()
        {
            ++pc;
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
            runtimeVerify(instruction.op1.IsInteger);
            int exprID = (int)instruction.op1.opdata;


            // The SSA assignment flag
            runtimeVerify(instruction.op2.IsInteger);
            bool isSSA = (1 == (int)instruction.op2.opdata);

            runtimeVerify(instruction.op3.IsInteger);
            int modBlkID = (int)instruction.op3.opdata;


            // The current function and class scope
            int ci = Constants.kInvalidIndex;
            int fi = Constants.kGlobalScope;
            bool isInFunction = IsInsideFunction();

            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);
                {
                    isInFunction = core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun);
                }
            }

            if (isInFunction)
            {
                ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
                fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;
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
                            var symbols = Properties.executingGraphNode.symbolListWithinExpression;

                            if (isSetter)
                            {
                                int count = symbols.Count;
                                if (count > 0)
                                {
                                    symbols = symbols.Take(count - 1).ToList();
                                }
                            }

                            GCSymbols(symbols);
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

            // Find dependent nodes and mark them dirty
            int reachableNodes = UpdateGraph(exprID, modBlkID, isSSA);

            if (core.Options.ApplyUpdate)
            {
                // Go to the first dirty pc
                SetupNextExecutableGraph(fi, ci);
            }
            else
            {
                // Go to the next pc
                pc++;

                // Given the next pc, get the next graphnode to execute and mark it clean
                if (core.Options.IsDeltaExecution)
                {
                    // On delta execution, it is possible that the next graphnode is clean
                    // Retrieve the next dirty graphnode given the pc
                    // Associative update is handled when ApplyUpdate = true
                    Properties.executingGraphNode = istream.dependencyGraph.GetFirstDirtyGraphNode(pc, ci, fi);
                }
                else
                {
                    // On normal execution, just retrieve the graphnode associated with pc
                    // Associative update is handled in jdep
                    Properties.executingGraphNode = istream.dependencyGraph.GetGraphNode(pc, ci, fi);
                }

                if (Properties.executingGraphNode != null)
                {
                    Properties.executingGraphNode.isDirty = false;
                    pc = Properties.executingGraphNode.updateBlock.startpc;
                }
            }
            GC();
            return;
        }

        private void JDEP_Handler(Instruction instruction)
        {
            // The current function and class scope
            int ci = DSASM.Constants.kInvalidIndex;
            int fi = DSASM.Constants.kGlobalScope;
            bool isInFunction = IsInsideFunction();

            if (core.Options.IDEDebugMode && core.ExecMode != InterpreterMode.kExpressionInterpreter)
            {
                Validity.Assert(core.DebugProps.DebugStackFrame.Count > 0);
                isInFunction = core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun);
            }

            if (isInFunction)
            {
                ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
                fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;
            }
            SetupNextExecutableGraph(fi, ci);
        }

        private void PUSHDEP_Handler(Instruction instruction)
        {
            // The symbol block
            runtimeVerify(instruction.op1.IsBlockIndex);
            int block = (int)instruction.op1.opdata;

            runtimeVerify(instruction.op2.IsInteger);
            int depth = (int)instruction.op2.opdata;

            // The symbol and its class index
            runtimeVerify(instruction.op3.IsClassIndex);
            int classIndex = (int)instruction.op3.opdata;

            // Get the identifier list
            List<StackValue> symbolList = new List<StackValue>();
            for (int n = 0; n < depth; ++n)
            {
                // TODO Jun: use the proper ID for this
                StackValue sv = rmem.Pop();
                runtimeVerify(sv.IsInteger);
                symbolList.Add(sv);
            }
            symbolList.Reverse();

            // TODO Jun: use the proper ID for this
            runtimeVerify(symbolList[0].IsInteger);
            int symindex = (int)symbolList[0].opdata;

            if (Constants.kInvalidIndex != symindex)
            {
                SymbolNode symnode;
                if (Constants.kInvalidIndex != classIndex)
                {
                    symnode = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symindex];
                }
                else
                {
                    symnode = exe.runtimeSymbols[block].symbolList[symindex];
                }

                AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                updateNode.symbol = symnode;
                updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;

                // Build the first symbol of the modified ref
                AssociativeGraph.UpdateNodeRef modifiedRef = new AssociativeGraph.UpdateNodeRef();
                modifiedRef.nodeList.Add(updateNode);
                modifiedRef.block = symnode.runtimeTableIndex;

                // Update the current type
                classIndex = symnode.datatype.UID;

                // Build the rest of the list of symbols of the modified ref
                for (int n = 1; n < symbolList.Count; ++n)
                {
                    // TODO Jun: This should be a memvarindex address type
                    runtimeVerify(symbolList[n].IsInteger);
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
                if (Constants.kInvalidIndex != symnode.classScope
                    && Constants.kInvalidIndex == symnode.functionIndex)
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
                    if (modifiedRef.Equals(istream.xUpdateList[i]))
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
        }

        private void DEPX_Handler()
        {
            runtimeVerify(Language.kAssociative == istream.language);

            // The current function and class scope
            int ci = Constants.kInvalidIndex;
            int fi = Constants.kGlobalScope;
            if (fepRun)
            {
                ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
                fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;
            }

            // Set the next graph to be executed
            SetupNextExecutableGraph(fi, ci);
        }

        private void THROW_Handler()
        {
#if ENABLE_EXCEPTION_HANDLING
            runtimeVerify(instruction.op1.IsBlockIndex);
            int blockId = (int)instruction.op1.opdata;

            runtimeVerify(instruction.op2.IsClassIndex);
            int classScope = (int)instruction.op2.opdata;

            runtimeVerify(instruction.op3.IsFunctionIndex);
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
            int newpc = Constants.kInvalidIndex;
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
                        rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                        rmem.PopFrame(StackFrame.kStackFrameSize);


                        // Restoring the registers require the current frame pointer of the stack frame 
                        RestoreRegistersFromStackFrame();

                        bounceType = (ProtoCore.DSASM.CallingConvention.BounceType)TX.opdata;

                        core.ExceptionHandlingManager.CurrentActiveRegistration = core.stackActiveExceptionRegistration.Pop();

            #region __MERGE_WITH_STACKUNWIND

                        // The excecution of last langage block is interrupted
                        // abnormally because of stack unwinding, so we need to 
                        // run GC to reclaim those allocated memory.
                        GCCodeBlock(core.RunningBlock);

                        newpc = Constants.kInvalidIndex;
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

                        int ci = (int)rmem.GetAtRelative(StackFrame.kFrameIndexClass).opdata;
                        int fi = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFunction).opdata;

                        int localCount = 0;
                        int paramCount = 0;
                        GetLocalAndParamCount(executingBlock, ci, fi, out localCount, out paramCount);

                        rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.kFrameIndexFramePointer).opdata;
                        rmem.PopFrame(StackFrame.kStackFrameSize + localCount + paramCount);
                    }
                }
            }
            return;
#else
            throw new NotImplementedException();
#endif
        }

        private void SETEXPUID_Handler()
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
        }

        #endregion

        private void Exec(Instruction instruction)
        {
            switch (instruction.opCode)
            {
                case OpCode.ALLOC:
                    {
                        ALLOC_Handler();
                        return;
                    }

                case OpCode.ALLOCC:
                    {
                        ALLOCC_Handler(instruction);
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
                        POPB_Handler();
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
                        POPG_Handler();
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

                case OpCode.SUB:
                    {
                        SUB_Handler(instruction);
                        return;
                    }

                case OpCode.MUL:
                    {
                        MUL_Handler(instruction);
                        return;
                    }

                case OpCode.DIV:
                    {
                        DIV_Handler(instruction);
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

                case OpCode.NQ:
                    {
                        NQ_Handler(instruction);
                        return;
                    }

                case OpCode.GT:
                    {
                        GT_Handler(instruction);
                        return;
                    }

                case OpCode.LT:
                    {
                        LT_Handler(instruction);
                        return;
                    }

                case OpCode.GE:
                    {
                        GE_Handler(instruction);
                        return;
                    }

                case OpCode.LE:
                    {
                        LE_Handler(instruction);
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
                        CALLC_Handler();
                        return;
                    }

                case OpCode.CALLR:
                    {
                        CALLR_Handler(instruction);
                        return;
                    }

                case OpCode.RETC:
                    {
                        RETC_Handler();
                        return;
                    }

                case OpCode.RETB:
                    {
                        RETB_Handler();
                        return;
                    }

                case OpCode.RETCN:
                    {
                        RETCN_Handler(instruction);
                        return;
                    }

                case OpCode.RETURN:
                    {
                        RETURN_Handler();
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

                case OpCode.JDEP:
                    {
                        JDEP_Handler(instruction);
                        return;
                    }

                case OpCode.CAST:
                    {
                        CAST_Handler();
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
                        DEPX_Handler();
                        return;
                    }

                case OpCode.THROW:
                    {
                        THROW_Handler();
                        return;
                    }

                case OpCode.SETEXPUID:
                    {
                        SETEXPUID_Handler();
                        return;
                    }
                default: //Unknown OpCode
                    throw new NotImplementedException("Unknown Op code, NIE Marker: {D6028708-CD47-4D0B-97FC-E681BD65DB5C}");
            }
        }

        [Conditional("GC_MARK_AND_SWEEP")]
        private void GC()
        {
            var currentFramePointer = rmem.FramePointer;
            var frames = rmem.GetStackFrames();
            var blockId = executingBlock;
            var gcRoots = new List<StackValue>();

            // Now garbage collection only happens on the top most block. 
            // We will loose this limiation soon.
            if (blockId != 0 || 
                rmem.CurrentStackFrame.StackFrameType != StackFrameType.kTypeLanguage)
            {
                return;
            }

#if DEBUG
            var gcRootSymbolNames = new List<string>();
#endif
            var isInNestedImperativeBlock = frames.Any(f =>
                {
                    var callerBlockId = f.FunctionCallerBlock;
                    var cbn = core.CompleteCodeBlockList[callerBlockId];
                    return cbn.language == Language.kImperative;
                });

            if (isInNestedImperativeBlock)
            {
                return;
            }

            foreach (var stackFrame in frames)
            {
                Validity.Assert(blockId != Constants.kInvalidIndex);
                var functionScope = stackFrame.FunctionScope;
                var classScope = stackFrame.ClassScope;

                IEnumerable<SymbolNode> symbolsInScope;
                if (blockId == 0)
                {
                    ICollection<SymbolNode> symbols;
                    if (classScope == Constants.kGlobalScope)
                    {
                        symbols = exe.runtimeSymbols[blockId].symbolList.Values;
                    }
                    else
                    {
                        symbols = core.ClassTable.ClassNodes[classScope].symbols.symbolList.Values;
                    }

                    symbolsInScope = symbols.Where(s => s.functionIndex == functionScope);
                }
                else
                {
                    // Call some language block, so symbols should come from
                    // the corresponding language block. 
                    var symbols = exe.runtimeSymbols[blockId]
                                     .symbolList
                                     .Values
                                     .Where(s => s.absoluteFunctionIndex == functionScope &&
                                                 s.absoluteClassScope == classScope);

                    List<SymbolNode> blockSymbols = new List<SymbolNode>();
                    blockSymbols.AddRange(symbols);

                    // One kind of block is construct block. This kind of block
                    // is not true block because the VM doesn't push a stack
                    // frame for it, and all variables defined in construct
                    // block are visible in language block. For example, 
                    // if-else, for-loop
                    var workingList = new Stack<int>();
                    workingList.Push(blockId);

                    while (workingList.Any())
                    {
                        blockId = workingList.Pop();
                        var block = core.CompleteCodeBlockList[blockId];

                        foreach (var child in block.children)
                        {
                            if (child.blockType != CodeBlockType.kConstruct)
                            {
                                continue;
                            }

                            var childBlockId = child.codeBlockId;
                            workingList.Push(childBlockId);

                            var childSymbols = exe.runtimeSymbols[childBlockId]
                                                  .symbolList
                                                  .Values
                                                  .Where(s => s.absoluteFunctionIndex == functionScope && 
                                                              s.absoluteClassScope == classScope);
                            blockSymbols.AddRange(childSymbols);
                        }
                    }

                    symbolsInScope = blockSymbols;
                }

                foreach (var symbol in symbolsInScope)
                {
                    StackValue value = rmem.GetSymbolValueOnFrame(symbol, currentFramePointer);
                    if (value.IsReferenceType)
                    {
                        gcRoots.Add(value);
#if DEBUG
                        gcRootSymbolNames.Add(symbol.name);
#endif
                    }
                }
#if DEBUG
                gcRootSymbolNames.Add("__thisptr");
#endif
                gcRoots.Add(stackFrame.ThisPtr);
                blockId = stackFrame.FunctionCallerBlock;
                currentFramePointer = stackFrame.FramePointer;
            }

            gcRoots.Add(RX);

            rmem.GC(gcRoots, this);
        }
    }
}
