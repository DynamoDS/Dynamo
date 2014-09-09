using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Nodes.Search;
using Dynamo.Search;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for StandardPanel.xaml
    /// </summary>
    public partial class StandardPanel : UserControl
    {
        #region Constants

        private const string ActionHeaderTag = "Action";
        private const string QueryHeaderTag = "Query";

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

            var classInfo = this.DataContext as ClassInformation;
            if ((sender as FrameworkElement).Tag.ToString() == QueryHeaderTag)
            {
                classInfo.CurrentDisplayMode = ClassInformation.DisplayMode.Query;
                secondaryMembers.ItemsSource = classInfo.QueryMembers;
            }
            else
            {
                classInfo.CurrentDisplayMode = ClassInformation.DisplayMode.Action;
                secondaryMembers.ItemsSource = classInfo.ActionMembers;
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

            bool isCreateListEmpty = !classInfo.CreateMembers.Any();
            bool isActionListEmpty = !classInfo.ActionMembers.Any();
            bool isQueryListEmpty = !classInfo.QueryMembers.Any();

            areAllListsPresented = !isCreateListEmpty && !isActionListEmpty && !isQueryListEmpty;

            // Hide all headers by default.
            classInfo.PrimaryHeaderVisibility = false;
            classInfo.SecondaryHeaderLeftVisibility = false;
            classInfo.SecondaryHeaderRightVisibility = false;

            // Set default values.
            classInfo.PrimaryHeaderGroup = SearchElementGroup.Create;
            classInfo.SecondaryHeaderLeftGroup = SearchElementGroup.Query;
            classInfo.SecondaryHeaderRightGroup = SearchElementGroup.Action;

            classInfo.CurrentDisplayMode = ClassInformation.DisplayMode.None;

            // Case when CreateMembers list is not empty.
            // We should present CreateMembers in primaryMembers.            
            if (!isCreateListEmpty)
            {
                classInfo.PrimaryHeaderVisibility = true;
                primaryMembers.ItemsSource = classInfo.CreateMembers;

                if (!isQueryListEmpty)
                {
                    classInfo.SecondaryHeaderLeftVisibility = true;
                    classInfo.SecondaryHeaderRightVisibility = true;
                    classInfo.CurrentDisplayMode = ClassInformation.DisplayMode.Query;

                    secondaryMembers.ItemsSource = classInfo.QueryMembers;
                }

                if (!isActionListEmpty && isQueryListEmpty)
                {
                    classInfo.SecondaryHeaderLeftVisibility = true;

                    secondaryMembers.ItemsSource = classInfo.ActionMembers;
                }

                return;
            }

            // Case when CreateMembers list is empty and ActionMembers list isn't empty.
            // ActionMembers will be presented in primaryMembers.
            // Depending on availibility of QueryMembers it will be shown as secondaryHeaderLeft.
            if (!isActionListEmpty)
            {
                classInfo.PrimaryHeaderVisibility = true;
                classInfo.PrimaryHeaderGroup = SearchElementGroup.Action;
                primaryMembers.ItemsSource = classInfo.ActionMembers;

                if (!isQueryListEmpty)
                {
                    classInfo.SecondaryHeaderLeftVisibility = true;
                    secondaryMembers.ItemsSource = classInfo.QueryMembers;
                }

                return;
            }

            // Case when CreateMembers and ActionMembers lists are empty.
            // If QueryMembers is not empty the list will be presented in primaryMembers. 
            if (!isQueryListEmpty)
            {
                classInfo.PrimaryHeaderGroup = SearchElementGroup.Query;
                primaryMembers.ItemsSource = classInfo.QueryMembers;
            }
        }
    }
}
