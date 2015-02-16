namespace Dynamo.Interfaces
{
    public enum LibraryVersion
    {
        None,
        Version219 = 219,
        Version220 = 220,
        Version221 = 221,
    };

    /// <summary>
    /// Through this interface, Dynamo determines the right geometry 
    /// library provider to use and its associated path configurations.
    /// </summary>
    public interface IGeometryConfiguration
    {
        /// <summary>
        /// Geometry library versions.
        /// </summary>
        LibraryVersion Version { get; }

        /// <summary>
        /// The full path of directory where ASM binaries reside.
        /// </summary>
        string ShapeManagerPath { get; }

        /// <summary>
        /// This property determines if ASM binaries are to be preloaded.
        /// Certain host application preloads these ASM binaries before Dynamo,
        /// in such cases there will not be a need to preload them again.
        /// </summary>
        bool PreloadShapeManager { get; }
    }
}
