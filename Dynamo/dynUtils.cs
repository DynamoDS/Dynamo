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
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis; //MDJ  - added for spatialfeildmanager access
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Dynamo.Controls;
using Dynamo.Elements;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using System.Collections;

namespace Dynamo.Utilities
{
    class dynUtils
    {
        private static ElementId _testid;
        /// <summary>
        /// Utility function to determine if an Element of the given ID exists in the document.
        /// </summary>
        /// <param name="e">ID to check.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public static bool TryGetElement(ElementId id, out Element e)
        {
            try
            {
                e = dynElementSettings.SharedInstance.Doc.Document.GetElement(id);
                if (e != null)
                {
                    _testid = e.Id;
                    return true;
                }
                else
                {
                    e = null;
                    return false;
                }
            }
            catch
            {
                e = null;
                return false;
            }
        }


        /// <summary>
        /// Makes a new generic IEnumerable instance out of a non-generic one.
        /// </summary>
        /// <typeparam name="T">The out-type of the new IEnumerable</typeparam>
        /// <param name="en">Non-generic IEnumerable</param>
        /// <returns></returns>
        public static IEnumerable<T> MakeEnumerable<T>(IEnumerable en)
        {
            foreach (T item in en)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Makes a new generic IEnumerable instance out of a non-generic one.
        /// </summary>
        /// <param name="en">Non-generic IEnumerable</param>
        /// <returns></returns>
        public static IEnumerable<object> MakeEnumerable(IEnumerable en)
        {
            return MakeEnumerable<object>(en);
        }

        public static string Ellipsis(string value, int desiredLength)
        {
            if (desiredLength > value.Length)
            {
                return value;
            }
            else
            {
                return value.Remove(desiredLength - 1) + "...";
            }
        }

        /// <summary>
        /// Creates a sketch plane by projecting one point's z coordinate down to the other's z coordinate.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="doc"></param>
        /// <param name="pt1">The start point</param>
        /// <param name="pt2">The end point</param>
        /// <returns></returns>
        public static SketchPlane CreateSketchPlaneForModelCurve(UIApplication app, UIDocument doc,
            XYZ pt1, XYZ pt2)
        {
            XYZ v1, v2, norm;

            if (pt1.X == pt2.X && pt1.Y == pt2.Y)
            {
                //this is a vertical line
                //make the other axis 
                v1 = (pt2 - pt1).Normalize();
                v2 = ((new XYZ(pt1.X, pt1.Y + 1.0, pt1.Z)) - pt1).Normalize();
                norm = v1.CrossProduct(v2);
            }
            else if (Math.Abs(pt2.Z - pt1.Z) > .00000001)
            {
                //flatten in the z direction
                v1 = (pt2 - pt1).Normalize();
                v2 = ((new XYZ(pt2.X, pt2.Y, pt1.Z)) - pt1).Normalize();
                norm = v1.CrossProduct(v2);
            }
            else if (Math.Abs(pt2.Y - pt1.Y) > .00000001)
            {
                //flatten in the y direction
                v1 = (pt2 - pt1).Normalize();
                v2 = ((new XYZ(pt2.X, pt1.Y, pt2.Z)) - pt1).Normalize();
                norm = v1.CrossProduct(v2);
            }
            else
            {
                //flatten in the x direction
                v1 = (pt2 - pt1).Normalize();
                v2 = ((new XYZ(pt1.X, pt2.Y, pt2.Z)) - pt1).Normalize();
                norm = v1.CrossProduct(v2);
            }
            Plane p = app.Application.Create.NewPlane(norm, pt1);

            SketchPlane sp = doc.Document.Create.NewSketchPlane(p);
            return sp;
        }
    }

    public class dynElementSettings
    {
        Autodesk.Revit.UI.UIApplication revit;
        Autodesk.Revit.UI.UIDocument doc;

        Level defaultLevel;
        DynamoWarningSwallower warningSwallower;
        dynBench bench;
        Dynamo.Controls.DragCanvas workbench;
        dynCollection dynColl;
        Transaction trans;
        TextWriter tw;
        Element spatialFieldManagerUpdated;
        HashSet<ElementId> userSelectedElements = new HashSet<ElementId>();

        private static dynElementSettings sharedInstance;

