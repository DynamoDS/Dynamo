using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Nodes;

namespace Dynamo.PackageManager.UI
{
    public class PackageItemLeafViewModel : PackageItemInternalViewModel
    {

        public PackageItemLeafViewModel(Assembly assembly, PackageItemViewModel parent) : base(assembly, parent)
        {
        }

        public PackageItemLeafViewModel(FunctionDefinition def, PackageItemViewModel parent) : base(def, parent)
        {
        }

    }

}
