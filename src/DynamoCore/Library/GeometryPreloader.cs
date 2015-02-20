using System;
using System.IO;
using System.Reflection;
using Dynamo.Interfaces;
using DynamoUtilities;
using ProtoScript.Runners;

namespace Dynamo.Library
{
    /// <summary>
    /// Geometry preloader class that helps with preloading Autodesk Shape 
    /// Manager (ASM) binaries through geometry library (LibG). This class being
    /// part of Dynamo core module, relies on IGeometryConfiguration supplied by
    /// the host application to determine the installed location of ASM binaries.
    /// </summary>
    /// 
    class GeometryPreloader
    {
        #region Class Data Members and Properties

        private readonly string dynamoRootFolder;
        private readonly string preloaderLocation;
        private readonly string geometryFactoryPath;
        private readonly IGeometryConfiguration configuration;

        internal string GeometryFactoryPath { get { return geometryFactoryPath; } }

        #endregion

        #region Public Class Operational Methods

        internal GeometryPreloader(IGeometryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;

            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            dynamoRootFolder = Path.GetDirectoryName(assemblyPath);

            var version = ((int)configuration.Version);
            var libGFolderName = string.Format("libg_{0}", version);
            preloaderLocation = Path.Combine(dynamoRootFolder, libGFolderName);
            geometryFactoryPath = Path.Combine(preloaderLocation, Utils.GeometryFactoryAssembly);

            // TODO(PATHMANAGER): This retains the existing behavior of SetLibGPath method. Move this
            // out when DynamoPathManager is completely replaced by generic path resolution mechanism.
            DynamoPathManager.Instance.AddResolutionPath(preloaderLocation);
        }

        internal bool Preload()
        {
            if (!configuration.PreloadShapeManager)
                return false;

            var asmLocation = configuration.ShapeManagerPath;
            Utils.PreloadAsmFromPath(preloaderLocation, asmLocation);
            return true;
        }

        #endregion
    }
}
