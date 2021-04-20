using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Dynamo.Core;
using Dynamo.Linting;
using Dynamo.Linting.Rules;
using Dynamo.LintingViewExtension.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.LintingViewExtension
{
    public class LinterViewModel : NotificationObject
    {
        private LinterExtensionDescriptor activeLinter;
        public LinterManager LinterManager { get; }
        public ViewLoadedParams ViewLoadedParams { get; }
        public ObservableCollection<IRuleIssue> NodeIssues { get; set; }
        public ObservableCollection<IRuleIssue> GraphIssues { get; set; }
        public DelegateCommand<string> SelectIssueNodeCommand { get; private set; }

        public LinterExtensionDescriptor ActiveLinter
        {
            get { return activeLinter; }
            set
            {
                if (activeLinter == value)
                    return;

                activeLinter = value;
                LinterManager.ActiveLinter = activeLinter;
                RaisePropertyChanged(nameof(ActiveLinter));
            }
        }

        public LinterViewModel(LinterManager linterManager, ViewLoadedParams viewLoadedParams)
        {
            LinterManager = linterManager ?? throw new ArgumentNullException(nameof(linterManager));
            ViewLoadedParams = viewLoadedParams;
            InitializeCommands();

            NodeIssues = new ObservableCollection<IRuleIssue>();
            GraphIssues = new ObservableCollection<IRuleIssue>();
            LinterManager.RuleEvaluationResults.CollectionChanged += RuleEvaluationResultsCollectionChanged;
        }

        private void InitializeCommands()
        {
            this.SelectIssueNodeCommand = new DelegateCommand<string>(this.SelectIssueNodeCommandExecute);
        }

        private void SelectIssueNodeCommandExecute(string nodeId)
        {
            var nodes = ViewLoadedParams.CurrentWorkspaceModel.Nodes;
            if (nodes is null || !nodes.Any()) { return; }

            var selectedNode = nodes.Where(x => x.GUID.ToString() == nodeId).FirstOrDefault();
            if (selectedNode is null) { return; }

            var cmd = new DynamoModel.SelectInRegionCommand(selectedNode.Rect, false);
            this.ViewLoadedParams.CommandExecutive.ExecuteCommand(cmd, null, null);

            (this.ViewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).FitViewCommand.Execute(null);
        }

        private void AddNewNodeIssue(string issueNodeId, string ruleId)
        {
            var issue = NodeIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (!(issue is null))
            {
                if (issue.AffectedNodes.Contains(issueNodeId))
                    return;

                issue.AffectedNodes.Add(issueNodeId);
                return;
            }

            var newIssue = new NodeRuleIssue(
                ruleId, GetLinterRule(ruleId) as NodeLinterRule);
            newIssue.AddResult(new List<string> { issueNodeId });

            NodeIssues.Add(newIssue);
        }

        private void AddNewGraphIssue(List<string> issueNodeIds, string ruleId)
        {
            var issue = GraphIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (!(issue is null))
            {
                foreach (var issueNode in issueNodeIds)
                {
                    if (issue.AffectedNodes.Contains(issueNode))
                        continue;

                    issue.AffectedNodes.Add(issueNode);
                    return;
                }
            }

            var newIssue = new GraphRuleIssue(
                ruleId, GetLinterRule(ruleId) as GraphLinterRule);
            newIssue.AddResult(issueNodeIds);

            GraphIssues.Add(newIssue);
        }

        private void RemoveNodeIssue(string issueNodeId, string ruleId)
        {
            var issue = NodeIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (issue is null ||
                !issue.AffectedNodes.Contains(issueNodeId))
                return;

            issue.AffectedNodes.Remove(issueNodeId);

            if (issue.AffectedNodes.Count == 0)
                NodeIssues.Remove(issue);
        }

        private void RemoveGraphIssue(List<string> issueNodeIds, string ruleId)
        {
            var issue = GraphIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (issue is null)
                return;

            GraphIssues.Remove(issue);
        }

        private LinterRule GetLinterRule(string id)
        {
            var linterExt = LinterManager.GetLinterExtension(ActiveLinter);
            return linterExt.
                LinterRules.
                Where(x => x.Id == id).
                FirstOrDefault();
        }

        private void RuleEvaluationResultsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        if (item is NodeRuleEvaluationResult nodeRuleEvaluationResult)
                            AddNewNodeIssue(nodeRuleEvaluationResult.NodeId, nodeRuleEvaluationResult.RuleId);
                        else if (item is GraphRuleEvaluationResult graphRuleEvaluationResult)
                            AddNewGraphIssue(graphRuleEvaluationResult.NodeIds.ToList(), graphRuleEvaluationResult.RuleId);
                    }
                    return;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        if (item is NodeRuleEvaluationResult nodeRuleEvaluationResult)
                            RemoveNodeIssue(nodeRuleEvaluationResult.NodeId, nodeRuleEvaluationResult.RuleId);
                        else if (item is GraphRuleEvaluationResult graphRuleEvaluationResult)
                            RemoveGraphIssue(graphRuleEvaluationResult.NodeIds.ToList(), graphRuleEvaluationResult.RuleId);
                    }
                    return;

                default:
                    break;
            }
        }
    }
}
