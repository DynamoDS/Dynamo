#region
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using System.Windows.Threading;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using Dynamo.Applications.Models;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoUnits;

using DynamoUtilities;

using RevitServices.Persistence;
using RevitServices.Transactions;
using RevitServices.Threading;

using MessageBox = System.Windows.Forms.MessageBox;

#endregion

namespace Dynamo.Applications
{
    [Transaction(TransactionMode.Manual),
     Regeneration(RegenerationOption.Manual)]
    public class DynamoRevit : IExternalCommand
    {
        private static DynamoViewModel dynamoViewModel;
        private static RevitDynamoModel revitDynamoModel;
        private static bool handledCrash;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            HandleDebug(commandData);
            
            InitializeCore(commandData);

            try
            {
                IdlePromise.ExecuteOnIdleAsync(
                    delegate
                    {
                        // create core data models
                        revitDynamoModel = InitializeCoreModel(commandData);
                        dynamoViewModel = InitializeCoreViewModel(revitDynamoModel);

                        // show the window
                        InitializeCoreView().Show();

                        TryOpenWorkspaceInCommandData(commandData);
                        SubscribeViewActivating(commandData);
                    });

                // Disable the Dynamo button to prevent a re-run
                DynamoRevitApp.DynamoButton.Enabled = false;
            }
            catch (Exception ex)
            {
                // notify instrumentation
                InstrumentationLogger.LogException(ex);
                StabilityTracking.GetInstance().NotifyCrash();

                MessageBox.Show(ex.ToString());

                DynamoRevitApp.DynamoButton.Enabled = true;

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        #region Initialization

        private static RevitDynamoModel InitializeCoreModel(ExternalCommandData commandData)
        {
            var prefs = PreferenceSettings.Load();
            var corePath =
                Path.GetFullPath(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\");

#if !ENABLE_DYNAMO_SCHEDULER

            return RevitDynamoModel.Start(
                new RevitDynamoModel.StartConfiguration()
                {
                    Preferences = prefs,
                    DynamoCorePath = corePath,
                    Context = GetRevitContext(commandData)
                });

#else

            return RevitDynamoModel.Start(
                new RevitDynamoModel.StartConfiguration()
                {
                    Preferences = prefs,
                    DynamoCorePath = corePath,
                    Context = GetRevitContext(commandData),
                    SchedulerThread = new RevitSchedulerThread(commandData.Application)
                });

#endif
        }

        private static DynamoViewModel InitializeCoreViewModel(RevitDynamoModel revitDynamoModel)
        {
            var vizManager = new RevitVisualizationManager(revitDynamoModel);

            var viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = revitDynamoModel,
                    VisualizationManager = vizManager,
                    WatchHandler =
                        new RevitWatchHandler(vizManager, revitDynamoModel.PreferenceSettings)
                });

            viewModel.RequestAuthentication +=
                 SingleSignOnManager.RegisterSingleSignOn;

            revitDynamoModel.ShuttingDown += (drm) =>
                IdlePromise.ExecuteOnShutdown(
                    delegate
                    {
                        if (null != DocumentManager.Instance.CurrentDBDocument)
                        {
                            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

                            var keeperId = vizManager.KeeperId;

                            if (keeperId != ElementId.InvalidElementId)
                            {
                                DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                            }

                            TransactionManager.Instance.ForceCloseTransaction();
                        }
                    });

            return viewModel;
        }

        private static DynamoView InitializeCoreView()
        {
            IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;
            var dynamoView = new DynamoView(dynamoViewModel);
            new WindowInteropHelper(dynamoView).Owner = mwHandle;

            handledCrash = false;

            dynamoView.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            dynamoView.Closing += DynamoView_Closing;
            dynamoView.Closed += DynamoView_Closed;

            SingleSignOnManager.UIDispatcher = dynamoView.Dispatcher;

            return dynamoView;
        }

        private static bool initializedCore;
        private static void InitializeCore(ExternalCommandData commandData)
        {
            if (initializedCore) return;

            IdlePromise.RegisterIdle(commandData.Application);
            InitializeAssemblies();
            InitializeUnits();
            InitializeDocumentManager(commandData);
            InitializeMigrationManager();

            initializedCore = true;
        }

        private static bool registeredViewActivating;
        private static void SubscribeViewActivating(ExternalCommandData commandData)
        {
            if (registeredViewActivating) return;
            commandData.Application.ViewActivating += Application_ViewActivating;
            registeredViewActivating = true;
        }

