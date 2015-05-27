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
                    vm.Model.OnRequestsRedraw(this, EventArgs.Empty);
                }
            }
        }

        public int MaxTessellationDivisions
        {
            get { return vm.VisualizationManager.RenderPackageFactory.TessellationParameters.MaxGridLines; }
            set
            {
                vm.VisualizationManager.RenderPackageFactory.TessellationParameters.MaxGridLines = value;
                vm.Model.OnRequestsRedraw(this, EventArgs.Empty);
            }
        }

        public VisualizationSettingsViewModel(DynamoViewModel viewModel)
        {
            vm = viewModel;
        } 
    }
}
