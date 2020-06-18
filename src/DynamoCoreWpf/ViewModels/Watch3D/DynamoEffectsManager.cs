using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Shaders;
using System;

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
    [Obsolete("Do not use! This will be moved to a new project in a future version of Dynamo.")]
    public class DynamoEffectsManager : DefaultEffectsManager
    {
        internal static readonly string DynamoMeshShaderName = "DynamoMeshShader";
        internal static readonly string DynamoPointShaderName = "DynamoPointShader";
        internal static readonly string DynamoLineShaderName = "DynamoLineShader";

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

        internal static class DynamoPointPixelShaderDescription
        {
            internal static byte[] PSPointDataSamplerByteCode
            {
                get { return Properties.Resources.psDynamoPoint; }
            }

            internal static readonly ShaderDescription PixelShaderDynamoPointDescription = new ShaderDescription(nameof(PixelShaderDynamoPointDescription),
                ShaderStage.Pixel, new ShaderReflector(), PSPointDataSamplerByteCode);
        }

        internal static class DynamoLinePixelShaderDescription
        {
            internal static byte[] PSLineDataSamplerByteCode
            {
                get { return Properties.Resources.psDynamoLine; }
            }

            internal static readonly ShaderDescription PixelShaderDynamoLineDescription = new ShaderDescription(nameof(PixelShaderDynamoLineDescription),
                ShaderStage.Pixel, new ShaderReflector(), PSLineDataSamplerByteCode);
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

            var dynamoCustomPointTech = new TechniqueDescription(DynamoPointShaderName)
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
                            DynamoPointPixelShaderDescription.PixelShaderDynamoPointDescription
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    }
                }
            };
            AddTechnique(dynamoCustomPointTech);

            var dynamoCustomLineTech = new TechniqueDescription(DynamoLineShaderName)
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
                            DefaultGSShaderDescriptions.GSLine,
                            DynamoLinePixelShaderDescription.PixelShaderDynamoLineDescription
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    }
                }
            };
            AddTechnique(dynamoCustomLineTech);
        }
    }
}
