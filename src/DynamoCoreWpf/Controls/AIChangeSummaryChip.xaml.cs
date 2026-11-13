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

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            chip.DataContext = (e.NewValue as WorkspaceViewModel)?.AIChangeSummaryChip;
        }

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMessageGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, messageButton))
            {
                return;
            }

            var peer = UIElementAutomationPeer.FromElement(messageButton)
                ?? UIElementAutomationPeer.CreatePeerForElement(messageButton);
            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                AutomationProperties.GetName(messageButton),
                "AIChangeSummaryChip");
        }
    }
}
