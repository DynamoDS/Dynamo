using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    public static class ElementFactory
    {
        /// <summary>
        /// A factory method for looking up and obtaining elements
        /// from the revit project
        /// </summary>
        /// <returns></returns>
        public static AbstractElement ByElementId(int id)
        {
            var ele = InternalGetElementById(id);

            if (ele != null)
            {
                return InternalGetDSTypeFromElement(ele);
            }
            
            throw new Exception("Could not get the element from the document");
        }

        /// <summary>
        /// A factory method for looking up and obtaining elements
        /// from the revit project
        /// </summary>
        /// <returns></returns>
        public static AbstractElement ByUniqueId(string uniqueId)
        {
            var ele = InternalGetElementByUniqueId(uniqueId);

            if (ele != null)
            {
                return InternalGetDSTypeFromElement(ele);
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

        /// <summary>
        /// Get the DS wrapper object given a Revit element
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        private static AbstractElement InternalGetDSTypeFromElement(Autodesk.Revit.DB.Element ele)
        {

            if (ele is Autodesk.Revit.DB.FamilyInstance)
            {
                return new DSFamilyInstance(ele as Autodesk.Revit.DB.FamilyInstance);
            }

            if (ele is Autodesk.Revit.DB.FamilySymbol)
            {
                return new DSFamilySymbol(ele as Autodesk.Revit.DB.FamilySymbol);
            }

            if (ele is Autodesk.Revit.DB.Family)
            {
                return new DSFamily(ele as Autodesk.Revit.DB.Family);
            }

            throw new Exception("The element is of an unknown type.");
        }

    }
}
