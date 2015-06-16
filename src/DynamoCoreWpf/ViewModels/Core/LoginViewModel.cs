using System.Windows.Input;

using Dynamo.Core;
using Dynamo.UI.Commands;

using Greg.AuthProviders;

namespace Dynamo.ViewModels
{
    public class LoginViewModel : NotificationObject
    {
        public readonly DynamoViewModel DynamoViewModel;
        public AuthenticationManager AuthenticationManager { get; set; }
        public ICommand ToggleLoginStateCommand { get; private set; }

        public LoginState LoginState
        {
            get
            {
                return AuthenticationManager.LoginState;
            }
        }

        public string Username
        {
            get
            {
                return AuthenticationManager.Username;
            }
        }

        public bool HasAuthProvider
        {
            get { return AuthenticationManager.HasAuthProvider; }
        }

        internal LoginViewModel(DynamoViewModel dynamoViewModel)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.AuthenticationManager = dynamoViewModel.Model.AuthenticationManager;

            this.ToggleLoginStateCommand = new DelegateCommand( AuthenticationManager.ToggleLoginState, AuthenticationManager.CanToggleLoginState);

            AuthenticationManager.LoginStateChanged += (loginState) =>
            {
                RaisePropertyChanged("LoginState");
                RaisePropertyChanged("Username");
            };

        }
    }
}
