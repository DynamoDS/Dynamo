using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;

namespace Revit.Elements
{
    //[SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public class ElementSelector
    {
        /// <summary>
        /// Get a collection of wrapped elements from the current document by type
        /// </summary>
        /// <typeparam name="T">The type of the Revit element to select</typeparam>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static IEnumerable<Element> ByType<T>(bool isRevitOwned) where T : Autodesk.Revit.DB.Element
        {
            return DocumentManager.Instance.ElementsOfType<T>().Select(x => x.ToDSType(isRevitOwned));
        }

        /// <summary>
        /// A factory method for looking up and obtaining elements
        /// from the revit project.
        /// </summary>
        /// <param name="elementId">The id of the element to select</param>
        /// <param name="isRevitOwned">Whether the returned object should be revit owned or not</param>
        /// <returns></returns>
        public static Element ByElementId(int elementId, bool isRevitOwned)
        {
            var ele = InternalGetElementById(elementId);

            if (ele != null)
            {
                return ele.ToDSType(isRevitOwned);
            }
            
            throw new Exception("Could not get the element from the document.");
        }

        public static Element ByElementId(int elementId)
        {
            var ele = InternalGetElementById(elementId);

            if (ele != null)
            {
                return ele.ToDSType(true);
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
        public static Element ByUniqueId(string uniqueId, bool isRevitOwned)
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
            Autodesk.Revit.DB.Element ele;
            var eleId = new ElementId(id);

            if (!Element.Document.TryGetElement(eleId, out ele))
            {
                throw new Exception("Could not obtain element from the current document!  The id may not be valid.");
            }

            return ele;
        }

        /// <summary>
        /// Obtain an element from the current document given the element's unique id
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Element InternalGetElementByUniqueId(string uniqueId)
        {
            Autodesk.Revit.DB.Element ele;

            if (!Element.Document.TryGetElement(uniqueId, out ele))
            {
                throw new Exception("Could not obtain element from the current document!  The unique id may not be valid.");
            }

            return ele;
        }
    }
}
