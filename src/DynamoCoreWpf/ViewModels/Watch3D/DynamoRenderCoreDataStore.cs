using System;
using System.Windows;

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
    internal class DynamoRenderCoreDataStore {
    
        public delegate bool FuncRef<T>(ref T item, T val);
        readonly FuncRef<bool> updateAction;
        public DynamoRenderCoreDataStore(FuncRef<bool> onUpdateDataAction)
        {
            updateAction = onUpdateDataAction;
        }

        private bool isFrozenData;

        /// <summary>
        /// Is this model Frozen.
        /// </summary>
        public bool IsFrozenData { get { return isFrozenData; } internal set { updateAction(ref isFrozenData, value); } }

        private bool isSelectedData;

        /// <summary>
        /// Is this model currently selected.
        /// </summary>
        public bool IsSelectedData { get { return isSelectedData; } internal set { updateAction(ref isSelectedData, value); } }

        private bool isIsolatedData;
        /// <summary>
        /// Is IsolationMode active.
        /// </summary>
        public bool IsIsolatedData { get { return isIsolatedData; } internal set { updateAction(ref isIsolatedData, value); } }

        private bool isSpecialData;
        /// <summary>
        /// Is this model marked as a special render package.
        /// </summary>
        public bool IsSpecialRenderPackageData { get { return isSpecialData; } internal set { updateAction(ref isSpecialData, value); } }

        private bool hasTransparencyData;
        /// <summary>
        /// Does this model have alpha less than 255.
        /// </summary>
        public bool HasTransparencyData { get { return hasTransparencyData; } internal set { updateAction(ref hasTransparencyData, value); } }

        private bool requiresPerVertexColor;
        /// <summary>
        /// Should this model display vertex colors.
        /// </summary>
        public bool RequiresPerVertexColor { get { return requiresPerVertexColor; } internal set { updateAction(ref requiresPerVertexColor, value); } }

        private bool isFlatShaded;
        /// <summary>
        /// Should this model disregard lighting calculations and display unlit texture or vertex colors.
        /// </summary>
        public bool IsFlatShaded { get { return isFlatShaded; } internal set { updateAction(ref isFlatShaded, value); } }

        /// <summary>
        /// Generates an int that packs all enum flags into a single int.
        /// Can be decoded using binary &amp;
        /// ie - Flags &amp; 1 = IsFrozen
        ///  - if flags == 000000 - all flags are off
        ///  - if flags == 000001 - frozen is enabled
        ///  - if flags == 100001 - frozen and flatshade are enabled.
        /// </summary>
        /// <returns></returns>
        public int GenerateEnumFromState()
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
        internal void SetPropertyData(DependencyPropertyChangedEventArgs args)
        {
            var depType = args.Property;
            var argval = (bool)args.NewValue;
            //use dependencyProperty to determine which data to set.
            if (depType == AttachedProperties.ShowSelectedProperty)
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


}
