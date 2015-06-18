using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// IRenderPackageFactory is used to create IRenderPackage objects suitable 
    /// for a specific rednering pipeline. IRenderPackages generated from IRenderPackageFactory 
    /// classes contain tessellated geometry for rendering, which may be stored 
    /// in different forms depending on the rendering pipeline being used. 
    /// </summary>
    public interface IRenderPackageFactory
    {
        /// <summary>
        /// The maximum number of subdivisions of a surface for tesselation.
        /// Used only by methods that tessellate BReps.
        /// </summary>
        int MaxTessellationDivisions { get; set; }

        /// <summary>
        /// Create an IRenderPackage object of the type manufactured by this factory.
        /// </summary>
        IRenderPackage CreateRenderPackage();
    }
}
