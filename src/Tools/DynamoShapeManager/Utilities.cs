using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;


namespace DynamoShapeManager
{
    public static class Utilities
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
        /// of preference. This argument cannot be null or empty.</param>
        /// <param name="location">The full path of the directory in which targeted
        /// ASM binaries are found. This argument cannot be null.</param>
        /// <returns>Returns LibraryVersion of ASM if any installed ASM is found, 
        /// or None otherwise.</returns>
        /// 
        public static LibraryVersion GetInstalledAsmVersion(List<LibraryVersion> versions, ref string location)
        {
            if ((versions == null) || versions.Count <= 0)
                throw new ArgumentNullException("versions");
            if (location == null)
                throw new ArgumentNullException("location");

            location = string.Empty;

            try
            {
                var installations = GetAsmInstallations();

                foreach (KeyValuePair<string, Tuple<int,int,int,int>> install in installations)
                {
                    if (versions.Exists(v => (int)v == install.Value.Item1))
                    {
                        location = install.Key;
                        return (LibraryVersion)install.Value.Item1;
                    }
                }
            }
            catch (Exception)
            {
                return LibraryVersion.None;
            }

            return LibraryVersion.None;
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
        /// core modules. This must represent a valid directory.</param>
        /// <param name="version">Version number of the targeted geometry library.
        /// If the resulting folder does not exist, this method throws an 
        /// FileNotFoundException.</param>
        /// <returns>The full path to GeometryFactoryAssembly assembly.</returns>
        /// 
        public static string GetGeometryFactoryPath(string rootFolder, LibraryVersion version)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");

            if (!Directory.Exists(rootFolder))
            {
                // Root directory must be specified and valid.
                throw new DirectoryNotFoundException(string.Format(
                    "Directory not found: {0}", rootFolder));
            }

            var libGFolderName = string.Format("libg_{0}", ((int) version));
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

        private static IEnumerable GetAsmInstallations()
        {
            string installDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assembly = Assembly.LoadFrom(Path.Combine(installDir, "DynamoInstallDetective.dll"));
            var type = assembly.GetType("DynamoInstallDetective.Utilities");

            var installationsMethod = type.GetMethod(
                "FindProductInstallations",
                BindingFlags.Public | BindingFlags.Static);

            if (installationsMethod == null)
            {
                throw new MissingMethodException("Method 'DynamoInstallDetective.Utilities.FindProductInstallations' not found");
            }

            var methodParams = new object[] { "Autodesk", "ASMAHL*.dll" };
            return installationsMethod.Invoke(null, methodParams) as IEnumerable;
        }
    }
}
