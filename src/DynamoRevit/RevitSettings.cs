#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Dynamo.Revit;
using RevitServices.Persistence;

#endregion

namespace Dynamo.Utilities
{
    internal class dynUtils
    {
        /// <summary>
        ///     Utility function to determine if an Element of the given ID exists in the document.
        /// </summary>
        /// <returns>True if exists, false otherwise.</returns>
        public static bool TryGetElement<T>(ElementId id, out T e) where T : Element
        {
            try
            {
                e = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(id) as T;
                return e != null && e.Id != null;
            }
            catch
            {
                e = null;
                return false;
            }
        }

        /// <summary>
        ///     Makes a new generic IEnumerable instance out of a non-generic one.
        /// </summary>
        /// <typeparam name="T">The out-type of the new IEnumerable</typeparam>
        /// <param name="en">Non-generic IEnumerable</param>
        /// <returns></returns>
        public static IEnumerable<T> MakeEnumerable<T>(IEnumerable en)
        {
            return en.Cast<T>();
        }

        /// <summary>
        ///     Makes a new generic IEnumerable instance out of a non-generic one.
        /// </summary>
        /// <param name="en">Non-generic IEnumerable</param>
        /// <returns></returns>
        public static IEnumerable<object> MakeEnumerable(IEnumerable en)
        {
            return MakeEnumerable<object>(en);
        }


        /// <summary>
        ///     Creates a sketch plane by projecting one point's z coordinate down to the other's z coordinate.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="doc"></param>
        /// <param name="pt1">The start point</param>
        /// <param name="pt2">The end point</param>
        /// <returns></returns>
        public static SketchPlane CreateSketchPlaneForModelCurve(
            UIApplication app, UIDocument doc, XYZ pt1, XYZ pt2)
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

            //TODO: 2014->2015
            //SketchPlane sp = doc.Document.Create.NewSketchPlane(p);
            var sp = Autodesk.Revit.DB.SketchPlane.Create(doc.Document, p);

