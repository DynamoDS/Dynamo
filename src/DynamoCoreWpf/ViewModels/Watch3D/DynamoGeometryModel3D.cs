using System;
using System.Windows;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model.Scene;

namespace Dynamo.Wpf.ViewModels.Watch3D
{

    [Flags]
    internal enum DynamoMeshShaderStates
    {
        None = 0,
        /// <summary>
        /// Used to determine if alpha should be lowered.
        /// </summary>
        IsFrozen = 1,
        /// <summary>
        /// Used to determine if selection color should be set.
        /// </summary>
        IsSelected = 2,
        /// <summary>
        /// Used to determine if alpha should be lowered.
        /// </summary>
        IsIsolated = 4,
        /// <summary>
        /// Used to mark a mesh as coming from a special or meta(gizmo) render package.
        /// </summary>
        IsSpecialRenderPackage = 8,
        /// <summary>
        /// Currently this flag is not used in the shader.
        /// </summary>
        HasTransparency = 16,
        /// <summary>
        /// Used to determine if vertex colors should be displayed with shading.
        /// </summary>
        RequiresPerVertexColor = 32,
        /// <summary>
        /// Currently this flag is not used in the shader.
        /// </summary>
        FlatShade = 64
    }

    /// <summary>
    /// A low level (non wpf) object which can be rendered. 
    /// This class handles actually updating the scene and shaders. Updates to the properties on this class invalidate
    /// the renderer and pass new data to the shader.
    /// This class can override callbacks that occur during low level rendering updates. The only one we use currently is
    /// OnUpdatePerModelStruct() to modify the per model data that is passed to our shader.
    /// </summary>
    internal class DynamoGeometryMeshCore : MeshRenderCore
    {

        private bool isFrozenData;

        /// <summary>
        /// Is this model Frozen.
        /// </summary>
        public bool IsFrozenData { get { return isFrozenData; } internal set { SetAffectsRender(ref isFrozenData, value); } }

        private bool isSelectedData;

        /// <summary>
        /// Is this model currently selected.
        /// </summary>
        public bool IsSelectedData { get { return isSelectedData; } internal set { SetAffectsRender(ref isSelectedData, value); } }

        private bool isIsolatedData;
        /// <summary>
        /// Is IsolationMode active.
        /// </summary>
        public bool IsIsolatedData { get { return isIsolatedData; } internal set { SetAffectsRender(ref isIsolatedData, value); } }

        private bool isSpecialData;
        /// <summary>
        /// Is this model marked as a special render package.
        /// </summary>
        public bool IsSpecialRenderPackageData { get { return isSpecialData; } internal set { SetAffectsRender(ref isSpecialData, value); } }

        private bool hasTransparencyData;
        /// <summary>
        /// Does this model have alpha less than 255.
        /// </summary>
        public bool HasTransparencyData { get { return hasTransparencyData; } internal set { SetAffectsRender(ref hasTransparencyData, value); } }

        private bool requiresPerVertexColor;
        /// <summary>
        /// Should this model display vertex colors.
        /// </summary>
        public bool RequiresPerVertexColor { get { return requiresPerVertexColor; } internal set { SetAffectsRender(ref requiresPerVertexColor, value); } }

        private bool isFlatShaded;
        /// <summary>
        /// Should this model disregard lighting calculations and display unlit texture or vertex colors.
        /// </summary>
        public bool IsFlatShaded { get { return isFlatShaded; } internal set { SetAffectsRender(ref isFlatShaded, value); } }

        /// <summary>
        /// Generates an int that packs all enum flags into a single int.
        /// Can be decoded using binary &
        /// ie - Flags & 1 = IsFrozen
        ///  - if flags == 000000 - all flags are off
        ///  - if flags == 000001 - frozen is enabled
        ///  - if flags == 100001 - frozen and flatshade are enabled.
        /// </summary>
        /// <returns></returns>
        private int GenerateEnumFromState()
        {
            var finalFlag = (int)(DynamoMeshShaderStates.None)
                 + (int)(IsFrozenData ? DynamoMeshShaderStates.IsFrozen : 0)
                 + (int)(IsSelectedData ? DynamoMeshShaderStates.IsSelected : 0)
                  + (int)(IsIsolatedData ? DynamoMeshShaderStates.IsIsolated : 0)
                   + (int)(IsSpecialRenderPackageData ? DynamoMeshShaderStates.IsSpecialRenderPackage : 0)
                    + (int)(HasTransparencyData ? DynamoMeshShaderStates.HasTransparency : 0)
                     + (int)(RequiresPerVertexColor ? DynamoMeshShaderStates.RequiresPerVertexColor : 0)
                      + (int)(IsFlatShaded ? DynamoMeshShaderStates.FlatShade : 0);

            return finalFlag;
        }

        protected override void OnUpdatePerModelStruct(RenderContext context)
        {
            base.OnUpdatePerModelStruct(context);
            //store the entire state of all our flags in the X component and decode in the vertex shader.
            //we'll pass this from the vertex shader to the fragment shader to determine color states.
            //Params is a helix material builtin and maps to vParams in the shader.
            modelStruct.Params.X = GenerateEnumFromState();
        }

        internal void SetPropertyData(DependencyPropertyChangedEventArgs args )
        {
            var depType = args.Property;
            var argval = (bool)args.NewValue;
            //use dependencyProperty to determine which data to set.
            if (depType == AttachedProperties.ShowSelectedProperty )
            {
                IsSelectedData = argval;
            }

            else if (depType == AttachedProperties.IsFrozenProperty)
            {
                IsFrozenData = argval;
            }

            else if (depType == AttachedProperties.IsolationModeProperty)
            {
                IsIsolatedData = argval;
            }

            else if (depType == AttachedProperties.IsSpecialRenderPackageProperty)
            {
                IsSpecialRenderPackageData = argval;
            }

            else if (depType == AttachedProperties.HasTransparencyProperty)
            {
                HasTransparencyData = argval;
            }

            else if (depType == DynamoGeometryModel3D.RequiresPerVertexColorationProperty)
            {
                RequiresPerVertexColor = argval;
            }


            //TODO we need to add FlatShader to AttachedProperties if we want to use it.
            //and add a case here.
        }
    }


    /// <summary>
    /// A Dynamo mesh class which supports sending data to our custom shader.
    /// </summary>
    public class DynamoGeometryModel3D : MaterialGeometryModel3D
    {
        public DynamoGeometryModel3D()
        {
        }

        protected override SceneNode OnCreateSceneNode()
        {
            return new DynamoMeshNode();
        }

        public static readonly DependencyProperty RequiresPerVertexColorationProperty =
            DependencyProperty.Register("RequiresPerVertexColoration", typeof(bool), 
                typeof(GeometryModel3D), new UIPropertyMetadata(false, RequirePerVertexColorationChanged));

        private static void RequirePerVertexColorationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((obj as GeometryModel3D).SceneNode.RenderCore as DynamoGeometryMeshCore).SetPropertyData(e);
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
    public class DynamoMeshNode : MeshNode
    {
   
        protected override RenderCore OnCreateRenderCore()
        {
            return new DynamoGeometryMeshCore();
        }

        protected override IRenderTechnique OnCreateRenderTechnique(IRenderHost host)
        {
            return host.EffectsManager[DynamoEffectsManager.DynamoMeshShaderName];
        }
    }
}
