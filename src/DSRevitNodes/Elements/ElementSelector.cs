
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;

namespace DSRevitNodes.Elements
{
    [Browsable(false)]
    public static class ElementSelector
    {
        /// <summary>
        /// Get a collection of wrapped elements from the current document by type
        /// </summary>
        /// <typeparam name="T">The type of the Revit element to select</typeparam>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static IEnumerable<AbstractElement> ByType<T>(bool isRevitOwned) where T : Autodesk.Revit.DB.Element
        {
            return DocumentManager.GetInstance().ElementsOfType<T>().Select(x => WrapElement(x, isRevitOwned));
        }

        /// <summary>
        /// Get a collection of wrapped elements from the current document by id.
        /// </summary>
        /// <param name="elementIds">A list of element ids.</param>
        /// <param name="areRevitOwned">Whether the object returned should be revit owned or not.</param>
        /// <returns></returns>
        public static IEnumerable<AbstractElement> ByElementIds(IList<int> elementIds, bool areRevitOwned)
        {
            var abEls = new List<AbstractElement>();

            foreach (var id in elementIds)
            {
                var ele = InternalGetElementById(id);
                if (ele != null)
                {
                    abEls.Add(WrapElement(ele, areRevitOwned));
                }

                throw new Exception("Could not get the element from the document.");
            }

            return abEls;
        }

        /// <summary>
        /// A factory method for looking up and obtaining elements
        /// from the revit project.
        /// </summary>
        /// <param name="elementId">The id of the element to select</param>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static AbstractElement ByElementId(int elementId, bool isRevitOwned)
        {
            var ele = InternalGetElementById(elementId);

            if (ele != null)
            {
                return WrapElement(ele, isRevitOwned);
            }
            
            throw new Exception("Could not get the element from the document.");
        }

        /// <summary>
        /// A factory method for looking up and obtaining elements
        /// from the revit project
        /// </summary>
        /// <param name="uniqueId">The unique id of the element to select</param>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static AbstractElement ByUniqueId(string uniqueId, bool isRevitOwned)
        {
            var ele = InternalGetElementByUniqueId(uniqueId);

            if (ele != null)
            {
                return WrapElement(ele, isRevitOwned);
            }

            throw new Exception("Could not get the element from the document");
        }

        /// <summary>
        /// If possible, wrap the element in a DS type
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static AbstractElement WrapElement(Autodesk.Revit.DB.Element ele, bool isRevitOwned)
        {
            AbstractElement result = null;

            if (ele is Autodesk.Revit.DB.ReferencePoint)
            {
                result = DSReferencePoint.FromExisting(ele as Autodesk.Revit.DB.ReferencePoint, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.Form)
            {
                result = DSForm.FromExisting(ele as Autodesk.Revit.DB.Form, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.FreeFormElement)
            {
                result = DSFreeForm.FromExisting(ele as Autodesk.Revit.DB.FreeFormElement, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.FamilyInstance &&
                AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol(ele as Autodesk.Revit.DB.FamilyInstance))
            {
                result = DSAdaptiveComponent.FromExisting(ele as Autodesk.Revit.DB.FamilyInstance, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.FamilyInstance)
            {
                result = DSFamilyInstance.FromExisting(ele as Autodesk.Revit.DB.FamilyInstance, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.FamilySymbol)
            {
                result = DSFamilySymbol.FromExisting(ele as Autodesk.Revit.DB.FamilySymbol, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.ModelCurve)
            {
                result = DSModelCurve.FromExisting(ele as Autodesk.Revit.DB.ModelCurve, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.Family)
            {
                result = DSFamily.FromExisting(ele as Autodesk.Revit.DB.Family, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.DividedPath)
            {
                result = DSDividedPath.FromExisting(ele as Autodesk.Revit.DB.DividedPath, isRevitOwned);
            }
            else if (ele is Autodesk.Revit.DB.DividedSurface)
            {
                result = DSDividedSurface.FromExisting(ele as Autodesk.Revit.DB.DividedSurface, isRevitOwned);
            }

            if (result == null)
            {
                throw new Exception("The Element cannot be wrapped as there is no existing type that wraps it.");
            }

            return result;

        }

        /// <summary>
        /// Internal helper method to get an element from the current document by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Element InternalGetElementById(int id)
        {
            Element ele;
            var eleId = new ElementId(id);

            if (!AbstractElement.Document.TryGetElement(eleId, out ele))
            {
                throw new Exception("Could not obtain element from the current document!  The id may not be valid.");
            }

            return ele;
        }

        /// <summary>
        /// Obtain an element from the current document given the element's unique id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Element InternalGetElementByUniqueId(string uniqueId)
        {
            Element ele;

            if (!AbstractElement.Document.TryGetElement(uniqueId, out ele))
            {
                throw new Exception("Could not obtain element from the current document!  The unique id may not be valid.");
            }

            return ele;
        }

    }
}
