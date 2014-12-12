using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public abstract class ViewModelBase : NotificationObject
    {
        public bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#endif
                return false;
            }
        }
    }
}
