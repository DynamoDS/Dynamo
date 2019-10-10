using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Exceptions;
using ProtoCore.Lang.Replication;
using ProtoCore.Properties;
using ProtoCore.Runtime;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    public class Executive : IExecutive
    {
        private readonly bool enableLogging = true;


        private readonly RuntimeCore runtimeCore;
        public RuntimeCore RuntimeCore
        {
            get
            {
                return runtimeCore;
            }
        }

        public Executable exe { get; set; }
        public Language executingLanguage = Language.Associative;

        protected int pc = Constants.kInvalidPC;
        public int PC { get { return pc; } }

        private bool fepRun;
        bool terminate;

        private InstructionStream istream;
        public RuntimeMemory rmem { get; set; }

        public StackValue RX { get; set; }
        public StackValue TX { get; set; }

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

        public List<AssociativeGraph.GraphNode> deferedGraphNodes { get; private set; }

        /// <summary>
        /// This is the list of graphnodes that are reachable from the current state
        /// This is updated for every bounce and function call
        /// </summary>
        private List<AssociativeGraph.GraphNode> graphNodesInProgramScope;
        
        public Executive(RuntimeCore runtimeCore, bool isFep = false)
        {
            IsExplicitCall = false;
            Validity.Assert(runtimeCore != null);

            this.runtimeCore = runtimeCore;
            enableLogging = runtimeCore.Options.Verbose;

            exe = runtimeCore.DSExecutable;
            istream = null;

            fepRun = isFep;
            Properties = new InterpreterProperties();

            rmem = runtimeCore.RuntimeMemory;

            // Execute DS View VM Log
            //
            debugFlags = (int)DebugFlags.ENABLE_LOG;

            bounceType = CallingConvention.BounceType.Implicit;

            deferedGraphNodes = new List<AssociativeGraph.GraphNode>();
        }

        /// <summary>
        /// Cache the graphnodes in scope
        /// </summary>
        private void SetupGraphNodesInScope()
        {
            int ci = Constants.kInvalidIndex;
            int fi = Constants.kInvalidIndex;
            if (!IsGlobalScope())
            {
                ci = rmem.CurrentStackFrame.ClassScope;
                fi = rmem.CurrentStackFrame.FunctionScope;
            }
            graphNodesInProgramScope = istream.dependencyGraph.GetGraphNodesAtScope(ci, fi);
        }

        /// <summary>
        /// Bounce instantiates a new Executive 
        /// Execution jumps to the new executive
        /// This iverload handles debugger properties when bouncing
        /// </summary>
        /// <param name="exeblock"></param>
        /// <param name="entry"></param>
        /// <param name="context"></param>
        /// <param name="stackFrame"></param>
        /// <param name="locals"></param>
        /// <param name="exec"></param>
        /// <param name="fepRun"></param>
        /// <param name="breakpoints"></param>
        /// <returns></returns>
        public StackValue Bounce(int exeblock, int entry, StackFrame stackFrame, int locals = 0, bool fepRun = false, Executive exec = null, List<Instruction> breakpoints = null)
        {
            if (stackFrame != null)
            {
                rmem.PushFrameForLocals(locals);
                rmem.PushStackFrame(stackFrame);
                runtimeCore.DebugProps.SetUpBounce(exec, stackFrame.FunctionCallerBlock, stackFrame.ReturnPC);
            }
            return runtimeCore.ExecutionInstance.Execute(exeblock, entry, fepRun, breakpoints);
        }

        /// <summary>
        /// Bounce to an existing executive
        /// </summary>
        /// <param name="exeblock"></param>
        /// <param name="entry"></param>
        /// <param name="context"></param>
        /// <param name="stackFrame"></param>
        /// <param name="locals"></param>
        /// <param name="fepRun"></param>
        /// <param name="exec"></param>
        /// <param name="breakpoints"></param>
        /// <returns></returns>
        public StackValue BounceUsingExecutive(
           DSASM.Executive executive,
           int exeblock,
           int entry,
           StackFrame stackFrame,
           int locals = 0,
           bool fepRun = false,
           DSASM.Executive exec = null,
           List<Instruction> breakpoints = null)
        {
            if (stackFrame != null)
            {
                rmem.PushFrameForLocals(locals);
                rmem.PushStackFrame(stackFrame);
                runtimeCore.DebugProps.SetUpBounce(exec, stackFrame.FunctionCallerBlock, stackFrame.ReturnPC);
            }
            executive.Execute(exeblock, entry, breakpoints);
            return executive.RX;
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
            StackValue svFunctionBlock = rmem.GetAtRelative(StackFrame.FrameIndexFunctionBlockIndex);
            Validity.Assert(svFunctionBlock.IsBlockIndex);

            int exeblock = svFunctionBlock.BlockIndex;

            fepRun = true;
            SetupExecutiveForCall(exeblock, entry);
        }

        // TODO Jun: Optimization - instead of inspecting the stack, just store the 'is in function' flag in the stackframe
        // Performance would only siffer if you have so a huge number of nested language blocks
        // TODO Jun: Optimization - instead of inspecting the stack, just store the 'is in function' flag in the stackframe
        // Performance would only siffer if you have so a huge number of nested language blocks
        private bool GetCurrentScope(out int classIndex, out int functionIndex)
        {
            int fpRestore = rmem.FramePointer;
            StackValue svFrameType = rmem.GetAtRelative(StackFrame.FrameIndexStackFrameType);
            classIndex = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
            functionIndex = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;
            while (svFrameType.IsFrameType)
            {
                if (svFrameType.FrameType == StackFrameType.Function)
                {
                    rmem.FramePointer = fpRestore;
                    return true;
                }

                StackValue framePointer = rmem.GetAtRelative(StackFrame.FrameIndexFramePointer);
                rmem.FramePointer = (int)framePointer.IntegerValue;
                if (rmem.FramePointer < StackFrame.StackFrameSize)
                {
                    break;
                }
                
                // The top of stack is for global variables, so it is possible to 
                // be an invalid frame.
                svFrameType = rmem.GetAtRelative(StackFrame.FrameIndexStackFrameType);
                if (svFrameType.IsFrameType)
                {
                    classIndex = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
                    functionIndex = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;
                }
            }
            rmem.FramePointer = fpRestore;
            return false;
        }

        private void RestoreFromCall()
        {
            int ci = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
            int fi = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;
            int blockId = rmem.GetAtRelative(StackFrame.FrameIndexBlockIndex).BlockIndex;
            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                ReturnSiteGC(blockId, ci, fi);
            }

            ProcedureNode procNode = GetProcedureNode(blockId, ci, fi);
            if (procNode.IsConstructor)
            {
                RX = rmem.GetAtRelative(StackFrame.FrameIndexThisPtr);
            }
            else
            {
                RX = rmem.Pop();
            }

            pc = (int)rmem.GetAtRelative(StackFrame.FrameIndexReturnAddress).IntegerValue;
            executingBlock = rmem.GetAtRelative(StackFrame.FrameIndexCallerBlockIndex).BlockIndex;
            istream = exe.instrStreamList[executingBlock];
            runtimeCore.RunningBlock = executingBlock;

            StackFrameType callerType = rmem.GetAtRelative(StackFrame.FrameIndexCallerStackFrameType).FrameType;
            fepRun = callerType == StackFrameType.Function;

            // If we're returning from a block to a function, the instruction stream needs to be restored.
            StackValue sv = rmem.GetAtRelative(StackFrame.FrameIndexTX);
            CallingConvention.CallType callType = sv.CallType;
            IsExplicitCall = CallingConvention.CallType.Explicit == callType || CallingConvention.CallType.ExplicitBase == callType;

            List<bool> execStateRestore = new List<bool>();
            if (!runtimeCore.Options.IDEDebugMode || runtimeCore.Options.RunMode == InterpreterMode.Expression)
            {
                int localCount = procNode.LocalCount;
                int paramCount = procNode.ArgumentTypes.Count;

                execStateRestore = RetrieveExecutionStatesFromStack(localCount, paramCount);

                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.FrameIndexFramePointer).IntegerValue;
                rmem.PopFrame(StackFrame.StackFrameSize + localCount + paramCount + execStateRestore.Count);

                if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    ResumeRegistersFromStackExceptRX();
                    bounceType = (CallingConvention.BounceType)TX.CallType;
                }

                if (execStateRestore.Any())
                {
                    Validity.Assert(execStateRestore.Count == procNode.GraphNodeList.Count);
                    for (int n = 0; n < execStateRestore.Count; ++n)
                    {
                        procNode.GraphNodeList[n].isDirty = execStateRestore[n];
                    }
                }
            }

            terminate = !IsExplicitCall;
            bool isDispose = CoreUtils.IsDisposeMethod(procNode.Name);
            if (isDispose)
            {
                terminate = true;
            }

            // Let the return graphNode always be active 
            if (!procNode.IsConstructor && null != Properties.executingGraphNode)
            {
                Properties.executingGraphNode.isDirty = true;
            }
            Properties = PopInterpreterProps();

            if (IsExplicitCall)
            {
                bool wasDebugPropsPopped = false;
                if (!isDispose)
                {
                    wasDebugPropsPopped = DebugReturn(procNode, pc);
                }

                // This condition should only be reached in the following cases:
                // 1. Debug StepOver or External Function call in non-replicating mode
                // 2. Normal execution in Serial (explicit call), non-replicating mode
                if (!procNode.IsConstructor && !wasDebugPropsPopped)
                {
                    RX = CallSite.PerformReturnTypeCoerce(procNode, runtimeCore, RX);
                    if (CoreUtils.IsDotMethod(procNode.Name))
                    {
                        RX = IndexIntoArray(RX, Properties.functionCallDotCallDimensions);
                        rmem.PopFrame(Constants.kDotCallArgCount);
                    }

                    if (Properties.DominantStructure != null)
                    {
                        RX = AtLevelHandler.RestoreDominantStructure(RX, Properties.DominantStructure, null, runtimeCore); 
                    }
                }
            }

            SetupGraphNodesInScope();          
        }

        private void RestoreFromBounce()
        {
            // Comment Jun:
            // X-lang dependency should be done for all languages 
            // as they can potentially trigger parent block updates 

            // Propagate only on lang block bounce (non fep)
            // XLangUpdateDependencyGraph requires the executingBlock to be the current running block (the block before leaving language block)
            XLangUpdateDependencyGraph(executingBlock);

            executingBlock = rmem.GetAtRelative(StackFrame.FrameIndexCallerBlockIndex).BlockIndex;
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
            if (rmem.FramePointer >= StackFrame.StackFrameSize)
            {
                fepRun = false;
                StackFrameType callerType = rmem.GetAtRelative(StackFrame.FrameIndexCallerStackFrameType).FrameType;
                if (callerType == StackFrameType.Function)
                {
                    fepRun = true;
                }
            }

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            Validity.Assert(null != istream.instrList);

            pc = (int)rmem.GetAtRelative(StackFrame.FrameIndexReturnAddress).IntegerValue;
        }

        public void PushInterpreterProps(InterpreterProperties properties)
        {
            runtimeCore.InterpreterProps.Push(new InterpreterProperties(properties));
        }

        public InterpreterProperties PopInterpreterProps()
        {
            return runtimeCore.InterpreterProps.Pop();
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

            //  Entering a nested block requires all the nodes of that block to be executed
            if (executingBlock > 0)
            {
                ProtoCore.AssociativeEngine.Utils.MarkAllGraphNodesDirty(executingBlock, graphNodesInProgramScope);
            }

            if (fepRun)
            {
                UpdateMethodDependencyGraph(pc, fi, ci);
            }
            else
            {
                if (!runtimeCore.Options.IsDeltaExecution)
                {
                    pc = SetupGraphNodesForEntry(pc);
                    SetupGraphEntryPoint(pc, IsGlobalScope());
                }
                else
                {
                    SetupGraphEntryPoint(pc, IsGlobalScope());
                }
            }
        }

        private void SetupExecutive(int exeblock, int entry)
        {
            PushInterpreterProps(Properties);
            Properties.Reset();

            if (runtimeCore.Options.RunMode == InterpreterMode.Normal)
            {
                exe = runtimeCore.DSExecutable;
            }
            else if (runtimeCore.Options.RunMode == InterpreterMode.Expression)
            {
                exe = runtimeCore.ExprInterpreterExe;
            }
            else
            {
                Validity.Assert(false, "Invalid execution mode");
            }

            executingBlock = exeblock;

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);

            Validity.Assert(null != istream.instrList);

            SetupGraphNodesInScope();

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

            if (Language.Associative == executingLanguage)
            {
                SetupEntryPoint();
            }

            if (runtimeCore.Options.RunMode == InterpreterMode.Expression)
            {
                pc = entry;
            }

            Validity.Assert(null != rmem);
        }

        private void SetupExecutiveForCall(int exeblock, int entry)
        {
            PushInterpreterProps(Properties);
            Properties.Reset();

            if (runtimeCore.Options.RunMode == InterpreterMode.Normal)
            {
                exe = runtimeCore.DSExecutable;
            }
            else if (runtimeCore.Options.RunMode == InterpreterMode.Expression)
            {
                exe = runtimeCore.ExprInterpreterExe;
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

            SetupGraphNodesInScope();

            pc = entry;

            executingLanguage = exe.instrStreamList[exeblock].language;

            if (Language.Associative == executingLanguage)
            {
                int ci = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
                int fi = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;
                UpdateMethodDependencyGraph(pc, fi, ci);
            }
        }

        public void GetCallerInformation(out int classIndex, out int functionIndex)
        {
            classIndex = Constants.kGlobalScope;
            functionIndex = Constants.kGlobalScope;

            if (rmem.FramePointer >= StackFrame.StackFrameSize)
            {
                classIndex = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
                functionIndex = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;
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
            return ret;
        }

        private List<StackValue> PopArgumentsFromStack(int argumentCount)
        {
            List<StackValue> arguments = new List<StackValue>();
            int stackindex = rmem.Stack.Count - 1;

            for (int p = 0; p < argumentCount; ++p)
            {
                arguments.Add(rmem.Stack[stackindex]);
                stackindex = stackindex - 1;
            }
            rmem.PopFrame(argumentCount);

            return arguments;
        }

        public StackValue Callr(int blockDeclId,
                                int functionIndex, 
                                int classIndex, 
                                ref bool explicitCall, 
                                bool isDynamicCall = false, 
                                bool hasDebugInfo = false)
        {
            // This is curently unused but required for stack alignment
            var svDepth = rmem.Pop();
            int depth = (int)svDepth.IntegerValue;

            ProcedureNode fNode = null;

            bool isCallingMemberFunction = Constants.kInvalidIndex != classIndex;
            if (isCallingMemberFunction)
            {
                fNode = exe.classTable.ClassNodes[classIndex].ProcTable.Procedures[functionIndex];

                if (depth > 0 && fNode.IsConstructor)
                {
                    string message = String.Format(Resources.KCallingConstructorOnInstance, fNode.Name);
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.CallingConstructorOnInstance, message);
                    return StackValue.Null;
                }
            }
            else
            {
                // Global function
                fNode = exe.procedureTable[blockDeclId].Procedures[functionIndex];
            }

            // Build the arg values list
            var arguments = new List<StackValue>();

            // Retrive the param values from the stack
            int stackindex = rmem.Stack.Count - 1;

            List<StackValue> dotCallDimensions = new List<StackValue>();
            if (fNode.Name.Equals(Constants.kDotMethodName))
            {
                int firstDotArgIndex = stackindex - (Constants.kDotCallArgCount - 1);
                StackValue svLHS = rmem.Stack[firstDotArgIndex];
                arguments.Add(svLHS);

                // Retrieve the indexed dimensions into the dot call
                int arrayDimIndex = stackindex - (Constants.kDotCallArgCount - Constants.kDotArgIndexArrayIndex - 1);
                StackValue svArrayPtrDimesions = rmem.Stack[arrayDimIndex];
                Validity.Assert(svArrayPtrDimesions.IsArray);

                int arrayCountIndex = stackindex - (Constants.kDotCallArgCount - Constants.kDotArgIndexDimCount - 1);
                StackValue svDimensionCount = rmem.Stack[arrayCountIndex];
                Validity.Assert(svDimensionCount.IsInteger);

                // If array dimension were provided then retrive the final pointer 
                if (svDimensionCount.IntegerValue > 0)
                {
                    var dimArray = rmem.Heap.ToHeapObject<DSArray>(svArrayPtrDimesions);
                    Validity.Assert(dimArray.Count == svDimensionCount.IntegerValue);
                    dotCallDimensions.AddRange(dimArray.Values);
                }
            }
            else
            {
                arguments = PopArgumentsFromStack(fNode.ArgumentTypes.Count);
                arguments.Reverse();
            }

            var replicationGuides = new List<List<ReplicationGuide>>();
            var atLevels = new List<AtLevel>();

            Runtime.Context runtimeContext = new Runtime.Context();

            // Comment Jun: These function do not require replication guides
            // TODO Jun: Move these conditions or refactor JIL code emission so these checks dont reside here (Post R1)
            if (Constants.kDotMethodName != fNode.Name
                && Constants.kFunctionRangeExpression != fNode.Name)
            {
                // Comment Jun: If this is a non-dot call, cache the guides first and retrieve them on the actual function call
                // TODO Jun: Ideally, cache the replication guides in the dynamic function node
                replicationGuides = GetCachedReplicationGuides(arguments.Count);
                atLevels = GetCachedAtLevels(arguments.Count);
            }

            // if is dynamic call, the final pointer has been resovled in the ProcessDynamicFunction function
            StackValue svThisPtr = StackValue.Null;

            if (depth > 0)
            {
                svThisPtr = rmem.Pop();
                if (!svThisPtr.IsPointer)
                {
                    string message = String.Format(Resources.kInvokeMethodOnInvalidObject, fNode.Name);
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.DereferencingNonPointer, message);
                    return StackValue.Null;
                }
            }
            else
            {
                // There is no depth, but check if the function is a member function
                // If its a member function, the this pointer is required by the core to pass on to the FEP call
                if (isCallingMemberFunction && !fNode.IsConstructor && !fNode.IsStatic)
                {
                    // A member function
                    // Get the this pointer as this class instance would have already been cosntructed
                    svThisPtr = rmem.CurrentStackFrame.ThisPtr;
                }
                else if (fNode.Name.Equals(Constants.kInlineConditionalMethodName))
                {
                    // The built-in inlinecondition function is global but it is treated as a conditional execution rather than a normal function call
                    // This is why the class scope  needs to be preserved such that the auto-generated language blocks in an inline conditional can still refer to member functions and properties
                    svThisPtr = rmem.CurrentStackFrame.ThisPtr;
                }
                else
                {
                    // Global
                    svThisPtr = StackValue.BuildPointer(Constants.kInvalidPointer);
                }
            }

            if (svThisPtr.IsPointer &&
                svThisPtr.Pointer != Constants.kInvalidIndex &&
                svThisPtr.metaData.type != Constants.kInvalidIndex)
            {
                int runtimeClassIndex = svThisPtr.metaData.type;
                ClassNode runtimeClass = exe.classTable.ClassNodes[runtimeClassIndex];
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

            int blockDecl = blockDeclId;

            if (null != Properties.executingGraphNode)
            {
                exe.ExecutingGraphnode = Properties.executingGraphNode;
            }

            // Get the cached callsite, creates a new one for a first-time call
            CallSite callsite = runtimeCore.RuntimeData.GetCallSite(classIndex, fNode.Name, exe, runtimeCore);
            Validity.Assert(null != callsite);

            List<StackValue> registers = GetRegisters();

            // Get the execution states of the current stackframe
            int currentScopeClass = Constants.kInvalidIndex;
            int currentScopeFunction = Constants.kInvalidIndex;
            GetCallerInformation(out currentScopeClass, out currentScopeFunction);

            // Handle execution states at the FEP
            var stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, runtimeCore.RunningBlock, fepRun ? StackFrameType.Function : StackFrameType.LanguageBlock, StackFrameType.Function, 0, rmem.FramePointer, blockDeclId, registers, 0);
            StackValue sv = StackValue.Null;

            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                if (runtimeCore.ContinuationStruct.IsFirstCall)
                {
                    runtimeCore.DebugProps.SetUpCallrForDebug(
                                                       runtimeCore,
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
                    runtimeCore.DebugProps.SetUpCallrForDebug(
                                                       runtimeCore,
                                                       this,
                                                       fNode,
                                                       pc,
                                                       false,
                                                       callsite,
                                                       runtimeCore.ContinuationStruct.InitialArguments,
                                                       replicationGuides,
                                                       stackFrame,
                                                       runtimeCore.ContinuationStruct.InitialDotCallDimensions,
                                                       hasDebugInfo);
                }
            }

            //Dispatch without recursion tracking 
            explicitCall = false;
            IsExplicitCall = explicitCall;

            var argumentAtLevels = AtLevelHandler.GetArgumentAtLevelStructure(arguments, atLevels, runtimeCore);
            sv = callsite.JILDispatch(argumentAtLevels.Arguments, replicationGuides, argumentAtLevels.DominantStructure, stackFrame, runtimeCore, runtimeContext);
            if (sv.IsExplicitCall)
            {
                // Set the interpreter properties for function calls
                // These are used when performing GC on return 
                // The GC occurs: 
                //      1. In this instruction for implicit calls
                //      2. In the return instruction
                //
                Properties.functionCallArguments = argumentAtLevels.Arguments;
                Properties.functionCallDotCallDimensions = dotCallDimensions;
                Properties.DominantStructure = argumentAtLevels.DominantStructure;

                explicitCall = true;
                IsExplicitCall = explicitCall;
                CallExplicit(sv.ExplicitCallEntry);
            }

            // If the function was called implicitly, The code below assumes this and must be executed
            if (!explicitCall)
            {
                // Restore debug properties after returning from a CALL/CALLR
                if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
                {
                    runtimeCore.DebugProps.RestoreCallrForNoBreak(runtimeCore, fNode);
                }

                if (CoreUtils.IsDotMethod(fNode.Name))
                {
                    sv = IndexIntoArray(sv, dotCallDimensions);
                    rmem.PopFrame(Constants.kDotCallArgCount);
                }
            }
            return sv;
        }

        private StackValue CallrForMemberFunction(int blockIndex,
                                                  int classIndex,
                                                  int procIndex,
                                                  bool hasDebugInfo,
                                                  ref bool isExplicitCall)
        {
            var svDepth = rmem.Pop();
            Validity.Assert(svDepth.IsInteger);

            var arrayDim = rmem.Pop();
            Validity.Assert(arrayDim.IsArrayDimension);

            ClassNode classNode = exe.classTable.ClassNodes[classIndex];
            ProcedureNode procNode = classNode.ProcTable.Procedures[procIndex];

            // Get all arguments and replications 
            var arguments = PopArgumentsFromStack(procNode.ArgumentTypes.Count);
            arguments.Reverse();
            var repGuides = GetCachedReplicationGuides( arguments.Count + 1);
            var atLevels = GetCachedAtLevels(arguments.Count + 1);

            StackValue lhs = rmem.Pop();
            StackValue thisObject = lhs;
            bool isValidThisPointer = true;
            if (lhs.IsArray)
            {
                isValidThisPointer = ArrayUtils.GetFirstNonArrayStackValue(lhs, ref thisObject, runtimeCore);
                arguments.Insert(0, lhs);
            }

            if (!isValidThisPointer || (!thisObject.IsPointer && !thisObject.IsArray))
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.DereferencingNonPointer,
                                              Resources.kDeferencingNonPointer);
                return StackValue.Null;
            }

            var registers = GetRegisters();

            var stackFrame = new StackFrame(thisObject, classIndex, procIndex, pc + 1, 0, runtimeCore.RunningBlock, fepRun ? StackFrameType.Function : StackFrameType.LanguageBlock, StackFrameType.Function, 0, rmem.FramePointer, 0, registers, 0);

            var callsite = runtimeCore.RuntimeData.GetCallSite(classIndex, procNode.Name, exe, runtimeCore);

            Validity.Assert(null != callsite);

            bool setDebugProperty = runtimeCore.Options.IDEDebugMode &&
                                    runtimeCore.Options.RunMode != InterpreterMode.Expression &&
                                    procNode != null;

            if (setDebugProperty)
            {
                runtimeCore.DebugProps.SetUpCallrForDebug(
                                                   runtimeCore,
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

            var argumentAtLevels = AtLevelHandler.GetArgumentAtLevelStructure(arguments, atLevels, runtimeCore);

            StackValue sv = callsite.JILDispatch(argumentAtLevels.Arguments,
                                                 repGuides,
                                                 argumentAtLevels.DominantStructure,
                                                 stackFrame,
                                                 runtimeCore,
                                                 new Runtime.Context());

            isExplicitCall = sv.IsExplicitCall;
            if (isExplicitCall)
            {
                Properties.functionCallArguments = argumentAtLevels.Arguments;
                Properties.functionCallDotCallDimensions = new List<StackValue>();
                Properties.DominantStructure = argumentAtLevels.DominantStructure;
                CallExplicit(sv.ExplicitCallEntry);
            }

            return sv;
        }

        private void logVMMessage(string msg)
        {
            if (!enableLogging)
                return;

            if (0 != (debugFlags & (int)DebugFlags.ENABLE_LOG))
            {
                if (exe.EventSink != null && exe.EventSink.PrintMessage != null)
                {
                    exe.EventSink.PrintMessage("VMLog: " + msg + "\n");
                }
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
                    symbolName = exe.classTable.ClassNodes[ci].Name + "::" + symbolName;
                }
                string lhs = watchPrompt + symbolName;

                string rhs = null;
                StackValue snode = rmem.GetSymbolValue(symbol);
                if (snode.IsPointer)
                {
                    int type = snode.metaData.type;
                    string cname = exe.classTable.ClassNodes[type].Name;
                    rhs = cname + ":ptr(" + snode.Pointer + ")";
                }
                else if (snode.IsArray)
                {
                    int rawPtr = snode.ArrayPointer;
                    rhs = "Array:ptr(" + rawPtr + "):{" + GetArrayTrace(snode, blockId, index, new HashSet<int> { rawPtr } ) + "}";
                }
                else if (snode.IsFunctionPointer)
                {
                    rhs = "fptr: " + snode.FunctionPointer;
                }
                else if (snode.IsInteger)
                {
                    rhs = snode.IntegerValue.ToString();
                }
                else if (snode.IsDouble)
                {
                    double data = snode.DoubleValue;
                    rhs = data.ToString("R").IndexOf('.') != -1 ? data.ToString("R") : data.ToString("R") + ".0";
                }
                else if (snode.IsBoolean)
                {
                    rhs = snode.BooleanValue.ToString().ToLower();
                }
                else if (snode.IsChar)
                {
                    Char character = Convert.ToChar(snode.CharValue);
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
                    exe.EventSink.PrintMessage(lhs + " = " + rhs + "\n");
                }
            }
        }

        private string UnboxArray(StackValue snode, int blockId, int index)
        {
            String rhs = null;
            if (snode.IsArray)
            {
                rhs = "{" + GetArrayTrace(snode, blockId, index, new HashSet<int> { snode.ArrayPointer}) + "}";
            }
            else if (snode.IsInteger)
            {
                int data = (int)snode.IntegerValue;
                rhs = data.ToString();
            }
            else if (snode.IsDouble)
            {
                double data = snode.DoubleValue;
                rhs = data.ToString("R").IndexOf('.') != -1 ? data.ToString("R") : data.ToString("R") + ".0";
            }
            else if (snode.IsBoolean)
            {
                bool data = snode.BooleanValue;
                rhs = data.ToString().ToLower();
            }
            else if (snode.IsNull)
            {
                rhs = Literal.Null;
            }
            else if (snode.IsChar)
            {
                Char character = Convert.ToChar(snode.CharValue);
                rhs = "'" + character + "'";
            }
            else if (snode.IsString)
            {
                rhs = UnboxString(snode);
            }
            else if (snode.IsPointer)
            {
                int type = snode.metaData.type;
                string cname = exe.classTable.ClassNodes[type].Name;
                rhs = cname + ":ptr(" + snode.Pointer.ToString() + ")";
            }
            return rhs;
        }

        private string UnboxString(StackValue pointer)
        {
            if (!pointer.IsString)
                return null;

            string str = rmem.Heap.ToHeapObject<DSString>(pointer).Value;
            if (string.IsNullOrEmpty(str))
                return null;

            return "\"" + str + "\"";
        }

        private string GetArrayTrace(StackValue pointer, int blockId, int index, HashSet<int> pointers)
        {
            StringBuilder arrayelements = new StringBuilder();
            var array = rmem.Heap.ToHeapObject<DSArray>(pointer);

            for (int n = 0; n < array.Count; ++n)
            {
                StackValue sv = array.GetValueFromIndex(n, runtimeCore);
                if (sv.IsArray)
                {
                    int ptr = sv.ArrayPointer;
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
                    arrayelements.Append(UnboxArray(array.GetValueFromIndex(n, runtimeCore), blockId, index));
                }

                if (n < array.Count - 1)
                {
                    arrayelements.Append(",");
                }
            }
            return arrayelements.ToString();
        }

        public void SetupNextExecutableGraph(int function, int classscope)
        {
            Validity.Assert(istream != null);
            if (istream.language != Language.Associative)
            {
                return;
            }

            bool isUpdated = false;
            List<AssociativeGraph.GraphNode> graphNodes = graphNodesInProgramScope;
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

                        // Clear runtime warning for the first run in delta
                        // execution.
                        if (runtimeCore.Options.IsDeltaExecution && 
                            (Properties.executingGraphNode == null ||
                             Properties.executingGraphNode.OriginalAstID != graphNode.OriginalAstID))
                        {
                            runtimeCore.RuntimeStatus.ClearWarningsForAst(graphNode.OriginalAstID);
                        }

                        // Set the current graphnode being executed
                        Properties.executingGraphNode = graphNode;
                        runtimeCore.RuntimeExpressionUID = graphNode.exprUID;

                        if (runtimeCore.Options.dynamicCycleCheck)
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
            if (graphNodesInProgramScope == null)
            {
                return;
            }

            if (runtimeCore.Options.ApplyUpdate && isGlobalScope)
            {
                Validity.Assert(graphNodesInProgramScope.Count > 0);

                // The default entry point on ApplyUpdate is the first graphNode
                entrypoint = graphNodesInProgramScope[0].updateBlock.startpc;
            }

            if (graphNodesInProgramScope.Count > 0)
            {
                Properties.executingGraphNode = graphNodesInProgramScope[0];
            }

            foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in graphNodesInProgramScope)
            {
                if (!graphNode.isActive)
                {
                    continue;
                }

                if (runtimeCore.Options.IsDeltaExecution)
                {
                    // COmment Jun: start from graphnodes whose update blocks are in the range of the entry point
                    bool inStartRange = graphNode.updateBlock.startpc >= entrypoint;
                    if (graphNode.isDirty && inStartRange)
                    {
                        pc = graphNode.updateBlock.startpc;
                        graphNode.isDirty = false;
                        Properties.executingGraphNode = graphNode;
                        runtimeCore.RuntimeExpressionUID = graphNode.exprUID;
                        break;
                    }
                }
                else if (graphNode.updateBlock.startpc == entrypoint)
                {
                    Properties.executingGraphNode = graphNode;
                    runtimeCore.RuntimeExpressionUID = graphNode.exprUID;
                    if (graphNode.isDirty)
                    {
                        graphNode.isDirty = false;
                        //count how many times one graphNode has been edited
                        graphNode.counter++;
                        break;
                    }
                }
            }

            if (runtimeCore.Options.IsDeltaExecution)
            {
                runtimeCore.RuntimeStatus.ClearWarningsForAst(Properties.executingGraphNode.OriginalAstID);
            }
        }

        private void HandleCycle()
        {
            List<AssociativeGraph.GraphNode> nodeIterations = Properties.nodeIterations;
            FindCycleStartNodeAndEndNode(nodeIterations);

            if (enableLogging)
            {
                foreach (AssociativeGraph.GraphNode node in nodeIterations)
                {
                    Console.WriteLine("nodes " + node.updateNodeRefList[0].nodeList[0].symbol.name);
                }
            }

            runtimeCore.RuntimeStatus.LogWarning(WarningID.CyclicDependency, Resources.kCyclicDependency);
            foreach (AssociativeGraph.GraphNode node in nodeIterations)
            {
                node.isCyclic = true;

                Validity.Assert(!node.isReturn);
                SymbolNode symbol = node.updateNodeRefList[0].nodeList[0].symbol;
                Validity.Assert(null != symbol);
                rmem.SetSymbolValue(symbol, StackValue.Null);

                node.dependentList.Clear();
                node.isActive = false;
            }
            Properties.nodeIterations = new List<AssociativeGraph.GraphNode>();
        }

        private bool HasCyclicDependency(AssociativeGraph.GraphNode node)
        {
            return node.counter > runtimeCore.Options.kDynamicCycleThreshold;
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
            bool isDataModified = svGraphNode.RawData != svUpdateNode.RawData;
            bool isDoubleDataModified = svGraphNode.IsDouble && svGraphNode.DoubleValue != svUpdateNode.ToDouble().DoubleValue;
            bool isTypeModified = !svGraphNode.IsInvalid && !svUpdateNode.IsInvalid && svGraphNode.optype != svUpdateNode.optype;

            // Jun Comment: an invalid optype means that the value was not set
            bool isInvalid = svGraphNode.IsInvalid || svUpdateNode.IsInvalid;

            return isInvalid || isPointerModified || isArrayModified || isDataModified || isDoubleDataModified || isTypeModified;
        }

        private int UpdateGraph(int exprUID, bool isSSAAssign)
        {
            List<AssociativeGraph.GraphNode> reachableGraphNodes = null;
            if (runtimeCore.Options.DirectDependencyExecution)
            {
                // Data flow execution prototype
                // Dependency has already been resolved at compile time
                // Get the reachable nodes directly from the executingGraphNode
                reachableGraphNodes = new List<AssociativeGraph.GraphNode>(Properties.executingGraphNode.ChildrenNodes);
            }
            else
            {
                // Find reachable graphnodes
                reachableGraphNodes = AssociativeEngine.Utils.UpdateDependencyGraph(
                    Properties.executingGraphNode, this, exprUID, isSSAAssign, runtimeCore.Options.ExecuteSSA, executingBlock, false);
            }

            // Mark reachable nodes as dirty
            Validity.Assert(reachableGraphNodes != null);
            if (reachableGraphNodes.Count > 0)
            {
                for (int n = 0; n < reachableGraphNodes.Count; ++n)
                {
                    AssociativeGraph.GraphNode gnode = reachableGraphNodes[n];
                    gnode.isDirty = true;

                    if (gnode.isCyclic)
                    {
                        // If the graphnode is cyclic, mark it as not dirst so it wont get executed 
                        // Sets its cyclePoint graphnode to be not dirty so it also doesnt execute.
                        // The cyclepoint is the other graphNode that the current node cycles with
                        gnode.isDirty = false;
                        if (null != gnode.cyclePoint)
                        {
                            gnode.cyclePoint.isDirty = false;
                            gnode.cyclePoint.isCyclic = true;
                        }
                    }
                }
            }

            // Get all redefined graphnodes
            int classScope = Constants.kInvalidIndex;
            int functionScope = Constants.kInvalidIndex;
            GetCallerInformation(out classScope, out functionScope);
            var nodesInScope = istream.dependencyGraph.GetGraphNodesAtScope(classScope, functionScope);
            List<AssociativeGraph.GraphNode> redefinedNodes = 
                AssociativeEngine.Utils.GetRedefinedGraphNodes(runtimeCore, Properties.executingGraphNode, nodesInScope, classScope, functionScope);
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
                foreach (var symbol in gnode.symbolListWithinExpression)
                {
                    rmem.SetSymbolValue(symbol, StackValue.Null);
                }
                gnode.isActive = false;
            }
            return reachableGraphNodes.Count;
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
            if (graphNodesInProgramScope != null)
            {
                foreach (AssociativeGraph.GraphNode graphNode in graphNodesInProgramScope)
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
            }
            return setentry;
        }

        private void UpdateMethodDependencyGraph(int entry, int procIndex, int classIndex)
        {
            int setentry = entry;
            bool isFirstGraphSet = false;
            AssociativeGraph.GraphNode entryNode = null;

            StackValue svFunctionBlock = rmem.GetAtRelative(StackFrame.FrameIndexFunctionBlockIndex);
            Validity.Assert(svFunctionBlock.IsBlockIndex);
            int langBlockDecl = svFunctionBlock.BlockIndex;
            ProcedureNode procNode = GetProcedureNode(langBlockDecl, classIndex, procIndex);

            List<AssociativeGraph.GraphNode> graphNodes = procNode.GraphNodeList;
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

        private void XLangUpdateDependencyGraph(int currentLangBlock)
        {
            int classScope = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
            int functionScope = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;

            List<AssociativeGraph.UpdateNodeRef> upadatedList = new List<AssociativeGraph.UpdateNodeRef>();

            // For every instruction list in the executable
            foreach (InstructionStream xInstrStream in exe.instrStreamList)
            {
                // If the instruction list is valid, is associative and has more than 1 graph node
                if (null != xInstrStream && Language.Associative == xInstrStream.language && xInstrStream.dependencyGraph.GraphList.Count > 0)
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
                                || exe.CompleteCodeBlocks[currentLangBlock].IsMyAncestorBlock(graphNode.languageBlockId))
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
                                        exe.classTable.ClassNodes[firstSymbolInUpdatedRef.classScope].Symbols.symbolList.Count <= firstSymbolInUpdatedRef.symbolTableIndex)
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
            if (fp >= rmem.GlobOffset + StackFrame.StackFrameSize)
            {
                RX = rmem.GetAtRelative(StackFrame.FrameIndexRX);
                TX = rmem.GetAtRelative(StackFrame.FrameIndexTX);
            }
        }

        private void ResumeRegistersFromStackExceptRX()
        {
            int fp = rmem.FramePointer;
            if (fp >= rmem.GlobOffset + StackFrame.StackFrameSize)
            {
                TX = rmem.GetAtRelative(StackFrame.FrameIndexTX);
            }
       }

        private void SaveRegistersToStack()
        {
            int fp = rmem.FramePointer;
            if (fp >= rmem.GlobOffset + StackFrame.StackFrameSize)
            {
                rmem.SetAtRelative(StackFrame.FrameIndexRX, RX);
                rmem.SetAtRelative(StackFrame.FrameIndexTX, TX);
            }
        }

        public List<StackValue> GetRegisters()
        {
            return new List<StackValue> { RX, TX };
        }

        /// <summary>
        /// Performs type coercion of returned value and GC of arguments, this ptr and Dot methods
        /// </summary>
        private void DebugPerformCoercionAndGC(DebugFrame debugFrame)
        {
            ProcedureNode procNode = debugFrame.FinalFepChosen != null ? debugFrame.FinalFepChosen.procedureNode : null;
            if (!debugFrame.IsBaseCall)
            {
                RX = CallSite.PerformReturnTypeCoerce(procNode, runtimeCore, RX);

                if (debugFrame.ThisPtr == null && CoreUtils.IsDotMethod(procNode.Name))
                {
                    RX = IndexIntoArray(RX, debugFrame.DotCallDimensions);
                    rmem.PopFrame(Constants.kDotCallArgCount);
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
            Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);

            DebugFrame debugFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();
            bool isReplicating = debugFrame.IsReplicating;

            if (!isReplicating)
            {
                bool isResume = debugFrame.IsResume;

                // RestoreCallrForNoBreak and PerformReturnTypeCoerce are NOT called if this is true 
                // so these have to be explicitly called here
                if (isResume)
                {
                    debugFrame = runtimeCore.DebugProps.DebugStackFrame.Pop();
                    waspopped = true;
                    if (runtimeCore.DebugProps.DebugStackFrame.Count > 1)
                    {
                        DebugFrame frame = runtimeCore.DebugProps.DebugStackFrame.Peek();
                        frame.IsResume = true;
                    }

                    DebugPerformCoercionAndGC(debugFrame);

                    // Restore registers except RX on popping of function stackframe
                    ResumeRegistersFromStackExceptRX();

                    terminate = false;
                }

                if (runtimeCore.DebugProps.RunMode.Equals(Runmode.StepOut) && pc == runtimeCore.DebugProps.StepOutReturnPC)
                {
                    runtimeCore.Breakpoints.Clear();
                    runtimeCore.Breakpoints.AddRange(runtimeCore.DebugProps.AllbreakPoints);
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
            Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);

            debugFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();

            isReplicating = debugFrame.IsReplicating;

            if (!isReplicating)
            {
                bool isResume = debugFrame.IsResume;

                // Comment Jun: Since we dont step into _Dispose() calls, then its debugframe should not be popped off here.
                bool isDispose = debugFrame.IsDisposeCall;

                // RestoreCallrForNoBreak and PerformReturnTypeCoerce are NOT called if this is true
                // or for base class ctor calls and therefore need to be taken care of here
                if ((isResume || debugFrame.IsBaseCall) && !isDispose)
                {
                    debugFrame = runtimeCore.DebugProps.DebugStackFrame.Pop();
                    waspopped = true;

                    if (isResume)
                    {
                        if (runtimeCore.DebugProps.DebugStackFrame.Count > 1)
                        {
                            DebugFrame frame = runtimeCore.DebugProps.DebugStackFrame.Peek();
                            frame.IsResume = true;
                        }
                    }

                    DebugPerformCoercionAndGC(debugFrame);

                    // Restore registers except RX on popping of function stackframe
                    ResumeRegistersFromStackExceptRX();

                    terminate = false;
                }

                Properties.executingGraphNode = debugFrame.ExecutingGraphNode;

                if (runtimeCore.DebugProps.RunMode.Equals(Runmode.StepOut) && pc == runtimeCore.DebugProps.StepOutReturnPC)
                {
                    runtimeCore.Breakpoints.Clear();
                    runtimeCore.Breakpoints.AddRange(runtimeCore.DebugProps.AllbreakPoints);
                }
            }

            // Restore return address and lang block
            pc = (int)rmem.GetAtRelative(StackFrame.FrameIndexReturnAddress).IntegerValue;
            exeblock = rmem.GetAtRelative(StackFrame.FrameIndexCallerBlockIndex).BlockIndex;

            istream = exe.instrStreamList[exeblock];
            instructions = istream.instrList;
            executingLanguage = istream.language;

            ci = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
            fi = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex).FunctionIndex;

            int localCount;
            int paramCount;
            int blockId = rmem.GetAtRelative(StackFrame.FrameIndexFunctionBlockIndex).BlockIndex;
            GetLocalAndParamCount(blockId, ci, fi, out localCount, out paramCount);

            // Get execution states
            List<bool> execStateRestore = new List<bool>();
            execStateRestore = RetrieveExecutionStatesFromStack(localCount, paramCount);

            // Pop function stackframe as this is not allowed in Ret/Retc in debug mode
            rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.FrameIndexFramePointer).IntegerValue;

            rmem.PopFrame(StackFrame.StackFrameSize + localCount + paramCount + execStateRestore.Count); 

            ResumeRegistersFromStackExceptRX();

            //StackValue svFrameType = rmem.GetAtRelative(StackFrame.kFrameIndexCallerStackFrameType);
            StackValue svFrameType = rmem.GetAtRelative(StackFrame.FrameIndexStackFrameType);
            StackFrameType frametype = svFrameType.FrameType;
            if (frametype == StackFrameType.LanguageBlock)
            {
                bounceType = TX.BounceType;
            }
            return waspopped;
        }

        void RestoreDebugPropsOnReturnFromLangBlock(ref int exeblock, ref List<Instruction> instructions)
        {
            // On the new stack frame, this dependency has already been executed at retb in RestoreFromBounce
            //XLangUpdateDependencyGraph(exeblock);

            Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);
            {
                // Restore fepRun
                DebugFrame debugFrame = runtimeCore.DebugProps.DebugStackFrame.Pop();

                bool isResume = debugFrame.IsResume;

                if (isResume)
                {
                    if (runtimeCore.DebugProps.DebugStackFrame.Count > 1)
                    {
                        DebugFrame frame = runtimeCore.DebugProps.DebugStackFrame.Peek();
                        frame.IsResume = true;
                    }

                    terminate = false;

                    // Restore registers except RX on popping of language stackframe
                    ResumeRegistersFromStackExceptRX();
                }

                // Restore return address and lang block    
                pc = (int)rmem.GetAtRelative(StackFrame.FrameIndexReturnAddress).IntegerValue;
                exeblock = rmem.GetAtRelative(StackFrame.FrameIndexCallerBlockIndex).BlockIndex;

                istream = exe.instrStreamList[exeblock];
                instructions = istream.instrList;
                executingLanguage = istream.language;

                Properties.executingGraphNode = debugFrame.ExecutingGraphNode;

                // Pop language stackframe as this is not allowed in Retb in debug mode
                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.FrameIndexFramePointer).IntegerValue;
                rmem.PopFrame(StackFrame.StackFrameSize);

                ResumeRegistersFromStackExceptRX();
                bounceType = TX.BounceType;
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
            var tempFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();

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
                runtimeCore.DebugProps.DebugEntryPC = runtimeCore.DebugProps.ReturnPCFromDispose;
                //break;
            }
            else
            {
                debugFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();
                // If call returns to Dot Call, restore debug props for Dot call
                if (debugFrame.IsDotCall)
                {
                    waspopped = RestoreDebugPropsOnReturnFromBuiltIns();
                }
                runtimeCore.DebugProps.DebugEntryPC = currentPC;
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
            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                waspopped = DebugReturnFromFunctionCall(currentPC, ref exeblock, out ci, out fi, out isReplicating, out debugFrame);

                if (!waspopped)
                {
                    runtimeCore.DebugProps.RestoreCallrForNoBreak(runtimeCore, procNode, isReplicating);
                }
            }

            return waspopped;
        }

        private void SetupExecutive(int exeblock, int entry, Language language, List<Instruction> breakpoints)
        {
            // exe need to be assigned at the constructor, 
            // for function call with replication, gc is triggered to handle the parameter and return value at FunctionEndPoint
            // gc requirs exe to be not null but at that point, Execute has not been called
            //Validity.Assert(exe == null);
            exe = runtimeCore.DSExecutable;
            executingBlock = exeblock;

            runtimeCore.DebugProps.CurrentBlockId = exeblock;

            istream = exe.instrStreamList[exeblock];
            Validity.Assert(null != istream);
            runtimeCore.DebugProps.DebugEntryPC = entry;

            List<Instruction> instructions = istream.instrList;
            Validity.Assert(null != instructions);

            SetupGraphNodesInScope();

            // Restore the previous state
            //rmem = runtimeCore.RuntimeMemory;
            rmem = runtimeCore.RuntimeMemory;

            if (runtimeCore.DebugProps.isResume)   // resume from a breakpoint, 
            {
                Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);

                DebugFrame debugFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();

                // TODO: The FepRun info need not be cached in DebugProps any longer
                // as it can be replaced by StackFrameType in rmem.Stack - pratapa
                fepRun = debugFrame.FepRun == 1;
                //StackFrameType stackFrameType = (StackFrameType)rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameType).opdata;
                //fepRun = (stackFrameType == StackFrameType.kTypeFunction) ? true : false;

                //ResumeRegistersFromStack();

                fepRunStack = runtimeCore.DebugProps.FRStack;

                Properties = PopInterpreterProps();
                Properties.executingGraphNode = runtimeCore.DebugProps.executingGraphNode;
                deferedGraphNodes = runtimeCore.DebugProps.deferedGraphnodes;

            }
            else
            {
                PushInterpreterProps(Properties);
                Properties.Reset();
            }

            if (false == fepRun)
            {
                if (runtimeCore.DebugProps.isResume) // resume from a breakpoint, 
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

            if (Language.Associative == executingLanguage && !runtimeCore.DebugProps.isResume)
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
                    runtimeCore.ReasonForExecutionSuspend = ReasonForExecutionSuspend.NoEntryPoint;
                    terminateExec = true;
                    //break;
                }
                else if (pc >= runningInstructions.Count)
                {
                    runtimeCore.ReasonForExecutionSuspend = ReasonForExecutionSuspend.EndOfFile;
                    terminateExec = true;
                    //break;
                }
                else
                {
                    Validity.Assert(breakpoints.Contains(runningInstructions[currentPC]));
                    runtimeCore.ReasonForExecutionSuspend = ReasonForExecutionSuspend.Breakpoint;
                    logVMMessage("Breakpoint at: " + runningInstructions[currentPC]);

                    Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);
                    {
                        DebugFrame debugFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();

                        // Since the first frame always belongs to the global language block
                        if (runtimeCore.DebugProps.DebugStackFrame.Count > 1)
                        {
                            debugFrame.IsResume = true;
                        }
                    }
                    SaveRegistersToStack();

                    runtimeCore.DebugProps.isResume = true;
                    runtimeCore.DebugProps.FRStack = fepRunStack;
                    runtimeCore.DebugProps.executingGraphNode = Properties.executingGraphNode;
                    runtimeCore.DebugProps.deferedGraphnodes = deferedGraphNodes;

                    if (runtimeCore.DebugProps.RunMode == Runmode.StepNext)
                    {
                        foreach (DebugFrame debugFrame in runtimeCore.DebugProps.DebugStackFrame)
                        {
                            debugFrame.FunctionStepOver = false;
                        }
                    }

                    runtimeCore.RunningBlock = executingBlock;
                    PushInterpreterProps(Properties);

                    if (runtimeCore.DebugProps.FirstStackFrame != null)
                    {
                        runtimeCore.DebugProps.FirstStackFrame = null;
                    }
                    throw new DebugHalting(); 
                }
            }
            return terminateExec;
        }

        /// <summary>
        /// This is the VM execution entry function
        /// </summary>
        /// <param name="exeblock"></param>
        /// <param name="entry"></param>
        /// <param name="breakpoints"></param>
        /// <param name="language"></param>
        public void Execute(int exeblock, int entry, List<Instruction> breakpoints, Language language = Language.NotSpecified)
        {
            terminate = true;
            if (entry != Constants.kInvalidPC)
            {
                terminate = false;
                if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
                {
                    ExecuteDebug(exeblock, entry, breakpoints, language);
                }
                else
                {
                    Execute(exeblock, entry, language);
                }
            }
        }

        // This will be called only at the time of creation of the main interpreter in the explicit case OR
        // for every implicit function call (like in replication) OR 
        // for every implicit bounce (like in dynamic lang block in inline condition) OR
        // for a Debug Resume from a breakpoint
        private void ExecuteDebug(int exeblock, int entry, List<Instruction> breakpoints, Language language = Language.NotSpecified)
        {
            // TODO Jun: Call RestoreFromBounce here?
            StackValue svType = rmem.GetAtRelative(StackFrame.FrameIndexStackFrameType);
            StackFrameType type = svType.FrameType;
            if (StackFrameType.LanguageBlock == type || StackFrameType.Function == type)
            {
                ResumeRegistersFromStack();
                bounceType = TX.BounceType;
            }

            SetupExecutive(exeblock, entry, language, breakpoints);


            bool debugRun = (0 != (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER));
            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("Start JIL Execution - " + CoreUtils.GetLanguageString(language));
            }

            runtimeCore.DebugProps.isResume = false;

            while (!terminate)
            {
                // This will be true only for inline conditions in Associative blocks 
                if (runtimeCore.DebugProps.InlineConditionOptions.isInlineConditional &&
                    runtimeCore.DebugProps.InlineConditionOptions.instructionStream == exeblock && runtimeCore.DebugProps.InlineConditionOptions.endPc == pc)
                {
                    // turn off inline conditional flag
                    {
                        runtimeCore.DebugProps.InlineConditionOptions.isInlineConditional = false;
                        runtimeCore.DebugProps.InlineConditionOptions.startPc = Constants.kInvalidIndex;
                        runtimeCore.DebugProps.InlineConditionOptions.endPc = Constants.kInvalidIndex;
                        runtimeCore.DebugProps.InlineConditionOptions.instructionStream = 0;
                    }

                    // if no longer inside a replicated/external function call, restore breakpoints
                    if (!runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsReplicating) &&
                        !runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsExternalFunction))
                    {
                        if (runtimeCore.DebugProps.InlineConditionOptions.ActiveBreakPoints.Count > 0)
                        {
                            runtimeCore.Breakpoints.Clear();
                            runtimeCore.Breakpoints.AddRange(runtimeCore.DebugProps.InlineConditionOptions.ActiveBreakPoints);
                            runtimeCore.DebugProps.InlineConditionOptions.ActiveBreakPoints.Clear();
                        }
                    }
                }

                List<Instruction> instructions = istream.instrList;

                // Execute the instruction!
                Instruction executeInstruction = instructions[pc];
                Exec(instructions[pc]);

                bool restoreInstructionStream = executeInstruction.opCode == OpCode.CALLR || executeInstruction.opCode == OpCode.RETURN;
                if (restoreInstructionStream && IsExplicitCall)
                {
                    // The instruction stream list is updated on callr
                    instructions = istream.instrList;
                    exeblock = executingBlock;
                    runtimeCore.DebugProps.CurrentBlockId = exeblock;
                }

                // Disabling support for stepping into replicating function calls temporarily - pratapa
                // Check if the current instruction is a return from a function call or constructor

                DebugFrame tempFrame = null;
                if (!IsExplicitCall && instructions[runtimeCore.DebugProps.DebugEntryPC].opCode == OpCode.RETURN)
                {
                    int ci, fi;
                    bool isReplicating;
                    DebugFrame debugFrame;
                    DebugReturnFromFunctionCall(pc, ref exeblock, out ci, out fi, out isReplicating, out debugFrame);

                    instructions = istream.instrList;
                    executingBlock = exeblock;
                    runtimeCore.DebugProps.CurrentBlockId = exeblock;

                    SetupGraphNodesInScope();
                }
                else if (executeInstruction.opCode == OpCode.RETB)
                {
                    tempFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();

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
                        runtimeCore.DebugProps.DebugEntryPC = runtimeCore.DebugProps.ReturnPCFromDispose;
                        break;
                    }
                    else
                    {
                        runtimeCore.DebugProps.DebugEntryPC = pc;
                    }
                    // Comment Jun: On explictit bounce, only on retb we update the executing block
                    // as the block scope has already change by returning to the caller block
                    executingBlock = exeblock;
                    runtimeCore.RunningBlock = exeblock;
                }
                else
                {
                    runtimeCore.DebugProps.DebugEntryPC = pc;
                }

                DebugFrame frame = runtimeCore.DebugProps.DebugStackFrame.Peek();
                if (frame.IsInlineConditional)
                {
                    RestoreDebugPropsOnReturnFromBuiltIns();
                    runtimeCore.DebugProps.DebugEntryPC = pc;
                }

                //runtimeCore.RuntimeMemory = rmem;
                runtimeCore.RuntimeMemory = rmem;

                bool terminateExec = HandleBreakpoint(breakpoints, istream.instrList, pc);
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


        private void Execute(int exeblock, int entry, Language language = Language.NotSpecified)
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
                if(runtimeCore.CancellationPending)
                {
                    throw new ExecutionCancelledException();
                }

                if (pc >= istream.instrList.Count || pc < 0)
                {
                    break;
                }
                Exec(istream.instrList[pc]);
            }

            if (!fepRun || fepRun && debugRun)
            {
                logVMMessage("End JIL Execution - " + engine);
            }
        }

        protected SymbolNode GetSymbolNode(int blockId, int classIndex, int symbolIndex)
        {
            if (Constants.kGlobalScope == classIndex)
            {
                return exe.runtimeSymbols[blockId].symbolList[symbolIndex];
            }
            else
            {
                return exe.classTable.ClassNodes[classIndex].Symbols.symbolList[symbolIndex];
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
                case AddressType.String:
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
                    data.metaData.type = exe.TypeSystem.GetType(opSymbol);
                    break;
                case AddressType.StaticType:
                    data = opSymbol;
                    break;
                case AddressType.Register:
                    switch (opSymbol.Register)
                    {
                        case Registers.RX:
                            data = RX;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case AddressType.VarIndex:
                    SymbolNode symbol = GetSymbolNode(blockId, opClass.ClassIndex, opSymbol.VariableIndex);
                    data = rmem.GetSymbolValue(symbol);
                    break;

                case AddressType.MemVarIndex:
                    data = rmem.GetMemberData(opSymbol.MemberVariableIndex, opClass.ClassIndex, exe);
                    break;

                case AddressType.StaticMemVarIndex:
                    SymbolNode staticMember = GetSymbolNode(blockId, Constants.kGlobalScope, opSymbol.StaticVariableIndex);
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
            int symbolIndex = op1.SymbolIndex;
            int classIndex = op2.ClassIndex;
            SymbolNode symbol = GetSymbolNode(blockId, classIndex, symbolIndex);
            int offset = symbol.index;
            runtimeCore.watchStack[offset] = opVal;
        }

        private void PushW(int block, StackValue op1, StackValue op2)
        {
            int symbol = op1.SymbolIndex;
            int scope = op2.ClassIndex;
            SymbolNode node;
            if (Constants.kGlobalScope == scope)
            {
                node = exe.runtimeSymbols[block].symbolList[symbol];
            }
            else
            {
                node = exe.classTable.ClassNodes[scope].Symbols.symbolList[symbol];
            }

            int offset = node.index;
            //For watch symbol, use watching stack.
            if (runtimeCore.WatchSymbolList.Contains(node))
            {
                rmem.Push(runtimeCore.watchStack[offset]);
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

                    SymbolNode symbol = GetSymbolNode(blockId, op2.ClassIndex, op1.SymbolIndex);
                    opPrev = rmem.GetSymbolValue(symbol);
                    rmem.SetSymbolValue(symbol, opVal);
                    exe.UpdatedSymbols.Add(symbol);

                    if (IsDebugRun())
                    {
                        logWatchWindow(blockId, op1.SymbolIndex);
                        System.Console.ReadLine();
                    }

                    if (Constants.kGlobalScope == op2.ClassIndex)
                    {
                        logWatchWindow(blockId, op1.SymbolIndex);
                    }

                    RecordExecutedGraphNode();
                    break;

                case AddressType.StaticMemVarIndex:
                    var staticMember = GetSymbolNode(blockId, Constants.kGlobalScope, op1.StaticVariableIndex);
                    opPrev = rmem.GetSymbolValue(staticMember);
                    rmem.SetSymbolValue(staticMember, opVal);
                    exe.UpdatedSymbols.Add(staticMember);

                    if (IsDebugRun())
                    {
                        logWatchWindow(blockId, op1.StaticVariableIndex);
                        System.Console.ReadLine();
                    }

                    logWatchWindow(blockId, op1.StaticVariableIndex);
                    break;
                case AddressType.Register:
                    {
                        StackValue data = opVal;
                        switch (op1.Register)
                        {
                            case Registers.RX:
                                opPrev = RX;
                                RX = data;
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

            StackValue ret = StackValue.Null;
            if (value.IsArray)
            {
                ret = runtimeCore.Heap.ToHeapObject<DSArray>(value).SetValueForIndices(dimlist, data, runtimeCore);
            }
            else if (value.IsString)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidIndexing, Resources.kStringIndexingCannotBeAssigned);
                ret = StackValue.Null;
            }
            else
            {
                StackValue svArray;

                try
                {
                    svArray = rmem.Heap.AllocateArray(new StackValue[] { });
                }
                catch (RunOutOfMemoryException)
                {
                    svArray = StackValue.Null;
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                }

                rmem.SetSymbolValue(symbolnode, svArray);

                var array = rmem.Heap.ToHeapObject<DSArray>(svArray);
                if (!value.IsNull)
                {
                    array.SetValueForIndex(0, value, runtimeCore);
                }
                ret = array.SetValueForIndices(dimlist, data, runtimeCore);
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

            RecordExecutedGraphNode();
            return ret;
        }

        private bool IsDebugRun()
        {
            return (debugFlags & (int)DebugFlags.SPAWN_DEBUGGER) != 0;
        }


        protected void runtimeVerify(bool condition, string msg = "Dsasm runtime error. Exiting...\n")
        {
            // TODO Jun: hook this up to a runtime error handler            
            if (!condition)
                throw new RuntimeException(msg);
        }

        public StackValue GetIndexedArray(StackValue svArray, List<StackValue> indices)
        {
            if (indices.Count == 0)
                return svArray;

            if (svArray.IsArray)
            {
                var array = runtimeCore.Heap.ToHeapObject<DSArray>(svArray);
                return array.GetValueFromIndices(indices, runtimeCore);
            }
            else if (svArray.IsString)
            {
                StackValue[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, runtimeCore);
                if (zippedIndices == null || zippedIndices.Length == 0)
                {
                    return StackValue.Null;
                }
                var dsString = runtimeCore.Heap.ToHeapObject<DSString>(svArray);
                var substrings = zippedIndices.Select(s => dsString.GetValueAtIndex(s[0], runtimeCore));
                if (!substrings.All(s => s.IsString))
                    return StackValue.Null;

                string result = string.Join(string.Empty, substrings.Select(s => rmem.Heap.ToHeapObject<DSString>(s).Value));
                return runtimeCore.RuntimeMemory.Heap.AllocateString(result);
            }
            else
                return StackValue.Null;
        }

        public StackValue GetIndexedArrayW(int dimensions, int blockId, StackValue op1, StackValue op2)
        {
            var dims = new List<StackValue>();
            for (int n = dimensions - 1; n >= 0; --n)
            {
                dims.Insert(0, rmem.Pop());
            }

            int symbolIndex = op1.SymbolIndex;
            int classIndex = op2.ClassIndex;

            SymbolNode symbolNode = GetSymbolNode(blockId, classIndex, symbolIndex);
            int stackindex = symbolNode.index;
            string varname = symbolNode.name;

            StackValue thisArray;
            if (runtimeCore.Options.RunMode == InterpreterMode.Expression && runtimeCore.WatchSymbolList.Contains(symbolNode))
            {
                thisArray = runtimeCore.watchStack[symbolNode.index];
            }
            else
            {
                if (op1.IsMemberVariableIndex)
                {
                    StackValue thisptr = rmem.GetAtRelative(StackFrame.FrameIndexThisPtr);
                    thisArray = rmem.Heap.ToHeapObject<DSObject>(thisptr).GetValueFromIndex(stackindex, runtimeCore);
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

                runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.IndexIntoNonArrayObject);
                return StackValue.Null;
            }

            StackValue result;
            try
            {
                result = GetIndexedArray(thisArray, dims);
            }
            catch (ArgumentOutOfRangeException)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.IndexIntoNonArrayObject);
                return StackValue.Null;
            }

            return result;
        }

        public StackValue GetIndexedArray(List<StackValue> dims, int blockId, StackValue op1, StackValue op2)
        {
            int symbolIndex = op1.SymbolIndex;
            int classIndex = op2.ClassIndex;

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

                runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.IndexIntoNonArrayObject);
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
                    result = StackValue.BuildString(result.ArrayPointer);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.IndexIntoNonArrayObject);
                return StackValue.Null;
            }

            return result;
        }

        private bool ResolveDynamicFunction(Instruction instr, out bool isMemberFunctionPointer)
        {
            isMemberFunctionPointer = false;
            int fptr = Constants.kInvalidIndex;
            int functionDynamicIndex = instr.op1.DynamicIndex;
            int classIndex = instr.op2.ClassIndex;
            int depth = (int)rmem.Pop().IntegerValue;
            runtimeVerify(functionDynamicIndex != Constants.kInvalidIndex);
            bool isFunctionPointerCall = false;

            var dynamicFunction = exe.DynamicFuncTable.GetFunctionAtIndex(functionDynamicIndex);

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
                fptr = fpSv.FunctionPointer;
            }

            //retrieve the function arguments
            List<StackValue> argSvList = new List<StackValue>();
            for (int i = 0; i < argumentNumber; i++)
            {
                StackValue argSv = rmem.Pop();
                argSvList.Add(argSv);
            }

            int lefttype = Constants.kGlobalScope;
            if (depth > 0)
            {
                //resolve the identifier list            
                StackValue pSv = rmem.Pop();
                rmem.Push(pSv);
                lefttype = pSv.metaData.type;
            }

            int type = lefttype;

            if (depth > 0)
            {
                // check whether it is function pointer, this checking is done at runtime to handle the case
                // when turning on converting dot operator to function call
                if (!((int)PrimitiveType.Void == type || Constants.kInvalidIndex == type || exe.classTable.ClassNodes[type].Symbols == null))
                {
                    bool hasThisSymbol;
                    AddressType addressType;
                    SymbolNode node = null;
                    bool isStatic = false;
                    ClassNode classNode = exe.classTable.ClassNodes[type];
                    int symbolIndex = ClassUtils.GetSymbolIndex(classNode, procName, type, Constants.kGlobalScope, runtimeCore.RunningBlock, exe.CompleteCodeBlocks, out hasThisSymbol, out addressType);

                    if (Constants.kInvalidIndex != symbolIndex)
                    {
                        if (addressType == AddressType.StaticMemVarIndex)
                        {
                            node = exe.CodeBlocks[0].symbolTable.symbolList[symbolIndex];
                            isStatic = true;
                        }
                        else
                        {
                            node = exe.classTable.ClassNodes[type].Symbols.symbolList[symbolIndex];
                        }
                    }
                    if (node != null)
                    {
                        isFunctionPointerCall = true;
                        StackValue fpSv = isStatic ? StackValue.BuildStaticMemVarIndex(node.symbolTableIndex) : StackValue.BuildPointer(node.symbolTableIndex);
                        if (fpSv.IsStaticVariableIndex)
                        {
                            StackValue op2 = StackValue.BuildClassIndex(Constants.kInvalidIndex);
                            fpSv = GetOperandData(0, fpSv, op2);
                        }
                        else
                        {
                            StackValue ptr = rmem.Stack.Last();
                            fpSv = rmem.Heap.ToHeapObject<DSObject>(ptr).GetValueFromIndex(fpSv.Pointer, runtimeCore);
                        }

                        // Check the last pointer
                        if (fpSv.IsPointer || fpSv.IsInvalid)
                        {
                            // Determine if we still need to move one more time on the heap
                            // Peek into the pointed data using nextPtr. 
                            // If nextPtr is not a pointer (a primitive) then return the data at nextPtr
                            var data = rmem.Heap.ToHeapObject<DSObject>(fpSv).GetValueFromIndex(0, runtimeCore);

                            bool isActualData = !data.IsPointer && !data.IsArray && !data.IsInvalid; 
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
                        fptr = fpSv.FunctionPointer;
                    }
                }
            }

            ProcedureNode procNode = null;
            if (isFunctionPointerCall)
            {
                FunctionPointerNode fptrNode;
                if (exe.FuncPointerTable.functionPointerDictionary.TryGetByFirst(fptr, out fptrNode))
                {
                    int blockId = fptrNode.blockId;
                    int procId = fptrNode.procId;
                    int classId = fptrNode.classScope;

                    if (Constants.kGlobalScope == classId)
                    {
                        procName = exe.procedureTable[blockId].Procedures[procId].Name;
                        CodeBlock codeblock = ProtoCore.Utils.CoreUtils.GetCodeBlock(exe.CodeBlocks, blockId);
                        procNode = CoreUtils.GetFunctionBySignature(procName, arglist, codeblock);
                    }
                    else
                    {
                        procNode = exe.classTable.ClassNodes[classId].ProcTable.Procedures[procId];
                        isMemberFunctionPointer = !procNode.IsConstructor && !procNode.IsStatic;                        
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
                    ProcedureNode memProcNode = exe.classTable.ClassNodes[type].GetMemberFunction(procName, arglist, classIndex, out isAccessible, out realType);

                    if (memProcNode == null)
                    {
                        string property;
                        if (CoreUtils.TryGetPropertyName(procName, out property))
                        {
                            string classname = exe.classTable.ClassNodes[type].Name;
                            string message = String.Format(Resources.kPropertyOfClassNotFound, property, classname);
                            runtimeCore.RuntimeStatus.LogWarning(WarningID.MethodResolutionFailure, message);
                        }
                        else
                        {
                            string message = String.Format(Resources.kMethodResolutionFailure, procName);
                            runtimeCore.RuntimeStatus.LogWarning(WarningID.MethodResolutionFailure, message);
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

            if (null != procNode && Constants.kInvalidIndex != procNode.ID)
            {
                if (isFunctionPointerCall && depth > 0) //constructor or static function or function pointer call
                {
                    rmem.Pop(); //remove the array dimension for "isLeftClass" or final pointer for "isFunctionPointerCall"
                    depth = 0;
                }

                //push back the function arguments
                for (int i = argSvList.Count - 1; i >= 0; i--)
                {
                    rmem.Push(argSvList[i]);
                }
                //push value-not-provided default argument
                for (int i = arglist.Count; i < procNode.ArgumentInfos.Count; i++)
                {
                    rmem.Push(StackValue.BuildDefaultArgument());
                }

                // Push the function declaration block  
                StackValue opblock = StackValue.BuildBlockIndex(procNode.RuntimeIndex);
                instr.op3 = opblock;

                rmem.Push(StackValue.BuildInt(depth));

                //Modify the operand data
                instr.op1 = StackValue.BuildFunctionIndex(procNode.ID);
                instr.op2 = StackValue.BuildClassIndex(type);

                return true;
            }

            return false;
        }

        // GC for local code blocks if,for,while
        public void GCCodeBlock(int blockId, int functionIndex = Constants.kGlobalScope, int classIndex = Constants.kInvalidIndex)
        {
            foreach (SymbolNode sn in exe.runtimeSymbols[blockId].symbolList.Values)
            {   
                bool allowGC = sn.classScope == classIndex 
                    && sn.functionIndex == functionIndex 
                    && !sn.name.Equals(Constants.kWatchResultVar);

                if (runtimeCore.Options.GCTempVarsOnDebug && runtimeCore.Options.ExecuteSSA)
                {
                    if (runtimeCore.Options.IDEDebugMode)
                    {
                        allowGC = sn.classScope == classIndex 
                            && sn.functionIndex == functionIndex 
                            && !sn.name.Equals(Constants.kWatchResultVar)
                            && !sn.isSSATemp;
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

        public void ReturnSiteGC(int blockId, int classIndex, int functionIndex)
        {
            foreach (CodeBlock cb in exe.CompleteCodeBlocks[blockId].children)
            {
                if (cb.blockType == CodeBlockType.Construct)
                    GCCodeBlock(cb.codeBlockId, functionIndex, classIndex);
            }
        }

        public void Modify_istream_instrList_FromSetValue(int blockId, int pc, StackValue op)
        {
            exe.instrStreamList[blockId].instrList[pc].op1 = op;
        }

        public void Modify_istream_entrypoint_FromSetValue(int blockId, int pc)
        {
            exe.instrStreamList[blockId].entrypoint = pc;
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
            if (classIndex == Constants.kGlobalScope)
            {
                return exe.procedureTable[blockId].Procedures[functionIndex];
            }
            else
            { 
                return exe.classTable.ClassNodes[classIndex].ProcTable.Procedures[functionIndex];
            }
        }

        private void GetLocalAndParamCount(int blockId, int classIndex, int functionIndex, out int localCount, out int paramCount)
        {
            localCount = paramCount = 0;

            if (Constants.kGlobalScope != classIndex)
            {
                localCount = exe.classTable.ClassNodes[classIndex].ProcTable.Procedures[functionIndex].LocalCount;
                paramCount = exe.classTable.ClassNodes[classIndex].ProcTable.Procedures[functionIndex].ArgumentTypes.Count;
            }
            else
            {
                localCount = exe.procedureTable[blockId].Procedures[functionIndex].LocalCount;
                paramCount = exe.procedureTable[blockId].Procedures[functionIndex].ArgumentTypes.Count;
            }
        }

        public List<List<ReplicationGuide>> GetCachedReplicationGuides(int argumentCount)
        {
            int index = runtimeCore.ReplicationGuides.Count - argumentCount;
            if (index >= 0)
            {
                var replicationGuides = runtimeCore.ReplicationGuides.GetRange(index, argumentCount);
                runtimeCore.ReplicationGuides.RemoveRange(index, argumentCount);
                return replicationGuides;
            }
            return new List<List<ReplicationGuide>>();
        }

        public List<AtLevel> GetCachedAtLevels(int argumentCount)
        {
            int index = runtimeCore.AtLevels.Count - argumentCount;
            if (index >= 0)
            {
                var atLevels = runtimeCore.AtLevels.GetRange(index, argumentCount);
                runtimeCore.AtLevels.RemoveRange(index, argumentCount);
                return atLevels;
            }
            return new List<AtLevel>();
        }

        #region Opcode Handlers
        private void ALLOCC_Handler(Instruction instruction)
        {
            fepRunStack.Push(fepRun);
            int type = instruction.op1.ClassIndex;
            MetaData metadata;
            metadata.type = type;
            StackValue pointer = rmem.Heap.AllocatePointer(exe.classTable.ClassNodes[type].Size, metadata);
            rmem.SetAtRelative(StackFrame.FrameIndexThisPtr, pointer);

            ++pc;
        }

        private void LOADELEMENT_Handler(Instruction instruction)
        {
            int dimensions = rmem.Pop().ArrayDimension;
            var dims = new List<StackValue>();
            for (int n = 0; n < dimensions; n++)
            {
                dims.Add(rmem.Pop());
            }
            dims.Reverse();
            StackValue array = rmem.Pop();

            if (instruction.op1.IsVariableIndex ||
                instruction.op1.IsMemberVariableIndex ||
                instruction.op1.IsPointer ||
                instruction.op1.IsArray ||
                instruction.op1.IsStaticVariableIndex ||
                instruction.op1.IsFunctionPointer)
            {
                int blockId = instruction.op3.BlockIndex;
                StackValue element = GetIndexedArray(dims, blockId, instruction.op1, instruction.op2);
                rmem.Push(element);
            }
            else
            {
                StackValue element = GetIndexedArray(array, dims);
                rmem.Push(element);
            }

            ++pc;
        }

        private void PUSH_Handler(Instruction instruction)
        {
            int blockId = Constants.kInvalidIndex;
            StackValue op1 = instruction.op1;

            if (op1.IsVariableIndex ||
                op1.IsMemberVariableIndex ||
                op1.IsPointer ||
                op1.IsArray ||
                op1.IsStaticVariableIndex ||
                op1.IsFunctionPointer)
            {
                blockId = instruction.op3.BlockIndex;
            }

            int fp = runtimeCore.RuntimeMemory.FramePointer;

            if (runtimeCore.Options.RunMode == InterpreterMode.Expression && instruction.op1.IsThisPtr)
            {
                runtimeCore.RuntimeMemory.FramePointer = runtimeCore.watchFramePointer;
            }

            StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);

            if (runtimeCore.Options.RunMode == InterpreterMode.Expression && instruction.op1.IsThisPtr)
            {
                runtimeCore.RuntimeMemory.FramePointer = fp;
            }

            rmem.Push(opdata1);

            ++pc;
        }

        private void PUSHW_Handler(Instruction instruction)
        {
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

                StackValue svBlock = instruction.op3; 
                blockId = svBlock.BlockIndex;
            }

            int fp = runtimeCore.RuntimeMemory.FramePointer;
            if (runtimeCore.Options.RunMode == InterpreterMode.Expression)
                runtimeCore.RuntimeMemory.FramePointer = runtimeCore.watchFramePointer;

            PushW(blockId, op1, op2);

            if (runtimeCore.Options.RunMode == InterpreterMode.Expression)
                runtimeCore.RuntimeMemory.FramePointer = fp;

            ++pc;
        }

        private void PUSHB_Handler(Instruction instruction)
        {
            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                runtimeCore.RuntimeMemory.PushConstructBlockId(instruction.op1.BlockIndex);
            }
            ++pc;
        }

        private void PUSHM_Handler(Instruction instruction)
        {
            int blockId = instruction.op3.BlockIndex;

            if (instruction.op1.IsStaticVariableIndex)
            {
                rmem.Push(StackValue.BuildBlockIndex(blockId));
                rmem.Push(instruction.op1);
            }
            else if (instruction.op1.IsClassIndex)
            {
                rmem.Push(StackValue.BuildClassIndex(instruction.op1.ClassIndex));
            }
            else
            {
                StackValue opdata1 = GetOperandData(blockId, instruction.op1, instruction.op2);
                rmem.Push(opdata1);
            }

            ++pc;
        }

        private void PUSH_VARSIZE_Handler(Instruction instruction)
        {
            // TODO Jun: This is a temporary solution to retrieving the array size until lib files are implemented
            int symbolIndex = instruction.op1.VariableIndex;
            int blockId = instruction.op2.BlockIndex;
            int classIndex = instruction.op3.ClassIndex;

            SymbolNode snode = GetSymbolNode(blockId, classIndex, symbolIndex);
            runtimeVerify(null != snode);

            StackValue svArrayToIterate = rmem.GetSymbolValue(snode);

            // Check if the array to iterate is a valid array
            StackValue key = StackValue.Null;
            if (svArrayToIterate.IsArray)
            {
                var array = rmem.Heap.ToHeapObject<DSArray>(svArrayToIterate);
                if (array.Keys.Any())
                {
                    key = StackValue.BuildArrayKey(svArrayToIterate, 0);
                    key.metaData = svArrayToIterate.metaData;
                }
                else
                {
                    key = StackValue.Null;
                }
            }
            else if (svArrayToIterate.IsString)
            {
                var str = rmem.Heap.ToHeapObject<DSString>(svArrayToIterate);
                if (str.Value.Any())
                {
                    key = StackValue.BuildArrayKey(svArrayToIterate, 0);
                    key.metaData = svArrayToIterate.metaData;
                }
                else
                {
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

        private void PUSHREPGUIDE_Handler(Instruction instruction)
        {
            runtimeVerify(instruction.op1.IsReplicationGuide);
            runtimeVerify(instruction.op2.IsBoolean);
            rmem.Push(instruction.op1);
            rmem.Push(instruction.op2);
            ++pc;
        }

        private void PUSHLEVEL_Handler(Instruction instruction)
        {
            int level = (int)instruction.op1.IntegerValue;
            bool isDominant = instruction.op2.BooleanValue;

            runtimeCore.AtLevels.Add(new AtLevel(level, isDominant));
            ++pc;
        } 

        private void POPREPGUIDES_Handler(Instruction instruction)
        {
            int guides = instruction.op1.ReplicationGuide;
            List<ReplicationGuide> argGuides = new List<ReplicationGuide>();
            for (int i = 0; i < guides; ++i)
            {
                StackValue svGuideProperty = rmem.Pop();
                bool isLongest = svGuideProperty.BooleanValue;

                StackValue svGuide = rmem.Pop();
                int guideNumber = svGuide.ReplicationGuide;

                argGuides.Add(new ReplicationGuide(guideNumber, isLongest));
            }

            argGuides.Reverse();
            runtimeCore.ReplicationGuides.Add(argGuides);
            ++pc;
        }

        private void SETELEMENT_Helper(Instruction instruction)
        {
            StackValue svDim = rmem.Pop();
            int dimensions = svDim.ArrayDimension;

            int blockId = instruction.op3.BlockIndex;

            bool isSSANode = Properties.executingGraphNode != null && Properties.executingGraphNode.IsSSANode();
            StackValue svData;

            // The returned stackvalue is used by watch test framework - pratapa
            StackValue tempSvData = StackValue.Null;
            List<StackValue> dimList = new List<StackValue>();
            for (int i = 0; i < dimensions; ++i)
            {
                dimList.Add(rmem.Pop());
            }
            dimList.Reverse();

            svData = rmem.Pop();
            tempSvData = svData;
            PopToIndexedArray(blockId, instruction.op1.SymbolIndex, instruction.op2.ClassIndex, dimList, svData);

            rmem.Heap.GC();
            ++pc;
        }

        private void CAST_Handler(Instruction instruction)
        {
            StackValue type = instruction.op1;
            int staticType = type.metaData.type;
            int rank = type.Rank;

            StackValue tempSvData = StackValue.Null;
            StackValue data = rmem.Pop();
            StackValue coercedData = TypeSystem.Coerce(data, staticType, rank, runtimeCore);
            rmem.Push(coercedData);
            ++pc;
        }

        protected StackValue POP_helper(Instruction instruction, out int blockId, out int dimensions)
        {
            dimensions = 0;
            blockId = instruction.op3.BlockIndex;

            StackValue svData = rmem.Pop();

            bool isSSANode = Properties.executingGraphNode != null && Properties.executingGraphNode.IsSSANode();
            if (isSSANode)
            {
                // Double check to avoid the case like
                //    %tvar = obj;
                //    %tSSA = %tvar;
                blockId = runtimeCore.RunningBlock;
            }
            else if (svData.IsArray)
            {
                svData = runtimeCore.Heap.ToHeapObject<DSArray>(svData).CopyArray(runtimeCore); ;
            }

            StackValue tempSvData = svData;
            var preValue = PopTo(blockId, instruction.op1, instruction.op2, svData);

            if (runtimeCore.Options.ExecuteSSA)
            {
                if (!isSSANode)
                {
                    if (preValue.IsPointer && svData.IsPointer)
                    {
                        if (preValue.Pointer != svData.Pointer)
                        {
                            if (null != Properties.executingGraphNode)
                            {
                                Properties.executingGraphNode.reExecuteExpression = true;
                            }
                        }
                    }
                }
            }

            rmem.Heap.GC();
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
            int blockId = Constants.kInvalidIndex;
            int staticType = (int)PrimitiveType.Var;
            int rank = Constants.kArbitraryRank;
            if (instruction.op1.IsVariableIndex ||
                instruction.op1.IsPointer ||
                instruction.op1.IsArray)
            {
                StackValue svBlock = instruction.op3;
                blockId = svBlock.BlockIndex;
            }

            StackValue svData = rmem.Pop();
            StackValue coercedValue = TypeSystem.Coerce(svData, staticType, rank, runtimeCore);
            PopToW(blockId, instruction.op1, instruction.op2, coercedValue);

            rmem.Heap.GC();
            ++pc;
        }

        private void SETMEMELEMENT_Helper(Instruction instruction)
        {
            int classIndex = Constants.kInvalidIndex;

            StackValue op1 = instruction.op1;

            StackValue svBlock = instruction.op2;
            int blockId = svBlock.BlockIndex;

            StackValue svDim = rmem.Pop();
            int dimensions = svDim.ArrayDimension;

            List<StackValue> dimList = new List<StackValue>();
            for (int i = 0; i < dimensions; ++i)
            {
                dimList.Insert(0, rmem.Pop());
            }

            StackValue svData = rmem.Pop();

            // The returned stackvalue is used by watch test framework - pratapa
            svData.metaData.type = exe.TypeSystem.GetType(svData);
            if (instruction.op1.IsStaticVariableIndex)
            {
                PopToIndexedArray(blockId, instruction.op1.SymbolIndex, Constants.kGlobalScope, dimList, svData);
                ++pc;
                return;
            }

            int symbolIndex = instruction.op1.SymbolIndex;
            classIndex = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
            int stackIndex = exe.classTable.ClassNodes[classIndex].Symbols.symbolList[symbolIndex].index;

            //==================================================
            //  1. If allocated... bypass auto allocation
            //  2. If pointing to a class, just point to the class directly, do not allocate a new pointer
            //==================================================

            StackValue svThis = rmem.CurrentStackFrame.ThisPtr;
            var thisObject = rmem.Heap.ToHeapObject<DSObject>(svThis);
            StackValue svProperty = thisObject.GetValueFromIndex(stackIndex, runtimeCore);

            SymbolNode symbolnode = GetSymbolNode(blockId, classIndex, symbolIndex);

            if (svProperty.IsPointer || (svProperty.IsArray && dimensions == 0))
            {
                // The data to assign is already a pointer
                if (svData.IsPointer || svData.IsArray)
                {
                    // Assign the src pointer directily to this property
                    thisObject.SetValueAtIndex(stackIndex, svData, runtimeCore);
                }
                else
                {
                    try
                    {
                        StackValue svNewProperty = rmem.Heap.AllocatePointer(new[] { svData });
                        thisObject.SetValueAtIndex(stackIndex, svNewProperty, runtimeCore);
                    }
                    catch (RunOutOfMemoryException)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                        thisObject.SetValueAtIndex(stackIndex, StackValue.Null, runtimeCore);
                    }
                }
            }
            else if (svProperty.IsArray)
            {
                var propertyArray = rmem.Heap.ToHeapObject<DSArray>(svProperty);
                propertyArray.SetValueForIndices(dimList, svData, runtimeCore);
            }
            else // This property has NOT been allocated
            {
                if (svData.IsPointer || svData.IsArray)
                {
                    thisObject.SetValueAtIndex(stackIndex, svData, runtimeCore);
                }
                else
                {
                    StackValue svNewProperty = rmem.Heap.AllocatePointer(new[] { svData });
                    thisObject.SetValueAtIndex(stackIndex, svNewProperty, runtimeCore);
                }
            }

            ++pc;

            return;
        }

        protected StackValue POPM_Helper(Instruction instruction, out int blockId, out int classIndex)
        {
            classIndex = Constants.kInvalidIndex;

            StackValue op1 = instruction.op1;

            StackValue svBlock = instruction.op2;
            blockId = svBlock.BlockIndex;

            StackValue svData = rmem.Pop();

            // The returned stackvalue is used by watch test framework - pratapa
            StackValue tempSvData = svData;

            svData.metaData.type = exe.TypeSystem.GetType(svData);

            // TODO(Jun/Jiong): Find a more reliable way to update the current block Id
            //runtimeCore.DebugProps.CurrentBlockId = blockId;

            if (instruction.op1.IsStaticVariableIndex)
            {
                PopTo(blockId, instruction.op1, instruction.op2, svData);
                ++pc;
                return tempSvData;
            }

            int symbolIndex = instruction.op1.SymbolIndex;
            classIndex = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex).ClassIndex;
            int stackIndex = exe.classTable.ClassNodes[classIndex].Symbols.symbolList[symbolIndex].index;

            //==================================================
            //  1. If allocated... bypass auto allocation
            //  2. If pointing to a class, just point to the class directly, do not allocate a new pointer
            //==================================================

            StackValue svThis = rmem.CurrentStackFrame.ThisPtr;
            var thisObject = rmem.Heap.ToHeapObject<DSObject>(svThis);
            StackValue svProperty = thisObject.GetValueFromIndex(stackIndex, runtimeCore);

            Type targetType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);

            if (svProperty.IsPointer || svProperty.IsArray)
            {
                // The data to assign is already a pointer
                if (svData.IsPointer || svData.IsArray)
                {
                    // Assign the src pointer directily to this property
                    thisObject.SetValueAtIndex(stackIndex, svData, runtimeCore);
                }
                else
                {
                    StackValue svNewProperty = rmem.Heap.AllocatePointer(new [] { svData });
                    thisObject.SetValueAtIndex(stackIndex, svNewProperty, runtimeCore);
                }
            }
            else // This property has NOT been allocated
            {
                if (svData.IsPointer || svData.IsArray)
                {
                    thisObject.SetValueAtIndex(stackIndex, svData, runtimeCore);
                }
                else
                {
                    try
                    {
                        StackValue svNewProperty = rmem.Heap.AllocatePointer(new[] { svData });
                        thisObject.SetValueAtIndex(stackIndex, svNewProperty, runtimeCore);
                    }
                    catch (RunOutOfMemoryException)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                        thisObject.SetValueAtIndex(stackIndex, StackValue.Null, runtimeCore);
                    }
                }
            }

            ++pc;

            return svData;
        }

        protected virtual void POPM_Handler(Instruction instruction)
        {
            int blockId;
            int ci;
            POPM_Helper(instruction, out blockId, out ci);
        }

        private void ADD_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            // Need to optmize these if-elses to a table. 
            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata1.IntegerValue + opdata2.IntegerValue);

            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata1.IsDouble ? opdata1.DoubleValue : opdata1.IntegerValue;
                double value2 = opdata2.IsDouble ? opdata2.DoubleValue : opdata2.IntegerValue;

                opdata2 = StackValue.BuildDouble(value1 + value2);
            }
            else if (opdata1.IsString || opdata2.IsString)
            {
                opdata2 = CoreUtils.AddStackValueString(opdata1, opdata2, runtimeCore);
            }
            else if (opdata2.IsArrayKey && opdata1.IsInteger)
            {
                if (opdata1.IntegerValue == 1)
                {
                    opdata2 = opdata2.GetNextKey(runtimeCore);
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

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void SUB_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata2.IntegerValue - opdata1.IntegerValue);
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                double value2 = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                opdata2 = StackValue.BuildDouble(value1 - value2);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void MUL_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsInteger && opdata2.IsInteger)
            {
                opdata2 = StackValue.BuildInt(opdata1.IntegerValue * opdata2.IntegerValue);
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata1.IsDouble ? opdata1.DoubleValue : opdata1.IntegerValue;
                double value2 = opdata2.IsDouble ? opdata2.DoubleValue : opdata2.IntegerValue;
                opdata2 = StackValue.BuildDouble(value1 * value2);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void DIV_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            //division is always carried out as a double
            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double lhs = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                double rhs = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                opdata2 = StackValue.BuildDouble(lhs / rhs);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void MOD_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsInteger && opdata2.IsInteger)
                {
                    long lhs = opdata2.IntegerValue;
                    long rhs = opdata1.IntegerValue;
                    if (rhs == 0)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(WarningID.ModuloByZero, Resources.ModuloByZero);
                        opdata2 = StackValue.Null;
                    }
                    else
                    {
                        var intMod = lhs % rhs;

                        // In order to follow the conventions of Java, Python, Scala and Google's calculator,
                        // the returned modulo will follow the sign of the divisor (not the dividend). 

                        if (intMod < 0 && rhs > 0 || intMod > 0 && rhs < 0)
                        {
                            intMod = intMod + rhs;
                        }

                        opdata2 = StackValue.BuildInt(intMod);
                    }
                }
                else
                {
                    double lhs = opdata2.IsDouble ? opdata2.DoubleValue : opdata2.IntegerValue;
                    double rhs = opdata1.IsDouble ? opdata1.DoubleValue : opdata1.IntegerValue;
                    var doubleMod = lhs % rhs;

                    if (doubleMod < 0 && rhs > 0 || doubleMod > 0 && rhs < 0)
                    {
                        doubleMod = doubleMod + rhs;
                    }
                    opdata2 = StackValue.BuildDouble(doubleMod);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
       
        private void NEG_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            if (opdata1.IsInteger)
            {
                opdata1 = StackValue.BuildInt(-opdata1.IntegerValue);
            }
            else if (opdata1.IsDouble)
            {
                opdata1 = StackValue.BuildDouble(-opdata1.DoubleValue);
            }
            else 
            {
                opdata1 = StackValue.Null;
            }

            rmem.Push(opdata1);
            ++pc;
        }

        private void AND_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            opdata1 = opdata1.ToBoolean(runtimeCore);
            opdata2 = opdata2.ToBoolean(runtimeCore);
            if (opdata1.IsNull || opdata2.IsNull)
            {
                opdata2 = StackValue.Null;
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata2.BooleanValue && opdata1.BooleanValue);
            }
            rmem.Push(opdata2);

            ++pc;
        }

        private void OR_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            opdata1 = opdata1.ToBoolean(runtimeCore);
            opdata2 = opdata2.ToBoolean(runtimeCore);
            if (opdata1.IsNull || opdata2.IsNull)
            {
                opdata2 = StackValue.Null;
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata2.BooleanValue || opdata1.BooleanValue);
            }

            rmem.Push(opdata2);
            ++pc;
        }

        private void NOT_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();

            opdata1 = opdata1.ToBoolean(runtimeCore);
            if (!opdata1.IsNull)
            {
                opdata1 = StackValue.BuildBoolean(!opdata1.BooleanValue);
            }

            rmem.Push(opdata1);
            ++pc;
        }

        private void EQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsBoolean || opdata2.IsBoolean)
            {
                opdata1 = opdata1.ToBoolean(runtimeCore);
                opdata2 = opdata2.ToBoolean(runtimeCore);
                if (opdata1.IsNull || opdata2.IsNull) 
                {
                    opdata2 = StackValue.Null;
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.BooleanValue == opdata2.BooleanValue);
                }
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double value1 = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                    double value2 = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                    opdata2 = StackValue.BuildBoolean(MathUtils.Equals(value1, value2));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.IntegerValue== opdata2.IntegerValue);
                }
            }
            else if (opdata1.IsString && opdata2.IsString)
            {
                int diffIndex = StringUtils.CompareString(opdata2, opdata1, runtimeCore);
                opdata2 = StackValue.BuildBoolean(diffIndex == 0);
            }
            else
            {
                opdata2 = StackValue.BuildBoolean(opdata1.Equals(opdata2));
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void NQ_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsBoolean || opdata2.IsBoolean)
            {
                opdata1 = opdata1.ToBoolean(runtimeCore);
                opdata2 = opdata2.ToBoolean(runtimeCore);
                if (opdata1.IsNull || opdata2.IsNull) 
                {
                    opdata2 = StackValue.Null;
                }
                else
                {

                    opdata2 = StackValue.BuildBoolean(opdata1.BooleanValue != opdata2.BooleanValue);
                }
            }
            else if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double value1 = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                    double value2 = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                    opdata2 = StackValue.BuildBoolean(!MathUtils.Equals(value1, value2));
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata1.IntegerValue != opdata2.IntegerValue);
                }
            }
            else if (opdata1.IsString && opdata2.IsString)
            {
                int diffIndex = StringUtils.CompareString(opdata1, opdata2, runtimeCore);
                opdata2 = StackValue.BuildBoolean(diffIndex != 0);
            }
            else 
            {
                opdata2 = StackValue.BuildBoolean(!opdata1.Equals(opdata2));
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void GT_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                var value1 = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                var value2 = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                opdata2 = StackValue.BuildBoolean(value1 > value2);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void LT_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                double value1 = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                double value2 = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                opdata2 = StackValue.BuildBoolean(value1 < value2);
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void GE_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double lhs = opdata2.IsDouble ? opdata2.DoubleValue : opdata2.IntegerValue;
                    double rhs = opdata1.IsDouble ? opdata1.DoubleValue : opdata1.IntegerValue;
                    opdata2 = StackValue.BuildBoolean(lhs >= rhs);
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata2.IntegerValue >= opdata1.IntegerValue);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }
        
        private void LE_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            StackValue opdata2 = rmem.Pop();

            if (opdata1.IsNumeric && opdata2.IsNumeric)
            {
                if (opdata1.IsDouble || opdata2.IsDouble)
                {
                    double lhs = opdata2.IsDouble ? opdata2.DoubleValue: opdata2.IntegerValue;
                    double rhs = opdata1.IsDouble ? opdata1.DoubleValue: opdata1.IntegerValue;
                    opdata2 = StackValue.BuildBoolean(lhs <= rhs);
                }
                else
                {
                    opdata2 = StackValue.BuildBoolean(opdata2.IntegerValue <= opdata1.IntegerValue);
                }
            }
            else
            {
                opdata2 = StackValue.Null;
            }

            rmem.Push(opdata2);
            ++pc;
        }

        private void ALLOCA_Handler(Instruction instruction)
        {
            StackValue op1 = instruction.op1;
            int size;
            if (op1.IsInteger)
            {
                size = (int)op1.IntegerValue; //Number of the elements in the array
            }
            else
            {
                StackValue arraySize = GetOperandData(op1);
                size = (int)arraySize.IntegerValue;
            }

            runtimeVerify(Constants.kInvalidIndex != size);

            StackValue[] svs = new StackValue[size];
            for (int i = size - 1; i >= 0; i--)
            {
                StackValue value = rmem.Pop();
                svs[i] = value;
            }
            StackValue pointer;
            try
            {
                pointer = rmem.Heap.AllocateArray(svs);
            }
            catch (RunOutOfMemoryException)
            {
                pointer = StackValue.Null;
                runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
            }

            if (instruction.op2.IsString)
            {
                pointer = StackValue.BuildString(pointer.ArrayPointer);
            }
            rmem.Push(pointer);

            if (instruction.op3.IsReplicationGuide)
            {
                Validity.Assert(instruction.op3.ReplicationGuide == 0);
                runtimeCore.ReplicationGuides.Add(new List<ReplicationGuide> { });
            }

            ++pc;
        }

        private void BOUNCE_Handler(Instruction instruction)
        {
            // We disallow language blocks inside watch window currently - pratapa
            Validity.Assert(InterpreterMode.Expression != runtimeCore.Options.RunMode);

            int blockId = instruction.op1.BlockIndex;

            // Comment Jun: On a bounce, update the debug property to reflect this.
            // Before the explicit bounce, this was done in Execute() which is now no longer the case
            // as Execute is only called once during first bounce and succeeding bounce reuse the same interpreter
            runtimeCore.DebugProps.CurrentBlockId = blockId;

            // TODO(Jun/Jiong): Considering store the orig block id to stack frame
            runtimeCore.RunningBlock = blockId;

            runtimeCore.RuntimeMemory = rmem;
            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                runtimeCore.RuntimeMemory.PushConstructBlockId(blockId);
            }

            int ci = Constants.kInvalidIndex;
            int fi = Constants.kInvalidIndex;
            if (rmem.Stack.Count >= StackFrame.StackFrameSize)
            {
                StackValue sci = rmem.GetAtRelative(StackFrame.FrameIndexClassIndex);
                StackValue sfi = rmem.GetAtRelative(StackFrame.FrameIndexFunctionIndex);
                if (sci.IsClassIndex && sfi.IsFunctionIndex)
                {
                    ci = sci.ClassIndex;
                    fi = sfi.FunctionIndex;
                }
            }

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
            int blockDecl = rmem.GetAtRelative(StackFrame.FrameIndexFunctionBlockIndex).BlockIndex;
            int blockCaller = executingBlock;

            StackFrameType type = StackFrameType.LanguageBlock;
            int depth = (int)rmem.GetAtRelative(StackFrame.FrameIndexStackFrameDepth).IntegerValue;
            int framePointer = runtimeCore.RuntimeMemory.FramePointer;

            // Comment Jun: Use the register TX to store explicit/implicit bounce state
            bounceType = CallingConvention.BounceType.Explicit;
            TX = StackValue.BuildCallingConversion((int)CallingConvention.BounceType.Explicit);

            List<StackValue> registers = GetRegisters();

            StackFrameType callerType = (fepRun) ? StackFrameType.Function : StackFrameType.LanguageBlock;


            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                // Comment Jun: Temporarily disable debug mode on bounce
                //Validity.Assert(false); 

                //Validity.Assert(runtimeCore.Breakpoints != null);
                //blockDecl = blockCaller = runtimeCore.DebugProps.CurrentBlockId;

                runtimeCore.DebugProps.SetUpBounce(this, blockCaller, returnAddr);

                StackFrame stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth + 1, framePointer, 0, registers, 0);
                Language bounceLangauge = exe.instrStreamList[blockId].language;
                BounceExplicit(blockId, 0, bounceLangauge, stackFrame, runtimeCore.Breakpoints);
            }
            else //if (runtimeCore.Breakpoints == null)
            {
                StackFrame stackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth + 1, framePointer, 0, registers, 0);

                Language bounceLangauge = exe.instrStreamList[blockId].language;
                BounceExplicit(blockId, 0, bounceLangauge, stackFrame);
            }
        }

        private void CALL_Handler(Instruction instruction)
        {
            PushInterpreterProps(Properties);

            int fi = instruction.op1.FunctionIndex;
            int ci = instruction.op2.ClassIndex;

            rmem.Pop();

            StackValue svBlock = rmem.Pop();
            int blockId = svBlock.BlockIndex;
            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                rmem.PushConstructBlockId(blockId);
            }

            ProcedureNode fNode;
            if (ci != Constants.kInvalidIndex)
            {
                fNode = exe.classTable.ClassNodes[ci].ProcTable.Procedures[fi];
            }
            else
            {
                fNode = exe.procedureTable[blockId].Procedures[fi];
            }

            // Disabling support for stepping into replicating function calls temporarily 
            // This CALL instruction has a corresponding RETC instruction
            // and for debugger purposes for every RETURN/RETC where we restore the states,
            // we need a corresponding SetUpCallr to save the states. Therefore this call here - pratapa
            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                runtimeCore.DebugProps.SetUpCallrForDebug(runtimeCore, this, fNode, pc, true);
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

            if (instruction.op3.IsInteger && instruction.op3.IntegerValue >= 0)
            {
                // thisptr should be the pointer to the instance of derive class
                svThisPointer = rmem.GetAtRelative(StackFrame.FrameIndexThisPtr);
                // how many instruction offset? basically it should be 1 to skip ALLOCC
                pcoffset = (int)instruction.op3.IntegerValue;

                // To simulate CALLR. We have to retrive the param values from the
                // stack and reverse these values and save back to the stack. Otherwise
                // in base constructor all params will be in reverse order
                List<StackValue> argvalues = new List<StackValue>();
                int stackindex = rmem.Stack.Count - 1;
                for (int idx = 0; idx < fNode.ArgumentTypes.Count; ++idx)
                {
                    StackValue value = rmem.Stack[stackindex--];
                    argvalues.Add(value);

                    // Probably it is useless in calling base constructor
                    bool hasGuide = rmem.Stack[stackindex].IsReplicationGuide;
                    if (hasGuide)
                    {
                        var replicationGuideList = new List<int>();

                        // Retrieve replication guides
                        value = rmem.Stack[stackindex--];
                        int guides = value.ReplicationGuide;
                        if (guides > 0)
                        {
                            for (int i = 0; i < guides; ++i)
                            {
                                value = rmem.Stack[stackindex--];
                                replicationGuideList.Add(value.ReplicationGuide);
                            }
                        }
                        replicationGuideList.Reverse();
                    }
                }
                rmem.PopFrame(fNode.ArgumentTypes.Count);
                for (int idx = 0; idx < fNode.ArgumentTypes.Count; ++idx)
                {
                    rmem.Push(argvalues[idx]);
                }
            }

            // TODO: Jun: To set these at compile time. They are being hardcoded to 0 temporarily as
            // class methods are always defined only at the global scope
            int blockDecl = 0;
            int blockCaller = 0;


            // Comment Jun: the caller type is the current type in the stackframe
            StackFrameType callerType = (fepRun) ? StackFrameType.Function : StackFrameType.LanguageBlock;

            StackValue svCallConvention = StackValue.BuildCallingConversion((int)CallingConvention.CallType.ExplicitBase);
            TX = svCallConvention;


            // On implicit call, the SX is set in JIL Fep
            // On explicit call, the SX should be directly set here

            List<StackValue> registers = GetRegisters();

            // Comment Jun: the depth is always 0 for a function call as we are reseting this for each function call
            // This is only incremented for every language block bounce
            int depth = 0;

            StackFrameType type = StackFrameType.Function;

            StackFrame stackFrame = new StackFrame(svThisPointer, ci, fi, pc + 1, blockDecl, blockCaller, callerType, type, depth, rmem.FramePointer, blockDecl, registers, 0);

            rmem.PushFrameForLocals(fNode.LocalCount);
            rmem.PushStackFrame(stackFrame);

            // Now let's go to the function
            pc = fNode.PC + pcoffset;
            fepRunStack.Push(false);

            // A standard call instruction must reset the graphnodes for associative
            if (Language.Associative == executingLanguage)
            {
                UpdateMethodDependencyGraph(pc, fi, ci);
            }

            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                rmem.PopConstructBlockId();
            }
            SetupGraphNodesInScope();
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

            int functionIndex = instr.op1.FunctionIndex;
            int classIndex = instr.op2.ClassIndex;
            int blockIndex = instr.op3.BlockIndex;

            ++runtimeCore.FunctionCallDepth;

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
                RX = CallrForMemberFunction(blockIndex, classIndex, functionIndex, instr.debug != null, ref explicitCall);
            }
            else if (!runtimeCore.Options.IsDeltaExecution)
            {
                RX = Callr(blockIndex, functionIndex, classIndex, ref explicitCall, isDynamicCall, instr.debug != null);
            }
            else
            {
                //
                // Comment Jun:
                //      Running in graph mode, nullify the result and continue.
                //      The only affected downstream operations are the ones connected to the graph associated with this call
                try
                {
                    RX = Callr(blockIndex, functionIndex, classIndex, ref explicitCall, isDynamicCall, instr.debug != null);
                }
                catch (ReplicationCaseNotCurrentlySupported e)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.ReplicationWarning, e.Message);
                    RX = StackValue.Null;
                }
            }

            --runtimeCore.FunctionCallDepth;

            if (!explicitCall)
            {
                ++pc;
            }
        }

        private void RETB_Handler()
        {
            RX = rmem.Pop();

            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                runtimeCore.RuntimeMemory.PopConstructBlockId();
            }

            if (!runtimeCore.Options.IsDeltaExecution || (runtimeCore.Options.IsDeltaExecution && 0 != runtimeCore.RunningBlock))
            {
                GCCodeBlock(runtimeCore.RunningBlock);
            }

            if (CallingConvention.BounceType.Explicit == bounceType)
            {
                RestoreFromBounce();
                runtimeCore.RunningBlock = executingBlock;
            }

            if (CallingConvention.BounceType.Implicit == bounceType)
            {
                pc = (int)rmem.GetAtRelative(StackFrame.FrameIndexReturnAddress).IntegerValue;
                terminate = true;
            }


            StackFrameType type;

            // Comment Jun: Just want to see if this is the global rerb, in which case we dont retrieve anything
            //if (executingBlock > 0)
            {
                StackValue svCallerType = rmem.GetAtRelative(StackFrame.FrameIndexCallerStackFrameType);
                type = svCallerType.FrameType;
            }

            // Pop the frame as we are adding stackframes for language blocks as well - pratapa
            // Do not do this for the final Retb 
            //if (runtimeCore.RunningBlock != 0)
            if (!runtimeCore.Options.IDEDebugMode || runtimeCore.Options.RunMode == InterpreterMode.Expression)
            {
                rmem.FramePointer = (int)rmem.GetAtRelative(StackFrame.FrameIndexFramePointer).IntegerValue;
                rmem.PopFrame(StackFrame.StackFrameSize);

                if (bounceType == CallingConvention.BounceType.Explicit)
                {
                    // Restoring the registers require the current frame pointer of the stack frame 
                    ResumeRegistersFromStackExceptRX();

                    bounceType = TX.BounceType;
                }
            }

            if (type == StackFrameType.Function)
            {
                // Comment Jun: 
                // Consider moving this to a restore to function method

                // If this language block was called explicitly, only then do we need to restore the instruction stream
                if (bounceType == CallingConvention.BounceType.Explicit)
                {
                    // If we're returning from a block to a function, the instruction stream needs to be restored.
                    StackValue sv = rmem.GetAtRelative(StackFrame.FrameIndexTX);
                    Validity.Assert(sv.IsCallingConvention);
                    CallingConvention.CallType callType = sv.CallType;
                    if (CallingConvention.CallType.Explicit == callType)
                    {
                        int callerblock = rmem.GetAtRelative(StackFrame.FrameIndexFunctionBlockIndex).BlockIndex;
                        istream = exe.instrStreamList[callerblock];
                    }
                }
            }
            Properties = PopInterpreterProps();
            SetupGraphNodesInScope();   
        }

        private void RETCN_Handler(Instruction instruction)
        {
            if (runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                runtimeCore.RuntimeMemory.PopConstructBlockId();
            }

            StackValue op1 = instruction.op1;
            int blockId = op1.BlockIndex;

            CodeBlock codeBlock = exe.CompleteCodeBlocks[blockId];
            runtimeVerify(codeBlock.blockType == CodeBlockType.Construct);
            GCCodeBlock(blockId);
            pc++;
        }

        private List<bool> RetrieveExecutionStatesFromStack(int localSize, int paramSize)
        {
            // Retrieve the execution execution states 
            List<bool> execStateRestore = new List<bool>();
            int execstates = (int)rmem.GetAtRelative(StackFrame.FrameIndexExecutionStates).IntegerValue;
            if (execstates > 0)
            {
                int offset = StackFrame.StackFrameSize + localSize + paramSize;
                for (int n = 0; n < execstates; ++n)
                {
                    int relativeIndex = -offset - n - 1;
                    StackValue svState = rmem.GetAtRelative(relativeIndex);
                    Validity.Assert(svState.IsBoolean);
                    execStateRestore.Add(svState.BooleanValue);
                }
            }
            return execStateRestore;
        }

        private void RETURN_Handler()
        {
            runtimeVerify(rmem.ValidateStackFrame());

            if (runtimeCore.Options.ExecuteSSA)
            {
                if (runtimeCore.Options.GCTempVarsOnDebug && runtimeCore.Options.IDEDebugMode)
                {
                    // GC anonymous variables in the return stmt
                    if (null != Properties.executingGraphNode && !Properties.executingGraphNode.IsSSANode())
                    {
                        Properties.executingGraphNode.symbolListWithinExpression.Clear();
                    }
                }
            }

            RestoreFromCall();
        }

        private void JMP_Handler(Instruction instruction)
        {
            pc = instruction.op1.LabelIndex;
        }

        private void CJMP_Handler(Instruction instruction)
        {
            StackValue opdata1 = rmem.Pop();
            
            if (opdata1.IsDouble)
            {
                if (opdata1.DoubleValue.Equals(0))
                {
                    pc = instruction.op1.LabelIndex;
                }
                else
                {
                    pc += 1; 
                }
            }
            else
            {
                if (opdata1.IsPointer)
                {
                    pc += 1;
                }
                else if (0 == opdata1.RawData)
                {
                    pc = instruction.op1.LabelIndex;
                }
                else
                {
                    pc += 1;
                }
            }
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
            int exprID = (int)instruction.op1.IntegerValue;
            // The SSA assignment flag
            bool isSSA = instruction.op2.BooleanValue;

            // The current function and class scope
            int ci = Constants.kInvalidIndex;
            int fi = Constants.kGlobalScope;

            int classIndex = Constants.kInvalidIndex;
            int functionIndex = Constants.kGlobalScope;
            bool isInFunction = GetCurrentScope(out classIndex, out functionIndex);

            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);
                {
                    isInFunction = runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun);
                }
            }

            if (isInFunction)
            {
                ci = classIndex;
                fi = functionIndex;
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
                if (runtimeCore.Options.ExecuteSSA)
                {
                    if (runtimeCore.Options.GCTempVarsOnDebug && runtimeCore.Options.IDEDebugMode)
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

                            Properties.executingGraphNode.symbolListWithinExpression.Clear();
                        }
                    }
                }

                if (runtimeCore.Options.ExecuteSSA)
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
            UpdateGraph(exprID, isSSA);

            if (runtimeCore.Options.ApplyUpdate)
            {
                // Go to the first dirty pc
                SetupNextExecutableGraph(fi, ci);
            }
            else
            {
                // Go to the next pc
                pc++;
                Properties.executingGraphNode = GetNextGraphNodeToExecute(pc, ci, fi);
                if (Properties.executingGraphNode != null)
                {
                    Properties.executingGraphNode.isDirty = false;
                    pc = Properties.executingGraphNode.updateBlock.startpc;
                }
            }

            return;
        }

        /// <summary>
        /// Returns the next graphnode to execute given the current next pc and scope
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="ci"></param>
        /// <param name="fi"></param>
        private AssociativeGraph.GraphNode GetNextGraphNodeToExecute(int nextPC, int ci, int fi)
        {
            AssociativeGraph.GraphNode nextGraphNode = null;

            // Given the next pc, get the next graphnode to execute and mark it clean
            if (runtimeCore.Options.IsDeltaExecution)
            {
                if (IsGlobalScope())
                {
                    // At the global scope, no associative update occurs. Dirty nodes are accumulated and only executed on ApplyUpdate
                    // This behavior conforms to the Transaction Update design
                    // https://docs.google.com/a/adsk-oss.com/document/d/1v-eV16hzeBINKKY-F8sa6b0s_Q4Fafih1uOus8hYyD8

                    // On delta execution, it is possible that the next graphnode is clean
                    // Retrieve the next dirty graphnode given the pc
                    // Associative update is handled when ApplyUpdate = true
                    nextGraphNode = ProtoCore.AssociativeEngine.Utils.GetFirstDirtyGraphNodeFromPC(nextPC, graphNodesInProgramScope);
                }
                else
                {
                    // Allow immediate update if we are in a local scope.
                    nextGraphNode = ProtoCore.AssociativeEngine.Utils.GetFirstDirtyGraphNodeFromPC(Constants.kInvalidIndex, graphNodesInProgramScope);
                }
            }
            else
            {
                // On normal execution, just retrieve the graphnode associated with pc
                // Associative update is handled in jdep
                nextGraphNode = ProtoCore.AssociativeEngine.Utils.GetGraphNodeAtPC(nextPC, graphNodesInProgramScope);
            }
            return nextGraphNode;
        }

        private void JDEP_Handler(Instruction instruction)
        {
            // The current function and class scope
            int ci = DSASM.Constants.kInvalidIndex;
            int fi = DSASM.Constants.kGlobalScope;

            int classIndex = Constants.kInvalidIndex;
            int functionIndex = Constants.kGlobalScope;
            bool isInFunction = GetCurrentScope(out classIndex, out functionIndex);


            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                Validity.Assert(runtimeCore.DebugProps.DebugStackFrame.Count > 0);
                isInFunction = runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun);
            }

            if (isInFunction)
            {
                ci = classIndex;
                fi = functionIndex;
            }

            SetupNextExecutableGraph(fi, ci);
        }

        private void PUSHDEP_Handler(Instruction instruction)
        {
            // The symbol block
            int block = instruction.op1.BlockIndex;
            int depth = (int)instruction.op2.IntegerValue;
            // The symbol and its class index
            int classIndex = instruction.op3.ClassIndex;

            // Get the identifier list
            List<StackValue> symbolList = new List<StackValue>();
            for (int n = 0; n < depth; ++n)
            {
                // TODO Jun: use the proper ID for this
                StackValue sv = rmem.Pop();
                symbolList.Add(sv);
            }
            symbolList.Reverse();

            // TODO Jun: use the proper ID for this
            int symindex = (int)symbolList[0].IntegerValue;

            if (Constants.kInvalidIndex != symindex)
            {
                SymbolNode symnode;
                if (Constants.kInvalidIndex != classIndex)
                {
                    symnode = exe.classTable.ClassNodes[classIndex].Symbols.symbolList[symindex];
                }
                else
                {
                    symnode = exe.runtimeSymbols[block].symbolList[symindex];
                }

                AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                updateNode.symbol = symnode;
                updateNode.nodeType = AssociativeGraph.UpdateNodeType.Symbol;

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
                    symindex = (int)symbolList[n].IntegerValue;

                    // Get the symbol and append it to the modified ref
                    updateNode = new AssociativeGraph.UpdateNode();
                    updateNode.symbol = exe.classTable.ClassNodes[classIndex].Symbols.symbolList[symindex];
                    updateNode.nodeType = AssociativeGraph.UpdateNodeType.Symbol;

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

       

        private void SETEXPUID_Handler()
        {
            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                if (runtimeCore.DebugProps.RunMode == Runmode.StepNext)
                {
                    if (!runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsFunctionStepOver))
                    {
                        // if ec is at end of an expression in imperative lang block
                        // we force restore the breakpoints                     
                        runtimeCore.Breakpoints.Clear();
                        runtimeCore.Breakpoints.AddRange(runtimeCore.DebugProps.AllbreakPoints);
                    }
                }
            }

            pc++;
        }

        #endregion

        private void Exec(Instruction instruction)
        {
            if (rmem.Heap.IsWaitingForRoots)
            {
                var gcroots = CollectGCRoots();
                rmem.Heap.SetRoots(gcroots, this);
            }

            switch (instruction.opCode)
            {
                case OpCode.NEWOBJ:
                    {
                        ALLOCC_Handler(instruction);
                        return;
                    }

                case OpCode.CAST:
                    {
                        CAST_Handler(instruction);
                        return;
                    }

                case OpCode.LOADELEMENT:
                    {
                        LOADELEMENT_Handler(instruction);
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

                case OpCode.PUSHBLOCK:
                    {
                        PUSHB_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHM:
                    {
                        PUSHM_Handler(instruction);
                        return;
                    }

                case OpCode.PUSH_ARRAYKEY:
                    {
                        PUSH_VARSIZE_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHREPGUIDE:
                    {
                        PUSHREPGUIDE_Handler(instruction);
                        return;
                    }

                case OpCode.PUSHLEVEL:
                    {
                        PUSHLEVEL_Handler(instruction);
                        return;
                    }

                case OpCode.POPREPGUIDES:
                    {
                        POPREPGUIDES_Handler(instruction);
                        return;
                    }

                case OpCode.SETELEMENT:
                    {
                        SETELEMENT_Helper(instruction);
                        return;
                    }

                case OpCode.SETMEMElEMENT:
                    {
                        SETMEMELEMENT_Helper(instruction);
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

                case OpCode.POPM:
                    {
                        POPM_Handler(instruction);
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

                case OpCode.NEWARR:
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

                case OpCode.CALLR:
                    {
                        CALLR_Handler(instruction);
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

                case OpCode.JDEP:
                    {
                        JDEP_Handler(instruction);
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

                case OpCode.SETEXPUID:
                    {
                        SETEXPUID_Handler();
                        return;
                    }
                default: //Unknown OpCode
                    throw new NotImplementedException("Unknown Op code, NIE Marker: {D6028708-CD47-4D0B-97FC-E681BD65DB5C}");
            }
        }

        private void RecordExecutedGraphNode()
        {
            if (runtimeCore.Options.IsDeltaExecution && IsGlobalScope())
            {
                runtimeCore.RecordExtecutedGraphNode(Properties.executingGraphNode);
            }
        }

        public List<StackValue> CollectGCRoots()
        {
            var gcRoots = new List<StackValue>();
            if (RX.IsReferenceType)
                gcRoots.Add(RX);
            gcRoots.AddRange(runtimeCore.CallSiteGCRoots);
            gcRoots.AddRange(rmem.Stack.Where(s => s.IsReferenceType));
            return gcRoots;
        }
    }
}
