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
        private readonly IGeometryConfiguration configuration;

        internal GeometryPreloader(IGeometryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;
        }

        internal bool Preload()
        {
            if (!configuration.PreloadShapeManager)
                return false;

            var assemblyPath = Assembly.GetCallingAssembly().Location;
            var directory = Path.GetDirectoryName(assemblyPath);

            var version = ((int) configuration.Version);
            var libGFolder = string.Format("libg_{0}", version);
            var preloaderLocation = Path.Combine(directory, libGFolder);

            var asmLocation = configuration.ShapeManagerPath;
            Utils.PreloadAsmFromPath(preloaderLocation, asmLocation);
            return true;
        }
    }
}
