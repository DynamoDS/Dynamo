using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo;
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
            Assert.AreEqual(true, PythonEngineSelector.lazy.IsValueCreated);
            Assert.AreEqual(evaluatorClass, PythonEngineSelector.Instance.IronPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineSelector.Instance.IronPythonEvaluationMethod);

            PythonEngineSelector.Instance.GetEvaluatorInfo(PythonEngineVersion.CPython3, out evaluatorClass, out evaluationMethod);
            Assert.AreEqual(evaluatorClass, PythonEngineSelector.Instance.CPythonEvaluatorClass);
            Assert.AreEqual(evaluationMethod, PythonEngineSelector.Instance.CPythonEvaluationMethod);

            Assert.AreEqual(true, PythonEngineSelector.Instance.IsCPythonEnabled);
            Assert.AreEqual(true, PythonEngineSelector.Instance.IsIronPythonEnabled);
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
    }
}
