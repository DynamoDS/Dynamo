using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dynamo.PackageManager.UI
{
    public class PackageDependencyLeafViewModel : PackageDependencyInternalViewModel
    {

        public PackageDependencyLeafViewModel(Assembly assembly, PackageDependencyViewModel parent) : base(assembly, parent)
        {
        }

        public PackageDependencyLeafViewModel(FunctionDefinition def, PackageDependencyViewModel parent) : base(def, parent)
        {
        }

    }

}
