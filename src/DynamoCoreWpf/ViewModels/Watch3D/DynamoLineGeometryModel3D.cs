using System.Linq;
using System.Runtime.InteropServices;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A struct describing the layout of a line vertex for Dynamo.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DynamoLineVertex
    {
        public Vector4 Position;
        public Color4 Color;
        public Vector4 Parameters;
        public const int SizeInBytes = 4 * (4 + 4 + 4);
    }

    /// <summary>
    /// A Dynamo line class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoLineGeometryModel3D : LineGeometryModel3D
    {
        public override int VertexSizeInBytes
        {
            get
            {
                return DynamoLineVertex.SizeInBytes;
            }
        }

        /// <summary>
        /// Override LineGeometryModel3D's Attach method to 
        /// provide a buffer of DynamoLineVertices
        /// </summary>
        /// <param name="host"></param>
        public override void Attach(IRenderHost host)
        {
            renderTechnique = host.RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Lines];
            base.Attach(host);

            if (Geometry == null)
                return;

            if (renderHost.RenderTechnique == host.RenderTechniquesManager.RenderTechniques.Get(DeferredRenderTechniqueNames.Deferred) ||
                renderHost.RenderTechnique == host.RenderTechniquesManager.RenderTechniques.Get(DeferredRenderTechniqueNames.Deferred))
                return;

            vertexLayout = renderHost.EffectsManager.GetLayout(renderTechnique);
            effectTechnique = effect.GetTechniqueByName(renderTechnique.Name);

            effectTransforms = new EffectTransformVariables(effect);

            var geometry = Geometry as LineGeometry3D;

            if (geometry != null)
            {
                vertexBuffer = Device.CreateBuffer(BindFlags.VertexBuffer, VertexSizeInBytes, CreateVertexArray());
                indexBuffer = Device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices.ToArray());
            }

            hasInstances = (Instances != null) && (Instances.Any());
            bHasInstances = effect.GetVariableByName("bHasInstances").AsScalar();
            if (hasInstances)
            {
                instanceBuffer = Buffer.Create(Device, instanceArray, new BufferDescription(Matrix.SizeInBytes * instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));
            }

            vViewport = effect.GetVariableByName("vViewport").AsVector();
            vLineParams = effect.GetVariableByName("vLineParams").AsVector();

            var lineParams = new Vector4((float)Thickness, (float)Smoothness, 0, 0);
            vLineParams.Set(lineParams);

            OnRasterStateChanged(DepthBias);

            Device.ImmediateContext.Flush();
        }

        private DynamoLineVertex[] CreateVertexArray()
        {
            var positions = Geometry.Positions.ToArray();
            var vertexCount = Geometry.Positions.Count;
            var color = Color;
            var result = new DynamoLineVertex[vertexCount];
            var colors = Geometry.Colors;

            for (var i = 0; i < vertexCount; i++)
            {
                Color4 finalColor;
                if (colors != null && colors.Any())
                {
                    finalColor = color * colors[i];
                }
                else
                {
                    finalColor = color;
                }

                var isSelected = (bool)GetValue(AttachedProperties.ShowSelectedProperty);
                var isIsolationMode = (bool)GetValue(AttachedProperties.IsolationModeProperty);
                var isSpecialPackage = (bool)GetValue(AttachedProperties.IsSpecialRenderPackageProperty);

                result[i] = new DynamoLineVertex
                {
                    Position = new Vector4(positions[i], 1f),
                    Color = finalColor,
                    Parameters = new Vector4(isSelected ? 1 : 0,
                                            (isIsolationMode && !isSpecialPackage) ? 1 : 0, 0, 0)
                };
            }

            return result;
        }

    }
}
