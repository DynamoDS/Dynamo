using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Visualization
{
    /// <summary>
    /// IRenderPackageFactory is used to create IRenderPackage objects suitable 
    /// for a specific rednering pipeline. IRenderPackages generated from IRenderPackageFactory 
    /// classes contain tessellated geometry for rendering, which may be stored 
    /// in different forms depending on the rendering pipeline being used. 
    /// </summary>
    public interface IRenderPackageFactory
    {
        TessellationParameters TessellationParameters { get; set; }

        /// <summary>
        /// Create an IRenderPackage object of the type manufactured by this factory.
        /// </summary>
        IRenderPackage CreateRenderPackage();
    }
}
