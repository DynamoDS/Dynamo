using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Dynamo.PackageManager
{
    internal class PkgAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string RootDir;
        private IEnumerable<FileInfo> pkgAssemblies = null;
        public PkgAssemblyLoadContext(string name, string pkgRoot, bool unloadable = true) : base(name, unloadable)
        {
            this.RootDir = pkgRoot;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            pkgAssemblies ??= new DirectoryInfo(RootDir).EnumerateFiles("*.dll", new EnumerationOptions() { RecurseSubdirectories = true });

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
            pkgAssemblies ??= new DirectoryInfo(RootDir).EnumerateFiles("*.dll", new EnumerationOptions() { RecurseSubdirectories = true });

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
