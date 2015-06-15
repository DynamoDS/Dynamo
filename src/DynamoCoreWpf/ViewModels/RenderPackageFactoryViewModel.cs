using Dynamo.Interfaces;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels
{
    public class RenderPackageFactoryViewModel : ViewModelBase
    {
        private readonly IRenderPackageFactory factory;

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

        public RenderPackageFactoryViewModel(IRenderPackageFactory factory)
        {
            this.factory = factory;
        }
    }
}
