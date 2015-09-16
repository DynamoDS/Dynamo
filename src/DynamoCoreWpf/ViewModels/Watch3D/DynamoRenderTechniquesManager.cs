using HelixToolkit.Wpf.SharpDX;

using System.Collections.Generic;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
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
