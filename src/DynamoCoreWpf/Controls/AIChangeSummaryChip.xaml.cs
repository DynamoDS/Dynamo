using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Canvas chip summarizing the latest Autodesk Assistant turn.
    /// </summary>
    internal partial class AIChangeSummaryChip : UserControl
    {
        public AIChangeSummaryChip()
        {
            InitializeComponent();
        }

        /// <summary>The sentence Narrator reads. Exposed for tests.</summary>
        internal Button MessageButton => messageButton;

        /// <summary>"Show". Exposed for tests.</summary>
        internal Button ShowButton => showButton;

        /// <summary>"Dismiss". Exposed for tests.</summary>
        internal Button DismissButton => dismissButton;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            chip.DataContext = (e.NewValue as WorkspaceViewModel)?.AIChangeSummaryChip;
        }

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnChipButtonGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not Button button || !ReferenceEquals(e.OriginalSource, button))
            {
                return;
            }

            var peer = UIElementAutomationPeer.FromElement(button)
                ?? UIElementAutomationPeer.CreatePeerForElement(button);
            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                AutomationProperties.GetName(button),
                "AIChangeSummaryChip");
        }
    }
}
