using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Dynamo.Utilities;

using DynamoUtilities;

namespace TestServices
{
    public class AssemblyResolver
    {
        private AssemblyHelper assemblyHelper;

        /// <summary>
        /// Setup the assembly resolver using the base path 
        /// specified in the config file.
        /// </summary>
        public void Setup()
        {
            if (assemblyHelper != null) return;

            var testConfig = new TestSessionConfiguration();
            assemblyHelper = new AssemblyHelper(testConfig.DynamoCorePath, null);
            AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
        }

        public void TearDown()
        {
            if (assemblyHelper == null)
                return;

            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
        }

    }
    
}
 