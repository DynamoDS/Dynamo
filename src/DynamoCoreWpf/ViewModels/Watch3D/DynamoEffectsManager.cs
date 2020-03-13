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


    /// <summary>
    /// The DynamoEffectsManager is loads shaders
    /// from shader byte code, and defines data layouts for rendering. 
    /// By extending the DefaultEffectsManager, the DynamoEffectsManager 
    /// makes available effects like Blinn rendering, and adds custom
    /// Dynamo rendering shaders for meshes.
    /// For an intro to dx vertex and fragment shaders see 
    /// https://docs.microsoft.com/en-us/windows/win32/direct3dgetstarted/work-with-shaders-and-shader-resources
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
                    return Dynamo.Wpf.Properties.Resources.vsDynamoMesh;
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
                    return Dynamo.Wpf.Properties.Resources.psDynamoMesh;
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
