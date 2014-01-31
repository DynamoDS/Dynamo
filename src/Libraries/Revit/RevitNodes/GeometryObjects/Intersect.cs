using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using RevitServices.Persistence;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Edge = Revit.GeometryObjects.Edge;
using Face = Autodesk.DesignScript.Geometry.Face;
using Line = Autodesk.DesignScript.Geometry.Line;
using Point = Autodesk.DesignScript.Geometry.Point;
using Solid = Autodesk.DesignScript.Geometry.Solid;

namespace Revit.GeometryObjects
{
    public struct CurveFaceIntersectionResult
    {

        public Point Point
        {
            get; internal set;
        }

        public double CurveParameter
        {
            get;
            internal set;
        }

        public double[] FaceParameter
        {
            get;
            internal set;
        }

        public Edge Edge
        {
            get;
            internal set;
        }

        public double EdgeParameter
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return String.Format("CurveFaceIntersectionResult - Point: {0}, CurveParameter: {1}, Edge: {2}, EdgeParameter: {3}", 
                this.Point, 
                this.CurveParameter, 
                this.Edge,
                this.EdgeParameter );
        }
    }

    /// <summary>
    /// Intersect faces
    /// </summary>
    public class Intersect
    {

        /// <summary>
        /// Calculate the intersection of a curve and face
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="face"></param>
        /// <returns>A list of DSCurveFaceIntersectionResult</returns>
        public static List<CurveFaceIntersectionResult> CurveFace(Autodesk.DesignScript.Geometry.Curve curve, Revit.GeometryObjects.Face face)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (face == null)
            {
                throw new ArgumentNullException("face");
            }

            var revitFace = face.InternalFace;
            Autodesk.Revit.DB.IntersectionResultArray xsects = null;

            var result = revitFace.Intersect(curve.ToRevitType(), out xsects);

            var intersections = new List<CurveFaceIntersectionResult>();

            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    var intersection = new CurveFaceIntersectionResult();
                    try
                    {
                        intersection.EdgeParameter = ir.EdgeParameter;
                    }
                    catch
                    {
                        intersection.EdgeParameter = 0;
                    }

                    if (ir.EdgeObject != null)
                    {
                        intersection.Edge = Edge.FromExisting(ir.EdgeObject);
                    }
                    
                    intersection.CurveParameter = ir.Parameter;

                    intersection.FaceParameter = new double[] { ir.UVPoint.U, ir.UVPoint.V };
                    intersection.Point = ir.XYZPoint.ToPoint();

                    intersections.Add( intersection );
                }
            }

            return intersections;
        }

        /// <summary>
        /// Calculate the intersection of two faces.  This will be a list of curves.  
        /// </summary>
        /// <param name="face1"></param>
        /// <param name="face2"></param>
        /// <returns>A list of curves or an empty list if there is no intersection</returns>
        public static List<Autodesk.DesignScript.Geometry.Curve> FaceFace( Revit.GeometryObjects.Face face1, Revit.GeometryObjects.Face face2 )
        {
            if (face1 == null)
            {
                throw new ArgumentNullException("face1");
            }

            if (face2 == null)
            {
                throw new ArgumentNullException("face2");
            }

            var revitFace1 = face1.InternalFace;
            var revitFace2 = face2.InternalFace;

            Autodesk.Revit.DB.Curve curve;
            var rez = revitFace1.Intersect(revitFace2, out curve);

            if (rez == FaceIntersectionFaceResult.Intersecting)
            {
                return new List<Autodesk.DesignScript.Geometry.Curve>(){curve.ToProtoType()};
            }

            return new List<Autodesk.DesignScript.Geometry.Curve>();
        }
    }
}