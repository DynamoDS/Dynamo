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
                        new ShaderPassDescription(DefaultPassNames.MeshOutline)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshDefault,
                            DefaultPSShaderDescriptions.PSMeshXRay
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSOverlayBlending,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSLessEqualNoWrite
                    }, new ShaderPassDescription(DefaultPassNames.EffectOutlineP1)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshWireframe,
                            DefaultPSShaderDescriptions.PSMeshOutlineQuadStencil
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSSourceAlways,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSMeshOutlineP1,
                        StencilRef = 1
                    },
                    new ShaderPassDescription(DefaultPassNames.EffectMeshXRayP1)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshWireframe,
                            DefaultPSShaderDescriptions.PSDepthStencilOnly
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.NoBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSEffectMeshXRayP1,
                    },
                    new ShaderPassDescription(DefaultPassNames.EffectMeshXRayP2)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshDefault,
                            DefaultPSShaderDescriptions.PSEffectMeshXRay
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSOverlayBlending,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSEffectMeshXRayP2,
                        StencilRef = 1
                    },
                    new ShaderPassDescription(DefaultPassNames.EffectMeshXRayGridP1)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshWireframe,
                            DefaultPSShaderDescriptions.PSDepthStencilOnly
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.NoBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSEffectMeshXRayGridP1,
                        StencilRef = 1
                    },
                    new ShaderPassDescription(DefaultPassNames.EffectMeshXRayGridP2)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshWireframe,
                            DefaultPSShaderDescriptions.PSDepthStencilOnly
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.NoBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSEffectMeshXRayGridP2,
                        StencilRef = 1
                    },
                    new ShaderPassDescription(DefaultPassNames.EffectMeshXRayGridP3)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshDefault,
                            DefaultPSShaderDescriptions.PSEffectXRayGrid
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSEffectMeshXRayGridP3,
                        StencilRef = 1
                    },
                    new ShaderPassDescription(DefaultPassNames.EffectMeshDiffuseXRayGridP3)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshDefault,
                            DefaultPSShaderDescriptions.PSEffectDiffuseXRayGrid
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSEffectMeshXRayGridP3,
                        StencilRef = 1
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
