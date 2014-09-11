using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Nodes.Search;
using Dynamo.Search;
using Dynamo.Search.SearchElements;

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
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;

            var searchElement = listBoxItem.DataContext as SearchElementBase;
            if (searchElement != null)
                searchElement.Execute();
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
            if (classInfo == null)
                return;

            bool hasCreateMembers = classInfo.CreateMembers.Any();
            bool hasActionMembers = classInfo.ActionMembers.Any();
            bool hasQueryMembers = classInfo.QueryMembers.Any();

            areAllListsPresented = hasCreateMembers && hasActionMembers && hasQueryMembers;

            // Hide all headers by default.
            classInfo.IsPrimaryHeaderVisible = false;
            classInfo.IsSecondaryHeaderLeftVisible = false;
            classInfo.IsSecondaryHeaderRightVisible = false;

            // Set default values.
            classInfo.PrimaryHeaderGroup = SearchElementGroup.Create;
            classInfo.SecondaryHeaderLeftGroup = SearchElementGroup.Query;
            classInfo.SecondaryHeaderRightGroup = SearchElementGroup.Action;

            classInfo.CurrentDisplayMode = ClassInformation.DisplayMode.None;

            // Case when CreateMembers list is not empty.
            // We should present CreateMembers in primaryMembers.            
            if (hasCreateMembers)
            {
                classInfo.IsPrimaryHeaderVisible = true;
                primaryMembers.ItemsSource = classInfo.CreateMembers;

                if (hasQueryMembers)
                {
                    classInfo.IsSecondaryHeaderLeftVisible = true;

                    secondaryMembers.ItemsSource = classInfo.QueryMembers;
                }

                if (hasActionMembers)
                {
                    classInfo.IsSecondaryHeaderRightVisible = true;

                    if (!hasQueryMembers)
                        secondaryMembers.ItemsSource = classInfo.ActionMembers;
                }

                // For case when all lists are presented we should specify
                // correct CurrentDisplayMode.
                if (hasQueryMembers && hasActionMembers)
                    classInfo.CurrentDisplayMode = ClassInformation.DisplayMode.Query;

                return;
            }

            // Case when CreateMembers list is empty and ActionMembers list isn't empty.
            // ActionMembers will be presented in primaryMembers.
            // Depending on availibility of QueryMembers it will be shown as secondaryHeaderLeft.
            if (hasActionMembers)
            {
                classInfo.IsPrimaryHeaderVisible = true;
                classInfo.PrimaryHeaderGroup = SearchElementGroup.Action;
                primaryMembers.ItemsSource = classInfo.ActionMembers;

                if (hasQueryMembers)
                {
                    classInfo.IsSecondaryHeaderLeftVisible = true;
                    secondaryMembers.ItemsSource = classInfo.QueryMembers;
                }

                return;
            }

            // Case when CreateMembers and ActionMembers lists are empty.
            // If QueryMembers is not empty the list will be presented in primaryMembers. 
            if (hasQueryMembers)
            {
                classInfo.PrimaryHeaderGroup = SearchElementGroup.Query;
                primaryMembers.ItemsSource = classInfo.QueryMembers;
            }
        }
    }
}
