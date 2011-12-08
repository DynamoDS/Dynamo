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
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Analysis;//MDJ needed for spatialfeildmanager

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

                System.Drawing.Bitmap dynamoIcon = Dynamo.Applications.Properties.Resources.Nodes_32_32;

                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         dynamoIcon.GetHbitmap(),
                         IntPtr.Zero,
                         System.Windows.Int32Rect.Empty,
                         System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                pushButton.LargeImage = bitmapSource;
                pushButton.Image = bitmapSource;



                // MDJ = element level events and dyanmic model update

                // Register sfm updater with Revit
                //DynamoUpdater updater = new DynamoUpdater(application.ActiveAddInId);
                //UpdaterRegistry.RegisterUpdater(updater);
                //// Change Scope = any spatial field element
                //ElementClassFilter SpatialFieldFilter = new ElementClassFilter(typeof(SpatialFieldManager));
                ////ElementClassFilter SpatialFieldFilter = new ElementClassFilter(typeof(SpatialFieldManager));
                //// Change type = element addition
                //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), SpatialFieldFilter,
                //Element.GetChangeTypeAny()); // Element.GetChangeTypeElementAddition()


                DynamoUpdater updater = new DynamoUpdater(application.ActiveAddInId);//, sphere.Id, view.Id);
                if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId())) UpdaterRegistry.RegisterUpdater(updater);
                ElementClassFilter SpatialFieldFilter = new ElementClassFilter(typeof(SpatialFieldManager));
                ElementClassFilter familyFilter = new ElementClassFilter(typeof(FamilyInstance));
                ElementCategoryFilter massFilter = new ElementCategoryFilter(BuiltInCategory.OST_Mass);
                IList<ElementFilter> filterList = new List<ElementFilter>();
                filterList.Add(SpatialFieldFilter);
                filterList.Add(familyFilter);
                filterList.Add(massFilter);
                LogicalOrFilter filter = new LogicalOrFilter(filterList);

                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeGeometry());
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementDeletion());



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

            DynamoUpdater updater = new DynamoUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }

        public class DynamoUpdater : IUpdater
        {
            static AddInId m_appId;
            static UpdaterId m_updaterId;
            SpatialFieldManager m_sfm = null;
            FamilyInstance m_fam = null;
            // constructor takes the AddInId for the add-in associated with this updater
            public DynamoUpdater(AddInId id)
            {
                m_appId = id;
                m_updaterId = new UpdaterId(m_appId, new Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")); //[Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")]
            }
            public void Execute(UpdaterData data)
            {
                Document doc = data.GetDocument();
                Autodesk.Revit.DB.View view = doc.ActiveView;
                SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view);

                UpdaterData tempData = data;
          
                if (sfm != null)
                {
                    // Cache the spatial field manager if ther is one
                    if (m_sfm == null)
                    {
                        //FilteredElementCollector collector = new FilteredElementCollector(doc);
                        //collector.OfClass(typeof(SpatialFieldManager));
                        //var sfm = from element in collector
                        //          select element;
                        //if (sfm.Count<Element>() > 0) // if we actually found an SFM
                        //{
                        //m_sfm = sfm.Cast<SpatialFieldManager>().ElementAt<SpatialFieldManager>(0);
                        m_sfm = sfm;
                        //TaskDialog.Show("ah hah", "found spatial field manager adding to cache");
                        //}

                    }
                    if (m_sfm != null)
                    {
                        // if we find an sfm has been updated and it matches what  already have one cached, send it to dyanmo
                        //foreach (ElementId addedElemId in data.GetAddedElementIds())
                        //{
                            //SpatialFieldManager sfm = doc.get_Element(addedElemId) as SpatialFieldManager;
                            //if (sfm != null)
                            //{
                               // TaskDialog.Show("ah hah", "found spatial field manager yet, passing to dynamo");
                        dynElementSettings.SharedInstance.SpatialFieldManagerUpdated = sfm;
                        //Dynamo.Elements.OnDynElementReadyToBuild(EventArgs.Empty);//kick it
                            //}
                        //}
                    }
                }
                else
                {
                    //TaskDialog.Show("ah hah", "no spatial field manager yet, please run sr tool");
                }
            }


            public string GetAdditionalInformation()
            {
                return "Watch for changes to Analysis Results object (Spatial Field Manager) and pass this to Dynamo";
            }
            public ChangePriority GetChangePriority()
            {
                return ChangePriority.FloorsRoofsStructuralWalls;
            }
            public UpdaterId GetUpdaterId()
            {
                return m_updaterId;
            }
            public string GetUpdaterName()
            {
                return "Dyanmo Analysis Results Watcher";
            }
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

