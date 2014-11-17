using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using Dynamo.Interfaces;
using RevitServices.Persistence;

namespace Revit.Interactivity
{
    internal class RevitReferenceSelectionHelper : IModelSelectionHelper<Reference>
    {
        private static readonly RevitReferenceSelectionHelper instance =
            new RevitReferenceSelectionHelper();

        public static RevitReferenceSelectionHelper Instance
        {
            get { return instance; }
        }

        public IEnumerable<Reference> RequestSelectionOfType(
            string selectionMessage, SelectionType selectionType, SelectionObjectType objectType,
            ILogger logger)
        {
            switch (selectionType)
            {
                case SelectionType.One:
                    return RequestReferenceSelection(selectionMessage, logger, objectType);

                case SelectionType.Many:
                    return RequestMultipleReferencesSelection(selectionMessage, logger, objectType);
            }

            return null;
        }

        #region private static methods

        private static IEnumerable<Reference> RequestReferenceSelection(
            string message, ILogger logger, SelectionObjectType selectionType)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Reference reference = null;

            var choices = doc.Selection;
            choices.Elements.Clear();

            logger.Log(message);

            switch (selectionType)
            {
                case SelectionObjectType.Face:
                    reference = doc.Selection.PickObject(ObjectType.Face, message);
                    break;
                case SelectionObjectType.Edge:
                    reference = doc.Selection.PickObject(ObjectType.Edge, message);
                    break;
                case SelectionObjectType.PointOnFace:
                    reference = doc.Selection.PickObject(ObjectType.PointOnElement, message);
                    break;
            }

            return reference == null ? null : new List<Reference> { reference };
        }

        private static IEnumerable<Reference> RequestMultipleReferencesSelection(
            string message, ILogger logger, SelectionObjectType selectionType)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            IList<Reference> references = null;

            var choices = doc.Selection;
            choices.Elements.Clear();

            logger.Log(message);

            switch (selectionType)
            {
                case SelectionObjectType.Face:
                    references = doc.Selection.PickObjects(ObjectType.Face, message);
                    break;
                case SelectionObjectType.Edge:
                    references = doc.Selection.PickObjects(ObjectType.Edge, message);
                    break;
                case SelectionObjectType.PointOnFace:
                    references = doc.Selection.PickObjects(ObjectType.PointOnElement, message);
                    break;
            }

            if (references == null || !references.Any())
                return null;

