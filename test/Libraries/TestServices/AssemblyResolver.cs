using System;
using System.Collections.Generic;
using Dynamo.Utilities;

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
        public void Setup(string corePath, IEnumerable<string> additionalResolutionPaths = null)
        {
            if (assemblyHelper != null) return;

            assemblyHelper = new AssemblyHelper(corePath, additionalResolutionPaths);
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
 