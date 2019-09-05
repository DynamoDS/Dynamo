using System;
using Dynamo.Applications;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;

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

    }
}
