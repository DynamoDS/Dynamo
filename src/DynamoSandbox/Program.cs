using System;
using System.Diagnostics;
using System.Windows;
using Dynamo;
using Dynamo.Controls;
using Dynamo.ViewModels;

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

                var app = new Application();

                //create the view
                var ui = new DynamoView();
                ui.DataContext = controller.DynamoViewModel;
                controller.UIDispatcher = ui.Dispatcher;

                app.Run(ui);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
