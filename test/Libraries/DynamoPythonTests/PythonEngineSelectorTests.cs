using System;
using System.Collections;
using System.Linq;
using Dynamo;
using Dynamo.PythonServices;
using Dynamo.PythonServices.EventHandlers;
using Dynamo.Utilities;
using NUnit.Framework;
using PythonNodeModels;
using static Dynamo.Models.DynamoModel;

namespace DynamoPythonTests
{
    [TestFixture]
    class PythonEngineSelectorTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test will cover the use case of the API to query certain Python engine ability for evaluation
        /// </summary>
        [Test]
        public void TestEngineSelectorInitialization()
        {
            Assert.AreEqual(true, PythonEngineManager.Instance.AvailableEngines.Any(x => x.Name == PythonEngineManager.PythonNet3EngineName));
            Assert.AreEqual(false, PythonEngineManager.Instance.AvailableEngines.Any(x => x.Name == PythonEngineManager.IronPython2EngineName));
        }

        [Test]
        public void CanCopydAndPasteAndUndoPythonEngine()
        {
            var pyNode = new PythonNode();

            CurrentDynamoModel.ExecuteCommand(new Dynamo.Models.DynamoModel.CreateNodeCommand(pyNode, 0, 0, false, false));
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            pyNode.EngineName = PythonEngineManager.PythonNet3EngineName;
            CurrentDynamoModel.AddToSelection(pyNode);
           
            CurrentDynamoModel.Copy();
            Assert.AreEqual(1, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<PythonNode>().All(x => x.EngineName == PythonEngineManager.PythonNet3EngineName));

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            pyNode.EngineName = PythonEngineManager.IronPython2EngineName;

            CurrentDynamoModel.ExecuteCommand(
                 new UpdateModelValueCommand(
                     Guid.Empty, pyNode.GUID, nameof(PythonNode.EngineName), PythonEngineManager.PythonNet3EngineName));
            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.PythonNet3EngineName);

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));

            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.IronPython2EngineName);
        }

        [Test]
        public void CPytonEngineManagerAPITest()
        {
            var pythonNet3Eng = PythonEngineManager.Instance.AvailableEngines.FirstOrDefault(x => x.Name == PythonEngineManager.PythonNet3EngineName);

            Assert.IsNotNull(pythonNet3Eng);

            EvaluationStartedEventHandler start1 = ((code, bindings, scopeSet) => { scopeSet("IN", new ArrayList { " ", "  " }); });
            pythonNet3Eng.EvaluationStarted += start1;

            int counter = 0;
            EvaluationFinishedEventHandler end = ((state, code, bindings, scopeGet) => { counter++; });
            pythonNet3Eng.EvaluationFinished += end;

            var inputM = pythonNet3Eng.InputDataMarshaler as DataMarshaler;
            inputM.RegisterMarshaler((string s) => s.Length);

            var output = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(
                "OUT = sum(IN)", new ArrayList(), new ArrayList());

            inputM.UnregisterMarshalerOfType<string>();

            Assert.AreEqual(3, output);

            var outputM = pythonNet3Eng.OutputDataMarshaler as DataMarshaler;
            outputM.RegisterMarshaler((string s) => s.Length);

            EvaluationStartedEventHandler start2 = ((code, bindings, scopeSet) => { scopeSet("TEST", new ArrayList { "", " ", "  " }); });
            pythonNet3Eng.EvaluationStarted += start2;

            output = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(
                "OUT = TEST",
                new ArrayList(),
                new ArrayList());

            outputM.UnregisterMarshalerOfType<string>();

            Assert.AreEqual(new[] { 0, 1, 2 }, output);
            Assert.AreEqual(2, counter);

            pythonNet3Eng.EvaluationStarted -= start1;
            pythonNet3Eng.EvaluationStarted -= start2;
            pythonNet3Eng.EvaluationFinished -= end;
        }
    }
}
