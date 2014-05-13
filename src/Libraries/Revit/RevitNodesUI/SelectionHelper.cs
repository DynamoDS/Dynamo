﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI.Selection;
using Dynamo;
using Dynamo.Utilities;
using RevitServices.Persistence;

namespace Revit.Interactivity
{
    internal class SelectionHelper
    {
        public static ReferencePoint RequestReferencePointSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            ReferencePoint rp = null;

            Selection choices = doc.Selection;

            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

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
                rp = DocumentManager.Instance.CurrentDBDocument.GetElement(pointRef) as ReferencePoint;
            }
            return rp;
        }

        public static CurveElement RequestCurveElementSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Selection choices = doc.Selection;

            choices.Elements.Clear();

            //MessageBox.Show(message);
            dynSettings.DynamoLogger.Log(message);

            Reference curveRef = doc.Selection.PickObject(ObjectType.Element);
            
            return DocumentManager.Instance.CurrentDBDocument.GetElement(curveRef) as CurveElement;
        }

        public static List<ElementId> RequestMultipleCurveElementsSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Selection choices = doc.Selection;
            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

            var ca = new ElementArray();
            ISelectionFilter selFilter = new CurveSelectionFilter();
            return doc.Selection.PickElementsByRectangle(//selFilter,
                "Window select multiple curves.").Select(x => x.Id).ToList();

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

        public class DividedSurfaceSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is DividedSurface)
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        public static Face RequestFaceSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Face f = null;

            Selection choices = doc.Selection;

            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

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
                GeometryObject geob = DocumentManager.Instance.CurrentDBDocument.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                f = geob as Face;
            }
            return f;

        }

        public static Reference RequestFaceReferenceSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Reference faceRef = null;

            var choices = doc.Selection;
            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);
            faceRef = doc.Selection.PickObject(ObjectType.Face);

            return faceRef;
        }

        public static Reference RequestEdgeReferenceSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Selection choices = doc.Selection;
            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);
            Reference edgeRef = doc.Selection.PickObject(ObjectType.Edge);

            return edgeRef;
        }

        public static Form RequestFormSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Form f = null;

            Selection choices = doc.Selection;

            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

            Reference formRef = doc.Selection.PickObject(ObjectType.Element);

            if (formRef != null)
            {
                //get the element
                var el = DocumentManager.Instance.CurrentDBDocument.GetElement(formRef);
                f = el as Form;
            }
            return f;
        }

        public static DividedSurface RequestDividedSurfaceSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            DividedSurface f = null;

            Selection choices = doc.Selection;

            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

            Reference formRef = doc.Selection.PickObject(ObjectType.Element, new DividedSurfaceSelectionFilter());

            if (formRef != null)
            {
                //get the element
                var el = DocumentManager.Instance.CurrentDBDocument.GetElement(formRef);
                f = (DividedSurface)el;
            }
            return f;
        }

        public static FamilySymbol RequestFamilySymbolByInstanceSelection(string message, ref FamilyInstance fi)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            try
            {
                //FamilySymbol fs = null;

                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                dynSettings.DynamoLogger.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    fi = doc.Document.GetElement(fsRef) as FamilyInstance;

                    if (fi != null)
                    {
                        return fi.Symbol;
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

        public static FamilyInstance RequestFamilyInstanceSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            try
            {
                Selection choices = doc.Selection;

                choices.Elements.Clear();

                //MessageBox.Show(message);
                dynSettings.DynamoLogger.Log(message);

                Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                if (fsRef != null)
                {
                    var inst = doc.Document.GetElement(fsRef.ElementId) as FamilyInstance;
                    return inst;
                }
                return null;
            }
            catch (Exception ex)
            {
                dynSettings.DynamoLogger.Log(ex);
                return null;
            }
        }

        public static Level RequestLevelSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Level l = null;

            Selection choices = doc.Selection;

            choices.Elements.Clear();

            //MessageBox.Show(message);
            dynSettings.DynamoLogger.Log(message);

            Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

            if (fsRef != null)
            {
                l = (Level)doc.Document.GetElement(fsRef.ElementId);
            }

            return l;
        }

        public static ElementId RequestAnalysisResultInstanceSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;
            
            try
            {

                var view = doc.ActiveView;

                SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view);

                if (sfm != null)
                {
                    sfm.GetRegisteredResults();

                    Selection choices = doc.Selection;

                    choices.Elements.Clear();

                    //MessageBox.Show(message);
                    dynSettings.DynamoLogger.Log(message);

                    Reference fsRef = doc.Selection.PickObject(ObjectType.Element);

                    if (fsRef != null)
                    {
                        var analysisResult = doc.Document.GetElement(fsRef.ElementId);

                        return analysisResult.Id;
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

        public static ElementId RequestModelElementSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Element selectedElement = null;

            Selection choices = doc.Selection;
            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

            Reference fsRef = doc.Selection.PickObject(ObjectType.Element);
            if (fsRef != null)
            {
                selectedElement = doc.Document.GetElement(fsRef.ElementId);
                if (selectedElement is FamilyInstance || selectedElement is HostObject ||
                        selectedElement is ImportInstance ||
                        selectedElement is CombinableElement)
                    return selectedElement.Id;
            }

            return selectedElement != null ? selectedElement.Id : null;
        }

        public static Reference RequestReferenceXYZSelection(string message)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Selection choices = doc.Selection;
            choices.Elements.Clear();

            dynSettings.DynamoLogger.Log(message);

            Reference xyzRef = doc.Selection.PickObject(ObjectType.PointOnElement);

            return xyzRef;
        }

        public static List<ElementId> RequestDividedSurfaceFamilyInstancesSelection(string message)
        {
            var ds = RequestDividedSurfaceSelection(message);

            var result = new List<ElementId>();

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

                        if (fi != null)
                        {
                            //put the family instance into the tree
                            result.Add(fi.Id);
                        }
                    }
                    v = v + 1;
                }

                u = u + 1;
            }

            return result;
        }
    }
}
