using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;

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
        public static GeometryObject ByReferenceStableRepresentation(string referenceString)
        {
            var geob = InternalGetGeometryByStableRepresentation(referenceString);

            if (geob != null)
            {
                return GeometryObjectWrapper.Wrap(geob);
            }

            throw new Exception("Could not get a geometry object from the current document using the provided reference.");
        }

        /// <summary>
        /// Return a ProtoGeometry Curve object from a string representation of the curve's reference.
        /// </summary>
        /// <param name="referenceString"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Curve ByCurve(string referenceString)
        {
            var geob = InternalGetGeometryByStableRepresentation(referenceString);
            var curve = geob as Autodesk.Revit.DB.Curve;

            if (curve != null)
            {
                return curve.ToProtoType();
            }

            throw new Exception("The reference representation does not represent a curve.");
        }

        /// <summary>
        /// Internal helper method to get a reference from the current document by its stable representation.
        /// </summary>
        /// <param name="representation">The Revit-provided stable representation of the reference.</param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.GeometryObject InternalGetGeometryByStableRepresentation(string representation)
        {
            var geometryReference = Reference.ParseFromStableRepresentation(DocumentManager.Instance.CurrentDBDocument, representation);
            var geob =
                    DocumentManager.Instance
                        .CurrentDBDocument.GetElement(geometryReference)
                        .GetGeometryObjectFromReference(geometryReference);

            return geob;
        }
    }
}
