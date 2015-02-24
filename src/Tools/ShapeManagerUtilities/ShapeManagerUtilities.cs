using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ShapeManagerUtilities
{
    public static class ShapeManagerUtilities
    {
        public static readonly string GeometryFactoryAssembly = "LibG.ProtoInterface.dll";
        public static readonly string PreloaderAssembly = "LibG.AsmPreloader.Managed.dll";
        public static readonly string PreloaderClassName = "Autodesk.LibG.AsmPreloader";
        public static readonly string PreloaderMethodName = "PreloadAsmLibraries";

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

                int versionIndex = -1;
                foreach (var version in versions)
                {
                    versionIndex++;
                    var assemblyName = string.Format("ASMAHL{0}A.DLL", version);

                    foreach (var directoryInfo in directories)
                    {
                        if (directoryInfo.GetFiles(assemblyName).Length <= 0)
                            continue;

                        location = directoryInfo.FullName;
                        return versionIndex;
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
        /// PreloaderAssembly assembly. This argument must represent a valid path 
        /// to the loader.</param>
        /// <param name="asmLocation">Full path of the folder that contains ASM 
        /// binaries to load. This argument cannot be null or empty.</param>
        /// 
        public static void PreloadAsmFromPath(string preloaderLocation, string asmLocation)
        {
            if (string.IsNullOrEmpty(preloaderLocation) || !Directory.Exists(preloaderLocation))
                throw new ArgumentException("preloadedPath");
            if (string.IsNullOrEmpty(asmLocation) || !Directory.Exists(asmLocation))
                throw new ArgumentException("asmLocation");

            var preloaderPath = Path.Combine(preloaderLocation, PreloaderAssembly);

            Debug.WriteLine("ASM Preloader: {0}", preloaderPath);
            Debug.WriteLine("ASM Location: {0}", asmLocation);

            var libG = Assembly.LoadFrom(preloaderPath);
            var preloadType = libG.GetType(PreloaderClassName);

            var preloadMethod = preloadType.GetMethod(PreloaderMethodName,
                BindingFlags.Public | BindingFlags.Static);

            if (preloadMethod == null)
            {
                throw new MissingMethodException(
                    string.Format("Method '{0}' not found", PreloaderMethodName));
            }

            var methodParams = new object[] { asmLocation };
            preloadMethod.Invoke(null, methodParams);

            Debug.WriteLine("Successfully loaded ASM binaries");
        }

        /// <summary>
        /// Call this method to resolve full path to GeometryFactoryAssembly 
        /// assembly, given the root folder and the version. This method throws 
        /// an exception if either of the folders/assembly cannot be found.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_xxx folder, where 'xxx' represents the library version. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.
        /// </param>
        /// <param name="version">Version number of the targeted geometry library.
        /// This argument must be the three-digit value (e.g. 219) that gets 
        /// appended to form a valid 'LibG_xxx' folder name. If the resulting 
        /// folder does not exist, this method throws an FileNotFoundException.
        /// </param>
        /// <returns>The full path to GeometryFactoryAssembly assembly.</returns>
        /// 
        public static string GetGeometryFactoryPath(string rootFolder, int version)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");

            if (!Directory.Exists(rootFolder))
            {
                // Root directory must be specified and valid.
                throw new DirectoryNotFoundException(string.Format(
                    "Directory not found: {0}", rootFolder));
            }

            var libGFolderName = string.Format("libg_{0}", version);
            var libGFolder = Path.Combine(rootFolder, libGFolderName);
            if (!Directory.Exists(libGFolder))
            {
                // LibG_xxx folder must be valid.
                throw new DirectoryNotFoundException(string.Format(
                    "Directory not found: {0}", libGFolder));
            }

            var assemblyPath = Path.Combine(libGFolder, GeometryFactoryAssembly);
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException(string.Format(
                    "File not found: {0}", assemblyPath));
            }

            return assemblyPath;
        }
    }
}
