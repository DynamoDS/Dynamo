using System.Configuration;
using System.IO;
using Dynamo.Interfaces;
using DynamoUtilities;

namespace Dynamo
{
    public class GeometryConfigurationForTests : IGeometryConfiguration
    {
        private readonly string geometryFactoryPath;
        private readonly string shapeManagerPath;
        private readonly LibraryVersion libraryVersion = LibraryVersion.None;

        public GeometryConfigurationForTests()
        {
            var versions = new[]
            {
                ((int) LibraryVersion.Version219),
                ((int) LibraryVersion.Version220),
                ((int) LibraryVersion.Version221),
            };

            var location = string.Empty;
            var installed = Utils.GetInstalledAsmVersion(versions, ref location);
            if (installed >= 0)
            {
                libraryVersion = ((LibraryVersion)versions[installed]);
                shapeManagerPath = location;
            }
        }

        public LibraryVersion Version
        {
            get { return libraryVersion; }
        }

        public string ShapeManagerPath
        {
            get { return shapeManagerPath; }
        }

        public bool PreloadShapeManager
        {
            get { return true; }
        }

        public void PreloadAsm(string rootDirectory)
        {
            var version = ((int)libraryVersion);
            var preloaderLocation = Path.Combine(rootDirectory, string.Format("libg_{0}", version));
            Utils.PreloadAsmFromPath(preloaderLocation, ShapeManagerPath);
        }

        public string GetGeometryFactoryPath(string rootDirectory)
        {
            var version = ((int) libraryVersion);
            return Utils.GetGeometryFactoryPath(rootDirectory, version);
        }
    }
}
