using System.Windows.Controls;
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
            //Browser.Settings.IsIndexedDBEnabled = false;
            //Browser.Settings.IsJavaScriptEnabled = true;
            //Browser.Settings.IsScriptNotifyAllowed = true;

            InitializeComponent();

           // this.Browser.MenuHandler = new LibraryViewContextMenuHandler();
        }
        //TODO what does this do

        /*
        private class LibraryViewContextMenuHandler : IContextMenuHandler
        {
            public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                model.Clear();
            }

            public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                return false;
            }

            public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
            }

            public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return false;
            }
        }
        */
    }
}