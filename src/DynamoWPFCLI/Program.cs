using System;
using System.Threading;
using Dynamo.Applications;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using static System.Windows.Threading.Dispatcher;

namespace DynamoWPFCLI
{
    internal class Program
    {
        [STAThread]
        internal static void Main(string[] args)
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

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    Console.WriteLine("Starting DynamoWPFCLI in keepalive mode");
                    Console.ReadLine();

                    ShutDown();
                }
                else
                {
                    var viewModel = StartupDaynamo(cmdLineArgs);

                    var runner = new CommandLineRunnerWPF(viewModel);
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

        /// <summary>
        /// Start Dynamo Model and ViewModel per cmdLineArgs parameters.
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        private static DynamoViewModel StartupDaynamo(StartupUtils.CommandLineArguments cmdLineArgs)
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

            var viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = model,
                    Watch3DViewModel = new DefaultWatch3DViewModel(null, new Watch3DViewModelStartupParams(model))
                    {
                        Active = false,
                        CanBeActivated = false
                    }
                });
            return viewModel;
        }

        private static void RunKeepAlive(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            try
            {
                StartupDaynamo(cmdLineArgs);

                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("DynamoWPFCLI is running in keepalive mode");
                Console.WriteLine("Press Enter to shutdown...");

                Run();
            }
            catch
            {
                Console.WriteLine("Server is shutting down due to an error");
            }
        }

        private static void ShutDown()
        {
            Environment.Exit(0);
        }

    }
}
