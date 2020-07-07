using System.Collections.Generic;
using Dynamo;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    class PythonEvalHelperTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("PythonNodeModels.dll");
        }

        [Test]
        public void TestHelperInitialState()
        {
            Assert.AreEqual( false, PythonEvaluationHelper.lazy.IsValueCreated);
        }

        [Test]
        public void TestHelperInitialization()
        {
            Assert.AreEqual(false, PythonEvaluationHelper.lazy.IsValueCreated);

            var evaluatorClass = string.Empty;
            var evaluationMethod = string.Empty;

            PythonEvaluationHelper.Instance.GetEvaluatorInfo(PythonEngineVersion.IronPython2, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(true, PythonEvaluationHelper.lazy.IsValueCreated);
            Assert.AreEqual(evaluatorClass, PythonEvaluationHelper.IronPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEvaluationHelper.IronPythonEvaluationMethod);
        }
    }
}
