using System;
using System.Collections.Generic;
using Dynamo.Configuration;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dynamo.Wpf.Views.Debug
{
    /// <summary>
    /// Interaction logic for DebugModesWindow.xaml
    /// </summary>
    public partial class DebugModesWindow : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DebugModesWindow()
        {
            InitializeComponent();

            var items = new List<DebugModeListItem>();
            foreach (var entry in DebugModes.GetDebugModes())
            {
                items.Add(new DebugModeListItem() {
                    Name = entry.Value.Name,
                    Enabled = entry.Value.Enabled
                });
            }
            CheckList.ItemsSource = items;
        }
        private void OnDebugModeItemClick(object sender, RoutedEventArgs e)
        {
            var listItem = (System.Windows.Controls.ListBoxItem)sender;
            if (null == listItem)
            {
                throw new Exception("Invalid Control type found. Expected ListBoxItem");
            }
            var dataContext = (DebugModeListItem)listItem.DataContext;
            if (null == dataContext)
            {
                throw new Exception("Invalid data context type found. Expected DebugModeItem");
            }
            var debugMode = DebugModes.GetDebugMode(dataContext.Name);
            if (null == debugMode)
            {
                throw new Exception("Invalid debug mode found");      
            }
            SelectedDbgMode.Text = debugMode.Description;
        }
        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            foreach (DebugModeListItem item in CheckList.Items)
            {
                DebugModes.SetDebugModeEnabled(item.Name, item.Enabled);
            }
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e) {}

        private class DebugModeListItem
        {
            public string Name { get; set; }

            public bool Enabled { get; set; }
        }
    }
}
