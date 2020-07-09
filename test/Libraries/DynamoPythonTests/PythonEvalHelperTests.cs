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
            Assert.AreEqual(false, PythonEngineSelector .lazy.IsValueCreated);
            Assert.AreEqual(false, PythonEngineSelector .IsCPythonEnabled);
            Assert.AreEqual(false, PythonEngineSelector .IsIronPythonEnabled);
        }

        /// <summary>
        /// This test will cover the use case of the API to query certain Python engine ability for evaluation
        /// </summary>
        [Test]
        public void TestHelperInitialization()
        {
            Assert.AreEqual(false, PythonEngineSelector .lazy.IsValueCreated);
            PythonEngineSelector .Instance.GetEvaluatorInfo(PythonEngineVersion.IronPython2, out string evaluatorClass, out string evaluationMethod);
            Assert.AreEqual(true, PythonEngineSelector .lazy.IsValueCreated);
            Assert.AreEqual(evaluatorClass, PythonEngineSelector .IronPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineSelector .IronPythonEvaluationMethod);

            PythonEngineSelector .Instance.GetEvaluatorInfo(PythonEngineVersion.CPython3, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEngineSelector .CPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineSelector .CPythonEvaluationMethod);

            Assert.AreEqual(true, PythonEngineSelector .IsCPythonEnabled);
            Assert.AreEqual(true, PythonEngineSelector .IsIronPythonEnabled);
        }
    }
}
