using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Scene;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A Dynamo point class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoPointGeometryModel3D : PointGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new DynamoPointNode() { Material = material };
        }
    }

    internal class DynamoPointNode : PointNode
    {
        protected override RenderCore OnCreateRenderCore()
        {
            return new DynamoPointLineCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[DynamoEffectsManager.DynamoPointLineShaderName];
        }
    }
}
