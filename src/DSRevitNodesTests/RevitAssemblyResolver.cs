using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DSRevitNodesTests
{
    static class RevitAssemblyResolver
    {
        internal static string GetRevitDirectory()
        {
            var dirs = new[]
            {
                Path.GetDirectoryName(@"C:\Program Files\Autodesk\Revit Architecture 2014"),
                Path.GetDirectoryName(@"C:\Program Files\Autodesk\Vasari Beta 3"),
                Path.GetDirectoryName(@"C:\Program Files\Autodesk\Revit Architecture 2013\Program"),
            };

            return dirs.FirstOrDefault(Directory.Exists);
        }

        public static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            var revitDir = GetRevitDirectory();
            if (String.IsNullOrEmpty(revitDir)) return null;

            if (args.Name.Contains("RevitAPIUI"))
            {
                var strTempAssmbPath = Path.Combine(revitDir, @"RevitAPIUI.dll");
                return Assembly.LoadFrom(strTempAssmbPath);
            }
            
            if (args.Name.Contains("RevitAPI"))
            {
                var strTempAssmbPath = Path.Combine(revitDir, @"RevitAPI.dll");
                return Assembly.LoadFrom(strTempAssmbPath);
            }

            return null;
        }
    }
}
