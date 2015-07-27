using Dynamo.Publish.ViewModels;
using Dynamo.Wpf.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        public PublishView(PublishViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.UIDispatcher = Dispatcher;
        }
    }
}
