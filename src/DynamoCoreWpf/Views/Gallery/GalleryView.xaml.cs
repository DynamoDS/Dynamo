using System.Windows.Controls;
using Dynamo.Wpf.ViewModels.Core;

namespace Dynamo.Wpf.Views.Gallery
{
    /// <summary>
    /// Interaction logic for GalleryView.xaml
    /// </summary>
    public partial class GalleryView : UserControl
    {
        public GalleryViewModel ViewModel { get; private set; }

        public GalleryView(GalleryViewModel galleryViewModel)
        {
            InitializeComponent();
            DataContext = galleryViewModel;
            ViewModel = galleryViewModel;
            Logging.Analytics.TrackScreenView("Gallery");
        }

        private void GalleryView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true; //So that the clicking doesn't bubble-up to the galleryBackground.
        }
    }
}