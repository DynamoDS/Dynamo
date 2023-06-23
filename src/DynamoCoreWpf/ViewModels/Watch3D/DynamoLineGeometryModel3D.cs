using System;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Scene;


namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A Dynamo line class which supports the RenderCustom technique.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class DynamoLineGeometryModel3D : LineGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            // Override the default value of 100 set in the base LineMaterialCore class:
            // https://github.com/helix-toolkit/helix-toolkit/blob/develop/Source/HelixToolkit.SharpDX.Shared/Model/Material/LineMaterialCore.cs#L80
            // as we are using it to pass state info to the shader.
            material.FadingNearDistance = 0;
            return new DynamoLineNode { Material = material };
        }

        /// <summary>
        /// This method is used to set the dynamo interaction state into an existing material slot
        /// which is sent to the shader buffer for lines.
        /// </summary>
        /// <param name="state"></param>
        internal void SetState(int state)
        {
            // Reuse FadingNearDistance because our shader does not use it and we need space for an int
            // to store our interaction state which will be sent to the shader.
            material.FadingNearDistance = state;
        }
    }

    public class DynamoLineNode : LineNode
    {
        protected override RenderCore OnCreateRenderCore()
        {
            return new DynamoPointLineRenderCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[DynamoEffectsManager.DynamoLineShaderName];
        }
    }
}
