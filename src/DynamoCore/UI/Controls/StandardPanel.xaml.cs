using System.Linq;
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
        #region Constants

        private const string CreateHeaderString = "CREATE";
        private const string ActionHeaderString = "ACTIONS";
        private const string QueryHeaderString = "QUERY";
        private const string ActionHeaderTag = "actionHeader";
        private const string QueryHeaderTag = "queryHeader";

        #endregion

        // Specifies if all Lists (CreateMembers, QueryMembers and ActionMembers) are not empty
        // and should be presented on StandardPanel.
        private bool areAllListsPresented;

        public StandardPanel()
        {
            InitializeComponent();
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            // In this cases at addCetgoryList will be situated not more one
            // list. We don't need switch between lists.
            if (!areAllListsPresented)
                return;

            int ultraBoldIndex;

            if ((sender as FrameworkElement).Tag.ToString() == QueryHeaderTag)
            {
                secondaryMembers.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;

                ultraBoldIndex = 0;
            }
            else
            {
                secondaryMembers.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;

                ultraBoldIndex = 1;
            }

            //// Setting styles. 
            //if (areAllListsPresented)
            //{
            //    (addCategoryHeaders.Children[ultraBoldIndex] as TextBlock).FontWeight = FontWeights.UltraBold;
            //    (addCategoryHeaders.Children[1 - ultraBoldIndex] as TextBlock).FontWeight = FontWeights.Normal;
            //}
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

            bool isCreateListEmpty = !classInfo.CreateMembers.Any();
            bool isActionListEmpty = !classInfo.ActionMembers.Any();
            bool isQueryListEmpty = !classInfo.QueryMembers.Any();

            areAllListsPresented = !isCreateListEmpty && !isActionListEmpty && !isQueryListEmpty;

            // Case when CreateMembers list is not empty.
            // We should present CreateMembers in topCategoryList.
            // Depending on availibility of QueryMembers and ActionMembers
            // new TextBlock will be added to addCategoryList.
            if (!isCreateListEmpty)
            {
                primaryMembers.ItemsSource = classInfo.CreateMembers;

                if (!isQueryListEmpty)
                {
                    CreateAndPlaceQueryHeader(true);

                    secondaryMembers.ItemsSource = classInfo.QueryMembers;
                }

                if (!isActionListEmpty)
                {
                    CreateAndPlaceActionHeader(isQueryListEmpty);

                    if (isQueryListEmpty)
                        secondaryMembers.ItemsSource = classInfo.ActionMembers;
                }

                return;
            }

            // Case when CreateMembers list is empty and ActionMembers list isn't empty.
            // ActionMembers will be presented in topCategoryList.
            // Depending on availibility of QueryMembers it will be added to addCategoryList.
            if (!isActionListEmpty)
            {
                primaryHeader.Text = ActionHeaderString;
                primaryMembers.ItemsSource = (this.DataContext as ClassInformation).ActionMembers;

                if (!isQueryListEmpty)
                {
                    CreateAndPlaceQueryHeader();

                    secondaryMembers.ItemsSource = classInfo.QueryMembers;
                }

                return;
            }

            // Case when CreateMembers and ActionMembers lists are empty.
            // If QueryMembers is not empty the list will be presented in topCategoryList. 
            if (!isQueryListEmpty)
            {
                primaryHeader.Text = QueryHeaderString;
                primaryMembers.ItemsSource = (this.DataContext as ClassInformation).QueryMembers;

                return;
            }

            // All lists are empty. We should hide topCategoryHeader TextBlock
            primaryHeader.Visibility = Visibility.Collapsed;
        }

        private void CreateAndPlaceActionHeader(bool makeUltraBold = false)
        {
            TextBlock actionHeader = new TextBlock();
            actionHeader.Tag = ActionHeaderTag;
            actionHeader.Text = ActionHeaderString;
            actionHeader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
            if (makeUltraBold)
                actionHeader.FontWeight = FontWeights.UltraBold;
            actionHeader.PreviewMouseLeftButtonDown += OnHeaderMouseDown;

            //addCategoryHeaders.Children.Add(actionHeader);
        }

        private void CreateAndPlaceQueryHeader(bool makeUltraBold = false)
        {
            TextBlock queryHeader = new TextBlock();
            queryHeader.Tag = QueryHeaderTag;
            queryHeader.Text = QueryHeaderString;
            queryHeader.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(221, 221, 221));
            if (makeUltraBold)
                queryHeader.FontWeight = FontWeights.UltraBold;
            queryHeader.PreviewMouseLeftButtonDown += OnHeaderMouseDown;

            //addCategoryHeaders.Children.Add(queryHeader);
        }
    }
}
