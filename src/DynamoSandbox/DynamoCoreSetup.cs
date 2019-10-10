using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.DynamoSandbox;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using System.Linq;
using Dynamo.DynamoSandbox.Properties;

namespace DynamoSandbox
{
    class DynamoCoreSetup
    {
        private SettingsMigrationWindow migrationWindow;
        private DynamoViewModel viewModel = null;
        private string commandFilePath;
        private Stopwatch startupTimer = Stopwatch.StartNew();
        private string ASMPath;
        private const string sandboxWikiPage = @"https://github.com/DynamoDS/Dynamo/wiki/How-to-Utilize-Dynamo-Builds";

        [DllImport("msvcrt.dll")]
        public static extern int _putenv(string env);

        public DynamoCoreSetup(string[] args)
        {
            var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
            var locale = StartupUtils.SetLocale(cmdLineArgs);
            _putenv(locale);
            commandFilePath = cmdLineArgs.CommandFilePath;
            ASMPath = cmdLineArgs.ASMPath;
        }

        public void RunApplication(Application app)
        {
            try
            {
                DynamoModel.RequestMigrationStatusDialog += MigrationStatusDialogRequested;
                DynamoModel model;
                Dynamo.Applications.StartupUtils.ASMPreloadFailure += ASMPreloadFailureHandler;
                if (!String.IsNullOrEmpty(ASMPath))
                {
                    model = Dynamo.Applications.StartupUtils.MakeModel(false,ASMPath);
                }
                else
                {
                    model = Dynamo.Applications.StartupUtils.MakeModel(false);
                }
                
                viewModel = DynamoViewModel.Start(
                    new DynamoViewModel.StartConfiguration()
                    {
                        CommandFilePath = commandFilePath,
                        DynamoModel = model,
                        Watch3DViewModel = 
                            HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(
                                null,
                                new Watch3DViewModelStartupParams(model), 
                                model.Logger),
                        ShowLogin = true
                    });

                var view = new DynamoView(viewModel);
                view.Loaded += OnDynamoViewLoaded;

                app.Run(view);

                DynamoModel.RequestMigrationStatusDialog -= MigrationStatusDialogRequested;
                Dynamo.Applications.StartupUtils.ASMPreloadFailure -= ASMPreloadFailureHandler;

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
                    Dynamo.Logging.Analytics.TrackException(e, true);

                    if (viewModel != null)
                    {
                        // Show the unhandled exception dialog so user can copy the 
                        // crash details and report the crash if she chooses to.
                        viewModel.Model.OnRequestsCrashPrompt(null,
                            new CrashPromptArgs(e.Message + "\n\n" + e.StackTrace));

                        // Give user a chance to save (but does not allow cancellation)
                        viewModel.Exit(allowCancel: false);
                    }
                    else
                    {
                        //show a message dialog box with the exception so the user
                        //can effectively report the issue.
                        var shortStackTrace = String.Join(Environment.NewLine,e.StackTrace.Split(Environment.NewLine.ToCharArray()).Take(10));

                        var result = MessageBox.Show($"{Resources.SandboxCrashMessage} {Environment.NewLine} {e.Message}" +
                            $"  {Environment.NewLine} {e.InnerException?.Message} {Environment.NewLine} {shortStackTrace} {Environment.NewLine} " +
                             Environment.NewLine + string.Format(Resources.SandboxBuildsPageDialogMessage, sandboxWikiPage),

                            "DynamoSandbox",
                            MessageBoxButton.YesNo,MessageBoxImage.Error);

                        if(result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(sandboxWikiPage);
                        }
                    }
                }
                catch {
                }

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void ASMPreloadFailureHandler(string failureMessage)
        {
            MessageBox.Show(failureMessage, "DynamoSandbox", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        void OnDynamoViewLoaded(object sender, RoutedEventArgs e)
        {
            CloseMigrationWindow();
            Analytics.TrackStartupTime("DynamoSandbox", startupTimer.Elapsed);
        }

        private void CloseMigrationWindow()
        {
            if (migrationWindow == null)
                return;

            migrationWindow.Close();
            migrationWindow = null;
        }

        private void MigrationStatusDialogRequested(SettingsMigrationEventArgs args)
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

    }
}
