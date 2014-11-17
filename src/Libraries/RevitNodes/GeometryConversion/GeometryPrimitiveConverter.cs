using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using RevitServices.Persistence;
using Line = Autodesk.Revit.DB.Line;
using System.ComponentModel;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class GeometryPrimitiveConverter
    {
        #region Proto -> Revit types

        public static Autodesk.Revit.DB.BoundingBoxXYZ ToRevitBoundingBox(
            Autodesk.DesignScript.Geometry.CoordinateSystem cs,
            Autodesk.DesignScript.Geometry.Point minPoint,
            Autodesk.DesignScript.Geometry.Point maxPoint, bool convertUnits = true)
        {
            var rbb = new BoundingBoxXYZ();
            rbb.Enabled = true;

            rbb.Transform = cs.ToTransform(convertUnits);

            rbb.Max = maxPoint.ToXyz(convertUnits);
            rbb.Min = minPoint.ToXyz(convertUnits);

            return rbb;
        }

        public static Autodesk.Revit.DB.BoundingBoxXYZ ToRevitType(this Autodesk.DesignScript.Geometry.BoundingBox bb, bool convertUnits = true)
        {
            return ToRevitBoundingBox(bb.ContextCoordinateSystem, bb.MinPoint, bb.MaxPoint, convertUnits);
        }

        public static Autodesk.Revit.DB.XYZ ToRevitType(this Autodesk.DesignScript.Geometry.Point pt, bool convertUnits = true)
        {
            return pt.ToXyz(convertUnits);
        }

        public static Autodesk.Revit.DB.XYZ ToRevitType(this Vector vec, bool convertUnits = false)
        {
            return vec.ToXyz(convertUnits);
        }

        public static Autodesk.Revit.DB.XYZ ToXyz(this Autodesk.DesignScript.Geometry.Point pt, bool convertUnits = true)
        {
            if (convertUnits) pt = pt.InHostUnits();
            return new XYZ(pt.X, pt.Y, pt.Z);
        }

        public static Autodesk.Revit.DB.XYZ ToXyz(this Vector vec, bool convertUnits = false)
        {
            if (convertUnits) vec = vec.Scale(UnitConverter.DynamoToHostFactor);
            return new XYZ(vec.X, vec.Y, vec.Z);
        }

        public static Autodesk.Revit.DB.Transform ToTransform(this CoordinateSystem cs, bool convertUnits = true)
        {
            var trans = Transform.Identity;
            trans.Origin = cs.Origin.ToXyz(convertUnits);
            trans.BasisX = cs.XAxis.ToXyz();
            trans.BasisY = cs.YAxis.ToXyz();
            trans.BasisZ = cs.ZAxis.ToXyz();
            return trans;
        }

        public static Autodesk.Revit.DB.Plane ToPlane(this Autodesk.DesignScript.Geometry.Plane plane, bool convertUnits = true)
        {
            return new Autodesk.Revit.DB.Plane(plane.Normal.ToXyz(), plane.Origin.ToXyz(convertUnits));
        }

        public static List<XYZ> ToXyzs(this List<Autodesk.DesignScript.Geometry.Point> list, bool convertUnits = true)
        {
            return list.ConvertAll((x) => x.ToXyz(convertUnits));
        }

        public static XYZ[] ToXyzs(this Autodesk.DesignScript.Geometry.Point[] list, bool convertUnits = true)
        {
            return list.Select((x) => x.ToXyz(convertUnits)).ToArray();
        }

        public static XYZ[] ToXyzs(this Autodesk.DesignScript.Geometry.Vector[] list, bool convertUnits = false)
        {
            return list.Select((x) => x.ToXyz(convertUnits)).ToArray();
        }

        public static DoubleArray ToDoubleArray(this double[] list)
        {
            var n = new DoubleArray();
            list.ToList().ForEach(x => n.Append(ref x));
            return n;
        }

        internal static Autodesk.Revit.DB.UV[] ToUvs(this double[][] uvArr)
        {
            var uvs = new Autodesk.Revit.DB.UV[uvArr.Length];
            var count = 0;
            foreach (var row in uvArr)
            {
                if (row.Length != 2)
                {
                    throw new Exception("Each element of the input array should be length 2");
                }
                else
                {
                    uvs[count++] = new Autodesk.Revit.DB.UV(row[0], row[1]);
                }
            }

            return uvs;
        }

        internal static Autodesk.DesignScript.Geometry.UV[] ToDSUvs(this double[][] uvArr)
        {
            var uvs = new Autodesk.DesignScript.Geometry.UV[uvArr.Length];
            var count = 0;
            foreach (var row in uvArr)
            {
                if (row.Length != 2)
                {
                    throw new Exception("Each element of the input array should be length 2");
                }
                else
                {
                    uvs[count++] = Autodesk.DesignScript.Geometry.UV.ByCoordinates(row[0], row[1]);
                }
            }

            return uvs;
        }

        #endregion

        #region Revit -> Proto types

        public static Autodesk.DesignScript.Geometry.BoundingBox ToProtoType(this Autodesk.Revit.DB.BoundingBoxXYZ xyz, bool convertUnits = true)
        {
            xyz.Enabled = true;

            return Autodesk.DesignScript.Geometry.BoundingBox.ByCornersCoordinateSystem(
                xyz.Min.ToPoint(convertUnits), xyz.Max.ToPoint(convertUnits),
                xyz.Transform.ToCoordinateSystem(convertUnits));
        }

        public static Autodesk.DesignScript.Geometry.Point ToPoint(this XYZ xyz, bool convertUnits = true)
        {
            var pt = Autodesk.DesignScript.Geometry.Point.ByCoordinates(xyz.X, xyz.Y, xyz.Z);
            return convertUnits ? pt.InDynamoUnits() : pt;
        }

        public static Autodesk.DesignScript.Geometry.Point ToProtoType(this Autodesk.Revit.DB.Point point, bool convertUnits = true)
        {
            return point.Coord.ToPoint(convertUnits);
        }

        public static Vector ToVector(this XYZ xyz, bool convertUnits = false)
        {
            var v = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(xyz.X, xyz.Y, xyz.Z);
            return convertUnits ? v.Scale(UnitConverter.HostToDynamoFactor) : v;
        }

        public static Autodesk.DesignScript.Geometry.UV ToProtoType(this Autodesk.Revit.DB.UV uv)
        {
            return Autodesk.DesignScript.Geometry.UV.ByCoordinates(uv.U, uv.V);
        }

        public static Autodesk.DesignScript.Geometry.Plane ToPlane(this Autodesk.Revit.DB.Plane plane, bool convertUnits = true)
        {
            return Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(plane.Origin.ToPoint(convertUnits), plane.Normal.ToVector());
        }

        public static CoordinateSystem ToCoordinateSystem(this Transform t, bool convertUnits = true)
        {
            return CoordinateSystem.ByOriginVectors(t.Origin.ToPoint(convertUnits), t.BasisX.ToVector(), t.BasisY.ToVector());
        }

        public static List<Autodesk.DesignScript.Geometry.Point> ToPoints(this List<XYZ> list, bool convertUnits = true)
        {
            return list.ConvertAll((x) => x.ToPoint(convertUnits));
        }

        #endregion

        #region Degrees, radians

        public static double ToRadians(this double degrees)
        {
            return degrees*Math.PI/180.0;
        }

        public static double ToDegrees(this double degrees)
        {
            return degrees * 180.0 / Math.PI;
        }

        #endregion

        #region X&UZ

        public static XYZ GetPerpendicular(this XYZ xyz)
        {
            var ixn = xyz.Normalize();
            var xn = new XYZ(1, 0, 0);

            if (ixn.IsAlmostEqualTo(xn))
            {
                xn = new XYZ(0,1,0);
            }

            return ixn.CrossProduct(xn).Normalize();
        }

        public static Vector GetPerpendicular(this Vector vector)
        {
            return vector.ToXyz().GetPerpendicular().ToVector();
        }

        #endregion

    }
}
