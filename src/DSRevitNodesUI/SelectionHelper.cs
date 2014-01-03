using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI.Selection;
using Dynamo;
using RevitServices.Persistence;

namespace DSRevitNodes.Interactivity
{
    internal class SelectionHelper
    {
        public static Autodesk.Revit.DB.ReferencePoint RequestReferencePointSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            ReferencePoint rp = null;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            //create some geometry options so that we computer references
            var opts = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            Reference pointRef = doc.Selection.PickObject(ObjectType.Element);

            if (pointRef != null)
            {
                rp = DocumentManager.GetInstance().CurrentDBDocument.GetElement(pointRef) as ReferencePoint;
            }
            return rp;
        }

        public static CurveElement RequestCurveElementSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            CurveElement c = null;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

            choices.Elements.Clear();

            //MessageBox.Show(message);
            DynamoLogger.Instance.Log(message);

            Reference curveRef = doc.Selection.PickObject(ObjectType.Element);

            c = DocumentManager.GetInstance().CurrentDBDocument.GetElement(curveRef) as CurveElement;

            return c;
        }

        public static List<Element> RequestMultipleCurveElementsSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            var ca = new ElementArray();
            ISelectionFilter selFilter = new CurveSelectionFilter();
            return doc.Selection.PickElementsByRectangle(//selFilter,
                "Window select multiple curves.").ToList();

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

        public static Face RequestFaceSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Face f = null;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            //create some geometry options so that we computer references
            var opts = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            Reference faceRef = doc.Selection.PickObject(ObjectType.Face);

            if (faceRef != null)
            {
                GeometryObject geob = DocumentManager.GetInstance().CurrentDBDocument.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                f = geob as Face;
            }
            return f;

        }

        public static Reference RequestFaceReferenceSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Reference faceRef = null;

            var choices = doc.Selection;
            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);
            faceRef = doc.Selection.PickObject(ObjectType.Face);

            return faceRef;
        }

        public static Reference RequestEdgeReferenceSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            Reference edgeRef = doc.Selection.PickObject(ObjectType.Edge);

            return edgeRef;
        }

        public static Form RequestFormSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Form f = null;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            //create some geometry options so that we computer references
            var opts = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = false
            };

            Reference formRef = doc.Selection.PickObject(ObjectType.Element);

            if (formRef != null)
            {
                //the suggested new method didn't exist in API?
                f = DocumentManager.GetInstance().CurrentDBDocument.GetElement(formRef) as Form;
            }
            return f;
        }

        public static FamilySymbol RequestFamilySymbolByInstanceSelection(string message, ref FamilyInstance fi)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            try
            {
                //FamilySymbol fs = null;

                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                DynamoLogger.Instance.Log(message);

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
                DynamoLogger.Instance.Log(ex);
                return null;
            }
        }

        public static FamilyInstance RequestFamilyInstanceSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            try
            {
                Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                DynamoLogger.Instance.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    var inst = doc.Document.GetElement(fsRef.ElementId) as FamilyInstance;
                    return inst;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log(ex);
                return null;
            }
        }

        public static Level RequestLevelSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Level l = null;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

            choices.Elements.Clear();

            //MessageBox.Show(message);
            DynamoLogger.Instance.Log(message);

            Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

            if (fsRef != null)
            {
                l = (Level)doc.Document.GetElement(fsRef.ElementId);
            }

            return l;
        }

        public static Element RequestAnalysisResultInstanceSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            try
            {

                View view = doc.ActiveView as View;

                SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view);
                Element AnalysisResult;

                if (sfm != null)
                {
                    sfm.GetRegisteredResults();

                    Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

                    choices.Elements.Clear();

                    //MessageBox.Show(message);
                    DynamoLogger.Instance.Log(message);

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
                DynamoLogger.Instance.Log(ex);
                return null;
            }
        }

        public static Element RequestModelElementSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Element selectedElement = null;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;

            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

            if (fsRef != null)
            {
                selectedElement = doc.Document.GetElement(fsRef.ElementId);
                if (selectedElement is FamilyInstance || selectedElement is HostObject ||
                        selectedElement is ImportInstance ||
                        selectedElement is CombinableElement)
                    return selectedElement;
            }

            return selectedElement;
        }

        public static Reference RequestReferenceXYZSelection(string message)
        {
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            Autodesk.Revit.UI.Selection.Selection choices = doc.Selection;
            choices.Elements.Clear();

            DynamoLogger.Instance.Log(message);

            Reference xyzRef = doc.Selection.PickObject(ObjectType.PointOnElement);

            return xyzRef;
        }

        public static List<Element> RequestDividedSurfaceFamilyInstancesSelection(string message)
        {
            var form = RequestFormSelection(message);

            var result = new List<Element>();

            var dsd = form.GetDividedSurfaceData();

            if (dsd == null)
                throw new Exception("The selected form has no divided surface data.");

            foreach (Reference r in dsd.GetReferencesWithDividedSurfaces())
            {
                var ds = dsd.GetDividedSurfaceForReference(r);

                var gn = new GridNode();

                int u = 0;
                while (u < ds.NumberOfUGridlines)
                {
                    gn.UIndex = u;

                    int v = 0;
                    while (v < ds.NumberOfVGridlines)
                    {
                        gn.VIndex = v;

                        //"Reports whether a grid node is a "seed node," a node that is associated with one or more tiles."
                        if (ds.IsSeedNode(gn))
                        {
                            var fi = ds.GetTileFamilyInstance(gn, 0);

                            //put the family instance into the tree
                            result.Add(fi);
                        }
                        v = v + 1;
                    }

                    u = u + 1;
                }
            }

            return result;
        }
    }
}
