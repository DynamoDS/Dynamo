using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Dynamo;
using NUnit.Framework;
using static DSIronPythonTests.IronPythonTests;

namespace DynamoPythonTests
{
    public class PythonEvalTestsWithLibraries : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FFITarget.dll");
        }

        public IEnumerable<PythonEvaluatorDelegate> Evaluators = new List<PythonEvaluatorDelegate> {
            DSCPython.CPythonEvaluator.EvaluatePythonScript,
            DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript
        };

        [Test]
        public void TestBigIntegerEncoding()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
from FFITarget import DummyMath

# Provide Python int as arguments: Python => .NET
sum = DummyMath.Sum(11111111111111111111, 11111111111111111111)
# sum contains a BigInteger and we use it in Python + operation: .NET => Python
sum = sum + 1

OUT = sum
";
            var empty = new ArrayList();
            var expected = BigInteger.Parse("22222222222222222223");
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.AreEqual(expected, result);
            }
        }
    }
}
