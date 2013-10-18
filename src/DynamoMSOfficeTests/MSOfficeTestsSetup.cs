using System;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [SetUpFixture]
    public class UITestSetup
    {
        [SetUp]
        public void Setup()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void RunAfterAllTests()
        {
            
        }
    }
}
