using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    internal static class DSElementFactory
    {
        /// <summary>
        /// A factory method for looking up and obtaining elements
        /// from the revit project
        /// </summary>
        /// <returns></returns>
        public static AbstractElement ByElementId(int id)
        {
            // get the element
            TransactionManager.GetInstance().EnsureInTransaction(AbstractElement.Document);

            var eleId = new ElementId(id);
            Element ele;

            if (!AbstractElement.Document.TryGetElement(eleId, out ele))
            {
                throw new Exception("Could not obtain element");
            }

            TransactionManager.GetInstance().TransactionTaskDone();

            // return the appropriate type
            if (ele is Autodesk.Revit.DB.FamilyInstance)
            {
                return new DSFamilyInstance(ele as Autodesk.Revit.DB.FamilyInstance);
            }
            
            if (ele is Autodesk.Revit.DB.Family)
            {
                return new DSFamily(ele as Autodesk.Revit.DB.Family);
            }

            throw new Exception("The element is of an unknown type.");

        }

    }
}
