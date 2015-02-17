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
    /// Autodesk Shape Manager (ASM) lives outside of Dynamo core as independent
    /// entities. There might be multiple versions of ASM being installed on the 
    /// machine. This interface is provided as a mean for Dynamo host applications
    /// (e.g. DynamoSandbox.exe or Revit add-on) to specify the right ASM version 
    /// to load.
    /// </summary>
    /// 
    public interface IGeometryConfiguration
    {
        /// <summary>
        /// Version of ASM binaries to use.
        /// </summary>
        LibraryVersion Version { get; }

        /// <summary>
        /// The full path of directory where ASM binaries reside.
        /// </summary>
        string ShapeManagerPath { get; }

        /// <summary>
        /// This property determines if ASM binaries are to be preloaded. Some 
        /// host applications (e.g. Revit) preloads these ASM binaries before 
        /// Dynamo, in such cases there will not be a need to preload them again.
        /// </summary>
        bool PreloadShapeManager { get; }
    }
}
