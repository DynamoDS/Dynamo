using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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

        internal IEnumerable<string> NodeIds
        {
            get { return (IEnumerable<string>)GetValue(ResultsProperty); }
            set { SetValue(ResultsProperty, value); }
        }

        public static readonly DependencyProperty ResultsProperty = DependencyProperty.Register(
            nameof(NodeIds),
            typeof(IEnumerable<string>),
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
