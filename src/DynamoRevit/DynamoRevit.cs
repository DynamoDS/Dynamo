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

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoUnits;

using DynamoUtilities;

using RevitServices.Elements;
using RevitServices.Persistence;

using MessageBox = System.Windows.Forms.MessageBox;
using RevThread = RevitServices.Threading;

#endregion

namespace Dynamo.Applications
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual),
     Regeneration(RegenerationOption.Manual)]
    public class DynamoRevit : IExternalCommand
    {
        private DynamoViewModel dynamoViewModel;
        private DynamoRevitModel dynamoRevitModel;

        private bool handledCrash;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            RevThread.IdlePromise.RegisterIdle(commandData.Application);

            HandleDebug(commandData);
            InitializeAssemblies();

            try
            {
                var logger = new DynamoLogger(DynamoPathManager.Instance.Logs);

                if (DocumentManager.Instance.CurrentUIApplication == null)
                    DocumentManager.Instance.CurrentUIApplication = commandData.Application;

                DocumentManager.OnLogError += dynSettings.DynamoLogger.Log;

                updater = new RevitServicesUpdater( DynamoRevitApp.ControlledApplication, DynamoRevitApp.Updaters);
                updater.ElementAddedForID += ElementMappingCache.GetInstance().WatcherMethodForAdd;
                updater.ElementsDeleted += ElementMappingCache.GetInstance().WatcherMethodForDelete;

                RevThread.IdlePromise.ExecuteOnIdleAsync(
                    delegate
                    {
                        //get window handle
                        IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                        var r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                        string context = r.Replace(commandData.Application.Application.VersionName, "");

                        //they changed the application version name conventions for vasari
                        //it no longer has a version year so we can't compare it to other versions
                        if (context == "Vasari")
                            context = "Vasari 2014";

                        dynamoViewModel = InitializeCore( updater,
                            logger,
                            context );

                        var dynamoView = new DynamoView
                        {
                            DataContext = dynamoViewModel.DynamoViewModel
                        };
                        dynamoViewModel.UIDispatcher = dynamoView.Dispatcher;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoView).Owner = mwHandle;

                        handledCrash = false;

                        dynamoView.Show();

                        TryOpenWorkspaceInCommandData(commandData);

                        dynamoView.Dispatcher.UnhandledException += DispatcherOnUnhandledException;
                        dynamoView.Closing += dynamoView_Closing;
                        dynamoView.Closed += dynamoView_Closed;

                        commandData.Application.ViewActivating += Application_ViewActivating;
                    });

                // Disable the Dynamo button to prevent a re-run
                DynamoRevitApp.DynamoButton.Enabled = false;
            }
            catch (Exception ex)
            {
                InstrumentationLogger.LogException(ex);
                StabilityTracking.GetInstance().NotifyCrash();

                MessageBox.Show(ex.ToString());

                DynamoRevitApp.DynamoButton.Enabled = true;

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void TryOpenWorkspaceInCommandData(ExternalCommandData commandData)
        {

            if (commandData.JournalData != null && commandData.JournalData.ContainsKey("dynPath"))
                dynamoViewModel.Model.OpenWorkspace(commandData.JournalData["dynPath"]);
        }


        public DynamoViewModel InitializeCore(
            RevitServicesUpdater updater, DynamoLogger logger, string context)
        {
            BaseUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
            BaseUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
            BaseUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;

            var updateManager = new UpdateManager.UpdateManager(logger);

            string corePath =
                Path.GetFullPath(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\");


            var dynamoModel = new DynamoRevitModel( context, 

            var dynamoController = new DynamoViewModel(
                updater,
                context,
                updateManager,
                corePath);

            // Generate a view model to be the data context for the view
            dynamoController.DynamoViewModel = new DynamoViewModel(dynamoController, null);
            dynamoController.DynamoViewModel.RequestAuthentication +=
                dynamoController.RegisterSingleSignOn;
            dynamoController.DynamoViewModel.CurrentSpaceViewModel.CanFindNodesFromElements = true;
            dynamoController.DynamoViewModel.CurrentSpaceViewModel.FindNodesFromElements =
                dynamoController.FindNodesFromSelection;

            // Register the view model to handle sign-on requests
            dynamoController.RequestAuthentication +=
                SingleSignOnManager.RegisterSingleSignOn;

            dynamoController.VisualizationManager = new RevitVisualizationManager();

            return dynamoController;
        }

        private void InitializeAssemblies()
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

        private void HandleDebug(ExternalCommandData commandData)
        {
            if (commandData.JournalData != null && commandData.JournalData.ContainsKey("debug"))
            {
                if (bool.Parse(commandData.JournalData["debug"]))
                    Debugger.Launch();
            }
        }

        /// <summary>
        ///     Handler for Revit's ViewActivating event.
        ///     Addins are not available in some views in Revit, notably perspective views.
        ///     This will present a warning that Dynamo is not available to run and disable the run button.
        ///     This handler is called before the ViewActivated event registered on the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_ViewActivating(object sender, ViewActivatingEventArgs e)
        {
            this.dynamoRevitModel.SetRunEnabledBasedOnContext(e.NewActiveView);
        }

        /// <summary>
        ///     A method to deal with unhandle exceptions.  Executes right before Revit crashes.
        ///     Dynamo is still valid at this time, but further work may cause corruption.  Here,
        ///     we run the ExitCommand, allowing the user to save all of their work.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Info about the exception</param>
        private void DispatcherOnUnhandledException(
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

                this.dynamoRevitModel.Logger.LogError("Dynamo Unhandled Exception");
                this.dynamoRevitModel.Logger.LogError(exceptionMessage);
            }
            catch { }

            try
            {
                DynamoModel.IsCrashing = true;
                this.dynamoRevitModel.OnRequestsCrashPrompt(
                    this,
                    new CrashPromptArgs(args.Exception.Message + "\n\n" + args.Exception.StackTrace));
                this.dynamoViewModel.Exit(false); // don't allow cancellation
            }
            catch { }
            finally
            {
                args.Handled = true;

                // KILLDYNSETTINGS - this is suspect
                this.dynamoRevitModel.Logger.Dispose();
                DynamoRevitApp.DynamoButton.Enabled = true;
            }
        }

        /// <summary>
        ///     Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void dynamoView_Closing(object sender, EventArgs e)
        {
            RevThread.IdlePromise.ClearPromises();
            RevThread.IdlePromise.Shutdown();
        }

        /// <summary>
        ///     Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closed(object sender, EventArgs e)
        {
            var view = (DynamoView)sender;

            dynamoRevitModel.RevitUpdater.Dispose();
            DocumentManager.OnLogError -= this.dynamoRevitModel.Logger.Log;

            view.Dispatcher.UnhandledException -= DispatcherOnUnhandledException;
            view.Closing -= dynamoView_Closing;
            view.Closed -= dynamoView_Closed;
            DocumentManager.Instance.CurrentUIApplication.ViewActivating -=
                Application_ViewActivating;

            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyHelper.ResolveAssembly;
            AppDomain.CurrentDomain.AssemblyResolve -=
                Analyze.Render.AssemblyHelper.ResolveAssemblies;

            // KILLDYNSETTINGS - this is suspect
            dynamoRevitModel.Logger.Dispose();

            DynamoRevitApp.DynamoButton.Enabled = true;
        }

    }
}
