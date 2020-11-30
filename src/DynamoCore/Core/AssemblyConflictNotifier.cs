using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Properties;

namespace Dynamo.Core
{
    /// <summary>
    /// Checks and notifies user if incompatibilities between loaded assemblies.
    /// </summary>
    public static class AssemblyConflictNotifier
    {
        /// <summary>
        /// List of assembly names to ingnore when checking for incompatibilities.
        /// </summary>
        public static IEnumerable<string> AssemblyNamesToIgnore;

        /// <summary>
        ///  Flag specifying whether to skip all checks.
        /// </summary>
        public static bool SkipChecks = true;

        /// <summary>
        /// The default white list of dependencies to be ignored.
        /// </summary>
        private static readonly string[] DefaultAssemblyNamesToIgnore = new string[] { "Newtonsoft.Json", "RevitAPI.dll", "RevitAPIUI.dll" };

        private static readonly List<Exception> Exceptions = new List<Exception>();

        static AssemblyConflictNotifier()
        {
            void AssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                if (SkipChecks)
                {
                    return;
                }
                var exceptions = GetVersionMismatchedReferencesInAppDomain(args.LoadedAssembly, AssemblyNamesToIgnore.Concat(DefaultAssemblyNamesToIgnore));
                Exceptions.Concat(exceptions);
            }

            AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoad;
        }

        internal static IEnumerable<Exception> GetVersionMismatchedReferencesInAppDomain(Assembly assembly, IEnumerable<string> assemblyNamesToIgnore)
        {
            // Get all assemblies that are currently loaded into the appdomain.
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            // Ignore some assemblies(Revit assemblies) that we know work and have changed their version number format or do not align
            // with semantic versioning.

            var loadedAssemblyNames = loadedAssemblies.Select(assem => assem.GetName()).ToList();
            loadedAssemblyNames.RemoveAll(assemblyName => assemblyNamesToIgnore.Contains(assemblyName.Name));

            //build dict- ignore those with duplicate names.
            var loadedAssemblyDict = loadedAssemblyNames.GroupBy(assm => assm.Name).ToDictionary(g => g.Key, g => g.First());

            var output = new List<Exception>();

            foreach (var currentReferencedAssembly in assembly.GetReferencedAssemblies().Concat(new AssemblyName[] { assembly.GetName() }))
            {
                if (loadedAssemblyDict.ContainsKey(currentReferencedAssembly.Name))
                {
                    //if the dll is already loaded, then check that our required version is not greater than the currently loaded one.
                    var loadedAssembly = loadedAssemblyDict[currentReferencedAssembly.Name];
                    if (currentReferencedAssembly.Version.Major > loadedAssembly.Version.Major)
                    {
                        //there must exist a loaded assembly which references the newer version of the assembly which we require - lets find it:

                        var referencingNewerVersions = new List<AssemblyName>();
                        foreach (var originalLoadedAssembly in loadedAssemblies)
                        {
                            foreach (var refedAssembly in originalLoadedAssembly.GetReferencedAssemblies())
                            {
                                //if the version matches then this is one our guys
                                if (refedAssembly.Version == loadedAssembly.Version)
                                {
                                    referencingNewerVersions.Add(originalLoadedAssembly.GetName());
                                }
                            }
                        }

                        output.Add(new FileLoadException(
                            string.Format(Resources.MismatchedAssemblyVersion, assembly.FullName, currentReferencedAssembly.FullName)
                            + Environment.NewLine + Resources.MismatchedAssemblyList + Environment.NewLine +
                            string.Join(", ", referencingNewerVersions.Select(x => x.Name).Distinct().ToArray())));
                    }
                }
            }
            return output;
        }

        internal static void LogExceptions(Models.DynamoModel DynamoModel)
        {
            foreach (var x in Exceptions)
            {
                DynamoModel.Logger.LogNotification(DynamoModel.GetType().ToString(),
                x.GetType().ToString(), Resources.MismatchedAssemblyVersionShortMessage,
                x.Message);
            }
            Exceptions.Clear();
        }
    }
}
