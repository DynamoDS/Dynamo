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
        private const string NONE_DESCRIPTOR_GUID = "7b75fb44-43fd-4631-a878-29f4d5d8399a";

        #region Private Fields
        private static Uri linterViewHelpLink = new Uri("LintingViewExtension;LinterViewHelpDoc.html", UriKind.Relative);
        private LinterExtensionDescriptor activeLinter;
        private LinterManager linterManager;
        private ViewLoadedParams viewLoadedParams;
        private Dispatcher dispatcher;
        private LinterExtensionDescriptor defaultDescriptor;
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
                linterManager.ActiveLinter = activeLinter;
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
        
        /// <summary>
        /// The default descriptor is used if no linter is needed for the graph. This is a dummy descriptor that has no extension associated to it.
        /// This property returns the descriptor as a list for binding purposes.
        /// </summary>
        public IList<LinterExtensionDescriptor> DefaultDescriptor { get => new List<LinterExtensionDescriptor> { defaultDescriptor }; }

        #endregion

        public LinterViewModel(LinterManager linterManager, ViewLoadedParams viewLoadedParams)
        {
            this.linterManager = linterManager ?? throw new ArgumentNullException(nameof(linterManager));
            this.viewLoadedParams = viewLoadedParams;
            this.dispatcher = viewLoadedParams.DynamoWindow.Dispatcher;
            InitializeCommands();
            CreateDefaultDummyLinterDescriptor();

            // If there are no active linter we set it to the default one.
            if (this.linterManager.ActiveLinter is null)
            {
                ActiveLinter = defaultDescriptor;
            }

            NodeIssues = new ObservableCollection<IRuleIssue>();
            GraphIssues = new ObservableCollection<IRuleIssue>();
            this.linterManager.RuleEvaluationResults.CollectionChanged += RuleEvaluationResultsCollectionChanged;
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

                issue.AffectedNodes.Add(issueNode);
                return;
            }

            var newIssue = new NodeRuleIssue(ruleId, GetLinterRule(ruleId) as NodeLinterRule);
            newIssue.AddAffectedNodes(new List<NodeModel> { issueNode });

            this.dispatcher.Invoke(() => { NodeIssues.Add(newIssue); });
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

                    issue.AffectedNodes.Add(issueNode);
                    return;
                }
            }

            var newIssue = new GraphRuleIssue(ruleId, GetLinterRule(ruleId) as GraphLinterRule);
            newIssue.AddAffectedNodes(issueNodes);

            this.dispatcher.Invoke(() => { GraphIssues.Add(newIssue); });
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

            issue.AffectedNodes
                .Remove(issue.AffectedNodes
                    .Where(x => x.GUID.ToString() == issueNodeId)
                    .FirstOrDefault());

            if (issue.AffectedNodes.Count == 0)
            {
                this.dispatcher.Invoke(() => { NodeIssues.Remove(issue); });
            }
        }

        private void RemoveGraphIssue(string ruleId)
        {
            var issue = GraphIssues.Where(x => x.Id == ruleId).FirstOrDefault();
            if (issue is null) return;

            this.dispatcher.Invoke(() => { GraphIssues.Remove(issue); });
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
                    this.GraphIssues.Clear();
                    this.NodeIssues.Clear();
                    return;

                default:
                    break;
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

        private void CreateDefaultDummyLinterDescriptor()
        {
            defaultDescriptor = new LinterExtensionDescriptor(NONE_DESCRIPTOR_GUID, Properties.Resources.NoneLinterDescriptorName);
        }
        #endregion
    }
}
