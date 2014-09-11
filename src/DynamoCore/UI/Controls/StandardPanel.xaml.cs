using System.Collections.Generic;
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
        private IEnumerable<BrowserInternalElement> hiddenMembers;
        private ClassInformation castedDataContext;

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
            castedDataContext = this.DataContext as ClassInformation;
            if (castedDataContext == null)
                return;

            bool hasCreateMembers = castedDataContext.CreateMembers.Any();
            bool hasActionMembers = castedDataContext.ActionMembers.Any();
            bool hasQueryMembers = castedDataContext.QueryMembers.Any();

            areAllListsPresented = hasCreateMembers && hasActionMembers && hasQueryMembers;

            // Hide all headers by default.
            castedDataContext.IsPrimaryHeaderVisible = false;
            castedDataContext.IsSecondaryHeaderLeftVisible = false;
            castedDataContext.IsSecondaryHeaderRightVisible = false;

            // Set default values.
            castedDataContext.PrimaryHeaderGroup = SearchElementGroup.Create;
            castedDataContext.SecondaryHeaderLeftGroup = SearchElementGroup.Query;
            castedDataContext.SecondaryHeaderRightGroup = SearchElementGroup.Action;

            castedDataContext.CurrentDisplayMode = ClassInformation.DisplayMode.None;

            // Case when CreateMembers list is not empty.
            // We should present CreateMembers in primaryMembers.            
            if (hasCreateMembers)
            {
                castedDataContext.IsPrimaryHeaderVisible = true;
                primaryMembers.ItemsSource = castedDataContext.CreateMembers;

                if (hasQueryMembers)
                {
                    castedDataContext.IsSecondaryHeaderLeftVisible = true;

                    secondaryMembers.ItemsSource = castedDataContext.QueryMembers;
                }

                if (hasActionMembers)
                {
                    castedDataContext.IsSecondaryHeaderRightVisible = true;

                    if (!hasQueryMembers)
                        secondaryMembers.ItemsSource = castedDataContext.ActionMembers;
                }

                // For case when all lists are presented we should specify
                // correct CurrentDisplayMode.
                if (hasQueryMembers && hasActionMembers)
                    castedDataContext.CurrentDisplayMode = ClassInformation.DisplayMode.Query;

                ShowMoreButton();
                return;
            }

            // Case when CreateMembers list is empty and ActionMembers list isn't empty.
            // ActionMembers will be presented in primaryMembers.
            // Depending on availibility of QueryMembers it will be shown as secondaryHeaderLeft.
            if (hasActionMembers)
            {
                castedDataContext.IsPrimaryHeaderVisible = true;
                castedDataContext.PrimaryHeaderGroup = SearchElementGroup.Action;
                primaryMembers.ItemsSource = castedDataContext.ActionMembers;

                if (hasQueryMembers)
                {
                    castedDataContext.IsSecondaryHeaderLeftVisible = true;
                    secondaryMembers.ItemsSource = castedDataContext.QueryMembers;
                }

                ShowMoreButton();
                return;
            }

            // Case when CreateMembers and ActionMembers lists are empty.
            // If QueryMembers is not empty the list will be presented in primaryMembers. 
            if (hasQueryMembers)
            {
                castedDataContext.PrimaryHeaderGroup = SearchElementGroup.Query;
                primaryMembers.ItemsSource = castedDataContext.QueryMembers;
            }
        }

        private void OnMoreButtonClick(object sender, RoutedEventArgs e)
        {
            HideMoreButton();
        }

        private void ShowMoreButton()
        {
            castedDataContext.IsMoreButtonVisible = false;

            var members = secondaryMembers.ItemsSource as IEnumerable<BrowserInternalElement>;
            if (members != null && members.Count() > 4)
            {
                hiddenMembers = members.Skip(4);
                secondaryMembers.ItemsSource = members.Take(4);

                castedDataContext.IsMoreButtonVisible = true;
            }
        }

        private void HideMoreButton()
        {
            if (hiddenMembers != null)
            {
                var members = secondaryMembers.ItemsSource as IEnumerable<BrowserInternalElement>;
                var allMembers = members.ToList();
                allMembers.AddRange(hiddenMembers);

                secondaryMembers.ItemsSource = allMembers;
                hiddenMembers = null;
            }

            castedDataContext.IsMoreButtonVisible = false;
        }
    }
}
