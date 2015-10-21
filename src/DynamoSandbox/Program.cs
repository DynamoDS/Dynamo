using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.DynamoSandbox;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.ViewModels;
using Dynamo.Applications;
using Dynamo.Logging;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace DynamoSandbox
{
   
    internal class Program
    {
        private static SettingsMigrationWindow migrationWindow;
        
        private static void MakeStandaloneAndRun(string commandFilePath, out DynamoViewModel viewModel)
        {
            var model = Dynamo.Applications.StartupUtils.MakeModel(false);
            DynamoModel.RequestMigrationStatusDialog += MigrationStatusDialogRequested;

            viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    CommandFilePath = commandFilePath,
                    DynamoModel = model,
                    Watch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(new Watch3DViewModelStartupParams(model), model.Logger)
                });

            var view = new DynamoView(viewModel);
            view.Loaded += (sender, args) => CloseMigrationWindow();

            var app = new Application();
            app.Run(view);

            DynamoModel.RequestMigrationStatusDialog -= MigrationStatusDialogRequested;
        }

        private static void CloseMigrationWindow()
        {
            if (migrationWindow == null)
                return;

            migrationWindow.Close();
            migrationWindow = null;
        }

        private static void MigrationStatusDialogRequested(SettingsMigrationEventArgs args)
        {
            if (args.EventStatus == SettingsMigrationEventArgs.EventStatusType.Begin)
            {
                migrationWindow = new SettingsMigrationWindow();
                migrationWindow.Show();
            }
            else if (args.EventStatus == SettingsMigrationEventArgs.EventStatusType.End)
            {
                CloseMigrationWindow();
            }
        }


        [DllImport("msvcrt.dll")]
        public static extern int _putenv(string env);

        [STAThread]
        public static void Main(string[] args)
        {
            DynamoViewModel viewModel = null;
            try
            {
                var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
                var locale = Dynamo.Applications.StartupUtils.SetLocale(cmdLineArgs);
                    _putenv(locale);

                    MakeStandaloneAndRun(cmdLineArgs.CommandFilePath, out viewModel);
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
