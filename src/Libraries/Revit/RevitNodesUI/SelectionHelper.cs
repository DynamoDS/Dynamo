using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using Dynamo.Interfaces;
using RevitServices.Persistence;

namespace Revit.Interactivity
{
    internal class SelectionHelper : IModelSelectionHelper
    {
        private static readonly SelectionHelper instance = new SelectionHelper();

        public static SelectionHelper Instance
        {
            get { return instance; }
        }

        public List<string> RequestElementSelection<T>(string selectionMessage, out object selectionTarget, 
            SelectionType selectionType, SelectionObjectType objectType, ILogger logger)
        {
            selectionTarget = null;

            switch(selectionType)
            {
                case SelectionType.One:
                    return RequestElementSelection<T>(selectionMessage, out selectionTarget, logger);
                case SelectionType.Many:
                    return RequestMultipleElementsSelection<T>(
                        selectionMessage,
                        out selectionTarget,
                        logger);
            }

            return null;
        }

        public List<string> RequestElementSubSelection<T>(
            string selectionMessage, out object selectionTarget, SelectionType selectionType,
            SelectionObjectType objectType, ILogger logger)
        {
            selectionTarget = null;
            List<string> selection = null;
            
            switch (selectionType)
            {
                case SelectionType.One:
                    selection = RequestElementSelection<T>(selectionMessage, out selectionTarget, logger);
                    break;
                case SelectionType.Many:
                    selection = RequestMultipleElementsSelection<T>(
                        selectionMessage,
                        out selectionTarget,
                        logger);
                    break;
            }

            // Process the sub-selection of elements for a type
            Func<List<string>, List<string>> subSelectionFunction = null;

            if (typeof(T) == typeof(DividedSurface))
            {
                subSelectionFunction = GetFamilyInstancesFromDividedSurface;
            }

            return subSelectionFunction == null
                ? selection
                : subSelectionFunction.Invoke(selection);
        }

        public List<string> RequestReferenceSelection(string selectionMessage, out object selectionTarget, 
            SelectionType selectionType, SelectionObjectType objectType, ILogger logger)
        {
            selectionTarget = null;

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

        /// <summary>
        /// Request an element in a selection.
        /// </summary>
        /// <typeparam name="T">The type of the Element.</typeparam>
        /// <param name="selectionMessage">The message to display.</param>
        /// <param name="logger">A logger.</param>
        /// <returns></returns>
        private static List<string> RequestElementSelection<T>(string selectionMessage, out object selectionTarget, ILogger logger)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            Element e = null;
            selectionTarget = null;

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
                selectionTarget = e;
            }

            return new List<string>() { e.UniqueId };
        }

        /// <summary>
        /// Request multiple elements in a selection
        /// </summary>
        /// <typeparam name="T">The type of the Elements.</typeparam>
        /// <param name="message">The message to display.</param>
        /// <param name="selectionTarget">An object which, when modified, should update this selection.</param>
        /// <param name="logger">A logger.</param>
        /// <returns></returns>
        private static List<string> RequestMultipleElementsSelection<T>(string selectionMessage, out object selectionTarget, ILogger logger)
        {
            var doc = DocumentManager.Instance.CurrentUIDocument;

            var choices = doc.Selection;
            choices.Elements.Clear();

            logger.Log(selectionMessage);

            // Don't pass anything back, as everything we care about will be
            // passed in the element list.
            selectionTarget = null;

            var elementRefs = doc.Selection.PickElementsByRectangle(
                new ElementSelectionFilter<T>(),
                selectionMessage);

            return elementRefs.Select(x => x.UniqueId).ToList();

        }

        /// <summary>
        /// Request a selection and return a stable reference to that reference.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="selectionType">The type of reference selection.</param>
        /// <returns></returns>
        private static List<string> RequestReferenceSelection(string message, ILogger logger, SelectionObjectType selectionType)
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

            return new List<string>() { reference.ConvertToStableRepresentation(doc.Document) };

        }

        /// <summary>
        /// Request a selection and return a stable reference to that reference.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="selectionType">The type of reference selection.</param>
        /// <returns></returns>
        private static List<string> RequestMultipleReferencesSelection(string message, ILogger logger, SelectionObjectType selectionType)
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

            return references.Select(r => r.ConvertToStableRepresentation(doc.Document)).ToList();

        }

        private static List<string> GetFamilyInstancesFromDividedSurface(List<string> uuids)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var divSurfs =
                uuids.Select(doc.GetElement).Where(el => el != null).Cast<DividedSurface>();

            var result = new List<string>();

            foreach (var ds in divSurfs)
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
                                result.Add(fi.UniqueId);
                            }
                        }
                        v = v + 1;
                    }

                    u = u + 1;
                } 
            }
            
            return result;
        }

        #endregion

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
}
