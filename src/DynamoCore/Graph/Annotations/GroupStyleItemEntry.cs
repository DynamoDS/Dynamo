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

        public string HexColorString
        {
            get { return "#" + hexColorString; }
            set
            {
                hexColorString = value;
                RaisePropertyChanged(nameof(HexColorString));
            }
        }
        public string TextContent
        {
            get { return textContent; }
            set
            {
                textContent = value;
                RaisePropertyChanged(nameof(TextContent));
            }
        }
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

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
