
#define __RUN_TESTFILE

using System;
using ProtoCore;
using ProtoScript.Runners;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using System.IO;

namespace ProtoTestConsoleRunner
{
    class Program
    {
        static void Run(string filename, bool verbose)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Cannot find file " + filename);
                return;
            }
            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
            core.Options.DumpByteCode = verbose;
            core.Options.Verbose = verbose;
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();

            RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = runner.LoadAndExecute(filename, core, out runtimeCore);
        }

        static void DevRun()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();


            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
#if DEBUG
            core.Options.DumpByteCode = true;
            core.Options.Verbose = true;
#else
            core.Options.DumpByteCode = false;
            core.Options.Verbose = false;
#endif
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());
            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();

#if __RUN_TESTFILE
            // Assuming current directory in test/debug mode is "...\Dynamo\bin\AnyCPU\Debug"
            RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = runner.LoadAndExecute(@"..\..\..\test\core\dsevaluation\DSFiles\test.ds", core, out runtimeCore);
#else
            //inlineconditional_656_6
            ProtoTest.DebugTests.BasicTests test = new ProtoTest.DebugTests.BasicTests();
            test.Setup();
            test.inlineconditional_656_6();
#endif
            long ms = sw.ElapsedMilliseconds;
            sw.Stop();
            Console.WriteLine(ms);
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                Run(args[0], true);
            }
            else
            {
                DevRun();
            }
        }
    }
}