            return sp;
        }
    }

    public static class dynRevitSettings
    {
        private static Options geometryOptions;

        public static Stack<ElementsContainer> ElementsContainers =
            new Stack<ElementsContainer>(new[] { new ElementsContainer() });

        public static Element SpatialFieldManagerUpdated { get; set; }
        public static Level DefaultLevel { get; set; }
        public static DynamoWarningSwallower WarningSwallower { get; set; }

        public static Options GeometryOptions
        {
            get
            {
                if (geometryOptions == null)
                {
                    geometryOptions = new Options
                    {
                        ComputeReferences = true,
                        DetailLevel = ViewDetailLevel.Medium,
                        IncludeNonVisibleObjects = false
                    };
                }

                return geometryOptions;
            }
        }

        public static DynamoController_Revit Controller { get; internal set; }

        public class DynamoWarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
            {
                // inside event handler, get all warnings

                IList<FailureMessageAccessor> failures = a.GetFailureMessages();

                foreach (FailureMessageAccessor f in failures)
                {
                    // check failure definition ids
                    // against ones to dismiss:

                    FailureDefinitionId id = f.GetFailureDefinitionId();

                    if (BuiltInFailures.InaccurateFailures.InaccurateLine == id
                        || BuiltInFailures.OverlapFailures.DuplicateInstances == id
                        || BuiltInFailures.InaccurateFailures.InaccurateCurveBasedFamily == id
                        || BuiltInFailures.InaccurateFailures.InaccurateBeamOrBrace == id
                        || BuiltInFailures.InaccurateFailures.InaccurateLine == id)
                        a.DeleteWarning(f);
                    else
                        a.RollBackPendingTransaction();
                }
                return FailureProcessingResult.Continue;
            }
        }

        public class SelectionHelper
        {
            public static ReferencePoint RequestReferencePointSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                ReferencePoint rp = null;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                //create some geometry options so that we computer references
                //var opts = new Options
                //{
                //    ComputeReferences = true,
                //    DetailLevel = ViewDetailLevel.Medium,
                //    IncludeNonVisibleObjects = false
                //};

                Reference pointRef = doc.Selection.PickObject(ObjectType.Element);

                if (pointRef != null)
                {
                    rp =
                        DocumentManager.Instance.CurrentUIDocument.Document.GetElement(pointRef) as
                            ReferencePoint;
                }
                return rp;
            }

            public static CurveElement RequestCurveElementSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                dynSettings.DynamoLogger.Log(message);

                Reference curveRef = doc.Selection.PickObject(ObjectType.Element);

                var c = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument.Document.GetElement(
                    curveRef) as CurveElement;

                return c;
            }

            public static IList<Element> RequestMultipleCurveElementsSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                //var ca = new ElementArray();
                //ISelectionFilter selFilter = new CurveSelectionFilter();
                return doc.Selection.PickElementsByRectangle(
                    //selFilter,
                    "Window select multiple curves.");
            }

            public static Face RequestFaceSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Face f = null;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                Reference faceRef = doc.Selection.PickObject(ObjectType.Face);

                if (faceRef != null)
                {
                    GeometryObject geob =
                        DocumentManager.Instance.CurrentUIDocument.Document.GetElement(faceRef)
                            .GetGeometryObjectFromReference(faceRef);
                    f = geob as Face;
                }
                return f;
            }

            // MDJ TODO - this is really hacky. I want to just use the face but evaluating the ref fails later on in pointOnSurface, the ref just returns void, not sure why.
            public static Reference RequestFaceReferenceSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);
                Reference faceRef = doc.Selection.PickObject(ObjectType.Face);

                return faceRef;
            }

            public static Reference RequestEdgeReferenceSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                Reference edgeRef = doc.Selection.PickObject(ObjectType.Edge);

                return edgeRef;
            }

            public static Form RequestFormSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Form f = null;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                Reference formRef = doc.Selection.PickObject(ObjectType.Element);

                if (formRef != null)
                {
                    //the suggested new method didn't exist in API?
                    f = DocumentManager.Instance.CurrentUIDocument.Document.GetElement(formRef) as Form;
                }
                return f;
            }

            public static FamilySymbol RequestFamilySymbolByInstanceSelection(
                string message, ref FamilyInstance fi)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                try
                {
                    //FamilySymbol fs = null;

                    Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                    choices.Elements.Clear();

                    //MessageBox.Show(message);
                    dynSettings.DynamoLogger.Log(message);

                    Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                    if (fsRef != null)
                    {
                        fi = doc.Document.GetElement(fsRef) as FamilyInstance;

                        if (fi != null)
                            return fi.Symbol;
                        return null;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    dynSettings.DynamoLogger.Log(ex);
                    return null;
                }
            }

            public static FamilyInstance RequestFamilyInstanceSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                try
                {
                    Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                    choices.Elements.Clear();

                    //MessageBox.Show(message);
                    dynSettings.DynamoLogger.Log(message);

                    Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                    if (fsRef != null)
                        return doc.Document.GetElement(fsRef.ElementId) as FamilyInstance;
                    return null;
                }
                catch (Exception ex)
                {
                    dynSettings.DynamoLogger.Log(ex);
                    return null;
                }
            }

            public static Element RequestLevelSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Level l = null;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                dynSettings.DynamoLogger.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                    l = (Level)doc.Document.GetElement(fsRef.ElementId);

                return l;
            }

            public static Element RequestAnalysisResultInstanceSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                try
                {
                    View view = doc.ActiveView;

                    SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view);

                    if (sfm != null)
                    {
                        sfm.GetRegisteredResults();

                        Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                        choices.Elements.Clear();

                        //MessageBox.Show(message);
                        dynSettings.DynamoLogger.Log(message);

                        Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                        if (fsRef != null)
                        {
                            Element analysisResult = doc.Document.GetElement(fsRef.ElementId);

                            return analysisResult;
                        }
                        return null;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    dynSettings.DynamoLogger.Log(ex);
                    return null;
                }
            }

            public static Element RequestModelElementSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Element selectedElement = null;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    selectedElement = doc.Document.GetElement(fsRef.ElementId);
                    if (selectedElement is FamilyInstance || selectedElement is HostObject
                        || selectedElement is ImportInstance || selectedElement is CombinableElement)
                        return selectedElement;
                }

                return selectedElement;
            }

            public static Reference RequestReferenceXYZSelection(string message)
            {
                UIDocument doc = DocumentManager.Instance.CurrentUIDocument;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
                choices.Elements.Clear();

                dynSettings.DynamoLogger.Log(message);

                Reference xyzRef = doc.Selection.PickObject(ObjectType.PointOnElement);

                return xyzRef;
            }

            public class CurveSelectionFilter : ISelectionFilter
            {
                public bool AllowElement(Element element)
                {
                    if (element.Category.Name == "Model Lines" || element.Category.Name == "Lines")
                        return true;
                    return false;
                }

                public bool AllowReference(Reference refer, XYZ point)
                {
                    return false;
                }
            }
        }
    }
}
