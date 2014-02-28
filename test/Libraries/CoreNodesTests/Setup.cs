using System;
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
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void RunAfterAnyTests()
        {

        }
    }
}
