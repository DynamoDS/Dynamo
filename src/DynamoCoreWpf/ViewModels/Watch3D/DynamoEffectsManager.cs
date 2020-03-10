using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Shaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.IO;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    internal static class DynamoCustomShaderNames {
       internal static string DynamoCustomMeshShader ="DynamoCustomMeshShader";
    }


    //TODO update this comment
    /// <summary>
    /// The DynamoEffectsManager is loads Effects
    /// from shader byte code, and defines data layouts for rendering. 
    /// By extending the DefaultEffectsManager, the DynamoEffectsManager 
    /// makes available effects like Blinn rendering, and adds custom
    /// Dynamo rendering effects for points, lines, and meshes.
    /// For more information on DirectX Effects, see 
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ff476136(v=vs.85).aspx
    /// </summary>
    public class DynamoEffectsManager : DefaultEffectsManager
    {
        public DynamoEffectsManager() : base() {
            AddDynamoTechniques();
        }

        internal class DynamoCustomMeshRenderVertexShaderDescription
        {
            
            public static byte[] VSMeshDataSamplerByteCode
            {
                get
                {
                    return Dynamo.Wpf.Properties.Resources.vsMeshDataSampling;
                }
            }

            public static ShaderDescription  VertexShaderDynamoMeshDescription = new ShaderDescription(nameof(VertexShaderDynamoMeshDescription), ShaderStage.Vertex,
          new ShaderReflector(), VSMeshDataSamplerByteCode);
        }

        internal class DynamoCustomMeshRenderPixelShaderDescription
        {
            public static byte[] PSMeshDataSamplerByteCode
            {
                get
                {
                    return Dynamo.Wpf.Properties.Resources.psMeshDataSampling;
                }
            }

            public static ShaderDescription PixelShaderDynamoMeshDescription = new ShaderDescription(nameof(PixelShaderDynamoMeshDescription), ShaderStage.Pixel,
          new ShaderReflector(), PSMeshDataSamplerByteCode);
        }


        protected void AddDynamoTechniques()
        {
            var dynamoCustomMeshTech = new TechniqueDescription(DynamoCustomShaderNames.DynamoCustomMeshShader)
            {

                InputLayoutDescription = new InputLayoutDescription(DynamoCustomMeshRenderVertexShaderDescription.VSMeshDataSamplerByteCode, DefaultInputLayout.VSInput),
                PassDescriptions = new[]
                {
                    new ShaderPassDescription(DefaultPassNames.Default)
                    {
                        ShaderList = new ShaderDescription[]
                        {
                            DynamoCustomMeshRenderVertexShaderDescription.VertexShaderDynamoMeshDescription,
                            DynamoCustomMeshRenderPixelShaderDescription.PixelShaderDynamoMeshDescription,
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    },
                    new ShaderPassDescription(DefaultPassNames.Wireframe)
                    {
                        ShaderList = new[]
                        {
                           DynamoCustomMeshRenderVertexShaderDescription.VertexShaderDynamoMeshDescription,
                            DefaultPSShaderDescriptions.PSMeshWireframe
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    }
                }
            };

            AddTechnique(dynamoCustomMeshTech);
        }
    }
}
