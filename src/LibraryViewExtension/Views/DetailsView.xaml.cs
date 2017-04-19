using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using Dynamo.LibraryUI.ViewModels;

namespace Dynamo.LibraryUI.Views
{
    /// <summary>
    /// Interaction logic for DetailsView.xaml
    /// </summary>
    public partial class DetailsView : UserControl
    {
        private Grid parentGrid;
        private UIElement lastVisibleItem;

        public DetailsView(DetailsViewModel viewModel, Grid parent)
        {
            this.DataContext = viewModel;
            this.parentGrid = parent;

            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings { RemoteDebuggingPort = 8088 };
                //to fix Fickering set disable-gpu to true
                settings.CefCommandLineArgs.Add("disable-gpu", "1");
                Cef.Initialize(settings);
            }

            InitializeComponent();

            this.IsVisibleChanged += OnVisibilityChange;
        }

        private void UpdateVisibility()
        {
            foreach (UIElement item in parentGrid.Children)
            {
                if (item.Visibility == Visibility.Visible && item != this)
                {
                    lastVisibleItem = item;
                    lastVisibleItem.Visibility = Visibility.Collapsed;
                    break;
                }
            }
        }

        private void OnVisibilityChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility != Visibility.Visible)
            {
                if(lastVisibleItem != null) lastVisibleItem.Visibility = Visibility.Visible;
            }
            else
            {
                lastVisibleItem = null;
                UpdateVisibility();
            }
        }
    }
}
