using HelixToolkit.Wpf.SharpDX;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    /// <summary>
    /// The DynamoRenderTechniquesManager maintains a dictionary of
    /// available render techniques. A render technique is a single rendering function 
    /// inside an Effect, containing one or more render passes. The shaders 
    /// containing the render techniques are available in the \shaders folder.
    /// For more information on DirectX Effects and techniques see
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ff476136(v=vs.85).aspx
    /// </summary>
    public class DynamoRenderTechniquesManager : DefaultRenderTechniquesManager
    {
        protected override void InitTechniques()
        {
            AddRenderTechnique(DefaultRenderTechniqueNames.Blinn, Properties.Resources._dynamo);
            AddRenderTechnique(DefaultRenderTechniqueNames.Points, Properties.Resources._dynamo);
            AddRenderTechnique(DefaultRenderTechniqueNames.Lines, Properties.Resources._dynamo);
            AddRenderTechnique(DefaultRenderTechniqueNames.BillboardText, Properties.Resources._dynamo);
            AddRenderTechnique("RenderCustom", Properties.Resources._dynamo);
        }
    }
}
