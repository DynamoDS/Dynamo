using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Linting;
using Dynamo.Linting.Rules;
using Dynamo.LintingViewExtension.Controls;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.LintingViewExtension
{
    public class LinterViewModel : NotificationObject
    {
        #region Private Fields
        private static Uri linterViewHelpLink = new Uri("LintingViewExtension;LinterViewHelpDoc.html", UriKind.Relative);
        private LinterExtensionDescriptor activeLinter;
        private LinterManager linterManager;
        private ViewLoadedParams viewLoadedParams;
        private Dispatcher dispatcher;
        #endregion

        #region Public Properties
        /// <summary>
        /// Collection of node issues, this is used to bind to the UI
        /// </summary>
        public ObservableCollection<IRuleIssue> NodeIssues { get; set; }

        /// <summary>
        /// Collection of graph issues, this is used to bind to the UI
        /// </summary>
        public ObservableCollection<IRuleIssue> GraphIssues { get; set; }

        /// <summary>
        /// Command to select a node in the workspace from the linter view
        /// </summary>
        public DelegateCommand SelectIssueNodeCommand { get; private set; }

        /// <summary>
        /// Command to open a help page in the documentation browser
        /// </summary>
        public DelegateCommand OpenDocumentationBrowserCommand { get; private set; }

        /// <summary>
        /// The selected linter, used to bind to the UI and activate a linter on the linter manager
        /// </summary>
        public LinterExtensionDescriptor ActiveLinter
        {
            get { return activeLinter; }
            set
            {
                if (activeLinter == value)
                    return;

                activeLinter = value;
                linterManager.SetActiveLinter(activeLinter);
                RaisePropertyChanged(nameof(ActiveLinter));
            }
        }

        /// <summary>
        /// Collection of linters available on the LinterManager
        /// </summary>
        public List<LinterExtensionDescriptor> AvailableLinters
        {
            get => linterManager.AvailableLinters.ToList();
        }
        
        #endregion

        public LinterViewModel(LinterManager linterManager, ViewLoadedParams viewLoadedParams)
        {
            this.linterManager = linterManager ?? throw new ArgumentNullException(nameof(linterManager));
            this.viewLoadedParams = viewLoadedParams;
            dispatcher = viewLoadedParams.DynamoWindow.Dispatcher;
            InitializeCommands();

            this.activeLinter = this.linterManager.ActiveLinter;

            NodeIssues = new ObservableCollection<IRuleIssue>();
            GraphIssues = new ObservableCollection<IRuleIssue>();
            this.linterManager.RuleEvaluationResults.CollectionChanged += RuleEvaluationResultsCollectionChanged;
            this.linterManager.PropertyChanged += OnLinterManagerPropertyChange;
        }

        #region Private methods
        private void InitializeCommands()
        {
            this.SelectIssueNodeCommand = new DelegateCommand(this.SelectIssueNodeCommandExecute);
            this.OpenDocumentationBrowserCommand = new DelegateCommand(this.OpenDocumentationBrowserCommandExecute);
        }

        private void OpenDocumentationBrowserCommandExecute(object param)
        {
            viewLoadedParams.ViewModelCommandExecutive.OpenDocumentationLinkCommand(linterViewHelpLink);
        }

        private void SelectIssueNodeCommandExecute(object nodeId)
        {
            if (!(nodeId is string id)) return;

            var nodes = viewLoadedParams.CurrentWorkspaceModel.Nodes;
            if (nodes is null || !nodes.Any()) { return; }

            var selectedNode = nodes.Where(x => x.GUID.ToString() == id).FirstOrDefault();
            if (selectedNode is null) { return; }

            var cmd = new DynamoModel.SelectInRegionCommand(selectedNode.Rect, false);
            this.viewLoadedParams.CommandExecutive.ExecuteCommand(cmd, null, null);

            this.viewLoadedParams.ViewModelCommandExecutive.FitViewCommand();
        }

        private void AddNewNodeIssue(string issueNodeId, string ruleId)
        {
            var issueNode = NodeFromId(issueNodeId);

            var issue = NodeIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (!(issue is null))
            {
                if (issue.AffectedNodes.Contains(issueNode)) return;

                dispatcher.Invoke(() => { issue.AffectedNodes.Add(issueNode); });
                return;
            }

            var newIssue = new NodeRuleIssue(ruleId, GetLinterRule(ruleId) as NodeLinterRule);

            dispatcher.Invoke(() =>
            {
                newIssue.AddAffectedNodes(new List<NodeModel> { issueNode });
                NodeIssues.Add(newIssue);
            });
        }

        private void AddNewGraphIssue(List<string> issueNodeIds, string ruleId)
        {
            var issueNodes = issueNodeIds.Any() ? 
                issueNodeIds.Select(i => NodeFromId(i)).ToList() : 
                new List<NodeModel>();

            var issue = GraphIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (!(issue is null))
            {
                foreach (var issueNode in issueNodes)
                {
                    if (issue.AffectedNodes.Contains(issueNode)) continue;

                    dispatcher.Invoke(() => { issue.AffectedNodes.Add(issueNode); });
                    return;
                }
            }

            var newIssue = new GraphRuleIssue(ruleId, GetLinterRule(ruleId) as GraphLinterRule);
            
            dispatcher.Invoke(() => {
                newIssue.AddAffectedNodes(issueNodes);
                GraphIssues.Add(newIssue);
            });
        }

        private void RemoveNodeIssue(string issueNodeId, string ruleId)
        {
            var issueNode = NodeFromId(issueNodeId);
            var issue = NodeIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (issue is null ||
                !issue.AffectedNodes.Any(n => n.GUID.ToString() == issueNodeId))
            {
                return;
            }

            dispatcher.Invoke(() => {
                issue.AffectedNodes
                    .Remove(issue.AffectedNodes
                        .Where(x => x.GUID.ToString() == issueNodeId)
                        .FirstOrDefault());

                if (issue.AffectedNodes.Count == 0)
                {
                    NodeIssues.Remove(issue);
                }
            });
        }

        private void RemoveGraphIssue(string ruleId)
        {
            var issue = GraphIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (issue is null) return;

            dispatcher.Invoke(() => { GraphIssues.Remove(issue); });
        }

        private LinterRule GetLinterRule(string id)
        {
            if(linterManager.TryGetLinterExtension(ActiveLinter, out LinterExtensionBase linterExt))
            {
                return linterExt.
                    LinterRules.
                    Where(x => x.Id == id).
                    FirstOrDefault();
            }

            return null;
        }

        private void RuleEvaluationResultsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        if (item is NodeRuleEvaluationResult nodeRuleEvaluationResult)
                        {
                            AddNewNodeIssue(nodeRuleEvaluationResult.NodeId, nodeRuleEvaluationResult.RuleId);
                        }

                        else if (item is GraphRuleEvaluationResult graphRuleEvaluationResult)
                        {
                            AddNewGraphIssue(graphRuleEvaluationResult.NodeIds.ToList(), graphRuleEvaluationResult.RuleId);
                        }
                    }
                    return;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        if (item is NodeRuleEvaluationResult nodeRuleEvaluationResult)
                        {
                            RemoveNodeIssue(nodeRuleEvaluationResult.NodeId, nodeRuleEvaluationResult.RuleId);
                        }

                        else if (item is GraphRuleEvaluationResult graphRuleEvaluationResult)
                        {
                            RemoveGraphIssue(graphRuleEvaluationResult.RuleId);
                        }
                    }
                    return;

                case NotifyCollectionChangedAction.Reset:
                    dispatcher.Invoke(() =>
                    {
                        GraphIssues.Clear();
                        NodeIssues.Clear();
                    });
                    return;

                default:
                    break;
            }
        }

        private void OnLinterManagerPropertyChange(object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if (e.PropertyName == nameof(linterManager.ActiveLinter))
            {
                ActiveLinter = (sender as LinterManager)?.ActiveLinter;
            }
        }
        
        private NodeModel NodeFromId(string nodeId)
        {
            if (nodeId is null)
            {
                throw new ArgumentNullException(nameof(nodeId));
            }

            var node = viewLoadedParams.CurrentWorkspaceModel
                .Nodes
                .Where(n => n.GUID.ToString() == nodeId)
                .FirstOrDefault();

            return node;
        }

        public void Dispose()
        {
            this.linterManager.RuleEvaluationResults.CollectionChanged -= RuleEvaluationResultsCollectionChanged;
            this.linterManager.PropertyChanged -= OnLinterManagerPropertyChange;
        }
        #endregion
    }
}
