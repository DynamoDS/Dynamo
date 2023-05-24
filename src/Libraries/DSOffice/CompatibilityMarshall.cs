#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System;

//Can't use Marshall on NET6 so creating this class in order to keep functionality on Excel.cs file
public static class CompatibilityMarshall
{
    internal const String OLEAUT32 = "oleaut32.dll";
    internal const String OLE32 = "ole32.dll";

    [System.Security.SecurityCritical]  // auto-generated_required
    public static Object GetActiveObject(String progID)
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
    [SecurityCritical]
    private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

    [DllImport(OLE32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

    [DllImport(OLEAUT32, PreserveSig = false)]
    [ResourceExposure(ResourceScope.None)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);

}
#endif
