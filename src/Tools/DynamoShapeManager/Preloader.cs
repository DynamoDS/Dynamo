using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace DynamoShapeManager
{
    public enum LibraryVersion
    {
        None,
        Version219 = 219,
        Version220 = 220,
        Version221 = 221,
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

        private readonly LibraryVersion version;
        private readonly string shapeManagerPath;
        private readonly string preloaderLocation;
        private readonly string geometryFactoryPath;

        /// <summary>
        /// This static data member represents the location of the shape manager
        /// that has been successfully preloaded. It is made static to ensure no 
        /// more than one ASM gets preloaded in the same address space.
        /// </summary>
        private static string preloadedShapeManagerPath = string.Empty;

        public LibraryVersion Version { get { return version; } }
        public string PreloaderLocation { get { return preloaderLocation; } }
        public string GeometryFactoryPath { get { return geometryFactoryPath; } }

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
                LibraryVersion.Version219,
                LibraryVersion.Version220,
                LibraryVersion.Version221
            })
        {
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
        public Preloader(string rootFolder, IEnumerable<LibraryVersion> versions)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);
            if ((versions == null) || !versions.Any())
                throw new ArgumentNullException("versions");

            var versionList = versions.ToList();

            shapeManagerPath = string.Empty; // Folder that contains ASM binaries.
            version = Utilities.GetInstalledAsmVersion(versionList, ref shapeManagerPath);
            
            var libGFolderName = string.Format("libg_{0}", ((int) version));
            preloaderLocation = Path.Combine(rootFolder, libGFolderName);
            geometryFactoryPath = Path.Combine(preloaderLocation,
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

            this.version = version;
            this.shapeManagerPath = shapeManagerPath;

            var libGFolderName = string.Format("libg_{0}", ((int) version));
            preloaderLocation = Path.Combine(rootFolder, libGFolderName);
            geometryFactoryPath = Path.Combine(preloaderLocation,
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
        public Preloader(string rootFolder, LibraryVersion version)
            : this(rootFolder, new[] { version }) { }

        public void Preload()
        {
            if (version == LibraryVersion.None)
                return;

            if (!string.IsNullOrEmpty(preloadedShapeManagerPath))
            {
                // A previous preloading was done. If this call is targeting 
                // shape manager from a different folder, then that represents 
                // an API misuse -- ASM of different versions should not be 
                // loaded in the same process.
                // 
                const StringComparison opt = StringComparison.InvariantCultureIgnoreCase;
                if (string.Compare(preloadedShapeManagerPath, shapeManagerPath, opt) != 0)
                {
                    var message = string.Format("Different versions of ASM cannot be loaded " +
                        "in the same process:\n\nFirst attempt: {0}\nSecond attempt: {1}",
                        preloadedShapeManagerPath, shapeManagerPath);

                    throw new InvalidOperationException(message);
                }

                // A previous preload was done before this which targeted 
                // the same assembly version, bail, without loading it again.
                return;
            }

            preloadedShapeManagerPath = shapeManagerPath;
            Utilities.PreloadAsmFromPath(preloaderLocation, shapeManagerPath);
        }

        #endregion
    }
}
