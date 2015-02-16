using Dynamo.Interfaces;
using DynamoUtilities;

namespace Dynamo.DynamoSandbox
{
    class GeometryConfiguration : IGeometryConfiguration
    {
        private bool initialized;
        private string shapeManagerPath;
        private LibraryVersion libraryVersion = LibraryVersion.None;

        public LibraryVersion Version
        {
            get
            {
                InitializeIfNeeded();
                return libraryVersion;
            }
        }

        public string ShapeManagerPath
        {
            get
            {
                InitializeIfNeeded();
                return shapeManagerPath;
            }
        }

        public bool PreloadShapeManager
        {
            get { return true; }
        }

        private void InitializeIfNeeded()
        {
            if (initialized)
                return;

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
                libraryVersion = ((LibraryVersion) versions[installed]);
                shapeManagerPath = location;
            }

            initialized = true;
        }
    }
}
