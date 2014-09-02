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
        private bool areAllListsPresented = false;

        public StandardPanel()
        {
            InitializeComponent();
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            int ultraBoldIndex;

            if (((sender as FrameworkElement).Tag as string) == "queryHeader")
            {
                addCategoryList.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;

                ultraBoldIndex = 0;
            }
            else
            {
                addCategoryList.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;

                ultraBoldIndex = 1;
            }

            if (areAllListsPresented)
            {
                (addCategoryHeaders.Children[ultraBoldIndex] as TextBlock).FontWeight = FontWeights.UltraBold;
                (addCategoryHeaders.Children[1 - ultraBoldIndex] as TextBlock).FontWeight = FontWeights.Normal;
            }
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

            bool isCreateListEmpty = (classInfo.CreateMembers as List<BrowserInternalElement>).Count == 0;
            bool isActionListEmpty = (classInfo.ActionMembers as List<BrowserInternalElement>).Count == 0;
            bool isQueryListEmpty = (classInfo.QueryMembers as List<BrowserInternalElement>).Count == 0;

            areAllListsPresented = !isCreateListEmpty && !isActionListEmpty && !isQueryListEmpty;

            if (!isCreateListEmpty)
            {
                topCategoryList.ItemsSource = classInfo.CreateMembers;

                if (!isQueryListEmpty)
                {
                    createAndPlaceQueryHeader(true);

                    addCategoryList.ItemsSource = classInfo.QueryMembers;
                }

                if (!isActionListEmpty)
                {
                    createAndPlaceActionHeader(isQueryListEmpty);

                    if (isQueryListEmpty)
                        addCategoryList.ItemsSource = classInfo.ActionMembers;
                }

                if (addCategoryHeaders.Children.Count > 1)
                    (addCategoryHeaders.Children[1] as FrameworkElement).Margin = new Thickness(10, 0, 0, 0);

                return;
            }

            if (!isActionListEmpty)
            {
                topCategoryHeader.Text = "ACTIONS";
                topCategoryList.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;

                if (!isQueryListEmpty)
                {
                    createAndPlaceQueryHeader();

                    addCategoryList.ItemsSource = classInfo.QueryMembers;
                }

                return;
            }

            if (!isQueryListEmpty)
            {
                topCategoryHeader.Text = "QUERY";
                topCategoryList.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;

                return;
            }

            topCategoryHeader.Visibility = Visibility.Collapsed;
        }

        private void createAndPlaceActionHeader(bool makeUltraBold = false)
        {
            TextBlock actionHeader = new TextBlock();
            actionHeader.Tag = "actionHeader";
            actionHeader.Text = "ACTIONS";
            actionHeader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
            if (makeUltraBold)
                actionHeader.FontWeight = FontWeights.UltraBold;
            actionHeader.PreviewMouseLeftButtonDown += OnHeaderMouseDown;

            addCategoryHeaders.Children.Add(actionHeader);
        }

        private void createAndPlaceQueryHeader(bool makeUltraBold = false)
        {
            TextBlock queryHeader = new TextBlock();
            queryHeader.Tag = "queryHeader";
            queryHeader.Text = "QUERY";
            queryHeader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
            if (makeUltraBold)
                queryHeader.FontWeight = FontWeights.UltraBold;
            queryHeader.PreviewMouseLeftButtonDown += OnHeaderMouseDown;

            addCategoryHeaders.Children.Add(queryHeader);
        }
    }
}
