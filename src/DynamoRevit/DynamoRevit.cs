using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Dynamo.Applications.Properties;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUnits;
using Dynamo.UpdateManager;
using RevitServices.Elements;
using RevitServices.Transactions;
using RevitServices.Persistence;

using IWin32Window = System.Windows.Interop.IWin32Window;
using MessageBox = System.Windows.Forms.MessageBox;
using Rectangle = System.Drawing.Rectangle;
using RevThread = RevitServices.Threading;
using System.IO;

namespace Dynamo.Applications
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        private static readonly string assemblyName = Assembly.GetExecutingAssembly().Location;
        private static ResourceManager res;
        public static ControlledApplication ControlledApplication;
        public static List<IUpdater> Updaters = new List<IUpdater>();
        internal static PushButton dynamoButton;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                ControlledApplication = application.ControlledApplication;

                RevThread.IdlePromise.RegisterIdle(application);
                TransactionManager.SetupManager(new AutomaticTransactionStrategy());
                ElementBinder.IsEnabled = true;

                //TAF load english_us TODO add a way to localize
                res = Resource_en_us.ResourceManager;
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(res.GetString("App_Description"));

                dynamoButton =
                        (PushButton)ribbonPanel.AddItem(
                            new PushButtonData(
                                "Dynamo 0.7 Alpha",
                                res.GetString("App_Name"),
                                assemblyName,
                                "Dynamo.Applications.DynamoRevit"));
                
                
                Bitmap dynamoIcon = Resources.logo_square_32x32;
                
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    dynamoIcon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                dynamoButton.LargeImage = bitmapSource;
                dynamoButton.Image = bitmapSource;

                RegisterAdditionalUpdaters(application);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }

        /// <summary>
        /// Register some document updaters. Generally, document updaters 
        // should be handled by the ModelUpdater class. But there are some
        // cases where the document modifications handled there do no catch
        // certain document interactions. Those should be registered here.
        /// </summary>
        /// <param name="application"></param>
        private static void RegisterAdditionalUpdaters(UIControlledApplication application)
        {
            
            var sunUpdater = new SunPathUpdater(application.ActiveAddInId);

            if (!UpdaterRegistry.IsUpdaterRegistered(sunUpdater.GetUpdaterId()))
                UpdaterRegistry.RegisterUpdater(sunUpdater);

            var sunFilter = new ElementClassFilter(typeof (SunAndShadowSettings));
            var filterList = new List<ElementFilter> {sunFilter};
            ElementFilter filter = new LogicalOrFilter(filterList);
            UpdaterRegistry.AddTrigger(sunUpdater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
            Updaters.Add(sunUpdater);
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevit : IExternalCommand
    {
        public static RevitServicesUpdater Updater;
        private DynamoController dynamoController;
        private bool handledCrash;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            if (revit.JournalData != null &&
                revit.JournalData.ContainsKey("debug"))
            {
                if (bool.Parse(revit.JournalData["debug"]))
                {
                    Debugger.Launch();
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += Analyze.Render.AssemblyHelper.ResolveAssemblies;

            //Add an assembly load step for the System.Windows.Interactivity assembly
            //Revit owns a version of this as well. Adding our step here prevents a duplicative
            //load of the dll at a later time.
            var assLoc = Assembly.GetExecutingAssembly().Location;
            var interactivityPath = Path.Combine(Path.GetDirectoryName(assLoc), "System.Windows.Interactivity.dll");
            var interactivityAss = Assembly.LoadFrom(interactivityPath);

            DynamoRevitApp.dynamoButton.Enabled = false;

            try
            {
                #region default level

                var fecLevel = new FilteredElementCollector(revit.Application.ActiveUIDocument.Document);
                fecLevel.OfClass(typeof(Level));
                var defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                var logger = new DynamoLogger();
                dynSettings.DynamoLogger = logger;

                if (DocumentManager.Instance.CurrentUIApplication == null)
                    DocumentManager.Instance.CurrentUIApplication = revit.Application;

                DocumentManager.OnLogError += dynSettings.DynamoLogger.Log;

                dynRevitSettings.DefaultLevel = defaultLevel;

                //TODO: has to be changed when we handle multiple docs
                Updater = new RevitServicesUpdater(DynamoRevitApp.ControlledApplication, DynamoRevitApp.Updaters);
                Updater.ElementAddedForID += ElementMappingCache.GetInstance().WatcherMethodForAdd;
                Updater.ElementsDeleted += ElementMappingCache.GetInstance().WatcherMethodForDelete;

                RevThread.IdlePromise.ExecuteOnIdleAsync(
                    delegate
                    {
                        //get window handle
                        IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                        var r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
                        string context = r.Replace(revit.Application.Application.VersionName, "");

                        //they changed the application version name conventions for vasari
                        //it no longer has a version year so we can't compare it to other versions
                        //TODO:come up with a more stable way to test for Vasari beta 3
                        if (context == "Vasari")
                            context = "Vasari 2014";

                        dynamoController = CreateDynamoRevitControllerAndViewModel(Updater, logger, context);

                        var dynamoView = new DynamoView { DataContext = dynamoController.DynamoViewModel };
                        dynamoController.UIDispatcher = dynamoView.Dispatcher;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoView).Owner = mwHandle;

                        handledCrash = false;

                        dynamoView.Show();

                        if (revit.JournalData != null &&
                            revit.JournalData.ContainsKey("dynPath"))
                        {
                            dynamoController.DynamoModel.OpenWorkspace(revit.JournalData["dynPath"]);
                        }

                        dynamoView.Dispatcher.UnhandledException += DispatcherOnUnhandledException; 
                        dynamoView.Closing += dynamoView_Closing;
                        dynamoView.Closed += dynamoView_Closed;

                        revit.Application.ViewActivating += Application_ViewActivating;
                    });
            }
            catch (Exception ex)
            {
                //isRunning = false;
                MessageBox.Show(ex.ToString());

                dynSettings.DynamoLogger.LogError(ex.Message);
                dynSettings.DynamoLogger.LogError(ex.StackTrace);
                dynSettings.DynamoLogger.LogError("Dynamo log ended " + DateTime.Now);

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public static DynamoController_Revit CreateDynamoRevitControllerAndViewModel(RevitServicesUpdater updater, DynamoLogger logger, string context)
        {
            BaseUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
            BaseUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
            BaseUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;

            var updateManager = new UpdateManager.UpdateManager(logger);
            var dynamoController = new DynamoController_Revit(updater, context, updateManager);

            // Generate a view model to be the data context for the view
            dynamoController.DynamoViewModel = new DynamoRevitViewModel(dynamoController, null);
            dynamoController.DynamoViewModel.RequestAuthentication +=
                ((DynamoController_Revit) dynamoController).RegisterSingleSignOn;
            dynamoController.DynamoViewModel.CurrentSpaceViewModel.CanFindNodesFromElements = true;
            dynamoController.DynamoViewModel.CurrentSpaceViewModel.FindNodesFromElements =
                ((DynamoController_Revit) dynamoController).FindNodesFromSelection;

            // Register the view model to handle sign-on requests
            dynSettings.Controller.DynamoViewModel.RequestAuthentication +=
                ((DynamoController_Revit) dynamoController).RegisterSingleSignOn;

            dynamoController.VisualizationManager = new VisualizationManagerRevit();

            return dynamoController;
        }

        /// <summary>
        /// Handler for Revit's ViewActivating event. 
        /// Addins are not available in some views in Revit, notably perspective views.
        /// This will present a warning that Dynamo is not available to run and disable the run button.
        /// This handler is called before the ViewActivated event registered on the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_ViewActivating(object sender, ViewActivatingEventArgs e)
        {
            SetRunEnabledBasedOnContext(e);
        }

        public static void SetRunEnabledBasedOnContext(ViewActivatingEventArgs e)
        {
            var view = e.NewActiveView as View3D;

            if (view != null
                && view.IsPerspective
                && dynSettings.Controller.Context != Context.VASARI_2013
                && dynSettings.Controller.Context != Context.VASARI_2014)
            {
                dynSettings.DynamoLogger.LogWarning(
                    "Dynamo is not available in a perspective view. Please switch to another view to Run.",
                    WarningLevel.Moderate);
                dynSettings.Controller.DynamoViewModel.RunEnabled = false;
            }
            else
            {
                dynSettings.DynamoLogger.Log(string.Format("Active view is now {0}", e.NewActiveView.Name));

                // If there is a current document, then set the run enabled
                // state based on whether the view just activated is 
                // the same document.
                if (DocumentManager.Instance.CurrentUIDocument != null)
                {
                    dynSettings.Controller.DynamoViewModel.RunEnabled =
                        e.NewActiveView.Document.Equals(DocumentManager.Instance.CurrentDBDocument);

                    if (dynSettings.Controller.DynamoViewModel.RunEnabled == false)
                    {
                        dynSettings.DynamoLogger.LogWarning("Dynamo is not pointing at this document. Run will be disabled.",
                            WarningLevel.Error);
                    }
                }
            }
        }

        /// <summary>
        /// A method to deal with unhandle exceptions.  Executes right before Revit crashes.
        /// Dynamo is still valid at this time, but further work may cause corruption.  Here, 
        /// we run the ExitCommand, allowing the user to save all of their work.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Info about the exception</param>
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;

            // only handle a single crash per Dynamo sesh, this should be reset in the initial command
            if (handledCrash)
            {
                return;
            }

            handledCrash = true;

            var exceptionMessage = args.Exception.Message;

            try
            {
                dynSettings.DynamoLogger.LogError("Dynamo Unhandled Exception");
                dynSettings.DynamoLogger.LogError(exceptionMessage);
            }
            catch
            {

            }

            try
            {
                dynSettings.Controller.IsCrashing = true;
                dynSettings.Controller.OnRequestsCrashPrompt(this, new CrashPromptArgs(args.Exception.Message + "\n\n" + args.Exception.StackTrace));
                dynSettings.Controller.DynamoViewModel.Exit(false); // don't allow cancellation
            }
            catch
            {

            }
            finally
            {
                args.Handled = true;
                ((DynamoLogger)dynSettings.DynamoLogger).Dispose();
                DynamoRevitApp.dynamoButton.Enabled = true;
            }
            
        }

        /// <summary>
        /// Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closing(object sender, EventArgs e)
        {
            RevThread.IdlePromise.ClearPromises();
            RevThread.IdlePromise.Shutdown();
        }

        /// <summary>
        /// Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closed(object sender, EventArgs e)
        {
            var view = (DynamoView)sender;

            Updater.Dispose();
            DocumentManager.OnLogError -= dynSettings.DynamoLogger.Log;

            view.Dispatcher.UnhandledException -= DispatcherOnUnhandledException;
            view.Closing -= dynamoView_Closing;
            view.Closed -= dynamoView_Closed;
            DocumentManager.Instance.CurrentUIApplication.ViewActivating -= Application_ViewActivating;

            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyHelper.CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve -= Analyze.Render.AssemblyHelper.ResolveAssemblies;

            ((DynamoLogger) dynSettings.DynamoLogger).Dispose();

            DynamoRevitApp.dynamoButton.Enabled = true;
        }
    }

    internal class WindowHandle : IWin32Window
    {
        private readonly IntPtr hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h, "expected non-null window handle");
            hwnd = h;
        }

        public IntPtr Handle
        {
            get { return hwnd; }
        }
    }
}