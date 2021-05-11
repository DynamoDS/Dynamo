using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    /// <summary>
    /// Interaction logic for IssueGroup.xaml
    /// </summary>
    public partial class IssueGroup : UserControl
    {
        #region DependencyProperties

        internal IEnumerable<NodeModel> IssueNodes
        {
            get { return (IEnumerable<NodeModel>)GetValue(IssueNodesProperty); }
            set { SetValue(IssueNodesProperty, value); }
        }

        public static readonly DependencyProperty IssueNodesProperty = DependencyProperty.Register(
            nameof(IssueNodes),
            typeof(IEnumerable<NodeModel>),
            typeof(IssueGroup)
        );

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(IssueGroup)
        );

        public string CallToAction
        {
            get { return (string)GetValue(CallToActionProperty); }
            set { SetValue(CallToActionProperty, value); }
        }

        public static readonly DependencyProperty CallToActionProperty = DependencyProperty.Register(
            nameof(CallToAction),
            typeof(string),
            typeof(IssueGroup)
        );

        public SeverityCodesEnum SeverityCode
        {
            get { return (SeverityCodesEnum)GetValue(SeverityCodeProperty); }
            set { SetValue(SeverityCodeProperty, value); }
        }

        public static readonly DependencyProperty SeverityCodeProperty = DependencyProperty.Register(
            nameof(SeverityCode),
            typeof(SeverityCodesEnum),
            typeof(IssueGroup)
        );

        #endregion DependencyProperties

        public IssueGroup()
        {
            InitializeComponent();
        }
    }
}
