using System;
using System.Collections.Generic;

namespace ProtoScript.Runners
{
    [Obsolete("ProtoScriptRunner has been deprecated in favor of ProtoScripTestRunner")]
    public class ProtoScriptRunner
    {
        public ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        private string PromptAndInsertInput()
        {
            const string sPromptInput = "DS (q' to quit) > ";
            System.Console.Write(sPromptInput);
            string inString = System.Console.ReadLine();
            System.Console.Write("\n");
            return inString;
        }

        public bool Compile(string code, ProtoCore.Core core, out int blockId)
        {
            bool buildSucceeded = false;
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                // No More HashAngleReplace for unified parser (Fuqiang)
                //String strSource = ProtoCore.Utils.LexerUtils.HashAngleReplace(code);
                System.IO.MemoryStream sourceMemStream = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(code));
                ProtoScript.GenerateScript gs = new ProtoScript.GenerateScript(core);

                core.Script = gs.preParseFromStream(sourceMemStream);

                foreach (ProtoCore.LanguageCodeBlock codeblock in core.Script.codeblockList)
                {
                    ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                    ProtoCore.Language id = codeblock.language;

                    core.Executives[id].Compile(out blockId, null, codeblock, context, EventSink);
                }

                core.BuildStatus.ReportBuildResult();

                buildSucceeded = core.BuildStatus.BuildSucceeded;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return buildSucceeded;
        }

        public void Execute(ProtoCore.Core core)
        {
            try
            {             
                foreach (ProtoCore.DSASM.CodeBlock codeblock in core.CodeBlockList)
                {
                    ProtoCore.Runtime.Context context = new ProtoCore.Runtime.Context();

                    int locals = 0;
                    core.Bounce(codeblock.codeBlockId, codeblock.instrStream.entrypoint, context, new ProtoCore.DSASM.StackFrame(core.GlobOffset), locals, EventSink);
                }
            }
            catch
            {
                throw;
            }
        }


        public void Execute(string code, ProtoCore.Core core)
        {
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(code, core, out blockId);
            if (succeeded)
            {
                core.GenerateExecutable();
                core.Rmem.PushGlobFrame(core.GlobOffset);
                core.RunningBlock = blockId;
                Execute(core);
                core.Heap.Free();
            }
            else
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
        }

        public void LoadAndExecute(string pathFilename, ProtoCore.Core core)
        {
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(pathFilename);
            } 
            catch (System.IO.IOException) 
            {
			    throw new FatalError("Cannot open file " + pathFilename);
		    }


            string strSource = reader.ReadToEnd();
            reader.Dispose();

            if (EventSink != null && EventSink.BeginDocument != null)
            {
                EventSink.BeginDocument.Invoke("Started executing script: " + pathFilename + "\n");
            }

            Execute(strSource, core);

            if (EventSink != null && EventSink.EndDocument != null)
            {
                EventSink.EndDocument.Invoke("Done executing script: " + pathFilename + "\n");
            }
        }
    }
}
