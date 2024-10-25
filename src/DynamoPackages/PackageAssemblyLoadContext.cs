using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace DynamoPackages
{
    internal class PkgAssemblyLoadContext : AssemblyLoadContext
    {
        private string pkgRoot;
        private IEnumerable<FileInfo> pkgAssemblies = null;
        public PkgAssemblyLoadContext(string name, string pkgRoot, bool unloadable = true) : base(name, unloadable)
        {
            this.pkgRoot = pkgRoot;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var oldAssem = Default.Assemblies.FirstOrDefault(x => x.GetName().Equals(assemblyName));
            if (oldAssem != null)
            {
                // not sure about this. Hoping we can avoid loading assemblies that are alrady loaded in the default context.
                return null;
            }

            pkgAssemblies ??= new DirectoryInfo(pkgRoot).EnumerateFiles("*.dll", new EnumerationOptions() { RecurseSubdirectories = true });

            var targetAssemName = assemblyName.Name + ".dll";
            var targetAssembly = pkgAssemblies.FirstOrDefault(x => x.Name == targetAssemName);
            if (targetAssembly != null)
            {
                return LoadFromAssemblyPath(targetAssembly.FullName);
            }
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            pkgAssemblies ??= new DirectoryInfo(pkgRoot).EnumerateFiles("*.dll", new EnumerationOptions() { RecurseSubdirectories = true });

            var targetAssemName = unmanagedDllName  + ".dll";
            var targetAssembly = pkgAssemblies.FirstOrDefault(x => x.Name == targetAssemName);
            if (targetAssembly != null)
            {
                return NativeLibrary.Load(targetAssembly.FullName);
            }
            return IntPtr.Zero;
        }
    }
}
