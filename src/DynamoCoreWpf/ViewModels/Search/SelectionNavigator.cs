using Dynamo.Search;
using Dynamo.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dynamo.ViewModels
{

    public enum NavigationDirection
    {
        Backward, Forward
    }

    public class SelectionNavigator
    {
        /// <summary>
        /// Used during library search key navigation. Indicates which item index is selected.
        /// </summary>
        private int selectedCategoryIndex = -1;

        /// <summary>
        /// Used during library search key navigation. Indicates which group index is selected.
        /// </summary>
        private int selectedMemberGroupIndex = -1;

        /// <summary>
        /// Used during library search key navigation. Indicates which category index is selected.
        /// </summary>
        private int selectedMemberIndex = -1;

        private IEnumerable<SearchCategory> root = null;

        private NodeSearchElementViewModel selection = null;

        /// <summary>
        /// Currently selected member.
        /// </summary>
        public NodeSearchElementViewModel CurrentlySelection
        {
            get { return selection; }
        }

        internal SelectionNavigator(IEnumerable<SearchCategory> rootTree)
        {
            UpdateRootCategories(rootTree);
        }

        internal void UpdateRootCategories(IEnumerable<SearchCategory> rootTree)
        {
            root = rootTree;

            if (!root.Any())
                return;

            selectedCategoryIndex = 0;
            selectedMemberGroupIndex = 0;
            selectedMemberIndex = 0;

            selection = GetSelectionFromIndices();
            selection.IsSelected = true;
        }

        internal void MoveSelection(NavigationDirection direction)
        {
            var offset = direction == NavigationDirection.Backward ? -1 : 1;

            if (!root.Any())
                return;

            // First we update the selection index.
            selectedMemberIndex += offset;

            var selectedCategory = root.ElementAt(selectedCategoryIndex);
            var selectedMemberGroup = selectedCategory.MemberGroups.ElementAt(selectedMemberGroupIndex);

            // Clear the current selection, no matter what.
            selection.IsSelected = false;
            selection = null;

            // It's first element. Can't move further. We need jump to previous member group.
            if (selectedMemberIndex < 0)
            {
                selectedMemberGroupIndex += offset;

                // It's first element of first member group. We can't move further.
                // We need jump to previous category.
                if (selectedMemberGroupIndex < 0)
                {
                    selectedCategoryIndex += offset;

                    // It's first element of first member group of first category. There is no place to move.
                    if (selectedCategoryIndex < 0)
                    {
                        selectedCategoryIndex = 0;
                        selectedMemberGroupIndex = 0;
                        selectedMemberIndex = 0;
                    }
                    else
                    {
                        // Since we are moving from the bottom to the top, 
                        // we should select last member and last group.
                        var cat = root.ElementAt(selectedCategoryIndex);
                        selectedMemberGroupIndex = cat.MemberGroups.Count() - 1;

                        var group = cat.MemberGroups.ElementAt(selectedMemberGroupIndex);

                        selectedMemberIndex = group.Members.Count() - 1;
                    }
                }
                else
                {
                    // Select last member of selected member group.
                    var group = root.ElementAt(selectedCategoryIndex).MemberGroups.ElementAt(selectedMemberGroupIndex);
                    selectedMemberIndex = group.Members.Count() - 1;
                }
            }
            else
            {
                // Determine the current group size.
                var members = selectedMemberGroup.Members.Count();

                // It's last member of member group. We can't move further.
                // We need jump to next member group.
                if (selectedMemberIndex >= members)
                {
                    selectedMemberIndex = 0;
                    selectedMemberGroupIndex += offset;

                    var memberGroups = root.ElementAt(selectedCategoryIndex).MemberGroups.Count();

                    // It's last member of last member group. We can't move further.
                    // We need jump to next category.
                    if (selectedMemberGroupIndex >= memberGroups)
                    {
                        selectedMemberGroupIndex = 0;
                        selectedCategoryIndex += offset;

                        // It's last member of last member group of last category. There is no place to move.
                        if (selectedCategoryIndex >= root.Count())
                        {
                            selectedCategoryIndex = root.Count() - 1;
                            selectedMemberGroupIndex = root.Last().MemberGroups.Count() - 1;
                            selectedMemberIndex = root.Last().MemberGroups.Last().Members.Count() - 1;
                        }
                    }
                }
            }

            // Get the new selection and mark it as selected.
            selection = GetSelectionFromIndices();
            selection.IsSelected = true;
        }

        private NodeSearchElementViewModel GetSelectionFromIndices()
        {
            var selectedCategory = root.ElementAt(selectedCategoryIndex);
            if (selectedCategory == null)
                return null;

            var selectedMemberGroup = selectedCategory.MemberGroups.ElementAt(selectedMemberGroupIndex);
            if (selectedMemberGroup == null)
                return null;

            var selectedMember = selectedMemberGroup.Members.ElementAt(selectedMemberIndex);
            return selectedMember;
        }
    }
}
