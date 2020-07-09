using System.Collections.Generic;
using Dynamo;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    [TestFixture]
    class PythonEvalHelperTests: DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            // Add multiple libraries to better simulate typical Dynamo application usage.
            libraries.Add("DSCPython.dll");
            libraries.Add("DSIronPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test will cover the initial state of the Singleton
        /// </summary>
        [Test]
        public void TestHelperInitial_State()
        {
            Assert.AreEqual(false, PythonEvaluationHelper.lazy.IsValueCreated);
            Assert.AreEqual(false, PythonEvaluationHelper.IsCPythonEnabled);
            Assert.AreEqual(false, PythonEvaluationHelper.IsIronPythonEnabled);
        }

        /// <summary>
        /// This test will cover the use case of the API to query certain Python engine ability for evaluation
        /// </summary>
        [Test]
        public void TestHelperInitialization()
        {
            Assert.AreEqual(false, PythonEvaluationHelper.lazy.IsValueCreated);
            PythonEvaluationHelper.Instance.GetEvaluatorInfo(PythonEngineVersion.IronPython2, out string evaluatorClass, out string evaluationMethod);
            Assert.AreEqual(true, PythonEvaluationHelper.lazy.IsValueCreated);
            Assert.AreEqual(evaluatorClass, PythonEvaluationHelper.IronPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEvaluationHelper.IronPythonEvaluationMethod);

            PythonEvaluationHelper.Instance.GetEvaluatorInfo(PythonEngineVersion.CPython3, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEvaluationHelper.CPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEvaluationHelper.CPythonEvaluationMethod);

            Assert.AreEqual(true, PythonEvaluationHelper.IsCPythonEnabled);
            Assert.AreEqual(true, PythonEvaluationHelper.IsIronPythonEnabled);
        }
    }
}
