using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.Extensions;
using Dynamo.LibraryUI.Properties;
using Dynamo.Models;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;

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

        private string address;
    }
}
