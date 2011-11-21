//Copyright 2011 Ian Keough

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
using Dynamo.Elements;
using Dynamo.Controls;
using System.Xml.Serialization;
using Dynamo.Utilities;


namespace Dynamo.Applications
{

    //MDJ - Added by Matt Jezyk - 10.27.2011
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    public class DynamoRevitApp : IExternalApplication 
    {

        static private string m_AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static private string m_AssemblyDirectory = Path.GetDirectoryName(m_AssemblyName);


        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create new ribbon panel
                RibbonPanel ribbonPanel = application.CreateRibbonPanel("Visual Programming"); //MDJ todo - move hard-coded strings out to resource files
                
                //Create a push button in the ribbon panel 

                PushButton pushButton = ribbonPanel.AddItem(new PushButtonData("Dynamo",
                    "Dynamo", m_AssemblyName, "Dynamo.Applications.DynamoRevit")) as PushButton;


                // Set the large image shown on button using a resource from DynamoRevit.resx (namespace is Dynamo.Applications.DynamoRevitResources)

                System.Drawing.Bitmap dynamoIcon = DynamoRevitResources.Nodes_32_32;

                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         dynamoIcon.GetHbitmap(),
                         IntPtr.Zero,
                         System.Windows.Int32Rect.Empty,
                         System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;

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
    //MDJ - End of chunk added by Matt Jezyk

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
                failOpt.SetFailuresPreprocessor(new DynamoWarningSwallower());
                trans.SetFailureHandlingOptions(failOpt);

                m_revit.Idling += new EventHandler<IdlingEventArgs>(OnIdling);

                #region default level
                Level defaultLevel = null;
                FilteredElementCollector fecLevel = new FilteredElementCollector(m_doc.Document);
                fecLevel.OfClass(typeof(Level));
                for (int i = 0; i < fecLevel.ToElements().Count; i++)
                {
                    defaultLevel = fecLevel.ToElements()[i] as Level;
                    break;
                }

                #endregion

                DynamoWarningSwallower swallow = new DynamoWarningSwallower();

                dynElementSettings.SharedInstance.Revit = m_revit;
                dynElementSettings.SharedInstance.Doc = m_doc;
                dynElementSettings.SharedInstance.DefaultLevel = defaultLevel;
                dynElementSettings.SharedInstance.WarningSwallower = swallow;
                dynElementSettings.SharedInstance.MainTransaction = trans;
                dynElementSettings.SharedInstance.Writer = tw;
                //dynElementSettings settings = new dynElementSettings(m_revit, m_doc,
                    //defaultLevel, swallow, trans);


                //show the log
                dynamoForm = new dynBench();

                //get the window handle
                Process process = Process.GetCurrentProcess();
                new System.Windows.Interop.WindowInteropHelper(dynamoForm).Owner = process.MainWindowHandle;
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

                return Autodesk.Revit.UI.Result.Failed;
            }

            trans.Commit();

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        void OnIdling(object sender, IdlingEventArgs e)
        {

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

        void GetStyles(ref Element dynVolatileStyle, ref Element dynPersistentStyle, 
                    ref Element dynXStyle, ref Element dynYStyle, ref Element dynZStyle)
        {

            Curve tick = m_revit.Application.Create.NewLineBound(new XYZ(), new XYZ(0, 1, 0));
            Plane p = new Plane(new XYZ(0,0,1), new XYZ());
            SketchPlane sp = m_doc.Document.Create.NewSketchPlane(p);
            ModelCurve ml = m_doc.Document.Create.NewModelCurve(tick, sp);
            ElementArray styles = ml.LineStyles;
            ElementArrayIterator styleIter = styles.ForwardIterator();
            
            while (styleIter.MoveNext())
            {
                Element style = styleIter.Current as Element;
                if (style.Name == "dynVolatile")
                {
                    dynVolatileStyle = style;
                }
                else if (style.Name == "dynPersistent")
                {
                    dynPersistentStyle = style;
                }
                else if (style.Name == "dynX")
                {
                    dynXStyle = style;
                }
                else if (style.Name == "dynY")
                {
                    dynYStyle = style;
                }
                else if (style.Name == "dynZ")
                {
                    dynZStyle = style;
                }
            }
        }
    }

    


}
