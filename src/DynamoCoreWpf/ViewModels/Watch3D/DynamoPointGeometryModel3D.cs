using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PointsVertex
    {
        public Vector4 Position;
        public Color4 Color;
        public const int SizeInBytes = 4 * (4 + 4);
    }

    public class DynamoPointGeometryModel3D : PointGeometryModel3D
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        public override void Attach(IRenderHost host)
        {
            renderTechnique = Techniques.RenderPoints;
            base.Attach(host);

            if (this.Geometry == null)
                return;

#if DEFERRED
            if (renderHost.RenderTechnique == Techniques.RenderDeferred || renderHost.RenderTechnique == Techniques.RenderGBuffer)
                return;
#endif

            // --- get device
            vertexLayout = EffectsManager.Instance.GetLayout(renderTechnique);
            effectTechnique = effect.GetTechniqueByName(renderTechnique.Name);

            effectTransforms = new EffectTransformVariables(this.effect);

            // --- get geometry
            var geometry = this.Geometry as PointGeometry3D;

            // -- set geometry if given
            if (geometry != null)
            {
                /// --- set up buffers            
                this.vertexBuffer = Device.CreateBuffer(BindFlags.VertexBuffer, Geometry3D.PointsVertex.SizeInBytes, this.CreatePointVertexArray());
            }

            /// --- set up const variables
            vViewport = effect.GetVariableByName("vViewport").AsVector();
            //this.vFrustum = effect.GetVariableByName("vFrustum").AsVector();
            vPointParams = effect.GetVariableByName("vPointParams").AsVector();

            /// --- set effect per object const vars
            var pointParams = new Vector4((float)this.Size.Width, (float)this.Size.Height, (float)this.Figure, (float)this.FigureRatio);
            vPointParams.Set(pointParams);

            /// --- create raster state
            OnRasterStateChanged(this.DepthBias);

            /// --- flush
            Device.ImmediateContext.Flush();
        }

        /// <summary>
        /// Creates a <see cref="T:PointsVertex[]"/>.
        /// </summary>
        private Geometry3D.PointsVertex[] CreatePointVertexArray()
        {
            var positions = this.Geometry.Positions.Array;
            var vertexCount = this.Geometry.Positions.Count;
            var color = this.Color;
            var result = new Geometry3D.PointsVertex[vertexCount];

            if (this.Geometry.Colors != null && this.Geometry.Colors.Any())
            {
                var colors = this.Geometry.Colors;
                for (var i = 0; i < vertexCount; i++)
                {
                    result[i] = new Geometry3D.PointsVertex
                    {
                        Position = new Vector4(positions[i], 1f),
                        Color = color * colors[i],
                    };
                }
            }
            else
            {
                for (var i = 0; i < vertexCount; i++)
                {
                    result[i] = new Geometry3D.PointsVertex
                    {
                        Position = new Vector4(positions[i], 1f),
                        Color = color,
                    };
                }
            }

            return result;
        }
    }
}
