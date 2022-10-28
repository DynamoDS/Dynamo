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
        private IDSDKManager idsdkManager;

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
                idsdkManager = new IDSDKManager();

                splashScreen = new Dynamo.DynamoSandbox.SplashScreen();
                splashScreen.webView.NavigationCompleted += WebView_NavigationCompleted;
                splashScreen.RequestLaunchDynamo = LaunchDynamo;
                splashScreen.RequestImportSettings = ImportSettings;
                splashScreen.RequestSignIn = SignIn;
                splashScreen.RequestSignOut = SignOut;
                splashScreen.Show();

                app.Run();

                DynamoModel.RequestMigrationStatusDialog -= MigrationStatusDialogRequested;
                Dynamo.Applications.StartupUtils.ASMPreloadFailure -= ASMPreloadFailureHandler;
                // WebView2 could be null at this moment to prevent crash
                if (splashScreen.webView != null)
                {
                    splashScreen.webView.NavigationCompleted -= WebView_NavigationCompleted;
                }
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
                        viewModel.Model.OnRequestsCrashPrompt(new CrashErrorReportArgs(e));

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

        /// <summary>
        /// Import setting file from chosen path
        /// </summary>
        /// <param name="fileContent"></param>
        private async void ImportSettings(string fileContent)
        {
            if (viewModel.PreferencesViewModel.importSettingsContent(fileContent))
            {
                splashScreen.SetImportStatus(ImportStatus.success, Resources.SplashScreenSettingsImported, string.Empty);
            }
            else
            {
                splashScreen.SetImportStatus(ImportStatus.error, Resources.SplashScreenFailedImportSettings, Resources.SplashScreenImportSettingsFailDescription);
            }
        }

        private bool SignIn(bool status)
        {
            if (!status) return idsdkManager.IsLoggedIn();
            bool ret = idsdkManager.Login();
            return ret;
        }

        private bool SignOut()
        {
            bool ret = idsdkManager.Logout();
            return ret;
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

            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Resources.SplashScreenLaunchingDynamo, 70));
            dynamoView = new DynamoView(viewModel);

            // If user lauching Dynamo first time or picked to always show splash screen, display it. Otherwise, display Dynamo view directly.
            if (viewModel.PreferenceSettings.IsFirstRun || viewModel.PreferenceSettings.EnableStaticSplashScreen)
            {
                splashScreen.SetSignInStatus(idsdkManager.IsLoggedIn());
                splashScreen.SetLoadingDone(idsdkManager.IsLoggedIn());
            }
            else
            {
                LaunchDynamo(true);
            }
        }

        private void LaunchDynamo(bool isCheckboxChecked)
        {
            viewModel.PreferenceSettings.EnableStaticSplashScreen = !isCheckboxChecked;
            splashScreen.Close();
            Application.Current.MainWindow = dynamoView;
            dynamoView.Show();
            dynamoView.Activate();
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            splashScreen.SetLabels(idsdkManager.IsLoggedIn());
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
