using System;
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
using Dynamo.Units;
using Dynamo.Utilities;
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
        internal static ControlledApplication ControlledApplication;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //TAF load english_us TODO add a way to localize
                res = Resource_en_us.ResourceManager;
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(res.GetString("App_Description"));
                
                var pushButton =
                        ribbonPanel.AddItem(
                            new PushButtonData(
                                "Dynamo 0.7 Alpha",
                                res.GetString("App_Name"),
                                assemblyName,
                                "Dynamo.Applications.DynamoRevit")) as PushButton;

                Bitmap dynamoIcon = Resources.logo_square_32x32;

                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    dynamoIcon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;

                ControlledApplication = application.ControlledApplication;

                RevThread.IdlePromise.RegisterIdle(application);
                TransactionManager.SetupManager(new AutomaticTransactionStrategy());
                ElementBinder.IsEnabled = true;

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
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
        private static DynamoView dynamoView;
        private DynamoController dynamoController;
        private static bool isRunning;
        public static double? DynamoViewX = null;
        public static double? DynamoViewY = null;
        public static double? DynamoViewWidth = null;
        public static double? DynamoViewHeight = null;
        private bool handledCrash;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += DynamoRaaS.AssemblyHelper.ResolveAssemblies;

            //Add an assembly load step for the System.Windows.Interactivity assembly
            //Revit owns a version of this as well. Adding our step here prevents a duplicative
            //load of the dll at a later time.
            var assLoc = Assembly.GetExecutingAssembly().Location;
            var interactivityPath = Path.Combine(Path.GetDirectoryName(assLoc), "System.Windows.Interactivity.dll");
            var interactivityAss = Assembly.LoadFrom(interactivityPath);

            //When a user double-clicks the Dynamo icon, we need to make
            //sure that we don't create another instance of Dynamo.
            if (isRunning)
            {
                Debug.WriteLine("Dynamo is already running.");
                if (dynamoView != null)
                {
                    dynamoView.Focus();
                }
                return Result.Succeeded;
            }

            isRunning = true;

            try
            {
                #region default level

                var fecLevel = new FilteredElementCollector(revit.Application.ActiveUIDocument.Document);
                fecLevel.OfClass(typeof(Level));
                var defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                if (DocumentManager.Instance.CurrentUIApplication == null)
                    DocumentManager.Instance.CurrentUIApplication = revit.Application;

                DocumentManager.OnLogError += DynamoLogger.Instance.Log;

                dynRevitSettings.DefaultLevel = defaultLevel;

                //TODO: has to be changed when we handle multiple docs
                Updater = new RevitServicesUpdater(DynamoRevitApp.ControlledApplication);

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

                        BaseUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
                        BaseUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
                        BaseUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;

                        dynamoController = new DynamoController_Revit(Updater, typeof(DynamoRevitViewModel), context);
                        
                        dynamoView = new DynamoView { DataContext = dynamoController.DynamoViewModel };
                        dynamoController.UIDispatcher = dynamoView.Dispatcher;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoView).Owner = mwHandle;

                        handledCrash = false;

                        dynamoView.WindowStartupLocation = WindowStartupLocation.Manual;

                        Rectangle bounds = Screen.PrimaryScreen.Bounds;
                        dynamoView.Left = DynamoViewX ?? bounds.X;
                        dynamoView.Top = DynamoViewY ?? bounds.Y;
                        dynamoView.Width = DynamoViewWidth ?? 1000.0;
                        dynamoView.Height = DynamoViewHeight ?? 800.0;

                        dynamoView.Show();

                        dynamoView.Dispatcher.UnhandledException += DispatcherOnUnhandledException; 
                        dynamoView.Closing += dynamoView_Closing;
                        dynamoView.Closed += dynamoView_Closed;

                        revit.Application.ViewActivating += Application_ViewActivating;
                    });
            }
            catch (Exception ex)
            {
                isRunning = false;
                MessageBox.Show(ex.ToString());

                DynamoLogger.Instance.LogError(ex.Message);
                DynamoLogger.Instance.LogError(ex.StackTrace);
                DynamoLogger.Instance.LogError("Dynamo log ended " + DateTime.Now);

                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Callback on Revit view activation. Addins are not available in some views in Revit, notably perspective views.
        /// This will present a warning that Dynamo is not available to run and disable the run button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_ViewActivating(object sender, ViewActivatingEventArgs e)
        {
            var view = e.NewActiveView as View3D;

            if (view != null 
                && view.IsPerspective
                && dynSettings.Controller.Context != Context.VASARI_2013
                && dynSettings.Controller.Context != Context.VASARI_2014)
            {
                DynamoLogger.Instance.LogWarning(
                    "Dynamo is not available in a perspective view. Please switch to another view to Run.",
                    WarningLevel.Moderate);
                dynSettings.Controller.DynamoViewModel.RunEnabled = false;
            }
            else
            {
                //alert the user of the new active view and enable the run button
                DynamoLogger.Instance.LogWarning(string.Format("Active view is now {0}", e.NewActiveView.Name), WarningLevel.Mild);
                dynSettings.Controller.DynamoViewModel.RunEnabled = true;
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
                DynamoLogger.Instance.LogError("Dynamo Unhandled Exception");
                DynamoLogger.Instance.LogError(exceptionMessage);
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
            }
            
        }

        /// <summary>
        /// Executes right before Dynamo closes, gives you the chance to cache whatever you might want.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dynamoView_Closing(object sender, EventArgs e)
        {
            // cache the size of the window for later reloading
            DynamoViewX = dynamoView.Left;
            DynamoViewY = dynamoView.Top;
            DynamoViewWidth = dynamoView.ActualWidth;
            DynamoViewHeight = dynamoView.ActualHeight;
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

            dynamoView = null;
            isRunning = false;

            Updater.Dispose();
            DocumentManager.OnLogError -= DynamoLogger.Instance.Log;

            view.Dispatcher.UnhandledException -= DispatcherOnUnhandledException;
            view.Closing -= dynamoView_Closing;
            view.Closed -= dynamoView_Closed;
            DocumentManager.Instance.CurrentUIApplication.ViewActivating -= Application_ViewActivating;

            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyHelper.CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve -= DynamoRaaS.AssemblyHelper.ResolveAssemblies;
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