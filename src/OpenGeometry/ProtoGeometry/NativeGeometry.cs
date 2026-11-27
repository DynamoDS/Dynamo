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
        private const uint SupportedAbi = 1;
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

        internal static SolidHandle Difference(SolidHandle solid, SolidHandle other)
        {
            var status = Subtract(solid, other, out var result);
            return CheckHandle(status, result, "Difference");
        }

        internal static double Volume(SolidHandle solid)
        {
            Check(GetVolume(solid, out var volume), "Volume");
            return volume;
        }

        internal static NativeVertex[] Tessellate(SolidHandle solid, double tolerance)
        {
            var status = CreateMesh(solid, tolerance, out var mesh);
            using var snapshot = CheckHandle(status, mesh, "Tessellation");
            var vertices = new NativeVertex[checked((int)GetVertexCount(snapshot))];
            Check(CopyVertices(snapshot, vertices, (uint)vertices.Length), "Tessellation");
            return vertices;
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

        [DllImport(Bridge, EntryPoint = "og_abi_version", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint GetAbiVersion();

        [DllImport(Bridge, EntryPoint = "og_create_box", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateBox(in NativePoint corner, double sizeX, double sizeY, double sizeZ, out SolidHandle solid);

        [DllImport(Bridge, EntryPoint = "og_create_cylinder", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateCylinder(in NativePoint start, in NativePoint end, double radius, out SolidHandle solid);

        [DllImport(Bridge, EntryPoint = "og_difference", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Subtract(SolidHandle solid, SolidHandle other, out SolidHandle result);

        [DllImport(Bridge, EntryPoint = "og_volume", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetVolume(SolidHandle solid, out double volume);

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
    }
}
