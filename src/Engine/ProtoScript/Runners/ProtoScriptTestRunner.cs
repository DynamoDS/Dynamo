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

            core.ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

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
                core.Executives[id].Compile(out blockId, null, globalBlock, context, EventSink);

                core.BuildStatus.ReportBuildResult();

                int errors = 0;
                int warnings = 0;
                buildSucceeded = core.BuildStatus.GetBuildResult(out errors, out warnings);
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

            core.ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

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
                core.Executives[id].Compile(out blockId, null, globalBlock, context, EventSink);

                core.BuildStatus.ReportBuildResult();

                int errors = 0;
                int warnings = 0;
                buildSucceeded = core.BuildStatus.GetBuildResult(out errors, out warnings);
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

            core.ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

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

                core.Executives[id].Compile(out blockId, null, globalBlock, context, EventSink, codeblock);

                core.BuildStatus.ReportBuildResult();

                int errors = 0;
                int warnings = 0;
                buildSucceeded = core.BuildStatus.GetBuildResult(out errors, out warnings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return buildSucceeded;
        }

        public void Execute(ProtoCore.Core core, ProtoCore.Runtime.Context context)
        {
            try
            {
                core.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionBegin);
                foreach (ProtoCore.DSASM.CodeBlock codeblock in core.CodeBlockList)
                {
                    //ProtoCore.Runtime.Context context = new ProtoCore.Runtime.Context();

                    int locals = 0;


                    // Comment Jun:
                    // On first bounce, the stackframe depth is initialized to -1 in the Stackfame constructor.
                    // Passing it to bounce() increments it so the first depth is always 0
                    ProtoCore.DSASM.StackFrame stackFrame = new ProtoCore.DSASM.StackFrame(core.GlobOffset);
                    
                    // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                    // Register TX is used for this.
                    StackValue svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);
                    stackFrame.SetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kRegisterTX, svCallConvention);

                    core.Bounce(codeblock.codeBlockId, codeblock.instrStream.entrypoint, context, stackFrame, locals, EventSink);
                }
                core.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionEnd);
            }
            catch 
            {
                core.NotifyExecutionEvent(ProtoCore.ExecutionStateEventArgs.State.kExecutionEnd);
                throw;
            }
        }

        public ExecutionMirror Execute(string code, ProtoCore.Core core, Dictionary<string, Object> values, bool isTest = true)
        {
            //Inject the context data values from external source.
            core.AddContextData(values);
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(code, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                core.Rmem.PushGlobFrame(core.GlobOffset);
                core.RunningBlock = blockId;

                Execute(core, new ProtoCore.Runtime.Context());

                if (!isTest) { core.Heap.Free(); }
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }

            if (isTest && !core.Options.CompileToLib)
            {
                return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
            }

            // Save the Callsite state for this execution
            if (core.EnableCallsiteExecutionState)
            {
                ProtoCore.CallsiteExecutionState.SaveState(core.csExecutionState);
            }

            return null;
        }

        public ExecutionMirror Execute(ProtoCore.CompileTime.Context staticContext, ProtoCore.Runtime.Context runtimeContext, ProtoCore.Core core, bool isTest = true)
        {
            Validity.Assert(null != staticContext.SourceCode && String.Empty != staticContext.SourceCode);
            
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(staticContext, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                core.Rmem.PushGlobFrame(core.GlobOffset);
                core.RunningBlock = blockId;
                core.InitializeContextGlobals(staticContext.GlobalVarList);

                Validity.Assert(null != runtimeContext);
                Execute(core, runtimeContext);
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

            // Save the Callsite state for this execution
            if (core.EnableCallsiteExecutionState)
            {
                ProtoCore.CallsiteExecutionState.SaveState(core.csExecutionState);
            }

            return null;
        }

        public ExecutionMirror Execute(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList, ProtoCore.Core core, bool isTest = true)
        {
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(astList, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                core.Rmem.PushGlobFrame(core.GlobOffset);
                core.RunningBlock = blockId;

                Execute(core, new ProtoCore.Runtime.Context());
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

            // Save the Callsite state for this execution
            if (core.EnableCallsiteExecutionState)
            {
                ProtoCore.CallsiteExecutionState.SaveState(core.csExecutionState);
            }

            return null;
        }

        public ExecutionMirror Execute(string code, ProtoCore.Core core, bool isTest = true)
        {
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(code, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                core.Rmem.PushGlobFrame(core.GlobOffset);
                core.RunningBlock = blockId;

                Execute(core, new ProtoCore.Runtime.Context());
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

            // Save the Callsite state for this execution
            if (core.EnableCallsiteExecutionState)
            {
                ProtoCore.CallsiteExecutionState.SaveState(core.csExecutionState);
            }

            return null;
        }

        public ExecutionMirror LoadAndExecute(string filename, ProtoCore.Core core, bool isTest = true)
        {
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(filename, Encoding.UTF8, true);
            }
            catch (System.IO.IOException)
            {
                throw new FatalError("Cannot open file " + filename);
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
