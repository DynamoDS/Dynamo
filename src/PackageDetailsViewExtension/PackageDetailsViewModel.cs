using Dynamo.Core;
using Dynamo.PackageManager.ViewModels;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject
    {
        public PackageManagerSearchElementViewModel PackageManagerSearchElementViewModel { get; }
        public PackageDetailsViewModel(PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)
        {
            PackageManagerSearchElementViewModel = packageManagerSearchElementViewModel;
        }
    }
}
