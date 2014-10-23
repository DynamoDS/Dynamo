using System.Collections.Generic;
using Dynamo.Nodes.Search;
using System.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.UI;

namespace Dynamo.Search
{
    public class SearchMemberGroup
    {
        private readonly List<BrowserInternalElement> members;
        private List<NodeSearchElement> parentMembers;

        public string Name { get; private set; }

        public string Prefix
        {
            get
            {
                int startIndexOfGroupType = Name.LastIndexOf(Configurations.ShortenedCategoryDelimiter) + 2;
                // +2, because after ">" always stays space character.
                return Name.Substring(0, startIndexOfGroupType);
            }
        }

        public string GroupType
        {
            get
            {
                int startIndexOfGroupType = Name.LastIndexOf(Configurations.ShortenedCategoryDelimiter) + 2;
                return Name.Substring(startIndexOfGroupType);
            }
        }

        public IEnumerable<BrowserInternalElement> Members
        {
            get { return members; }
        }

        public IEnumerable<BrowserInternalElement> ParentMembers
        {
            get { return parentMembers; }
        }

        internal SearchMemberGroup(string name)
        {
            Name = name;
            members = new List<BrowserInternalElement>();
            parentMembers = new List<NodeSearchElement>();
        }

        //some UI properties which control style of one MemberGroup

        internal void AddMember(BrowserInternalElement node)
        {
            members.Add(node);
            if (!parentMembers.Any()) 
                parentMembers = node.Parent.Items.OfType<NodeSearchElement>().
                    Where(parentNode => parentNode.Group == (node as NodeSearchElement).Group).ToList();
        }

        public bool ContainsMember(BrowserInternalElement member)
        {
           return Members.Contains(member);
        }
    }
}
