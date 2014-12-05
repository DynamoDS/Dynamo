using System.Linq;

using Dynamo.Applications;

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
using Dynamo.Core.Threading;
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
using Autodesk.Revit.DB.Events;

#endregion

#if ENABLE_DYNAMO_SCHEDULER

namespace RevitServices.Threading
{
    // SCHEDULER: This class will be removed once DynamoScheduler work is 
    // tested working. When that happens, all the callers will be redirected
    // to use RevitDynamoModel.DynamoScheduler directly for task scheduling.
    // 
    public static class IdlePromise
    {
        [ThreadStatic]
        private static bool idle;
        public static bool InIdleThread
        {
            get { return idle; }
            set { idle = value; }
        }

        /// <summary>
        /// Call this method to schedule a DelegateBasedAsyncTask for execution.
        /// </summary>
        /// <param name="p">The delegate to execute on the idle thread.</param>
        /// <param name="completionHandler">Event handler that will be invoked 
        /// when the scheduled DelegateBasedAsyncTask is completed. This parameter
        /// is optional.</param>
        /// 
        internal static void ExecuteOnIdleAsync(DynamoScheduler scheduler, Action p,
            AsyncTaskCompletedHandler completionHandler = null)
        {
            var task = new DelegateBasedAsyncTask(scheduler);

            task.Initialize(p);
            if (completionHandler != null)
                task.Completed += completionHandler;

            scheduler.ScheduleForExecution(task);
        }
    }
}

#endif

namespace Dynamo.Applications
{
    [Transaction(TransactionMode.Manual),
     Regeneration(RegenerationOption.Manual)]
    public class DynamoRevit : IExternalCommand
    {
        private static ExternalCommandData extCommandData;
        private static DynamoViewModel dynamoViewModel;
        private static RevitDynamoModel revitDynamoModel;
        private static bool handledCrash;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            HandleDebug(commandData);
            
            InitializeCore(commandData);

            try
            {
#if ENABLE_DYNAMO_SCHEDULER
                extCommandData = commandData;

                // create core data models
                revitDynamoModel = InitializeCoreModel(extCommandData);
                dynamoViewModel = InitializeCoreViewModel(revitDynamoModel);

                // handle initialization steps after RevitDynamoModel is created.
                revitDynamoModel.HandlePostInitialization();

                // show the window
                InitializeCoreView().Show();

                TryOpenWorkspaceInCommandData(extCommandData);
                SubscribeApplicationEvents(extCommandData);
#else

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
#endif
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

        public static RevitDynamoModel RevitDynamoModel
        {
            get
            {
                return revitDynamoModel;
            }
            set
            {
                revitDynamoModel = value;
            }
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

            revitDynamoModel.PackageManagerClient.RequestAuthentication +=
                 SingleSignOnManager.RegisterSingleSignOn;

#if ENABLE_DYNAMO_SCHEDULER

            revitDynamoModel.ShutdownStarted += (drm) =>
            {
                var uiApplication = DocumentManager.Instance.CurrentUIApplication;
                uiApplication.Idling += DeleteKeeperElementOnce;
            };

#else

            revitDynamoModel.ShutdownStarted += (drm) =>
                IdlePromise.ExecuteOnShutdown(DeleteKeeperElement);

#endif
            return viewModel;
        }

        private static DynamoView InitializeCoreView()
        {
            IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;
            var dynamoView = new DynamoView(dynamoViewModel);
            new WindowInteropHelper(dynamoView).Owner = mwHandle;

            handledCrash = false;

            dynamoView.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            dynamoView.Closed += OnDynamoViewClosed;

            SingleSignOnManager.UIDispatcher = dynamoView.Dispatcher;

            return dynamoView;
        }

        private static bool initializedCore;
        private static void InitializeCore(ExternalCommandData commandData)
        {
            if (initializedCore) return;

#if !ENABLE_DYNAMO_SCHEDULER
            IdlePromise.RegisterIdle(commandData.Application);
#endif

            InitializeAssemblies();
            InitializeUnits();
            InitializeDocumentManager(commandData);
            InitializeMigrationManager();

            initializedCore = true;
        }

        private static bool hasRegisteredApplicationEvents;
        private static void SubscribeApplicationEvents(ExternalCommandData commandData)
        {
            if (hasRegisteredApplicationEvents)
            {
                return;
            }

            commandData.Application.ViewActivating += OnApplicationViewActivating;
            commandData.Application.ViewActivated += OnApplicationViewActivated;

            commandData.Application.Application.DocumentClosing += OnApplicationDocumentClosing;
            commandData.Application.Application.DocumentClosed += OnApplicationDocumentClosed;
            commandData.Application.Application.DocumentOpened += OnApplicationDocumentOpened;

            hasRegisteredApplicationEvents = true;
        }

        private static void InitializeMigrationManager()
        {
            MigrationManager.Instance.MigrationTargets.Add(typeof(WorkspaceMigrationsRevit));
        }

        public static void InitializeUnits()
        {
            // set revit units
            BaseUnit.HostApplicationInternalAreaUnit = AreaUnit.SquareFoot;
            BaseUnit.HostApplicationInternalLengthUnit = LengthUnit.DecimalFoot;
            BaseUnit.HostApplicationInternalVolumeUnit = VolumeUnit.CubicFoot;
        }

        public static void InitializeAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                Analyze.Render.AssemblyHelper.ResolveAssemblies;
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
                DynamoRevitApp.DynamoButton.Enabled = true;
            }
        }