            return references;
        }

        #endregion
    }

    internal class RevitElementSelectionHelper<T> : IModelSelectionHelper<T> where T : Element
    {
        private static readonly RevitElementSelectionHelper<T> instance = new RevitElementSelectionHelper<T>();

        public static RevitElementSelectionHelper<T> Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Request an element in a selection.
        /// </summary>
        /// <typeparam name="T">The type of the Element.</typeparam>
        /// <param name="selectionMessage">The message to display.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="selectionType">The selection type.</param>
        /// <param name="objectType">The selection object type.</param>
        /// <returns></returns>
        public IEnumerable<T> RequestSelectionOfType(
            string selectionMessage, SelectionType selectionType, SelectionObjectType objectType,
            ILogger logger)
        {
            switch (selectionType)
            {
                case SelectionType.One:
                    return RequestElementSelection(selectionMessage, logger);

                case SelectionType.Many:
                    return RequestMultipleElementsSelection(selectionMessage, logger);
            }

            return null;
        }

        public static IEnumerable<Element> GetFamilyInstancesFromDividedSurface(DividedSurface ds)
        {
            var gn = new GridNode();

            var u = 0;
            while (u < ds.NumberOfUGridlines)
            {
                gn.UIndex = u;

                var v = 0;

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
                            yield return fi;
                        }
                    }
                    v = v + 1;
                }

                u = u + 1;
            }
        }

        #region private static methods

        private static IEnumerable<T> RequestElementSelection(string selectionMessage, ILogger logger)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Element e = null;

            var choices = doc.Selection;
            choices.Elements.Clear();

            logger.Log(selectionMessage);

            var elementRef = doc.Selection.PickObject(
                ObjectType.Element,
                new ElementSelectionFilter<T>(),
                selectionMessage);

            if (elementRef != null)
            {
                e = DocumentManager.Instance.CurrentDBDocument.GetElement(elementRef);
            }

            return new[] { e }.Cast<T>();
        }

        private static IEnumerable<T> RequestMultipleElementsSelection(
            string selectionMessage, ILogger logger)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            var choices = doc.Selection;
            choices.Elements.Clear();

            logger.Log(selectionMessage);

            var elements = doc.Selection.PickElementsByRectangle(
                new ElementSelectionFilter<T>(),
                selectionMessage);

            return elements.Cast<T>();
        }

        #endregion
    }

    internal class ElementSelectionFilter<T> : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is T;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    #region junk

    //public static Reference RequestFaceReferenceSelection(string message, ILogger logger)
    //{
    //    var doc = DocumentManager.Instance.CurrentUIDocument;

    //    Reference faceRef = null;

    //    var choices = doc.Selection;
    //    choices.Elements.Clear();

    //    logger.Log(message);
    //    faceRef = doc.Selection.PickObject(ObjectType.Face);

    //    return faceRef;
    //}

    //public static Reference RequestEdgeReferenceSelection(string message, ILogger logger)
    //{
    //    var doc = DocumentManager.Instance.CurrentUIDocument;

    //    var choices = doc.Selection;
    //    choices.Elements.Clear();

    //    logger.Log(message);
    //    var edgeRef = doc.Selection.PickObject(ObjectType.Edge);

    //    return edgeRef;
    //}

    //public static DividedSurface RequestDividedSurfaceSelection(string message, ILogger logger)
    //{
    //    var doc = DocumentManager.Instance.CurrentUIDocument;

    //    DividedSurface f = null;

    //    var choices = doc.Selection;

    //    choices.Elements.Clear();

    //    logger.Log(message);

    //    var formRef = doc.Selection.PickObject(
    //        ObjectType.Element,
    //        new DividedSurfaceSelectionFilter());

    //    if (formRef != null)
    //    {
    //        //get the element
    //        var el = DocumentManager.Instance.CurrentDBDocument.GetElement(formRef);
    //        f = (DividedSurface)el;
    //    }
    //    return f;
    //}

    //public static ElementId RequestAnalysisResultInstanceSelection(
    //    string message, ILogger logger)
    //{
    //    var doc = DocumentManager.Instance.CurrentUIDocument;

    //    try
    //    {
    //        var view = doc.ActiveView;

    //        var sfm = SpatialFieldManager.GetSpatialFieldManager(view);

    //        if (sfm != null)
    //        {
    //            sfm.GetRegisteredResults();

    //            var choices = doc.Selection;

    //            choices.Elements.Clear();

    //            logger.Log(message);

    //            var fsRef = doc.Selection.PickObject(ObjectType.Element);

    //            if (fsRef != null)
    //            {
    //                var analysisResult = doc.Document.GetElement(fsRef.ElementId);

    //                return analysisResult.Id;
    //            }
    //            return null;
    //        }
    //        return null;
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.Log(ex);
    //        return null;
    //    }
    //}

    //public static ElementId RequestModelElementSelection(string message, ILogger logger)
    //{
    //    var doc = DocumentManager.Instance.CurrentUIDocument;

    //    Element selectedElement = null;

    //    var choices = doc.Selection;
    //    choices.Elements.Clear();

    //    logger.Log(message);

    //    var fsRef = doc.Selection.PickObject(ObjectType.Element);
    //    if (fsRef != null)
    //    {
    //        selectedElement = doc.Document.GetElement(fsRef.ElementId);
    //        if (selectedElement is FamilyInstance || selectedElement is HostObject ||
    //            selectedElement is ImportInstance ||
    //            selectedElement is CombinableElement)
    //            return selectedElement.Id;
    //    }

    //    return selectedElement != null ? selectedElement.Id : null;
    //}

    //public static Reference RequestReferenceXYZSelection(string message, ILogger logger)
    //{
    //    var doc = DocumentManager.Instance.CurrentUIDocument;

    //    var choices = doc.Selection;
    //    choices.Elements.Clear();

    //    logger.Log(message);

    //    var xyzRef = doc.Selection.PickObject(ObjectType.PointOnElement);

    //    return xyzRef;
    //}

    //public class CurveSelectionFilter : ISelectionFilter
    //{
    //    public bool AllowElement(Element element)
    //    {
    //        if (element.Category.Name == "Model Lines" || element.Category.Name == "Lines")
    //            return true;
    //        return false;
    //    }

    //    public bool AllowReference(Reference refer, XYZ point)
    //    {
    //        return false;
    //    }
    //}

    //public class DividedSurfaceSelectionFilter : ISelectionFilter
    //{
    //    public bool AllowElement(Element elem)
    //    {
    //        if (elem is DividedSurface)
    //            return true;
    //        return false;
    //    }

    //    public bool AllowReference(Reference reference, XYZ position)
    //    {
    //        return false;
    //    }
    //}


    #endregion
}
