using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.SelfHostAPI;
using Dynamo.Utilities;

namespace DynamoServiceSandbox
{
    static class Program
    {
        private static ManualResetEvent waitHandle = new ManualResetEvent(false);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Running Dynamo sandbox with a command file:
                // DynamoSandbox.exe /c "C:\file path\file.xml"
                // 
                string commandFilePath = string.Empty;
                bool enableServer = false;

                for (int i = 0; i < args.Length; ++i)
                {
                    string arg = args[i];

                    if (arg.ToLower() == "/server")
                    {
                        enableServer = true;
                        continue;
                    }

                    // Looking for '/c'
                    if (arg.Length != 2 || (arg[0] != '/'))
                        continue;

                    if (arg[1] == 'c' || (arg[1] == 'C'))
                    {
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[i + 1];
                    }
                }

                DynamoController.MakeSandbox(commandFilePath);
                if (enableServer)
                {
                    WebApiServer.Run();
                    dynSettings.EnableServer();
                }

                waitHandle.WaitOne();
            }
            catch (Exception e)
            {
#if DEBUG
                // Display the recorded command XML when the crash happens, so that it maybe saved and re-run later
                dynSettings.Controller.DynamoViewModel.SaveRecordedCommand.Execute(null);
#endif

                try
                {
                    dynSettings.Controller.IsCrashing = true;
                    // Show the unhandled exception dialog so user can copy the 
                    // crash details and report the crash if she chooses to.
                    dynSettings.Controller.OnRequestsCrashPrompt(null,
                        new CrashPromptArgs(e.Message + "\n\n" + e.StackTrace));

                    // Give user a chance to save (but does not allow cancellation)
                    bool allowCancellation = false;
                    dynSettings.Controller.DynamoViewModel.Exit(allowCancellation);
                }
                catch
                {
                }

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                ((DynamoLogger)dynSettings.DynamoLogger).Dispose();
            }
        }
    }
}
