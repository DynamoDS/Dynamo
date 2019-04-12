using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    /// <summary>
    /// This class, combined with ActionNodeModel, allows you to inject arbitrary testing code into a
    /// NodeModel without having to build an entire separate Assembly.
    /// 
    /// Note this class is public so it can be loaded into a ProtoCore.Core.
    /// </summary>
    public static class ActionDict
    {
        private static readonly Dictionary<string, Action> actions = new Dictionary<string, Action>();
 
        internal static string Register(Action action)
        {
            var id = Guid.NewGuid();

            actions.Add(id.ToString(), action);

            return id.ToString();
        }

        public static bool Eval(string id)
        {
            actions[id]();
            return true;
        }
    }

    /// <summary>
    /// Run an arbitrary action as a NodeModel
    /// </summary>
    internal class ActionNodeModel : NodeModel
    {
        private readonly string actionId;

        public ActionNodeModel(Action action, bool periodic = false)
        {
            this.actionId = ActionDict.Register(action);
            this.CanUpdatePeriodically = periodic;

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", "")));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildFunctionCall<string, bool>(ActionDict.Eval,
                new List<AssociativeNode> { AstFactory.BuildStringNode(actionId) });

            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    internal class PeriodicEvaluationTests : DynamoModelTestBase
    {
        #region Overrides

        /// <summary>
        /// We ask for the current assembly, including ActionDict, to be loaded into
        /// the current ProtoCore
        /// </summary>
        /// <param name="libraries"></param>
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DynamoCoreTests.dll");
        }

        #endregion

        #region Helper methods

        private HomeWorkspaceModel Workspace
        {
            get
            {
                return CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            }
        }

        #endregion

        [Test]
        [Category("Failure")] //LC: I don't understand how this test isn't incredibly sensitive to execution timing
        public void StartPeriodicEvaluation_CanCompleteMultipleRuns()
        {
            Workspace.RunSettings.RunType = RunType.Periodic;
            Workspace.RunSettings.RunPeriod = 90;

            var count = 0;

            Workspace.AddAndRegisterNode(new ActionNodeModel(() =>
            {
                count++;
            }, true));

            Workspace.StartPeriodicEvaluation();

            Thread.Sleep(1000);

            // There should be 11 runs initiated in 1000 ms
            // as the RunPeriod is 100 ms
            Assert.AreEqual(11, count);
        }

        [Test]
        //LC: I don't understand how this test isn't incredibly sensitive to execution timing
        public void StartPeriodicEvaluation_CompletesFewerRunsWhenRunTimeIsGreaterThanEvaluationTime()
        {
            Workspace.RunSettings.RunType = RunType.Periodic;
            Workspace.RunSettings.RunPeriod = 100;

            var count = 0;

            Workspace.AddAndRegisterNode(new ActionNodeModel(() =>
            {
                count++;
                Thread.Sleep(200);
            }, true));

            Workspace.StartPeriodicEvaluation();

            Thread.Sleep(1000);

            // There should be 6 200 ms runs initiated in a 1000 ms
            // even with the run period as 
            Assert.AreEqual(6, count);
        }

        [Test]
        [Category("Failure")]
        public void DynamoModel_OpenFileFromPath_StartsPeriodicEvaluation()
        {
            // asser that SampleLibraryZeroTouch must be loaded
            Assert.IsTrue(CurrentDynamoModel.LibraryServices.ImportedLibraries.Any(x => x.Contains("SampleLibraryZeroTouch")));

            var ws = Open<HomeWorkspaceModel>(TestDirectory, "core", "periodic", "simple.dyn");

            Thread.Sleep(2000);

            Assert.Greater(ws.EvaluationCount, 1);
        }

    }
}
