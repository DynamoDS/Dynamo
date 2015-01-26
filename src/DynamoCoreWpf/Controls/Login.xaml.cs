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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.ViewModels;
using Greg.AuthProviders;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private readonly PackageManagerClientViewModel _viewModel;

        public Login(PackageManagerClientViewModel viewModel)
        {
            this.DataContext = viewModel;
            this._viewModel = viewModel;

            InitializeComponent();
        }

        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LoginState == LoginState.LoggedIn)
            {
                var button = (Button) sender;
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
            else if (_viewModel.LoginState == LoginState.LoggedOut)
            {
                _viewModel.LoginLogoutCommand.Execute(null);
            }
        }
    }
}