        #endregion

        #region Shutdown

        /// <summary>
        ///     Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDynamoViewClosed(object sender, EventArgs e)
        {
            var view = (DynamoView)sender;

            revitDynamoModel.RevitServicesUpdater.Dispose();
            DocumentManager.OnLogError -= revitDynamoModel.Logger.Log;

            view.Dispatcher.UnhandledException -= Dispatcher_UnhandledException;
            view.Closed -= OnDynamoViewClosed;
            DocumentManager.Instance.CurrentUIApplication.ViewActivating -=
                OnApplicationViewActivating;

            AppDomain.CurrentDomain.AssemblyResolve -=
                Analyze.Render.AssemblyHelper.ResolveAssemblies;

            // KILLDYNSETTINGS - this is suspect
            revitDynamoModel.Logger.Dispose();

            DynamoRevitApp.DynamoButton.Enabled = true;

            revitDynamoModel = null;
        }

        #region Application event handler
        /// <summary>
        /// Handler for Revit's DocumentOpened event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationDocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            if (revitDynamoModel != null)
            {
                revitDynamoModel.HandleApplicationDocumentOpened();
            }
        }

        /// <summary>
        /// Handler for Revit's DocumentClosing event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            if (revitDynamoModel != null)
            {
                revitDynamoModel.HandleApplicationDocumentClosing(e.Document);
            }
        }

        /// <summary>
        /// Handler for Revit's DocumentClosed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationDocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            if (revitDynamoModel != null)
            {
                revitDynamoModel.HandleApplicationDocumentClosed();
            }
        }

        /// <summary>
        /// Handler for Revit's ViewActivating event.
        /// Addins are not available in some views in Revit, notably perspective views.
        /// This will present a warning that Dynamo is not available to run and disable the run button.
        /// This handler is called before the ViewActivated event registered on the RevitDynamoModel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationViewActivating(object sender, ViewActivatingEventArgs e)
        {
            if (revitDynamoModel != null)
            {
                revitDynamoModel.SetRunEnabledBasedOnContext(e.NewActiveView);
            }
        }

        /// <summary>
        /// Handler for Revit's ViewActivated event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationViewActivated(object sender, ViewActivatedEventArgs e)
        {
            if (revitDynamoModel != null)
            {
                revitDynamoModel.HandleRevitViewActivated();
            }
        }
        #endregion

#if ENABLE_DYNAMO_SCHEDULER

        private static void DeleteKeeperElementOnce(object sender, IdlingEventArgs idlingEventArgs)
        {
            var uiApplication = DocumentManager.Instance.CurrentUIApplication;
            uiApplication.Idling -= DeleteKeeperElementOnce;
            DynamoRevit.DeleteKeeperElement();
        }

#endif

        /// <summary>
        /// This method access Revit API, therefore it needs to be called only 
        /// by idle thread (i.e. in an 'UIApplication.Idling' event handler).
        /// </summary>
        private static void DeleteKeeperElement()
        {
#if !ENABLE_DYNAMO_SCHEDULER

            if (!IdlePromise.InIdleThread)
            {
                throw new AccessViolationException(
                    "'DeleteKeeperElement' must be called in idle thread");
            }

#endif

            var dbDoc = DocumentManager.Instance.CurrentDBDocument;
            if (null == dbDoc || (dynamoViewModel == null))
                return;

            var vizManager = dynamoViewModel.VisualizationManager as RevitVisualizationManager;
            if (vizManager != null)
            {
                var keeperId = vizManager.KeeperId;
                if (keeperId != ElementId.InvalidElementId)
                {
                    TransactionManager.Instance.EnsureInTransaction(dbDoc);
                    DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                    TransactionManager.Instance.ForceCloseTransaction();
                }
            }
        }

        #endregion
    }
}
