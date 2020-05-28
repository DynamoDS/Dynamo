using System.Windows;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Wpf.SharpDX.Render;
using HelixToolkit.Wpf.SharpDX.Shaders;
using SharpDX;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    internal class DynamoPointLineMaterialVariable : MaterialVariable
    {
        private readonly DynamoPointLineMaterialCore material;
        public ShaderPass PointPass { get; }

        public ShaderPass ShadowPass { get; }

        public ShaderPass DepthPass { get; }

        public DynamoPointLineMaterialVariable(IEffectsManager manager, IRenderTechnique technique, ConstantBufferDescription pointMaterialConstantBufferDesc,
            DynamoPointLineMaterialCore materialCore, string pointPassName = "Default", string shadowPassName = "RenderShadow",
            string depthPassName = "DepthPrepass") : base(manager, technique, pointMaterialConstantBufferDesc, materialCore)
        {
            PointPass = technique[pointPassName];
            ShadowPass = technique[shadowPassName];
            DepthPass = technique[depthPassName];
            material = materialCore;
        }

        protected override void OnInitialPropertyBindings()
        {
            AddPropertyBinding("IsFrozen",
                () => WriteValue("pfParams",
                    new Vector4(material.IsFrozen ? 1 : 0, material.IsSelected ? 1 : 0,
                        material.IsIsolated ? 1 : 0, material.IsSpecialRenderPackage ? 1 : 0)));
            AddPropertyBinding("IsSelected",
                () => WriteValue("pfParams",
                    new Vector4(material.IsFrozen ? 1 : 0, material.IsSelected ? 1 : 0,
                        material.IsIsolated ? 1 : 0, material.IsSpecialRenderPackage ? 1 : 0)));
            AddPropertyBinding("IsIsolated",
                () => WriteValue("pfParams",
                    new Vector4(material.IsFrozen ? 1 : 0, material.IsSelected ? 1 : 0,
                        material.IsIsolated ? 1 : 0, material.IsSpecialRenderPackage ? 1 : 0)));
            AddPropertyBinding("IsSpecialRenderPackage",
                () => WriteValue("pfParams",
                    new Vector4(material.IsFrozen ? 1 : 0, material.IsSelected ? 1 : 0,
                        material.IsIsolated ? 1 : 0, material.IsSpecialRenderPackage ? 1 : 0)));
        }

        public override bool BindMaterialResources(RenderContext context, DeviceContextProxy deviceContext, ShaderPass shaderPass)
        {
            return true;
        }

        public override ShaderPass GetPass(RenderType renderType, RenderContext context)
        {
            return PointPass;
        }

        public override ShaderPass GetShadowPass(RenderType renderType, RenderContext context)
        {
            return ShadowPass;
        }

        public override ShaderPass GetWireframePass(RenderType renderType, RenderContext context)
        {
            return ShaderPass.NullPass;
        }

        public override ShaderPass GetDepthPass(RenderType renderType, RenderContext context)
        {
            return DepthPass;
        }

        public override void Draw(DeviceContextProxy deviceContext, IAttachableBufferModel bufferModel, int instanceCount)
        {
            DrawPoints(deviceContext, bufferModel.VertexBuffer[0].ElementCount, instanceCount);
        }

        internal void SetPropertyData(DependencyPropertyChangedEventArgs args)
        {
            var depType = args.Property;
            var argval = (bool)args.NewValue;

            //use dependencyProperty to determine which data to set.
            if (depType == AttachedProperties.ShowSelectedProperty)
            {
                material.IsSelected = argval;
            }

            else if (depType == AttachedProperties.IsFrozenProperty)
            {
                material.IsFrozen = argval;
            }

            else if (depType == AttachedProperties.IsolationModeProperty)
            {
                material.IsIsolated = argval;
            }

            else if (depType == AttachedProperties.IsSpecialRenderPackageProperty)
            {
                material.IsSpecialRenderPackage = argval;
            }
        }
    }

    internal class DynamoPointLineMaterialCore : MaterialCore
    {
        private bool isFrozen;
        private bool isSelected;
        private bool isIsolated;
        private bool isSpecialRenderPackage;

        public bool IsFrozen
        {
            set
            {
                Set(ref isFrozen, value, nameof(IsFrozen));
            }
            get
            {
                return isFrozen;
            }
        }

        public bool IsSelected
        {
            set
            {
                Set(ref isSelected, value, nameof(IsSelected));
            }
            get
            {
                return isSelected;
            }
        }

        public bool IsIsolated
        {
            set
            {
                Set(ref isIsolated, value, nameof(IsIsolated));
            }
            get
            {
                return isIsolated;
            }
        }

        public bool IsSpecialRenderPackage
        {
            set
            {
                Set(ref isSpecialRenderPackage, value, nameof(IsSpecialRenderPackage));
            }
            get
            {
                return isSpecialRenderPackage;
            }
        }

        public override MaterialVariable CreateMaterialVariables(IEffectsManager manager, IRenderTechnique technique)
        {
            return new DynamoPointLineMaterialVariable(manager, technique, MaterialVariable.DefaultPointLineConstantBufferDesc, this);
        }
    }

    internal class DynamoPointLineCore : PointLineRenderCore
    {
    }
}
