using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoCore.Utils;
using ProtoCore;
using ProtoFFI;
using ProtoScript.Runners;

namespace ProtoTestFx
{
    public class DebugTestFx
    {
        public static void CompareDebugAndRunResults(string code)
        {
            Core runCore = null;

            try
            {
                runCore = TestRunnerRunOnly(code);
            }
            catch (Exception e)
            {
                Assert.Ignore("Ignored due to Exception from run: " + e.ToString());
            }



            {
                Core debugRunCore = DebugRunnerRunOnly(code);
                CompareCores(runCore, debugRunCore);
            }
            {
                Core stepOverCore = DebugRunnerStepOver(code);
                CompareCores(runCore, stepOverCore);
            }

            {
                Core stepInCore = DebugRunerStepIn(code);
                CompareCores(runCore, stepInCore);
            }

            

        }

        internal static ProtoCore.Core TestRunnerRunOnly(string code)
        {


            ProtoCore.Core core;
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScriptTestRunner();


            ProtoScript.Config.RunConfiguration runnerConfig;
            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new ProtoScriptTestRunner();

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.Execute(code, core);

            return core;
        }

        internal  static ProtoCore.Core DebugRunnerRunOnly(string code)
        {
            ProtoCore.Core core;
            DebugRunner fsr;
            ProtoScript.Config.RunConfiguration runnerConfig;
            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            options.GCTempVarsOnDebug = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = null;

            vms = fsr.Run();

            return core;
        }

        internal static ProtoCore.Core DebugRunnerStepOver(string code)
        {
            //Internal setup

            ProtoCore.Core core;
            DebugRunner fsr;
            ProtoScript.Config.RunConfiguration runnerConfig;
            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

             // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            options.GCTempVarsOnDebug = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = null;
 
            while (!fsr.isEnded)
                vms = fsr.StepOver();

            return core;

        }

        internal static ProtoCore.Core DebugRunerStepIn(string code)
        {
            //Internal setup

            ProtoCore.Core core;
            DebugRunner fsr;
            ProtoScript.Config.RunConfiguration runnerConfig;
            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            options.GCTempVarsOnDebug = false;

            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = null;

            while (!fsr.isEnded)
                vms = fsr.Step();

            return core;

        }

        internal static void CompareCores(Core c1, Core c2)
        {
            Assert.AreEqual(c1.DSExecutable.runtimeSymbols.Length, c2.DSExecutable.runtimeSymbols.Length);


            for (int symTableIndex = 0; symTableIndex < c1.DSExecutable.runtimeSymbols.Length; symTableIndex++)
            {
                foreach (SymbolNode symNode in c1.DSExecutable.runtimeSymbols[symTableIndex].symbolList.Values)
                {

                    ExecutionMirror runExecMirror = new ExecutionMirror(c1.CurrentExecutive.CurrentDSASMExec,
                                                                        c1);
                    ExecutionMirror debugExecMirror =
                        new ExecutionMirror(c2.CurrentExecutive.CurrentDSASMExec, c2);

                    bool lookupOk = false;
                    StackValue runValue = StackValue.Null;

                    if (symNode.name.StartsWith("%") || symNode.functionIndex != Constants.kInvalidIndex)
                        continue; //Don't care about internal variables

                    try
                    {
                        runValue = runExecMirror.GetGlobalValue(symNode.name);
                        Console.WriteLine(symNode.name + ": " + runValue);
                        lookupOk = true;

                    }
                    catch (NotImplementedException)
                    {

                    }
                    catch (Exception ex)
                    {
                        if ((ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException) &&
                            (c1.RunningBlock != symNode.runtimeTableIndex))
                        {
                            // Quite possible that variables defined in the inner
                            // language block have been garbage collected and 
                            // stack frame pointer has been adjusted when return
                            // to the outer block. 
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                    if (lookupOk)
                    {
                        StackValue debugValue = debugExecMirror.GetGlobalValue(symNode.name);
                        if (!StackUtils.CompareStackValues(debugValue, runValue, c2, c1))
                        {
                            Assert.Fail(string.Format("\tThe value of variable \"{0}\" doesn't match in run mode and in debug mode.\n", symNode.name));
                        }
                    }
                }
            }

        }
    }
}
