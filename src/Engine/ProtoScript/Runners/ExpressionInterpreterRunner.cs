using System;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;

namespace ProtoScript.Runners
{
    public class ExpressionInterpreterRunner
    {
        private ProtoCore.Core Core;
        private ProtoCore.RuntimeCore runtimeCore;
        private readonly ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        public ExpressionInterpreterRunner(ProtoCore.Core core, ProtoCore.RuntimeCore runtimeCore)
        {
            Core = core;
            this.runtimeCore = runtimeCore;
        }

        public bool Compile(string code, int currentBlockID, out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.Language = ProtoCore.Language.Associative;
                //globalBlock.language = ProtoCore.Language.kImperative;
                globalBlock.Code = code;

                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                context.SetExprInterpreterProperties(currentBlockID, runtimeCore.RuntimeMemory, runtimeCore.watchClassScope, runtimeCore.DebugProps);
                ProtoCore.Language id = globalBlock.Language;

                runtimeCore.ExprInterpreterExe.iStreamCanvas = new InstructionStream(globalBlock.Language, Core);

                // Save the global offset and restore after compilation
                int offsetRestore = Core.GlobOffset;
                Core.GlobOffset = runtimeCore.RuntimeMemory.Stack.Count;

                Core.Compilers[id].Compile(out blockId, null, globalBlock, context, EventSink);

                // Restore the global offset
                Core.GlobOffset = offsetRestore;

                Core.BuildStatus.ReportBuildResult();

                buildSucceeded = Core.BuildStatus.BuildSucceeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return buildSucceeded;
        }

        public ExecutionMirror Execute(string code)
        {
            bool ssastate = Core.Options.GenerateSSA;
            bool ssastateExec = Core.Options.ExecuteSSA;

            runtimeCore.Options.RunMode = ProtoCore.DSASM.InterpreterMode.Expression;

            runtimeCore.Options.GenerateSSA = false;
            runtimeCore.Options.ExecuteSSA = false;

            code = string.Format("{0} = {1};", Constants.kWatchResultVar, code);

            // TODO Jun: Move this initaliztion of the exe into a unified function
            //Core.ExprInterpreterExe = new Executable();

            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            runtimeCore.RuntimeMemory.AlignStackForExprInterpreter();

            //Initialize the watch stack and watchBaseOffset
            //The watchBaseOffset is used to indexing the watch variables and related temporary variables
            Core.watchBaseOffset = 0;
            runtimeCore.watchStack.Clear();

            bool succeeded = Compile(code, runtimeCore.GetCurrentBlockId(), out blockId);

            //Clear the warnings and errors so they will not continue impact the next compilation.
            Core.BuildStatus.ClearErrors();
            Core.BuildStatus.ClearWarnings();

            for (int i = 0; i < Core.watchBaseOffset; ++i )
                runtimeCore.watchStack.Add(StackValue.Null);

            //Record the old function call depth
            //Fix IDE-523: part of error for watching non-existing member
            int oldFunctionCallDepth = runtimeCore.FunctionCallDepth;

            //Record the old start PC
            int oldStartPC = Core.watchStartPC;
            if (succeeded)
            {

                //a2. Record the old start PC for restore instructions
                Core.watchStartPC = runtimeCore.ExprInterpreterExe.instrStreamList[blockId].instrList.Count;
                Core.GenerateExprExeInstructions(blockId);
                
                //a3. Record the old running block
                int restoreBlock = runtimeCore.RunningBlock;
                runtimeCore.RunningBlock = blockId;

                //a4. Record the old debug entry PC and stack size of FileFepChosen
                int oldDebugEntryPC = runtimeCore.DebugProps.DebugEntryPC;

                //a5. Record the frame pointer for referencing to thisPtr
                runtimeCore.watchFramePointer = runtimeCore.RuntimeMemory.FramePointer;

                // The "Core.Bounce" below is gonna adjust the "FramePointer" 
                // based on the current size of "Core.Rmem.Stack". All that is 
                // good except that "Bounce" does not restore the previous value 
                // of frame pointer after "bouncing back". Here we make a backup
                // of it and restore it right after the "Core.Bounce" call.
                // 
                //Core.Executives[Core.CodeBlockList[Core.RunningBlock].language].
                try
                {
                    ProtoCore.DSASM.StackFrame stackFrame = null;
                    int locals = 0;

                    runtimeCore.CurrentExecutive.CurrentDSASMExec.Bounce(blockId, Core.watchStartPC, stackFrame, locals);

                    // As Core.InterpreterProps stack member is pushed to every time the Expression Interpreter begins executing
                    // it needs to be popped off at the end for stack alignment - pratapa
                    runtimeCore.InterpreterProps.Pop();
                }
                catch
                { }

                //r5. Restore frame pointer.
                runtimeCore.RuntimeMemory.FramePointer = runtimeCore.watchFramePointer; 

                //r4. Restore the debug entry PC and stack size of FileFepChosen
                runtimeCore.DebugProps.DebugEntryPC = oldDebugEntryPC;

                //r3. Restore the running block 
                runtimeCore.RunningBlock = restoreBlock;

                //r2. Restore the instructions in Core.ExprInterpreterExe
                int from = Core.watchStartPC;
                int elems = runtimeCore.ExprInterpreterExe.iStreamCanvas.instrList.Count;
                runtimeCore.ExprInterpreterExe.instrStreamList[blockId].instrList.RemoveRange(from, elems);

                //Restore the start PC
                Core.watchStartPC = oldStartPC;

                //Restore the function call depth
                //Fix IDE-523: part of error for watching non-existing member
                runtimeCore.FunctionCallDepth = oldFunctionCallDepth;


                //Clear the watchSymbolList
                foreach (SymbolNode node in runtimeCore.WatchSymbolList)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex == node.classScope)
                        Core.DSExecutable.runtimeSymbols[node.runtimeTableIndex].Remove(node);
                    else
                        Core.ClassTable.ClassNodes[node.classScope].Symbols.Remove(node);
                }
            }
            else
            {
                //Restore the start PC
                Core.watchStartPC = oldStartPC;

                //Restore the function call depth
                //Fix IDE-523: part of error for watching non-existing member
                runtimeCore.FunctionCallDepth = oldFunctionCallDepth;

                //Clear the watchSymbolList
                foreach (SymbolNode node in runtimeCore.WatchSymbolList)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex == node.classScope)
                        Core.DSExecutable.runtimeSymbols[node.runtimeTableIndex].Remove(node);
                    else
                        Core.ClassTable.ClassNodes[node.classScope].Symbols.Remove(node);
                }

                // TODO: investigate why additional elements are added to the stack.
                runtimeCore.RuntimeMemory.RestoreStackForExprInterpreter();

                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            // TODO: investigate why additional elements are added to the stack.
            runtimeCore.RuntimeMemory.RestoreStackForExprInterpreter();

            runtimeCore.Options.GenerateSSA = ssastate;
            runtimeCore.Options.ExecuteSSA = ssastateExec;
            runtimeCore.Options.RunMode = ProtoCore.DSASM.InterpreterMode.Normal;

            return new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
        }
    }
}
