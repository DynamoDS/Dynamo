using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;
using Dynamo.Models;
using Dynamo.Scheduler;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Dynamo.Tests.Linting
{
    [TestFixture]
    class LinterManagerTests
    {
        const string MOCK_GUID = "358321af-2633-4697-b475-81632582eba0";
        const string MOCK_Name = "Test Extension";
        const string MOCK_RULE_ID = "1";

        private Mock<LinterExtensionBase> mockExtension;
        private Mock<NodeLinterRule> mockRule;

        private DynamoModel model;

        [SetUp]
        public void Init()
        {
            mockExtension = new Mock<LinterExtensionBase>() { CallBase = true };
            mockRule = new Mock<NodeLinterRule> { CallBase = true };

            // Setup mock rule
            mockRule.Setup(r => r.Id).Returns(MOCK_RULE_ID);
            
            // Setup mock LinterExtension
            mockExtension.Setup(e => e.UniqueId).Returns(MOCK_GUID);
            mockExtension.Setup(e => e.Name).Returns(MOCK_Name);

            model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    Extensions = new List<IExtension> { mockExtension.Object },
                    ProcessMode = TaskProcessMode.Synchronous
                });
        }

        [Test]
        public void LinterExtensionsGetsAddedToManager()
        {
            // Assert
            Assert.That(model.LinterManager.AvailableLinters.Any(x => x.Id == MOCK_GUID));
        }


        [Test]
        public void CanActivateLinter()
        {
            // Arrange
            var activeLinterBefore = model.LinterManager.ActiveLinter;
            Assert.That(!model.LinterManager.IsExtensionActive(MOCK_GUID));

            // Act
            model.LinterManager.SetActiveLinter(model.LinterManager.AvailableLinters
                .Where(x => x.Id == MOCK_GUID)
                .FirstOrDefault());

            // Assert
            Assert.That(model.LinterManager.ActiveLinter != activeLinterBefore);
            Assert.That(model.LinterManager.ActiveLinter.Id == MOCK_GUID);
            Assert.That(model.LinterManager.IsExtensionActive(MOCK_GUID));
        }


        [Test]
        public void LinterManagerStoresRuleEvaluationResults()
        {
            // Arrange
            var evaluationResultsBefore = model.LinterManager.RuleEvaluationResults.ToList();
            SetupNodeNameChangedRule(mockRule);

            mockExtension.Object.AddLinterRule(mockRule.Object);

            // Act
            var failureNode = new DummyNode();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(failureNode, 0, 0, false, false));

            // When setting a linter as the active linter, that linters Activate() gets called
            // which will initialize the rules using the init function. As we only have one mock rule
            // that checks if the node name is "NewNodeName" no failed evaluation results should be created here.
            model.LinterManager.SetActiveLinter(model.LinterManager.AvailableLinters
                .Where(x => x.Id == MOCK_GUID)
                .FirstOrDefault());

            // Update graph nodes name to trigger the node rule evaluation
            failureNode.Name = "NewNodeName";

            // Assert
            CollectionAssert.IsEmpty(evaluationResultsBefore);
            Assert.That(model.LinterManager.RuleEvaluationResults.Count == 1);
            Assert.That(model.LinterManager.RuleEvaluationResults.
                Any(x => x.RuleId == MOCK_RULE_ID));

            Assert.That(model.LinterManager.RuleEvaluationResults.
                Any(x => x is NodeRuleEvaluationResult));

            Assert.That(model.LinterManager.RuleEvaluationResults.
                Where(x => x is NodeRuleEvaluationResult).
                Cast<NodeRuleEvaluationResult>().
                Any(x => x.NodeId == failureNode.GUID.ToString()));
        }

        [Test]
        public void VerifyThatOnlyRulesFromActiveLinterGetsEvaluated()
        {
            // Arrange
            var secondLinterExtId = "cddab693-9f38-4a66-a600-a758f2c6c817";
            var secondLinterExtName = "Another Test Extension";
            var secondLinterExt = new Mock<LinterExtensionBase>() { CallBase = true };
            secondLinterExt.Setup(e => e.UniqueId).Returns(secondLinterExtId);
            secondLinterExt.Setup(e => e.Name).Returns(secondLinterExtName);

            var nodeRuleId = "999";
            var nodeRule = new Mock<NodeLinterRule> { CallBase = true };
            nodeRule.Setup(r => r.Id).Returns(nodeRuleId);

            SetupNodeNameChangedRule(mockRule);
            SetupNodeNameChangedRule(nodeRule);

            mockExtension.Object.AddLinterRule(mockRule.Object);
            secondLinterExt.Object.AddLinterRule(nodeRule.Object);

            // Act
            // we need to call this to invoke LinterExtensionReady which will add the
            // extension to the LinterManager
            secondLinterExt.Object.InitializeBase(model.LinterManager);
            secondLinterExt.Object.Startup(mockExtension.Object.ReadyParamsRef.StartupParams);
            model.ExtensionManager.Add(secondLinterExt.Object);

            var failureNode = new DummyNode();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(failureNode, 0, 0, false, false));

            // First we set the active linter to the mock extension created in SetUp, this will initialize that extension
            // and subscribe everything. Then we change the active linter again but to the mock extension created in this test
            // this is to simulate a change in the active linter and to make sure we are only getting results from the active linter
            // even though two linters have been initialized.
            model.LinterManager.SetActiveLinter(model.LinterManager.AvailableLinters
                .Where(x => x.Id == MOCK_GUID)
                .FirstOrDefault());

            model.LinterManager.SetActiveLinter(model.LinterManager.AvailableLinters
                .Where(x => x.Id == secondLinterExtId)
                .FirstOrDefault());

            failureNode.Name = "NewNodeName";

            // Assert
            Assert.That(model.LinterManager.AvailableLinters.Count == 3);
            Assert.That(model.LinterManager.RuleEvaluationResults.Count == 1);
            Assert.That(model.LinterManager.RuleEvaluationResults.
                Any(x => x.RuleId == nodeRuleId));

            Assert.That(model.LinterManager.RuleEvaluationResults.
                Any(x => x is NodeRuleEvaluationResult));

            Assert.That(model.LinterManager.RuleEvaluationResults.
                Where(x => x is NodeRuleEvaluationResult).
                Cast<NodeRuleEvaluationResult>().
                Any(x => x.NodeId == failureNode.GUID.ToString()));
        }

        private void SetupNodeNameChangedRule(Mock<NodeLinterRule> rule)
        {
            rule.Setup(x => x.EvaluationTriggerEvents).
                Returns(new List<string> { nameof(NodeModel.Name) });

            rule.Protected().Setup<RuleEvaluationStatusEnum>("EvaluateFunction", ItExpr.IsAny<NodeModel>(), ItExpr.IsAny<string>()).
                Returns((NodeModel node, string changedEvent) => NodeRuleEvaluateFunction(node, changedEvent));

            rule.Protected().Setup<List<Tuple<RuleEvaluationStatusEnum, string>>>("InitFunction", ItExpr.IsAny<WorkspaceModel>()).
                Returns((WorkspaceModel wm) => NodeRuleInitFuction(wm));
        }

        private RuleEvaluationStatusEnum NodeRuleEvaluateFunction(NodeModel node, string changedEvent)
        {
            return node.Name == "NewNodeName" ? 
                RuleEvaluationStatusEnum.Failed : 
                RuleEvaluationStatusEnum.Passed;
        }

        private List<Tuple<RuleEvaluationStatusEnum, string>> NodeRuleInitFuction(WorkspaceModel wm)
        {
            var results = new List<Tuple<RuleEvaluationStatusEnum, string>>();
            foreach (var node in wm.Nodes)
            {
                var evaluationStatus = NodeRuleEvaluateFunction(node, "init");
                if (evaluationStatus == RuleEvaluationStatusEnum.Passed)
                    continue;

                var valueTuple = Tuple.Create(evaluationStatus, node.GUID.ToString());
                results.Add(valueTuple);
            }
            return results;
        }

    }
}
