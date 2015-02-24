using System;
using System.Collections.Generic;
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

        public LibraryVersion Version { get { return version; } }
        public string PreloaderLocation { get { return preloaderLocation; } }
        public string GeometryFactoryPath { get { return geometryFactoryPath; } }

        #endregion

        #region Public Class Operational Methods

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
            var index = Utilities.GetInstalledAsmVersion(versions, ref shapeManagerPath);
            version = ((index >= 0) ? versionList[index] : LibraryVersion.None);

            var libGFolderName = string.Format("libg_{0}", ((int) version));
            preloaderLocation = Path.Combine(rootFolder, libGFolderName);
            geometryFactoryPath = Path.Combine(preloaderLocation,
                Utilities.GeometryFactoryAssembly);
        }

        public void Preload()
        {
            if (version != LibraryVersion.None)
                Utilities.PreloadAsmFromPath(preloaderLocation, shapeManagerPath);
        }

        #endregion
    }
}
