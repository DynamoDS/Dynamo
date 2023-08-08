using System;
using System.Windows; 
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Model.Scene;


namespace Dynamo.Wpf.ViewModels.Watch3D
{

  

    /// <summary>
    /// A low level (non wpf) object which can be rendered. 
    /// This class handles actually updating the scene and shaders. Updates to the properties on this class invalidate
    /// the renderer and pass new data to the shader.
    /// This class can override callbacks that occur during low level rendering updates. The only one we use currently is
    /// OnUpdatePerModelStruct() to modify the per model data that is passed to our shader.
    /// </summary>
    internal class DynamoGeometryMeshCore : MeshRenderCore
    {
        internal DynamoRenderCoreDataStore dataCore;
        public DynamoGeometryMeshCore()
        {
            dataCore = new DynamoRenderCoreDataStore(SetAffectsRender<bool>);
        }

        protected override void OnUpdatePerModelStruct(RenderContext context)
        {
            base.OnUpdatePerModelStruct(context);
            //store the entire state of all our flags in the X component and decode in the vertex shader.
            //we'll pass this from the vertex shader to the fragment shader to determine color states.
            //Params is a helix material builtin and maps to vParams in the shader.
            modelStruct.Params.X = dataCore.GenerateEnumFromState();
        }

        internal void SetPropertyData(DependencyPropertyChangedEventArgs args)
        {
            dataCore.SetPropertyData(args);
        }
    }


    /// <summary>
    /// A Dynamo mesh class which supports sending data to our custom shader.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class DynamoGeometryModel3D : MaterialGeometryModel3D
    {
        protected override SceneNode OnCreateSceneNode()
        {
            return new DynamoMeshNode();
        }

        [Obsolete("This property will be deprecated and made internal in Dynamo 3.0.")]
        public static readonly DependencyProperty RequiresPerVertexColorationProperty =
            DependencyProperty.Register("RequiresPerVertexColoration", typeof(bool), 
                typeof(GeometryModel3D), new UIPropertyMetadata(false, RequirePerVertexColorationChanged));

        private static void RequirePerVertexColorationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            AttachedProperties.HandleMeshPropertyChange(obj as DynamoGeometryModel3D, e);
        }

        /// <summary>
        /// Does this model require displaying vertex colors.
        /// </summary>
        public bool RequiresPerVertexColoration
        {
            get
            {
                return (bool)GetValue(RequiresPerVertexColorationProperty);
            }
            set { SetValue(RequiresPerVertexColorationProperty, value); }
        }

    }

    /// <summary>
    /// This class represents a DynamoMesh in the scene and is used to attach a custom shader and 
    /// a custom Core - that lets us pass custom data to the shader.
    /// Each WPF Model object relates to a sharpdx - scene node - Element3d is a wrapper on sceneNode so
    /// these are essentially the same.
    /// </summary>
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class DynamoMeshNode : MeshNode
    {
   
        protected override RenderCore OnCreateRenderCore()
        {
            return new DynamoGeometryMeshCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IEffectsManager effectsManager)
        {
            return effectsManager[DynamoEffectsManager.DynamoMeshShaderName];
        }
    }
}
