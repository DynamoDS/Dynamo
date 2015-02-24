using System;
using System.IO;

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

        private readonly string preloaderLocation;
        private readonly string geometryFactoryPath;

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
        /// <param name="version">The version of shape manager to load.</param>
        /// 
        public Preloader(string rootFolder, LibraryVersion version)
        {
            if (string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException(rootFolder);
            if (version == LibraryVersion.None)
                throw new ArgumentNullException("version");

            var libGFolderName = string.Format("libg_{0}", ((int) version));
            preloaderLocation = Path.Combine(rootFolder, libGFolderName);
            geometryFactoryPath = Path.Combine(preloaderLocation,
                Utilities.GeometryFactoryAssembly);

            // TODO(PATHMANAGER): This retains the existing behavior of SetLibGPath
            // method. Move this out when DynamoPathManager is completely replaced 
            // by generic path resolution mechanism.
            // 
            // DynamoPathManager.Instance.AddResolutionPath(preloaderLocation);
        }

        public void Preload(string shapeManagerPath)
        {
            Utilities.PreloadAsmFromPath(preloaderLocation, shapeManagerPath);
        }

        #endregion
    }
}
