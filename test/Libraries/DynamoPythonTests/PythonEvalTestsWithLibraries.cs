using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dynamo;
using Dynamo.Interfaces;
using Dynamo.Models;
using NUnit.Framework;
using static DSIronPythonTests.IronPythonTests;

namespace DynamoPythonTests
{
    public class PythonEvalTestsWithLibraries : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var config = base.CreateStartConfiguration(settings);
            config.Preferences.CustomPackageFolders.Add(Path.Combine(TestDirectory, @"core\python\pkgs"));
            return config;
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
clr.AddReference('MyZeroTouch')
from MyZeroTouch import MyMath

# MyMath.Sum is an imported method that sums two BigInteger and returns one also
# Provide Python int as arguments: Python => .NET
sum = MyMath.Sum(11111111111111111111, 11111111111111111111)
# sum contains a BigInteger and we use it in Python + operation: .NET => Python
sum = sum + 1

OUT = sum
";
            var empty = new ArrayList();
            var expected = BigInteger.Parse("222222222222222222223");
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.AreEqual(expected, result);
            }
        }
    }
}
