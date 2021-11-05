using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo;
using Dynamo.PythonServices;
using Dynamo.Utilities;
using NUnit.Framework;
using PythonNodeModels;
using static Dynamo.Models.DynamoModel;

namespace DynamoPythonTests
{
    [TestFixture]
    class PythonEngineSelectorTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            // Add multiple libraries to better simulate typical Dynamo application usage.
            libraries.Add("DSCPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test will cover the use case of the API to query certain Python engine ability for evaluation
        /// </summary>
        [Test]
        public void TestEngineSelectorInitialization()
        {
            PythonEngineManager.Instance.GetEvaluatorInfo(PythonEngineVersion.IronPython2, out string evaluatorClass, out string evaluationMethod);
            Assert.AreEqual(true, PythonEngineManager.lazy.IsValueCreated);
            Assert.AreEqual(PythonEngineManager.DummyEvaluatorClass, evaluatorClass);
            Assert.AreEqual(PythonEngineManager.DummyEvaluatorMethod, evaluationMethod);

            PythonEngineManager.Instance.GetEvaluatorInfo(PythonEngineVersion.CPython3, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEngineManager.CPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineManager.CPythonEvaluationMethod);

            Assert.AreEqual(true, PythonEngineManager.Instance.AvailableEngines.Any(x => x.Version == PythonEngineVersion.CPython3));
            Assert.AreEqual(false, PythonEngineManager.Instance.AvailableEngines.Any(x => x.Version == PythonEngineVersion.IronPython2));
        }

        [Test]
        public void CanCopydAndPasteAndUndoPythonEngine()
        {
            var pyNode = new PythonNode();

            CurrentDynamoModel.ExecuteCommand(new Dynamo.Models.DynamoModel.CreateNodeCommand(pyNode, 0, 0, false, false));
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            pyNode.Engine = PythonEngineVersion.CPython3;
            CurrentDynamoModel.AddToSelection(pyNode);
           
            CurrentDynamoModel.Copy();
            Assert.AreEqual(1, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<PythonNode>().All(x => x.Engine == PythonEngineVersion.CPython3));

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            pyNode.Engine = PythonEngineVersion.IronPython2;

            CurrentDynamoModel.ExecuteCommand(
                 new UpdateModelValueCommand(
                     Guid.Empty, pyNode.GUID, nameof(PythonNode.Engine), PythonEngineVersion.CPython3.ToString()));
            Assert.AreEqual(pyNode.Engine,PythonEngineVersion.CPython3);

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));

            Assert.AreEqual(pyNode.Engine, PythonEngineVersion.IronPython2);
        }

        [Test]
        public void CPytonEngineManagerAPITest()
        {
            var cPython3Eng = PythonEngineManager.Instance.CreateEngineProxy(
                Assembly.GetAssembly(typeof(DSCPython.CPythonEvaluator)), 
                PythonEngineVersion.CPython3);

            Assert.IsNotNull(cPython3Eng);

            cPython3Eng.OnEvaluationBegin((code, bindings, scopeSet) => { scopeSet("IN", new ArrayList { " ", "  " }); });

            int counter = 0;
            cPython3Eng.OnEvaluationEnd((state, code, bindings, scopeGet) => { counter++; });

            var inputM = cPython3Eng.GetInputMarshaler() as DataMarshaler;
            inputM.RegisterMarshaler((string s) => s.Length);

            var output = DSCPython.CPythonEvaluator.EvaluatePythonScript(
                "OUT = sum(IN)", new ArrayList(), new ArrayList());

            inputM.UnregisterMarshalerOfType<string>();

            Assert.AreEqual(3, output);

            var outputM = cPython3Eng.GetOutputMarshaler() as DataMarshaler;
            outputM.RegisterMarshaler((string s) => s.Length);

            cPython3Eng.OnEvaluationBegin((code, bindings, scopeSet) => { scopeSet("TEST", new ArrayList { "", " ", "  " }); });

            output = DSCPython.CPythonEvaluator.EvaluatePythonScript(
                "OUT = TEST",
                new ArrayList(),
                new ArrayList());

            outputM.UnregisterMarshalerOfType<string>();

            Assert.AreEqual(new[] { 0, 1, 2 }, output);
            Assert.AreEqual(2, counter);
        }
    }
}
