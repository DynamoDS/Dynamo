using System.Windows.Controls;
using CefSharp;
using Dynamo.LibraryUI.ViewModels;

namespace Dynamo.LibraryUI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView(LibraryViewModel viewModel)
        {
            this.DataContext = viewModel;

            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings { RemoteDebuggingPort = 8088 };
                //to fix Fickering set disable-gpu to true
                settings.CefCommandLineArgs.Add("disable-gpu", "1");
                Cef.Initialize(settings);
            }
            
            InitializeComponent();
        }
    }
}