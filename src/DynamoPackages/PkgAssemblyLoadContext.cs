using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace DynamoPackages
{
    internal class PkgAssemblyLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;
        public PkgAssemblyLoadContext(string name, string pluginPath, bool unloadable = true) : base(name, unloadable)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);

        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                var newAss = LoadFromAssemblyPath(assemblyPath);
                var assContext = AssemblyLoadContext.GetLoadContext(newAss);
                return newAss;
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
