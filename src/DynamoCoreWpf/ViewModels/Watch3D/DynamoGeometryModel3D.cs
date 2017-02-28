﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A struct describing the layout of a mesh vertex for Dynamo.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DynamoMeshVertex
    {
        public Vector4 Position;
        public Color4 Color;
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;

        /// <summary>
        /// Store a number of parameters for Dynamo in a Vector4.
        /// 0 - IsSelected
        /// 1 - RequiresPerVertexColoration
        /// </summary>
        public Vector4 Parameters;

        public const int SizeInBytes = 4 * (4 + 4 + 2 + 3 + 3 + 3 + 4);
    }

    /// <summary>
    /// A Dynamo mesh class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoGeometryModel3D : MaterialGeometryModel3D
    {
        private readonly RenderTechnique renderTechniqueInternal;

        public DynamoGeometryModel3D(RenderTechnique renderTechnique)
        {
            renderTechniqueInternal = renderTechnique;
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

        public override int VertexSizeInBytes
        {
            get
            {
                return DynamoMeshVertex.SizeInBytes;
            }
        }

        protected override void OnRasterStateChanged(int depthBias)
        {
            if (!IsAttached) return;

            Disposer.RemoveAndDispose(ref rasterState);
            /// --- set up rasterizer states
            var rasterStateDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                DepthBias = depthBias,
                DepthBiasClamp = -1000,
                SlopeScaledDepthBias = +0,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = true,

                //IsMultisampleEnabled = true,
                //IsAntialiasedLineEnabled = true,                    
                //IsScissorEnabled = true,
            };

            try
            {
                rasterState = new RasterizerState(Device, rasterStateDesc);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Override MaterialGeometryModel3D's Attach method to 
        /// provide a buffer of DynamoMeshVertices
        /// </summary>
        public override void Attach(IRenderHost host)
        {
            base.Attach(host);

            renderTechnique = renderTechniqueInternal;

            if (Geometry == null)
                return;

            vertexLayout = host.EffectsManager.GetLayout(renderTechnique);
            effectTechnique = effect.GetTechniqueByName(renderTechnique.Name);

            effectTransforms = new EffectTransformVariables(effect);

            AttachMaterial();

            var geometry = Geometry as MeshGeometry3D;

            if (geometry == null)
            {
                throw new Exception("Geometry must not be null");
            }

            vertexBuffer = Device.CreateBuffer(BindFlags.VertexBuffer, VertexSizeInBytes, CreateVertexArray());
            indexBuffer = Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int),Geometry.Indices.ToArray());

            hasInstances = (Instances != null) && (Instances.Any());
            bHasInstances = effect.GetVariableByName("bHasInstances").AsScalar();
            if (hasInstances)
            {
                instanceBuffer = Buffer.Create(Device, instanceArray, new BufferDescription(Matrix.SizeInBytes * instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));
            }

            OnRasterStateChanged(DepthBias);

            Device.ImmediateContext.Flush();
        }

        public override void Detach()
        {
            Disposer.RemoveAndDispose(ref vertexBuffer);
            Disposer.RemoveAndDispose(ref indexBuffer);
            Disposer.RemoveAndDispose(ref instanceBuffer);
            Disposer.RemoveAndDispose(ref effectMaterial);
            Disposer.RemoveAndDispose(ref effectTransforms);
            Disposer.RemoveAndDispose(ref texDiffuseMapView);
            Disposer.RemoveAndDispose(ref texNormalMapView);
            Disposer.RemoveAndDispose(ref bHasInstances);
            Disposer.RemoveAndDispose(ref rasterState);

            renderTechnique = null;
            phongMaterial = null;
            effectTechnique = null;
            vertexLayout = null;

            base.Detach();
        }

        public override void Render(RenderContext renderContext)
        {
            if (!IsRendering)
            {
                return;
            }

            if (Geometry == null)
            {
                return;
            }

            if (Visibility != Visibility.Visible)
            {
                return;
            }

            if (renderContext.IsShadowPass)
            {
                if (!IsThrowingShadow)
                {
                    return;
                }
            }

            /// --- set constant paramerers             
            var worldMatrix = modelMatrix * renderContext.WorldMatrix;
            effectTransforms.mWorld.SetMatrix(ref worldMatrix);

            /// --- check shadowmaps
            hasShadowMap = renderHost.IsShadowMapEnabled;
            effectMaterial.bHasShadowMapVariable.Set(hasShadowMap);

            /// --- set material params      
            if (phongMaterial != null)
            {
                effectMaterial.vMaterialDiffuseVariable.Set(phongMaterial.DiffuseColor);
                effectMaterial.vMaterialAmbientVariable.Set(phongMaterial.AmbientColor);
                effectMaterial.vMaterialEmissiveVariable.Set(phongMaterial.EmissiveColor);
                effectMaterial.vMaterialSpecularVariable.Set(phongMaterial.SpecularColor);
                effectMaterial.vMaterialReflectVariable.Set(phongMaterial.ReflectiveColor);
                effectMaterial.sMaterialShininessVariable.Set(phongMaterial.SpecularShininess);

                /// --- has samples              
                effectMaterial.bHasDiffuseMapVariable.Set(phongMaterial.DiffuseMap != null);
                effectMaterial.bHasNormalMapVariable.Set(phongMaterial.NormalMap != null);

                /// --- set samplers
                if (phongMaterial.DiffuseMap != null)
                {
                    effectMaterial.texDiffuseMapVariable.SetResource(texDiffuseMapView);
                }

                if (phongMaterial.NormalMap != null)
                {
                    effectMaterial.texNormalMapVariable.SetResource(texNormalMapView);
                }
            }

            /// --- check instancing
            hasInstances = (Instances != null) && (Instances.Any());
            if (bHasInstances != null)
            {
                bHasInstances.Set(hasInstances);
            }

            /// --- set context
            Device.ImmediateContext.InputAssembler.InputLayout = vertexLayout;
            Device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            /// --- set rasterstate            
            Device.ImmediateContext.Rasterizer.State = rasterState;

            if (hasInstances)
            {
                /// --- update instance buffer
                if (isChanged)
                {
                    instanceBuffer = Buffer.Create(Device, instanceArray, new BufferDescription(Matrix.SizeInBytes * instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));
                    DataStream stream;
                    Device.ImmediateContext.MapSubresource(instanceBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                    stream.Position = 0;
                    stream.WriteRange(instanceArray, 0, instanceArray.Length);
                    Device.ImmediateContext.UnmapSubresource(instanceBuffer, 0);
                    stream.Dispose();
                    isChanged = false;
                }

                /// --- INSTANCING: need to set 2 buffers            
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new[] 
                {
                    new VertexBufferBinding(vertexBuffer, DynamoMeshVertex.SizeInBytes, 0),
                    new VertexBufferBinding(instanceBuffer, Matrix.SizeInBytes, 0),
                });

                /// --- render the geometry
                effectTechnique.GetPassByIndex(0).Apply(Device.ImmediateContext);
                /// --- draw
                Device.ImmediateContext.DrawIndexedInstanced(Geometry.Indices.Count, instanceArray.Length, 0, 0, 0);
            }
            else
            {
                /// --- bind buffer                
                Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, DynamoMeshVertex.SizeInBytes, 0));
                /// --- render the geometry
                effectTechnique.GetPassByIndex(0).Apply(Device.ImmediateContext);
                /// --- draw
                Device.ImmediateContext.DrawIndexed(Geometry.Indices.Count, 0, 0);
            }
        }

        public override void Dispose()
        {
            Detach();
        }

        private DynamoMeshVertex[] CreateVertexArray()
        {
            var geometry = (MeshGeometry3D)Geometry;
            var colors = geometry.Colors != null ? geometry.Colors.ToArray() : null;
            var textureCoordinates = geometry.TextureCoordinates != null ? geometry.TextureCoordinates.ToArray() : null;
            var texScale = TextureCoodScale;
            var normals = geometry.Normals != null ? geometry.Normals.ToArray() : null;
            var tangents = geometry.Tangents != null ? geometry.Tangents.ToArray() : null;
            var bitangents = geometry.BiTangents != null ? geometry.BiTangents.ToArray() : null;
            var positions = geometry.Positions.ToArray();
            var vertexCount = geometry.Positions.Count;
            var result = new DynamoMeshVertex[vertexCount];

            var isSelected = (bool)GetValue(AttachedProperties.ShowSelectedProperty);
            var isIsolationMode = (bool)GetValue(AttachedProperties.IsolationModeProperty);
            var isSpecialPackage = (bool)GetValue(AttachedProperties.IsSpecialRenderPackageProperty);

            for (var i = 0; i < vertexCount; i++)
            {
                result[i] = new DynamoMeshVertex
                {
                    Position = new Vector4(positions[i], 1f),
                    Color = colors != null ? colors[i] : Color4.White,
                    TexCoord = textureCoordinates != null ? texScale * textureCoordinates[i] : Vector2.Zero,
                    Normal = normals != null ? normals[i] : Vector3.Zero,
                    Tangent = tangents != null ? tangents[i] : Vector3.Zero,
                    BiTangent = bitangents != null ? bitangents[i] : Vector3.Zero,
                    Parameters = new Vector4(isSelected ? 1 : 0,
                                            (isIsolationMode && !isSpecialPackage) ? 1 : 0,
                                             RequiresPerVertexColoration ? 1 : 0, 0)
                };
            }

            return result;
        }
    }
}
