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

        public DynamoEffectsManager() : base() {
            AddDynamoTechniques();
        }

        internal class DynamoMeshRenderVertexShaderDescription
        {
            protected DynamoMeshRenderVertexShaderDescription()
            {
                // Do nothing for now
            }

            public static byte[] VSMeshDataSamplerByteCode
            {
                get
                {
                    return Dynamo.Wpf.Properties.Resources.vsDynamoMesh;
                }
            }

            public static readonly ShaderDescription  VertexShaderDynamoMeshDescription = new ShaderDescription(nameof(VertexShaderDynamoMeshDescription), ShaderStage.Vertex,
          new ShaderReflector(), VSMeshDataSamplerByteCode);
        }

        internal class DynamoMeshRenderPixelShaderDescription
        {
            protected DynamoMeshRenderPixelShaderDescription()
            {
                // Do nothing for now
            }

            public static byte[] PSMeshDataSamplerByteCode
            {
                get
                {
                    return Dynamo.Wpf.Properties.Resources.psDynamoMesh;
                }
            }

            public static readonly ShaderDescription PixelShaderDynamoMeshDescription = new ShaderDescription(nameof(PixelShaderDynamoMeshDescription), ShaderStage.Pixel,
          new ShaderReflector(), PSMeshDataSamplerByteCode);
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
                        ShaderList = new ShaderDescription[]
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
        }
    }
}
