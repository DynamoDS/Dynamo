using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.ViewModels;

using DynamoUtilities;

namespace DynamoSandbox
{
    class Program
    {
        private static DynamoViewModel MakeStandaloneAndRun(string commandFilePath)
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
                    CommandFilePath = commandFilePath,
                    DynamoModel = model
                });

            var view = new DynamoView(viewModel);

            var app = new Application();
            app.Run(view);

            return viewModel;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            DynamoViewModel viewModel = null;

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

                viewModel = MakeStandaloneAndRun(commandFilePath);
            }
            catch (Exception e)
            {

                try
                {
#if DEBUG
                    // Display the recorded command XML when the crash happens, so that it maybe saved and re-run later
                    viewModel.SaveRecordedCommand.Execute(null);
#endif

                    DynamoModel.IsCrashing = true;
                    InstrumentationLogger.LogException(e);
                    StabilityTracking.GetInstance().NotifyCrash();

                    // Show the unhandled exception dialog so user can copy the 
                    // crash details and report the crash if she chooses to.
                    viewModel.Model.OnRequestsCrashPrompt(null,
                        new CrashPromptArgs(e.Message + "\n\n" + e.StackTrace));

                    // Give user a chance to save (but does not allow cancellation)
                    bool allowCancellation = false;
                    viewModel.Exit(allowCancellation);
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
