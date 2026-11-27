using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Autodesk.DesignScript.Geometry;

namespace OpenGeometry
{
    internal static class GeometryJsonSerializer
    {
        internal const string VectorTypeId = "autodesk.math:vector3d-1.0.0";
        internal const string MatrixTypeId = "autodesk.math:matrix44d-1.0.0";
        internal const string BoundingBoxTypeId = "autodesk.geometry:boundingbox3d-1.0.0";
        internal const string PointTypeId = "autodesk.math:point3d-1.0.0";
        internal const string MeshTypeId = "dynamo.geometry:mesh-1.0.0";
        internal const string PlaneTypeId = "autodesk.geometry:plane-1.0.0";
        internal const string LineTypeId = "autodesk.geometry:linesegment3d-1.0.0";
        internal const string CircleTypeId = "autodesk.geometry:circle3d-1.0.0";
        internal const string ArcTypeId = "autodesk.geometry:arc3d-1.0.0";
        internal const string EllipseTypeId = "autodesk.geometry:ellipse3d-1.0.0";
        internal const string EllipseArcTypeId = "autodesk.geometry:ellipsearc3d-1.0.0";
        internal const string HelixTypeId = "autodesk.geometry:helix3d-1.0.0";
        internal const string NurbsCurveTypeId = "autodesk.geometry:nurbscurve3d-1.0.0";
        internal const string CurveTypeId = "autodesk.geometry:curve3d-1.0.0";
        internal const string RectangleTypeId = "dynamo.geometry:rectangle-1.0.0";
        internal const string PolygonTypeId = "autodesk.geometry:polygon3d-1.0.0";
        internal const string PolyCurveTypeId = "autodesk.geometry:polycurve3d-1.0.0";
        internal const string CuboidTypeId = "dynamo.geometry:cuboid-1.0.0";
        internal const string CylinderTypeId = "autodesk.geometry:cylinder-1.0.0";
        internal const string SphereTypeId = "autodesk.geometry:sphere-1.0.0";
        internal const string ConeTypeId = "autodesk.geometry:cone-1.0.0";
        internal const string SolidTypeId = "dynamo.geometry:solid-1.0.0";
        internal const string NurbsSurfaceTypeId = "autodesk.geometry:nurbssurface-1.0.0";
        internal const string PolySurfaceTypeId = "autodesk.geometry:polysurface-1.0.0";
        internal const string SurfaceTypeId = "autodesk.geometry:surface-1.0.0";
        internal const string PanelSurfaceTypeId = "autodesk.geometry:panelsurface-1.0.0";

        internal static string SerializeVector(Vector vector)
        {
            ArgumentNullException.ThrowIfNull(vector);
            return WriteJson(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("$typeid", VectorTypeId);
                writer.WriteNumber("x", vector.X);
                writer.WriteNumber("y", vector.Y);
                writer.WriteNumber("z", vector.Z);
                writer.WriteEndObject();
            });
        }

        internal static Vector DeserializeVector(string json)
        {
            using var doc = ParseDocument(json, nameof(json));
            var root = doc.RootElement;
            return ReadVector(root);
        }

