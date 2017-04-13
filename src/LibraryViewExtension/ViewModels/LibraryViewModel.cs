using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.LibraryUI.ViewModels
{
    /// <summary>
    /// Package Manager View Loader
    /// </summary>
    public class LibraryViewModel : NotificationObject
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="commandExecutive"></param>
        public LibraryViewModel(string address)
        {
            this.address = address;
        }

        /// <summary>
        /// Returns Web URL to bind
        /// </summary>
        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                RaisePropertyChanged("Address");
            }
        }

        /// <summary>
        /// Checks if the view is visible
        /// </summary>
        public bool IsVisible
        {
            get { return visible; }
            set
            {
                visible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        private bool visible = true;
        private string address;
    }
}
