using System;
using System.Linq;
using System.Threading;
using Dynamo.Applications;
using Dynamo.Models;

namespace DynamoCLI
{
    internal class Program
    {
        [STAThread]
        static internal void Main(string[] args)
        {
            try
            {
                var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
                var locale = StartupUtils.SetLocale(cmdLineArgs);
                if (cmdLineArgs.DisableAnalytics)
                {
                    Dynamo.Logging.Analytics.DisableAnalytics = true;
                }

                if (cmdLineArgs.KeepAlive)
                {
                    var thread = new Thread(() => RunKeepAlive(cmdLineArgs));

                    thread.Name = "DynamoModelKeepAlive";
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    Console.WriteLine("Starting DynamoCLI in keepalive mode");
                    Console.ReadLine();

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

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void RunKeepAlive(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            try
            {
                StartupDynamo(cmdLineArgs);

                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("DynamoCLI is running in keepalive mode");
                Console.WriteLine("Press Enter to shutdown...");

                System.Windows.Threading.Dispatcher.Run();
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
            if (!String.IsNullOrEmpty(cmdLineArgs.ASMPath))
            {
                model = Dynamo.Applications.StartupUtils.MakeModel(true, cmdLineArgs.ASMPath, cmdLineArgs.AnalyticsInfo);
            }
            else
            {
                model = Dynamo.Applications.StartupUtils.MakeModel(true, string.Empty, cmdLineArgs.AnalyticsInfo);
            }

            model.ShutdownCompleted += (m) => { ShutDown(); };

            cmdLineArgs.ImportedPaths.ToList().ForEach(path =>
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
