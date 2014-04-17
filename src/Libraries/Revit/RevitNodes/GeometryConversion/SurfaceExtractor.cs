using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public static class SurfaceExtractor
    {
        public static Surface ExtractSurface(Autodesk.Revit.DB.PlanarFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            edgeLoops = edgeLoops.ToList();

            var x = face.get_Vector(0).ToVector();
            var y = face.get_Vector(1).ToVector();

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
            // Note: Internal representation of the cone
            // S(u, v) = Origin + cos(u)*Radius[0] + sin(u)*Radius[1] + v*Axis

            edgeLoops = edgeLoops.ToList();

            // Get some data from the face
            var axis = face.Axis.ToVector();
            var x = face.get_Radius(0).ToVector();
            var y = face.get_Radius(1).ToVector();
            var rad = x.Length;
            var oax = face.Origin.ToPoint();

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
            return c1.Extrude(axis.Scale(4*maxLength));
        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.ConicalFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            // Note: Internal representation of the cone
            // S(u, v) = Origin + v*[sin(HalfAngle)*(cos(u)*Radius[0] + sin(u)*Radius[1]) + cos(HalfAngle)*Axis]

            edgeLoops = edgeLoops.ToList();

            // Get some data from the face
            var axis = face.Axis.ToVector();
            var x = face.get_Radius(0).ToVector();
            var y = face.get_Radius(1).ToVector();
            var tipPt = face.Origin.ToPoint();
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
            var numU = face.get_Params(0).Size;
            var numV = face.get_Params(1).Size;

            // unpack the points
            var points = face.Points;
           
            // structure the points
            var ptArr = new Autodesk.DesignScript.Geometry.Point[numV][];
            var count = 0;
            for (var i = 0; i < numV; i++)
            {
                ptArr[i] = new Autodesk.DesignScript.Geometry.Point[numU];

                for (var j = 0; j < numU; j++)
                {
                    ptArr[i][j] = points[count++].ToPoint();
                }
            }

            // unpack the tangents
            var uTangents = face.get_Tangents(0);
            var vtangents = face.get_Tangents(1);

            // structure the points
            // PB: could be simplified to just get the end vectors, but this is easier to debug
            var uTangentsArr = new Autodesk.DesignScript.Geometry.Vector[numV][];
            var vTangentsArr = new Autodesk.DesignScript.Geometry.Vector[numV][];

            count = 0;
            for (var i = 0; i < numV; i++)
            {
                uTangentsArr[i] = new Autodesk.DesignScript.Geometry.Vector[numU];
                vTangentsArr[i] = new Autodesk.DesignScript.Geometry.Vector[numU];

                for (var j = 0; j < numU; j++)
                {
                    uTangentsArr[i][j] = uTangents[count].ToVector();
                    vTangentsArr[i][j] = vtangents[count].ToVector();
                    count++;
                }
            }

            // the required u tangents are the first and last row of the utangents 2d arr
            var uStartTangents = uTangentsArr[0];
            var uEndTangents = uTangentsArr[numV-1];

            // the required v tangents are the first and last col of the vtangents 2d arr
            var vStartTangents = vTangentsArr.Select(x => x[0]).ToArray();
            var vEndTangents = vTangentsArr.Select(x => x[numU-1]).ToArray();

            return NurbsSurface.ByPointsTangents(ptArr, uStartTangents, uEndTangents, vStartTangents, vEndTangents);
        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.RevolvedFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            var crv = face.Curve.ToProtoType();
            var axis = face.Axis.ToVector();
            var o = face.Origin.ToPoint();
            var x = face.get_Radius(0);
            var y = face.get_Radius(1);

            // TODO: ensure x is in direction of profile plane
            return Surface.ByRevolve(crv, o, axis, 0, 360);
        }

        public static Surface ExtractSurface(Autodesk.Revit.DB.RuledFace face, IEnumerable<PolyCurve> edgeLoops)
        {
            // Naive lofting operation
            // TODO: Check the parameterization of the two curves
            var c0 = face.get_Curve(0).ToProtoType();
            var c1 = face.get_Curve(1).ToProtoType();

            return Surface.ByLoft(new[] {c0, c1});
        }
    }
}