        LinearGradientBrush errorBrush;
        LinearGradientBrush activeBrush;
        LinearGradientBrush selectedBrush;
        LinearGradientBrush deadBrush;

        //colors taken from:
        //http://cloford.com/resources/colours/500col.htm
        System.Windows.Media.Color colorGreen1 = System.Windows.Media.Color.FromRgb(193, 255, 193);
        System.Windows.Media.Color colorGreen2 = System.Windows.Media.Color.FromRgb(155, 250, 155);
        System.Windows.Media.Color colorRed1 = System.Windows.Media.Color.FromRgb(255, 64, 64);
        System.Windows.Media.Color colorRed2 = System.Windows.Media.Color.FromRgb(205, 51, 51);
        System.Windows.Media.Color colorOrange1 = System.Windows.Media.Color.FromRgb(255, 193, 37);
        System.Windows.Media.Color colorOrange2 = System.Windows.Media.Color.FromRgb(238, 180, 34);
        System.Windows.Media.Color colorGray1 = System.Windows.Media.Color.FromRgb(220, 220, 220);
        System.Windows.Media.Color colorGray2 = System.Windows.Media.Color.FromRgb(192, 192, 192);


        public Element SpatialFieldManagerUpdated
        {
            get { return spatialFieldManagerUpdated; }
            set
            {
                spatialFieldManagerUpdated = value;
            }
        }

        //public ElementSet UserSelectedElements
        //{
        //    get { return userSelectedElements; }
        //    set
        //    {
        //        userSelectedElements = value;
        //    }
        //}

        public Autodesk.Revit.UI.UIApplication Revit
        {
            get { return revit; }
            set { revit = value; }
        }
        public Autodesk.Revit.UI.UIDocument Doc
        {
            get { return doc; }
            set { doc = value; }
        }
        public Level DefaultLevel
        {
            get { return defaultLevel; }
            set { defaultLevel = value; }
        }
        public DynamoWarningSwallower WarningSwallower
        {
            get { return warningSwallower; }
            set { warningSwallower = value; }
        }
        public Dynamo.Controls.DragCanvas Workbench
        {
            get { return workbench; }
            set { workbench = value; }
        }
        public dynCollection Collection
        {
            get { return dynColl; }
            set { dynColl = value; }
        }
        public Transaction MainTransaction
        {
            get { return trans; }
            set { trans = value; }
        }
        public LinearGradientBrush ErrorBrush
        {
            get { return errorBrush; }
            set { errorBrush = value; }
        }
        public LinearGradientBrush ActiveBrush
        {
            get { return activeBrush; }
            set { activeBrush = value; }
        }
        public LinearGradientBrush SelectedBrush
        {
            get { return selectedBrush; }
            set { selectedBrush = value; }
        }
        public LinearGradientBrush DeadBrush
        {
            get { return deadBrush; }
            set { deadBrush = value; }
        }
        public dynBench Bench
        {
            get { return bench; }
            set { bench = value; }
        }
        public TextWriter Writer
        {
            get { return tw; }
            set { tw = value; }
        }
        public static dynElementSettings SharedInstance
        {
            get
            {
                if (sharedInstance == null)
                {
                    sharedInstance = new dynElementSettings();
                    sharedInstance.SetupBrushes();
                }
                return sharedInstance;
            }
        }

        /*
        public dynElementSettings(Autodesk.Revit.UI.UIApplication app, Autodesk.Revit.UI.UIDocument doc, Level defaultLevel, DynamoWarningSwallower warningSwallower, Transaction t)
       {

           this.revit = app;
           this.doc = doc;
           this.defaultLevel = defaultLevel;
           this.warningSwallower = warningSwallower;
           this.trans = t;

            SetupBrushes();

       }
        */

