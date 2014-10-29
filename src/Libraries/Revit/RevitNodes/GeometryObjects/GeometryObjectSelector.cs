using System;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;

namespace Revit.GeometryObjects
{
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryObjectSelector
    {
        /// <summary>
        /// Return an AbstractGeometryObject given a string representation of the geometry's reference.
        /// </summary>
        /// <param name="referenceString"></param>
        /// <returns></returns>
        public static object ByReferenceStableRepresentation(string referenceString)
        {
            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                var elRef =
                    Reference.ParseFromStableRepresentation(doc, referenceString);

                var geob =
                    DocumentManager.Instance
                        .CurrentDBDocument.GetElement(elRef)
                        .GetGeometryObjectFromReference(elRef);

                return geob != null ? geob.Convert(elRef) : null;
            }
            catch(Exception ex)
            {
                throw new Exception("Could not get a geometry object from the current document using the provided reference.");
            }
        }
    }
}
