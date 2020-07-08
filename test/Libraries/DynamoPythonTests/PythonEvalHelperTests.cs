using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    [TestFixture]
    class PythonEvalHelperTests
    {
        [Test]
        public void TestHelperInitialState()
        {
            Assert.AreEqual(false, PythonEvaluationHelper.lazy.IsValueCreated);
            Assert.AreEqual(false, PythonEvaluationHelper.IsCPythonEnabled);
            Assert.AreEqual(false, PythonEvaluationHelper.IsIronPythonEnabled);
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

            PythonEvaluationHelper.Instance.GetEvaluatorInfo(PythonEngineVersion.CPython3, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEvaluationHelper.CPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEvaluationHelper.CPythonEvaluationMethod);

            Assert.AreEqual(true, PythonEvaluationHelper.IsCPythonEnabled);
            Assert.AreEqual(true, PythonEvaluationHelper.IsIronPythonEnabled);
        }
    }
}
