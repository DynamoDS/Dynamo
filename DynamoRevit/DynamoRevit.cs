//Copyright 2012 Ian Keough

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
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

using Autodesk.Revit.UI.Selection;
using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Analysis;//MDJ needed for spatialfeildmanager

using Dynamo;
using Dynamo.Elements;
using Dynamo.Controls;
using System.Xml.Serialization;
using Dynamo.Utilities;
using System.Windows.Interop;
using System.Reflection;
using System.Windows;


namespace Dynamo.Applications
{
    //MDJ - Added by Matt Jezyk - 10.27.2011
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class DynamoRevitApp : IExternalApplication
    {
        static private string m_AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static private string m_AssemblyDirectory = Path.GetDirectoryName(m_AssemblyName);
        static public DynamoUpdater updater;

        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel("Visual Programming"); //MDJ todo - move hard-coded strings out to resource files

                //Create a push button in the ribbon panel 

                PushButton pushButton = ribbonPanel.AddItem(new PushButtonData("Dynamo",
                    "Dynamo", m_AssemblyName, "Dynamo.Applications.DynamoRevit")) as PushButton;

                System.Drawing.Bitmap dynamoIcon = Dynamo.Applications.Properties.Resources.Nodes_32_32;

                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         dynamoIcon.GetHbitmap(),
                         IntPtr.Zero,
                         System.Windows.Int32Rect.Empty,
                         System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;

                // MDJ = element level events and dyanmic model update
                // MDJ 6-8-12  trying to get new dynamo to watch for user created ref points and re-reun definitin when they are moved

                IdlePromise.RegisterIdle(application);

                updater = new DynamoUpdater(application.ActiveAddInId, application.ControlledApplication);
                if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId())) UpdaterRegistry.RegisterUpdater(updater);

                ElementClassFilter SpatialFieldFilter = new ElementClassFilter(typeof(SpatialFieldManager));
                ElementClassFilter familyFilter = new ElementClassFilter(typeof(FamilyInstance));
                ElementCategoryFilter refPointFilter = new ElementCategoryFilter(BuiltInCategory.OST_ReferencePoints);
                ElementClassFilter modelCurveFilter = new ElementClassFilter(typeof(CurveElement));
                ElementClassFilter sunFilter = new ElementClassFilter(typeof(SunAndShadowSettings));
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
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    class DynamoRevit : IExternalCommand
    {
        Autodesk.Revit.UI.UIApplication m_revit;
        Autodesk.Revit.UI.UIDocument m_doc;
        static dynBench dynamoForm;
        TextWriter tw;

        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData revit, ref string message, ElementSet elements)
        {
            if (dynamoForm != null)
            {
                dynamoForm.Focus();
                return Result.Succeeded;
            }

            SplashScreen splashScreen = null;
            try
            {
                //create a log file
                string tempPath = System.IO.Path.GetTempPath();
                string logPath = Path.Combine(tempPath, "dynamoLog.txt");

                if (File.Exists(logPath))
                    File.Delete(logPath);

                tw = new StreamWriter(logPath);
                tw.WriteLine("Dynamo log started " + System.DateTime.Now.ToString());

                m_revit = revit.Application;
                m_doc = m_revit.ActiveUIDocument;

                #region default level

                Level defaultLevel = null;
                FilteredElementCollector fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof(Level));
                defaultLevel = fecLevel.ToElements()[0] as Level;

                #endregion

                dynElementSettings.SharedInstance.Revit = m_revit;
                dynElementSettings.SharedInstance.Doc = m_doc;
                dynElementSettings.SharedInstance.DefaultLevel = defaultLevel;
                dynElementSettings.SharedInstance.Writer = tw;

                IdlePromise.ExecuteOnIdle(new Action(
                    delegate
                    {
                        //get window handle
                        IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                        //prepare and show splash
                        splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), "splash.png");
                        splashScreen.Show(false, true);

                        //show the window
                        dynamoForm = new dynBench(DynamoRevitApp.updater, splashScreen);

                        //set window handle and show dynamo
                        new System.Windows.Interop.WindowInteropHelper(dynamoForm).Owner = mwHandle;

                        if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
                        {
                            dynamoForm.WindowStartupLocation = WindowStartupLocation.Manual;

                            System.Drawing.Rectangle bounds = System.Windows.Forms.Screen.AllScreens[1].Bounds;
                            dynamoForm.Left = bounds.X;
                            dynamoForm.Top = bounds.Y;
                            dynamoForm.Loaded += new RoutedEventHandler(dynamoForm_Loaded);
                        }
                        else
                        {
                            //System.Drawing.Rectangle bounds = System.Windows.Forms.Screen.AllScreens[0].Bounds;
                            //dynamoForm.Left = bounds.X;
                            //dynamoForm.Top = bounds.Y;
                            dynamoForm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        }

                        dynamoForm.Show();

                        dynamoForm.Closed += new EventHandler(dynamoForm_Closed);
                    }
                ));
            }
            catch (Exception ex)
            {
                tw.WriteLine("Dynamo log ended " + System.DateTime.Now.ToString());

                tw.Close();
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                return Result.Failed;
            }

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        void dynamoForm_Closed(object sender, EventArgs e)
        {
            dynamoForm = null;
        }

        void dynamoForm_Loaded(object sender, RoutedEventArgs e)
        {
            ((dynBench)sender).WindowState = WindowState.Maximized;
        }
    }


    class WindowHandle : System.Windows.Interop.IWin32Window
    {
        IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            Debug.Assert(IntPtr.Zero != h,
              "expected non-null window handle");

            _hwnd = h;
        }

        public IntPtr Handle
        {
            get
            {
                return _hwnd;
            }
        }
    }
}

