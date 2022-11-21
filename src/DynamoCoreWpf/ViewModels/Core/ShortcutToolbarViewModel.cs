using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System;
using System.Windows;

namespace Dynamo.Wpf.ViewModels.Core
{
    internal class ShortcutToolbarViewModel : ViewModelBase
    {
        public ShortcutToolbarViewModel(DynamoViewModel dynamoViewModel)
        {
            NotificationsNumber = 0;
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(dynamoViewModel.ShowSaveImageDialogAndSaveResult);
        }

        private int notificationsNumber;

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


        /// <summary>
        /// Exports an image from the user's 3D background or workpace
        /// </summary>
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
    }
}
