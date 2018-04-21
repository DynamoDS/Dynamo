using System;
using Dynamo.Applications;
using Dynamo.Models;
using Dynamo.ViewModels;

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
                var model = StartupUtils.MakeModel(true);
                var viewModel = DynamoViewModel.Start(
                    new DynamoViewModel.StartConfiguration()
                    {
                        DynamoModel = model
                    });

                var runner = new CommandLineRunner(viewModel);
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
