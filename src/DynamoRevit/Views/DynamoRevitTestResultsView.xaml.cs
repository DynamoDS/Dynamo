using System;
using System.Windows;
using Dynamo.Utilities;

namespace Dynamo.Applications
{
    /// <summary>
    /// Interaction logic for DynamoRevitTestResultsView.xaml
    /// </summary>
    public partial class DynamoRevitTestResultsView : Window
    {
        public DynamoRevitTestResultsView()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DynamoRevitTestResultsView_OnClosed(object sender, EventArgs e)
        {
            IdlePromise.ExecuteOnIdle(delegate
            {
                DynamoLogger.Instance.FinishLogging();
            });
        }
    }
}
