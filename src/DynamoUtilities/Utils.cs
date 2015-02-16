using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DynamoUtilities
{
    public class Utils
    {
        /// <summary>
        /// Call this method to determine the version of ASM that is installed 
        /// on the user machine. The method scans through a list of known Autodesk 
        /// product folders for ASM binaries with the targeted version.
        /// </summary>
        /// <param name="versions">A list of version numbers to check for in order 
        /// of preference. For example, { 221, 220, 219 }. This argument cannot be 
        /// null or empty.</param>
        /// <param name="location">The full path of the directory in which targeted
        /// ASM binaries are found. This argument cannot be null.</param>
        /// <returns>Returns a zero based index into the versions list if any 
        /// installed ASM is found, or -1 otherwise.</returns>
        /// 
        public static int GetInstalledAsmVersion(IEnumerable<int> versions, ref string location)
        {
            if ((versions == null) || !versions.Any())
                throw new ArgumentNullException("versions");
            if (location == null)
                throw new ArgumentNullException("location");

            location = string.Empty;

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var baseSearchDirectory = Path.Combine(programFiles, "Autodesk");

            try
            {
                var root = new DirectoryInfo(baseSearchDirectory);
                var subDirs = root.GetDirectories();
                var directories = subDirs.Where(d => d.Name.Contains("Revit")
                    || d.Name.Contains("Vasari")).ToList();

                if (!directories.Any())
                    return -1;

                foreach (var version in versions)
                {
                    var assemblyName = string.Format("ASMAHL{0}A.DLL", version);

                    foreach (var directoryInfo in directories)
                    {
                        if (directoryInfo.GetFiles(assemblyName).Length <= 0)
                            continue;

                        location = directoryInfo.FullName;
                        return version;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return -1;
        }

        /// <summary>
        /// Call this method to preload ASM binaries from a specific location. This 
        /// method does not have a return value, any failures in loading ASM binaries
        /// will result in an exception being thrown.
        /// </summary>
        /// <param name="preloaderLocation">Full path of the folder that contains  
        /// LibG.AsmPreloader.Managed.dll assembly. This argument must represent 
        /// a valid path to the loader.</param>
        /// <param name="asmLocation">Full path of the folder that contains ASM 
        /// binaries to load. This argument cannot be null or empty.</param>
        /// 
        public static void PreloadAsmFromPath(string preloaderLocation, string asmLocation)
        {
            if (string.IsNullOrEmpty(preloaderLocation) || !Directory.Exists(preloaderLocation))
                throw new ArgumentException("preloadedPath");
            if (string.IsNullOrEmpty(asmLocation) || !Directory.Exists(asmLocation))
                throw new ArgumentException("asmLocation");

            const string preloaderName = "LibG.AsmPreloader.Managed.dll";
            var preloaderPath = Path.Combine(preloaderLocation, preloaderName);

            Debug.WriteLine("ASM Preloader: {0}", preloaderPath);
            Debug.WriteLine("ASM Location: {0}", asmLocation);

            var libG = Assembly.LoadFrom(preloaderPath);
            var preloadType = libG.GetType("Autodesk.LibG.AsmPreloader");

            var preloadMethod = preloadType.GetMethod("PreloadAsmLibraries",
                BindingFlags.Public | BindingFlags.Static);

            if (preloadMethod == null)
                throw new MissingMethodException(@"Method ""PreloadAsmLibraries"" not found");

            var methodParams = new object[] { asmLocation };
            preloadMethod.Invoke(null, methodParams);

            Debug.WriteLine("Successfully loaded ASM binaries");
        }
    }
}
