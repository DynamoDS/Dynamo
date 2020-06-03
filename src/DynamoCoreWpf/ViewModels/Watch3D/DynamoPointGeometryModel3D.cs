using System;
using System.Windows;
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

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[DynamoEffectsManager.DynamoPointShaderName];
        }
    }

    internal class DynamoPointLineRenderCore : PointLineRenderCore
    {
        private DynamoRenderCoreDataStore dataCore;
        public DynamoPointLineRenderCore()
        {
            dataCore = new DynamoRenderCoreDataStore(SetAffectsRender<bool>);
        }

        internal void SetPropertyData(DependencyPropertyChangedEventArgs args, GeometryModel3D geo)
        {
            dataCore.SetPropertyData(args);
            
            if(geo is DynamoPointGeometryModel3D || geo is DynamoLineGeometryModel3D)
            {
                if (geo is DynamoPointGeometryModel3D)
                {
                    (geo as DynamoPointGeometryModel3D).SetState(dataCore.GenerateEnumFromState());
                }
                else
                {
                    (geo as DynamoLineGeometryModel3D).SetState(dataCore.GenerateEnumFromState());
                }
            }
           
        }

    }
}
