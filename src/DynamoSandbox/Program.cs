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

                var app = new Application();

                var converters = new ResourceDictionary
                    {
                        Source = new Uri("/DynamoElements;component/UI/Themes/DynamoConverters.xaml", UriKind.Relative)
                    };
                app.Resources.MergedDictionaries.Add(converters);

                var colors = new ResourceDictionary
                    {
                        Source =
                            new Uri("/DynamoElements;component/UI/Themes/DynamoColorsAndBrushes.xaml", UriKind.Relative)
                    };
                app.Resources.MergedDictionaries.Add(colors);

                var modern = new ResourceDictionary
                    {
                        Source = new Uri("/DynamoElements;component/UI/Themes/DynamoModern.xaml", UriKind.Relative)
                    };
                app.Resources.MergedDictionaries.Add(modern);

                var text = new ResourceDictionary
                    {
                        Source = new Uri("/DynamoElements;component/UI/Themes/DynamoText.xaml", UriKind.Relative)
                    };
                app.Resources.MergedDictionaries.Add(text);

                //Application.LoadComponent(new Uri("/DynamoElements;component/UI/Themes/DynamoColorsAndBrushes.xaml", UriKind.Relative));
                //Application.LoadComponent(new Uri("/DynamoElements;component/UI/Themes/DynamoModern.xaml", UriKind.Relative));
                //Application.LoadComponent(new Uri("/DynamoElements;component/UI/Themes/DynamoText.xaml", UriKind.Relative));
                //Application.LoadComponent(new Uri("/DynamoElements;component/UI/Themes/DynamoConverters.xaml",UriKind.Relative));

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
