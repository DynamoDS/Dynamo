using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Interfaces;

namespace Dynamo.Extensions
{
    public interface IExtension : IDisposable, ILogSource
    {
        string Name { get; }
        string Id { get; }

        event Action<Assembly> RequestLoadNodeLibrary;
        event Func<string, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectory;

        void Load(IPreferences preferences, IPathManager pathManager);
    }
}
