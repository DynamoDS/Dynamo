using System;
using System.Linq;
using System.Threading;
using Dynamo.Applications;
using Dynamo.Logging;
using Dynamo.Models;

namespace DynamoCLI
{
    internal class Program
    {
        private static EventWaitHandle suspendEvent = new AutoResetEvent(false);

        [STAThread]
        internal static void Main(string[] args)
        {
            bool useConsole = true;
            
            try
            {
                var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
                useConsole = !cmdLineArgs.NoConsole;
                var locale = StartupUtils.SetLocale(cmdLineArgs);

                cmdLineArgs.SetDisableAnalytics();

                if (cmdLineArgs.KeepAlive)
                {
                    var thread = new Thread(() => RunKeepAlive(cmdLineArgs))
                    {
                        Name = "DynamoModelKeepAlive"
                    };
                    thread.Start();

                    if (!useConsole)
                    {
                        suspendEvent.WaitOne();
                    }
                    else
                    {
                        Console.WriteLine("Starting DynamoCLI in keepalive mode");
                        Console.ReadLine();
                    }

                    ShutDown();
                }
                else
                {
                    var model = StartupDynamo(cmdLineArgs);

                    var runner = new CommandLineRunner(model);
                    runner.Run(cmdLineArgs);
                }
            }
            catch (Exception e)
            {
                try
                {
                    DynamoModel.IsCrashing = true;
                    Dynamo.Logging.Analytics.TrackException(e, true);
                }
                catch
                {
                }

                if (useConsole)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

            }
        }

        private static void RunKeepAlive(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            try
            {
                StartupDynamo(cmdLineArgs);

                if (!cmdLineArgs.NoConsole)
                {
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("DynamoCLI is running in keepalive mode");
                    Console.WriteLine("Press Enter to shutdown...");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Server is shutting down due to an error : " + ex.ToString());
            }
        }

        /// <summary>
        /// Start Dynamo Model and ViewModel per cmdLineArgs parameters.
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        private static DynamoModel StartupDynamo(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            DynamoModel model;
            model = StartupUtils.MakeCLIModel(cmdLineArgs);

            if (!string.IsNullOrEmpty(cmdLineArgs.CERLocation))
            {
                model.CERLocation = cmdLineArgs.CERLocation;
            }

            model.ShutdownCompleted += (m) => { ShutDown(); };

            cmdLineArgs.ImportedPaths?.ToList().ForEach(path =>
            {
                CommandLineRunner.ImportAssembly(model, path);
            });

            return model;
        }

        private static void ShutDown()
        {
            Environment.Exit(0);
        }

    }
}
