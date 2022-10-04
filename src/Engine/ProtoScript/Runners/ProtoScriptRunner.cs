using System;
using System.Collections.Generic;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;

namespace ProtoScript.Runners
{

    public class ProtoScriptRunner
    {
        public ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        private bool Compile(string code, ProtoCore.Core core, ProtoCore.CompileTime.Context context)
        {
            bool buildSucceeded = false;
            try
            {
                // No More HashAngleReplace for unified parser (Fuqiang)
                //String strSource = ProtoCore.Utils.LexerUtils.HashAngleReplace(code);    

                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.Language = ProtoCore.Language.Associative;
                globalBlock.Code = code;

                //passing the global Assoc wrapper block to the compiler
                ProtoCore.Language id = globalBlock.Language;
                int blockId = Constants.kInvalidIndex;
                core.Compilers[id].Compile(out blockId, null, globalBlock, context, EventSink);

                core.BuildStatus.ReportBuildResult();
                buildSucceeded = core.BuildStatus.BuildSucceeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return buildSucceeded;
        }


        private bool Compile(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, ProtoCore.Core core, ProtoCore.CompileTime.Context context)
        {
            bool buildSucceeded = false;
            if (astList.Count <= 0)
            {
                // Nothing to compile
                buildSucceeded = true;
            }
            else
            {
                try
                {
                    //defining the global Assoc block that wraps the entire .ds source file
                    ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                    globalBlock.Language = ProtoCore.Language.Associative;
                    globalBlock.Code = string.Empty;

                    //passing the global Assoc wrapper block to the compiler
                    context.SetData(string.Empty, new Dictionary<string, object>(), null);
                    ProtoCore.Language id = globalBlock.Language;


                    ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
                    codeblock.Body.AddRange(astList);

                    int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                    core.Compilers[id].Compile(out blockId, null, globalBlock, context, EventSink, codeblock);

                    core.BuildStatus.ReportBuildResult();

                    buildSucceeded = core.BuildStatus.BuildSucceeded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return buildSucceeded;
        }

        private ProtoCore.RuntimeCore CreateRuntimeCore(ProtoCore.Core core)
        {
            ProtoCore.RuntimeCore runtimeCore = new ProtoCore.RuntimeCore(core.Heap);
            runtimeCore.SetupForExecution(core, core.GlobOffset);
            return runtimeCore;
        }

        /// <summary>
        /// Execute the data stored in core
        /// This is the entry point of all DS code to be executed
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public ProtoCore.RuntimeCore ExecuteVM(ProtoCore.Core core)
        {
            ProtoCore.RuntimeCore runtimeCore = CreateRuntimeCore(core);  
            runtimeCore.StartTimer();
            try
            {
                foreach (ProtoCore.DSASM.CodeBlock codeblock in core.CodeBlockList)
                {
                    // Comment Jun:
                    // On first bounce, the stackframe depth is initialized to -1 in the Stackfame constructor.
                    // Passing it to bounce() increments it so the first depth is always 0
                    ProtoCore.DSASM.StackFrame stackFrame = new ProtoCore.DSASM.StackFrame(core.GlobOffset);
                    stackFrame.FramePointer = runtimeCore.RuntimeMemory.FramePointer;

                    // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                    // Register TX is used for this.
                    StackValue svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.Implicit);
                    stackFrame.TX = svCallConvention;

                    // Initialize the entry point interpreter
                    int locals = 0; // This is the global scope, there are no locals
                    ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(runtimeCore);
                    runtimeCore.CurrentExecutive.CurrentDSASMExec = interpreter.runtime;
                    runtimeCore.CurrentExecutive.CurrentDSASMExec.Bounce(codeblock.codeBlockId, codeblock.instrStream.entrypoint, stackFrame, locals);
                }
                runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.ExecutionEnd);
            }
            catch
            {
                runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.ExecutionEnd);
                throw;
            }
            return runtimeCore;
        }

