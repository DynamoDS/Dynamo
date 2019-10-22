using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Shaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
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

        /// <summary>
        /// Load effects used by Dynamo for rendering. Custom input layouts are
        /// created which specify extra COLOR components used to hold data 
        /// about the selection state and vertex coloration of objects.
        /// See <see cref="T:DynamoMeshVertex"/>, <see cref="T:DynamoPointVertex"/>, and <see cref="T:DynamoLineVertex"/>
        /// for examples of how these layouts are used.
        /// </summary>
        protected void AddDynamoTechniques()
        {
            var custom = new TechniqueDescription("RenderCustom")
            {
                InputLayoutDescription = new InputLayoutDescription(DefaultVSShaderByteCodes.VSMeshDefault, DefaultInputLayout.VSInput),
                PassDescriptions = new[]
                {
                    new ShaderPassDescription(DefaultPassNames.Default)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshDefault,
                            DefaultPSShaderDescriptions.PSMeshBlinnPhong
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    },
                    new ShaderPassDescription(DefaultPassNames.Wireframe)
                    {
                        ShaderList = new[]
                        {
                            DefaultVSShaderDescriptions.VSMeshWireframe,
                            DefaultPSShaderDescriptions.PSMeshWireframe
                        },
                        BlendStateDescription = DefaultBlendStateDescriptions.BSAlphaBlend,
                        DepthStencilStateDescription = DefaultDepthStencilDescriptions.DSSDepthLess
                    }
                }
            };

            AddTechnique(custom);
        }
    }
}
