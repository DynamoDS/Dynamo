using System;
using System.IO;
using System.Reflection;
using System.Windows;

using DynamoUtilities;

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

                var methodParams = new object[1];

                if (DynamoPathManager.Instance.ASM219Host == null)
                    methodParams[0] = DynamoPathManager.Instance.ASM220Host;
                else
                    methodParams[0] = DynamoPathManager.Instance.ASM219Host;

                preloadMethod.Invoke(null, methodParams);
            }

            var webSocketServer = new WebServer(new WebSocket(), new SessionManager());
            webSocketServer.Start();

            var app = new Application();
            app.Run();
        }
    }
}
