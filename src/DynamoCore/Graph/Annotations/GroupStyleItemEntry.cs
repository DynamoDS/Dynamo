namespace Dynamo.Graph.Annotations
{
    /// <summary>
    /// This class represent a GroupStyle item that will be shown in the Group ContextMenu (basically a MenuItem)
    /// </summary>
    internal class GroupStyleItemEntry : GroupStyleBase
    {
        private string hexColorString;
        private string textContent;
        private bool isChecked = false;
        private bool isDefault = false;

        /// <summary>
        /// This property will be used to store the color in hexadecimal string that will be shown in the GroupStyles context menu
        /// </summary>
        public string HexColorString
        {
            get { return "#" + hexColorString; }
            set
            {
                hexColorString = value;
                RaisePropertyChanged(nameof(HexColorString));
            }
        }

        /// <summary>
        /// This property will store the Group Name (shown in a label) that is shown in the GroupStyles context menu
        /// </summary>
        public string TextContent
        {
            get { return textContent; }
            set
            {
                textContent = value;
                RaisePropertyChanged(nameof(TextContent));
            }
        }

        /// <summary>
        /// This property will say it we should display the checkmark in the MenuItem (appearing in the GroupStyles context menu)
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        /// <summary>
        /// This property will describe if the current GroupStyle added is a default one or a custom one.
        /// </summary>
        public bool IsDefault
        {
            get { return isDefault; }
            set
            {
                isDefault = value;
                RaisePropertyChanged(nameof(IsDefault));
            }
        }
    }
}
