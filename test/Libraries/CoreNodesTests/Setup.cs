using System;
using Dynamo;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void RunBeforeAnyTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.ResolveAssembly;
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
        }
    }
}
