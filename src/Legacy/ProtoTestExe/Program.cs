#define __RUN_TESTFILE
using System;
using ProtoCore;
using ProtoScript.Runners;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using System.IO;

namespace ProtoTestExe
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
            ExecutionMirror mirror = runner.LoadAndExecute(@"defectverify.ds", core);

#else

            ProtoTest.GraphCompiler.MicroFeatureTests test = new ProtoTest.GraphCompiler.MicroFeatureTests();
            test.Setup();
            test.GraphILTest_Assign02();
            
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
                Run(args[0], false);
            }
            else
            {
                DevRun();
            }
        }
    }
}