        void SetupBrushes()
        {

            sharedInstance.errorBrush = new LinearGradientBrush();
            sharedInstance.errorBrush.StartPoint = new System.Windows.Point(0.5, 0);
            sharedInstance.errorBrush.EndPoint = new System.Windows.Point(0.5, 1);
            sharedInstance.errorBrush.GradientStops.Add(new GradientStop(colorRed1, 0.0));
            sharedInstance.errorBrush.GradientStops.Add(new GradientStop(colorRed2, .25));
            sharedInstance.errorBrush.GradientStops.Add(new GradientStop(colorRed2, 1.0));

            sharedInstance.activeBrush = new LinearGradientBrush();
            sharedInstance.activeBrush.StartPoint = new System.Windows.Point(0.5, 0);
            sharedInstance.activeBrush.EndPoint = new System.Windows.Point(0.5, 1);
            sharedInstance.activeBrush.GradientStops.Add(new GradientStop(colorOrange1, 0.0));
            sharedInstance.activeBrush.GradientStops.Add(new GradientStop(colorOrange2, .25));
            sharedInstance.activeBrush.GradientStops.Add(new GradientStop(colorOrange2, 1.0));

            sharedInstance.selectedBrush = new LinearGradientBrush();
            sharedInstance.selectedBrush.StartPoint = new System.Windows.Point(0.5, 0);
            sharedInstance.selectedBrush.EndPoint = new System.Windows.Point(0.5, 1);
            sharedInstance.selectedBrush.GradientStops.Add(new GradientStop(colorGreen1, 0.0));
            sharedInstance.selectedBrush.GradientStops.Add(new GradientStop(colorGreen2, .25));
            sharedInstance.selectedBrush.GradientStops.Add(new GradientStop(colorGreen2, 1.0));

            sharedInstance.deadBrush = new LinearGradientBrush();
            sharedInstance.deadBrush.StartPoint = new System.Windows.Point(0.5, 0);
            sharedInstance.deadBrush.EndPoint = new System.Windows.Point(0.5, 1);
            sharedInstance.deadBrush.GradientStops.Add(new GradientStop(colorGray1, 0.0));
            sharedInstance.deadBrush.GradientStops.Add(new GradientStop(colorGray2, .25));
            sharedInstance.deadBrush.GradientStops.Add(new GradientStop(colorGray2, 1.0));

        }
    }

    public class DynamoWarningSwallower : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(
            FailuresAccessor a)
        {
            // inside event handler, get all warnings

            IList<FailureMessageAccessor> failures
                = a.GetFailureMessages();

            foreach (FailureMessageAccessor f in failures)
            {
                // check failure definition ids
                // against ones to dismiss:

                FailureDefinitionId id
                    = f.GetFailureDefinitionId();

                if (BuiltInFailures.InaccurateFailures.InaccurateLine == id ||
                    BuiltInFailures.OverlapFailures.DuplicateInstances == id ||
                    BuiltInFailures.InaccurateFailures.InaccurateCurveBasedFamily == id ||
                    BuiltInFailures.InaccurateFailures.InaccurateBeamOrBrace == id ||
                    BuiltInFailures.InaccurateFailures.InaccurateLine == id
                    )
                {
                    a.DeleteWarning(f);
                }
                else
                {
                    a.RollBackPendingTransaction();
                }

            }
            return FailureProcessingResult.Continue;
        }
    }

    public class SelectionHelper
    {

        //RequestReferencePointSelection
        public static ReferencePoint RequestReferencePointSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                ReferencePoint rp = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                //create some geometry options so that we computer references
                Autodesk.Revit.DB.Options opts = new Options();
                opts.ComputeReferences = true;
                opts.DetailLevel = ViewDetailLevel.Medium;
                opts.IncludeNonVisibleObjects = false;

                Reference pointRef = doc.Selection.PickObject(ObjectType.Element);
                //Reference pointRef = IdlePromise<Reference>.ExecuteOnIdle(
                //    () => doc.Selection.PickObject(ObjectType.Element)
                //);


