using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Interfaces
{
    public interface IRenderPackageFactory
    {
        int MaxTesselationDivisions { get; set; }
        IRenderPackage CreateRenderPackage();
    }
}
