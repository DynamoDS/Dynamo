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

            if (DynamoPathManager.Instance.FindAndSetASMHostPath())
            {
                if (DynamoPathManager.Instance.ASM219Host == null)
                {
                    DynamoPathManager.Instance.SetLibGPath("libg_220");
                    DynamoPathManager.Instance.ASMVersion = DynamoPathManager.Asm.Version220;
                }

                var libG = Assembly.LoadFrom(DynamoPathManager.Instance.AsmPreloader);

                Type preloadType = libG.GetType("Autodesk.LibG.AsmPreloader");

                MethodInfo preloadMethod = preloadType.GetMethod("PreloadAsmLibraries",
                    BindingFlags.Public | BindingFlags.Static);

                object[] methodParams = new object[1];

                if (DynamoPathManager.Instance.ASM219Host == null)
                    methodParams[0] = DynamoPathManager.Instance.ASM220Host;
                else
                    methodParams[0] = DynamoPathManager.Instance.ASM219Host;

                preloadMethod.Invoke(null, methodParams);
            }

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
