using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoCore.Utils;

namespace ProtoScript.Runners
{
    /// <summary>
    /// Debug runner is intended for debug IDE etc. use, it starts a virtual machine in debug mode
    /// These calls are blocking, but maintain a seperate VM Thread intfrastructure
    /// </summary>
    public class DebugRunner
    {
        private bool inited;
        private bool executionsuspended;
        private VMState lastState;
        private ProtoCore.Core core;
        public ProtoCore.RuntimeCore runtimeCore;
        private String code;
        private List<Dictionary<DebugInfo, Instruction>> diList;
        private readonly List<Instruction> allbreakPoints = new List<Instruction>();
        public bool isEnded { get; set; }

        public VMState LastState
        {
            get 
            {
               return lastState; 
            }
            private set 
            {
               lastState = value; 
            }
        }

        public Instruction CurrentInstruction { get; private set; }  

        int resumeBlockID;
        public DebugRunner(ProtoCore.Core core)
        {
            this.core = core;
            this.core.Options.IDEDebugMode = true;
            RegisteredBreakpoints = new List<Breakpoint>();
            executionsuspended = false;
        }

        private ProtoCore.RuntimeCore CreateRuntimeCore(ProtoCore.Core core)
        {
            ProtoCore.RuntimeCore runtimeCore = new ProtoCore.RuntimeCore(core.Heap);
            runtimeCore.SetupForExecution(core, core.GlobOffset);
            return runtimeCore;
        }

        /// <summary>
        /// Setup to run with customised launch options
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Ready to run?</returns>
        /// 
        private bool _PreStart(string code, Config.RunConfiguration configuration, string fileName)
        {
            this.code = code;
            if (null == core)
            {
                core = new ProtoCore.Core(new ProtoCore.Options { IDEDebugMode = true });
                core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
                core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));

            }


            if (null != fileName)
            {
                core.CurrentDSFileName = Path.GetFullPath(fileName);
                core.Options.RootModulePathName = Path.GetFullPath(fileName);
            }

