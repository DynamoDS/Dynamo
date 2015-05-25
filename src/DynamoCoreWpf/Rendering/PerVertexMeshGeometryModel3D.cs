using System;
using System.Linq;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Dynamo.Wpf.Rendering
{
    public class PerVertexMeshGeometryModel3D : MeshGeometryModel3D
    {
        public override void Attach(IRenderHost host)
        {
            base.Attach(host);

            this.renderTechnique = Techniques.RenderPerVertexPhong;

            if (this.Geometry == null)
                return;

            this.vertexLayout = EffectsManager.Instance.GetLayout(this.renderTechnique);
            this.effectTechnique = effect.GetTechniqueByName(this.renderTechnique.Name);

            this.effectTransforms = new EffectTransformVariables(this.effect);

            this.AttachMaterial();

            var geometry = this.Geometry as MeshGeometry3D;

            if (geometry == null)
            {
                throw new Exception("Geometry must not be null");
            }

            this.vertexBuffer = Device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes,
                this.CreateDefaultVertexArray());
            this.indexBuffer = Device.CreateBuffer(BindFlags.IndexBuffer, sizeof (int),
                this.Geometry.Indices.ToArray());
            
            this.hasInstances = (this.Instances != null) && (this.Instances.Any());
            this.bHasInstances = this.effect.GetVariableByName("bHasInstances").AsScalar();
            if (this.hasInstances)
            {
                this.instanceBuffer = Buffer.Create(this.Device, this.instanceArray, new BufferDescription(Matrix.SizeInBytes * this.instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));
            }

            this.OnRasterStateChanged(this.DepthBias);

            this.Device.ImmediateContext.Flush();
        }

        private DefaultVertex[] CreateDefaultVertexArray()
        {
            var geometry = (MeshGeometry3D)this.Geometry;
            var colors = geometry.Colors != null ? geometry.Colors.ToArray() : null;
            var textureCoordinates = geometry.TextureCoordinates != null ? geometry.TextureCoordinates.ToArray() : null;
            var texScale = this.TextureCoodScale;
            var normals = geometry.Normals != null ? geometry.Normals.ToArray() : null;
            var tangents = geometry.Tangents != null ? geometry.Tangents.ToArray() : null;
            var bitangents = geometry.BiTangents != null ? geometry.BiTangents.ToArray() : null;
            var positions = geometry.Positions.ToArray();
            var vertexCount = geometry.Positions.Count;
            var result = new DefaultVertex[vertexCount];

            for (var i = 0; i < vertexCount; i++)
            {
                result[i] = new DefaultVertex
                {
                    Position = new Vector4(positions[i], 1f),
                    Color = colors != null ? colors[i] : Color4.White,
                    TexCoord = textureCoordinates != null ? texScale * textureCoordinates[i] : Vector2.Zero,
                    Normal = normals != null ? normals[i] : Vector3.Zero,
                    Tangent = tangents != null ? tangents[i] : Vector3.Zero,
                    BiTangent = bitangents != null ? bitangents[i] : Vector3.Zero,
                };
            }

            return result;
        }
    }
}
