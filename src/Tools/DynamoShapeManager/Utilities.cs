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
        /// <summary>
        /// Key words for Products containing ASM binaries
        /// </summary>
        private static readonly List<string> ProductsWithASM = new List<string>() { "Revit", "Civil", "FormIt" };

        #region public properties
        public static readonly string GeometryFactoryAssembly = "LibG.ProtoInterface.dll";
        public static readonly string PreloaderAssembly = "LibG.AsmPreloader.Managed.dll";
        public static readonly string PreloaderClassName = "Autodesk.LibG.AsmPreloader";

        /// <summary>
        /// This method is defined in libG.AsmPreloader for actual ASM preload
        /// </summary>
        public static readonly string PreloaderMethodName = "PreloadAsmLibraries";

        /// <summary>
        /// The mask to filter ASM binary
        /// </summary>
        public static readonly string ASMFileMask = "ASMAHL*.dll";
        #endregion


        /// <summary>
        /// Call this method to determine the version of ASM that is installed 
        /// on the user machine. The method scans through a list of known Autodesk 
        /// product folders for ASM binaries with the targeted version.
        /// </summary>
        /// <param name="versions">A list of version numbers to check for in order 
        /// of preference. This argument cannot be null or empty.</param>
        /// <param name="location">The full path of the directory in which targeted
        /// ASM binaries are found. This argument cannot be null.</param>
        /// <param name="rootFolder">This method makes use of DynamoInstallDetective
        /// to determine the installation location of various Autodesk products. This 
        /// argument is not optional and must represent the full path to the folder 
        /// which contains DynamoInstallDetective.dll. An exception is thrown if the 
        /// assembly cannot be located.</param>
        /// <returns>Returns LibraryVersion of ASM if any installed ASM is found, 
        /// or None otherwise.</returns>
        /// 
        [Obsolete("Please use version of this method which accepts precise collection of version objects.")]
        public static LibraryVersion GetInstalledAsmVersion(List<LibraryVersion> versions, ref string location, string rootFolder)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);
            if ((versions == null) || versions.Count <= 0)
                throw new ArgumentNullException("versions");
            if (location == null)
                throw new ArgumentNullException("location");

            location = string.Empty;

            try
            {
                var installations = GetAsmInstallations(rootFolder);

                foreach (var v in versions)
                {
                    foreach (KeyValuePair<string, Tuple<int, int, int, int>> install in installations)
                    {
                        if ((int)v == install.Value.Item1)
                        {
                            location = install.Key;
                            return (LibraryVersion)install.Value.Item1;
                        }
                    }
                }


                //Fallback mechanism, look inside libg folders if any of them
                //contain ASM dlls.
                foreach (var v in versions)
                {
                    var folderName = string.Format("libg_{0}", (int)v);
                    var dir = new DirectoryInfo(Path.Combine(rootFolder, folderName));
                    if (!dir.Exists)
                        continue;

                    var files = dir.GetFiles(ASMFileMask);
                    if (!files.Any())
                        continue;

                    location = dir.FullName;
                    return v; // Found version.
                }
            }
            catch (Exception)
            {
                return LibraryVersion.None;
            }

            return LibraryVersion.None;
        }

        /// <summary>
        /// Call this method to determine the version of ASM that is installed 
        /// on the user machine. The method scans through a list of known Autodesk 
        /// product folders for ASM binaries with the targeted version.
        /// </summary>
        /// <param name="versions">A IEnumerable of version numbers to check for in order 
        /// of preference. This argument cannot be null or empty.</param>
        /// <param name="location">The full path of the directory in which targeted
        /// ASM binaries are found. This argument cannot be null.</param>
        /// <param name="rootFolder">This method makes use of DynamoInstallDetective
        /// to determine the installation location of various Autodesk products. This 
        /// argument is not optional and must represent the full path to the folder 
        /// which contains DynamoInstallDetective.dll. An exception is thrown if the 
        /// assembly cannot be located.</param>
        /// <param name="getASMInstallsFunc"> A delegate which can be used to replace the default ASM install
        /// lookup method. This is primarily used for testing. The delegate should return an IEnumerable
        /// of Tuples - these represent versions of ASM which are located on the user's machine.</param>
        /// <returns>Returns System.Version of ASM if any installed ASM is found, 
        /// or null otherwise.</returns>
        /// 
        public static Version GetInstalledAsmVersion2(IEnumerable<Version> versions, ref string location, string rootFolder, Func<string, IEnumerable> getASMInstallsFunc = null)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);
            if ((versions == null) || versions.Count() <= 0)
                throw new ArgumentNullException("versions");
            if (location == null)
                throw new ArgumentNullException("location");

            location = string.Empty;

            try
            {
                // use the passed lookup function if it exists,
                // else use the default asm install lookup -
                // this is used for testing
                getASMInstallsFunc = getASMInstallsFunc ?? GetAsmInstallations;
                var installations = getASMInstallsFunc(rootFolder);

                // first find the exact match or the lowest matching within same major version
                foreach (var version in versions)
                {
                    Dictionary<Version, string> versionToLocationDic = new Dictionary<Version, string>();
                    foreach (KeyValuePair<string, Tuple<int, int, int, int>> install in installations)
                    {
                        var installVersion = new Version(install.Value.Item1, install.Value.Item2, install.Value.Item3);
                        if (version.Major == installVersion.Major && !versionToLocationDic.ContainsKey(installVersion))
                        {
                            versionToLocationDic.Add(installVersion, install.Key);
                        }
                    }

                    // When there is major version matching, continue the search
                    if (versionToLocationDic.Count != 0)
                    {
                        versionToLocationDic.TryGetValue(version, out location);
                        // If exact matching version found, return it
                        if (location != null)
                        {
                            return version;
                        }
                        // If no matching version, return the lowest within same major
                        else
                        {
                            location = versionToLocationDic[versionToLocationDic.Keys.Min()];
                            return versionToLocationDic.Keys.Min();
                        }
                    }
                }

                // Fallback mechanism, look inside libg folders if any of them contains ASM dlls.
                foreach (var v in versions)
                {
                    var folderName = string.Format("libg_{0}_{1}_{2}", v.Major, v.Minor, v.Build);
                    var dir = new DirectoryInfo(Path.Combine(rootFolder, folderName));
                    if (!dir.Exists)
                        continue;

                    var files = dir.GetFiles(ASMFileMask);
                    if (!files.Any())
                        continue;

                    location = dir.FullName;
                    return v;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
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
            // this will either be empty, the originally requested preloaderLocation, or a remapped location
            // based on the old libG version number.
            var preloaderLocationToLoad = "";

            // if we can't find the preloader location directly as passed
            // try converting it to a precise version location.
            if (!Directory.Exists(preloaderLocation))
            {
                // Path/To/Extern/LibG_223 ->  Path/To/Extern/LibG_223_0_1
                preloaderLocationToLoad = RemapOldLibGPathToNewVersionPath(preloaderLocation);
            }
            // the directory exists, just load it.
            else
            {
                preloaderLocationToLoad = preloaderLocation;
            }

            if (string.IsNullOrEmpty(preloaderLocationToLoad))
            {
                throw new ArgumentException("preloadedPath");
            }
            if (string.IsNullOrEmpty(asmLocation) || !Directory.Exists(asmLocation))
            {
                throw new ArgumentException("asmLocation");
            }
            var preloaderPath = Path.Combine(preloaderLocationToLoad, PreloaderAssembly);

            Debug.WriteLine(string.Format("LibG ASM Preloader location: {0}", preloaderPath));
            Debug.WriteLine(string.Format("ASM loading location: {0}", asmLocation));

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
        /// Attempts to remap a an old LibG path to a new one using a version map.
        /// We assume that the leaf directory is of the form LibG_[Version].
        /// </summary>
        /// <param name="preloaderLocation"></param>
        /// <returns> new version LibG path or Empty string if the path could not be remapped.</returns>
        internal static string RemapOldLibGPathToNewVersionPath(string preloaderLocation)
        {
            if (String.IsNullOrEmpty(preloaderLocation))
            {
                return string.Empty;
            }
            var folderName = Path.GetFileName(preloaderLocation);
            var splitName = folderName.Split('_');
            if (splitName.Count() == 2)
            {
                LibraryVersion outVersion;
                if (Enum.TryParse<LibraryVersion>(string.Format("Version{0}", splitName[1]), out outVersion))
                {
                    var version = DynamoShapeManager.Preloader.MapLibGVersionEnumToFullVersion(outVersion);
                    return Path.Combine(
                        Path.GetDirectoryName(preloaderLocation),
                        string.Format("libg_{0}_{1}_{2}", version.Major, version.Minor, version.Build)
                        );
                }
            }

            return "";
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
        [Obsolete("Please use GetGeometryFactoryPath2(string rootFolder, Version version).")]
        public static string GetGeometryFactoryPath(string rootFolder, LibraryVersion version)
        {
            return GetGeometryFactoryPath2(rootFolder, Preloader.MapLibGVersionEnumToFullVersion(version));
        }

        /// <summary>
        /// Call this method to resolve full path to GeometryFactoryAssembly 
        /// assembly, given the root folder and the version. This method throws 
        /// an exception if either of the folders/assembly cannot be found.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_xxx_y_z folder, where 'xxx y z' represents the library version of asm. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.</param>
        /// <param name="version">Version number of the targeted geometry library.
        /// If the resulting folder does not exist, this method throws an 
        /// FileNotFoundException.</param>
        /// <returns>The full path to GeometryFactoryAssembly assembly.</returns>
        /// 
        public static string GetGeometryFactoryPath2(string rootFolder, Version version)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");

            if (!Directory.Exists(rootFolder))
            {
                // Root directory must be specified and valid.
                throw new DirectoryNotFoundException(string.Format(
                    "Directory not found: {0}", rootFolder));
            }
            //IMPORTANT_ Going forward libg folders will be named as follows: libg_major_minor_build - in reference to ASM.

            var libGFolderName = string.Format("libg_{0}_{1}_{2}", version.Major, version.Minor, version.Build);
            var libGFolder = Path.Combine(rootFolder, libGFolderName);
            if (!Directory.Exists(libGFolder))
            {
                // LibG_version folder must be valid.
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


        private static IEnumerable GetAsmInstallations(string rootFolder)
        {
            var assemblyPath = Path.Combine(Path.Combine(rootFolder, "DynamoInstallDetective.dll"));
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException(assemblyPath);

            var assembly = Assembly.LoadFrom(assemblyPath);

            var type = assembly.GetType("DynamoInstallDetective.Utilities");

            var installationsMethod = type.GetMethod(
                "FindMultipleProductInstallations",
                BindingFlags.Public | BindingFlags.Static);

            if (installationsMethod == null)
            {
                throw new MissingMethodException("Method 'DynamoInstallDetective.Utilities.FindProductInstallations' not found");
            }


            var methodParams = new object[] { ProductsWithASM, ASMFileMask };
            return installationsMethod.Invoke(null, methodParams) as IEnumerable;
        }
    }
}
