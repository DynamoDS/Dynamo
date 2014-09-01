using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Nodes.Search;

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
        private void OnActionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: change style to correct one. 
            // For now it is about bold font for selected collection.
            addCategoryList.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;
            //action.FontWeight = FontWeights.UltraBold;
            //query.FontWeight = FontWeights.Normal;
        }

        private void OnQueryMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // TODO: change style to correct one. 
            // For now it is about bold font for selected collection.
            addCategoryList.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;
            //action.FontWeight = FontWeights.Normal;
            //query.FontWeight = FontWeights.UltraBold;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            addCategoryList.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //TODO: Execute node class.
            MessageBox.Show("test");
        }

        private void OnListBoxItemMouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem fromSender = sender as ListBoxItem;
            libraryToolTipPopup.PlacementTarget = fromSender;
            libraryToolTipPopup.SetDataContext(fromSender.DataContext);
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }

        private void GridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var classInfo = this.DataContext as ClassInformation;
            if ((classInfo.CreateMembers as List<BrowserInternalElement>).Count > 0)
            {
                // Case when we have Create members.
                topCategoryList.ItemsSource = (this.DataContext as ClassInformation).CreateMembers;
                if ((classInfo.QueryMembers as List<BrowserInternalElement>).Count > 0)
                {
                    // Case when we have Query members.
                    // Here we should add Query textblock dynamically. 
                    TextBlock queryHeader = new TextBlock();
                    queryHeader.Text = "QUERY";
                    queryHeader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
                    queryHeader.PreviewMouseLeftButtonDown += OnQueryMouseLeftButtonDown;
                    addCategoryHeaders.Children.Add(queryHeader);
                }

                if ((classInfo.ActionMembers as List<BrowserInternalElement>).Count > 0)
                {
                    // Case when we have Action members.
                    // Here we should add Action textblock dynamically.
                    TextBlock actionHeader = new TextBlock();
                    actionHeader.Text = "ACTIONS";
                    actionHeader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
                    actionHeader.Margin = new Thickness(10, 0, 0, 0);
                    actionHeader.PreviewMouseLeftButtonDown += OnActionMouseLeftButtonDown;
                    addCategoryHeaders.Children.Add(actionHeader);
                }
            }
            else
            {
                // Case when we don't have Create members.
                // Header text should be changed for correct value.
                // ActionMembers items are shown.
                topCategoryHeader.Text = "ACTIONS";
                topCategoryList.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;

                // TODO: find out should we show QueryMembers collection with header.
            }
        }
    }
}
