using Dynamo.Core;
using Dynamo.UI.Commands;
using Dynamo.UI.Controls;
using Dynamo.ViewModels;
using System.Windows;

namespace Dynamo.Wpf.ViewModels.Core
{
    internal class ShortcutToolbarViewModel : ViewModelBase
    {
        /// <summary>
        /// Exports an image from the user's 3D background or workpace
        /// </summary>
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }

        public DelegateCommand SignOutCommand { get; set; }
        private AuthenticationManager authManager;
        private ShortcutToolbar shortcutToolbarView;

        private int notificationsNumber;


        public ShortcutToolbarViewModel(ShortcutToolbar view, DynamoViewModel dynamoViewModel)
        {
            NotificationsNumber = 0;
            shortcutToolbarView = view;
            authManager = dynamoViewModel.Model.AuthenticationManager;
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(dynamoViewModel.ShowSaveImageDialogAndSaveResult);
            SignOutCommand = new DelegateCommand(authManager.ToggleLoginState);
            authManager.LoginStateChanged += (o) => { SignOutHandler(); RaisePropertyChanged(nameof(LoginState)); };
        }

        /// <summary>
        /// This property represents the number of new notifications 
        /// </summary>
        public int NotificationsNumber { 
            get {
                return notificationsNumber;
            } 
            set {
                notificationsNumber = value;
                RaisePropertyChanged(nameof(IsNotificationsCounterVisible));
            }
        }

        /// <summary>
        /// Keeps track of the user's login state
        /// </summary>
        public string LoginState
        {
            get
            {
                return authManager.LoginState.ToString();
            }
        }
        /// <summary>
        /// Keeps track of the logged in user's username
        /// </summary>
        public string Username
        {
            get
            {
                return authManager.Username;
            }
        }

        public bool IsNotificationsCounterVisible
        { 
            get 
            {
                if (NotificationsNumber == 0)
                {
                    return false;
                }
                return true;
            }
        }
        private void SignOutHandler()
        {
            if (!authManager.IsLoggedIn()) {
                shortcutToolbarView.txtSignIn.Text = Properties.Resources.SignInButtonText;
                shortcutToolbarView.logoutOption.Visibility = Visibility.Collapsed;
            }
        }
    }
}
