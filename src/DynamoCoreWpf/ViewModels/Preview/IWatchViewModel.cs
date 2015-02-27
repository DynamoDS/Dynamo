using Dynamo.Interfaces;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public interface IWatchViewModel
    {
        DelegateCommand GetBranchVisualizationCommand { get; set; }
        bool WatchIsResizable { get; set; }
        DelegateCommand CheckForLatestRenderCommand { get; set; }
        DynamoViewModel ViewModel { get;}
        IVisualizationManager VisualizationManager { get;}
    }
}
