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
                RaisePropertyChanged(nameof(NotificationsCounterVisibility));
            }
        }

        public Visibility NotificationsCounterVisibility { 
            get 
            {
                if (NotificationsNumber == 0)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }
    }
}
