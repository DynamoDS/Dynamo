using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

using Dynamo.Models;


namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for StandardPanel.xaml
    /// </summary>
    public partial class StandardPanel : UserControl
    {
        public StandardPanel()
        {
            InitializeComponent();
        }
        private void ActionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            queryActionMethods.ItemsSource = (this.DataContext as ClassDetails).ActionMembers;
            action.FontWeight = FontWeights.UltraBold;
            query.FontWeight = FontWeights.Normal;
        }

        private void QueryMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            queryActionMethods.ItemsSource = (this.DataContext as ClassDetails).QueryMembers;
            action.FontWeight = FontWeights.Normal;
            query.FontWeight = FontWeights.UltraBold;
        }

        private void queryActionMethods_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            queryActionMethods.ItemsSource = (this.DataContext as ClassDetails).QueryMembers;
        }
    }
}
