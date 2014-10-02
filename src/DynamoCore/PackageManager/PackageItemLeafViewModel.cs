using System.Reflection;

namespace Dynamo.PackageManager.UI
{
    public class PackageItemLeafViewModel : PackageItemInternalViewModel
    {

        public PackageItemLeafViewModel(Assembly assembly, PackageItemViewModel parent) : base(assembly, parent)
        {
        }

        public PackageItemLeafViewModel(CustomNodeDefinition def, PackageItemViewModel parent) : base(def, parent)
        {
        }

    }

}