        internal static string SerializeCoordinateSystem(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return WriteJson(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("$typeid", MatrixTypeId);
                WritePointProp(writer, "origin", cs.Origin);
                WriteVectorProp(writer, "xAxis", cs.XAxis);
                WriteVectorProp(writer, "yAxis", cs.YAxis);
                WriteVectorProp(writer, "zAxis", cs.ZAxis);
                writer.WriteStartArray("m");
                writer.WriteNumberValue(cs.XAxis.X);
                writer.WriteNumberValue(cs.YAxis.X);
                writer.WriteNumberValue(cs.ZAxis.X);
                writer.WriteNumberValue(cs.Origin.X);
                writer.WriteNumberValue(cs.XAxis.Y);
                writer.WriteNumberValue(cs.YAxis.Y);
                writer.WriteNumberValue(cs.ZAxis.Y);
                writer.WriteNumberValue(cs.Origin.Y);
                writer.WriteNumberValue(cs.XAxis.Z);
                writer.WriteNumberValue(cs.YAxis.Z);
                writer.WriteNumberValue(cs.ZAxis.Z);
                writer.WriteNumberValue(cs.Origin.Z);
                writer.WriteNumberValue(0.0);
                writer.WriteNumberValue(0.0);
                writer.WriteNumberValue(0.0);
                writer.WriteNumberValue(1.0);
                writer.WriteEndArray();
                writer.WriteEndObject();
            });
        }

        internal static CoordinateSystem DeserializeCoordinateSystem(string json)
        {
            using var doc = ParseDocument(json, nameof(json));
            var root = doc.RootElement;
            if (root.TryGetProperty("origin", out var originEl)
                && root.TryGetProperty("xAxis", out var xEl)
                && root.TryGetProperty("yAxis", out var yEl)
                && root.TryGetProperty("zAxis", out var zEl))
            {
                return CoordinateSystem.ByOriginVectors(
                    ReadPoint(originEl),
                    ReadVector(xEl),
                    ReadVector(yEl),
                    ReadVector(zEl));
            }

            if (root.TryGetProperty("m", out var mEl) && mEl.ValueKind == JsonValueKind.Array && mEl.GetArrayLength() == 16)
            {
                var v = mEl.EnumerateArray().Select(e => e.GetDouble()).ToArray();
                return CoordinateSystem.ByOriginVectors(
                    Point.ByCoordinates(v[3], v[7], v[11]),
                    Vector.ByCoordinates(v[0], v[4], v[8]),
                    Vector.ByCoordinates(v[1], v[5], v[9]),
                    Vector.ByCoordinates(v[2], v[6], v[10]));
            }

            throw new FormatException("Invalid CoordinateSystem JSON payload.");
        }

        internal static string SerializeBoundingBox(BoundingBox box)
        {
            ArgumentNullException.ThrowIfNull(box);
            return WriteJson(writer =>
            {
                writer.WriteStartObject();
                writer.WriteString("$typeid", BoundingBoxTypeId);
                WritePointProp(writer, "min", box.MinPoint);
                WritePointProp(writer, "max", box.MaxPoint);
                writer.WriteEndObject();
            });
        }

        internal static BoundingBox DeserializeBoundingBox(string json)
        {
            using var doc = ParseDocument(json, nameof(json));
            var root = doc.RootElement;
            var minEl = TryGetAnyProperty(root, "min", "minPoint", "MinPoint");
            var maxEl = TryGetAnyProperty(root, "max", "maxPoint", "MaxPoint");
            return BoundingBox.ByCorners(ReadPoint(minEl), ReadPoint(maxEl));
        }

        internal static string SerializeMesh(Mesh mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            return WriteJson(writer => WriteMeshObject(writer, mesh));
        }

        internal static Mesh DeserializeMesh(string json)
        {
            using var doc = ParseDocument(json, nameof(json));
            return ReadMeshObject(doc.RootElement);
        }

        internal static string SerializeGeometry(Geometry geometry)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            return WriteJson(writer => WriteGeometryObject(writer, geometry));
        }

        internal static Geometry DeserializeGeometry(string json)
        {
            using var doc = ParseDocument(json, nameof(json));
            return ReadGeometryObject(doc.RootElement);
        }

        private static void WriteGeometryObject(Utf8JsonWriter writer, Geometry geometry)
        {
            switch (geometry)
            {
                case Point pt:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", PointTypeId);
                    writer.WriteNumber("x", pt.X);
                    writer.WriteNumber("y", pt.Y);
                    writer.WriteNumber("z", pt.Z);
                    writer.WriteEndObject();
                    return;

                case Plane pl:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", PlaneTypeId);
                    WritePointProp(writer, "origin", pl.Origin);
                    WriteVectorProp(writer, "normal", pl.Normal);
                    WriteVectorProp(writer, "xAxis", pl.XAxis);
                    writer.WriteEndObject();
                    return;

                case Mesh mesh:
                    WriteMeshObject(writer, mesh);
                    return;

                case PanelSurface panelSurface:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", PanelSurfaceTypeId);
                    writer.WriteStartArray("vertices");
                    foreach (var v in panelSurface.VerticesArray)
                        WritePointArray(writer, v);
                    writer.WriteEndArray();
                    writer.WriteStartArray("panels");
                    foreach (var p in panelSurface.PanelsArray)
                    {
                        writer.WriteStartArray();
                        foreach (var idx in p)
                            writer.WriteNumberValue(idx);
                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    return;

                case Line line:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", LineTypeId);
                    WritePointProp(writer, "start", line.StartPoint);
                    WritePointProp(writer, "end", line.EndPoint);
                    writer.WriteEndObject();
                    return;

                case Circle circle:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", CircleTypeId);
                    WritePointProp(writer, "center", circle.CenterPoint);
                    WriteVectorProp(writer, "normal", circle.Normal);
                    WriteVectorProp(writer, "xAxis", circle.Plane.XAxis);
                    writer.WriteNumber("radius", circle.Radius);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(circle.Handle)));
                    writer.WriteEndObject();
                    return;

                case Arc arc:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", ArcTypeId);
                    WritePointProp(writer, "center", arc.CenterPoint);
                    WriteVectorProp(writer, "normal", arc.Normal);
                    writer.WriteNumber("radius", arc.Radius);
                    writer.WriteNumber("startAngle", arc.StartAngle);
                    writer.WriteNumber("sweepAngle", arc.SweepAngle);
                    WritePointProp(writer, "startPoint", arc.StartPoint);
                    WritePointProp(writer, "endPoint", arc.EndPoint);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(arc.Handle)));
                    writer.WriteEndObject();
                    return;

                case Ellipse ellipse:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", EllipseTypeId);
                    WritePointProp(writer, "center", ellipse.CenterPoint);
                    WriteVectorProp(writer, "majorAxis", ellipse.MajorAxis);
                    WriteVectorProp(writer, "minorAxis", ellipse.MinorAxis);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(ellipse.Handle)));
                    writer.WriteEndObject();
                    return;

                case EllipseArc ellipseArc:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", EllipseArcTypeId);
                    WritePointProp(writer, "center", ellipseArc.CenterPoint);
                    WriteVectorProp(writer, "normal", ellipseArc.Plane.Normal);
                    WriteVectorProp(writer, "xAxis", ellipseArc.Plane.XAxis);
                    WriteVectorProp(writer, "majorAxis", ellipseArc.MajorAxis);
                    WriteVectorProp(writer, "minorAxis", ellipseArc.MinorAxis);
                    writer.WriteNumber("xRadius", ellipseArc.XRadius);
                    writer.WriteNumber("yRadius", ellipseArc.YRadius);
                    writer.WriteNumber("startAngle", ellipseArc.StartAngle);
                    writer.WriteNumber("sweepAngle", ellipseArc.SweepAngle);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(ellipseArc.Handle)));
                    writer.WriteEndObject();
                    return;

                case Helix helix:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", HelixTypeId);
                    WritePointProp(writer, "axisPoint", helix.AxisPoint);
                    WriteVectorProp(writer, "axisDirection", helix.AxisDirection);
                    WritePointProp(writer, "startPoint", helix.StartPoint);
                    writer.WriteNumber("pitch", helix.Pitch);
                    writer.WriteNumber("radius", helix.Radius);
                    writer.WriteNumber("angle", helix.Angle);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(helix.Handle)));
                    writer.WriteEndObject();
                    return;

                case Rectangle rect:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", RectangleTypeId);
                    writer.WriteStartArray("corners");
                    foreach (var c in rect.Points)
                        WritePointArray(writer, c);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    return;

                case Polygon polygon:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", PolygonTypeId);
                    writer.WriteStartArray("points");
                    foreach (var p in polygon.Points)
                        WritePointArray(writer, p);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    return;

                case PolyCurve polyCurve:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", PolyCurveTypeId);
                    writer.WriteStartArray("curves");
                    foreach (var seg in polyCurve.Curves())
                        WriteGeometryObject(writer, seg);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    return;

                case NurbsCurve nurbsCurve:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", NurbsCurveTypeId);
                    writer.WriteNumber("degree", nurbsCurve.Degree);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(nurbsCurve.Handle)));
                    writer.WriteEndObject();
                    return;

                case Curve curve:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", CurveTypeId);
                    writer.WriteString("curveSmb", Convert.ToBase64String(NativeGeometry.SerializeCurve(curve.Handle)));
                    writer.WriteEndObject();
                    return;

                case Cuboid cuboid:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", CuboidTypeId);
                    writer.WriteNumber("width", cuboid.Width);
                    writer.WriteNumber("length", cuboid.Length);
                    writer.WriteNumber("height", cuboid.Height);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(cuboid.Handle)));
                    writer.WriteEndObject();
                    return;

                case Cylinder cylinder:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", CylinderTypeId);
                    WritePointProp(writer, "startPoint", cylinder.StartPoint);
                    WritePointProp(writer, "endPoint", cylinder.EndPoint);
                    writer.WriteNumber("radius", cylinder.Radius);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(cylinder.Handle)));
                    writer.WriteEndObject();
                    return;

                case Sphere sphere:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", SphereTypeId);
                    WritePointProp(writer, "centerPoint", sphere.CenterPoint);
                    writer.WriteNumber("radius", sphere.Radius);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(sphere.Handle)));
                    writer.WriteEndObject();
                    return;

                case Cone cone:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", ConeTypeId);
                    WritePointProp(writer, "startPoint", cone.StartPoint);
                    WritePointProp(writer, "endPoint", cone.EndPoint);
                    writer.WriteNumber("startRadius", cone.StartRadius);
                    writer.WriteNumber("endRadius", cone.EndRadius);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(cone.Handle)));
                    writer.WriteEndObject();
                    return;

                case Solid solid:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", SolidTypeId);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(solid.Handle)));
                    writer.WriteEndObject();
                    return;

                case NurbsSurface nurbsSurface:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", NurbsSurfaceTypeId);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(nurbsSurface.Handle)));
                    writer.WriteEndObject();
                    return;

                case PolySurface polySurface:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", PolySurfaceTypeId);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(polySurface.Handle)));
                    writer.WriteEndObject();
                    return;

                case Surface surface:
                    writer.WriteStartObject();
                    writer.WriteString("$typeid", SurfaceTypeId);
                    writer.WriteString("brepSmb", Convert.ToBase64String(NativeGeometry.SerializeBrep(surface.Handle)));
                    writer.WriteEndObject();
                    return;

                default:
                    throw new NotSupportedException($"JSON serialization is not supported for geometry type '{geometry.GetType().FullName}'.");
            }
        }

        private static Geometry ReadGeometryObject(JsonElement root)
        {
            if (!root.TryGetProperty("$typeid", out var typeIdEl) || typeIdEl.ValueKind != JsonValueKind.String)
                throw new FormatException("Geometry JSON payload is missing a string '$typeid' property.");

            var typeId = typeIdEl.GetString();
            switch (typeId)
            {
                case PointTypeId:
                    return ReadPoint(root);

                case PlaneTypeId:
                    return Plane.ByOriginNormalXAxis(
                        ReadPoint(root.GetProperty("origin")),
                        ReadVector(root.GetProperty("normal")),
                        ReadVector(root.GetProperty("xAxis")));

                case MeshTypeId:
                    return ReadMeshObject(root);

                case PanelSurfaceTypeId:
                {
                    var verts = root.GetProperty("vertices").EnumerateArray().Select(ReadPoint).ToArray();
                    var panels = root.GetProperty("panels")
                        .EnumerateArray()
                        .Select(arr => arr.EnumerateArray().Select(e => e.GetInt32()).ToArray())
                        .ToArray();
                    return new PanelSurface(verts, panels);
                }

                case LineTypeId:
                    return Line.ByStartPointEndPoint(
                        ReadPoint(root.GetProperty("start")),
                        ReadPoint(root.GetProperty("end")));

                case CircleTypeId:
                {
                    var center = ReadPoint(root.GetProperty("center"));
                    var normal = ReadVector(root.GetProperty("normal"));
                    var radius = root.GetProperty("radius").GetDouble();
                    var plane = root.TryGetProperty("xAxis", out var xAxisEl)
                        ? Plane.ByOriginNormalXAxis(center, normal, ReadVector(xAxisEl))
                        : Plane.ByOriginNormal(center, normal);
                    if (root.TryGetProperty("curveSmb", out var smbEl) && smbEl.ValueKind == JsonValueKind.String)
                    {
                        var handle = NativeGeometry.DeserializeCurve(Convert.FromBase64String(smbEl.GetString()));
                        return new Circle(handle, center, radius, plane);
                    }
                    return Circle.ByPlaneRadius(plane, radius);
                }

                case ArcTypeId:
                {
                    var center = ReadPoint(root.GetProperty("center"));
                    var normal = ReadVector(root.GetProperty("normal"));
                    var radius = root.GetProperty("radius").GetDouble();
                    var startAngle = root.GetProperty("startAngle").GetDouble();
                    var sweepAngle = root.GetProperty("sweepAngle").GetDouble();
                    if (root.TryGetProperty("curveSmb", out var smbEl) && smbEl.ValueKind == JsonValueKind.String)
                    {
                        var handle = NativeGeometry.DeserializeCurve(Convert.FromBase64String(smbEl.GetString()));
                        return new Arc(handle, center, radius, startAngle, sweepAngle, normal);
                    }
                    return Arc.ByCenterPointRadiusAngle(center, radius, startAngle, startAngle + sweepAngle, normal);
                }

                case EllipseTypeId:
                {
                    var center = ReadPoint(root.GetProperty("center"));
                    var major = ReadVector(root.GetProperty("majorAxis"));
                    var minor = ReadVector(root.GetProperty("minorAxis"));
                    if (root.TryGetProperty("curveSmb", out var smbEl) && smbEl.ValueKind == JsonValueKind.String)
                    {
                        var handle = NativeGeometry.DeserializeCurve(Convert.FromBase64String(smbEl.GetString()));
                        return new Ellipse(handle, center, major, minor);
                    }
                    return Ellipse.ByOriginVectors(center, major, minor);
                }

                case EllipseArcTypeId:
                {
                    var center = ReadPoint(root.GetProperty("center"));
                    var normal = ReadVector(root.GetProperty("normal"));
                    var xAxis = ReadVector(root.GetProperty("xAxis"));
                    var major = ReadVector(root.GetProperty("majorAxis"));
                    var minor = ReadVector(root.GetProperty("minorAxis"));
                    var startAngle = root.GetProperty("startAngle").GetDouble();
                    var sweepAngle = root.GetProperty("sweepAngle").GetDouble();
                    var xRadius = root.TryGetProperty("xRadius", out var xrEl) ? xrEl.GetDouble() : major.Length;
                    var yRadius = root.TryGetProperty("yRadius", out var yrEl) ? yrEl.GetDouble() : minor.Length;
                    var plane = Plane.ByOriginNormalXAxis(center, normal, xAxis);
                    if (root.TryGetProperty("curveSmb", out var smbEl) && smbEl.ValueKind == JsonValueKind.String)
                    {
                        var handle = NativeGeometry.DeserializeCurve(Convert.FromBase64String(smbEl.GetString()));
                        return new EllipseArc(handle, plane, major, minor, startAngle, sweepAngle, xRadius, yRadius);
                    }
                    return EllipseArc.ByPlaneRadiiAngles(plane, xRadius, yRadius, startAngle, sweepAngle);
                }

                case HelixTypeId:
                {
                    var axisPt = ReadPoint(root.GetProperty("axisPoint"));
                    var axisDir = ReadVector(root.GetProperty("axisDirection"));
                    var pitch = root.GetProperty("pitch").GetDouble();
                    var angle = root.GetProperty("angle").GetDouble();
                    if (root.TryGetProperty("curveSmb", out var smbEl) && smbEl.ValueKind == JsonValueKind.String)
                    {
                        var radius = root.GetProperty("radius").GetDouble();
                        var handle = NativeGeometry.DeserializeCurve(Convert.FromBase64String(smbEl.GetString()));
                        return new Helix(handle, axisPt, axisDir, pitch, radius, angle);
                    }
                    var startPt = ReadPoint(root.GetProperty("startPoint"));
                    return Helix.ByAxis(axisPt, axisDir, startPt, pitch, angle);
                }

                case RectangleTypeId:
                {
                    var corners = root.GetProperty("corners").EnumerateArray().Select(ReadPoint).ToArray();
                    if (corners.Length != 4)
                        throw new FormatException("Rectangle JSON requires 4 corner points.");
                    return Rectangle.ByCornerPoints(corners[0], corners[1], corners[2], corners[3]);
                }

                case PolygonTypeId:
                {
                    var pts = root.GetProperty("points").EnumerateArray().Select(ReadPoint).ToArray();
                    return Polygon.ByPoints(pts);
                }

                case PolyCurveTypeId:
                {
                    var segs = root.GetProperty("curves")
                        .EnumerateArray()
                        .Select(el => (Curve)ReadGeometryObject(el))
                        .ToArray();
                    return PolyCurve.ByJoinedCurves(segs);
                }

                case NurbsCurveTypeId:
                {
                    var degree = root.GetProperty("degree").GetInt32();
                    var handle = NativeGeometry.DeserializeCurve(
                        Convert.FromBase64String(root.GetProperty("curveSmb").GetString()));
                    return new NurbsCurve(handle, degree);
                }

                case CurveTypeId:
                {
                    var handle = NativeGeometry.DeserializeCurve(
                        Convert.FromBase64String(root.GetProperty("curveSmb").GetString()));
                    return new Curve(handle);
                }

                case CuboidTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new Cuboid(
                        handle,
                        root.GetProperty("width").GetDouble(),
                        root.GetProperty("length").GetDouble(),
                        root.GetProperty("height").GetDouble());
                }

                case CylinderTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new Cylinder(
                        handle,
                        ReadPoint(root.GetProperty("startPoint")),
                        ReadPoint(root.GetProperty("endPoint")),
                        root.GetProperty("radius").GetDouble());
                }

                case SphereTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new Sphere(
                        handle,
                        ReadPoint(root.GetProperty("centerPoint")),
                        root.GetProperty("radius").GetDouble());
                }

                case ConeTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new Cone(
                        handle,
                        ReadPoint(root.GetProperty("startPoint")),
                        ReadPoint(root.GetProperty("endPoint")),
                        root.GetProperty("startRadius").GetDouble(),
                        root.GetProperty("endRadius").GetDouble());
                }

                case SolidTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new Solid(handle);
                }

                case NurbsSurfaceTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new NurbsSurface(handle);
                }

                case PolySurfaceTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new PolySurface(handle);
                }

                case SurfaceTypeId:
                {
                    var handle = NativeGeometry.DeserializeBrep(
                        Convert.FromBase64String(root.GetProperty("brepSmb").GetString()));
                    return new Surface(handle);
                }

                case "dynamo.geometry:sab-1.0.0":
                    throw NativeGeometry.UnsupportedFormat("SAB deserialization");

                case "dynamo.geometry:tsm-1.0.0":
                    throw NativeGeometry.UnsupportedFormat("T-Spline deserialization");

                default:
                    throw new NotSupportedException($"Unsupported geometry JSON '$typeid': '{typeId}'.");
            }
        }

        private static void WriteMeshObject(Utf8JsonWriter writer, Mesh mesh)
        {
            writer.WriteStartObject();
            writer.WriteString("$typeid", MeshTypeId);
            writer.WriteStartArray("vertexPositions");
            foreach (var v in mesh.VertexPositions)
                WritePointArray(writer, v);
            writer.WriteEndArray();
            writer.WriteStartArray("faceIndices");
            foreach (var f in mesh.FaceIndices)
            {
                writer.WriteStartArray();
                writer.WriteNumberValue(f.A);
                writer.WriteNumberValue(f.B);
                writer.WriteNumberValue(f.C);
                if (f.Count == 4)
                    writer.WriteNumberValue(f.D);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private static Mesh ReadMeshObject(JsonElement root)
        {
            var verts = root.GetProperty("vertexPositions").EnumerateArray().Select(ReadPoint).ToArray();
            var faces = new List<IndexGroup>();
            foreach (var faceEl in root.GetProperty("faceIndices").EnumerateArray())
            {
                var idx = faceEl.EnumerateArray().Select(e => e.GetUInt32()).ToArray();
                if (idx.Length == 3)
                    faces.Add(IndexGroup.ByIndices(idx[0], idx[1], idx[2]));
                else if (idx.Length == 4)
                    faces.Add(IndexGroup.ByIndices(idx[0], idx[1], idx[2], idx[3]));
                else
                    throw new FormatException("Mesh JSON faceIndices must contain 3 or 4 indices.");
            }
            return Mesh.ByPointsFaceIndices(verts, faces);
        }

        private static string WriteJson(Action<Utf8JsonWriter> writeAction)
        {
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                writeAction(writer);
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static JsonDocument ParseDocument(string json, string paramName)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON string must not be null or empty.", paramName);
            return JsonDocument.Parse(json);
        }

        private static void WritePointProp(Utf8JsonWriter writer, string name, Point pt)
        {
            writer.WriteStartObject(name);
            writer.WriteNumber("x", pt.X);
            writer.WriteNumber("y", pt.Y);
            writer.WriteNumber("z", pt.Z);
            writer.WriteEndObject();
        }

        private static void WritePointArray(Utf8JsonWriter writer, Point pt)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(pt.X);
            writer.WriteNumberValue(pt.Y);
            writer.WriteNumberValue(pt.Z);
            writer.WriteEndArray();
        }

        private static void WriteVectorProp(Utf8JsonWriter writer, string name, Vector v)
        {
            writer.WriteStartObject(name);
            writer.WriteNumber("x", v.X);
            writer.WriteNumber("y", v.Y);
            writer.WriteNumber("z", v.Z);
            writer.WriteEndObject();
        }

        private static Point ReadPoint(JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Array)
            {
                var coords = el.EnumerateArray().Select(e => e.GetDouble()).ToArray();
                if (coords.Length != 3)
                    throw new FormatException("Point array must have 3 coordinates.");
                return Point.ByCoordinates(coords[0], coords[1], coords[2]);
            }
            return Point.ByCoordinates(
                GetCoord(el, "x", "X"),
                GetCoord(el, "y", "Y"),
                GetCoord(el, "z", "Z"));
        }

        private static Vector ReadVector(JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Array)
            {
                var coords = el.EnumerateArray().Select(e => e.GetDouble()).ToArray();
                if (coords.Length != 3)
                    throw new FormatException("Vector array must have 3 components.");
                return Vector.ByCoordinates(coords[0], coords[1], coords[2]);
            }
            return Vector.ByCoordinates(
                GetCoord(el, "x", "X"),
                GetCoord(el, "y", "Y"),
                GetCoord(el, "z", "Z"));
        }

        private static double GetCoord(JsonElement obj, string lower, string upper)
        {
            if (obj.TryGetProperty(lower, out var v))
                return v.GetDouble();
            if (obj.TryGetProperty(upper, out v))
                return v.GetDouble();
            throw new FormatException($"Missing coordinate property '{lower}'.");
        }

        private static JsonElement TryGetAnyProperty(JsonElement obj, params string[] names)
        {
            foreach (var n in names)
            {
                if (obj.TryGetProperty(n, out var el))
                    return el;
            }
            throw new FormatException($"Missing required JSON property ({string.Join(" / ", names)}).");
        }
    }
}
