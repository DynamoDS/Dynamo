using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.DynamoSandbox.Properties;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace DynamoSandbox
{
    class DynamoCoreSetup
    {
        private Dynamo.UI.Views.SplashScreen splashScreen;

        private readonly string commandFilePath;
        private readonly string CERLocation;
        private readonly string ASMPath;
        private readonly HostAnalyticsInfo analyticsInfo;
        private readonly bool noNetworkMode;
        private const string sandboxWikiPage = @"https://github.com/DynamoDS/Dynamo/wiki/How-to-Utilize-Dynamo-Builds";
        private DynamoViewModel viewModel = null;

        [DllImport("msvcrt.dll")]
        public static extern int _putenv(string env);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args"></param>
        public DynamoCoreSetup(string[] args)
        {
            var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
            var locale = StartupUtils.SetLocale(cmdLineArgs);
            _putenv(locale);

            cmdLineArgs.SetDisableAnalytics();

            if (!string.IsNullOrEmpty(cmdLineArgs.CERLocation))
            {
                CERLocation = cmdLineArgs.CERLocation;
            }

            commandFilePath = cmdLineArgs.CommandFilePath;
            ASMPath = cmdLineArgs.ASMPath;
            analyticsInfo = cmdLineArgs.AnalyticsInfo;
            noNetworkMode = cmdLineArgs.NoNetworkMode;
        }

        public void RunApplication(Application app)
        {
            try
            {
                Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // This line validates if the WebView2 Runtime is installed in the computer before launching DynamoSandbox,
                // if is not we return and then exit Dynamo Sandbox
                if (!WebView2Utilities.ValidateWebView2RuntimeInstalled())
                    return;
                StartupUtils.ASMPreloadFailure += ASMPreloadFailureHandler;

                splashScreen = new Dynamo.UI.Views.SplashScreen();
                splashScreen.DynamicSplashScreenReady += LoadDynamoView;
                splashScreen.Show();
                app.Run();

                StartupUtils.ASMPreloadFailure -= ASMPreloadFailureHandler;
            }
            catch (DynamoServices.AssemblyBlockedException e)
            {
                var failureMessage = string.Format(Dynamo.Properties.Resources.CoreLibraryLoadFailureForBlockedAssembly, e.Message);
                MessageBoxService.Show(
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

                        var result = MessageBoxService.Show(e.Message +
                            $"  {Environment.NewLine} {e.InnerException?.Message} {Environment.NewLine} {shortStackTrace} {Environment.NewLine} " +
                             Environment.NewLine + string.Format(Resources.SandboxBuildsPageDialogMessage, sandboxWikiPage),
                             Resources.SandboxCrashMessage, MessageBoxButton.YesNo, MessageBoxImage.Error);

                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo(sandboxWikiPage) { UseShellExecute = true });
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

        private void LoadDynamoView()
        {
            DynamoModel model;
            model = StartupUtils.MakeModel(false, noNetworkMode, ASMPath ?? string.Empty, analyticsInfo);
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

            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Dynamo.Wpf.Properties.Resources.SplashScreenLaunchingDynamo, 70));
            splashScreen.DynamoView = new DynamoView(viewModel);
            splashScreen.OnRequestStaticSplashScreen();

            splashScreen.DynamicSplashScreenReady -= LoadDynamoView;
            Analytics.TrackStartupTime("DynamoSandbox", TimeSpan.FromMilliseconds(splashScreen.totalLoadingTime));
        }

        private void ASMPreloadFailureHandler(string failureMessage)
        {
            MessageBoxService.Show(failureMessage, "DynamoSandbox", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            e.Handled = true;
            CrashGracefully(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            CrashGracefully(ex);
        }

        private void CrashGracefully(Exception ex)
        {
            try
            {
                viewModel?.Model?.Logger?.LogError($"Unhandled exception {ex.Message}");

                DynamoModel.IsCrashing = true;
                Analytics.TrackException(ex, true);
                CrashReportTool.ShowCrashErrorReportWindow(viewModel, new Dynamo.Core.CrashErrorReportArgs(ex));
            }
            catch
            { }

            viewModel?.Exit(false); // don't allow cancellation
        }
    }
}