                if (pointRef != null)
                {
                    rp = settings.Doc.Document.GetElement(pointRef) as ReferencePoint;
                }
                return rp;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }


        }
        public static CurveElement RequestCurveElementSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                CurveElement c = null;
                Curve cv = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                dynElementSettings.SharedInstance.Bench.Log(message);

                Reference curveRef = doc.Selection.PickObject(ObjectType.Element);

                //c = curveRef.Element as ModelCurve;
                c = dynElementSettings.SharedInstance.Revit.ActiveUIDocument.Document.GetElement(curveRef) as CurveElement;

                if (c != null)
                {
                    cv = c.GeometryCurve;
                }
                return c;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }
        }


        public static CurveArray RequestMultipleCurveElementsSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                //CurveElement c = null;
                //Curve cv = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();


                //MessageBox.Show(message);
                dynElementSettings.SharedInstance.Bench.Log(message);

                CurveArray ca = new CurveArray();
                ISelectionFilter selFilter = new CurveSelectionFilter();
                IList<Element> eList = doc.Selection.PickElementsByRectangle(//selFilter,
                    "Select multiple curves") as IList<Element>;


                foreach (CurveElement c in eList)
                {
                    if (c != null)
                    {
                        ca.Append(c.GeometryCurve as Curve);
                    }
                }
                return ca;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }
        }

        public class CurveSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Model Lines" || element.Category.Name == "Lines")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        public static Face RequestFaceSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                Face f = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                //create some geometry options so that we computer references
                Autodesk.Revit.DB.Options opts = new Options();
                opts.ComputeReferences = true;
                opts.DetailLevel = ViewDetailLevel.Medium;
                opts.IncludeNonVisibleObjects = false;

                Reference faceRef = doc.Selection.PickObject(ObjectType.Face);

                if (faceRef != null)
                {

                    GeometryObject geob = settings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);

                    f = geob as Face;
                }
                return f;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }


        }


        // MDJ TODO - this is really hacky. I want to just use the face but evaluating the ref fails later on in pointOnSurface, the ref just returns void, not sure why.

        public static Reference RequestFaceReferenceSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                Face f = null;
                //Reference r = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                //create some geometry options so that we computer references
                Autodesk.Revit.DB.Options opts = new Options();
                opts.ComputeReferences = true;
                opts.DetailLevel = ViewDetailLevel.Medium;
                opts.IncludeNonVisibleObjects = false;

                Reference faceRef = doc.Selection.PickObject(ObjectType.Face);

                if (faceRef != null)
                {

                    GeometryObject geob = settings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);

                    f = geob as Face;
                }
                //return f;
                return faceRef;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }


        }
        public static Form RequestFormSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                Form f = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                //create some geometry options so that we computer references
                Autodesk.Revit.DB.Options opts = new Options();
                opts.ComputeReferences = true;
                opts.DetailLevel = ViewDetailLevel.Medium;
                opts.IncludeNonVisibleObjects = false;

                Reference formRef = doc.Selection.PickObject(ObjectType.Element);

                if (formRef != null)
                {
                    //the suggested new method didn't exist in API?
                    f = settings.Doc.Document.GetElement(formRef) as Form;
                }
                return f;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }


        }


        public static FamilySymbol RequestFamilySymbolByInstanceSelection(UIDocument doc, string message,
            dynElementSettings settings, ref FamilyInstance fi)
        {
            try
            {
                //FamilySymbol fs = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    fi = doc.Document.GetElement(fsRef) as FamilyInstance;

                    if (fi != null)
                    {
                        return fi.Symbol;
                    }
                    else return null;
                }
                else return null;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }
        }

        public static FamilyInstance RequestFamilyInstanceSelection(UIDocument doc, string message,
            dynElementSettings settings)
        {
            try
            {
                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    return doc.Document.GetElement(fsRef.ElementId) as FamilyInstance;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }
        }


        public static Element RequestLevelSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {
                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                settings.Bench.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    return doc.Document.GetElement(fsRef.ElementId) as Level;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }
        }


        public static Element RequestAnalysisResultInstanceSelection(UIDocument doc, string message, dynElementSettings settings)
        {
            try
            {

                View view = doc.ActiveView as View;

                SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view);
                Element AnalysisResult;

                if (sfm != null)
                {
                    sfm.GetRegisteredResults();

                    Selection choices = doc.Selection;

                    choices.Elements.Clear();

                    //MessageBox.Show(message);
                    settings.Bench.Log(message);

                    Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                    if (fsRef != null)
                    {
                        AnalysisResult = doc.Document.GetElement(fsRef.ElementId) as Element;

                        if (AnalysisResult != null)
                        {
                            return AnalysisResult;
                        }
                        else return null;
                    }
                    else return null;
                }
                else return null;
            }
            catch (Exception ex)
            {
                settings.Bench.Log(ex);
                return null;
            }
        }
    }


}