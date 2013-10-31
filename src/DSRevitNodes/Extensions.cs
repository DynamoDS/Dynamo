using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using Line = Autodesk.Revit.DB.Line;

namespace DSRevitNodes
{
    public static class Extensions
    {
        /// <summary>
        /// Convert an XYZ to a Point
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Point ToPoint(this XYZ xyz )
        {
            return Autodesk.DesignScript.Geometry.Point.ByCoordinates(xyz.X, xyz.Y, xyz.Z);
        }

        /// <summary>
        /// Convert a Point to an XYZ
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static XYZ ToXyz(this Autodesk.DesignScript.Geometry.Point pt)
        {
            return new XYZ(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Convert a ReferencePoint to an XYZ.
        /// </summary>
        /// <param name="refPt"></param>
        /// <returns></returns>
        public static XYZ ToXyz(this Autodesk.Revit.DB.ReferencePoint refPt)
        {
            return refPt.Position;
        }

        /// <summary>
        /// Convert an XYZ to a Vector.
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public static Vector ToVector(this XYZ xyz)
        {
            return Vector.ByCoordinates(xyz.X, xyz.Y, xyz.Z);
        }

        /// <summary>
        /// Convert a Vector to an XYZ.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static XYZ ToXyz(this Vector vec)
        {
            return new XYZ(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// Convert a Revit Plane to a DS Plane.
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Plane ToPlane(this Autodesk.Revit.DB.Plane plane)
        {
            return Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(plane.Origin.ToPoint(), plane.Normal.ToVector());
        }

        /// <summary>
        /// Convert a DS Plane to a Revit Plane.
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.Plane ToPlane(this Autodesk.DesignScript.Geometry.Plane plane)
        {
            return new Autodesk.Revit.DB.Plane(plane.Normal.ToXyz(), plane.Origin.ToXyz());
        }

        /// <summary>
        /// Convert a Transform to a CoordinateSystem
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static CoordinateSystem ToCoordinateSystem(this Transform t)
        {
            return CoordinateSystem.ByOriginVectors(t.Origin.ToPoint(), t.BasisX.ToVector(), t.BasisY.ToVector());
        }

        /// <summary>
        /// Convert a CoordinateSystem to a Transform
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public static Transform ToTransform(this CoordinateSystem cs)
        {
            var trans = Transform.Identity;
            trans.Origin = cs.Origin.ToXyz();
            trans.BasisX = cs.XAxis.ToXyz();
            trans.BasisY = cs.YAxis.ToXyz();
            trans.BasisZ = cs.ZAxis.ToXyz();
            return trans;
        }

        /// <summary>
        /// Convert Revit Line to DS Line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Line ToLine(this Autodesk.Revit.DB.Line line)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                line.get_EndPoint(0).ToPoint(), line.get_EndPoint(1).ToPoint());
        }
    }
}