            //Run the compilation process
            if (Compile(out resumeBlockID))
            {
                inited = true;
                runtimeCore = CreateRuntimeCore(core);

                FirstExec();
                diList = BuildReverseIndex();
                return true;
            }
            else
            {
                inited = false;
                return false;
            }
        }
        
        public bool PreStart(string src, Config.RunConfiguration configuration = new Config.RunConfiguration())
        {
            return _PreStart(src, configuration, null);
        }
        public bool LoadAndPreStart(string src, Config.RunConfiguration configuration = new Config.RunConfiguration())
        {
            return _PreStart(File.ReadAllText(src), configuration, src);
        }

        /// <summary>
        /// Setup to run with the default options
        /// </summary>

        /// <summary>
        /// Perform one execution step on the VM
        /// </summary>
        /// <returns></returns>
        /// 


        public VMState Step()
        {
            DebuggerStateCheckBeforeRun();

            runtimeCore.DebugProps.RunMode = ProtoCore.Runmode.StepIn;
            runtimeCore.DebugProps.AllbreakPoints = allbreakPoints;
            lastState =  RunVM(allbreakPoints);
            return lastState;
        }

        public VMState StepOver()
        {
            DebuggerStateCheckBeforeRun();
            // check if the current instruction is a function call instruction 
            // if it is, set a breakpoint at the next instruction and call Run
            // if not, call Step
            VMState vms = null;

            runtimeCore.DebugProps.AllbreakPoints = allbreakPoints;
            Instruction instr = GetCurrentInstruction();
            if (instr.opCode == OpCode.CALL ||
                instr.opCode == OpCode.CALLR)
            {
                runtimeCore.DebugProps.RunMode = ProtoCore.Runmode.StepNext;
                List<Instruction> instructions = new List<Instruction>();
                foreach (Breakpoint bp in RegisteredBreakpoints)
                {
                    instructions.Add(BreakpointToInstruction(bp));
                }
                vms = RunVM(instructions);
            }
            else
            {
                vms = Step();
            }

            return vms;
        }

        public VMState StepOut()
        {
            VMState vms = null;

            DebuggerStateCheckBeforeRun();

            if(runtimeCore.DebugProps.DebugStackFrameContains(ProtoCore.DebugProperties.StackFrameFlagOptions.FepRun))
            {
                runtimeCore.DebugProps.RunMode = ProtoCore.Runmode.StepOut;
                runtimeCore.DebugProps.AllbreakPoints = allbreakPoints;

                runtimeCore.DebugProps.StepOutReturnPC = (int)runtimeCore.RuntimeMemory.GetAtRelative(StackFrame.kFrameIndexReturnAddress).opdata;

                List<Instruction> instructions = new List<Instruction>();
                foreach (Breakpoint bp in RegisteredBreakpoints)
                {
                    instructions.Add(BreakpointToInstruction(bp));
                }
                vms = RunVM(instructions);
            }
            else
            {
                vms = Run();
            }

            return vms;
        }

        public VMState Run()
        {
            DebuggerStateCheckBeforeRun();

            runtimeCore.DebugProps.RunMode = ProtoCore.Runmode.RunTo;
            runtimeCore.DebugProps.AllbreakPoints = allbreakPoints;

            List<Instruction> instructions = new List<Instruction>();
            foreach (Breakpoint bp in RegisteredBreakpoints)
            {
                instructions.Add(BreakpointToInstruction(bp));
            }
            return RunVM(instructions);
        }

        private void DebuggerStateCheckBeforeRun()
        {
            if (lastState != null)
                lastState.Invalidate();
            if (!inited)
                throw new RunnerNotInitied();
            if (isEnded)
                throw new EndofScriptException();
        }
        private VMState RunVM(List<Instruction> breakPoints)
        {
            //Get the next available location and set a break point
            //Unset the break point at the current location
            Instruction currentInstr = null; // will be instantialized when a proper breakpoint is reached
            VMState vms = null;
            try
            {
                if (executionsuspended)
                    runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionResume);

                Execute(runtimeCore.DebugProps.DebugEntryPC, breakPoints);
                isEnded = true; // the script has ended smoothly, 
            }
            catch (ProtoCore.Exceptions.DebugHalting)
            {
                if (runtimeCore.CurrentExecutive == null) //This was before the VM was properly started
                    return null;
                currentInstr = GetCurrentInstruction(); // set the current instruction to the current breakpoint instruction
                runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionBreak);
                executionsuspended = true;
            }
            catch (ProtoCore.Exceptions.EndOfScript)
            {
                isEnded = true;
            }
            finally
            {
                ExecutionMirror execMirror = new ProtoCore.DSASM.Mirror.ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
                vms = new VMState(execMirror, core);
                vms.isEnded = isEnded;
                ProtoCore.CodeModel.CodePoint start = new ProtoCore.CodeModel.CodePoint();
                ProtoCore.CodeModel.CodePoint end = new ProtoCore.CodeModel.CodePoint();

                if (currentInstr != null) // equal to null means that the whole script has ended, reset the cursor
                {
                    start = InstructionToBeginCodePoint(currentInstr);
                    end = InstructionToEndCodePoint(currentInstr);
                }
                CurrentInstruction = currentInstr;

                vms.ExecutionCursor = new ProtoCore.CodeModel.CodeRange { StartInclusive = start, EndExclusive = end };
            }

            return vms;

        }
        public void Shutdown()
        {
            if (lastState != null)
                lastState.Invalidate();

            try
            {
                //core.heap.Free();
            }
            catch (Exception)
            { }

            //Drop the VM state objects so they can be GCed
            runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionEnd);
            lastState = null;
            core = null;

        }

        private Instruction GetCurrentInstruction()
        {
            return core.DSExecutable.instrStreamList[runtimeCore.RunningBlock].instrList[runtimeCore.DebugProps.DebugEntryPC];
        }

        private ProtoCore.CodeModel.CodePoint InstructionToBeginCodePoint(Instruction instr)
        {
            if (instr.debug == null)
                throw new InvalidOperationException("This instuction has no source representation");

            return instr.debug.Location.StartInclusive;
        }
        private ProtoCore.CodeModel.CodePoint InstructionToEndCodePoint(Instruction instr)
        {
            if (instr.debug == null)
                throw new InvalidOperationException("This instuction has no source representation");

            return instr.debug.Location.EndExclusive;
        }

        /* private Instruction BreakpointToInstruction(Breakpoint bp)
        {
            //@PERF
            //Compute the distance between this location and all the registered available codepoints

            int lineNo = bp.Location.LineNo;
            int closestDistance = int.MaxValue;
            Dictionary<DebugInfo, Instruction> closest = diList[0];

            //First find the closest list to this
            foreach (Dictionary<DebugInfo, Instruction> lineDis in diList)
            {
                int diLine = lineDis.Values.First().debug.line;
                if (Math.Abs(lineNo - diLine) < closestDistance)
                {
                    closestDistance = Math.Abs(lineNo - diLine);
                    closest = lineDis;
                }
            }

            //@TODO(luke)
            //For now return the first item on the line, this won't be correct in general
            return closest.Values.First();

        } */

        /// <summary>
        /// This method runs until the next breakpoint is reached
        /// </summary>
        

        

        /// <summary>
        /// Terminate the program and reclaim the resources
        /// </summary>
        

        #region Breakpoints

        //Handle registration of breakpoints
        //For now do the simple add/remove, may need to do more than this

        //public Breakpoint GetBreakpointAtLine(int line)
        //{
        //    foreach (Breakpoint b in RegisteredBreakpoints)
        //    {
        //        if (b.Location.LineNo == line)
        //        {
        //            return b;
        //        }
        //    }
        //    return null;
        //}

        public List<Breakpoint> RegisteredBreakpoints
        {
            get;
            private set;
        }
        public void RegisterBreakpoint(Breakpoint bp)
        {
            RegisteredBreakpoints.Add(bp);
        }
        public void UnRegisterBreakpoint(Breakpoint bp)
        {
            RegisteredBreakpoints.Remove(bp);
        }
        public bool ToggleBreakpoint(ProtoCore.CodeModel.CodePoint cp)
        {
            Breakpoint bp = BuildBreakPointFromCodePoint(cp);

            if (bp == null)
                return false;

            if (RegisteredBreakpoints.Count(x => x.Location == bp.Location) != 0)
                RegisteredBreakpoints.Remove(bp);
            else
                RegisteredBreakpoints.Add(bp);

            return true;
        }

        private Breakpoint BuildBreakPointFromCodePoint(ProtoCore.CodeModel.CodePoint codePoint)
        {
            if (codePoint.SourceLocation == null)
                codePoint.SourceLocation = new ProtoCore.CodeModel.CodeFile();
            // get the instructions with debug info in at cursor position and in the same file
            IEnumerable<Instruction> sameLine = allbreakPoints.Where(x => x.debug.Location.StartInclusive.SourceLocation == codePoint.SourceLocation &&
                x.debug.Location.EndExclusive.SourceLocation == codePoint.SourceLocation &&
                x.debug.Location.StartInclusive.LineNo == codePoint.LineNo);
            if (sameLine.Count() == 0)
                return null;

            IEnumerable<Instruction> sameCol = sameLine.Where(x => x.debug.Location.StartInclusive.CharNo <= codePoint.CharNo && x.debug.Location.EndExclusive.CharNo >= codePoint.CharNo);
            Instruction instr = null;
            if (sameCol.Count() != 0)
            {
                instr = sameCol.ElementAt(0);
                foreach (Instruction i in sameCol)
                {
                    if (i.debug.Location.EndExclusive.CharNo - i.debug.Location.StartInclusive.CharNo < instr.debug.Location.EndExclusive.CharNo - instr.debug.Location.StartInclusive.CharNo)
                        instr = i;
                }
            }
            else
            {
                instr = sameLine.ElementAt(0);
                foreach (Instruction i in sameLine)
                {
                    if (i.debug.Location.EndExclusive.CharNo - i.debug.Location.StartInclusive.CharNo > instr.debug.Location.EndExclusive.CharNo - instr.debug.Location.StartInclusive.CharNo)
                        instr = i;
                }
            }

            return new Breakpoint(instr.debug);
        }
        private Instruction BreakpointToInstruction(Breakpoint bp)
        {
            return allbreakPoints.Where(x => x.debug.Location == bp.Location).ElementAt(0);
        }
        /// <summary>
        /// A list of the current known breakpoints
        /// Interact with through register and unregister methods
        /// </summary>
        

        #endregion

        private readonly ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        private bool Compile(out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                globalBlock.body = code;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;

                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.language;
                core.Compilers[id].Compile(out blockId, null, globalBlock, context);

                core.BuildStatus.ReportBuildResult();

                buildSucceeded = core.BuildStatus.BuildSucceeded;
                core.GenerateExecutable();

            }
            catch (Exception ex)
            {
                Messages.FatalCompileError fce = new Messages.FatalCompileError { Message = ex.ToString() };

                Console.WriteLine(fce.Message);
                return false;
            }

            return buildSucceeded;
        }

        /// <summary>
        /// First exec starts up the VM and breaks at the first instruction
        /// </summary>
        private void FirstExec()
        {
            List<Instruction> bps = new List<Instruction>();
            runtimeCore.DebugProps.DebugEntryPC = core.DSExecutable.instrStreamList[0].entrypoint;

            foreach (InstructionStream instrStream in core.DSExecutable.instrStreamList)
            {
                //Register the first initial breakpoint
                if (null != instrStream)
                {
                    for (int i = 0; i < instrStream.instrList.Count; i++)
                    {
                        if (instrStream.instrList[i].debug != null)
                        {
                            bps.Add(instrStream.instrList[i]);
                            break;
                        }
                    }
                }
            }
            try
            {
                // Jun Comment: Do not pre execute, wait for the next click
                //Execute(entryPoint, bps);
            }
            catch (ProtoCore.Exceptions.DebugHalting)
            { }

        }

        /// <summary>
        /// Walk over the registered Debug points 
        /// </summary>
        private List<Dictionary<DebugInfo, Instruction>> BuildReverseIndex()
        {
            //List of Lines -> List of Debug Infos
            List<Dictionary<DebugInfo, Instruction>> ret = new List<Dictionary<DebugInfo, Instruction>>();

            foreach (InstructionStream instrStream in core.DSExecutable.instrStreamList)
            {
                if (null != instrStream)
                {
                    int lastLineMarker = -1;
                    Dictionary<DebugInfo, Instruction> lastDiList = null;
                    //Register the first initial breakpoint
                    for (int i = 0; i < instrStream.instrList.Count; i++)
                    {
                        DebugInfo di = instrStream.instrList[i].debug;
                        if (di != null)
                        {
                            if (instrStream.instrList[i].opCode != OpCode.BOUNCE)
                                allbreakPoints.Add(instrStream.instrList[i]);

                            if (di.Location.StartInclusive.LineNo != lastLineMarker)
                            {
                                Validity.Assert(di.Location.StartInclusive.LineNo > lastLineMarker);
                                lastDiList = new Dictionary<DebugInfo, Instruction>();
                                lastDiList.Add(di, instrStream.instrList[i]);
                                ret.Add(lastDiList);
                            }
                            else
                                lastDiList.Add(di, instrStream.instrList[i]);

                        }
                    }
                }
            }

            return ret;
        }

        private ExecutionMirror Execute(int programCounterToExecuteFrom, List<Instruction> breakpoints, bool fepRun = false)
        {
            runtimeCore.Breakpoints = breakpoints;
            resumeBlockID = runtimeCore.RunningBlock;


            if (runtimeCore.DebugProps.FirstStackFrame != null)
            {
                runtimeCore.DebugProps.FirstStackFrame.FramePointer = core.GlobOffset;

                // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                // Register TX is used for this.
                StackValue svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);
                runtimeCore.DebugProps.FirstStackFrame.TX = svCallConvention;
            }

            // Initialize the entry point interpreter
            int locals = 0; // This is the global scope, there are no locals
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(runtimeCore);
            runtimeCore.CurrentExecutive.CurrentDSASMExec = interpreter.runtime;
            runtimeCore.CurrentExecutive.CurrentDSASMExec.Bounce(
                resumeBlockID, 
                programCounterToExecuteFrom,
                runtimeCore.DebugProps.FirstStackFrame, 
                locals, 
                fepRun,
                null,
                breakpoints);

            return new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);

        }
        /// <summary>
        /// This class supports introgation of the VM state
        /// </summary>
        public class VMState
        {
            public ExecutionMirror mirror { get; private set; }
            private ProtoCore.Core core;
            public bool isEnded { get; set; }
            public ProtoCore.CodeModel.CodeRange ExecutionCursor { get; set; }

            public VMState(ExecutionMirror mirror, ProtoCore.Core core, int fi = -1)
            {
                this.mirror = mirror;
                this.core = core;
            }

            /// <summary>
            /// Have we been through a state transition?
            /// </summary>
            public bool IsInvalid { get; private set; }

            public void Invalidate()
            {
                IsInvalid = true;
            }

            public List<Obj> DumpScope()
            {


                throw new NotImplementedException();
            }

            public Obj ResolveName(String name)
            {

                return mirror.GetValue(name);
            }



        }

        public class Breakpoint
        {
            // public ProtoCore.CodeModel.CodePoint Location { get; private set; }
            //public Breakpoint(ProtoCore.CodeModel.CodePoint location)
            //{
            //    this.Location = location;
            //}

            public ProtoCore.CodeModel.CodeRange Location { get; set; }

            public Breakpoint(DebugInfo di, string sourceLocation = null)
            {
                Location = di.Location;
            }
            public override bool Equals(object obj)
            {
                if (obj is Breakpoint)
                {
                    Breakpoint bp = obj as Breakpoint;
                    if (Location.Equals(bp.Location))
                        return true;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Location == null ? 10589 : Location.GetHashCode();
            }
        }

        public class RunnerNotInitied : Exception
        { }

        public class EndofScriptException : Exception
        { }
    }
}
