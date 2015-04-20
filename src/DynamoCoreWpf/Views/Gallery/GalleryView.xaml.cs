using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf.Views.Gallery
{
    /// <summary>
    /// Interaction logic for GalleryView.xaml
    /// </summary>
    public partial class GalleryView : UserControl
    {
        public GalleryView(GalleryViewModel galleryViewModel)
        {
            InitializeComponent();
            DataContext = galleryViewModel;
        }

        private void GalleryView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true; //So that the clicking doesn't bubble-up to the DynamoView.
        }
    }
}