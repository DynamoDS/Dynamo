using System;
using System.Collections.Generic;
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
using Dynamo.Publish.ViewModels;

namespace Dynamo.Publish.Views
{
    /// <summary>
    /// Interaction logic for InviteView.xaml
    /// </summary>
    public partial class InviteView : Window
    {
        public InviteView(InviteViewModel inviteViewModel)
        {
            InitializeComponent();
            DataContext = inviteViewModel;
            Loaded += inviteViewModel.InviteLoad;
        }
    }
}