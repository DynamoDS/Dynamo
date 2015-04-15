using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoScript.Runners
{
    public class ProtoRunner
    {
        private ProtoCore.Core RunnerCore = null;
        private ProtoCore.RuntimeCore runtimeCore = null;
        private ProtoCore.CompileTime.Context ExecutionContext = null;

        // TODO Jun: The implementation of ProtoScriptTestRunner needs to go in here
        private ProtoScriptTestRunner Runner = null;

        public ProtoVMState PreStart(String source)
        {
            return PreStart(source, new Dictionary<string, object>());
        }

        public ProtoVMState PreStart(String source, Dictionary<string, Object> context)
        {
            ProtoCore.Options options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;


            RunnerCore = new ProtoCore.Core(options);
            RunnerCore.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(RunnerCore));
            RunnerCore.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(RunnerCore));

            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            //Validity.Assert(null == ExecutionContext);
            ExecutionContext = new ProtoCore.CompileTime.Context(source, context);

            //Validity.Assert(null == Runner);
            Runner = new ProtoScriptTestRunner();

            // TODO Jun: Implement run and halt at the first instruction
            //ProtoCore.DSASM.Mirror.ExecutionMirror mirror = null; // runner.Execute(executionContext, RunnerCore);

            return new ProtoVMState(RunnerCore, runtimeCore);
        }

        public ProtoVMState PreStart(String source, ProtoVMState state)
        {
            throw new NotImplementedException();
        }

        public ProtoVMState PreStart(String source, ProtoVMState state, Dictionary<string, Object> context)
        {
            throw new NotImplementedException();
        }


        #region Synchronous API

        public ProtoVMState StepIn(ProtoVMState state)
        {
            throw new NotImplementedException();
        }

        public ProtoVMState StepOver(ProtoVMState state)
        {
            throw new NotImplementedException();
        }

        public ProtoVMState StepOut(ProtoVMState state)
        {
            throw new NotImplementedException();
        }

        public ProtoVMState RunToNextBreakpoint(ProtoVMState state)
        {
            Validity.Assert(null != RunnerCore);
            Validity.Assert(null != ExecutionContext);
            Validity.Assert(null != Runner);

            // TODO Jun: Implement as DebugRunner, where breakpoints are inserted here.
            ProtoCore.RuntimeCore runtimeCore = null;
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = Runner.Execute(ExecutionContext, RunnerCore, out runtimeCore);

            return new ProtoVMState(RunnerCore, runtimeCore);
        }

        #endregion

        #region Sync Notifications API

        /// <summary>
        /// Inform the virtual machine that the value of an external variable mau have changed and any
        /// necessary variables may be updated
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public ProtoVMState NotifyChange(string variableName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assign the new value to the variable whose name has been specified and compute
        /// any necessary updates
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public ProtoVMState NotifyChange(string variableName, Object newValue)
        {
            throw new NotImplementedException();
        }


        #endregion



        public class ProtoVMState
        {
            private ProtoCore.Core core;
            private ProtoCore.RuntimeCore runtimeCore;

            public ProtoVMState(ProtoCore.Core core, ProtoCore.RuntimeCore runtimeCore)
            {
                this.core = core;
                this.runtimeCore = runtimeCore;
            }

            public ProtoCore.Mirror.RuntimeMirror LookupName(string name, int blockID)
            {
                // TODO Jun: The expression interpreter must be integrated into the mirror
                runtimeCore.RuntimeMemory.PushConstructBlockId(blockID);
                runtimeCore.DebugProps.CurrentBlockId = blockID;
                ProtoScript.Runners.ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, runtimeCore);

                List<ProtoCore.Core.CodeBlockCompilationSnapshot> snapShots = null;
                if (core.Options.IsDeltaExecution)
                {
                    snapShots = ProtoCore.Core.CodeBlockCompilationSnapshot.CaptureCoreCompileState(core);
                }
                ProtoCore.DSASM.Mirror.ExecutionMirror mirror = watchRunner.Execute(name);
                if (core.Options.IsDeltaExecution && snapShots != null)
                {
                    core.ResetDeltaCompileFromSnapshot(snapShots);
                }
                ProtoCore.Lang.Obj objExecVal = mirror.GetWatchValue();

                ProtoCore.Mirror.RuntimeMirror runtimeMirror = new ProtoCore.Mirror.RuntimeMirror(new ProtoCore.Mirror.MirrorData(core, objExecVal.DsasmValue), runtimeCore, core);
                Validity.Assert(runtimeMirror != null);

                return runtimeMirror;
            }

            public StackValue LookupNameToSV(string name)
            {
                throw new NotImplementedException();
            }

            public List<string> GetAllNamesInscope()
            {
                throw new IdentityNotMappedException();
            }
        }
    }
}
