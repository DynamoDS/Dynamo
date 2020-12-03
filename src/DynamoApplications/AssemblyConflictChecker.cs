using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamoApplications.Properties;

namespace Dynamo.Applications
{
    /// <summary>
    /// Checks for incompatibilities between loaded assemblies.
    /// </summary>
    public class AssemblyConflictChecker : IDisposable
    {
        /// <summary>
        /// List of assembly names to ingnore when checking for incompatibilities.
        /// </summary>
        public IEnumerable<string> AssemblyNamesToIgnore = new List<string>();

        /// <summary>
        ///  Flag specifying whether to skip all checks.
        /// </summary>
        public bool Enable;

        /// <summary>
        /// The default white list of dependencies to be ignored.
        /// </summary>
        private readonly string[] DefaultAssemblyNamesToIgnore = new string[] { "Newtonsoft.Json" };
        public Exception[] Exceptions { get { return Conflicts.Where(x => x.Value == false).Select(x => x.Key).ToArray(); } }

        private readonly Dictionary<Exception, bool> Conflicts = new Dictionary<Exception, bool>();

        private void AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (!Enable)
            {
                return;
            }
            var conflicts = GetVersionMismatchedReferencesInAppDomain(args.LoadedAssembly);
            var ignoreList = AssemblyNamesToIgnore?.Concat(DefaultAssemblyNamesToIgnore) ?? DefaultAssemblyNamesToIgnore;

            foreach (var conflict in conflicts)
            {
                var ignored = ignoreList.Contains(conflict.Key);
                Conflicts.Add(conflict.Value, ignored);
            }
        }

        /// <summary>
        /// Constructor for AssemblyConflictNotifier
        /// </summary>
        public AssemblyConflictChecker()
        {
            Enable = true;
            AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoad;
        }

        /// <summary>
        /// Dispose function for AssemblyConflictNotifier
        /// </summary>
        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= AssemblyLoad;
        }

        /// <summary>
        /// Logs any assembly conflicts found.
        /// </summary>
        public void LogConflicts(Models.DynamoModel DynamoModel)
        {
            foreach (var conflict in Conflicts)
            {
                bool isIgnored = conflict.Value;
                var x = conflict.Key;

                if (isIgnored)
                {
                    DynamoModel.Logger.LogInfo(GetType().ToString(), x.Message);
                } else
                {
                    DynamoModel.Logger.LogNotification(DynamoModel.GetType().ToString(),
                    x.GetType().ToString(), Resources.MismatchedAssemblyVersionShortMessage,
                    x.Message);
                }
            }
            Conflicts.Clear();
        }

        internal static Dictionary<string, Exception> GetVersionMismatchedReferencesInAppDomain(Assembly assembly)
        {
            // Get all assemblies that are currently loaded into the appdomain.
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            // Ignore some assemblies(Revit assemblies) that we know work and have changed their version number format or do not align
            // with semantic versioning.

            var loadedAssemblyNames = loadedAssemblies.Select(assem => assem.GetName()).ToList();

            //build dict- ignore those with duplicate names.
            var loadedAssemblyDict = loadedAssemblyNames.GroupBy(assm => assm.Name).ToDictionary(g => g.Key, g => g.First());

            var output = new Dictionary<string, Exception>();
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
                        output.Add(currentReferencedAssembly.Name, new FileLoadException(
                                string.Format(Resources.MismatchedAssemblyVersion, assembly.FullName, currentReferencedAssembly.FullName)
                                + Environment.NewLine + Resources.MismatchedAssemblyList + Environment.NewLine +
                                string.Join(", ", referencingNewerVersions.Select(x => x.Name).Distinct().ToArray())));
                    }
                }
            }
            return output;
        }
    }
}
