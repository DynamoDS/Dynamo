using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Dynamo.LibraryUI.Properties;
using Dynamo.Models;
using Newtonsoft.Json;

namespace Dynamo.LibraryUI.ViewModels
{
    /// <summary>
    /// Package Manager View Loader
    /// </summary>
    public class LibraryViewModel
    {
        public string Address { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address"></param>
        public LibraryViewModel(string address)
        {
            this.Address = address;
        }

        public void OnItemClicked(string item)
        {
            MessageBox.Show(item + " Clicked");
        }
    }
}
