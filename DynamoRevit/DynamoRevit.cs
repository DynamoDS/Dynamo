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

            updater = new DynamoUpdater(application.ActiveAddInId, application.ControlledApplication);
            if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId())) UpdaterRegistry.RegisterUpdater(updater);
            //ElementClassFilter SpatialFieldFilter = new ElementClassFilter(typeof(SpatialFieldManager));
            //ElementCategoryFilter massFilter = new ElementCategoryFilter(BuiltInCategory.OST_Mass);

            ElementClassFilter familyFilter = new ElementClassFilter(typeof(FamilyInstance));
            ElementCategoryFilter refPointFilter = new ElementCategoryFilter(BuiltInCategory.OST_ReferencePoints);
            ElementClassFilter modelCurveFilter = new ElementClassFilter(typeof(CurveElement));
            ElementClassFilter sunFilter = new ElementClassFilter(typeof(SunAndShadowSettings));
            IList<ElementFilter> filterList = new List<ElementFilter>();
            //filterList.Add(SpatialFieldFilter);

            //filterList.Add(massFilter);
            filterList.Add(familyFilter);
            filterList.Add(modelCurveFilter);
            filterList.Add(refPointFilter);
            filterList.Add(sunFilter);
            ElementFilter filter = new LogicalOrFilter(filterList);

            //ElementFilter filter = new ElementClassFilter(typeof(Element));

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
      dynBench dynamoForm;
      TextWriter tw;
      Transaction trans;

      public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData revit, ref string message, ElementSet elements)
      {
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

            trans = new Transaction(m_doc.Document, "Dynamo");
            trans.Start();

            FailureHandlingOptions failOpt = trans.GetFailureHandlingOptions();
            //failOpt.SetFailuresPreprocessor(new DynamoWarningSwallower());
            trans.SetFailureHandlingOptions(failOpt);

            #region default level
            Level defaultLevel = null;
            FilteredElementCollector fecLevel = new FilteredElementCollector(m_doc.Document);
            fecLevel.OfClass(typeof(Level));
            //for (int i = 0; i < fecLevel.ToElements().Count; i++)
            //{
            //    defaultLevel = fecLevel.ToElements()[i] as Level;
            //    break;
            //}
            defaultLevel = fecLevel.ToElements()[0] as Level;

            #endregion

            //DynamoWarningSwallower swallow = new DynamoWarningSwallower();

            dynElementSettings.SharedInstance.Revit = m_revit;
            dynElementSettings.SharedInstance.Doc = m_doc;
            dynElementSettings.SharedInstance.DefaultLevel = defaultLevel;
            //dynElementSettings.SharedInstance.WarningSwallower = swallow;
            dynElementSettings.SharedInstance.MainTransaction = trans;
            dynElementSettings.SharedInstance.Writer = tw;
            //dynElementSettings settings = new dynElementSettings(m_revit, m_doc,
            //defaultLevel, swallow, trans);

            //get window handle
            IntPtr h = Process.GetCurrentProcess().MainWindowHandle;

            //prepare and show splash
            splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), "splash.png");
            splashScreen.Show(false, true);

            //show the window
            dynamoForm = new dynBench(DynamoRevitApp.updater, splashScreen);

            //var revitWindow = (Window)HwndSource.FromHwnd(h).RootVisual;
            //var w = revitWindow.ActualWidth;
            //var h = revitWindow.ActualHeight;

            //set window handle and show dynamo
            new System.Windows.Interop.WindowInteropHelper(dynamoForm).Owner = h;
            dynamoForm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dynamoForm.Show();

            if (dynamoForm.DialogResult.HasValue && dynamoForm.DialogResult.Value == false)   //the WPF false is "cancel"
            {
               tw.WriteLine("Dynamo log ended " + System.DateTime.Now.ToString());
               tw.Close();

               return Autodesk.Revit.UI.Result.Cancelled;
            }

         }
         catch (Exception e)
         {
            trans.Dispose();
            Debug.WriteLine(e.Message + ":" + e.StackTrace);
            Debug.WriteLine(e.InnerException);
            message = e.Message + " : " + e.StackTrace;

            if (tw != null)
            {
               tw.WriteLine(e.Message);
               tw.WriteLine(e.StackTrace);
               tw.Close();
            }

            //if (splashScreen != null)
            //   splashScreen.Close(TimeSpan.FromMilliseconds(100));

            return Autodesk.Revit.UI.Result.Failed;
         }

         trans.Commit();

         return Autodesk.Revit.UI.Result.Succeeded;
      }

      //public Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
      //{
      //    ElementUpdater updater = new ElementUpdater(application.ActiveAddInId);
      //    UpdaterRegistry.RegisterUpdater(updater);
      //    ElementClassFilter elementFilter = new ElementClassFilter(typeof(Element));
      //    UpdaterRegistry.AddTrigger(updater.getUpdaterId(), elementFilter, Element.GetChangeTypeParameter());

      //    return Result.Succeeded;
      //}

      //public Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
      //{
      //    return Result.Succeeded;
      //}

      //void GetStyles(ref Element dynVolatileStyle, ref Element dynPersistentStyle,
      //            ref Element dynXStyle, ref Element dynYStyle, ref Element dynZStyle)
      //{
      //   Curve tick = m_revit.Application.Create.NewLineBound(new XYZ(), new XYZ(0, 1, 0));
      //   Plane p = new Plane(new XYZ(0, 0, 1), new XYZ());
      //   SketchPlane sp = m_doc.Document.Create.NewSketchPlane(p);
      //   ModelCurve ml = m_doc.Document.Create.NewModelCurve(tick, sp);
      //   ElementArray styles = ml.GetLineStyleIds();
      //   ElementArrayIterator styleIter = styles.ForwardIterator();

      //   while (styleIter.MoveNext())
      //   {
      //      Element style = styleIter.Current as Element;
      //      if (style.Name == "dynVolatile")
      //      {
      //         dynVolatileStyle = style;
      //      }
      //      else if (style.Name == "dynPersistent")
      //      {
      //         dynPersistentStyle = style;
      //      }
      //      else if (style.Name == "dynX")
      //      {
      //         dynXStyle = style;
      //      }
      //      else if (style.Name == "dynY")
      //      {
      //         dynYStyle = style;
      //      }
      //      else if (style.Name == "dynZ")
      //      {
      //         dynZStyle = style;
      //      }
      //   }
      //}
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

