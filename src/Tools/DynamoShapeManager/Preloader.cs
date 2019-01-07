using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DynamoShapeManager
{
    [Obsolete("Please use system.version instead of this enum to specify geometry library versions to load")]
    public enum LibraryVersion
    {
        None,
        Version224 = 224,
        Version223 = 223,
        Version222 = 222,
        Version221 = 221
    };

    /// <summary>
    /// Shape manager preloader class that helps with preloading Autodesk Shape 
    /// Manager (ASM) binaries through geometry library (LibG). This class being
    /// part of Dynamo core module, relies on IGeometryConfiguration supplied by
    /// the host application to determine the installed location of ASM binaries.
    /// </summary>
    /// 
    public class Preloader
    {

        #region Class Data Members and Properties
        /// <summary>
        /// This static data member represents the location of the shape manager
        /// that has been successfully preloaded. It is made static to ensure no 
        /// more than one ASM gets preloaded in the same address space.
        /// </summary>
        private static string preloadedShapeManagerPath = string.Empty;
        [Obsolete("Please use the Version2 Property instead.")]
        public LibraryVersion Version { get { return (LibraryVersion)Version2.Major; } }
        public Version Version2 { get; }

        public string ShapeManagerPath { get; set; }
        public string PreloaderLocation { get; }
        public string GeometryFactoryPath { get; }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Constructs a preloader object to help preload shape manager.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_xxx folder, where 'xxx' represents the library version. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.
        /// </param>
        /// 
        public Preloader(string rootFolder)
            : this(rootFolder, new[]
            {
                    new Version(225,0,0),
                    new Version(224,4,0),
                    new Version(224,0,1),
                    new Version(223,0,1),
                    new Version(222,0,0),
                    new Version(221,0,0)
            })
        {
        }

        //used to convert libraryVersions to the real asm versions they represented
        private static Dictionary<LibraryVersion, Version> ASMLibVersionToVersion = new Dictionary<LibraryVersion, Version>()
        {
            { LibraryVersion.Version221, new Version(221,0,0) },
            { LibraryVersion.Version222, new Version(222,0,0) },
            { LibraryVersion.Version223, new Version(223,0,1) },
            { LibraryVersion.Version224, new Version(224,0,1) },

        };

        internal static Version MapLibGVersionEnumToFullVersion(LibraryVersion libVersion)
        {
            Version version;
            if (ASMLibVersionToVersion.TryGetValue(libVersion, out version))
            {
                return version;
            }
            else
            {
                version = new Version((int)libVersion, 0, 0);
                Console.WriteLine("Could not find a full version mapping for LibGVersion: " + libVersion + "returning: " + version.ToString());
                return version;
            }
        }

        /// <summary>
        /// Constructs a preloader object to help preload a specific version of 
        /// shape manager.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_xxx folder, where 'xxx' represents the library version. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.
        /// </param>
        /// <param name="versions">A list of version numbers to check for in order 
        /// of preference. This argument cannot be null or empty.</param>
        /// 
        [Obsolete("please use constructor Preloader constructor with signature Preloader(string,IEnumerable<Version>)")]
        public Preloader(string rootFolder, IEnumerable<LibraryVersion> versions) :
            this(rootFolder, versions.Select(libVersion => MapLibGVersionEnumToFullVersion(libVersion)))
        {

        }

        /// <summary>
        /// Constructs a preloader object to help preload a specific version of 
        /// shape manager.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_major_minor_build folder. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.
        /// </param>
        /// <param name="versions">A list of version numbers to check for in order 
        /// of preference. This argument cannot be null or empty.</param>
        /// 
        public Preloader(string rootFolder, IEnumerable<Version> versions)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);
            if ((versions == null) || !versions.Any())
                throw new ArgumentNullException("versions");

            var versionList = versions.ToList();

            var shapeManagerPath = string.Empty; // Folder that contains ASM binaries.
            Version2 = Utilities.GetInstalledAsmVersion2(versionList, ref shapeManagerPath, rootFolder);
            ShapeManagerPath = shapeManagerPath;
            PreloaderLocation = Utilities.GetLibGPreloaderLocation(Version2, rootFolder);
            GeometryFactoryPath = Path.Combine(PreloaderLocation,
                Utilities.GeometryFactoryAssembly);
        }

        /// <summary>
        /// Constructs a preloader object to help preload the specified version 
        /// of shape manager from the given directory.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_xxx folder, where 'xxx' represents the library version. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.
        /// </param>
        /// <param name="shapeManagerPath">The directory from where shape manager
        /// binaries can be preloaded from.</param>
        /// <param name="version">The version of shape manager.</param>
        /// 
        [Obsolete("please use Preloader constructor with signature Preloader(string,IEnumerable<Version>)")]
        public Preloader(string rootFolder, string shapeManagerPath, LibraryVersion version)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);

            if (string.IsNullOrEmpty(shapeManagerPath))
                throw new ArgumentNullException("shapeManagerPath");
            if (!Directory.Exists(shapeManagerPath))
                throw new DirectoryNotFoundException(shapeManagerPath);

            if (version == LibraryVersion.None)
                throw new ArgumentOutOfRangeException("version");

            this.Version2 = MapLibGVersionEnumToFullVersion(version);
            this.ShapeManagerPath = shapeManagerPath;

            var libGFolderName = string.Format("libg_{0}_{1}_{2}", this.Version2.Major, this.Version2.Minor, this.Version2.Build);
            PreloaderLocation = Path.Combine(rootFolder, libGFolderName);
            GeometryFactoryPath = Path.Combine(PreloaderLocation,
                Utilities.GeometryFactoryAssembly);
        }

        /// <summary>
        /// Construct a Preloader by specifying a required library version.
        /// </summary>
        /// <param name="rootFolder">Full path of the directory that contains 
        /// LibG_xxx folder, where 'xxx' represents the library version. In a 
        /// typical setup this would be the same directory that contains Dynamo 
        /// core modules. This must represent a valid directory.
        /// </param>
        /// <param name="version">The version of shape manager.</param>
        /// <returns></returns>
        [Obsolete("please use constructor Preloader constructor with signature Preloader(string,IEnumerable<Version>)")]
        public Preloader(string rootFolder, LibraryVersion version)
            : this(rootFolder, new[] { version }) { }

        /// <summary>
        /// Attempts to load the geometry library binaries using the version and location
        /// specified when the Preloader was constructed.
        /// </summary>
        public void Preload()
        {
            if (Version2 == null)
                return;

            if (!string.IsNullOrEmpty(preloadedShapeManagerPath))
            {
                // A previous preloading was done. If this call is targeting 
                // shape manager from a different folder, then that represents 
                // an API misuse -- ASM of different versions should not be 
                // loaded in the same process.
                // 
                const StringComparison opt = StringComparison.InvariantCultureIgnoreCase;
                if (string.Compare(preloadedShapeManagerPath, ShapeManagerPath, opt) != 0)
                {
                    var message = string.Format("Different versions of ASM cannot be loaded " +
                        "in the same process:\n\nFirst attempt: {0}\nSecond attempt: {1}",
                        preloadedShapeManagerPath, ShapeManagerPath);

                    throw new InvalidOperationException(message);
                }

                // A previous preload was done before this which targeted 
                // the same assembly version, bail, without loading it again.
                return;
            }

            preloadedShapeManagerPath = ShapeManagerPath;
            Utilities.PreloadAsmFromPath(PreloaderLocation, ShapeManagerPath);
        }

        #endregion
    }
}
