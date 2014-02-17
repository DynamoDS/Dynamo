using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;

namespace ProtoScript.Runners
{
    public class ExpressionInterpreterRunner
    {
        private ProtoCore.Core Core;
        private readonly ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        public ExpressionInterpreterRunner(ProtoCore.Core core)
        {
            Core = core;
            core.ExecMode = ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter;
        }

        public bool Compile(string code, out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                //globalBlock.language = ProtoCore.Language.kImperative;
                globalBlock.body = code;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.language;

                Core.ExprInterpreterExe.iStreamCanvas = new InstructionStream(globalBlock.language, Core);

                // Save the global offset and restore after compilation
                int offsetRestore = Core.GlobOffset;
                Core.GlobOffset = Core.Rmem.Stack.Count;
                
                Core.Executives[id].Compile(out blockId, null, globalBlock, context, EventSink);

                // Restore the global offset
                Core.GlobOffset = offsetRestore;

                Core.BuildStatus.ReportBuildResult();

                int errors = 0;
                int warnings = 0;
                buildSucceeded = Core.BuildStatus.GetBuildResult(out errors, out warnings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return buildSucceeded;
        }

        public ExecutionMirror Execute(string code)
        {
            bool ssastate = Core.Options.FullSSA;
            Core.Options.FullSSA = false;
            code = string.Format("{0} = {1};", Constants.kWatchResultVar, code);

            // TODO Jun: Move this initaliztion of the exe into a unified function
            //Core.ExprInterpreterExe = new Executable();

            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            Core.Rmem.AlignStackForExprInterpreter();

            //Initialize the watch stack and watchBaseOffset
            //The watchBaseOffset is used to indexing the watch variables and related temporary variables
            Core.watchBaseOffset = 0;
            Core.watchStack.Clear();

            bool succeeded = Compile(code, out blockId);

            //Clear the warnings and errors so they will not continue impact the next compilation.
            //Fix IDE-662
            Core.BuildStatus.Errors.Clear();
            Core.BuildStatus.Warnings.Clear();

            for (int i = 0; i < Core.watchBaseOffset; ++i )
                Core.watchStack.Add(StackValue.Null);

            //Record the old function call depth
            //Fix IDE-523: part of error for watching non-existing member
            int oldFunctionCallDepth = Core.FunctionCallDepth;

            //Record the old start PC
            int oldStartPC = Core.startPC;
            if (succeeded)
            {

                //a2. Record the old start PC for restore instructions
                Core.startPC = Core.ExprInterpreterExe.instrStreamList[blockId].instrList.Count;
                Core.GenerateExprExeInstructions(blockId);
                
                //a3. Record the old running block
                int restoreBlock = Core.RunningBlock;
                Core.RunningBlock = blockId;

                //a4. Record the old debug entry PC and stack size of FileFepChosen
                int oldDebugEntryPC = Core.DebugProps.DebugEntryPC;

                //a5. Record the frame pointer for referencing to thisPtr
                Core.watchFramePointer = Core.Rmem.FramePointer;

                // The "Core.Bounce" below is gonna adjust the "FramePointer" 
                // based on the current size of "Core.Rmem.Stack". All that is 
                // good except that "Bounce" does not restore the previous value 
                // of frame pointer after "bouncing back". Here we make a backup
                // of it and restore it right after the "Core.Bounce" call.
                // 
                //Core.Executives[Core.CodeBlockList[Core.RunningBlock].language].
                ProtoCore.Runtime.Context context = new ProtoCore.Runtime.Context();

                try
                {
                    ProtoCore.DSASM.StackFrame stackFrame = null;
                    int locals = 0;

                    StackValue sv = Core.Bounce(blockId, Core.startPC, context, stackFrame, locals, EventSink);

                    // As Core.InterpreterProps stack member is pushed to every time the Expression Interpreter begins executing
                    // it needs to be popped off at the end for stack alignment - pratapa
                    Core.InterpreterProps.Pop();
                }
                catch
                { }

                //r5. Restore frame pointer.
                Core.Rmem.FramePointer = Core.watchFramePointer; 

                //r4. Restore the debug entry PC and stack size of FileFepChosen
                Core.DebugProps.DebugEntryPC = oldDebugEntryPC;

                //r3. Restore the running block 
                Core.RunningBlock = restoreBlock;

                //r2. Restore the instructions in Core.ExprInterpreterExe
                int from = Core.startPC;
                int elems = Core.ExprInterpreterExe.iStreamCanvas.instrList.Count;
                Core.ExprInterpreterExe.instrStreamList[blockId].instrList.RemoveRange(from, elems);

                //Restore the start PC
                Core.startPC = oldStartPC;

                //Restore the function call depth
                //Fix IDE-523: part of error for watching non-existing member
                Core.FunctionCallDepth = oldFunctionCallDepth;


                //Clear the watchSymbolList
                foreach (SymbolNode node in Core.watchSymbolList)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex == node.classScope)
                        Core.DSExecutable.runtimeSymbols[node.runtimeTableIndex].Remove(node);
                    else
                        Core.ClassTable.ClassNodes[node.classScope].symbols.Remove(node);
                }
            }
            else
            {
                //Restore the start PC
                Core.startPC = oldStartPC;

                //Restore the function call depth
                //Fix IDE-523: part of error for watching non-existing member
                Core.FunctionCallDepth = oldFunctionCallDepth;

                //Clear the watchSymbolList
                foreach (SymbolNode node in Core.watchSymbolList)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex == node.classScope)
                        Core.DSExecutable.runtimeSymbols[node.runtimeTableIndex].Remove(node);
                    else
                        Core.ClassTable.ClassNodes[node.classScope].symbols.Remove(node);
                }

                // TODO: investigate why additional elements are added to the stack.
                Core.Rmem.RestoreStackForExprInterpreter();

                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            // TODO: investigate why additional elements are added to the stack.
            Core.Rmem.RestoreStackForExprInterpreter();

            Core.Options.FullSSA = ssastate;

            return new ExecutionMirror(Core.CurrentExecutive.CurrentDSASMExec, Core);
        }
    }
}
