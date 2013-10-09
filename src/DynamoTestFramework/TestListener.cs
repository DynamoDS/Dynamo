using System;
using NUnit.Core;
using NUnit.Core.Filters;

namespace Dynamo.Tests
{
    public class TestListener : EventListener
    {
        public TestListener() { }
        public void RunStarted(string name, int testCount) { }
        public void RunFinished(TestResult result) { }
        public void RunFinished(Exception exception) { }
        public void TestStarted(TestName testName) { }
        public void TestFinished(TestResult result) { }
        public void SuiteStarted(TestName testName) { }
        public void SuiteFinished(TestResult result) { }
        public void UnhandledException(Exception exception) { }
        public void TestOutput(TestOutput testOutput) { }
    }
}
