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

        /// <summary>
        /// Currently selected member.
        /// </summary>
        public NodeSearchElementViewModel CurrentSelection
        {
            get
            {
                return GetSelectionFromIndices();
            }
        }

        internal SelectionNavigator(IEnumerable<SearchCategory> rootTree)
        {
            UpdateRootCategories(rootTree);
        }

        internal void UpdateRootCategories(IEnumerable<SearchCategory> rootTree)
        {
            root = rootTree;

            if (root == null || !root.Any())
            {
                selectedCategoryIndex = -1;
                selectedMemberGroupIndex = -1;
                selectedMemberIndex = -1;
                return;
            }

            SelectItem(0, 0, 0);
        }

        internal void MoveSelection(NavigationDirection direction)
        {
            if (root == null || !root.Any())
                return;

            var selectedCategory = root.ElementAt(selectedCategoryIndex);
            var selectedMemberGroup = selectedCategory.MemberGroups.ElementAt(selectedMemberGroupIndex);

            // Clear the current selection, no matter what.
            CurrentSelection.IsSelected = false;

            if (direction == NavigationDirection.Backward)
            {
                if (selectedMemberIndex != 0)
                {
                    selectedMemberIndex--;
                }
                else
                {
                    if (selectedMemberGroupIndex != 0)
                    {
                        selectedMemberGroupIndex--;

                        // Select last member of new member group.
                        var category = root.ElementAt(selectedCategoryIndex);
                        var group = category.MemberGroups.ElementAt(selectedMemberGroupIndex);
                        selectedMemberIndex = group.Members.Count() - 1;
                    }
                    else
                    {
                        if (selectedCategoryIndex != 0)
                        {
                            selectedCategoryIndex--;

                            // Select last group and last member of this group.
                            var category = root.ElementAt(selectedCategoryIndex);
                            selectedMemberGroupIndex = category.MemberGroups.Count() - 1;

                            var group = category.MemberGroups.ElementAt(selectedMemberGroupIndex);
                            selectedMemberIndex = group.Members.Count() - 1;
                        }
                    }
                }
            }
            else
            {
                // Determine the current group size.
                var members = selectedMemberGroup.Members.Count();

                if (selectedMemberIndex < members - 1) // There's still next member.
                {
                    selectedMemberIndex++;
                }
                else
                {
                    var memberGroups = root.ElementAt(selectedCategoryIndex).MemberGroups.Count();
                    if (selectedMemberGroupIndex < memberGroups - 1) // There's still next group.
                    {
                        selectedMemberIndex = 0;
                        selectedMemberGroupIndex++;
                    }
                    else if (selectedCategoryIndex < root.Count() - 1) // There's still next category.
                    {
                        selectedMemberIndex = 0;
                        selectedMemberGroupIndex = 0;
                        selectedCategoryIndex++;
                    }
                }
            }

            // Get the new selection and mark it as selected.
            CurrentSelection.IsSelected = true;
        }

        private NodeSearchElementViewModel GetSelectionFromIndices()
        {
            if ((selectedCategoryIndex == -1) &&
                (selectedMemberGroupIndex == -1) &&
                (selectedMemberIndex == -1))
            {
                // No selection.
                return null;
            }

            var selectedCategory = root.ElementAt(selectedCategoryIndex);
            if (selectedCategory == null)
                return null;

            var selectedMemberGroup = selectedCategory.MemberGroups.ElementAt(selectedMemberGroupIndex);
            if (selectedMemberGroup == null)
                return null;

            var selectedMember = selectedMemberGroup.Members.ElementAt(selectedMemberIndex);
            return selectedMember;
        }

        private void SelectItem(int categoryIndex, int memberGroupIndex, int memberIndex)
        {
            var selection = GetSelectionFromIndices();
            if (selection != null)
            {
                selection.IsSelected = false;
            }

            selectedCategoryIndex = categoryIndex;
            selectedMemberGroupIndex = memberGroupIndex;
            selectedMemberIndex = memberIndex;

            selection = GetSelectionFromIndices();
            if (selection != null)
            {
                selection.IsSelected = true;
            }
        }
    }
}
