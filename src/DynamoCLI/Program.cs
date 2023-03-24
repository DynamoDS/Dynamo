using System;
using System.Linq;
using System.Threading;
using Dynamo.Applications;
using Dynamo.Models;

namespace DynamoCLI
{
    internal class Program
    {
        private static EventWaitHandle suspendEvent = new AutoResetEvent(false);

        [STAThread]
        static internal void Main(string[] args)
        {
            bool useConsole = true;
            
            try
            {
                var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
                useConsole = !cmdLineArgs.NoConsole;
                var locale = StartupUtils.SetLocale(cmdLineArgs);
                if (cmdLineArgs.DisableAnalytics)
                {
                    Dynamo.Logging.Analytics.DisableAnalytics = true;
                }

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
            catch
            {
                Console.WriteLine("Server is shutting down due to an error");
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
            model = Dynamo.Applications.StartupUtils.MakeCLIModel(String.IsNullOrEmpty(cmdLineArgs.ASMPath) ? string.Empty : cmdLineArgs.ASMPath,
                cmdLineArgs.UserDataFolder,
                cmdLineArgs.CommonDataFolder,
                cmdLineArgs.AnalyticsInfo,
                cmdLineArgs.ServiceMode);

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
