using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

public class ScreenRecorder
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect); 
    
    public String SessionID { get; private set; }
    public String Path { get; private set; }
    private int counter;

    public ScreenRecorder(String path)
    {
        SessionID = DateTime.Now.ToString("yyyyMMdd-HH24mmss");
        Path = path;
        counter = 0;

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
    }


    private
        string BuildFileName 
        ()
        {
            string outTarget = System.IO.Path.Combine(Path, SessionID + "-" + counter.ToString() + ".png");
            return outTarget;
        }

        /// <summary>
        /// Capture the AutoCAD window now, returning the path of the file saved
        /// </summary>
        /// <returns></returns>
    public
        String CaptureNow 
        ()
        {
            if (InitGeometryReflectionInfo())
                update_display.Invoke(null, new object[]{});
        
            IntPtr main_window = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            RECT rect;
            GetWindowRect(main_window, out rect);

            int width = (int)(rect.Right - rect.Left);
            int height = (int)(rect.Bottom - rect.Top);

            string filename = BuildFileName();
            using (Bitmap image = new Bitmap(width, height))
            {

                Graphics g = Graphics.FromImage(image);
                g.CopyFromScreen((int)(rect.Left), (int)(rect.Top), 0, 0, new System.Drawing.Size(width, height));

                image.Save(filename);
            }

            counter++;

            return filename;

        }

    public
        String CaptureOnChange
        (Object dependent)
    {
        return CaptureNow();
    }

    private Type geometry_type = null;
    private MethodInfo update_display = null;
    private bool InitGeometryReflectionInfo()
    {
        if (null != update_display)
            return true;

        Assembly protocore_assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("ProtoCore")).First();
        if (null == protocore_assembly)
            return false;
        
        Uri proto_uri = new Uri(System.IO.Path.GetDirectoryName(protocore_assembly.GetName().CodeBase));
        string proto_path = proto_uri.AbsolutePath;
        string protogeometry_path = System.IO.Path.Combine(proto_path, "ProtoGeometry.dll");

        if (!System.IO.File.Exists(protogeometry_path))
            return false;

        Assembly protogeometry_assembly = Assembly.LoadFrom(protogeometry_path);
        geometry_type = protogeometry_assembly.GetType(@"Autodesk.DesignScript.Geometry.Geometry");
        update_display = geometry_type.GetMethod("UpdateDisplay", BindingFlags.Public | BindingFlags.Static);

        return null != update_display;
    }
}



