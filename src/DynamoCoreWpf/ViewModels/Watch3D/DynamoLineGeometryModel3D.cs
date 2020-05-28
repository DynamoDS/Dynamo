using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Scene;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A Dynamo line class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoLineGeometryModel3D : LineGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new DynamoLineNode();
        }
    }

    public class DynamoLineNode : LineNode
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
