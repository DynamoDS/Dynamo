using System;
using NUnit.Framework;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;

namespace ProtoTestFx
{
    public class DebugTestFx
    {
        public static void CompareDebugAndRunResults(string code, string defectID = "")
        {
            Core runCore = null;
            RuntimeCore runtimeCore = null;
            try
            {
                runCore = TestRunnerRunOnly(code, out runtimeCore);
            }
            catch (Exception e)
            {
                Assert.Ignore("Ignored due to Exception from run: " + e.ToString());
            }

            {
                RuntimeCore debugRuntimeCore = null;
                Core debugRunCore = DebugRunnerRunOnly(code, out debugRuntimeCore);
                CompareCores(runtimeCore, debugRuntimeCore, defectID);
                debugRuntimeCore.Cleanup();
            }

            {
                RuntimeCore stepOverRuntimeCore = null;
                Core stepOverCore = DebugRunnerStepOver(code, out stepOverRuntimeCore);
                CompareCores(runtimeCore, stepOverRuntimeCore, defectID);
                stepOverRuntimeCore.Cleanup();
            }

            {
                RuntimeCore stepInRuntimeCore = null;
                Core stepInCore = DebugRunerStepIn(code, out stepInRuntimeCore);
                CompareCores(runtimeCore, stepInRuntimeCore, defectID);
                stepInRuntimeCore.Cleanup();
            }

            runtimeCore.Cleanup();

        }

        internal static ProtoCore.Core TestRunnerRunOnly(string code, out RuntimeCore runtimeCore)
        {
            ProtoCore.Core core;
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScriptRunner();

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;

            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";
            options.IncludeDirectories.Add(testPath);

            core = new ProtoCore.Core(options);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));

            fsr = new ProtoScriptRunner();

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            runtimeCore = fsr.Execute(code, core);

            return core;
        }

        internal  static ProtoCore.Core DebugRunnerRunOnly(string code, out RuntimeCore runtimeCore)
        {
            ProtoCore.Core core;
            DebugRunner fsr;

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.GCTempVarsOnDebug = false;

            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";
            options.IncludeDirectories.Add(testPath);

            core = new ProtoCore.Core(options);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));

            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.PreStart(code);
            DebugRunner.VMState vms = null;

            vms = fsr.Run();
            runtimeCore = fsr.runtimeCore;
            return core;
        }

        internal static ProtoCore.Core DebugRunnerStepOver(string code, out RuntimeCore runtimeCore)
        {
            //Internal setup

            ProtoCore.Core core;
            DebugRunner fsr;

             // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.GCTempVarsOnDebug = false;

            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";
            options.IncludeDirectories.Add(testPath);

            core = new ProtoCore.Core(options);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));


            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.PreStart(code);
            DebugRunner.VMState vms = null;
 
            while (!fsr.isEnded)
                vms = fsr.StepOver();

            runtimeCore = fsr.runtimeCore;
            return core;

        }

        internal static ProtoCore.Core DebugRunerStepIn(string code, out RuntimeCore runtimeCore)
        {
            //Internal setup

            ProtoCore.Core core;
            DebugRunner fsr;

            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.GCTempVarsOnDebug = false;

            string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";
            options.IncludeDirectories.Add(testPath);

            core = new ProtoCore.Core(options);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));

            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            //Run

            fsr.PreStart(code);
            DebugRunner.VMState vms = null;

            while (!fsr.isEnded)
                vms = fsr.Step();

            runtimeCore = fsr.runtimeCore;
            return core;

        }

        internal static void CompareCores(RuntimeCore rtcore1, RuntimeCore rtcore2, string defectID = "")
        {
            Assert.AreEqual(rtcore1.DSExecutable.runtimeSymbols.Length, rtcore1.DSExecutable.runtimeSymbols.Length, defectID);


            for (int symTableIndex = 0; symTableIndex < rtcore1.DSExecutable.runtimeSymbols.Length; symTableIndex++)
            {
                foreach (SymbolNode symNode in rtcore1.DSExecutable.runtimeSymbols[symTableIndex].symbolList.Values)
                {

                    ExecutionMirror runExecMirror = new ExecutionMirror(rtcore1.CurrentExecutive.CurrentDSASMExec,rtcore1);
                    ExecutionMirror debugExecMirror = new ExecutionMirror(rtcore2.CurrentExecutive.CurrentDSASMExec, rtcore2);

                    bool lookupOk = false;
                    StackValue runValue = StackValue.Null;

                    if (symNode.name.StartsWith("%") || symNode.functionIndex != Constants.kInvalidIndex)
                        continue; //Don't care about internal variables

                    try
                    {
                        runValue = runExecMirror.GetValue(symNode.name);
                        lookupOk = true;

                    }
                    catch (NotImplementedException)
                    {
                    }
                    catch (SymbolNotFoundException)
                    {
                    }
                    catch (Exception ex)
                    {
                        if ((ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException) &&
                            (rtcore1.RunningBlock != symNode.runtimeTableIndex))
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
                        StackValue debugValue = debugExecMirror.GetValue(symNode.name);
                        if (!StackUtils.CompareStackValues(debugValue, runValue, rtcore2, rtcore1))
                        {
                            Assert.Fail(string.Format("\tThe value of variable \"{0}\" doesn't match in run mode and in debug mode.\nTracked by {1}", symNode.name, defectID));
                        }
                    }
                }
            }

        }
    }
}
