using System;
using System.Diagnostics;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;

namespace DynamoSandbox
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            DynamoView.DynamoApp app = null;

            try
            {
                // Running Dynamo sandbox with a command file:
                // DynamoSandbox.exe /c "C:\file path\file.xml"
                // 
                string commandFilePath = string.Empty;
                for (int i = 0; i < args.Length; ++i)
                {
                    // Looking for '/c'
                    string arg = args[i];
                    if (arg.Length != 2 || (arg[0] != '/'))
                        continue;

                    if (arg[1] == 'c' || (arg[1] == 'C'))
                    {
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[i + 1];
                    }
                }

                app = DynamoView.MakeStandaloneAndRun(commandFilePath);
            }
            catch (Exception e)
            {

                try
                {
#if DEBUG
                    // Display the recorded command XML when the crash happens, so that it maybe saved and re-run later
                    app.ViewModel.SaveRecordedCommand.Execute(null);
#endif

                    DynamoModel.IsCrashing = true;
                    InstrumentationLogger.LogException(e);
                    StabilityTracking.GetInstance().NotifyCrash();

                    // Show the unhandled exception dialog so user can copy the 
                    // crash details and report the crash if she chooses to.
                    app.ViewModel.Model.OnRequestsCrashPrompt(null,
                        new CrashPromptArgs(e.Message + "\n\n" + e.StackTrace));

                    // Give user a chance to save (but does not allow cancellation)
                    bool allowCancellation = false;
                    app.ViewModel.Exit(allowCancellation);
                }
                catch
                {
                }

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

        }
    }
}
