using Dynamo.Interfaces;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using Dynamo.Wpf.Rendering;

namespace Dynamo.Wpf.ViewModels
{
    public class RenderPackageFactoryViewModel : ViewModelBase
    {
        private readonly IRenderPackageFactory factory;

        public IRenderPackageFactory Factory
        {
            get { return factory; }
        }

        public bool ShowEdges
        {
            get { return factory.TessellationParameters.ShowEdges; }
            set
            {
                if (factory.TessellationParameters.ShowEdges == value) return;
                factory.TessellationParameters.ShowEdges = value;
                RaisePropertyChanged("ShowEdges");
            }
        }

        public int MaxTessellationDivisions
        {
            get { return factory.TessellationParameters.MaxTessellationDivisions; }
            set
            {
                if (factory.TessellationParameters.MaxTessellationDivisions == value) return;
                factory.TessellationParameters.MaxTessellationDivisions = value;
                RaisePropertyChanged("MaxTessellationDivisions");
            }
        }

        public RenderPackageFactoryViewModel(IPreferences preferenceSettings)
        {
            this.factory = new HelixRenderPackageFactory()
            {
                TessellationParameters = { ShowEdges = preferenceSettings.ShowEdges }
            };
        }
    }
}
