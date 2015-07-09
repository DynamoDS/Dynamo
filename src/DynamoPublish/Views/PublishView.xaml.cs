using Dynamo.Publish.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamo.Publish.Views
{
    /// <summary>
    /// Interaction logic for PublishView.xaml
    /// </summary>
    public partial class PublishView : Window
    {
        public PublishViewModel ViewModel
        {
            get
            {
                return DataContext as PublishViewModel;
            }
        }

        public PublishView(PublishViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.PublishView = this;
        }

        private void OnPublishViewClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