        /// <summary>
        /// ExecuteLive is called by the liverunner where a persistent RuntimeCore is provided
        /// ExecuteLive assumes only a single global scope
        /// </summary>
        /// <param name="core"></param>
        /// <param name="runtimeCore"></param>
        /// <param name="runningBlock"></param>
        /// <param name="staticContext"></param>
        /// <param name="runtimeContext"></param>
        /// <returns></returns>
        public ProtoCore.RuntimeCore ExecuteLive(ProtoCore.Core core, ProtoCore.RuntimeCore runtimeCore)
        {
            try
            {
                Executable exe = runtimeCore.DSExecutable;
                Validity.Assert(exe.CodeBlocks.Count == 1);
                CodeBlock codeBlock = runtimeCore.DSExecutable.CodeBlocks[0];
                int codeBlockID = codeBlock.codeBlockId;

                // Comment Jun:
                // On first bounce, the stackframe depth is initialized to -1 in the Stackfame constructor.
                // Passing it to bounce() increments it so the first depth is always 0
                ProtoCore.DSASM.StackFrame stackFrame = new ProtoCore.DSASM.StackFrame(core.GlobOffset);
                stackFrame.FramePointer = runtimeCore.RuntimeMemory.FramePointer;

                // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                // Register TX is used for this.
                StackValue svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.Implicit);
                stackFrame.TX = svCallConvention;

                // Initialize the entry point interpreter
                int locals = 0; // This is the global scope, there are no locals
                if (runtimeCore.CurrentExecutive.CurrentDSASMExec == null)
                {
                    ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(runtimeCore);
                    runtimeCore.CurrentExecutive.CurrentDSASMExec = interpreter.runtime;
                }

                runtimeCore.CurrentExecutive.CurrentDSASMExec.BounceUsingExecutive(
                    runtimeCore.CurrentExecutive.CurrentDSASMExec,
                    codeBlock.codeBlockId,
                    runtimeCore.StartPC,
                    stackFrame,
                    locals);

                runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.ExecutionEnd);
            }
            catch
            {
                runtimeCore.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.ExecutionEnd);
                throw;
            }
            return runtimeCore;
        }


        /// <summary>
        /// Compile and execute the source that is stored in the static context
        /// </summary>
        /// <param name="staticContext"></param>
        /// <param name="runtimeContext"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ExecutionMirror Execute(
            ProtoCore.CompileTime.Context staticContext,
            ProtoCore.Core core,
            out ProtoCore.RuntimeCore runtimeCoreOut,
            bool isTest = true)
        {
            Validity.Assert(null != staticContext.SourceCode && String.Empty != staticContext.SourceCode);
            ProtoCore.RuntimeCore runtimeCore = null;

            core.AddContextData(staticContext.GlobalVarList);

            string code = staticContext.SourceCode;
            bool succeeded = CompileAndGenerateExe(code, core, staticContext);
            if (succeeded)
            {
                runtimeCore = ExecuteVM(core);
                if (!isTest)
                {
                    runtimeCore.RuntimeMemory.Heap.Free();
                }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }
            runtimeCoreOut = runtimeCore;

            if (isTest)
            {
                return new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
            }

            return null;
        }

        /// <summary>
        /// Compile and execute the given list of ASTs
        /// </summary>
        /// <param name="astList"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ProtoCore.RuntimeCore Execute(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, ProtoCore.Core core, bool isTest = true)
        {
            ProtoCore.RuntimeCore runtimeCore = null;
            bool succeeded = CompileAndGenerateExe(astList, core, new ProtoCore.CompileTime.Context());
            if (succeeded)
            {
                runtimeCore = ExecuteVM(core);
                if (!isTest) 
                {
                    runtimeCore.RuntimeMemory.Heap.Free();
                }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            if (isTest)
            {
                runtimeCore.Mirror = new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
            }
            return runtimeCore;
        }


        /// <summary>
        /// Compile and execute the given sourcecode
        /// </summary>
        /// <param name="code"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ProtoCore.RuntimeCore Execute(string sourcecode, ProtoCore.Core core, bool isTest = true)
        {
            ProtoCore.RuntimeCore runtimeCore = null;
            bool succeeded = CompileAndGenerateExe(sourcecode, core, new ProtoCore.CompileTime.Context());
            if (succeeded)
            {
                try
                {
                    runtimeCore = ExecuteVM(core);
                }
                catch (ProtoCore.Exceptions.ExecutionCancelledException)
                {
                    Console.WriteLine("The execution has been cancelled!");
                }

                if (!isTest)
                {
                    runtimeCore.RuntimeMemory.Heap.Free();
                }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            if (isTest)
            {
                runtimeCore.Mirror = new ExecutionMirror(runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore);
            }
            return runtimeCore;
        }

        /// <summary>
        /// Load and executes the DS code in the specified file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ProtoCore.RuntimeCore LoadAndExecute(string filename, ProtoCore.Core core, bool isTest = true)
        {
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(filename, Encoding.UTF8, true);
            }
            catch (System.IO.IOException)
            {
                throw new Exception("Cannot open file " + filename);
            }

            string strSource = reader.ReadToEnd();
            reader.Dispose();

            core.Options.RootModulePathName = ProtoCore.Utils.FileUtils.GetFullPathName(filename);
            core.CurrentDSFileName = core.Options.RootModulePathName;
            return Execute(strSource, core);
        }

        
        /// <summary>
        /// The public method to compile DS code and stores the executable in core
        /// </summary>
        /// <param name="sourcecode"></param>
        /// <param name="compileCore"></param>
        /// <returns></returns>
        public bool CompileAndGenerateExe(string sourcecode, ProtoCore.Core compileCore, ProtoCore.CompileTime.Context context)
        {
            bool succeeded = Compile(sourcecode, compileCore, context);
            if (succeeded)
            {
                compileCore.GenerateExecutable();
            }
            return succeeded;
        }

        /// <summary>
        /// The public method to compile DS AST and stores the executable in core
        /// </summary>
        /// <param name="astList"></param>
        /// <param name="compileCore"></param>
        /// <returns></returns>
        public bool CompileAndGenerateExe(
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, 
            ProtoCore.Core compileCore,
            ProtoCore.CompileTime.Context context)
        {
            bool succeeded = Compile(astList, compileCore, context);
            if (succeeded)
            {
                compileCore.GenerateExecutable();
            }
            return succeeded;
        }
    }
}

