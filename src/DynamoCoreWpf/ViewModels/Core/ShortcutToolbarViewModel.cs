using Dynamo.ViewModels;
using System.Windows;

namespace Dynamo.Wpf.ViewModels.Core
{
    internal class ShortcutToolbarViewModel : ViewModelBase
    {
        public ShortcutToolbarViewModel()
        {
            NotificationsNumber = 0;
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
    }
}
