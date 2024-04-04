using System;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A Dynamo point class which supports the RenderCustom technique.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class DynamoPointGeometryModel3D : PointGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new DynamoPointNode { Material = material };
        }

        internal void SetState(int state)
        {
            // Reuse FadingNearDistance because our shader does not use it and we need space for an int
            // to store our interaction state which will be sent to the shader.
            material.FadingNearDistance = state;
        }
    }

    internal class DynamoPointNode : PointNode
    {
        protected override RenderCore OnCreateRenderCore()
        {
            return new DynamoPointLineRenderCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
        {
            return effectsManager[DynamoEffectsManager.DynamoPointShaderName];
        }
    }

}
