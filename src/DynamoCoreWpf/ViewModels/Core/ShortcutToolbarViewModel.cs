using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class ShortcutToolbarViewModel : ViewModelBase
    {
        public ShortcutToolbarViewModel()
        {
            NotificationsNumber = 0;
        }

        public int NotificationsNumber { get; set; }
    }
}
