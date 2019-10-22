using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A Dynamo mesh class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoGeometryModel3D : MeshGeometryModel3D
    {
        public DynamoGeometryModel3D()
        {
        }

        protected override SceneNode OnCreateSceneNode()
        {
            var node = base.OnCreateSceneNode();
            node.OnSetRenderTechnique = (host) => { return host.EffectsManager["RenderCustom"]; };
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
