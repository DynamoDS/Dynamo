using System;
using System.Windows;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo;
using DynamoUtilities;
using System.IO;
using System.Reflection;

namespace DynamoWebServer
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            DynamoPathManager.Instance.InitializeCore(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            DynamoPathManager.Instance.PreloadASMLibraries();

            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    Preferences = PreferenceSettings.Load()
                });

            var viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    CommandFilePath = null,
                    DynamoModel = model
                });

            // Comment out if we do not need a view.
            var view = new DynamoView(viewModel);

            var webSocketServer = new WebServer(viewModel, new WebSocket());

            webSocketServer.Start();

            var app = new Application();
            app.Exit += webSocketServer.ProcessExit;
            app.Run(view);
        }
    }
}
