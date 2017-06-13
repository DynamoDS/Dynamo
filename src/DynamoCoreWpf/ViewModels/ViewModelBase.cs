using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;

namespace Dynamo.ViewModels
{
    public abstract class ViewModelBase : NotificationObject
    {
        [JsonIgnore]
        public bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
