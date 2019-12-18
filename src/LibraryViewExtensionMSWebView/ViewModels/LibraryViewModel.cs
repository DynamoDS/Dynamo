using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Core;

namespace Dynamo.LibraryViewExtensionMSWebView.ViewModels
{
    /// <summary>
    /// Library View Loader
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

        //TODO this is no longer used...
        //unless we redirect navigateToString to address.
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
        /// Checks and updates the visibility property
        /// </summary>
        public bool IsVisible
        {
            get { return Visibility == Visibility.Visible; }
            set { Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Controls visibilty of the view
        /// </summary>
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                RaisePropertyChanged("Visibility");
            }
        }

        private Visibility visibility = Visibility.Visible;
        private string address;
    }
}
