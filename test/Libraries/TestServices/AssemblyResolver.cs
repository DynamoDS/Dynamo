using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Dynamo.Utilities;

using DynamoUtilities;

namespace TestServices
{
    public static class AssemblyResolver
    {
        private static bool resolverSetup;

        /// <summary>
        /// Setup the assembly resolver using the base path 
        /// specified in the config file.
        /// </summary>
        public static void Setup()
        {
            if (resolverSetup) return;

            var testConfig = new TestSessionConfiguration();
            DynamoPathManager.Instance.InitializeCore(testConfig.DynamoCorePath);
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            resolverSetup = true;
        }

        /// <summary>
        /// Setup the assembly resolver, specifying a core path.
        /// </summary>
        /// <param name="corePath"></param>
        public static void Setup(string corePath)
        {
            if (resolverSetup) return;

            DynamoPathManager.Instance.InitializeCore(corePath);
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            resolverSetup = true;
        }

        public static string GetDynamoRootDirectory()
        {
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
            return assemDir.Parent.FullName;
        }

    }
    
}
 