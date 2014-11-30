using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{

    /// <summary>
    /// This class is required to extract the underlying surface representation from a Revit Face.
    /// All Face types are supported.
    /// </summary>
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public static class SurfaceExtractor
    {
        public static Surface ExtractSurface(Autodesk.Revit.DB.PlanarFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            edgeLoops = edgeLoops.ToList();

            var x = face.get_Vector(0).ToVector(false);
            var y = face.get_Vector(1).ToVector(false);

            // Construct planar surface larger than the biggest edge loop
            var or = edgeLoops.First().StartPoint; // don't use origin provided by revit, could be anywhere
            var maxLength = edgeLoops.Max(pc => pc.Length);
            var bigx = x.Scale(maxLength * 2);
            var bigy = y.Scale(maxLength * 2);

            return Surface.ByPerimeterPoints(new[] {    or.Subtract(bigx).Subtract(bigy), 
                                                        or.Add(bigx).Subtract(bigy), 
                                                        or.Add(bigx).Add(bigy),
                                                        or.Subtract(bigx).Add(bigy) });
        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.CylindricalFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            // Note: Internal representation of the cylinder
            // S(u, v) = Origin + cos(u)*Radius[0] + sin(u)*Radius[1] + v*Axis

            edgeLoops = edgeLoops.ToList();

            // Get some data from the face
            var axis = face.Axis.ToVector(false);
            var x = face.get_Radius(0).ToVector(false);
            var y = face.get_Radius(1).ToVector(false);
            var rad = x.Length;
            var oax = face.Origin.ToPoint(false);

            // project closest point on edge loop onto axis
            // this gives a more reliable origin as the revit origin
            // could be anywhere on the axis
            var pt = edgeLoops.First().StartPoint;
            var dir = pt.Subtract( oax.AsVector() );
            var projLength = dir.AsVector().Dot(axis);
            var o = oax.Add(axis.Normalized().Scale(projLength));

            // We don't know the start and end point of the cylindrical surface
            // so we use the maxLength of the edgeLoops as a conservative guess
            var maxLength = edgeLoops.Max(pc => pc.Length);

            // Get the "base point" of the cylinder
            var basePoint = o.Add(axis.Reverse().Scale(2 * maxLength));

            // Build the "base circle" of the cylinder
            var pl1 = Autodesk.DesignScript.Geometry.Plane.ByOriginXAxisYAxis(basePoint, x.Normalized(), y.Normalized());
            var c1 = Circle.ByPlaneRadius(pl1, rad);

            // extrude the cylindrical surface - again using the conservative maxLength
            var result = c1.Extrude(axis.Scale(4*maxLength));
            pl1.Dispose();
            c1.Dispose();
            return result;
        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.ConicalFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            // Note: Internal representation of the cone
            // S(u, v) = Origin + v*[sin(HalfAngle)*(cos(u)*Radius[0] + sin(u)*Radius[1]) + cos(HalfAngle)*Axis]

            edgeLoops = edgeLoops.ToList();

            // Get some data from the face
            var axis = face.Axis.ToVector(false);
            var x = face.get_Radius(0).ToVector(false);
            var y = face.get_Radius(1).ToVector(false);
            var tipPt = face.Origin.ToPoint(false);
            var ang = face.HalfAngle;

            // We use the max length in order to help find the lowest possible base point for the cone
            var maxLength = edgeLoops.Max(pc => pc.Length);

            // We don't know the "base" point of the cone, so we build it here
            // by projecting a point on the edge loops onto the axis
            var pt = edgeLoops.First().StartPoint;
            var dir = pt.Subtract( tipPt.AsVector() );
            var projLength = dir.AsVector().Dot(axis);
            var height = projLength + 2 * maxLength; 
            var o = tipPt.Add(axis.Normalized().Scale(height));

            // there's not an easy way to create a conical surface in protogeometry outside of this
            // note this coordinate system has the z axis reversed because we're building the cone
            // from it's flat bottom surface
            var baseCS = CoordinateSystem.ByOriginVectors(o, x, y.Reverse());

            // Construct the radius
            var rad = Math.Cos(ang)*height;
            var cone = Cone.ByCoordinateSystemHeightRadius(baseCS, height, rad);

            // PB: this is iffy code - we need to extract the surface that's not touching the origin
            //return cone.Faces.Select(f => f.SurfaceGeometry()).First(s => s.DistanceTo(o) > 1e-5);

            // the flat face of the cone is currently the second face in the Faces enumeration
            return cone.Faces[1].SurfaceGeometry(); 
        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.HermiteFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            // The number of interpolating points in the u direction is given by get_Params
            var uParams = face.get_Params(0).Cast<double>().ToArray();
            var vParams = face.get_Params(1).Cast<double>().ToArray();

            var numU = uParams.Length;
            var numV = vParams.Length;

            // unpack the points
            var points = face.Points;
           
            // structure the points as 2d array - numU x numV
            var ptArr = new Autodesk.DesignScript.Geometry.Point[numV][];
            var count = 0;
            for (var i = 0; i < numV; i++)
            {
                ptArr[i] = new Autodesk.DesignScript.Geometry.Point[numU];

                for (var j = 0; j < numU; j++)
                {
                    ptArr[i][j] = points[count++].ToPoint(false);
                }
            }

            // unpack the tangents
            var uTangents = face.get_Tangents(0);
            var vTangents = face.get_Tangents(1);

            // structure the tangents as 2d array - numU x numV
            var uTangentsArr = new Vector[numV][];
            var vTangentsArr = new Vector[numV][];

            count = 0;
            for (var i = 0; i < numV; i++)
            {
                uTangentsArr[i] = new Vector[numU];
                vTangentsArr[i] = new Vector[numU];

                for (var j = 0; j < numU; j++)
                {
                    uTangentsArr[i][j] = uTangents[count].ToVector(false);
                    vTangentsArr[i][j] = vTangents[count].ToVector(false);
                    count++;
                }
            }

            // u tangents run in increasing column direction
            var uStartTangents = uTangentsArr.Select(x => x[0]).ToArray();
            var uEndTangents = uTangentsArr.Select(x => x[numU - 1]).ToArray();

            // v tangents run in increasing row direction
            var vStartTangents = vTangentsArr[0];
            var vEndTangents = vTangentsArr[numV-1];

            // The mixed derivs are the twist vectors - dP / dUdV
            var md = face.MixedDerivs;
            Vector[] mds =
            {
                md[0].ToVector(false), 
                md[numU - 1].ToVector(false), 
                md[(md.Count - 1) - (numU-1)].ToVector(false),
                md[md.Count - 1].ToVector(false)
            };

            return NurbsSurface.ByPointsTangentsKnotsDerivatives(   ptArr, 
                                                                    uStartTangents, 
                                                                    uEndTangents, 
                                                                    vStartTangents, 
                                                                    vEndTangents,
                                                                    HermiteToNurbs.Clamp(uParams),
                                                                    HermiteToNurbs.Clamp(vParams), 
                                                                    mds);

        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.RevolvedFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            var crv = face.Curve.ToProtoType(false);

            var axis = face.Axis.ToVector(false);
            var o = face.Origin.ToVector(false);
            var x = face.get_Radius(0).ToVector(false);
            var y = face.get_Radius(1).ToVector(false);

            // Note: The profile curve is represented in the coordinate system of the revolve
            //       so we need to transform it into the global coordinate system
            var revolveCs = CoordinateSystem.Identity();
            var globalCs = CoordinateSystem.ByOriginVectors(o.AsPoint(), x, y);

            var crvTrf = (Autodesk.DesignScript.Geometry.Curve)crv.Transform(revolveCs, globalCs);
            crv.Dispose();

            var srf0 =
                Surface.ByRevolve(crvTrf, o.AsPoint(), axis.Normalized(), 0, 360);
            var srf = srf0.FlipNormalDirection();
            srf0.Dispose();
            crvTrf.Dispose();

            var ptOnSrf = srf.PointAtParameter(0.5, 0.5);
            var projRes = face.Project(ptOnSrf.ToXyz());

            if (projRes == null) return srf;

            var uvOnFace = projRes.UVPoint;

            var normOnFace = face.ComputeNormal(uvOnFace).ToVector();
            var normOnSrf = srf.NormalAtParameter(0.5, 0.5);

            // if the normal is reversed, reverse the surface
            if (normOnFace.Dot(normOnSrf) < 0)
            {
                var srf1 = srf.FlipNormalDirection();
                srf.Dispose();
                return srf1;
            }

            return srf;

        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.RuledFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            var c0 = face.get_Curve(0).ToProtoType(false);
            var c1 = face.get_Curve(1).ToProtoType(false);

            var result = Surface.ByLoft(new[] {c0, c1});
            c0.Dispose();
            c1.Dispose();
            return result;
        }
    }
}
