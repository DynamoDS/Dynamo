using System;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OpenGeometry
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly record struct NativePoint(double X, double Y, double Z);

    [StructLayout(LayoutKind.Sequential)]
    internal readonly record struct NativeVertex(NativePoint Position, NativePoint Normal);

    internal static class NativeGeometry
    {
        private const string Bridge = "OpenGeometryBridge";
        private const uint SupportedAbi = 9;
        private static readonly ResourceManager Messages = new("OpenGeometry.Messages", typeof(NativeGeometry).Assembly);

        internal sealed class SolidHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SolidHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                ReleaseSolid(handle);
                return true;
            }
        }

        internal sealed class CurveHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private CurveHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                ReleaseCurve(handle);
                return true;
            }
        }

        internal sealed class MeshHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private MeshHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                ReleaseMesh(handle);
                return true;
            }
        }

        internal sealed class GeomListHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private GeomListHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                ReleaseGeomList(handle);
                return true;
            }
        }

        internal sealed class BytesHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private BytesHandle() : base(true) { }

            protected override bool ReleaseHandle()
            {
                ReleaseBytes(handle);
                return true;
            }
        }

        static NativeGeometry()
        {
            if (GetAbiVersion() != SupportedAbi)
                throw new InvalidOperationException(Messages.GetString("IncompatibleBridge"));
        }

        internal static NotSupportedException UnsupportedFormat(string formatName)
        {
            return new NotSupportedException(string.Format(
                CultureInfo.CurrentCulture,
                Messages.GetString("UnsupportedFormat"),
                formatName));
        }

        /* ==================== Solids ==================== */

        internal static SolidHandle Box(NativePoint corner, double sizeX, double sizeY, double sizeZ)
        {
            var status = CreateBox(in corner, sizeX, sizeY, sizeZ, out var solid);
            return CheckHandle(status, solid, "Box");
        }

        internal static SolidHandle Cylinder(NativePoint start, NativePoint end, double radius)
        {
            var status = CreateCylinder(in start, in end, radius, out var solid);
            return CheckHandle(status, solid, "Cylinder");
        }

        internal static SolidHandle Sphere(NativePoint center, double radius)
        {
            var status = CreateSphere(in center, radius, out var solid);
            return CheckHandle(status, solid, "Sphere");
        }

        internal static SolidHandle Cone(NativePoint start, NativePoint end, double startRadius, double endRadius)
        {
            var status = CreateCone(in start, in end, startRadius, endRadius, out var solid);
            return CheckHandle(status, solid, "Cone");
        }

        internal static SolidHandle Difference(SolidHandle solid, SolidHandle other)
        {
            var status = Subtract(solid, other, out var result);
            return CheckHandle(status, result, "Difference");
        }

        internal static SolidHandle Union(SolidHandle solid, SolidHandle other)
        {
            var status = Unite(solid, other, out var result);
            return CheckHandle(status, result, "Union");
        }

        internal static SolidHandle Intersect(SolidHandle solid, SolidHandle other)
        {
            var status = IntersectSolids(solid, other, out var result);
            return CheckHandle(status, result, "Intersect");
        }

        internal static SolidHandle Translate(SolidHandle solid, NativePoint delta)
        {
            var status = TranslateSolid(solid, in delta, out var result);
            return CheckHandle(status, result, "Translate");
        }

        internal static SolidHandle Rotate(SolidHandle solid, NativePoint origin, NativePoint axis, double degrees)
        {
            var status = RotateSolid(solid, in origin, in axis, degrees, out var result);
            return CheckHandle(status, result, "Rotate");
        }

        internal static SolidHandle Scale(SolidHandle solid, NativePoint origin, double sx, double sy, double sz)
        {
            var status = ScaleSolid(solid, in origin, sx, sy, sz, out var result);
            return CheckHandle(status, result, "Scale");
        }

        internal static SolidHandle Transform(
            SolidHandle solid, NativePoint origin, NativePoint xAxis, NativePoint yAxis, NativePoint zAxis)
        {
            var status = TransformSolid(solid, in origin, in xAxis, in yAxis, in zAxis, out var result);
            return CheckHandle(status, result, "Transform");
        }

        internal static double Volume(SolidHandle solid)
        {
            Check(GetVolume(solid, out var volume), "Volume");
            return volume;
        }

        internal static double Area(SolidHandle solid)
        {
            Check(GetArea(solid, out var area), "Area");
            return area;
        }

        internal static NativePoint Centroid(SolidHandle solid)
        {
            Check(GetCentroid(solid, out var centroid), "Centroid");
            return centroid;
        }

        internal static (NativePoint Min, NativePoint Max) BoundingBox(SolidHandle solid)
        {
            Check(GetBoundingBox(solid, out var min, out var max), "BoundingBox");
            return (min, max);
        }

        internal static (NativePoint Point, double Distance) ClosestPoint(SolidHandle solid, NativePoint query)
        {
            Check(GetClosestPoint(solid, in query, out var closest, out var distance), "ClosestPoint");
            return (closest, distance);
        }

        internal static double Distance(SolidHandle a, SolidHandle b)
        {
            Check(GetSolidDistance(a, b, out var distance), "Distance");
            return distance;
        }

        internal static NativeVertex[] Tessellate(SolidHandle solid, double tolerance)
        {
            var status = CreateMesh(solid, tolerance, out var mesh);
            using var snapshot = CheckHandle(status, mesh, "Tessellation");
            var vertices = new NativeVertex[checked((int)GetVertexCount(snapshot))];
            Check(CopyVertices(snapshot, vertices, (uint)vertices.Length), "Tessellation");
            return vertices;
        }

        /* ==================== Curves ==================== */

        internal static CurveHandle Line(NativePoint start, NativePoint end)
        {
            var status = CreateLine(in start, in end, out var curve);
            return CheckHandle(status, curve, "CurveCreation");
        }

        internal static CurveHandle Circle(NativePoint center, NativePoint xAxis, NativePoint yAxis, double radius)
        {
            var status = CreateCircle(in center, in xAxis, in yAxis, radius, out var curve);
            return CheckHandle(status, curve, "CurveCreation");
        }

        internal static CurveHandle Arc(
            NativePoint center, NativePoint xAxis, NativePoint yAxis, double radius, double startDeg, double endDeg)
        {
            var status = CreateArc(in center, in xAxis, in yAxis, radius, startDeg, endDeg, out var curve);
            return CheckHandle(status, curve, "CurveCreation");
        }

        internal static CurveHandle InterpCurve(NativePoint[] points)
        {
            var status = CreateInterpCurve(points, (uint)points.Length, out var curve);
            return CheckHandle(status, curve, "CurveCreation");
        }

        internal static CurveHandle ControlPointCurve(NativePoint[] points, int degree)
        {
            var status = CreateControlPointCurve(points, (uint)points.Length, degree, out var curve);
            return CheckHandle(status, curve, "CurveCreation");
        }

        internal static double CurveLength(CurveHandle curve)
        {
            Check(GetCurveLength(curve, out var length), "CurveQuery");
            return length;
        }

        internal static bool CurveIsClosed(CurveHandle curve)
        {
            Check(GetCurveIsClosed(curve, out var isClosed), "CurveQuery");
            return isClosed != 0;
        }

        internal static (NativePoint Point, NativePoint Tangent, NativePoint Normal) CurveEvaluate(
            CurveHandle curve, double normalizedT)
        {
            Check(EvaluateCurve(curve, normalizedT, out var pt, out var tan, out var nrm), "CurveQuery");
            return (pt, tan, nrm);
        }

        internal static double CurveParameterAtLength(CurveHandle curve, double segmentLength)
        {
            Check(GetCurveParameterAtLength(curve, segmentLength, out var t), "CurveQuery");
            return t;
        }

        internal static (NativePoint Min, NativePoint Max) CurveBoundingBox(CurveHandle curve)
        {
            Check(GetCurveBoundingBox(curve, out var min, out var max), "CurveQuery");
            return (min, max);
        }

        internal static (NativePoint Point, double Parameter, double Distance) CurveClosestPoint(
            CurveHandle curve, NativePoint query)
        {
            Check(GetCurveClosestPoint(curve, in query, out var closest, out var t, out var dist), "CurveQuery");
            return (closest, t, dist);
        }

        internal static CurveHandle CurveReverse(CurveHandle curve)
        {
            var status = ReverseCurve(curve, out var result);
            return CheckHandle(status, result, "CurveTransform");
        }

        internal static CurveHandle CurveTrim(CurveHandle curve, double startT, double endT)
        {
            var status = TrimCurve(curve, startT, endT, out var result);
            return CheckHandle(status, result, "CurveTransform");
        }

        internal static CurveHandle CurveTranslate(CurveHandle curve, NativePoint delta)
        {
            var status = TranslateCurve(curve, in delta, out var result);
            return CheckHandle(status, result, "CurveTransform");
        }

        internal static CurveHandle CurveRotate(CurveHandle curve, NativePoint origin, NativePoint axis, double degrees)
        {
            var status = RotateCurve(curve, in origin, in axis, degrees, out var result);
            return CheckHandle(status, result, "CurveTransform");
        }

        internal static CurveHandle CurveScale(CurveHandle curve, NativePoint origin, double sx, double sy, double sz)
        {
            var status = ScaleCurve(curve, in origin, sx, sy, sz, out var result);
            return CheckHandle(status, result, "CurveTransform");
        }

        internal static CurveHandle CurveTransform(
            CurveHandle curve, NativePoint origin, NativePoint xAxis, NativePoint yAxis, NativePoint zAxis)
        {
            var status = TransformCurve(curve, in origin, in xAxis, in yAxis, in zAxis, out var result);
            return CheckHandle(status, result, "CurveTransform");
        }

        internal static NativePoint[] CurveSample(CurveHandle curve, uint segments)
        {
            Check(SampleCurve(curve, segments, null, 0, out var count), "Tessellation");
            var points = new NativePoint[checked((int)count)];
            Check(SampleCurve(curve, segments, points, count, out _), "Tessellation");
            return points;
        }

        /* ==================== Surfaces & PolySurfaces ==================== */

        internal static SolidHandle SurfaceByFourPoints(NativePoint p0, NativePoint p1, NativePoint p2, NativePoint p3)
        {
            var status = CreateSurfaceByFourPoints(in p0, in p1, in p2, in p3, out var result);
            return CheckHandle(status, result, "SurfaceCreation");
        }

        internal static SolidHandle SurfaceByPatch(CurveHandle[] curves)
        {
            return WithCurveHandlePointers(curves, (ptrs, count) =>
            {
                var status = CreateSurfaceByPatch(ptrs, count, out var result);
                return CheckHandle(status, result, "SurfaceCreation");
            });
        }

        internal static SolidHandle SurfaceByLoft(CurveHandle[] profiles)
        {
            return WithCurveHandlePointers(profiles, (ptrs, count) =>
            {
                var status = CreateSurfaceByLoft(ptrs, count, out var result);
                return CheckHandle(status, result, "SurfaceCreation");
            });
        }

        internal static (NativePoint Point, NativePoint Normal) SurfaceEvaluate(
            SolidHandle surface, double normalizedU, double normalizedV)
        {
            Check(EvaluateSurface(surface, normalizedU, normalizedV, out var pt, out var nrm), "SurfaceEvaluation");
            return (pt, nrm);
        }

        internal static SolidHandle JoinSurfaces(SolidHandle[] surfaces, bool makeSolid)
        {
            return WithSolidHandlePointers(surfaces, (ptrs, count) =>
            {
                var status = JoinSurfaceBreps(ptrs, count, makeSolid ? 1 : 0, out var result);
                return CheckHandle(status, result, "SurfaceJoin");
            });
        }

        /* ==================== Topology Queries ==================== */

        internal static (uint Faces, uint Edges, uint Vertices) TopologyCounts(SolidHandle brep)
        {
            Check(GetTopologyCounts(brep, out var f, out var e, out var v), "TopologyQuery");
            return (f, e, v);
        }

        internal static (double Area, NativePoint Min, NativePoint Max) FaceInfo(SolidHandle brep, uint faceIndex)
        {
            Check(GetFaceInfo(brep, faceIndex, out var area, out var min, out var max), "TopologyQuery");
            return (area, min, max);
        }

        internal static SolidHandle FaceToSurface(SolidHandle brep, uint faceIndex)
        {
            var status = ExtractFaceSurface(brep, faceIndex, out var result);
            return CheckHandle(status, result, "TopologyQuery");
        }

        internal static uint[] FaceEdges(SolidHandle brep, uint faceIndex)
        {
            return QueryIndexArray((buf, cap, out count) => GetFaceEdges(brep, faceIndex, buf, cap, out count));
        }

        internal static uint[] FaceVertices(SolidHandle brep, uint faceIndex)
        {
            return QueryIndexArray((buf, cap, out count) => GetFaceVertices(brep, faceIndex, buf, cap, out count));
        }

        internal static (uint StartVertex, uint EndVertex, double Length) EdgeInfo(SolidHandle brep, uint edgeIndex)
        {
            Check(GetEdgeInfo(brep, edgeIndex, out var sv, out var ev, out var len), "TopologyQuery");
            return (sv, ev, len);
        }

        internal static uint[] EdgeFaces(SolidHandle brep, uint edgeIndex)
        {
            return QueryIndexArray((buf, cap, out count) => GetEdgeFaces(brep, edgeIndex, buf, cap, out count));
        }

        internal static CurveHandle EdgeCurve(SolidHandle brep, uint edgeIndex)
        {
            var status = ExtractEdgeCurve(brep, edgeIndex, out var result);
            return CheckHandle(status, result, "TopologyQuery");
        }

        internal static NativePoint VertexPoint(SolidHandle brep, uint vertexIndex)
        {
            Check(GetVertexPoint(brep, vertexIndex, out var pt), "TopologyQuery");
            return pt;
        }

        internal static uint[] VertexEdges(SolidHandle brep, uint vertexIndex)
        {
            return QueryIndexArray((buf, cap, out count) => GetVertexEdges(brep, vertexIndex, buf, cap, out count));
        }

        internal static uint[] VertexFaces(SolidHandle brep, uint vertexIndex)
        {
            return QueryIndexArray((buf, cap, out count) => GetVertexFaces(brep, vertexIndex, buf, cap, out count));
        }

        /* ==================== Modeling Operations ==================== */

        internal static SolidHandle CurveExtrude(CurveHandle[] curves, NativePoint direction, double distance, bool capEnds)
        {
            return WithCurveHandlePointers(curves, (ptrs, count) =>
            {
                var status = ExtrudeCurves(ptrs, count, in direction, distance, capEnds ? 1 : 0, out var result);
                return CheckHandle(status, result, "Extrude");
            });
        }

        internal static SolidHandle SolidByLoft(CurveHandle[] curves, uint[] curvesPerProfile)
        {
            return WithCurveHandlePointers(curves, (ptrs, _) =>
            {
                var status = CreateSolidByLoft(ptrs, curvesPerProfile, (uint)curvesPerProfile.Length, out var result);
                return CheckHandle(status, result, "Loft");
            });
        }

        internal static SolidHandle SolidBySweep(CurveHandle[] profileCurves, CurveHandle pathCurve)
        {
            return WithCurveHandlePointers(profileCurves, (ptrs, count) =>
            {
                var status = CreateSolidBySweep(ptrs, count, pathCurve, out var result);
                return CheckHandle(status, result, "Sweep");
            });
        }

        internal static SolidHandle SolidByRevolve(
            CurveHandle[] profileCurves, NativePoint axisOrigin, NativePoint axisDir, double startDeg, double sweepDeg)
        {
            return WithCurveHandlePointers(profileCurves, (ptrs, count) =>
            {
                var status = CreateSolidByRevolve(ptrs, count, in axisOrigin, in axisDir, startDeg, sweepDeg, out var result);
                return CheckHandle(status, result, "Revolve");
            });
        }

        internal static SolidHandle SurfaceOffset(SolidHandle surface, double distance)
        {
            var status = OffsetSurfaceBrep(surface, distance, out var result);
            return CheckHandle(status, result, "Offset");
        }

        internal static SolidHandle SurfaceThicken(SolidHandle surface, double thickness, bool bothSides)
        {
            var status = ThickenSurfaceBrep(surface, thickness, bothSides ? 1 : 0, out var result);
            return CheckHandle(status, result, "Thicken");
        }

        internal static SolidHandle SolidFilletEdges(SolidHandle solid, uint[] edgeIndices, double radius, bool chamfer)
        {
            var status = FilletSolidEdges(solid, edgeIndices, (uint)edgeIndices.Length, radius, chamfer ? 1 : 0, out var result);
            return CheckHandle(status, result, chamfer ? "Chamfer" : "Fillet");
        }

        internal static SolidHandle SolidThinShell(SolidHandle solid, double internalThickness, double externalThickness)
        {
            var status = ThinShellSolid(solid, internalThickness, externalThickness, out var result);
            return CheckHandle(status, result, "ThinShell");
        }

        internal static SolidHandle SolidCutPlaneOrNull(SolidHandle solid, NativePoint origin, NativePoint normal)
        {
            var status = CutSolidPlane(solid, in origin, in normal, out var result);
            if (status != 0)
            {
                result.Dispose();
                throw CreateException(status, "Split");
            }
            if (result.IsInvalid)
            {
                result.Dispose();
                return null;
            }
            return result;
        }

        /* ==================== Slice D: Curves, NURBS Surfaces, Trimming, Intersections ==================== */

        internal static CurveHandle Ellipse(
            NativePoint center, NativePoint xAxis, NativePoint yAxis, double radiusX, double radiusY)
        {
            var status = CreateEllipse(in center, in xAxis, in yAxis, radiusX, radiusY, out var curve);
            return CheckHandle(status, curve, "Ellipse");
        }

        internal static CurveHandle EllipseArc(
            NativePoint center,
            NativePoint xAxis,
            NativePoint yAxis,
            double radiusX,
            double radiusY,
            double startDeg,
            double sweepDeg)
        {
            var status = CreateEllipseArc(
                in center, in xAxis, in yAxis, radiusX, radiusY, startDeg, sweepDeg, out var curve);
            return CheckHandle(status, curve, "Ellipse");
        }

        internal static CurveHandle Helix(
            NativePoint axisPoint, NativePoint axisDir, NativePoint startPoint, double pitch, double angleTurnsDeg)
        {
            var status = CreateHelix(in axisPoint, in axisDir, in startPoint, pitch, angleTurnsDeg, out var curve);
            return CheckHandle(status, curve, "Helix");
        }

        internal static CurveHandle CurveOffset(CurveHandle curve, double distance)
        {
            var status = OffsetCurve(curve, distance, out var result);
            return CheckHandle(status, result, "CurveOffset");
        }

        internal static (NativePoint[] Points, CurveHandle[] Curves) CurveProjectOntoBrep(
            CurveHandle curve, SolidHandle brep, NativePoint direction)
        {
            var status = ProjectCurveOntoBrep(curve, brep, in direction, out var list);
            using var owned = CheckHandle(status, list, "CurveProject");
            return DrainGeomList(owned, "CurveProject");
        }

        internal static SolidHandle NurbsSurfaceByControlPoints(
            NativePoint[] points, uint uCount, uint vCount, int uDegree, int vDegree)
        {
            var status = CreateNurbsSurfaceByControlPoints(points, uCount, vCount, uDegree, vDegree, out var result);
            return CheckHandle(status, result, "NurbsSurface");
        }

        internal static SolidHandle NurbsSurfaceByPoints(
            NativePoint[] points, uint uCount, uint vCount, int uDegree, int vDegree)
        {
            var status = CreateNurbsSurfaceByPoints(points, uCount, vCount, uDegree, vDegree, out var result);
            return CheckHandle(status, result, "NurbsSurface");
        }

        internal static (int UDegree, int VDegree, uint UCount, uint VCount, bool IsRational, bool IsPeriodicU, bool IsPeriodicV)
            NurbsSurfaceInfo(SolidHandle surface)
        {
            Check(
                GetNurbsSurfaceInfo(
                    surface,
                    out var uDeg,
                    out var vDeg,
                    out var uCount,
                    out var vCount,
                    out var rational,
                    out var periodicU,
                    out var periodicV),
                "NurbsSurface");
            return (uDeg, vDeg, uCount, vCount, rational != 0, periodicU != 0, periodicV != 0);
        }

        internal static (NativePoint[] Points, double[] Weights) NurbsSurfaceControlPoints(SolidHandle surface)
        {
            Check(GetNurbsSurfaceControlPoints(surface, null, null, 0, out var count), "NurbsSurface");
            var pts = new NativePoint[checked((int)count)];
            var wts = new double[checked((int)count)];
            Check(GetNurbsSurfaceControlPoints(surface, pts, wts, count, out _), "NurbsSurface");
            return (pts, wts);
        }

        internal static double[] NurbsSurfaceKnots(SolidHandle surface, bool vDirection)
        {
            var dir = vDirection ? 1 : 0;
            Check(GetNurbsSurfaceKnots(surface, dir, null, 0, out var count), "NurbsSurface");
            var knots = new double[checked((int)count)];
            Check(GetNurbsSurfaceKnots(surface, dir, knots, count, out _), "NurbsSurface");
            return knots;
        }

        internal static SolidHandle SurfaceBySweep(CurveHandle profile, CurveHandle path)
        {
            var status = CreateSurfaceBySweep(profile, path, out var result);
            return CheckHandle(status, result, "Sweep");
        }

        internal static SolidHandle SurfaceBySweep2Rails(CurveHandle path, CurveHandle guideRail, CurveHandle profile)
        {
            var status = CreateSurfaceBySweep2Rails(path, guideRail, profile, out var result);
            return CheckHandle(status, result, "Sweep");
        }

        internal static SolidHandle SurfaceByRevolve(
            CurveHandle[] profileCurves, NativePoint axisOrigin, NativePoint axisDir, double startDeg, double sweepDeg)
        {
            return WithCurveHandlePointers(profileCurves, (ptrs, count) =>
            {
                var status = CreateSurfaceByRevolve(
                    ptrs, count, in axisOrigin, in axisDir, startDeg, sweepDeg, out var result);
                return CheckHandle(status, result, "Revolve");
            });
        }

        internal static SolidHandle SurfaceTrimWithCurves(
            SolidHandle surface, CurveHandle[] curves, uint[] curvesPerLoop)
        {
            return WithCurveHandlePointers(curves, (ptrs, _) =>
            {
                var status = TrimSurfaceWithCurves(
                    surface, ptrs, curvesPerLoop, (uint)curvesPerLoop.Length, out var result);
                return CheckHandle(status, result, "SurfaceTrim");
            });
        }

        internal static (NativePoint[] Points, CurveHandle[] Curves) IntersectCurves(CurveHandle a, CurveHandle b)
        {
            var status = IntersectCurvePair(a, b, out var list);
            using var owned = CheckHandle(status, list, "Intersection");
            return DrainGeomList(owned, "Intersection");
        }

        internal static (NativePoint[] Points, CurveHandle[] Curves) IntersectCurveBrep(
            CurveHandle curve, SolidHandle brep)
        {
            var status = IntersectCurveWithBrep(curve, brep, out var list);
            using var owned = CheckHandle(status, list, "Intersection");
            return DrainGeomList(owned, "Intersection");
        }

        internal static (NativePoint[] Points, CurveHandle[] Curves) IntersectBreps(SolidHandle a, SolidHandle b)
        {
            var status = IntersectBrepPair(a, b, out var list);
            using var owned = CheckHandle(status, list, "Intersection");
            return DrainGeomList(owned, "Intersection");
        }

        internal static (NativePoint[] Points, CurveHandle[] Curves) IntersectBrepPlane(
            SolidHandle brep, NativePoint origin, NativePoint normal)
        {
            var status = IntersectBrepWithPlane(brep, in origin, in normal, out var list);
            using var owned = CheckHandle(status, list, "Intersection");
            return DrainGeomList(owned, "Intersection");
        }

        internal static NativeVertex[] PolyBoolean(
            NativePoint[] vertsA,
            uint[] trisA,
            NativePoint[] vertsB,
            uint[] trisB,
            int operation)
        {
            var status = RunPolyBoolean(
                vertsA,
                (uint)vertsA.Length,
                trisA,
                (uint)(trisA.Length / 3),
                vertsB,
                (uint)vertsB.Length,
                trisB,
                (uint)(trisB.Length / 3),
                operation,
                out var mesh);
            using var snapshot = CheckHandle(status, mesh, "MeshBoolean");
            var vertices = new NativeVertex[checked((int)GetVertexCount(snapshot))];
            Check(CopyVertices(snapshot, vertices, (uint)vertices.Length), "MeshBoolean");
            return vertices;
        }

        internal static NativeVertex[] PolyPlaneCut(
            NativePoint[] verts, uint[] tris, NativePoint planeOrigin, NativePoint planeNormal, bool makeSolid)
        {
            var status = RunPolyPlaneCut(
                verts,
                (uint)verts.Length,
                tris,
                (uint)(tris.Length / 3),
                in planeOrigin,
                in planeNormal,
                makeSolid ? 1 : 0,
                out var mesh);
            using var snapshot = CheckHandle(status, mesh, "MeshPlaneCut");
            var vertices = new NativeVertex[checked((int)GetVertexCount(snapshot))];
            Check(CopyVertices(snapshot, vertices, (uint)vertices.Length), "MeshPlaneCut");
            return vertices;
        }

        internal static NativePoint[] PolySectionPlane(
            NativePoint[] verts, uint[] tris, NativePoint planeOrigin, NativePoint planeNormal)
        {
            var status = RunPolySectionPlane(
                verts,
                (uint)verts.Length,
                tris,
                (uint)(tris.Length / 3),
                in planeOrigin,
                in planeNormal,
                out var list);
            using var owned = CheckHandle(status, list, "MeshSection");
            var (points, _) = DrainGeomList(owned, "MeshSection");
            return points;
        }

        internal static NativeVertex[] PolyMakeWatertight(NativePoint[] verts, uint[] tris)
        {
            var status = RunPolyMakeWatertight(
                verts, (uint)verts.Length, tris, (uint)(tris.Length / 3), out var mesh);
            using var snapshot = CheckHandle(status, mesh, "MeshWatertight");
            var vertices = new NativeVertex[checked((int)GetVertexCount(snapshot))];
            Check(CopyVertices(snapshot, vertices, (uint)vertices.Length), "MeshWatertight");
            return vertices;
        }

        internal static SolidHandle BrepMirror(
            SolidHandle solid, NativePoint planeOrigin, NativePoint planeX, NativePoint planeY)
        {
            var status = MirrorBrep(solid, in planeOrigin, in planeX, in planeY, out var result);
            return CheckHandle(status, result, "Mirror");
        }

        internal static CurveHandle CurveMirror(
            CurveHandle curve, NativePoint planeOrigin, NativePoint planeX, NativePoint planeY)
        {
            var status = MirrorCurve(curve, in planeOrigin, in planeX, in planeY, out var result);
            return CheckHandle(status, result, "Mirror");
        }

        internal static CurveHandle RationalCurve(
            NativePoint[] points, double[] weights, double[] knots, int degree)
        {
            var status = CreateRationalCurve(
                points, weights, (uint)points.Length, knots, (uint)knots.Length, degree, out var result);
            return CheckHandle(status, result, "CurveCreation");
        }

        private static (NativePoint[] Points, CurveHandle[] Curves) DrainGeomList(
            GeomListHandle list, string operation)
        {
            var ptCount = checked((int)GetGeomListPointCount(list));
            var points = new NativePoint[ptCount];
            for (var i = 0; i < ptCount; i++)
                Check(GetGeomListPoint(list, (uint)i, out points[i]), operation);

            var crvCount = checked((int)GetGeomListCurveCount(list));
            var curves = new CurveHandle[crvCount];
            for (var i = 0; i < crvCount; i++)
            {
                var status = TakeGeomListCurve(list, (uint)i, out var curve);
                curves[i] = CheckHandle(status, curve, operation);
            }
            return (points, curves);
        }

        /* ==================== Slice H: Open-Format BRep & Curve Serialization ==================== */

        internal static byte[] SerializeBrep(SolidHandle solid)
        {
            var status = SerializeBrepNative(solid, out var handle);
            using var owned = CheckHandle(status, handle, "SerializeBrep");
            return DrainBytes(owned, "SerializeBrep");
        }

        internal static SolidHandle DeserializeBrep(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Serialized BRep payload must not be empty.", nameof(data));
            var status = DeserializeBrepNative(data, checked((uint)data.Length), out var result);
            return CheckHandle(status, result, "DeserializeBrep");
        }

        internal static byte[] SerializeCurve(CurveHandle curve)
        {
            var status = SerializeCurveNative(curve, out var handle);
            using var owned = CheckHandle(status, handle, "SerializeCurve");
            return DrainBytes(owned, "SerializeCurve");
        }

        internal static CurveHandle DeserializeCurve(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Serialized curve payload must not be empty.", nameof(data));
            var status = DeserializeCurveNative(data, checked((uint)data.Length), out var result);
            return CheckHandle(status, result, "DeserializeCurve");
        }

        internal static void ExportBrepSmb(SolidHandle solid, string filePath, bool binary = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be empty.", nameof(filePath));
            Check(ExportBrepSmbNative(solid, filePath, binary ? 1 : 0), "ExportSmb");
        }

        internal static SolidHandle ImportBrepSmb(string filePath, bool binary = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be empty.", nameof(filePath));
            var status = ImportBrepSmbNative(filePath, binary ? 1 : 0, out var result);
            return CheckHandle(status, result, "ImportSmb");
        }

        private static byte[] DrainBytes(BytesHandle handle, string operation)
        {
            var len = GetBytesLength(handle);
            var buffer = new byte[checked((int)len)];
            if (len > 0)
                Check(CopyBytes(handle, buffer, len), operation);
            return buffer;
        }

        /* ==================== Helpers ==================== */

        private delegate int IndexQueryCallback(uint[] buffer, uint capacity, out uint actualCount);

        private static uint[] QueryIndexArray(IndexQueryCallback callback)
        {
            Check(callback(null, 0, out var count), "TopologyQuery");
            if (count == 0)
                return Array.Empty<uint>();
            var buffer = new uint[checked((int)count)];
            Check(callback(buffer, count, out _), "TopologyQuery");
            return buffer;
        }

        private static T WithCurveHandlePointers<T>(CurveHandle[] handles, Func<IntPtr[], uint, T> action)
        {
            var added = new bool[handles.Length];
            var ptrs = new IntPtr[handles.Length];
            try
            {
                for (var i = 0; i < handles.Length; i++)
                {
                    handles[i].DangerousAddRef(ref added[i]);
                    ptrs[i] = handles[i].DangerousGetHandle();
                }
                return action(ptrs, (uint)ptrs.Length);
            }
            finally
            {
                for (var i = 0; i < handles.Length; i++)
                {
                    if (added[i])
                        handles[i].DangerousRelease();
                }
            }
        }

        private static T WithSolidHandlePointers<T>(SolidHandle[] handles, Func<IntPtr[], uint, T> action)
        {
            var added = new bool[handles.Length];
            var ptrs = new IntPtr[handles.Length];
            try
            {
                for (var i = 0; i < handles.Length; i++)
                {
                    handles[i].DangerousAddRef(ref added[i]);
                    ptrs[i] = handles[i].DangerousGetHandle();
                }
                return action(ptrs, (uint)ptrs.Length);
            }
            finally
            {
                for (var i = 0; i < handles.Length; i++)
                {
                    if (added[i])
                        handles[i].DangerousRelease();
                }
            }
        }

        private static T CheckHandle<T>(int status, T handle, string operation)
            where T : SafeHandleZeroOrMinusOneIsInvalid
        {
            if (status == 0 && !handle.IsInvalid)
                return handle;

            handle.Dispose();
            throw CreateException(status == 0 ? 3 : status, operation);
        }

        private static void Check(int status, string operation)
        {
            if (status == 0)
                return;

            throw CreateException(status, operation);
        }

        private static Exception CreateException(int status, string operation)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Messages.GetString("OperationFailed"),
                Messages.GetString(operation), status);
            return status == 1 ? new ArgumentException(message) : new InvalidOperationException(message);
        }

        /* ==================== P/Invoke ==================== */

        [DllImport(Bridge, EntryPoint = "og_abi_version", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetAbiVersion();

        [DllImport(Bridge, EntryPoint = "og_create_box", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateBox(in NativePoint corner, double sizeX, double sizeY, double sizeZ, out SolidHandle solid);

        [DllImport(Bridge, EntryPoint = "og_create_cylinder", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateCylinder(in NativePoint start, in NativePoint end, double radius, out SolidHandle solid);

        [DllImport(Bridge, EntryPoint = "og_create_sphere", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSphere(in NativePoint center, double radius, out SolidHandle solid);

        [DllImport(Bridge, EntryPoint = "og_create_cone", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateCone(
            in NativePoint start, in NativePoint end, double startRadius, double endRadius, out SolidHandle solid);

        [DllImport(Bridge, EntryPoint = "og_difference", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Subtract(SolidHandle solid, SolidHandle other, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_union", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Unite(SolidHandle solid, SolidHandle other, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_intersect", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int IntersectSolids(SolidHandle solid, SolidHandle other, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_translate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TranslateSolid(SolidHandle solid, in NativePoint delta, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_rotate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RotateSolid(
            SolidHandle solid, in NativePoint origin, in NativePoint axis, double degrees, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_scale", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ScaleSolid(
            SolidHandle solid, in NativePoint origin, double sx, double sy, double sz, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_transform", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TransformSolid(
            SolidHandle solid,
            in NativePoint origin,
            in NativePoint xAxis,
            in NativePoint yAxis,
            in NativePoint zAxis,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_volume", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetVolume(SolidHandle solid, out double volume);

        [DllImport(Bridge, EntryPoint = "og_area", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetArea(SolidHandle solid, out double area);

        [DllImport(Bridge, EntryPoint = "og_centroid", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCentroid(SolidHandle solid, out NativePoint centroid);

        [DllImport(Bridge, EntryPoint = "og_bounding_box", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetBoundingBox(SolidHandle solid, out NativePoint min, out NativePoint max);

        [DllImport(Bridge, EntryPoint = "og_closest_point", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetClosestPoint(
            SolidHandle solid, in NativePoint query, out NativePoint closest, out double distance);

        [DllImport(Bridge, EntryPoint = "og_solid_distance", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSolidDistance(SolidHandle a, SolidHandle b, out double distance);

        [DllImport(Bridge, EntryPoint = "og_tessellate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateMesh(SolidHandle solid, double tolerance, out MeshHandle mesh);

        [DllImport(Bridge, EntryPoint = "og_mesh_vertex_count", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetVertexCount(MeshHandle mesh);

        [DllImport(Bridge, EntryPoint = "og_copy_mesh_vertices", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CopyVertices(MeshHandle mesh, [Out] NativeVertex[] vertices, uint capacity);

        [DllImport(Bridge, EntryPoint = "og_release_solid", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseSolid(IntPtr solid);

        [DllImport(Bridge, EntryPoint = "og_release_mesh", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseMesh(IntPtr mesh);

        /* Curve P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_curve_create_line", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateLine(in NativePoint start, in NativePoint end, out CurveHandle curve);

        [DllImport(Bridge, EntryPoint = "og_curve_create_circle", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateCircle(
            in NativePoint center, in NativePoint xAxis, in NativePoint yAxis, double radius, out CurveHandle curve);

        [DllImport(Bridge, EntryPoint = "og_curve_create_arc", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateArc(
            in NativePoint center,
            in NativePoint xAxis,
            in NativePoint yAxis,
            double radius,
            double startDeg,
            double endDeg,
            out CurveHandle curve);

        [DllImport(Bridge, EntryPoint = "og_curve_create_interp_points", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateInterpCurve(
            [In] NativePoint[] points, uint count, out CurveHandle curve);

        [DllImport(Bridge, EntryPoint = "og_curve_create_control_points", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateControlPointCurve(
            [In] NativePoint[] points, uint count, int degree, out CurveHandle curve);

        [DllImport(Bridge, EntryPoint = "og_curve_length", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCurveLength(CurveHandle curve, out double length);

        [DllImport(Bridge, EntryPoint = "og_curve_is_closed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCurveIsClosed(CurveHandle curve, out int isClosed);

        [DllImport(Bridge, EntryPoint = "og_curve_evaluate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int EvaluateCurve(
            CurveHandle curve, double normalizedT, out NativePoint point, out NativePoint tangent, out NativePoint normal);

        [DllImport(Bridge, EntryPoint = "og_curve_parameter_at_length", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCurveParameterAtLength(CurveHandle curve, double segmentLength, out double normalizedT);

        [DllImport(Bridge, EntryPoint = "og_curve_bounding_box", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCurveBoundingBox(CurveHandle curve, out NativePoint min, out NativePoint max);

        [DllImport(Bridge, EntryPoint = "og_curve_closest_point", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCurveClosestPoint(
            CurveHandle curve, in NativePoint query, out NativePoint closest, out double normalizedT, out double distance);

        [DllImport(Bridge, EntryPoint = "og_curve_reverse", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ReverseCurve(CurveHandle curve, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_trim", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TrimCurve(CurveHandle curve, double startT, double endT, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_translate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TranslateCurve(CurveHandle curve, in NativePoint delta, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_rotate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RotateCurve(
            CurveHandle curve, in NativePoint origin, in NativePoint axis, double degrees, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_scale", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ScaleCurve(
            CurveHandle curve, in NativePoint origin, double sx, double sy, double sz, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_transform", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TransformCurve(
            CurveHandle curve,
            in NativePoint origin,
            in NativePoint xAxis,
            in NativePoint yAxis,
            in NativePoint zAxis,
            out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_sample", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SampleCurve(
            CurveHandle curve, uint segments, [Out] NativePoint[] points, uint capacity, out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_release_curve", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseCurve(IntPtr curve);

        /* Surface & PolySurface P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_surface_by_four_points", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSurfaceByFourPoints(
            in NativePoint p0, in NativePoint p1, in NativePoint p2, in NativePoint p3, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_by_patch", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSurfaceByPatch([In] IntPtr[] curves, uint count, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_by_loft", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSurfaceByLoft([In] IntPtr[] profiles, uint count, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_evaluate", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int EvaluateSurface(
            SolidHandle surface, double normalizedU, double normalizedV, out NativePoint point, out NativePoint normal);

        [DllImport(Bridge, EntryPoint = "og_brep_join_surfaces", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int JoinSurfaceBreps(
            [In] IntPtr[] breps, uint count, int makeSolid, out SolidHandle result);

        /* Topology P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_brep_topology_counts", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTopologyCounts(
            SolidHandle brep, out uint faceCount, out uint edgeCount, out uint vertexCount);

        [DllImport(Bridge, EntryPoint = "og_brep_face_info", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceInfo(
            SolidHandle brep, uint faceIndex, out double area, out NativePoint min, out NativePoint max);

        [DllImport(Bridge, EntryPoint = "og_brep_face_to_surface", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ExtractFaceSurface(SolidHandle brep, uint faceIndex, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_brep_face_edges", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceEdges(
            SolidHandle brep, uint faceIndex, [Out] uint[] edgeIndices, uint capacity, out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_brep_face_vertices", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFaceVertices(
            SolidHandle brep, uint faceIndex, [Out] uint[] vertexIndices, uint capacity, out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_brep_edge_info", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetEdgeInfo(
            SolidHandle brep, uint edgeIndex, out uint startVertexIndex, out uint endVertexIndex, out double length);

        [DllImport(Bridge, EntryPoint = "og_brep_edge_faces", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetEdgeFaces(
            SolidHandle brep, uint edgeIndex, [Out] uint[] faceIndices, uint capacity, out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_brep_edge_curve", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ExtractEdgeCurve(SolidHandle brep, uint edgeIndex, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_brep_vertex_point", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetVertexPoint(SolidHandle brep, uint vertexIndex, out NativePoint point);

        [DllImport(Bridge, EntryPoint = "og_brep_vertex_edges", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetVertexEdges(
            SolidHandle brep, uint vertexIndex, [Out] uint[] edgeIndices, uint capacity, out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_brep_vertex_faces", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetVertexFaces(
            SolidHandle brep, uint vertexIndex, [Out] uint[] faceIndices, uint capacity, out uint actualCount);

        /* Modeling P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_curve_extrude", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ExtrudeCurves(
            [In] IntPtr[] curves,
            uint count,
            in NativePoint direction,
            double distance,
            int capEnds,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_solid_by_loft", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSolidByLoft(
            [In] IntPtr[] curves,
            [In] uint[] curvesPerProfile,
            uint profileCount,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_solid_by_sweep", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSolidBySweep(
            [In] IntPtr[] profileCurves,
            uint profileCount,
            CurveHandle pathCurve,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_solid_by_revolve", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSolidByRevolve(
            [In] IntPtr[] profileCurves,
            uint profileCount,
            in NativePoint axisOrigin,
            in NativePoint axisDir,
            double startDeg,
            double sweepDeg,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_offset", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int OffsetSurfaceBrep(SolidHandle surface, double distance, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_thicken", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ThickenSurfaceBrep(
            SolidHandle surface, double thickness, int bothSides, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_solid_fillet_edges", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FilletSolidEdges(
            SolidHandle solid,
            [In] uint[] edgeIndices,
            uint edgeCount,
            double radius,
            int chamfer,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_solid_thin_shell", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ThinShellSolid(
            SolidHandle solid, double internalThickness, double externalThickness, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_solid_cut_plane", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CutSolidPlane(
            SolidHandle solid, in NativePoint origin, in NativePoint normal, out SolidHandle result);

        /* Slice D P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_curve_create_ellipse", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateEllipse(
            in NativePoint center,
            in NativePoint xAxis,
            in NativePoint yAxis,
            double radiusX,
            double radiusY,
            out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_create_ellipse_arc", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateEllipseArc(
            in NativePoint center,
            in NativePoint xAxis,
            in NativePoint yAxis,
            double radiusX,
            double radiusY,
            double startDeg,
            double sweepDeg,
            out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_create_helix", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateHelix(
            in NativePoint axisPoint,
            in NativePoint axisDir,
            in NativePoint startPoint,
            double pitch,
            double angleTurnsDeg,
            out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_offset", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int OffsetCurve(CurveHandle curve, double distance, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_project_onto_brep", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ProjectCurveOntoBrep(
            CurveHandle curve, SolidHandle brep, in NativePoint direction, out GeomListHandle result);

        [DllImport(Bridge, EntryPoint = "og_nurbs_surface_by_control_points", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateNurbsSurfaceByControlPoints(
            [In] NativePoint[] points, uint uCount, uint vCount, int uDegree, int vDegree, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_nurbs_surface_by_points", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateNurbsSurfaceByPoints(
            [In] NativePoint[] points, uint uCount, uint vCount, int uDegree, int vDegree, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_nurbs_surface_info", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNurbsSurfaceInfo(
            SolidHandle surface,
            out int uDegree,
            out int vDegree,
            out uint uCount,
            out uint vCount,
            out int isRational,
            out int isPeriodicU,
            out int isPeriodicV);

        [DllImport(Bridge, EntryPoint = "og_nurbs_surface_control_points", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNurbsSurfaceControlPoints(
            SolidHandle surface,
            [Out] NativePoint[] points,
            [Out] double[] weights,
            uint capacity,
            out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_nurbs_surface_knots", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetNurbsSurfaceKnots(
            SolidHandle surface,
            int vDirection,
            [Out] double[] knots,
            uint capacity,
            out uint actualCount);

        [DllImport(Bridge, EntryPoint = "og_surface_by_sweep", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSurfaceBySweep(
            CurveHandle profile, CurveHandle path, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_by_sweep_2_rails", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSurfaceBySweep2Rails(
            CurveHandle path, CurveHandle guideRail, CurveHandle profile, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_by_revolve", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateSurfaceByRevolve(
            [In] IntPtr[] profileCurves,
            uint profileCount,
            in NativePoint axisOrigin,
            in NativePoint axisDir,
            double startDeg,
            double sweepDeg,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_surface_trim_with_curves", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TrimSurfaceWithCurves(
            SolidHandle surface,
            [In] IntPtr[] curves,
            [In] uint[] curvesPerLoop,
            uint loopCount,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_intersect_curves", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int IntersectCurvePair(
            CurveHandle curveA, CurveHandle curveB, out GeomListHandle result);

        [DllImport(Bridge, EntryPoint = "og_intersect_curve_brep", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int IntersectCurveWithBrep(
            CurveHandle curve, SolidHandle brep, out GeomListHandle result);

        [DllImport(Bridge, EntryPoint = "og_intersect_breps", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int IntersectBrepPair(
            SolidHandle brepA, SolidHandle brepB, out GeomListHandle result);

        [DllImport(Bridge, EntryPoint = "og_intersect_brep_plane", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int IntersectBrepWithPlane(
            SolidHandle brep, in NativePoint planeOrigin, in NativePoint planeNormal, out GeomListHandle result);

        [DllImport(Bridge, EntryPoint = "og_geom_list_point_count", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetGeomListPointCount(GeomListHandle list);

        [DllImport(Bridge, EntryPoint = "og_geom_list_get_point", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetGeomListPoint(GeomListHandle list, uint index, out NativePoint point);

        [DllImport(Bridge, EntryPoint = "og_geom_list_curve_count", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetGeomListCurveCount(GeomListHandle list);

        [DllImport(Bridge, EntryPoint = "og_geom_list_take_curve", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TakeGeomListCurve(GeomListHandle list, uint index, out CurveHandle curve);

        [DllImport(Bridge, EntryPoint = "og_release_geom_list", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseGeomList(IntPtr list);

        /* Slice E P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_poly_boolean", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RunPolyBoolean(
            [In] NativePoint[] vertsA,
            uint vertCountA,
            [In] uint[] trisA,
            uint triCountA,
            [In] NativePoint[] vertsB,
            uint vertCountB,
            [In] uint[] trisB,
            uint triCountB,
            int operation,
            out MeshHandle result);

        /* Slice F P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_poly_plane_cut", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RunPolyPlaneCut(
            [In] NativePoint[] verts,
            uint vertCount,
            [In] uint[] tris,
            uint triCount,
            in NativePoint planeOrigin,
            in NativePoint planeNormal,
            int makeSolid,
            out MeshHandle result);

        [DllImport(Bridge, EntryPoint = "og_poly_section_plane", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RunPolySectionPlane(
            [In] NativePoint[] verts,
            uint vertCount,
            [In] uint[] tris,
            uint triCount,
            in NativePoint planeOrigin,
            in NativePoint planeNormal,
            out GeomListHandle result);

        [DllImport(Bridge, EntryPoint = "og_poly_make_watertight", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int RunPolyMakeWatertight(
            [In] NativePoint[] verts,
            uint vertCount,
            [In] uint[] tris,
            uint triCount,
            out MeshHandle result);

        /* Slice G P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_brep_mirror", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MirrorBrep(
            SolidHandle solid,
            in NativePoint planeOrigin,
            in NativePoint planeX,
            in NativePoint planeY,
            out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_mirror", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int MirrorCurve(
            CurveHandle curve,
            in NativePoint planeOrigin,
            in NativePoint planeX,
            in NativePoint planeY,
            out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_create_rational", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateRationalCurve(
            [In] NativePoint[] points,
            [In] double[] weights,
            uint pointCount,
            [In] double[] knots,
            uint knotCount,
            int degree,
            out CurveHandle result);

        /* Slice H P/Invokes */
        [DllImport(Bridge, EntryPoint = "og_brep_serialize", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SerializeBrepNative(SolidHandle solid, out BytesHandle result);

        [DllImport(Bridge, EntryPoint = "og_brep_deserialize", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DeserializeBrepNative([In] byte[] data, uint length, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_serialize", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SerializeCurveNative(CurveHandle curve, out BytesHandle result);

        [DllImport(Bridge, EntryPoint = "og_curve_deserialize", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int DeserializeCurveNative([In] byte[] data, uint length, out CurveHandle result);

        [DllImport(Bridge, EntryPoint = "og_bytes_length", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetBytesLength(BytesHandle bytes);

        [DllImport(Bridge, EntryPoint = "og_bytes_copy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CopyBytes(BytesHandle bytes, [Out] byte[] outData, uint capacity);

        [DllImport(Bridge, EntryPoint = "og_release_bytes", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReleaseBytes(IntPtr bytes);

        [DllImport(Bridge, EntryPoint = "og_brep_export_smb", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int ExportBrepSmbNative(SolidHandle solid, string filePath, int binary);

        [DllImport(Bridge, EntryPoint = "og_brep_import_smb", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int ImportBrepSmbNative(string filePath, int binary, out SolidHandle result);
    }
}
