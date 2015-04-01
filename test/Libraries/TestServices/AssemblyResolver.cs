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
            Setup((new TestSessionConfiguration()).DynamoCorePath);
        }

        /// <summary>
        /// Setup the assembly resolver, specifying a core path.
        /// </summary>
        /// <param name="corePath"></param>
        public void Setup(string corePath)
        {
            if (assemblyHelper != null) return;

            assemblyHelper = new AssemblyHelper(corePath, null);
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
 