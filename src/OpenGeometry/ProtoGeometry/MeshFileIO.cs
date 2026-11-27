using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace OpenGeometry
{
    internal static class MeshFileIO
    {
        internal static Mesh[] ImportFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Mesh file not found: {filePath}", filePath);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".obj" => ReadObj(filePath),
                ".stl" => new[] { ReadStl(filePath) },
                ".ply" => new[] { ReadPly(filePath) },
                _ => throw new NotSupportedException(
                    $"Mesh.ImportFile does not support '{ext}'. Supported open formats: .obj, .stl, .ply.")
            };
        }

        internal static string ExportMeshes(string filePath, IEnumerable<Mesh> meshes)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));
            ArgumentNullException.ThrowIfNull(meshes);
            var list = meshes.Where(m => m != null).ToArray();
            if (list.Length == 0)
                throw new ArgumentException("Mesh.ExportMeshes requires at least one mesh.", nameof(meshes));

            var fullPath = Path.GetFullPath(filePath);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            switch (ext)
            {
                case ".obj":
                    WriteObj(fullPath, list);
                    return filePath;
                case ".stl":
                    WriteStlBinary(fullPath, list);
                    return filePath;
                case ".ply":
                    WritePlyAscii(fullPath, list);
                    return filePath;
                default:
                    throw new NotSupportedException(
                        $"Mesh.ExportMeshes does not support '{ext}'. Supported open formats: .obj, .stl, .ply.");
            }
        }

        private static Mesh[] ReadObj(string path)
        {
            var allPositions = new List<Point>();
            var groups = new List<List<int[]>>();
            var currentFaces = new List<int[]>();

            foreach (var rawLine in File.ReadLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
                    continue;

                if (line.StartsWith("v ", StringComparison.Ordinal) || line.StartsWith("v\t", StringComparison.Ordinal))
                {
                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        var x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        var y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        var z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                        allPositions.Add(Point.ByCoordinates(x, y, z));
                    }
                }
                else if (line.StartsWith("o ", StringComparison.Ordinal) || line.StartsWith("g ", StringComparison.Ordinal))
                {
                    if (currentFaces.Count > 0)
                    {
                        groups.Add(currentFaces);
                        currentFaces = new List<int[]>();
                    }
                }
                else if (line.StartsWith("f ", StringComparison.Ordinal) || line.StartsWith("f\t", StringComparison.Ordinal))
                {
                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        var indices = new int[parts.Length - 1];
                        for (var i = 1; i < parts.Length; i++)
                        {
                            var token = parts[i];
                            var slash = token.IndexOf('/');
                            var idxText = slash >= 0 ? token.Substring(0, slash) : token;
                            var rawIdx = int.Parse(idxText, CultureInfo.InvariantCulture);
                            indices[i - 1] = rawIdx < 0 ? allPositions.Count + rawIdx : rawIdx - 1;
                        }
                        currentFaces.Add(indices);
                    }
                }
            }

            if (currentFaces.Count > 0)
                groups.Add(currentFaces);

            if (groups.Count == 0)
                throw new InvalidDataException($"OBJ file contains no faces: {path}");

            var result = new List<Mesh>(groups.Count);
            foreach (var faceList in groups)
            {
                var remap = new Dictionary<int, uint>();
                var localVerts = new List<Point>();
                var localFaces = new List<IndexGroup>();

                uint MapVertex(int globalIndex)
                {
                    if (globalIndex < 0 || globalIndex >= allPositions.Count)
                        throw new InvalidDataException($"OBJ vertex index out of range: {globalIndex + 1}");
                    if (remap.TryGetValue(globalIndex, out var local))
                        return local;
                    local = (uint)localVerts.Count;
                    remap[globalIndex] = local;
                    localVerts.Add(allPositions[globalIndex]);
                    return local;
                }

                foreach (var face in faceList)
                {
                    if (face.Length == 3)
                    {
                        localFaces.Add(IndexGroup.ByIndices(
                            MapVertex(face[0]), MapVertex(face[1]), MapVertex(face[2])));
                    }
                    else if (face.Length == 4)
                    {
                        localFaces.Add(IndexGroup.ByIndices(
                            MapVertex(face[0]), MapVertex(face[1]), MapVertex(face[2]), MapVertex(face[3])));
                    }
                    else if (face.Length > 4)
                    {
                        var v0 = MapVertex(face[0]);
                        for (var i = 1; i + 1 < face.Length; i++)
                            localFaces.Add(IndexGroup.ByIndices(v0, MapVertex(face[i]), MapVertex(face[i + 1])));
                    }
                }

                if (localFaces.Count > 0)
                    result.Add(Mesh.ByPointsFaceIndices(localVerts, localFaces));
            }
            return result.ToArray();
        }

        private static void WriteObj(string path, Mesh[] meshes)
        {
            using var writer = new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.WriteLine("# Exported by OpenGeometry for Dynamo");
            var vertexBase = 1;
            for (var m = 0; m < meshes.Length; m++)
            {
                var mesh = meshes[m];
                writer.WriteLine($"o mesh_{m}");
                var verts = mesh.VertexPositions;
                foreach (var v in verts)
                {
                    writer.WriteLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "v {0:G17} {1:G17} {2:G17}",
                        v.X, v.Y, v.Z));
                }
                foreach (var f in mesh.FaceIndices)
                {
                    if (f.Count == 4)
                    {
                        writer.WriteLine(string.Format(
                            CultureInfo.InvariantCulture,
                            "f {0} {1} {2} {3}",
                            vertexBase + f.A, vertexBase + f.B, vertexBase + f.C, vertexBase + f.D));
                    }
                    else
                    {
                        writer.WriteLine(string.Format(
                            CultureInfo.InvariantCulture,
                            "f {0} {1} {2}",
                            vertexBase + f.A, vertexBase + f.B, vertexBase + f.C));
                    }
                }
                vertexBase += verts.Length;
            }
        }

        private static Mesh ReadStl(string path)
        {
            var bytes = File.ReadAllBytes(path);
            if (IsBinaryStl(bytes))
                return ReadStlBinary(bytes);
            return ReadStlAscii(path);
        }

        private static bool IsBinaryStl(byte[] bytes)
        {
            if (bytes.Length < 84)
                return false;
            var triCount = BitConverter.ToUInt32(bytes, 80);
            var expectedSize = 84L + (long)triCount * 50L;
            if (expectedSize == bytes.Length)
                return true;
            // Some files start with "solid" even if binary; if size doesn't match binary layout, try ASCII.
            return false;
        }

        private static Mesh ReadStlBinary(byte[] bytes)
        {
            var triCount = BitConverter.ToUInt32(bytes, 80);
            if (triCount == 0)
                throw new InvalidDataException("STL file contains 0 triangles.");

            var uniqueMap = new Dictionary<(long, long, long), uint>();
            var pts = new List<Point>();
            var faces = new List<IndexGroup>((int)triCount);

            uint GetOrAdd(double x, double y, double z)
            {
                var key = ((long)Math.Round(x * 1e6), (long)Math.Round(y * 1e6), (long)Math.Round(z * 1e6));
                if (uniqueMap.TryGetValue(key, out var idx))
                    return idx;
                idx = (uint)pts.Count;
                uniqueMap[key] = idx;
                pts.Add(Point.ByCoordinates(x, y, z));
                return idx;
            }

            var offset = 84;
            for (uint t = 0; t < triCount; t++)
            {
                offset += 12; // skip facet normal
                var a = GetOrAdd(
                    BitConverter.ToSingle(bytes, offset),
                    BitConverter.ToSingle(bytes, offset + 4),
                    BitConverter.ToSingle(bytes, offset + 8));
                offset += 12;
                var b = GetOrAdd(
                    BitConverter.ToSingle(bytes, offset),
                    BitConverter.ToSingle(bytes, offset + 4),
                    BitConverter.ToSingle(bytes, offset + 8));
                offset += 12;
                var c = GetOrAdd(
                    BitConverter.ToSingle(bytes, offset),
                    BitConverter.ToSingle(bytes, offset + 4),
                    BitConverter.ToSingle(bytes, offset + 8));
                offset += 14; // 12 bytes + 2-byte attribute count
                if (a != b && b != c && a != c)
                    faces.Add(IndexGroup.ByIndices(a, b, c));
            }
            return Mesh.ByPointsFaceIndices(pts, faces);
        }

        private static Mesh ReadStlAscii(string path)
        {
            var uniqueMap = new Dictionary<(long, long, long), uint>();
            var pts = new List<Point>();
            var faces = new List<IndexGroup>();
            var tri = new List<uint>(3);

            uint GetOrAdd(double x, double y, double z)
            {
                var key = ((long)Math.Round(x * 1e6), (long)Math.Round(y * 1e6), (long)Math.Round(z * 1e6));
                if (uniqueMap.TryGetValue(key, out var idx))
                    return idx;
                idx = (uint)pts.Count;
                uniqueMap[key] = idx;
                pts.Add(Point.ByCoordinates(x, y, z));
                return idx;
            }

            foreach (var rawLine in File.ReadLines(path))
            {
                var line = rawLine.Trim();
                if (!line.StartsWith("vertex ", StringComparison.OrdinalIgnoreCase))
                    continue;
                var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                    continue;
                var x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                var y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                var z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                tri.Add(GetOrAdd(x, y, z));
                if (tri.Count == 3)
                {
                    if (tri[0] != tri[1] && tri[1] != tri[2] && tri[0] != tri[2])
                        faces.Add(IndexGroup.ByIndices(tri[0], tri[1], tri[2]));
                    tri.Clear();
                }
            }

            if (faces.Count == 0)
                throw new InvalidDataException($"ASCII STL file contains no triangles: {path}");
            return Mesh.ByPointsFaceIndices(pts, faces);
        }

        private static void WriteStlBinary(string path, Mesh[] meshes)
        {
            var allTris = new List<(Point A, Point B, Point C)>();
            foreach (var mesh in meshes)
            {
                var verts = mesh.VertexPositions;
                foreach (var f in mesh.FaceIndices)
                {
                    allTris.Add((verts[f.A], verts[f.B], verts[f.C]));
                    if (f.Count == 4)
                        allTris.Add((verts[f.A], verts[f.C], verts[f.D]));
                }
            }

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream, Encoding.ASCII);
            var header = new byte[80];
            Encoding.ASCII.GetBytes("OpenGeometry for Dynamo Binary STL").CopyTo(header, 0);
            writer.Write(header);
            writer.Write((uint)allTris.Count);

            foreach (var (a, b, c) in allTris)
            {
                var ab = Vector.ByTwoPoints(a, b);
                var ac = Vector.ByTwoPoints(a, c);
                var n = ab.Cross(ac);
                var unit = n.Length > 1e-12 ? n.Normalized() : Vector.ZAxis();
                writer.Write((float)unit.X);
                writer.Write((float)unit.Y);
                writer.Write((float)unit.Z);
                writer.Write((float)a.X); writer.Write((float)a.Y); writer.Write((float)a.Z);
                writer.Write((float)b.X); writer.Write((float)b.Y); writer.Write((float)b.Z);
                writer.Write((float)c.X); writer.Write((float)c.Y); writer.Write((float)c.Z);
                writer.Write((ushort)0);
            }
        }

        private static Mesh ReadPly(string path)
        {
            using var reader = new StreamReader(path);
            var first = reader.ReadLine()?.Trim();
            if (!string.Equals(first, "ply", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Invalid PLY header in {path}");

            var vertexCount = 0;
            var faceCount = 0;
            var isAscii = false;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("format ascii", StringComparison.OrdinalIgnoreCase))
                    isAscii = true;
                else if (trimmed.StartsWith("element vertex ", StringComparison.OrdinalIgnoreCase))
                    vertexCount = int.Parse(trimmed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[2], CultureInfo.InvariantCulture);
                else if (trimmed.StartsWith("element face ", StringComparison.OrdinalIgnoreCase))
                    faceCount = int.Parse(trimmed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[2], CultureInfo.InvariantCulture);
                else if (string.Equals(trimmed, "end_header", StringComparison.OrdinalIgnoreCase))
                    break;
            }

            if (!isAscii)
                throw new NotSupportedException("Only ASCII PLY files are currently supported by Mesh.ImportFile.");
            if (vertexCount < 3 || faceCount < 1)
                throw new InvalidDataException($"PLY file contains insufficient geometry: {path}");

            var verts = new Point[vertexCount];
            for (var i = 0; i < vertexCount; i++)
            {
                var parts = (reader.ReadLine() ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                verts[i] = Point.ByCoordinates(
                    double.Parse(parts[0], CultureInfo.InvariantCulture),
                    double.Parse(parts[1], CultureInfo.InvariantCulture),
                    double.Parse(parts[2], CultureInfo.InvariantCulture));
            }

            var faces = new List<IndexGroup>(faceCount);
            for (var i = 0; i < faceCount; i++)
            {
                var parts = (reader.ReadLine() ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4)
                    continue;
                var n = int.Parse(parts[0], CultureInfo.InvariantCulture);
                if (n == 3 && parts.Length >= 4)
                {
                    faces.Add(IndexGroup.ByIndices(
                        uint.Parse(parts[1], CultureInfo.InvariantCulture),
                        uint.Parse(parts[2], CultureInfo.InvariantCulture),
                        uint.Parse(parts[3], CultureInfo.InvariantCulture)));
                }
                else if (n == 4 && parts.Length >= 5)
                {
                    faces.Add(IndexGroup.ByIndices(
                        uint.Parse(parts[1], CultureInfo.InvariantCulture),
                        uint.Parse(parts[2], CultureInfo.InvariantCulture),
                        uint.Parse(parts[3], CultureInfo.InvariantCulture),
                        uint.Parse(parts[4], CultureInfo.InvariantCulture)));
                }
                else if (n > 4 && parts.Length >= n + 1)
                {
                    var v0 = uint.Parse(parts[1], CultureInfo.InvariantCulture);
                    for (var k = 2; k < n; k++)
                    {
                        faces.Add(IndexGroup.ByIndices(
                            v0,
                            uint.Parse(parts[k], CultureInfo.InvariantCulture),
                            uint.Parse(parts[k + 1], CultureInfo.InvariantCulture)));
                    }
                }
            }
            return Mesh.ByPointsFaceIndices(verts, faces);
        }

        private static void WritePlyAscii(string path, Mesh[] meshes)
        {
            var allVerts = new List<Point>();
            var allFaces = new List<IndexGroup>();
            foreach (var mesh in meshes)
            {
                var offset = (uint)allVerts.Count;
                allVerts.AddRange(mesh.VertexPositions);
                foreach (var f in mesh.FaceIndices)
                {
                    allFaces.Add(f.Count == 4
                        ? IndexGroup.ByIndices(offset + f.A, offset + f.B, offset + f.C, offset + f.D)
                        : IndexGroup.ByIndices(offset + f.A, offset + f.B, offset + f.C));
                }
            }

            using var writer = new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.WriteLine("ply");
            writer.WriteLine("format ascii 1.0");
            writer.WriteLine($"element vertex {allVerts.Count}");
            writer.WriteLine("property float x");
            writer.WriteLine("property float y");
            writer.WriteLine("property float z");
            writer.WriteLine($"element face {allFaces.Count}");
            writer.WriteLine("property list uchar int vertex_indices");
            writer.WriteLine("end_header");

            foreach (var v in allVerts)
            {
                writer.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:G9} {1:G9} {2:G9}",
                    v.X, v.Y, v.Z));
            }
            foreach (var f in allFaces)
            {
                if (f.Count == 4)
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "4 {0} {1} {2} {3}", f.A, f.B, f.C, f.D));
                else
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "3 {0} {1} {2}", f.A, f.B, f.C));
            }
        }
    }
}
