

namespace Dynamo.ViewModels
{
    public class CompactBubbleViewModel : ViewModelBase
    {
        #region Properties

        private string nodeLabel;
        public string NodeLabel
        {
            get { return nodeLabel; }
            set
            {
                nodeLabel = value;
                RaisePropertyChanged("NodeLabel");
            }
        }

        private int numberOfLevels;
        public int NumberOfLevels
        {
            get { return numberOfLevels; }
            set
            {
                numberOfLevels = value;
                RaisePropertyChanged("NumberOfLevels");
            }
        }

        private int numberOfItems;
        public int NumberOfItems
        {
            get { return numberOfItems; }
            set
            {
                numberOfItems = value;
                RaisePropertyChanged("NumberOfItems");
                RaisePropertyChanged("ShowNumberOfItems");
            }
        }

        public bool ShowNumberOfItems
        {
            get { return NumberOfItems > 1; }
        }

        #endregion

        #region Public Methods

        public CompactBubbleViewModel()
        {
        }

        public CompactBubbleViewModel(string nodeLabel, int levels, int items)
        {
            NodeLabel = nodeLabel;
            NumberOfLevels = levels;
            NumberOfItems = items;
        }

        #endregion
    }
}
