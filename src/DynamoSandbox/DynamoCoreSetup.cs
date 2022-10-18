using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.DynamoSandbox;
using Dynamo.DynamoSandbox.Properties;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels.Watch3D;


namespace DynamoSandbox
{
    class DynamoCoreSetup
    {
        private Dynamo.DynamoSandbox.SplashScreen splashScreen;
        private DynamoViewModel viewModel = null;
        private readonly string commandFilePath;
        private readonly string CERLocation;
        private readonly Stopwatch startupTimer = Stopwatch.StartNew();
        private readonly string ASMPath;
        private readonly HostAnalyticsInfo analyticsInfo;
        private const string sandboxWikiPage = @"https://github.com/DynamoDS/Dynamo/wiki/How-to-Utilize-Dynamo-Builds";
        private DynamoView dynamoView;

        [DllImport("msvcrt.dll")]
        public static extern int _putenv(string env);

        public DynamoCoreSetup(string[] args)
        {
            var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
            var locale = StartupUtils.SetLocale(cmdLineArgs);
            _putenv(locale);

            if (cmdLineArgs.DisableAnalytics)
            {
                Analytics.DisableAnalytics = true;
            }

            if (!string.IsNullOrEmpty(cmdLineArgs.CERLocation))
            {
                CERLocation = cmdLineArgs.CERLocation;
            }

            commandFilePath = cmdLineArgs.CommandFilePath;
            ASMPath = cmdLineArgs.ASMPath;
            analyticsInfo = cmdLineArgs.AnalyticsInfo;
        }

        public void RunApplication(Application app)
        {
            try
            {
                DynamoModel.RequestUpdateLoadBarStatus += DynamoModel_RequestUpdateLoadBarStatus;

                splashScreen = new Dynamo.DynamoSandbox.SplashScreen();
                splashScreen.webView.NavigationCompleted += WebView_NavigationCompleted;
                splashScreen.RequestLaunchDynamo = LaunchDynamo;
                splashScreen.RequestImportSettings = ImportSettings;
                splashScreen.Show();

                app.Run();

                DynamoModel.RequestMigrationStatusDialog -= MigrationStatusDialogRequested;
                Dynamo.Applications.StartupUtils.ASMPreloadFailure -= ASMPreloadFailureHandler;
                splashScreen.webView.NavigationCompleted -= WebView_NavigationCompleted;
            }
            catch (DynamoServices.AssemblyBlockedException e)
            {
                var failureMessage = string.Format(Dynamo.Properties.Resources.CoreLibraryLoadFailureForBlockedAssembly, e.Message);
                Dynamo.Wpf.Utilities.MessageBoxService.Show(
                    failureMessage, Dynamo.Properties.Resources.CoreLibraryLoadFailureMessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
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
                    Analytics.TrackException(e, true);

                    if (viewModel != null)
                    {
                        // Show the unhandled exception dialog so user can copy the 
                        // crash details and report the crash if she chooses to.
                        viewModel.Model.OnRequestsCrashPrompt(null,
                            new CrashPromptArgs(e));

                        // Give user a chance to save (but does not allow cancellation)
                        viewModel.Exit(allowCancel: false);
                    }
                    else
                    {
                        //show a message dialog box with the exception so the user
                        //can effectively report the issue.
                        var shortStackTrace = String.Join(Environment.NewLine, e.StackTrace.Split(Environment.NewLine.ToCharArray()).Take(10));

                        var result = Dynamo.Wpf.Utilities.MessageBoxService.Show(e.Message +
                            $"  {Environment.NewLine} {e.InnerException?.Message} {Environment.NewLine} {shortStackTrace} {Environment.NewLine} " +
                             Environment.NewLine + string.Format(Resources.SandboxBuildsPageDialogMessage, sandboxWikiPage),
                             Resources.SandboxCrashMessage, MessageBoxButton.YesNo, MessageBoxImage.Error);

                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start(sandboxWikiPage);
                        }
                    }
                }
                catch
                {
                    // Do nothing for now.
                }

                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private async void ImportSettings(string fileContent)
        {
            var importResult = false;

            importResult = viewModel.PreferencesViewModel.importSettingsContent(fileContent);
            if (importResult)
            {
                splashScreen.SetImportStatus(ImportStatus.success, Resources.SplashScreenSettingsImported, string.Empty);
            }
            else
            {
                splashScreen.SetImportStatus(ImportStatus.error, Resources.SplashScreenFailedImportSettings, Resources.SplashScreenImportSettingsFailDescription);
            }
        }

        private void DynamoModel_RequestUpdateLoadBarStatus(SplashScreenLoadEventArgs args)
        {
            if(splashScreen != null)
            {
                splashScreen.SetBarProperties(Dynamo.Utilities.AssemblyHelper.GetDynamoVersion().ToString(),
                    args.LoadDescription, args.BarSize);
            }
        }

        private void LoadDynamoView()
        {
            DynamoModel model;
            Dynamo.Applications.StartupUtils.ASMPreloadFailure += ASMPreloadFailureHandler;

            model = Dynamo.Applications.StartupUtils.MakeModel(false, ASMPath ?? string.Empty, analyticsInfo);

            model.CERLocation = CERLocation;

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

            dynamoView = new DynamoView(viewModel);

            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Resources.SplashScreenLaunchingDynamo, 100));

            if (viewModel.PreferenceSettings.IsFirstRun || viewModel.PreferenceSettings.EnableStaticSplashScreen)
            {
                splashScreen.SetLoadingDone();
            }
            else
            {
                LaunchDynamo(false);
            }
        }

        private void LaunchDynamo(bool showStaticSplashScreenAgain)
        {
            viewModel.PreferenceSettings.EnableStaticSplashScreen = !showStaticSplashScreenAgain;
            splashScreen.Close();
            Application.Current.MainWindow = dynamoView;
            dynamoView.Show();
            dynamoView.Activate();
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            splashScreen.SetLabels();
            LoadDynamoView();
            splashScreen.webView.NavigationCompleted -= WebView_NavigationCompleted;
        }

        private void ASMPreloadFailureHandler(string failureMessage)
        {
            MessageBoxService.Show(failureMessage, "DynamoSandbox", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        void OnDynamoViewLoaded(object sender, RoutedEventArgs e)
        {
            CloseMigrationWindow();
            Analytics.TrackStartupTime("DynamoSandbox", startupTimer.Elapsed);
        }

        private void CloseMigrationWindow()
        {
            if (splashScreen == null)
                return;

            splashScreen.Close();
            splashScreen = null;
        }

        private void MigrationStatusDialogRequested(SettingsMigrationEventArgs args)
        {
            if (args.EventStatus == SettingsMigrationEventArgs.EventStatusType.Begin)
            {
                splashScreen = new Dynamo.DynamoSandbox.SplashScreen();
                splashScreen.ShowDialog();
            }
            else if (args.EventStatus == SettingsMigrationEventArgs.EventStatusType.End)
            {
                CloseMigrationWindow();
            }
        }

    }
}
