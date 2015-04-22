using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Interfaces
{
    public interface IRenderPackageFactory
    {
        int MaxTessellationDivisions { get; set; }
        IRenderPackage CreateRenderPackage();
    }
}
