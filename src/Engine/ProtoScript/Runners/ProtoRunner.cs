using System;
using System.Collections.Generic;
using ProtoCore.Utils;

namespace ProtoScript.Runners
{
    public class ProtoRunner
    {
        private ProtoCore.Core RunnerCore = null;
        private ProtoCore.RuntimeCore runtimeCore = null;
        private ProtoCore.CompileTime.Context ExecutionContext = null;

        // TODO Jun: The implementation of ProtoScriptRunner needs to go in here
        private ProtoScriptRunner Runner = null;

        public void PreStart(String source, Dictionary<string, Object> context)
        {
            ProtoCore.Options options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;


            RunnerCore = new ProtoCore.Core(options);
            RunnerCore.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(RunnerCore));
            RunnerCore.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(RunnerCore));

            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            ExecutionContext = new ProtoCore.CompileTime.Context(source, context);

            Runner = new ProtoScriptRunner();
        }


        #region Synchronous API

        public void RunToNextBreakpoint()
        {
            Validity.Assert(null != RunnerCore);
            Validity.Assert(null != ExecutionContext);
            Validity.Assert(null != Runner);

            // TODO Jun: Implement as DebugRunner, where breakpoints are inserted here.
            ProtoCore.RuntimeCore runtimeCore = null;
            Runner.Execute(ExecutionContext, RunnerCore, out runtimeCore);
        }

        #endregion
    }
}
