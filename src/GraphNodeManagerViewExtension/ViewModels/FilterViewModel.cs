using Dynamo.Core;
using System.Windows.Media;

namespace Dynamo.GraphNodeManager.ViewModels
{
    public class FilterViewModel : NotificationObject
    {
        #region Private Fields
        private readonly GraphNodeManagerViewModel graphNodeManagerViewModel;
        private bool isFilterOn = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Title of the Filter
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Image icon associated with the Filter
        /// </summary>
        public ImageSource FilterImage { get; internal set; }

        /// <summary>
        /// The toggle that controls the visibility of the Filtered elements
        /// </summary>
        public bool IsFilterOn
        {
            get => isFilterOn;
            internal set
            {
                if (isFilterOn == value) return;
                isFilterOn = value;
                RaisePropertyChanged(nameof(IsFilterOn));
            }
        }
        #endregion

        public FilterViewModel(GraphNodeManagerViewModel vm)
        {
            this.graphNodeManagerViewModel = vm;
        }

        internal void Toggle(object obj)
        {
            IsFilterOn = !IsFilterOn;
            graphNodeManagerViewModel.NodesCollectionFilter_Changed();
        }
    }
}
