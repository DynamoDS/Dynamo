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
        private const uint SupportedAbi = 3;
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
    }
}
