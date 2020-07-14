using System.Collections.Generic;
using Dynamo;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    [TestFixture]
    class PythonEngineSelectorTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            // Add multiple libraries to better simulate typical Dynamo application usage.
            libraries.Add("DSCPython.dll");
            libraries.Add("DSIronPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test will cover the use case of the API to query certain Python engine ability for evaluation
        /// </summary>
        [Test]
        public void TestEngineSelectorInitialization()
        {
            PythonEngineSelector.Instance.GetEvaluatorInfo(PythonEngineVersion.IronPython2, out string evaluatorClass, out string evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEngineSelector.Instance.IronPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineSelector.Instance.IronPythonEvaluationMethod);

            PythonEngineSelector.Instance.GetEvaluatorInfo(PythonEngineVersion.CPython3, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEngineSelector.Instance.CPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineSelector.Instance.CPythonEvaluationMethod);

            Assert.AreEqual(true, PythonEngineSelector.Instance.IsCPythonEnabled);
            Assert.AreEqual(true, PythonEngineSelector.Instance.IsIronPythonEnabled);
        }
    }
}
