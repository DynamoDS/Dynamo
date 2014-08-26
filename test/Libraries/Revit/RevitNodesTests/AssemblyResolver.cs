using System;
using System.IO;
using System.Reflection;

using Dynamo.Utilities;
using DynamoUtilities;

namespace DSRevitNodesTests
{
    public static class AssemblyResolver
    {
        private static bool resolverSetup;
        public static void Setup()
        {
            if (resolverSetup) return;

            DynamoPathManager.Instance.ASMVersion = DynamoPathManager.Asm.Version220;
            DynamoPathManager.Instance.InitializeCore(GetDynamoRootDirectory());

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            resolverSetup = true;
        }

        internal static string GetDynamoRootDirectory()
        {
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
            return assemDir.Parent.FullName;
        }

    }
    
}
 