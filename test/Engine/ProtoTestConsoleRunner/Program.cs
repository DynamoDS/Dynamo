//#define __RUN_TESTFILE


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
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            core.Options.DumpByteCode = verbose;
            core.Options.Verbose = verbose;
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
            ExecutionMirror mirror = runner.LoadAndExecute(filename, core);
        }

        static void DevRun()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();


            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
#if DEBUG
            core.Options.DumpByteCode = true;
            core.Options.Verbose = true;
#else
            core.Options.DumpByteCode = false;
            core.Options.Verbose = false;
#endif

#if __RUN_TESTFILE
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());
            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
            ExecutionMirror mirror = runner.LoadAndExecute(@"c:\project\github\test.ds", core);
#else
            ProtoTest.Associative.MicroFeatureTests test = new ProtoTest.Associative.MicroFeatureTests();
            test.Setup();
            test.TestLocalKeywordFromImperative02();


            //ProtoTest.LiveRunner.MicroFeatureTests test = new ProtoTest.LiveRunner.MicroFeatureTests();
            //test.Setup();
            //test.TestNestedLanguageBlockReExecution04();

            //ProtoTest.Associative.MicroFeatureTests test = new ProtoTest.Associative.MicroFeatureTests();
            //test.Setup();
            //test.TestNoUpdate01();

            //ProtoTest.DebugTests.RunWatchTests test = new ProtoTest.DebugTests.RunWatchTests();
            //test.DebugWatch1684_T88_1461985_Update_In_Nested_Blocks_2();
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
