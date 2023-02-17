using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.Views;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Utilities for the Preferences panel
    /// </summary>
    static class PreferencesPanelUtilities
    {
        /// <summary>
        /// This static method will open the Preferences panel in the specified tab (if is provided) and expand the specified expander (if is provided)
        /// </summary>
        /// <param name="mainWindow">This window needs to be the DynamoView</param>
        /// <param name="location">Location in which the Preference panel will be shown</param>
        /// <param name="tabName">Tab name in which the preference panel will be opened</param>
        /// <param name="expanderName">Expander name that will be expanded (it has to be inside the tab)</param>
        public static void OpenPreferencesPanel(Window mainWindow, WindowStartupLocation location, string tabName = "", string expanderName = "")
        {
            var preferencesWindow = new PreferencesView(mainWindow as DynamoView);

            if(!string.IsNullOrEmpty(tabName))
            {
                var tabControl = preferencesWindow.preferencesTabControl;
                if (tabControl == null) return;
                var preferencesTab = (from TabItem tabItem in tabControl.Items
                                      where tabItem.Header.ToString().Equals(tabName)
                                      select tabItem).FirstOrDefault();
                if (preferencesTab == null) return;
                tabControl.SelectedItem = preferencesTab;

                if(!string.IsNullOrEmpty(expanderName))
                {
                    var listExpanders = WpfUtilities.ChildrenOfType<Expander>(preferencesTab.Content as Grid);
                    var tabExpander = (from expander in listExpanders
                                       where expander.Header.ToString().Equals(expanderName)
                                       select expander).FirstOrDefault();
                    if (tabExpander == null) return;
                    tabExpander.IsExpanded = true;
                }
            }
            preferencesWindow.WindowStartupLocation = location;
            preferencesWindow.ShowDialog();
        }
    }
}
