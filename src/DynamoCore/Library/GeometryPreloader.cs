using System;
using System.IO;
using System.Reflection;
using Dynamo.Interfaces;
using DynamoUtilities;
using ProtoScript.Runners;

namespace Dynamo.Library
{
    class GeometryPreloader
    {
        #region Class Data Members and Properties

        private readonly string dynamoRootFolder;
        private readonly string libGFolderName;
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

            var assemblyPath = Assembly.GetCallingAssembly().Location;
            dynamoRootFolder = Path.GetDirectoryName(assemblyPath);

            var version = ((int)configuration.Version);
            libGFolderName = string.Format("libg_{0}", version);

            geometryFactoryPath = Path.Combine(dynamoRootFolder,
                libGFolderName, Utils.GeometryFactoryAssembly);
        }

        internal bool Preload()
        {
            if (!configuration.PreloadShapeManager)
                return false;

            var preloaderLocation = Path.Combine(dynamoRootFolder, libGFolderName);
            var asmLocation = configuration.ShapeManagerPath;
            Utils.PreloadAsmFromPath(preloaderLocation, asmLocation);

            return true;
        }

        #endregion
    }
}
