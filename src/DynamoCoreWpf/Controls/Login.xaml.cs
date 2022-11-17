using System.Windows;
using System.Windows.Controls;
using Dynamo.ViewModels;
using Greg.AuthProviders;
using Dynamo.Models;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private readonly DynamoModel _viewModel;

        public Login(DynamoModel model)
        {
            this.DataContext = model;
            this._viewModel = model;

            InitializeComponent();
        }

        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.AuthenticationManager.LoginState == LoginState.LoggedIn)
            {
                var button = (Button) sender;
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
            else if (_viewModel.AuthenticationManager.LoginState == LoginState.LoggedOut)
            {
                _viewModel.AuthenticationManager.ToggleLoginState();
            }
        }
    }
}
