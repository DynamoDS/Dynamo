using System;
using System.IO;
using System.Reflection;

namespace Dynamo.Utilities
{
    public static class AssemblyHelper
    {
        /// <summary>
        /// Attempts to resolve an assembly from the dll directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\dll";
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath))
                return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        public static Version GetDynamoVersion()
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetName().Version;
        }
    }
}
