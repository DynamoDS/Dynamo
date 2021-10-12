using System;
using System.Threading;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.ViewModels.Watch3D;
using static System.Windows.Threading.Dispatcher;

namespace DynamoWPFCLI
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

                if (cmdLineArgs.KeepAlive)
                {
                    var thread = new Thread(() => RunKeepAlive(cmdLineArgs));

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    Console.ReadLine();

                    ShutDown();
                }
                else
                {
                    DynamoModel model;
                    if (!String.IsNullOrEmpty(cmdLineArgs.ASMPath))
                    {
                        model = Dynamo.Applications.StartupUtils.MakeModel(true, cmdLineArgs.ASMPath);
                    }
                    else
                    {
                        model = Dynamo.Applications.StartupUtils.MakeModel(true);
                    }
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

        private static void RunKeepAlive(StartupUtils.CommandLineArguments cmdLineArgs)
        {
            try
            {
                DynamoModel model;
                if (!String.IsNullOrEmpty(cmdLineArgs.ASMPath))
                {
                    model = Dynamo.Applications.StartupUtils.MakeModel(true, cmdLineArgs.ASMPath);
                }
                else
                {
                    model = Dynamo.Applications.StartupUtils.MakeModel(true);
                }

                model.ShutdownCompleted += (m) =>
                {
                    ShutDown();
                };

                DefaultWatch3DViewModel defaultWatch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(null, new Watch3DViewModelStartupParams(model), model.Logger);
                defaultWatch3DViewModel.Active = false;
                defaultWatch3DViewModel.CanBeActivated = false;

                var viewModel = DynamoViewModel.Start(
                    new DynamoViewModel.StartConfiguration()
                    {
                        DynamoModel = model,
                        Watch3DViewModel = defaultWatch3DViewModel
                    });

                var dynView = new DynamoView(viewModel);

                var sharedViewExtensionLoadedParams = new ViewLoadedParams(dynView, viewModel);

                foreach (var ext in dynView.viewExtensionManager.ViewExtensions)
                {
                    try
                    {
                        ext.Loaded(sharedViewExtensionLoadedParams);
                        Console.WriteLine("loaded " + ext.Name);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(ext.Name + ": " + exc.Message);
                    }
                }
           
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
