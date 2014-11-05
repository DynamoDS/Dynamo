using System.Collections.Generic;
using System.Linq;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Search
{
    public class SearchMemberGroup : NotificationObject
    {
        private readonly List<BrowserInternalElement> members;

        public string FullyQualifiedName { get; private set; }

        public string Prefix
        {
            get
            {
                if (string.IsNullOrEmpty(FullyQualifiedName))
                    return string.Empty;

                var delimiter = string.Format(" {0} ", Configurations.ShortenedCategoryDelimiter);

                int index = FullyQualifiedName.IndexOf(delimiter);
                var name = index > -1 ? FullyQualifiedName.Substring(index + delimiter.Length)
                                      : FullyQualifiedName;

                // Skip past the last delimiter and get the group name.
                index = name.LastIndexOf(delimiter);
                return index > -1 ? name.Substring(0, index + delimiter.Length)
                                  : string.Empty;
            }
        }

        public string GroupName
        {
            get
            {
                int startIndexOfGroupType = FullyQualifiedName.LastIndexOf(Configurations.ShortenedCategoryDelimiter) + 2;
                return FullyQualifiedName.Substring(startIndexOfGroupType);
            }
        }

        public IEnumerable<BrowserInternalElement> Members
        {
            get
            {
                if (!showAllMembers)
                    return members;

                if (members.Count == 0) return new List<BrowserInternalElement>();

                var firstMember = members[0] as NodeSearchElement;

                // Parent items can contain 3 type of groups all together: create, action and query.
                // We have to show only those elements, that are in the same group.
                var siblings = firstMember.Parent.Items.OfType<BrowserInternalElement>().
                        Where(parentNode => (parentNode as NodeSearchElement).Group == firstMember.Group);

                return siblings;
            }
        }

        private bool showAllMembers = false;

        internal SearchMemberGroup(string fullyQualifiedName)
        {
            FullyQualifiedName = fullyQualifiedName;
            members = new List<BrowserInternalElement>();
        }

        //some UI properties which control style of one MemberGroup

        internal void AddMember(BrowserInternalElement node)
        {
            members.Add(node);
        }

        public bool ContainsMember(BrowserInternalElement member)
        {
            return Members.Contains(member);
        }

        public void ExpandAllMembers()
        {
            showAllMembers = true;
            RaisePropertyChanged("Members");
        }
    }
}
