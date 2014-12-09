using System;
using System.Linq;
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

            DynamoPathManager.PreloadAsmLibraries(DynamoPathManager.Instance);

            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    Preferences = PreferenceSettings.Load()
                });

            var webSocketServer = new WebServer(model, new WebSocket());

            webSocketServer.Start();

            var app = new Application();
            app.Exit += webSocketServer.ProcessExit;
            app.Run();
        }
    }
}
