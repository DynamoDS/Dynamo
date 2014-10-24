using System.Collections.Generic;
using Dynamo.Nodes.Search;
using System.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Search
{
    public class SearchMemberGroup : NotificationObject
    {
        private readonly List<BrowserInternalElement> members;

        public string Name { get; private set; }

        public string Prefix
        {
            get
            {
                // +2, because after ">" always stays space character.
                int startIndexOfGroupType = Name.LastIndexOf(Configurations.ShortenedCategoryDelimiter) + 2;
                return Name.Substring(0, startIndexOfGroupType);
            }
        }

        public string GroupName
        {
            get
            {
                int startIndexOfGroupType = Name.LastIndexOf(Configurations.ShortenedCategoryDelimiter) + 2;
                return Name.Substring(startIndexOfGroupType);
            }
        }

        public IEnumerable<BrowserInternalElement> Members
        {
            get 
            {
                if (!showAllMembers)
                    return members;

                if (members.Count == 0) return null;

                var firstMember = members[0] as NodeSearchElement;
                return firstMember.Parent.Items.OfType<BrowserInternalElement>().
                        Where(parentNode => (parentNode as NodeSearchElement).Group == firstMember.Group).ToList();
            }
        }

        private bool showAllMembers = false;

        internal SearchMemberGroup(string name)
        {
            Name = name;
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
