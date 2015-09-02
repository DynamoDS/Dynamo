using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.UI;
using Dynamo.Wpf.ViewModels;
using System;

namespace Dynamo.Search
{
    public class SearchMemberGroup : NotificationObject, ISearchEntryViewModel
    {
        private List<NodeSearchElementViewModel> members;

        public string FullyQualifiedName { get; private set; }

        public bool IsExpanded { get { return true; } }

        public string Prefix
        {
            get
            {
                if (string.IsNullOrEmpty(FullyQualifiedName))
                    return string.Empty;

                int index = FullyQualifiedName.IndexOf(delimiter);
                var name = index > -1 ? FullyQualifiedName.Substring(index + delimiter.Length)
                                      : FullyQualifiedName;

                index = name.LastIndexOf(delimiter);
                return index > -1 ? name.Substring(0, index + delimiter.Length)
                                  : string.Empty;
            }
        }

        public string GroupName
        {
            get
            {
                // Skip past the last delimiter and get the group name.
                int index = FullyQualifiedName.LastIndexOf(delimiter);
                return index > -1 ? FullyQualifiedName.Substring(index + delimiter.Length)
                                  : string.Empty;
            }
        }

        /// <summary>
        /// Category to which all memebers are belong to.
        /// </summary>
        public NodeCategoryViewModel Category { get; private set; }

        public IEnumerable<NodeSearchElementViewModel> Members
        {
            get
            {
                if (!showAllMembers)
                    return members;

                if (members.Count == 0) return members;

                var firstMember = members[0] as NodeSearchElementViewModel;

                // Parent items can contain 3 type of groups all together: create, action and query.
                // We have to show only those elements, that are in the same group.
                var siblings = Category.Entries.Where(e => e.Model.Group == firstMember.Model.Group);
                return siblings;
            }
        }

        private bool showAllMembers = false;
        private string delimiter = Configurations.CategoryDelimiterWithSpaces;

        internal SearchMemberGroup(string fullyQualifiedName, NodeCategoryViewModel category = null)
        {
            FullyQualifiedName = fullyQualifiedName;
            Category = category;
            members = new List<NodeSearchElementViewModel>();
        }

        internal void AddMember(NodeSearchElementViewModel node)
        {
            members.Add(node);
        }

        internal bool ContainsMember(NodeSearchElementViewModel member)
        {
            return Members.Any(m => m.Model.FullName == member.Model.FullName);
        }

        internal void ExpandAllMembers()
        {
            showAllMembers = true;
            RaisePropertyChanged("Members");
        }

        internal void Sort()
        {
            members = members.OrderBy(x => x.Name).ToList();
        }

        public string Name
        {
            get { return String.Empty; }
        }

        public bool Visibility
        {
            get { return true; }
        }

        public bool IsSelected
        {
            get { return false; }
        }

        public string Description
        {
            get { return String.Empty; }
        }

        public System.Windows.Input.ICommand ClickedCommand
        {
            get { return null; }
        }

        public ElementTypes ElementType
        {
            get { return ElementTypes.None; }
        }

        public void Dispose()
        {
        }
    }
}
