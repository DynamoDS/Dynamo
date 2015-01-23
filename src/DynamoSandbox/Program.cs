using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dynamo;
using Dynamo.Applications.Authentication;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Services;
using Dynamo.ViewModels;
using DynamoUtilities;
using Greg;
using Greg.AuthProviders;

namespace DynamoSandbox
{
    internal class Program
    {
        private static void MakeStandaloneAndRun(string commandFilePath, ref DynamoViewModel viewModel)
        {
            // authentication
            var url = ConfigurationManager.AppSettings["authAddress"];
            var authProvider = new OxygenProvider(url);
            var loginService = new LoginService(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            authProvider.RequestLogin += loginService.ShowLogin;

            DynamoPathManager.Instance.InitializeCore(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            DynamoPathManager.PreloadAsmLibraries(DynamoPathManager.Instance);

            var model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    Preferences = PreferenceSettings.Load(),
                    AuthProvider = authProvider
                });

            viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    CommandFilePath = commandFilePath,
                    DynamoModel = model
                });

            var view = new DynamoView(viewModel);

            var app = new Application();
            app.Run(view);
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
                var commandFilePath = string.Empty;
                for (var i = 0; i < args.Length; ++i)
                {
                    // Looking for '/c'
                    var arg = args[i];
                    if (arg.Length != 2 || (arg[0] != '/'))
                        continue;

                    if (arg[1] == 'c' || (arg[1] == 'C'))
                    {
                        // If there's at least one more argument...
                        if (i < args.Length - 1)
                            commandFilePath = args[i + 1];
                    }
                }

                MakeStandaloneAndRun(commandFilePath, ref viewModel);
            }
            catch (Exception e)
            {
                try
                {
#if DEBUG
                    // Display the recorded command XML when the crash happens, 
                    // so that it maybe saved and re-run later
                    if (viewModel != null)
                        viewModel.SaveRecordedCommand.Execute(null);
#endif

                    DynamoModel.IsCrashing = true;
                    InstrumentationLogger.LogException(e);
                    StabilityTracking.GetInstance().NotifyCrash();

                    if (viewModel != null)
                    {
                        // Show the unhandled exception dialog so user can copy the 
                        // crash details and report the crash if she chooses to.
                        viewModel.Model.OnRequestsCrashPrompt(null,
                            new CrashPromptArgs(e.Message + "\n\n" + e.StackTrace));

                        // Give user a chance to save (but does not allow cancellation)
                        viewModel.Exit(allowCancel: false);
                    }
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