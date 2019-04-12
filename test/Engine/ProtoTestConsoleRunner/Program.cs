using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ProtoCore;
using ProtoScript.Runners;

namespace ProtoTestConsoleRunner
{
    class Program
    {
        static long maxNetMemory = 0;
        static long maxPrivateWorkingSet = 0;
        static bool done = false;

        static void CollectingMemory()
        {
            Process currentProcess = Process.GetCurrentProcess();
            var privateByteCounter = new PerformanceCounter("Process", "Working Set - Private", currentProcess.ProcessName);
            var netMemoryCounter = new PerformanceCounter(".NET CLR Memory", "# Bytes in all Heaps", currentProcess.ProcessName);

            while (!done)
            {
                maxNetMemory = Math.Max(netMemoryCounter.NextSample().RawValue / 1024, maxNetMemory);
                maxPrivateWorkingSet = Math.Max(privateByteCounter.NextSample().RawValue / 1024, maxPrivateWorkingSet);
                Thread.Sleep(100);
            }

            privateByteCounter.Dispose();
            netMemoryCounter.Dispose();
        }

        static void Run(string filename, bool verbose)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("Cannot find file " + filename);
                return;
            }

            var profilingThread = new Thread(new ThreadStart(CollectingMemory));
            profilingThread.Start();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
            ProtoCore.Core core = new Core(opts);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));
            core.Options.DumpByteCode = verbose;
            core.Options.Verbose = verbose;
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            ProtoScriptRunner runner = new ProtoScriptRunner();
            runner.LoadAndExecute(filename, core);
            long ms = sw.ElapsedMilliseconds;
            sw.Stop();

            done = true;
            profilingThread.Join();

            Console.WriteLine("{0},{1},{2}", ms, maxNetMemory, maxPrivateWorkingSet);
        }

        static void DevRun()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();


            var opts = new Options();
            opts.ExecutionMode = ExecutionMode.Serial;
#if DEBUG
            opts.DumpByteCode = true;
            opts.Verbose = true;
#else
            opts.DumpByteCode = false;
            opts.Verbose = false;
#endif
            ProtoCore.Core core = new Core(opts);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));

            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());
            ProtoScriptRunner runner = new ProtoScriptRunner();

            // Assuming current directory in test/debug mode is "...\Dynamo\bin\AnyCPU\Debug"
            runner.LoadAndExecute(@"..\..\..\test\core\dsevaluation\DSFiles\test.ds", core);

            long ms = sw.ElapsedMilliseconds;
            sw.Stop();
            Console.WriteLine(ms);
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                bool verbose = args.Length >= 2 && args[1] == "-v";
                Run(args[0], verbose);
            }
            else
            {
                DevRun();
            }
        }
    }
}
