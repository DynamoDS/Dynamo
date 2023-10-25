using System;
using System.Linq;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Threading;
using Dynamo.Applications;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using static System.Windows.Threading.Dispatcher;

namespace DynamoWPFCLI
{
    internal class Program
    {
        private static EventWaitHandle suspendEvent = new AutoResetEvent(false);

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
#endif
        [STAThread]
        internal static void Main(string[] args)
        {
            bool useConsole = true;
            try
            {
                var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
                useConsole = !cmdLineArgs.NoConsole;
                var locale = StartupUtils.SetLocale(cmdLineArgs);

                if (cmdLineArgs.DisableAnalytics)
                {
                    Analytics.DisableAnalytics = true;
                }
                if (cmdLineArgs.ServiceMode)
                {
                    Console.WriteLine("Starting DynamoWPFCLI in service mode");
                }
                if (cmdLineArgs.KeepAlive)
                {
                    var thread = new Thread(() => RunKeepAlive(cmdLineArgs))
                    {
                        Name = "DynamoModelKeepAlive"
                    };
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    if (!useConsole)
                    {
                        suspendEvent.WaitOne();
                    }
                    else
                    {
                        Console.WriteLine("Starting DynamoWPFCLI in keepalive mode");
                        Console.ReadLine();
                    }

                    ShutDown();
                }
                else
                {
                    var viewModel = StartupDynamo(cmdLineArgs);

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

                if (useConsole)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Start Dynamo Model and ViewModel per cmdLineArgs parameters.
        /// </summary>
        /// <param name="cmdLineArgs"></param>
        /// <returns></returns>
        private static DynamoViewModel StartupDynamo(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            var model = StartupUtils.MakeCLIModel(cmdLineArgs);

            if (!string.IsNullOrEmpty(cmdLineArgs.CERLocation))
            {
                model.CERLocation = cmdLineArgs.CERLocation;
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

            cmdLineArgs.ImportedPaths.ToList().ForEach(path =>
            {
                ImportAssembly(model, path);
            });

            return viewModel;
        }

        private static void RunKeepAlive(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            try
            {
                StartupDynamo(cmdLineArgs);

                if (!cmdLineArgs.NoConsole)
                {
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine("DynamoWPFCLI is running in keepalive mode");
                    Console.WriteLine("Press Enter to shutdown...");
                }

                Run();
            }
            catch
            {
                Console.WriteLine("Server is shutting down due to an error");
            }
        }

        /// <summary>
        /// Attempts to import an assembly as a node library from a given file path.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="path"></param>
        private static void ImportAssembly(DynamoModel model, string path)
        {
            try
            {
                var filePath = new System.IO.FileInfo(path);
                if (!filePath.Exists)
                {
                    Console.WriteLine($"could not find requested import library at path{path}");
                }
                else
                {
                    Console.WriteLine($"attempting to import assembly {path}");
                    var assembly = System.Reflection.Assembly.LoadFile(path);
                    model.LoadNodeLibrary(assembly, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception while trying to load assembly {path}: {e}");
            }
        }

        private static void ShutDown()
        {
            Environment.Exit(0);
        }

    }
}
