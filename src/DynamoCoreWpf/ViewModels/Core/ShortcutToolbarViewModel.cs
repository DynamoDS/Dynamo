using Dynamo.Core;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Core
{
    internal class ShortcutToolbarViewModel : ViewModelBase
    {
        /// <summary>
        /// Exports an image from the user's 3D background or workpace
        /// </summary>
        public DelegateCommand ValidateWorkSpaceBeforeToExportAsImageCommand { get; set; }

        public DelegateCommand SignOutCommand { get; set; }
        private AuthenticationManager authManager;

        private int notificationsNumber;

        public ShortcutToolbarViewModel(DynamoViewModel dynamoViewModel)
        {
            NotificationsNumber = 0;
            authManager = dynamoViewModel.Model.AuthenticationManager;
            ValidateWorkSpaceBeforeToExportAsImageCommand = new DelegateCommand(dynamoViewModel.ValidateWorkSpaceBeforeToExportAsImage);
            SignOutCommand = new DelegateCommand(authManager.ToggleLoginState);
            authManager.LoginStateChanged += (o) => { RaisePropertyChanged(nameof(LoginState)); };
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
    }
}
