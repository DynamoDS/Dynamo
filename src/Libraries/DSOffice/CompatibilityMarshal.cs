#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System;

/// <summary>
/// Can't use Marshal on NET6 so this class replaces that functionality in order to keep the Excel.cs file (Excel nodes) working
/// </summary>
internal static class CompatibilityMarshal
{
    private const String OLEAUT32 = "oleaut32.dll";
    private const String OLE32 = "ole32.dll";

    /// <summary>
    /// Works like Marshal.GetActiveObject on Net6
    /// </summary>
    [SecurityCritical]  // auto-generated_required
    internal static Object GetActiveObject(String progID)
    {
        Object obj = null;
        Guid clsid;

        // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
        // CLSIDFromProgIDEx doesn't exist.
        try
        {
            CLSIDFromProgIDEx(progID, out clsid);
        }
        catch (Exception)
        {
            CLSIDFromProgID(progID, out clsid);
        }

        GetActiveObject(ref clsid, IntPtr.Zero, out obj);
        return obj;
    }

    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]  // auto-generated_required
    private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]  // auto-generated_required
    private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

    [DllImport(OLEAUT32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]  // auto-generated_required
    private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);

}
#endif
