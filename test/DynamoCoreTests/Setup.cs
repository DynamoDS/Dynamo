using System;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void RunBeforeAllTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void RunAfterAllTests()
        {

        } 
    }
}
