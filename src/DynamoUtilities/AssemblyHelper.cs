using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Dynamo.Utilities
{
    public class AssemblyHelper
    {
        private readonly string moduleRootFolder;
        private readonly IEnumerable<string> additionalResolutionPaths;
        private bool testMode;

        public AssemblyHelper(string moduleRootFolder, IEnumerable<string> additionalResolutionPaths, bool testMode = false)
        {
            if (additionalResolutionPaths == null)
                additionalResolutionPaths = new List<string>();

            this.moduleRootFolder = moduleRootFolder;
            this.additionalResolutionPaths = additionalResolutionPaths;
            this.testMode = testMode;
        }

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
        public Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                var targetAssemblyName = new AssemblyName(args.Name).Name + ".dll";

                // First check the core path
                string assemblyPath = Path.Combine(moduleRootFolder, targetAssemblyName);
                if (testMode)
                {
                    Console.WriteLine("trying to resolve " + targetAssemblyName + " at " + assemblyPath);
                }
                if (File.Exists(assemblyPath))
                {
                    if (testMode)
                    {
                        Console.WriteLine("loading from " + assemblyPath);

                    }
                    return Assembly.LoadFrom(assemblyPath);
                }

                // Then check all additional resolution paths
                foreach (var resolutionPath in additionalResolutionPaths)
                {
                    assemblyPath = Path.Combine(resolutionPath, targetAssemblyName);
                    if (testMode)
                    {
                        Console.WriteLine("trying to resolve " + targetAssemblyName + " at " + assemblyPath);
                    }

                    if (File.Exists(assemblyPath))
                    {
                        if (testMode)
                        {
                            Console.WriteLine("loading from " + assemblyPath);
                        }
                        return Assembly.LoadFrom(assemblyPath);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("There location of the assembly, " +
                    "{0} could not be resolved for loading.", args.Name), ex);
            }
        }

        public static Version GetDynamoVersion(bool includeRevisionNumber = true)
        {
            var assembly = Assembly.GetCallingAssembly();
            var version = assembly.GetName().Version;
            return includeRevisionNumber
                ? version
                : new Version(version.Major, version.Minor, version.Build, 0);
        }
    }
}
