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

        /// <summary>
        /// This method is used to set the dynamo interaction state into an existing material slot
        /// which is sent to the shader buffer for lines.
        /// </summary>
        /// <param name="state"></param>
        internal void SetState(int state)
        {  
             //TODO should we also use this for points if it works for lines to make them consistent?
            this.material.FadingNearDistance = state;
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
            //TODO create new shader for lines and use it here.
            return host.EffectsManager[DynamoEffectsManager.DynamoPointShaderName];
        }
    }
}
