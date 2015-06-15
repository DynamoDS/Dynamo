using System;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels
{
    public class VisualizationSettingsViewModel
    {
        private readonly DynamoViewModel vm;

        public bool ShowEdges
        {
            get { return vm.Model.PreferenceSettings.ShowEdges; }
            set
            {
                if (vm.Model.PreferenceSettings.ShowEdges != value)
                {
                    vm.Model.PreferenceSettings.ShowEdges = value;
                    vm.VisualizationManager.RenderPackageFactory.TessellationParameters.ShowEdges = value;
                    vm.VisualizationManager.UpdateAllNodeVisualsAndNotify();
                }
            }
        }

        public int MaxTessellationDivisions
        {
            get { return vm.VisualizationManager.RenderPackageFactory.TessellationParameters.MaxTessellationDivisions; }
            set
            {
                vm.VisualizationManager.RenderPackageFactory.TessellationParameters.MaxTessellationDivisions = value;
                vm.VisualizationManager.UpdateAllNodeVisualsAndNotify();
            }
        }

        public VisualizationSettingsViewModel(DynamoViewModel viewModel)
        {
            vm = viewModel;
        }
    }
}
