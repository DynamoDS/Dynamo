
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
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
            return DocumentManager.GetInstance().ElementsOfType<T>().Select(x => x.ToDSType(isRevitOwned));
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
                return ele.ToDSType(isRevitOwned);
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
                return ele.ToDSType(isRevitOwned);
            }

            throw new Exception("Could not get the element from the document");
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
