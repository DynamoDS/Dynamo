using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Core
{
    internal class ShortcutToolbarViewModel : ViewModelBase
    {
        public ShortcutToolbarViewModel()
        {
            NotificationsNumber = 0;
        }

        /// <summary>
        /// This property represents the number of new notifications 
        /// </summary>
        public int NotificationsNumber { get; set; }
    }
}
