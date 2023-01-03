using Dynamo.Core;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class stores the group styles added by the user
    /// </summary>
    public class StyleItem : NotificationObject
    {
        private string hexColorString;
        private string name;
        private bool isDefault = false;

        /// This property will contain the Group Name of the stored style
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// This property will contain the color in hexadecimal
        public string HexColorString
        {
            get { return hexColorString; }
            set
            {
                hexColorString = value;
                RaisePropertyChanged(nameof(HexColorString));
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
