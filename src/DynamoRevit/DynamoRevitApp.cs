using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Dynamo.Applications.Properties;
using Dynamo.Utilities;

using DynamoUtilities;

using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

using MessageBox = System.Windows.Forms.MessageBox;

namespace Dynamo.Applications
{
    

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic),
     Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        private static readonly string assemblyName = Assembly.GetExecutingAssembly().Location;
        private static ResourceManager res;
        public static ControlledApplication ControlledApplication;
        public static List<IUpdater> Updaters = new List<IUpdater>();
        internal static PushButton DynamoButton;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                SetupDynamoPaths(application);

                SubscribeAssemblyResolvingEvent();

                ControlledApplication = application.ControlledApplication;

                TransactionManager.SetupManager(new AutomaticTransactionStrategy());
                ElementBinder.IsEnabled = true;

                //TAF load english_us TODO add a way to localize
                res = Resource_en_us.ResourceManager;

                // Create new ribbon panel
                RibbonPanel ribbonPanel =
                    application.CreateRibbonPanel(res.GetString("App_Description"));

                DynamoButton =
                    (PushButton)
                        ribbonPanel.AddItem(
                            new PushButtonData(
                                "Dynamo 0.7",
                                res.GetString("App_Name"),
                                assemblyName,
                                "Dynamo.Applications.DynamoRevit"));


                Bitmap dynamoIcon = Resources.logo_square_32x32;

                BitmapSource bitmapSource =
                    Imaging.CreateBitmapSourceFromHBitmap(
                        dynamoIcon.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                DynamoButton.LargeImage = bitmapSource;
                DynamoButton.Image = bitmapSource;

                RegisterAdditionalUpdaters(application);

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
            UnsubscribeAssemblyResolvingEvent();

            return Result.Succeeded;
        }

        // should be handled by the ModelUpdater class. But there are some
        // cases where the document modifications handled there do no catch
        // certain document interactions. Those should be registered here.
        /// <summary>
        ///     Register some document updaters. Generally, document updaters
        /// </summary>
        /// <param name="application"></param>
        private static void RegisterAdditionalUpdaters(UIControlledApplication application)
        {
            var sunUpdater = new SunPathUpdater(application.ActiveAddInId);

            if (!UpdaterRegistry.IsUpdaterRegistered(sunUpdater.GetUpdaterId()))
                UpdaterRegistry.RegisterUpdater(sunUpdater);

            var sunFilter = new ElementClassFilter(typeof(SunAndShadowSettings));
            var filterList = new List<ElementFilter> { sunFilter };
            ElementFilter filter = new LogicalOrFilter(filterList);
            UpdaterRegistry.AddTrigger(
                sunUpdater.GetUpdaterId(),
                filter,
                Element.GetChangeTypeAny());
            Updaters.Add(sunUpdater);
        }

        private static void SetupDynamoPaths(UIControlledApplication application)
        {
            // The executing assembly will be in Revit_20xx, so 
            // we have to walk up one level. Unfortunately, we
            // can't use DynamoPathManager here because those are not
            // initialized until the DynamoModel is constructed.
            string assDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Add the Revit_20xx folder for assembly resolution
            DynamoPathManager.Instance.AddResolutionPath(assDir);

            // Setup the core paths
            DynamoPathManager.Instance.InitializeCore(Path.GetFullPath(assDir + @"\.."));

            // Add Revit-specific paths for loading.
            DynamoPathManager.Instance.AddPreloadLibrary(Path.Combine(assDir, "RevitNodes.dll"));
            DynamoPathManager.Instance.AddPreloadLibrary(Path.Combine(assDir, "SimpleRaaS.dll"));

            //add an additional node processing folder
            DynamoPathManager.Instance.Nodes.Add(Path.Combine(assDir, "nodes"));

            // Set the LibG folder based on the context.
            // LibG is set to reference the libg_219 folder by default.
            var versionInt = int.Parse(application.ControlledApplication.VersionNumber);
            switch (versionInt)
            {
                case 2016:
                    DynamoPathManager.Instance.SetLibGPath("221");
                    break;
                case 2015:
                    DynamoPathManager.Instance.SetLibGPath("220");
                    break;
                default:
                    DynamoPathManager.Instance.SetLibGPath("219");
                    break;
            }
        }

        private void SubscribeAssemblyResolvingEvent()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;
        }

        private void UnsubscribeAssemblyResolvingEvent()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyHelper.ResolveAssembly;
        }
    }
}