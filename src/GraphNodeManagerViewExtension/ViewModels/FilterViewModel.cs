using Dynamo.Core;

namespace Dynamo.GraphNodeManager.ViewModels
{
    public class FilterViewModel : NotificationObject
    {
        private readonly GraphNodeManagerViewModel graphNodeManagerViewModel;
        
        public string Name { get; internal set; }

        private bool isFilterOn = false;
        public bool IsFilterOn
        {
            get
            {
                return isFilterOn;
            }
            internal set
            {
                isFilterOn = value;
                RaisePropertyChanged(nameof(IsFilterOn));
            }
        }

        public FilterViewModel(GraphNodeManagerViewModel vm)
        {
            this.graphNodeManagerViewModel = vm;
        }

        public void Toggle(object obj)
        {
            IsFilterOn = !IsFilterOn;
            graphNodeManagerViewModel.NodesCollectionFilter_Changed();
        }
    }
}
