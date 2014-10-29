
using System;
using ProtoCore;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;
using System.Collections.Generic;


namespace ProtoScript
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            bool testScript = string.Equals(args[0], "test") && args.Length == 2;
            var opts = new Options();
            if (args.Length == 0 || testScript)
            {
                opts.WebRunner = false;
            }
            else
            {
                opts.WebRunner = true;
            }
            opts.ExecutionMode = ExecutionMode.Serial;

            ProtoCore.Core core = new Core(opts);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));


            if (!opts.WebRunner)
            {
                string fileName;
                if (testScript)
                {
                    fileName = args[1];
                }
                else
                {
                    fileName = @"..\..\..\Scripts\defectverify.ds";
                }

                ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                ExecutionMirror mirror = runner.LoadAndExecute(fileName, core);
                string str = mirror.GetCoreDump();
                //Console.WriteLine(str);
            }
            else //if (arg)
            {
                // @TODO(Gemeng): Do set "core.ExecutionLog" to some file :)

                ProtoScriptWebRunner runner = new ProtoScriptWebRunner();
                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                ExecutionMirror mirror = runner.LoadAndExecute(args[0], core);
                if (null != mirror)
                {
                    string str = mirror.GetCoreDump();
                    // core.coreDumpWriter.WriteLine(str);

                }

                // @TODO(Gemeng): 'coreDumpWriter' has been removed, instead of 
                // doing that we should create an output file stream here with a
                // file name, and then write the "str" content into it.
                // 
                // core.coreDumpWriter.Close();
            }


            //runner.LoadAndExecute(@"..\..\..\Scripts\array.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\arrayargs.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class2.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class3.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class4.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class5.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class6.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class7.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class8.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class9.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\class10.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\demo.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\expr.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\fusionarray.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\replication.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\simple.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\simple2.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\functionoverload.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\classfunctionoverload.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\inheritance.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\inheritance2.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\update.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\null.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\forloop.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\blockassign_imperative.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\blockassign_associative.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\test.ds", core);
            //runner.LoadAndExecute(@"..\..\..\Scripts\importtest.ds", core);

            //ExecutionMirror mirror = null;   



            //string str = mirror.GetCoreDump();
            long ms = sw.ElapsedMilliseconds;
            sw.Stop();

            if (!opts.WebRunner)
            {
                Console.WriteLine(ms);
                Console.ReadLine();
            }
        }
    }
}

