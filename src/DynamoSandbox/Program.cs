using System;
using System.Diagnostics;
using System.Windows;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace DynamoSandbox
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            DynamoLogger.Instance.StartLogging();

            try
            {
                var controller = new DynamoController(new Dynamo.FSchemeInterop.ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE);

                //create the view
                dynSettings.Bench = new DynamoView();
                dynSettings.Bench.DataContext = controller.DynamoViewModel;
                controller.UIDispatcher = dynSettings.Bench.Dispatcher;

                var app = new Application();
                app.Run(dynSettings.Bench);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
