using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            DynamoPathManager.Instance.InitializeCore(GetDynamoRootDirectory());
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;

            resolverSetup = true;
        }

        private static string GetDynamoRootDirectory()
        {
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo(Path.GetDirectoryName(assemPath));
            return assemDir.Parent.FullName;
        }

    }
    
}
 