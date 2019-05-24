using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Interfaces
{
    public interface IPackage
    {
        string Name { get; set; }

        string VersionName { get; set; }

        IEnumerable<Assembly> NodeLibraries { get; }

        //bool ContainsNodeLibraries { get; }
    }
}
