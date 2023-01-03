namespace Dynamo.ViewModels
{
    /// <summary>
    /// Class containing data to display in compact preview bubble
    /// </summary>
    public class CompactBubbleViewModel : ViewModelBase
    {
        #region Properties

        private string nodeLabel;
        /// <summary>
        /// Represents type of node output
        /// </summary>
        public string NodeLabel
        {
            get { return nodeLabel; }
            set
            {
                nodeLabel = value;
                RaisePropertyChanged("NodeLabel");
            }
        }

        private int numberOfItems;
        /// <summary>
        /// Number of items in the overall list if node output is a list
        /// </summary>
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

        /// <summary>
        /// Indicates if number of list items is shown
        /// </summary>
        public bool ShowNumberOfItems
        {
            get { return NumberOfItems > 0 && IsCollection; }
        }

        /// <summary>
        /// Indicates if number of list items is shown
        /// </summary>
        internal bool IsCollection { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of <cref name="CompactBubbleViewModel"/> class with empty data
        /// </summary>
        public CompactBubbleViewModel(bool isCollection)
        {
            IsCollection = isCollection;
        }

        /// <summary>
        /// Creates an instance of <cref name="CompactBubbleViewModel"/> class with specified data
        /// </summary>
        /// <param name="nodeLabel">Text representing type of node output</param>
        /// <param name="items">Number of items in the overall list if node output is a list</param>
        public CompactBubbleViewModel(string nodeLabel, int items)
        {
            NodeLabel = nodeLabel;
            NumberOfItems = items;
        }

        #endregion
    }
}
