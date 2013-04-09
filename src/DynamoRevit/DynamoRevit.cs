//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Dynamo.Applications.Properties;
using Dynamo.Controls;
using Dynamo.Utilities;
using IWin32Window = System.Windows.Interop.IWin32Window;
using MessageBox = System.Windows.Forms.MessageBox;
using Rectangle = System.Drawing.Rectangle;
//MDJ needed for spatialfeildmanager
//TAF added to get strings from resource files

namespace Dynamo.Applications
{
    //MDJ - Added by Matt Jezyk - 10.27.2011
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        private static readonly string m_AssemblyName = Assembly.GetExecutingAssembly().Location;
        private static string m_AssemblyDirectory = Path.GetDirectoryName(m_AssemblyName);
        public static DynamoUpdater updater;
        private static ResourceManager res;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //TAF load english_us TODO add a way to localize
                res = Resource_en_us.ResourceManager;
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(res.GetString("App_Description"));

                //Create a push button in the ribbon panel 
                var pushButton = ribbonPanel.AddItem(new PushButtonData("Dynamo",
                                                                        res.GetString("App_Name"), m_AssemblyName,
                                                                        "Dynamo.Applications.DynamoRevit")) as
                                 PushButton;

                Bitmap dynamoIcon = Resources.Nodes_32_32;


                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    dynamoIcon.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;

                // MDJ = element level events and dyanmic model update
                // MDJ 6-8-12  trying to get new dynamo to watch for user created ref points and re-run definition when they are moved

                IdlePromise.RegisterIdle(application);

                updater = new DynamoUpdater(application.ActiveAddInId, application.ControlledApplication);
                if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
                    UpdaterRegistry.RegisterUpdater(updater);

                var SpatialFieldFilter = new ElementClassFilter(typeof (SpatialFieldManager));
                var familyFilter = new ElementClassFilter(typeof (FamilyInstance));
                var refPointFilter = new ElementCategoryFilter(BuiltInCategory.OST_ReferencePoints);
                var modelCurveFilter = new ElementClassFilter(typeof (CurveElement));
                var sunFilter = new ElementClassFilter(typeof (SunAndShadowSettings));
                IList<ElementFilter> filterList = new List<ElementFilter>();

                filterList.Add(SpatialFieldFilter);
                filterList.Add(familyFilter);
                filterList.Add(modelCurveFilter);
                filterList.Add(refPointFilter);
                filterList.Add(sunFilter);

                ElementFilter filter = new LogicalOrFilter(filterList);

                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());

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
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevit : IExternalCommand
    {
        private static dynBench dynamoBench;
        private UIDocument m_doc;
        private UIApplication m_revit;


        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            if (dynamoBench != null)
            {
                dynamoBench.Focus();
                return Result.Succeeded;
            }

            //SplashScreen splashScreen = null
            Window splashScreen = null;

            dynSettings.StartLogging();

            try
            {
                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                var fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof (Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynRevitSettings.Revit = m_revit;
                dynRevitSettings.Doc = m_doc;
                dynRevitSettings.DefaultLevel = defaultLevel;

                IdlePromise.ExecuteOnIdle(delegate
                    {
                        //get window handle
                        IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                        //prepare and show splash
                        splashScreen = new DynamoSplash();

                        //show the window
                        var dynamoController = new DynamoController_Revit(DynamoRevitApp.updater);
                        dynamoBench = dynamoController.Bench;

                        //set window handle and show dynamo
                        new WindowInteropHelper(dynamoBench).Owner = mwHandle;

                        dynamoBench.WindowStartupLocation = WindowStartupLocation.Manual;

                        Rectangle bounds = Screen.PrimaryScreen.Bounds;
                        dynamoBench.Left = bounds.X;
                        dynamoBench.Top = bounds.Y;
                        dynamoBench.Loaded += dynamoForm_Loaded;

                        dynamoBench.Show();

                        dynamoBench.Closed += dynamoForm_Closed;
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                if (dynSettings.Writer != null)
                {
                    dynSettings.Writer.WriteLine(ex.Message);
                    dynSettings.Writer.WriteLine(ex.StackTrace);
                    dynSettings.Writer.WriteLine("Dynamo log ended " + DateTime.Now.ToString());
                }
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void dynamoForm_Closed(object sender, EventArgs e)
        {
            dynamoBench = null;
        }

        private void dynamoForm_Loaded(object sender, RoutedEventArgs e)
        {
            ((dynBench) sender).WindowState = WindowState.Maximized;
        }
    }

    internal class WindowHandle : IWin32Window
    {
        private readonly IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
                         "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}