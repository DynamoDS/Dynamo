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
        private NodeSearchElementViewModel topResult = null;

        /// <summary>
        /// Currently selected member.
        /// </summary>
        public NodeSearchElementViewModel CurrentlySelection
        {
            get
            {
                return selection ?? topResult;
            }
        }

        internal SelectionNavigator(IEnumerable<SearchCategory> rootTree)
        {
            UpdateRootCategories(rootTree);
        }

        internal void UpdateRootCategories(IEnumerable<SearchCategory> rootTree, NodeSearchElementViewModel topResult = null)
        {
            root = rootTree;

            if (root == null || !root.Any())
            {
                selection = null;
                topResult = null;
                return;
            }

            selectedCategoryIndex = -1;
            selectedMemberGroupIndex = -1;
            selectedMemberIndex = -1;

            this.topResult = topResult;
            this.selection = topResult;
        }

        internal void MoveSelection(NavigationDirection direction)
        {
            if (root == null || !root.Any())
                return;

            // Selection can be null, if user just searched and currently selected element is top result.
            if (selection == topResult)
            {
                // We can only move forward...
                if (direction == NavigationDirection.Forward)
                {
                    selectedCategoryIndex = 0;
                    selectedMemberGroupIndex = 0;
                    selectedMemberIndex = 0;

                    topResult.IsSelected = false;
                    selection = GetSelectionFromIndices();
                    selection.IsSelected = true;
                    return;
                }
                else
                    // Selected element is top result.
                    // There is no way to move backward.
                    return;
            }

            var selectedCategory = root.ElementAt(selectedCategoryIndex);
            var selectedMemberGroup = selectedCategory.MemberGroups.ElementAt(selectedMemberGroupIndex);

            // Clear the current selection, no matter what.
            selection.IsSelected = false;
            selection = null;

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
                        else // No place to move back. Clear selection. Select top result.
                        {
                            selection = GetSelectionFromIndices();
                            selection.IsSelected = false;

                            selectedCategoryIndex = -1;
                            selectedMemberGroupIndex = -1;
                            selectedMemberIndex = -1;

                            selection = topResult;
                            topResult.IsSelected = true;
                            return;
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
            selection = GetSelectionFromIndices();
            selection.IsSelected = true;
        }

        private NodeSearchElementViewModel GetSelectionFromIndices()
        {
            if ((selectedCategoryIndex == -1) &&
                (selectedMemberGroupIndex == -1) &&
                (selectedMemberIndex == -1))
            {
                // No selection, return topResult instead.
                return topResult;
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
    }
}
