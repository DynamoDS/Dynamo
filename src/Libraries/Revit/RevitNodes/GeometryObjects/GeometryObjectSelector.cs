using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;

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
            var geometryReference = Reference.ParseFromStableRepresentation(DocumentManager.Instance.CurrentDBDocument, representation);
            var geob =
                    DocumentManager.Instance
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
            dynamic dynGeom = geom;
            return Convert(dynGeom);
        }

        private static Face Convert(Autodesk.Revit.DB.Face geom)
        {
            return Face.FromExisting(geom);
        }

        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Curve geom)
        {
            return geom.ToProtoType();
        }

        private static Edge Convert(Autodesk.Revit.DB.Edge geom)
        {
            return Edge.FromExisting(geom);
        }

        private static Point Convert(Autodesk.Revit.DB.Point geom)
        {
            return Point.ByCoordinates(geom.Coord.X, geom.Coord.Y, geom.Coord.Z);
        }

        private static PolyCurve Convert(PolyLine geom)
        {
            return PolyCurve.ByPoints(geom.GetCoordinates().Select(x=>Point.ByCoordinates(x.X,x.Y,x.Z)).ToArray(), true);
        }

        private static Revit.GeometryObjects.Solid Convert(Autodesk.Revit.DB.Solid geom)
        {
            return Solid.FromExisting(geom);
        }

        /*
         * 
         * Cannot introduce without extending Revit
         * 
        private static Autodesk.DesignScript.Geometry.Solid Convert(Autodesk.Revit.DB.Solid geom)
        {
            return Solid.FromExisting(geom);
        }
         */

        private static PolyCurve Convert(Profile geom)
        {
            return geom.Curves.ToProtoTypes();
        }

        private static Autodesk.DesignScript.Geometry.Mesh Convert(Autodesk.Revit.DB.Mesh geom)
        {
            return geom.ToProtoType();
        }
    }
}