        private static void InitializeMigrationManager()
        {
            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrationsRevit));
        }

        public static void InitializeUnits()
        {
            // set revit units
            BaseUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
            BaseUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
            BaseUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;
        }

        public static void InitializeAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                Analyze.Render.AssemblyHelper.ResolveAssemblies;

            //Add an assembly load step for the System.Windows.Interactivity assembly
            //Revit owns a version of this as well. Adding our step here prevents a duplicative
            //load of the dll at a later time.
            string interactivityPath = Path.Combine(
                DynamoPathManager.Instance.MainExecPath,
                "System.Windows.Interactivity.dll");

            if (File.Exists(interactivityPath))
                Assembly.LoadFrom(interactivityPath);
        }

        private static void InitializeDocumentManager(ExternalCommandData commandData)
        {
            if (DocumentManager.Instance.CurrentUIApplication == null)
                DocumentManager.Instance.CurrentUIApplication = commandData.Application;
        }

        #endregion

        #region Helpers

        private void HandleDebug(ExternalCommandData commandData)
        {
            if (commandData.JournalData != null && commandData.JournalData.ContainsKey("debug"))
            {
                if (Boolean.Parse(commandData.JournalData["debug"]))
                    Debugger.Launch();
            }
        }

        private static void TryOpenWorkspaceInCommandData(ExternalCommandData commandData)
        {

            if (commandData.JournalData != null && commandData.JournalData.ContainsKey("dynPath"))
                dynamoViewModel.Model.OpenWorkspace(commandData.JournalData["dynPath"]);
        }

        private static string GetRevitContext(ExternalCommandData commandData)
        {
            var r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
            string context = r.Replace(commandData.Application.Application.VersionName, "");

            //they changed the application version name conventions for vasari
            //it no longer has a version year so we can't compare it to other versions
            if (context == "Vasari")
                context = "Vasari 2014";

            return context;
        }

        #endregion

        #region View Activation

        /// <summary>
        ///     Handler for Revit's ViewActivating event.
        ///     Addins are not available in some views in Revit, notably perspective views.
        ///     This will present a warning that Dynamo is not available to run and disable the run button.
        ///     This handler is called before the ViewActivated event registered on the RevitDynamoModel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ViewActivating(object sender, ViewActivatingEventArgs e)
        {
            revitDynamoModel.SetRunEnabledBasedOnContext(e.NewActiveView);
        }

        #endregion

        #region Exception

        /// <summary>
        ///     A method to deal with unhandle exceptions.  Executes right before Revit crashes.
        ///     Dynamo is still valid at this time, but further work may cause corruption.  Here,
        ///     we run the ExitCommand, allowing the user to save all of their work.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Info about the exception</param>
        private static void Dispatcher_UnhandledException(
            object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;

            // only handle a single crash per Dynamo sesh, this should be reset in the initial command
            if (handledCrash)
                return;

            handledCrash = true;

            string exceptionMessage = args.Exception.Message;

            try
            {
                InstrumentationLogger.LogException(args.Exception);
                StabilityTracking.GetInstance().NotifyCrash();

                revitDynamoModel.Logger.LogError("Dynamo Unhandled Exception");
                revitDynamoModel.Logger.LogError(exceptionMessage);
            }
            catch { }

            try
            {
                DynamoModel.IsCrashing = true;
                revitDynamoModel.OnRequestsCrashPrompt(
                    revitDynamoModel,
                    new CrashPromptArgs(args.Exception.Message + "\n\n" + args.Exception.StackTrace));
                dynamoViewModel.Exit(false); // don't allow cancellation
            }
            catch { }
            finally
            {
                args.Handled = true;

                // KILLDYNSETTINGS - this is suspect
                revitDynamoModel.Logger.Dispose();
                DynamoRevitApp.DynamoButton.Enabled = true;
            }
        }

        #endregion

        #region Shutdown

        /// <summary>
        ///     Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DynamoView_Closing(object sender, EventArgs e)
        {
            IdlePromise.ClearPromises();
            IdlePromise.Shutdown();
        }

        /// <summary>
        ///     Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DynamoView_Closed(object sender, EventArgs e)
        {
            var view = (DynamoView)sender;

            revitDynamoModel.RevitServicesUpdater.Dispose();
            DocumentManager.OnLogError -= revitDynamoModel.Logger.Log;

            view.Dispatcher.UnhandledException -= Dispatcher_UnhandledException;
            view.Closing -= DynamoView_Closing;
            view.Closed -= DynamoView_Closed;
            DocumentManager.Instance.CurrentUIApplication.ViewActivating -=
                Application_ViewActivating;

            AppDomain.CurrentDomain.AssemblyResolve -=
                Analyze.Render.AssemblyHelper.ResolveAssemblies;

            // KILLDYNSETTINGS - this is suspect
            revitDynamoModel.Logger.Dispose();

            DynamoRevitApp.DynamoButton.Enabled = true;
        }

        #endregion
    }
}
