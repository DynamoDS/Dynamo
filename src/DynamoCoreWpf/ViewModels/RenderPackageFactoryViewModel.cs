using Dynamo.Interfaces;
using Dynamo.ViewModels;
using Dynamo.Visualization;
using Dynamo.Configuration;
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
                if (value >= 8 && value <= 12)
                    factory.TessellationParameters.Tolerance = 0;
                else
                    factory.TessellationParameters.Tolerance = -1;

                RaisePropertyChanged("MaxTessellationDivisions");
            }
        }

        // TODO: Simplify this constructor once IPreferences and IRenderPrecisionPreference have been consolidated in 3.0 (DYN-1699)
        public RenderPackageFactoryViewModel(IPreferences preferenceSettings)
        {
            var ps = preferenceSettings as PreferenceSettings;
            if (ps != null)
            {
                this.factory = new HelixRenderPackageFactory()
                {
                    TessellationParameters = { ShowEdges = ps.ShowEdges, MaxTessellationDivisions = ps.RenderPrecision }
                };
            }
            else
            {
                this.factory = new HelixRenderPackageFactory()
                {
                    TessellationParameters = { ShowEdges = preferenceSettings.ShowEdges}
                };
            }
            
        }
    }
}
