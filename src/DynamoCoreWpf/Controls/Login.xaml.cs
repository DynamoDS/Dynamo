using System.Windows;
using System.Windows.Controls;

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
            if (_viewModel.AuthenticationManager.LoginState == LoginState.LoggedIn)
            {
                var button = (Button) sender;
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
            else if (_viewModel.AuthenticationManager.LoginState == LoginState.LoggedOut)
            {
                _viewModel.ToggleLoginStateCommand.Execute(null);
            }
        }
    }
}
