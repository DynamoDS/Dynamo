namespace Dynamo.Interfaces
{
    public enum LibraryVersion
    {
        Version219,
        Version220,
        Version221,
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
    }
}
