using System.Linq;
using System.Runtime.InteropServices;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Extensions;
using SharpDX;
using SharpDX.Direct3D11;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// A struct describing the layout of a point vertex for Dynamo.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DynamoPointVertex
    {
        public Vector4 Position;
        public Color4 Color;
        public Vector4 Parameters;
        public const int SizeInBytes = 4 * (4 + 4 + 4);
    }

    /// <summary>
    /// A Dynamo point class which supports the RenderCustom technique.
    /// </summary>
    public class DynamoPointGeometryModel3D : PointGeometryModel3D
    {
        public override int VertexSizeInBytes
        {
            get
            {
                return DynamoPointVertex.SizeInBytes;
            }
        }

        /// <summary>
        /// Override PointGeometryModel3D's Attach method to 
        /// provide a buffer of DynamoPointVertices
        /// </summary>
        public override void Attach(IRenderHost host)
        {
            var techManager = host.RenderTechniquesManager;

            renderTechnique = techManager.RenderTechniques[DefaultRenderTechniqueNames.Points];
            base.Attach(host);

            if (Geometry == null)
                return;

            if (renderHost.RenderTechnique == host.RenderTechniquesManager.RenderTechniques.Get(DeferredRenderTechniqueNames.Deferred) ||
                renderHost.RenderTechnique == host.RenderTechniquesManager.RenderTechniques.Get(DeferredRenderTechniqueNames.Deferred))
                return;

            vertexLayout = host.EffectsManager.GetLayout(renderTechnique);
            effectTechnique = effect.GetTechniqueByName(renderTechnique.Name);

            effectTransforms = new EffectTransformVariables(effect);

            var geometry = Geometry as PointGeometry3D;
            if (geometry != null)
            {          
                vertexBuffer = Device.CreateBuffer(BindFlags.VertexBuffer, VertexSizeInBytes, CreateVertexArray());
            }

            vViewport = effect.GetVariableByName("vViewport").AsVector();
            vPointParams = effect.GetVariableByName("vPointParams").AsVector();

            var pointParams = new Vector4((float)Size.Width, (float)Size.Height, (float)Figure, (float)FigureRatio);
            vPointParams.Set(pointParams);

            OnRasterStateChanged(DepthBias);

            Device.ImmediateContext.Flush();
        }

        /// <summary>
        /// Creates a <see cref="T:PointsVertex[]"/>.
        /// </summary>
        private DynamoPointVertex[] CreateVertexArray()
        {
            var positions = Geometry.Positions.ToArray();
            var vertexCount = Geometry.Positions.Count;
            var color = Color;
            var result = new DynamoPointVertex[vertexCount];
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

                result[i] = new DynamoPointVertex
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
