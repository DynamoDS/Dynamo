using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Dynamo.Wpf.ViewModels.Watch3D
{

    public class DynamoGeometryModelNode : MeshNode
    {
        //TODO add any properties we wish to set here and 
        //send to the shaders. These will end up setting properties on the 
        //core which updates a ModelStruct that is sent to the shaders.

        public bool IsFrozen
        {
            get
            {
                return (RenderCore as DynamoGeometryMeshCore).IsFrozenData;
            }
            set
            {
                (RenderCore as DynamoGeometryMeshCore).IsFrozenData = value;
            }
        }

        protected override RenderCore OnCreateRenderCore()
        {
            return new DynamoGeometryMeshCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[DynamoCustomShaderNames.DynamoCustomMeshShader];
        }


    }

    [Flags]
    internal enum DynamoMeshShaderFlags
    {
        None = 0,
        IsFrozen = 1,
        IsSelected = 2,
        IsIsolated = 4,
        IsSpecialRenderPackage = 8,
        //TODO do we need this flag?
        HasTransparency = 16,
        //TODO add vertex colors
        //TODO add flat shade?
    }

    internal class DynamoGeometryMeshCore : MeshRenderCore
    {

        public bool IsFrozenData { get; internal set; }
        public bool IsSelectedData { get; internal set; }
        public bool IsIsolatedData { get; internal set; }
        public bool IsSpecialRenderPackageData { get; internal set; }
        public bool HasTransparencyData { get; internal set; }

        private int GenerateEnumFromState()
        {
            var finalData = (int)(DynamoMeshShaderFlags.None)
                 + (int)(IsFrozenData ? DynamoMeshShaderFlags.IsFrozen : 0)
                 + (int)(IsSelectedData ? DynamoMeshShaderFlags.IsSelected : 0)
                  + (int)(IsIsolatedData ? DynamoMeshShaderFlags.IsIsolated : 0)
                   + (int)(IsSpecialRenderPackageData ? DynamoMeshShaderFlags.IsSpecialRenderPackage : 0)
                    + (int)(HasTransparencyData ? DynamoMeshShaderFlags.HasTransparency : 0);

            return finalData;
        }

        protected override void OnUpdatePerModelStruct(RenderContext context)
        {
            base.OnUpdatePerModelStruct(context);
            //store the entire state in the X component and decode in the shader.
            modelStruct.BoolParams.X = GenerateEnumFromState();
            modelStruct.Params.Y = 2f;
        }
    }


    /// <summary>
    /// A Dynamo mesh class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoGeometryModel3D : MeshGeometryModel3D //TODO old base class okay? MaterialGeo?
    {
        public DynamoGeometryModel3D()
        {
        }

        protected override SceneNode OnCreateSceneNode()
        {
            var node = base.OnCreateSceneNode();
            node.OnSetRenderTechnique = (host) => { return host.EffectsManager[DynamoCustomShaderNames.DynamoCustomMeshShader]; };
            return node;
        }

        public static readonly DependencyProperty RequiresPerVertexColorationProperty =
            DependencyProperty.Register("RequiresPerVertexColoration", typeof(bool), typeof(GeometryModel3D), new UIPropertyMetadata(false));

        public bool RequiresPerVertexColoration
        {
            get
            {
                return (bool)GetValue(RequiresPerVertexColorationProperty);
            }
            set { SetValue(RequiresPerVertexColorationProperty, value); }
        }
    }
}
