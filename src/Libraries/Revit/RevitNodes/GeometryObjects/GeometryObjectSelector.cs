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
            var geometryReference = Reference.ParseFromStableRepresentation(DocumentManager.Instance.CurrentDBDocument, referenceString);

            var geob =
                DocumentManager.Instance
                    .CurrentDBDocument.GetElement(geometryReference)
                    .GetGeometryObjectFromReference(geometryReference);

            if (geob != null)
            {
                return geob.Convert(geometryReference);
            }

            throw new Exception("Could not get a geometry object from the current document using the provided reference.");
        }

    }
}
