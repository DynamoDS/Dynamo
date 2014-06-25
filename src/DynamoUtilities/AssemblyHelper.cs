using System;
using System.IO;
using System.Reflection;
using DynamoUtilities;

namespace Dynamo.Utilities
{
    public static class AssemblyHelper
    {
        /// <summary>
        /// Handler to the ApplicationDomain's AssemblyResolve event.
        /// If an assembly's location cannot be resolved, an exception is
        /// thrown. Failure to resolve an assembly will leave Dynamo in 
        /// a bad state, so we should throw an exception here which gets caught 
        /// by our unhandled exception handler and presents the crash dialogue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                // First check the core path
                string assemblyPath = Path.Combine(DynamoPathManager.Instance.MainExecPath, new AssemblyName(args.Name).Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

                // Then check the dll path
                assemblyPath = Path.Combine(DynamoPathManager.Instance.Asm, new AssemblyName(args.Name).Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

                // Then check all additional resolution paths
                foreach (var addPath in DynamoPathManager.Instance.AdditionalResolutionPaths)
                {
                    assemblyPath = Path.Combine(addPath, new AssemblyName(args.Name).Name + ".dll");
                    if (File.Exists(assemblyPath))
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("There location of the assembly, {0} could not be resolved for loading.", args.Name), ex);
            }
        }

        public static Version GetDynamoVersion()
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetName().Version;
        }

        public static Assembly LoadLibG()
        {
            var libG = Assembly.LoadFrom(DynamoPathManager.Instance.Asm);
            return libG;
        }
    }
}
