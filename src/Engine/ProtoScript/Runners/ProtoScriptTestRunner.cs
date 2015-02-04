using System;
using System.Text;
using System.Collections.Generic;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoCore.DSASM;

namespace ProtoScript.Runners
{

    public class ProtoScriptTestRunner
    {
        public ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();


        public bool Compile(ProtoCore.CompileTime.Context context, ProtoCore.Core core, out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                // No More HashAngleReplace for unified parser (Fuqiang)
                //String strSource = ProtoCore.Utils.LexerUtils.HashAngleReplace(code);    

                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;

                Validity.Assert(null != context.SourceCode && String.Empty != context.SourceCode);
                globalBlock.body = context.SourceCode;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.Language id = globalBlock.language;
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

        public bool Compile(string code, ProtoCore.Core core, out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                // No More HashAngleReplace for unified parser (Fuqiang)
                //String strSource = ProtoCore.Utils.LexerUtils.HashAngleReplace(code);    

                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                globalBlock.body = code;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.language;
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


        public bool Compile(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, ProtoCore.Core core, out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                globalBlock.body = string.Empty;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                context.SetData(string.Empty, new Dictionary<string, object>(), null);
                ProtoCore.Language id = globalBlock.language;

                
		        ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
                codeblock.Body.AddRange(astList);

                core.Compilers[id].Compile(out blockId, null, globalBlock, context, EventSink, codeblock);

                core.BuildStatus.ReportBuildResult();

                buildSucceeded = core.BuildStatus.BuildSucceeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return buildSucceeded;
        }

        /// <summary>
        /// Execute the data stored in core
        /// This is the entry point of all DS code to be executed
        /// </summary>
        /// <param name="core"></param>
        /// <param name="runningBlock"></param>
        /// <param name="staticContext"></param>
        /// <param name="runtimeContext"></param>
        public void Execute(ProtoCore.Core core, int runningBlock, ProtoCore.CompileTime.Context staticContext, ProtoCore.Runtime.Context runtimeContext)
        {
            // Move these core setup to runtime core 
            core.Rmem.PushFrameForGlobals(core.GlobOffset);
            core.RunningBlock = runningBlock;

            ProtoCore.RuntimeCore runtimeCore = new ProtoCore.RuntimeCore(core.Options, core.DSExecutable, runtimeContext, core.DebuggerProperties);
            core.RuntimeCoreBridge = runtimeCore;
            runtimeCore.RuntimeStatus = new ProtoCore.RuntimeStatus(core);

            try
            {
                core.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionBegin);
                foreach (ProtoCore.DSASM.CodeBlock codeblock in core.CodeBlockList)
                {
                    // Comment Jun:
                    // On first bounce, the stackframe depth is initialized to -1 in the Stackfame constructor.
                    // Passing it to bounce() increments it so the first depth is always 0
                    ProtoCore.DSASM.StackFrame stackFrame = new ProtoCore.DSASM.StackFrame(core.GlobOffset);
                    stackFrame.FramePointer = core.Rmem.FramePointer;
                    
                    // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                    // Register TX is used for this.
                    StackValue svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);
                    stackFrame.TX = svCallConvention;

                    // Initialize the entry point interpreter
                    int locals = 0; // This is the global scope, there are no locals
                    ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
                    core.CurrentExecutive.CurrentDSASMExec = interpreter.runtime;
                    core.CurrentExecutive.CurrentDSASMExec.Bounce(codeblock.codeBlockId, codeblock.instrStream.entrypoint, runtimeContext, stackFrame, locals);
                }
                core.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionEnd);
            }
            catch 
            {
                core.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionEnd);
                throw;
            }
        }
        

        /// <summary>
        /// Compile and execute the source that is stored in the static context
        /// </summary>
        /// <param name="staticContext"></param>
        /// <param name="runtimeContext"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ExecutionMirror Execute(ProtoCore.CompileTime.Context staticContext, ProtoCore.Runtime.Context runtimeContext, ProtoCore.Core core, bool isTest = true)
        {
            Validity.Assert(null != staticContext.SourceCode && String.Empty != staticContext.SourceCode);

            core.AddContextData(staticContext.GlobalVarList);
   
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(staticContext, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                Validity.Assert(null != runtimeContext);
                Execute(core, blockId, staticContext, runtimeContext);
                if (!isTest)
                {
                    core.Heap.Free();
                }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            if (isTest && !core.Options.CompileToLib)
            {
                return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
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
        public ExecutionMirror Execute(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, ProtoCore.Core core, bool isTest = true)
        {
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(astList, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                Execute(core, blockId, new ProtoCore.CompileTime.Context(), new ProtoCore.Runtime.Context());
                if (!isTest) 
                { 
                    core.Heap.Free(); 
                }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            if (isTest && !core.Options.CompileToLib)
            {
                return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
            }

            return null;
        }
      

        /// <summary>
        /// Compile and execute the given sourcecode
        /// </summary>
        /// <param name="code"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ExecutionMirror Execute(string sourcecode, ProtoCore.Core core, bool isTest = true)
        {
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(sourcecode, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                try
                {
                    Execute(core, blockId, new ProtoCore.CompileTime.Context(), new ProtoCore.Runtime.Context());
                }
                catch (ProtoCore.Exceptions.ExecutionCancelledException e)
                {
                    Console.WriteLine("The execution has been cancelled!");             
                }
                
                if (!isTest)
                {
                    core.Heap.Free();
                }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            if (isTest && !core.Options.CompileToLib)
            {
                return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
            }

            return null;
        }

        /// <summary>
        /// Load and execute the DS code in the specified file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="core"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        public ExecutionMirror LoadAndExecute(string filename, ProtoCore.Core core, bool isTest = true)
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
            //Start the timer       
            core.StartTimer();

            core.Options.RootModulePathName = ProtoCore.Utils.FileUtils.GetFullPathName(filename);
            core.CurrentDSFileName = core.Options.RootModulePathName;
            Execute(strSource, core);

            if (isTest && !core.Options.CompileToLib)
                return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
            else
                return null;
        }
    }
}
