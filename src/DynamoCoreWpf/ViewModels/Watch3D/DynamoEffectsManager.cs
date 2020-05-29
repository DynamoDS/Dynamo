using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Shaders;

namespace Dynamo.Wpf.ViewModels.Watch3D
{

    /// <summary>
    /// The DynamoEffectsManager loads shaders
    /// from shader byte code, and defines data layouts for rendering. 
    /// By extending the DefaultEffectsManager, the DynamoEffectsManager 
    /// makes available effects like Blinn rendering, and adds custom
    /// Dynamo rendering shaders for meshes.
    /// For an intro to dx vertex and fragment shaders see 
    /// https://docs.microsoft.com/en-us/windows/win32/direct3dgetstarted/work-with-shaders-and-shader-resources
    /// </summary>
    public class DynamoEffectsManager : DefaultEffectsManager
    {
        internal static readonly string DynamoMeshShaderName = "DynamoMeshShader";
        internal static readonly string DynamoPointLineShaderName = "DynamoPointLineShader";

        public DynamoEffectsManager()
        {
            AddDynamoTechniques();
        }

        internal static class DynamoMeshRenderVertexShaderDescription
        {
            public static byte[] VSMeshDataSamplerByteCode
            {
                get
                {
                    return Properties.Resources.vsDynamoMesh;
                }
            }

            public static readonly ShaderDescription  VertexShaderDynamoMeshDescription = new ShaderDescription(nameof(VertexShaderDynamoMeshDescription), ShaderStage.Vertex,
          new ShaderReflector(), VSMeshDataSamplerByteCode);
        }

        internal static class DynamoMeshRenderPixelShaderDescription
        {
            public static byte[] PSMeshDataSamplerByteCode
            {
                get
                {
                    return Properties.Resources.psDynamoMesh;
                }
            }

            public static readonly ShaderDescription PixelShaderDynamoMeshDescription = new ShaderDescription(nameof(PixelShaderDynamoMeshDescription), ShaderStage.Pixel,
          new ShaderReflector(), PSMeshDataSamplerByteCode);
        }

        internal static class DynamoPointLineVertexShaderDescription
        {
            internal static byte[] VSPointLineDataSamplerByteCode
            {
                get { return Properties.Resources.vsDynamoPointLine; }
            }

            internal static readonly ShaderDescription VertexShaderDynamoPointLineDescription = new ShaderDescription(nameof(VertexShaderDynamoPointLineDescription), 
                    ShaderStage.Vertex, new ShaderReflector(), VSPointLineDataSamplerByteCode);
        }

        internal static class DynamoPointLinePixelShaderDescription
        {
            internal static byte[] PSPointLineDataSamplerByteCode
            {
                get { return Properties.Resources.psDynamoPointLine; }
            }

            internal static readonly ShaderDescription PixelShaderDynamoPointLineDescription = new ShaderDescription(nameof(PixelShaderDynamoPointLineDescription),
                ShaderStage.Pixel, new ShaderReflector(), PSPointLineDataSamplerByteCode);
        }

        protected void AddDynamoTechniques()
        {
            var dynamoCustomMeshTech = new TechniqueDescription(DynamoMeshShaderName)
            {

                InputLayoutDescription = new InputLayoutDescription(DynamoMeshRenderVertexShaderDescription.VSMeshDataSamplerByteCode, DefaultInputLayout.VSInput),
                PassDescriptions = new[]
                {
                    new ShaderPassDescription(DefaultPassNames.Default)
                    {
                        ShaderList = new[]
                        {
                            DynamoMeshRenderVertexShaderDescription.VertexShaderDynamoMeshDescription,
                            DynamoMeshRenderPixelShaderDescription.PixelShaderDynamoMeshDescription,
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    },
                }
            };
            AddTechnique(dynamoCustomMeshTech);

            var dynamoCustomPointLineTech = new TechniqueDescription(DynamoPointLineShaderName)
            {
                InputLayoutDescription = new InputLayoutDescription(
                    DynamoPointLineVertexShaderDescription.VSPointLineDataSamplerByteCode,
                    DefaultInputLayout.VSInputPoint),
                PassDescriptions = new[]
                {
                    new ShaderPassDescription(DefaultPassNames.Default)
                    {
                        ShaderList = new[]
                        {     
                            DynamoPointLineVertexShaderDescription.VertexShaderDynamoPointLineDescription,
                            DefaultGSShaderDescriptions.GSPoint,
                            DynamoPointLinePixelShaderDescription.PixelShaderDynamoPointLineDescription
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    }
                }
            };
            AddTechnique(dynamoCustomPointLineTech);
        }
    }
}
