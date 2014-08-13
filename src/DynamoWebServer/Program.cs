using System;
using System.Windows;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo;

namespace DynamoWebServer
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
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

            var webSocketServer = new WebServer(viewModel,
                new WebSocket(), new SessionManager());

            webSocketServer.Start();

            var app = new Application();
            app.Run(view);
        }
    }
}
