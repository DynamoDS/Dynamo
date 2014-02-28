using System;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;

namespace Revit.GeometryObjects
{
    public class GeometryObjectSelector
    {
        /// <summary>
        /// Return an AbstractGeometryObject given a string representation of the geometry's reference.
        /// </summary>
        /// <param name="referenceString"></param>
        /// <returns></returns>
        public static AbstractGeometryObject ByReferenceStableRepresentation(string referenceString)
        {
            var geob = InternalGetGeometryByStableRepresentation(referenceString);

            if (geob != null)
            {
                return WrapGeometryObject(geob);
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
        private static GeometryObject InternalGetGeometryByStableRepresentation(string representation)
        {
            var geometryReference = Reference.ParseFromStableRepresentation(DocumentManager.GetInstance().CurrentDBDocument, representation);
            var geob =
                    DocumentManager.GetInstance()
                        .CurrentDBDocument.GetElement(geometryReference)
                        .GetGeometryObjectFromReference(geometryReference);

            return geob;
        }

        /// <summary>
        /// If possible, wrap the geometry object in a DS type.
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public static AbstractGeometryObject WrapGeometryObject(GeometryObject geom)
        {
            AbstractGeometryObject result = null;

            if (geom is Autodesk.Revit.DB.Face)
            {
                result = Face.FromExisting(geom as Autodesk.Revit.DB.Face);
            }

            return result;
        }
    }
}
